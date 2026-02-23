using System.Text.Json.Serialization;

namespace FrontMenuWeb.DTOS;

public class UserCodeReturnFromAPIIfoodDto
{
        [JsonPropertyName("userCode")] public string? UserCode { get; set; }
        [JsonPropertyName("authorizationCodeVerifier")] public string? AuthorizationCodeVerifier { get; set; }
        [JsonPropertyName("verificationUrl")] public string? VerificationUrl { get; set; }
        [JsonPropertyName("verificationUrlComplete")] public string? VerificationUrlComplete { get; set; }
        [JsonPropertyName("expiresIn")] public int ExpiresIn { get; set; }
    
}
