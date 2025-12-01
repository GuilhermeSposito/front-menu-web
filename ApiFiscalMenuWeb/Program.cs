using ApiFiscalMenuWeb.Controllers;
using ApiFiscalMenuWeb.Filters;
using ApiFiscalMenuWeb.Models.Dtos;
using ApiFiscalMenuWeb.Services;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Text.Json.Serialization;
using Unimake.Business.DFe.Servicos;
using Unimake.Business.Security;

var builder = WebApplication.CreateBuilder(args);

#region Injeções de dependências
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddScoped<IBPTServices>();
builder.Services.AddScoped<NfService>();

builder.Services.AddHttpClient("ApiAutorizada", client =>
{
    client.BaseAddress = new Uri("https://syslogicadev.com/api/v1/"); //new Uri("https://localhost:3030");//
});

builder.Services.AddHttpClient("ApiIBPT", client =>
{
    client.BaseAddress = new Uri("https://apidoni.ibpt.org.br/api/v1/produtos"); //new Uri("https://localhost:3030");//
});

builder.Services.AddControllers(option =>
{
    option.Filters.Add(typeof(ApiExceptionFilter));
}).AddJsonOptions(option =>
{
    option.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
});
#endregion

builder.Services.AddCors(options =>
{
    options.AddPolicy("CorsLiberado", policy =>
    {
        policy
            .AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader();
    });
});

var app = builder.Build();
app.UseCors("CorsLiberado");

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}


app.MapControllers();

app.Run();
