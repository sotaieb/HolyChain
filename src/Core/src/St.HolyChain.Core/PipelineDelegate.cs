namespace St.HolyChain.Core;

public delegate Task PipelineDelegate<TRequest, TContext>(PipelineRequest<TRequest, TContext> request, CancellationToken cancellationToken);
