namespace CyberStone.Core;

public class CaptchaOptions
{
  public bool Enabled { get; set; }
  public int ExpiredSeconds { get; set; } = 300;
  public int CodeLength { get; set; } = 4;

  public string? Type { get; set; }
}