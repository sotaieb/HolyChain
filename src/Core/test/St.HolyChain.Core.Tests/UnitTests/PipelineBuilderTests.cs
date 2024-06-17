using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using St.HolyChain.Core.Abstractions;
using St.HolyChain.Core.Builder;
using St.HolyChain.Core.Models;
using St.HolyChain.Core.Settings;
using Xunit.Abstractions;

namespace St.HolyChain.Core.Tests.UnitTests;

public class PipelineBuilderTests
{
    private readonly ITestOutputHelper _output;

    public PipelineBuilderTests(ITestOutputHelper output)
    {
        _output = output;
    }

    [Fact]
    public async Task Build_WhenRunInSequence_ExecutesPipeline()
    {
        // Arrange
        var auditProvider = new Mock<IAuditProvider>();
        var auditService = new Mock<IAuditService>();

        auditProvider.Setup(x => x.GetAuditService())
            .Returns(auditService.Object)
            .Verifiable();

        auditService.Setup(x => x.WriteLogEventAsync(It.IsAny<AuditEvent<MyRequest, IPipelineRequestContext<MyContext>>>()))
            .Returns(Task.CompletedTask)
            .Verifiable();

        var logger = new Mock<ILogger<IPipeline<MyRequest, MyContext>>>();

        logger.Setup(x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()))
            .Verifiable();

        var options = new Mock<IOptionsMonitor<HandlerOptions>>();
        options.Setup(x =>
            x.Get(nameof(MyActivity1))).Returns(new HandlerOptions
            {
                OrderId = 1,
                Key = nameof(MyActivity1),
                Tags = ["Fr"]
            });
        options.Setup(x =>
            x.Get(nameof(MyActivity2))).Returns(new HandlerOptions
            {
                OrderId = 2,
                Key = nameof(MyActivity2),
                Tags = ["Pl"]
            });

        var handlers = new List<Activity<MyRequest, MyContext>> { new MyActivity1(), new MyActivity2() };
        var registry = new HandlerRegistry<MyRequest, MyContext>(handlers, options.Object);

        var sut = new PipelineBuilder<MyRequest, MyContext>(auditProvider.Object, logger.Object)
            .ConfigureRegistry(registry);
        var request = PipelineRequestBuilder.Create<MyRequest, MyContext>()
            .For(new MyRequest())
            .Build();

        // Act
        var app = sut
            .UseHandlersInSequence()
            .Build(request);
        await app.Invoke(request, CancellationToken.None);

        // Assert
        request.Context.Data.Value1.Should().Be(2);
    }

    public record MyRequest();

    public record MyContext
    {
        public int Value1 { get; set; }
    }

    public class MyActivity1 : Activity<MyRequest, MyContext>
    {
        public override Task HandleAsync(MyRequest request, IPipelineRequestContext<MyContext> pipelineRequestContext,
            CancellationToken cancellationToken = default)
        {
            pipelineRequestContext.Data.Value1 += 1;

            return Task.CompletedTask;
        }
    }
    public class MyActivity2 : Activity<MyRequest, MyContext>
    {
        public override Task HandleAsync(MyRequest request, IPipelineRequestContext<MyContext> pipelineRequestContext,
            CancellationToken cancellationToken = default)
        {
            pipelineRequestContext.Data.Value1 += 1;

            return Task.CompletedTask;
        }
    }
}

