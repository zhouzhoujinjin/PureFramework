using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using CyberStone.Core.Entities;
using CyberStone.Core.Models;
using CyberStone.Core.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace CyberStone.Core.Managers
{
  public partial class UserManager
  {
    private readonly JsonSerializerOptions jsonSerializerOptions = new JsonSerializerOptions
    {
      Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
      PropertyNameCaseInsensitive = true,
      DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
    };

    /// <summary>
    /// 上传用户头像
    /// </summary>
    /// <param name="image"></param>
    /// <param name="imageSaveFolder"></param>
    /// <returns></returns>
    public string? UploadImage(IFormFile image, string imageSaveFolder = "avatars")
    {
      var rootPath = Path.Combine(uploadOptions.AbsolutePath, imageSaveFolder);
      var fileName = UploadUtils.MoveFile(image, rootPath, false);
      if (string.IsNullOrEmpty(fileName)) return null;
      fileName = UploadUtils.GetUrl(fileName, rootPath, $"{uploadOptions.WebRoot}/{imageSaveFolder}");

      return fileName;
    }

    /// <summary>
    /// 上传用户头像
    /// </summary>
    /// <param name="base64"></param>
    /// <param name="imageSaveFolder"></param>
    /// <returns></returns>
    public string? UploadImage(string base64, string imageSaveFolder = "avatars")
    {
      var rootPath = Path.Combine(uploadOptions.AbsolutePath, imageSaveFolder);
      var fileName = UploadUtils.CreateBase64Image(base64, rootPath, false);
      if (string.IsNullOrEmpty(fileName)) return null;
      fileName = UploadUtils.GetUrl(fileName, rootPath, $"{uploadOptions.WebRoot}/{imageSaveFolder}");

      return fileName;
    }

    /// <summary>
    /// 获得用户全部资料
    /// </summary>
    /// <param name="userId"></param>
    /// <returns></returns>
    public async Task<Dictionary<string, object?>> GetUserProfilesAsync(long userId)
    {
      var result = await cache.GetAsync(string.Format(CacheKeys.UserProfiles, userId), async () =>
      {
        var sql = context.UserProfiles.Include(up => up.ProfileKey)
          .ThenInclude(p => p.ValueSpace)
          .Where(up => up.UserId == userId && up.ProfileKey.IsVisible)
          .Select(up => new Profile
          {
            Id = up.Id,
            Name = up.ProfileKey.Name,
            CategoryCode = up.ProfileKey.CategoryCode,
            ValueSpaceName = up.ProfileKey.ValueSpace.Name,
            ProfileKeyName = up.ProfileKey.Name,
            Value = up.Value
          });
        var s = sql.ToQueryString();
        var profiles = await sql.ToListAsync();

        return profiles.ToDictionary(p => $"{p.Name}/{p.CategoryCode}", DeserializeValue);
      }, new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10) });
      return result ?? new Dictionary<string, object?>();
    }

    public async Task<T?> GetUserProfileAsync<T>(long userId, string profileKey)
    {
      var profiles = await GetUserProfilesAsync(userId);
      var kv = profiles.FirstOrDefault(p =>
        string.Equals(profileKey, p.Key.Split("/").First(), StringComparison.CurrentCultureIgnoreCase));
      return (T?)(kv.Value ?? default(T));
    }

    public async Task<object> GetUserProfileAsync(long userId, string profileKey)
    {
      var profiles = await GetUserProfilesAsync(userId);
      return profiles.FirstOrDefault(p =>
        string.Equals(profileKey, p.Key.Split("/").First(), StringComparison.CurrentCultureIgnoreCase));
    }

    public async Task<Dictionary<string, object?>> GetUserProfilesDataAsync(long userId, IEnumerable<string> profileKeys)
    {
      var profiles = await GetUserProfilesAsync(userId);
      return profiles.Where(
        p => profileKeys.Contains(p.Key.Split("/").First())
      ).ToDictionary(p => p.Key.Split("/").First(), p => p.Value);
    }

    public async Task<Dictionary<string, object?>> GetUserProfilesDataAsync(long userId)
    {
      var profiles = await GetUserProfilesAsync(userId);
      return profiles.ToDictionary(p => p.Key.Split("/").First(), p => p.Value);
    }

    public async Task<int> AddProfilesAsync(long userId, Dictionary<string, JsonElement> profiles)
    {
      var dict = new Dictionary<string, object?>();
      foreach (var kv in profiles)
      {
        switch (kv.Value.ValueKind)
        {
          case JsonValueKind.String:
            dict[kv.Key] = kv.Value.ToString();
            break;

          case JsonValueKind.Number:
            var result = kv.Value.TryGetInt32(out var v);
            dict[kv.Key] = result ? v : kv.Value.GetDouble();
            break;

          case JsonValueKind.True:
            dict[kv.Key] = true;
            break;

          case JsonValueKind.False:
            dict[kv.Key] = false;
            break;

          default:
            dict[kv.Key] = null;
            break;
        }
      }

      return await AddProfilesAsync(userId, dict);
    }

    public async Task<int> AddProfilesAsync(long userId, Dictionary<string, object?> profiles)
    {
      var dictProfileKeys = profiles.Keys.ToDictionary(k => k, k => k.ToUpper());
      var entities = await userProfiles.Include(x => x.ProfileKey)
        .Where(x => x.UserId == userId && dictProfileKeys.Values.Contains(x.ProfileKey.Name.ToUpper()))
        .ToDictionaryAsync(x => x.ProfileKey.Name, x => x);
      var cleanedProfiles = new Dictionary<string, object>();
      var allKeys = profileKeyMap.GetAllProfileKeys();
      foreach (var profileKey in allKeys)
      {
        var profileName = profileKey.Name.ToCamelCase();
        var (key, _) = dictProfileKeys.FirstOrDefault(x => x.Value == (profileKey.Name + "Code").ToUpper());
        if (!string.IsNullOrEmpty(key))
        {
          var result =
            (profileKey.ValueSpace as CodeValueSpace)!.TryGetCodeByValue(profiles[key]!.ToString(), out var value);
          if (result)
          {
            cleanedProfiles[profileName] = value!;
          }
        }
      }

      foreach (var key in dictProfileKeys)
      {
        var profileKey = allKeys.FirstOrDefault(x => x.Name.ToUpper() == key.Value);
        if (profileKey == null)
        {
          throw new CyberStoneException($"资料信息中不存在 [{key.Key}]");
        }

        cleanedProfiles[profileKey.Name] = profiles[key.Key]!;
      }

      foreach (var (key, value) in cleanedProfiles)
      {
        if (entities.ContainsKey(key))
        {
          entities[key].Value = ProfileSerialize(value);
          context.UserProfiles.Update(entities[key]);
        }
        else
        {
          entities[key] = new UserProfileEntity()
          {
            Value = ProfileSerialize(value),
            FullName = key,
            UserId = userId,
            ProfileKeyId = profileKeyMap.Value[key].Id
          };
          context.UserProfiles.Add(entities[key]);
        }
      }
      var res = await context.SaveChangesAsync();

      await cache.RemoveAsync(string.Format(CacheKeys.Users, userId));
      await cache.RemoveAsync(string.Format(CacheKeys.UserProfiles, userId));
      return res;
    }

    public async Task<int> AddProfileAsync(UserEntity user, string profileKeyName, object value)
    {
      return await AddProfileAsync(user.Id, profileKeyName, value);
    }

    public async Task<int> AddProfileAsync(long userId, string profileKeyName, object value)
    {
      var profileKeys = profileKeyMap.Value;

      var _ = profileKeys.TryGetValue(profileKeyName, out var profileKey);

      if (profileKey == null)
      {
        return 0;
      }

      var entity = await userProfiles.Where(x => x.UserId == userId && x.ProfileKeyId == profileKey.Id)
        .FirstOrDefaultAsync();
      if (entity == null)
      {
        entity = new UserProfileEntity
        {
          Value = ProfileSerialize(value),
          FullName = profileKey.Name,
          UserId = userId,
          ProfileKeyId = profileKey.Id
        };
        context.Add(entity);
      }
      else
      {
        entity.Value = ProfileSerialize(value);
        context.Update(entity);
      }

      await cache.RemoveAsync(string.Format(CacheKeys.Users, userId));
      await cache.RemoveAsync(string.Format(CacheKeys.UserProfiles, userId));
      return await context.SaveChangesAsync();
    }

    /// <summary>
    /// 根据用户名生成用户对象
    /// </summary>
    /// <param name="userName"></param>
    /// <param name="profileNamesOrCategories"></param>
    /// <returns></returns>
    public async Task<User?> GetBriefUserAsync(
      string userName,
      IEnumerable<string>? profileNamesOrCategories = null
    )
    {
      var normalizedUserName = this.NormalizeName(userName);
      var cachedUser = await cache.GetAsync<User>(string.Format(CacheKeys.Users, normalizedUserName));

      if (cachedUser == null)
      {
        var userEntity = await Users.Where(u => u.NormalizedUserName == normalizedUserName).Select(
          u => new UserEntity
          {
            Id = u.Id,
            UserName = u.UserName
          }).FirstOrDefaultAsync();

        if (userEntity != null)
        {
          return await GetBriefUserAsync(userEntity, profileNamesOrCategories, true);
        }
      }
      else
      {
        var filteredProfileKeys = profileKeyMap.GetFilteredProfileKeyNames(profileNamesOrCategories);
        var user = new User
        {
          Id = cachedUser.Id,
          UserName = cachedUser.UserName,
          Profiles = cachedUser.Profiles
            .Where(x => filteredProfileKeys.Contains(x.Key))
            .ToDictionary(x => x.Key, x => x.Value)
        };
        return user;
      }

      return null;
    }

    /// <summary>
    /// 根据用户 Id 生成用户对象
    /// </summary>
    /// <param name="userId"></param>
    /// <param name="profileNamesOrCategories"></param>
    /// <returns></returns>
    public async Task<User?> GetBriefUserAsync(
      long userId,
      IEnumerable<string>? profileNamesOrCategories = null
    )
    {
      var cachedUser = await cache.GetAsync<User>(string.Format(CacheKeys.Users, userId));

      if (cachedUser == null)
      {
        var userEntity = await Users.Where(u => u.Id == userId).Select(
          u => new UserEntity
          {
            Id = u.Id,
            UserName = u.UserName
          }).FirstOrDefaultAsync();

        if (userEntity != null)
        {
          return await GetBriefUserAsync(userEntity, profileNamesOrCategories, true);
        }
      }
      else
      {
        var filteredProfileKeys = profileKeyMap.GetFilteredProfileKeyNames(profileNamesOrCategories);
        var user = new User
        {
          Id = cachedUser.Id,
          UserName = cachedUser.UserName,
          Profiles = cachedUser.Profiles
            .Where(x => filteredProfileKeys.Contains(x.Key))
            .ToDictionary(x => x.Key, x => x.Value)
        };
        return user;
      }

      return null;
    }

    /// <summary>
    /// 根据用户实体 生成用户对象
    /// </summary>
    /// <param name="userEntity"></param>
    /// <param name="profileNamesOrCategories"></param>
    /// <param name="force">是否不从缓存测试读取而直接从数据库读取</param>
    /// <returns></returns>
    public async Task<User?> GetBriefUserAsync(
      UserEntity userEntity,
      IEnumerable<string>? profileNamesOrCategories = null,
      bool force = false
    )
    {
      User? cachedUser;
      if (!force)
      {
        cachedUser = await cache.GetAsync(
          string.Format(CacheKeys.Users, userEntity.Id), async () =>
          {
            var user = new User(userEntity.Id, userEntity.UserName);
            var profiles = await GetUserProfilesDataAsync(userEntity.Id);
            user.Profiles = profiles;
            return user;
          },
          new DistributedCacheEntryOptions { AbsoluteExpiration = DateTimeOffset.MaxValue }
        );
      }
      else
      {
        cachedUser = new User(userEntity.Id, userEntity.UserName);
        var profiles = await GetUserProfilesDataAsync(userEntity.Id);
        cachedUser.Profiles = profiles;
        await cache.SetAsync(string.Format(CacheKeys.Users, userEntity.Id), cachedUser,
          new DistributedCacheEntryOptions { AbsoluteExpiration = DateTimeOffset.MaxValue });
      }

      if (cachedUser == null) return null;

      var filteredProfileKeys = profileKeyMap.GetFilteredProfileKeyNames(profileNamesOrCategories);
      var user = new User
      {
        Id = cachedUser.Id,
        UserName = cachedUser.UserName,
        Profiles = cachedUser.Profiles
          .Where(x => filteredProfileKeys.Contains(x.Key))
          .ToDictionary(x => x.Key, x => x.Value)
      };

      return user;
    }

    public async Task<IEnumerable<User>> GetBriefUsersAsync(
      IEnumerable<UserEntity> userEntities,
      IEnumerable<string>? profileNamesOrCategories = null,
      bool force = false
    )
    {
      var cachedUsers = new List<User>();
      var uncachedUserEntities = userEntities.ToList();
      var filteredProfileKeys = profileKeyMap.GetFilteredProfileKeyNames(profileNamesOrCategories);
      if (!force)
      {
        uncachedUserEntities = new List<UserEntity>();
        foreach (var user in userEntities)
        {
          var u = await cache.GetAsync<User>(string.Format(CacheKeys.Users, user.Id));
          if (u == null)
          {
            uncachedUserEntities.Add(user);
          }
          else
          {
            cachedUsers.Add(new User
            {
              Id = u.Id,
              UserName = u.UserName,
              Profiles = u.Profiles.Where(x => filteredProfileKeys.Contains(x.Key))
                .ToDictionary(x => x.Key, x => x.Value)
            });
          }
        }
      }

      foreach (var userEntity in uncachedUserEntities)
      {
        var user = new User(userEntity.Id, userEntity.UserName);
        var profiles = await GetUserProfilesDataAsync(userEntity.Id);
        user.Profiles = profiles;
        await cache.SetAsync(string.Format(CacheKeys.Users, userEntity.Id), user,
          new DistributedCacheEntryOptions { AbsoluteExpiration = DateTimeOffset.MaxValue });
        cachedUsers.Add(new User
        {
          Id = user.Id,
          UserName = user.UserName,
          Profiles = user.Profiles.Where(x => filteredProfileKeys.Contains(x.Key))
            .ToDictionary(x => x.Key, x => x.Value)
        });
      }

      return cachedUsers;
    }

    /// <summary>
    /// 根据用户名列表获取用户信息
    /// </summary>
    /// <param name="userNames"></param>
    /// <param name="profileNamesOrCategories"></param>
    /// <returns></returns>
    public async Task<IEnumerable<User>> GetBriefUsersAsync(
      IEnumerable<string> userNames,
      IEnumerable<string>? profileNamesOrCategories = null
    )
    {
      var normalizedNames = userNames.Select(NormalizeName);

      var userEntities = await Users.Where(u => normalizedNames.Contains(u.NormalizedUserName)).Select(
        u => new UserEntity
        {
          Id = u.Id,
          UserName = u.UserName
        }
      ).ToArrayAsync();

      var cachedUsers = new List<User>();
      var uncachedUserEntities = new List<UserEntity>();

      var filteredProfileKeys = profileKeyMap.GetFilteredProfileKeyNames(profileNamesOrCategories);
      foreach (var user in userEntities)
      {
        var u = await cache.GetAsync<User>(string.Format(CacheKeys.Users, user.Id));
        if (u == null)
        {
          uncachedUserEntities.Add(user);
        }
        else
        {
          cachedUsers.Add(new User
          {
            Id = u.Id,
            UserName = u.UserName,
            Profiles = u.Profiles.Where(x => filteredProfileKeys.Contains(x.Key))
              .ToDictionary(x => x.Key, x => x.Value)
          });
        }
      }

      var uncachedUsers = await GetBriefUsersAsync(uncachedUserEntities, profileNamesOrCategories, true);
      cachedUsers.AddRange(uncachedUsers);
      return cachedUsers.OrderBy(x => x.UserName);
    }

    public async Task<IEnumerable<User>> GetBriefUsersAsync(
      IEnumerable<long> userIds,
      IEnumerable<string>? profileNamesOrCategories = null
    )
    {
      var cachedUsers = new List<User>();
      var unCachedUserIds = new List<long>();

      var filteredProfileKeys = profileKeyMap.GetFilteredProfileKeyNames(profileNamesOrCategories);
      foreach (var userId in userIds)
      {
        var u = await cache.GetAsync<User>(string.Format(CacheKeys.Users, userId));
        if (u == null)
        {
          unCachedUserIds.Add(userId);
        }
        else
        {
          cachedUsers.Add(new User
          {
            Id = u.Id,
            UserName = u.UserName,
            Profiles = u.Profiles.Where(x => filteredProfileKeys.Contains(x.Key))
              .ToDictionary(x => x.Key, x => x.Value)
          });
        }
      }

      if (unCachedUserIds.Count > 0)
      {
        var userEntities = await Users
          .Where(u => unCachedUserIds.Contains(u.Id) && u.IsDeleted == false)
          .Select(
            u => new UserEntity
            {
              Id = u.Id,
              UserName = u.UserName
            }).ToArrayAsync();

        var uncachedUsers = await GetBriefUsersAsync(userEntities, profileNamesOrCategories, true);
        cachedUsers.AddRange(uncachedUsers);
      }

      return cachedUsers.OrderBy(x => x.UserName);
    }

    private object? DeserializeValue(Profile profile)
    {
      if (profile.Value is string value)
      {
        var valueType = profileKeyMap.Get(profile.ProfileKeyName).ProfileType;
        return (valueType.IsPrimitive || valueType == typeof(string))
          ? Convert.ChangeType(profile.Value, valueType)
          : JsonSerializer.Deserialize(value, valueType);
      }
      else
      {
        return profile.Value;
      }
    }

    private string ProfileSerialize(object? value)
    {
      if (value == null)
      {
        return "";
      }

      var valueType = value.GetType();

      if (valueType.IsPrimitive || valueType == typeof(string) || valueType == typeof(DateTime))
      {
        return value.ToString() ?? string.Empty;
      }

      return JsonSerializer.Serialize(value, jsonSerializerOptions);
    }

    public async Task<long?> FindUserIdByUniqueProfileAsync(string profileKeyName, object value)
    {
      return await context.UserProfiles
        .Where(p => p.ProfileKey.Name == profileKeyName && p.Value == value.ToString())
        .Select(p => p.UserId).FirstOrDefaultAsync();
    }
  }
}