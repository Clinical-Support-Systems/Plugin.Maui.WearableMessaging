using System.Collections.Concurrent;

namespace Plugin.Maui.WearableMessaging;

/// <summary>
/// Loopback in-process implementation of <see cref="IWearableMessaging"/> for fast local development
/// (primarily intended for iOS Simulator where a paired watch app isn't available during early UI work).
/// </summary>
public sealed class LoopbackWearableMessaging : IWearableMessaging
{
    private readonly ConcurrentDictionary<string, object> _appContext = new();

    /// <inheritdoc />
    public event EventHandler<MessageReceivedEventArgs>? MessageReceived;

    /// <inheritdoc />
    public event EventHandler<ApplicationContextChangedEventArgs>? ApplicationContextChanged;

    /// <inheritdoc />
    public event EventHandler<WearableStateChangedEventArgs>? WearableStateChanged;

    /// <inheritdoc />
    public event EventHandler<FileTransferCompletedEventArgs>? FileTransferCompleted;

    /// <inheritdoc />
    public event EventHandler<UserInfoReceivedEventArgs>? UserInfoReceived;

    /// <summary>
    /// Initializes a new instance of the LoopbackWearableMessaging class and raises the WearableStateChanged event to
    /// indicate that all wearable states are active.
    /// </summary>
    /// <remarks>This constructor triggers the WearableStateChanged event with all state flags set to <see
    /// langword="true"/>. This ensures that any event subscribers are immediately notified of the initial wearable
    /// state upon instantiation.</remarks>
    public LoopbackWearableMessaging()
    {
        WearableStateChanged?.Invoke(this, new WearableStateChangedEventArgs(true, true, true));
    }

    /// <inheritdoc />
    public Task<bool> IsWearableReachable() => Task.FromResult(true);

    /// <inheritdoc />
    public Task<bool> IsWearableAppInstalled() => Task.FromResult(true);

    /// <inheritdoc />
    public Task<bool> IsSupported() => Task.FromResult(true);

    /// <inheritdoc />
    public Task SendMessageAsync(string key, string value)
        => SendMessageAsync(new Dictionary<string, string> { { key, value } });

    /// <inheritdoc />
    public Task SendMessageAsync(Dictionary<string, string> message)
    {
        MessageReceived?.Invoke(this, new MessageReceivedEventArgs(message));
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task<Dictionary<string, string>> SendMessageWithReplyAsync(
        Dictionary<string, string> message,
        TimeSpan? timeout = null)
    {
        var reply = new Dictionary<string, string>(message)
        {
            ["_reply"] = "ok"
        };
        MessageReceived?.Invoke(this, new MessageReceivedEventArgs(message, _ => { }));
        return Task.FromResult(reply);
    }

    /// <inheritdoc />
    public Task UpdateApplicationContextAsync(Dictionary<string, object> context)
    {
        _appContext.Clear();
        foreach (var kv in context)
            _appContext[kv.Key] = kv.Value;
        ApplicationContextChanged?.Invoke(this, new ApplicationContextChangedEventArgs(new Dictionary<string, object>(_appContext)));
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task<Dictionary<string, object>> GetApplicationContextAsync()
        => Task.FromResult(new Dictionary<string, object>(_appContext));

    /// <inheritdoc />
    public Task TransferFileAsync(string filePath, Dictionary<string, object>? metadata = null)
    {
        FileTransferCompleted?.Invoke(this, new FileTransferCompletedEventArgs(filePath, metadata, true));
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task TransferUserInfoAsync(Dictionary<string, object> userInfo)
    {
        // In loopback mode, immediately trigger the UserInfoReceived event to simulate reception
        UserInfoReceived?.Invoke(this, new UserInfoReceivedEventArgs(userInfo));
        return Task.CompletedTask;
    }
}
