using FrontMenuWeb.Models;
using FrontMenuWeb.Models.Merchant;
using SophosSyncDesktop.DataBase.Db;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace SophosSyncDesktop.Models;

public static class AppState
{
    public static ClsMerchant? MerchantLogado { get; set; }
    public static string? Token { get; set; }

    public static async Task<ClsMerchant?> GetMerchantAsync()
    {
        if (!String.IsNullOrEmpty(AppState.Token))
            try
            {
                HttpClient client = new HttpClient();
                client.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", AppState.Token);

                var response = await client.GetAsync("https://syslogicadev.com/api/v1/merchants/details");

                if (response.IsSuccessStatusCode)
                {
                    var conteudoJson = await response.Content.ReadAsStringAsync();
                    if (!string.IsNullOrEmpty(conteudoJson))
                    {
                        ClsMerchant? merchant = JsonSerializer.Deserialize<ClsMerchant>(conteudoJson);
                        if (merchant != null)
                        {
                            AppState.MerchantLogado = merchant;
                        }
                        else
                        {
                            MessageBox.Show("Falha ao desserializar os dados do merchant.");
                        }
                    }
                    else
                    {
                        MessageBox.Show("Resposta inválida do servidor.");
                    }
                }
                else
                {
                    MessageBox.Show("Não foi possível conectar ao servidor. Verifique sua conexão com a internet.");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erro ao iniciar monitoramento: " + ex.Message);
            }

        return AppState.MerchantLogado;
    }

    public static async Task<string?> Login(string email, string senha)
    {
        try
        {
            HttpClient client = new HttpClient();
            LoginModel loginModel = new LoginModel
            {
                Email = email,
                Senha = senha
            };

            var response = await client.PostAsJsonAsync("https://syslogicadev.com/api/v1/auth/login", loginModel);
            if (response != null)
            {
                if (response.IsSuccessStatusCode)
                {
                    var resultString = await response.Content.ReadAsStringAsync();

                    var result = await response.Content.ReadFromJsonAsync<LoginResult>();
                    using AppDbContext db = new AppDbContext();
                    var infoLogin = db.InfosDeLogin.FirstOrDefault();
                    if (infoLogin != null)
                    {
                        infoLogin.Token = result?.Token ?? "";
                        db.SaveChanges();
                    }
                    else
                    {
                        InfosDeLogin novoInfoLogin = new InfosDeLogin
                        {
                            Email = email,
                            Senha = senha,
                            Token = result?.Token ?? ""
                        };
                        db.InfosDeLogin.Add(novoInfoLogin);
                        db.SaveChanges();
                    }


                        SophosSyncDesktop.Models.AppState.Token = result?.Token;
                    await SophosSyncDesktop.Models.AppState.GetMerchantAsync();
                }
                else
                {
                    MessageBox.Show($"Falha no login. Verifique suas credenciais.", "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            else
            {
                MessageBox.Show("Resposta nula do servidor.", "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);

            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Erro ao efetuar login: {ex.Message}", "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        return Token;
    }
}
