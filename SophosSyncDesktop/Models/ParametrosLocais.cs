using System.ComponentModel.DataAnnotations.Schema;

namespace SophosSyncDesktop.Models;

[Table("parametros_locais")]
public class ParametrosLocais
{
    [Column("id")] public int Id { get; set; }
    [Column("usa_desktop")] public bool UsaDesktop { get; set; } = false;
}
