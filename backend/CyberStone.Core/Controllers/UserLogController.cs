using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CyberStone.Core.Managers;
using CyberStone.Core.Models;
using CyberStone.Core.Utils;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;

namespace CyberStone.Core.Controllers
{
  [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Policy = ClaimNames.ApiPermission)]
  [Route("/api/admin/userLogs")]
  public class UserLogController : ControllerBase
  {
    private readonly UserLogManager userLogManager;

    public UserLogController(UserLogManager userLogManager)
    {
      this.userLogManager = userLogManager;
    }

    [HttpGet(Name = "日志列表")]
    [UserLog(UserLogLevel.Classified)]
    public async Task<PagedAjaxResp<UserLog>> IndexAsync(string? query = null, int page = 1, int size = 20)
    {
      var dict = query != null
            ? JsonSerializer.Deserialize<Dictionary<string, string>>(query)!
            : new Dictionary<string, string>();

      var (list, total) = await userLogManager.ListUserLogsAsync(dict, page, size);
      return PagedAjaxResp<UserLog>.Create(list, total, page, size);
    }
  }
}