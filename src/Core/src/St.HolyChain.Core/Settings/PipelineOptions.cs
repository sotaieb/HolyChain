using St.HolyChain.Core.Abstractions;
using St.HolyChain.Core.Models;

namespace St.HolyChain.Core.Settings;

public class PipelineOptions<TRequest, TContext> : PipelineOptions
{   
    public ICompensationStrategy<TRequest, TContext>? CompensationStrategy { get; set; }
    public Func<IPipelineRequestContext<TContext>, IPipelineRequestContext<TContext>>? MapHandlerStartedEvent { get; set; }
    public Func<IPipelineRequestContext<TContext>, IPipelineRequestContext<TContext>>? MapHandlerCompletedEvent { get; set; }
    public Func<IPipelineRequestContext<TContext>, IPipelineRequestContext<TContext>>? MapCompletedEvent { get; set; }
}

public class PipelineOptions
{
    public string Id { get; set; } = default!;
    public string[] Tags { get; set; } = default!;
    public RunMode RunMode { get; set; }
    public bool EnableRetry { get; set; }
    public bool EnableAudit { get; set; }
    public bool EnableLog { get; set; }
    public bool EnableCompensation { get; set; }
    public bool IsRetry { get; set; }
    public bool CleanCompletedEvents { get; set; }
}