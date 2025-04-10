using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using CyberStone.Core.Converters;
using CyberStone.Core.Entities;
using CyberStone.Core.Managers;
using CyberStone.Core.Utils;
using System;
using System.Globalization;
using System.IdentityModel.Tokens.Jwt;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using CyberStone.Authorizations;
using CyberStone.Core.Utils.Captcha;
using System.Security.Claims;
using CyberStone.Core.Models;
using Microsoft.AspNetCore.StaticFiles;
using CyberStone.Core.TreeFeature;

namespace CyberStone.Core
{
  public static class CyberStoneExtensions
  {
    public static void AddCyberStone(
        this IServiceCollection services,
        IConfiguration configuration)
    {
      services.Configure<UploadOptions>(options =>
      {
        options.FolderMask = configuration.GetValue<string>("UploadOptions:FolderMask") ?? "";
        options.FileNameGenerator = configuration.GetValue<string>("UploadOptions:FileNameGenerator") ?? "uuid";
        options.Path = configuration.GetValue<string>("UploadOptions:Path") ?? "uploads";
        options.WebRoot = configuration.GetValue<string>("UploadOptions:WebRoot") ?? "/uploads";
        var path = options.Path.StartsWith('/') ? options.Path : Path.Combine(Directory.GetCurrentDirectory(), options.Path);
        options.AbsolutePath = path;
      });

      var mvcBuilder = services.AddControllers();

      mvcBuilder.AddJsonOptions(options =>
      {
        options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
        options.JsonSerializerOptions.Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping;
        options.JsonSerializerOptions.Converters.Add(new JsonValueSpaceConverter());
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter(namingPolicy: JsonNamingPolicy.CamelCase));
      });

      //添加树处理
      services.AddScoped<TreeNodeManager>();
      
      services.Configure<UserLogOptions>(o =>
      {
        o.LogLevel = configuration.GetValue<UserLogLevel>("UserLogOptions:LogLevel");
      });

      #region 时间格式化设置

      // Linux 下默认格式不确定
      if (CultureInfo.CurrentCulture.Clone() is CultureInfo culture)
      {
        culture.DateTimeFormat.LongDatePattern = "yyyy-MM-dd";
        culture.DateTimeFormat.ShortDatePattern = "yyyy-MM-dd";
        culture.DateTimeFormat.ShortTimePattern = "HH:mm";
        culture.DateTimeFormat.LongTimePattern = "HH:mm:ss";
        culture.DateTimeFormat.ShortTimePattern = "HH:mm";
        CultureInfo.DefaultThreadCurrentCulture = culture;
        CultureInfo.CurrentCulture = culture;
      }

      #endregion 时间格式化设置

      #region 缓存设置

      services.AddMemoryCache();
      if (configuration.GetValue<string>("CacheOptions:Active") == "FileCache")
      {
        services.AddFileCache(options =>
        {
          options.CacheFolder = configuration.GetValue<string>("CacheOptions:FileCache:CacheFolder") ?? "cache";
        });
      }
      else
      {
        services.AddStackExchangeRedisCache(options =>
        {
          options.Configuration = configuration.GetValue<string>("CacheOptions:RedisCache:Server") + ",abortConnect=true";
          var pwd = configuration.GetValue<string>("CacheOptions:RedisCache:Password");
          if (pwd != null)
          {
            options.Configuration += $",password={pwd}";
          }
          options.InstanceName = configuration.GetValue<string>("CacheOptions:RedisCache:Instance");
        });
      }

      #endregion 缓存设置

      var connectionString = configuration.GetConnectionString("Default");
      var dbType = configuration.GetValue<string>("DatabaseType") ?? "mysql";
      var dbCompatibilityLevel = configuration.GetValue<int?>("DatabaseCompatibilityLevel");
      switch (dbType.ToLower())
      {
        case "mysql":
          services.AddDbContext<CyberStoneDbContext>(options =>
          {
            options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString), o =>
            {
              o.MigrationsAssembly(nameof(CyberStone));
            });
          });
          break;

        case "sqlserver":
        case "mssql":
          services.AddDbContext<CyberStoneDbContext>(options =>
          {
            options.UseSqlServer(connectionString, o =>
            {
              o.MigrationsAssembly(nameof(CyberStone));
              if (dbCompatibilityLevel != null)
              {
                o.UseCompatibilityLevel(dbCompatibilityLevel.Value);
              }
            });
          });
          break;

        case "oracle":
          services.AddDbContext<CyberStoneDbContext>(options =>
          {
            options.UseOracle(connectionString, o =>
            {
              o.MigrationsAssembly(nameof(CyberStone));
              o.UseOracleSQLCompatibility(OracleSQLCompatibility.DatabaseVersion19);
            });
          });
          break;
        default:
          throw new NotImplementedException("您输入的数据库类型暂时还不支持");
      }

      #region 注册 ValueSpaceMap，系统可以直接注入 ValueSpaceMap 以获得 ValueSpace

      services.AddSingleton(sp =>
      {
        return ValueSpaceManager.ValueSpaceMap;
      });

      #endregion 注册 ValueSpaceMap，系统可以直接注入 ValueSpaceMap 以获得 ValueSpace

      #region 注册 ProfileKeyMap，系统可以直接注入 ProfileKeyMap 以获得 ProfileKey

      services.AddSingleton(pp =>
      {
        return ProfileKeyManager.ProfileKeyMap;
      });

      #endregion 注册 ProfileKeyMap，系统可以直接注入 ProfileKeyMap 以获得 ProfileKey

      #region 添加 Manager 依赖
      services.AddScoped<ConfigManager<GlobalSettings>>();
      services.AddScoped<DepartmentManager>();
      services.AddScoped<ValueSpaceManager>();
      services.AddScoped<MenuManager>();
      services.AddScoped<SettingManager>();
      services.AddScoped<TokenManager>();
      services.AddScoped<ProfileKeyManager>();
      services.AddScoped<UserLogManager>();
      services.AddScoped<CaptchaManager>();

      #endregion 添加 Manager 依赖

      #region 验证码设置

      var captchaOptions = configuration.GetSection(nameof(CaptchaOptions));
      services.AddHeiCaptcha();
      services.Configure<CaptchaOptions>(captchaOptions);

      #endregion

      #region 注册 Identity 及 JsonWebToken
      services.Configure<PasswordSecurityOptions>(options =>
      {
        options.AesKey = configuration.GetValue<string>("PasswordSecurityOptions:AesKey");
        options.FrontendHash = configuration.GetValue<string>("PasswordSecurityOptions:FrontendHash") ?? "none";
        options.DefaultPassword = configuration.GetValue<string>("PasswordSecurityOptions:DefaultPassword") ?? "123456";
      });
      services.AddIdentity<UserEntity, RoleEntity>(options =>
      {
        options.Password.RequireDigit = false;
        options.Password.RequireUppercase = false;
        options.Password.RequireLowercase = false;
        options.Password.RequiredUniqueChars = 0;

        options.Password.RequiredLength = 32;
        options.Password.RequireNonAlphanumeric = false;
        options.User.RequireUniqueEmail = false;
        options.ClaimsIdentity.UserNameClaimType = ClaimTypes.Name;
        options.Lockout = new LockoutOptions
        {
          AllowedForNewUsers = true,
          DefaultLockoutTimeSpan = TimeSpan.FromMinutes(20),
          MaxFailedAccessAttempts = 5
        };
      }).AddRoles<RoleEntity>()
      .AddUserManager<UserManager>()
      .AddSignInManager<SignInManager>()
      .AddRoleManager<RoleManager>()
      .AddDefaultTokenProviders()
      .AddEntityFrameworkStores<CyberStoneDbContext>();

      #endregion 注册 Identity 及 JsonWebToken

      #region Jwt 设置

      var jwtAppSettingOptions = configuration.GetSection(nameof(JwtIssuerOptions));
      var signingKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(jwtAppSettingOptions[nameof(JwtIssuerOptions.SecretKey)]!));
      services.Configure<JwtIssuerOptions>(options =>
      {
        options.Issuer = jwtAppSettingOptions[nameof(JwtIssuerOptions.Issuer)] ?? "cyberstone";
        options.Audience = jwtAppSettingOptions[nameof(JwtIssuerOptions.Audience)] ?? "cyberstone";
        var validFor = jwtAppSettingOptions["AccessTokenExpiredMinutes"];
        if (!string.IsNullOrEmpty(validFor) && int.TryParse(validFor, out var t))
        {
          options.ValidFor = TimeSpan.FromMinutes(t);
        }
        options.SigningCredentials = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256);
      });

      var tokenValidationParameters = new TokenValidationParameters
      {
        NameClaimType = JwtRegisteredClaimNames.NameId,
        ValidateIssuer = true,
        ValidIssuer = jwtAppSettingOptions[nameof(JwtIssuerOptions.Issuer)],

        ValidateAudience = true,
        ValidAudience = jwtAppSettingOptions[nameof(JwtIssuerOptions.Audience)],

        ValidateIssuerSigningKey = true,
        IssuerSigningKey = signingKey,

        //RequireExpirationTime = true,
        RequireExpirationTime = false,
        //ValidateLifetime = true,
        ValidateLifetime = false,
        ClockSkew = TimeSpan.Zero
      };
      JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();

      services.AddAuthentication(options =>
      {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
      }).AddJwtBearer(options =>
      {
        options.RequireHttpsMetadata = false;
        options.Audience = jwtAppSettingOptions[nameof(JwtIssuerOptions.Audience)];
        options.SaveToken = false;
        options.TokenValidationParameters = tokenValidationParameters;
        options.RequireHttpsMetadata = false;
      });

      #endregion Jwt 设置

      #region PermissionAuthorization 设置

      services.AddScoped<IAuthorizationHandler, PermissionAuthorizationHandler>();
      services.AddAuthorization(options =>
      {

        options.AddPolicy(ClaimNames.ApiPermission, builder =>
        {
          builder.AddRequirements(new PermissionRequirement());
        });
      });

      #endregion PermissionAuthorization 设置
    }

    public static void UsePureCode(this IApplicationBuilder app)
    {
      // 上传文件配置
      var uploadOptions = app.ApplicationServices.GetService<IOptions<UploadOptions>>()!.Value;
      if (!Directory.Exists(uploadOptions.AbsolutePath))
      {
        try
        {
          Directory.CreateDirectory(uploadOptions.AbsolutePath);
        }
        catch
        {
          // ignored
        }
      }

      app.UseStaticFiles(new StaticFileOptions
      {
        FileProvider = new PhysicalFileProvider(uploadOptions.AbsolutePath),
        RequestPath = uploadOptions.WebRoot,
        ServeUnknownFileTypes = true
      });

      // 手动初始化全局控制器的单例数据，避免锁
      using var scope = app.ApplicationServices.CreateScope();
      var dbContext = scope.ServiceProvider.GetService<CyberStoneDbContext>();
      var valueSpaceManager = scope.ServiceProvider.GetService<ValueSpaceManager>();
      valueSpaceManager?.Initialize();
      var profileKeyManager = scope.ServiceProvider.GetService<ProfileKeyManager>();
      profileKeyManager?.Initialize();
    }
  }
}