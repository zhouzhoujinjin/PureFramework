using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Caching.Distributed;
using CyberStone.Core;
using CyberStone.Core.Entities;
using CyberStone.Core.Managers;
using CyberStone.Core.Utils;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace CyberStone.Authorizations
{
  internal class PermissionAuthorizationHandler : AuthorizationHandler<PermissionRequirement>
  {
    private readonly UserManager _userManager;
    private readonly RoleManager _roleManager;
    private readonly IDistributedCache _cache;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IWebHostEnvironment _env;

    public PermissionAuthorizationHandler(
      UserManager userManager, IWebHostEnvironment env,
      IDistributedCache cache,
      RoleManager roleManager, IHttpContextAccessor httpContextAccessor)
    {
      _userManager = userManager;
      _roleManager = roleManager;
      _cache = cache;
      _httpContextAccessor = httpContextAccessor;
      _env = env;
    }

    private async Task<List<KeyValuePair<string, string>>?> GetClaims(UserEntity user)
    {
      var claims = new List<Claim> {
          new Claim(JwtRegisteredClaimNames.NameId, user.Id.ToString())
        };
      var userRoleNames = await _userManager.GetRolesAsync(user);
      var userRoles = _roleManager.Roles.Where(x => userRoleNames.Contains(x.Name!)).ToList();
      foreach (var role in userRoles)
      {
        var roleClaims = await _roleManager.GetClaimsAsync(role);
        var permissions = roleClaims.Where(x => x.Type == ClaimNames.ApiPermission);
        claims.AddRange(permissions);
      }
      return claims.Distinct(EqualityFactory.Create<Claim>((x, y) => x != null && y != null && x.Value == y.Value)).Select(x => new KeyValuePair<string, string>(x.Type, x.Value)).ToList();
    }

    protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, PermissionRequirement requirement)
    {
      var user = await _userManager.GetUserAsync(context.User);
      if(user == null)
      {
        context.Fail();
        return;
      }
      var cachedClaimKey = string.Format(CacheKeys.UserClaims, user.Id);
      var claimPairs = await _cache.GetAsync(
        cachedClaimKey!,
        async () => await GetClaims(user!),
        new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(3600) }
      );

      if (claimPairs == null)
      {
        return;
      }
      var claims = claimPairs.Select(x => new Claim(x.Key, x.Value));

      var appIdentity = new ClaimsIdentity(claims);
      context.User.AddIdentity(appIdentity);

      if (_env.EnvironmentName == "Development")
      {
        context.Succeed(requirement);
      }

      var endpoint = (context.Resource as HttpContext)!.GetEndpoint() as RouteEndpoint;
      var template = endpoint?.RoutePattern.RawText;
      var method = _httpContextAccessor.HttpContext!.Request.Method.ToUpper();

      var permission = $"{method} {template}";

      if (claims.Any(x => x.Value == permission))
      {
        context.Succeed(requirement);
      }
    }
  }
}