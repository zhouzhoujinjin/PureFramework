using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Options;
using CyberStone.Core.Models;
using CyberStone.Core.Utils;
using CyberStone.Core.Utils.Captcha;
using System;
using System.Threading.Tasks;

namespace CyberStone.Core.Managers;

public class CaptchaManager(
  SecurityCodeHelper securityCode,
  IDistributedCache cache,
  IOptions<CaptchaOptions> optionsAccessor)
{
  public bool Enabled => optionsAccessor.Value.Enabled;

  public async Task<CaptchaModel> CreateCodeAsync()
  {
    var options = optionsAccessor.Value;
    if (!options.Enabled)
      return new CaptchaModel();
    var id = Guid.NewGuid().ToString();
    var code = options.Type != "english"
      ? securityCode.GetRandomCnText(options.CodeLength)
      : securityCode.GetRandomEnDigitalText(options.CodeLength);
    var imageBytes = securityCode.GetBubbleCodeByte(code);
    await cache.SetAsync(GetCacheKey(id), code, new DistributedCacheEntryOptions()
    {
      AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(options.ExpiredSeconds)
    });

    return new CaptchaModel
    {
      Id = id,
      Content = ToPngBase64(imageBytes)
    };
  }

  private static string GetCacheKey(string id)
  {
    return string.Format(CacheKeys.CaptchaKey, id);
  }

  public async Task<CaptchaCheckResult> CheckCodeAsync(string? id, string? code)
  {
    if (!optionsAccessor.Value.Enabled) return CaptchaCheckResult.Success;

    if (id == null) return CaptchaCheckResult.NotFound;

    var storedCode = await cache.GetAsync<string?>(GetCacheKey(id));
    if (storedCode == null)
    {
      return CaptchaCheckResult.NotFound;
    }
    else if (storedCode != code)
    {
      return CaptchaCheckResult.Failure;
    }
    else
    {
      await cache.RemoveAsync(GetCacheKey(id));
      return CaptchaCheckResult.Success;
    }
  }

  private static string ToPngBase64(byte[] imageData) => $"data:image/png;base64,{Convert.ToBase64String(imageData)}";
}