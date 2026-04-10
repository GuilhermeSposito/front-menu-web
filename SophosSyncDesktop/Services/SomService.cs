using NAudio.Wave;

namespace SophosSyncDesktop.Services;

public static class SomService
{
    private static readonly string PastaSons =
        Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Sounds");

    public static void TocarPedidoDelivery() => Tocar("pedido-delivery.mp3");
    public static void TocarPedidoMesa()     => Tocar("pedido-mesa.mp3");

    // Fire-and-forget: não bloqueia a thread do SocketIO
    private static void Tocar(string nomeArquivo)
    {
        _ = Task.Run(() =>
        {
            try
            {
                var caminho = Path.Combine(PastaSons, nomeArquivo);
                if (!File.Exists(caminho)) return;

                using var mp3Reader = new Mp3FileReader(caminho);
                using var output    = new WaveOutEvent();
                output.Init(mp3Reader);
                output.Play();

                while (output.PlaybackState == PlaybackState.Playing)
                    Thread.Sleep(50);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[SOM] Erro ao tocar '{nomeArquivo}': {ex.Message}");
            }
        });
    }
}
