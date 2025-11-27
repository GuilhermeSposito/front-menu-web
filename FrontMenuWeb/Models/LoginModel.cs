using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace FrontMenuWeb.Models;

public class LoginModel
{
    [Required(ErrorMessage = "O email é obrigatório")]
    [EmailAddress(ErrorMessage = "Email inválido")]
    public string Email { get; set; }

    [Required(ErrorMessage = "A senha é obrigatória")]
    public string Senha { get; set; }

    public string IsAdmin { get; set; } = "false"; // Default
}

public class LoginResult
{
    [JsonPropertyName("sucess")]
    public bool Success { get; set; }

    [JsonPropertyName("token")]
    public string Token { get; set; }

    [JsonPropertyName("expiresIn")]
    public int ExpiresIn { get; set; }
}
