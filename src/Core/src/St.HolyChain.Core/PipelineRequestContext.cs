using St.HolyChain.Core.Abstractions;
using St.HolyChain.Core.Models;

namespace St.HolyChain.Core;

public class PipelineRequestContext : IPipelineRequestContext
{
    public IDataCollection DataCollection { get; internal set; } = new DataCollection();
}

public class PipelineRequestContext<TContext> : PipelineRequestContext, IPipelineRequestContext<TContext> where TContext : new()
{
    public TContext Data { get; set; } = new();
}
