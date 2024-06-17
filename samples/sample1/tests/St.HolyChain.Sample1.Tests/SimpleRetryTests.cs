using FluentAssertions;
using HolyChain.Sample1;
using HolyChain.Sample1.UseCases.CreateOrder.Abstractions;
using HolyChain.Sample1.UseCases.CreateOrder.Activities.CreateOrder;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using St.HolyChain.Core.Extensions;
using St.HolyChain.TestTools;
using System.Net;
using System.Net.Http.Json;
using Xunit.Abstractions;

namespace St.HolyChain.Sample1.Tests;

public class SimpleRetryTests : IDisposable
{
    private readonly ITestOutputHelper _output;

    public SimpleRetryTests(ITestOutputHelper output)
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
            
            services.AddHolyChain(x => x.RegisterServicesFromAssembly(typeof(DependencyInjection).Assembly))
                .AddInMemoryAuditProvider();
            services.AddApplication();

        }, configureEndPoints: app =>
        {
            app.MapPost("/create",
                async (CreateOrderRequest request, ICreateOrderService service,
                    CancellationToken cancellationToken) =>
                {
                    var _ = await service.RunAsync(request, cancellationToken);

                    return Results.Created();
                });
        });

        using (var host = await hostBuilder.StartAsync())
        {
            var httpClient = host.GetTestClient();

            var cancellationTokenSource = new CancellationTokenSource();
            cancellationTokenSource.CancelAfter(500);
            // Act

            var actual1 = () => httpClient.PostAsJsonAsync("/create", new CreateOrderRequest
            {
                ThrowException = true
            }, cancellationTokenSource.Token);

            await actual1.Should().ThrowAsync<Exception>();

            var cancellationTokenSource2 = new CancellationTokenSource();
            var actual2 = await httpClient.PostAsJsonAsync("/create", new CreateOrderRequest
            {
                ThrowException = false
            }, cancellationTokenSource2.Token);
            //Assert 
            actual2.StatusCode.Should().Be(HttpStatusCode.Created);
        }
    }

    public void Dispose()
    {
    }
}