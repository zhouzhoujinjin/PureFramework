using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using CyberStone.Core.Entities;
using CyberStone.Core.Models;
using CyberStone.Core.Utils;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace CyberStone.Core.Managers
{
  public class UserQueryModel
  {
    public IQueryable<UserEntity> Users { get; set; } = null!;
    public IEnumerable<string> ProfileKeyNames { get; set; } = null!;
  }

  public partial class UserManager : UserManager<UserEntity>
  {
    private readonly IDistributedCache cache;
    private readonly CyberStoneDbContext context;
    private readonly ProfileKeyMap profileKeyMap;
    private readonly UploadOptions uploadOptions;
    private readonly PasswordSecurityOptions passwordSecurityOptions;
    private readonly DbSet<UserProfileEntity> userProfiles;

    public UserManager(
      CyberStoneDbContext context,
      ProfileKeyMap profileKeyMap,
      IOptions<UploadOptions> uploadOptionsAccessor,
      IUserStore<UserEntity> store,
      IOptions<IdentityOptions> optionsAccessor,
      IPasswordHasher<UserEntity> passwordHasher,
      IEnumerable<IUserValidator<UserEntity>> userValidators,
      IEnumerable<IPasswordValidator<UserEntity>> passwordValidators,
      ILookupNormalizer keyNormalizer,

        IOptions<PasswordSecurityOptions> passwordSecurityOptionsAccessor,
      IdentityErrorDescriber errors,
      IServiceProvider services,
      ILogger<UserManager<UserEntity>> logger,
      IDistributedCache cache
    ) : base(
      store, optionsAccessor, passwordHasher, userValidators,
      passwordValidators, keyNormalizer, errors, services, logger
    )
    {
      this.cache = cache;
      this.context = context;
      this.profileKeyMap = profileKeyMap;
      userProfiles = context.UserProfiles;
      uploadOptions = uploadOptionsAccessor.Value;
      passwordSecurityOptions = passwordSecurityOptionsAccessor.Value;
    }

    public async Task AddOrUpdateClaimAsync(UserEntity user, string type, string value)
    {
      var claim = await context.UserClaims.FirstOrDefaultAsync(x => x.UserId == user.Id && x.ClaimType == type);
      if (claim == null)
      {
        await AddClaimAsync(user, new Claim(type, value));
      }
      else
      {
        claim.ClaimValue = value;
        await context.SaveChangesAsync();
      }
    }

    public async Task AddOrUpdateClaimsAsync(UserEntity user, Dictionary<string, string> claims)
    {
      context.UserClaims.RemoveRange(context.UserClaims.Where(x =>
        x.UserId == user.Id && claims.ContainsKey(x.ClaimType!)));
      await context.SaveChangesAsync();
      await AddClaimsAsync(user, claims.ToArray().Select(x => new Claim(x.Key, x.Value)));
    }

    public async Task<IList<Claim>> GetClaimsAsync(long userId)
    {
      var entity = await FindByIdAsync(userId.ToString());
      return await GetClaimsAsync(entity!);
    }

    public override string? GetUserName(ClaimsPrincipal principal)
    {
      return principal.Claims.Where(x => x.Type == ClaimTypes.Name).Select(x => x.Value)
        .FirstOrDefault();
    }

    public override async Task<UserEntity?> GetUserAsync(ClaimsPrincipal principal)
    {
      if (principal == null)
      {
        throw new ArgumentNullException(nameof(principal));
      }

      var name = GetUserName(principal);
      if (name == null)
      {
        return await Task.FromResult<UserEntity>(null!);
      }

      return await FindByNameAsync(name);
    }

    public async Task<(IEnumerable<AdminUser>, int)> ListUsersWithRolesAsync(Dictionary<string, string> conditions,
      int page, int size)
    {
      var userQuery = BuildFindUsersQuery(conditions);
      var users = userQuery.Users.Include(u => u.UserRoles).ThenInclude(ur => ur.Role);

      var count = await users.CountAsync();
      var data = await users.OrderBy(u => u.UserName).Skip(Math.Max(page - 1, 0) * size).Take(size).ToArrayAsync();
      var adminUsers = new List<AdminUser>();
      foreach (var u in data)
      {
        var user = await GetBriefUserAsync(u, [SystemProfileKeyCategory.Public]);
        var adminUser = AdminUser.FromUser(user!);
        adminUser.Roles = u.UserRoles.ToDictionary(ur => ur.Role.Name, ur => ur.Role.Title);
        adminUser.IsDeleted = u.IsDeleted;
        adminUser.IsVisible = u.IsVisible;
        var profiles = adminUser.Profiles;

        if (profiles != null)
        {
          adminUser.CreatorId = profiles.ContainsKey("CreatorId") ? (long)profiles["CreatorId"] : default;
          adminUser.CreatorName = profiles.ContainsKey("CreatorName") ? profiles["CreatorName"].ToString() : null;
          adminUser.FullName = profiles.ContainsKey("FullName") ? profiles["FullName"].ToString() : null;
        }
        adminUsers.Add(adminUser);
      }

      return (adminUsers, count);
    }

    public async Task<IEnumerable<UserEntity>> GetUsersForClaimTypeAsync(string claimType)
    {
      return await context.Users.Where(u => u.UserClaims.Select(x => x.ClaimType).Contains(claimType)).ToListAsync();
    }

    public async Task<IEnumerable<User>> FindUsersAsync(
      Dictionary<string, string> conditions,
      int page = 0,
      int size = 1000
    )
    {
      var userQuery = BuildFindUsersQuery(conditions);
      var userIds = await userQuery.Users.Skip(Math.Max(page - 1, 0) * size).Take(size).Select(u => u.Id)
        .ToArrayAsync();
      var result = await GetBriefUsersAsync(userIds, userQuery.ProfileKeyNames);
      return result;
    }

    public async Task<int> FindUsersCountAsync(Dictionary<string, string> conditions)
    {
      var userQuery = BuildFindUsersQuery(conditions);
      return await userQuery.Users.CountAsync();
    }

    public UserQueryModel BuildFindUsersQuery(
      Dictionary<string, string> conditions
    )
    {
      IQueryable<UserEntity> users = Users;
      var profileKeyNames = new HashSet<string>();
      foreach (var kv in conditions)
      {
        if (kv.Key == "visible")
        {
          users = users.Where(u => u.IsVisible == bool.Parse(kv.Value));
        }
        else if (kv.Key == "deleted")
        {
          users = users.Where(u => u.IsDeleted == bool.Parse(kv.Value));
        }
        else if (kv.Key == "userName")
        {
          var ups = userProfiles
            .Where(up => up.FullName == "fullName" && up.Value.StartsWith(kv.Value))
            .Distinct().Select(up => up.UserId);
          users = users.Where(u => u.NormalizedUserName.StartsWith(kv.Value.ToUpper()) || ups.Contains(u.Id));
        }
        else if (kv.Key == "role")
        {
          var roles = kv.Value.Split(",").Select(x => x.ToUpper());
          users = users.Where(u => u.UserRoles.Select(x => x.Role.NormalizedName).Intersect(roles).Any());
        }
        else if (kv.Key == "any")
        {
          var ups = userProfiles.Where(up => up.ProfileKey.IsSearchable && up.Value.StartsWith(kv.Value)).Distinct()
            .Select(up => up.UserId);
          users = users.Where(u => ups.Contains(u.Id));
        }
        else
        {
          var ups = userProfiles
            .Where(up => up.FullName == kv.Key && up.Value.StartsWith(kv.Value))
            .Distinct().Select(up => up.UserId);
          users = users.Where(u => ups.Contains(u.Id));
          profileKeyNames.Add(kv.Key);
        }
      }

      if (!conditions.ContainsKey("deleted"))
      {
        users = users.Where(u => u.IsDeleted == false);
      }

      profileKeyMap.GetBriefKeys().ForEach(p => profileKeyNames.Add(p.Name));
      return new UserQueryModel
      {
        Users = users,
        ProfileKeyNames = profileKeyNames
      };
    }

    public async Task<bool> IsExistUserAsync(string userName)
    {
      var user = await Users.Where(u => u.UserName.Equals(userName)).FirstOrDefaultAsync();
      return user != null;
    }

    public async Task<UserEntity?> AddUserAsync(string userName, bool isVisible, string password = "123456")
    {
      var entity = new UserEntity
      {
        UserName = userName,
        IsDeleted = false,
        IsVisible = isVisible
      };
      entity.PasswordHash = PasswordHasher.HashPassword(entity, password);
      var result = await this.CreateAsync(entity);
      return result.Succeeded ? entity : null;
    }

    public async Task<bool> SetDeletedAsync(string userName, bool isDeleted)
    {
      var entity = await Users.FirstOrDefaultAsync(x => x.UserName.Equals(userName));
      if (entity != null)
      {
        entity.IsDeleted = isDeleted;
        var result = await UpdateAsync(entity);
        return result == IdentityResult.Success;
      }

      return false;
    }

    public async Task<bool> ResetPasswordAsync(string userName)
    {
      var entity = await Users.FirstOrDefaultAsync(x => x.UserName.Equals(userName));
      if (entity != null)
      {
        var defaultPassword = passwordSecurityOptions.DefaultPassword;
        switch (passwordSecurityOptions.FrontendHash)
        {
          case "md5":
            defaultPassword = defaultPassword!.ComputeMd5().ToLower(); break;

          default:
            break;
        }

        entity.PasswordHash = PasswordHasher.HashPassword(entity, "123456");
        var result = await UpdateAsync(entity);
        return result == IdentityResult.Success;
      }

      return false;
    }

    public async Task UpdateUserRoleAsync(string roleName, IEnumerable<User> users)
    {
      var existUsers = await GetUsersInRoleAsync(roleName);
      if (existUsers.Count > 0)
      {
        foreach (var o in existUsers)
        {
          await RemoveFromRoleAsync(o, roleName);
          await cache.RemoveAsync(string.Format(CacheKeys.UserClaims, o.Id));
        }
      }

      await AddUserRoleAsync(roleName, users);
    }

    public async Task AddUserRoleAsync(string roleName, IEnumerable<User> users)
    {
      foreach (var item in users)
      {
        var user = await FindByIdAsync(item.Id.ToString());
        await AddToRoleAsync(user, roleName);
        await cache.RemoveAsync(string.Format(CacheKeys.UserClaims, user.Id));
      }
    }

    public async Task DeleteUserRoleAsync(string roleName)
    {
      var users = await GetUsersInRoleAsync(roleName);
      await users.ForEachAsync(async o =>
      {
        await cache.RemoveAsync(string.Format(CacheKeys.UserClaims, o.Id));
        await RemoveFromRoleAsync(o, roleName);
      });
    }

#pragma warning disable CA1822 // 将成员标记为 static

    public bool HasClaim(ClaimsPrincipal principal, string claimType, string claimValue)
    {
      return principal.Claims.Any(x => x.Type == claimType && x.Value == claimValue);
    }

#pragma warning restore CA1822 // 将成员标记为 static
  }
}