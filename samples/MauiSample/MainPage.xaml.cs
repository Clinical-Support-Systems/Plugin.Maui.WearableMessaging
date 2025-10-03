using Plugin.Maui.WearableMessaging;

namespace MauiSample
{
    public partial class MainPage : ContentPage
    {
        private readonly IWearableMessaging _wearableMessaging;
        private int _count;

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


        private async Task SendToWatchAsync()
        {
            if (await _wearableMessaging.IsWearableReachable())
            {
                await _wearableMessaging.SendMessageAsync("counter", DateTime.UtcNow.Ticks.ToString());
            }
        }

        private void OnCounterClicked(object? sender, EventArgs e)
        {
            _count++;

            CounterBtn.Text = _count == 1 ? $"Clicked {_count} time" : $"Clicked {_count} times";

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
