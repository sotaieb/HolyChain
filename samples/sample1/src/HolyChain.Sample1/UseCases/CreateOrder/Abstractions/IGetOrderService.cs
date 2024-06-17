using HolyChain.Sample1.UseCases.CreateOrder.Activities.GetOrder;

namespace HolyChain.Sample1.UseCases.CreateOrder.Abstractions;

public interface IGetOrderService
{
    Task<GetOrderContext> RunAsync(GetOrderRequest request, CancellationToken cancellationToken);
}