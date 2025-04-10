using CyberStone.Core.Managers;
using CyberStone.Core.Models;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace CyberStone.Core.Utils
{
  public class UserLogAttribute : ActionFilterAttribute
  {
    protected DateTime startTime;

    private bool needLog;
    private UserLogLevel logLevel;

    public UserLogAttribute(UserLogLevel logLevel = UserLogLevel.Classified)
    {
      this.logLevel = logLevel;
    }

    public override void OnActionExecuting(ActionExecutingContext filterContext)
    {
      if (filterContext.ActionDescriptor is Microsoft.AspNetCore.Mvc.Controllers.ControllerActionDescriptor)
      {
        var descriptor = (Microsoft.AspNetCore.Mvc.Controllers.ControllerActionDescriptor)filterContext.ActionDescriptor;
        var attr = descriptor.MethodInfo.CustomAttributes.FirstOrDefault(x => x.AttributeType == typeof(UserLogAttribute));
        needLog = attr != null;
      };
      startTime = DateTime.Now;
    }

    public override async Task OnResultExecutionAsync(ResultExecutingContext filterContext, ResultExecutionDelegate next)
    {
      var httpContext = filterContext.HttpContext;

      if (!needLog)
      {
        await next();
        return;
      }

      var options = filterContext.HttpContext.RequestServices.GetService<IOptions<UserLogOptions>>();
      if(options == null || options.Value.LogLevel== UserLogLevel.None || options.Value.LogLevel > this.logLevel) { 
        await next();
        return;
      }

      // 防止用户反复刷新页面，导致存入数据库的日志过多
      var exists = httpContext.Request.Headers.TryGetValue("UrlReferrer", out var referrer);
      if (exists && httpContext.Request.GetEncodedUrl().Equals(referrer))
      {
        await next();
        return;
      }



      await next();

      var svc = filterContext.HttpContext.RequestServices;
      var manager = (svc.GetService(typeof(UserLogManager)) as UserLogManager)!;
      var duration = (DateTime.Now - startTime);
      var createdAt = DateTime.Now;

      var userAgent = httpContext.Request.Headers["User-Agent"].ToString();
      var device = userAgent.Contains("micromessenger") ? "wechat" : "browser";
      device += userAgent.Contains("iphone") ? "/ios" : userAgent.Contains("android") ? "/android" : "/pc";
      var userId = httpContext.GetUserId();

      var userLog = new UserLog()
      {
        Url = httpContext.Request.GetDisplayUrl(),
        Method = httpContext.Request.Method,
        Device = device,
        Data = httpContext.Request.QueryString.ToString(),
        Ip = httpContext.Connection.RemoteIpAddress?.ToString(),
        UserAgent = userAgent,
        Duration = (int)duration.TotalMilliseconds,
        UserId = userId,
        CreatedTime = createdAt
      };
      await manager.AddLog(userLog);
    }
  }
}