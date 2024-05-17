using System;
using Microsoft.Win32.SafeHandles;

namespace GalaxyBudsClient.Platform.Windows.Devices
{
    sealed class RegisterDeviceNotificationSafeHandle : SafeHandleZeroOrMinusOneIsInvalid
    {
        public RegisterDeviceNotificationSafeHandle()
            : base(true) // ownsHandle
        {
        }

        protected override bool ReleaseHandle()
        {
            return btUnregister(this.handle);
        }

        internal static bool btUnregister(IntPtr hDevNotification)
        {
            System.Diagnostics.Debug.Assert(hDevNotification != IntPtr.Zero, "btUnregister, not registered.");
            bool success = UnmanagedDevice.UnregisterDeviceNotification(hDevNotification);
            hDevNotification = IntPtr.Zero;
            System.Diagnostics.Debug.Assert(success, "UnregisterDeviceNotification success false.");
            return success;
        }
    }

}
