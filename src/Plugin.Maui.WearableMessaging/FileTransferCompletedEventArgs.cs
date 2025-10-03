namespace Plugin.Maui.WearableMessaging;

/// <summary>
///     Event arguments for when a file transfer from the wearable device completes.
/// </summary>
public class FileTransferCompletedEventArgs : EventArgs
{
    /// <summary>
    ///     Initializes a new instance of the FileTransferCompletedEventArgs class with information about a completed file
    ///     transfer operation.
    /// </summary>
    /// <param name="filePath">The full path of the file that was transferred. Cannot be null or empty.</param>
    /// <param name="metadata">
    ///     A dictionary containing additional metadata related to the file transfer. If null, an empty
    ///     dictionary is used.
    /// </param>
    /// <param name="success">
    ///     A value indicating whether the file transfer completed successfully. Set to <see langword="true" /> if the
    ///     transfer succeeded; otherwise, <see langword="false" />.
    /// </param>
    /// <param name="errorMessage">
    ///     An optional error message describing the reason for failure if the transfer was not successful. Can be null if
    ///     no error occurred.
    /// </param>
    public FileTransferCompletedEventArgs(
        string filePath,
        Dictionary<string, object>? metadata,
        bool success,
        string? errorMessage = null)
    {
        FilePath = filePath;
        Metadata = metadata ?? new Dictionary<string, object>();
        Success = success;
        ErrorMessage = errorMessage;
    }

    /// <summary>
    ///     The local file path where the transferred file was saved.
    /// </summary>
    public string FilePath { get; }

    /// <summary>
    ///     Metadata associated with the transferred file.
    /// </summary>
    public Dictionary<string, object> Metadata { get; }

    /// <summary>
    ///     Indicates whether the transfer was successful.
    /// </summary>
    public bool Success { get; }

    /// <summary>
    ///     Error message if the transfer failed.
    /// </summary>
    public string? ErrorMessage { get; }
}
