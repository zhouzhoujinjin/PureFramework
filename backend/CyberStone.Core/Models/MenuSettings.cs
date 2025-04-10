using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace CyberStone.Core.Models
{
  public class MenuItem
  {
    /// <summary>
    /// 菜单项类型
    /// </summary>
    [JsonPropertyName("type")]
    public MenuItemType Type { get; set; } = MenuItemType.Route;

    /// <summary>
    /// 路径，可以为 "/" 或 ":" 分割，前者为路由项，后者为权限点
    /// </summary>
    [JsonPropertyName("path")]
    public string Path { get; set; } = "";

    [JsonPropertyName("title")]
    public string Title { get; set; } = "";

    [JsonPropertyName("iconName")]
    public string? IconName { get; set; }

    [JsonPropertyName("hideChildren")]
    public bool HideChildren { get; set; }

    [JsonPropertyName("hidden")]
    public bool Hidden { get; set; }

    [JsonPropertyName("children")]
    public ICollection<MenuItem>? Children { get; set; }
  }

  public class MenuSettings
  {
    [JsonPropertyName("value")]
    public ICollection<MenuItem>? Value { get; set; }
  }
}