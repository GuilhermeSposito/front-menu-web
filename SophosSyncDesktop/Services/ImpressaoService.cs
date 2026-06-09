
using FrontMenuWeb.DTOS;
using FrontMenuWeb.Models.Caixa;
using FrontMenuWeb.Models.Merchant;
using FrontMenuWeb.Models.Pedidos;
using FrontMenuWeb.Models.Vendas;
using FrontMenuWebSheredModels.DTOS;
using SophosSyncDesktop.DataBase.Db;
using SophosSyncDesktop.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Drawing.Printing;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Linq;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;
using DANFe = Unimake.Unidanfe;

namespace SophosSyncDesktop.Services;

public class ImpressaoService
{

    #region Fontes utilizadas na impressão
    public static bool DestacaObservacoes { get; set; } = true;
    public Font FonteSeparadoresSimples { get; set; } = new Font("DejaVu sans mono", 8, FontStyle.Bold);
    public static Font FonteCódigoDeBarras = new Font("3 of 9 Barcode", 35, FontStyle.Regular);
    public Font FonteComplemento { get; set; } = new Font("DejaVu sans mono", 9, FontStyle.Bold);
    public Font FonteComplementoNaComanda { get; set; } = new Font("DejaVu sans mono", 9, FontStyle.Bold);
    public Font FonteFechamentoDeCaixa { get; set; } = new Font("DejaVu sans mono", 8, FontStyle.Bold);
    public Font FonteItemFechamento { get; set; } = new Font("DejaVu sans mono", 8, FontStyle.Bold);
    public Font FonteTotaisFechamento { get; set; } = new Font("DejaVu sans mono", 8, FontStyle.Bold);
    public Font FonteNomeDoCliente { get; set; } = new Font("DejaVu sans mono", 14, FontStyle.Bold);
    public Font FonteItens { get; set; } = new Font("DejaVu sans mono", 12, FontStyle.Bold);
    public Font FonteContaEntregaEConta { get; set; } = new Font("DejaVu sans mono", 12, FontStyle.Bold);
    public Font FonteItens2 { get; set; } = new Font("DejaVu sans mono", 11, FontStyle.Bold);
    public Font FonteItensComanda { get; set; } = new Font("DejaVu sans mono", 11, FontStyle.Bold);
    public Font FonteCPF { get; set; } = new Font("DejaVu sans mono", 8, FontStyle.Bold);
    public Font FontQtdDescVunitVTotal { get; set; } = new Font("DejaVu sans mono", 8, FontStyle.Bold);
    public Font FonteTotaisNovo { get; set; } = new Font("DejaVu sans mono", 12, FontStyle.Regular);
    public Font FonteInfosPagamento { get; set; } = new Font("DejaVu sans mono", 10, FontStyle.Bold);
    public Font FonteDetalhesDoPedido { get; set; } = new Font("DejaVu sans mono", 9, FontStyle.Bold);
    public Font FonteLegendaDoTamanho { get; set; } = new Font("DejaVu sans mono", 12, FontStyle.Bold);
    public Font FonteSophos { get; set; } = new Font("Montserrat", 15, FontStyle.Bold);
    public Font FonteAlinhada { get; set; } = new Font("Courier New", 8, FontStyle.Bold);
    public int ValorEspacamento = 19;
    #endregion

    #region Funções que chamam a impressão
    public async Task Imprimir(string jsonDoPedido, string AppQueEnviou)
    {
        try
        {
            using (AppDbContext db = new AppDbContext())
            {
                AtualizaTamanhoDeFontesParametrizados();

                ImpressorasConfigs Imps = db.Impressoras.FirstOrDefault() ?? new ImpressorasConfigs();
                ClsPedido Pedido = JsonSerializer.Deserialize<ClsPedido>(jsonDoPedido) ?? throw new Exception("Erro ao desserializr pedido");

                //verificar se o pedido é sophos e esta aberto
                if (((Pedido.CriadoPor == "SOPHOS" && Pedido.StatusPedido != "ABERTO") || (Pedido.CriadoPor != "SOPHOS")) && !Pedido.ImprimeApenasComanda)
                {
                    //primeiro imprime pedido
                    if (!string.IsNullOrEmpty(Imps.ImpressoraCaixa) && VerificaSeNaoEstaSemImpressora(Imps.ImpressoraCaixa))
                    {

                        List<ClsImpressaoDefinicoes> ConteudoParaImpressaoDoPedido = DefineCaracteristicasDePedidoParaImpressao(Pedido, AppQueEnviou);
                        await ImprimirPagina(ConteudoParaImpressaoDoPedido, Imps.ImpressoraCaixa, ValorEspacamento);

                    }

                    //imprime na impressora auxiliar se tiver
                    if (!string.IsNullOrEmpty(Imps.ImpressoraAux) && VerificaSeNaoEstaSemImpressora(Imps.ImpressoraAux))
                    {
                        List<ClsImpressaoDefinicoes> ConteudoParaImpressaoDoPedido = DefineCaracteristicasDePedidoParaImpressao(Pedido, AppQueEnviou);
                        await ImprimirPagina(ConteudoParaImpressaoDoPedido, Imps.ImpressoraAux, ValorEspacamento);

                    }
                }


                if (AppState.MerchantLogado is not null && !Pedido.ImprimeApenasPedido)
                {
                    await ImprimirComanda(jsonDoPedido, Pedido.CriadoPor, false);
                }


            }
        }
        catch (Exception ex)
        {
            Console.Write(ex.ToString());
        }
    }
    public async Task ImprimirComanda(string jsonDoPedido, string AppQueEnviou, bool EMesa = true, bool TemSeparacaoPorItem = false)
    {
        try
        {
            AtualizaTamanhoDeFontesParametrizados();

            using (AppDbContext db = new AppDbContext())
            {
                if (EMesa)
                {
                    ImpressorasConfigs Imps = db.Impressoras.FirstOrDefault() ?? new ImpressorasConfigs();
                    PedidoMesaDto Pedido = JsonSerializer.Deserialize<PedidoMesaDto>(jsonDoPedido) ?? throw new Exception("Erro ao desserializr pedido");

                    List<ItensPorImpressoraDto> produtosAgrupados = Pedido.Itens
                               .Where(i => !i.ECouvert)
                               .SelectMany(i => i.Produto == null ? new[] { new { Origem = 0, Impressora = (string?)null, Item = i } } : new[]
                                {
                                         new { Origem = 1, Impressora = i.Produto.ImpressoraComanda1, Item = i },
                                         new { Origem = 2, Impressora = i.Produto.ImpressoraComanda2, Item = i }
                                })
                               .Where(x => x.Impressora != "Nao")
                               .GroupBy(x => new { x.Impressora, x.Origem })
                               .Select(grupo => new ItensPorImpressoraDto
                               {
                                   Impressora = grupo.Key.Impressora,
                                   Itens = grupo.Select(x => x.Item).ToList()
                               })
                               .ToList();

                    foreach (var Prods in produtosAgrupados)
                    {
                        if (Prods.Impressora is not null && (Prods.Impressora.Contains("Não Imprime", StringComparison.OrdinalIgnoreCase) || Prods.Impressora.Contains("Nao", StringComparison.OrdinalIgnoreCase)))
                            continue;

                        PedidoMesaDto PedidoAtualizadoComItensAgrupados = Pedido;
                        PedidoAtualizadoComItensAgrupados.Itens = Prods.Itens;
                        PedidoMesaDto PedidoAtualizadoComItensAgrupadosAuxiliar = new PedidoMesaDto
                        {
                            IdentificacaoMesaOuComanda = Pedido.IdentificacaoMesaOuComanda,
                            NomeCliente = Pedido.NomeCliente,
                            Itens = Prods.Itens
                        };

                        string Impressora = RetornaImpressoraSelecionadaNoCadastroDeProduto(Imps, Prods.Impressora);
                        if (Impressora == "Sem Impressora" || Impressora == "Não Imprime")
                            continue;

                        var QtdDeLoops = 1;

                        if (AppState.MerchantLogado is not null)
                            QtdDeLoops = AppState.MerchantLogado!.ImprimeComandasSeparadaPorProdutos ? Prods.Itens.Count() : 1;

                        var IndiceDoItemAtual = 1;
                        for (var i = 0; i < QtdDeLoops; i++)
                        {
                            if (QtdDeLoops > 1 || AppState.MerchantLogado!.ImprimeComandasSeparadaPorProdutos)//se for maior que 1 é porque é separado por item
                            {
                                var ItemAtual = Prods.Itens[i];
                                PedidoAtualizadoComItensAgrupados.Itens = new List<ItensPedido> { ItemAtual };

                                if (ItemAtual.Quantidade > 1)
                                {
                                    PedidoAtualizadoComItensAgrupados.Itens = new List<ItensPedido>();
                                    var QtdOriginalDoProduto = ItemAtual.Quantidade;
                                    for (var x = 0; x < QtdOriginalDoProduto; x++)
                                    {
                                        ItemAtual.Quantidade = 1;
                                        PedidoAtualizadoComItensAgrupados.Itens.Add(ItemAtual);
                                    }
                                }
                            }

                            var SeparaPorItem = 1f;
                            if (QtdDeLoops > 1 || AppState.MerchantLogado!.ImprimeComandasSeparadaPorProdutos)
                                SeparaPorItem = PedidoAtualizadoComItensAgrupados.Itens.Count;

                            for (var y = 0; y < SeparaPorItem; y++)
                            {
                                if (QtdDeLoops > 1 || AppState.MerchantLogado!.ImprimeComandasSeparadaPorProdutos)
                                    PedidoAtualizadoComItensAgrupadosAuxiliar.Itens = new List<ItensPedido> { PedidoAtualizadoComItensAgrupados.Itens[y] };

                                List<ClsImpressaoDefinicoes> ConteudoParaImpressaoDoPedidoMesa = DefineCaracteristicasDaComandaParaImpressaoMesa(PedidoAtualizadoComItensAgrupadosAuxiliar, AppQueEnviou);

                                for (var interador = 0; interador < AppState.MerchantLogado?.QtdViasDaComanda; interador++)
                                    await ImprimirPagina(ConteudoParaImpressaoDoPedidoMesa, Impressora, ValorEspacamento);

                                IndiceDoItemAtual++;
                            }

                        }


                    }

                }
                else  //merchant nunca vai entrar nulo aqui
                {
                    ImpressorasConfigs Imps = db.Impressoras.FirstOrDefault() ?? new ImpressorasConfigs();
                    ClsPedido Pedido = JsonSerializer.Deserialize<ClsPedido>(jsonDoPedido) ?? throw new Exception("Erro ao desserializr pedido");

                    bool SeparaComandaPorItens = AppState.MerchantLogado!.ImprimeComandasSeparadaPorProdutos;
                    float QtdDeItensDoPedido = Pedido.Itens.Count();
                    if (SeparaComandaPorItens)
                        QtdDeItensDoPedido = Pedido.Itens.Sum(x => x.Quantidade);

                    List<ItensPorImpressoraDto> produtosAgrupados = Pedido.Itens
                       .SelectMany(i => i.Produto == null ? new[] { new { Origem = 0, Impressora = (string?)null, Item = i } } : new[]
                        {
                                new {  Origem = 1, Impressora = i.Produto.ImpressoraComanda1, Item = i },
                                new {  Origem = 2, Impressora = i.Produto.ImpressoraComanda2, Item = i }
                        })
                       .Where(x => x.Impressora != "Nao")
                       .GroupBy(x => new { x.Impressora, x.Origem })
                       .Select(grupo => new ItensPorImpressoraDto
                       {
                           Impressora = grupo.Key.Impressora,
                           Itens = grupo.Select(x => x.Item).ToList()
                       })
                       .ToList();

                    foreach (ItensPorImpressoraDto Prods in produtosAgrupados)
                    {
                        if (Prods.Impressora is not null && (Prods.Impressora.Contains("Não Imprime", StringComparison.OrdinalIgnoreCase) || Prods.Impressora.Contains("Nao", StringComparison.OrdinalIgnoreCase)))
                            continue;

                        ClsPedido PedidoAtualizadoComItensAgrupados = Pedido;
                        List<ClsImpressaoDefinicoes> ConteudoParaImpressaoDoPedido = new List<ClsImpressaoDefinicoes>();
                        ClsPedido PedidoAtualizadoComItensAgrupadosAuxiliar = new ClsPedido(Pedido);

                        PedidoAtualizadoComItensAgrupados.Itens = Prods.Itens;

                        string Impressora = RetornaImpressoraSelecionadaNoCadastroDeProduto(Imps, Prods.Impressora);
                        if (Impressora == "Sem Impressora")
                            continue;

                        var QtdDeLoops = AppState.MerchantLogado!.ImprimeComandasSeparadaPorProdutos ? Prods.Itens.Count() : 1;

                        var IndiceDoItemAtual = 1;
                        for (var i = 0; i < QtdDeLoops; i++)
                        {
                            if (QtdDeLoops > 1 || AppState.MerchantLogado!.ImprimeComandasSeparadaPorProdutos)//se for maior que 1 é porque é separado por item
                            {
                                var ItemAtual = Prods.Itens[i];
                                PedidoAtualizadoComItensAgrupados.Itens = new List<ItensPedido> { ItemAtual };

                                if (ItemAtual.Quantidade > 1)
                                {
                                    PedidoAtualizadoComItensAgrupados.Itens = new List<ItensPedido>();
                                    var QtdOriginalDoProduto = ItemAtual.Quantidade;
                                    for (var x = 0; x < QtdOriginalDoProduto; x++)
                                    {
                                        ItemAtual.Quantidade = 1;
                                        PedidoAtualizadoComItensAgrupados.Itens.Add(ItemAtual);
                                    }
                                }
                            }

                            var SeparaPorItem = 1f;
                            if (QtdDeLoops > 1 || AppState.MerchantLogado!.ImprimeComandasSeparadaPorProdutos)
                                SeparaPorItem = PedidoAtualizadoComItensAgrupados.Itens.Count;

                            for (var y = 0; y < SeparaPorItem; y++)
                            {
                                if (QtdDeLoops > 1 || AppState.MerchantLogado!.ImprimeComandasSeparadaPorProdutos)
                                    PedidoAtualizadoComItensAgrupadosAuxiliar.Itens = new List<ItensPedido> { PedidoAtualizadoComItensAgrupados.Itens[y] };

                                ConteudoParaImpressaoDoPedido = DefineCaracteristicasDaComandaParaImpressaoDeliveryEBalcao(PedidoAtualizadoComItensAgrupadosAuxiliar, AppQueEnviou, AppState.MerchantLogado!.ImprimeComandasSeparadaPorProdutos, QtdDeItensDoPedido, IndiceDoItemAtual);

                                if ((Pedido.TipoDePedido == "DELIVERY" && AppState.MerchantLogado!.ImprimeComandasDelivery) || ((Pedido.TipoDePedido == "BALCÃO" || Pedido.TipoDePedido == "CHECKOUT") && AppState.MerchantLogado!.ImprimeComandasBalcao))
                                    for (var interador = 0; interador < AppState.MerchantLogado.QtdViasDaComanda; interador++)
                                        await ImprimirPagina(ConteudoParaImpressaoDoPedido, Impressora, ValorEspacamento);

                                IndiceDoItemAtual++;
                            }
                        }
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Console.Write(ex.ToString());
        }
    }

    public async Task ImprimirFechamento(string jsonDoFechamento)
    {
        try
        {
            AtualizaTamanhoDeFontesParametrizados();
            using (AppDbContext db = new AppDbContext())
            {
                ImpressorasConfigs Imps = db.Impressoras.FirstOrDefault() ?? new ImpressorasConfigs();
                ClsFechamentoDeCaixa Fechamento = JsonSerializer.Deserialize<ClsFechamentoDeCaixa>(jsonDoFechamento) ?? throw new Exception("Erro ao desserializr fechamento");

                //primeiro imprime pedido
                if (!string.IsNullOrEmpty(Imps.ImpressoraCaixa) && VerificaSeNaoEstaSemImpressora(Imps.ImpressoraCaixa))
                {
                    List<ClsImpressaoDefinicoes> ConteudoParaImpressaoDoPedido = DefineCaracteristicasDoFechamentoParaImpressao(Fechamento);
                    await ImprimirPagina(ConteudoParaImpressaoDoPedido, Imps.ImpressoraCaixa, 14);
                }

            }
        }
        catch (Exception ex)
        {
            Console.Write(ex.ToString());
        }
    }

    public async Task ImprimirFechamentoMotoboy(string jsonDoFechamentoDoMotoboy)
    {
        try
        {
            AtualizaTamanhoDeFontesParametrizados();
            using (AppDbContext db = new AppDbContext())
            {
                ImpressorasConfigs Imps = db.Impressoras.FirstOrDefault() ?? new ImpressorasConfigs();
                ClsResumoExpedicao Fechamento = JsonSerializer.Deserialize<ClsResumoExpedicao>(jsonDoFechamentoDoMotoboy) ?? throw new Exception("Erro ao desserializr fechamento");

                //primeiro imprime pedido
                if (!string.IsNullOrEmpty(Imps.ImpressoraCaixa) && VerificaSeNaoEstaSemImpressora(Imps.ImpressoraCaixa))
                {
                    List<ClsImpressaoDefinicoes> ConteudoParaImpressaoDoPedido = DefineCaracteristicasDoFechamentodeMotoboyParaImpressao(Fechamento);
                    await ImprimirPagina(ConteudoParaImpressaoDoPedido, Imps.ImpressoraCaixa, 14);
                }

            }
        }
        catch (Exception ex)
        {
            Console.Write(ex.ToString());
        }
    }

    public async Task ImprimirSangria(string json)
    {
        try
        {
            AtualizaTamanhoDeFontesParametrizados();
            using (AppDbContext db = new AppDbContext())
            {
                ImpressorasConfigs Imps = db.Impressoras.FirstOrDefault() ?? new ImpressorasConfigs();
                ClsSangria sangria = JsonSerializer.Deserialize<ClsSangria>(json) ?? throw new Exception("Erro ao desserializar sangria");

                if (!string.IsNullOrEmpty(Imps.ImpressoraCaixa) && VerificaSeNaoEstaSemImpressora(Imps.ImpressoraCaixa))
                {
                    List<ClsImpressaoDefinicoes> conteudo = DefineCaracteristicasDaSangriaParaImpressao(sangria);
                    await ImprimirPagina(conteudo, Imps.ImpressoraCaixa, ValorEspacamento);
                }
            }
        }
        catch (Exception ex)
        {
            Console.Write(ex.ToString());
        }
    }

    public async Task ImprimirSuprimento(string json)
    {
        try
        {
            AtualizaTamanhoDeFontesParametrizados();
            using (AppDbContext db = new AppDbContext())
            {
                ImpressorasConfigs Imps = db.Impressoras.FirstOrDefault() ?? new ImpressorasConfigs();
                ClsSuprimento suprimento = JsonSerializer.Deserialize<ClsSuprimento>(json) ?? throw new Exception("Erro ao desserializar suprimento");

                if (!string.IsNullOrEmpty(Imps.ImpressoraCaixa) && VerificaSeNaoEstaSemImpressora(Imps.ImpressoraCaixa))
                {
                    List<ClsImpressaoDefinicoes> conteudo = DefineCaracteristicasDoSuprimentoParaImpressao(suprimento);
                    await ImprimirPagina(conteudo, Imps.ImpressoraCaixa, ValorEspacamento);
                }
            }
        }
        catch (Exception ex)
        {
            Console.Write(ex.ToString());
        }
    }

    public async Task ImprimirFechamentoDeConta(string json)
    {
        try
        {
            AtualizaTamanhoDeFontesParametrizados();
            using var db = new AppDbContext();
            ImpressorasConfigs Imps = db.Impressoras.FirstOrDefault() ?? new ImpressorasConfigs();
            AvisoContaDto aviso = JsonSerializer.Deserialize<AvisoContaDto>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true })
                ?? throw new Exception("Erro ao desserializar aviso de conta");

            if (!string.IsNullOrEmpty(Imps.ImpressoraCaixa) && VerificaSeNaoEstaSemImpressora(Imps.ImpressoraCaixa))
            {
                List<ClsImpressaoDefinicoes> conteudo = DefineCaracteristicasDeFechamentoDeConta(aviso);
                await ImprimirPagina(conteudo, Imps.ImpressoraCaixa, ValorEspacamento);
            }
        }
        catch (Exception ex)
        {
            Console.Write(ex.ToString());
        }
    }


    #endregion

    #region Define as características da impressão

    #region Define Características das comandas para impressão
    private List<ClsImpressaoDefinicoes> DefineCaracteristicasDaComandaParaImpressaoMesa(PedidoMesaDto pedido, string AppQueEnviou)
    {
        bool JaImprimiuMesa = false;
        List<ClsImpressaoDefinicoes> Conteudo = new List<ClsImpressaoDefinicoes>();
        string? NomeGarcomQueEnviou = string.Empty;
        string? NomeFuncionarioQueEnviou = string.Empty;

        AdicionaConteudo(Conteudo, $"{AppState.MerchantLogado?.LegendaNomeUltilizadoParaPlaced}: {pedido.IdentificacaoMesaOuComanda.ToString().PadLeft(2, '0')}", FonteDetalhesDoPedido, Alinhamentos.Centro);
        //========================================================================================       
        AdicionaConteudo(Conteudo, AdicionarSeparadorDuplo(), FonteSeparadoresSimples);

        if (AppState.MerchantLogado is not null && AppState.MerchantLogado.UltilizaRequisicaoDeMesaNoItem)
        {
            var Mesa = pedido.Itens.GroupBy(x => x.NumeroMesaItem).ToList();
            if (Mesa.Count == 1 && Mesa[0].Key != 0)
            {
                int NumeroDaMesa = Mesa[0].Key;
                AdicionaConteudo(Conteudo, $"MESA: {NumeroDaMesa}", FonteDetalhesDoPedido, Alinhamentos.Centro);
                //========================================================================================       
                JaImprimiuMesa = true;
                AdicionaConteudo(Conteudo, AdicionarSeparadorDuplo(), FonteSeparadoresSimples);
            }
        }

        if (AppState.MerchantLogado is not null && AppState.MerchantLogado.ImprimeHorarioNaComanda)
        {
            var PrimeiroItem = pedido.Itens.FirstOrDefault();
            if (PrimeiroItem is not null)
            {
                AdicionaConteudo(Conteudo, $"Emitido às {PrimeiroItem.CriadoEm:t}", FonteDetalhesDoPedido);
                //========================================================================================       
                AdicionaConteudo(Conteudo, AdicionarSeparadorDuplo(), FonteSeparadoresSimples);

            }
        }

        foreach (var item in pedido.Itens)
        {
            string? Tamanho = null;
            if (!string.IsNullOrEmpty(item.LegTamanhoEscolhido))
            {
                Tamanho = item.LegTamanhoEscolhido;
                if (Tamanho.Length > 10)
                    Tamanho = Tamanho.Substring(0, 10);
            }

            if (item.Produto?.Fracionado == true && AppState.MerchantLogado is not null && AppState.MerchantLogado.PulaItensFracionadosParaProximaLinha)
            {
                var partesFracionado = item.Descricao.Split('&');
                for (int i = 0; i < partesFracionado.Length; i++)
                {
                    var nome = partesFracionado[i].Trim();
                    if (!string.IsNullOrEmpty(nome))
                        AdicionaConteudo(Conteudo, i == 0 ? $"{item.Quantidade}  {nome}" : $"   {nome}", FonteItensComanda);
                }

                var tamanhoFrac = !string.IsNullOrEmpty(Tamanho) ? $"TAM: {Tamanho}" : "";
                AdicionaConteudo(Conteudo, tamanhoFrac, FonteLegendaDoTamanho);
            }
            else
            {
                var tamanhoStr = !string.IsNullOrEmpty(Tamanho) ? $"TAM: {Tamanho}" : "";
                AdicionaConteudo(Conteudo, $"  {item.Quantidade}    {tamanhoStr}", FonteItensComanda);
                AdicionaConteudo(Conteudo, $"{item.Descricao}", FonteItensComanda);
            }

            if (item.Complementos.Count > 0)
            {
                foreach (var complemento in item.Complementos)
                {
                    AdicionaConteudo(Conteudo, $"  {complemento.Quantidade}- {complemento.Descricao}", FonteComplementoNaComanda, eObs: true);
                }
            }



            if (!String.IsNullOrEmpty(item.Observacoes))
            {
                AdicionaConteudo(Conteudo, $" ", FonteComplementoNaComanda, eObs: true);
                AdicionaConteudo(Conteudo, $"Obs: {item.Observacoes}", FonteComplementoNaComanda, eObs: true);
            }

            if (!String.IsNullOrEmpty(item.NomeCliente))
            {
                AdicionaConteudo(Conteudo, $"Cliente: {item.NomeCliente}", FonteComplementoNaComanda, eObs: true);
            }

            if (AppState.MerchantLogado is not null && AppState.MerchantLogado.UltilizaRequisicaoDeMesaNoItem)
                if (item.NumeroMesaItem > 0 && !JaImprimiuMesa)
                    AdicionaConteudo(Conteudo, $"Mesa: {item.NumeroMesaItem}", FonteComplementoNaComanda, eObs: true);


            AdicionaConteudo(Conteudo, AdicionarSeparadorDuplo(), FonteSeparadoresSimples);

            NomeGarcomQueEnviou = item.Garcon?.Nome ?? NomeGarcomQueEnviou;
            NomeFuncionarioQueEnviou = item.Funcionario?.Nome ?? NomeFuncionarioQueEnviou;
            //------------------------------------------------------------------------------------------
        }

        if (!string.IsNullOrEmpty(NomeGarcomQueEnviou))
        {
            AdicionaConteudo(Conteudo, $"Garçom: {NomeGarcomQueEnviou}", FonteComplementoNaComanda, alinhamento: Alinhamentos.Centro);
            AdicionaConteudo(Conteudo, $" ", FonteComplementoNaComanda, alinhamento: Alinhamentos.Centro);
        }

        if (!string.IsNullOrEmpty(NomeFuncionarioQueEnviou))
        {
            AdicionaConteudo(Conteudo, $"Enviado Por: {NomeFuncionarioQueEnviou}", FonteComplementoNaComanda, alinhamento: Alinhamentos.Centro);
            AdicionaConteudo(Conteudo, $" ", FonteComplementoNaComanda, alinhamento: Alinhamentos.Centro);
        }

        AdicionaConteudo(Conteudo, "Sophos - WEB", FonteSophos, Alinhamentos.Centro);
        AdicionaConteudo(Conteudo, "www.sophos-erp.com.br", FonteCPF, Alinhamentos.Centro);

        return Conteudo;
    }

    private List<ClsImpressaoDefinicoes> DefineCaracteristicasDaComandaParaImpressaoDeliveryEBalcao(ClsPedido pedido, string AppQueEnviou, bool ItemSeparadoPorComanda = false, float qtdItens = 0, int IndiceDoItemAtual = 0)
    {
        List<ClsImpressaoDefinicoes> Conteudo = new List<ClsImpressaoDefinicoes>();

        if (AppState.MerchantLogado is not null)
            AdicionaConteudo(Conteudo, AppState.MerchantLogado.NomeFantasia, FonteDetalhesDoPedido, Alinhamentos.Centro);
        AdicionaConteudo(Conteudo, AdicionarSeparadorDuplo(), FonteSeparadoresSimples);
        //========================================================================================       

        if (pedido.TipoDePedido != "DELIVERY")
        {
            if (AppState.MerchantLogado is not null && AppState.MerchantLogado.ImprimeSenhaBalcao)
            {
                int seed = pedido.Id;
                var random = new Random(seed);

                int senha = random.Next(500, 999);

                AdicionaConteudo(Conteudo, $"SENHA: {senha}", FonteLegendaDoTamanho, Alinhamentos.Centro);
                AdicionaConteudo(Conteudo, AdicionarSeparadorDuplo(), FonteSeparadoresSimples);
            }
            //========================================================================================       
        }

        DateTime EntregarAté = pedido.CriadoEm;
        if (AppState.MerchantLogado is not null)
        {
            if (pedido.TipoDePedido == "DELIVERY")
            {
                EntregarAté = pedido.CriadoEm.AddMinutes((double?)AppState.MerchantLogado?.TempoDeEntregaEMmin ?? 0);
            }
            else
            {
                EntregarAté = pedido.CriadoEm.AddMinutes((double?)AppState.MerchantLogado?.TempoDeRetiradaEMmin ?? 0);
            }
        }
        if (AppState.MerchantLogado is null || !AppState.MerchantLogado.ImprimeHorarioLimiteNoPedido)
        {
            AdicionaConteudo(Conteudo, $"Entregar até as {EntregarAté:t}", FonteDetalhesDoPedido);
        }
        AdicionaConteudo(Conteudo, $"Pedido {(pedido.TipoDePedido == "CHECKOUT" ? "CAIXA" : pedido.TipoDePedido)}", FonteDetalhesDoPedido);
        AdicionaConteudo(Conteudo, $"Conta Nº:   {pedido.DisplayId}", FonteContaEntregaEConta);
        AdicionaConteudo(Conteudo, AdicionarSeparadorDuplo(), FonteSeparadoresSimples);
        //========================================================================================       

        if (AppState.MerchantLogado!.ImprimeNomeNaComanda)
        {
            if (pedido.Cliente is not null)
            {
                AdicionaConteudo(Conteudo, $"Nome: {pedido.Cliente.Nome}", FonteNomeDoCliente);
                AdicionaConteudo(Conteudo, AdicionarSeparadorDuplo(), FonteSeparadoresSimples);
            }
        }

        //========================================================================================
        if (ItemSeparadoPorComanda)
        {
            AdicionaConteudo(Conteudo, $"Item: {IndiceDoItemAtual}/{qtdItens}", FonteDetalhesDoPedido);
        }

        AdicionaConteudo(Conteudo, $"Qtdade.  Descrição Do Item.", FontQtdDescVunitVTotal);
        AdicionaConteudo(Conteudo, $"Tam.", FontQtdDescVunitVTotal);
        AdicionaConteudo(Conteudo, AdicionarSeparadorSimples(), FonteSeparadoresSimples);
        foreach (var item in pedido.Itens)
        {
            if (item.Produto?.Fracionado == true && AppState.MerchantLogado is not null && AppState.MerchantLogado.PulaItensFracionadosParaProximaLinha)
            {
                var partesFracionadoComanda = item.Descricao.Split('&');
                for (int i = 0; i < partesFracionadoComanda.Length; i++)
                {
                    var nome = partesFracionadoComanda[i].Trim();
                    if (!string.IsNullOrEmpty(nome))
                        AdicionaConteudo(Conteudo, i == 0 ? $"{item.Quantidade}X  {nome}" : $"       {nome}", FonteItensComanda);
                }
            }
            else
            {

                AdicionaConteudo(Conteudo, $"{item.Quantidade}X  {item.Descricao}", FonteItensComanda);
            }


            if (!string.IsNullOrEmpty(item.LegTamanhoEscolhido))
            {
                AdicionaConteudo(Conteudo, $"{item.LegTamanhoEscolhido}", FonteLegendaDoTamanho);
            }

            if (item.Complementos.Count > 0)
            {
                AdicionaConteudo(Conteudo, $"\n", FonteCPF);
                foreach (var complemento in item.Complementos)
                {
                    AdicionaConteudo(Conteudo, $"{complemento.Quantidade}- {complemento.Descricao}", FonteComplementoNaComanda, eObs: true);
                }
            }

            if (!String.IsNullOrEmpty(item.Observacoes))
            {
                AdicionaConteudo(Conteudo, $"Obs: {item.Observacoes}", FonteComplementoNaComanda, eObs: true);
            }

            AdicionaConteudo(Conteudo, AdicionarSeparadorSimples(), FonteSeparadoresSimples);
            //------------------------------------------------------------------------------------------
        }


        AdicionaConteudo(Conteudo, "Sophos - WEB", FonteSophos, Alinhamentos.Centro);
        AdicionaConteudo(Conteudo, "www.sophos-erp.com.br", FonteCPF, Alinhamentos.Centro);

        return Conteudo;
    }
    #endregion

    #region Definição do pedido para impressão
    private List<ClsImpressaoDefinicoes> DefineCaracteristicasDePedidoParaImpressao(ClsPedido pedido, string AppQueEnviou)
    {
        List<ClsImpressaoDefinicoes> Conteudo = new List<ClsImpressaoDefinicoes>();

        if (pedido.CriadoPor != "SOPHOS")
        {
            AdicionaConteudo(Conteudo, pedido.CriadoPor, FonteLegendaDoTamanho, Alinhamentos.Centro);
        }

        if (pedido.TipoDePedido != "MESA")
            AdicionaConteudo(Conteudo, pedido.TipoDePedido == "DELIVERY" ? "E N T R E G A" : pedido.TipoDePedido == "CHECKOUT" ? "C A I X A" : "R E T I R A D A", FonteLegendaDoTamanho);

        if (AppState.MerchantLogado is not null)
            AdicionaConteudo(Conteudo, AppState.MerchantLogado.NomeFantasia, FonteDetalhesDoPedido);

        AdicionaConteudo(Conteudo, AdicionarSeparadorDuplo(), FonteSeparadoresSimples);
        //========================================================================================

        if (pedido.TipoDePedido != "DELIVERY" && AppState.MerchantLogado is not null && AppState.MerchantLogado.ImprimeSenhaBalcao)
        {
            int seed = pedido.Id;
            var random = new Random(seed);

            int senha = random.Next(500, 999);

            AdicionaConteudo(Conteudo, $"SENHA: {senha}", FonteLegendaDoTamanho, Alinhamentos.Centro);
            AdicionaConteudo(Conteudo, AdicionarSeparadorDuplo(), FonteSeparadoresSimples);
            //========================================================================================       
        }
        //========================================================================================       
        AdicionaConteudoLR(Conteudo, $"Controle Interno", "Sem valor fiscal", FonteCPF);
        AdicionaConteudo(Conteudo, $"Criado em: {pedido.CriadoEm:G}", FonteDetalhesDoPedido);
        AdicionaConteudo(Conteudo, $"Pedido criado por {pedido.CriadoPor}", FonteDetalhesDoPedido);

        var stringControle = $"{(pedido.TipoDePedido == "CHECKOUT" ? "CAIXA" : pedido.TipoDePedido)}";
        if (pedido.TipoDePedido == "MESA")
            stringControle = $"{AppState.MerchantLogado?.LegendaNomeUltilizadoParaPlaced ?? "Mesa"}: {pedido.Mesa?.CodigoExterno}";

        AdicionaConteudo(Conteudo, $"Controle: {stringControle}", FonteDetalhesDoPedido);
        AdicionaConteudo(Conteudo, $"Conta Nº:   {pedido.DisplayId}", FonteContaEntregaEConta, eObs: true);
        AdicionaConteudo(Conteudo, AdicionarSeparadorDuplo(), FonteSeparadoresSimples);
        //------------------------------------------------------------------------------------------

        DateTime EntregarAté = pedido.CriadoEm;
        if (AppState.MerchantLogado is not null)
        {
            if (pedido.TipoDePedido == "DELIVERY")
            {
                EntregarAté = pedido.CriadoEm.AddMinutes((double?)AppState.MerchantLogado?.TempoDeEntregaEMmin ?? 0);
            }
            else
            {
                EntregarAté = pedido.CriadoEm.AddMinutes((double?)AppState.MerchantLogado?.TempoDeRetiradaEMmin ?? 0);
            }
        }

        bool PedidoAgendado = pedido.PedidoAgendado;
        if (PedidoAgendado)
        {
            AdicionaConteudo(Conteudo, $"***PEDIDO  AGENDADO***", FonteContaEntregaEConta, Alinhamentos.Centro, eObs: true);
            AdicionaConteudo(Conteudo, AdicionarSeparadorDuplo(), FonteSeparadoresSimples);
        }

        if (pedido.TipoDePedido != "MESA")
            if (AppState.MerchantLogado is null || AppState.MerchantLogado.ImprimeHorarioLimiteNoPedido)
            {
                if (PedidoAgendado)
                    EntregarAté = pedido.HorarioDataAgendamento ?? EntregarAté;

                AdicionaConteudo(Conteudo, $"Entregar Até: {EntregarAté:t}", FonteContaEntregaEConta);
                AdicionaConteudo(Conteudo, AdicionarSeparadorSimples(), FonteSeparadoresSimples);
            }

        if (!string.IsNullOrEmpty(pedido.ObservacaoDoPedido))
        {
            AdicionaConteudo(Conteudo, $"OBS: {pedido.ObservacaoDoPedido}", FonteInfosPagamento, eObs: true);
            AdicionaConteudo(Conteudo, AdicionarSeparadorDuplo(), FonteSeparadoresSimples);
        }

        //------------------------------------------------------------------------------------------
        if (pedido.Cliente is not null)
        {
            AdicionaConteudo(Conteudo, pedido.Cliente.Nome, FonteDetalhesDoPedido);
            AdicionaConteudo(Conteudo, $"Telefone: {MascararTelefone(pedido.Cliente.Telefone)}", FonteDetalhesDoPedido);

            if (pedido.TipoDePedido != "DELIVERY")
                AdicionaConteudo(Conteudo, AdicionarSeparadorSimples(), FonteSeparadoresSimples);
        }

        if (pedido.TipoDePedido == "DELIVERY")
        {

            if (pedido.Endereco is not null)
            {
                AdicionaConteudo(Conteudo, $"{pedido.Endereco.Rua}, Nº{pedido.Endereco.Numero}", FonteDetalhesDoPedido);
                AdicionaConteudo(Conteudo, $"Bairro: {pedido.Endereco.Bairro}", FonteDetalhesDoPedido);

                if (!String.IsNullOrEmpty(pedido.Endereco.Complemento))
                    AdicionaConteudo(Conteudo, $"Complemento: {pedido.Endereco.Complemento}", FonteDetalhesDoPedido);

                if (!String.IsNullOrEmpty(pedido.Endereco.Referencia))
                    AdicionaConteudo(Conteudo, $"Referencia: {pedido.Endereco.Complemento}", FonteDetalhesDoPedido);


                AdicionaConteudo(Conteudo, AdicionarSeparadorSimples(), FonteSeparadoresSimples);
            }
        }

        //------------------------------------------------------------------------------------------
        if (pedido.TipoDePedido == "MESA")
        {
            if (AppState.MerchantLogado?.JuntaItensNoFechamentoDeConta ?? false)
            {
                pedido.Itens = pedido.Itens
                    .GroupBy(i => new { i.ProdutoId, i.Descricao, i.ECouvert, i.Funcionario })
                    .Select(g => new ItensPedido
                    {
                        ProdutoId = g.Key.ProdutoId,
                        Descricao = g.Key.Descricao,
                        Quantidade = g.Sum(i => i.Quantidade),
                        PrecoTotal = g.Sum(i => i.PrecoTotal),
                        PrecoUnitario = g.First().PrecoUnitario,
                        NumeroMesaItem = g.First().NumeroMesaItem,
                        Complementos = g.SelectMany(i => i.Complementos).ToList(),
                        Funcionario = g.Key.Funcionario,
                        Observacoes = string.Join(" | ", g.Where(i => !string.IsNullOrEmpty(i.Observacoes)).Select(i => i.Observacoes))
                    })
                    .ToList();
            }
        }



        AdicionaConteudo(Conteudo, $"Qtdade.  Descrição Do Item.", FontQtdDescVunitVTotal);
        AdicionaConteudoLR(Conteudo, $"Tam.", "V.Unit.   Total.", FontQtdDescVunitVTotal);
        AdicionaConteudo(Conteudo, AdicionarSeparadorSimples(), FonteSeparadoresSimples);
        foreach (var item in pedido.Itens)
        {
            if (pedido.TipoDePedido == "MESA" && (AppState.MerchantLogado?.UltilizaRequisicaoDeMesaNoItem ?? false))
            {
                if (item.NumeroMesaItem != 0)
                {
                    AdicionaConteudo(Conteudo, $"Mesa: {item.NumeroMesaItem}", new Font("Dejavu Sans Mono", 8));
                }
            }

            if (item.Produto?.Fracionado == true && AppState.MerchantLogado is not null && AppState.MerchantLogado.PulaItensFracionadosParaProximaLinha)
            {
                var partesFracionado = item.Descricao.Split('&');
                for (int i = 0; i < partesFracionado.Length; i++)
                {
                    var nome = partesFracionado[i].Trim();
                    if (!string.IsNullOrEmpty(nome))
                        AdicionaConteudo(Conteudo, i == 0 ? $"{item.Quantidade}  {nome}" : $"   {nome}", FonteItens2);
                }

            }
            else
            {

                AdicionaConteudo(Conteudo, $"{item.Quantidade}X  {item.Descricao}", FonteItens2);
            }
            AdicionaConteudo(Conteudo, $"{item.PrecoUnitario:F2}    {item.PrecoTotal:F2}", FonteCPF, alinhamento: Alinhamentos.Direita);

            if (!string.IsNullOrEmpty(item.LegTamanhoEscolhido))
            {
                AdicionaConteudo(Conteudo, $"{item.LegTamanhoEscolhido}", FonteLegendaDoTamanho);
            }

            if (item.Complementos.Count > 0)
            {
                AdicionaConteudo(Conteudo, $"\n", FonteCPF);
                foreach (var complemento in item.Complementos)
                {
                    AdicionaConteudo(Conteudo, $"{complemento.Quantidade}- {complemento.Descricao} - {complemento.PrecoTotal.ToString("C")}", FonteComplemento, eObs: true);
                }
            }

            if (!String.IsNullOrEmpty(item.Observacoes))
            {
                AdicionaConteudo(Conteudo, $"Obs: {item.Observacoes}", FonteComplemento, eObs: true);
            }

            if(pedido.TipoDePedido == "MESA")
            {
                if(item.Funcionario is not null && item.Funcionario.Nome == "QRCode")
                {
                    AdicionaConteudo(Conteudo, $"{item.Funcionario.Nome}", new Font("Dejavu Sans Mono", 8), eObs: true);
                }
            }

            AdicionaConteudo(Conteudo, AdicionarSeparadorSimples(), FonteSeparadoresSimples);
            //------------------------------------------------------------------------------------------
        }

        if (pedido.ValorDosItens > 0)
            AdicionaConteudo(Conteudo, $"SUB TOTAL. . . .  : {pedido.ValorDosItens:F2}", FonteTotaisNovo);
        if (pedido.TaxaEntregaValor > 0)
            AdicionaConteudo(Conteudo, $"TAXA DE ENTREGA . : {pedido.TaxaEntregaValor:F2}", FonteTotaisNovo);
        if (pedido.AcrescimoValor + pedido.ServicoValor > 0)
            AdicionaConteudo(Conteudo, $"TAXAS ADICIONAIS  : {(pedido.AcrescimoValor + pedido.ServicoValor):F2} ", FonteTotaisNovo);
        if (pedido.DescontoValor > 0)
            AdicionaConteudo(Conteudo, $"DESCONTOS. . . .  : {pedido.DescontoValor:F2}", FonteTotaisNovo);
        if (pedido.IncentivosExternosValor > 0)
            AdicionaConteudo(Conteudo, $"INCENTIVOS . . .  : {pedido.IncentivosExternosValor:F2}", FonteTotaisNovo);

        AdicionaConteudo(Conteudo, AdicionarSeparadorSimples(), FonteSeparadoresSimples);
        //------------------------------------------------------------------------------------------
        AdicionaConteudo(Conteudo, $"TOTAL DA CONTA .  : {pedido.ValorTotal:F2}", FonteTotaisNovo);

        //------------------------------------------------------------------------------------------


        AdicionaConteudo(Conteudo, AdicionarSeparadorSimples(), FonteSeparadoresSimples);

        if (pedido.Pagamentos is not null && pedido.Pagamentos.Count > 0)
        {
            foreach (var pagamento in pedido.Pagamentos)
            {
                if (pagamento.FormaDePagamento is not null)
                {
                    bool ePedidoPagoOnline = pagamento.FormaDePagamento.PagamentoOnline;
                    var InfoSeSeraPago = ePedidoPagoOnline ? "PAGO ONLINE COM" : "PEDIDO SERÁ PAGO COM";

                    if (pedido.EtapaPedido == "FINALIZADO")
                        InfoSeSeraPago = InfoSeSeraPago.Replace("SERÁ", "FOI");

                    AdicionaConteudo(Conteudo, $"{InfoSeSeraPago} ({pagamento.FormaDePagamento.Descricao}) -- VALOR: {pagamento.ValorTotal.ToString("C")}", FonteInfosPagamento);
                    if (pagamento.FormaDePagamento.EDinheiro && pagamento.Troco > 0)
                    {
                        AdicionaConteudo(Conteudo, $"LEVAR TROCO: {pagamento.Troco.ToString("C")}", FonteInfosPagamento);
                    }
                }
            }
        }
        else if (pedido.IsConvenio)
        {
            AdicionaConteudo(Conteudo, $"PEDIDO DE CONVÊNIO - NÃO RECEBER NA ENTREGA", FonteInfosPagamento);
        }

        if (pedido.CriadoPor == "IFOOD")
        {
            try
            {
                var PedidoIfood = JsonSerializer.Deserialize<PedidoIfoodDto>(pedido.JsonPedidoDeIntegracao ?? "") ?? throw new Exception("Erro ao desserializr pedido do ifood");
                if (!string.IsNullOrEmpty(PedidoIfood.Customer.DocumentNumber))
                {
                    AdicionaConteudo(Conteudo, AdicionarSeparadorSimples(), FonteSeparadoresSimples);
                    AdicionaConteudo(Conteudo, $"{PedidoIfood.Customer.DocumentType} na NFce: {PedidoIfood.Customer.DocumentNumber}", FonteInfosPagamento);

                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

        }

        AdicionaConteudo(Conteudo, AdicionarSeparadorSimples(), FonteSeparadoresSimples);


        AdicionaConteudo(Conteudo, "Sophos - WEB", FonteSophos, Alinhamentos.Centro);
        AdicionaConteudo(Conteudo, "www.sophos-erp.com.br", FonteCPF, Alinhamentos.Centro);


        if (AppState.MerchantLogado is not null && AppState.MerchantLogado.UsaCodigoDeBarrasParAProximaEtapa)
            AdicionaConteudo(Conteudo, $"*{pedido.Id}*", FonteCódigoDeBarras, Alinhamentos.Centro);


        return Conteudo;
    }
    #endregion

    #region Definição do aviso de conta para impressão
    private static string SepPontilhado() => "- - - - - - - - - - - - - - - - - - - -";

    private List<ClsImpressaoDefinicoes> DefineCaracteristicasDeFechamentoDeConta(AvisoContaDto aviso)
    {
        var legenda = AppState.MerchantLogado?.LegendaNomeUltilizadoParaPlaced ?? "Mesa";
        var legendaCouvert = AppState.MerchantLogado?.LegendaCouvert.ToString() ?? "Couvert";
        List<ClsImpressaoDefinicoes> Conteudo = new();

        decimal taxaPct = AppState.MerchantLogado?.TaxaDeServicoPercent ?? 0;

        // ── Cabeçalho ─────────────────────────────────────────────────────────
        if (AppState.MerchantLogado is not null)
            AdicionaConteudo(Conteudo, AppState.MerchantLogado.NomeFantasia, FonteFechamentoDeCaixa, Alinhamentos.Centro);

        AdicionaConteudo(Conteudo, "VIA DE FECHAMENTO", FonteFechamentoDeCaixa, Alinhamentos.Centro);
        AdicionaConteudo(Conteudo, $"{DateTime.Now:g}", FonteFechamentoDeCaixa, Alinhamentos.Centro);
        AdicionaConteudo(Conteudo, AdicionarSeparadorSimples(), FonteSeparadoresSimples);
        var identificador = aviso.PedidoId is not null
            ? $"Balcão - Pedido #{aviso.DisplayId ?? aviso.PedidoId.ToString()}"
            : $"{legenda} #{aviso.Mesa}";
        AdicionaConteudo(Conteudo, identificador, FonteDetalhesDoPedido);
        AdicionaConteudo(Conteudo, AdicionarSeparadorSimples(), FonteSeparadoresSimples);

        // ── Separado por cliente ───────────────────────────────────────────────
        if (aviso.SepararPorCliente && aviso.Comandas is not null && aviso.Comandas.Count > 1)
        {
            foreach (var comanda in aviso.Comandas)
            {
                var nome = string.IsNullOrEmpty(comanda.Nome) ? "Sem nome" : comanda.Nome;
                AdicionaConteudo(Conteudo, $"Cliente: {nome}", FonteFechamentoDeCaixa, Alinhamentos.Centro);

                AdicionaConteudoLR(Conteudo, "QTD  ITENS", "TOTAL", FonteFechamentoDeCaixa);
                AdicionaConteudo(Conteudo, AdicionarSeparadorSimples(), FonteSeparadoresSimples);

                if (AppState.MerchantLogado?.JuntaItensNoFechamentoDeConta ?? false)
                    comanda.Itens = comanda.Itens.GroupBy(i => new { i.Descricao, i.ECouvert }).Select(g => new AvisoContaItemDto
                    {
                        Descricao = g.Key.Descricao,
                        Quantidade = g.Sum(i => i.Quantidade),
                        PrecoUnitario = g.FirstOrDefault()?.PrecoUnitario ?? 0,
                        PrecoTotal = g.Sum(i => i.PrecoTotal),
                        NumerosDeMesaJuntados = string.Join(", ", g.Select(i => i.NumeroMesaItem).Distinct()),
                        ECouvert = g.Key.ECouvert,
                    }).ToList();

                AdicionaConteudo(Conteudo, $"Qtdade.  Descrição Do Item.", FontQtdDescVunitVTotal);
                AdicionaConteudo(Conteudo, $"V.Unit.   Total.", FontQtdDescVunitVTotal, Alinhamentos.Direita);
                AdicionaConteudo(Conteudo, AdicionarSeparadorSimples(), FonteSeparadoresSimples);

                foreach (var item in comanda.Itens)
                    if (!item.ECouvert)
                        AdicionarLinhasDeItem(Conteudo, item);

                if (comanda.Totais.TaxaDeServico > 0)
                {
                    AdicionaConteudo(Conteudo, " ", FonteTotaisFechamento);
                    AdicionaConteudoLR(Conteudo, $"Tx. de Serviço: ({taxaPct}%)", comanda.Totais.TaxaDeServico.ToString("C"), FonteFechamentoDeCaixa, eObs: true);
                }

                var couverts = comanda.Itens.Where(i => i.ECouvert).ToList();
                if (couverts.Count > 0)
                {
                    if (aviso.Itens is null)
                        aviso.Itens = new List<AvisoContaItemDto>();

                    aviso.Itens?.AddRange(couverts);
                }

                comanda.Totais.CouvertIndividual += couverts.Sum(i => i.Quantidade);
                if (couverts.Count > 0)
                {
                    var avisoCouvertAntes = aviso.Couvert;
                    aviso.Couvert = new AvisoContaCouvertDto
                    {
                        QtdPessoas = avisoCouvertAntes?.QtdPessoas ?? 0 + ((int?)couverts.Sum(i => i.Quantidade) ?? 1),
                        ValorPorPessoa = couverts.FirstOrDefault()?.PrecoUnitario ?? 0f,
                    };
                }

                if (comanda.Totais.CouvertIndividual > 0)
                {
                    var qtdCouvert = aviso.Couvert?.PorCliente?.FirstOrDefault(c => c.Nome == comanda.Nome)?.Qtd ?? 0;
                    var valorUnit = aviso.Couvert?.ValorPorPessoa ?? 0;

                    if (qtdCouvert == 0)
                    {
                        qtdCouvert = (int?)couverts.Where(i => i.NomeCliente == comanda.Nome).Sum(i => i.Quantidade) ?? 1;
                        if (qtdCouvert > 0)
                            comanda.Totais.CouvertIndividual = valorUnit * qtdCouvert;
                    }

                    var labelCouvert = qtdCouvert > 1 ? $"{legendaCouvert} ({qtdCouvert}x {valorUnit:C})" : legendaCouvert;
                    AdicionaConteudoLR(Conteudo, labelCouvert, comanda.Totais.CouvertIndividual.ToString("C"), FonteTotaisFechamento, eObs: true);
                }

                AdicionaConteudoLR(Conteudo, "TOTAL:", comanda.Totais.Total.ToString("C"), FonteTotaisFechamento);
                AdicionaConteudo(Conteudo, AdicionarSeparadorDuplo(), FonteSeparadoresSimples);
            }
        }
        else
        {
            // Usa aviso.Itens se disponível; quando vem apenas uma comanda os itens estão nela
            var itensParaImprimir = aviso.Itens ?? aviso.Comandas?.FirstOrDefault()?.Itens;

            if (itensParaImprimir is not null)
            {
                var nomesDistintos = itensParaImprimir
                    .Where(i => !string.IsNullOrEmpty(i.NomeCliente))
                    .Select(i => i.NomeCliente!)
                    .Distinct()
                    .ToList();

                if (nomesDistintos.Count == 1)
                {
                    AdicionaConteudo(Conteudo, $"Cliente: {nomesDistintos[0]}", FonteFechamentoDeCaixa, Alinhamentos.Centro);
                    AdicionaConteudo(Conteudo, AdicionarSeparadorSimples(), FonteSeparadoresSimples);
                }

                var couverts = itensParaImprimir.Where(i => i.ECouvert).ToList();
                if (aviso.Itens is null)
                {
                    aviso.Itens = new List<AvisoContaItemDto>();
                    aviso.Itens.AddRange(couverts);
                }

                var avisoCouvertAntes = aviso.Couvert;
                aviso.Couvert = new AvisoContaCouvertDto
                {
                    QtdPessoas = avisoCouvertAntes?.QtdPessoas ?? 0 + ((int?)couverts.Sum(i => i.Quantidade) ?? 1),
                    ValorPorPessoa = couverts.FirstOrDefault()?.PrecoUnitario ?? aviso.Couvert?.ValorPorPessoa ?? 0,
                };

                aviso.CouvertTotalMesa += couverts.Sum(i => i.PrecoTotal);

                if (AppState.MerchantLogado?.JuntaItensNoFechamentoDeConta ?? false)
                    itensParaImprimir = itensParaImprimir.GroupBy(i => new { i.Descricao, i.ECouvert }).Select(g => new AvisoContaItemDto
                    {
                        Descricao = g.Key.Descricao,
                        Quantidade = g.Sum(i => i.Quantidade),
                        PrecoUnitario = g.FirstOrDefault()?.PrecoUnitario ?? 0,
                        PrecoTotal = g.Sum(i => i.PrecoTotal),
                        NumerosDeMesaJuntados = string.Join(", ", g.Select(i => i.NumeroMesaItem).Distinct()),
                        ECouvert = g.Key.ECouvert,
                    }).ToList();

                AdicionaConteudo(Conteudo, $"Qtdade.  Descrição Do Item.", FontQtdDescVunitVTotal);
                AdicionaConteudo(Conteudo, $"V.Unit.   Total.", FontQtdDescVunitVTotal, Alinhamentos.Direita);
                AdicionaConteudo(Conteudo, AdicionarSeparadorSimples(), FonteSeparadoresSimples);

                foreach (var item in itensParaImprimir)
                    if (!item.ECouvert)
                        AdicionarLinhasDeItem(Conteudo, item);

                AdicionaConteudo(Conteudo, AdicionarSeparadorDuplo(), FonteSeparadoresSimples);
            }
        }

        // ── Consumo total ─────────────────────────────────────────────────────
        AdicionaConteudo(Conteudo, "Consumo Total", FonteFechamentoDeCaixa, Alinhamentos.Centro);
        AdicionaConteudo(Conteudo, AdicionarSeparadorDuplo(), FonteSeparadoresSimples);

        var valorDosCouvertsEnviadoComoItem = aviso.Itens?.Where(i => i.ECouvert).Sum(i => i.PrecoTotal) ?? 0;
        aviso.SubtotalDaMesa = aviso.SubtotalDaMesa - valorDosCouvertsEnviadoComoItem;
        AdicionaConteudoLR(Conteudo, "Valor dos Itens", aviso.SubtotalDaMesa.ToString("C"), FonteTotaisFechamento);

        if (aviso.TaxaDeServicoDaMesa > 0)
            AdicionaConteudoLR(Conteudo, $"Tx. de Serviço: ({taxaPct}%)", aviso.TaxaDeServicoDaMesa.ToString("C"), FonteTotaisFechamento);



        if (aviso.CouvertTotalMesa > 0 && aviso.Couvert is not null)
        {
            if (aviso.Couvert.PorCliente != null && aviso.Couvert.PorCliente.Count > 0)
            {
                // Detalha couvert por cliente
                foreach (var cc in aviso.Couvert.PorCliente.Where(c => c.Valor > 0))
                    AdicionaConteudoLR(Conteudo, $"{legendaCouvert} {cc.Nome} ({cc.Qtd}x {aviso.Couvert.ValorPorPessoa:C})", cc.Valor.ToString("C"), FonteTotaisFechamento);

                if (aviso.CouvertAvulso > 0 && aviso.Couvert.Avulso > 0)
                    AdicionaConteudoLR(Conteudo, $"{legendaCouvert} Avulso ({aviso.Couvert.Avulso}x {aviso.Couvert.ValorPorPessoa:C})", aviso.CouvertAvulso.ToString("C"), FonteTotaisFechamento);
            }
            else
            {
                AdicionaConteudoLR(Conteudo, $"{legendaCouvert} ({aviso.Couvert.QtdPessoas}x {aviso.Couvert.ValorPorPessoa:C})", aviso.CouvertTotalMesa.ToString("C"), FonteTotaisFechamento);
            }
        }

        if (aviso.Desconto > 0)
            AdicionaConteudoLR(Conteudo, "Desconto", $"-{aviso.Desconto:C}", FonteTotaisFechamento);

        if (aviso.TaxaAdicional > 0)
            AdicionaConteudoLR(Conteudo, "Taxa Adicional", aviso.TaxaAdicional.ToString("C"), FonteTotaisFechamento);

        AdicionaConteudo(Conteudo, AdicionarSeparadorSimples(), FonteSeparadoresSimples);

        var totalAExibir = aviso.TotalFinal > 0
            ? aviso.TotalFinal
            : aviso.SubtotalDaMesa + aviso.TaxaDeServicoDaMesa + aviso.CouvertTotalMesa - aviso.Desconto + aviso.TaxaAdicional;
        AdicionaConteudoLR(Conteudo, $"TOTAL A PAGAR:", totalAExibir.ToString("C"), FonteTotaisNovo);

        if (aviso.Pagamentos?.Count > 0)
        {
            AdicionaConteudo(Conteudo, AdicionarSeparadorSimples(), FonteTotaisFechamento);
            AdicionaConteudo(Conteudo, "Formas de Pagamento", FonteTotaisFechamento, Alinhamentos.Centro);
            foreach (var pag in aviso.Pagamentos)
                AdicionaConteudoLR(Conteudo, pag.Descricao, pag.Valor.ToString("C"), FonteTotaisFechamento);
        }

        if (aviso.Troco > 0)
            AdicionaConteudoLR(Conteudo, "Troco", aviso.Troco.ToString("C"), FonteTotaisFechamento);
        AdicionaConteudo(Conteudo, SepPontilhado(), FonteSeparadoresSimples);

        if (AppState.MerchantLogado?.ImprimeMediaPorPessoaNoFechamentoDaMesa ?? false)
        {
            if ((aviso.Couvert?.QtdPessoas ?? 0) > 0)
            {
                var mediaPorPessoa = aviso.TotalFinal > 0 ? aviso.TotalFinal / (aviso.Couvert?.QtdPessoas ?? 1) : totalAExibir / (aviso.Couvert?.QtdPessoas ?? 1);
                AdicionaConteudoLR(Conteudo, "NRO DE PESSOAS", $"{aviso.Couvert?.QtdPessoas ?? 1}", FonteTotaisFechamento);
                AdicionaConteudoLR(Conteudo, "MÉDIA POR PESSOA", mediaPorPessoa.ToString("C"), FonteTotaisFechamento);
                AdicionaConteudo(Conteudo, SepPontilhado(), FonteSeparadoresSimples);
            }
        }




        AdicionaConteudo(Conteudo, "Sophos - WEB", FonteSophos, Alinhamentos.Centro);
        AdicionaConteudo(Conteudo, "www.sophos-erp.com.br", FonteCPF, Alinhamentos.Centro);

        return Conteudo;
    }

    private void AdicionarLinhasDeItem(List<ClsImpressaoDefinicoes> Conteudo, AvisoContaItemDto item)
    {
        if (((AppState.MerchantLogado?.UltilizaRequisicaoDeMesaNoItem ?? false) && item.NumeroMesaItem is not null && item.NumeroMesaItem != 0) || !string.IsNullOrEmpty(item.NumerosDeMesaJuntados))
            AdicionaConteudo(Conteudo, $"Mesa: {(item.NumeroMesaItem is null ? item.NumerosDeMesaJuntados : item.NumeroMesaItem)}", new Font("Dejavu Sans Mono", 8));

        AdicionaConteudo(Conteudo, $"{item.Quantidade}x {item.Descricao}", FonteItemFechamento);
        AdicionaConteudo(Conteudo, $"{item.PrecoUnitario:F2}   {item.PrecoTotal:F2}", FonteItemFechamento, Alinhamentos.Direita);

        if (!(AppState.MerchantLogado?.NaoImprimeComplementosNoFechamento ?? false))
            if (item.Complementos?.Count > 0)
            {
                var leg = !string.IsNullOrEmpty(item.LegTamanhoEscolhido)
                    ? (item.LegTamanhoEscolhido.Length > 30 ? item.LegTamanhoEscolhido[..30] + "..." : item.LegTamanhoEscolhido)
                    : null;
                var cabecalhoComp = leg is not null ? $"{item.Descricao} - {leg}:" : $"{item.Descricao} - complementos:";
                AdicionaConteudo(Conteudo, cabecalhoComp, FonteItemFechamento, eObs: true);
                if (!string.IsNullOrEmpty(item.LegTamanhoEscolhido))

                    AdicionaConteudo(Conteudo, item.LegTamanhoEscolhido, FonteItemFechamento, eObs: true);
                foreach (var c in item.Complementos)
                    AdicionaConteudo(Conteudo, $"  - {c.Quantidade}x  {c.Descricao}", FonteItemFechamento, eObs: true);
            }
            else if (!string.IsNullOrEmpty(item.LegTamanhoEscolhido))
            {
                var leg = item.LegTamanhoEscolhido.Length > 30 ? item.LegTamanhoEscolhido[..30] + "..." : item.LegTamanhoEscolhido;
                AdicionaConteudo(Conteudo, $"  {leg}", FonteItemFechamento, eObs: true);
            }
    }
    #endregion

    #region Definição da sangria para impressão
    private List<ClsImpressaoDefinicoes> DefineCaracteristicasDaSangriaParaImpressao(ClsSangria sangria)
    {
        List<ClsImpressaoDefinicoes> Conteudo = new List<ClsImpressaoDefinicoes>();

        if (AppState.MerchantLogado is not null)
            AdicionaConteudo(Conteudo, AppState.MerchantLogado.NomeFantasia, FonteDetalhesDoPedido, Alinhamentos.Centro);

        AdicionaConteudo(Conteudo, AdicionarSeparadorDuplo(), FonteSeparadoresSimples);
        AdicionaConteudo(Conteudo, "SANGRIA DE CAIXA", FonteFechamentoDeCaixa, Alinhamentos.Centro);
        AdicionaConteudo(Conteudo, AdicionarSeparadorDuplo(), FonteSeparadoresSimples);

        AdicionaConteudo(Conteudo, $"Data: {sangria.CriadoEm:dd/MM/yyyy HH:mm}", FonteFechamentoDeCaixa);
        AdicionaConteudo(Conteudo, AdicionarSeparadorSimples(), FonteSeparadoresSimples);

        if (sangria.Funcionario is not null)
        {
            AdicionaConteudo(Conteudo, $"Operador: {sangria.Funcionario.Nome}", FonteFechamentoDeCaixa);
            AdicionaConteudo(Conteudo, AdicionarSeparadorSimples(), FonteSeparadoresSimples);
        }
        else
        {
            AdicionaConteudo(Conteudo, $"Operador: ADMIN", FonteFechamentoDeCaixa);
            AdicionaConteudo(Conteudo, AdicionarSeparadorSimples(), FonteSeparadoresSimples);
        }

        AdicionaConteudo(Conteudo, $"Descricao:", FonteFechamentoDeCaixa);
        AdicionaConteudo(Conteudo, sangria.Descricao, FonteItemFechamento);
        AdicionaConteudo(Conteudo, AdicionarSeparadorDuplo(), FonteSeparadoresSimples);

        AdicionaConteudoLR(Conteudo, "VALOR DA SANGRIA:", sangria.Valor.ToString("C"), FonteFechamentoDeCaixa);
        AdicionaConteudo(Conteudo, AdicionarSeparadorDuplo(), FonteSeparadoresSimples);

        AdicionaConteudo(Conteudo, "Sophos - WEB", FonteSophos, Alinhamentos.Centro);
        AdicionaConteudo(Conteudo, "www.sophos-erp.com.br", FonteCPF, Alinhamentos.Centro);

        return Conteudo;
    }
    #endregion

    #region Definição do suprimento para impressão
    private List<ClsImpressaoDefinicoes> DefineCaracteristicasDoSuprimentoParaImpressao(ClsSuprimento suprimento)
    {
        List<ClsImpressaoDefinicoes> Conteudo = new List<ClsImpressaoDefinicoes>();

        if (AppState.MerchantLogado is not null)
            AdicionaConteudo(Conteudo, AppState.MerchantLogado.NomeFantasia, FonteDetalhesDoPedido, Alinhamentos.Centro);

        AdicionaConteudo(Conteudo, AdicionarSeparadorDuplo(), FonteSeparadoresSimples);
        AdicionaConteudo(Conteudo, "SUPRIMENTO DE CAIXA", FonteFechamentoDeCaixa, Alinhamentos.Centro);
        AdicionaConteudo(Conteudo, AdicionarSeparadorDuplo(), FonteSeparadoresSimples);

        AdicionaConteudo(Conteudo, $"Data: {suprimento.CriadoEm:dd/MM/yyyy HH:mm}", FonteFechamentoDeCaixa);
        AdicionaConteudo(Conteudo, AdicionarSeparadorSimples(), FonteSeparadoresSimples);

        if (suprimento.Funcionario is not null)
        {
            AdicionaConteudo(Conteudo, $"Operador: {suprimento.Funcionario.Nome}", FonteFechamentoDeCaixa);
            AdicionaConteudo(Conteudo, AdicionarSeparadorSimples(), FonteSeparadoresSimples);
        }

        AdicionaConteudo(Conteudo, "Descricao:", FonteFechamentoDeCaixa);
        AdicionaConteudo(Conteudo, suprimento.Descricao, FonteFechamentoDeCaixa);
        AdicionaConteudo(Conteudo, AdicionarSeparadorDuplo(), FonteSeparadoresSimples);

        AdicionaConteudoLR(Conteudo, "VALOR DO SUPRIMENTO:", suprimento.Valor.ToString("C"), FonteFechamentoDeCaixa);
        AdicionaConteudo(Conteudo, AdicionarSeparadorDuplo(), FonteSeparadoresSimples);

        AdicionaConteudo(Conteudo, "Sophos - WEB", FonteSophos, Alinhamentos.Centro);
        AdicionaConteudo(Conteudo, "www.sophos-erp.com.br", FonteCPF, Alinhamentos.Centro);

        return Conteudo;
    }
    #endregion

    #region Definição do fechamento de caixa e motoboy para impressão
    private List<ClsImpressaoDefinicoes> DefineCaracteristicasDoFechamentoParaImpressao(ClsFechamentoDeCaixa Fechamento)
    {
        List<ClsImpressaoDefinicoes> Conteudo = new List<ClsImpressaoDefinicoes>();
        var legendaCouvertFechamento = AppState.MerchantLogado?.LegendaCouvert.ToString() ?? "Couvert";

        void Ln(string label, float valor) =>
            AdicionaConteudoLR(Conteudo, label, ((decimal)valor).ToString("C"), FonteFechamentoDeCaixa);

        var faturamentoBruto = Fechamento.ValorTotalEmVendas;
        var totalDoCaixa = Fechamento.ValorDeAbertura + Fechamento.Suprimentos - Fechamento.Sangrias + faturamentoBruto;
        var totalRecebimentos = Fechamento.RecebimentosPorTipo?.Values.Sum() ?? 0f;
        var labelFaltouSobrou = Fechamento.Faltou > 0 ? "FALTOU (=)" : "SOBROU (=)";
        var valorAbsFaltou = Math.Abs(Fechamento.Faltou);

        if (AppState.MerchantLogado is not null)
            AdicionaConteudo(Conteudo, AppState.MerchantLogado.NomeFantasia, FonteDetalhesDoPedido, Alinhamentos.Centro);
        AdicionaConteudo(Conteudo, AdicionarSeparadorDuplo(), FonteSeparadoresSimples);
        AdicionaConteudo(Conteudo, "FECHAMENTO DO CAIXA", FonteFechamentoDeCaixa, Alinhamentos.Centro);
        if (Fechamento.Caixa is not null)
        {
            AdicionaConteudo(Conteudo, $"{Fechamento.Caixa.DataAbertura:D}", FonteFechamentoDeCaixa, Alinhamentos.Centro);
            string RealizadoPor = Fechamento.Caixa.FuncionarioFechamento is not null ? Fechamento.Caixa.FuncionarioFechamento.Nome : "ADMIN";
            AdicionaConteudo(Conteudo, $"Aberto em {Fechamento.Caixa.DataAbertura}", FonteFechamentoDeCaixa, Alinhamentos.Centro);
            AdicionaConteudo(Conteudo, $"Fechado em {Fechamento.Caixa.DataFechamento}", FonteFechamentoDeCaixa, Alinhamentos.Centro);
            AdicionaConteudo(Conteudo, $"Realizado por ..: {RealizadoPor}", FonteFechamentoDeCaixa, Alinhamentos.Centro);
        }

        AdicionaConteudo(Conteudo, AdicionarSeparadorDuplo(), FonteFechamentoDeCaixa);

        // OPERAÇÃO DE VENDAS
        AdicionaConteudo(Conteudo, "OPERAÇÃO DE VENDAS", FonteFechamentoDeCaixa, eObs: true);
        AdicionaConteudo(Conteudo, AdicionarSeparadorSimples(), FonteSeparadoresSimples);
        Ln("TOTAL DE ITENS VENDIDOS (+)", Fechamento.TotalDeItensVendidos);
        Ln("ACRESCIMOS (+)", Fechamento.TotalDeArescimos);
        Ln("CORTESIA/DESCONTOS (-)", Fechamento.TotalEmDescontos + Fechamento.TotalEmIncentivos);
        Ln("TAXAS DE ENTREGA (+)", Fechamento.TotalTaxaEntrega);
        AdicionaConteudo(Conteudo, AdicionarSeparadorSimples(), FonteSeparadoresSimples);
        Ln($"{legendaCouvertFechamento.ToUpper()}S (+)", Fechamento.TotalDeCouvert);
        AdicionaConteudo(Conteudo, AdicionarSeparadorSimples(), FonteSeparadoresSimples);
        Ln("TAXA DE SERVIÇO (+)", Fechamento.TotalDeServico);
        AdicionaConteudo(Conteudo, AdicionarSeparadorSimples(), FonteSeparadoresSimples);
        Ln("FATURAMENTO BRUTO (=)", faturamentoBruto);
        AdicionaConteudo(Conteudo, AdicionarSeparadorSimples(), FonteSeparadoresSimples);

        // MOVIMENTAÇÕES DO CAIXA
        Ln("CAIXA INICIAL (+)", Fechamento.ValorDeAbertura);
        Ln("ENTRADAS DO CAIXA (+)", Fechamento.Suprimentos);
        Ln("SAÍDAS DO CAIXA (-)", Fechamento.Sangrias);
        Ln("TROCOS (-)", Fechamento.Trocos);
        AdicionaConteudo(Conteudo, AdicionarSeparadorSimples(), FonteSeparadoresSimples);
        Ln("TOTAL DO CAIXA (=)", totalDoCaixa);
        AdicionaConteudo(Conteudo, AdicionarSeparadorDuplo(), FonteSeparadoresSimples);

        // CONTAGEM FÍSICA
        AdicionaConteudo(Conteudo, "CONTAGEM FÍSICA DO CAIXA", FonteFechamentoDeCaixa, eObs: true);
        AdicionaConteudo(Conteudo, AdicionarSeparadorDuplo(), FonteSeparadoresSimples);
        Ln("DÉBITO INFORMADO. (=)", (float?)(Fechamento.Caixa?.ContagemFisicaDebito) ?? 0);
        Ln("CRÉDITO INFORMADO. (=)", (float?)(Fechamento.Caixa?.ContagemFisicaCredito) ?? 0);
        Ln("PIX INFORMADO. (=)", (float?)(Fechamento.Caixa?.ContagemFisicaPix) ?? 0);
        if ((Fechamento.Caixa?.ContagemFisicaOnline ?? 0) > 0)
            Ln("ONLINE INFORMADO. (=)", (float?)(Fechamento.Caixa?.ContagemFisicaOnline) ?? 0);
        AdicionaConteudo(Conteudo, AdicionarSeparadorSimples(), FonteSeparadoresSimples);
        Ln("DINHEIRO INFORMADO. (=)", (float?)(Fechamento.Caixa?.ValorCaixaEmDinFinal) ?? 0);

        Ln("VALOR ESPERADO DIN. (=)", Fechamento.ValorEsperadoEmDinheiro);
        AdicionaConteudo(Conteudo, AdicionarSeparadorSimples(), FonteSeparadoresSimples);

        if (valorAbsFaltou > 0)
        {
            Ln(labelFaltouSobrou, valorAbsFaltou);
            AdicionaConteudo(Conteudo, AdicionarSeparadorSimples(), FonteSeparadoresSimples);
        }

        // DISTRIBUIÇÃO POR FORMA
        AdicionaConteudo(Conteudo, "DISTRIBUIÇÃO POR FORMA DE RECEBIMENTO", FonteFechamentoDeCaixa, eObs: true);
        AdicionaConteudo(Conteudo, AdicionarSeparadorSimples(), FonteSeparadoresSimples);

        foreach (var pagamento in Fechamento.RecebimentosPorTipo ?? [])
            Ln($"{pagamento.Key} (+)", pagamento.Value);

        AdicionaConteudo(Conteudo, AdicionarSeparadorSimples(), FonteSeparadoresSimples);
        Ln("TOTAL RECEBIMENTOS (=)", totalRecebimentos);
        AdicionaConteudo(Conteudo, AdicionarSeparadorSimples(), FonteSeparadoresSimples);


        AdicionaConteudo(Conteudo, "Sophos - WEB", FonteSophos, Alinhamentos.Centro);
        AdicionaConteudo(Conteudo, "www.sophos-erp.com.br", FonteCPF, Alinhamentos.Centro);
        return Conteudo;
    }

    private List<ClsImpressaoDefinicoes> DefineCaracteristicasDoFechamentodeMotoboyParaImpressao(ClsResumoExpedicao fechamento)
    {
        List<ClsImpressaoDefinicoes> Conteudo = new List<ClsImpressaoDefinicoes>();

        if (AppState.MerchantLogado is not null)
            AdicionaConteudo(Conteudo, AppState.MerchantLogado.NomeFantasia, FonteDetalhesDoPedido, Alinhamentos.Centro);
        AdicionaConteudo(Conteudo, AdicionarSeparadorDuplo(), FonteSeparadoresSimples);
        //========================================================================================        
        AdicionaConteudo(Conteudo, $"FECHAMENTO DOS MOTOBOYS", FonteFechamentoDeCaixa, Alinhamentos.Centro);
        AdicionaConteudo(Conteudo, $"{fechamento.GeradoEm:G}", FonteFechamentoDeCaixa, Alinhamentos.Centro);
        AdicionaConteudo(Conteudo, AdicionarSeparadorDuplo(), FonteSeparadoresSimples);
        //========================================================================================        
        AdicionaConteudo(Conteudo, $"MOTOBOY(S) SELECIONADO(S)", FonteFechamentoDeCaixa, Alinhamentos.Centro);
        AdicionaConteudo(Conteudo, AdicionarSeparadorDuplo(), FonteSeparadoresSimples);
        //-----------------------------------------------------------------------------------------
        AdicionaConteudo(Conteudo, $"NOME     TELEFONE    QTDE    VALOR TOTAL", FonteFechamentoDeCaixa);
        AdicionaConteudo(Conteudo, AdicionarSeparadorSimples(), FonteSeparadoresSimples);
        foreach (var motoboy in fechamento.Motoboys)
        {
            AdicionaConteudo(Conteudo, $"{motoboy.Nome} - {motoboy.Telefone} - {motoboy.QuantidadePedidos} - {motoboy.TotalEntregas:C}", FonteFechamentoDeCaixa, eObs: true);
        }
        AdicionaConteudo(Conteudo, $"\n", FonteFechamentoDeCaixa);
        AdicionaConteudo(Conteudo, AdicionarSeparadorDuplo(), FonteSeparadoresSimples);
        //========================================================================================        
        AdicionaConteudo(Conteudo, $"PEDIDOS ATRIBUIDOS", FonteFechamentoDeCaixa, Alinhamentos.Centro);
        AdicionaConteudo(Conteudo, AdicionarSeparadorDuplo(), FonteSeparadoresSimples);
        //========================================================================================        
        AdicionaConteudo(Conteudo, $"NMR     MOTOBOY    FORMA     VALOR TOTAL", FonteFechamentoDeCaixa);
        AdicionaConteudo(Conteudo, AdicionarSeparadorSimples(), FonteSeparadoresSimples);
        //-----------------------------------------------------------------------------------------
        foreach (var pedido in fechamento.Pedidos)
        {
            AdicionaConteudo(Conteudo, $"Nº{pedido.DisplayId} - {pedido.Motoboy}  {pedido.FormasDeRecebimento} - {pedido.ValorTotal:C}", FonteFechamentoDeCaixa, eObs: true);
        }
        AdicionaConteudo(Conteudo, AdicionarSeparadorDuplo(), FonteSeparadoresSimples);
        //========================================================================================        
        AdicionaConteudo(Conteudo, $"FECHAMENTO (TOTALIZADORES)", FonteFechamentoDeCaixa, Alinhamentos.Centro);
        AdicionaConteudo(Conteudo, AdicionarSeparadorDuplo(), FonteSeparadoresSimples);
        //========================================================================================
        if (fechamento.Trocos is not null && fechamento.Trocos.HouveTroco)
            AdicionaConteudo(Conteudo, $"TROCO:                       {fechamento.Trocos.TotalTrocos:C}", FonteFechamentoDeCaixa, eObs: true);

        AdicionaConteudo(Conteudo, $"TOTAL DOS PEDIDOS:           {fechamento.TotalPedidos:C}", FonteFechamentoDeCaixa, eObs: true);
        AdicionaConteudo(Conteudo, $"VALOR A RECEBER:             {fechamento.TotalEntregas:C}", FonteFechamentoDeCaixa, eObs: true);


        AdicionaConteudo(Conteudo, AdicionarSeparadorDuplo(), FonteSeparadoresSimples);

        return Conteudo;
    }
    #endregion


    #endregion

    #region Funções de impressão

    public static Task ImprimirPagina(List<ClsImpressaoDefinicoes> conteudo, string impressora1, int espacamento)
    {
        var tcs = new TaskCompletionSource<bool>();

        var thread = new Thread(() =>
        {
            try
            {
                PrintDocument printDocument = new PrintDocument();
                printDocument.PrinterSettings.PrinterName = impressora1;
                printDocument.DefaultPageSettings.PaperSize = new PaperSize("Custom", 280, 500000);
                printDocument.DefaultPageSettings.Margins = new Margins(0, 0, 0, 0);
                printDocument.PrintPage += (sender, e) => PrintPageHandler(sender, e, conteudo, espacamento);
                printDocument.Print();
                tcs.SetResult(true);
            }
            catch (Exception ex)
            {
                tcs.SetException(ex);
            }
        });

        thread.SetApartmentState(ApartmentState.STA);
        thread.IsBackground = true;
        thread.Start();

        return tcs.Task;
    }

    public static void PrintPageHandler(object sender, PrintPageEventArgs e, List<ClsImpressaoDefinicoes> conteudo, int separacao)
    {
        try
        {
            int Y = 0;
            float pageW = e.PageBounds.Width;

            foreach (var item in conteudo)
            {
                // Itens com texto direito fixo (resultado do AdicionaConteudoLR)
                if (item.TextoDireita is not null)
                {
                    float dirW = e.Graphics.MeasureString(item.TextoDireita, item.Fonte).Width;
                    float esqMax = pageW - dirW;

                    float esqTotal = e.Graphics.MeasureString(item.Texto, item.Fonte).Width;
                    if (esqTotal <= esqMax)
                    {
                        // cabe numa linha só
                        DesenhaTextoNaLinha(e, item, item.Texto, Y, pageW);
                        e.Graphics.DrawString(item.TextoDireita, item.Fonte, Brushes.Black, pageW - dirW, Y);
                    }
                    else
                    {
                        // word-wrap da esquerda; direita fica na última linha
                        var palavras = item.Texto.Split(' ');
                        string linhaAtual = "";
                        var linhas = new System.Collections.Generic.List<string>();

                        foreach (var palavra in palavras)
                        {
                            string teste = linhaAtual + palavra + " ";
                            if (e.Graphics.MeasureString(teste, item.Fonte).Width > esqMax && !string.IsNullOrEmpty(linhaAtual))
                            {
                                linhas.Add(linhaAtual);
                                linhaAtual = palavra + " ";
                            }
                            else
                            {
                                linhaAtual = teste;
                            }
                        }
                        if (!string.IsNullOrWhiteSpace(linhaAtual))
                            linhas.Add(linhaAtual);

                        for (int li = 0; li < linhas.Count; li++)
                        {
                            DesenhaTextoNaLinha(e, item, linhas[li], Y, pageW);
                            if (li == linhas.Count - 1)
                                e.Graphics.DrawString(item.TextoDireita, item.Fonte, Brushes.Black, pageW - dirW, Y);
                            if (li < linhas.Count - 1)
                                Y += separacao;
                        }
                    }

                    Y += separacao;
                    if (Y > e.MarginBounds.Height) { e.HasMorePages = true; return; }
                    e.HasMorePages = false;
                    continue;
                }

                float larguraTotal = e.Graphics.MeasureString(item.Texto, item.Fonte).Width;

                if (larguraTotal <= pageW)
                {
                    DesenhaTextoNaLinha(e, item, item.Texto, Y, pageW);
                    Y += separacao;
                    if (Y > e.MarginBounds.Height) { e.HasMorePages = true; return; }
                    e.HasMorePages = false;
                    continue;
                }

                // word-wrap: escreve o máximo que cabe e só quebra quando a próxima palavra não couber
                var palavrasSimples = item.Texto.Split(' ');
                string linhaSimples = "";

                foreach (var palavra in palavrasSimples)
                {
                    string teste = linhaSimples + palavra + " ";
                    float testeW = e.Graphics.MeasureString(teste, item.Fonte).Width;

                    if (testeW > pageW && !string.IsNullOrEmpty(linhaSimples))
                    {
                        DesenhaTextoNaLinha(e, item, linhaSimples, Y, pageW);
                        Y += separacao;
                        linhaSimples = palavra + " ";
                    }
                    else
                    {
                        linhaSimples = teste;
                    }
                }

                if (!string.IsNullOrWhiteSpace(linhaSimples))
                    DesenhaTextoNaLinha(e, item, linhaSimples, Y, pageW);

                Y += separacao;

                if (Y > e.MarginBounds.Height) { e.HasMorePages = true; return; }
                else e.HasMorePages = false;
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message);
        }
    }

    private static void DesenhaTextoNaLinha(PrintPageEventArgs e, ClsImpressaoDefinicoes item, string texto, int Y, float pageW)
    {
        if (item.Alinhamento == Alinhamentos.Centro)
        {
            e.Graphics.DrawString(texto, item.Fonte, Brushes.Black, Centro(texto, item.Fonte, e), Y);
        }
        else if (item.Alinhamento == Alinhamentos.Direita)
        {
            SizeF tam = e.Graphics.MeasureString(texto, item.Fonte);
            if (item.eObs && DestacaObservacoes)
            {
                RectangleF rect = new RectangleF(0, Y, pageW, tam.Height);
                e.Graphics.FillRectangle(Brushes.LightSlateGray, rect);
            }
            e.Graphics.DrawString(texto, item.Fonte, Brushes.Black, pageW - tam.Width, Y);
        }
        else if (!item.eObs || !DestacaObservacoes)
        {
            e.Graphics.DrawString(texto, item.Fonte, Brushes.Black, 0, Y);
        }
        else
        {
            PointF ponto = new PointF(0, Y);
            SizeF tam = e.Graphics.MeasureString(texto, item.Fonte);
            RectangleF rect = new RectangleF(ponto, new SizeF(pageW, tam.Height));
            e.Graphics.FillRectangle(Brushes.LightSlateGray, rect);
            e.Graphics.DrawString(texto, item.Fonte, Brushes.Black, 0, Y);
        }
    }

    public static float Centro(string Texto, Font Fonte, System.Drawing.Printing.PrintPageEventArgs e)
    {
        SizeF Tamanho = e.Graphics.MeasureString(Texto, Fonte);

        float Meio = e.PageBounds.Width / 2 - Tamanho.Width / 2;

        return Meio;
    }


    #endregion

    #region Funções auxiliares de impressão
    private static string MascararTelefone(string? telefone)
    {
        if (string.IsNullOrWhiteSpace(telefone)) return "";

        var digits = new string(telefone.Where(char.IsDigit).ToArray());

        return digits.Length == 11
            ? $"({digits[..2]}) {digits[2..7]}-{digits[7..]}"  // celular: (16) 99236-6175
            : digits.Length == 10
                ? $"({digits[..2]}) {digits[2..6]}-{digits[6..]}"  // fixo: (16) 9923-6175
                : telefone; // formato desconhecido — retorna como veio
    }

    private bool VerificaSeNaoEstaSemImpressora(string impCadastrada)
    {
        return impCadastrada != "Sem Impressora" && !string.IsNullOrEmpty(impCadastrada);
    }

    public static void AdicionaConteudo(List<ClsImpressaoDefinicoes> Conteudo, string conteudoTexto, Font fonte, Alinhamentos alinhamento = Alinhamentos.Esquerda, bool eObs = false)
    {
        Conteudo.Add(new ClsImpressaoDefinicoes() { Texto = conteudoTexto, Fonte = fonte, Alinhamento = alinhamento, eObs = eObs });
    }

    public static void AdicionaConteudoLR(List<ClsImpressaoDefinicoes> Conteudo, string esq, string dir, Font fonte, bool eObs = false)
    {
        Conteudo.Add(new ClsImpressaoDefinicoes() { Texto = esq, TextoDireita = dir, Fonte = fonte, eObs = eObs });
    }

    public static string AdicionarSeparadorSimples()
    {
        return "----------------------------------------";
    }
    public static string AdicionarSeparadorDuplo()
    {
        return "========================================";
    }

    private string RetornaImpressoraSelecionadaNoCadastroDeProduto(ImpressorasConfigs Imps, string? ImpressoraCadastrada)
    {
        switch (ImpressoraCadastrada)
        {
            case "Cz1":
                return Imps.ImpressoraCz1;
            case "Cz2":
                return Imps.ImpressoraCz2;
            case "Cz3":
                return Imps.ImpressoraCz3;
            case "Cz4":
                return Imps.ImpressoraCz4;
            case "Cz5":
                return Imps.ImpressoraCz5;
            case "Bar":
                return Imps.ImpressoraBar;
            case "Bar2":
                return Imps.ImpressoraBar2;
            case "Bar3":
                return Imps.ImpressoraBar3;
            case null:
                return Imps.ImpressoraCz1;
            default:
                return Imps.ImpressoraCz1;
        }
    }

    #endregion

    #region funções de impressão de NFs
    public async void ImprimeDANFE(string CaminhoDeArquivo = @"C:\SophosCompany\Triburatios\Autorizadas-12-2025\35251262538536000112650010000000451311844160-procnfe.xml")
    {
        using (AppDbContext db = new AppDbContext())
        {
            string? ImpressoraCaixa = null;
            string? ImpressoraDanfe = null;
            ImpressorasConfigs Imps = db.Impressoras.FirstOrDefault() ?? new ImpressorasConfigs();

            if (!string.IsNullOrEmpty(Imps.ImpressoraCaixa) && VerificaSeNaoEstaSemImpressora(Imps.ImpressoraCaixa))
            {
                ImpressoraCaixa = Imps.ImpressoraCaixa;
            }
            if (!string.IsNullOrEmpty(Imps.ImpressoraDanfe) && VerificaSeNaoEstaSemImpressora(Imps.ImpressoraDanfe))
            {
                ImpressoraDanfe = Imps.ImpressoraDanfe;
            }

            var tipo = ObterTipoDeNota(CaminhoDeArquivo);

            if (tipo == "NFe")
            {
                var config = new DANFe.Configurations.UnidanfeConfiguration
                {
                    Arquivo = CaminhoDeArquivo,
                    Copias = 1,
                    Visualizar = true,
                    Imprimir = false,
                    LarguraBobina = 210,
                    Impressora = ImpressoraDanfe,
                    Logotipo = "C:\\SophosCompany\\LogoDanfe.png",

                };

                if (!string.IsNullOrEmpty(ImpressoraDanfe))
                    DANFe.UnidanfeServices.Execute(config);
            }
            else if (tipo == "NFCe")
            {
                var config = new DANFe.Configurations.UnidanfeConfiguration
                {
                    Arquivo = CaminhoDeArquivo,
                    Copias = 1,
                    Visualizar = false,
                    Imprimir = true,
                    LarguraBobina = 75,
                    Impressora = ImpressoraCaixa,
                    Logotipo = "C:\\SophosCompany\\LogoCupom.png",
                    NomeRemetente = AppState.MerchantLogado?.NomeFantasia ?? "",
                    TextoMarcaDAgua = "NFC-e - Documento Auxiliar da Nota Fiscal de Consumidor Eletrônica",
                    NomeImpressao = "NFC-e - DANFE NFC-e",
                };

                if (!string.IsNullOrEmpty(ImpressoraCaixa))
                    DANFe.UnidanfeServices.Execute(config);
            }

        }
    }

    public string ObterTipoDeNota(string caminhoXml)
    {
        XDocument xml = XDocument.Load(caminhoXml);

        XNamespace ns = "http://www.portalfiscal.inf.br/nfe";

        var modelo = xml
            .Descendants(ns + "ide")
            .Elements(ns + "mod")
            .FirstOrDefault()?.Value;

        return modelo switch
        {
            "55" => "NFe",
            "65" => "NFCe",
            _ => "Modelo desconhecido"
        };
    }
    #endregion

    private void AtualizaTamanhoDeFontesParametrizados()
    {
        if (AppState.MerchantLogado is not null)
        {
            FonteTotaisNovo = new Font("DejaVu sans mono", AppState.MerchantLogado.TamFonteTotais, FontStyle.Bold);
            FonteDetalhesDoPedido = new Font("DejaVu sans mono", AppState.MerchantLogado.TamFonteDetalhesPedido, FontStyle.Bold);
            FonteComplemento = new Font("DejaVu sans mono", AppState.MerchantLogado.TamFonteDescricaoComplemento, FontStyle.Bold);
            FonteContaEntregaEConta = new Font("DejaVu sans mono", AppState.MerchantLogado.TamFonteTempoEntregaEConta, FontStyle.Bold);
            FonteItens2 = new Font("DejaVu sans mono", AppState.MerchantLogado.TamFonteDescricaoItem, FontStyle.Bold);
            FonteCPF = new Font("DejaVu sans mono", AppState.MerchantLogado.TamFonteValorItem, FontStyle.Bold); //ta sendo o valor dos itens 
            FonteInfosPagamento = new Font("DejaVu sans mono", AppState.MerchantLogado.TamFonteInfosPag, FontStyle.Bold);
            FonteNomeDoCliente = new Font("DejaVu sans mono", AppState.MerchantLogado.TamFonteNomeClienteComanda, FontStyle.Bold);
            FonteItensComanda = new Font("DejaVu sans mono", AppState.MerchantLogado.TamFonteDescricaoItemNaComanda, FontStyle.Bold);
            FonteComplementoNaComanda = new Font("DejaVu sans mono", AppState.MerchantLogado.TamFonteDescricaoComplementoNaComanda, FontStyle.Bold);
            FonteItemFechamento = new Font("DejaVu sans mono", AppState.MerchantLogado.TamanhoItemNoFechamento, FontStyle.Bold);
            FonteTotaisFechamento = new Font("DejaVu sans mono", AppState.MerchantLogado.TamanhoTotaisFechamento, FontStyle.Bold);
            FonteFechamentoDeCaixa = new Font("DejaVu sans mono", AppState.MerchantLogado.FonteFechamentoDeCaixa, FontStyle.Bold);

            ValorEspacamento = AppState.MerchantLogado.EspacamentoNaImpressao;

            DestacaObservacoes = AppState.MerchantLogado.DestacaObsNaImpressao;
        }
    }
}

#region Classes de definição de impressão
public enum Alinhamentos
{
    Esquerda,
    Direita,
    Centro
}

public class ClsImpressaoDefinicoes
{
    public string Texto { get; set; }
    public string? TextoDireita { get; set; }
    public Font Fonte { get; set; }
    public Alinhamentos Alinhamento { get; set; }
    public bool eObs { get; set; }

    public ClsImpressaoDefinicoes() { }
}
#endregion