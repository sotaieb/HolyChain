namespace St.HolyChain.Core.Abstractions;

public interface IPipelineRequestContext
{
    IDataCollection DataCollection { get; }
}

public interface IPipelineRequestContext<out TContext> : IPipelineRequestContext
{
    TContext Data { get; }
}