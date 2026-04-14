using Microsoft.AspNetCore.Components.WebView.WindowsForms;
using Microsoft.Extensions.DependencyInjection;
using MudBlazor.Services;
using SophosSyncDesktop.BlazorComponents;
using SophosSyncDesktop.DataBase.Db;
using SophosSyncDesktop.Services;

namespace SophosSyncDesktop.Views;

public class BlazorDesktopViwer : Form
{
    public BlazorDesktopViwer(Form? owner = null)
    {
        Text = "Sophos Sync";
        FormBorderStyle = FormBorderStyle.None;
        var screen = owner is not null
            ? Screen.FromControl(owner)
            : Screen.FromPoint(Cursor.Position);
        Bounds = screen.Bounds;

        // Ao mover entre monitores, reajusta para ocupar a tela inteira do monitor atual
        bool _ajustando = false;
        Move += (s, e) =>
        {
            if (_ajustando) return;
            var telaAtual = Screen.FromControl(this);
            if (Bounds != telaAtual.Bounds)
            {
                _ajustando = true;
                Bounds = telaAtual.Bounds;
                _ajustando = false;
            }
        };

        TaskScheduler.UnobservedTaskException += (s, e) =>
        {
            e.SetObserved();
            // Ignora erros internos conhecidos do MudBlazor (ex: mudElementRef não inicializado a tempo)
            if (e.Exception.InnerException?.Message?.Contains("mudElementRef") == true)
                return;
            Invoke(() => MessageBox.Show(e.Exception.ToString(), "Task Exception", MessageBoxButtons.OK, MessageBoxIcon.Error));
        };

        try
        {
            var services = new ServiceCollection();
            services.AddWindowsFormsBlazorWebView();
            services.AddMudServices();
            services.AddDbContext<AppDbContext>();
            services.AddSingleton<AppShellService>(new AppShellService(
                fechar: () => Invoke(Close),
                abrirPaginaInicial: () => Invoke(() =>
                {
                    var paginaInicial = Application.OpenForms
                        .OfType<PaginaInicial>()
                        .FirstOrDefault();
                    if (paginaInicial is not null)
                    {
                        paginaInicial.Show();
                        paginaInicial.BringToFront();
                    }
                }),
                atualizarImpressoras: () => Invoke(() =>
                {
                    var paginaInicial = Application.OpenForms
                        .OfType<PaginaInicial>()
                        .FirstOrDefault();
                    if (paginaInicial is not null)
                        paginaInicial.AlimentaComboBoxsDeImpressoras(paginaInicial);
                })
            ));
#if DEBUG
            services.AddBlazorWebViewDeveloperTools();
#endif

            var blazorWebView = new BlazorWebView
            {
                Dock = DockStyle.Fill,
                HostPage = @"wwwroot\hybrid\index.html",
                Services = services.BuildServiceProvider()
            };

            blazorWebView.BlazorWebViewInitializing += (s, e) =>
                System.Diagnostics.Debug.WriteLine("[BlazorHybrid] Initializing...");

            blazorWebView.BlazorWebViewInitialized += (s, e) =>
                System.Diagnostics.Debug.WriteLine("[BlazorHybrid] Initialized OK");

            blazorWebView.RootComponents.Add<App>("#app");
            Controls.Add(blazorWebView);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"{ex.GetType().Name}: {ex.Message}\n\n{ex.StackTrace}",
                "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
}
