using St.HolyChain.Core.Abstractions;

namespace St.HolyChain.Core.Extensions;

public static class PipelineBuilderExtensions
{
    public static IPipelineBuilder<TRequest, TContext> UseDelegate<TRequest, TContext>(this IPipelineBuilder<TRequest, TContext> app,
        Func<PipelineRequest<TRequest, TContext>, CancellationToken, Func<Task>, Task> component)
    {
        return app.Use(next =>
        {
            return (context, cancellationToken) =>
            {
                Func<Task> funcNext = () => next(context, cancellationToken);
                return component(context, cancellationToken, funcNext);
            };
        });
    }
}
