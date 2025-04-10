using Microsoft.EntityFrameworkCore;
using CyberStone.Core.Entities;
using CyberStone.Core.Models;
using System.Collections.Generic;
using System.Linq;

namespace CyberStone.Core.Managers
{
  public class ProfileKeyMap
  {
    public SortedDictionary<string, ProfileKey> Value { get; set; } = new();

    public ProfileKey Get(string name)
    {
      return Value[name];
    }

    public IEnumerable<ProfileKey> GetAllProfileKeys()
    {
      return Value.Values;
    }

    public IEnumerable<ProfileKey> GetBriefKeys()
    {
      return Value.Values.Where(x => x.IsBrief);
    }

    public IEnumerable<string> GetBriefKeyNames()
    {
      return Value.Values.Where(x => x.IsBrief).Select(x => x.Name);
    }

    public IEnumerable<ProfileKey> GetFilteredProfileKeys(IEnumerable<string>? profileKeysOrCategories)
    {
      if (profileKeysOrCategories == null)
      {
        return GetBriefKeys();
      }

      var pkoc = profileKeysOrCategories.Select(x => x.ToUpper());
      return Value.Values.Where(x => pkoc.Contains(x.Name.ToUpper()) || pkoc.Contains(x.CategoryCode.ToUpper()));
    }

    public IEnumerable<string> GetFilteredProfileKeyNames(IEnumerable<string>? profileKeysOrCategories)
    {
      if (profileKeysOrCategories == null || !profileKeysOrCategories.Any())
      {
        return GetBriefKeyNames();
      }

      var pkoc = profileKeysOrCategories.Select(x => x.ToUpper());
      return Value.Values
        .Where(x => pkoc.Contains(x.Name.ToUpper()) || pkoc.Contains(x.CategoryCode.ToUpper()))
        .Select(x => x.Name);
    }
  }

  public class ProfileKeyManager
  {
    private readonly DbSet<ProfileKeyEntity> profileKeySet;
    private readonly ValueSpaceMap valueSpaceMap;

    internal static ProfileKeyMap ProfileKeyMap { get; } = new();

    public SortedDictionary<string, ProfileKey> GetPkMap() => ProfileKeyMap.Value;

    public ProfileKeyManager(
        CyberStoneDbContext context,
        ValueSpaceMap valueSpaceMap)
    {
      profileKeySet = context.ProfileKeys;
      this.valueSpaceMap = valueSpaceMap;
    }

    public void Initialize()
    {
      var values = profileKeySet.Where(x => x.IsVisible).AsNoTracking().ToList();
      var map = ProfileKeyMap.Value;
      foreach (var val in values)
      {
        map[val.Name] = new ProfileKey
        {
          Id = val.Id,
          CategoryCode = val.CategoryCode,
          Name = val.Name,
          IsBrief = val.IsBrief,
          Seachable = val.IsSearchable,
          ProfileTypeName = val.ProfileType!.FullName,
          ProfileType = val.ProfileType,
          IsPublic = val.CategoryCode == SystemProfileKeyCategory.Public,
          ValueSpaceName = val.ValueSpace?.Name,
          ValueSpace = val.ValueSpace == null ? null : valueSpaceMap.Get(val.ValueSpace!.Name)
        };
      }
    }

    /// <summary>
    /// 获得摘要资料的键信息
    /// </summary>
    /// <returns></returns>
    public IEnumerable<ProfileKey> GetBriefKeys()
    {
      return ProfileKeyMap.Value
        .Where(x => x.Value.IsBrief)
        .OrderBy(x => x.Key)
        .Select(x => x.Value);
    }

    /// <summary>
    /// 获得所有
    /// </summary>
    /// <returns></returns>
    public IEnumerable<ProfileKey> GetAllKeys()
    {
      return ProfileKeyMap.Value.Values;
    }

    public IEnumerable<ProfileKey> GetFilteredProfileKeys(IEnumerable<string>? profileKeysOrCategories)
    {
      if (profileKeysOrCategories == null)
      {
        return GetBriefKeys();
      }

      return GetAllKeys()
        .Where(x => profileKeysOrCategories.Contains(x.Name) || profileKeysOrCategories.Contains(x.CategoryCode));
    }

    public Dictionary<string, object?> GetUserFilteredProfiles(Dictionary<string, object> profiles, IEnumerable<ProfileKey> profileKeys)
    {
      var result = new Dictionary<string, object?>();
      foreach (var key in profileKeys)
      {
        if (profiles.ContainsKey(key.Name))
        {
          if (key.CategoryCode == SystemProfileKeyCategory.Secret)
          {
            var s = profiles[key.Name] as string;
            if (!string.IsNullOrEmpty(s))
            {
              var startIndex = s.Length < 3 ? 1 : s.Length / 3;
              var endIndex = s.Length < 3 ? s.Length - 1 : s.Length - s.Length / 3;
              s = s.Remove(startIndex, endIndex - startIndex).Insert(startIndex, new string('*', endIndex - startIndex));
            }
            result[key.Name] = s;
          }
          else
          {
            result[key.Name] = profiles[key.Name];
          }
        }
      }
      return result;
    }
  }
}