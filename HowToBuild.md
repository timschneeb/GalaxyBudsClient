# How to Build GalaxyBudsClient (All Platforms)

This guide explains how to build GalaxyBudsClient on different platforms. Currently, only macOS instructions are complete. Windows and Linux instructions are marked as TODO.

## macOS

### Prerequisites
- macOS (latest recommended)
- Xcode installed
- .NET SDK 8 or newer (recommended install via Homebrew: `brew install --cask dotnet-sdk`)

### Steps
1. **Build the native library**
   ```sh
   cd GalaxyBudsClient.Platform.OSX/Native
   ./build.sh
   ```
   This will generate `libNativeInterop.dylib` in `GalaxyBudsClient.Platform.OSX/Native/Build/Release/`.

2. **Restore .NET dependencies**
   ```sh
   dotnet restore
   ```

3. **Build the main project**
   ```sh
   dotnet build GalaxyBudsClient/GalaxyBudsClient.csproj
   ```

4. **Run the application**
   ```sh
   dotnet run --project GalaxyBudsClient/GalaxyBudsClient.csproj
   ```

If you encounter issues, ensure `libNativeInterop.dylib` is in the correct folder and Xcode is properly installed.

---

## Windows (TODO)
- [ ] Add build instructions for Windows

## Linux (TODO)
- [ ] Add build instructions for Linux

---

For questions or contributions, open an issue or pull request on GitHub.
