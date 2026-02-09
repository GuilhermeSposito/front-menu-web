using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SophosSyncDesktop.Models;

[Table("impressoras")]
public class ImpressorasConfigs
{
    [Column("id")] public int Id { get; set; }
    [Column("impressora_caixa")] public string ImpressoraCaixa { get; set; } = "Sem Impressora";
    [Column("impressora_aux")] public string ImpressoraAux { get; set; } = "Sem Impressora";
    [Column("impressora_Cz1")] public string ImpressoraCz1 { get; set; } = "Sem Impressora";
    [Column("impressora_Cz2")] public string ImpressoraCz2 { get; set; } = "Sem Impressora";
    [Column("impressora_Cz3")] public string ImpressoraCz3 { get; set; } = "Sem Impressora";
    [Column("impressora_Bar")] public string ImpressoraBar { get; set; } = "Sem Impressora";
    [Column("impressora_Danfe")] public string ImpressoraDanfe { get; set; } = "Sem Impressora";
    [Column("caminho_salvamento_json")] public string CaminhoSalvamentoDoJson { get; set; } = "Downloads";
    [Column("caminho_salvamento_nfe")] public string CaminhoSalvamentoDasNfe { get; set; } = @$"C:\ArqNfe\Autorizadas\";

}
