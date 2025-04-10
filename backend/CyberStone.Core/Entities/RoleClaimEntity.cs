using Microsoft.AspNetCore.Identity;

namespace CyberStone.Core.Entities
{
  public class RoleClaimEntity : IdentityRoleClaim<long>
  {
    public RoleEntity? Role { get; set; }
  }
}