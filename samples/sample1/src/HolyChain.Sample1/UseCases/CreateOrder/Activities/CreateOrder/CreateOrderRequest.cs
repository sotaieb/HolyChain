namespace HolyChain.Sample1.UseCases.CreateOrder.Activities.CreateOrder;

public class CreateOrderRequest
{
    public string UserId { get; set; } = "MyUserId";
    public bool ThrowException { get; set; }
}