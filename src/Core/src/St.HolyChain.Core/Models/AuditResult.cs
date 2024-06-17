using St.HolyChain.Core.Abstractions;

namespace St.HolyChain.Core.Models;

public class AuditResult<TRequest, TContext>
{
    public List<AuditEvent<TRequest, TContext>> Events { get; }
    public List<AuditEvent<TRequest, TContext>> FailedEvents { get; }
    public List<AuditEvent<TRequest, TContext>> CompletedEvents { get; }
    public AuditEvent<TRequest, TContext>? StartedEvent { get; }
    public AuditEvent<TRequest, TContext>? LastCompletedEvent { get; }
    public AuditEvent<TRequest, TContext>? LastFailedEvent { get; }

    public AuditResult(List<AuditEvent<TRequest, TContext>> events)
    {
        ArgumentNullException.ThrowIfNull(events);

        Events = events;
        FailedEvents = Events.Where(x => x.Status is ActivityStatus.HandlerFailed or ActivityStatus.HandlerCancelled)
            .ToList();
        CompletedEvents = Events.Where(x => x.Status is ActivityStatus.HandlerCompleted)
            .ToList();
        StartedEvent = Events.LastOrDefault(x => x.Status == ActivityStatus.Started);
        LastCompletedEvent = CompletedEvents.LastOrDefault(x => x.Status == ActivityStatus.HandlerCompleted);
        LastFailedEvent = FailedEvents.LastOrDefault(x =>
            x.Status is ActivityStatus.HandlerFailed or ActivityStatus.HandlerCancelled);
    }

    public bool CanRetry => Events.Any() && StartedEvent is not null && LastCompletedEvent is not null && LastFailedEvent is not null;
}