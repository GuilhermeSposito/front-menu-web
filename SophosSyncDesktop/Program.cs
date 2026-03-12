using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SophosSyncDesktop.DataBase.Db;
using SophosSyncDesktop.Services;
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
                Application.Run(new PaginaInicial(new ImpressaoService()));
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ocorreu um erro ao iniciar o aplicativo: " + ex.Message, "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);  
            }
         
        }
    }
}