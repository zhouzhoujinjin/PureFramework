using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CyberStone.Core.Models;
using CyberStone.Core.Utils;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CyberStone.Core.Controllers
{
  public partial class RoleController
  {
    [HttpGet("/api/briefRoles", Name = "角色扼要列表")]
    [Authorize(Policy = ClaimNames.ApiPermission)]
    [UserLog(UserLogLevel.Classified)]
    public async Task<AjaxResp<IEnumerable<Role>>> ListBriefRolesAsync()
    {
      return new()
      {
        Data = (await roleManager.GetSimpleRolesAsync()).Select(x => new Role
        {
          Name = x.Key,
          Title = x.Value
        }).OrderBy(x => x.Name),
      };
    }
  }
}