namespace SoftWell.RtClearing.Routing;

public class UnknownClearingServiceException : Exception
{
    public UnknownClearingServiceException(string tradeId) : base(GetMessage(tradeId))
    {
        TradeId = tradeId ?? throw new ArgumentNullException(nameof(tradeId));
    }

    public string TradeId { get; }

    private static string GetMessage(string tradeId)
    {
        return $"Unknown clearing service for trade '{tradeId}'";
    }
}
