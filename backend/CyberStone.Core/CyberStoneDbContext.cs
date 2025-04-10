using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using CyberStone.Core.Entities;
using CyberStone.Core.Utils;
using System;

namespace CyberStone.Core
{
  public class CyberStoneDbContext : IdentityDbContext<
      UserEntity, RoleEntity, long, UserClaimEntity, UserRoleEntity,
      IdentityUserLogin<long>, RoleClaimEntity, IdentityUserToken<long>>
  {
    private readonly string dbType = "mssql";
    public CyberStoneDbContext()
    {
    }

    public CyberStoneDbContext(DbContextOptions<CyberStoneDbContext> options, IConfiguration configuration) : base(options)
    {
      dbType = configuration.GetValue<string>("DatabaseType")?.ToLower() ?? "mssql";
    }

    public DbSet<SettingEntity> Settings { get; set; } = null!;
    public DbSet<ProfileKeyEntity> ProfileKeys { get; set; } = null!;
    public DbSet<ValueSpaceEntity> ValueSpaces { get; set; } = null!;
    public DbSet<UserLogEntity> UserLogs { get; set; } = null!;
    public DbSet<UserProfileEntity> UserProfiles { get; set; } = null!;

    public DbSet<DepartmentEntity> Departments { get; set; } = null!;
    public DbSet<DepartmentUserEntity> DepartmentUsers { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder builder)
    {
      base.OnModelCreating(builder);

      builder.Entity<ProfileKeyEntity>(entity =>
      {
        entity.ToTable("Core_ProfileKey").HasKey(e => e.Id);
        entity.Property(e => e.Id).ValueGeneratedOnAdd();
        entity.HasOne(e => e.ValueSpace).WithMany().HasForeignKey(e => e.ValueSpaceId);
        entity.Property(e => e.ProfileType).HasConversion<string?>(x => x != null ? x.FullName : null, x => x != null ? Type.GetType(x) : null);
        entity.HasMany<UserProfileEntity>().WithOne(e => e.ProfileKey).HasForeignKey(e => e.ProfileKeyId);
      });

      builder.Entity<RoleEntity>(entity =>
      {
        entity.ToTable("Core_Role").HasKey(e => e.Id);
        entity.Property(e => e.Id).ValueGeneratedOnAdd();
        entity.HasMany(e => e.RoleClaims).WithOne(e => e.Role).HasForeignKey(e => e.RoleId);
        entity.HasMany(e => e.UserRoles).WithOne(e => e.Role).HasForeignKey(c => c.RoleId);
      });
      builder.Entity<RoleClaimEntity>(entity =>
      {
        entity.ToTable("Core_RoleClaim").HasKey(e => e.Id);
        entity.Property(e => e.Id).ValueGeneratedOnAdd();
        entity.Property(e => e.RoleId).HasColumnName("RoleId");
      });

      builder.Entity<SettingEntity>(entity =>
      {
        entity.ToTable("Core_Setting").HasKey(e => e.Id);
        entity.Property(e => e.Id).ValueGeneratedOnAdd();
        entity.Ignore(e => e.IsGlobal);
      });

      builder.Entity<UserEntity>(entity =>
      {
        entity.ToTable("Core_User");
        entity.Property(e => e.Id).ValueGeneratedOnAdd();

        entity.HasMany(u => u.UserClaims).WithOne().HasForeignKey(c => c.UserId);
        entity.HasMany(u => u.UserRoles).WithOne(e => e.User).HasForeignKey(c => c.UserId);
        entity.HasMany(u => u.UserProfiles).WithOne(e => e.User).HasForeignKey(c => c.UserId);
      });

      builder.Entity<UserProfileEntity>(entity =>
      {
        entity.ToTable("Core_UserProfile");
        entity.Property(e => e.Id).ValueGeneratedOnAdd();
      });

      builder.Entity<UserClaimEntity>(entity =>
      {
        entity.ToTable("Core_UserClaim");
        entity.Property(e => e.Id).ValueGeneratedOnAdd();
        entity.Property(e => e.UserId).HasColumnName("UserId");
      });

      builder.Entity<IdentityUserLogin<long>>(entity =>
      {
        entity.ToTable("Core_UserLogin");
        entity.Property(e => e.UserId).HasColumnName("UserId");
      });

      builder.Entity<IdentityUserToken<long>>(entity =>
      {
        entity.ToTable("Core_UserToken");
        entity.Property(e => e.UserId).HasColumnName("UserId");
      });
      builder.Entity<UserRoleEntity>(entity =>
      {
        entity.ToTable("Core_UserRole").HasKey(e => new { e.UserId, e.RoleId });
        entity.HasOne(userRole => userRole.Role).WithMany(role => role.UserRoles)
                  .HasForeignKey(userRole => userRole.RoleId);
        entity.HasOne(userRole => userRole.User).WithMany(user => user.UserRoles)
                  .HasForeignKey(userRole => userRole.UserId);
      });

      builder.Entity<UserLogEntity>(entity =>
      {
        entity.ToTable("Core_UserLog").HasKey(e => e.Id);
        entity.Property(e => e.Id).ValueGeneratedOnAdd();
      });

      builder.Entity<ValueSpaceEntity>(entity =>
      {
        entity.ToTable("Core_ValueSpace").HasKey(e => e.Id);
        entity.Property(e => e.Id).ValueGeneratedOnAdd();
        entity.Property(e => e.ValueSpaceType).HasColumnName("Type").HasConversion<string>();
        entity.Property(e => e.ConfigureLevel).HasColumnName("ConfigureLevelCode").HasConversion<string>();
      });

      builder.Entity<DepartmentEntity>(entity =>
      {
        entity.ToTable("Core_Department").HasKey(e => e.Id);
        entity.Property(e => e.Id).ValueGeneratedOnAdd();
        entity.HasOne(e => e.Parent).WithMany(e => e.Children).HasForeignKey(e => e.ParentId);
        entity.HasOne(e => e.Creator).WithMany().HasForeignKey(e => e.CreatorId);
      });

      builder.Entity<DepartmentUserEntity>(entity =>
      {
        entity.ToTable("Core_DepartmentUser").HasKey(e => e.Id);
        entity.Property(e => e.Id).ValueGeneratedOnAdd();
        entity.HasOne(e => e.Department).WithMany(e => e.Users).HasForeignKey(e => e.DepartmentId);
        entity.HasOne(e => e.User).WithMany().HasForeignKey(e => e.UserId);
      });

      builder.Entity<TreeNodeEntity>(entity =>
      {
        entity.ToTable("Core_TreeNode").HasKey(e => e.Id);
        entity.Property(e => e.Id).ValueGeneratedOnAdd();
        entity.Property(e => e.ExtendData).HasJsonConversion();
        entity.HasOne(e => e.Parent).WithMany(e => e.Children).HasForeignKey(e => e.ParentId);

        if (dbType == "oracle")
        {
          entity.Property(e => e.Id).HasValueGenerator((_, __) => new LongSequenceValueGenerator("S_CORE_TREENODE"));
        }
      });

      if (dbType == "oracle")
      {
        OracleModelCreating(builder);
      }
    }

    private static void OracleModelCreating(ModelBuilder builder)
    {
      builder.Entity<ProfileKeyEntity>(entity =>
      {
        entity.Property(e => e.Id).HasValueGenerator((_, __) => new LongSequenceValueGenerator("S_CORE_PROFILEKEY"));
      });

      builder.Entity<RoleEntity>(entity =>
      {
        entity.Property(e => e.Id).HasValueGenerator((_, __) => new LongSequenceValueGenerator("S_CORE_ROLE"));
      });
      builder.Entity<RoleClaimEntity>(entity =>
      {
        entity.Property(e => e.Id).HasValueGenerator((_, __) => new IntSequenceValueGenerator("S_CORE_ROLECLAIM"));
      });

      builder.Entity<SettingEntity>(entity =>
      {
        entity.Property(e => e.Id).HasValueGenerator((_, __) => new LongSequenceValueGenerator("S_CORE_SETTING"));
        entity.Property(e => e.Value).HasColumnType("CLOB");
      });

      builder.Entity<UserEntity>(entity =>
      {
        entity.Property(e => e.Id).HasValueGenerator((_, __) => new LongSequenceValueGenerator("S_CORE_USER"));
      });

      builder.Entity<UserProfileEntity>(entity =>
      {
        entity.Property(e => e.Id).HasValueGenerator((_, __) => new LongSequenceValueGenerator("S_CORE_USERPROFILE"));
      });

      builder.Entity<UserClaimEntity>(entity =>
      {
        entity.Property(e => e.Id).HasValueGenerator((_, __) => new IntSequenceValueGenerator("S_CORE_USERCLAIM"));
      });

      builder.Entity<UserLogEntity>(entity =>
      {
        entity.Property(e => e.Id).HasValueGenerator((_, __) => new LongSequenceValueGenerator("S_CORE_USERLOG"));
      });

      builder.Entity<ValueSpaceEntity>(entity =>
      {
        entity.Property(e => e.Id).HasValueGenerator((_, __) => new LongSequenceValueGenerator("S_CORE_VALUESPACE"));
      });

      builder.Entity<DepartmentEntity>(entity =>
      {
        entity.Property(e => e.Id).HasValueGenerator((_, __) => new LongSequenceValueGenerator("S_CORE_DEPARTMENT"));
      });

      builder.Entity<DepartmentUserEntity>(entity =>
      {
        entity.Property(e => e.Id).HasValueGenerator((_, __) => new LongSequenceValueGenerator("S_CORE_DEPARTMENTUSER"));
      });
    }
  }
}