using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SophosSyncDesktop.Models;

[Table("infos_de_login")]
public class InfosDeLogin
{
    [Column("id")] public int Id { get; set; }
    [Column("email")] public string Email { get; set; } = "";
    [Column("senha")] public string Senha { get; set; } = "";
    [Column("token")] public string Token { get; set; } = "";
}
