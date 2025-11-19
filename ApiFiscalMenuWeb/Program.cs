using ApiFiscalMenuWeb.Models.Dtos;
using ApiFiscalMenuWeb.Services;
using Unimake.Business.DFe.Servicos;
using Unimake.Business.Security;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

app.MapGet("/", () =>
{
    return "Chegou na api";
});

app.MapPost("/verificar-status-do-certificado", (InfosDoCertificadoDto infosDto) => NfService.VerificaStatusDoCertificadoDigital(infosDto));

app.Run();

//https://syslogicadev.com/apifiscal