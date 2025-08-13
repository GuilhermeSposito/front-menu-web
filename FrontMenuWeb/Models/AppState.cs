using FrontMenuWeb.Models.Merchant;

namespace FrontMenuWeb.Models;

public class AppState
{
   public ClsMerchant MerchantLogado { get; set; } = new ClsMerchant();

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
            default:
                return "/images/bancos/default-bank-icon.png"; // Ícone padrão caso não haja correspondência
        }
    }
}
