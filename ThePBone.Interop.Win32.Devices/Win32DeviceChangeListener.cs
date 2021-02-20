using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices;
using InTheHand.Net.Bluetooth;
using InTheHand.Net.Bluetooth.Msft;
using Serilog;
using ThePBone.Interop.Win32.Devices.Bluetooth;
using ThePBone.Interop.Win32.Devices.Utils;
using static ThePBone.Interop.Win32.Devices.UnmanagedDevice;

namespace ThePBone.Interop.Win32.Devices
{
    // ReSharper disable InconsistentNaming
#pragma warning disable 169, 649

    public class Win32DeviceChangeListener : IDisposable
    {
        private static readonly object Padlock = new object();
        private static Win32DeviceChangeListener? _instance;
        public static Win32DeviceChangeListener Instance
        {
            get
            {
                lock (Padlock)
                {
                    return _instance ??= new Win32DeviceChangeListener();
                }
            }
        }

        public static void Init()
        {
            lock (Padlock)
            {
                _instance ??= new Win32DeviceChangeListener();
            }
        }


        private readonly WndProcClient _wndProc = new WndProcClient();

        public event EventHandler<BluetoothWin32RadioInRangeEventArgs>? DeviceInRange;
        public event EventHandler<BluetoothWin32RadioOutOfRangeEventArgs>? DeviceOutOfRange;

        private static readonly Int32 _OffsetOfData;
        private RegisterDeviceNotificationSafeHandle? _hDevNotification;
        static Win32DeviceChangeListener()
        {
            _OffsetOfData = Marshal.OffsetOf(typeof(DEV_BROADCAST_HANDLE__withData),
                "dbch_data__0").ToInt32();
            if (IntPtr.Size == 4)
            {
                Debug.Assert(40 == _OffsetOfData);
            }
            else
            {
                Debug.Assert(52 == _OffsetOfData);
            }
        }

        public Win32DeviceChangeListener()
        {
            _wndProc.MessageReceived += WndProcClient_MessageReceived;

            BluetoothRadio? microsoftWin32BluetoothRadio = null;
            foreach (var cur in BluetoothRadio.AllRadios)
            {
                if (cur.SoftwareManufacturer == Manufacturer.Microsoft)
                {
                    microsoftWin32BluetoothRadio = cur;
                    break;
                }
            }
            
            if (microsoftWin32BluetoothRadio == null)
            {
                throw new InvalidOperationException("There is no Radio using the Microsoft Bluetooth stack.");
            }
            else
            {
                IntPtr bluetoothRadioHandle = microsoftWin32BluetoothRadio.Handle;
                if (bluetoothRadioHandle == IntPtr.Zero)
                    throw new ArgumentException("The bluetoothRadioHandle may not be NULL.");

                btRegister(bluetoothRadioHandle);
            }
        }

        public Win32DeviceChangeListener(BluetoothRadio microsoftWin32BluetoothRadio)
        {
            if (microsoftWin32BluetoothRadio == null) throw new ArgumentNullException("microsoftWin32BluetoothRadio");
            if (microsoftWin32BluetoothRadio.SoftwareManufacturer != Manufacturer.Microsoft)
                throw new ArgumentException("The specified Radio does not use the Microsoft Bluetooth stack.");
            IntPtr bluetoothRadioHandle = microsoftWin32BluetoothRadio.Handle;
            if (bluetoothRadioHandle == IntPtr.Zero)
                throw new ArgumentException("The bluetoothRadioHandle may not be NULL.");
            
            btRegister(bluetoothRadioHandle);
        }

        internal void btRegister(IntPtr bluetoothRadioHandle)
        {
            Debug.Assert(_hDevNotification == null, "btRegister, already set.");
            Debug.Assert(_hDevNotification == null || _hDevNotification.IsInvalid, "btRegister, already registered.");
            IntPtr windowHandle = _wndProc.WindowHandle;
            DEV_BROADCAST_HANDLE devHandle = new DEV_BROADCAST_HANDLE(bluetoothRadioHandle);
            RegisterDeviceNotificationSafeHandle hDevNotification
                = RegisterDeviceNotification_SafeHandle(windowHandle,
                    ref devHandle, RegisterDeviceNotificationFlags.DEVICE_NOTIFY_WINDOW_HANDLE);
            if (hDevNotification.IsInvalid)
            {
                throw new Win32Exception(Marshal.GetLastWin32Error());
            }
            _hDevNotification = hDevNotification;
        }


        private void WndProcClient_MessageReceived(object? sender, WndProcClient.WindowMessage e)
        {
            Console.WriteLine(e);
            switch (e.Msg)
            {
                case WndProcClient.WindowsMessage.WM_DEVICECHANGE:
                    Dbt subId = (Dbt) e.wParam.ToInt64();
                    if (subId == Dbt.CustomEvent
                            || subId == Dbt.DeviceArrival
                            || subId == Dbt.DeviceQueryRemove
                            || subId == Dbt.DeviceQueryRemoveFailed
                            || subId == Dbt.DeviceRemoveComplete
                            || subId == Dbt.DeviceRemovePending
                            || subId == Dbt.DeviceTypeSpecific)
                    {
                        DoBroadcastHdr(e);
                    }
                    break;
            }
        }

        private void DoBroadcastHdr(WndProcClient.WindowMessage m)
        {
            //IntPtr pXXX;
            String text = String.Empty;
            DEV_BROADCAST_HDR hdr = (DEV_BROADCAST_HDR) Marshal.PtrToStructure(m.lParam, typeof(DEV_BROADCAST_HDR));
            if (hdr.dbch_devicetype == DbtDevTyp.Port)
            {
                DoDevTypPort(ref m, ref text, ref hdr);
            }
            else if (hdr.dbch_devicetype == DbtDevTyp.Handle)
            {
                DoDevTypHandle(ref m, ref text);
            }
        }

        private void DoDevTypHandle(ref WndProcClient.WindowMessage m, ref String text)
        {
            DEV_BROADCAST_HANDLE hdrHandle = (DEV_BROADCAST_HANDLE) Marshal.PtrToStructure(m.lParam, typeof(DEV_BROADCAST_HANDLE));
            var pData = PointerUtils.Add(m.lParam, _OffsetOfData);
            if (BluetoothDeviceNotificationEvent.BthPortDeviceInterface == hdrHandle.dbch_eventguid)
            {
                text += "GUID_BTHPORT_DEVICE_INTERFACE";
            }
            else if (BluetoothDeviceNotificationEvent.RadioInRange == hdrHandle.dbch_eventguid)
            {
                text += "GUID_BLUETOOTH_RADIO_IN_RANGE";
                BTH_RADIO_IN_RANGE inRange
                    = (BTH_RADIO_IN_RANGE)Marshal.PtrToStructure(pData, typeof(BTH_RADIO_IN_RANGE));
                text += String.Format(" 0x{0:X12}", inRange.deviceInfo.address);
                text += String.Format(" is ({0}) 0x{0:X}", inRange.deviceInfo.flags);
                text += String.Format(" was ({0}) 0x{0:X}", inRange.previousDeviceFlags);
                var bdi0 = BLUETOOTH_DEVICE_INFO.Create(inRange.deviceInfo);
                var e = BluetoothWin32RadioInRangeEventArgs.Create(
                        inRange.previousDeviceFlags,
                        inRange.deviceInfo.flags, bdi0);
                DeviceInRange?.Invoke(this, e);

            }
            else if (BluetoothDeviceNotificationEvent.RadioOutOfRange == hdrHandle.dbch_eventguid)
            {
                BTH_RADIO_OUT_OF_RANGE outOfRange
                    = (BTH_RADIO_OUT_OF_RANGE)Marshal.PtrToStructure(pData, typeof(BTH_RADIO_OUT_OF_RANGE));
                text += "GUID_BLUETOOTH_RADIO_OUT_OF_RANGE";
                text += String.Format(System.Globalization.CultureInfo.InvariantCulture,
                    " 0x{0:X12}", outOfRange.deviceAddress);
                var e = BluetoothWin32RadioOutOfRangeEventArgs.Create(
                        outOfRange.deviceAddress);

                DeviceOutOfRange?.Invoke(this, e);
            }
            else if (BluetoothDeviceNotificationEvent.PinRequest == hdrHandle.dbch_eventguid)
            {
                text += "GUID_BLUETOOTH_PIN_REQUEST";
            }
            else if (BluetoothDeviceNotificationEvent.L2capEvent == hdrHandle.dbch_eventguid)
            {
                text += "GUID_BLUETOOTH_L2CAP_EVENT";
            }
            else if (BluetoothDeviceNotificationEvent.HciEvent == hdrHandle.dbch_eventguid)
            {
                text += "GUID_BLUETOOTH_HCI_EVENT";
            }
            else if (BluetoothDeviceNotificationEvent.AuthenticationRequestEvent == hdrHandle.dbch_eventguid)
            {
                text += "GUID_BLUETOOTH_AUTHENTICATION_REQUEST";
            }
            else if (BluetoothDeviceNotificationEvent.KeyPressEvent == hdrHandle.dbch_eventguid)
            {
                text += "GUID_BLUETOOTH_KEYPRESS_EVENT";
            }
            else if (BluetoothDeviceNotificationEvent.HciVendorEvent == hdrHandle.dbch_eventguid)
            {
                text += "GUID_BLUETOOTH_HCI_VENDOR_EVENT";
            }
            else
            {
                text += "Unknown event: " + hdrHandle.dbch_eventguid;
            }
            
            Log.Verbose("Interop.Win32: Device changed: " + text);
        }

        private static void DoDevTypPort(ref WndProcClient.WindowMessage m, ref String text, ref DEV_BROADCAST_HDR hdr)
        {
            text += "Port: ";
            //DEV_BROADCAST_PORT hdrPort = (DEV_BROADCAST_PORT)m.GetLParam(typeof(DEV_BROADCAST_PORT));
            const int OffsetOfStringMember = 12; // ints not pointers, so fixed size.
            System.Diagnostics.Debug.Assert(OffsetOfStringMember
                == Marshal.OffsetOf(typeof(DEV_BROADCAST_PORT), "____dbcp_name").ToInt64());
            int cbSpaceForString = hdr.dbch_size - OffsetOfStringMember;
            string? str;
            if (cbSpaceForString > 0)
            {
                Int64 startPtrXX = OffsetOfStringMember + m.lParam.ToInt64();
                IntPtr startPtr = (IntPtr)startPtrXX;
                // We won't use the length parameter in method PtrToStringUni as the
                // string we have here is null-terminated and often has trailing nulls
                // also, using the length overload would force their inclusion.
                str = Marshal.PtrToStringUni(startPtr);
            }
            else
            {
                str = null;
            }
            text += str;
        }

        public void Dispose()
        {
            try
            {
                if (_hDevNotification != null && !_hDevNotification.IsClosed)
                {
                    _hDevNotification.Close();
                }
            }
            finally{}
        }
    }
}
