using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using St.HolyChain.Core.Abstractions;
using St.HolyChain.Core.Audit.Providers.Default;
using St.HolyChain.Core.Audit.Providers.InMemory;
using St.HolyChain.Core.Configurations;
using St.HolyChain.Core.Helpers;
using St.HolyChain.Core.Settings;

namespace St.HolyChain.Core.Extensions;

public static class ServiceCollectionExtensions
{
    private const string InMemoryAuditKey = "audit.memory";
    private const string SystemFileAuditkey = "audit.file";
    private const string DefaultAuditKey = "audit.default";

    public static IServiceCollection AddHolyChain(this IServiceCollection services, Action<HolyChainConfiguration> configuration)
    {
        var serviceConfig = new HolyChainConfiguration();

        configuration.Invoke(serviceConfig);

        return services.AddHolyChain(serviceConfig);
    }

    public static IServiceCollection AddHolyChain(this IServiceCollection services, HolyChainConfiguration configuration)
    {
        if (!configuration.AssembliesToRegister.Any())
        {
            throw new ArgumentException("No assemblies found to scan.");
        }

        ServiceRegistrarHelper.AddHandlers(services, configuration);

        services.AddHolyChainServices();

        return services;
    }

    public static IServiceCollection AddHolyChain<TRequest, TContext>(this IServiceCollection services,
        Action<HolyChainHandlerConfigurator<TRequest, TContext>> configure) where TContext : IContext, new()
    {
        var configurator = new HolyChainHandlerConfigurator<TRequest, TContext>(services);
        configure.Invoke(configurator);

        services.AddHolyChainServices();

        return services;
    }

    public static IServiceCollection AddHolyChain<TRequest>(this IServiceCollection services,
        Action<HolyChainHandlerConfigurator<TRequest>> configure)
    {
        var configurator = new HolyChainHandlerConfigurator<TRequest>(services);
        configure.Invoke(configurator);

        services.AddHolyChainServices();

        return services;
    }

    public static IServiceCollection AddHolyChainServices(this IServiceCollection services)
    {
        services.AddMemoryCache();

        services.TryAdd(new ServiceDescriptor(typeof(IPipelineFactory), typeof(PipelineFactory),
            ServiceLifetime.Scoped));

        services.TryAdd(new ServiceDescriptor(typeof(IPipelineBuilder<,>), typeof(PipelineBuilder<,>),
            ServiceLifetime.Scoped));
        services.TryAdd(new ServiceDescriptor(typeof(IPipeline<,>), typeof(Pipeline<,>),
            ServiceLifetime.Scoped));
        services.TryAdd(new ServiceDescriptor(typeof(IPipeline<>), typeof(Pipeline<>),
            ServiceLifetime.Scoped));

        services.TryAdd(new ServiceDescriptor(typeof(IHandlerRegistry<,>), typeof(HandlerRegistry<,>),
            ServiceLifetime.Scoped));
        services.TryAdd(new ServiceDescriptor(typeof(IHandlerRegistry<>), typeof(HandlerRegistry<>),
            ServiceLifetime.Scoped));
        services.TryAdd(new ServiceDescriptor(typeof(ICompensationStrategy<,>), typeof(DefaultCompensationStrategy<,>),
            ServiceLifetime.Scoped));


        services.TryAddSingleton(sp =>
        {
            var options = sp.GetRequiredService<IOptions<AuditOptions>>();

            return sp.GetRequiredKeyedService<IAuditProvider>(options.Value.ProviderKey);
        });

        services.AddDefaultAuditProvider();
        return services;
    }

    public static IServiceCollection ConfigureHolyChain(this IServiceCollection services, Action<HolyChainHandlerOptionConfigurator> configure)
    {
        var configurator = new HolyChainHandlerOptionConfigurator(services);
        configure.Invoke(configurator);

        return services;
    }

    public static IServiceCollection AddActivityHandler<T>(this IServiceCollection services, Action<HandlerOptions>? configure = null) where T : IHandler
    {
        var interfaceType = typeof(T).FindInterfacesThatClose(typeof(IHandler<,>)).First();

        services.AddTransient(interfaceType, typeof(T));
        if (configure is not null)
        {
            services.Configure<HandlerOptions>(typeof(T).Name, x =>
            {
                x.Key = typeof(T).Name;
                configure.Invoke(x);
            });
        }

        return services;
    }

    public static IServiceCollection ConfigureActivityHandler<T>(this IServiceCollection services, Action<HandlerOptions> configure) where T : IHandler
    {
        services.Configure<HandlerOptions>(typeof(T).Name, configure.Invoke);

        return services;
    }

    public static IServiceCollection AddDefaultAuditProvider(this IServiceCollection services)
    {
        services.TryAddKeyedSingleton<IAuditService, DefaultAuditService>(DefaultAuditKey);
        services.TryAddKeyedSingleton<IAuditProvider, DefaultAuditProvider>(DefaultAuditKey);

        services.Configure<AuditOptions>(x => x.ProviderKey = DefaultAuditKey);

        return services;
    }

    public static IServiceCollection AddInMemoryAuditProvider(this IServiceCollection services)
    {

        services.TryAddKeyedSingleton<IAuditService, InMemoryAuditService>(InMemoryAuditKey);
        services.TryAddKeyedSingleton<IAuditProvider, InMemoryAuditProvider>(InMemoryAuditKey);

        services.PostConfigure<AuditOptions>(x => x.ProviderKey = InMemoryAuditKey);

        return services;
    }
}