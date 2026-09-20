using System;
using System.Runtime.InteropServices;
using System.Text;
using Serilog;

namespace GalaxyBudsClient.Platform.SpatialAudio;

/// <summary>
/// CoreAudio HAL helper to query audio devices, names, and UIDs on macOS.
/// </summary>
public static unsafe class CoreAudioDeviceHelper
{
    private const string CoreAudioLib = "/System/Library/Frameworks/CoreAudio.framework/CoreAudio";
    private const string CoreFoundationLib = "/System/Library/Frameworks/CoreFoundation.framework/CoreFoundation";

    [StructLayout(LayoutKind.Sequential)]
    private struct AudioObjectPropertyAddress
    {
        public uint mSelector;
        public uint mScope;
        public uint mElement;
    }

    private const uint kAudioObjectSystemObject = 1;
    private const uint kAudioObjectPropertyScopeGlobal = 0x676c6f62; // 'glob'
    private const uint kAudioObjectPropertyScopeOutput = 0x6f757470; // 'outp'
    private const uint kAudioObjectPropertyScopeInput = 0x696e7074;  // 'inpt'
    private const uint kAudioObjectPropertyElementMain = 0;

    private const uint kAudioHardwarePropertyDevices = 0x64657623;   // 'dev#'
    private const uint kAudioObjectPropertyName = 0x6c6e616d;       // 'lnam'
    private const uint kAudioDevicePropertyDeviceUID = 0x75696420;   // 'uid '
    private const uint kAudioDevicePropertyStreams = 0x73746d23;     // 'stm#'

    private const uint kCFStringEncodingUTF8 = 0x08000100;

    [DllImport(CoreAudioLib)]
    private static extern int AudioObjectGetPropertyDataSize(
        uint inObjectID,
        AudioObjectPropertyAddress* inAddress,
        uint inQualifierDataSize,
        void* inQualifierData,
        uint* outDataSize);

    [DllImport(CoreAudioLib)]
    private static extern int AudioObjectGetPropertyData(
        uint inObjectID,
        AudioObjectPropertyAddress* inAddress,
        uint inQualifierDataSize,
        void* inQualifierData,
        uint* ioDataSize,
        void* outData);

    [DllImport(CoreFoundationLib)]
    private static extern bool CFStringGetCString(
        IntPtr theString,
        byte* buffer,
        long bufferSize,
        uint encoding);

    [DllImport(CoreFoundationLib)]
    private static extern void CFRelease(IntPtr cf);

    /// <summary>
    /// Searches CoreAudio for an output audio device matching the given name pattern (e.g. "Galaxy Buds" or "Buds").
    /// Returns its unique hardware UID (e.g. "40-35-E6-2C-81-0B:output"), or null if not found.
    /// </summary>
    public static string? FindOutputDeviceUid(string nameContains = "Buds")
    {
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            return null;

        try
        {
            var addr = new AudioObjectPropertyAddress
            {
                mSelector = kAudioHardwarePropertyDevices,
                mScope = kAudioObjectPropertyScopeGlobal,
                mElement = kAudioObjectPropertyElementMain
            };

            uint dataSize = 0;
            var status = AudioObjectGetPropertyDataSize(kAudioObjectSystemObject, &addr, 0, null, &dataSize);
            if (status != 0 || dataSize == 0) return null;

            var deviceCount = (int)(dataSize / sizeof(uint));
            var devices = stackalloc uint[deviceCount];
            status = AudioObjectGetPropertyData(kAudioObjectSystemObject, &addr, 0, null, &dataSize, devices);
            if (status != 0) return null;

            for (var i = 0; i < deviceCount; i++)
            {
                var devId = devices[i];

                // Check if device has output streams
                var streamAddr = new AudioObjectPropertyAddress
                {
                    mSelector = kAudioDevicePropertyStreams,
                    mScope = kAudioObjectPropertyScopeOutput,
                    mElement = kAudioObjectPropertyElementMain
                };
                uint streamSize = 0;
                AudioObjectGetPropertyDataSize(devId, &streamAddr, 0, null, &streamSize);
                if (streamSize == 0) continue; // No output channels

                // Get device name
                var name = GetStringProperty(devId, kAudioObjectPropertyName);
                if (name != null && name.Contains(nameContains, StringComparison.OrdinalIgnoreCase))
                {
                    var uid = GetStringProperty(devId, kAudioDevicePropertyDeviceUID);
                    if (!string.IsNullOrEmpty(uid))
                    {
                        Log.Information("CoreAudioDeviceHelper: Found target audio output device '{Name}' with UID '{UID}'", name, uid);
                        return uid;
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Log.Warning(ex, "CoreAudioDeviceHelper: Error enumerating CoreAudio output devices");
        }

        return null;
    }

    private static string? GetStringProperty(uint objectId, uint selector)
    {
        var addr = new AudioObjectPropertyAddress
        {
            mSelector = selector,
            mScope = kAudioObjectPropertyScopeGlobal,
            mElement = kAudioObjectPropertyElementMain
        };

        IntPtr cfStr = IntPtr.Zero;
        uint size = (uint)sizeof(IntPtr);
        var status = AudioObjectGetPropertyData(objectId, &addr, 0, null, &size, &cfStr);
        if (status != 0 || cfStr == IntPtr.Zero) return null;

        try
        {
            const int maxLen = 256;
            var buffer = stackalloc byte[maxLen];
            if (CFStringGetCString(cfStr, buffer, maxLen, kCFStringEncodingUTF8))
            {
                return Marshal.PtrToStringUTF8((IntPtr)buffer);
            }
            return null;
        }
        finally
        {
            CFRelease(cfStr);
        }
    }
}
