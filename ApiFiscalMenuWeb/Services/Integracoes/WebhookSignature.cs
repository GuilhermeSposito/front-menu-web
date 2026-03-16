using System.Security.Cryptography;
using System.Text;

namespace ApiFiscalMenuWeb.Services.Integracoes;

public class WebhookSignature
{
    private string BytesToHex(byte[] bytes)
    {
        return Convert.ToHexString(bytes).ToLower();
    }

    public bool ValidateSignature(string secret, byte[] bodyBytes, string signature)
    {
        var keyBytes = Encoding.UTF8.GetBytes(secret);

        using var hmac = new HMACSHA256(keyBytes);
        var hashBytes = hmac.ComputeHash(bodyBytes);

        var expected = Convert.ToHexString(hashBytes).ToLower();

        Console.WriteLine($"Signature: {signature}");
        Console.WriteLine($"Expected: {expected}");

        return CryptographicOperations.FixedTimeEquals(
            Encoding.ASCII.GetBytes(expected),
            Encoding.ASCII.GetBytes(signature)
        );
    }
}
