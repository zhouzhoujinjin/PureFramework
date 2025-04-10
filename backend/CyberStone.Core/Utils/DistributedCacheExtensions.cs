using Microsoft.Extensions.Caching.Distributed;
using System;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace CyberStone.Core.Utils
{
  public static class DistributedCacheExtensions
  {
    private static readonly JsonSerializerOptions JsonSerializerOptions = new()
    {
      PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
      Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
      DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
      PropertyNameCaseInsensitive = true
    };

    public static void Set<TValue>(this IDistributedCache cache, string key, TValue value, DistributedCacheEntryOptions options)
    {
      cache.Set(key, Encoding.UTF8.GetBytes(JsonSerializer.Serialize(value, JsonSerializerOptions)), options);
    }

    public static async Task SetAsync<TValue>(this IDistributedCache cache, string key, TValue value, DistributedCacheEntryOptions options)
    {
      await cache.SetAsync(key, Encoding.UTF8.GetBytes(JsonSerializer.Serialize(value, JsonSerializerOptions)), options).ConfigureAwait(false);
    }

    public static TValue? Get<TValue>(this IDistributedCache cache, string key)
    {
      var value = cache.Get(key);
      return value == null ? default : JsonSerializer.Deserialize<TValue>(Encoding.UTF8.GetString(value), JsonSerializerOptions);
    }

    public static TValue? Get<TValue>(this IDistributedCache cache, string key, Func<TValue> func, DistributedCacheEntryOptions options)
    {
      var value = Get<TValue>(cache, key);
      if (value != null) return value;

      value = func.Invoke();
      if (value == null) return default;
      cache.Set(key, value, options);
      return value;
    }

    public static async Task<TValue?> GetAsync<TValue>(this IDistributedCache cache, string key)
    {
      var value = await cache.GetAsync(key);

      return value == null ? default : JsonSerializer.Deserialize<TValue>(Encoding.UTF8.GetString(value), JsonSerializerOptions);
    }

    public static async Task<TValue?> GetAsync<TValue>(this IDistributedCache cache, string key, Func<Task<TValue>> func, DistributedCacheEntryOptions options)
    {
      var value = await GetAsync<TValue>(cache, key);
      if (value != null) return value;

      value = await func.Invoke();
      if (value == null) return default;
      await cache.SetAsync(key, value, options);
      return value;
    }
  }
}