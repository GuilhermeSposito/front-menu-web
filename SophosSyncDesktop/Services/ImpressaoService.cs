using FrontMenuWeb.Models.Pedidos;
using FrontMenuWeb.Models.Vendas;
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

namespace SophosSyncDesktop.Services;

public class ImpressaoService
{

    #region Fontes utilizadas na impressão
    public Font FonteGeral { get; set; } = new Font("DejaVu sans mono mono", 11, FontStyle.Bold);
    public Font FonteSeparadoresSimples { get; set; } = new Font("DejaVu sans mono", 8, FontStyle.Bold);
    public Font FonteSeparadores { get; set; } = new Font("DejaVu sans mono", 11, FontStyle.Bold);
    public Font FonteCódigoDeBarras { get; set; } = new Font("3 of 9 Barcode", 35, FontStyle.Regular);
    public Font FonteNomeRestaurante { get; set; } = new Font("DejaVu sans mono", 15, FontStyle.Bold);
    public Font FonteEndereçoDoRestaurante { get; set; } = new Font("DejaVu sans mono", 9, FontStyle.Bold);
    public Font FonteNúmeroDoPedido { get; set; } = new Font("DejaVu sans mono", 17, FontStyle.Bold);
    public Font FonteDetalhesDoPedido { get; set; } = new Font("DejaVu sans mono", 9, FontStyle.Bold);
    public Font FonteFechamentoDeCaixa { get; set; } = new Font("DejaVu sans mono", 8, FontStyle.Bold);
    public Font FonteDetalhesDoPedidoRegular { get; set; } = new Font("DejaVu sans mono", 9, FontStyle.Regular);
    public Font FonteNúmeroDoTelefone { get; set; } = new Font("DejaVu sans mono", 11, FontStyle.Bold);
    public Font FonteNomeDoCliente { get; set; } = new Font("DejaVu sans mono", 15, FontStyle.Bold);
    public Font FonteEndereçoDoCliente { get; set; } = new Font("DejaVu sans mono", 10, FontStyle.Bold);
    public Font FonteItens { get; set; } = new Font("DejaVu sans mono", 12, FontStyle.Bold);
    public Font FonteItens2 { get; set; } = new Font("DejaVu sans mono", 11, FontStyle.Bold);
    public Font FonteOpcionais { get; set; } = new Font("DejaVu sans mono", 11, FontStyle.Regular);
    public Font FonteObservaçõesItem { get; set; } = new Font("DejaVu sans mono", 11, FontStyle.Bold);
    public Font FonteTotaisDoPedido { get; set; } = new Font("DejaVu sans mono", 10, FontStyle.Bold);
    public Font FonteCPF { get; set; } = new Font("DejaVu sans mono", 8, FontStyle.Bold);
    public Font FontQtdDescVunitVTotal { get; set; } = new Font("DejaVu sans mono", 8, FontStyle.Bold);
    public Font FonteTotaisNovo { get; set; } = new Font("DejaVu sans mono", 12, FontStyle.Regular);
    public Font FonteInfosPagamento { get; set; } = new Font("DejaVu sans mono", 10, FontStyle.Bold);
    public Font FonteSophos { get; set; } = new Font("Montserrat", 15, FontStyle.Bold);
    #endregion

    #region Funções que chamam a impressão
    public async Task Imprimir(string jsonDoPedido, string AppQueEnviou)
    {
        try
        {
            using (AppDbContext db = new AppDbContext())
            {
                ImpressorasConfigs Imps = db.Impressoras.FirstOrDefault() ?? new ImpressorasConfigs();
                ClsPedido Pedido = JsonSerializer.Deserialize<ClsPedido>(jsonDoPedido) ?? throw new Exception("Erro ao desserializr pedido");

                //primeiro imprime pedido
                if (!string.IsNullOrEmpty(Imps.ImpressoraCaixa) && VerificaSeEstaSemImpressora(Imps.ImpressoraCaixa))
                {
                    List<ClsImpressaoDefinicoes> ConteudoParaImpressaoDoPedido = DefineCaracteristicasDePedidoParaImpressao(Pedido, AppQueEnviou);
                    await ImprimirPagina(ConteudoParaImpressaoDoPedido, Imps.ImpressoraCaixa, 19);
                }

                //imprime na impressora auxiliar se tiver
                if (!string.IsNullOrEmpty(Imps.ImpressoraAux) && VerificaSeEstaSemImpressora(Imps.ImpressoraAux))
                {
                    List<ClsImpressaoDefinicoes> ConteudoParaImpressaoDoPedido = DefineCaracteristicasDePedidoParaImpressao(Pedido, AppQueEnviou);
                    await ImprimirPagina(ConteudoParaImpressaoDoPedido, Imps.ImpressoraAux, 19);
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

        AdicionaConteudo(Conteudo, $"\n                     {pedido.DisplayId}", FonteItens);
        AdicionaConteudo(Conteudo, $"Controle: {pedido.TipoDePedido}   Conta Nº:", FonteDetalhesDoPedido);
        AdicionaConteudo(Conteudo, AdicionarSeparadorSimples(), FonteSeparadoresSimples);
        //------------------------------------------------------------------------------------------
        AdicionaConteudo(Conteudo, $"Entregar Até: {pedido.CriadoEm:t}", FonteItens);
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
        AdicionaConteudo(Conteudo, $"              Tam.  V.Unit.   Total.", FontQtdDescVunitVTotal);
        AdicionaConteudo(Conteudo, AdicionarSeparadorSimples(), FonteSeparadoresSimples);
        foreach (var item in pedido.Itens)
        {
            AdicionaConteudo(Conteudo, $"{item.Quantidade}X  {item.Descricao}", FonteItens2);
            AdicionaConteudo(Conteudo, $"                      {item.PrecoUnitario:F2}     {item.PrecoTotal:F2}", FonteCPF);

            if (item.Complementos.Count > 0)
            {
                AdicionaConteudo(Conteudo, $"\n", FonteCPF);
                foreach (var complemento in item.Complementos)
                {
                    AdicionaConteudo(Conteudo, $"{complemento.Quantidade}- {complemento.Descricao} - {complemento.PrecoTotal.ToString("C")}", FonteEndereçoDoRestaurante, eObs: true);
                }
            }

            if (!String.IsNullOrEmpty(item.Observacoes))
            {
                AdicionaConteudo(Conteudo, $"Obs: {item.Observacoes}", FonteEndereçoDoRestaurante, eObs: true);
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
        AdicionaConteudo(Conteudo, "syslogicadev.com", FonteCPF, Alinhamentos.Centro);

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
        AdicionaConteudo(Conteudo, "syslogicadev.com", FonteCPF, Alinhamentos.Centro);
        return Conteudo;
    }
    #endregion

    #region Definição das comandas para impressão
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

    #endregion
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