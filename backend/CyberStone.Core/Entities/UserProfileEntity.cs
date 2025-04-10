namespace CyberStone.Core.Entities
{
  public class UserProfileEntity
  {
    public long Id { get; set; }
    public string? FullName { get; set; }
    public long ProfileKeyId { get; set; }
    public ProfileKeyEntity? ProfileKey { get; set; }
    public long UserId { get; set; }
    public string? Value { get; set; }
    public UserEntity? User { get; set; }
  }
}