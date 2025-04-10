using System.Text.Json.Serialization;

namespace CyberStone.Core.Models
{
  /// <summary>
  /// 权限点
  /// </summary>
  public class PermissionModel
  {
    /// <summary>
    /// 名称
    /// </summary>
    [JsonPropertyName("name")]
    public string Name { get; set; } = null!;

    /// <summary>
    /// 值
    /// </summary>
    [JsonPropertyName("value")]
    public string Value { get; set; } = null!;

    /// <summary>
    /// 组
    /// </summary>
    [JsonPropertyName("group")]
    public string Group { get; set; } = "";
  }
}