using ApiFiscalMenuWeb.Models;
using ApiFiscalMenuWeb.Models.Dtos;
using FrontMenuWeb.Components.Modais.ModaisDeVendas;
using FrontMenuWeb.Models;
using FrontMenuWeb.Models.Financeiro;
using FrontMenuWeb.Models.Fiscal;
using FrontMenuWeb.Models.Merchant;
using FrontMenuWeb.Models.Pedidos;
using FrontMenuWeb.Models.Pessoas;
using FrontMenuWeb.Models.Produtos;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Headers;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Unimake.Business.DFe;
using Unimake.Business.DFe.Servicos;
using Unimake.Business.DFe.Servicos.NFe;
using Unimake.Business.DFe.Xml.EFDReinf;
using Unimake.Business.DFe.Xml.ESocial;
using Unimake.Business.DFe.Xml.NFe;
using Unimake.Business.Security;

namespace ApiFiscalMenuWeb.Services;

public class NfService
{
    #region Props
    private readonly IHttpClientFactory _factory;
    private readonly IBPTServices _ibptServices;
    private ClsMerchant? _merchantAtual { get; set; }
    private string CnpjMerchantAtual { get; set; } = string.Empty;
    private double ValorTotalTribNfAtual = 0;

    public NfService(IHttpClientFactory factory, IBPTServices iBPTServices)
    {
        _factory = factory;
        _ibptServices = iBPTServices;
    }
    #endregion

    #region Funções de conexão com a Nest API
    /// Recupera os dados do Merchant na Nest API
    public async Task<ClsMerchant?> GetMerchantFromNestApi(string token)
    {
        var client = _factory.CreateClient("ApiAutorizada");
        AdicionaTokenNaRequisicao(client, token);

        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(40));

        HttpResponseMessage response;

        try
        {
            response = await client.GetAsync("merchants/details", cts.Token);
        }
        catch (OperationCanceledException)
        {
            throw new TimeoutException("A requisição para 'merchants/details' excedeu o tempo limite.");
        }

        var content = await response.Content.ReadAsStringAsync(cts.Token);

        if (!response.IsSuccessStatusCode)
            return null;

        var merchant = JsonSerializer.Deserialize<ClsMerchant>(content);

        return merchant;
    }

    public async Task<bool> AtualizaMerchantInNestApi(string token, ClsMerchant UpdatedMerchant)
    {
        var client = _factory.CreateClient("ApiAutorizada");
        AdicionaTokenNaRequisicao(client, token);

        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(40));

        HttpResponseMessage response;

        try
        {
            response = await client.PatchAsJsonAsync($"merchants/update/{UpdatedMerchant.Id}", UpdatedMerchant, cts.Token);
            Console.WriteLine(await response.Content.ReadAsStringAsync());
        }
        catch (OperationCanceledException)
        {
            throw new TimeoutException("A requisição para 'atualizar o estabelecimento' excedeu o tempo limite.");
        }

        if (!response.IsSuccessStatusCode)
            return false;

        return true;
    }

    public async Task<bool> CreateRegistroDaNFInNestApi(string token, NfeReturnDto returnDto)
    {
        var client = _factory.CreateClient("ApiAutorizada");
        AdicionaTokenNaRequisicao(client, token);

        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(40));

        HttpResponseMessage response;

        try
        {
            response = await client.PostAsJsonAsync($"nfs", returnDto, cts.Token);
        }
        catch (OperationCanceledException)
        {
            throw new TimeoutException("A requisição para 'Registrar a NF EMITIDA' excedeu o tempo limite.");
        }

        if (!response.IsSuccessStatusCode)
            return false;

        return true;
    }
    #endregion

    #region Funções de Verificação de status
    /// Verifica o status da NFe
    public async Task<ReturnApiRefatored<object>> VerificaStatusDaNFe(string token)
    {
        ClsMerchant? merchant = await GetMerchantFromNestApi(token);

        if (merchant is null || (String.IsNullOrEmpty(merchant.CertificadoBase64) || String.IsNullOrEmpty(merchant.SenhaCertificado)))
            throw new UnauthorizedAccessException("Certificado ou senha não informados");

        //Função auxiliar para carregar o certificado digital
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

        return new ReturnApiRefatored<object>
        {
            Status = "success",
            Data = new Data<object>
            {
                Messages = new List<string> { $"{StatusServico.Result.CStat} - {StatusServico.Result.XMotivo}" }
            }
        };
    }

    public async Task<ReturnApiRefatored<object>> VerificaStatusDaNFCe(string token)
    {
        ClsMerchant? merchant = await GetMerchantFromNestApi(token);

        if (merchant is null || (String.IsNullOrEmpty(merchant.CertificadoBase64) || String.IsNullOrEmpty(merchant.SenhaCertificado)))
            throw new UnauthorizedAccessException("Certificado ou senha não informados");

        //Função auxiliar para carregar o certificado digital
        var CertificadoSelecionado = CarregaCertificadoDigitalBySophos(merchant.CertificadoBase64, merchant.SenhaCertificado);

        var xml = new ConsStatServ
        {
            Versao = "4.00",
            CUF = UFBrasil.SP,
            TpAmb = TipoAmbiente.Producao,
        };

        var config = new Configuracao
        {
            TipoDFe = TipoDFe.NFCe,
            TipoEmissao = TipoEmissao.Normal,
            CertificadoDigital = CertificadoSelecionado
        };

        var StatusServico = new Unimake.Business.DFe.Servicos.NFCe.StatusServico(xml, config);
        StatusServico.Executar();

        return new ReturnApiRefatored<object>
        {
            Status = "success",
            Data = new Data<object>
            {
                Messages = new List<string> { $"{StatusServico.Result.CStat} - {StatusServico.Result.XMotivo}" }
            }
        };
    }


    #endregion

    #region Funções de Criação da NFe/NFCe
    public async Task<ReturnApiRefatored<NfeReturnDto>> EmissaoDeNFe(string token, ClsPedido Pedido)
    {
        ClsMerchant? merchant = await GetMerchantFromNestApi(token);
        if (merchant is null)
            throw new UnauthorizedAccessException("Não permitido a emissão NFe");

        _merchantAtual = merchant;

        #region Verificações
        EnderecoMerchant? enderecoMerchant = merchant.EnderecosMerchant.FirstOrDefault();
        if (enderecoMerchant is null)
            throw new BadHttpRequestException("Nenhum endereço foi fornecido para emissão da NFe");

        DocumentosMerchant? DocumentoMerchant = merchant.Documentos.FirstOrDefault();
        if (DocumentoMerchant is null)
            throw new BadHttpRequestException("Nenhum documento foi fornecido para emissão da NFe");

        CnpjMerchantAtual = LimparCnpj(DocumentoMerchant.Cnpj);

        ClsPessoas Destinatario = Pedido.Cliente ?? throw new BadHttpRequestException("Nenhum cliente foi fornecido para emissão da NFe");
        if (string.IsNullOrEmpty(Destinatario.Endereco?.Rua))
        {
            Destinatario.Endereco = Pedido.Endereco ?? new EnderecoPessoa { Rua = "Sem Logradouro", Numero = "SN", Bairro = "Não Informado" };
        }
        #endregion

        int ProxNmrNfe = merchant.UltimoNmrSerieNFe + 1;
        merchant.UltimoNmrSerieNFe = ProxNmrNfe; //atrubuir novo valor para atualizarmos o banco de dados

        #region Criação do XML da NFe
        EnviNFe xml = await CriaXmlDeNfeSN(Pedido: Pedido, merchant: merchant, enderecoMerchant: enderecoMerchant, DocumentoMerchant: DocumentoMerchant, Destinatario: Destinatario, ProxNmrNfe: ProxNmrNfe); //CriaXmlDeExemplo();
        #endregion
        var configuracao = new Configuracao
        {
            TipoDFe = TipoDFe.NFe,
            TipoAmbiente = TipoAmbiente.Homologacao,
            CertificadoDigital = CarregaCertificadoDigitalBySophos(merchant.CertificadoBase64!, merchant.SenhaCertificado!)
        };
        var autorizacao = new Autorizacao(xml, configuracao);
        autorizacao.Executar();

        if (autorizacao.Result.ProtNFe is null)
            throw new Exception("Erro na emissão da NFe: Protocolo não retornado");

        var ReturnMessage = $"{autorizacao.Result.ProtNFe.InfProt.CStat} - {autorizacao.Result.ProtNFe.InfProt.XMotivo}";

        //autorizacao.GravarXmlDistribuicao(@"C:\SophosCompany\Tributario\Autorizadas"); //Só usar em dev para gravar o XML na pasta

        string? XmlDeDistribuicao = null;
        switch (autorizacao.Result.ProtNFe.InfProt.CStat)
        {
            case 100: //Autorizado o uso da NFe
            case 110: //Uso Denegado //quando o destinatário está na lista de bloqueio
            case 150: //Autorizado o uso da NFe fora de prazo
            case 250: //NF-e esta denegada na base do sefaz
            case 301: //Uso Denegado: irreguralirade fiscal do emitente
            case 302: //Uso denegado> irreguralirade fiscal do destinatario
            case 303: //Uso denegado> destinatario na lista de bloqueio
                var ProcNfe = autorizacao.NfeProcResult.GerarXML();
                XmlDeDistribuicao = ProcNfe.OuterXml;
                break;
            default:
                break;
        }

        var NmrProtocolo = autorizacao.Result.ProtNFe.InfProt.NProt;
        bool AtualizacaoResult = await AtualizaMerchantInNestApi(token, merchant);

        var DataToReturn = new NfeReturnDto
        {
            PedidoCaixaId = Pedido.Id,
            NFTipo = 65,
            ChaveNf = autorizacao.Result.ProtNFe.InfProt.ChNFe,
            Cstat = autorizacao.Result.ProtNFe.InfProt.CStat,
            Xmotivo = autorizacao.Result.ProtNFe.InfProt.XMotivo,
            NmrProtocolo = NmrProtocolo,
            NmrDaNf = ProxNmrNfe,
            XmlStringField = XmlDeDistribuicao
        };

        await CreateRegistroDaNFInNestApi(token, DataToReturn);

        return new ReturnApiRefatored<NfeReturnDto>
        {
            Status = "success",
            Data = new Data<NfeReturnDto>
            {
                Messages = new List<string> { $"{ReturnMessage}" },
                ObjetoWhenWriting = DataToReturn
            }
        };
    }

    public async Task<ReturnApiRefatored<NfeReturnDto>> EmissaoDeNFCe(string token, EnNfCeDto EnvNfceDTO)
    {
        ClsMerchant? merchant = await GetMerchantFromNestApi(token);
        if (merchant is null)
            throw new UnauthorizedAccessException("Não permitido a emissão NFCe");

        _merchantAtual = merchant;
        #region Verificações
        ClsPedido Pedido = EnvNfceDTO.Pedido;

        EnderecoMerchant? enderecoMerchant = merchant.EnderecosMerchant.FirstOrDefault();
        if (enderecoMerchant is null)
            throw new BadHttpRequestException("Nenhum endereço foi fornecido para emissão da NFe");

        DocumentosMerchant? DocumentoMerchant = merchant.Documentos.FirstOrDefault();
        if (DocumentoMerchant is null)
            throw new BadHttpRequestException("Nenhum documento foi fornecido para emissão da NFe");

        int ProxNmrNFCe = merchant.UltimoNmrSerieNFCe + 1;
        merchant.UltimoNmrSerieNFCe = ProxNmrNFCe; //atrubuir novo valor para atualizarmos o banco de dados

        ClsPessoas? Destinatario = Pedido.Cliente;
        #endregion
        CnpjMerchantAtual = LimparCnpj(DocumentoMerchant.Cnpj);
        #region Incrementação do numero da NFCe
        int ProxNmrNfe = merchant.UltimoNmrSerieNFCe + 1;
        merchant.UltimoNmrSerieNFCe = ProxNmrNfe; //atrubuir novo valor para atualizarmos o banco de dados
        #endregion

        var xml = CriaXmlDeNFCeSN(merchant, enderecoMerchant, DocumentoMerchant, EnvNfceDTO, ProxNmrNFCe);

        Console.WriteLine(xml.Result.GerarXML());

        var configuracao = new Configuracao
        {
            TipoDFe = TipoDFe.NFCe,
            TipoAmbiente = TipoAmbiente.Homologacao,
            CertificadoDigital = CarregaCertificadoDigitalBySophos(merchant.CertificadoBase64!, merchant.SenhaCertificado!),
            CSC = "b3aedaf9-8544-4664-a391-e4b360b58b72",
            CSCIDToken = 1
        };

        var autorizacao = new Unimake.Business.DFe.Servicos.NFCe.Autorizacao(xml.Result, configuracao);
        autorizacao.Executar();

        if (autorizacao.Result.ProtNFe is null)
            throw new Exception("Erro na emissão da NFCe: Protocolo não retornado");

        var ReturnMessage = $"{autorizacao.Result.ProtNFe.InfProt.CStat} - {autorizacao.Result.ProtNFe.InfProt.XMotivo}";

       //autorizacao.GravarXmlDistribuicao(@"C:\SophosCompany\Tributario\Autorizadas"); //Só usar em dev para gravar o XML na pasta 

        string? XmlDeDistribuicao = null;
        switch (autorizacao.Result.ProtNFe.InfProt.CStat)
        {
            case 100: //Autorizado o uso da NFe
            case 110: //Uso Denegado //quando o destinatário está na lista de bloqueio
            case 150: //Autorizado o uso da NFe fora de prazo
            case 250: //NFC-e esta denegada na base do sefaz
            case 301: //Uso Denegado: irreguralirade fiscal do emitente
            case 302: //Uso denegado> irreguralirade fiscal do destinatario
            case 303: //Uso denegado> destinatario na lista de bloqueio
                var ProcNfe = autorizacao.NfeProcResult.GerarXML();
                XmlDeDistribuicao = ProcNfe.OuterXml;
                break;
            default:
                break;
        }

        var NmrProtocolo = autorizacao.Result.ProtNFe.InfProt.NProt;
        bool AtualizacaoResult = await AtualizaMerchantInNestApi(token, merchant);

        var DataToReturn = new NfeReturnDto
        {
            PedidoCaixaId = Pedido.Id,
            NFTipo = 55,
            ChaveNf = autorizacao.Result.ProtNFe.InfProt.ChNFe,
            Cstat = autorizacao.Result.ProtNFe.InfProt.CStat,
            Xmotivo = autorizacao.Result.ProtNFe.InfProt.XMotivo,
            NmrProtocolo = NmrProtocolo,
            NmrDaNf = ProxNmrNfe,
            XmlStringField = XmlDeDistribuicao
        };

        await CreateRegistroDaNFInNestApi(token, DataToReturn);

        return new ReturnApiRefatored<NfeReturnDto>
        {
            Status = "success",
            Data = new Data<NfeReturnDto>
            {
                Messages = new List<string> { $"{ReturnMessage}" },
                ObjetoWhenWriting = DataToReturn
            }
        };
    }
    #endregion

    #region Funções Auxiliares

    private async Task<EnviNFe> CriaXmlDeNFCeSN(ClsMerchant merchant, EnderecoMerchant enderecoMerchant, DocumentosMerchant DocumentoMerchant, EnNfCeDto enNfCeDto, int ProxNumeroNFCe)
    {
        return new EnviNFe
        {
            Versao = "4.00",
            IdLote = "000000000000001",
            IndSinc = SimNao.Sim,
            NFe = new List<NFe>
            {
                new NFe
                {
                    InfNFe =  new List<InfNFe> //Lista de InfNFe --> Porém só tem 1 NFe por vez
                    {
                        new InfNFe
                        {
                            Versao = "4.00",
                            Ide = new Ide
                            {
                                CUF = UFBrasil.SP,
                                NatOp = "VENDA DE MERCADORIA",
                                Mod = ModeloDFe.NFCe,
                                Serie = 1,
                                NNF = ProxNumeroNFCe,
                                DhEmi = DateTime.Now,
                                DhSaiEnt = DateTime.Now,
                                TpNF = TipoOperacao.Saida,
                                IdDest = DestinoOperacao.OperacaoInterna,
                                CMunFG = enderecoMerchant.Cidade.NumCidade,
                                TpImp = FormatoImpressaoDANFE.NFCe,
                                TpEmis = TipoEmissao.Normal,
                                TpAmb = TipoAmbiente.Homologacao,
                                FinNFe = FinalidadeNFe.Normal,
                                IndFinal = SimNao.Sim,
                                IndPres = IndicadorPresenca.OperacaoPresencial,
                                ProcEmi = ProcessoEmissao.AplicativoContribuinte,
                                VerProc = $"Teste"
                            },
                            Emit = new Emit //Tag de Emitente
                            {
                                CNPJ = LimparCnpj(DocumentoMerchant.Cnpj),
                                XNome = merchant.RazaoSocial,
                                XFant = merchant.NomeFantasia,
                                EnderEmit = new EnderEmit
                                {
                                    XLgr = enderecoMerchant.Rua,
                                    Nro = enderecoMerchant.Numero,
                                    XBairro = enderecoMerchant.Bairro,
                                    CMun = enderecoMerchant.Cidade.NumCidade,
                                    XMun = enderecoMerchant.Cidade.Descricao,
                                    UF = UFBrasil.SP,
                                    CEP = enderecoMerchant.Cep,
                                },
                                IE = DocumentoMerchant.IncricaoEstadual,
                                IM = DocumentoMerchant.InscricaoMunicipal,
                                CNAE = DocumentoMerchant.Cnae,
                                CRT = CRT.SimplesNacional
                            },
                            Dest = RetornaDestinatarioDeNFCe(enNfCeDto, enNfCeDto.Pedido.Cliente),
                            Det =  await RetornaDetsDosProdutosNoPedido(ItensDoPedido:enNfCeDto.Pedido.Itens, Pedido: enNfCeDto.Pedido, TipoDFe.NFCe), //Itens do pedido
                            Total = new Total
                            {
                               ICMSTot = new ICMSTot
                               {
                                    VBC = 0.00,
                                    VICMS = 0.00,
                                    VICMSDeson = 0.00,
                                    VFCP = 0.00,
                                    VFCPST = 0.00,
                                    VFCPSTRet = 0.00,
                                    VBCST = 0.00,
                                    VProd = Convert.ToDouble(enNfCeDto.Pedido.ValorDosItens),
                                    VFrete = 0.00,
                                    VSeg = 0.00,
                                    VDesc = 0.00,
                                    VII = 0.00,
                                    VIPI = 0.00,
                                    VIPIDevol = 0.00,
                                    VPIS = 0.00,
                                    VCOFINS = 0.00,
                                    VOutro = Convert.ToDouble(enNfCeDto.Pedido.TaxaEntregaValor + enNfCeDto.Pedido.AcrescimoValor + enNfCeDto.Pedido.ServicoValor), 
                                    VNF = Convert.ToDouble(enNfCeDto.Pedido.ValorTotal),
                                    VTotTrib = ValorTotalTribNfAtual
                               }
                            },         
                            Transp = RetornaTransportedaNF(enNfCeDto.Pedido, merchant, DocumentoMerchant, enderecoMerchant, TipoDFe.NFCe),
                            Pag = new Pag
                            {
                                DetPag = ReturnInfosDePags(Pedido: enNfCeDto.Pedido)
                            },
                            InfAdic = new InfAdic
                            {
                                InfCpl = "NFCE emitida para teste"
                            }
                        }
                    }
                }
            }
        };
    }

    private Transp RetornaTransportedaNF(ClsPedido Pedido, ClsMerchant merchant, DocumentosMerchant DocumentoMerchant, EnderecoMerchant enderecoMerchant, TipoDFe? tipoNFE = TipoDFe.NFe)
    {
        if (Pedido.TipoDePedido == "DELIVERY" && tipoNFE != TipoDFe.NFCe)
        {
            return new Transp
            {
                ModFrete = ModalidadeFrete.ContratacaoFretePorContaRemetente_CIF,
                Transporta = new Transporta
                {
                    CNPJ = LimparCnpj(DocumentoMerchant.Cnpj),
                    XNome = merchant.RazaoSocial,
                    IE = DocumentoMerchant.IncricaoEstadual,
                    XEnder = enderecoMerchant.Rua,
                    XMun = enderecoMerchant.Cidade.Descricao,
                    UF = UFBrasil.SP
                },
                Vol = new List<Vol>
                {
                     new Vol
                     {
                        QVol = 1,
                        Esp = "Caixa",
                        Marca = "Marca Teste",
                        NVol = "0",
                     }
                }
            };
        }
        else
        {
            return new Transp
            {
                ModFrete = ModalidadeFrete.SemOcorrenciaTransporte
            };
        }
    }

    private Dest? RetornaDestinatarioDeNFCe(EnNfCeDto enNfCeDto, ClsPessoas? Cliente)
    {
        if (enNfCeDto.CPF is not null)
        {
            return new Dest
            {
                CPF = LimparCnpj(enNfCeDto.CPF),
                XNome = "NF-E EMITIDA EM AMBIENTE DE HOMOLOGACAO - SEM VALOR FISCAL",//enNfCeDto.NomeCliente ?? "CONSUMIDOR FINAL",
                IndIEDest = IndicadorIEDestinatario.NaoContribuinte,
            };
        }
        else
        {
            //implementar o resto depois;
        }

        return null;
    }

    private async Task<EnviNFe> CriaXmlDeNfeSN(ClsPedido Pedido, ClsMerchant merchant, EnderecoMerchant enderecoMerchant, DocumentosMerchant DocumentoMerchant, ClsPessoas Destinatario, int ProxNmrNfe)
    {
        return new EnviNFe
        {
            Versao = "4.00",
            IdLote = "000000000000001",
            IndSinc = SimNao.Sim,
            NFe = new List<NFe> //Infos da Nfe

            {
                new NFe
                {
                    InfNFe =  new List<InfNFe> //Lista de InfNFe --> Porém só tem 1 NFe por vez
                    {
                        new InfNFe
                        {
                            Versao = "4.00",
                            Ide = new Ide
                            {
                                CUF = UFBrasil.SP,
                                NatOp = "VENDA DE MERCADORIA",
                                Mod = ModeloDFe.NFe,
                                Serie = 1,
                                NNF = ProxNmrNfe,
                                DhEmi = DateTime.Now,
                                DhSaiEnt = DateTime.Now,
                                TpNF = TipoOperacao.Saida,
                                IdDest = DestinoOperacao.OperacaoInterna,
                                CMunFG = enderecoMerchant.Cidade.NumCidade,
                                TpImp = FormatoImpressaoDANFE.NormalRetrato,
                                TpEmis = TipoEmissao.Normal,
                                TpAmb = TipoAmbiente.Homologacao,
                                FinNFe = FinalidadeNFe.Normal,
                                IndFinal = SimNao.Sim,
                                IndPres = IndicadorPresenca.OperacaoPresencial,
                                ProcEmi = ProcessoEmissao.AplicativoContribuinte,
                                VerProc = $"Teste 123"
                            },
                            Emit = new Emit //Tag de Emitente
                            {
                                CNPJ = LimparCnpj(DocumentoMerchant.Cnpj),
                                XNome = merchant.RazaoSocial,
                                XFant = merchant.NomeFantasia,
                                EnderEmit = new EnderEmit
                                {
                                    XLgr = enderecoMerchant.Rua,
                                    Nro = enderecoMerchant.Numero,
                                    XBairro = enderecoMerchant.Bairro,
                                    CMun = enderecoMerchant.Cidade.NumCidade,
                                    XMun = enderecoMerchant.Cidade.Descricao,
                                    UF = UFBrasil.SP,
                                    CEP = enderecoMerchant.Cep,
                                },
                                IE = DocumentoMerchant.IncricaoEstadual,
                                IM = DocumentoMerchant.InscricaoMunicipal,
                                CNAE = DocumentoMerchant.Cnae,
                                CRT = CRT.SimplesNacional
                            },
                            Dest = new Dest // Tag de Destinatário
                            {
                                CNPJ = LimparCnpj(Destinatario.Cnpj!),
                                XNome = "NF-E EMITIDA EM AMBIENTE DE HOMOLOGACAO - SEM VALOR FISCAL",// Destinatario.Nome,
                                EnderDest = new EnderDest
                                {
                                    XLgr = Destinatario.Endereco?.Rua,
                                    Nro = Destinatario.Endereco?.Numero,
                                    XBairro = Destinatario.Endereco?.Bairro,
                                    CMun = enderecoMerchant.Cidade.NumCidade,
                                    XMun = enderecoMerchant.Cidade.Descricao,
                                    UF = UFBrasil.SP,
                                    CEP = Destinatario.Endereco?.Cep,
                                },
                                IndIEDest = IndicadorIEDestinatario.ContribuinteICMS,

                                IE = Destinatario.InscricaoEstadual,
                                Email = "guilhermesposito14@gmail.com"

                            },
                            Det = await RetornaDetsDosProdutosNoPedido(ItensDoPedido:Pedido.Itens, Pedido: Pedido), //Itens do pedido
                            Total = new Total
                            {
                               ICMSTot = new ICMSTot
                               {
                                    VBC = 0.00,
                                    VICMS = 0.00,
                                    VICMSDeson = 0.00,
                                    VFCP = 0.00,
                                    VFCPST = 0.00,
                                    VFCPSTRet = 0.00,
                                    VBCST = 0.00,
                                    VProd = Convert.ToDouble(Pedido.ValorDosItens + Pedido.AcrescimoValor + Pedido.ServicoValor),
                                    VFrete = Convert.ToDouble(Pedido.TaxaEntregaValor),
                                    VSeg = 0.00,
                                    VDesc = 0.00,
                                    VII = 0.00,
                                    VIPI = 0.00,
                                    VIPIDevol = 0.00,
                                    VPIS = 0.00,
                                    VCOFINS = 0.00,
                                    VOutro = 0.00,
                                    VNF = Convert.ToDouble(Pedido.ValorTotal),
                                    VTotTrib = ValorTotalTribNfAtual
                               }
                            },
                            Transp = new Transp
                            {
                                ModFrete = ModalidadeFrete.ContratacaoFretePorContaRemetente_CIF,
                                Transporta = new Transporta
                                {
                                    CNPJ = LimparCnpj(DocumentoMerchant.Cnpj),
                                    XNome = merchant.RazaoSocial,
                                    IE = DocumentoMerchant.IncricaoEstadual,
                                    XEnder = enderecoMerchant.Rua,
                                    XMun = enderecoMerchant.Cidade.Descricao,
                                    UF = UFBrasil.SP
                                },
                                Vol = new List<Vol>
                                {
                                    new Vol
                                    {
                                        QVol = 1,
                                        Esp = "Caixa",
                                        Marca = "Marca Teste",
                                        NVol = "0",
                                    }
                                }
                            },
                            Cobr = new Cobr
                            {
                                Fat = new Fat
                                {
                                    NFat = "0001",
                                    VOrig = Pedido.ValorTotal,
                                    VDesc = 0.00,
                                    VLiq = Pedido.ValorTotal
                                },
                            },
                            Pag = new Pag
                            {
                                DetPag = ReturnInfosDePags(Pedido: Pedido)
                            },
                            InfAdic = new InfAdic
                            {
                                InfCpl = "NFE emitida para teste"
                            }
                        },
                    }
                }
            }
        };
    }

    private List<DetPag> ReturnInfosDePags(ClsPedido Pedido)
    {
        var pags = new List<DetPag>();

        foreach (var pag in Pedido.Pagamentos)
        {
            var detPag = new DetPag
            {
                IndPag = IndicadorPagamento.PagamentoVista,
                TPag = RetornaIndicadorDePagamento(pag.FormaDePagamento),
                VPag = pag.ValorTotal,
                Card = new Card
                {
                    TpIntegra = TipoIntegracaoPagamento.PagamentoNaoIntegrado
                }
            };

            pags.Add(detPag);
        }

        return pags;
    }

    private MeioPagamento RetornaIndicadorDePagamento(ClsFormaDeRecebimento Forma)
    {
        if (Forma is null)
            return MeioPagamento.Dinheiro;

        if (Forma.EDEbito)
            return MeioPagamento.CartaoDebito;

        if (Forma.ECredito)
            return MeioPagamento.CartaoCredito;

        if (Forma.EDinheiro)
            return MeioPagamento.Dinheiro;

        if (Forma.EPix)
            return MeioPagamento.PagamentoInstantaneo;

        if (Forma.Descricao.Contains("boleto", StringComparison.OrdinalIgnoreCase))
            return MeioPagamento.BoletoBancario;

        return MeioPagamento.Outros;
    }

    private async Task<List<Det>> RetornaDetsDosProdutosNoPedido(List<ItensPedido> ItensDoPedido, ClsPedido Pedido, TipoDFe? tipoNFE = TipoDFe.NFe)
    {

        var Dets = new List<Det>();

        double ValorFreteDiluidoPorItem = Pedido.TaxaEntregaValor / ItensDoPedido.Count;
        double ValorOutrosDiluidoPorItem = (Pedido.AcrescimoValor + Pedido.ServicoValor) / ItensDoPedido.Count;

        if (tipoNFE == TipoDFe.NFCe)
        {
            ValorFreteDiluidoPorItem = 0.00; //NFC-e não aceita frete
            ValorOutrosDiluidoPorItem = (Pedido.TaxaEntregaValor + Pedido.AcrescimoValor + Pedido.ServicoValor) / ItensDoPedido.Count;
        }

        int ContadorItem = 1;
        foreach (var item in ItensDoPedido)
        {
            if (item.Produto is null)
                continue;

            if (item.Produto.NCM is null)
                continue;

            var valorTotalTrib = await _ibptServices.GetIBPTValor
                        (cnpj: CnpjMerchantAtual, ncm: item.Produto.NCM, uf: "SP", descricao: item.Produto.Descricao, item.PrecoTotal);

            ValorTotalTribNfAtual += valorTotalTrib;

            var Det = new Det
            {
                NItem = ContadorItem,
                Prod = new Prod
                {
                    CProd = item.Produto.CodigoInterno,
                    CEAN = String.IsNullOrEmpty(item.Produto.CodBarras) ? "SEM GTIN" : item.Produto.CodBarras,
                    XProd = "NOTA FISCAL EMITIDA EM AMBIENTE DE HOMOLOGACAO - SEM VALOR FISCAL", //item.Descricao,
                    NCM = item.Produto.NCM,
                    CFOP = item.Produto.csosn == "500" ? "5405" : "5101",
                    UCom = "UN",
                    QCom = Convert.ToDecimal(item.Quantidade),
                    VUnCom = Convert.ToDecimal(item.PrecoUnitario),
                    VProd = Convert.ToDouble(item.PrecoTotal),
                    VFrete = ValorFreteDiluidoPorItem,
                    VOutro =  ValorOutrosDiluidoPorItem,
                    CEANTrib = String.IsNullOrEmpty(item.Produto.CodBarras) ? "SEM GTIN" : item.Produto.CodBarras,
                    UTrib = "UN",
                    QTrib = Convert.ToDecimal(item.Quantidade),
                    VUnTrib = Convert.ToDecimal(item.PrecoUnitario),
                    IndTot = SimNao.Sim,
                    NItemPed = ContadorItem.ToString()
                },
                Imposto = new Imposto
                {
                    VTotTrib = valorTotalTrib,
                    ICMS = CriarICMS(item, Pedido),
                    PIS = new PIS
                    {
                        PISOutr = new PISOutr
                        {
                            CST = "99",
                            VBC = 0.00,
                            PPIS = 0.00,
                            VPIS = 0.00
                        }
                    },
                    COFINS = new COFINS
                    {
                        COFINSOutr = new COFINSOutr
                        {
                            CST = "99",
                            VBC = 0.00,
                            PCOFINS = 0.00,
                            VCOFINS = 0.00
                        }
                    }
                }
            };

            Dets.Add(Det);
            ContadorItem++;
        }

        return Dets;
    }


    #region Funções de criação de ICMS dinamico
    public ICMS CriarICMS(ItensPedido prod, ClsPedido Pedido)
    {
        //cod 1 e 2 e 4 é simples nacional 4 é mei  --> CRT é o codigo do regime tributario

        var EmpresaOptanteSimplesNacional = true; //mudar depois para pegar do merchant

        if (prod.Produto is not null)
            if (EmpresaOptanteSimplesNacional)
            {
                switch (prod.Produto.csosn) //codigo de situação da operação do simples nacional
                {
                    case "500":
                        return CriarICMSN00(prod.Produto, prod, Pedido); //cria esse quando é Simples Nacional
                    case "101":
                        return CriarICM101(prod.Produto, prod, Pedido);
                    //Idependente de qual seja o código 102, 103, 300 ou 400, a criação do ICMS é a mesma
                    case "102":
                    case "103":
                    case "300":
                    case "400":
                        return CriarICM102(prod.Produto, prod, Pedido);
                    default:
                        throw new Exception($"CST/CSOSN não suportado: {prod.Produto.csosn}");
                }
            }
            else
            {
                switch (prod.Produto.CST) //codigo situação tributaria
                {
                    case "60":
                        return CriarICMS00(prod.Produto, prod, Pedido);
                    default:
                        throw new Exception($"CST/CSOSN não suportado: {prod.Produto.CST}");
                }

            }
        else
        {
            throw new Exception("Produto não encontrado para criação do ICMS");
        }
    }



    private ICMS CriarICM900(ClsProduto item, ItensPedido ItemRefNoPedido, ClsPedido Pedido)
    {
        return new ICMS
        {
            ICMSSN900 = new ICMSSN900
            {
                Orig = DefineOrigemDaMercadoria(item.OrigemProduto!),
                CSOSN = item.csosn!,
                PCredSN = 6.00, //Colocar o percentual de credito do simples nacional (Que a empresa usa) (Depende do cnae e do faturamento)
                VCredICMSSN = 0.00
            }
        };
    }

    private ICMS CriarICM101(ClsProduto item, ItensPedido ItemRefNoPedido, ClsPedido Pedido)
    {
        return new ICMS
        {
            ICMSSN101 = new ICMSSN101
            {
                Orig = DefineOrigemDaMercadoria(item.OrigemProduto!),
                CSOSN = item.csosn!,
                PCredSN = 6.00, //Colocar o percentual de credito do simples nacional (Que a empresa usa) (Depende do cnae e do faturamento)
                VCredICMSSN = CalculaValorDoIcmsDoProduto(ItemRefNoPedido, Pedido)
            }
        };
    }

    private ICMS CriarICMS00(ClsProduto item, ItensPedido ItemRefNoPedido, ClsPedido Pedido)
    {
        return new ICMS
        {
            ICMS00 = new ICMS00
            {
                Orig = DefineOrigemDaMercadoria(item.OrigemProduto!),
                CST = item.CST!,
                VICMS = 0.00,
                PICMS = 0.00,
                VBC = 0.00,
                PFCP = 0.00,
                VFCP = 0.00
            }
        };
    }

    private ICMS CriarICMSN00(ClsProduto item, ItensPedido ItemRefNoPedido, ClsPedido Pedido)
    {
        return new ICMS
        {
            ICMSSN500 = new ICMSSN500
            {
                Orig = DefineOrigemDaMercadoria(item.OrigemProduto!),
                CSOSN = item.csosn!,
            }
        };
    }

    private ICMS CriarICM102(ClsProduto item, ItensPedido ItemRefNoPedido, ClsPedido Pedido)
    {
        return new ICMS
        {
            ICMSSN102 = new ICMSSN102
            {
                Orig = DefineOrigemDaMercadoria(item.OrigemProduto!),
                CSOSN = item.csosn!,
            }
        };
    }

    #endregion

    private double CalculaValorDoIcmsDoProduto(ItensPedido item, ClsPedido Pedido)
    {
        var BaseDeCalculoDoProduto = CalculaBaseDeCalculoDoProduto(item, Pedido);
        if (item.Produto is null)
            throw new Exception("Produto não encontrado para cálculo do ICMS");

        double PercentualIcms = Convert.ToDouble(item.Produto.Aliquota.Icms);
        double ValorIcms = (BaseDeCalculoDoProduto * (PercentualIcms / 100));
        return ValorIcms;
    }

    private double CalculaBaseDeCalculoDoProduto(ItensPedido item, ClsPedido Pedido)
    {
        //Base de calculo do produto é ==> ValorDoItem + Frete(Fracionado) + Seguro + OutrasDespesas(Fracionada) - Desconto(Fracionado)
        double ValorDoItem = item.PrecoTotal;
        double FreteFracionado = Pedido.TaxaEntregaValor / Pedido.Itens.Count;
        double Seguro = 0.00;
        double OutrasDespesasFracionada = 0 / Pedido.Itens.Count;
        double DescontoFracionado = Pedido.DescontoValor / Pedido.Itens.Count;
        double BaseDeCalculo = ValorDoItem + FreteFracionado + Seguro + OutrasDespesasFracionada - DescontoFracionado;
        return BaseDeCalculo;
    }

    private OrigemMercadoria DefineOrigemDaMercadoria(string CodDaOrigem)
    {
        switch (CodDaOrigem)
        {
            case "0":
                return OrigemMercadoria.Nacional;
            case "1":
                return OrigemMercadoria.Estrangeira;
            case "2":
                return OrigemMercadoria.Estrangeira2;
            case "3":
                return OrigemMercadoria.Nacional3;
            case "4":
                return OrigemMercadoria.Nacional4;
            case "5":
                return OrigemMercadoria.Nacional5;
            case "6":
                return OrigemMercadoria.Estrangeira6;
            case "7":
                return OrigemMercadoria.Estrangeira7;
            default:
                return OrigemMercadoria.Nacional;
        }
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

    public static string LimparCnpj(string cnpj)
    {
        return new string(cnpj.Where(char.IsDigit).ToArray());
    }

    private EnviNFe CriaXmlDeExemplo() //função temporario para teste
    {
        return new EnviNFe
        {
            Versao = "4.00",
            IdLote = "000000000000001",
            IndSinc = SimNao.Sim,
            NFe = new List<NFe> //Infos da Nfe
            {
                new NFe
                {
                    InfNFe =  new List<InfNFe> //Lista de InfNFe --> Porém só tem 1 NFe por vez
                    {
                        new InfNFe
                        {
                            Versao = "4.00",
                            Ide = new Ide
                            {
                                CUF = UFBrasil.SP,
                                NatOp = "VENDA",
                                Mod = ModeloDFe.NFe,
                                Serie = 1,
                                NNF = 7,
                                DhEmi = DateTime.Now,
                                DhSaiEnt = DateTime.Now,
                                TpNF = TipoOperacao.Saida,
                                IdDest = DestinoOperacao.OperacaoInterna,
                                CMunFG = 3548906,
                                TpImp = FormatoImpressaoDANFE.NormalRetrato,
                                TpEmis = TipoEmissao.Normal,
                                TpAmb = TipoAmbiente.Homologacao,
                                FinNFe = FinalidadeNFe.Normal,
                                IndFinal = SimNao.Sim,
                                IndPres = IndicadorPresenca.OperacaoPresencial,
                                ProcEmi = ProcessoEmissao.AplicativoContribuinte,
                                VerProc = "TESTE 1.00"
                            },
                            Emit = new Emit //Tag de Emitente
                            {
                                CNPJ = "62538536000112",
                                XNome = "Sophos Aplicativos e Tecnologia LTDA",
                                XFant = "Sophos Apps",
                                EnderEmit = new EnderEmit
                                {
                                    XLgr = "Rua Miguel Petroni",
                                    Nro = "2338",
                                    XBairro = "Bandeirantes",
                                    CMun = 3548906,
                                    XMun = "SAO CARLOS",
                                    UF = UFBrasil.SP,
                                    CEP = "13563470",
                                    Fone = "16992366175"
                                },
                                IE = "637769679116",
                                IM = "108158",
                                CNAE = "6201501",
                                CRT = CRT.SimplesNacional
                            },
                            Dest = new Dest // Tag de Destinatário
                            {
                                CPF = "46193939830",
                                XNome = "NF-E EMITIDA EM AMBIENTE DE HOMOLOGACAO - SEM VALOR FISCAL",
                                EnderDest = new EnderDest
                                {
                                    XLgr = "Rua Dr Jonas Novaes",
                                    Nro = "979",
                                    XBairro = "Planalto Paraiso",
                                    CMun = 3548906,
                                    XMun = "SAO CARLOS",
                                    UF = UFBrasil.SP,
                                    CEP = "13562020",
                                },
                                IndIEDest = IndicadorIEDestinatario.NaoContribuinte,
                                Email = "guilhermesposito14@gmail.com"
                            },
                            Det = new List<Det>
                            {
                                new Det
                                {
                                    NItem = 1,
                                    Prod = new Prod
                                    {
                                        CProd = "0001",
                                        CEAN = "SEM GTIN",
                                        XProd = "MARMITEX",
                                        NCM = "21069090",
                                        CFOP = "5102",
                                        UCom = "UN",
                                        QCom = 38.0000M,
                                        VUnCom = 25.5000M,
                                        VProd = 969.00,
                                        CEANTrib = "SEM GTIN",
                                        UTrib = "UN",
                                        QTrib = 38.0000M,
                                        VUnTrib = 25.5000M,
                                        IndTot = SimNao.Sim,
                                        NItemPed = "1",
                                        XPed = "pedido 203",
                                        VFrete = 7.50
                                    },
                                    Imposto = new Imposto
                                    {
                                        VTotTrib = 108.53,
                                        ICMS = new ICMS
                                        {
                                            ICMSSN102 = new ICMSSN102
                                            {
                                                Orig = OrigemMercadoria.Nacional,
                                                CSOSN = "102"
                                            }
                                        },
                                        PIS = new PIS
                                        {
                                            PISOutr = new PISOutr
                                            {
                                                CST = "99",
                                                VBC = 0.00,
                                                PPIS = 0.00,
                                                VPIS = 0.00
                                            }
                                        },
                                        COFINS = new COFINS
                                        {
                                            COFINSOutr = new COFINSOutr
                                            {
                                                CST = "99",
                                                VBC = 0.00,
                                                PCOFINS = 0.00,
                                                VCOFINS = 0.00
                                            }
                                        }
                                    },
                                }
                            }, //Itens do pedido,
                            Total = new Total
                            {
                                ICMSTot = new ICMSTot
                                {
                                    VBC = 0.00,
                                    VICMS = 0.00,
                                    VICMSDeson = 0.00,
                                    VFCP = 0.00,
                                    VFCPST = 0.00,
                                    VFCPSTRet = 0.00,
                                    VBCST = 0.00,
                                    VProd = 969.00,
                                    VFrete = 7.50,
                                    VSeg = 0.00,
                                    VDesc = 0.00,
                                    VII = 0.00,
                                    VIPI = 0.00,
                                    VIPIDevol = 0.00,
                                    VPIS = 0.00,
                                    VCOFINS = 0.00,
                                    VOutro = 0.00,
                                    VNF = 976.50,
                                    VTotTrib = 108.53
                                }
                            },
                            Transp = new Transp
                            {
                                ModFrete = ModalidadeFrete.ContratacaoFretePorContaRemetente_CIF,
                                Transporta = new Transporta
                                {
                                    CNPJ = "62538536000112",
                                    XNome = "Sophos Aplicativos e Tecnologia LTDA",
                                    IE = "637769679116",
                                    XEnder = "Rua Miguel Petroni, 2338",
                                    XMun = "SAO CARLOS",
                                    UF = UFBrasil.SP
                                },
                                Vol = new List<Vol>
                                {
                                    new Vol
                                    {
                                        QVol = 1,
                                        Esp = "Caixa",
                                        Marca = "Marca Teste",
                                        NVol = "0",
                                    }
                                }
                            },
                            Cobr = new Cobr
                            {
                                Fat = new Fat
                                {
                                    NFat = "0001",
                                    VOrig = 976.50,
                                    VDesc = 0.00,
                                    VLiq = 976.50
                                },
                            },
                            Pag = new Pag
                            {
                                DetPag = new List<DetPag>
                                {
                                    new DetPag
                                    {
                                        IndPag = IndicadorPagamento.PagamentoVista,
                                        TPag = MeioPagamento.CartaoCredito,
                                        VPag = 976.50,
                                        Card = new Card
                                        {
                                            TpIntegra = TipoIntegracaoPagamento.PagamentoNaoIntegrado
                                        }
                                    }
                                }
                            },
                            InfAdic = new InfAdic
                            {
                                InfCpl = "NFE emitida para teste"
                            }
                        },
                    }
                }
            }
        };
    }

    #endregion
}
