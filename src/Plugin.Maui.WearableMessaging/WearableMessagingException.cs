namespace Plugin.Maui.WearableMessaging;

/// <summary>
///     Exception thrown when wearable messaging operations fail.
/// </summary>
public class WearableMessagingException : Exception
{
    /// <summary>
    ///     Initializes a new instance of the WearableMessagingException class.
    /// </summary>
    public WearableMessagingException()
    {
    }

    /// <summary>
    /// Initializes a new instance of the WearableMessagingException class with a specified error message.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    public WearableMessagingException(string message) : base(message)
    {
    }

    /// <summary>
    /// Initializes a new instance of the WearableMessagingException class with a specified error message and a
    /// reference to the inner exception that is the cause of this exception.
    /// </summary>
    /// <param name="message">The error message that explains the reason for the exception.</param>
    /// <param name="innerException">The exception that is the cause of the current exception, or a null reference if no inner exception is
    /// specified.</param>
    public WearableMessagingException(string message, Exception innerException)
        : base(message, innerException)
    {
    }

    /// <summary>
    /// Initializes a new instance of the WearableMessagingException class with a specified error message and error
    /// code.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    /// <param name="errorCode">A string representing the error code associated with the exception. This can be used to identify the specific
    /// error condition.</param>
    public WearableMessagingException(string message, string errorCode)
        : base(message)
    {
        ErrorCode = errorCode;
    }

    /// <summary>
    /// Initializes a new instance of the WearableMessagingException class with a specified error message, error code,
    /// and a reference to the inner exception that is the cause of this exception.
    /// </summary>
    /// <param name="message">The error message that explains the reason for the exception.</param>
    /// <param name="errorCode">A string representing the specific error code associated with the exception.</param>
    /// <param name="innerException">The exception that is the cause of the current exception, or a null reference if no inner exception is
    /// specified.</param>
    public WearableMessagingException(string message, string errorCode, Exception innerException)
        : base(message, innerException)
    {
        ErrorCode = errorCode;
    }

    /// <summary>
    ///     The error code associated with the exception.
    /// </summary>
    public string? ErrorCode { get; }
}
