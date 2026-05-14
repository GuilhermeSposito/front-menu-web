using ApiFiscalMenuWeb.Services;
using Microsoft.Extensions.Hosting;

namespace ApiFiscalMenuWeb.Models.BackGroundWorkers;

public class DelmatchTokenRenewalWorker : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<DelmatchTokenRenewalWorker> _logger;

    public DelmatchTokenRenewalWorker(IServiceProvider serviceProvider, ILogger<DelmatchTokenRenewalWorker> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                VerificarERenovarTokens();
                await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[DelmatchTokenRenewalWorker] Erro no loop de renovação de tokens");
            }
        }
    }

    private void VerificarERenovarTokens()
    {
        using var scope = _serviceProvider.CreateScope();
        var nestApiService = scope.ServiceProvider.GetRequiredService<NestApiServices>();
        nestApiService.SolicitaVerificacaoDeTokens(); // fire-and-forget
    }
}
