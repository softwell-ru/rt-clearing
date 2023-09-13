using System.Collections.Concurrent;
using Moq;

namespace SoftWell.RtClearing.Testing;

public class TestRtTradeReporter
{
    private readonly Mock<IRtTradeReporter> _mock = new(MockBehavior.Strict);

    private readonly ConcurrentBag<TradeMatchingStatusChanged> _statusChanges = new();

    public TestRtTradeReporter()
    {
        _mock.Setup(x => x.ReportStatusChangedAsync(It.IsAny<TradeMatchingStatusChanged>(), It.IsAny<CancellationToken>()))
            .Callback<TradeMatchingStatusChanged, CancellationToken>((m, _) =>
            {
                _statusChanges.Add(m);
            })
            .Returns(Task.CompletedTask)
            .Verifiable();
    }

    public IRtTradeReporter Object => _mock.Object;

    protected TimeSpan WaitInterval { get; } = TimeSpan.FromMilliseconds(100);

    public async Task<TradeMatchingStatusChanged> WaitForTradeStatusChangedAsync(
        Func<TradeMatchingStatusChanged, bool> filter,
        CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(filter);

        while (true)
        {
            await Task.Delay(WaitInterval, ct);

            var current = _statusChanges.ToList();

            foreach (var m in current)
            {
                if (filter(m)) return m;
            }
        }
    }
}