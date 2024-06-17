using HolyChain.Sample1;
using HolyChain.Sample1.UseCases.CreateOrder.Activities.CreateOrder;
using HolyChain.Sample1.UseCases.CreateOrder.Activities.GetOrder;
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
using System.Net.Http.Json;
using Xunit.Abstractions;

namespace St.HolyChain.Sample1.Tests
{
    public class MultiParallelTests : IDisposable
    {
        private readonly ITestOutputHelper _output;

        public MultiParallelTests(ITestOutputHelper output)
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

            services.AddHolyChain(x =>
                x.RegisterServicesFromAssembly(typeof(DependencyInjection).Assembly))
                .AddInMemoryAuditProvider();
            services.AddApplication();

        }, configureEndPoints: app =>
        {
            app.MapPost("/create",
                async (CreateOrderRequest request, IPipelineFactory pipelineFactory,
                    CancellationToken cancellationToken) =>
                {
                    var pipeline = pipelineFactory.GetPipeline<CreateOrderRequest, CreateOrderContext>();

                    var context = await pipeline.RunAsync(request, configureContext: x =>
                    {

                    }, configureOptions: x =>
                    {
                        x.Id = request.UserId;
                        x.RunMode = RunMode.Parallel;
                        x.EnableRetry = true;
                        x.CleanCompletedEvents = true;
                    }, cancellationToken);

                    return Results.Created();
                });

            app.MapGet("/get", async ([AsParameters] GetOrderRequest request, IPipelineFactory pipelineFactory,
                    CancellationToken cancellationToken) =>
            {
                var pipeline = pipelineFactory.GetPipeline<GetOrderRequest, GetOrderContext>();

                var context = await pipeline.RunAsync(request, configureContext: x =>
                {

                }, configureOptions: x =>
                {
                    x.Id = request.UserId;
                    x.RunMode = RunMode.Parallel;
                    x.EnableRetry = true;
                    x.CleanCompletedEvents = true;
                }, cancellationToken);

                return Results.Ok(context);
            });
        });

            using var host = await hostBuilder.StartAsync();

            var httpClient = host.GetTestClient();

            // Act


            var tasks = new List<Task>();
            for (var i = 0; i < 20; i++)
            {
                var cancellationTokenSource = new CancellationTokenSource();
                var task1 = httpClient.PostAsJsonAsync("/create", new CreateOrderRequest(),
                    cancellationToken: cancellationTokenSource.Token);
                tasks.Add(task1);

                var query = QueryString.Create("UserId", Guid.NewGuid().ToString());

                var cancellationTokenSource2 = new CancellationTokenSource();
                var task2 = httpClient.GetAsync($"/get{query.ToUriComponent()}", cancellationTokenSource2.Token);
                tasks.Add(task2);
            }

            await Task.WhenAll(tasks.ToArray());

            //Assert

        }

        public void Dispose()
        {
        }
    }
}