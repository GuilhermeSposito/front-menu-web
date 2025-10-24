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
            using (var context = new AppDbContext())
            {
                context.Database.Migrate();
            }

            ApplicationConfiguration.Initialize();
            Application.Run(new PaginaInicial(new ImpressaoService()));
        }
    }
}