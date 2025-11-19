using SophosSyncDesktop.DataBase.Db;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Unimake.Business.DFe.Servicos;
using Unimake.Security.Platform;

namespace SophosSyncDesktop.Views.TestesNfe;

public partial class TesteComCertificado : Form
{
    public TesteComCertificado() => InitializeComponent();

    private void CarregarCertificado()
    {
        try
        {
            var certificadoService = new CertificadoDigital();

            byte[] certificadoByte = certificadoService.ToByteArray(@"C:\SophosCompany\251 - SOPHOS APLICATIVOS E TECNOLOGIA LTDA_Senha Sophos@1234_Validade 08 09 2026.pfx");
            string base64 = Convert.ToBase64String(certificadoByte);

            using (AppDbContext db = new AppDbContext())
            {
                var configs = db.Impressoras.FirstOrDefault();

                if (configs is null)
                    throw new Exception("Config não existe");

                configs.base64Certificado = base64;
                db.SaveChanges();
            }

        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.ToString());
        }

    }

    private void VerificarCertificado()
    {
        try
        {
            using (AppDbContext db = new AppDbContext())
            {
                var configs = db.Impressoras.FirstOrDefault();

                if (configs is null)
                    throw new Exception("Config não existe");

                byte[] CertificaodByttes = Convert.FromBase64String(configs.base64Certificado);

                var certificadoService = new CertificadoDigital();
                var CertificadoSelecionado = certificadoService.CarregarCertificadoDigitalA1(CertificaodByttes, "Sophos@1234");

                var config = new Configuracao
                {
                    TipoDFe = TipoDFe.NFe,
                    TipoEmissao = TipoEmissao.Normal,
                    TipoAmbiente = TipoAmbiente.Homologacao,
                    CertificadoDigital = CertificadoSelecionado
                };

                MessageBox.Show(config.CertificadoDigital.Subject);

            }
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.ToString());
        }
    }

    private void button1_Click(object sender, EventArgs e)
    {
        CarregarCertificado();
    }

    private void button2_Click(object sender, EventArgs e)
    {
        VerificarCertificado();
    }
}

/*   
*/