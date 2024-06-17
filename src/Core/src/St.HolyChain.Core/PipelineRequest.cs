using St.HolyChain.Core.Abstractions;
using St.HolyChain.Core.Models;
using St.HolyChain.Core.Settings;

namespace St.HolyChain.Core;

public class PipelineRequest<TRequest, TContext>
{
    public TRequest Request { get; set; } = default!;
    public IPipelineRequestContext<TContext> Context { get; set; } = default!;
    public PipelineOptions<TRequest, TContext> Options { get; set; } = default!;
    public AuditResult<TRequest, IPipelineRequestContext<TContext>>? AuditResult { get; set; }
}