
using FrontMenuWeb.DTOS;
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
using DANFe = Unimake.Unidanfe;

namespace SophosSyncDesktop.Services;

public class ImpressaoService
{

    #region Fontes utilizadas na impressão
    public Font FonteSeparadoresSimples { get; set; } = new Font("DejaVu sans mono", 8, FontStyle.Bold);
    public static Font FonteCódigoDeBarras = new Font("3 of 9 Barcode", 35, FontStyle.Regular);
    public Font FonteComplemento { get; set; } = new Font("DejaVu sans mono", 9, FontStyle.Bold);
    public Font FonteComplementoNaComanda { get; set; } = new Font("DejaVu sans mono", 9, FontStyle.Bold);
    public Font FonteFechamentoDeCaixa { get; set; } = new Font("DejaVu sans mono", 8, FontStyle.Bold);
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


                if (AppState.MerchantLogado is not null)
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
                               .SelectMany(i => i.Produto == null ? new[] { new { Impressora = (string?)null, Item = i } } : new[]
                                {
                                                            new { Impressora = i.Produto.ImpressoraComanda1, Item = i },
                                                            new { Impressora = i.Produto.ImpressoraComanda2, Item = i }
                                })
                               .GroupBy(x => x.Impressora)
                               .Select(grupo => new ItensPorImpressoraDto
                               {
                                   Impressora = grupo.Key,
                                   Itens = grupo.Select(x => x.Item).ToList()
                               })
                               .ToList();

                    foreach (var Prods in produtosAgrupados)
                    {
                        if (Prods.Impressora is not null && (Prods.Impressora.Contains("Não Imprime", StringComparison.OrdinalIgnoreCase) || Prods.Impressora.Contains("Nao", StringComparison.OrdinalIgnoreCase)))
                            continue;

                        PedidoMesaDto PedidoAtualizadoComItensAgrupados = Pedido;
                        PedidoMesaDto PedidoAtualizadoComItensAgrupadosAuxiliar = Pedido;
                        PedidoAtualizadoComItensAgrupados.Itens = Prods.Itens;

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


                            /*if (AppState.MerchantLogado is not null)
                            {
                                for (var interador = 0; interador < AppState.MerchantLogado.QtdViasDaComanda; interador++)
                                    await ImprimirPagina(ConteudoParaImpressaoDoPedidoMesa, Impressora, ValorEspacamento);

                            }
                            else
                            {
                                await ImprimirPagina(ConteudoParaImpressaoDoPedidoMesa, Impressora, ValorEspacamento);
                            }*/

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
                       .SelectMany(i => i.Produto == null ? new[] { new { Impressora = (string?)null, Item = i } } : new[]
                        {
                                new { Impressora = i.Produto.ImpressoraComanda1, Item = i },
                                new { Impressora = i.Produto.ImpressoraComanda2, Item = i }
                        })
                       .GroupBy(x => x.Impressora)
                       .Select(grupo => new ItensPorImpressoraDto
                       {
                           Impressora = grupo.Key,
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




    #endregion

    #region Define as características da impressão

    #region Define Características das comandas para impressão
    private List<ClsImpressaoDefinicoes> DefineCaracteristicasDaComandaParaImpressaoMesa(PedidoMesaDto pedido, string AppQueEnviou)
    {
        List<ClsImpressaoDefinicoes> Conteudo = new List<ClsImpressaoDefinicoes>();

        if (AppState.MerchantLogado is not null)
            AdicionaConteudo(Conteudo, AppState.MerchantLogado.NomeFantasia, FonteDetalhesDoPedido, Alinhamentos.Centro);

        AdicionaConteudo(Conteudo, AdicionarSeparadorDuplo(), FonteSeparadoresSimples);
        //========================================================================================       
        AdicionaConteudo(Conteudo, $"{AppState.MerchantLogado?.LegendaNomeUltilizadoParaPlaced}: {pedido.IdentificacaoMesaOuComanda.ToString().PadLeft(2, '0')}", FonteDetalhesDoPedido, Alinhamentos.Centro);
        //========================================================================================       
        AdicionaConteudo(Conteudo, AdicionarSeparadorDuplo(), FonteSeparadoresSimples);

        //------------------------------------------------------------------------------------------
        AdicionaConteudo(Conteudo, $"Qtdade.  Descrição Do Item.", FontQtdDescVunitVTotal);
        AdicionaConteudo(Conteudo, $" Tam. ", FontQtdDescVunitVTotal);
        AdicionaConteudo(Conteudo, AdicionarSeparadorSimples(), FonteSeparadoresSimples);
        foreach (var item in pedido.Itens)
        {
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

            if (item.Complementos.Count > 0)
            {
                AdicionaConteudo(Conteudo, $"\n", FonteCPF);
                foreach (var complemento in item.Complementos)
                {
                    AdicionaConteudo(Conteudo, $"{complemento.Quantidade}- {complemento.Descricao} - {complemento.PrecoTotal.ToString("C")}", FonteComplementoNaComanda, eObs: true);
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

    private List<ClsImpressaoDefinicoes> DefineCaracteristicasDaComandaParaImpressaoDeliveryEBalcao(ClsPedido pedido, string AppQueEnviou, bool ItemSeparadoPorComanda = false, float qtdItens = 0, int IndiceDoItemAtual = 0)
    {
        List<ClsImpressaoDefinicoes> Conteudo = new List<ClsImpressaoDefinicoes>();

        if (AppState.MerchantLogado is not null)
            AdicionaConteudo(Conteudo, AppState.MerchantLogado.NomeFantasia, FonteDetalhesDoPedido, Alinhamentos.Centro);
        AdicionaConteudo(Conteudo, AdicionarSeparadorDuplo(), FonteSeparadoresSimples);
        //========================================================================================       

        if (pedido.TipoDePedido != "DELIVERY")
        {
            int seed = pedido.Id;
            var random = new Random(seed);

            int senha = random.Next(500, 999);

            AdicionaConteudo(Conteudo, $"SENHA: {senha}", FonteLegendaDoTamanho, Alinhamentos.Centro);
            AdicionaConteudo(Conteudo, AdicionarSeparadorDuplo(), FonteSeparadoresSimples);
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
        AdicionaConteudo(Conteudo, $"Controle Interno \t Sem valor fiscal", FonteCPF);
        AdicionaConteudo(Conteudo, $"Criado em: {pedido.CriadoEm:G}", FonteDetalhesDoPedido);
        AdicionaConteudo(Conteudo, $"Pedido criado por {pedido.CriadoPor}", FonteDetalhesDoPedido);

        AdicionaConteudo(Conteudo, $"Controle: {(pedido.TipoDePedido == "CHECKOUT" ? "CAIXA" : pedido.TipoDePedido)}", FonteDetalhesDoPedido);
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
        AdicionaConteudo(Conteudo, $"Qtdade.  Descrição Do Item.", FontQtdDescVunitVTotal);
        AdicionaConteudo(Conteudo, $"Tam.          V.Unit.   Total.", FontQtdDescVunitVTotal);
        AdicionaConteudo(Conteudo, AdicionarSeparadorSimples(), FonteSeparadoresSimples);
        foreach (var item in pedido.Itens)
        {
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
            AdicionaConteudo(Conteudo, $"                      {item.PrecoUnitario:F2}     {item.PrecoTotal:F2}", FonteCPF);

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

            AdicionaConteudo(Conteudo, AdicionarSeparadorSimples(), FonteSeparadoresSimples);
            //------------------------------------------------------------------------------------------
        }

        if (pedido.ValorDosItens > 0)
            AdicionaConteudo(Conteudo, $"SUB TOTAL. . . .  : {pedido.ValorDosItens:F2}", FonteTotaisNovo);
        if (pedido.TaxaEntregaValor > 0)
            AdicionaConteudo(Conteudo, $"TAXA DE ENTREGA . : {pedido.TaxaEntregaValor:F2}", FonteTotaisNovo);
        if (pedido.AcrescimoValor > 0)
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


        foreach (var pagamento in pedido.Pagamentos)
        {
            if (pagamento.FormaDePagamento is not null)
            {
                bool ePedidoPagoOnline = pagamento.FormaDePagamento.PagamentoOnline;
                var InfoSeSeraPago = ePedidoPagoOnline ? "PAGO ONLINE COM" : "PEDIDO SERÁ PAGO COM";

                AdicionaConteudo(Conteudo, $"{InfoSeSeraPago} ({pagamento.FormaDePagamento.Descricao}) -- VALOR: {pedido.ValorTotal.ToString("C")}", FonteInfosPagamento);
                if (pagamento.FormaDePagamento.EDinheiro && pagamento.Troco > 0)
                {
                    AdicionaConteudo(Conteudo, $"LEVAR TROCO: {pagamento.Troco.ToString("C")}", FonteInfosPagamento);
                }
            }
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

    #region Definição do fechamento de caixa e motoboy para impressão
    private List<ClsImpressaoDefinicoes> DefineCaracteristicasDoFechamentoParaImpressao(ClsFechamentoDeCaixa Fechamento)
    {
        List<ClsImpressaoDefinicoes> Conteudo = new List<ClsImpressaoDefinicoes>();

        // Helper: alinha label (22 chars) + sinal (5 chars) + valor (12 chars) = 39 chars total
        string Ln(string label, string sinal, float valor)
        {
            var lbl = label.Length >= 22 ? label[..22] : label.PadRight(22, '.');
            return $"{lbl}:({sinal}) {valor.ToString("C").PadLeft(11)}";
        }

        var faturamentoBruto = Fechamento.ValorTotalEmVendas;
        var totalDoCaixa = Fechamento.ValorDeAbertura + Fechamento.Suprimentos - Fechamento.Sangrias + faturamentoBruto;
        var totalRecebimentos = Fechamento.RecebimentosPorTipo?.Values.Sum() ?? 0f;
        var labelFaltouSobrou = Fechamento.Faltou > 0 ? "FALTOU" : "SOBROU";
        var valorAbsFaltou = Math.Abs(Fechamento.Faltou);

        if (AppState.MerchantLogado is not null)
            AdicionaConteudo(Conteudo, AppState.MerchantLogado.NomeFantasia, FonteDetalhesDoPedido, Alinhamentos.Centro);
        AdicionaConteudo(Conteudo, AdicionarSeparadorDuplo(), FonteSeparadoresSimples);
        AdicionaConteudo(Conteudo, "FECHAMENTO DO CAIXA", FonteFechamentoDeCaixa, Alinhamentos.Centro);
        AdicionaConteudo(Conteudo, AdicionarSeparadorDuplo(), FonteFechamentoDeCaixa);

        // OPERAÇÃO DE VENDAS
        AdicionaConteudo(Conteudo, "OPERAÇÃO DE VENDAS", FonteFechamentoDeCaixa);
        AdicionaConteudo(Conteudo, AdicionarSeparadorSimples(), FonteSeparadoresSimples);
        AdicionaConteudo(Conteudo, Ln("TOTAL DAS VENDAS", "+", Fechamento.ValorTotalEmVendas), FonteFechamentoDeCaixa);
        AdicionaConteudo(Conteudo, Ln("ACRESCIMOS", "+", Fechamento.TotalDeArescimos), FonteFechamentoDeCaixa);
        AdicionaConteudo(Conteudo, Ln("CORTESIA/DESCONTOS", "-", Fechamento.TotalEmDescontos + Fechamento.TotalEmIncentivos), FonteFechamentoDeCaixa);
        AdicionaConteudo(Conteudo, Ln("TAXAS DE ENTREGA", "+", Fechamento.TotalTaxaEntrega), FonteFechamentoDeCaixa);
        AdicionaConteudo(Conteudo, AdicionarSeparadorSimples(), FonteSeparadoresSimples);
        AdicionaConteudo(Conteudo, Ln("FATURAMENTO BRUTO", "=", faturamentoBruto), FonteFechamentoDeCaixa);
        AdicionaConteudo(Conteudo, AdicionarSeparadorSimples(), FonteSeparadoresSimples);

        // MOVIMENTAÇÕES DO CAIXA
        AdicionaConteudo(Conteudo, Ln("CAIXA INICIAL", "+", Fechamento.ValorDeAbertura), FonteFechamentoDeCaixa);
        AdicionaConteudo(Conteudo, Ln("ENTRADAS DO CAIXA", "+", Fechamento.Suprimentos), FonteFechamentoDeCaixa);
        AdicionaConteudo(Conteudo, Ln("SAÍDAS DO CAIXA", "-", Fechamento.Sangrias), FonteFechamentoDeCaixa);
        AdicionaConteudo(Conteudo, Ln("TROCOS", "-", Fechamento.Trocos), FonteFechamentoDeCaixa);
        AdicionaConteudo(Conteudo, AdicionarSeparadorSimples(), FonteSeparadoresSimples);
        AdicionaConteudo(Conteudo, Ln("TOTAL DO CAIXA", "=", totalDoCaixa), FonteFechamentoDeCaixa);
        AdicionaConteudo(Conteudo, AdicionarSeparadorSimples(), FonteSeparadoresSimples);

        // CONTAGEM FÍSICA
        AdicionaConteudo(Conteudo, "CONTAGEM FÍSICA DO CAIXA", FonteFechamentoDeCaixa);
        AdicionaConteudo(Conteudo, AdicionarSeparadorSimples(), FonteSeparadoresSimples);
        AdicionaConteudo(Conteudo, Ln("VALOR ESPERADO DIN.", "=", Fechamento.ValorEsperadoEmDinheiro), FonteFechamentoDeCaixa);
        AdicionaConteudo(Conteudo, AdicionarSeparadorSimples(), FonteSeparadoresSimples);

        if (valorAbsFaltou > 0)
        {
            AdicionaConteudo(Conteudo, Ln(labelFaltouSobrou, "=", valorAbsFaltou), FonteFechamentoDeCaixa);
            AdicionaConteudo(Conteudo, AdicionarSeparadorSimples(), FonteSeparadoresSimples);
        }

        // DISTRIBUIÇÃO POR FORMA
        AdicionaConteudo(Conteudo, "DISTRIBUIÇÃO POR FORMA DE RECEBIMENTO", FonteFechamentoDeCaixa, Alinhamentos.Centro);
        AdicionaConteudo(Conteudo, AdicionarSeparadorSimples(), FonteSeparadoresSimples);

        foreach (var pagamento in Fechamento.RecebimentosPorTipo ?? [])
        {
            AdicionaConteudo(Conteudo, Ln(pagamento.Key, "+", pagamento.Value), FonteFechamentoDeCaixa);
        }

        AdicionaConteudo(Conteudo, AdicionarSeparadorSimples(), FonteSeparadoresSimples);
        AdicionaConteudo(Conteudo, Ln("TOTAL RECEBIMENTOS", "=", totalRecebimentos), FonteFechamentoDeCaixa);
        AdicionaConteudo(Conteudo, AdicionarSeparadorSimples(), FonteSeparadoresSimples);

        // RESUMO POR TIPO
        AdicionaConteudo(Conteudo, Ln("VENDAS À VISTA", "=", Fechamento.TotalEmDinheiro), FonteFechamentoDeCaixa);
        AdicionaConteudo(Conteudo, Ln("VENDAS EM CARTÃO", "=", Fechamento.TotalEmCartoes), FonteFechamentoDeCaixa);
        AdicionaConteudo(Conteudo, Ln("PGTOS ONLINE", "=", Fechamento.PagoOnline), FonteFechamentoDeCaixa);
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
                printDocument.DefaultPageSettings.Margins = new Margins(10, 10, 10, 10);
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

            foreach (var item in conteudo)
            {
                var tamanhoFrase = e.Graphics.MeasureString(item.Texto, item.Fonte).Width;

                if (tamanhoFrase < e.PageBounds.Width)
                {
                    if (item.Alinhamento == Alinhamentos.Centro)
                    {
                        e.Graphics.DrawString(item.Texto, item.Fonte, Brushes.Black, Centro(item.Texto, item.Fonte, e), Y);
                    }
                    else if (!item.eObs)
                    {
                        e.Graphics.DrawString(item.Texto, item.Fonte, Brushes.Black, 0, Y);
                        Y += separacao;
                        continue;
                    }
                    else if (item.eObs)
                    {
                        PointF ponto = new PointF(0, Y);

                        SizeF tamanhoTexto = e.Graphics.MeasureString(item.Texto, item.Fonte);
                        RectangleF retanguloTexto = new RectangleF(ponto, new SizeF(e.PageBounds.Width, tamanhoTexto.Height));

                        e.Graphics.FillRectangle(Brushes.LightSlateGray, retanguloTexto);
                        e.Graphics.DrawString(item.Texto, item.Fonte, Brushes.Black, 0, Y);

                        Y += separacao;

                        continue;
                    }
                }


                var listPalavras = item.Texto.Split(" ").ToList();
                string frase = "";

                foreach (var palavra in listPalavras)
                {

                    frase += palavra + " ";

                    tamanhoFrase = e.Graphics.MeasureString(frase, item.Fonte).Width;

                    if (tamanhoFrase > e.PageBounds.Width - 70 && frase != "")
                    {
                        if (item.Alinhamento == Alinhamentos.Centro)
                        {

                            e.Graphics.DrawString(frase, item.Fonte, Brushes.Black, Centro(item.Texto, item.Fonte, e), Y);
                            Y += separacao;
                            frase = "";
                            continue;

                        }
                        else if (!item.eObs)
                        {
                            e.Graphics.DrawString(frase, item.Fonte, Brushes.Black, 0, Y);
                            Y += separacao;
                            frase = "";
                            continue;
                        }
                        else if (item.eObs)
                        {
                            PointF ponto = new PointF(0, Y);

                            SizeF tamanhoTexto = e.Graphics.MeasureString(frase, item.Fonte);
                            RectangleF retanguloTexto = new RectangleF(ponto, new SizeF(e.PageBounds.Width, tamanhoTexto.Height));

                            e.Graphics.FillRectangle(Brushes.LightSlateGray, retanguloTexto);
                            e.Graphics.DrawString(frase, item.Fonte, Brushes.Black, 0, Y);

                            Y += separacao;
                            frase = "";

                            continue;
                        }

                    }

                    if (frase != "")
                    {
                        if (item.Alinhamento == Alinhamentos.Centro)
                        {

                            e.Graphics.DrawString(frase, item.Fonte, Brushes.Black, Centro(item.Texto, item.Fonte, e), Y);

                        }
                        else if (!item.eObs)
                        {
                            e.Graphics.DrawString(frase, item.Fonte, Brushes.Black, 0, Y);

                        }
                        else if (item.eObs)
                        {
                            PointF ponto = new PointF(0, Y);

                            SizeF tamanhoTexto = e.Graphics.MeasureString(frase, item.Fonte);
                            RectangleF retanguloTexto = new RectangleF(ponto, new SizeF(e.PageBounds.Width, tamanhoTexto.Height));


                            e.Graphics.FillRectangle(Brushes.LightSlateGray, retanguloTexto);
                            e.Graphics.DrawString(frase, item.Fonte, Brushes.Black, 0, Y);

                            //continue;
                        }

                    }

                }

                frase = "";
                Y += separacao;
            }


        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message);
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

    public static string AdicionarSeparadorSimples()
    {
        return "---------------------------------------";
    }
    public static string AdicionarSeparadorDuplo()
    {
        return "=======================================";
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
            case "Bar":
                return Imps.ImpressoraBar;
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
            //FontQtdDescVunitVTotal = new Font("DejaVu sans mono", AppState.MerchantLogado.TamFonteLegendaDosItens, FontStyle.Regular);
            FonteContaEntregaEConta = new Font("DejaVu sans mono", AppState.MerchantLogado.TamFonteTempoEntregaEConta, FontStyle.Bold);
            FonteItens2 = new Font("DejaVu sans mono", AppState.MerchantLogado.TamFonteDescricaoItem, FontStyle.Bold);
            FonteCPF = new Font("DejaVu sans mono", AppState.MerchantLogado.TamFonteValorItem, FontStyle.Bold); //ta sendo o valor dos itens 
            FonteInfosPagamento = new Font("DejaVu sans mono", AppState.MerchantLogado.TamFonteInfosPag, FontStyle.Bold);
            FonteNomeDoCliente = new Font("DejaVu sans mono", AppState.MerchantLogado.TamFonteNomeClienteComanda, FontStyle.Bold);
            FonteItensComanda = new Font("DejaVu sans mono", AppState.MerchantLogado.TamFonteDescricaoItemNaComanda, FontStyle.Bold);
            FonteComplementoNaComanda = new Font("DejaVu sans mono", AppState.MerchantLogado.TamFonteDescricaoComplementoNaComanda, FontStyle.Bold);

            ValorEspacamento = AppState.MerchantLogado.EspacamentoNaImpressao;
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
    public Font Fonte { get; set; }
    public Alinhamentos Alinhamento { get; set; }
    public bool eObs { get; set; }


    public ClsImpressaoDefinicoes() { }
}
#endregion