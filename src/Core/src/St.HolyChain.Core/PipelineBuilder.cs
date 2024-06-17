using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using St.HolyChain.Core.Abstractions;
using St.HolyChain.Core.Audit.Providers.Default;
using St.HolyChain.Core.Exceptions;
using St.HolyChain.Core.Extensions;
using St.HolyChain.Core.Helpers;
using St.HolyChain.Core.Models;

namespace St.HolyChain.Core;

public static class PipelineBuilder
{
    public static IPipelineBuilder<TRequest, TContext> Create<TRequest, TContext>() where TContext : new()
        => new PipelineBuilder<TRequest, TContext>();
}

public class PipelineBuilder<TRequest, TContext> : IPipelineBuilder<TRequest, TContext> where TContext : new()
{
    private ILogger<IPipeline<TRequest, TContext>> _logger;
    private IAuditService _auditService;
    private IHandlerRegistry<TRequest, TContext> _handlerRegistry;

    private readonly List<Func<PipelineDelegate<TRequest, TContext>, PipelineDelegate<TRequest, TContext>>> _components = new();

    internal PipelineBuilder()
    {
        _auditService = new DefaultAuditService();
        _logger = NullLogger<IPipeline<TRequest, TContext>>.Instance;
    }
    public PipelineBuilder(IAuditProvider auditProvider,
        ILogger<IPipeline<TRequest, TContext>> logger)
    {
        _auditService = auditProvider.GetAuditService();
        _logger = logger;
    }

    public IPipelineBuilder<TRequest, TContext> ConfigureRegistry(IHandlerRegistry<TRequest, TContext> registry)
    {
        ArgumentNullException.ThrowIfNull(registry);
        _handlerRegistry = registry;

        return this;
    }
    public IPipelineBuilder<TRequest, TContext> ConfigureLogger(ILogger<IPipeline<TRequest, TContext>> logger)
    {
        ArgumentNullException.ThrowIfNull(logger);
        _logger = logger;

        return this;
    }
    public IPipelineBuilder<TRequest, TContext> ConfigureAudit(IAuditService auditService)
    {
        ArgumentNullException.ThrowIfNull(auditService);
        _auditService = auditService;

        return this;
    }

    public IPipelineBuilder<TRequest, TContext> UseEntryPoint()
    {
        this.UseDelegate(async (context, cancellationToken, next) =>
        {
            try
            {
                if (context.Options.EnableLog)
                {
                    _logger.LogInformation("Process {processName} started", typeof(TContext).Name);
                }

                if (context.Options.EnableAudit)
                {
                    await _auditService.WriteLogEventAsync(new AuditEvent<TRequest, IPipelineRequestContext<TContext>>
                    {
                        ChainId = context.Options.Id,
                        Request = context.Request,
                        Status = ActivityStatus.Started
                    });
                }

                await next();

                if (context.Options.EnableAudit)
                {
                    if (context.Options.CleanCompletedEvents)
                    {
                        await _auditService.CleanLogEventsAsync<TRequest, TContext>(context.Options.Id);
                    }
                    else
                    {
                        await _auditService.WriteLogEventAsync(new AuditEvent<TRequest, IPipelineRequestContext<TContext>>
                        {
                            ChainId = context.Options.Id,
                            Request = context.Request,
                            Context = context.Options.MapCompletedEvent?.Invoke(context.Context) ?? context.Context,
                            Status = ActivityStatus.Completed
                        });
                    }
                }

                if (context.Options.EnableLog)
                {
                    _logger.LogInformation("Process {processName} completed", typeof(TContext).Name);
                }

            }
            catch (Exception ex)
            {
                if (context.Options.EnableLog)
                {
                    await _auditService.WriteLogEventAsync(new AuditEvent<TRequest, IPipelineRequestContext<TContext>>
                    {
                        ChainId = context.Options.Id,
                        ErrorMessage = ex.ToString(),
                        Status = ActivityStatus.Failed
                    });
                }

                if (context.Options.EnableLog)
                {
                    _logger.LogInformation("Process {processName} failed", typeof(TContext).Name);
                }

                throw new ChainException(ex);
            }
        });

        return this;
    }

    public IPipelineBuilder<TRequest, TContext> UseCompensation()
    {
        this.UseDelegate(async (context, cancellationToken, next) =>
        {
            var auditResult = await _auditService.GetAuditEventsAsync<TRequest, TContext>(context.Options.Id);

            if (context.Options.CompensationStrategy is not null &&
                 context.Options.CompensationStrategy.CanRetry(auditResult) ||
                context.Options.CompensationStrategy is null && auditResult.CanRetry)
            {
                context.Request = auditResult.StartedEvent!.Request;
                context.Context = new PipelineRequestContext<TContext>
                {
                    Data = auditResult.LastCompletedEvent!.Context.Data,
                    DataCollection = context.Context.DataCollection,
                };

                context.Options.IsRetry = true;

                context.AuditResult = auditResult;

                if (context.Options.EnableAudit)
                {
                    await _auditService.CleanLogEventsAsync<TRequest, TContext>(context.Options.Id);
                }

                if (context.Options.EnableLog)
                {
                    _logger.LogInformation("Retry of {key} (Process Id: {chainId}) is applied", typeof(TContext).Name, context.Options.Id);
                }
            }

            await next();
        });

        return this;
    }

    public IPipelineBuilder<TRequest, TContext> UseHandlersInSequence()
    {
        var handlers = _handlerRegistry.OrderBy(x => x.Options.GroupId)
            .ThenBy(y => y.Options.OrderId);

        foreach (var handler in handlers)
        {
            this.UseDelegate(async (context, cancellationToken, next) =>
            {
                await new HandlerRunner<TRequest, TContext>(handler, _auditService, _logger).HandleAsync(context, cancellationToken);
                await next();
            });
        }

        return this;
    }

    public IPipelineBuilder<TRequest, TContext> UseHandlersInParallel()
    {
        var groups = _handlerRegistry.GroupBy(y => y.Options.GroupId)
            .OrderBy(x => x.Key)
            .ToDictionary(k => k.Key, v =>
                v.OrderBy(o => o.Options.OrderId));


        foreach (var handlers in groups)
        {
            UseHandlerGroupInParallel(handlers.Value);
        }
        return this;
    }

    public IPipelineBuilder<TRequest, TContext> UseHandlerGroupInParallel(IEnumerable<IHandler<TRequest, TContext>> handlers)
    {
        this.UseDelegate(async (context, cancellationToken, next) =>
        {
            var stack = new Dictionary<string, Task>();

            foreach (var handler in handlers)
            {
                if (handler.Options.DependsOn.Length > 0)
                {
                    var dependencyKeys = handler.Options.DependsOn.Where(x => stack.ContainsKey(x)).ToArray();
                    var dependencyTasks = dependencyKeys.Select(x => stack[x]).ToArray();

                    await TaskHelper.WhenAll(dependencyTasks);

                    foreach (var item in dependencyKeys)
                    {
                        stack.Remove(item);
                    }
                }

                var task = new HandlerRunner<TRequest, TContext>(handler, _auditService, _logger).HandleAsync(context, cancellationToken);

                if (handler.Options.IsSync)
                {
                    await task;
                }
                else
                {
                    stack.Add(handler.Options.Key, task);
                }
            }

            if (stack.Keys.Count > 0)
            {
                await TaskHelper.WhenAll(stack.Values.ToArray());
            }

            await next();
        });


        return this;
    }

    public IPipelineBuilder<TRequest, TContext> Use(Func<PipelineDelegate<TRequest, TContext>, PipelineDelegate<TRequest, TContext>> application)
    {
        _components.Add(application);

        return this;
    }

    public PipelineDelegate<TRequest, TContext> Build(PipelineRequest<TRequest, TContext> request)
    {
        ArgumentNullException.ThrowIfNull(_handlerRegistry);

        PipelineDelegate<TRequest, TContext> app = (_, _) => Task.CompletedTask;
        for (var c = _components.Count - 1; c >= 0; c--)
        {
            app = _components[c](app);
        }

        return app;
    }
}