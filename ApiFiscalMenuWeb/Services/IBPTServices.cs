using System.Text.Json.Serialization;

namespace ApiFiscalMenuWeb.Services;

public class IBPTServices
{
    #region Props
    private readonly IHttpClientFactory _factory;
    public IBPTServices(IHttpClientFactory factory)
    {
        _factory = factory;
    }
    #endregion

    public async Task<float> GetIBPTValor(string cnpj, string ncm, string uf, string descricao, float valor, string gtin = "SEM%GTIN")
    {
        try
        {
            var client = _factory.CreateClient("ApiIBPT");

            var response = await client.GetAsync($"?token=VROoQxeyOD4wBqTS6neLweOUlkCajSf4j9uInykJRFORUrvYBDBkhpd1lrJ5gKVG&" +
                $"cnpj={cnpj}&" +
                $"codigo={ncm}&" +
                $"uf={uf}&" +
                $"ex=0&" +
                $"descricao={Uri.EscapeDataString(descricao)}&" +
                $"unidadeMedida=UN&" +
                $"valor={valor.ToString(System.Globalization.CultureInfo.InvariantCulture)}&" +
                $"gtin={gtin}");

            if (!response.IsSuccessStatusCode)
                return valor * 0.30f;

            var content = await response.Content.ReadAsStringAsync();
            var ibptResponse = System.Text.Json.JsonSerializer.Deserialize<IBPTResponse>(content);

            float VTotalTrib = ibptResponse!.ValorTributoNacional +
                              ibptResponse.ValorTributoEstadual +
                              ibptResponse.ValorTributoMunicipal;

            return VTotalTrib;
        }
        catch (TaskCanceledException ex)
        {
            // Timeout ou cancelamento
            Console.WriteLine("Timeout/cancelado de IBPT: " + ex.Message);
            return valor * 0.30f;
        }
        catch (Exception ex)
        {
            Console.WriteLine("Erro geral de IBPT: " + ex.Message);
            return valor * 0.30f;
        }

    }
}

class IBPTResponse
{
    [JsonPropertyName("Codigo")] public string Codigo { get; set; } = string.Empty;
    [JsonPropertyName("Descricao")] public string Descricao { get; set; } = string.Empty;
    [JsonPropertyName("Nacional")] public float Nacional { get; set; }
    [JsonPropertyName("Estadual")] public float Estadual { get; set; }
    [JsonPropertyName("Importado")] public float Importado { get; set; }
    [JsonPropertyName("Municipal")] public float Municipal { get; set; }
    [JsonPropertyName("Valor")] public float Valor { get; set; }
    [JsonPropertyName("ValorTributoNacional")] public float ValorTributoNacional { get; set; }
    [JsonPropertyName("ValorTributoEstadual")] public float ValorTributoEstadual { get; set; }
    [JsonPropertyName("ValorTributoImportado")] public float ValorTributoImportado { get; set; }
    [JsonPropertyName("ValorTributoMunicipal")] public float ValorTributoMunicipal { get; set; }
}