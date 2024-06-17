using St.HolyChain.Core.Models;

namespace St.HolyChain.Core.Abstractions;

public interface IAuditService
{
    Task WriteLogEventAsync<TRequest, TContext>(AuditEvent<TRequest, IPipelineRequestContext<TContext>> auditEvent);
    Task<AuditResult<TRequest, IPipelineRequestContext<TContext>>> GetAuditEventsAsync<TRequest, TContext>(string chainId);
    Task CleanLogEventsAsync<TRequest, TContext>(string chainId);
}