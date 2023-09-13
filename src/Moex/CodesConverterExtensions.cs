using SoftWell.RtClearing.Moex;

namespace SoftWell.RtCodes;

public static class CodesConverterExtensions
{
    public static ValueTask<string> ConvertRtPartyIdToMoexAsync(this ICodesConverter converter, string partyId, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(converter);
        ArgumentNullException.ThrowIfNull(partyId);

        return converter.ConvertRtPartyIdAsync(partyId, KnownSchemes.Moex.PartyId, ct);
    }

    public static ValueTask<string> ConvertRtProductIdToMoexAsync(this ICodesConverter converter, string productId, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(converter);
        ArgumentNullException.ThrowIfNull(productId);

        return converter.ConvertRtProductIdAsync(productId, KnownSchemes.Moex.ProductId, ct);
    }
}
