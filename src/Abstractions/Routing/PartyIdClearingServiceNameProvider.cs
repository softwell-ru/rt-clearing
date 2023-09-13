namespace SoftWell.RtClearing.Routing;

/// <summary>
/// Имплементация <see cref="IClearingServiceNameProvider" />, которая в качестве имени клирингового сервиса для ордера возвращает PartyId клиринговой организации
/// </summary>
public class PartyIdClearingServiceNameProvider : IClearingServiceNameProvider
{
    public string GetClearingServiceName(IClearingMeta meta)
    {
        ArgumentNullException.ThrowIfNull(meta);

        return meta.GetClearingOrganizationPartyId();
    }
}
