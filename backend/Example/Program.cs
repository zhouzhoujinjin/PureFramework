using Backend;

public class Program
{
  public static void Main(string[] args)
  {
    CreateHostBuilder(args).Build().Run();
  }

  public static IHostBuilder CreateHostBuilder(string[] args) =>
      Host.CreateDefaultBuilder(args)
          .ConfigureLogging(logging =>
          {
            logging.AddConsole();
          })
          .ConfigureWebHostDefaults(webBuilder =>
          {
            webBuilder.UseStartup<Startup>()
            .UseKestrel(options =>
            {
              options.Limits.MaxRequestBodySize = null; // or a given limit
            });
          });

}