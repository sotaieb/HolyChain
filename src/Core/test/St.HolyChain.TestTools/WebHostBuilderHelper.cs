using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http.Json;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace St.HolyChain.TestTools;

public static class WebHostBuilderHelpers
{
    public static IHostBuilder CreateTestServer(Action<IServiceCollection>? configureServices = null, Action<IApplicationBuilder>? configureApplication = null,
        Action<IEndpointRouteBuilder>? configureEndPoints = null)
    {
        var hostBuilder = new HostBuilder();
        var builder = hostBuilder.ConfigureWebHost(webBuilder =>
            {
                webBuilder.UseTestServer()
                    .ConfigureServices(services =>
                    {
                        var configuration = new ConfigurationBuilder()
                            .SetBasePath(Environment.CurrentDirectory)
                            .AddJsonFile("appsettings.json", optional: false)
                            .AddEnvironmentVariables()
                            .Build();

                        services.AddSingleton<IConfiguration>(configuration);
                        services.AddLogging(builder => builder.AddConsole());

                        services.AddRouting();
                        configureServices?.Invoke(services);

                        services.Configure<JsonOptions>(x =>
                        {
                            x.SerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
                            x.SerializerOptions.PropertyNameCaseInsensitive = false;
                            x.SerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
                        });

                    }).Configure(app =>
                    {
                        app.UseRouting();
                        configureApplication?.Invoke(app);
                        app.UseEndpoints(endpoints =>
                        {
                            configureEndPoints?.Invoke(endpoints);
                        });
                    });
            });

        return builder;
    }
}


