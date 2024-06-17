using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using St.HolyChain.Core.Abstractions;

namespace St.HolyChain.Core.Audit.Providers.InMemory;

public class InMemoryAuditProvider : IAuditProvider
{
    private readonly IAuditService _auditService;
    public InMemoryAuditProvider([FromKeyedServices("audit.memory")] IAuditService auditService)
    {
        _auditService = auditService;
    }
    public IAuditService GetAuditService()
    {
        return _auditService;
    }
}