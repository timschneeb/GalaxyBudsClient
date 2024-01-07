using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using ThePBone.OSX.Native.Unmanaged;

namespace ThePBone.OSX.Native
{
    public static class SystemDialogs
    {
        public static bool SelectBluetoothDevice(Guid[] guids, out string? mac, out string? name, out bool isPaired)
        {
            unsafe
            {
                var result = new Device();
                var arrayPtr = new byte*[guids.Length];

                for (var i = 0; i < guids.Length; i++)
                {
                    var guid = guids[i];
                    fixed (byte* rawGuid = guid.ToByteArray())
                    {
                        arrayPtr[i] = rawGuid;
                    }
                }

                UI_BTSEL_RESULT code;
                fixed (byte** uuids = arrayPtr)
                {
                    code = Unmanaged.SystemDialogs.ui_select_bt_device(uuids, (byte)guids.Length, ref result);
                }

                if (code != UI_BTSEL_RESULT.UI_BTSEL_SUCCESS)
                {
                    mac = null;
                    name = null;
                    isPaired = false;
                    return false;
                }
                
                mac = Marshal.PtrToStringAnsi(result.mac_address);
                name = Marshal.PtrToStringAnsi(result.device_name);
                isPaired = result.is_paired;
                Memory.btdev_free(ref result);
                return true;
            }

        } 
    }
}