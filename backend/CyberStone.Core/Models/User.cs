using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace CyberStone.Core.Models
{
  public class User
  {
    public User()
    {
      Profiles = new Dictionary<string, object?>();
    }

    public User(long id, string userName)
    {
      Id = id;
      UserName = userName;
      Profiles = new Dictionary<string, object?>();
    }

    [JsonPropertyName("id")]
    public long Id { get; set; }

    [JsonPropertyName("userName")]
    public string UserName { get; set; } = null!;

    [JsonPropertyName("profiles")]
    public Dictionary<string, object?> Profiles { get; set; }

    [JsonPropertyName("roles")]
    public Dictionary<string, string>? Roles { get; set; }
  }

  public class AdminUser : User
  {
    public bool IsDeleted { get; set; }

    public bool IsVisible { get; set; }

    public long CreatorId { get; set; }
    public string? CreatorName { get; set; }
    public string? FullName { get; set; }

    public static AdminUser FromUser(User user) =>
      new AdminUser
      {
        Id = user.Id,
        UserName = user.UserName,
        Profiles = user.Profiles
      };
  }
}