using System.Runtime.CompilerServices;
using Microsoft.Extensions.Logging;
using QuickFix;
using QuickFix.FIX50SP2;
using SoftWell.Fix.Initiator;
using SoftWell.Fpml.Confirmation;
using SoftWell.Fpml.Confirmation.Serialization;
using SoftWell.Fpml.Serialization;

namespace SoftWell.RtClearing.RtFix;

public class FixToDocumentsAdapter<TFixMessagesReader> : IDocumentsSource
    where TFixMessagesReader : IFixMessagesReader
{
    private readonly TFixMessagesReader _fixMessagesReader;

    private readonly IDocumentSerializer _serializer;

    private readonly ILogger _logger;

    public FixToDocumentsAdapter(
        TFixMessagesReader fixMessagesReader,
        IDocumentSerializer serializer,
        ILogger<FixToDocumentsAdapter<TFixMessagesReader>> logger)
    {
        _fixMessagesReader = fixMessagesReader ?? throw new ArgumentNullException(nameof(fixMessagesReader));
        _serializer = serializer ?? throw new ArgumentNullException(nameof(serializer));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public IAsyncEnumerator<Document> GetAsyncEnumerator(CancellationToken cancellationToken = default)
    {
        return ReadAsync(cancellationToken).GetAsyncEnumerator(cancellationToken);
    }

    private async IAsyncEnumerable<Document> ReadAsync([EnumeratorCancellation] CancellationToken ct = default)
    {
        await foreach (var msg in _fixMessagesReader.WithCancellation(ct))
        {
            if (!msg.IsOfType<TradeCaptureReport>(TradeCaptureReport.MsgType, out var tcr)) continue;

            using var _ = _logger.BeginScope("TradeReportID={TradeReportID}", tcr.TradeReportID.getValue());

            if (!tcr.IsSetSecurityXML())
            {
                _logger.LogWarning("Сообщение {FixMessage} не содержит поля SecurityXML", msg);
                continue;
            }

            Document doc = null!;

            try
            {
                doc = _serializer.DeserializeFromUtf8String(tcr.SecurityXML.getValue());
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Ошибка при десериализации сообщения об ордере: {FixMessage}", msg);
                continue;
            }

            yield return doc;
        }
    }
}
