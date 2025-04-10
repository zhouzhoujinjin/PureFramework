using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CyberStone.Core.Managers;
using CyberStone.Core.Models;
using CyberStone.Core.Utils;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CyberStone.Core.Controllers
{
  [ApiController]
  [Route("api/admin/system/menu", Name = "菜单")]
  [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Policy = ClaimNames.ApiPermission)]
  public class MenuController : ControllerBase
  {
    private readonly MenuManager menuManager;

    public MenuController(MenuManager menuManager)
    {
      this.menuManager = menuManager;
    }

    [HttpGet(Name = "全部菜单")]
    [UserLog(UserLogLevel.Classified)]
    public async Task<AjaxResp<ICollection<MenuItem>?>> IndexAsync()
    {
      var menus = await menuManager.GetMenuAsync();
      return new AjaxResp<ICollection<MenuItem>?> { Data = menus?.Value };
    }

    [HttpPost(Name = "更新菜单")]
    [UserLog(UserLogLevel.Classified)]
    public async Task<AjaxResp> UpdateAsync([FromBody] ICollection<MenuItem>? menu)
    {
      await menuManager.UpdateMenusAsync(menu);

      return new AjaxResp { Message = $"更新菜单成功" };
    }
  }
}