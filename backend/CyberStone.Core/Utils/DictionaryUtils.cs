using System.Collections;
using System.Collections.Generic;

namespace CyberStone.Core.Utils
{
  public static class DictionaryUtils
  {
    public static Dictionary<string, string> Merge(params Dictionary<string, string>[] dicts)
    {
      var result = new Dictionary<string, string>();

      foreach (var dict in dicts)
      {
        foreach (var item in dict)
        {
          result[item.Key] = item.Value;
        }
      }
      return result;
    }

    public static Dictionary<string, string> Flattern(Dictionary<string, object> dict)
    {
      var result = new Dictionary<string, string>();

      foreach (var kv in dict)
      {
        Flatten(kv.Value, kv.Key, result);
      }

      return result;
    }

    private static void Flatten(object? v, string parentKey, Dictionary<string, string> result)
    {
      if (v == null)
      {
        return;
      }
      var type = v.GetType();

      if (v is IList && type.IsGenericType && type.IsAssignableFrom(typeof(List<>)))
      {
        var index = 0;

        if (v is not IEnumerable list)
        {
          return;
        }

        foreach (var item in list)
        {
          Flatten(item, $"{parentKey}.{index}", result);
          index++;
        }
      }
      else if (v is IDictionary && type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Dictionary<,>))
      {
        if (v is not IEnumerable<KeyValuePair<string, object>> dict)
        {
          return;
        }

        foreach (var kv in dict)
        {
          Flatten(kv.Value, $"{parentKey}.{kv.Key}", result);
        }
      }
      else
      {
        result[parentKey] = v.ToString()??"";
      }
    }

    public static Dictionary<string, object?> Nest(Dictionary<string, string> data)
    {
      var result = new Dictionary<string, object?>();
      foreach (var kv in data)
      {
        var parts = kv.Key.Split(".");
        object? parent = result;
        var nextType = true;
        for (var i = 0; i < parts.Length; i++)
        {
          var part = parts[i];
          var dict = parent as Dictionary<string, object?>;
          var list = parent as List<object>;
          if (i < parts.Length - 1)
          {
            var r = int.TryParse(parts[i + 1], out var index);
            nextType = !r; // 如果是整型，下面应该是列表了，而不是字典
            if (r)
            {
              if (dict != null)
              {
                var rr = dict.TryGetValue(part, out parent);
                if (!rr)
                {
                  parent = new List<object>();
                }
                dict[part] = parent;
              }
              else
              {
                while (list!.Count < index + 1)
                {
                  list.Add(new List<object>());
                }
                parent = list[index];
              }
            }
            else
            {
              if (dict != null)
              {
                var rr = dict.TryGetValue(part, out parent);
                if (!rr)
                {
                  parent = new Dictionary<string, object>();
                }
                dict[part] = parent;
              }
              else
              {
                while (list!.Count < index + 1)
                {
                  list.Add(new Dictionary<string, object>());
                }
                parent = list[index];
              }
            }
          }
          else
          {
            if (nextType)
            {
              dict![part] = kv.Value;
            }
            else
            {
              var r = int.TryParse(parts[i], out var index);
              if (r)
              {
                list![index] = kv.Value;
              }
            }
          }
        }
      }

      return result;
    }
  }
}