using Microsoft.EntityFrameworkCore;
using CyberStone.Core.Entities;
using CyberStone.Core.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CyberStone.Core.Utils;

namespace CyberStone.Core.Managers
{
  public class ValueSpaceMap
  {
    public SortedDictionary<string, ValueSpace> Value { get; set; } = [];

    /// <summary>
    /// 代码类型的值空间获得标题对应的代码
    /// </summary>
    /// <param name="name"></param>
    /// <param name="title"></param>
    /// <returns></returns>
    public string GetCodeByNameTitle(string name, string title)
    {
      Value.TryGetValue(name, out var vs);
      if (vs == null || vs.ValueSpaceType != ValueSpaceType.Code) return "";
      var kv = ((Dictionary<string, string>)vs.Conditions!).FirstOrDefault(kv => kv.Value == title);
      return kv.Key;
    }

    /// <summary>
    /// 代码类型的值空间获得代码对应的标题
    /// </summary>
    /// <param name="name"></param>
    /// <param name="key"></param>
    /// <returns></returns>
    public string? GetTitleByNameKey(string name, object key)
    {
      var title = string.Empty;
      name = name.ToCamelCase();
      Value.TryGetValue(name, out var vs);
      if (vs is { ValueSpaceType: ValueSpaceType.Code })
      {
        ((Dictionary<string, string>)vs.Conditions!)?.TryGetValue(key.ToString()!, out title);
      }

      return title;
    }

    /// <summary>
    /// 范围类型的值空间获得值对应的标题
    /// </summary>
    /// <param name="name"></param>
    /// <param name="value"></param>
    /// <returns></returns>
    public string? GetTitleByNameValue(string name, float value)
    {
      string title = string.Empty;
      Value.TryGetValue(name, out var vs);
      if (vs == null || vs.ValueSpaceType != ValueSpaceType.Range) return title;
      var range = vs as RangeValueSpace;
      return range?.TitleOf(value);
    }

    public ValueSpace Get(string name)
    {
      return Value[name];
    }

    public T Get<T>(string name) where T : ValueSpace
    {
      return (T)Value[name];
    }
  }

  public class ValueSpaceManager
  {
    public ValueSpaceManager(CyberStoneDbContext context)
    {
      this.context = context;
      valueSpaceSet = context.ValueSpaces;
    }

    public void Initialize()
    {
      var values = valueSpaceSet.AsNoTracking().ToList();
      var map = new SortedDictionary<string, ValueSpace>();
      foreach (var val in values)
      {
        switch (val.ValueSpaceType)
        {
          case ValueSpaceType.Code:
            map[val.Name] = new CodeValueSpace(val.Name, val.Title, val.ConfigureLevel, ParseCodes(val));
            break;

          case ValueSpaceType.Range:
            map[val.Name] = new RangeValueSpace(val.Name, val.Title, val.ConfigureLevel, ParseRanges(val));
            break;

          case ValueSpaceType.Regex:
            map[val.Name] = new RegexValueSpace(val.Name, val.Title, val.ConfigureLevel, ParseRegexPatterns(val));
            break;
        }
      }

      ValueSpaceMap.Value = map;
    }

    internal static ValueSpaceMap ValueSpaceMap { get; private set; } = new ValueSpaceMap();
    private readonly CyberStoneDbContext context;
    private readonly DbSet<ValueSpaceEntity> valueSpaceSet;

#pragma warning disable CA1822 // 将成员标记为 static

    public SortedDictionary<string, ValueSpace> GetVsMap() => ValueSpaceMap.Value;

#pragma warning restore CA1822 // 将成员标记为 static

    #region CRUD

    public ValueSpaceEntity? GetByName(string name)
    {
      return valueSpaceSet.AsNoTracking().FirstOrDefault(r => r.Name == name);
    }

    public ValueSpaceEntity? GetById(long id)
    {
      return valueSpaceSet.AsNoTracking().FirstOrDefault(x => x.Id == id);
    }

    public void Save(ValueSpace dto)
    {
      ValueSpaceEntity? vs = Serialize(dto);
      if (vs != null)
      {
        valueSpaceSet.Update(vs);
        context.SaveChanges();
        Initialize();
      }
    }

    public void Add(ValueSpace dto)
    {
      var vs = Serialize(dto);
      if (vs != null)
      {
        valueSpaceSet.Add(vs);
        context.SaveChanges();
        Initialize();
      }
    }

    public void Delete(ValueSpaceEntity vs)
    {
      valueSpaceSet.Remove(vs);
      context.SaveChanges();
      Initialize();
    }

    public async Task SaveAsync(ValueSpace dto)
    {
      var vs = Serialize(dto);
      if (vs != null)
      {
        valueSpaceSet.Update(vs);
        await context.SaveChangesAsync();
        Initialize();
      }
    }

    public async Task AddAsync(ValueSpace dto)
    {
      var vs = Serialize(dto);
      if (vs != null)
      {
        valueSpaceSet.Add(vs);
        await context.SaveChangesAsync();
        Initialize();
      }
    }

    public async Task DeleteAsync(ValueSpaceEntity vs)
    {
      valueSpaceSet.Remove(vs);
      await context.SaveChangesAsync();
      Initialize();
    }

    #endregion CRUD

    private static Dictionary<string, string> ParseCodes(ValueSpaceEntity vs)
    {
      var items = vs.Items.Split('\n');
      var cvs = new Dictionary<string, string>();
      foreach (var item in items)
      {
        if (string.IsNullOrEmpty(item))
        {
          continue;
        }

        var cv = item.Trim().Split(':');
        cvs.Add(cv[0], cv.Length == 1 ? cv[0] : cv[1]);
      }

      return cvs;
    }

    private static Dictionary<string, float> ParseRanges(ValueSpaceEntity vs)
    {
      var items = vs.Items.Split('\n');
      var ranges = new Dictionary<string, float>();
      foreach (var item in items)
      {
        if (string.IsNullOrEmpty(item))
        {
          continue;
        }

        var parts = item.Trim().Split(":");
        string range, title;
        if (parts.Length == 1)
        {
          if (string.IsNullOrEmpty(parts[0])) continue;
          range = title = parts[0];
        }
        else
        {
          if (string.IsNullOrEmpty(parts[1])) continue;
          title = parts[0];
          range = parts[1];
        }

        if (float.TryParse(range, out var v))
        {
          ranges.Add(title, v);
        }
      }

      return ranges;
    }

    private static List<string> ParseRegexPatterns(ValueSpaceEntity vs)
    {
      var items = vs.Items.Split('\n').ToList();
      return items;
    }

    private ValueSpaceEntity? Serialize(ValueSpace dto)
    {
      var vs = GetByName(dto.Name);
      if (vs == null)
      {
        return null;
      }
      vs.Title = dto.Title;
      vs.ValueSpaceType = dto.ValueSpaceType;

      switch (dto.ValueSpaceType)
      {
        case ValueSpaceType.Code:
          var codeStr = string.Empty;
          foreach (var (key, value) in (Dictionary<string, string>)dto.Conditions!)
          {
            if (string.IsNullOrEmpty(key.Trim()))
            {
              continue;
            }

            codeStr += key + ":" + value + "\n";
          }

          vs.Items = codeStr[..^1];
          break;

        case ValueSpaceType.Range:
          var rangeStr = string.Empty;
          foreach (var i in (Dictionary<string, float>)dto.Conditions!)
          {
            if (string.IsNullOrEmpty(i.Key.Trim()))
            {
              continue;
            }

            rangeStr += i.Key + ":" + i.Value.ToString("0.00") + "\n";
          }

          vs.Items = rangeStr[..^1];
          break;

        case ValueSpaceType.Regex:
          vs.Items = string.Join("\n", dto.GetItemNames());
          break;

        default:
          break;
      }

      return vs;
    }
  }
}