using St.HolyChain.Core.Abstractions;

namespace St.HolyChain.Core.Models;

public class AuditEvent<TRequest, TContext>
{
    public DateTime CreationDate => DateTime.UtcNow;
    public virtual string ChainId { get; set; } = default!;
    public virtual int OrderId { get; set; } = default!;
    public virtual int GroupId { get; set; } = default!;
    public virtual TRequest Request { get; set; } = default!;
    public virtual TContext Context { get; set; } = default!;
    public virtual ActivityStatus Status { get; set; } = default!;
    public virtual string ErrorMessage { get; set; } = default!;
}

public class AuditEvent
{
    public DateTime CreationDate => DateTime.UtcNow;
    public virtual string ChainId { get; set; } = default!;
    public virtual int OrderId { get; set; } = default!;
    public virtual int GroupId { get; set; } = default!;
    public virtual object Request { get; set; } = default!;
    public virtual object Context { get; set; } = default!;
    public virtual ActivityStatus Status { get; set; } = default!;
    public virtual string ErrorMessage { get; set; } = default!;
}
