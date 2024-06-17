using Microsoft.Extensions.Logging;

namespace St.HolyChain.Core.Abstractions;

public interface IPipelineBuilder<TRequest, TContext>
{
    IPipelineBuilder<TRequest, TContext> ConfigureRegistry(IHandlerRegistry<TRequest, TContext> registry);
    IPipelineBuilder<TRequest, TContext> ConfigureLogger(ILogger<IPipeline<TRequest, TContext>> logger);
    IPipelineBuilder<TRequest, TContext> ConfigureAudit(IAuditService auditService);
    IPipelineBuilder<TRequest, TContext> UseEntryPoint();
    IPipelineBuilder<TRequest, TContext> UseCompensation();
    IPipelineBuilder<TRequest, TContext> UseHandlersInSequence();
    IPipelineBuilder<TRequest, TContext> UseHandlersInParallel();
    IPipelineBuilder<TRequest, TContext> UseHandlerGroupInParallel(IEnumerable<IHandler<TRequest, TContext>> handlers);
    IPipelineBuilder<TRequest, TContext> Use(Func<PipelineDelegate<TRequest, TContext>, PipelineDelegate<TRequest, TContext>> application);
    PipelineDelegate<TRequest, TContext> Build(PipelineRequest<TRequest, TContext> request);
}
