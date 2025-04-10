using System;
using System.Text;

namespace CyberStone.Core.Utils
{
  public static class StringExtension
  {
    public static string ToCamelCase(this string str)
    {
      if (!string.IsNullOrEmpty(str) && str.Length > 1)
      {
        return char.ToLower(str[0]) + str[1..];
      }
      return str;
    }

    public static string ToPascalCase(this string str)
    {
      if (!string.IsNullOrEmpty(str) && str.Length > 1)
      {
        return char.ToUpper(str[0]) + str[1..];
      }
      return str;
    }

    public static string ComputeMd5(this string data)
    {
      if (string.IsNullOrEmpty(data))
      {
        throw new ArgumentNullException(nameof(data));
      }

      using var md5 = System.Security.Cryptography.MD5.Create();
      var hsah = md5.ComputeHash(Encoding.UTF8.GetBytes(data));
      return BitConverter.ToString(hsah).Replace("-", "");
    }
  }
}