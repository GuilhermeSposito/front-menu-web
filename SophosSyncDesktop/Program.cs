using Microsoft.EntityFrameworkCore;
using SophosSyncDesktop.DataBase.Db;
using SophosSyncDesktop.Services;

namespace SophosSyncDesktop
{
    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            try
            {
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