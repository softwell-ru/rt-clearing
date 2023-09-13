namespace SoftWell.RtClearing.Routing;

public interface IClearingServiceNameProvider
{
    /// <summary>
    /// Возвращает имя клирингового сервиса для переданного ордера
    /// </summary>
    /// <param name="meta">Мета-информация ордера</param>
    /// <returns></returns>
    string GetClearingServiceName(IClearingMeta meta);
}
