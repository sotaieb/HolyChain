using St.HolyChain.Core.Abstractions;
using St.HolyChain.Core.Settings;

namespace St.HolyChain.Core.Builder;

public static class PipelineRequestBuilder
{
    public static PipelineRequestBuilder<TRequest, TContext> Create<TRequest, TContext>() where TContext : new() => new();
}

public class PipelineRequestBuilder<TRequest, TContext> : IPipelineRequestBuilder<TRequest, TContext> where TContext : new()
{
    private TRequest _request = default!;
    private readonly IPipelineRequestContext<TContext> _context = new PipelineRequestContext<TContext>();
    private readonly PipelineOptions<TRequest, TContext> _options = new();

    internal PipelineRequestBuilder()
    {

    }
    public IPipelineRequestBuilder<TRequest, TContext> For(TRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        _request = request;

        return this;
    }

    public IPipelineRequestBuilder<TRequest, TContext> AddContext(Action<IPipelineRequestContext<TContext>>? configureRequest)
    {
        configureRequest?.Invoke(_context);

        return this;
    }


    public IPipelineRequestBuilder<TRequest, TContext> AddOptions(
        Action<PipelineOptions<TRequest, TContext>>? configureOptions)
    {
        configureOptions?.Invoke(_options);

        if (string.IsNullOrWhiteSpace(_options.Id))
        {
            _options.Id = Guid.NewGuid().ToString();
        }
        _options.Tags ??= [];

        return this;
    }

    public PipelineRequest<TRequest, TContext> Build()
    {
        ArgumentNullException.ThrowIfNull(_request);

        var request = new PipelineRequest<TRequest, TContext>
        {
            Options = _options,
            Context = _context,
            Request = _request
        };

        return request;
    }
}
