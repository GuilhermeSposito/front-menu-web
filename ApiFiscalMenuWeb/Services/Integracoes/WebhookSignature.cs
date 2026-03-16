using System.Security.Cryptography;
using System.Text;

namespace ApiFiscalMenuWeb.Services.Integracoes;

public class WebhookSignature
{
    private string BytesToHex(byte[] bytes)
    {
        return Convert.ToHexString(bytes).ToLower();
    }

    public bool ValidateSignature(string secret, string body, string signature)
    {
        try
        {
            var keyBytes = Encoding.UTF8.GetBytes(secret);
            var bodyBytes = Encoding.UTF8.GetBytes(body);

            using var hmac = new HMACSHA256(keyBytes);
            var hashBytes = hmac.ComputeHash(bodyBytes);

            var expected = BytesToHex(hashBytes);

            return CryptographicOperations.FixedTimeEquals(
                Encoding.UTF8.GetBytes(expected),
                Encoding.UTF8.GetBytes(signature)
            );
        }
        catch
        {
            return false;
        }
    }
}
