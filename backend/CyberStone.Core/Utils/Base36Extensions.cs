using System;
using System.Collections.Generic;
using System.Linq;

namespace CyberStone.Core.Utils
{
  public static class Base36Extensions
  {
    private static readonly string CharList = "0123456789abcdefghijklmnopqrstuvwxyz";

    public static string ToBase36(this Guid guid)
    {
      var hex = guid.ToString("N");
      return Convert(hex, 16, 36);
    }

    public static string ToBase62(this Guid guid)
    {
      var hex = guid.ToString("N");
      return Convert(hex, 16, 36);
    }

    public static string ToBase36(this long value)
    {
      return Convert(value.ToString(), 10, 36);
    }

    public static string ToBase36(this DateTimeOffset time)
    {
      var dec = time.ToUnixTimeMilliseconds();
      return Convert(dec.ToString(), 10, 36);
    }

    public static string ToBase36(this DateTime time)
    {
      return new DateTimeOffset(time).ToBase36();
    }

    public static string Convert(string number, int fromBase, int toBase)
    {
      int length = number.Length;
      string result = string.Empty;
      List<int> nibbles = number.Select(c => CharList.IndexOf(c)).ToList();
      int newlen;
      do
      {
        int value = 0;
        newlen = 0;
        for (var i = 0; i < length; ++i)
        {
          value = value * fromBase + nibbles[i];
          if (value >= toBase)
          {
            if (newlen == nibbles.Count)
            {
              nibbles.Add(0);
            }
            nibbles[newlen++] = value / toBase;
            value %= toBase;
          }
          else if (newlen > 0)
          {
            if (newlen == nibbles.Count)
            {
              nibbles.Add(0);
            }
            nibbles[newlen++] = 0;
          }
        }
        length = newlen;
        result = CharList[value] + result;
      }
      while (newlen != 0);
      return result;
    }
  }
}