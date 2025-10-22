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
    public static Font FonteGeral = new Font("DejaVu sans mono mono", 11, FontStyle.Bold);
    public static Font FonteSeparadoresSimples = new Font("DejaVu sans mono", 8, FontStyle.Bold);
    public static Font FonteSeparadores = new Font("DejaVu sans mono", 11, FontStyle.Bold);
    public static Font FonteCódigoDeBarras = new Font("3 of 9 Barcode", 35, FontStyle.Regular);
    public static Font FonteNomeRestaurante = new Font("DejaVu sans mono", 15, FontStyle.Bold);
    public static Font FonteEndereçoDoRestaurante = new Font("DejaVu sans mono", 9, FontStyle.Bold);
    public static Font FonteNúmeroDoPedido = new Font("DejaVu sans mono", 17, FontStyle.Bold);
    public static Font FonteDetalhesDoPedido = new Font("DejaVu sans mono", 9, FontStyle.Bold);
    public static Font FonteDetalhesDoPedidoRegular = new Font("DejaVu sans mono", 9, FontStyle.Regular);
    public static Font FonteNúmeroDoTelefone = new Font("DejaVu sans mono", 11, FontStyle.Bold);
    public static Font FonteNomeDoCliente = new Font("DejaVu sans mono", 15, FontStyle.Bold);
    public static Font FonteEndereçoDoCliente = new Font("DejaVu sans mono", 10, FontStyle.Bold);
    public static Font FonteItens = new Font("DejaVu sans mono", 12, FontStyle.Bold);
    public static Font FonteItens2 = new Font("DejaVu sans mono", 11, FontStyle.Bold);
    public static Font FonteOpcionais = new Font("DejaVu sans mono", 11, FontStyle.Regular);
    public static Font FonteObservaçõesItem = new Font("DejaVu sans mono", 11, FontStyle.Bold);
    public static Font FonteTotaisDoPedido = new Font("DejaVu sans mono", 10, FontStyle.Bold);
    public static Font FonteCPF = new Font("DejaVu sans mono", 8, FontStyle.Bold);
    public static Font FontQtdDescVunitVTotal = new Font("DejaVu sans mono", 8, FontStyle.Bold);
    public static Font FonteTotaisNovo = new Font("DejaVu sans mono", 12, FontStyle.Regular);
    public static Font FonteInfosPagamento = new Font("DejaVu sans mono", 10, FontStyle.Bold);

    public async Task Imprimir(string jsonDoPedido, string AppQueEnviou)
    {
        try
        {
            List<ClsImpressaoDefinicoes> ConteudoParaImpressao = new List<ClsImpressaoDefinicoes>();
            ClsPedido Pedido = JsonSerializer.Deserialize<ClsPedido>(jsonDoPedido) ?? throw new Exception("Erro ao desserializr pedido");
            await DefineCaracteristicasDePedido(ConteudoParaImpressao, Pedido, AppQueEnviou);

            await ImprimirPagina(ConteudoParaImpressao, "MP-4200 TH", 19);
        }
        catch (Exception ex)
        {
            Console.Write(ex.ToString()); //depois criar os logs
        }
    }

    private async Task DefineCaracteristicasDePedido(List<ClsImpressaoDefinicoes> Conteudo, ClsPedido pedido, string AppQueEnviou)
    {

        AdicionaConteudo(Conteudo, "Sophos Menu Testes", FonteDetalhesDoPedido, Alinhamentos.Centro);
        AdicionaConteudo(Conteudo, AdicionarSeparadorDuplo(), FonteSeparadoresSimples);
        //------------------------------------------------------------------------------------------
        //------------------------------------------------------------------------------------------

        AdicionaConteudo(Conteudo, $"Controle Interno \t Sem valor fiscal", FonteCPF);
        AdicionaConteudo(Conteudo, $"Criado às: {pedido.CriadoEm:t}", FonteDetalhesDoPedido);
        AdicionaConteudo(Conteudo, $"Pedido {pedido.CriadoPor} N°:  #{pedido.Id}", FonteDetalhesDoPedido);

        AdicionaConteudo(Conteudo, $"Controle: {pedido.TipoDePedido}", FonteDetalhesDoPedido);
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
                    AdicionaConteudo(Conteudo, $"{complemento.Quantidade}- {complemento.Descricao} - {complemento.PrecoTotal.ToString("C")}", FonteEndereçoDoRestaurante);
                }
            }


            if (!String.IsNullOrEmpty(item.Observacoes))
            {
                AdicionaConteudo(Conteudo, $"Obs: {item.Observacoes}", FonteEndereçoDoRestaurante);
            }

            AdicionaConteudo(Conteudo, AdicionarSeparadorSimples(), FonteSeparadoresSimples);
        }

        AdicionaConteudo(Conteudo, $"SUB TOTAL. . . .  : {0:F2}", FonteTotaisNovo);
        AdicionaConteudo(Conteudo, $"TAXA DE ENTREGA . : {0:F2}", FonteTotaisNovo);
        AdicionaConteudo(Conteudo, $"TAXA ADICIONAL .  : {0:F2} ", FonteTotaisNovo);
        AdicionaConteudo(Conteudo, $"CORTESIA . . . .  : {0:F2}", FonteTotaisNovo);
        AdicionaConteudo(Conteudo, $"INCENTIVOS . . .  : {0:F2}", FonteTotaisNovo);
        AdicionaConteudo(Conteudo, AdicionarSeparadorSimples(), FonteSeparadoresSimples);
        //------------------------------------------------------------------------------------------
        AdicionaConteudo(Conteudo, $"TOTAL DA CONTA .  : {0:F2}", FonteTotaisNovo);


        AdicionaConteudo(Conteudo, AppQueEnviou, FonteNomeDoCliente, Alinhamentos.Centro);
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
}


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