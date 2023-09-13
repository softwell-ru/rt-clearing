namespace SoftWell.RtClearing;

public enum TradeMatchingStatus
{
    None = 0,

    /// <summary>
    /// Принята на рассмотрение
    /// </summary>
    New,

    /// <summary>
    /// Смэтчилась
    /// </summary>
    Matched,

    // возможно еще Cancelled

    /// <summary>
    /// Отменена
    /// </summary>
    Rejected,

    /// <summary>
    /// Другое
    /// </summary>
    Other
}