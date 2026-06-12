using ApiFiscalMenuWeb.Services.Integracoes;
using Microsoft.Extensions.Hosting;

namespace ApiFiscalMenuWeb.Models.BackGroundWorkers;

public class CcmPollingWorker : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<CcmPollingWorker> _logger;

    public CcmPollingWorker(IServiceProvider serviceProvider, ILogger<CcmPollingWorker> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await ExecutarPolling();

        // O PollingInit deve ser chamado a cada 30 segundos, sempre, sem interrupção
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);
                await ExecutarPolling();
            }
            catch (OperationCanceledException)
            {
                // Aplicação sendo encerrada
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[CcmPollingWorker] Erro no loop do polling");
            }
        }
    }

    private async Task ExecutarPolling()
    {
        try
        {
            using var scope = _serviceProvider.CreateScope();
            var service = scope.ServiceProvider.GetRequiredService<CcmServices>();
            await service.PollingInit();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[CcmPollingWorker] Erro ao executar o PollingInit");
        }
    }
}
