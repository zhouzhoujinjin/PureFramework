using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.File;
using System;

namespace Microsoft.Extensions.DependencyInjection
{
  /// <summary>
  /// Extension methods for setting up Redis distributed cache related services in an <see cref="IServiceCollection" />.
  /// </summary>
  public static class FileCacheServiceCollectionExtensions
  {
    /// <summary>
    /// Adds file caching services to the specified <see cref="IServiceCollection" />.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection" /> to add services to.</param>
    /// <param name="setupAction">An <see cref="Action{FileCacheOptions}"/> to configure the provided
    /// <see cref="FileCacheOptions"/>.</param>
    /// <returns>The <see cref="IServiceCollection"/> so that additional calls can be chained.</returns>
    public static IServiceCollection AddFileCache(this IServiceCollection services, Action<FileCacheOptions> setupAction)
    {
      if (services == null)
      {
        throw new ArgumentNullException(nameof(services));
      }

      if (setupAction == null)
      {
        throw new ArgumentNullException(nameof(setupAction));
      }

      services.AddOptions();
      services.Configure(setupAction);
      services.Add(ServiceDescriptor.Singleton<IDistributedCache, FileCache>());

      return services;
    }
  }
}