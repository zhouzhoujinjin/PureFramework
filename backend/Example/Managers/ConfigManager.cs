using Backend.Models;
using CyberStone.Core.Managers;

namespace Backend.Managers
{
  public class ConfigManager
  {
    private readonly SettingManager _settingManager;

    public ConfigManager(SettingManager settingManager)
    {
      _settingManager = settingManager;
    }

    /// <summary>
    /// 获得系统设置信息
    /// </summary>
    /// <returns></returns>
    public async Task<SystemConfigModel> GetAllConfigAsync()
    {
      return await _settingManager.GetGlobalSettings<SystemConfigModel>();
    }

    /// <summary>
    /// 修改系统设置信息
    /// </summary>
    /// <param name="configSetting">系统设置信息</param>
    /// <returns></returns>
    public async Task SaveConfigAsync(SystemConfigModel? configSetting = null)
    {
      var config = await _settingManager.GetGlobalSettings<SystemConfigModel>();
      if (configSetting != null)
      {
        config = configSetting;
      }
      await _settingManager.SaveGlobalSettingAsync(config);
    }
  }
}