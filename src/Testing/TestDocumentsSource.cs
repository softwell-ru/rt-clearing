using System.Threading.Channels;
using SoftWell.Fpml.Confirmation;

namespace SoftWell.RtClearing.Testing;

public class TestDocumentsSource : IDocumentsSource, IDisposable
{
    private readonly Channel<Document> _channel;

    private bool _disposedValue;

    public TestDocumentsSource()
    {
        _channel = Channel.CreateUnbounded<Document>(new UnboundedChannelOptions
        {
            AllowSynchronousContinuations = false,
            SingleReader = true,
            SingleWriter = true
        });
    }

    public IAsyncEnumerator<Document> GetAsyncEnumerator(CancellationToken cancellationToken = default)
    {
        return _channel.Reader.ReadAllAsync(cancellationToken).GetAsyncEnumerator(cancellationToken);
    }

    public void EmulateDocument(Document document)
    {
        ArgumentNullException.ThrowIfNull(document);

        _channel.Writer.TryWrite(document);
    }

    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (_disposedValue) return;

        if (disposing)
        {
            _channel.Writer.TryComplete();
        }

        _disposedValue = true;
    }
}