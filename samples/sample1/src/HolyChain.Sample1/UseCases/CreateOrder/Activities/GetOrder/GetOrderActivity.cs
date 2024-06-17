using Microsoft.Extensions.Logging;
using St.HolyChain.Core;
using St.HolyChain.Core.Abstractions;

namespace HolyChain.Sample1.UseCases.CreateOrder.Activities.GetOrder;

public class GetOrderActivity : Activity<GetOrderRequest, GetOrderContext>
{
    private readonly ILogger<GetOrderActivity> _logger;

    public GetOrderActivity(ILogger<GetOrderActivity> logger)
    {
        _logger = logger;
    }

    public override Task HandleAsync(GetOrderRequest request, IPipelineRequestContext<GetOrderContext> chainContext,
        CancellationToken cancellationToken = default)
    {
        return HandleInternalAsync(request, chainContext, cancellationToken);
    }

    internal async Task HandleInternalAsync(GetOrderRequest request, IPipelineRequestContext<GetOrderContext> chainContext,
        CancellationToken cancellationToken = default)
    {
        await Task.Delay(200, cancellationToken);

        chainContext.Data.Value2 = "2";
    }
}

