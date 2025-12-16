namespace Plugin.Maui.WearableMessaging;

/// <summary>
///     Main interface for wearable device communication.
///     Provides methods for messaging, data synchronization, and connectivity management.
/// </summary>
public interface IWearableMessaging
{
    /// <summary>
    ///     Checks if a wearable device is currently reachable and available for immediate communication.
    /// </summary>
    /// <returns>True if the wearable is reachable, false otherwise.</returns>
    Task<bool> IsWearableReachable();

    /// <summary>
    ///     Checks if the companion wearable app is installed on the paired device.
    /// </summary>
    /// <returns>True if the wearable app is installed, false otherwise.</returns>
    Task<bool> IsWearableAppInstalled();

    /// <summary>
    ///     Checks if wearable communication is supported on the current device/platform.
    /// </summary>
    /// <returns>True if wearable communication is supported, false otherwise.</returns>
    Task<bool> IsSupported();

    /// <summary>
    ///     Sends a simple key-value message to the wearable device.
    /// </summary>
    /// <param name="key">The message key/identifier.</param>
    /// <param name="value">The message value.</param>
    /// <exception cref="WearableMessagingException">Thrown when the message fails to send.</exception>
    Task SendMessageAsync(string key, string value);

    /// <summary>
    ///     Sends a dictionary of key-value pairs to the wearable device.
    /// </summary>
    /// <param name="message">Dictionary containing the message data.</param>
    /// <exception cref="WearableMessagingException">Thrown when the message fails to send.</exception>
    Task SendMessageAsync(Dictionary<string, string> message);

    /// <summary>
    ///     Sends a message to the wearable device and waits for a reply.
    /// </summary>
    /// <param name="message">Dictionary containing the message data.</param>
    /// <param name="timeout">Maximum time to wait for a reply. Default is 5 seconds.</param>
    /// <returns>The reply message from the wearable device.</returns>
    /// <exception cref="TimeoutException">Thrown when no reply is received within the timeout period.</exception>
    /// <exception cref="WearableMessagingException">Thrown when the message fails to send.</exception>
    Task<Dictionary<string, string>> SendMessageWithReplyAsync(
        Dictionary<string, string> message,
        TimeSpan? timeout = null);

    /// <summary>
    ///     Updates the application context that is automatically synchronized with the wearable device.
    ///     The latest context replaces any previous context.
    /// </summary>
    /// <param name="context">Dictionary containing the application context data.</param>
    /// <exception cref="WearableMessagingException">Thrown when the context fails to update.</exception>
    Task UpdateApplicationContextAsync(Dictionary<string, object> context);

    /// <summary>
    ///     Gets the current application context received from the wearable device.
    /// </summary>
    /// <returns>Dictionary containing the current application context.</returns>
    Task<Dictionary<string, object>> GetApplicationContextAsync();

    /// <summary>
    ///     Transfers a file to the wearable device in the background.
    /// </summary>
    /// <param name="filePath">Path to the file to transfer.</param>
    /// <param name="metadata">Optional metadata associated with the file.</param>
    /// <exception cref="FileNotFoundException">Thrown when the file doesn't exist.</exception>
    /// <exception cref="WearableMessagingException">Thrown when the transfer fails to start.</exception>
    Task TransferFileAsync(string filePath, Dictionary<string, object>? metadata = null);

    /// <summary>
    ///     Transfers user info data to the wearable device in the background. Unlike messages,
    ///     user info transfers are queued and delivered even when the wearable is not reachable.
    /// </summary>
    /// <param name="userInfo">Dictionary containing the user info data to transfer.</param>
    /// <exception cref="WearableMessagingException">Thrown when the transfer fails to start.</exception>
    Task TransferUserInfoAsync(Dictionary<string, object> userInfo);


    /// <summary>
    ///     Raised when a message is received from the wearable device.
    /// </summary>
    event EventHandler<MessageReceivedEventArgs>? MessageReceived;

    /// <summary>
    ///     Raised when the application context from the wearable device changes.
    /// </summary>
    event EventHandler<ApplicationContextChangedEventArgs>? ApplicationContextChanged;

    /// <summary>
    ///     Raised when the wearable device connection state changes.
    /// </summary>
    event EventHandler<WearableStateChangedEventArgs>? WearableStateChanged;

    /// <summary>
    ///     Raised when a file transfer from the wearable device completes.
    /// </summary>
    event EventHandler<FileTransferCompletedEventArgs>? FileTransferCompleted;

    /// <summary>
    ///     Raised when user info is received from the wearable device.
    /// </summary>
    event EventHandler<UserInfoReceivedEventArgs>? UserInfoReceived;
}
