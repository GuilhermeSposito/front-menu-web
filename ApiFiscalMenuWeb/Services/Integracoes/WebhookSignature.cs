using System.Security.Cryptography;
using System.Text;

namespace ApiFiscalMenuWeb.Services.Integracoes;

public class WebhookSignature
{
    private string BytesToHex(byte[] bytes)
    {
        StringBuilder sb = new StringBuilder(bytes.Length * 2);
        foreach (byte b in bytes)
        {
            sb.AppendFormat("{0:x2}", b);
        }
        return sb.ToString();
    }

    public bool ValidateSignature(string secret, byte[] bodyBytes, string signature)
    {
        try
        {
            byte[] keyBytes = Encoding.UTF8.GetBytes(secret);

            using var hmac = new HMACSHA256(keyBytes);

            byte[] hash = hmac.ComputeHash(bodyBytes);

            string expected = BytesToHex(hash);

            return expected.Equals(signature, StringComparison.OrdinalIgnoreCase);
        }
        catch (Exception ex)
        {
            return false;
        }
       
    }
}
