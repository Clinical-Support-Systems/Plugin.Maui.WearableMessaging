# Sample Applications

This directory contains sample applications demonstrating how to use Plugin.Maui.WearableMessaging.

## Available Samples

### Coming Soon

Sample applications will be added demonstrating:

1. **BasicMessaging** - Simple message sending and receiving
   - iOS + Apple Watch
   - Android + Wear OS

2. **DataSync** - Application context synchronization
   - Real-time data updates
   - State management

3. **FileTransfer** - File transfer between devices
   - Image sharing
   - Document transfer

4. **WorkoutTracker** - Complete workout tracking app
   - Heart rate monitoring
   - GPS tracking
   - Real-time sync

## Structure

Each sample will include:

```
SampleName/
├── SampleName.sln
├── SampleName/                  # MAUI app
│   └── SampleName.csproj
├── SampleName.Watch/            # Apple Watch app (SwiftUI)
│   └── SampleName.Watch.xcodeproj
└── SampleName.WearOS/           # Wear OS app (Kotlin)
    └── build.gradle
```

## Running the Samples

### iOS + Apple Watch

1. Open the solution in Visual Studio for Mac
2. Build and deploy the MAUI app to an iPhone
3. Open the Watch project in Xcode
4. Build and deploy the Watch app
5. Pair your Apple Watch with the iPhone

### Android + Wear OS

1. Open the solution in Visual Studio
2. Build and deploy the MAUI app to an Android phone
3. Build and deploy the Wear OS app to your watch
4. Pair your Wear OS watch with the phone

## Requirements

- Visual Studio 2022 or later
- .NET 8.0 SDK
- MAUI workload installed

### For iOS
- macOS with Xcode 15+
- iOS device and Apple Watch
- Apple Developer account

### For Android
- Android SDK 30+
- Android phone and Wear OS watch
- Google Play Services

## Contributing Samples

Have a great sample idea? We'd love to see it! Please:
1. Follow the existing sample structure
2. Include comprehensive comments
3. Add a README specific to your sample
4. Test on both platforms
5. Submit a pull request

## Need Help?

- Check the [main documentation](../README.md)
- Review the [usage guide](../USAGE.md)
- Open an issue on GitHub
