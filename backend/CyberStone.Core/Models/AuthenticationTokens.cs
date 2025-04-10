using System.Text.Json.Serialization;

namespace CyberStone.Core.Models
{
  public class AuthenticationTokens
  {
    /// <summary>
    /// 访问票据
    /// </summary>
    [JsonPropertyName("accessToken")]
    public string AccessToken { get; set; } = null!;

    /// <summary>
    /// 刷新票据
    /// </summary>
    [JsonPropertyName("refreshToken")]
    public string? RefreshToken { get; set; }
  }
}