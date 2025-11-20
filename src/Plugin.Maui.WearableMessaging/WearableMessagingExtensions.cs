namespace Plugin.Maui.WearableMessaging;

/// <summary>
///     Extension methods for registering the WearableMessaging plugin.
/// </summary>
public static class WearableMessagingExtensions
{
    /// <summary>
    ///     Configures the application to use the WearableMessaging plugin.
    ///     Registers the IWearableMessaging service as a singleton.
    /// </summary>
    /// <param name="builder">The MauiAppBuilder instance.</param>
    /// <returns>The MauiAppBuilder for method chaining.</returns>
    public static MauiAppBuilder UseWearableMessaging(this MauiAppBuilder builder)
    {
        builder.Services.AddSingleton<IWearableMessaging>(_ =>
        {
#if ANDROID
            return new Platforms.Android.WearableMessagingImplementation();
#elif IOS
            return new Platforms.iOS.WearableMessagingImplementation();
#else
            return new WearableMessagingNotSupportedImplementation();
#endif
        });

        return builder;
    }

    /// <summary>
    ///     Configures the application to use the WearableMessaging plugin with custom options.
    /// </summary>
    /// <param name="builder">The MauiAppBuilder instance.</param>
    /// <param name="configure">Action to configure WearableMessaging options.</param>
    /// <returns>The MauiAppBuilder for method chaining.</returns>
    public static MauiAppBuilder UseWearableMessaging(
        this MauiAppBuilder builder,
        Action<WearableMessagingOptions> configure)
    {
        var options = new WearableMessagingOptions();
        configure(options);

        builder.Services.AddSingleton(options);
        builder.Services.AddSingleton<IWearableMessaging>(_ =>
        {
#if ANDROID
            return new Platforms.Android.WearableMessagingImplementation(options);
#elif IOS
            return new Platforms.iOS.WearableMessagingImplementation(options);
#else
            return new WearableMessagingNotSupportedImplementation();
#endif
        });

        return builder;
    }
}
