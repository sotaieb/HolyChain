using St.HolyChain.Core.Abstractions;
using St.HolyChain.Core.Models;

namespace St.HolyChain.Core.Audit.Providers.Default;

internal class DefaultAuditService : IAuditService
{

    public Task WriteLogEventAsync<TRequest, TContext>(AuditEvent<TRequest, IPipelineRequestContext<TContext>> auditEvent) 
        => Task.CompletedTask;

    Task<AuditResult<TRequest, IPipelineRequestContext<TContext>>> IAuditService.GetAuditEventsAsync<TRequest, TContext>(string chainId) 
        => Task.FromResult(new AuditResult<TRequest, IPipelineRequestContext<TContext>>([]));


    public Task CleanLogEventsAsync<TRequest, TContext>(string chainId) => Task.CompletedTask;
}