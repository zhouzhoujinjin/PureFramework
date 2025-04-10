using Microsoft.Extensions.Options;
using CyberStone.Core.Managers;
using CyberStone.Core.Models;
using System.Threading.Tasks;

namespace CyberStone.Core.Managers
{
  public class ConfigManager<T> where T : GlobalSettings, new()
  {
    private readonly SettingManager _settingManager;
    private readonly CaptchaOptions _captchaOptions;

    public ConfigManager(SettingManager settingManager, IOptions< CaptchaOptions> captachOptionsAccessor)
    {
      _settingManager = settingManager;
      _captchaOptions = captachOptionsAccessor.Value;
    }

    /// <summary>
    /// 获得系统设置信息
    /// </summary>
    /// <returns></returns>
    public async Task<T> GetAllConfigAsync()
    {
      var settings = await _settingManager.GetGlobalSettings<T>();
      settings.EnableCaptcha = _captchaOptions.Enabled;
      return settings;
    }

    /// <summary>
    /// 修改系统设置信息
    /// </summary>
    /// <param name="configSetting">系统设置信息</param>
    /// <returns></returns>
    public async Task SaveConfigAsync(T? configSetting = default)
    {
      var config = await _settingManager.GetGlobalSettings<T>();
      if (configSetting != null)
      {
        config = configSetting;
      }
      await _settingManager.SaveGlobalSettingAsync(config);
    }
  }
}
