using ApiFiscalMenuWeb.Models;
using ApiFiscalMenuWeb.Models.Dtos;
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
    private readonly IHttpClientFactory _factory;
    public NfService(IHttpClientFactory factory)
    {
        _factory = factory;
    }


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


    public async Task<string> VerificaStatusDoCertificadoDigital(string token)
    {
        Merchant? merchant = await GetMerchantFromNestApi(token);

        if (merchant is null ||  (String.IsNullOrEmpty(merchant.CertificadoBase64) || String.IsNullOrEmpty(merchant.SenhaCertificado)) )
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

        var StatusServico = new StatusServico(xml, config);
        StatusServico.Executar();

        return $"{StatusServico.Result.CStat} {StatusServico.Result.XMotivo}";
    }

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

}
