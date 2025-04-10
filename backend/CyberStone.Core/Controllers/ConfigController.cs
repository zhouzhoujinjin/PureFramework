using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CyberStone.Core.Managers;
using CyberStone.Core.Models;
using CyberStone.Core.Utils;
using System.Threading.Tasks;

namespace CyberStone.Core.Controllers
{
  public abstract class ConfigControllerBase<T> : ControllerBase where T : GlobalSettings, new()
  {
    private readonly ConfigManager<T> configManager;

    public ConfigControllerBase(ConfigManager<T> configManager)
    {
      this.configManager = configManager;
    }

    [Route("/WW_verify_{code}.txt")]
    public string Index([FromRoute] string code)
    {
      return code;
    }

    [HttpGet("/api/config", Name = "系统设置")]
    [UserLog(UserLogLevel.Classified)]
    public async Task<AjaxResp<T>> IndexAsync()
    {
      var config = await configManager.GetAllConfigAsync();
      return new AjaxResp<T> { Data = config };
    }
    [HttpPut("/api/admin/config", Name = "更新系统设置")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Policy = ClaimNames.ApiPermission)]
    [UserLog(UserLogLevel.Classified)]
    public async Task<AjaxResp> Save([FromBody] T config)
    {
      await configManager.SaveConfigAsync(config);
      return new AjaxResp
      {
        Message = "操作完成",
        Data = null
      };
    }
  }
}
