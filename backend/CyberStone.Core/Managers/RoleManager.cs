using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ActionConstraints;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using CyberStone.Core.Entities;
using CyberStone.Core.Models;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using CyberStone.Core.Utils;

namespace CyberStone.Core.Managers
{
  public class RoleMap
  {
    public SortedDictionary<string, IList<PermissionModel>> Value { get; set; } = [];
  }

  public class RoleManager : RoleManager<RoleEntity>
  {
    private readonly CyberStoneDbContext context;
    private readonly IActionDescriptorCollectionProvider provider;
    private readonly UserManager userManager;
    private readonly IDistributedCache cache;

    public RoleManager(
      CyberStoneDbContext context,
        IRoleStore<RoleEntity> store,
        IEnumerable<IRoleValidator<RoleEntity>> roleValidators,
        ILookupNormalizer keyNormalizer,
        IdentityErrorDescriber errors,
        IActionDescriptorCollectionProvider provider,
        IDistributedCache cache,
        ILogger<RoleManager<RoleEntity>> logger,
        UserManager userManager
    ) : base(
        store,
        roleValidators,
        keyNormalizer,
        errors,
        logger
    )
    {
      this.context = context;
      this.provider = provider;
      this.userManager = userManager;
      this.cache = cache;
    }

    public async Task<IEnumerable<string?>> GetClaimsAsync(string claimType, IEnumerable<string> roleNames)
    {
      return await Roles.Include(r => r.RoleClaims).Where(r => roleNames.Contains(r.Name)).SelectMany(r => r.RoleClaims).Where(c => c.ClaimType == claimType).Select(c => c.ClaimValue).ToArrayAsync();
    }

    public async Task<SortedDictionary<string, string>> GetSimpleRolesAsync()
    {
      var d = await Roles.ToDictionaryAsync(r => r.Name!, r => r.Title);
      return new SortedDictionary<string, string>(d);
    }

    public IEnumerable<PermissionModel> GetPermissionActions()
    {
      var groups = new Dictionary<string, string>();
      var permissions = provider.ActionDescriptors.Items.Where(x => x.EndpointMetadata.Any(y => y is AuthorizeAttribute && (y as AuthorizeAttribute)?.Policy == ClaimNames.ApiPermission))
      .Select(x =>
      {
        var group = (x as ControllerActionDescriptor)?.ControllerName!;
        if (!groups.TryGetValue(group, out var friendlyGroup))
        {
          friendlyGroup = ((x as ControllerActionDescriptor)?.ControllerTypeInfo?.GetCustomAttributes(typeof(RouteAttribute), false)?.First() as RouteAttribute)?.Name ?? group;
          groups[group] = friendlyGroup;
        }
        return new PermissionModel
        {
          Name = x.AttributeRouteInfo!.Name!,
          Value = $"{x?.ActionConstraints?.OfType<HttpMethodActionConstraint>().FirstOrDefault()?.HttpMethods.First()} {x!.AttributeRouteInfo.Template}",
          Group = friendlyGroup
        };
      });
      return permissions;
    }

    public async Task<IEnumerable<Role>> GetRolesWithUsersAsync(int page, int size)
    {
      var roles = await Roles.Include(x => x.UserRoles).ThenInclude(x => x.User)
          .Skip(Math.Max(page - 1, 0) * size)
          .Take(size).Select(x => new Role
          {
            Name = x.Name!,
            Title = x.Title,
            Users = x.UserRoles.Where(x => !x.User.IsDeleted).Select(u => new User
            {
              Id = u.UserId
            }).ToList()
          }).ToListAsync();
      foreach (var role in roles)
      {
        for (var i = 0; i < role.Users.Count; i++)
        {
          role.Users[i] = await userManager.GetBriefUserAsync(role.Users[i].Id, new string[] { "fullName", "avatar" });
        }
      }
      return roles;
    }

    public async Task<Role> GetRoleWithUsersAndClaimsAsync(string name)
    {
      var roleEntity = await FindByNameAsync(name);
      var claims = await GetClaimsAsync(roleEntity);
      var users = await context.UserRoles.Include(x => x.User)
          .Where(r => r.RoleId == roleEntity.Id).Select(x => new User
          {
            Id = x.UserId
          }).ToListAsync();

      var role = new Role
      {
        Name = roleEntity.Name,
        Title = roleEntity.Title,
        Users = new List<User>()
      };
      for (var i = 0; i < users.Count; i++)
      {
        role.Users.Add(await userManager.GetBriefUserAsync(users[i].Id, new string[] { "public", "fullName", "avatar", "phoneNumber" }));
      }
      role.Claims = claims.Select(c => c.Value).ToList();
      return role;
    }

    public async Task<bool> CheckPermission(UserEntity user, string permission)
    {
      var claims = (await cache.GetAsync(string.Format(CacheKeys.UserClaims, user.Id), async () =>
      {
        var claims = new List<Claim> {
          new Claim(JwtRegisteredClaimNames.NameId, user.Id.ToString())
        };
        var userRoleNames = await userManager.GetRolesAsync(user);
        var userRoles = Roles.Where(x => userRoleNames.Contains(x.Name)).ToList();
        foreach (var role in userRoles)
        {
          var roleClaims = await GetClaimsAsync(role);
          var permissions = roleClaims.Where(x => x.Type == ClaimNames.ApiPermission);
          claims.AddRange(permissions);
        }
        return claims.Distinct(EqualityFactory.Create<Claim>((x, y) => x != null && y != null && x.Value == y.Value)).Select(x => new KeyValuePair<string, string>(x.Type, x.Value)).ToList();
      }, new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(3600) }))?.Select(x => new Claim(x.Key, x.Value));
      return claims == null ? false : claims.Where(x => x.Value == permission).Any();
    }

    public async Task<ICollection<MenuItem>?> FilterMenusAsync(ICollection<MenuItem>? routes, UserEntity? user)
    {
      if (user == null)
      {
        return null;
      }
      var roles = await Roles.Where(x => x.UserRoles.Any(y => y.UserId == user.Id)).ToArrayAsync();
      var allClaims = new HashSet<string>();
      foreach (var r in roles)
      {
        (await GetClaimsAsync(r))
            .Where(c => c.Type == ClaimNames.RoutePermission)
            .Select(c => c.Value)
            .ForEach(c => allClaims.Add(c));
      }

      return FilterRoutes(routes, allClaims);
    }

    private ICollection<MenuItem>? FilterRoutes(ICollection<MenuItem>? routes, IEnumerable<string> includePaths)
    {
      var resultRoutes = new List<MenuItem>();
      if (routes != null)
      {
        routes.ForEach(r =>
        {
          if (includePaths.Contains(r.Path))
          {
            resultRoutes.Add(r);
            r.Children = FilterRoutes(r.Children, includePaths);
          }
        });
      }
      return resultRoutes;
    }

#pragma warning disable CA1822 // 将成员标记为 static

    public IList<Claim> GetClaims(IEnumerable<string> claims)
    {
      IList<Claim> claimList = new List<Claim>();
      claims.ForEach(claim =>
      {
        string[] typeAndValue = claim.Split(new char[] { ',' });
        claimList.Add(new Claim(typeAndValue[0], typeAndValue[1]));
      });
      return claimList;
    }

#pragma warning restore CA1822 // 将成员标记为 static

    public async Task UpdateRoleAsync(string roleName, string title)
    {
      var role = await FindByNameAsync(roleName);
      if (role == null)
      {
        return;
      }
      role.Title = title ?? role.Title;
      await UpdateAsync(role);
    }

    public async Task UpdateClaimAsync(string roleName, IList<Claim> claims)
    {
      var role = await FindByNameAsync(roleName);
      if (role == null)
      {
        return;
      }
      var existClaims = await GetClaimsAsync(role);
      await existClaims.ForEachAsync(o => RemoveClaimAsync(role, o));
      await AddClaimsAsync(role, claims);
    }

    public async Task<bool> IsExistRoleAsync(string roleName)
    {
      var user = await Roles.FirstOrDefaultAsync(o => o.Name.Equals(roleName));
      return user != null;
    }

    public async Task<RoleEntity?> AddRoleAsync(string roleName, string roleTitle)
    {
      var entity = new RoleEntity
      {
        Name = roleName,
        Title = roleTitle
      };
      var ir = await this.CreateAsync(entity);
      if (ir.Succeeded)
      {
        return entity;
      }
      return null;
    }

    public async Task AddClaimsAsync(RoleEntity role, IList<Claim> claims)
    {
      if (claims.Count > 0)
      {
        foreach (var item in claims)
        {
          await AddClaimAsync(role, item);
        }
      }
    }

    public async Task<bool> DeleteRoleAsync(string roleName)
    {
      var entity = await FindByNameAsync(roleName);
      var ir = await DeleteAsync(entity);
      return ir.Succeeded;
    }

    public async Task DeleteRoleClaimAsync(string roleName)
    {
      var role = await FindByNameAsync(roleName);
      var claims = await GetClaimsAsync(role);
      await claims.ForEachAsync(o => RemoveClaimAsync(role, o));
    }
  }
}