using ApiFiscalMenuWeb.Services.Integracoes;
using Microsoft.Extensions.Hosting;

namespace ApiFiscalMenuWeb.Models.BackGroundWorkers;

public class DelmatchPollingWorker : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<DelmatchPollingWorker> _logger;

    public DelmatchPollingWorker(IServiceProvider serviceProvider, ILogger<DelmatchPollingWorker> logger)
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
                await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
                await ExecutarPolling();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[DelmatchPollingWorker] Erro no loop do polling");
            }
        }
    }

    private async Task ExecutarPolling()
    {
        using var scope = _serviceProvider.CreateScope();
        var service = scope.ServiceProvider.GetRequiredService<B1DeliveryServices>();
        await service.ExecutarPollingTodasEmpresas();
    }
}
