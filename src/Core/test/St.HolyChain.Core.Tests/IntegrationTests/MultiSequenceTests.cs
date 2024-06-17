using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using St.HolyChain.Core.Abstractions;
using St.HolyChain.Core.Extensions;
using St.HolyChain.TestTools;
using System.Net.Http.Json;
using Xunit.Abstractions;

namespace St.HolyChain.Core.Tests.IntegrationTests
{
    public class MultiSequenceTests : IDisposable
    {
        private readonly ITestOutputHelper _output;
        public MultiSequenceTests(ITestOutputHelper output)
        {
            _output = output;
        }

        [Fact]
        public async Task Test1()
        {
            // Arrange
            var hostBuilder = WebHostBuilderHelpers.CreateTestServer(configureServices: services =>
            {
                services.AddSingleton<ILoggerProvider>(_ => new XunitLoggerProvider(_output));

                services.AddHolyChain<SimpleRequest1, SimpleContext1>(config =>
                {
                    config.UseHandler<CreateOrder1>(x => x.OrderId = 10)
                            .UseHandler<RegisterOrder1>()
                                .UseHandler<PublishOrder1>();
                });

                services.AddHolyChain<SimpleRequest2, SimpleContext2>(config =>
                {
                    config.UseHandler<CreateOrder2>()
                        .UseHandler<RegisterOrder2>()
                        .UseHandler<PublishOrder2>();
                });

            }, configureEndPoints: app =>
            {
                app.MapPost("/check", async (CancellationToken cancellationToken) =>
                {
                    await Task.Delay(300, cancellationToken);
                    return Results.Created();
                });

                app.MapPost("/create1", async (SimpleRequest1 request,
                    IPipeline<SimpleRequest1, SimpleContext1> pipeline,
                    CancellationToken cancellationToken) =>
                {
                    var context = await pipeline.RunAsync(request,
                        configureOptions: x => x.EnableLog = true,
                        cancellationToken: cancellationToken);

                    return Results.Created();
                });

                app.MapPost("/create2", async (SimpleRequest2 request,
                    IPipeline<SimpleRequest2, SimpleContext2> pipeline,
                    CancellationToken cancellationToken) =>
                {
                    var context = await pipeline.RunAsync(request,
                        configureOptions: x => x.EnableLog = true,
                        cancellationToken: cancellationToken);
                    return Results.Created();
                });
            });

            using var host = await hostBuilder.StartAsync();

            var httpClient = host.GetTestClient();

            // Act

            await httpClient.PostAsJsonAsync("/check", new object());

            var tasks = new List<Task>();
            for (var i = 0; i < 10; i++)
            {
                var task1 = httpClient.PostAsJsonAsync("/create1", new SimpleRequest1());
                tasks.Add(task1);

                var task2 = httpClient.PostAsJsonAsync("/create2", new SimpleRequest2());
                tasks.Add(task2);
            }

            await Task.WhenAll(tasks.ToArray());

            // Assert
        }

        public void Dispose()
        {
        }

        public class SimpleRequest1();

        public class SimpleContext1() : IContext;

        public class SimpleRequest2();

        public class SimpleContext2() : IContext;

        public sealed class CreateOrder1 : Activity<SimpleRequest1, SimpleContext1>
        {
            public override Task HandleAsync(SimpleRequest1 request, IPipelineRequestContext<SimpleContext1> pipelineRequestContext,
                CancellationToken cancellationToken = default)
            {
                return Task.Delay(100, cancellationToken);
            }
        }

        public sealed class RegisterOrder1 : Activity<SimpleRequest1, SimpleContext1>
        {
            public override Task HandleAsync(SimpleRequest1 request2, IPipelineRequestContext<SimpleContext1> pipelineRequestContext,
                CancellationToken cancellationToken = default)
            {
                return Task.Delay(100, cancellationToken);
            }

        }
        public sealed class PublishOrder1 : Activity<SimpleRequest1, SimpleContext1>
        {
            public override Task HandleAsync(SimpleRequest1 request1, IPipelineRequestContext<SimpleContext1> pipelineRequestContext,
                CancellationToken cancellationToken = default)
            {
                return Task.Delay(100, cancellationToken);
            }
        }

        public sealed class CreateOrder2 : Activity<SimpleRequest2, SimpleContext2>
        {
            public override Task HandleAsync(SimpleRequest2 request2, IPipelineRequestContext<SimpleContext2> pipelineRequestContext,
                CancellationToken cancellationToken = default)
            {
                return Task.Delay(100, cancellationToken);
            }
        }

        public sealed class RegisterOrder2 : Activity<SimpleRequest2, SimpleContext2>
        {
            public override Task HandleAsync(SimpleRequest2 request2, IPipelineRequestContext<SimpleContext2> pipelineRequestContext,
                CancellationToken cancellationToken = default)
            {
                return Task.Delay(100, cancellationToken);
            }

        }
        public sealed class PublishOrder2 : Activity<SimpleRequest2, SimpleContext2>
        {
            public override Task HandleAsync(SimpleRequest2 request1, IPipelineRequestContext<SimpleContext2> pipelineRequestContext,
                CancellationToken cancellationToken = default)
            {
                return Task.Delay(100, cancellationToken);
            }
        }
    }
}