#if ANDROID
using Plugin.Maui.WearableMessaging.Platforms.Android;

namespace Plugin.Maui.WearableMessaging.Tests;

public class AndroidWearableMessagingImplementationTests
{
    [Test]
    public async Task Constructor_DoesNotInitializeWearableClients()
    {
        var implementation = new WearableMessagingImplementation(
            new WearableMessagingOptions(),
            _ => throw CreateApiUnavailableException(),
            _ => throw new InvalidOperationException("Should not be invoked during construction"),
            _ => throw new InvalidOperationException("Should not be invoked during construction"));

        await Assert.That(implementation).IsNotNull();
    }

    [Test]
    public async Task IsSupported_ReturnsFalse_WhenWearableApiUnavailable()
    {
        var implementation = CreateUnavailableImplementation();

        var result = await implementation.IsSupported();

        await Assert.That(result).IsFalse();
    }

    [Test]
    public async Task SendMessageAsync_ThrowsWearableMessagingException_WhenApiUnavailable()
    {
        var implementation = CreateUnavailableImplementation();

        var threwExpected = false;

        try
        {
            await implementation.SendMessageAsync(new Dictionary<string, string>
            {
                { "key", "value" }
            });
        }
        catch (WearableMessagingException ex)
        {
            threwExpected = ex.Message.Contains("Wearable API is not available on this device");
        }

        await Assert.That(threwExpected).IsTrue();
    }

    [Test]
    public async Task IsWearableReachable_ReturnsFalse_WhenApiUnavailable()
    {
        var implementation = CreateUnavailableImplementation();

        var result = await implementation.IsWearableReachable();

        await Assert.That(result).IsFalse();
    }

    private static WearableMessagingImplementation CreateUnavailableImplementation()
    {
        return new WearableMessagingImplementation(
            new WearableMessagingOptions(),
            _ => throw CreateApiUnavailableException(),
            _ => throw new InvalidOperationException("Data client should not be created when wearable API is unavailable"),
            _ => throw new InvalidOperationException("Capability client should not be created when wearable API is unavailable"));
    }

    private static Exception CreateApiUnavailableException()
    {
        return new InvalidOperationException("17: API: Wearable.API is not available on this device");
    }
}
#endif
