using System;
using System.Text.Json.Serialization;

namespace CyberStone.Core.Models
{
  /// <summary>
  /// 资料点
  /// </summary>
  public class ProfileKey
  {
    [JsonIgnore]
    public long Id { get; set; }

    /// <summary>
    /// 名称
    /// </summary>
    [JsonPropertyName("name")]
    public string Name { get; set; } = null!;

    [JsonPropertyName("isPublic")]
    public bool IsPublic { get; set; }

    [JsonPropertyName("isBrief")]
    public bool IsBrief { get; set; }

    [JsonPropertyName("seachable")]
    public bool Seachable { get; set; }

    [JsonPropertyName("valueSpaceName")]
    public string? ValueSpaceName { get; set; }

    [JsonPropertyName("profileTypeName")]
    public string? ProfileTypeName { get; set; }

    [JsonIgnore]
    public Type? ProfileType { get; set; }

    /// <summary>
    /// 分类代码
    /// </summary>
    [JsonPropertyName("categoryCode")]
    public string CategoryCode { get; set; } = "";

    /// <summary>
    /// 值空间名称
    /// </summary>
    [JsonPropertyName("valueSpace")]
    public ValueSpace? ValueSpace { get; set; }
  }

  /// <summary>
  /// 资料点
  /// </summary>
  public class Profile
  {
    public long Id { get; set; }

    /// <summary>
    /// 名称
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// 分类代码
    /// </summary>
    public string CategoryCode { get; set; }

    /// <summary>
    /// 值空间名称
    /// </summary>
    public string ValueSpaceName { get; set; }

    public string ProfileKeyName { get; set; }

    /// <summary>
    /// 值
    /// </summary>
    public object? Value { get; set; }
  }
}