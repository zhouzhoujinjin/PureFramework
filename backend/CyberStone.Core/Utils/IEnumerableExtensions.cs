using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CyberStone.Core.Utils
{
  public static class IEnumerableExtensions
  {
    public static void ForEach<T>(this IEnumerable<T> array, Action<T> act)
    {
      if (array == null)
      {
        return;
      }

      foreach (var i in array)
      {
        act(i);
      }
    }

    public static void ForEach<T>(this IEnumerable<T> array, Action<T, int> act)
    {
      if (array == null)
      {
        return;
      }

      var i = 0;
      foreach (var item in array)
      {
        act(item, i);
        i++;
      }
    }

    public static async Task ForEachAsync<T>(this IEnumerable<T> list, Func<T, Task> func)
    {
      if (list == null || !list.Any())
      {
        return;
      }
      foreach (var value in list)
      {
        await func(value);
      }
    }

    public static async Task ForEachAsync<T>(this IEnumerable<T> list, Func<T, int, Task> func)
    {
      if (list == null || !list.Any())
      {
        return;
      }
      var index = 0;
      foreach (var value in list)
      {
        await func(value, index);
        index++;
      }
    }
  }
}