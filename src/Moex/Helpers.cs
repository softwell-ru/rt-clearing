using QuickFix;

namespace SoftWell.RtClearing.Moex;

public static class Helpers
{
    /// <summary>
    /// Добавляет необходимые изменения для MOEX. Например, пустой пароль меняет на пробел
    /// </summary>
    /// <param name="sessionSettings"></param>
    public static void FixSessionSettings(SessionSettings sessionSettings)
    {
        ArgumentNullException.ThrowIfNull(sessionSettings);

        foreach (var s in sessionSettings.GetSessions())
        {
            var settings = sessionSettings.Get(s);
            if (settings.Has("PASSWORD") && settings.GetString("PASSWORD") == "")
            {
                // MOEX требует, чтобы при отсутствии пароля слался пробел
                settings.SetString("PASSWORD", " ");
            }
            // для NEWPASSWORD не будем такого делать
        }
    }

    public static IMessageStoreFactory GetDefaultMessageFactory(SessionSettings sessionSettings)
    {
        ArgumentNullException.ThrowIfNull(sessionSettings);

        return new FileStoreFactory(sessionSettings);
    }

    public static ILogFactory GetDefaultLogFactory(SessionSettings sessionSettings)
    {
        ArgumentNullException.ThrowIfNull(sessionSettings);

        return new FileLogFactory(sessionSettings);
    }
}