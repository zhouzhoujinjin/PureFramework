using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;

namespace CyberStone.Core.Entities
{
  public class UserEntity : IdentityUser<long>
  {
    public UserEntity()
    {
      UserRoles = new List<UserRoleEntity>();
      UserClaims = new List<UserClaimEntity>();
      UserProfiles = new List<UserProfileEntity>();
    }

    public bool IsDeleted { get; set; }
    public bool IsVisible { get; set; }

    public ICollection<UserRoleEntity> UserRoles { get; set; }
    public ICollection<UserClaimEntity> UserClaims { get; set; }
    public ICollection<UserProfileEntity> UserProfiles { get; set; }
    public DateTime CreatedTime { get; set; }
  }

  public class UserClaimEntity : IdentityUserClaim<long>
  {
  }
}