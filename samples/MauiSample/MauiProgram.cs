using Microsoft.Extensions.Logging;
using Plugin.Maui.WearableMessaging;

namespace MauiSample
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
#if RELEASE
                .UseWearableMessaging(options =>
                {
                    options.DefaultReplyTimeout = TimeSpan.FromSeconds(30);
                    options.EnableDebugLogging = true;
                })
#endif
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });

#if DEBUG
    		builder.Logging.AddDebug();

            // Use the loopback implementation for local development and testing
            builder.Services.AddSingleton<IWearableMessaging, LoopbackWearableMessaging>();
#endif

            return builder.Build();
        }
    }
}
