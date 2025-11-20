namespace Plugin.Maui.WearableMessaging.Tests;

public class WearableMessagingOptionsTests
{
    [Test]
    public async Task DefaultValues_AreCorrect()
    {
        var options = new WearableMessagingOptions();
        await Assert.That(options.DefaultReplyTimeout).IsEqualTo(TimeSpan.FromSeconds(5));
        await Assert.That(options.AutoActivateSession).IsTrue();
        await Assert.That(options.EnableDebugLogging).IsFalse();
        await Assert.That(options.MaxFileTransferSize).IsEqualTo(10 * 1024 * 1024);
    }

    [Test]
    public async Task Setters_WorkCorrectly()
    {
        var options = new WearableMessagingOptions
        {
            DefaultReplyTimeout = TimeSpan.FromSeconds(10),
            AutoActivateSession = false,
            EnableDebugLogging = true,
            MaxFileTransferSize = 42
        };
        await Assert.That(options.DefaultReplyTimeout).IsEqualTo(TimeSpan.FromSeconds(10));
        await Assert.That(options.AutoActivateSession).IsFalse();
        await Assert.That(options.EnableDebugLogging).IsTrue();
        await Assert.That(options.MaxFileTransferSize).IsEqualTo(42);
    }
}
