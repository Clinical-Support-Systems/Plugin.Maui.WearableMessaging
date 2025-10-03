# Plugin.Maui.WearableMessaging

A .NET MAUI plugin that enables seamless bidirectional communication between MAUI applications and native wearable devices (Apple Watch, Wear OS).

[![NuGet](https://img.shields.io/nuget/v/Plugin.Maui.WearableMessaging.svg)](https://www.nuget.org/packages/Plugin.Maui.WearableMessaging/)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)

## Features

- 🔄 **Bidirectional Messaging** - Send and receive messages between MAUI app and wearable
- 📊 **Data Synchronization** - Keep application context synchronized across devices
- 🔌 **Connectivity Management** - Monitor wearable connection state and reachability
- 🎯 **Unified API** - Single cross-platform interface for iOS and Android
- ⚡ **Event-Driven** - React to messages and state changes with events
- 🛡️ **Type-Safe** - Strongly-typed C# API with async/await support

## Supported Platforms

| Platform | Wearable Device | Native Framework |
|----------|----------------|------------------|
| iOS | Apple Watch | WatchConnectivity |
| Android | Wear OS | Wearable Data Layer API |

## Installation

```bash
dotnet add package Plugin.Maui.WearableMessaging
```

## Quick Start

### 1. Register the Plugin

In your `MauiProgram.cs`:

```csharp
public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .UseWearableMessaging(); // Add this line

        return builder.Build();
    }
}
```

### 2. Use in Your Code

```csharp
public class MainViewModel
{
    private readonly IWearableMessaging _wearable;

    public MainViewModel(IWearableMessaging wearableMessaging)
    {
        _wearable = wearableMessaging;
        _wearable.MessageReceived += OnMessageReceived;
    }

    public async Task SendToWatchAsync()
    {
        if (await _wearable.IsWearableReachable())
        {
            await _wearable.SendMessageAsync("counter", "42");
        }
    }

    private void OnMessageReceived(object sender, MessageReceivedEventArgs e)
    {
        // Handle message from wearable
        Console.WriteLine($"Received: {e.Data["action"]}");
    }
}
```

## Simulator / Loopback Development Mode (iOS)

When iterating quickly on UI / business logic you may not yet have a paired Apple Watch app running (or you simply want instant turnaround without launching the watch simulator). A lightweight in‑process loopback implementation (`LoopbackWearableMessaging`) is included to short‑circuit all transport calls.

### What It Does
- Pretends the wearable is installed, paired, and reachable
- Immediately echoes request/reply messages (adds `"_reply":"ok"`)
- Raises `MessageReceived` for any outbound message you send
- Fires `ApplicationContextChanged` and `FileTransferCompleted` instantly

### What It Does NOT Simulate
- Real `WatchConnectivity` reachability transitions
- Background delivery / timing constraints
- Payload size limits or throttling
- Actual watch app install/uninstall state

### Conditional Registration Example
Register the loopback only on iOS Simulator in DEBUG; use the real implementation elsewhere:

```csharp
builder
    .UseMauiApp<App>()
#if DEBUG && IOS && TARGET_SIMULATOR
    .ConfigureServices(services =>
    {
        services.AddSingleton<IWearableMessaging, LoopbackWearableMessaging>();
    })
#else
    .UseWearableMessaging(o =>
    {
        o.DefaultReplyTimeout = TimeSpan.FromSeconds(30);
        o.EnableDebugLogging = true;
    })
#endif
```

### When to Switch to Real Devices
Move to a paired simulator (iPhone + Watch) or physical hardware before validating:
- Connectivity edge cases (unreachable / re-connect scenarios)
- Background wake / delayed delivery
- Larger payload or file transfer behavior
- Performance & battery impact

## Platform Setup

### iOS (Apple Watch)

1. Create a watchOS target in Xcode for your iOS project
2. Use `WatchConnectivity` framework in your Swift/SwiftUI watch app
3. Bundle the watch app with your MAUI iOS app

**SwiftUI Example:**

```swift
import WatchConnectivity

class WatchViewModel: NSObject, ObservableObject, WCSessionDelegate {
    func sendMessage() {
        let session = WCSession.default
        session.sendMessage(["action": "buttonPressed"], replyHandler: nil)
    }
}
```

### Android (Wear OS)

1. Create a Wear OS module in your Android project
2. Use Wearable Data Layer API in your Kotlin/Java wear app
3. Deploy as a separate APK or bundled module

**Kotlin Example:**

```kotlin
class WearViewModel {
    fun sendMessage(context: Context) {
        val messageClient = Wearable.getMessageClient(context)
        messageClient.sendMessage(nodeId, "/action", "buttonPressed".toByteArray())
    }
}
```

## API Reference

### IWearableMessaging Interface

```csharp
public interface IWearableMessaging
{
    // Connectivity
    Task<bool> IsWearableReachable();
    Task<bool> IsWearableAppInstalled();
    Task<bool> IsSupported();
    
    // Messaging
    Task SendMessageAsync(string key, string value);
    Task SendMessageAsync(Dictionary<string, string> message);
    Task<Dictionary<string, string>> SendMessageWithReplyAsync(
        Dictionary<string, string> message, 
        TimeSpan timeout);
    
    // Data Synchronization
    Task UpdateApplicationContextAsync(Dictionary<string, object> context);
    Task<Dictionary<string, object>> GetApplicationContextAsync();
    
    // File Transfer
    Task TransferFileAsync(string filePath, Dictionary<string, object> metadata = null);
    
    // Events
    event EventHandler<MessageReceivedEventArgs> MessageReceived;
    event EventHandler<ApplicationContextChangedEventArgs> ApplicationContextChanged;
    event EventHandler<WearableStateChangedEventArgs> WearableStateChanged;
}
```

## Examples

See the [samples](./samples/) directory for complete example applications:
- iOS + Apple Watch sample
- Android + Wear OS sample

## Requirements

- .NET 8.0 or higher
- MAUI workload installed
- For iOS: Xcode 15+, watchOS 9+
- For Android: Android SDK 30+, Wear OS 3+

## Contributing

Contributions are welcome! Please feel free to submit a Pull Request.

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## Acknowledgments

Built with ❤️ using .NET MAUI
