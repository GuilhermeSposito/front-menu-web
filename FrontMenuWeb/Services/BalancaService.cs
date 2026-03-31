using Microsoft.JSInterop;

namespace FrontMenuWeb.Services;

public class BalancaService
{
    private readonly IJSRuntime _js;

    public BalancaService(IJSRuntime js)
    {
        _js = js;
    }

    public async Task<BalancaResultado> ConectarAsync(int baudRate = 9600, bool forcarConexao = false)
    {
        return await _js.InvokeAsync<BalancaResultado>("balancaSerial.conectar", baudRate, forcarConexao);
    }

    public async Task<BalancaLeitura> LerPesoAsync()
    {
        return await _js.InvokeAsync<BalancaLeitura>("balancaSerial.lerPeso");
    }

    public async Task<BalancaResultado> DesconectarAsync()
    {
        return await _js.InvokeAsync<BalancaResultado>("balancaSerial.desconectar");
    }

    public async Task<bool> EstaConectadaAsync()
    {
        return await _js.InvokeAsync<bool>("balancaSerial.estaConectada");
    }
}

public class BalancaResultado
{
    public bool Sucesso { get; set; }
    public string Mensagem { get; set; } = string.Empty;
}

public class BalancaLeitura
{
    public bool Sucesso { get; set; }
    public double Peso { get; set; }
    public string Mensagem { get; set; } = string.Empty;
    public string? RespostaBruta { get; set; }
}
