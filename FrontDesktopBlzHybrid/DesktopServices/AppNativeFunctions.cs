using System;
using System.Collections.Generic;
using System.Drawing.Printing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FrontDesktopBlzHybrid.DesktopServices;

public class AppNativeFunctions
{
    public void CloseApp()
    {
#if WINDOWS
                Application.Current.Quit();
#endif
    }

    public List<string> GetImpressoras()
    {
        var impressoras = new List<string>();
#if WINDOWS7_0_OR_GREATER
             var impressorasINstaladas = PrinterSettings.InstalledPrinters;


                foreach (var impressora in impressorasINstaladas)
                {
                    impressoras.Add(impressora.ToString() ?? "imp não reconhecida");
                }

#endif
        return impressoras;
    }
}
