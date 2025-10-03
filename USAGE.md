# Usage Guide

This guide will help you integrate Plugin.Maui.WearableMessaging into your .NET MAUI application.

## Table of Contents
1. [Installation](#installation)
2. [Basic Setup](#basic-setup)
3. [Platform Configuration](#platform-configuration)
4. [Usage Examples](#usage-examples)
5. [Creating Native Wearable Apps](#creating-native-wearable-apps)

## Installation

Add the NuGet package to your MAUI project:

```bash
dotnet add package Plugin.Maui.WearableMessaging
```

## Basic Setup

### 1. Register the Plugin

In your `MauiProgram.cs`:

```csharp
using Plugin.Maui.WearableMessaging;

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

### 2. Configure Options (Optional)

```csharp
builder.UseWearableMessaging(options =>
{
    options.DefaultReplyTimeout = TimeSpan.FromSeconds(10);
    options.EnableDebugLogging = true;
    options.MaxFileTransferSize = 20 * 1024 * 1024; // 20MB
});
```

## Platform Configuration

### iOS (Apple Watch)

#### Requirements
- Xcode 15+
- watchOS 9+
- iOS deployment target 14.0+

#### Info.plist Configuration

Add to your iOS `Info.plist`:

```xml
<key>WKWatchKitApp</key>
<true/>
<key>NSSupportsLiveActivities</key>
<true/>
```

#### Entitlements

Your app needs the proper entitlements. Create or update `Entitlements.plist`:

```xml
<?xml version="1.0" encoding="UTF-8"?>
<!DOCTYPE plist PUBLIC "-//Apple//DTD PLIST 1.0//EN" "http://www.apple.com/DTDs/PropertyList-1.0.dtd">
<plist version="1.0">
<dict>
    <key>com.apple.security.application-groups</key>
    <array>
        <string>group.com.yourcompany.yourapp</string>
    </array>
</dict>
</plist>
```

### Android (Wear OS)

#### Requirements
- Android SDK 30+
- Wear OS 3+
- Google Play Services

#### AndroidManifest.xml Configuration

Add to your Android `AndroidManifest.xml`:

```xml
<uses-permission android:name="android.permission.WAKE_LOCK" />
<uses-permission android:name="android.permission.INTERNET" />

<application>
    <!-- Add capability to declare wearable support -->
    <meta-data
        android:name="com.google.android.wearable.standalone"
        android:value="false" />
</application>
```

#### Add capability XML

Create `Resources/xml/wearable_app_desc.xml`:

```xml
<?xml version="1.0" encoding="utf-8"?>
<wearableApp package="com.yourcompany.yourapp">
    <versionCode>1</versionCode>
    <versionName>1.0</versionName>
    <rawPathResId>wearable_app</rawPathResId>
</wearableApp>
```

## Usage Examples

### Example 1: Send a Simple Message

```csharp
public class MainPage : ContentPage
{
    private readonly IWearableMessaging _wearable;

    public MainPage(IWearableMessaging wearableMessaging)
    {
        _wearable = wearableMessaging;
        InitializeComponent();
    }

    private async void OnSendClicked(object sender, EventArgs e)
    {
        try
        {
            if (await _wearable.IsWearableReachable())
            {
                await _wearable.SendMessageAsync("action", "button_pressed");
                await DisplayAlert("Success", "Message sent!", "OK");
            }
            else
            {
                await DisplayAlert("Error", "Wearable not reachable", "OK");
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", ex.Message, "OK");
        }
    }
}
```

### Example 2: Receive Messages

```csharp
public class MessageService
{
    private readonly IWearableMessaging _wearable;

    public MessageService(IWearableMessaging wearableMessaging)
    {
        _wearable = wearableMessaging;
        _wearable.MessageReceived += OnMessageReceived;
    }

    private void OnMessageReceived(object sender, MessageReceivedEventArgs e)
    {
        if (e.Data.TryGetValue("action", out var action))
        {
            switch (action)
            {
                case "start_workout":
                    StartWorkout();
                    break;
                case "stop_workout":
                    StopWorkout();
                    break;
            }

            // Send reply if handler is available
            e.ReplyHandler?.Invoke(new Dictionary<string, string>
            {
                { "status", "success" },
                { "timestamp", DateTime.UtcNow.ToString("o") }
            });
        }
    }

    private void StartWorkout() { /* Implementation */ }
    private void StopWorkout() { /* Implementation */ }
}
```

### Example 3: Request-Reply Pattern

```csharp
private async Task<string> GetWeatherFromPhone()
{
    try
    {
        var request = new Dictionary<string, string>
        {
            { "action", "get_weather" },
            { "location", "current" }
        };

        var reply = await _wearable.SendMessageWithReplyAsync(
            request,
            timeout: TimeSpan.FromSeconds(10)
        );

        return reply.TryGetValue("temperature", out var temp) 
            ? temp 
            : "Unknown";
    }
    catch (TimeoutException)
    {
        return "Timeout";
    }
}
```

### Example 4: Sync Application Context

```csharp
public class DataSyncService
{
    private readonly IWearableMessaging _wearable;

    public DataSyncService(IWearableMessaging wearableMessaging)
    {
        _wearable = wearableMessaging;
        _wearable.ApplicationContextChanged += OnContextChanged;
    }

    public async Task UpdateUserSettings(UserSettings settings)
    {
        var context = new Dictionary<string, object>
        {
            { "theme", settings.Theme },
            { "units", settings.Units },
            { "lastSync", DateTime.UtcNow.ToString("o") }
        };

        await _wearable.UpdateApplicationContextAsync(context);
    }

    private void OnContextChanged(object sender, ApplicationContextChangedEventArgs e)
    {
        // Handle context updates from wearable
        if (e.Context.TryGetValue("heart_rate", out var hr))
        {
            UpdateHeartRate(Convert.ToInt32(hr));
        }
    }

    private void UpdateHeartRate(int heartRate) { /* Implementation */ }
}
```

### Example 5: Monitor Connection State

```csharp
public class WearableStatusViewModel : ObservableObject
{
    private readonly IWearableMessaging _wearable;
    private bool _isConnected;

    public bool IsConnected
    {
        get => _isConnected;
        set => SetProperty(ref _isConnected, value);
    }

    public WearableStatusViewModel(IWearableMessaging wearableMessaging)
    {
        _wearable = wearableMessaging;
        _wearable.WearableStateChanged += OnStateChanged;
        
        // Check initial state
        _ = CheckConnectionAsync();
    }

    private async Task CheckConnectionAsync()
    {
        IsConnected = await _wearable.IsWearableReachable();
    }

    private void OnStateChanged(object sender, WearableStateChangedEventArgs e)
    {
        IsConnected = e.IsReachable;
        
        if (e.IsAppInstalled && !e.IsReachable)
        {
            // App is installed but watch is not reachable
            ShowNotification("Watch disconnected");
        }
    }

    private void ShowNotification(string message) { /* Implementation */ }
}
```

## Creating Native Wearable Apps

### Apple Watch (SwiftUI)

Create a new watchOS target in Xcode and add this to your watch app:

```swift
import SwiftUI
import WatchConnectivity

class WatchViewModel: NSObject, ObservableObject, WCSessionDelegate {
    @Published var message: String = ""
    
    override init() {
        super.init()
        if WCSession.isSupported() {
            let session = WCSession.default
            session.delegate = self
            session.activate()
        }
    }
    
    func sendMessage() {
        let session = WCSession.default
        let message = ["action": "button_pressed"]
        session.sendMessage(message, replyHandler: { reply in
            print("Reply: \(reply)")
        }, errorHandler: { error in
            print("Error: \(error)")
        })
    }
    
    // WCSessionDelegate methods
    func session(_ session: WCSession, 
                 activationDidCompleteWith activationState: WCSessionActivationState, 
                 error: Error?) {
        print("Session activated: \(activationState.rawValue)")
    }
    
    func session(_ session: WCSession, 
                 didReceiveMessage message: [String : Any]) {
        DispatchQueue.main.async {
            if let action = message["action"] as? String {
                self.message = action
            }
        }
    }
}

struct ContentView: View {
    @StateObject private var viewModel = WatchViewModel()
    
    var body: some View {
        VStack {
            Text("Message: \(viewModel.message)")
            Button("Send to Phone") {
                viewModel.sendMessage()
            }
        }
    }
}
```

### Wear OS (Kotlin)

Create a Wear OS module and add this to your wear app:

```kotlin
import android.content.Context
import com.google.android.gms.wearable.*
import org.json.JSONObject

class WearViewModel(private val context: Context) {
    private val messageClient: MessageClient = Wearable.getMessageClient(context)
    private val dataClient: DataClient = Wearable.getDataClient(context)
    
    init {
        messageClient.addListener { messageEvent ->
            val json = String(messageEvent.data)
            val data = JSONObject(json)
            val action = data.optString("action")
            handleMessage(action)
        }
    }
    
    fun sendMessage(action: String) {
        val nodeClient = Wearable.getNodeClient(context)
        nodeClient.connectedNodes.addOnSuccessListener { nodes ->
            nodes.forEach { node ->
                val data = JSONObject()
                data.put("action", action)
                val bytes = data.toString().toByteArray()
                
                messageClient.sendMessage(node.id, "/message", bytes)
            }
        }
    }
    
    private fun handleMessage(action: String) {
        // Handle incoming messages
        when (action) {
            "start_workout" -> startWorkout()
            "stop_workout" -> stopWorkout()
        }
    }
    
    private fun startWorkout() { /* Implementation */ }
    private fun stopWorkout() { /* Implementation */ }
}
```

## Best Practices

1. **Always check connectivity** before sending messages
2. **Handle timeouts** gracefully for request-reply patterns
3. **Keep messages small** - there are payload size limits
4. **Use application context** for data that needs to persist
5. **Batch updates** when possible to reduce battery usage
6. **Test on real devices** - simulators/emulators have limitations

## Troubleshooting

### iOS
- Ensure both apps are signed with the same team
- Check that WatchConnectivity framework is linked
- Verify the watch app is properly bundled

### Android
- Ensure Google Play Services is installed and updated
- Check that wearable capability is declared
- Verify both apps use the same package name

## Additional Resources

- [Apple WatchConnectivity Documentation](https://developer.apple.com/documentation/watchconnectivity)
- [Android Wearable Data Layer API](https://developer.android.com/training/wearables/data-layer)
- [Plugin GitHub Repository](https://github.com/yourusername/Plugin.Maui.WearableMessaging)
