using SoftWell.Fpml.Confirmation;

namespace SoftWell.RtClearing;

/// <summary>
/// Метаинформация ордера
/// </summary>
public interface IClearingMeta
{
    /// <summary>
    /// Изначальный документ ордера
    /// </summary>
    Document Document { get; }

    /// <summary>
    /// Возвращает PartyId клиринговой организации для party, которой предназначен документ
    /// </summary>
    /// <returns>PartyId</returns>
    string GetClearingOrganizationPartyId();

    /// <summary>
    /// Возвращает идентификатор ордера
    /// </summary>
    /// <returns></returns>
    string GetTradeId();

    /// <summary>
    /// Возвращает комментарий сделки
    /// </summary>
    /// <returns></returns>
    string? GetComment();

    /// <summary>
    /// Возвращает инструмент ордера
    /// </summary>
    /// <returns></returns>
    string GetProductId();

    /// <summary>
    /// Возвращает Party по идентификатору в рамках документа
    /// </summary>
    /// <returns></returns>
    string GetPartyByHref(string partyHref);

    /// <summary>
    /// Возвращает Party, которой предназначен документ
    /// </summary>
    /// <returns></returns>
    string GetOnBehalfOf();
}
