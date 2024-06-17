using St.HolyChain.Core.Abstractions;
using St.HolyChain.Core.Models;

namespace St.HolyChain.Core;

internal class AuditService : IAuditService
{
    private readonly IAuditService _auditService;
    public AuditService(IAuditProvider auditProvider)
    {
        ArgumentNullException.ThrowIfNull(auditProvider);

        _auditService = auditProvider.GetAuditService();
    }

    public Task WriteLogEventAsync<TRequest, TContext>(AuditEvent<TRequest, IPipelineRequestContext<TContext>> auditEvent)
        => _auditService.WriteLogEventAsync(auditEvent);

    Task<AuditResult<TRequest, IPipelineRequestContext<TContext>>> IAuditService.GetAuditEventsAsync<TRequest, TContext>(string chainId)
        => _auditService.GetAuditEventsAsync<TRequest, TContext>(chainId);

    public Task CleanLogEventsAsync<TRequest, TContext>(string chainId)
         => _auditService.CleanLogEventsAsync<TRequest, TContext>(chainId);
}