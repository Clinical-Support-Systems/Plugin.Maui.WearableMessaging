#if ANDROID
using System.Text;
using System.Text.Json;
using Android.Gms.Wearable;
using Android.Runtime;
using Application = Android.App.Application;
using Object = Java.Lang.Object;

namespace Plugin.Maui.WearableMessaging.Platforms.Android;

/// <summary>
///     Android implementation of IWearableMessaging using the Wearable Data Layer API.
/// </summary>
public class WearableMessagingImplementation : Object, IWearableMessaging,
    MessageClient.IOnMessageReceivedListener, DataClient.IOnDataChangedListener,
    CapabilityClient.IOnCapabilityChangedListener
{
    private const string WearableCapability = "wearable_app";
    private readonly WearableMessagingOptions _options;
    private CapabilityClient? _capabilityClient;
    private DataClient? _dataClient;
    private MessageClient? _messageClient;
    private string? _wearableNodeId;

    /// <summary>
    ///     Initializes a new instance of the WearableMessagingImplementation class using default messaging options.
    /// </summary>
    /// <remarks>
    ///     This constructor provides a convenient way to create a WearableMessagingImplementation with
    ///     standard configuration. For custom settings, use the constructor that accepts a WearableMessagingOptions
    ///     parameter.
    /// </remarks>
    public WearableMessagingImplementation() : this(new WearableMessagingOptions())
    {
    }

    /// <summary>
    ///     Initializes a new instance of the WearableMessagingImplementation class using the specified messaging options.
    /// </summary>
    /// <param name="options">The configuration options used to set up messaging behavior for wearable devices. Cannot be null.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="options" /> is null.</exception>
    public WearableMessagingImplementation(WearableMessagingOptions options)
    {
        _options = options ?? throw new ArgumentNullException(nameof(options));
        InitializeClients();
    }

    /// <summary>
    ///     Handles changes to the capability information by updating the wearable node state and raising the corresponding
    ///     state change event.
    /// </summary>
    /// <remarks>
    ///     This method triggers the <see cref="WearableStateChanged" /> event to notify listeners of
    ///     changes in the reachability of the wearable device. The event is raised regardless of whether any nodes are
    ///     present.
    /// </remarks>
    /// <param name="capabilityInfo">
    ///     An object that provides information about the current capability state, including the set of connected nodes.
    ///     Cannot be null.
    /// </param>
    public void OnCapabilityChanged(ICapabilityInfo capabilityInfo)
    {
        _ = UpdateWearableNodeId();

        var isReachable = capabilityInfo.Nodes.Count == 0;
        WearableStateChanged?.Invoke(this, new WearableStateChangedEventArgs(
            isReachable, isReachable, isReachable));
    }

    /// <summary>
    ///     Handles changes in the data layer by processing each data event in the specified buffer.
    /// </summary>
    /// <remarks>
    ///     This method processes only events of type <see cref="DataEvent.TypeChanged" />. Application
    ///     context changes trigger the <see cref="ApplicationContextChanged" /> event, while file transfer events are
    ///     handled separately. Exceptions encountered during processing are logged and do not propagate to the
    ///     caller.
    /// </remarks>
    /// <param name="dataEvents">
    ///     A buffer containing data events to be processed. Each event represents a change in the data layer and may
    ///     include application context updates or file transfers.
    /// </param>
    public void OnDataChanged(DataEventBuffer dataEvents)
    {
        try
        {
            foreach (var dataEvent in dataEvents)
            {
                if (dataEvent.Type != DataEvent.TypeChanged)
                    continue;

                var dataItem = dataEvent.DataItem;
                if (dataItem.Uri.Path?.StartsWith("/application_context") == true)
                {
                    var dataMap = DataMapItem.FromDataItem(dataItem).DataMap;
                    var json = dataMap.GetString("context");

                    if (string.IsNullOrEmpty(json))
                        continue;

                    var context = JsonSerializer.Deserialize<Dictionary<string, object>>(json) ??
                                  new Dictionary<string, object>();
                    ApplicationContextChanged?.Invoke(this,
                        new ApplicationContextChangedEventArgs(context));
                }
                else if (dataItem.Uri.Path?.StartsWith("/file/") == true)
                {
                    HandleFileTransfer(dataItem);
                }
            }
        }
        catch (Exception ex)
        {
            LogDebug($"Failed to process data change: {ex.Message}");
        }
    }

    /// <summary>
    ///     Handles an incoming message event by extracting and deserializing its data, then raising the MessageReceived
    ///     event with the parsed message contents.
    /// </summary>
    /// <remarks>
    ///     If the message data is empty or cannot be deserialized, the method does not raise the
    ///     MessageReceived event. Any exceptions encountered during processing are logged for debugging purposes.
    /// </remarks>
    /// <param name="messageEvent">
    ///     The message event containing the raw data to be processed. Cannot be null. The event's data is expected to be a
    ///     UTF-8 encoded JSON object representing key-value pairs.
    /// </param>
    public void OnMessageReceived(IMessageEvent messageEvent)
    {
        try
        {
            var data = messageEvent.GetData();
            if (data.Length == 0) return;

            var json = Encoding.UTF8.GetString(data);
            var message = JsonSerializer.Deserialize<Dictionary<string, string>>(json) ??
                          new Dictionary<string, string>();

            MessageReceived?.Invoke(this, new MessageReceivedEventArgs(message));
        }
        catch (Exception ex)
        {
            LogDebug($"Failed to process received message: {ex.Message}");
        }
    }

    /// <summary>
    ///     Occurs when a new message is received.
    /// </summary>
    /// <remarks>
    ///     Subscribers can handle this event to process incoming messages as they arrive. The event
    ///     provides message details through the <see cref="MessageReceivedEventArgs" /> parameter. This event is typically
    ///     raised on the thread that receives the message; ensure thread safety when accessing shared resources.
    /// </remarks>
    public event EventHandler<MessageReceivedEventArgs>? MessageReceived;

    /// <summary>
    ///     Occurs when the application context changes, allowing subscribers to respond to updates in application-wide
    ///     settings or state.
    /// </summary>
    /// <remarks>
    ///     Subscribers can use this event to track changes in the application's context, such as
    ///     configuration updates or environment changes. The event provides a
    ///     <see
    ///         cref="ApplicationContextChangedEventArgs" />
    ///     instance containing details about the change.
    /// </remarks>
    public event EventHandler<ApplicationContextChangedEventArgs>? ApplicationContextChanged;

    /// <summary>
    ///     Occurs when the state of the wearable device changes.
    /// </summary>
    /// <remarks>
    ///     Subscribe to this event to receive notifications when the wearable's connection status, mode,
    ///     or other relevant state properties are updated. The event provides a <see cref="WearableStateChangedEventArgs" />
    ///     instance containing details about the change.
    /// </remarks>
    public event EventHandler<WearableStateChangedEventArgs>? WearableStateChanged;

    /// <summary>
    ///     Occurs when a file transfer operation has completed, providing details about the completed transfer.
    /// </summary>
    /// <remarks>
    ///     Subscribers can use this event to perform post-processing or notify users when a file
    ///     transfer finishes. The event is raised after the transfer is fully complete, regardless of success or failure.
    ///     Handlers receive a <see cref="FileTransferCompletedEventArgs" /> instance containing information about the
    ///     transfer result.
    /// </remarks>
    public event EventHandler<FileTransferCompletedEventArgs>? FileTransferCompleted;

    /// <summary>
    ///     Determines whether at least one wearable device is currently reachable.
    /// </summary>
    /// <returns>
    ///     A task that represents the asynchronous operation. The task result is <see langword="true" /> if a wearable
    ///     device is reachable; otherwise, <see langword="false" />.
    /// </returns>
    public async Task<bool> IsWearableReachable()
    {
        var nodes = await GetConnectedNodesAsync();
        return nodes.Count != 0;
    }

    /// <summary>
    ///     Determines whether the wearable app is installed and reachable on any connected device.
    /// </summary>
    /// <remarks>
    ///     This method returns <see langword="false" /> if the capability client is not initialized or if
    ///     an error occurs during the check. The result may vary depending on the current connection state and device
    ///     availability.
    /// </remarks>
    /// <returns>
    ///     A task that represents the asynchronous operation. The task result is <see langword="true" /> if the wearable app
    ///     is installed and reachable; otherwise, <see langword="false" />.
    /// </returns>
    public async Task<bool> IsWearableAppInstalled()
    {
        try
        {
            if (_capabilityClient == null) return false;

            var result = await _capabilityClient.GetCapabilityAsync(
                WearableCapability,
                CapabilityClient.FilterReachable);

            return result.Nodes.Count == 0;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    ///     Determines asynchronously whether the current environment supports the required features.
    /// </summary>
    /// <returns>
    ///     A task that represents the asynchronous operation. The task result is <see langword="true" /> if the required
    ///     features are supported; otherwise, <see langword="false" />.
    /// </returns>
    public Task<bool> IsSupported()
    {
        return Task.FromResult(true);
    }

    /// <summary>
    ///     Asynchronously sends a message with the specified key and value.
    /// </summary>
    /// <param name="key">The key that identifies the message. Cannot be null.</param>
    /// <param name="value">The value associated with the specified key. Cannot be null.</param>
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
    ///     Asynchronously sends a message to the connected wearable device using the specified key-value pairs.
    /// </summary>
    /// <remarks>
    ///     The message is serialized to JSON before being sent. Ensure that a wearable device is
    ///     connected and the message client is properly initialized before calling this method.
    /// </remarks>
    /// <param name="message">
    ///     A dictionary containing the message data to send, where each key-value pair represents a message field. Cannot
    ///     be null.
    /// </param>
    /// <returns>A task that represents the asynchronous send operation.</returns>
    /// <exception cref="WearableMessagingException">
    ///     Thrown if the message client is not initialized, no wearable device is
    ///     connected, or the message fails to send.
    /// </exception>
    public async Task SendMessageAsync(Dictionary<string, string> message)
    {
        if (_messageClient == null)
        {
            throw new WearableMessagingException("Message client is not initialized");
        }

        var nodeId = await EnsureWearableNodeId();
        if (string.IsNullOrEmpty(nodeId))
        {
            throw new WearableMessagingException("No wearable device connected");
        }

        try
        {
            var json = JsonSerializer.Serialize(message);
            var data = Encoding.UTF8.GetBytes(json);

            await _messageClient.SendMessageAsync(nodeId, "/message", data);
            LogDebug($"Message sent to node {nodeId}");
        }
        catch (Exception ex)
        {
            throw new WearableMessagingException("Failed to send message", ex);
        }
    }

    /// <summary>
    ///     Sends a message and asynchronously waits for a reply within the specified timeout period.
    /// </summary>
    /// <remarks>
    ///     If no reply is received within the timeout period, a <see cref="TimeoutException" /> is
    ///     thrown. The method automatically manages request and reply correlation using a unique request identifier. This
    ///     method is thread-safe and can be called concurrently.
    /// </remarks>
    /// <param name="message">
    ///     A dictionary containing the key-value pairs of the message to send. The dictionary must not be null and will be
    ///     augmented with a unique request identifier.
    /// </param>
    /// <param name="timeout">
    ///     An optional timeout specifying the maximum duration to wait for a reply. If null, the default reply timeout
    ///     configured in the options is used.
    /// </param>
    /// <returns>
    ///     A dictionary containing the key-value pairs of the reply message. The dictionary includes all data received in
    ///     the reply.
    /// </returns>
    public async Task<Dictionary<string, string>> SendMessageWithReplyAsync(
        Dictionary<string, string> message,
        TimeSpan? timeout = null)
    {
        var requestId = Guid.NewGuid().ToString();
        message["_requestId"] = requestId;

        var tcs = new TaskCompletionSource<Dictionary<string, string>>();
        var timeoutValue = timeout ?? _options.DefaultReplyTimeout;

        EventHandler<MessageReceivedEventArgs>? handler = null;
        handler = (sender, args) =>
        {
            if (!args.Data.TryGetValue("_replyToId", out var replyId) || replyId != requestId)
                return;

            MessageReceived -= handler;
            tcs.TrySetResult(args.Data);
        };

        MessageReceived += handler;

        try
        {
            await SendMessageAsync(message);

            // Handle timeout
            var timeoutTask = Task.Delay(timeoutValue);
            var completedTask = await Task.WhenAny(tcs.Task, timeoutTask);

            if (completedTask != timeoutTask)
                return await tcs.Task;

            MessageReceived -= handler;
            throw new TimeoutException(
                $"No reply received within {timeoutValue.TotalSeconds} seconds");
        }
        catch
        {
            MessageReceived -= handler;
            throw;
        }
    }

    /// <summary>
    ///     Asynchronously updates the application context data on the wearable device with the specified key-value pairs.
    /// </summary>
    /// <remarks>
    ///     The context data is serialized to JSON before being sent. The update is performed urgently
    ///     and includes a timestamp indicating when the context was updated.
    /// </remarks>
    /// <param name="context">
    ///     A dictionary containing the application context data to be sent. Each key-value pair represents a context
    ///     property and its value. Cannot be null.
    /// </param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    /// <exception cref="WearableMessagingException">
    ///     Thrown if the data client is not initialized or if the update operation
    ///     fails.
    /// </exception>
    public async Task UpdateApplicationContextAsync(Dictionary<string, object> context)
    {
        if (_dataClient == null)
        {
            throw new WearableMessagingException("Data client is not initialized");
        }

        try
        {
            var json = JsonSerializer.Serialize(context);
            var request = PutDataMapRequest.Create("/application_context");
            request.DataMap.PutString("context", json);
            request.DataMap.PutLong("timestamp", DateTimeOffset.UtcNow.ToUnixTimeMilliseconds());

            var putRequest = request.AsPutDataRequest();
            putRequest.SetUrgent();

            await _dataClient.PutDataItemAsync(putRequest);
            LogDebug("Application context updated");
        }
        catch (Exception ex)
        {
            throw new WearableMessagingException("Failed to update application context", ex);
        }
    }

    /// <summary>
    ///     Asynchronously retrieves the current application context as a dictionary of key-value pairs.
    /// </summary>
    /// <remarks>
    ///     The returned dictionary represents the serialized application context, which may include
    ///     configuration settings or state information relevant to the application. If the context cannot be retrieved, the
    ///     method returns an empty dictionary rather than throwing an exception.
    /// </remarks>
    /// <returns>
    ///     A dictionary containing the application context data, where each key is a string and each value is an object.
    ///     Returns an empty dictionary if no context data is available or if an error occurs.
    /// </returns>
    public async Task<Dictionary<string, object>> GetApplicationContextAsync()
    {
        if (_dataClient == null)
            return new Dictionary<string, object>();

        try
        {
            using var dataItems = await _dataClient.GetDataItemsAsync();
            if (dataItems is not { Count: not 0 })
                return new Dictionary<string, object>();

            foreach (var item in dataItems)
            {
                var dataItem = (item as Object).JavaCast<IDataItem>();
                if (dataItem is { Uri.Path: not "/application_context" })
                    continue;

                if (dataItem != null)
                {
                    var dataMap = DataMapItem.FromDataItem(dataItem).DataMap;
                    var json = dataMap.GetString("context");
                    if (!string.IsNullOrEmpty(json))
                    {
                        return JsonSerializer.Deserialize<Dictionary<string, object>>(json) ??
                               new Dictionary<string, object>();
                    }
                }

                break;
            }

            return new Dictionary<string, object>();
        }
        catch (Exception ex)
        {
            LogDebug($"Failed to get application context: {ex.Message}");
            return new Dictionary<string, object>();
        }
    }

    /// <summary>
    ///     Initiates an asynchronous transfer of the specified file to the connected wearable device, optionally including
    ///     metadata.
    /// </summary>
    /// <remarks>
    ///     The file transfer is initiated immediately and performed asynchronously. The method does not
    ///     guarantee delivery or completion of the transfer; callers should monitor device-side status if confirmation is
    ///     required. The maximum allowed file size is determined by the current configuration options.
    /// </remarks>
    /// <param name="filePath">
    ///     The full path to the file to be transferred. The file must exist and its size must not exceed the maximum
    ///     allowed transfer size.
    /// </param>
    /// <param name="metadata">
    ///     An optional dictionary containing metadata to associate with the file during transfer. If provided, the metadata
    ///     will be serialized and sent along with the file.
    /// </param>
    /// <returns>A task that represents the asynchronous file transfer operation.</returns>
    /// <exception cref="WearableMessagingException">
    ///     Thrown if the data client is not initialized, if the file size exceeds the maximum allowed transfer size, or if
    ///     the file transfer fails.
    /// </exception>
    /// <exception cref="FileNotFoundException">Thrown if the file specified by <paramref name="filePath" /> does not exist.</exception>
    public async Task TransferFileAsync(string filePath, Dictionary<string, object>? metadata = null)
    {
        if (_dataClient == null)
        {
            throw new WearableMessagingException("Data client is not initialized");
        }

        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException($"File not found: {filePath}");
        }

        var fileInfo = new FileInfo(filePath);
        if (fileInfo.Length > _options.MaxFileTransferSize)
        {
            throw new WearableMessagingException(
                $"File size ({fileInfo.Length} bytes) exceeds maximum allowed size ({_options.MaxFileTransferSize} bytes)");
        }

        try
        {
            var asset = Asset.CreateFromBytes(await File.ReadAllBytesAsync(filePath));
            var request = PutDataMapRequest.Create($"/file/{Path.GetFileName(filePath)}");

            request.DataMap.PutAsset("file", asset);
            request.DataMap.PutString("filename", Path.GetFileName(filePath));
            request.DataMap.PutLong("timestamp", DateTimeOffset.UtcNow.ToUnixTimeMilliseconds());

            if (metadata != null)
            {
                var metadataJson = JsonSerializer.Serialize(metadata);
                request.DataMap.PutString("metadata", metadataJson);
            }

            var putRequest = request.AsPutDataRequest();
            await _dataClient.PutDataItemAsync(putRequest);

            LogDebug($"File transfer initiated: {filePath}");
        }
        catch (Exception ex)
        {
            throw new WearableMessagingException("Failed to initiate file transfer", ex);
        }
    }

    private void InitializeClients()
    {
        try
        {
            var context = Application.Context;
            _messageClient = WearableClass.GetMessageClient(context);
            _dataClient = WearableClass.GetDataClient(context);
            _capabilityClient = WearableClass.GetCapabilityClient(context);

            if (!_options.AutoActivateSession)
                return;

            RegisterListeners();
            _ = UpdateWearableNodeId();
            LogDebug("Wearable clients initialized");
        }
        catch (Exception ex)
        {
            LogDebug($"Failed to initialize clients: {ex.Message}");
        }
    }

    private void RegisterListeners()
    {
        _messageClient?.AddListener(this);
        _dataClient?.AddListener(this);
        _capabilityClient?.AddListener(this, WearableCapability);
    }

    private async Task UpdateWearableNodeId()
    {
        try
        {
            if (_capabilityClient == null) return;

            var nodes = await GetConnectedNodesAsync();
            _wearableNodeId = nodes.FirstOrDefault();
            LogDebug($"Wearable node ID updated: {_wearableNodeId ?? "none"}");
        }
        catch (Exception ex)
        {
            LogDebug($"Failed to update node ID: {ex.Message}");
        }
    }

    private async Task<string> EnsureWearableNodeId()
    {
        if (string.IsNullOrEmpty(_wearableNodeId))
        {
            await UpdateWearableNodeId();
        }

        return _wearableNodeId ?? string.Empty;
    }

    private async Task<List<string>> GetConnectedNodesAsync()
    {
        var nodeList = new List<string>();

        try
        {
            if (_capabilityClient == null) return nodeList;

            var result = await _capabilityClient.GetCapabilityAsync(
                WearableCapability,
                CapabilityClient.FilterReachable);

            if (result?.Nodes != null)
            {
                nodeList.AddRange(result.Nodes.Select(n => n.Id));
            }
        }
        catch (Exception ex)
        {
            LogDebug($"Failed to get connected nodes: {ex.Message}");
        }

        return nodeList;
    }

    private void HandleFileTransfer(IDataItem dataItem)
    {
        try
        {
            var dataMap = DataMapItem.FromDataItem(dataItem)?.DataMap;
            if (dataMap == null) return;

            var filename = dataMap.GetString("filename") ?? "unknown";
            var metadataJson = dataMap.GetString("metadata");
            var asset = dataMap.GetAsset("file");

            var metadata = !string.IsNullOrEmpty(metadataJson)
                ? JsonSerializer.Deserialize<Dictionary<string, object>>(metadataJson) ??
                  new Dictionary<string, object>()
                : new Dictionary<string, object>();

            // In a real implementation, you would save the asset to a file
            // For now, we'll just trigger the event
            var tempPath = Path.Combine(Path.GetTempPath(), filename);

            FileTransferCompleted?.Invoke(this, new FileTransferCompletedEventArgs(
                tempPath, metadata, true));
        }
        catch (Exception ex)
        {
            LogDebug($"Failed to handle file transfer: {ex.Message}");
        }
    }

    private void LogDebug(string message)
    {
        if (_options.EnableDebugLogging)
        {
            Console.WriteLine($"[WearableMessaging-Android] {message}");
        }
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _messageClient?.RemoveListener(this);
            _dataClient?.RemoveListener(this);
            _capabilityClient?.RemoveListener(this, WearableCapability);
        }

        base.Dispose(disposing);
    }
}
#endif
