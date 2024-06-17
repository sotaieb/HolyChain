using Microsoft.Extensions.Logging;
using St.HolyChain.Core;
using St.HolyChain.Core.Abstractions;

namespace HolyChain.Sample1.UseCases.CreateOrder.Activities.CreateOrder;

public class RegisterOrderActivity : Activity<CreateOrderRequest, CreateOrderContext>
{
    private readonly ILogger<RegisterOrderActivity> _logger;

    public RegisterOrderActivity(ILogger<RegisterOrderActivity> logger)
    {
        _logger = logger;
    }

    public override Task HandleAsync(CreateOrderRequest request, IPipelineRequestContext<CreateOrderContext> chainContext,
        CancellationToken cancellationToken = default)
    {
        return HandleInternalAsync(request, chainContext, cancellationToken);
    }

    internal async Task HandleInternalAsync(CreateOrderRequest request, IPipelineRequestContext<CreateOrderContext> chainContext,
        CancellationToken cancellationToken = default)
    {
        await Task.Delay(100, cancellationToken);

        chainContext.Data.Value5 = "5";

        if (chainContext.DataCollection.Get<bool>("ThrowException") == true)
        {
            throw new Exception("something NOK");
        }
    }
}

