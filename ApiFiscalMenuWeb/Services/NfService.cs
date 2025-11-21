using ApiFiscalMenuWeb.Models;
using ApiFiscalMenuWeb.Models.Dtos;
using FrontMenuWeb.Models.Pedidos;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Headers;
using System.Security.Cryptography.X509Certificates;
using System.Text.Json;
using Unimake.Business.DFe.Servicos;
using Unimake.Business.DFe.Servicos.NFe;
using Unimake.Business.DFe.Xml.NFe;
using Unimake.Business.Security;

namespace ApiFiscalMenuWeb.Services;

public class NfService
{
    #region Props
    private readonly IHttpClientFactory _factory;
    public NfService(IHttpClientFactory factory)
    {
        _factory = factory;
    }
    #endregion

    #region Funções de conexão com a Nest API
    /// Recupera os dados do Merchant na Nest API
    public async Task<Merchant?> GetMerchantFromNestApi(string token)
    {
        var client = _factory.CreateClient("ApiAutorizada");
        AdicionaTokenNaRequisicao(client, token);

        var response = await client.GetAsync("merchants/details");
        var content = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
            return null;

        var merchant = JsonSerializer.Deserialize<Merchant>(content);

        return merchant;
    }
    #endregion

    #region Funções de Verificação do Certificado Digital
    /// Verifica o status do certificado digital
    public async Task<RetunApiRefatored> VerificaStatusDoCertificadoDigital(string token)
    {
        Merchant? merchant = await GetMerchantFromNestApi(token);

        if (merchant is null || (String.IsNullOrEmpty(merchant.CertificadoBase64) || String.IsNullOrEmpty(merchant.SenhaCertificado)))
            throw new UnauthorizedAccessException("Certificado ou senha Não informados");

        var CertificadoSelecionado = CarregaCertificadoDigitalBySophos(merchant.CertificadoBase64, merchant.SenhaCertificado);

        var xml = new ConsStatServ
        {
            Versao = "4.00",
            CUF = UFBrasil.SP,
            TpAmb = TipoAmbiente.Homologacao,
        };

        var config = new Configuracao
        {
            TipoDFe = TipoDFe.NFe,
            TipoAmbiente = TipoAmbiente.Homologacao,
            CertificadoDigital = CertificadoSelecionado
        };

        ClsPedido Pedido = new ClsPedido
        {
            Id = 1,
            DisplayId = "0001",
        };

        var StatusServico = new StatusServico(xml, config);
        StatusServico.Executar();

        return new RetunApiRefatored
        {
            message = new List<string>
            {
                $"{StatusServico.Result.CStat} {StatusServico.Result.XMotivo} - {Pedido.DisplayId}"
            },
        };
    }
    #endregion

    #region Funções Auxiliares
    private void AdicionaTokenNaRequisicao(HttpClient client, string token)
    {
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
    }

    private X509Certificate2? CarregaCertificadoDigitalBySophos(string CertificadoEmBase64, string SenhaCertificado)
    {
        byte[] CertificaodByttes = Convert.FromBase64String(CertificadoEmBase64);

        var certificadoService = new CertificadoDigital();
        var CertificadoSelecionado = certificadoService.CarregarCertificadoDigitalA1(CertificaodByttes, SenhaCertificado);

        return CertificadoSelecionado;
    }
    #endregion
}
