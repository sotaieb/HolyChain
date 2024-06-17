using St.HolyChain.Core.Abstractions;

namespace HolyChain.Sample1.UseCases.CreateOrder.Activities.CreateOrder;

public class CreateOrderContext : IContext
{
    public string Value1 { get; set; } = default!;
    public string Value2 { get; set; } = default!;
    public string Value3 { get; set; } = default!;
    public string Value4 { get; set; } = default!;
    public string Value5 { get; set; } = default!;
}