using FluentAssertions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using St.HolyChain.Core.Abstractions;
using St.HolyChain.Core.Extensions;
using St.HolyChain.Core.Models;
using St.HolyChain.TestTools;
using System.Net;
using System.Net.Http.Json;
using Xunit.Abstractions;

namespace St.HolyChain.Core.Tests.IntegrationTests
{
    public class SimpleParallelRetryTests : IDisposable
    {
        private readonly ITestOutputHelper _output;
        public SimpleParallelRetryTests(ITestOutputHelper output)
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
                    config.UseHandler<GetAccount>(x => { x.GroupId = 1; })
                        .UseHandler<CreateOrder>(x => { x.GroupId = 2; })
                            .UseHandler<RegisterOrder>(x => { x.GroupId = 2; })
                                .UseHandler<PublishOrder>(x => { x.GroupId = 3; });
                }).AddInMemoryAuditProvider();

            }, configureEndPoints: app =>
            {
                app.MapPost("/create", async (SimpleRequest request,
                    IPipelineFactory pipelineFactory,
                    CancellationToken cancellationToken) =>
                {
                    var pipeline = pipelineFactory.GetPipeline<SimpleRequest, SimpleContext>();
                    var context = await pipeline.RunAsync(request,
                        configureContext: x => x.DataCollection.Set("ThrowException", request.ThrowException),
            configureOptions: x =>
                        {
                            x.EnableLog = true;
                            x.Id = request.Id;
                            x.RunMode = RunMode.Parallel;
                            x.EnableRetry = true;
                            x.EnableCompensation = true;
                        },
                        cancellationToken: cancellationToken);

                    return Results.Created("", context);
                });
            });

            using var host = await hostBuilder.StartAsync();
            var httpClient = host.GetTestClient();

            var processId = Guid.NewGuid().ToString();
            // Act
            var actual1 = () => httpClient.PostAsJsonAsync("/create", new SimpleRequest(true, processId));

            await actual1.Should().ThrowAsync<Exception>();

            var actual = await httpClient.PostAsJsonAsync("/create", new SimpleRequest(false, processId));

            // Assert
            actual.StatusCode.Should().Be(HttpStatusCode.Created);
            var response = await actual.Content.DeserializeHttpContentAsync<PipelineRequestContext<SimpleContext>>();
            response.Data.Value1.Should().Be(1);
            response.Data.Value2.Should().Be(2);
            response.Data.Value3.Should().Be(3);
            response.Data.Value4.Should().Be(4);

        }

        public void Dispose()
        {
        }

        public record SimpleRequest(bool ThrowException, string Id);

        public class SimpleContext : IContext
        {
            public int Value1 { get; set; }
            public int Value2 { get; set; }
            public int Value3 { get; set; }
            public int Value4 { get; set; }
        }

        public class GetAccount : Activity<SimpleRequest, SimpleContext>
        {
            public override Task HandleAsync(SimpleRequest request, IPipelineRequestContext<SimpleContext> pipelineRequestContext,
                CancellationToken cancellationToken = default)
            {
                pipelineRequestContext.Data.Value1 = 1;

                return Task.CompletedTask;
            }
        }

        public class CreateOrder : Activity<SimpleRequest, SimpleContext>
        {
            public override async Task HandleAsync(SimpleRequest request, IPipelineRequestContext<SimpleContext> pipelineRequestContext,
                CancellationToken cancellationToken = default)
            {
                await Task.Delay(100, cancellationToken);

                pipelineRequestContext.Data.Value2 = 2;
            }
        }

        public class RegisterOrder : Activity<SimpleRequest, SimpleContext>
        {
            public override Task HandleAsync(SimpleRequest request, IPipelineRequestContext<SimpleContext> pipelineRequestContext,
                CancellationToken cancellationToken = default)
            {
                if (pipelineRequestContext.DataCollection.Get<bool>("ThrowException") == true)
                {
                    throw new Exception("something NOK");
                }

                pipelineRequestContext.Data.Value3 = 3;

                return Task.CompletedTask;
            }
        }
        public class PublishOrder : Activity<SimpleRequest, SimpleContext>
        {
            public override Task HandleAsync(SimpleRequest request, IPipelineRequestContext<SimpleContext> pipelineRequestContext,
                CancellationToken cancellationToken = default)
            {
                pipelineRequestContext.Data.Value4 = 4;

                return Task.CompletedTask;
            }
        }
    }
}