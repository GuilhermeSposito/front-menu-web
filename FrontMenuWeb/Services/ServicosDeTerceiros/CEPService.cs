using Newtonsoft.Json;
using System.Net.Http.Json;
using System.Text.Json.Serialization;

namespace FrontMenuWeb.Services.ServicosDeTerceiros
{
    public class CEPService
    {
        private readonly HttpClient _http;

        public CEPService(HttpClient http)
        {
            _http = http;
        }

        public async Task<EnderecoViaCep?> ConsultarCepAsync(string cep)
        {
            try
            {
                var response = await _http.GetFromJsonAsync<EnderecoViaCep>($"{cep}/json/");
                return response;
            }
            catch
            {
                return null;
            }
        }
        public async Task<List<EnderecoViaCep>> ConsultarCepPorRuaAsync(string rua)
        {
            try
            {
                string RuaSemEspacos = rua.Replace(" ", "%20");
                Console.WriteLine($"Rua sem espaços: {RuaSemEspacos}");

                List<EnderecoViaCep> response = await _http.GetFromJsonAsync<List<EnderecoViaCep>>($"SP/São%20Carlos/{RuaSemEspacos}/json/");
                return response;
            }
            catch
            {
                return new List<EnderecoViaCep>();
            }
        }
    }

    public class EnderecoViaCep
    {
        [JsonPropertyName("cep")] public string? Cep { get; set; }
        [JsonPropertyName("logradouro")] public string? Logradouro { get; set; }
        [JsonPropertyName("complemento")] public string? Complemento { get; set; }
        [JsonPropertyName("bairro")] public string? Bairro { get; set; }
        [JsonPropertyName("localidade")] public string? Cidade { get; set; } 
        [JsonPropertyName("uf")] public string? Uf { get; set; }
        [JsonPropertyName("ibge")] public string? Ibge { get; set; }
        [JsonPropertyName("gia")] public string? Gia { get; set; }
        [JsonPropertyName("ddd")] public string? Ddd { get; set; }
        [JsonPropertyName("siafi")] public string? Siafi { get; set; }
    }
}
