using Microsoft.Extensions.Logging;
using QuickFix;
using QuickFix.Fields;
using SoftWell.Fix.Initiator;

namespace SoftWell.RtClearing.Moex;

public class MoexFixClient : FixClient, IMoexFixClient
{
    public MoexFixClient(
        SessionSettings sessionSettings,
        ILogger<MoexFixClient> logger) : base(sessionSettings, logger)
    {
    }

    protected override bool IsPasswordChangedLogon(Message logonMessage)
    {
        ArgumentNullException.ThrowIfNull(logonMessage);

        if (!logonMessage.IsSetField(new Text())) return false;

        var text = logonMessage.GetField(new Text()).getValue();
        return string.Equals(text, "(209) Password successfully changed", StringComparison.Ordinal);
    }

    protected override bool IsInvalidPasswordLogout(Message logoutMessage)
    {
        ArgumentNullException.ThrowIfNull(logoutMessage);

        if (!logoutMessage.IsSetField(new Text())) return false;

        var text = logoutMessage.GetField(new Text()).getValue();
        return text.StartsWith("ERROR: (207) Invalid password. User will be blocked after", StringComparison.Ordinal);
    }
}