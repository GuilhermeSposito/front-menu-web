using ApiFiscalMenuWeb.Services;
using Unimake.Business.DFe.Servicos;
using Unimake.Business.Security;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

app.MapPost("/", () =>
{
    return NfService.VerificaStatusDoCertificadoDigital();
});

app.Run();
