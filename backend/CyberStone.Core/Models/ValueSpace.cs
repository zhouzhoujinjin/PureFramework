using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;

namespace CyberStone.Core.Models
{
  public abstract class ValueSpace
  {
    [JsonPropertyName("name")] public string Name { get; set; } = "";

    [JsonPropertyName("title")] public string? Title { get; set; }

    [JsonConverter(typeof(JsonStringEnumConverter))]
    [JsonPropertyName("valueSpaceType")]
    public ValueSpaceType ValueSpaceType { get; internal init; } = ValueSpaceType.Code;

    [JsonConverter(typeof(JsonStringEnumConverter))]
    [JsonPropertyName("configureLevel")]
    public ConfigureLevel ConfigureLevel { get; internal init; } = ConfigureLevel.Configurable;

    public virtual object? Conditions { get; set; }

    public virtual List<string> GetItemNames() => throw new NotImplementedException();

    public virtual bool IsValid(object? test) => throw new NotImplementedException();
  }

  public class CodeValueSpace : ValueSpace
  {
    private Dictionary<string, string> map;

    public override object? Conditions
    {
      get => map;
      set => map = (value as Dictionary<string, string>)!;
    }

    public CodeValueSpace()
    {
      map = new Dictionary<string, string>();
      ValueSpaceType = ValueSpaceType.Code;
    }

    public CodeValueSpace(string name)
    {
      Name = name;
      map = new Dictionary<string, string>();
    }

    public CodeValueSpace(string name, string? title, ConfigureLevel configureLevel, IEnumerable<KeyValuePair<string, string>> items)
    {
      Name = name;
      Title = title;
      ValueSpaceType = ValueSpaceType.Code;
      ConfigureLevel = configureLevel;
      map = new Dictionary<string, string>();
      foreach (var (key, value) in items)
      {
        map.Add(key, value);
      }
    }

    public void SetValue(string code, string? value = null)
    {
      map[code] = value ?? code;
    }

    public bool TryGetCodeByValue(string? value, out string? result)
    {
      foreach (var (key, s) in map)
      {
        if (s != value) continue;
        result = key;
        return true;
      }
      result = null;
      return false;
    }

    public string? GetValue(string code, out bool result)
    {
      result = map.TryGetValue(code, out string? v);
      return v;
    }

    public override bool IsValid(object? test)
    {
      return test != null && map.ContainsKey(test.ToString()!);
    }

    //william 修改code可能会null的情况
    public string GetCodeFullName(object? code)
    {
      return null != code ? $"{Name}.{code}" : "";
    }

    public override List<string> GetItemNames()
    {
      return map.Select(i => i.Key).ToList();
    }
  }

  public class RegexValueSpace : ValueSpace
  {
    public override object Conditions => Regexes.Select(x => x.ToString());

    [JsonIgnore]
    public List<Regex> Regexes { get; set; }

    public override bool IsValid(object? test)
    {
      return test != null && Regexes.Any(x => x.IsMatch(test.ToString()!));
    }

    public RegexValueSpace(string name, string? title, ConfigureLevel configureLevel, List<string> patterns)
    {
      Name = name;
      Title = title;
      ValueSpaceType = ValueSpaceType.Regex;
      ConfigureLevel = configureLevel;
      Regexes = patterns.Select(x => new Regex(x)).ToList();
    }

    public override List<string> GetItemNames()
    {
      return new() { Name };
    }
  }

  public class RangeValueSpace : ValueSpace
  {
    public override object? Conditions
    {
      get => Ranges;
      set => Ranges = (value as Dictionary<string, float>)!;
    }

    [JsonIgnore]
    public Dictionary<string, float> Ranges { get; set; }

    public override bool IsValid(object? test)
    {
      return test != null && float.TryParse(test.ToString(), out _);
    }

    public string TitleOf(object value)
    {
      if (!IsValid(value)) return "";

      var v = (float)value;
      var kvs = Ranges.OrderBy(kv => kv.Value).ToArray();
      for (var i = 0; i < kvs.Length; i++)
      {
        if (i == 0 && v < kvs[i].Value) return kvs[i].Key;
        if (kvs[i].Value <= v && v < kvs[i + 1].Value)
        {
          return kvs[i].Key;
        }
      }
      return "";
    }

    public RangeValueSpace(string name, string? title, ConfigureLevel configureLevel, Dictionary<string, float> ranges)
    {
      Name = name;
      Title = title;
      ValueSpaceType = ValueSpaceType.Range;
      ConfigureLevel = configureLevel;
      Ranges = ranges;
    }

    public override List<string> GetItemNames()
    {
      return Ranges.Select(i => i.ToString()).ToList();
    }
  }
}