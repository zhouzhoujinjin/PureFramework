using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace CyberStone.Core.Models
{
  public class CustomPermission
  {
    [JsonPropertyName("name")]
    public string Name { get; set; } = null!;

    [JsonPropertyName("title")]
    public string? Title { get; set; }
  }

  public class CustomPermissionSettings
  {
    [JsonPropertyName("values")]
    public List<CustomPermission> Values { get; set; } = [];
  }
}