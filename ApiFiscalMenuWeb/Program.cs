using ApiFiscalMenuWeb.Controllers;
using ApiFiscalMenuWeb.Filters;
using ApiFiscalMenuWeb.Models.Dtos;
using ApiFiscalMenuWeb.Models.HandlersHttp;
using ApiFiscalMenuWeb.Services;
using FrontMenuWeb.Models;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Text.Json.Serialization;
using Unimake.Business.DFe.Servicos;
using Unimake.Business.Security;

var builder = WebApplication.CreateBuilder(args);

#region Injeções de dependências
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddScoped<CustomAuthorizationMessageUnimakeHandler>();
builder.Services.AddScoped<IBPTServices>();
builder.Services.AddScoped<NfService>();
builder.Services.AddScoped<MessageService>();

string UrlCors = builder.Configuration.GetValue<string>("UrlCors") ?? "";
string UrlSophos = builder.Configuration.GetValue<string>("UrlApiSophos") ?? "";
string UrlIBPT = builder.Configuration.GetValue<string>("UrlApiIbpt") ?? "";
string UrlMessageBrokerWhatsAppUnimake = builder.Configuration.GetValue<string>("UrlApiMessageBroker") ?? "";
string UrlMessageBrokerWhatsAppUnimakeAuth = builder.Configuration.GetValue<string>("UrlApiMessageBrokerAuth") ?? "";


builder.Services.AddHttpClient("ApiAutorizada", client =>
{
    client.BaseAddress = new Uri(UrlSophos);
    client.Timeout = TimeSpan.FromSeconds(5);
});

builder.Services.AddHttpClient("ApiIBPT", client =>
{
    client.BaseAddress = new Uri(UrlIBPT); 
    client.Timeout = TimeSpan.FromSeconds(5);
});

builder.Services.AddHttpClient("ApiMessageBrokerUnimake", client =>
{
    client.BaseAddress = new Uri(UrlMessageBrokerWhatsAppUnimake); 
    client.Timeout = TimeSpan.FromSeconds(30);
}).AddHttpMessageHandler<CustomAuthorizationMessageUnimakeHandler>();

builder.Services.AddHttpClient("ApiMessageBrokerUnimakeAuth", client =>
{
    client.BaseAddress = new Uri(UrlMessageBrokerWhatsAppUnimakeAuth); 
    client.Timeout = TimeSpan.FromSeconds(30);
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
            .WithOrigins(UrlCors)
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials(); 
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
