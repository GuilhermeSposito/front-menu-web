using System;
using System.Collections.Generic;
using System.Drawing.Drawing2D;
using System.Drawing.Printing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SophosSyncDesktop.Utils;

public class ClsEstiloComponentes
{
    public void CriaBordaArredondada(Control control, int radius) //Método para arredondar os cantos dos UserCntrol
    {
        GraphicsPath path = new GraphicsPath();
        int width = control.Width;
        int height = control.Height;
        path.AddArc(0, 0, radius, radius, 180, 90);
        path.AddArc(width - radius, 0, radius, radius, 270, 90);
        path.AddArc(width - radius, height - radius, radius, radius, 0, 90);
        path.AddArc(0, height - radius, radius, radius, 90, 90);
        path.CloseFigure();

        control.Region = new Region(path);
    }

    public List<string> ListaImpressoras()
    {
        var impressoras = PrinterSettings.InstalledPrinters;
        List<string> listaImpressoras = new List<string>();

        foreach (var item in impressoras)
        {
            listaImpressoras.Add(item.ToString());
        }

        return listaImpressoras;
    }
}
