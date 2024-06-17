using Microsoft.Extensions.DependencyInjection;
using St.HolyChain.Core.Abstractions;

namespace St.HolyChain.Core;

public class PipelineFactory : IPipelineFactory
{
    private readonly IServiceProvider _serviceProvider;

    public PipelineFactory(IServiceProvider serviceProvider)
        => _serviceProvider = serviceProvider;

    public IPipeline<TRequest, TContext> GetPipeline<TRequest, TContext>()
        => _serviceProvider.GetRequiredService<IPipeline<TRequest, TContext>>();
    public IPipeline<TRequest> GetPipeline<TRequest>()
        => _serviceProvider.GetRequiredService<IPipeline<TRequest>>();
}

