using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace CyberStone.Core.Models
{
  public class Role
  {
    [JsonPropertyName("name")]
    public string Name { get; set; } = null!;

    [JsonPropertyName("title")]
    public string? Title { get; set; }

    [JsonPropertyName("users")]
    public List<User> Users { get; set; } = [];

    [JsonPropertyName("claims")]
    public List<string> Claims { get; set; } = [];
  }
}