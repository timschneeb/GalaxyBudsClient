using System;
using System.Runtime.InteropServices;
using Serilog;

namespace ThePBone.Interop.Win32
{
    public static class AudioPlaybackDetection
    {
        public static bool IsWindowsPlayingSound()
        {
            try
            {
                // ReSharper disable once SuspiciousTypeConversion.Global
                var enumerator = (IMMDeviceEnumerator?)(new MMDeviceEnumerator());

                if (enumerator == null)
                {
                    Log.Error("IsWindowsPlayingSound: MMDeviceEnumerator is null");
                    return true;
                }
                
                var speakers = enumerator.GetDefaultAudioEndpoint(EDataFlow.eRender, ERole.eMultimedia);
                var meter = speakers.Activate(typeof(IAudioMeterInformation).GUID, 0, IntPtr.Zero) as IAudioMeterInformation;
                var value = meter?.GetPeakValue();

                // this is a bit tricky. 0 is the official "no sound" value
                // but for example, if you open a video and plays/stops with it (w/o killing the app/window/stream),
                // the value will not be zero, but something really small (around 1E-09)
                // so, depending on your context, it is up to you to decide
                // if you want to test for 0 or for a small value
                return (value ?? 1) > 1E-08;
            }
            catch (Exception e)
            {
                Log.Error($"IsWindowsPlayingSound: {e.Message}");
                return true;
            }
        }

        [ComImport, Guid("BCDE0395-E52F-467C-8E3D-C4579291692E")]
        private class MMDeviceEnumerator
        {
        }

        private enum EDataFlow
        {
            eRender,
            eCapture,
            eAll,
        }

        private enum ERole
        {
            eConsole,
            eMultimedia,
            eCommunications,
        }

        [InterfaceType(ComInterfaceType.InterfaceIsIUnknown), Guid("A95664D2-9614-4F35-A746-DE8DB63617E6")]
        private interface IMMDeviceEnumerator
        {
            void NotNeeded();
            IMMDevice GetDefaultAudioEndpoint(EDataFlow dataFlow, ERole role);
            // the rest is not defined/needed
        }

        [InterfaceType(ComInterfaceType.InterfaceIsIUnknown), Guid("D666063F-1587-4E43-81F1-B948E807363F")]
        private interface IMMDevice
        {
            [return: MarshalAs(UnmanagedType.IUnknown)]
            object Activate([MarshalAs(UnmanagedType.LPStruct)] Guid iid, int dwClsCtx, IntPtr pActivationParams);
            // the rest is not defined/needed
        }

        [InterfaceType(ComInterfaceType.InterfaceIsIUnknown), Guid("C02216F6-8C67-4B5B-9D00-D008E73E0064")]
        private interface IAudioMeterInformation
        {
            float GetPeakValue();
            // the rest is not defined/needed
        }

    }
}
