using CyberStone.Core.TreeFeature;
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace CyberStone.Core.Models
{
  public class Department: ITreeNode<Department>
  {
    [JsonPropertyName("id")]
    public long Id { get; set; }

    [JsonPropertyName("title")]
    public string Title { get; set; } = "";

    [JsonPropertyName("creator")]
    public User? Creator { get; set; }

    [JsonPropertyName("createdTime")]
    public DateTime? CreatedTime { get; set; }

    [JsonPropertyName("users")]
    public IEnumerable<DepartmentUser>? Users { get; set; }

    [JsonPropertyName("children")]
    public List<Department>? Children { get; set; }
  }

  public class DepartmentUser : User
  {
    [JsonPropertyName("userId")]
    public long UserId { get; set; }

    [JsonPropertyName("position")]
    public string? Position { get; set; }

    [JsonPropertyName("level")]
    public short Level { get; set; }

    [JsonPropertyName("isUserMajorDepartment")]
    public bool IsUserMajorDepartment { get; set; }

    [JsonPropertyName("departments")]
    public IEnumerable<UserDepartment>? Departments { get; set; }

    public static DepartmentUser FromUser(User user)
    {
      var du = new DepartmentUser
      {
        Id = user.Id,
        Profiles = user.Profiles,
        Roles = user.Roles,
        UserName = user.UserName
      };
      return du;
    }
  }

  public class UserDepartment
  {
    [JsonPropertyName("departmentId")]
    public long DepartmentId { get; set; }

    [JsonPropertyName("title")]
    public string? Title { get; set; }

    [JsonPropertyName("position")]
    public string? Position { get; set; }

    [JsonPropertyName("level")]
    public short? Level { get; set; }

    [JsonPropertyName("isUserMajorDepartment")]
    public bool? IsUserMajorDepartment { get; set; }
  }
}