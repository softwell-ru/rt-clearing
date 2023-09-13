namespace SoftWell.RtClearing;

/// <summary>
/// Сервис отправки ордеров на клиринг
/// </summary>
public interface IClearingService
{
    /// <summary>
    /// Отправляет ордер на клиринг
    /// </summary>
    /// <param name="meta">Мета-информация ордера</param>
    /// <param name="ct"></param>
    /// <returns></returns>
    Task RequestClearingAsync(IClearingMeta meta, CancellationToken ct = default);
}
