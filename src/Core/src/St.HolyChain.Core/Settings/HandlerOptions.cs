using St.HolyChain.Core.Abstractions;
using St.HolyChain.Core.Models;

namespace St.HolyChain.Core.Settings;

public class HandlerOptions
{
    public virtual string Key { get; set; } = default!;
    public virtual string[] Tags { get; set; } = [];
    public virtual bool IsEnabled { get; set; } = true;
    public virtual short OrderId { get; set; } = 0;
    public virtual short GroupId { get; set; } = 0;
    public virtual bool IsSync { get; set; }
    public virtual ActivityType Type { get; set; }
    public virtual string[] DependsOn { get; set; } = [];
}