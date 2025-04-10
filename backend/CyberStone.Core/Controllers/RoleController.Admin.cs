using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CyberStone.Core.Managers;
using CyberStone.Core.Models;
using CyberStone.Core.Utils;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CyberStone.Core.Controllers
{
  [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
  [Route("api/admin/roles", Name = "角色")]
  public partial class RoleController : ControllerBase
  {
    private readonly UserManager userManager;
    private readonly RoleManager roleManager;

    public RoleController(RoleManager roleManager, UserManager userManager)
    {
      this.userManager = userManager;
      this.roleManager = roleManager;
    }

    /// <summary>
    /// 获取所有后端的权限点
    /// </summary>
    /// <returns></returns>
    [HttpGet("/api/admin/permissions", Name = "权限点列表")]
    [Authorize(Policy = ClaimNames.ApiPermission)]
    [UserLog(UserLogLevel.Classified)]
    public AjaxResp<IEnumerable<PermissionModel>> Permissions()
    {
      return new AjaxResp<IEnumerable<PermissionModel>>
      {
        Data = roleManager.GetPermissionActions()
      };
    }

    /// <summary>
    /// 分页获取角色列表（包括角色中的用户）
    /// </summary>
    /// <param name="page"></param>
    /// <param name="size"></param>
    /// <returns></returns>
    [HttpGet(Name = "角色列表")]
    [Authorize(Policy = ClaimNames.ApiPermission)]
    [UserLog(UserLogLevel.Classified)]
    public async Task<PagedAjaxResp<Role>> IndexAsync(int page = 1, int size = 10)
    {
      return new PagedAjaxResp<Role>
      {
        Data = await roleManager.GetRolesWithUsersAsync(page, size),
        Total = await roleManager.Roles.CountAsync(),
        Page = page
      };
    }

    [HttpGet("{name}", Name = "角色详情")]
    [Authorize(Policy = ClaimNames.ApiPermission)]
    [UserLog(UserLogLevel.Classified)]
    public async Task<AjaxResp<Role>> GetRole(string name)
    {
      return new AjaxResp<Role>
      {
        Code = 0,
        Data = await roleManager.GetRoleWithUsersAndClaimsAsync(name)
      };
    }

    [HttpPost(Name = "添加角色")]
    [UserLog(UserLogLevel.Classified)]
    public async Task<AjaxResp> Add([FromBody] Role rwuac)
    {
      var role = await roleManager.AddRoleAsync(rwuac.Name, rwuac.Title ?? "");
      if (role != null)
      {
        var claims = roleManager.GetClaims(rwuac.Claims);
        await roleManager.AddClaimsAsync(role, claims);
        await userManager.AddUserRoleAsync(rwuac.Name, rwuac.Users);
      }
      return new AjaxResp { Message = "保存成功" };
    }

    [HttpPut("{name}", Name = "修改角色")]
    [UserLog(UserLogLevel.Classified)]
    public async Task<AjaxResp> Update(string name, [FromBody] Role roleWithUsersAndClaims)
    {
      var claims = roleManager.GetClaims(roleWithUsersAndClaims.Claims);
      await roleManager.UpdateRoleAsync(name, roleWithUsersAndClaims.Title ?? "");
      await roleManager.UpdateClaimAsync(name, claims);
      await userManager.UpdateUserRoleAsync(name, roleWithUsersAndClaims.Users);
      return new AjaxResp { Message = "保存成功" };
    }

    [HttpGet("{name}/isExist", Name = "判断角色是否存在")]
    [UserLog(UserLogLevel.Classified)]
    public async Task<AjaxResp<bool>> IsExistRole(string name)
    {
      var data = await roleManager.IsExistRoleAsync(name);
      return new AjaxResp<bool> { Data = data };
    }

    [HttpDelete("{roleName}", Name = "删除角色")]
    [UserLog(UserLogLevel.Classified)]
    public async Task<AjaxResp> Delete(string roleName)
    {
      await roleManager.DeleteRoleClaimAsync(roleName);
      await userManager.DeleteUserRoleAsync(roleName);
      await roleManager.DeleteRoleAsync(roleName);
      return new AjaxResp { Message = "操作成功" };
    }
  }
}