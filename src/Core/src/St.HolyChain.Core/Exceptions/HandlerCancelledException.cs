
namespace St.HolyChain.Core.Exceptions;

[Serializable]
public class HandlerCancelledException : OperationCanceledException
{
    public HandlerCancelledException() : base("handler is cancelled")
    {
    }
}