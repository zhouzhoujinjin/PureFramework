using CyberStone.Core.Managers;
using CyberStone.Core.Models;
using CyberStone.Core.Utils;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace CyberStone.Core.Controllers
{
  [ApiController]
  [Route("api/account", Name = "登录用户")]
  public class AccountController : ControllerBase
  {
    private readonly SignInManager signInManager;
    private readonly UserManager userManager;
    private readonly TokenManager tokenManager;
    private readonly MenuManager menuManager;
    private readonly RoleManager roleManager;
    private readonly CaptchaManager captchaManager;

    public AccountController(
        UserManager userManager,
        SignInManager signInManager,
        TokenManager tokenManager,
        MenuManager menuManager,
        RoleManager roleManager,
        CaptchaManager captchaManager
    )
    {
      this.signInManager = signInManager;
      this.userManager = userManager;
      this.tokenManager = tokenManager;
      this.menuManager = menuManager;
      this.roleManager = roleManager;
      this.captchaManager = captchaManager;
    }

    [HttpGet("captcha", Name = "获取验证码")]
    public async Task<AjaxResp<CaptchaModel>> GenerateCaptcha()
    {
      if (!captchaManager.Enabled)
      {
        return new AjaxResp<CaptchaModel>();
      }

      var data = await captchaManager.CreateCodeAsync();

      return new AjaxResp<CaptchaModel>
      {
        Data = data
      };
    }

    /// <summary>
    /// 使用用户名和密码登录
    /// </summary>
    /// <param name="user"></param>
    /// <returns></returns>
    [HttpPost("login", Name = "密码登录")]
    [UserLog(UserLogLevel.Classified)]
    public async Task<AjaxResp<AuthenticationTokens>> Post([FromBody] LoginUser user)
    {
      if (captchaManager.Enabled)
      {
        var captchaCheckResultResult = await captchaManager.CheckCodeAsync(user.CaptchaId, user.CaptchaCode);
        if (captchaCheckResultResult != CaptchaCheckResult.Success)
        {
          return new AjaxResp<AuthenticationTokens>
          {
            Code = 400,
            Message = captchaCheckResultResult == CaptchaCheckResult.Failure ? "验证码错误" : "验证码过期"
          };
        }
      }

      var result = await signInManager.PasswordSignInAsync(user.UserName, user.Password, false, true);

      // 避免出现 Cookie 信息
      Response.Cookies.Delete(".AspNetCore.Identity.Application");
      Response.Headers.Remove("Set-Cookie");

      if (result.IsLockedOut)
      {
        return new AjaxResp<AuthenticationTokens>()
        {
          Code = 0,
          Message = "用户被锁定",
          Data = new AuthenticationTokens()
        };
      }

      if (result.Succeeded)
      {
        var appUser = await userManager.FindByNameAsync(user.UserName);
        if (appUser != null && !appUser.IsDeleted && appUser.IsVisible)
        {
          return new AjaxResp<AuthenticationTokens>()
          {
            Code = 0,
            Message = "登录成功",
            Data = await tokenManager.GenerateTokensAsync(appUser)
          };
        }

      }

      return new AjaxResp<AuthenticationTokens>()
      {
        Code = 5001,
        Message = "登录失败",
        Data = null
      };
    }

    /// <summary>
    /// 获得当前用户的资料
    /// </summary>
    /// <returns></returns>
    [HttpGet("profile", Name = "用户资料")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [UserLog(UserLogLevel.Classified)]
    public async Task<AjaxResp<User>> GetProfile()
    {
      return new AjaxResp<User>
      {
        Data = await userManager.GetBriefUserAsync(HttpContext.GetUserId(), new[] { SystemProfileKeyCategory.Public })
      };
    }

    [HttpPost("profile", Name = "更新资料")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [UserLog(UserLogLevel.Classified)]
    public async Task<AjaxResp<User>> UpdateProfile([FromBody] Dictionary<string, JsonElement> profiles)
    {
      await userManager.AddProfilesAsync(HttpContext.GetUserId(), profiles);

      return new AjaxResp<User>
      {
        Data = await userManager.GetBriefUserAsync(HttpContext.GetUserId(), new[] { SystemProfileKeyCategory.Public }),
        Message = "更新资料成功"
      };
    }

    /// <summary>
    /// 获得用户主菜单
    /// </summary>
    /// <returns></returns>
    [HttpGet("menu", Name = "用户菜单")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [UserLog(UserLogLevel.Classified)]
    public async Task<AjaxResp<ICollection<MenuItem>?>> GetMainMenu()
    {
      var menu = await menuManager.GetMenuAsync();
      var userMenu = await roleManager.FilterMenusAsync(menu!.Value, await userManager.GetUserAsync(HttpContext.User));

      return new AjaxResp<ICollection<MenuItem>?> { Data = userMenu };
    }

    /// <summary>
    /// 上传头像
    /// </summary>
    /// <param name="avatar"></param>
    /// <returns></returns>
    [HttpPost("avatar", Name = "更新头像")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [UserLog(UserLogLevel.Classified)]
    public AjaxResp UploadAvatar(IFormFile avatar)
    {
      var data = userManager.UploadImage(avatar);
      return new AjaxResp { Data = data };
    }

    [HttpGet("permissions", Name = "权限")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [UserLog(UserLogLevel.Classified)]
    public async Task<AjaxResp> GetPermissionsAsync()
    {
      var user = await userManager.GetUserAsync(HttpContext.User);
      if (user == null)
      {
        return new AjaxResp { Code = 404, Message = "没有找到用户" };
      }
      var roles = await userManager.GetRolesAsync(user);
      var routePermissions = await roleManager.GetClaimsAsync(ClaimNames.RoutePermission, roles);
      var actionPermissions = await roleManager.GetClaimsAsync(ClaimNames.ActionPermission, roles);
      return new AjaxResp
      {
        Data = routePermissions.Concat(actionPermissions)
      };
    }

    [HttpPost("password", Name = "更新密码")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Policy = ClaimNames.ApiPermission)]
    [UserLog(UserLogLevel.Classified)]
    public async Task<AjaxResp> UpdatePassword([FromBody] ChangePasswords passwords)
    {
      var user = await userManager.GetUserAsync(HttpContext.User);
      if(user == null)
      {
        return new AjaxResp { Code = 404, Message = "没有找到用户" };
      }
      var result = await userManager.ChangePasswordAsync(user, passwords.OldPassword, passwords.NewPassword);

      return new AjaxResp
      {
        Data = result.Succeeded,
        Message = result.Succeeded ? "修改密码成功" : "修改密码失败: " + string.Join("; ", result.Errors.Select(x => x.Description))
      };
    }
  }
}