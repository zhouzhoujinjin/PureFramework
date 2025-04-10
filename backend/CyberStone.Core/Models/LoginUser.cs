using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace CyberStone.Core.Models
{
  public class LoginUser
  {
    /// <summary>
    /// 用户名
    /// </summary>
    [Required]
    [JsonPropertyName("userName")]
    public string UserName { get; set; } = null!;

    /// <summary>
    /// 密码
    /// </summary>
    [Required]
    [JsonPropertyName("password")]
    public string Password { get; set; } = null!;

    /// <summary>
    /// 验证码
    /// </summary>
    [JsonPropertyName("captchaId")]
    public string? CaptchaId { get; set; }

    /// <summary>
    /// 验证码
    /// </summary>
    [JsonPropertyName("captchaCode")]
    public string? CaptchaCode { get; set; }
  }
}