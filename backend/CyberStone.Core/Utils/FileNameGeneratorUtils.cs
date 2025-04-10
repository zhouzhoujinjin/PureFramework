using System;

namespace CyberStone.Core.Utils
{
  public static class FileNameGeneratorUtils
  {
    public const string Uuid = "uuid";
    public const string Md5 = "md5";
    public const string Guid = "guid";
    public const string Time = "time";

    public static string GenerateFileName(string type, string extension)
    {
      var baseName = type switch
      {
        Uuid => System.Guid.NewGuid().ToString("N").ToLower(),
        Guid => System.Guid.NewGuid().ToString(),
        Md5 => System.Guid.NewGuid().ToString().ComputeMd5(),
        Time => DateTime.Now.ToString("yyyyMMddhhmmssfff"),
        _ => throw new ArgumentOutOfRangeException($"{nameof(type)} is not supported, 'uuid', 'md5', 'guid', 'time' are supported"),
      };
      return baseName + "." + extension;
    }
  }
}