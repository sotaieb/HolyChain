using HolyChain.Sample1.UseCases.CreateOrder.Activities.CreateOrder;

namespace HolyChain.Sample1.UseCases.CreateOrder.Abstractions;

public interface ICreateOrderService
{
    Task<CreateOrderContext> RunAsync(CreateOrderRequest request, CancellationToken cancellationToken);
}