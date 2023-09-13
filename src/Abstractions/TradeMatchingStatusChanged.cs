namespace SoftWell.RtClearing;

/// <summary>
/// Описание смены статуса ордера
/// </summary>
public record TradeMatchingStatusChanged
{
    private DateTime _timestampUtc;

    /// <summary>
    /// Дата и время смены статуса в формате UTC
    /// </summary>
    public required DateTime TimestampUtc
    {
        get => _timestampUtc;
        // по фиксу микросекунды не передаются, поэтому и мы будем их обрезать
        init => _timestampUtc = new DateTime(value.Year, value.Month, value.Day, value.Hour, value.Minute, value.Second, value.Millisecond, value.Kind);
    }

    /// <summary>
    /// Идентификатор ордера в RuTerminal
    /// </summary>
    public required string TradeId { get; init; }

    /// <summary>
    /// Идентификатор внешней системы
    /// </summary>
    public required string ExternalSystem { get; init; }

    /// <summary>
    /// TradingSessionId
    /// </summary>
    public required string TradingSessionId { get; init; }

    /// <summary>
    /// Статус ордера
    /// </summary>
    public required TradeMatchingStatus Status { get; init; }

    /// <summary>
    /// Идентификатор ордера во внешней системе (опционально)
    /// </summary>
    public string? ExternalTradeId { get; init; }

    /// <summary>
    /// Комментарий (опционально)
    /// </summary>
    public string? Comment { get; init; }
}