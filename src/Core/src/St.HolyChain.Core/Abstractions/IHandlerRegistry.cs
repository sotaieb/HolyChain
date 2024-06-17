using Microsoft.Extensions.Options;
using St.HolyChain.Core.Settings;

namespace St.HolyChain.Core.Abstractions;

public interface IHandlerRegistry
{
    IOptionsMonitor<HandlerOptions> Options { get; }
}

public interface IHandlerRegistry<in TRequest> : IHandlerRegistry<TRequest, object>
{
}

public interface IHandlerRegistry<in TRequest, in TContext> : IHandlerRegistry,
    IEnumerable<IHandler<TRequest, TContext>>
{
}