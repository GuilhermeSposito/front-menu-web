using FrontDesktopBlzHybrid.DesktopServices;
using Microsoft.Extensions.Logging;
using Microsoft.Maui.LifecycleEvents;
using MudBlazor.Extensions;
using MudBlazor.Services;

namespace FrontDesktopBlzHybrid
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                });

           // #if WINDOWS
            builder.Services.AddSingleton<AppNativeFunctions>();
         //   #endif

            builder.Services.AddMudServices();
            builder.Services.AddMudExtensions();
            builder.Services.AddMauiBlazorWebView();

            #if DEBUG
    		builder.Services.AddBlazorWebViewDeveloperTools();
    		builder.Logging.AddDebug();
            #endif

            return builder.Build();
        }
    }
}
