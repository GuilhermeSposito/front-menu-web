﻿using FrontMenuWeb.Services;
using System.Text.Json.Serialization;

namespace FrontMenuWeb.Models.Produtos;

public class ClsGrupo
{

    [JsonPropertyName("id")] public int Id { get; set; }
    [JsonPropertyName("CodigoInterno")] public string? CodigoInterno { get; set; } = "0000";
    [JsonPropertyName("Descricao")] public string? Descricao { get; set; }
    [JsonPropertyName("UltilizarCarroChefe")] public bool UltilizarCarroChefe { get; set; } = false;

}
