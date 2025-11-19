using ApiFiscalMenuWeb.Controllers;
using ApiFiscalMenuWeb.Filters;
using ApiFiscalMenuWeb.Models.Dtos;
using ApiFiscalMenuWeb.Services;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Text.Json.Serialization;
using Unimake.Business.DFe.Servicos;
using Unimake.Business.Security;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddScoped<NfService>();

builder.Services.AddHttpClient("ApiAutorizada", client =>
{
    client.BaseAddress = new Uri("https://syslogicadev.com/api/v1/"); //new Uri("https://localhost:3030");//
});//teste

builder.Services.AddControllers(option =>
{
    option.Filters.Add(typeof(ApiExceptionFilter));
}).AddJsonOptions(option =>
{
    option.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
});

var app = builder.Build();
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapControllers();

app.Run();

//https://syslogicadev.com/apifiscal