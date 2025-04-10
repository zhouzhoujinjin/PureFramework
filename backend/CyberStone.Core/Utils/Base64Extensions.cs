using System;
using System.IO;

namespace CyberStone.Core.Utils
{
  public static class Base64Extensions
  {
    public static string GetBase64String(this Stream stream)
    {
      byte[] arr = new byte[stream.Length];
      stream.Position = 0;
      stream.Read(arr, 0, (int)stream.Length);
      return Convert.ToBase64String(arr);
    }
  }
}