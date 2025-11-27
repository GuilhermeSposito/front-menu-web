using FrontMenuWeb.Models;
using SophosSyncDesktop.DataBase.Db;
using SophosSyncDesktop.Utils;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SophosSyncDesktop.Views;

public partial class LoginForm : Form
{
    private readonly ClsEstiloComponentes _clsEstiloComponentes = new ClsEstiloComponentes();
    public LoginForm()
    {
        InitializeComponent();
        _clsEstiloComponentes.CriaBordaArredondada(panel1, 24);
    }

    private void bTnCancelar_Click(object sender, EventArgs e)
    {
        this.Close();
    }

    private async void btnEntrar_Click(object sender, EventArgs e)
    {
        try
        {
            string? Token = await SophosSyncDesktop.Models.AppState.Login(textEmail.Text, textSenha.Text);
            if (!string.IsNullOrEmpty(Token))
            {
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            else
            {
                MessageBox.Show("Falha no login. Verifique suas credenciais.", "Erro de Login", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Erro ao efetuar login: {ex.Message}", "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void textEmail_TextChanged(object sender, EventArgs e)
    {
        using AppDbContext db = new AppDbContext();
        var infoLogin = db.InfosDeLogin.FirstOrDefault();
        if (infoLogin != null)
        {
            infoLogin.Email = textEmail.Text;
            db.SaveChanges();
        }
    }

    private void textSenha_TextChanged(object sender, EventArgs e)
    {
        using AppDbContext db = new AppDbContext();
        var infoLogin = db.InfosDeLogin.FirstOrDefault();
        if (infoLogin != null)
        {
            infoLogin.Senha = textSenha.Text;
            db.SaveChanges();
        }
    }

    public void CarregaInfosDeLogin()
    {
        using AppDbContext db = new AppDbContext();
        var infoLogin = db.InfosDeLogin.FirstOrDefault();
        if (infoLogin != null)
        {
            textEmail.Text = infoLogin.Email;
            textSenha.Text = infoLogin.Senha;
        }
    }

    private void LoginForm_Load(object sender, EventArgs e)
    {
       CarregaInfosDeLogin();
    }
}
