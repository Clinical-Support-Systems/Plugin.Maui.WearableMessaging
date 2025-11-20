using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Plugin.Maui.WearableMessaging
{
    /// <summary>
    /// Implementation for platforms that don't support wearable messaging.
    /// All methods throw NotSupportedException.
    /// </summary>
    public class WearableMessagingNotSupportedImplementation : IWearableMessaging
    {
        public event EventHandler<MessageReceivedEventArgs>? MessageReceived;
        public event EventHandler<ApplicationContextChangedEventArgs>? ApplicationContextChanged;
        public event EventHandler<WearableStateChangedEventArgs>? WearableStateChanged;
        public event EventHandler<FileTransferCompletedEventArgs>? FileTransferCompleted;

        public Task<bool> IsWearableReachable()
        {
            return Task.FromResult(false);
        }

        public Task<bool> IsWearableAppInstalled()
        {
            return Task.FromResult(false);
        }

        public Task<bool> IsSupported()
        {
            return Task.FromResult(false);
        }

        public Task SendMessageAsync(string key, string value)
        {
            throw new NotSupportedException("Wearable messaging is not supported on this platform");
        }

        public Task SendMessageAsync(Dictionary<string, string> message)
        {
            throw new NotSupportedException("Wearable messaging is not supported on this platform");
        }

        public Task<Dictionary<string, string>> SendMessageWithReplyAsync(
            Dictionary<string, string> message,
            TimeSpan? timeout = null)
        {
            throw new NotSupportedException("Wearable messaging is not supported on this platform");
        }

        public Task UpdateApplicationContextAsync(Dictionary<string, object> context)
        {
            throw new NotSupportedException("Wearable messaging is not supported on this platform");
        }

        public Task<Dictionary<string, object>> GetApplicationContextAsync()
        {
            return Task.FromResult(new Dictionary<string, object>());
        }

        public Task TransferFileAsync(string filePath, Dictionary<string, object>? metadata = null)
        {
            throw new NotSupportedException("Wearable messaging is not supported on this platform");
        }
    }
}
