using System.Xml.Serialization;

namespace ApiFiscalMenuWeb.Models.Dtos.Ccm;

[XmlRoot("pedidos")]
public class Pedidos
{
    [XmlElement("pedido")]
    public List<Pedido> PedidoList { get; set; } = new List<Pedido>();

    public Pedidos()
    {

    }
}


[XmlRoot("pedido")]
public class Pedido
{
    [XmlElement("nroPedido")] public int NroPedido { get; set; }
    [XmlElement("retira")] public int Retira { get; set; }
    [XmlElement("ValorTotal")] public float ValorTotal { get; set; }
    [XmlElement("ValorTaxa")] public float ValorTaxa { get; set; }
    [XmlElement("TrocoPara")] public string? TrocoPara { get; set; }
    [XmlElement("CodPdvPagamento")] public int CodPdvPagamento { get; set; }
    [XmlElement("DescricaoPagamento")] public string? DescricaoPagamento { get; set; }
    [XmlElement("ObsGeraisPedido")] public string? ObsGeraisPedido { get; set; }
    [XmlElement("CodigoFilial")] public int CodigoFilial { get; set; }
    [XmlElement("StatusPedido")] public string? StatusPedido { get; set; }
    [XmlElement("DataHoraPedido")] public string? DataHoraPedido { get; set; }
    [XmlElement("entregarate")] public string? EntregarAte { get; set; }
    [XmlElement("PedidoCPF")] public string? PedidoCPF { get; set; }
    [XmlElement("OrigemPedido")] public string? OrigemPedido { get; set; }
    [XmlElement("travaPedido")] public int TravaPedido { get; set; }
    [XmlElement("travaMotivo")] public string? TravaMotivo { get; set; }
    [XmlElement("StatusAcompanhamento")] public string? StatusAcompanhamento { get; set; }
    [XmlElement("HorarioRetirada")] public string? HorarioRetirada { get; set; }
    [XmlElement("NumeroMesa")] public int NumeroMesa { get; set; }
    [XmlElement("CreditoUtilizado")] public float CreditoUtilizado { get; set; }
    [XmlElement("ValorBruto")] public float ValorBruto { get; set; }
    [XmlElement("CupomDesconto")] public string? CupomDesconto { get; set; }
    [XmlElement("ValorCupom")] public float ValorCupom { get; set; }
    [XmlElement("Debug")] public string? Debug { get; set; }
    [XmlElement("Agendamento")] public int Agendamento { get; set; }
    [XmlElement("DataAgendamento")] public string? DataAgendamento { get; set; }
    [XmlElement("HoraAgendamento")] public string? HoraAgendamento { get; set; }
    [XmlElement("DataHoraAgendamento")] public string? DataHoraAgendamento { get; set; }
    [XmlElement("PagamentoOnline")] public int PagamentoOnline { get; set; }
    [XmlElement("PedidoIntegrado")] public int PedidoIntegrado { get; set; }
    [XmlElement("cliente")] public Cliente Cliente { get; set; } = new Cliente();
    [XmlElement("endereco")] public Endereco Endereco { get; set; } = new Endereco();
    [XmlArray("itens"), XmlArrayItem("item")] public List<Item> Itens { get; set; } = new List<Item>();
    [XmlArray("brindes"), XmlArrayItem("brinde")] public List<Brinde> Brindes { get; set; } = new List<Brinde>();

    public Pedido()
    {

    }
}

public class Brinde
{
    [XmlElement("codPdvBrinde")] public string? CodPdvBrinde { get; set; }
    [XmlElement("descricao")] public string? Descricao { get; set; }
}

public class Cliente
{
    [XmlElement("codigo")] public int Codigo { get; set; }
    [XmlElement("nome")] public string? Nome { get; set; }
    [XmlElement("telefone")] public string? Telefone { get; set; }
    [XmlElement("email")] public string? Email { get; set; }
    [XmlElement("FaceCliente")] public string? FaceCliente { get; set; }

    public Cliente()
    {

    }
}

public class Endereco
{
    [XmlElement("rua")] public string? Rua { get; set; }
    [XmlElement("numero")] public string? Numero { get; set; }
    [XmlElement("complemento")] public string? Complemento { get; set; }
    [XmlElement("referencia")] public string? Referencia { get; set; }
    [XmlElement("bairro")] public string? Bairro { get; set; }
    [XmlElement("cidade")] public string? Cidade { get; set; }
    [XmlElement("estado")] public string? Estado { get; set; }
    [XmlElement("cep")] public string? Cep { get; set; }

    public Endereco()
    {

    }
}

public class Item
{
    [XmlElement("parte")] public List<Parte> Parte { get; set; } = new List<Parte>();
    [XmlElement("Codigo")] public int Codigo { get; set; }
    [XmlElement("CodPdv")] public string? CodPdv { get; set; }
    [XmlElement("CodPdvGrupo")] public string? CodPdvGrupo { get; set; }
    [XmlElement("Descricao")] public string? Descricao { get; set; }
    [XmlElement("NomeItem")] public string? NomeItem { get; set; }
    [XmlElement("Quantidade")] public int Quantidade { get; set; }
    [XmlElement("ValorUnit")] public float ValorUnit { get; set; }
    [XmlElement("ObsItem")] public string ObsItem { get; set; } = string.Empty;
    [XmlArray("adicionais"), XmlArrayItem("adicional")] public List<Adicional> Adicionais { get; set; } = new List<Adicional>();

    public Item()
    {

    }
}

public class Adicional
{
    [XmlElement("Codigo")] public int Codigo { get; set; }
    [XmlElement("CodPdv")] public string? CodPdv { get; set; }
    [XmlElement("Descricao")] public string? Descricao { get; set; }
    [XmlElement("Quantidade")] public int Quantidade { get; set; }
    [XmlElement("ValorUnit")] public float ValorUnit { get; set; }
    [XmlElement("CodPdvTipo")] public float CodPdvTipo { get; set; }
    [XmlElement("InfoTipo")] public string InfoTipo { get; set; } = string.Empty;

    public Adicional()
    {

    }
}

public class Parte
{
    [XmlElement("CodPdvItem")] public string CodPdvItem { get; set; } = string.Empty;
    [XmlElement("ObsParte")] public string? ObsParte { get; set; }
    [XmlElement("CodPdvGrupo")] public string CodPdvGrupo { get; set; } = string.Empty;

    public Parte()
    {

    }
}
