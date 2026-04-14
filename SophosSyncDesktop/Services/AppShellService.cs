namespace SophosSyncDesktop.Services;

public class AppShellService
{
    private readonly Action _fechar;
    private readonly Action _abrirPaginaInicial;
    private readonly Action _atualizarImpressoras;

    public AppShellService(Action fechar, Action abrirPaginaInicial, Action atualizarImpressoras)
    {
        _fechar = fechar;
        _abrirPaginaInicial = abrirPaginaInicial;
        _atualizarImpressoras = atualizarImpressoras;
    }

    public void FecharApp() => _fechar();
    public void AbrirPaginaInicial() => _abrirPaginaInicial();
    public void AtualizarImpressoras() => _atualizarImpressoras();
}
