using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Options;
using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using SystemFile = System.IO.File;

namespace Microsoft.Extensions.Caching.File
{
  public class FileCache : IDistributedCache, IDisposable
  {
    private readonly FileCacheOptions options;
    private static readonly object locker = new object();
    private SHA256 sha256;

    public FileCache(IOptions<FileCacheOptions> optionsAccessor)
    {
      options = optionsAccessor.Value;

      if (!string.IsNullOrEmpty(options.CacheFolder) && !Directory.Exists(options.CacheFolder))
      {
        Directory.CreateDirectory(options.CacheFolder!);
      }
      sha256 = SHA256.Create();
    }

    public byte[]? Get(string key)
    {
      var file = GetRealCacheKey(key);
      if (file == null)
      {
        return null;
      }
      var expiredTime = DateTimeOffset.FromUnixTimeMilliseconds(long.Parse(file[(file.IndexOf("_") + 1)..]));
      if (expiredTime > DateTimeOffset.UtcNow)
      {
        return SystemFile.ReadAllBytes(file);
      }
      else
      {
        SystemFile.Delete(file);
      }
      return null;
    }

    public async Task<byte[]?> GetAsync(string key, CancellationToken token = default)
    {
      var file = GetRealCacheKey(key);
      if (file == null)
      {
        return null;
      }
      var expiredTime = DateTimeOffset.FromUnixTimeMilliseconds(long.Parse(file[(file.IndexOf("_") + 1)..]));
      if (expiredTime > DateTimeOffset.UtcNow)
      {
        return await SystemFile.ReadAllBytesAsync(file, token);
      }
      else
      {
        SystemFile.Delete(file);
      }

      return null;
    }

    public void Refresh(string key)
    {
    }

    public async Task RefreshAsync(string key, CancellationToken token = default)
    {
      await Task.CompletedTask;
    }

    public void Remove(string key)
    {
      var files = Directory.GetFiles(options.CacheFolder, HashKey(key) + "_*").Reverse();

      if (files.Any())
      {
        lock (locker)
        {
          foreach (var file in files)
          {
            SystemFile.Delete(file);
          }
        }
      }
    }

    public async Task RemoveAsync(string key, CancellationToken token = default)
    {
      var files = Directory.GetFiles(options.CacheFolder, HashKey(key) + "_*").Reverse();

      if (files.Any())
      {
        lock (locker)
        {
          foreach (var file in files)
          {
            SystemFile.Delete(file);
          }
        }
      }
      await Task.CompletedTask;
    }

    public void Set(string key, byte[] value, DistributedCacheEntryOptions options)
    {
      DateTimeOffset expiredTime;
      if (options.AbsoluteExpiration.HasValue)
      {
        expiredTime = options.AbsoluteExpiration.Value;
      }
      else if (options.AbsoluteExpirationRelativeToNow.HasValue)
      {
        expiredTime = DateTimeOffset.Now + options.AbsoluteExpirationRelativeToNow.Value;
      }
      else
      {
        throw new ArgumentException("FileCache doesn't support SlidingExpiration property.");
      }

      var file = GenerateRealCacheKey(key, expiredTime);
      lock (locker)
      {
        Remove(key);
        SystemFile.WriteAllBytes(file, value);
      }
    }

    public async Task SetAsync(string key, byte[] value, DistributedCacheEntryOptions options, CancellationToken token = default)
    {
      DateTimeOffset expiredTime;
      if (options.AbsoluteExpiration.HasValue)
      {
        expiredTime = options.AbsoluteExpiration.Value;
      }
      else if (options.AbsoluteExpirationRelativeToNow.HasValue)
      {
        expiredTime = DateTimeOffset.Now + options.AbsoluteExpirationRelativeToNow.Value;
      }
      else
      {
        throw new ArgumentException("FileCache doesn't support SlidingExpiration property.");
      }

      var file = GenerateRealCacheKey(key, expiredTime);
      await RemoveAsync(key, token);
      lock (locker)
      {
        SystemFile.WriteAllBytes(file, value);
      }
      await Task.CompletedTask;
    }

    private string HashKey(string key)
    {
      var byteKey = Encoding.UTF8.GetBytes(key);
      byte[] hashValue = sha256.ComputeHash(byteKey);
      return string.Concat(hashValue.Select(b => b.ToString("X2")));
    }

    private string? GetRealCacheKey(string key)
    {
      var files = Directory.GetFiles(options.CacheFolder, HashKey(key) + "_*").Reverse();
      return files.FirstOrDefault();
    }

    private string GenerateRealCacheKey(string key, DateTimeOffset expiredTime)
    {
      return Path.Combine(options.CacheFolder, $"{HashKey(key)}_{expiredTime.ToUnixTimeMilliseconds()}");
    }

    public void Dispose()
    {
      GC.SuppressFinalize(this);
      sha256.Dispose();
    }
  }
}