
namespace St.HolyChain.Core.Exceptions;

[Serializable]
public class ChainException : Exception
{
    public ChainException(Exception ex) : base("An error has occurred on chain", ex)
    {
    }
}