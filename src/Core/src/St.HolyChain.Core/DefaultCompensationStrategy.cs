using St.HolyChain.Core.Abstractions;
using St.HolyChain.Core.Models;

namespace St.HolyChain.Core;

internal class DefaultCompensationStrategy<TRequest, TContext> : ICompensationStrategy<TRequest, TContext>
{
    public Func<IHandler<TRequest, TContext>, AuditResult<TRequest, IPipelineRequestContext<TContext>>, bool> Execute2()
    {
        return Query;

        bool Query(IHandler<TRequest, TContext> handler, AuditResult<TRequest, IPipelineRequestContext<TContext>> auditResult) =>
            // handler.Options.Type != ActivityType.Write ||  // if it's write do not re-execute and look at next condition
            handler.Options.GroupId >= auditResult.LastFailedEvent!.GroupId // get last failed writes and all next 
            && !auditResult.CompletedEvents.Any(h =>
                handler.Options.OrderId == h.OrderId &&
                handler.Options.GroupId == auditResult.LastFailedEvent!.GroupId
            );
    }

    public bool CanRetry(AuditResult<TRequest, IPipelineRequestContext<TContext>> auditResult)
    {
        var expression = auditResult.Events.Any() &&
           auditResult.StartedEvent is not null && auditResult.LastCompletedEvent is not null &&
           auditResult.LastFailedEvent is not null;

        return expression;
    }
}

internal class DefaultCompensationStrategy<TRequest> : DefaultCompensationStrategy<TRequest, object>
{
}