using Unimake.MessageBroker.Primitives.Model.Messages;
using Unimake.MessageBroker.Primitives.Model.Notifications;
using Unimake.MessageBroker.Services;
using Unimake.Primitives.UDebug;
namespace ApiFiscalMenuWeb.Services;



public class MessageService
{
    private readonly IHttpClientFactory _factory;
    private readonly Unimake.MessageBroker.Services.MessageService _messageService; 
    public MessageService(IHttpClientFactory factory)
    {
        _factory = factory;
    }

    public async Task SendMessageAsync(string WhatsAppNumber, string message, string InstanceName = "01KHRKZZ31AK3CBYG8J4Y7B9R2")
    {
        TextMessage Message = new TextMessage
        {
            InstanceName = "InstanceName",
            Text = $"Olá! Eu sou uma mensagem de teste 🌜☠️. \n Aqui, eu estou em uma nova linha.",
            To = "5516992366175",
        };

        

        //_messageService.SendTextMessageAsync(Message);

        HttpClient client = _factory.CreateClient("ApiMessageBrokerUnimake");
        client.DefaultRequestHeaders.Add("Authorization", "Bearer eyJhbGciOiJSUzI1NiIsImtpZCI6InVuaW1ha2UtcnNhLWtleS1TYW5kYm94IiwidHlwIjoiSldUIn0.eyJzdWIiOiI1MjAzMTY5NDI1MDEyNyIsInVuaXF1ZV9uYW1lIjoiNTIwMzE2OTQyNTAxMjciLCJpYXQiOjE3NzE0NDM2NDYsInNjb3BlIjoiVU1lc3Nlbmdlci5hbGwiLCJwZXJtcyI6IlJlYWQgV3JpdGUgQ3JlYXRlIERlbGV0ZSIsInJvbGUiOiJVc2VyIEFkbWluIEN1c3RvbWVyIiwianRpIjoiMjYyODkxZGY0YzRkNGFjNjk0OThkZjc3ODkzZTlkODIiLCJ0ZW5hbnQiOiI3MTU4MTE4NzY2NTUwMSIsIkNQRkNOUEoiOiIiLCJuYW1lIjoiVW5pZmFrZSBTb2Z0d2FyZSIsImdyYW50X3R5cGUiOiJDbGllbnRDcmVkZW50aWFscyIsImVudiI6IlNhbmRib3giLCJuYmYiOjE3NzE0NDM1ODUsImV4cCI6MTc3MTQ0NDU0NSwiaXNzIjoiaHR0cHM6Ly9hdXRoLnNhbmRib3gudW5pbWFrZS5zb2Z0d2FyZS8iLCJhdWQiOiJVTWVzc2VuZ2VyIn0.enbYdXCKrjnM1ctZOjNk7OY0N29r6vw5k8TV32PO6VWyThe4MFwyP1I_MD1RlbSuwL5gj4WlfQ71R1e3X-a_QvpBh57n8tC1u8GtQMfNfi_gYVNmNdvVXmbLdKosO4OX4STUXQSY_I8_9x7bk5S3Lr9m-CJzWQ5IzPi6hd3TniO2jlw_xAF22ke6fZbNEJb-jeZtlWa5GuVvv4dH4qb1UPMcDNbmiJrvPUNHEO4qB8opJBl1mvn-KZPT1jLDtF0B42PDRrIPoe-qbjsgTtScuimFFhrwWgT8uUgcArb4pIA1vDl4Etj4OElwWlMEGviiz5f_yVV9Sb8J4gv10dGtSQ"); 
        
        try
        {
            var response = await client.PostAsJsonAsync($"/api/v1/Messages/Publish/{InstanceName}", Message);
            if (response.IsSuccessStatusCode)
            {
                Console.WriteLine("Mensagem enviada com sucesso!");
            }
            else
            {
                Console.WriteLine($"Falha ao enviar mensagem. Status Code: {response.StatusCode}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Erro ao enviar mensagem: {ex.Message}");
        }


    }
}
