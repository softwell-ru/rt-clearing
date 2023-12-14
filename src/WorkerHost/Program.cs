using System.Globalization;
using System.Text;
using QuickFix;
using Serilog;
using SoftWell.RtClearing;
using SoftWell.RtClearing.Moex.Configuration;
using SoftWell.RtClearing.WorkerHost.Dummy;


Serilog.Debugging.SelfLog.Enable(Console.Error);

Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
QuickFix.CharEncoding.DefaultEncoding = Encoding.GetEncoding("Windows-1251");

var builder = Host.CreateApplicationBuilder(args);

builder.Configuration
    .AddJsonFile("appsettings.json", true, false)
    .AddJsonFile("appsettings.local.json", true, false);

builder.Services.AddSerilog(ConfigureLoggerConfiguration);

builder.Services.AddCodesConversion(
    // local file
    opts => opts.AddCsvFileSource("codes-mapping-dev.csv"));

builder.Services.AddXmlFpmlConfirmationSerialization();

// если будет просто константа, то компилятор будет ругаться на недосягаемый кусок кода в else
var useRtFix = bool.Parse("True");
var useMoexFix = bool.Parse("True");

if (useRtFix)
{
    var sessionSettings = GetSessionSettings("fix-configs/Rt/SessionSettings");
    builder.Services.AddRtFix(sessionSettings);
}
else
{
    builder.Services.AddSingleton<IDocumentsSource, ConstantDocumentsSource>();
    builder.Services.AddSingleton<IRtTradeReporter, LogWriterRtTradeReporter>();
}

builder.Services.AddDefaultClearingMetaExtractor();

builder.Services.AddDefaultClearingRouting(
    b =>
    {
        if (useMoexFix)
        {
            var options = builder.Configuration.GetSection("Moex").Get<MoexClearingOptions>()
                ?? throw new InvalidOperationException("Required configuration section 'Moex' is missing");
            var sessionSettings = GetSessionSettings("fix-configs/Moex/SessionSettings");
            b.AddNamedMoexClearingService("MOEX", options, sessionSettings);
        }
        else
        {
            b.AddNamedClearingService<LogWriterClearingService>("test");
            b.AddClearingServiceNameProvider(new ConstantClearingServiceNameProvider("test"));
        }
    });

builder.Services.AddClearingProcessing();

using var host = builder.Build();
await host.RunAsync();


static SessionSettings GetSessionSettings(string pathWithoutExtension)
{
    var localPath = $"{pathWithoutExtension}.local.cfg";
    var settingsPath = File.Exists(localPath) ? localPath : $"{pathWithoutExtension}.cfg";
    var sessionSettings = new SessionSettings(settingsPath);
    return sessionSettings;
}

static void ConfigureLoggerConfiguration(LoggerConfiguration config)
{
    const string shortTemplate = "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}";
    const string logsPath = "logs/app";

    config.MinimumLevel.Verbose();

    config
        .WriteTo.Async(x => x.Console(
            Serilog.Events.LogEventLevel.Verbose,
            outputTemplate: shortTemplate,
            formatProvider: CultureInfo.InvariantCulture));

    const int fileSizeLimitBytes = 1024 * 1024 * 100;
    var flushToDiskInterval = TimeSpan.FromSeconds(1);

    config.WriteTo.Async(
        c => c.Map(
            e => DateTime.Now.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
            (v, wt) =>
            {
                wt.File(
                    $"{logsPath}/{v}/log-.log",
                    rollingInterval: RollingInterval.Day,
                    retainedFileCountLimit: null,
                    fileSizeLimitBytes: fileSizeLimitBytes,
                    flushToDiskInterval: flushToDiskInterval,
                    buffered: true,
                    rollOnFileSizeLimit: true,
                    outputTemplate: shortTemplate,
                    formatProvider: CultureInfo.InvariantCulture);

                wt.File(
                    new Serilog.Formatting.Compact.CompactJsonFormatter(),
                    $"{logsPath}/{v}/trace/log-.json",
                    rollingInterval: RollingInterval.Hour,
                    retainedFileCountLimit: null,
                    fileSizeLimitBytes: fileSizeLimitBytes,
                    flushToDiskInterval: flushToDiskInterval,
                    buffered: true,
                    rollOnFileSizeLimit: true);
            },
            sinkMapCountLimit: 1),
        bufferSize: 100_000,
        blockWhenFull: true);

    var v = typeof(Program).Assembly?.GetName()?.Version
        ?? new Version(1, 0, 0, 0);

    config
        .Enrich.FromLogContext()
        .Enrich.WithThreadId()
        .Enrich.WithProperty("Version", v.ToString());
}