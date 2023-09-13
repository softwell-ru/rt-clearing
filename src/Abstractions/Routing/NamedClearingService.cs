using SoftWell.Fpml.Confirmation;

namespace SoftWell.RtClearing.Routing;

/// <inheritdoc />
public sealed class NamedClearingService : INamedClearingService
{
    private readonly IClearingService _service;

    public NamedClearingService(string name, IClearingService service)
    {
        Name = name ?? throw new ArgumentNullException(nameof(name));
        _service = service ?? throw new ArgumentNullException(nameof(service));
    }

    public string Name { get; }

    public Task RequestClearingAsync(IClearingMeta meta, CancellationToken ct = default)
    {
        return _service.RequestClearingAsync(meta, ct);
    }
}