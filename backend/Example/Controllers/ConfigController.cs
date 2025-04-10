using Backend.Managers;
using Backend.Models;
using CyberStone.Core;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers;

[ApiController]
[Route("/api/config", Name = "系统设置")]
public class ConfigController : ControllerBase
{
  private readonly ConfigManager configManager;

  public ConfigController(ConfigManager configManager)
  {
    this.configManager = configManager;
  }

  [HttpGet("/WW_verify_{code}.txt")]
  public string Index([FromRoute] string code)
  {
    return code;
  }

  [HttpGet(Name = "系统设置")]
  public async Task<AjaxResp<SystemConfigModel>> IndexAsync()
  {
    var config = await configManager.GetAllConfigAsync();
    return new AjaxResp<SystemConfigModel> { Data = config };
  }

  [HttpPost("/api/admin/config/update", Name = "更新系统设置")]
  [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme,Policy = ClaimNames.ApiPermission)]
  public async Task<AjaxResp> Save([FromBody] SystemConfigModel config)
  {
    await configManager.SaveConfigAsync(config);
    return new AjaxResp
    {
      Message = "操作完成",
      Data = null
    };
  }
}