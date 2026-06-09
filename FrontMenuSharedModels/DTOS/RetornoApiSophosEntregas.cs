using System.Text.Json.Serialization;

namespace FrontMenuWeb.DTOS;

public class RetornoApiSophosEntregas
{
    [JsonPropertyName("status")] public string? Status { get; set; } = string.Empty;
    [JsonPropertyName("message")] public string? Messages { get; set; } = string.Empty;
    [JsonPropertyName("data")] public DataRetunedSophosEntrega? Data { get; set; } = new DataRetunedSophosEntrega();

}

public class DataRetunedSophosEntrega
{
    [JsonPropertyName("message")] public string? message { get; set; } = string.Empty;
    [JsonPropertyName("MachineId")] public string? MachineId { get; set; } = string.Empty;
    [JsonPropertyName("LinkDeRastreio")] public string? LinkDeRastreio { get; set; } = string.Empty;
}

public class SolicitacaoParaSerEnviadaDto
{
    [JsonPropertyName("EnderecoDeOrigem")] public EnderecoDeOrigemDto? EnderecoDeOrigem { get; set; } = new EnderecoDeOrigemDto();
    [JsonPropertyName("EnderecoDeDestino")] public EnderecoDeDestinoDto? EnderecoDeDestino { get; set; } = new EnderecoDeDestinoDto();

    [JsonIgnore] public DateTime? DataField { get; set; } = null; //Função para converter a data para o formato que a API da sophos espera, que é dd/MM/yyyy, e para converter de volta para DateTime quando receber a resposta da API

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] [JsonPropertyName("data")]
    public string DataDeEntrega
    {
        get
        {
            if (DataField is null)
                return string.Empty;
            return DataField.Value.ToString("dd/MM/yyyy"); ;
        }
        set
        {
            if (DateTime.TryParse(value, out var parsedDate))
                DataField = parsedDate;
            else
                DataField = null;
        }
    }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("hora")]
    public string HoraDeEntrega
    {
        get
        {
            if (DataField is null)
                return string.Empty;
            return DataField.Value.ToString("HH:mm"); ;
        }
        set
        {
            if (DateTime.TryParse(value, out var parsedDate))
                DataField = parsedDate;
            else
                DataField = null;
        }
    }

    [JsonPropertyName("retorno")] public bool retorno { get; set; } = false;
    [JsonPropertyName("FormaDePagamento")] public string FormaDePagamento { get; set; } = "R";

}

public class EnderecoDeOrigemDto
{
    [JsonPropertyName("enderecoFormatadoOrigem")] public string? EnderecoFormatadoOrigem { get; set; } = string.Empty;
    [JsonPropertyName("bairroDeOrigem")] public string? BairroDeOrigem { get; set; } = string.Empty;
    [JsonPropertyName("ComplementoDeOrigem")] public string? ComplementoDeOrigem { get; set; } = string.Empty;
    [JsonPropertyName("ReferenciaDeOrigem")] public string? ReferenciaDeOrigem { get; set; } = string.Empty;
}

public class EnderecoDeDestinoDto
{
    [JsonPropertyName("idDeReferenciaExterna")] public string? IdDeReferenciaExterna { get; set; } = "Sophos";
    [JsonPropertyName("enderecoFormatadoDestino")] public string? EnderecoFormatadoDestino { get; set; } = string.Empty;
    [JsonPropertyName("BairroDestino")] public string? BairroDestino { get; set; } = string.Empty;
    [JsonPropertyName("ComplementoDestino")] public string? ComplementoDestino { get; set; } = string.Empty;
    [JsonPropertyName("referenciaDestino")] public string? ReferenciaDestino { get; set; } = string.Empty;
    [JsonPropertyName("cidadeDestino")] public string? CidadeDestino { get; set; } = string.Empty;
    [JsonPropertyName("estadoDestino")] public string? EstadoDestino { get; set; } = string.Empty;
    [JsonPropertyName("nomeClienteDestino")] public string? NomeClienteDestino { get; set; } = string.Empty;
    [JsonPropertyName("telefoneClienteDestino")] public string? TelefoneClienteDestino { get; set; } = string.Empty;

}

public class RetornoApiSophosEntregaResponse
{
    [JsonPropertyName("status")] public string? Status { get; set; } = string.Empty;
    [JsonPropertyName("message")] public string? Messages { get; set; } = string.Empty;
        [JsonPropertyName("data")] public DataRetornoApiSophosEntrega? Data { get; set; } = new DataRetornoApiSophosEntrega();

}

public class DataRetornoApiSophosEntrega
{
    [JsonPropertyName("message")] public string? message { get; set; } = string.Empty;
    [JsonPropertyName("MachineId")] public string? MachineId { get; set; } = string.Empty;
    [JsonPropertyName("LinkDeRastreio")] public string? LinkDeRastreio { get; set; } = string.Empty;
}

public class PedidoParaEstimativaRequestDto
{
    [JsonPropertyName("idDaEstimativa")] public string IdDaEstimativa { get; set; } = string.Empty;
    [JsonPropertyName("EnderecoDePartida")] public string EnderecoDePartida { get; set; } = string.Empty;
    [JsonPropertyName("BairroDePartida")] public string BairroDePartida { get; set; } = string.Empty;
    [JsonPropertyName("CidadeDePartida")] public string CidadeDePartida { get; set; } = string.Empty;
    [JsonPropertyName("EstadoDePartida")] public string EstadoDePartida { get; set; } = string.Empty;
    [JsonPropertyName("EnderecoDeDestino")] public string EnderecoDeDestino { get; set; } = string.Empty;
    [JsonPropertyName("BairroDeDestino")] public string BairroDeDestino { get; set; } = string.Empty;
    [JsonPropertyName("CidadeDeDestino")] public string CidadeDeDestino { get; set; } = string.Empty;
    [JsonPropertyName("EstadoDeDestino")] public string EstadoDeDestino { get; set; } = string.Empty;
    [JsonPropertyName("Data")] public string Data { get; set; } = string.Empty;
    [JsonPropertyName("Hora")] public string Hora { get; set; } = string.Empty;
    [JsonPropertyName("Retorno")] public bool Retorno { get; set; } = false;
}

public class CriaEstimativaRequestDto
{
    [JsonPropertyName("PedidosQueVaoSerEstimados")] public List<PedidoParaEstimativaRequestDto> PedidosQueVaoSerEstimados { get; set; } = new();
}

public class EstimativaDeEntregaDto
{
    [JsonPropertyName("IdDaEstimativa")] public string IdDaEstimativa { get; set; } = string.Empty;
    [JsonPropertyName("sucesso")] public bool Sucesso { get; set; }
    [JsonPropertyName("EstimativaDoValor")] public decimal? EstimativaDoValor { get; set; }
    [JsonPropertyName("EstimativaMinutos")] public int? EstimativaMinutos { get; set; }
    [JsonPropertyName("EstimativaPorKm")] public decimal? EstimativaPorKm { get; set; }
    [JsonPropertyName("MensagemDeErro")] public string? MensagemDeErro { get; set; }
}

public class CriaEstimativaDataDto
{
    [JsonPropertyName("message")] public string Message { get; set; } = string.Empty;
    [JsonPropertyName("Estimativas")] public List<EstimativaDeEntregaDto> Estimativas { get; set; } = new();
}

public class CriaEstimativaResponseDto
{
    [JsonPropertyName("status")] public string Status { get; set; } = string.Empty;
    [JsonPropertyName("data")] public CriaEstimativaDataDto? Data { get; set; }
}
