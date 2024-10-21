using System.Reflection.Metadata;
using System.Runtime.InteropServices;

namespace MarketSphere.Services;

public class SummarizerHostedService : BackgroundService, IDisposable
{
    private readonly ILogger<SummarizerHostedService> _logger;
    private readonly SummarizerHandler _handler;
    private readonly IServiceProvider _services;

    public SummarizerHostedService(ILogger<SummarizerHostedService> logger, SummarizerHandler handler, IServiceProvider services)
    {
        _logger = logger;
        _handler = handler;
        _services = services;
    }

    /// <summary>
    /// This method is called when the <see cref="T:Microsoft.Extensions.Hosting.IHostedService" /> starts. The implementation should return a task that represents
    /// the lifetime of the long running operation(s) being performed.
    /// </summary>
    /// <param name="stoppingToken">Triggered when <see cref="M:Microsoft.Extensions.Hosting.IHostedService.StopAsync(System.Threading.CancellationToken)" /> is called.</param>
    /// <returns>A <see cref="T:System.Threading.Tasks.Task" /> that represents the long running operations.</returns>
    /// <remarks>See <see href="https://docs.microsoft.com/dotnet/core/extensions/workers">Worker Services in .NET</see> for implementation guidelines.</remarks>
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await DoWork(stoppingToken);
    }

    private async Task DoWork(CancellationToken stoppingToken)
    {
        _logger.LogInformation(
            "Summarizer Service Hosted Service is working.");

        while (!stoppingToken.IsCancellationRequested)
        {
            string? url = _handler.PopCurrentUrl();

            if (!string.IsNullOrEmpty(url))
            {
                await ProcessComments(url, stoppingToken);
            }

            //_handler.Append(" 1");
            await Task.Delay(1000, stoppingToken);
        }
    }

    private async Task ProcessComments(string url, CancellationToken stoppingToken)
    {
        using var scope = _services.CreateScope();
        var scraper =
            scope.ServiceProvider
                .GetRequiredService<CitilinkShopScraper>();

        var gpt =
            scope.ServiceProvider
                .GetRequiredService<GptClient>();

        string text = await scraper.GetCommentsText(url);

        if (string.IsNullOrEmpty(text))
        {
            _handler.Append("Нет отзывов");
            return;
        }

        await foreach (var token in gpt.Summarize(text, stoppingToken))
        {
            _handler.Append(token);
        }
    }
}