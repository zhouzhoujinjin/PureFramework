using Backend.Managers;

namespace Backend
{
  public static class ServiceCollectionExtensions
  {
    public static void AddBackend(this IServiceCollection services, IConfiguration configuration)
    {
      services.AddScoped<ConfigManager>();
    }
  }
}
