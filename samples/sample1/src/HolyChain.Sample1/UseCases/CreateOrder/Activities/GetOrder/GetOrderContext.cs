using St.HolyChain.Core.Abstractions;

namespace HolyChain.Sample1.UseCases.CreateOrder.Activities.GetOrder;

public class GetOrderContext : IContext
{
    public string Value1 { get; set; } = default!;
    public string Value2 { get; set; } = default!;
    public string Value3 { get; set; } = default!;
}