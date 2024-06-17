using St.HolyChain.Core.Settings;

namespace St.HolyChain.Core.Abstractions;

public interface IHandler
{
    HandlerOptions Options { get; set; }
}

public interface IHandler<in TRequest> : IHandler<TRequest, object>
{
}


public interface IHandler<in TRequest, in TContext> : IHandler
{
    Task HandleAsync(TRequest request, IPipelineRequestContext<TContext> pipelineRequestContext,
        CancellationToken cancellationToken = default);
   
    Func<IPipelineRequestContext<TContext>, bool>? IsSatisfied { get; }
    Func<TRequest, IPipelineRequestContext<TContext>, CancellationToken, Task>? OnBeforeHandleAsync { get; }
    Func<TRequest, IPipelineRequestContext<TContext>, CancellationToken, Task>? OnAfterHandleAsync { get; }
    Func<TRequest, IPipelineRequestContext<TContext>, CancellationToken, Task<bool>>? OnCompensateAsync { get; }
}