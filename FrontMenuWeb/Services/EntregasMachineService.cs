using FrontMenuWeb.Models;
using FrontMenuWeb.Models.EntregaMachine;
using System.Net.Http.Json;
using System.Text.Json.Serialization;

namespace FrontMenuWeb.Services;

public class EntregasMachineService
{
    private HttpClient _http;
    public EntregasMachineService(HttpClient http)
    {
        _http = http;
    }

    public async Task<List<EmpresaMachine>> GetEmpresasIntegradas()
    {
        var response = await _http.GetFromJsonAsync<ReturnApiRefatored<EmpresaMachine>>("empresas-machine-integradas");  
        
        return response?.Data.Lista ??  new List<EmpresaMachine>();
    }
}

