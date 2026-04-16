namespace SophosSyncDesktop.Services;

public static class LogLocalService
{
    private static readonly string _pasta = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "SophosSync", "Logs");

    private static readonly object _lock = new();

    public static void LogImpressao(string displayId, string origem)
    {
        Gravar($"Pedido {displayId} | IMPRIMIU | {origem} | {DateTime.Now:dd/MM/yyyy HH:mm:ss}");
    }

    public static void LogErro(string displayId, string origem, string mensagem)
    {
        Gravar($"Pedido {displayId} | ERRO | {origem} | {DateTime.Now:dd/MM/yyyy HH:mm:ss} | {mensagem}");
    }

    private static void Gravar(string linha)
    {
        try
        {
            lock (_lock)
            {
                Directory.CreateDirectory(_pasta);
                string arquivo = Path.Combine(_pasta, $"log-{DateTime.Now:yyyy-MM-dd}.txt");
                File.AppendAllText(arquivo, linha + Environment.NewLine);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Log] Falha ao gravar log: {ex.Message}");
        }
    }
}
