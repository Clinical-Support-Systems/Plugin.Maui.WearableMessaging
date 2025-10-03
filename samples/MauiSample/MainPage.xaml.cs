using Plugin.Maui.WearableMessaging;
using static AndroidX.ConstraintLayout.Core.Motion.Utils.HyperSpline;

namespace MauiSample
{
    public partial class MainPage : ContentPage
    {
        private readonly IWearableMessaging _wearableMessaging;
        int count = 0;

        public MainPage(IWearableMessaging wearableMessaging)
        {
            _wearableMessaging = wearableMessaging;
            _wearableMessaging.MessageReceived += async (s, e) =>
            {
                if (e.Data.TryGetValue("counter", out var reply))
                {
                    await MainThread.InvokeOnMainThreadAsync(() =>
                    {
                        CounterBtn.Text = $"Received reply: {reply}";
                    });
                }
            };
            InitializeComponent();
        }

        
        public async Task SendToWatchAsync()
        {
            if (await _wearableMessaging.IsWearableReachable())
            {
                await _wearableMessaging.SendMessageAsync("counter", "42");
            }
        }

        private void OnCounterClicked(object? sender, EventArgs e)
        {
            count++;

            if (count == 1)
                CounterBtn.Text = $"Clicked {count} time";
            else
                CounterBtn.Text = $"Clicked {count} times";

            SemanticScreenReader.Announce(CounterBtn.Text);
        }

        private async void OnWearableTestClicked(object? sender, EventArgs e)
        {
            try
            {
                await SendToWatchAsync();
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }
    }
}
