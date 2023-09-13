namespace SoftWell.RtClearing.Routing;

/// <summary>
/// Обертка для именованных клиринговых сервисов
/// </summary>
public interface INamedClearingService : IClearingService
{
    string Name { get; }
}
