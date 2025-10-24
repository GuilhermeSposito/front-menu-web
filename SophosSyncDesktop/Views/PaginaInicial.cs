using Microsoft.Win32;
using SophosSyncDesktop.DataBase.Db;
using SophosSyncDesktop.Models;
using SophosSyncDesktop.Services;
using SophosSyncDesktop.Utils;
using System.IO;
using System.Text;
using System.Windows.Forms;

namespace SophosSyncDesktop;

public partial class PaginaInicial : Form
{
    private FileSystemWatcher watcher;
    private readonly ImpressaoService _impressaoService;
    private readonly ClsEstiloComponentes _clsEstiloComponentes = new ClsEstiloComponentes();
    public PaginaInicial(ImpressaoService ImpressaoService)
    {
        AdicionarInicializacao();

        _impressaoService = ImpressaoService;

        InitializeComponent();
        _clsEstiloComponentes.CriaBordaArredondada(panelDeSelects, 24);
        _clsEstiloComponentes.CriaBordaArredondada(panel1, 24);
        _clsEstiloComponentes.CriaBordaArredondada(this, 24);

        AlimentaComboBoxsDeImpressoras(this);
        AlimentaComboBoxDeImpressorasEmAdicionarNovaImpressora(this);

        // Configura o NotifyIcon
        SophosSync.Visible = true;

        var menu = new ContextMenuStrip();
        menu.BackColor = ColorTranslator.FromHtml("#F88113");
        menu.ForeColor = ColorTranslator.FromHtml("#FFFFF");
        menu.Items.Add("Abrir", null, (s, e) => ShowWindow());
        menu.Items.Add("Fechar", null, (s, e) => Application.Exit());
        SophosSync.ContextMenuStrip = menu;

        SophosSync.DoubleClick += (s, e) => ShowWindow();

        IniciarMonitoramento();
    }

    protected override void OnLoad(EventArgs e)
    {
        base.OnLoad(e);

        AdicionarInicializacao();

        SophosSync.BalloonTipTitle = "Sophos Sync";
        SophosSync.BalloonTipText = "O aplicativo foi iniciado com sucesso! E voc� j� est� pronto para imprimir pedidos!";
        SophosSync.BalloonTipIcon = ToolTipIcon.Info;
        SophosSync.ShowBalloonTip(3000); // 3 segundos

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
            this.Hide();       // Apenas esconde o formul�rio
            // N�o precisa mudar WindowState ou ShowInTaskbar aqui
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

    private void pictureBox2_Click(object sender, EventArgs e)
    {
        this.Hide();
    }

    public void AlimentaComboBoxsDeImpressoras(PaginaInicial instancia)
    {
        try
        {
            using (AppDbContext db = new AppDbContext())
            {
                ImpressorasConfigs Imps = db.Impressoras.FirstOrDefault() ?? new ImpressorasConfigs();

                instancia.comboBox1.Text = Imps.ImpressoraCaixa;
                instancia.comboBox6.Text = Imps.ImpressoraAux;
                instancia.comboBox2.Text = Imps.ImpressoraCz1;
                instancia.comboBox3.Text = Imps.ImpressoraCz2;
                instancia.comboBox4.Text = Imps.ImpressoraCz3;
                instancia.comboBox5.Text = Imps.ImpressoraBar;
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show("Erro ao carregar impressoras: " + ex.Message);
        }

    }

    public void AlimentaComboBoxDeImpressorasEmAdicionarNovaImpressora(PaginaInicial instancia)
    {
        List<string> listaDeImpressoras = _clsEstiloComponentes.ListaImpressoras();

        foreach (string imp in listaDeImpressoras)
        {
            instancia.comboBox1.Items.Add(imp);
            instancia.comboBox2.Items.Add(imp);
            instancia.comboBox3.Items.Add(imp);
            instancia.comboBox4.Items.Add(imp);
            instancia.comboBox5.Items.Add(imp);
            instancia.comboBox6.Items.Add(imp);
        }
    }



    private void AdicionarInicializacao()
    {
        try
        {
            string nomeApp = "SophosSync";
            string caminhoExe = Application.ExecutablePath;

            // Abre a chave de Run com permiss�o de escrita
            using (RegistryKey chave = Registry.CurrentUser.OpenSubKey(
                @"SOFTWARE\Microsoft\Windows\CurrentVersion\Run", true))
            {
                if (chave != null)
                {
                    // S� cria se n�o existir
                    object valorExistente = chave.GetValue(nomeApp);
                    if (valorExistente == null || valorExistente.ToString() != caminhoExe)
                    {
                        chave.SetValue(nomeApp, $"\"{caminhoExe}\"");
                        Console.WriteLine("Chave de inicializa��o criada/atualizada.");
                    }
                    else
                    {
                        Console.WriteLine("Chave de inicializa��o j� existe.");
                    }
                }
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show("Erro ao adicionar � inicializa��o: " + ex.Message);
        }
    }

    private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
    {
        string? valorSelecionado = comboBox1.SelectedItem?.ToString();

        if (string.IsNullOrEmpty(valorSelecionado))
            return;

        using (AppDbContext db = new AppDbContext())
        {
            // Verifica se j� existe uma configura��o salva
            var config = db.Impressoras.FirstOrDefault();
            if (config == null)
            {
                // Se n�o existir, cria uma nova
                config = new ImpressorasConfigs { ImpressoraCaixa = valorSelecionado };
                db.Impressoras.Add(config);
            }
            else
            {
                // Atualiza a existente
                config.ImpressoraCaixa = valorSelecionado;
                db.Impressoras.Update(config);
            }

            db.SaveChanges();
        }
    }

    private void comboBox6_SelectedIndexChanged(object sender, EventArgs e)
    {
        string? valorSelecionado = comboBox6.SelectedItem?.ToString();

        if (string.IsNullOrEmpty(valorSelecionado))
            return;

        using (AppDbContext db = new AppDbContext())
        {
            // Verifica se j� existe uma configura��o salva
            var config = db.Impressoras.FirstOrDefault();
            if (config == null)
            {
                // Se n�o existir, cria uma nova
                config = new ImpressorasConfigs { ImpressoraAux = valorSelecionado };
                db.Impressoras.Add(config);
            }
            else
            {
                // Atualiza a existente
                config.ImpressoraAux = valorSelecionado;
                db.Impressoras.Update(config);
            }

            db.SaveChanges();
        }
    }

    private void comboBox3_SelectedIndexChanged(object sender, EventArgs e)
    {
        string? valorSelecionado = comboBox3.SelectedItem?.ToString();

        if (string.IsNullOrEmpty(valorSelecionado))
            return;

        using (AppDbContext db = new AppDbContext())
        {
            // Verifica se j� existe uma configura��o salva
            var config = db.Impressoras.FirstOrDefault();
            if (config == null)
            {
                // Se n�o existir, cria uma nova
                config = new ImpressorasConfigs { ImpressoraCz1 = valorSelecionado };
                db.Impressoras.Add(config);
            }
            else
            {
                // Atualiza a existente
                config.ImpressoraCz1 = valorSelecionado;
                db.Impressoras.Update(config);
            }

            db.SaveChanges();
        }
    }

    private void comboBox2_SelectedIndexChanged(object sender, EventArgs e)
    {
        string? valorSelecionado = comboBox2.SelectedItem?.ToString();

        if (string.IsNullOrEmpty(valorSelecionado))
            return;

        using (AppDbContext db = new AppDbContext())
        {
            // Verifica se j� existe uma configura��o salva
            var config = db.Impressoras.FirstOrDefault();
            if (config == null)
            {
                // Se n�o existir, cria uma nova
                config = new ImpressorasConfigs { ImpressoraCz2 = valorSelecionado };
                db.Impressoras.Add(config);
            }
            else
            {
                // Atualiza a existente
                config.ImpressoraCz2 = valorSelecionado;
                db.Impressoras.Update(config);
            }

            db.SaveChanges();
        }
    }

    private void comboBox4_SelectedIndexChanged(object sender, EventArgs e)
    {
        string? valorSelecionado = comboBox3.SelectedItem?.ToString();

        if (string.IsNullOrEmpty(valorSelecionado))
            return;

        using (AppDbContext db = new AppDbContext())
        {
            // Verifica se j� existe uma configura��o salva
            var config = db.Impressoras.FirstOrDefault();
            if (config == null)
            {
                // Se n�o existir, cria uma nova
                config = new ImpressorasConfigs { ImpressoraCz3 = valorSelecionado };
                db.Impressoras.Add(config);
            }
            else
            {
                // Atualiza a existente
                config.ImpressoraCz3 = valorSelecionado;
                db.Impressoras.Update(config);
            }

            db.SaveChanges();
        }
    }

    private void comboBox5_SelectedIndexChanged(object sender, EventArgs e)
    {
        string? valorSelecionado = comboBox5.SelectedItem?.ToString();

        if (string.IsNullOrEmpty(valorSelecionado))
            return;

        using (AppDbContext db = new AppDbContext())
        {
            // Verifica se j� existe uma configura��o salva
            var config = db.Impressoras.FirstOrDefault();
            if (config == null)
            {
                // Se n�o existir, cria uma nova
                config = new ImpressorasConfigs { ImpressoraBar = valorSelecionado };
                db.Impressoras.Add(config);
            }
            else
            {
                // Atualiza a existente
                config.ImpressoraBar = valorSelecionado;
                db.Impressoras.Update(config);
            }

            db.SaveChanges();
        }
    }
}
