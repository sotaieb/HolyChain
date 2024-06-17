namespace St.HolyChain.Core.Abstractions;

public interface IPipelineFactory
{
    IPipeline<TRequest, TContext> GetPipeline<TRequest, TContext>();
    IPipeline<TRequest> GetPipeline<TRequest>();
}
