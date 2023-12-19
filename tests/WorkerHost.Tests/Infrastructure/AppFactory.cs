using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using QuickFix.FIX50SP2;
using SoftWell.Fix.Initiator.Rt;
using SoftWell.Fix.Initiator.Testing;
using SoftWell.RtClearing.Moex;
using SoftWell.RtClearing.Moex.Configuration;
using SoftWell.RtClearing.RtFix;
using SoftWell.RtClearing.Testing;
using SoftWell.RtCodes;

namespace SoftWell.RtClearing.WorkerHost.Tests.Infrastructure;

public class AppFactory : IDisposable, IAsyncDisposable
{
    private readonly Action<IServiceCollection>? _configureServices;

    private IHost _host;

    public AppFactory(
        Action<IServiceCollection>? configureServices = null)
    {
        _configureServices = configureServices;
        _host = CreateHostBuilder().Build();
        _host.Start();
    }

    public TestFixClient<IMoexFixClient> MoexFixClientMock { get; } = new();

    public TestFixClient<IRtFixClient> RtFixClientMock { get; } = new();

    public TestDocumentsSource DocumentsSource { get; } = new();

    public T Resolve<T>() where T : notnull
    {
        return _host.Services.GetRequiredService<T>();
    }

    public async ValueTask DisposeAsync()
    {
        await DisposeAsyncCore();
        Dispose(false);
        GC.SuppressFinalize(this);
    }

    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    protected IHostBuilder CreateHostBuilder()
    {
        var contentRoot = Directory.GetCurrentDirectory();

        return Host
            .CreateDefaultBuilder()
            .UseContentRoot(contentRoot)
            .UseEnvironment("Testing")
            .ConfigureServices((host, services) =>
            {
                ConfigureServices(host, services);
            });
    }

    protected virtual void ConfigureServices(HostBuilderContext host, IServiceCollection services)
    {
        services.AddSingleton<IDocumentsSource>(DocumentsSource);
        services.AddSingleton(RtFixClientMock.Object);
        services.AddSingleton<IRtFixMapper, RtFixMapper>();
        services.AddSingleton<MoexClearingOptions>();
        services.AddSingleton<IRtTradeReporter, RtFixTradeReporter<IRtFixClient>>();

        services.AddLogging(b => b.AddConsole().SetMinimumLevel(LogLevel.Trace));

        services.AddCodesConversion(
            opts => opts.AddCsvHttpSource(new Uri("https://confluence.softwell.ru/download/attachments/211877976/codes-mapping-dev.csv?api=v2")));
        services.AddXmlFpmlConfirmationSerialization();

        services.AddDefaultClearingMetaExtractor();

        services.AddDefaultClearingRouting(
            b =>
            {
                b.Services.AddSingleton(MoexFixClientMock.Object);

                b.Services.AddSingleton(sp =>
                {
                    var client = sp.GetRequiredService<IMoexFixClient>();
                return new MoexClearingService(
                        sp.GetRequiredService<MoexClearingOptions>(),
                        client,
                        sp.GetRequiredService<ICodesConverter>(),
                        sp.GetRequiredService<IRtTradeReporter>(),
                        sp.GetRequiredService<ILogger<MoexClearingService>>());
                });

                b.AddNamedClearingService<MoexClearingService>("MOEX");

                services.AddFixMessagesHandling<IMoexFixClient>(
                    opts => opts.AddMessagesHandler<ExecutionReport, MoexClearingService>());
            });

        services.AddClearingProcessing();

        _configureServices?.Invoke(services);
    }

    protected virtual async ValueTask DisposeAsyncCore()
    {
        if (_host is not null)
        {
            await _host.StopAsync();
            _host.Dispose();
        }

        _host = null!;
    }

    protected virtual void Dispose(bool disposing)
    {
        if (disposing)
        {
            _host?.Dispose();
            _host = null!;
        }
    }
}
