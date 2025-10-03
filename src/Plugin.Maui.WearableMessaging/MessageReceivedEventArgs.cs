namespace Plugin.Maui.WearableMessaging;

/// <summary>
/// Event arguments for when a message is received from the wearable device.
/// </summary>
public class MessageReceivedEventArgs : EventArgs
{
    /// <summary>
    /// The message data received from the wearable device.
    /// </summary>
    public Dictionary<string, string> Data { get; }

    /// <summary>
    /// Optional reply handler to send a response back to the wearable device.
    /// </summary>
    public Action<Dictionary<string, string>>? ReplyHandler { get; }

    /// <summary>
    /// Initializes a new instance of the MessageReceivedEventArgs class with the specified message data and an optional
    /// reply handler.
    /// </summary>
    /// <remarks>Use the replyHandler parameter to enable responses to the received message. If replyHandler
    /// is not specified, the event arguments are read-only and cannot be used to send a reply.</remarks>
    /// <param name="data">A dictionary containing the key-value pairs representing the message data. Cannot be null; if null, an empty
    /// dictionary is used.</param>
    /// <param name="replyHandler">An optional callback that, if provided, is invoked to send a reply using the message data. If null, no reply
    /// handler is associated.</param>
    public MessageReceivedEventArgs(Dictionary<string, string>? data, Action<Dictionary<string, string>>? replyHandler = null)
    {
        Data = data ?? new Dictionary<string, string>();
        ReplyHandler = replyHandler;
    }
}
