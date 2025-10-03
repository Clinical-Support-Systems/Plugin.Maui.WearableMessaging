# Contributing to Plugin.Maui.WearableMessaging

Thank you for your interest in contributing! This document provides guidelines for contributing to the project.

## Getting Started

1. **Fork the repository** on GitHub
2. **Clone your fork** locally
3. **Create a branch** for your feature or bugfix
4. **Make your changes** with clear, descriptive commits
5. **Test thoroughly** on both iOS and Android
6. **Submit a pull request** with a clear description

## Development Setup

### Prerequisites

- Visual Studio 2022 or later (Windows/Mac)
- .NET 8.0 SDK or later
- MAUI workload installed
- For iOS development: macOS, Xcode 15+
- For Android development: Android SDK 30+

### Building the Project

```bash
git clone https://github.com/yourusername/Plugin.Maui.WearableMessaging.git
cd Plugin.Maui.WearableMessaging
dotnet restore
dotnet build
```

### Running Tests

```bash
dotnet test
```

## Coding Guidelines

### Code Style

- Use C# naming conventions
- Follow Microsoft's C# coding conventions
- Use meaningful variable and method names
- Add XML documentation comments for public APIs
- Keep methods focused and concise

### Example

```csharp
/// <summary>
/// Sends a message to the wearable device.
/// </summary>
/// <param name="message">The message to send.</param>
/// <exception cref="WearableMessagingException">Thrown when the message fails to send.</exception>
public async Task SendMessageAsync(Dictionary<string, string> message)
{
    // Implementation
}
```

### Platform-Specific Code

Use conditional compilation for platform-specific code:

```csharp
#if IOS
    // iOS-specific code
#elif ANDROID
    // Android-specific code
#endif
```

## Testing

### Manual Testing

Before submitting a PR, test your changes on:
- iOS device with paired Apple Watch
- Android device with paired Wear OS watch
- Various connection states (connected, disconnected, out of range)

### Automated Testing

- Write unit tests for new functionality
- Ensure all existing tests pass
- Aim for high code coverage

## Pull Request Process

1. **Update documentation** if you're changing functionality
2. **Update CHANGELOG.md** with your changes
3. **Ensure all tests pass**
4. **Request review** from maintainers
5. **Address feedback** promptly

### PR Title Format

Use clear, descriptive titles:
- `feat: Add support for X`
- `fix: Resolve issue with Y`
- `docs: Update usage guide`
- `refactor: Improve Z implementation`
- `test: Add tests for A`

### PR Description Template

```markdown
## Description
Brief description of what this PR does

## Type of Change
- [ ] Bug fix
- [ ] New feature
- [ ] Breaking change
- [ ] Documentation update

## Testing
How has this been tested?

## Checklist
- [ ] Code follows style guidelines
- [ ] Self-review completed
- [ ] Documentation updated
- [ ] Tests added/updated
- [ ] All tests passing
- [ ] CHANGELOG.md updated
```

## Reporting Issues

### Bug Reports

Include:
- Clear description of the issue
- Steps to reproduce
- Expected behavior
- Actual behavior
- Platform (iOS/Android)
- Device information
- Plugin version
- Code sample (if applicable)

### Feature Requests

Include:
- Clear description of the feature
- Use case / motivation
- Proposed API (if applicable)
- Willing to implement? (yes/no)

## Code of Conduct

### Our Standards

- Be respectful and inclusive
- Welcome newcomers
- Focus on constructive feedback
- Be patient with questions
- Assume good intentions

### Enforcement

Violations may result in:
1. Warning
2. Temporary ban
3. Permanent ban

Report issues to [maintainer email]

## Questions?

Feel free to:
- Open an issue for questions
- Start a discussion on GitHub
- Reach out to maintainers

## License

By contributing, you agree that your contributions will be licensed under the MIT License.

---

Thank you for contributing! 🎉
