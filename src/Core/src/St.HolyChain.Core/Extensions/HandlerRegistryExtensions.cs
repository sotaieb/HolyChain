using St.HolyChain.Core.Abstractions;

namespace St.HolyChain.Core.Extensions;
public static class HandlerRegistryExtensions
{
    public static void ValidateSequenceChain<TRequest, TContext>(this IHandlerRegistry<TRequest, TContext> registry)
    {
        ArgumentNullException.ThrowIfNull(registry);
        if (!registry.Any())
        {
            throw new Exception("No Handlers found");
        }

        if (registry.GroupBy(y => y.Options.GroupId)
            .Any(x => x.GroupBy(y => y.Options.OrderId)
                .Any(z => z.Count() > 1)))
        {
            throw new Exception("Order is unique by group");
        }
    }

    public static void ValidateParallelChain<TRequest, TContext>(this IHandlerRegistry<TRequest, TContext> registry)
    {
        ArgumentNullException.ThrowIfNull(registry);
        if (!registry.Any())
        {
            throw new Exception("No Handlers found");
        }

        if (registry.GroupBy(y => y.Options.GroupId)
            .OrderBy(x => x.Key)
            .ToDictionary(k => k.Key, v =>
                v.OrderBy(o => o.Options.OrderId))
            .Any(x => x.Value
                .GroupBy(y => y.Options.OrderId)
                .Any(z => z.Count() > 1)))
        {
            throw new Exception("Order is unique by group");
        }
    }
}