using St.HolyChain.Core.Abstractions;
using St.HolyChain.Core.Builder;
using St.HolyChain.Core.Extensions;
using St.HolyChain.Core.Models;
using St.HolyChain.Core.Settings;

namespace St.HolyChain.Core;

internal class Pipeline<TRequest, TContext> : IPipeline<TRequest, TContext> where TContext : new()
{
    private readonly IHandlerRegistry<TRequest, TContext> _handlerRegistry;
    private readonly IPipelineBuilder<TRequest, TContext> _pipelineBuilder;

    public Pipeline(IHandlerRegistry<TRequest, TContext> handlerRegistry,
        IPipelineBuilder<TRequest, TContext> pipelineBuilder)
    {
        _handlerRegistry = handlerRegistry;
        _pipelineBuilder = pipelineBuilder;
    }

    public async Task<IPipelineRequestContext<TContext>> RunAsync(
        TRequest request,
        Action<IPipelineRequestContext<TContext>>? configureContext = null,
        Action<PipelineOptions<TRequest, TContext>>? configureOptions = null,
        CancellationToken cancellationToken = default)
    {
        IPipelineRequestBuilder<TRequest, TContext> pipelinePipelineRequestBuilder = new PipelineRequestBuilder<TRequest, TContext>();
        var pipelineRequest = pipelinePipelineRequestBuilder.For(request)
            .AddContext(configureContext)
            .AddOptions(configureOptions)
            .Build();

        var registry = _handlerRegistry;
        if (pipelineRequest.Options.Tags.Length > 0)
        {
            var set = new HashSet<string>(pipelineRequest.Options.Tags.Where(x => !string.IsNullOrWhiteSpace(x)),
                StringComparer.OrdinalIgnoreCase);

            var handlers = _handlerRegistry.Where(x =>
                x.Options.IsEnabled && set.IsSubsetOf(x.Options.Tags));

            registry = new HandlerRegistry<TRequest, TContext>(handlers);
        }

#if DEBUG
        if (pipelineRequest.Options.RunMode == RunMode.Parallel)
            registry.ValidateParallelChain();
        else
            registry.ValidateSequenceChain();
#endif

        _pipelineBuilder.ConfigureRegistry(registry);

        if (pipelineRequest.Options.EnableLog || pipelineRequest.Options.EnableAudit)
        {
            _pipelineBuilder.UseEntryPoint();
        }

        if (pipelineRequest.Options.EnableRetry)
        {
            _pipelineBuilder.UseCompensation();
        }

        if (pipelineRequest.Options.RunMode == RunMode.Parallel)
        {
            _pipelineBuilder.UseHandlersInParallel();
        }
        else
        {
            _pipelineBuilder.UseHandlersInSequence();
        }
        var pipeline = _pipelineBuilder.Build(pipelineRequest);

        var stack = new Dictionary<string, Task>();
        using var internalTokenSource = new CancellationTokenSource(int.MaxValue);
        using var cts = CancellationTokenSource.CreateLinkedTokenSource(internalTokenSource.Token, cancellationToken);

        await pipeline.Invoke(pipelineRequest, cts.Token);

        return pipelineRequest.Context;
    }
}

internal class Pipeline<TRequest>(IHandlerRegistry<TRequest> handlerRegistry,
    IPipelineBuilder<TRequest, object> pipelineBuilder) :
        Pipeline<TRequest, object>(handlerRegistry, pipelineBuilder), IPipeline<TRequest>;