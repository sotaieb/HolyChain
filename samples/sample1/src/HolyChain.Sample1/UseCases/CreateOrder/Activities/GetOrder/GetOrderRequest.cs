namespace HolyChain.Sample1.UseCases.CreateOrder.Activities.GetOrder;

public class GetOrderRequest
{
    public string UserId { get; set; } = "MyUserId";
    public bool? ThrowException { get; set; }
}