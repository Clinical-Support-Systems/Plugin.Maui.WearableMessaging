using System;
using System.Collections.Generic;


namespace Plugin.Maui.WearableMessaging;
/// <summary>
///     Event arguments for user info received events.
/// </summary>
public class UserInfoReceivedEventArgs : EventArgs
{
    /// <summary>
    ///     Initializes a new instance of the UserInfoReceivedEventArgs class with the specified user info data.
    /// </summary>
    /// <param name="userInfo">The user info data received from the wearable device.</param>
    public UserInfoReceivedEventArgs(Dictionary<string, object> userInfo)
    {
        UserInfo = userInfo ?? new Dictionary<string, object>();
    }

    /// <summary>
    ///     Gets the user info data received from the wearable device.
    /// </summary>
    public Dictionary<string, object> UserInfo { get; }
}
