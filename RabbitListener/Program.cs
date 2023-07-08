using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RabbitListener.Application;
using RabbitListener.Infrastructure;
using Serilog;
using Serilog.Events;
using Serilog.Formatting.Json;

namespace RabbitListener
{
    public class Program
    {
        public static void Main(string[] args)
        {
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Information()
                .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
                .Enrich.FromLogContext()
                .WriteTo.Console(new JsonFormatter())
                .CreateLogger();

            var host = Host.CreateDefaultBuilder(args)
                .ConfigureAppConfiguration((hostingContext, config) =>
                {
                    config.SetBasePath(Directory.GetCurrentDirectory());
                    config.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);
                    config.AddEnvironmentVariables();
                })
                .ConfigureServices((hostContext, services) =>
                {
                    services.AddSingleton<RabbitMQConnection>();
                    services.AddLogging(builder =>
                    {
                        builder.SetMinimumLevel(LogLevel.Information);
                        builder.AddSerilog();
                    });

                    services.AddHostedService<RabbitListenerService>();

                })
                .UseConsoleLifetime()
                .Build();

            host.Run();
        }
    }
}
