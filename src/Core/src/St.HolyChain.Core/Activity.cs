using St.HolyChain.Core.Abstractions;
using St.HolyChain.Core.Settings;

namespace St.HolyChain.Core;

public abstract class Activity<TRequest, TContext> : IHandler<TRequest, TContext>
{
    public abstract Task HandleAsync(TRequest request, IPipelineRequestContext<TContext> pipelineRequestContext,
        CancellationToken cancellationToken = default);

    public virtual HandlerOptions Options { get; set; } = default!;
    public virtual Func<IPipelineRequestContext<TContext>, bool>? IsSatisfied { get; set; }
    public virtual Func<TRequest, IPipelineRequestContext<TContext>, CancellationToken, Task>? OnBeforeHandleAsync { get; set; }
    public virtual Func<TRequest, IPipelineRequestContext<TContext>, CancellationToken, Task>? OnAfterHandleAsync { get; set; }
    public virtual Func<TRequest, IPipelineRequestContext<TContext>, CancellationToken, Task<bool>>? OnCompensateAsync
    {
        get;
        set;
    }
}

public abstract class Activity<TRequest> : Activity<TRequest, object>, IHandler<TRequest>
{
    public abstract Task HandleAsync(TRequest request, IPipelineRequestContext pipelineRequestContext,
        CancellationToken cancellationToken = default);

    public override Task HandleAsync(TRequest request, IPipelineRequestContext<object> pipelineRequestContext,
        CancellationToken cancellationToken = default) =>
        HandleAsync(request, pipelineRequestContext, cancellationToken);
}