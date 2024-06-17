using St.HolyChain.Core.Models;

namespace St.HolyChain.Core.Abstractions;

public interface ICompensationStrategy<TRequest, TContext>
{
    internal Func<IHandler<TRequest, TContext>, AuditResult<TRequest, IPipelineRequestContext<TContext>>, bool> Execute2();
    internal bool CanRetry(AuditResult<TRequest, IPipelineRequestContext<TContext>> auditResult);
}