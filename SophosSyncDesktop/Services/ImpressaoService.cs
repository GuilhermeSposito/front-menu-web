using FrontMenuWeb.DTOS;
using FrontMenuWeb.Models.Pedidos;
using FrontMenuWeb.Models.Vendas;
using Org.BouncyCastle.Crypto;
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
using System.Xml.Linq;
using DANFe = Unimake.Unidanfe;

namespace SophosSyncDesktop.Services;

public class ImpressaoService
{

    #region Fontes utilizadas na impressão
    public Font FonteSeparadoresSimples { get; set; } = new Font("DejaVu sans mono", 8, FontStyle.Bold);
    public Font FonteComplemento { get; set; } = new Font("DejaVu sans mono", 9, FontStyle.Bold);
    public Font FonteComplementoNaComanda { get; set; } = new Font("DejaVu sans mono", 9, FontStyle.Bold);
    public Font FonteDetalhesDoPedido { get; set; } = new Font("DejaVu sans mono", 9, FontStyle.Bold);
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
    public Font FonteLegendaDoTamanho { get; set; } = new Font("DejaVu sans mono", 12, FontStyle.Bold);
    public Font FonteSophos { get; set; } = new Font("Montserrat", 15, FontStyle.Bold);
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
                if (!string.IsNullOrEmpty(Imps.ImpressoraCaixa) && VerificaSeEstaSemImpressora(Imps.ImpressoraCaixa))
                {
                    List<ClsImpressaoDefinicoes> ConteudoParaImpressaoDoPedido = DefineCaracteristicasDePedidoParaImpressao(Pedido, AppQueEnviou);
                    await ImprimirPagina(ConteudoParaImpressaoDoPedido, Imps.ImpressoraCaixa, ValorEspacamento);
                }

                //imprime na impressora auxiliar se tiver
                if (!string.IsNullOrEmpty(Imps.ImpressoraAux) && !VerificaSeEstaSemImpressora(Imps.ImpressoraAux))
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
                        PedidoAtualizadoComItensAgrupados.Itens = Prods.Itens;


                        string Impressora = RetornaImpressoraSelecionadaNoCadastroDeProduto(Imps, Prods.Impressora);
                        if (Impressora == "Sem Impressora" || Impressora == "Não Imprime")
                            continue;

                        var QtdDeLoops = 1;

                        if (AppState.MerchantLogado is not null)
                            QtdDeLoops = AppState.MerchantLogado!.ImprimeComandasSeparadaPorProdutos ? Prods.Itens.Count() : 1;

                        for (var i = 0; i < QtdDeLoops; i++)
                        {
                            if (QtdDeLoops > 1)//se for maior que 1 é porque é separado por item
                            {
                                var ItemAtual = Prods.Itens[i];
                                PedidoAtualizadoComItensAgrupados.Itens = new List<ItensPedido> { ItemAtual };
                            }
                            List<ClsImpressaoDefinicoes> ConteudoParaImpressaoDoPedidoMesa = DefineCaracteristicasDaComandaParaImpressaoMesa(PedidoAtualizadoComItensAgrupados, AppQueEnviou);

                            if (AppState.MerchantLogado is not null)
                            {
                                for (var interador = 0; interador < AppState.MerchantLogado.QtdViasDaComanda; interador++)
                                    await ImprimirPagina(ConteudoParaImpressaoDoPedidoMesa, Impressora, ValorEspacamento);

                            }
                            else
                            {
                                await ImprimirPagina(ConteudoParaImpressaoDoPedidoMesa, Impressora, ValorEspacamento);
                            }

                        }
                    }

                }
                else  //merchant nunca vai entrar nulo aqui
                {
                    ImpressorasConfigs Imps = db.Impressoras.FirstOrDefault() ?? new ImpressorasConfigs();
                    ClsPedido Pedido = JsonSerializer.Deserialize<ClsPedido>(jsonDoPedido) ?? throw new Exception("Erro ao desserializr pedido");

                    int QtdDeItensDoPedido = Pedido.Itens.Count();

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
                        PedidoAtualizadoComItensAgrupados.Itens = Prods.Itens;

                        string Impressora = RetornaImpressoraSelecionadaNoCadastroDeProduto(Imps, Prods.Impressora);
                        if (Impressora == "Sem Impressora")
                            continue;

                        var QtdDeLoops = AppState.MerchantLogado!.ImprimeComandasSeparadaPorProdutos ? Prods.Itens.Count() : 1;

                        for (var i = 0; i < QtdDeLoops; i++)
                        {
                            if (QtdDeLoops > 1)//se for maior que 1 é porque é separado por item
                            {
                                var ItemAtual = Prods.Itens[i];
                                PedidoAtualizadoComItensAgrupados.Itens = new List<ItensPedido> { ItemAtual };
                            }

                            List<ClsImpressaoDefinicoes> ConteudoParaImpressaoDoPedido = DefineCaracteristicasDaComandaParaImpressaoDeliveryEBalcao(PedidoAtualizadoComItensAgrupados, AppQueEnviou, AppState.MerchantLogado!.ImprimeComandasSeparadaPorProdutos, QtdDeItensDoPedido, i);

                            if ((Pedido.TipoDePedido == "DELIVERY" && AppState.MerchantLogado!.ImprimeComandasDelivery) || (Pedido.TipoDePedido == "BALCÃO" && AppState.MerchantLogado!.ImprimeComandasBalcao))
                                for (var interador = 0; interador < AppState.MerchantLogado.QtdViasDaComanda; interador++)
                                    await ImprimirPagina(ConteudoParaImpressaoDoPedido, Impressora, ValorEspacamento);
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
                if (!string.IsNullOrEmpty(Imps.ImpressoraCaixa) && VerificaSeEstaSemImpressora(Imps.ImpressoraCaixa))
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
        AdicionaConteudo(Conteudo, $"MESA: {pedido.IdentificacaoMesaOuComanda.ToString().PadLeft(2, '0')}", FonteDetalhesDoPedido, Alinhamentos.Centro);

        //------------------------------------------------------------------------------------------
        AdicionaConteudo(Conteudo, $"Qtdade.  Descrição Do Item.", FontQtdDescVunitVTotal);
        AdicionaConteudo(Conteudo, $"              Tam.  V.Unit.   Total.", FontQtdDescVunitVTotal);
        AdicionaConteudo(Conteudo, AdicionarSeparadorSimples(), FonteSeparadoresSimples);
        foreach (var item in pedido.Itens)
        {
            AdicionaConteudo(Conteudo, $"{item.Quantidade}X  {item.Descricao}", FonteItensComanda);
            AdicionaConteudo(Conteudo, $"                      {item.PrecoUnitario:F2}     {item.PrecoTotal:F2}", FonteCPF);

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

        AdicionaConteudo(Conteudo, AdicionarSeparadorSimples(), FonteSeparadoresSimples);
        AdicionaConteudo(Conteudo, "Sophos - WEB", FonteSophos, Alinhamentos.Centro);
        AdicionaConteudo(Conteudo, "www.sophos-erp.com.br", FonteCPF, Alinhamentos.Centro);

        return Conteudo;
    }

    private List<ClsImpressaoDefinicoes> DefineCaracteristicasDaComandaParaImpressaoDeliveryEBalcao(ClsPedido pedido, string AppQueEnviou, bool ItemSeparadoPorComanda = false, int qtdItens = 0, int IndiceDoItemAtual = 0)
    {
        List<ClsImpressaoDefinicoes> Conteudo = new List<ClsImpressaoDefinicoes>();

        if (AppState.MerchantLogado is not null)
            AdicionaConteudo(Conteudo, AppState.MerchantLogado.NomeFantasia, FonteDetalhesDoPedido, Alinhamentos.Centro);

        AdicionaConteudo(Conteudo, AdicionarSeparadorDuplo(), FonteSeparadoresSimples);
        //========================================================================================       
        AdicionaConteudo(Conteudo, $"Pedido {pedido.TipoDePedido}", FonteDetalhesDoPedido);
        AdicionaConteudo(Conteudo, $"Entregar em até {AppState.MerchantLogado?.TempoDeRetiradaEMmin} Minutos", FonteDetalhesDoPedido);
        AdicionaConteudo(Conteudo, AdicionarSeparadorDuplo(), FonteSeparadoresSimples);
        //========================================================================================       

        AdicionaConteudo(Conteudo, $"Conta Nº:   {pedido.DisplayId}", FonteContaEntregaEConta);
        AdicionaConteudo(Conteudo, $"Controle: {pedido.TipoDePedido}", FonteDetalhesDoPedido);
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
            int IndiceDoItemAtualMaisUm = IndiceDoItemAtual + 1; //isso pra não mudar o valor original que veio do for 

            AdicionaConteudo(Conteudo, $"Item: {IndiceDoItemAtualMaisUm}/{qtdItens}", FonteDetalhesDoPedido);
        }

        AdicionaConteudo(Conteudo, $"Qtdade.  Descrição Do Item.", FontQtdDescVunitVTotal);
        AdicionaConteudo(Conteudo, $"Tam.                V.Unit.   Total.", FontQtdDescVunitVTotal);
        AdicionaConteudo(Conteudo, AdicionarSeparadorSimples(), FonteSeparadoresSimples);
        foreach (var item in pedido.Itens)
        {
            AdicionaConteudo(Conteudo, $"{item.Quantidade}X  {item.Descricao}", FonteItensComanda);
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
    #endregion

    #region Definição do pedido para impressão
    private List<ClsImpressaoDefinicoes> DefineCaracteristicasDePedidoParaImpressao(ClsPedido pedido, string AppQueEnviou)
    {
        List<ClsImpressaoDefinicoes> Conteudo = new List<ClsImpressaoDefinicoes>();

        if (AppState.MerchantLogado is not null)
            AdicionaConteudo(Conteudo, AppState.MerchantLogado.NomeFantasia, FonteDetalhesDoPedido, Alinhamentos.Centro);

        AdicionaConteudo(Conteudo, AdicionarSeparadorDuplo(), FonteSeparadoresSimples);
        //========================================================================================       
        AdicionaConteudo(Conteudo, $"Controle Interno \t Sem valor fiscal", FonteCPF);
        AdicionaConteudo(Conteudo, $"Criado às: {pedido.CriadoEm:t}", FonteDetalhesDoPedido);
        AdicionaConteudo(Conteudo, $"Pedido criado por {pedido.CriadoPor}", FonteDetalhesDoPedido);

        AdicionaConteudo(Conteudo, $"Conta Nº:   {pedido.DisplayId}", FonteContaEntregaEConta);
        AdicionaConteudo(Conteudo, $"Controle: {pedido.TipoDePedido}", FonteDetalhesDoPedido);
        AdicionaConteudo(Conteudo, AdicionarSeparadorDuplo(), FonteSeparadoresSimples);
        //------------------------------------------------------------------------------------------
        AdicionaConteudo(Conteudo, $"Entregar Até: {pedido.CriadoEm:t}", FonteContaEntregaEConta);
        AdicionaConteudo(Conteudo, AdicionarSeparadorSimples(), FonteSeparadoresSimples);

        //------------------------------------------------------------------------------------------

        if (pedido.Cliente is not null)
        {
            AdicionaConteudo(Conteudo, pedido.Cliente.Nome, FonteDetalhesDoPedido);
            AdicionaConteudo(Conteudo, $"Telefone: {pedido.Cliente.Telefone}", FonteDetalhesDoPedido);

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
            AdicionaConteudo(Conteudo, $"{item.Quantidade}X  {item.Descricao}", FonteItens2);
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

        AdicionaConteudo(Conteudo, $"SUB TOTAL. . . .  : {pedido.ValorDosItens:F2}", FonteTotaisNovo);
        AdicionaConteudo(Conteudo, $"TAXA DE ENTREGA . : {pedido.TaxaEntregaValor:F2}", FonteTotaisNovo);
        AdicionaConteudo(Conteudo, $"TAXAS ADICIONAIS  : {(pedido.AcrescimoValor + pedido.ServicoValor):F2} ", FonteTotaisNovo);
        AdicionaConteudo(Conteudo, $"DESCONTOS. . . .  : {pedido.DescontoValor:F2}", FonteTotaisNovo);
        AdicionaConteudo(Conteudo, $"INCENTIVOS . . .  : {pedido.IncentivosExternosValor:F2}", FonteTotaisNovo);
        AdicionaConteudo(Conteudo, AdicionarSeparadorSimples(), FonteSeparadoresSimples);
        //------------------------------------------------------------------------------------------
        AdicionaConteudo(Conteudo, $"TOTAL DA CONTA .  : {pedido.ValorTotal:F2}", FonteTotaisNovo);


        AdicionaConteudo(Conteudo, AdicionarSeparadorSimples(), FonteSeparadoresSimples);
        //------------------------------------------------------------------------------------------


        foreach (var pagamento in pedido.Pagamentos)
        {
            if (pagamento.FormaDePagamento is not null)
            {
                AdicionaConteudo(Conteudo, $"Pedido será pago com {pagamento.FormaDePagamento.Descricao} -- Valor: {pedido.ValorTotal.ToString("C")}", FonteInfosPagamento);
                if (pagamento.FormaDePagamento.EDinheiro && pagamento.Troco > 0)
                {
                    AdicionaConteudo(Conteudo, $"Leva Troco: {pagamento.Troco.ToString("C")}", FonteInfosPagamento);
                }
            }
        }


        AdicionaConteudo(Conteudo, AdicionarSeparadorSimples(), FonteSeparadoresSimples);
        AdicionaConteudo(Conteudo, "Sophos - WEB", FonteSophos, Alinhamentos.Centro);
        AdicionaConteudo(Conteudo, "www.sophos-erp.com.br", FonteCPF, Alinhamentos.Centro);

        return Conteudo;
    }
    #endregion

    #region Definição do fechamento de caixa para impressão
    private List<ClsImpressaoDefinicoes> DefineCaracteristicasDoFechamentoParaImpressao(ClsFechamentoDeCaixa Fechamento)
    {
        List<ClsImpressaoDefinicoes> Conteudo = new List<ClsImpressaoDefinicoes>();

        AdicionaConteudo(Conteudo, "Sophos Testes", FonteDetalhesDoPedido, Alinhamentos.Centro);
        AdicionaConteudo(Conteudo, AdicionarSeparadorDuplo(), FonteSeparadoresSimples);
        //========================================================================================        
        AdicionaConteudo(Conteudo, $"FECHAMENTO DO CAIXA", FonteFechamentoDeCaixa, Alinhamentos.Centro);
        AdicionaConteudo(Conteudo, $"Realizado Por: . admin", FonteFechamentoDeCaixa);
        AdicionaConteudo(Conteudo, $" ", FonteFechamentoDeCaixa);
        AdicionaConteudo(Conteudo, AdicionarSeparadorDuplo(), FonteFechamentoDeCaixa);
        //========================================================================================        
        AdicionaConteudo(Conteudo, AdicionarSeparadorSimples(), FonteFechamentoDeCaixa);
        //------------------------------------------------------------------------------------------
        AdicionaConteudo(Conteudo, $"OPERAÇÃO DE VENDAS", FonteFechamentoDeCaixa);
        //------------------------------------------------------------------------------------------
        AdicionaConteudo(Conteudo, AdicionarSeparadorSimples(), FonteSeparadoresSimples);
        AdicionaConteudo(Conteudo, $"TOTAL DAS VENDAS. . . .: (+)   {Fechamento.ValorTotalEmVendas.ToString("C")}", FonteFechamentoDeCaixa);
        AdicionaConteudo(Conteudo, $"ACRESCIMOS. . . . . . .: (+)   {Fechamento.TotalDeArescimos.ToString("C")}", FonteFechamentoDeCaixa);
        AdicionaConteudo(Conteudo, $"CORTESIA/DESCONTOS. . .: (-)   {(Fechamento.TotalEmDescontos + Fechamento.TotalEmIncentivos).ToString("C")}", FonteFechamentoDeCaixa);
        AdicionaConteudo(Conteudo, $"TAXAS DE ENTREGA. . . .: (+)   {Fechamento.TotalTaxaEntrega.ToString("C")}", FonteFechamentoDeCaixa);
        AdicionaConteudo(Conteudo, AdicionarSeparadorSimples(), FonteSeparadoresSimples);
        //------------------------------------------------------------------------------------------
        AdicionaConteudo(Conteudo, $"FATURAMENTO BRUTO.. . .: (+)   {Fechamento.ValorTotalEmVendas.ToString("C")}", FonteFechamentoDeCaixa);
        AdicionaConteudo(Conteudo, AdicionarSeparadorSimples(), FonteSeparadoresSimples);
        //------------------------------------------------------------------------------------------
        AdicionaConteudo(Conteudo, $"CAIXA INICIAL . . . . .: (+)   {Fechamento.ValorDeAbertura.ToString("C")}", FonteFechamentoDeCaixa);
        AdicionaConteudo(Conteudo, $"ENTRADAS DO CAIXA . . .: (+)   {Fechamento.Suprimentos.ToString("C")}", FonteFechamentoDeCaixa);
        AdicionaConteudo(Conteudo, $"SAÍDAS DO CAIXA . . . .: (+)   {Fechamento.Sangrias.ToString("C")}", FonteFechamentoDeCaixa);
        AdicionaConteudo(Conteudo, AdicionarSeparadorSimples(), FonteSeparadoresSimples);
        //------------------------------------------------------------------------------------------
        AdicionaConteudo(Conteudo, $"TOTAL DO CAIXA. . . . .: (+)   {Fechamento.ValorTotalEmVendas.ToString("C")}", FonteFechamentoDeCaixa);
        AdicionaConteudo(Conteudo, AdicionarSeparadorSimples(), FonteSeparadoresSimples);
        //------------------------------------------------------------------------------------------
        AdicionaConteudo(Conteudo, $" ", FonteFechamentoDeCaixa);
        AdicionaConteudo(Conteudo, AdicionarSeparadorSimples(), FonteSeparadoresSimples);
        //------------------------------------------------------------------------------------------
        AdicionaConteudo(Conteudo, $"CONTAGEM FÍSICA DO CAIXA", FonteFechamentoDeCaixa);
        AdicionaConteudo(Conteudo, AdicionarSeparadorSimples(), FonteSeparadoresSimples);
        //------------------------------------------------------------------------------------------
        AdicionaConteudo(Conteudo, $" ", FonteFechamentoDeCaixa);
        AdicionaConteudo(Conteudo, AdicionarSeparadorSimples(), FonteSeparadoresSimples);
        //------------------------------------------------------------------------------------------
        AdicionaConteudo(Conteudo, $"VALOR ESPERADO EM DIN. : (=)  {Fechamento.ValorEsperadoEmDinheiro.ToString("C")}", FonteFechamentoDeCaixa);
        AdicionaConteudo(Conteudo, AdicionarSeparadorSimples(), FonteSeparadoresSimples);
        //------------------------------------------------------------------------------------------
        AdicionaConteudo(Conteudo, $"FALTOU . . . . . . . . : (=)  {Fechamento.Faltou.ToString("C")}", FonteFechamentoDeCaixa);
        AdicionaConteudo(Conteudo, AdicionarSeparadorSimples(), FonteSeparadoresSimples);
        //------------------------------------------------------------------------------------------
        AdicionaConteudo(Conteudo, $" ", FonteFechamentoDeCaixa);
        AdicionaConteudo(Conteudo, AdicionarSeparadorSimples(), FonteSeparadoresSimples);
        //------------------------------------------------------------------------------------------
        AdicionaConteudo(Conteudo, $"DISTRIBUIÇÃO DAS FORMAS DE", FonteFechamentoDeCaixa, Alinhamentos.Centro);
        AdicionaConteudo(Conteudo, $"  RECEBIMENTO DO CAIXA   ", FonteFechamentoDeCaixa, Alinhamentos.Centro);
        AdicionaConteudo(Conteudo, AdicionarSeparadorSimples(), FonteSeparadoresSimples);
        //------------------------------------------------------------------------------------------
        foreach (var pagamento in Fechamento.RecebimentosPorTipo ?? [])
        {
            AdicionaConteudo(Conteudo, $"{pagamento.Key} . . . . . . :   {pagamento.Value.ToString("C")}", FonteFechamentoDeCaixa);
        }
        AdicionaConteudo(Conteudo, AdicionarSeparadorSimples(), FonteSeparadoresSimples);
        //------------------------------------------------------------------------------------------
        AdicionaConteudo(Conteudo, $"TOTAL DOS RECEBIMENTOS : (=)  {Fechamento.ValorTotalEmVendas.ToString("C")}", FonteFechamentoDeCaixa);
        AdicionaConteudo(Conteudo, AdicionarSeparadorSimples(), FonteSeparadoresSimples);
        //------------------------------------------------------------------------------------------
        AdicionaConteudo(Conteudo, $"VENDAS A VISTA . . . . : (=)  {Fechamento.TotalEmDinheiro.ToString("C")}", FonteFechamentoDeCaixa);
        AdicionaConteudo(Conteudo, $"VENDAS EM CARTÃO . . . : (=)  {Fechamento.TotalEmCartoes.ToString("C")}", FonteFechamentoDeCaixa);
        AdicionaConteudo(Conteudo, $"VENDAS COM PGTOS ONLINE: (=)  {0.ToString("C")}", FonteFechamentoDeCaixa);
        AdicionaConteudo(Conteudo, AdicionarSeparadorSimples(), FonteSeparadoresSimples);
        //------------------------------------------------------------------------------------------

        AdicionaConteudo(Conteudo, "Sophos - WEB", FonteSophos, Alinhamentos.Centro);
        AdicionaConteudo(Conteudo, "www.sophos-erp.com.br", FonteCPF, Alinhamentos.Centro);
        return Conteudo;
    }
    #endregion


    #endregion

    #region Funções de impressão

    public static async Task ImprimirPagina(List<ClsImpressaoDefinicoes> conteudo, string impressora1, int espacamento)
    {


        string printerName = impressora1;

        PrintDocument printDocument = new PrintDocument();
        printDocument.PrinterSettings.PrinterName = printerName;

        printDocument.DefaultPageSettings.PaperSize = new PaperSize("Custom", 280, 500000);
        printDocument.DefaultPageSettings.Margins = new Margins(10, 10, 10, 10);

        printDocument.PrintPage += (sender, e) => PrintPageHandler(sender, e, conteudo, espacamento);

        printDocument.Print();

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
    private bool VerificaSeEstaSemImpressora(string impCadastrada)
    {
        return impCadastrada != "Sem Impressora" || !string.IsNullOrEmpty(impCadastrada);
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

            if (!string.IsNullOrEmpty(Imps.ImpressoraCaixa) && VerificaSeEstaSemImpressora(Imps.ImpressoraCaixa))
            {
                ImpressoraCaixa = Imps.ImpressoraCaixa;
            }
            if (!string.IsNullOrEmpty(Imps.ImpressoraDanfe) && VerificaSeEstaSemImpressora(Imps.ImpressoraDanfe))
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
                    Logotipo = "C:\\SophosCompany\\Sem título.png",
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
                    LarguraBobina = 79,
                    Impressora = ImpressoraCaixa,
                    Logotipo = "C:\\SophosCompany\\Sem título.png"
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
            FonteTotaisNovo = new Font("DejaVu sans mono", AppState.MerchantLogado.TamFonteTotais, FontStyle.Regular);
            FonteDetalhesDoPedido = new Font("DejaVu sans mono", AppState.MerchantLogado.TamFonteDetalhesPedido, FontStyle.Regular);
            FonteComplemento = new Font("DejaVu sans mono", AppState.MerchantLogado.TamFonteDescricaoComplemento, FontStyle.Regular);
            //FontQtdDescVunitVTotal = new Font("DejaVu sans mono", AppState.MerchantLogado.TamFonteLegendaDosItens, FontStyle.Regular);
            FonteContaEntregaEConta = new Font("DejaVu sans mono", AppState.MerchantLogado.TamFonteTempoEntregaEConta, FontStyle.Regular);
            FonteItens2 = new Font("DejaVu sans mono", AppState.MerchantLogado.TamFonteDescricaoItem, FontStyle.Regular);
            FonteCPF = new Font("DejaVu sans mono", AppState.MerchantLogado.TamFonteValorItem, FontStyle.Regular); //ta sendo o valor dos itens 
            FonteInfosPagamento = new Font("DejaVu sans mono", AppState.MerchantLogado.TamFonteInfosPag, FontStyle.Regular);
            FonteNomeDoCliente = new Font("DejaVu sans mono", AppState.MerchantLogado.TamFonteNomeClienteComanda, FontStyle.Regular);

            FonteItensComanda = new Font("DejaVu sans mono", AppState.MerchantLogado.TamFonteDescricaoItemNaComanda, FontStyle.Regular);
            FonteComplementoNaComanda = new Font("DejaVu sans mono", AppState.MerchantLogado.TamFonteDescricaoComplementoNaComanda, FontStyle.Regular);

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