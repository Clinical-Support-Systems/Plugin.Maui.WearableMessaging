using Plugin.Maui.WearableMessaging;

namespace MauiSample
{
    public partial class MainPage : ContentPage
    {
        private readonly IWearableMessaging _wearableMessaging;
        int count = 0;

        public MainPage(IWearableMessaging wearableMessaging)
        {
            _wearableMessaging = wearableMessaging;
            InitializeComponent();
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
    }
}
