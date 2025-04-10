using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CyberStone.Core.Managers;
using CyberStone.Core.Models;
using CyberStone.Core.Utils;
using System.Collections.Generic;

namespace CyberStone.Core.Controllers
{
  [Route("api/valueSpaces", Name = "值空间")]
  [ApiController]
  [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
  public partial class ValueSpaceController : ControllerBase
  {
    private readonly ValueSpaceManager valueSpaceManager;

    public ValueSpaceController(ValueSpaceManager valueSpaceManager)
    {
      this.valueSpaceManager = valueSpaceManager;
    }

    /// <summary>
    /// 获取所有的值空间列表
    /// </summary>
    /// <returns></returns>
    [HttpGet(Name = "值空间字典")]
    [Authorize(Policy = ClaimNames.ApiPermission)]
    [UserLog(UserLogLevel.Classified)]
    public AjaxResp<SortedDictionary<string, ValueSpace>> Get()
    {
      var rsp = valueSpaceManager.GetVsMap();
      return new AjaxResp<SortedDictionary<string, ValueSpace>>
      {
        Data = rsp
      };
    }

    /// <summary>
    /// 根据名称获得指定的值空间
    /// </summary>
    /// <param name="name">值空间名称</param>
    /// <returns></returns>
    [HttpGet("{name}", Name = "值空间详情")]
    [UserLog(UserLogLevel.Classified)]
    public AjaxResp<ValueSpace> Get(string name)
    {
      var result = valueSpaceManager.GetVsMap().TryGetValue(name, out var valueSpace);
      if (result)
      {
        return new AjaxResp<ValueSpace>
        {
          Data = valueSpace
        };
      }
      else
      {
        return new AjaxResp<ValueSpace>
        {
          Code = 404,
          Message = "未找到",
        };
      }
    }
  }
}