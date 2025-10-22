using SophosSyncDesktop.Services;
using System.IO;
using System.Text;
using System.Windows.Forms;

namespace SophosSyncDesktop
{
    public partial class PaginaInicial : Form
    {
        private FileSystemWatcher watcher;
        private readonly ImpressaoService _impressaoService;
        public PaginaInicial(ImpressaoService ImpressaoService)
        {
            _impressaoService = ImpressaoService;

            InitializeComponent();

            // Configura o NotifyIcon
            SophosSync.Visible = true;

            var menu = new ContextMenuStrip();
            menu.Items.Add("Abrir", null, (s, e) => ShowWindow());
            menu.Items.Add("Fechar", null, (s, e) => Application.Exit());
            SophosSync.ContextMenuStrip = menu;

            SophosSync.DoubleClick += (s, e) => ShowWindow();

            IniciarMonitoramento();
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            // Esconde a janela ao iniciar
            this.Hide();
        }

        private void ShowWindow()
        {
            this.Show();
            this.WindowState = FormWindowState.Normal;
            this.ShowInTaskbar = true;
        }

        private void PaginaInicial_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (e.CloseReason == CloseReason.UserClosing)
            {
                e.Cancel = true;   // Cancela o fechamento
                this.Hide();       // Apenas esconde o formulário
                // Não precisa mudar WindowState ou ShowInTaskbar aqui
            }
        }

        private void IniciarMonitoramento()
        {
            string downloadsPath = Path.Combine(
                   Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
                   "Downloads"
               );

            watcher = new FileSystemWatcher(downloadsPath)
            {
                Filter = "*.*", // todos os arquivos
                NotifyFilter = NotifyFilters.FileName | NotifyFilters.LastWrite
            };

            watcher.Renamed += async (s, e) =>
            {
                if (e.Name.Contains("SOPHOS-WEB", StringComparison.OrdinalIgnoreCase) &&
                    Path.GetExtension(e.FullPath).Equals(".json", StringComparison.OrdinalIgnoreCase))
                {
                        await Task.Delay(500); // espera terminar o download
                    try
                    {
                        string conteudo = File.ReadAllText(e.FullPath, Encoding.UTF8);
                        await _impressaoService.Imprimir(conteudo, "SOPHOS");
                        File.Delete(e.FullPath);
                    }
                    catch (Exception ex)
                    {
                        Console.Write(ex.ToString());
                    }
                }
            };

            watcher.EnableRaisingEvents = true;
        }


    }
}
