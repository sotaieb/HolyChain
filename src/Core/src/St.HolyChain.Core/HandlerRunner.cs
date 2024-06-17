using Microsoft.Extensions.Logging;
using St.HolyChain.Core.Abstractions;
using St.HolyChain.Core.Models;

namespace St.HolyChain.Core;

internal class HandlerRunner<TRequest, TContext>
{
    private readonly IHandler<TRequest, TContext> _handler;
    private readonly IAuditService _auditService;
    private readonly ILogger<IPipeline<TRequest, TContext>> _logger;

    public HandlerRunner(IHandler<TRequest, TContext> handler,
        IAuditService auditService,
        ILogger<IPipeline<TRequest, TContext>> logger)
    {
        _handler = handler;
        _auditService = auditService;
        _logger = logger;
    }

    public async Task HandleAsync(PipelineRequest<TRequest, TContext> request,
        CancellationToken cancellationToken)
    {
        try
        {
            if (request.Options.EnableRetry && request is { AuditResult: not null, Options.IsRetry: true })
            {
                var compensationStrategy =
                    request.Options.CompensationStrategy ?? new DefaultCompensationStrategy<TRequest, TContext>();

                var shouldExecute = compensationStrategy.Execute2()
                    .Invoke(_handler, request.AuditResult);

                if (!shouldExecute)
                {
                    return;
                }
            }

            if (_handler.IsSatisfied is not null &&
                !_handler.IsSatisfied.Invoke(request.Context))
            {
                return;
            }

            if (request.Options.EnableLog)
            {
                _logger.LogInformation("The handler {handlerKey} ({groupId},{orderId}) is started",
                    _handler.Options.Key, _handler.Options.GroupId, _handler.Options.OrderId);
            }

            if (request.Options.EnableAudit)
            {
                await _auditService.WriteLogEventAsync(new AuditEvent<TRequest, IPipelineRequestContext<TContext>>
                {
                    ChainId = request.Options.Id,
                    OrderId = _handler.Options.OrderId,
                    GroupId = _handler.Options.GroupId,
                    Status = ActivityStatus.HandlerStarted
                });
            }

            if (request.Options is { EnableRetry: true, IsRetry: true } &&
                _handler.OnCompensateAsync is not null)
            {
                var compensated = await _handler.OnCompensateAsync.Invoke(request.Request,
                    request.Context, cancellationToken);
                if (compensated) return;
            }

            if (_handler.OnBeforeHandleAsync != null)
                await _handler.OnBeforeHandleAsync.Invoke(request.Request, request.Context,
                    cancellationToken);

            await _handler.HandleAsync(request.Request, request.Context, cancellationToken);

            if (_handler.OnAfterHandleAsync != null)
                await _handler.OnAfterHandleAsync.Invoke(request.Request, request.Context,
                    cancellationToken);

            if (request.Options.EnableLog)
            {
                _logger.LogInformation("The handler {handlerKey} ({groupId},{orderId}) is completed",
                    _handler.Options.Key, _handler.Options.GroupId, _handler.Options.OrderId);
            }

            if (request.Options.EnableAudit)
            {
                await _auditService.WriteLogEventAsync(new AuditEvent<TRequest, IPipelineRequestContext<TContext>>
                {
                    ChainId = request.Options.Id,
                    Context = request.Context,
                    OrderId = _handler.Options.OrderId,
                    GroupId = _handler.Options.GroupId,
                    Status = ActivityStatus.HandlerCompleted
                });
            }
        }
        catch (Exception e)
        {
            if (request.Options.EnableLog)
            {
                _logger.LogInformation("The handler {handlerKey} ({groupId},{orderId}) is failed",
                    _handler.Options.Key, _handler.Options.GroupId, _handler.Options.OrderId);

                if (cancellationToken.IsCancellationRequested)
                {
                    _logger.LogError("The handler {handlerKey} ({groupId},{orderId}) is cancelled internally",
                        _handler.Options.Key, _handler.Options.GroupId, _handler.Options.OrderId);
                }
            }

            if (request.Options.EnableAudit)
            {
                await _auditService.WriteLogEventAsync(new AuditEvent<TRequest, IPipelineRequestContext<TContext>>
                {
                    ChainId = request.Options.Id,
                    OrderId = _handler.Options.OrderId,
                    GroupId = _handler.Options.GroupId,
                    ErrorMessage = e.ToString(),
                    Status = ActivityStatus.HandlerFailed
                });
            }

            throw;
        }
    }
}