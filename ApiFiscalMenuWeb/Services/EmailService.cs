using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
//using System.Net;
//using System.Net.Mail;

namespace ApiFiscalMenuWeb.Services;

public class EmailService
{
    private readonly IConfiguration _configuration;

    public EmailService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

 
    public async Task EnviarAsync(string para, string assunto, string corpo)
    {
        var host = _configuration["Email:Host"];
        var port = int.Parse(_configuration["Email:Port"] ?? "587");
        var user = _configuration["Email:User"];
        var pass = _configuration["Email:Pass"];

        var message = new MimeMessage();
        message.From.Add(new MailboxAddress(
            "SOPHOS APLICATIVOS E TECNOLOGIA",
            user));

        message.To.Add(MailboxAddress.Parse(para));
        message.Subject = assunto;

        message.Body = new TextPart("html")
        {
            Text = corpo
        };

        using var client = new SmtpClient();

        // 🔥 equivalente ao rejectUnauthorized: false do Nest
        client.ServerCertificateValidationCallback = (s, c, h, e) => true;

        await client.ConnectAsync(host, port, SecureSocketOptions.StartTls);

        await client.AuthenticateAsync(user, pass);

        await client.SendAsync(message);

        await client.DisconnectAsync(true);
    }
}