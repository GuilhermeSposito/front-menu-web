using Microsoft.EntityFrameworkCore;
using SophosSyncDesktop.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SophosSyncDesktop.DataBase.Db;

public class AppDbContext: DbContext
{
    public DbSet<ImpressorasConfigs> Impressoras { get; set; }
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "configs.db");
        optionsBuilder.UseSqlite($"Data Source={path}");
    }
}
