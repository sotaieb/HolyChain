using Microsoft.Extensions.DependencyInjection;
using St.HolyChain.Core.Abstractions;
using St.HolyChain.Core.Settings;

namespace St.HolyChain.Core.Builder;

public class HandlerRegistryBuilder
{
    public static HandlerRegistryBuilder<TRequest, TContext> Create<TRequest, TContext>() => new();

    public static HandlerRegistryBuilder<TRequest, TContext> Create<TRequest, TContext>(IServiceProvider provider)
    {
        ArgumentNullException.ThrowIfNull(provider);
        return new HandlerRegistryBuilder<TRequest, TContext>(provider);
    }
}

public class HandlerRegistryBuilder<TRequest, TContext>
{
    private readonly IServiceProvider _serviceProvider;
    private readonly List<IHandler<TRequest, TContext>> _handlers = new();
    private readonly Dictionary<Type, IHandler<TRequest, TContext>> _registeredHandlers = new();
    private short _index = 0;

    internal HandlerRegistryBuilder()
    {
    }

    public HandlerRegistryBuilder(IServiceProvider serviceProvider)
    {
        ArgumentNullException.ThrowIfNull(serviceProvider);

        _serviceProvider = serviceProvider;
        _registeredHandlers = _serviceProvider.GetRequiredService<IEnumerable<IHandler<TRequest, TContext>>>()
            .ToDictionary(x => x.GetType(), y => y);

    }

    public HandlerRegistryBuilder<TRequest, TContext> AddHandler(IHandler<TRequest, TContext> handler,
        Action<HandlerOptions>? configureOptions = null)
    {
        ArgumentNullException.ThrowIfNull(handler);

        var options = new HandlerOptions
        {
            Key = typeof(IHandler<TRequest, TContext>).Name,
            OrderId = _index++
        };
        configureOptions?.Invoke(options);
        handler.Options = options;
        _handlers.Add(handler);

        return this;
    }

    public HandlerRegistryBuilder<TRequest, TContext> AddHandler<THandler>(Action<HandlerOptions>? configureOptions = null) where THandler : IHandler<TRequest, TContext>
    {
        if (!_registeredHandlers.TryGetValue(typeof(THandler), out var handler))
        {
            throw new Exception("Service not found");
        }

        var options = new HandlerOptions
        {
            Key = typeof(THandler).Name,
            OrderId = _index++
        };
        configureOptions?.Invoke(options);
        handler.Options = options;
        _handlers.Add(handler);

        return this;
    }

    public IHandlerRegistry<TRequest, TContext> Build() => new HandlerRegistry<TRequest, TContext>(_handlers);
}
