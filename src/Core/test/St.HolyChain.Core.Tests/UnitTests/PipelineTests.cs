using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using St.HolyChain.Core.Abstractions;
using St.HolyChain.Core.Builder;
using St.HolyChain.Core.Extensions;
using St.HolyChain.TestTools;
using Xunit.Abstractions;

namespace St.HolyChain.Core.Tests.UnitTests;

public class PipelineTests
{
    private readonly ITestOutputHelper _output;

    public PipelineTests(ITestOutputHelper output)
    {
        _output = output;
    }

    [Fact]
    public async Task RunAsync_WhenCustomRegistryAndPipeline_ExecutesPipeline()
    {
        // Arrange

        var factory = new LoggerFactory();    // add console provider
        factory.AddProvider(new XunitLoggerProvider(_output));   // add file provider
        var logger = factory.CreateLogger<IPipeline<MyRequest, MyContext>>();

        var registry = HandlerRegistryBuilder.Create<MyRequest, MyContext>()
            .AddHandler(new CreateOrder())
            .AddHandler(new RegisterOrder())
            .Build();

        var pipelineBuilder = PipelineBuilder.Create<MyRequest, MyContext>()
            .ConfigureLogger(logger);

        var pipeline = new Pipeline<MyRequest, MyContext>(registry, pipelineBuilder);

        // Act
        var context = await pipeline.RunAsync(new MyRequest(), configureOptions: x => x.EnableLog = true);

        // Assert
        context.Data.Value1.Should().Be(2);
    }

    [Fact]
    public async Task RunAsync_WhenProviderRegistryAndPipeline_ExecutesPipeline()
    {
        // Arrange
        var factory = new LoggerFactory();    // add console provider
        factory.AddProvider(new XunitLoggerProvider(_output));   // add file provider
        var logger = factory.CreateLogger<IPipeline<MyRequest, MyContext>>();

        var services = new ServiceCollection();
        services.AddActivityHandler<CreateOrder>();
        services.AddActivityHandler<RegisterOrder>();
        var provider = services.BuildServiceProvider();

        var registry = HandlerRegistryBuilder.Create<MyRequest, MyContext>(provider)
            .AddHandler<CreateOrder>()
            .AddHandler<RegisterOrder>()
            .Build();

        var pipelineBuilder = PipelineBuilder.Create<MyRequest, MyContext>()
            .ConfigureLogger(logger);

        var pipeline = new Pipeline<MyRequest, MyContext>(registry, pipelineBuilder);

        // Act
        var context = await pipeline.RunAsync(new MyRequest(), configureOptions: x => x.EnableLog = true);

        // Assert
        context.Data.Value1.Should().Be(2);
    }

    public record MyRequest();

    public record MyContext
    {
        public int Value1 { get; set; }
    }

    public class CreateOrder : Activity<MyRequest, MyContext>
    {
        public override Task HandleAsync(MyRequest request, IPipelineRequestContext<MyContext> pipelineRequestContext,
            CancellationToken cancellationToken = default)
        {
            pipelineRequestContext.Data.Value1 = 1;

            return Task.CompletedTask;
        }
    }

    public class RegisterOrder : Activity<MyRequest, MyContext>
    {
        public override Task HandleAsync(MyRequest request, IPipelineRequestContext<MyContext> pipelineRequestContext,
            CancellationToken cancellationToken = default)
        {
            pipelineRequestContext.Data.Value1 = 2;

            return Task.CompletedTask;
        }
    }
}