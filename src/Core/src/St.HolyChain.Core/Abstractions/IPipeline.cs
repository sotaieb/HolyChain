using St.HolyChain.Core.Settings;

namespace St.HolyChain.Core.Abstractions;

public interface IPipeline<TRequest, TContext>
{
    Task<IPipelineRequestContext<TContext>> RunAsync(
        TRequest request,
        Action<IPipelineRequestContext<TContext>>? configureContext = null,
        Action<PipelineOptions<TRequest, TContext>>? configureOptions = null,
        CancellationToken cancellationToken = default);
}

public interface IPipeline<TRequest> : IPipeline<TRequest, object>
{
}