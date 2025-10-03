namespace Plugin.Maui.WearableMessaging;

/// <summary>
///     Event arguments for when the wearable device connection state changes.
/// </summary>
public class WearableStateChangedEventArgs : EventArgs
{
    /// <summary>
    ///     Initializes a new instance of the WearableStateChangedEventArgs class with the specified wearable device state
    ///     information.
    /// </summary>
    /// <param name="isReachable">
    ///     Indicates whether the wearable device is currently reachable from the host device. Set to <see langword="true" />
    ///     if reachable; otherwise, <see langword="false" />.
    /// </param>
    /// <param name="isAppInstalled">
    ///     Indicates whether the required application is installed on the wearable device. Set to <see langword="true" /> if
    ///     installed; otherwise, <see langword="false" />.
    /// </param>
    /// <param name="isPaired">
    ///     Indicates whether the wearable device is paired with the host device. Set to <see langword="true" /> if paired;
    ///     otherwise, <see langword="false" />.
    /// </param>
    public WearableStateChangedEventArgs(bool isReachable, bool isAppInstalled, bool isPaired)
    {
        IsReachable = isReachable;
        IsAppInstalled = isAppInstalled;
        IsPaired = isPaired;
    }

    /// <summary>
    ///     Indicates whether the wearable device is currently reachable.
    /// </summary>
    public bool IsReachable { get; }

    /// <summary>
    ///     Indicates whether the wearable companion app is installed.
    /// </summary>
    public bool IsAppInstalled { get; }

    /// <summary>
    ///     Indicates whether the wearable device is currently paired.
    /// </summary>
    public bool IsPaired { get; }
}
