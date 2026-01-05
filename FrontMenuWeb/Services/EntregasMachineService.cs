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

    public async Task<ReturnApiRefatored<EmpresaMachine>> CreateEmpresa(EmpresaMachine empresa)
    {
        var response = await _http.PostAsJsonAsync($"empresas-machine-integradas", empresa);
        var responseContent = await response.Content.ReadFromJsonAsync<ReturnApiRefatored<EmpresaMachine>>();

        return responseContent ?? new ReturnApiRefatored<EmpresaMachine>();
    }

    public async Task<ReturnApiRefatored<EmpresaMachine>> EditEmpresa(EmpresaMachine empresa)
    {
        var response = await _http.PatchAsJsonAsync($"empresas-machine-integradas/update/{empresa.Id}", empresa);
        var responseContent = await response.Content.ReadFromJsonAsync<ReturnApiRefatored<EmpresaMachine>>();

        return responseContent ?? new ReturnApiRefatored<EmpresaMachine>();
    }

    public async Task<ReturnApiRefatored<EmpresaMachine>> DeleteEmpresa(int id)
    {
        var response = await _http.DeleteAsync($"empresas-machine-integradas/delete/{id}");
        var responseContent = await response.Content.ReadFromJsonAsync<ReturnApiRefatored<EmpresaMachine>>();
        return responseContent ?? new ReturnApiRefatored<EmpresaMachine>();
    }
}

