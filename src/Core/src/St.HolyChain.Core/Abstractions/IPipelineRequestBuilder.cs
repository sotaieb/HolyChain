using St.HolyChain.Core.Settings;

namespace St.HolyChain.Core.Abstractions;

public interface IPipelineRequestBuilder<TRequest, TContext> where TContext : new()
{
    IPipelineRequestBuilder<TRequest, TContext> For(TRequest request);
    IPipelineRequestBuilder<TRequest, TContext> AddContext(Action<IPipelineRequestContext<TContext>>? configureRequest);
    IPipelineRequestBuilder<TRequest, TContext> AddOptions(Action<PipelineOptions<TRequest, TContext>>? configureOptions);
    PipelineRequest<TRequest, TContext> Build();
}
