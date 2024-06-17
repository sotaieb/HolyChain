using Microsoft.Extensions.Options;
using St.HolyChain.Core.Abstractions;
using St.HolyChain.Core.Settings;
using System.Collections;

namespace St.HolyChain.Core;

internal class HandlerRegistry<TRequest, TContext> : IHandlerRegistry<TRequest, TContext>
{
    private readonly IEnumerable<IHandler<TRequest, TContext>> _handlers;

    public HandlerRegistry(IEnumerable<IHandler<TRequest, TContext>> handlers) =>
        _handlers = handlers ?? throw new ArgumentNullException(nameof(handlers));

    public HandlerRegistry(IEnumerable<IHandler<TRequest, TContext>> handlers,
        IOptionsMonitor<HandlerOptions> options)
    {
        _ = handlers ?? throw new ArgumentNullException(nameof(handlers));
        _handlers = handlers.Select(x =>
        {
            x.Options ??= options.Get(x.GetType().Name);
            return x;
        });
        Options = options;
    }

    public IEnumerator<IHandler<TRequest, TContext>> GetEnumerator() => _handlers.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public IOptionsMonitor<HandlerOptions> Options { get; }
}

internal class HandlerRegistry<TRequest> : HandlerRegistry<TRequest, object>, IHandlerRegistry<TRequest>
{
    public HandlerRegistry(IEnumerable<IHandler<TRequest>> handlers,
        IOptionsMonitor<HandlerOptions> options) : base(handlers, options)
    {
    }
}

