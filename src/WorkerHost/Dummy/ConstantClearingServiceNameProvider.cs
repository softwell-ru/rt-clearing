using SoftWell.RtClearing.Routing;

namespace SoftWell.RtClearing.WorkerHost.Dummy;

/// <summary>
/// Для всех документов возвращает переданное в конструктор имя в качестве имени клирингового сервиса
/// </summary>
public class ConstantClearingServiceNameProvider : IClearingServiceNameProvider
{
    private readonly string _name;

    public ConstantClearingServiceNameProvider(string name)
    {
        _name = name ?? throw new ArgumentNullException(nameof(name));
    }

    public string GetClearingServiceName(IClearingMeta meta)
    {
        return _name;
    }
}
