using Microsoft.AspNetCore.Identity;
using System.Collections.Generic;

namespace CyberStone.Core.Entities
{
  public class RoleEntity : IdentityRole<long>
  {

    public string Title { get; set; } = null!;

    public List<UserRoleEntity> UserRoles { get; set; } = [];
    public List<RoleClaimEntity> RoleClaims { get; set; } = [];
  }

  public class UserRoleEntity : IdentityUserRole<long>
  {
    public UserEntity? User { get; set; }
    public RoleEntity? Role { get; set; }
  }
}