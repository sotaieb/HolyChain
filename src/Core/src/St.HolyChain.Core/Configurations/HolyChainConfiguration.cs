using Microsoft.Extensions.DependencyInjection;
using St.HolyChain.Core.Abstractions;
using St.HolyChain.Core.Settings;
using System.Reflection;

namespace St.HolyChain.Core.Configurations;

public class HolyChainConfiguration
{
    internal List<Assembly> AssembliesToRegister { get; } = [];

    public Func<Type, bool> TypeEvaluator { get; set; } = t => true;

    public HolyChainConfiguration RegisterServicesFromAssembly(Assembly assembly)
    {
        AssembliesToRegister.Add(assembly);

        return this;
    }
}

public class HolyChainHandlerConfigurator<TRequest, TContext> where TContext : IContext, new()
{
    private readonly IServiceCollection _services;
    private short _orderId = 0;

    private HashSet<Type> Handlers { get; } = new();

    public HolyChainHandlerConfigurator(IServiceCollection services)
    {
        _services = services;
    }

    public HolyChainHandlerConfigurator<TRequest, TContext> UseHandler<T>(Action<HandlerOptions>? configure = null)
        where T : IHandler
    {
        if (!Handlers.Add(typeof(T)))
        {
            throw new Exception("Duplicated handler");
        }

        _services.AddTransient(typeof(IHandler<TRequest, TContext>), typeof(T));

        _services.Configure<HandlerOptions>(typeof(T).Name, x =>
        {
            x.Key = typeof(T).Name;
            x.OrderId = _orderId++;
            configure?.Invoke(x);
        });


        return this;
    }
}

public class HolyChainHandlerConfigurator<TRequest>
{
    private readonly IServiceCollection _services;
    private short _orderId = 0;

    private HashSet<Type> Handlers { get; } = new();

    public HolyChainHandlerConfigurator(IServiceCollection services)
    {
        _services = services;
    }

    public HolyChainHandlerConfigurator<TRequest> UseHandler<T>(Action<HandlerOptions>? configure = null)
        where T : IHandler
    {
        if (!Handlers.Add(typeof(T)))
        {
            throw new Exception("Duplicated handler");
        }

        _services.AddTransient(typeof(IHandler<TRequest>), typeof(T));

        _services.Configure<HandlerOptions>(typeof(T).Name, x =>
        {
            x.Key = typeof(T).Name;
            x.OrderId = _orderId++;
            configure?.Invoke(x);
        });

        return this;
    }
}

public class HolyChainHandlerOptionConfigurator
{
    private readonly IServiceCollection _services;
    private short _orderId = 0;

    private HashSet<Type> Handlers { get; } = new();

    public HolyChainHandlerOptionConfigurator(IServiceCollection services)
    {
        _services = services;
    }

    public HolyChainHandlerOptionConfigurator UseHandler<T>(Action<HandlerOptions>? configure = null)
        where T : IHandler
    {
        if (!Handlers.Add(typeof(T)))
        {
            throw new Exception("Duplicated handler");
        }

        _services.Configure<HandlerOptions>(typeof(T).Name, x =>
        {
            x.Key = typeof(T).Name;
            x.OrderId = _orderId++;
            configure?.Invoke(x);
        });

        return this;
    }
}

