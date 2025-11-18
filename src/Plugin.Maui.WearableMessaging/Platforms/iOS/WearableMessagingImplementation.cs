#if IOS
using Foundation;
using WatchConnectivity;

namespace Plugin.Maui.WearableMessaging.Platforms.iOS;

/// <summary>
///     iOS implementation of IWearableMessaging using the WatchConnectivity framework.
/// </summary>
public class WearableMessagingImplementation : IWearableMessaging
{
    private readonly WearableMessagingOptions _options;
    private WCSession? _session;
    private WcSessionDelegateImpl? _sessionDelegate;

    /// <summary>
    ///     Initializes a new instance of the WearableMessagingImplementation class using default options.
    /// </summary>
    /// <remarks>
    ///     This constructor sets up the messaging implementation with default configuration
    ///     values. For custom behavior, use the constructor that accepts a WearableMessagingOptions
    ///     parameter.
    /// </remarks>
    public WearableMessagingImplementation() : this(new WearableMessagingOptions())
    {
    }

    /// <summary>
    ///     Initializes a new instance of the WearableMessagingImplementation class using the specified messaging
    ///     options.
    /// </summary>
    /// <param name="options">The configuration options that define messaging behavior for the wearable device. Cannot be null.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="options" /> is null.</exception>
    public WearableMessagingImplementation(WearableMessagingOptions options)
    {
        _options = options ?? throw new ArgumentNullException(nameof(options));
        InitializeSession();
    }

    /// <summary>
    ///     Occurs when a new message is received.
    /// </summary>
    /// <remarks>
    ///     Subscribers can handle this event to process incoming messages as they arrive. The
    ///     event is raised on the thread that receives the message; ensure thread safety when accessing shared
    ///     resources in event handlers.
    /// </remarks>
    public event EventHandler<MessageReceivedEventArgs>? MessageReceived;

    /// <summary>
    ///     Occurs when the application context changes, allowing subscribers to respond to updates in application-wide
    ///     settings or state.
    /// </summary>
    /// <remarks>
    ///     Subscribe to this event to be notified when the application context is modified. The
    ///     event provides an <see cref="ApplicationContextChangedEventArgs" /> instance containing details about the
    ///     change. This event is typically raised after significant changes to global application data, such as user
    ///     preferences or environment settings.
    /// </remarks>
    public event EventHandler<ApplicationContextChangedEventArgs>? ApplicationContextChanged;

    /// <summary>
    ///     Occurs when the state of the wearable device changes.
    /// </summary>
    /// <remarks>
    ///     Subscribe to this event to receive notifications when the wearable device transitions
    ///     between states, such as connecting, disconnecting, or entering a low-power mode. The event provides a
    ///     <see
    ///         cref="WearableStateChangedEventArgs" />
    ///     instance containing details about the new state.
    /// </remarks>
    public event EventHandler<WearableStateChangedEventArgs>? WearableStateChanged;

    /// <summary>
    ///     Occurs when a file transfer operation has completed, providing details about the transfer result.
    /// </summary>
    /// <remarks>
    ///     Subscribers can use this event to perform post-processing or notify users when a file
    ///     transfer finishes. The event is raised regardless of whether the transfer succeeded or failed; check the
    ///     properties of <see cref="FileTransferCompletedEventArgs" /> for outcome details.
    /// </remarks>
    public event EventHandler<FileTransferCompletedEventArgs>? FileTransferCompleted;

    /// <summary>
    ///     Determines asynchronously whether the connected wearable device is currently reachable.
    /// </summary>
    /// <returns>
    ///     A task that represents the asynchronous operation. The task result is <see langword="true" /> if the wearable
    ///     device is reachable; otherwise, <see langword="false" />.
    /// </returns>
    public Task<bool> IsWearableReachable()
    {
        return Task.FromResult(_session?.Reachable ?? false);
    }

    /// <summary>
    ///     Determines asynchronously whether the wearable companion app is currently installed on the connected device.
    /// </summary>
    /// <returns>
    ///     A task that represents the asynchronous operation. The task result is <see langword="true" /> if the wearable
    ///     app is installed; otherwise, <see langword="false" />.
    /// </returns>
    public Task<bool> IsWearableAppInstalled()
    {
        return Task.FromResult(_session?.WatchAppInstalled ?? false);
    }

    /// <summary>
    ///     Determines asynchronously whether the current platform supports Watch Connectivity features.
    /// </summary>
    /// <returns>
    ///     A task that represents the asynchronous operation. The task result is <see langword="true" /> if Watch
    ///     Connectivity is supported on the current platform; otherwise, <see langword="false" />.
    /// </returns>
    public Task<bool> IsSupported()
    {
        return Task.FromResult(WCSession.IsSupported);
    }

    /// <summary>
    ///     Asynchronously sends a message with the specified key and value.
    /// </summary>
    /// <param name="key">The key that identifies the message. Cannot be null.</param>
    /// <param name="value">The value associated with the message key. Cannot be null.</param>
    /// <returns>A task that represents the asynchronous send operation.</returns>
    public Task SendMessageAsync(string key, string value)
    {
        var message = new Dictionary<string, string>
        {
            { key, value }
        };
        return SendMessageAsync(message);
    }

    /// <summary>
    ///     Asynchronously sends a message to the connected Apple Watch using WatchConnectivity.
    /// </summary>
    /// <remarks>
    ///     The method requires an active and reachable Apple Watch session. If the session is
    ///     not available or the watch is unreachable, the operation will fail immediately. The message is sent using
    ///     Apple's WatchConnectivity framework, and any errors encountered during transmission are reported via the
    ///     returned task.
    /// </remarks>
    /// <param name="message">
    ///     A dictionary containing key-value pairs representing the message to send. Keys and values must be non-null
    ///     strings.
    /// </param>
    /// <returns>
    ///     A task that represents the asynchronous send operation. The task completes when the message has been sent or
    ///     an error occurs.
    /// </returns>
    /// <exception cref="WearableMessagingException">
    ///     Thrown if WatchConnectivity is not supported, the Apple Watch is not reachable, or if an error occurs while
    ///     sending the message.
    /// </exception>
    public Task SendMessageAsync(Dictionary<string, string> message)
    {
        if (_session == null || !WCSession.IsSupported)
        {
            throw new WearableMessagingException("WatchConnectivity is not supported");
        }

        if (!_session.Reachable)
        {
            throw new WearableMessagingException("Apple Watch is not reachable");
        }

        var tcs = new TaskCompletionSource<bool>();
        var nsDict = ConvertToNativeDictionary(message);

        _session.SendMessage(nsDict, null, error =>
        {
            // ReSharper disable once ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
            if (error != null)
            {
                tcs.SetException(new WearableMessagingException(
                    $"Failed to send message: {error.LocalizedDescription}",
                    error.Code.ToString()));
            }
            else
            {
                tcs.SetResult(true);
            }
        });

        return tcs.Task;
    }

    /// <summary>
    ///     Sends a message to the connected Apple Watch and asynchronously waits for a reply.
    /// </summary>
    /// <remarks>
    ///     If no reply is received within the specified timeout, the returned task will fault
    ///     with a TimeoutException. The method requires a reachable Apple Watch and a supported WatchConnectivity
    ///     session.
    /// </remarks>
    /// <param name="message">
    ///     A dictionary containing the key-value pairs to send to the Apple Watch. Keys and values must be non-null
    ///     strings.
    /// </param>
    /// <param name="timeout">
    ///     An optional timeout specifying the maximum duration to wait for a reply. If not provided, the default reply
    ///     timeout is used.
    /// </param>
    /// <returns>
    ///     A task that represents the asynchronous operation. The task result contains a dictionary of key-value pairs
    ///     received in the reply from the Apple Watch.
    /// </returns>
    /// <exception cref="WearableMessagingException">
    ///     Thrown if WatchConnectivity is not supported or if the Apple Watch is not
    ///     reachable.
    /// </exception>
    public Task<Dictionary<string, string>> SendMessageWithReplyAsync(
        Dictionary<string, string> message,
        TimeSpan? timeout = null)
    {
        if (_session == null || !WCSession.IsSupported)
        {
            throw new WearableMessagingException("WatchConnectivity is not supported");
        }

        if (!_session.Reachable)
        {
            throw new WearableMessagingException("Apple Watch is not reachable");
        }

        var tcs = new TaskCompletionSource<Dictionary<string, string>>();
        var nsDict = ConvertToNativeDictionary(message);
        var timeoutValue = timeout ?? _options.DefaultReplyTimeout;

        _session.SendMessage(nsDict, reply =>
        {
            var replyDict = ConvertFromNativeDictionary(reply);
            tcs.SetResult(replyDict);
        }, error =>
        {
            tcs.SetException(new WearableMessagingException(
                $"Failed to send message: {error.LocalizedDescription}",
                error.Code.ToString()));
        });

        // Handle timeout
        Task.Delay(timeoutValue).ContinueWith(_ =>
        {
            if (!tcs.Task.IsCompleted)
            {
                tcs.TrySetException(new TimeoutException(
                    $"No reply received within {timeoutValue.TotalSeconds} seconds"));
            }
        });

        return tcs.Task;
    }

    /// <summary>
    ///     Updates the application context on the connected wearable device asynchronously.
    /// </summary>
    /// <remarks>
    ///     The application context is used to share the latest state information between the
    ///     host and the wearable device. This method overwrites any previously set context on the device. Ensure that
    ///     the provided context contains only data types supported by the underlying platform.
    /// </remarks>
    /// <param name="context">
    ///     A dictionary containing key-value pairs to set as the new application context. Keys must be non-null
    ///     strings; values must be serializable to platform-supported types.
    /// </param>
    /// <returns>A task that represents the asynchronous update operation.</returns>
    /// <exception cref="WearableMessagingException">
    ///     Thrown if WatchConnectivity is not supported, or if the update operation
    ///     fails.
    /// </exception>
    public Task UpdateApplicationContextAsync(Dictionary<string, object> context)
    {
        if (_session == null || !WCSession.IsSupported)
            throw new WearableMessagingException("WatchConnectivity is not supported");
        try
        {
            var nsDict = ConvertToNativeDictionary(context);
            _session.UpdateApplicationContext(nsDict, out var error);
            if (error != null)
            {
                throw new WearableMessagingException(
                    $"Failed to update application context: {error.LocalizedDescription}",
                    error.Code.ToString());
            }

            return Task.CompletedTask;
        }
        catch (Exception ex) when (ex is not WearableMessagingException)
        {
            throw new WearableMessagingException("Failed to update application context", ex);
        }
    }

    /// <summary>
    ///     Asynchronously retrieves the latest application context received from the paired device.
    /// </summary>
    /// <remarks>
    ///     The application context provides state information shared between the local and
    ///     paired device. Keys are converted to strings; only non-null keys are included in the result.
    /// </remarks>
    /// <returns>
    ///     A task that represents the asynchronous operation. The task result contains a dictionary of key-value pairs
    ///     representing the received application context. If no session is available or supported, the dictionary will
    ///     be empty.
    /// </returns>
    public Task<Dictionary<string, object>> GetApplicationContextAsync()
    {
        if (_session == null || !WCSession.IsSupported)
            return Task.FromResult(new Dictionary<string, object>());
        var context = _session.ReceivedApplicationContext;
        var dict = new Dictionary<string, object>();
        foreach (var keyObj in context.Keys)
        {
            var key = keyObj.ToString();
            if (key == null) continue;
            dict[key] = context[keyObj];
        }

        return Task.FromResult(dict);
    }

    /// <summary>
    ///     Initiates an asynchronous transfer of a file to the connected wearable device using WatchConnectivity.
    /// </summary>
    /// <remarks>
    ///     This method only initiates the file transfer; it does not wait for the transfer to
    ///     complete. Ensure that the wearable device is connected and WatchConnectivity is supported before calling
    ///     this method.
    /// </remarks>
    /// <param name="filePath">
    ///     The full path to the file to be transferred. The file must exist and its size must not exceed the maximum
    ///     allowed transfer size.
    /// </param>
    /// <param name="metadata">
    ///     An optional dictionary containing metadata to associate with the file transfer. May be null if no metadata
    ///     is required.
    /// </param>
    /// <returns>
    ///     A task that represents the asynchronous file transfer operation. The task completes when the transfer is
    ///     initiated.
    /// </returns>
    /// <exception cref="WearableMessagingException">
    ///     Thrown if WatchConnectivity is not supported, if the file size exceeds the maximum allowed, or if the
    ///     transfer cannot be initiated.
    /// </exception>
    /// <exception cref="FileNotFoundException">Thrown if the file specified by <paramref name="filePath" /> does not exist.</exception>
    public Task TransferFileAsync(string filePath, Dictionary<string, object>? metadata = null)
    {
        if (_session == null || !WCSession.IsSupported)
            throw new WearableMessagingException("WatchConnectivity is not supported");
        if (!File.Exists(filePath))
            throw new FileNotFoundException($"File not found: {filePath}");
        var fileInfo = new FileInfo(filePath);
        if (fileInfo.Length > _options.MaxFileTransferSize)
            throw new WearableMessagingException(
                $"File size ({fileInfo.Length} bytes) exceeds maximum allowed size ({_options.MaxFileTransferSize} bytes)");
        try
        {
            var fileUrl = NSUrl.FromFilename(filePath);
            var metaNs = metadata != null ? ConvertToNativeDictionary(metadata) : null;
            _session.TransferFile(fileUrl, metaNs);
            LogDebug($"File transfer initiated: {filePath}");
            return Task.CompletedTask;
        }
        catch (Exception ex)
        {
            throw new WearableMessagingException("Failed to initiate file transfer", ex);
        }
    }

    private void InitializeSession()
    {
        if (!WCSession.IsSupported)
        {
            LogDebug("WatchConnectivity is not supported on this device");
            return;
        }

        _session = WCSession.DefaultSession;
        _sessionDelegate = new WcSessionDelegateImpl(this);
        _session.Delegate = _sessionDelegate;

        if (!_options.AutoActivateSession)
            return;

        _session.ActivateSession();
        LogDebug("WatchConnectivity session activated");
    }

    private static Dictionary<string, string> ConvertFromNativeDictionary(NSDictionary<NSString, NSObject> replyMessage)
    {
        var dict = new Dictionary<string, string>();
        foreach (var keyObj in replyMessage.Keys)
        {
            var key = keyObj.ToString();
            if (key == null) continue;
            dict[key] = replyMessage.ObjectForKey(keyObj)?.ToString() ?? string.Empty;
        }

        return dict;
    }


    private static NSDictionary<NSString, NSObject> ConvertToNativeDictionary<T>(Dictionary<string, T> dict)
    {
        ArgumentNullException.ThrowIfNull(dict);

        var keys = new List<NSString>(dict.Count);
        var values = new List<NSObject>(dict.Count);

        foreach (var kvp in dict)
        {
            if (string.IsNullOrEmpty(kvp.Key))
                continue;

            keys.Add(new NSString(kvp.Key));
            values.Add(kvp.Value is null ? NSNull.Null : NSObject.FromObject(kvp.Value));
        }

        return NSDictionary<NSString, NSObject>.FromObjectsAndKeys(values.ToArray(), keys.ToArray());
    }

    // Backward compatible existing overload
    private static NSDictionary<NSString, NSObject> ConvertToNativeDictionary(Dictionary<string, string> dict)
    {
        return ConvertToNativeDictionary<string>(dict);
    }

    internal void OnMessageReceived(NSDictionary message, WCSessionReplyHandler? replyHandler)
    {
        var dict = new Dictionary<string, string>();
        foreach (var keyObj in message.Keys)
        {
            var key = keyObj.ToString();
            if (key == null) continue;
            dict[key] = message.ObjectForKey(keyObj)?.ToString() ?? string.Empty;
        }

        Action<Dictionary<string, string>>? reply = null;
        if (replyHandler != null)
        {
            reply = replyDict =>
            {
                var nsReply = ConvertToNativeDictionary(replyDict);
                replyHandler(nsReply);
            };
        }

        MessageReceived?.Invoke(this, new MessageReceivedEventArgs(dict, reply));
    }

    internal void OnApplicationContextChanged(NSDictionary context)
    {
        var ctx = new Dictionary<string, object>();
        foreach (var keyObj in context.Keys)
        {
            var key = keyObj.ToString();
            if (key == null) continue;
            ctx[key] = context[keyObj];
        }

        ApplicationContextChanged?.Invoke(this, new ApplicationContextChangedEventArgs(ctx));
    }

    internal void OnStateChanged()
    {
        var isReachable = _session?.Reachable ?? false;
        var isAppInstalled = _session?.WatchAppInstalled ?? false;
        var isPaired = _session?.Paired ?? false;

        WearableStateChanged?.Invoke(this, new WearableStateChangedEventArgs(
            isReachable, isAppInstalled, isPaired));
    }

    internal void OnFileTransferCompleted(NSUrl? fileUrl, NSDictionary? metadata, NSError? error)
    {
        var filePath = fileUrl?.Path ?? string.Empty;
        var meta = new Dictionary<string, object>();
        if (metadata != null)
        {
            foreach (var keyObj in metadata.Keys)
            {
                var key = keyObj.ToString();
                if (key == null) continue;
                meta[key] = metadata[keyObj];
            }
        }

        FileTransferCompleted?.Invoke(this, new FileTransferCompletedEventArgs(
            filePath, meta, error == null, error?.LocalizedDescription));
    }

    private void LogDebug(string message)
    {
        if (_options.EnableDebugLogging)
        {
            Console.WriteLine($"[WearableMessaging-iOS] {message}");
        }
    }
}

/// <summary>
///     Delegate implementation for handling WatchConnectivity session events.
/// </summary>
internal class WcSessionDelegateImpl : WCSessionDelegate
{
    private readonly WearableMessagingImplementation _implementation;

    public WcSessionDelegateImpl(WearableMessagingImplementation implementation)
    {
        _implementation = implementation;
    }

    public override void SessionReachabilityDidChange(WCSession session)
    {
        _implementation.OnStateChanged();
    }

    public override void SessionWatchStateDidChange(WCSession session)
    {
        _implementation.OnStateChanged();
    }

    public override void DidReceiveMessage(WCSession session, NSDictionary<NSString, NSObject> message)
    {
        _implementation.OnMessageReceived(message, null);
    }

    public override void DidReceiveMessage(WCSession session, NSDictionary<NSString, NSObject> message,
        WCSessionReplyHandler replyHandler)
    {
        _implementation.OnMessageReceived(message, replyHandler);
    }

    public override void DidReceiveApplicationContext(WCSession session,
        NSDictionary<NSString, NSObject> applicationContext)
    {
        _implementation.OnApplicationContextChanged(applicationContext);
    }

    public override void DidFinishFileTransfer(WCSession session, WCSessionFileTransfer fileTransfer, NSError? error)
    {
        _implementation.OnFileTransferCompleted(fileTransfer.File.FileUrl, fileTransfer.File.Metadata, error);
    }

    /// <summary>
    ///     Called when a file is received from the watch. Saves the file to the cache directory
    ///     and notifies the implementation of the completed transfer.
    /// </summary>
    /// <param name="session">The WatchConnectivity session.</param>
    /// <param name="file">The received file information.</param>
    public override void DidReceiveFile(WCSession session, WCSessionFile file)
    {
        try
        {
            var destUrl = SaveInbound(file);
            _implementation.OnFileTransferCompleted(destUrl, file.Metadata, error: null);
        }
        catch (Foundation.NSErrorException nse)
        {
            _implementation.OnFileTransferCompleted(file.FileUrl, file.Metadata, nse.Error);
        }
        catch (Exception ex)
        {
            var userInfo = Foundation.NSDictionary.FromObjectAndKey(
                new Foundation.NSString(ex.Message ?? "error"),
                Foundation.NSError.LocalizedDescriptionKey);
            var nsErr = new Foundation.NSError(
                new Foundation.NSString("Plugin.Maui.WearableMessaging"),
                -1,
                userInfo);
            _implementation.OnFileTransferCompleted(file.FileUrl, file.Metadata, nsErr);
        }
    }
    /// <summary>
    ///     Saves an inbound file from the watch to the local cache directory.
    /// </summary>
    /// <param name="file">The <see cref="WatchConnectivity.WCSessionFile"/> containing the file to save.</param>
    /// <returns>The <see cref="Foundation.NSUrl"/> of the saved file in the cache directory.</returns>
    /// <exception cref="Foundation.NSErrorException">Thrown when the file copy operation fails.</exception>
    private static Foundation.NSUrl SaveInbound(WatchConnectivity.WCSessionFile file)
    {
        var inbox = System.IO.Path.Combine(Microsoft.Maui.Storage.FileSystem.CacheDirectory, "WearInbox");
        System.IO.Directory.CreateDirectory(inbox);

        // Generate a unique filename using the ID from metadata or a new GUID.
        var id = file.Metadata?["id"]?.ToString() ?? System.Guid.NewGuid().ToString("N");
        var baseName = file.FileUrl?.LastPathComponent ?? "watch-file";
        var destPath = System.IO.Path.Combine(inbox, $"{id}-{baseName}");
        var destUrl = Foundation.NSUrl.FromFilename(destPath);

        // Remove existing file if present to avoid copy failure
        if (Foundation.NSFileManager.DefaultManager.FileExists(destPath))
        {
            Foundation.NSFileManager.DefaultManager.Remove(destUrl, out _);
        }
        Foundation.NSError? copyErr = null;
        Foundation.NSFileManager.DefaultManager.Copy(file.FileUrl, destUrl, out copyErr);
        if (copyErr is not null) throw new Foundation.NSErrorException(copyErr);

        return destUrl;
    }
}
#endif
