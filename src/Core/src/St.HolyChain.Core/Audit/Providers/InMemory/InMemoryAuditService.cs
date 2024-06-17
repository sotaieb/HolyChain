using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using St.HolyChain.Core.Abstractions;
using St.HolyChain.Core.Models;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace St.HolyChain.Core.Audit.Providers.InMemory;

public class InMemoryAuditService : IAuditService
{
    private const string AuditPrefixKey = "Audit";
    private readonly IMemoryCache _cache;
    private readonly InMemoryAuditOptions _options;

    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
    };

    public InMemoryAuditService(IMemoryCache cache, IOptions<InMemoryAuditOptions> options)
    {
        _cache = cache;
        _options = options.Value ?? throw new ArgumentNullException(nameof(options));
        _jsonOptions.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.CamelCase));
    }
    public Task WriteLogEventAsync<TRequest, TContext>(AuditEvent<TRequest, IPipelineRequestContext<TContext>> auditEvent)
    {
        if (_cache.TryGetValue<Dictionary<string, List<AuditEvent<TRequest, IPipelineRequestContext<TContext>>>>>(AuditPrefixKey, out var audit) && audit is not null)
        {
            if (audit.ContainsKey(auditEvent.ChainId))
            {
                audit[auditEvent.ChainId].Add(auditEvent);
            }
            else
            {
                audit.Add(auditEvent.ChainId, [auditEvent]);
            }

            _cache.Set(AuditPrefixKey, audit);

        }
        else
        {
            _cache.Set(AuditPrefixKey, new Dictionary<string, List<AuditEvent<TRequest, IPipelineRequestContext<TContext>>>>
                {
                    { auditEvent.ChainId, [auditEvent] }
                });
        }

        return Task.CompletedTask;
    }

    public Task<AuditResult<TRequest, IPipelineRequestContext<TContext>>> GetAuditEventsAsync<TRequest, TContext>(string chainId)
    {
        if (_cache.TryGetValue<Dictionary<string, List<AuditEvent<TRequest, IPipelineRequestContext<TContext>>>>>(AuditPrefixKey, out var audit) && audit is not null)
        {
            if (audit.TryGetValue(chainId, out var value))
            {
                return Task.FromResult(new AuditResult<TRequest, IPipelineRequestContext<TContext>>(value));
            }
        }

        return Task.FromResult(new AuditResult<TRequest, IPipelineRequestContext<TContext>>([]));
    }

    public Task CleanLogEventsAsync<TRequest, TContext>(string chainId)
    {

        if (_cache.TryGetValue<Dictionary<string, List<AuditEvent<TRequest, TContext>>>>(AuditPrefixKey, out var audit) &&
            audit is not null)
        {
            if (audit.Remove(chainId, out var value))
            {
                _cache.Set(AuditPrefixKey, audit);
            }
        }

        return Task.CompletedTask;
    }
}
