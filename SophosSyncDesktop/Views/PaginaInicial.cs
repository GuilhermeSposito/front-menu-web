using FrontMenuWeb.Models.Merchant;
using FrontMenuWeb.Models.Pedidos;
using Microsoft.Win32;
using SophosSyncDesktop.DataBase.Db;
using SophosSyncDesktop.Models;
using SophosSyncDesktop.Services;
using SophosSyncDesktop.Utils;
using SophosSyncDesktop.Views;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Windows.Forms;

namespace SophosSyncDesktop;

public partial class PaginaInicial : Form
{
    private FileSystemWatcher watcherPedidos;
    private FileSystemWatcher watcherFechamentosMotoboys;
    private FileSystemWatcher watcherFechamentos;
    private FileSystemWatcher WatcherNFs;
    private FileSystemWatcher WatcherMesas;

    private readonly ImpressaoService _impressaoService;
    private readonly WebSocketPedidosService _webSocketService;
    private readonly FilaDeImpressaoService _filaService;
    private readonly ClsEstiloComponentes _clsEstiloComponentes = new ClsEstiloComponentes();
    private System.Timers.Timer _timer;

    // Label criado em código para não exigir alteração no .resx do designer
    private Label _labelWsStatus = new Label();

    public PaginaInicial(ImpressaoService ImpressaoService, WebSocketPedidosService webSocketService, FilaDeImpressaoService filaService)
    {
        _impressaoService = ImpressaoService;
        _webSocketService = webSocketService;
        _filaService = filaService;

        _webSocketService.StatusChanged += OnWebSocketStatusChanged;

        InitializeComponent();
        _clsEstiloComponentes.CriaBordaArredondada(panelDeSelects, 24);
        _clsEstiloComponentes.CriaBordaArredondada(panel1, 24);
        _clsEstiloComponentes.CriaBordaArredondada(this, 24);

        // Configura label de status do WebSocket
        _labelWsStatus.AutoSize = true;
        _labelWsStatus.Font = new Font("Segoe UI", 8f, FontStyle.Regular);
        _labelWsStatus.ForeColor = Color.Gray;
        _labelWsStatus.BackColor = Color.Transparent;
        _labelWsStatus.Text = "WebSocket: aguardando conexão...";
        _labelWsStatus.Location = new Point(8, panel1.Height - 22);
        panel1.Controls.Add(_labelWsStatus);

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
        IniciarMonitoramentoMesa();
        IniciarMonitoramentoDeFechamentoDeCaixa();
        IniciarMonitoramentoDeNfs();
        IniciarMonitoramentoFechamentoDeMotoboy();

    }
    protected override async void OnLoad(EventArgs e)
    {
        base.OnLoad(e);

        using (AppDbContext db = new AppDbContext())
        {
            var infos = db.InfosDeLogin.FirstOrDefault();

            if (infos is not null)
            {
                string? token = await AppState.Login(infos.Email, infos.Senha);
                await AppState.GetMerchantAsync();
            }
        }

        // Só ativa WebSocket e timer de pedidos não impressos se pelo menos
        // um checkbox de impressão automática estiver habilitado
        bool deveAtivarImpressaoAutomatica;
        using (var db = new AppDbContext())
        {
            var config = db.Impressoras.FirstOrDefault();
            deveAtivarImpressaoAutomatica = (config?.ImprimirIfood ?? true) || (config?.ImprimirSophosCardapio ?? true);
        }

        if (deveAtivarImpressaoAutomatica)
        {
            await _webSocketService.ConectarAsync();
            IniciarMonitoramentoImpressaoDePedidosNaoImpressos();
        }

        SophosSync.BalloonTipTitle = "Sophos Sync";
        SophosSync.BalloonTipText = "O aplicativo foi iniciado com sucesso! E você já está pronto para imprimir pedidos!";
        SophosSync.BalloonTipIcon = ToolTipIcon.Info;
        SophosSync.ShowBalloonTip(1000); // 1 segundos

        this.Hide();
    }

    public void IniciarMonitoramentoImpressaoDePedidosNaoImpressos()
    {
        _timer = new System.Timers.Timer(20000);
        _timer.Elapsed += async (s, e) =>
        {
            // O timer só busca pedidos não impressos quando o WebSocket está fora do ar
            // (fallback). Se o WS estiver conectado e recebendo eventos, ele já cuida disso.
            if (_webSocketService.EstaConectado) return;
            await _filaService.BuscarPedidosNaoImpressosAsync();
        };
        _timer.AutoReset = true;
        _timer.Start();
    }

    private void OnWebSocketStatusChanged(bool conectado, string mensagem)
    {
        // Atualiza UI na thread correta (evento vem de thread do socket)
        if (InvokeRequired)
        {
            Invoke(() => OnWebSocketStatusChanged(conectado, mensagem));
            return;
        }

        _labelWsStatus.Text = $"WebSocket: {mensagem}";
        _labelWsStatus.ForeColor = conectado ? Color.LimeGreen : Color.OrangeRed;
        SophosSync.Text = $"Sophos Sync — WS: {mensagem}";
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
            return;
        }

        // Fechamento real (ex: Application.Exit) — desconecta o WebSocket
        _webSocketService.StatusChanged -= OnWebSocketStatusChanged;
        _ = _webSocketService.DesconectarAsync().ContinueWith(_ => _webSocketService.Dispose());
    }
    private void IniciarMonitoramento()
    {
        try
        {
            if (watcherPedidos != null)
            {
                watcherPedidos.EnableRaisingEvents = false; // Para o monitoramento
                watcherPedidos.Renamed -= OnWatcherRenamedLeituraDePedido; // Remove o evento (veja nota abaixo)
                watcherPedidos.Dispose(); // Limpa da mem�ria
            }

            using (AppDbContext db = new AppDbContext())
            {
                var config = db.Impressoras.FirstOrDefault();


                string downloadsPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads");

                if (config?.CaminhoSalvamentoDoJson is not null && config.CaminhoSalvamentoDoJson != "Downloads")
                {
                    downloadsPath = config.CaminhoSalvamentoDoJson;
                }

                watcherPedidos = new FileSystemWatcher(downloadsPath)
                {
                    Filter = "*.*", // todos os arquivos
                    NotifyFilter = NotifyFilters.FileName | NotifyFilters.LastWrite
                };

                watcherPedidos.Renamed += OnWatcherRenamedLeituraDePedido;

                watcherPedidos.EnableRaisingEvents = true;
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show("Erro ao iniciar monitoramento de pedidos, Por favor abra o aplicativo novamente.", "erro", buttons: MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private async void OnWatcherRenamedLeituraDePedido(object sender, RenamedEventArgs e)
    {
        if (e.Name.Contains("SOPHOS-WEB", StringComparison.OrdinalIgnoreCase) &&
            !e.Name.Contains("MESA", StringComparison.OrdinalIgnoreCase) &&
            Path.GetExtension(e.FullPath).Equals(".json", StringComparison.OrdinalIgnoreCase))
        {
            await Task.Delay(100);
            var pedido = (ClsPedido?)null;
            try
            {
                string conteudo = File.ReadAllText(e.FullPath, Encoding.UTF8);
                pedido = JsonSerializer.Deserialize<ClsPedido>(conteudo);

                // Tenta adquirir o slot — se o worker da fila já está imprimindo este pedido, descarta
                if (pedido is not null && !_filaService.TentarReservarImpressao(pedido.Id))
                {
                    File.Delete(e.FullPath);
                    return;
                }

                // Impressão imediata — ação manual do usuário, não vai para a fila
                await _impressaoService.Imprimir(conteudo, "SOPHOS");

                if (pedido is not null)
                {
                    await _filaService.MarcarComoImpressoPublicAsync(pedido);
                    LogLocalService.LogImpressao(pedido.DisplayId ?? pedido.Id.ToString(), "WATCHER");
                }

                File.Delete(e.FullPath);
            }
            catch (Exception ex)
            {
                LogLocalService.LogErro(pedido?.DisplayId ?? "?", "WATCHER", ex.Message);
                Console.WriteLine($"[Watcher] Erro ao imprimir pedido: {ex.Message}");
            }
            finally
            {
                if (pedido is not null)
                    _filaService.LiberarImpressao(pedido.Id);
            }
        }
    }
    private void IniciarMonitoramentoFechamentoDeMotoboy()
    {
        try
        {
            if (watcherFechamentosMotoboys != null)
            {
                watcherFechamentosMotoboys.EnableRaisingEvents = false; // Para o monitoramento
                watcherFechamentosMotoboys.Renamed -= OnWatcherRenamedFechamentoDeMotoboy; // Remove o evento (veja nota abaixo)
                watcherFechamentosMotoboys.Dispose(); // Limpa da mem�ria
            }

            using (AppDbContext db = new AppDbContext())
            {
                var config = db.Impressoras.FirstOrDefault();


                string downloadsPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads");

                if (config?.CaminhoSalvamentoDoJson is not null && config.CaminhoSalvamentoDoJson != "Downloads")
                {
                    downloadsPath = config.CaminhoSalvamentoDoJson;
                }

                watcherFechamentosMotoboys = new FileSystemWatcher(downloadsPath)
                {
                    Filter = "*.*", // todos os arquivos
                    NotifyFilter = NotifyFilters.FileName | NotifyFilters.LastWrite
                };

                watcherFechamentosMotoboys.Renamed += OnWatcherRenamedFechamentoDeMotoboy;

                watcherFechamentosMotoboys.EnableRaisingEvents = true;
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show("Erro ao iniciar monitoramento de pedidos, Por favor abra o aplicativo novamente.", "erro", buttons: MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
    private async void OnWatcherRenamedFechamentoDeMotoboy(object sender, RenamedEventArgs e)
    {
        if (e.Name.Contains("fechamento-motoboys", StringComparison.OrdinalIgnoreCase) &&
            !e.Name.Contains("MESA", StringComparison.OrdinalIgnoreCase) &&
            Path.GetExtension(e.FullPath).Equals(".json", StringComparison.OrdinalIgnoreCase))
        {
            await Task.Delay(100);
            try
            {
                string conteudo = File.ReadAllText(e.FullPath, Encoding.UTF8);
                await _impressaoService.ImprimirFechamentoMotoboy(conteudo);
                File.Delete(e.FullPath);
            }
            catch (Exception ex) { Console.WriteLine("Erro ao imprimir pedido"); }
        }
    }

    private void IniciarMonitoramentoMesa()
    {
        try
        {
            if (WatcherMesas != null)
            {
                WatcherMesas.EnableRaisingEvents = false;
                WatcherMesas.Renamed -= OnWatcherRenamedLeituraDePedidoMesa;
                WatcherMesas.Dispose();
            }

            using (AppDbContext db = new AppDbContext())
            {
                var config = db.Impressoras.FirstOrDefault();

                string downloadsPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads");

                if (config?.CaminhoSalvamentoDoJson is not null && config.CaminhoSalvamentoDoJson != "Downloads")
                {
                    downloadsPath = config.CaminhoSalvamentoDoJson;
                }

                WatcherMesas = new FileSystemWatcher(downloadsPath)
                {
                    Filter = "*.*", // todos os arquivos
                    NotifyFilter = NotifyFilters.FileName | NotifyFilters.LastWrite
                };

                WatcherMesas.Renamed += OnWatcherRenamedLeituraDePedidoMesa;

                WatcherMesas.EnableRaisingEvents = true;
            }
        }
        catch (Exception ex)
        {
            Console.Write(ex.ToString());
        }
    }

    private async void OnWatcherRenamedLeituraDePedidoMesa(object sender, RenamedEventArgs e)
    {
        if (e.Name.Contains("MESA", StringComparison.OrdinalIgnoreCase) &&
               Path.GetExtension(e.FullPath).Equals(".json", StringComparison.OrdinalIgnoreCase))
        {
            await Task.Delay(100); // espera terminar o download
            try
            {
                string conteudo = File.ReadAllText(e.FullPath, Encoding.UTF8);
                await _impressaoService.ImprimirComanda(conteudo, "SOPHOS");
                File.Delete(e.FullPath);
            }
            catch (Exception ex)
            {
                Console.Write(ex.ToString());
            }
        }
    }

    private void IniciarMonitoramentoDeFechamentoDeCaixa()
    {
        try
        {
            if (watcherFechamentos != null)
            {
                watcherFechamentos.EnableRaisingEvents = false;
                watcherFechamentos.Renamed -= OnWatcherRenamedLeituraDeFechamentoDeCaixa;
                watcherFechamentos.Dispose();
            }

            using (AppDbContext db = new AppDbContext())
            {
                var config = db.Impressoras.FirstOrDefault();

                string downloadsPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads");

                if (config?.CaminhoSalvamentoDoJson is not null && config.CaminhoSalvamentoDoJson != "Downloads")
                {
                    downloadsPath = config.CaminhoSalvamentoDoJson;
                }

                watcherFechamentos = new FileSystemWatcher(downloadsPath)
                {
                    Filter = "*.*", // todos os arquivos
                    NotifyFilter = NotifyFilters.FileName | NotifyFilters.LastWrite
                };

                watcherFechamentos.Renamed += OnWatcherRenamedLeituraDeFechamentoDeCaixa;

                watcherFechamentos.EnableRaisingEvents = true;
            }
        }
        catch (Exception ex)
        {
            Console.Write(ex.ToString());
        }

    }
    private async void OnWatcherRenamedLeituraDeFechamentoDeCaixa(object sender, RenamedEventArgs e)
    {
        if (e.Name.Contains("SOPHOS-FECHA", StringComparison.OrdinalIgnoreCase) && Path.GetExtension(e.FullPath).Equals(".json", StringComparison.OrdinalIgnoreCase))
        {
            await Task.Delay(500);
            try
            {
                string conteudo = File.ReadAllText(e.FullPath, Encoding.UTF8);

                if (!String.IsNullOrEmpty(conteudo))
                    await _impressaoService.ImprimirFechamento(conteudo);

                File.Delete(e.FullPath);
            }
            catch (Exception ex)
            {
                Console.Write(ex.ToString());
            }
        }
    }


    private void IniciarMonitoramentoDeNfs()
    {
        try
        {
            if (WatcherNFs != null)
            {
                WatcherNFs.EnableRaisingEvents = false;
                WatcherNFs.Renamed -= OnWatcherRenamedNfes;
                WatcherNFs.Dispose();
            }

            using (AppDbContext db = new AppDbContext())
            {
                var config = db.Impressoras.FirstOrDefault();

                string downloadsPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads");

                if (config?.CaminhoSalvamentoDoJson is not null && config.CaminhoSalvamentoDoJson != "Downloads")
                {
                    downloadsPath = config.CaminhoSalvamentoDoJson;
                }

                WatcherNFs = new FileSystemWatcher(downloadsPath)
                {
                    Filter = "*.*", // todos os arquivos
                    NotifyFilter = NotifyFilters.FileName | NotifyFilters.LastWrite
                };

                WatcherNFs.Renamed += OnWatcherRenamedNfes;

                WatcherNFs.EnableRaisingEvents = true;
            }
        }
        catch (Exception ex)
        {
            Console.Write(ex.ToString());
        }

    }
    private async void OnWatcherRenamedNfes(object sender, RenamedEventArgs e)
    {
        if (e.Name.Contains("-procnfe", StringComparison.OrdinalIgnoreCase) &&
       Path.GetExtension(e.FullPath).Equals(".xml", StringComparison.OrdinalIgnoreCase))
        {
            await Task.Delay(100);
            try
            {
                using (AppDbContext db = new AppDbContext())
                {
                    var config = db.Impressoras.FirstOrDefault();

                    var CaminhoDeSalvamentoDaNfe = @"C:\ArqNfe\Autorizadas";
                    var CaminhoSalvamentoDoJson = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads");

                    if (config is not null)
                    {
                        CaminhoDeSalvamentoDaNfe = config.CaminhoSalvamentoDasNfe ?? CaminhoDeSalvamentoDaNfe;
                    }

                    string mesAno = DateTime.Now.ToString("MM-yyyy");
                    string destino = CaminhoDeSalvamentoDaNfe + @$"\{mesAno}";

                    // cria a pasta caso n�o exista
                    if (!Directory.Exists(destino))
                        Directory.CreateDirectory(destino);

                    string destinoArquivo = Path.Combine(destino, e.Name);

                    // Se j� existir um arquivo com o mesmo nome, sobrescreve
                    if (File.Exists(destinoArquivo))
                        File.Delete(destinoArquivo);

                    File.Move(e.FullPath, destinoArquivo);

                    _impressaoService.ImprimeDANFE(destinoArquivo);

                    if (e.Name.Contains("-copia"))
                    {
                        File.Delete(destinoArquivo);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.Write(ex.ToString());
            }
        }
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
                instancia.comboBox3.Text = Imps.ImpressoraCz1;
                instancia.comboBox2.Text = Imps.ImpressoraCz2;
                instancia.comboBox4.Text = Imps.ImpressoraCz3;
                instancia.comboBox5.Text = Imps.ImpressoraBar;
                instancia.comboBoxImpressoraDanfe.Text = Imps.ImpressoraDanfe;
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
            instancia.comboBoxImpressoraDanfe.Items.Add(imp);
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
    private void btnConfig_Click(object sender, EventArgs e)
    {
        ConfigsGeral teste = new ConfigsGeral();
        teste.ShowDialog();
        IniciarMonitoramento(); // Reinicia o monitoramento para pegar poss�veis mudan�as no caminho de salvamento
        IniciarMonitoramentoMesa();
    }
    private void labeLogin_Click(object sender, EventArgs e)
    {
        LoginForm loginForm = new LoginForm();
        loginForm.ShowDialog();
    }
    private void comboBoxImpressoraDanfe_SelectedIndexChanged(object sender, EventArgs e)
    {
        string? valorSelecionado = comboBoxImpressoraDanfe.SelectedItem?.ToString();

        if (string.IsNullOrEmpty(valorSelecionado))
            return;

        using (AppDbContext db = new AppDbContext())
        {
            // Verifica se j� existe uma configura��o salva
            var config = db.Impressoras.FirstOrDefault();
            if (config == null)
            {
                // Se n�o existir, cria uma nova
                config = new ImpressorasConfigs { ImpressoraDanfe = valorSelecionado };
                db.Impressoras.Add(config);
            }
            else
            {
                // Atualiza a existente
                config.ImpressoraDanfe = valorSelecionado;
                db.Impressoras.Update(config);
            }

            db.SaveChanges();
        }
    }
}
