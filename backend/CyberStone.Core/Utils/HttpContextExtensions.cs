using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;

namespace CyberStone.Core.Utils
{
  public static class HttpContextExtensions
  {
    public static string? GetUserName(this HttpContext ctx)
    {
      return ctx.User?.Claims.FirstOrDefault(c => c.Type == "userName")?.Value;
    }

    public static long GetUserId(this HttpContext ctx)
    {
      var id = ctx.User?.Claims.FirstOrDefault(c => c.Type == "userId")?.Value;

      var result = long.TryParse(id, out var value);

      if (value == 0)
      {
        var srv = ctx.RequestServices.GetRequiredService(typeof(ILogger<HttpContext>)) as ILogger<HttpContext>;
        srv!.LogWarning($"[{nameof(GetUserId)}] 不存在登录用户，请检查是否发送登录信息");
      }
      return result ? value : default;
    }
  }
}