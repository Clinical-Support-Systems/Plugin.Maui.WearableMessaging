namespace Plugin.Maui.WearableMessaging;

/// <summary>
///     Configuration options for the WearableMessaging plugin.
/// </summary>
public class WearableMessagingOptions
{
    /// <summary>
    ///     Default timeout for messages that expect a reply.
    ///     Default is 5 seconds.
    /// </summary>
    public TimeSpan DefaultReplyTimeout { get; set; } = TimeSpan.FromSeconds(5);

    /// <summary>
    ///     Whether to automatically activate the session/connection on initialization.
    ///     Default is true.
    /// </summary>
    public bool AutoActivateSession { get; set; } = true;

    /// <summary>
    ///     Whether to log debug information.
    ///     Default is false.
    /// </summary>
    public bool EnableDebugLogging { get; set; } = false;

    /// <summary>
    ///     Maximum file size for transfers in bytes.
    ///     Default is 10MB.
    /// </summary>
    public long MaxFileTransferSize { get; set; } = 10 * 1024 * 1024;
}
