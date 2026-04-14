using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SophosSyncDesktop.DataBase.Db;
using SophosSyncDesktop.Models;
using SophosSyncDesktop.Services;
using SophosSyncDesktop.Views;
using System.Diagnostics;
using System.Threading;

namespace SophosSyncDesktop
{
    internal static class Program
    {
     
        [STAThread]
        static void Main()
        {
            try
            {
                var serviceCollection = new ServiceCollection();
                var serviceProvider = serviceCollection.BuildServiceProvider();
                string nomeDoProcesso = "SophosSyncDesktop";

                Process[] processos = Process.GetProcessesByName(nomeDoProcesso);

                if (processos.Length > 1)
                {
                    MessageBox.Show("O aplicativo já está em execução.", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning);

                    return;
                }

                using (var context = new AppDbContext())
                {
                    context.Database.Migrate();
                }

                ApplicationConfiguration.Initialize();

                // Suprime erros internos conhecidos do MudBlazor no Blazor Hybrid (timing de JS no WebView2)
                Application.ThreadException += (s, e) =>
                {
                    var msg = e.Exception?.Message ?? e.Exception?.InnerException?.Message ?? "";
                    if (msg.Contains("mudElementRef"))
                        return;
                    MessageBox.Show(e.Exception?.ToString(), "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
                };
                AppDomain.CurrentDomain.UnhandledException += (s, e) =>
                {
                    if (e.ExceptionObject is Exception ex)
                    {
                        var msg = ex.Message ?? ex.InnerException?.Message ?? "";
                        if (msg.Contains("mudElementRef"))
                            return;
                    }
                };

                var impressaoService = new ImpressaoService();
                var webSocketService = new WebSocketPedidosService(impressaoService);
                var paginaInicial = new PaginaInicial(impressaoService, webSocketService);

                bool usaDesktop;
                using (var ctx = new AppDbContext())
                    usaDesktop = ctx.ParametrosLocais.FirstOrDefault()?.UsaDesktop ?? false;

                if (usaDesktop)
                {
                    // Abre PaginaInicial invisível (sem flash) para ficar em OpenForms e manter o loop
                    paginaInicial.Opacity = 0;
                    paginaInicial.Load += (s, e) =>
                    {
                        paginaInicial.Hide();
                        paginaInicial.Opacity = 1;
                        new BlazorDesktopViwer(paginaInicial).Show();
                    };
                }

                Application.Run(paginaInicial);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ocorreu um erro ao iniciar o aplicativo: " + ex.Message, "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);  
            }
         
        }
    }
}