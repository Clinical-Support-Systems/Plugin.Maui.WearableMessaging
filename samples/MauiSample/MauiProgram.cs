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
#if DEBUG && IOS && TARGET_SIMULATOR
                // Use in-process loopback during iOS simulator debugging to avoid needing a watch app.
                .ConfigureServices(services =>
                {
                    services.AddSingleton<IWearableMessaging, LoopbackWearableMessaging>();
                })
#else
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
#endif

            return builder.Build();
        }
    }
}
