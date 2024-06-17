using FluentAssertions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using St.HolyChain.Core.Abstractions;
using St.HolyChain.Core.Extensions;
using St.HolyChain.TestTools;
using System.Net;
using System.Net.Http.Json;
using Xunit.Abstractions;

namespace St.HolyChain.Core.Tests.IntegrationTests
{
    public class UnTypedSequenceTests : IDisposable
    {
        private readonly ITestOutputHelper _output;
        public UnTypedSequenceTests(ITestOutputHelper output)
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

                services.AddHolyChain<SimpleRequest>(config =>
                {
                    config.UseHandler<CreateOrder>()
                        .UseHandler<RegisterOrder>()
                        .UseHandler<PublishOrder>();
                });

            }, configureEndPoints: app =>
            {
                app.MapPost("/create", async (SimpleRequest request,
                    [FromServices] IPipeline<SimpleRequest> pipeline,
                    CancellationToken cancellationToken) =>
                {
                    var context = await pipeline.RunAsync(request,
                        configureOptions: x => x.EnableLog = true,
                        cancellationToken: cancellationToken);
                    return Results.Created("", context.DataCollection.Data);
                });
            });


            using var host = await hostBuilder.StartAsync();
            var httpClient = host.GetTestClient();

            // Act
            var actual = await httpClient.PostAsJsonAsync("/create", new SimpleRequest
            {
                Value1 = 1,
                Value2 = 2,
                Value3 = 3
            });

            // Assert
            actual.StatusCode.Should().Be(HttpStatusCode.Created);
            var response = await actual.Content.DeserializeHttpContentAsync<Dictionary<string, int>>();


            response["Value1"].Should().Be(1);
            response["Value2"].Should().Be(2);
            response["Value3"].Should().Be(3);

        }


        [Fact]
        public async Task Test2()
        {
            // Arrange

            var hostBuilder = WebHostBuilderHelpers.CreateTestServer(configureServices: services =>
            {
                services.AddSingleton<ILoggerProvider>(_ => new XunitLoggerProvider(_output));

                services.AddHolyChain<SimpleRequest>(config =>
                {
                    config.UseHandler<CreateOrder>()
                        .UseHandler<RegisterOrder>()
                        .UseHandler<PublishOrder>();
                });

            }, configureEndPoints: app =>
            {
                app.MapPost("/create", async (SimpleRequest request,
                    [FromServices] IPipeline<SimpleRequest> pipeline,
                    CancellationToken cancellationToken) =>
                {

                    var context = await pipeline.RunAsync(request,
                        configureOptions: x => x.EnableLog = true,
                        cancellationToken: cancellationToken);

                    return Results.Created("", context.DataCollection.Data);
                });
            });

            using var host = await hostBuilder.StartAsync();
            var httpClient = host.GetTestClient();

            // Act

            var tasks = new List<Task<HttpResponseMessage>>();

            for (int i = 0; i < 10; i++)
            {
                var task = httpClient.PostAsJsonAsync("/create", new SimpleRequest
                {
                    Value1 = i,
                    Value2 = i + 1,
                    Value3 = i + 2
                });

                tasks.Add(task);
            }

            await Task.WhenAll(tasks);

            for (var i = 0; i < 10; i++)
            {
                var current = tasks[i].Result;

                // Assert
                current.StatusCode.Should().Be(HttpStatusCode.Created);
                var response = await current.Content.DeserializeHttpContentAsync<Dictionary<string, int>>();
                response["Value1"].Should().Be(i);
                response["Value2"].Should().Be(i + 1);
                response["Value3"].Should().Be(i + 2);
                _output.WriteLine(await current.Content.ReadAsStringAsync());
            }



        }

        public void Dispose()
        {
        }

        public class SimpleRequest
        {
            public int Value1 { get; set; }
            public int Value2 { get; set; }
            public int Value3 { get; set; }
        }

        public class CreateOrder : Activity<SimpleRequest>
        {
            public override Task HandleAsync(SimpleRequest request, IPipelineRequestContext pipelineRequestContext,
                CancellationToken cancellationToken = default)
            {
                pipelineRequestContext.DataCollection.Set("Value1", request.Value1);

                return Task.CompletedTask;
            }
        }

        public class RegisterOrder : Activity<SimpleRequest>
        {
            public override Task HandleAsync(SimpleRequest request, IPipelineRequestContext pipelineRequestContext,
                CancellationToken cancellationToken = default)
            {
                pipelineRequestContext.DataCollection.Set("Value2", request.Value2);

                return Task.CompletedTask;
            }
        }
        public class PublishOrder : Activity<SimpleRequest>
        {
            public override Task HandleAsync(SimpleRequest request, IPipelineRequestContext pipelineRequestContext,
                CancellationToken cancellationToken = default)
            {
                pipelineRequestContext.DataCollection.Set("Value3", request.Value3);
                return Task.CompletedTask;
            }
        }
    }
}