using ApiFiscalMenuWeb.Services.Integracoes;
using Microsoft.Extensions.Hosting;

namespace ApiFiscalMenuWeb.Models.BackGroundWorkers;


public class IfoodPollingWorker : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<IfoodPollingWorker> _logger;

    public IfoodPollingWorker(IServiceProvider serviceProvider, ILogger<IfoodPollingWorker> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await ExecutarPolling();

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await Task.Delay(TimeSpan.FromMinutes(7), stoppingToken);
                await ExecutarPolling();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro no loop do polling");
            }
        }
    }

    private async Task ExecutarPolling()
    {
        using (var scope = _serviceProvider.CreateScope())
        {
            var ifoodService = scope.ServiceProvider.GetRequiredService<IfoodServices>();

            await ifoodService.PollingIfood();
        }
    }
}