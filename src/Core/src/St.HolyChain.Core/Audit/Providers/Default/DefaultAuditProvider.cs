using Microsoft.Extensions.DependencyInjection;
using St.HolyChain.Core.Abstractions;

namespace St.HolyChain.Core.Audit.Providers.Default;

public class DefaultAuditProvider : IAuditProvider
{
    private readonly IAuditService _auditService;

    public DefaultAuditProvider([FromKeyedServices("audit.default")] IAuditService auditService)
    {
        _auditService = auditService;
    }

    public IAuditService GetAuditService()
    {
        return _auditService;
    }
}