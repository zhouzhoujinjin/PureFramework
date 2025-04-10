using System.Text.Json.Serialization;

namespace CyberStone.Core.Models
{
  public class ChangePasswords
  {
    [JsonPropertyName("oldPassword")]
    public string OldPassword { get; set; } = null!;

    [JsonPropertyName("newPassword")]
    public string NewPassword { get; set; } = null!;
  }
}