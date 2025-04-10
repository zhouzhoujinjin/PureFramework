using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CyberStone.Core.Utils
{
  public static class Base62Extensions
  {
    public const string chars = "0123456789abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ";

    /// <summary>
    /// 对 short 类型进行 Base62 编码
    /// </summary>
    /// <param name="original">数值</param>
    /// <returns>Base62 字符串</returns>
    public static string ToBase62(this short original)
    {
      var array = BitConverter.GetBytes(original);

      if (BitConverter.IsLittleEndian)
      {
        Array.Reverse(array);
      }

      return array.ToBase62();
    }

    /// <summary>
    /// 对 int 类型进行 Base62 编码
    /// </summary>
    /// <param name="original">数值</param>
    /// <returns>Base62 字符串</returns>
    public static string ToBase62(this int original)
    {
      var array = BitConverter.GetBytes(original);

      if (BitConverter.IsLittleEndian)
      {
        Array.Reverse(array);
      }

      return array.ToBase62();
    }

    /// <summary>
    /// 对 long 类型进行 Base62 编码
    /// </summary>
    /// <param name="original">数值</param>
    /// <returns>Base62 字符串</returns>
    public static string ToBase62(this long original)
    {
      var array = BitConverter.GetBytes(original);

      if (BitConverter.IsLittleEndian)
      {
        Array.Reverse(array);
      }

      return array.ToBase62();
    }

    /// <summary>
    /// 对 byte 数组 类型进行 Base62 编码
    /// </summary>
    /// <param name="original">字节数组</param>
    /// <returns>Base62 字符串</returns>
    public static string ToBase62(this byte[] original)
    {
      var arr = Array.ConvertAll(original, t => (int)t);

      var converted = BaseConvert(arr, 256, 62);
      var builder = new StringBuilder();
      foreach (var t in converted)
      {
        builder.Append(chars[t]);
      }
      return builder.ToString();
    }

    /// <summary>
    /// 对 字符串 类型进行 Base62 编码
    /// </summary>
    /// <param name="original">字符串</param>
    /// <returns>Base62 字符串</returns>
    public static string ToBase62(this string original) => Encoding.UTF8.GetBytes(original).ToBase62();

    /// <summary>
    /// 对 字符串 类型进行 Base62 编码
    /// </summary>
    /// <param name="original">字符串</param>
    /// <param name="charset">字符串编码</param>
    /// <returns>Base62 字符串</returns>
    public static string ToBase62(this string original, string charset) => Encoding.GetEncoding(charset).GetBytes(original).ToBase62();

    private static int[] BaseConvert(int[] source, int sourceBase, int targetBase)
    {
      var result = new List<int>();
      var leadingZeroCount = source.TakeWhile(x => x == 0).Count();
      int count;
      while ((count = source.Length) > 0)
      {
        var quotient = new List<int>();
        var remainder = 0;
        for (var i = 0; i != count; i++)
        {
          var accumulator = source[i] + remainder * sourceBase;
          var digit = accumulator / targetBase;
          remainder = accumulator % targetBase;
          if (quotient.Count > 0 || digit > 0)
          {
            quotient.Add(digit);
          }
        }

        result.Insert(0, remainder);
        source = quotient.ToArray();
      }
      result.InsertRange(0, Enumerable.Repeat(0, leadingZeroCount));
      return result.ToArray();
    }
  }
}