using FrontMenuWeb.Components.Modais.ModaisGenericos;
using FrontMenuWeb.Models.Merchant;
using MudBlazor;

namespace FrontMenuWeb.Models;

public class AppState
{
   public ClsMerchant MerchantLogado { get; set; } = new ClsMerchant();
   public bool CaixaAberto { get; set; } = false;

    public string GetBancoIcon(int codBanco)
    {
        switch (codBanco)
        {
            case 1:
                return "/images/bancos/bradesco-logo-4.png";
            case 2:
                return "/images/bancos/banco-do-brasil.png";
            case 3:
                return "/images/bancos/sicob-logo.png";
            case 4:
                return "/images/bancos/itau.png";
            case 5:
                return "/images/bancos/caixa-economica-logo.png";
            case 6:
                return "/images/bancos/logo-santander.png";
            case 7:
                return "/images/bancos/stone.png";
            case 8:
                return "/images/bancos/pagbank-logo.png";
            default:
                return "/images/bancos/default-bank-icon.png"; // Ícone padrão caso não haja correspondência
        }
    }

    public string GetIconeDoApp(string CeriadoPor)
    {
        switch (CeriadoPor)
        {
            case "SOPHOS":
                return "/images/SOPHOSLOGOLOGIN.jpg";
            case "IFOOD":
                return "/images/ifoodImagem.png";
            default:
                return "/images/SOPHOSLOGOLOGIN.jpg"; // Ícone padrão caso não haja correspondência

        }
    }

    public int DefineCodigoDoBancoDinamicamente(string NomeEscrito)
    {
        if (NomeEscrito.Contains("bradesco", StringComparison.OrdinalIgnoreCase))
            return 1;
        else if (NomeEscrito.Contains("banco do brasil", StringComparison.OrdinalIgnoreCase) || NomeEscrito.Contains("bb", StringComparison.OrdinalIgnoreCase))
            return 2;
        else if (NomeEscrito.Contains("sicoob", StringComparison.OrdinalIgnoreCase))
            return 3;
        else if (NomeEscrito.Contains("itau", StringComparison.OrdinalIgnoreCase))
            return 4;
        else if (NomeEscrito.Contains("Caixa Economica", StringComparison.OrdinalIgnoreCase))
            return 5;
        else if (NomeEscrito.Contains("Santander", StringComparison.OrdinalIgnoreCase))
            return 6;
        else if (NomeEscrito.Contains("Stone", StringComparison.OrdinalIgnoreCase))
            return 7;
        else if (NomeEscrito.Contains("PagBank", StringComparison.OrdinalIgnoreCase))
            return 8;
        else
            return 0;

    }
}
