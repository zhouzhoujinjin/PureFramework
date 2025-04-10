using System.Text.Json.Serialization;

namespace CyberStone.Core.Models
{
  /// <summary>
  /// 权限点
  /// </summary>
  public class Permission
  {
    /// <summary>
    /// 名称
    /// </summary>
    [JsonPropertyName("name")]
    public string Name { get; set; }

    /// <summary>
    /// 值
    /// </summary>
    [JsonPropertyName("value")]
    public string Value { get; set; }

    /// <summary>
    /// 组
    /// </summary>
    [JsonPropertyName("group")]
    public string Group { get; set; }
  }
}