using SoftWell.Fpml.Confirmation;

namespace SoftWell.RtClearing;

/// <summary>
/// Сервис по извлечению метаинформации ордера
/// </summary>
public interface IClearingMetaExtractor
{
    /// <summary>
    /// Проверяет, требуется ли отправка ордера на клиринг
    /// </summary>
    /// <param name="document">Документ ордера</param>
    /// <returns></returns>
    bool IsClearingRequested(Document document);

    /// <summary>
    /// Возвращает метаинформацию ордера
    /// </summary>
    /// <param name="document">Документ с ордером</param>
    /// <returns>Метаинформация ордера</returns>
    IClearingMeta Extract(Document document);
}
