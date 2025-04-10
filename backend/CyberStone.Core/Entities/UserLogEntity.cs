using System;

namespace CyberStone.Core.Entities
{
  public class UserLogEntity
  {
    public long Id { get; set; }
    public long UserId { get; set; }

    public string Url { get; set; } = "";

    public string Ip { get; set; } = "";

    public string Method { get; set; } = "";

    public string Device { get; set; } = "";

    public string UserAgent { get; set; } = "";

    public string Data { get; set; } = "";

    public DateTime CreatedTime { get; set; }

    public int Duration { get; set; }
  }
}