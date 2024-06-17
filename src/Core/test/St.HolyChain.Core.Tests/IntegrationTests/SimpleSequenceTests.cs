using FluentAssertions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
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
    public class SimpleSequenceTests : IDisposable
    {
        private readonly ITestOutputHelper _output;
        public SimpleSequenceTests(ITestOutputHelper output)
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

                services.AddHolyChain<SimpleRequest, SimpleContext>(config =>
                {
                    config.UseHandler<CreateOrder>()
                            .UseHandler<RegisterOrder>()
                                .UseHandler<PublishOrder>();
                });

            }, configureEndPoints: app =>
            {
                app.MapPost("/create", async (SimpleRequest request,
                    IPipeline<SimpleRequest, SimpleContext> pipeline,
                    CancellationToken cancellationToken) =>
                {
                    var context = await pipeline.RunAsync(request,
                        configureOptions: x => x.EnableLog = true,
                        cancellationToken: cancellationToken);

                    return Results.Created("",context);
                });
            });

            using var host = await hostBuilder.StartAsync();
            var httpClient = host.GetTestClient();

            // Act
            var actual = await httpClient.PostAsJsonAsync("/create", new SimpleRequest());

            // Assert
            actual.StatusCode.Should().Be(HttpStatusCode.Created);
            var response = await actual.Content.DeserializeHttpContentAsync<PipelineRequestContext<SimpleContext>>();
            response.Data.Value1.Should().Be(1);
            response.Data.Value2.Should().Be(2);
            response.Data.Value3.Should().Be(3);

        }

        public void Dispose()
        {
        }

        public class SimpleRequest();

        public class SimpleContext : IContext
        {
            public int Value1 { get; set; }
            public int Value2 { get; set; }
            public int Value3 { get; set; }
        } 

        public class CreateOrder : Activity<SimpleRequest, SimpleContext>
        {
            public override Task HandleAsync(SimpleRequest request, IPipelineRequestContext<SimpleContext> pipelineRequestContext,
                CancellationToken cancellationToken = default)
            {
                pipelineRequestContext.Data.Value1 = 1;

                return Task.CompletedTask;
            }
        }

        public class RegisterOrder : Activity<SimpleRequest, SimpleContext>
        {
            public override Task HandleAsync(SimpleRequest request, IPipelineRequestContext<SimpleContext> pipelineRequestContext,
                CancellationToken cancellationToken = default)
            {
                pipelineRequestContext.Data.Value2 = 2;

                return Task.CompletedTask;
            }
        }
        public class PublishOrder : Activity<SimpleRequest, SimpleContext>
        {
            public override Task HandleAsync(SimpleRequest request, IPipelineRequestContext<SimpleContext> pipelineRequestContext,
                CancellationToken cancellationToken = default)
            {
                pipelineRequestContext.Data.Value3 = 3;

                return Task.CompletedTask;
            }
        }
    }
}