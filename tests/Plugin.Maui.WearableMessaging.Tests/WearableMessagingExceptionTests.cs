namespace Plugin.Maui.WearableMessaging.Tests;

public class WearableMessagingExceptionTests
{
    [Test]
    public async Task DefaultConstructor_Works()
    {
        var ex = new WearableMessagingException();
        await Assert.That(ex).IsNotNull();
        await Assert.That(ex.ErrorCode).IsNull();
    }

    [Test]
    public async Task MessageConstructor_Works()
    {
        var ex = new WearableMessagingException("msg");
        await Assert.That(ex.Message).Contains("msg");
        await Assert.That(ex.ErrorCode).IsNull();
    }

    [Test]
    public async Task MessageAndInnerExceptionConstructor_Works()
    {
        var inner = new Exception("inner");
        var ex = new WearableMessagingException("msg", inner);
        await Assert.That(ex.Message).Contains("msg");
        await Assert.That(ex.InnerException).IsEqualTo(inner);
        await Assert.That(ex.ErrorCode).IsNull();
    }

    [Test]
    public async Task MessageAndErrorCodeConstructor_Works()
    {
        var ex = new WearableMessagingException("msg", "E123");
        await Assert.That(ex.Message).Contains("msg");
        await Assert.That(ex.ErrorCode).IsEqualTo("E123");
    }

    [Test]
    public async Task MessageErrorCodeAndInnerExceptionConstructor_Works()
    {
        var inner = new Exception("inner");
        var ex = new WearableMessagingException("msg", "E123", inner);
        await Assert.That(ex.Message).Contains("msg");
        await Assert.That(ex.ErrorCode).IsEqualTo("E123");
        await Assert.That(ex.InnerException).IsEqualTo(inner);
    }
}
