using Microsoft.Extensions.DependencyInjection;
using System;

namespace CyberStone.Core.Utils.Captcha
{
  public static class HeiCaptchaExtension
  {
    /// <summary>
    /// 启用HeiCaptcha
    /// </summary>
    /// <param name="services"></param>
    /// <returns></returns>
    public static IServiceCollection AddHeiCaptcha(this IServiceCollection services)
    {
      if (services == null)
      {
        throw new ArgumentNullException(nameof(services));
      }

      services.AddScoped<SecurityCodeHelper>();
      return services;
    }
  }


}
