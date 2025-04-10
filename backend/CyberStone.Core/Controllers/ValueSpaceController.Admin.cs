using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CyberStone.Core.Models;
using CyberStone.Core.Utils;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CyberStone.Core.Controllers
{
  public partial class ValueSpaceController
  {
    [HttpPut("/api/admin/valueSpaces/{name}", Name = "更新值空间")]
    [Authorize(Policy = ClaimNames.ApiPermission)]
    [UserLog(UserLogLevel.Classified)]
    public AjaxResp<ValueSpace> Put(string name, [FromBody] ValueSpace valueSpace)
    {
      var result = valueSpaceManager.GetVsMap().TryGetValue(name, out var vs);
      if (!result)
      {
        return new AjaxResp<ValueSpace>
        {
          Code = 404,
          Message = $"找不到 {name} 值空间"
        };
      }

      if (vs != null)
      {
        vs.Name = valueSpace.Name;
        vs.Title = valueSpace.Title;
        vs.Conditions = valueSpace.Conditions;

        valueSpaceManager.Save(vs);
      }

      return new AjaxResp<ValueSpace>
      {
        Code = 0,
        Data = vs
      };
    }

    [HttpGet("/api/admin/valueSpaces", Name = "值空间摘要列表")]
    [Authorize(Policy = ClaimNames.ApiPermission)]
    [UserLog(UserLogLevel.Classified)]
    public PagedAjaxResp<ValueSpace> Index(int page = 1, int size = 10)
    {
      var valueSpaces = valueSpaceManager.GetVsMap()
        .Select(v => v.Value)
        .OrderBy(v => v.Name)
        .Skip(Math.Max(page - 1, 0) * size)
        .Take(size).Select(v =>
        {
          if (v.ValueSpaceType == ValueSpaceType.Code)
          {
            return new CodeValueSpace(
              v.Name, v.Title, v.ConfigureLevel,
              ((Dictionary<string, string>)v.Conditions!).Take(6));
          }
          else
          {
            return v;
          }
        });
      return new PagedAjaxResp<ValueSpace>
      {
        Data = valueSpaces,
        Total = valueSpaceManager.GetVsMap().Count,
        Page = page
      };
    }
  }
}