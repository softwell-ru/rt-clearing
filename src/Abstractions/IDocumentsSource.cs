using SoftWell.Fpml.Confirmation;

namespace SoftWell.RtClearing;

/// <summary>
/// Источник ордеров из RuTerminal
/// </summary>
public interface IDocumentsSource : IAsyncEnumerable<Document>
{
}
