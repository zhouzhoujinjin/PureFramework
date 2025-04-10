using Microsoft.AspNetCore.Mvc;
using CyberStone.Core.Utils;
using TinyPinyin;

namespace CyberStone.Core.Controllers
{
  public class ToolsController : ControllerBase
  {
    [HttpGet("/api/tools/pinyin/{chinese}", Name = "拼音转换")]
    [UserLog(UserLogLevel.Info)]
    public AjaxResp<string> Pinyin(string chinese)
    {
      var data = PinyinHelper.GetPinyin(chinese, "").ToLower();
      return new AjaxResp<string> { Data = data };
    }
  }
}