using Microsoft.Extensions.Caching.Distributed;
using CyberStone.Core.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CyberStone.Core.Utils;

namespace CyberStone.Core.Managers
{
  /// <summary>
  /// 菜单信息
  /// Menu 信息统一存在系统的 MenuSettings 中，
  /// MenuSettings 中应保存全部的信息，系统将通过权限过滤掉某用户没有权限访问的菜单项。
  /// 系统默认包含主菜单（MAIN）和顶菜单（TOP），您也可以继续添加其他名称的菜单。
  /// </summary>
  public class MenuManager
  {
    private readonly SettingManager settingManager;
    private readonly IDistributedCache cache;

    public MenuManager(SettingManager settingManager, IDistributedCache cache)
    {
      this.settingManager = settingManager;
      this.cache = cache;
    }

    public async Task<MenuSettings?> GetMenuAsync()
    {
      return await cache.GetAsync(CacheKeys.Menu, async () => await settingManager.GetGlobalSettings<MenuSettings>(), new DistributedCacheEntryOptions
      {
        AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(86400)
      });
    }

    public async Task UpdateMenusAsync(ICollection<MenuItem>? menu = null)
    {
      var menuSetting = await settingManager.GetGlobalSettings<MenuSettings>();
      menuSetting.Value = menu;
      await settingManager.SaveGlobalSettingAsync(menuSetting);
      await cache.RemoveAsync(CacheKeys.Menu);
    }
  }
}