namespace Plugin.Maui.WearableMessaging;

/// <summary>
///     Event arguments for when the application context from the wearable device changes.
/// </summary>
public class ApplicationContextChangedEventArgs : EventArgs
{
    /// <summary>
    ///     Initializes a new instance of the ApplicationContextChangedEventArgs class with the specified application
    ///     context data.
    /// </summary>
    /// <remarks>
    ///     The context dictionary provides contextual information relevant to the application state
    ///     change. Keys and values should be chosen to reflect meaningful state transitions or metadata. The instance will
    ///     always have a non-null Context property.
    /// </remarks>
    /// <param name="context">
    ///     A dictionary containing key-value pairs that represent the updated application context. If null, an empty
    ///     dictionary is used.
    /// </param>
    public ApplicationContextChangedEventArgs(Dictionary<string, object>? context)
    {
        Context = context ?? new Dictionary<string, object>();
    }

    /// <summary>
    ///     The updated application context data.
    /// </summary>
    public Dictionary<string, object> Context { get; }
}
