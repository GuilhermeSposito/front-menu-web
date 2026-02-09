using Microsoft.EntityFrameworkCore;
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

namespace SophosSyncDesktop.Views;

public partial class ConfigsGeral : Form
{
    public ConfigsGeral()
    {
        InitializeComponent();
        LoadConfigs();
    }

    public async Task LoadConfigs()
    {
        try
        {
            using (AppDbContext dbContext = new AppDbContext())
            {
                var configs = await dbContext.Impressoras.FirstOrDefaultAsync();

                CaminhoDoSalvamentoDoJson.Text = configs?.CaminhoSalvamentoDoJson;
                CaminhoParaPastaDeArqNfe.Text = configs?.CaminhoSalvamentoDasNfe;
            }

        }
        catch (Exception ex)
        {
            MessageBox.Show($"Erro ao carregar as configurações: {ex.Message}", "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }


    #region Funções de Ação do form

    private void BackButton_Click(object sender, EventArgs e)
    {
        this.Close();
    }

    private void pictureBox2_Click(object sender, EventArgs e)
    {
        this.Close();
    }

    private void tabControleConfigs_DrawItem(object sender, DrawItemEventArgs e)
    {
        Graphics g = e.Graphics;
        Brush _textBrush;

        // Define a cor de fundo da guia
        Rectangle _tabBounds = tabControleConfigs.GetTabRect(e.Index);

        // 1. Pinta o fundo total do controle (o quadrado externo)
        using (SolidBrush b = new SolidBrush(ColorTranslator.FromHtml("#0F172A")))
        {
            e.Graphics.FillRectangle(b, tabControleConfigs.ClientRectangle);
        }

        if (e.State == DrawItemState.Selected)
        {
            Color minhaCorHex = ColorTranslator.FromHtml("#192436");
            using (SolidBrush brush = new SolidBrush(minhaCorHex))
            {
                g.FillRectangle(brush, _tabBounds);
            }
        }
        else
        {
            Color minhaCorHex = ColorTranslator.FromHtml("#0F172A");
            using (SolidBrush brush = new SolidBrush(minhaCorHex))
            {
                g.FillRectangle(brush, _tabBounds);
            }
        }

        // Define a cor do texto
        _textBrush = new SolidBrush(Color.White);

        // Desenha o texto
        string _tabName = tabControleConfigs.TabPages[e.Index].Text;
        StringFormat _stringFlags = new StringFormat();
        g.DrawString(_tabName, Font, _textBrush, _tabBounds, _stringFlags);
    }
    #endregion

    private async void CaminhoDoSalvamentoDoJson_TextChanged(object sender, EventArgs e)
    {
        using (AppDbContext dbContext = new AppDbContext())
        {
            var configs = await dbContext.Impressoras.FirstOrDefaultAsync();

            if (configs is not null)
                configs.CaminhoSalvamentoDoJson = CaminhoDoSalvamentoDoJson.Text;

            await dbContext.SaveChangesAsync();

        }
    }

    //Essa picture Box é a de "Selecionar Pasta" do campo "Caminho de Salvamento do Json"
    private async void pictureBox1_Click(object sender, EventArgs e)
    {
        using (FolderBrowserDialog fbd = new FolderBrowserDialog())
        {
            // Opcional: define uma descrição no topo da janela
            fbd.Description = "Selecione a pasta de destino";

            // Opcional: permite ao usuário criar novas pastas pelo seletor
            fbd.ShowNewFolderButton = true;

            // Abre a janela e verifica se o usuário clicou em OK
            if (fbd.ShowDialog() == DialogResult.OK)
            {
                string caminhoSelecionado = fbd.SelectedPath;

                using (AppDbContext dbContext = new AppDbContext())
                {
                    var configs = await dbContext.Impressoras.FirstOrDefaultAsync();

                    if (configs is not null)
                        configs.CaminhoSalvamentoDoJson = caminhoSelecionado;

                    CaminhoDoSalvamentoDoJson.Text = caminhoSelecionado;

                    await dbContext.SaveChangesAsync();

                }
            }
        }
    }

    private async void CaminhoParaPastaDeArqNfe_TextChanged(object sender, EventArgs e)
    {
        using (AppDbContext dbContext = new AppDbContext())
        {
            var configs = await dbContext.Impressoras.FirstOrDefaultAsync();

            if (configs is not null)
                configs.CaminhoSalvamentoDasNfe = CaminhoParaPastaDeArqNfe.Text;

            await dbContext.SaveChangesAsync();

        }
    }

    //Essa picture Box é a de "Selecionar Pasta" do campo "Caminho de Salvamento das Nfe"
    private async void pictureBox3_Click(object sender, EventArgs e)
    {
        using (FolderBrowserDialog fbd = new FolderBrowserDialog())
        {
            // Opcional: define uma descrição no topo da janela
            fbd.Description = "Selecione a pasta de destino";

            // Opcional: permite ao usuário criar novas pastas pelo seletor
            fbd.ShowNewFolderButton = true;

            // Abre a janela e verifica se o usuário clicou em OK
            if (fbd.ShowDialog() == DialogResult.OK)
            {
                string caminhoSelecionado = fbd.SelectedPath;

                using (AppDbContext dbContext = new AppDbContext())
                {
                    var configs = await dbContext.Impressoras.FirstOrDefaultAsync();

                    if (configs is not null)
                        configs.CaminhoSalvamentoDasNfe = caminhoSelecionado;

                    CaminhoParaPastaDeArqNfe.Text = caminhoSelecionado;

                    await dbContext.SaveChangesAsync();

                }
            }
        }
    }
}
