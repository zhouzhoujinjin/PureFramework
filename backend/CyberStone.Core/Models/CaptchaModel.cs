using System.Text.Json.Serialization;

namespace CyberStone.Core.Models;

public class CaptchaModel
{
  [JsonPropertyName("id")]
  public string Id { get; set; } = null!;
  [JsonPropertyName("content")]
  public string? Content { get; set; } = null!;
}