using Microsoft.Maui.Platform;
using Microsoft.UI.Windowing;
using Windows.Graphics;

namespace FrontDesktopBlzHybrid
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();
        }
        protected override Window CreateWindow(IActivationState? activationState)
        {
            var window = new Window(new MainPage()) { Title = "Sophos Desktop" };
            window.Created += (s, e) =>
            {

                SetFullScreen(window);
            };

            return window;
        }

        public void SetFullScreen(Window mauiWindow)
        {
#if WINDOWS
            try
            {
                // Obter a janela nativa
                var nativeWindow = mauiWindow.Handler.PlatformView as Microsoft.UI.Xaml.Window;
                if (nativeWindow == null)
                    return;

                // Obter AppWindow
                var windowId = Microsoft.UI.Win32Interop.GetWindowIdFromWindow(
                    WinRT.Interop.WindowNative.GetWindowHandle(nativeWindow)
                );

                var appWindow = AppWindow.GetFromWindowId(windowId);
                if (appWindow.Presenter is OverlappedPresenter)
                {
                    appWindow.SetPresenter(AppWindowPresenterKind.FullScreen);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Erro ao entrar em fullscreen: " + ex);
            }
#endif
        }

    }


}
