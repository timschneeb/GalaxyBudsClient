// 32feet.NET - Personal Area Networking for .NET
//
// InTheHand.Net.Bluetooth.Msft.WindowsBluetoothSecurity
// 
// Copyright (c) 2003-2011 In The Hand Ltd, All rights reserved.
// This source code is licensed under the MIT License

using System;
using System.Runtime.InteropServices;
using Microsoft.Win32;
using InTheHand.Net.Bluetooth.Factory;
using System.Diagnostics;

namespace InTheHand.Net.Bluetooth.Msft
{
    /// <summary>
    /// Handles security between bluetooth devices.
    /// </summary>
    sealed class WindowsBluetoothSecurity : IBluetoothSecurity
    {
        internal WindowsBluetoothSecurity()
        { }

        //#if WinXP
        private System.Collections.Hashtable authenticators = new System.Collections.Hashtable();
        //#endif

        #region Set PIN
        /// <summary>
        /// This function stores the personal identification number (PIN) for the Bluetooth device.
        /// </summary>
        /// <param name="device">Address of remote device.</param>
        /// <param name="pin">Pin, alphanumeric string of between 1 and 16 ASCII characters.</param>
        /// <remarks><para>On Windows CE platforms this calls <c>BthSetPIN</c>,
        /// its MSDN remarks say:
        /// </para>
        /// <para>&#x201C;Stores the pin for the Bluetooth device identified in pba.
        /// The active connection to the device is not necessary, nor is the presence
        /// of the Bluetooth controller. The PIN is persisted in the registry until
        /// BthRevokePIN is called.
        /// </para>
        /// <para>&#x201C;While the PIN is stored, it is supplied automatically
        /// after the PIN request is issued by the authentication mechanism, so the
        /// user will not be prompted for it. Typically, for UI-based devices, you
        /// would set the PIN for the duration of authentication, and then revoke
        /// it after authentication is complete.&#x201D;
        /// </para>
        /// <para>See also 
        /// <see cref="M:InTheHand.Net.Bluetooth.BluetoothSecurity.RevokePin(InTheHand.Net.BluetoothAddress)"/>
        /// </para>
        /// </remarks>
        /// <returns>True on success, else False.</returns>
        /// <seealso cref="M:InTheHand.Net.Bluetooth.BluetoothSecurity.RevokePin(InTheHand.Net.BluetoothAddress)"/>
        public bool SetPin(BluetoothAddress device, string pin)
        {
            if (device == null)
                throw new ArgumentNullException("device");
            if (pin == null)
                throw new ArgumentNullException("pin");
#if NETCF
            if (pin.Length < 1 | pin.Length > 16)
            {
                throw new ArgumentException("Pin must be between 1 and 16 characters long.");
            }
            byte[] pinbytes = System.Text.Encoding.ASCII.GetBytes(pin);
            int len = pin.Length;

            int result = NativeMethods.BthSetPIN(device.ToByteArray(), len, pinbytes);

            if (result != 0)
            {
                int error = Marshal.GetLastWin32Error();

                return false;
            }
            return true;
#else
            //remove existing listener
            if (authenticators.ContainsKey(device)) {
                BluetoothWin32Authentication bwa = (BluetoothWin32Authentication)authenticators[device];
                authenticators.Remove(device);
                bwa.Dispose();
            }
            authenticators.Add(device, new BluetoothWin32Authentication(device, pin));
            return true;
            //else
            //{
            //    throw new PlatformNotSupportedException("Use PairRequest to pair with a device from Windows XP");
            //}
#endif

        }
        #endregion

        #region Revoke PIN
        /// <summary>
        /// This function revokes the personal identification number (PIN) for the Bluetooth device.
        /// </summary>
        /// <remarks><para>On Windows CE platforms this calls <c>BthRevokePIN</c>,
        /// its MSDN remarks say:
        /// </para>
        /// <para>&#x201C;When the PIN is revoked, it is removed from registry.
        /// The active connection to the device is not necessary, nor is the presence
        /// of the Bluetooth controller.&#x201D;
        /// </para>
        /// <para>On Windows CE platforms this removes any pending BluetoothWin32Authentication object but does not remove the PIN for an already authenticated device.
        /// Use RemoveDevice to ensure a pairing is completely removed.</para>
        /// <para>See also 
        /// <see cref="M:InTheHand.Net.Bluetooth.BluetoothSecurity.SetPin(InTheHand.Net.BluetoothAddress,System.String)"/>
        /// </para>
        /// </remarks>
        /// <param name="device">The remote device.</param>
        /// <returns>True on success, else False.</returns>
        /// <seealso cref="M:InTheHand.Net.Bluetooth.BluetoothSecurity.SetPin(InTheHand.Net.BluetoothAddress,System.String)"/>
        public bool RevokePin(BluetoothAddress device)
        {
            if (device == null)
                throw new ArgumentNullException("device");
#if NETCF
			int result = NativeMethods.BthRevokePIN(device.ToByteArray());

			if(result != 0)
			{
				int error = Marshal.GetLastWin32Error();

				return false;
			}
			return true;
#else
            if (authenticators.ContainsKey(device)) {
                BluetoothWin32Authentication bwa = (BluetoothWin32Authentication)authenticators[device];
                authenticators.Remove(device);
                bwa.Dispose();
                return true;
            }
            return false;
            //throw new PlatformNotSupportedException("Use RemoveDevice to remove a pairing with a device from Windows XP");
#endif
        }
        #endregion


        #region Pair Request
        //TODO PairRequest XmlDocs for XP and CE pre 5.0.
        /// <summary>
        /// Intiates pairing for a remote device.
        /// </summary>
        /// <param name="device">Remote device with which to pair.</param>
        /// <param name="pin">Chosen PIN code, must be between 1 and 16 ASCII characters.</param>
        /// <remarks><para>On Windows CE platforms this calls <c>BthPairRequest</c>,
        /// its MSDN remarks say:
        /// </para>
        /// <para>&#x201C;BthPairRequest passes the parameters to the <c>BthSetPIN</c>
        /// function and creates an ACL connection. Once the connection is established,
        /// it calls the <c>BthAuthenticate</c> function to authenticate the device.&#x201D;
        /// </para>
        /// <para>On Windows XP/Vista platforms this calls <c>BluetoothAuthenticateDevice</c>,
        /// if the pin argument is set to null a Wizard is displayed to accept a PIN from the user,
        /// otherwise the function executes in transparent mode.
        /// </para>
        /// <para>See also 
        /// <see cref="M:InTheHand.Net.Bluetooth.BluetoothSecurity.SetPin(InTheHand.Net.BluetoothAddress,System.String)"/>
        /// </para>
        /// </remarks>
        /// <returns>Whether the operation was successful.</returns>
        public bool PairRequest(BluetoothAddress device, string pin)
        {
            if (device == null) {
                throw new ArgumentNullException("device");
            }
            if (device.ToInt64() == 0) {
                throw new ArgumentNullException("device", "A non-blank address must be specified.");
            }
#if NETCF
            if (pin == null)
            {
                throw new ArgumentNullException("pin");
            }

            bool success = false;
            IBluetoothDeviceInfo bdi = new WindowsBluetoothDeviceInfo(device);

            if (System.Environment.OSVersion.Version.Major >= 5)
            {
                byte[] pinbytes = System.Text.Encoding.ASCII.GetBytes(pin);
                int len = pin.Length;
                int result = NativeMethods.BthPairRequest(device.ToByteArray(), len, pinbytes);
                if (result == 0)
                {
                    success = true;
                }
             }
             else
             {
                //BthPairRequest is CE 5.0 onwards so we will do it with individual steps

                //preset outgoing pin
                success = SetPin(device, pin);

                if (success)
                {
                    int hresult;
                    ushort handle = 0;

                    //connect to device
                    try
                    {
                        hresult = NativeMethods.BthCreateACLConnection(device.ToByteArray(), out handle);
                        if (hresult != 0)
                        {
                            success = false;
                        }
                        else
                        {
                            //force authentication
                            hresult = NativeMethods.BthAuthenticate(device.ToByteArray());
                            if (hresult != 0)
                            {
                                success = false;
                            }
                        }

                    }
                    finally
                    {
                        if (handle != 0)
                        {
                            //close connection
                            hresult = NativeMethods.BthCloseConnection(handle);
                        }
                    }

                }
            }

            if (success)
            {
                //setup UI pairing (registry)
                RegistryKey rkDevices = Registry.LocalMachine.CreateSubKey(NativeMethods.ceRegistryRoot + "\\Device");
                RegistryKey rkNewDevice = rkDevices.CreateSubKey(device.ToString());
                rkNewDevice.SetValue("name", bdi.DeviceName);
                rkNewDevice.SetValue("trusted", 1);
                rkNewDevice.SetValue("class", bdi.ClassOfDevice.GetHashCode());

//#if V2
                RegistryKey rkServices = rkNewDevice.CreateSubKey("Services");
                ServiceRecord[] recs = bdi.GetServiceRecords(BluetoothService.SerialPort);
                //byte[][] recs = bdi.GetServiceRecordsUnparsedWindowsRaw(BluetoothService.SerialPort);
                
                if (recs.Length > 0)
                {
                    byte[] servRecord = recs[0].SourceBytes;
                    RegistryKey rkSerial = rkServices.CreateSubKey(BluetoothService.SerialPort.ToString());
                    rkSerial.SetValue("sdprecord", servRecord);
                    rkSerial.SetValue("Name", "Serial Port");
                    rkSerial.SetValue("enabled", 1);
                    int channel = ServiceRecordHelper.GetRfcommChannelNumber(recs[0]);
                    if (channel != -1) {
                        rkSerial.SetValue("channel", 0x14b0000 + channel);
                    } else {
                        System.Diagnostics.Debug.Fail("PairRequest CE, peer SPP record missing channel.");
                    }
                    rkSerial.Close();
                }
                rkServices.Close();
//#endif

                rkNewDevice.Close();
                rkDevices.Close();
            }

            return success;
#else
            //use other constructor to ensure struct size is set
            BLUETOOTH_DEVICE_INFO bdi = new BLUETOOTH_DEVICE_INFO(device.ToInt64());

            //string length, but allow for null pins for UI
            int length = 0;
            if (pin != null) {
                length = pin.Length;
            }
            int result = NativeMethods.BluetoothAuthenticateDevice(IntPtr.Zero, IntPtr.Zero, ref bdi, pin, length);

            if (result != 0) {
                //determine error cause from "result"...
                // ERROR_INVALID_PARAMETER      87
                // WAIT_TIMEOUT                258
                // ERROR_NOT_AUTHENTICATED    1244
                Debug.WriteLine("PairRequest/BAD failed with: " + result);
                return false;
            }
            return true;
#endif
        }

        /// <summary>
        /// Intiates pairing for a remote device
        /// with SSP if it is available.
        /// </summary>
        /// -
        /// <param name="device">Remote device with which to pair.</param>
        /// <param name="authenticationRequirement">
        /// Note: not supported by all platforms.
        /// </param>
        /// -
        /// <returns>Whether the operation was successful.</returns>
        bool /*IBluetoothSecurity.*/PairRequest(BluetoothAddress device,
            BluetoothAuthenticationRequirements authenticationRequirement)
        {
#if NETCF
            return ((IBluetoothSecurity)this).PairRequest(device, null);
#else
            if (device == null) {
                throw new ArgumentNullException("device");
            }
            if (device.ToInt64() == 0) {
                throw new ArgumentNullException("device", "A non-blank address must be specified.");
            }

            //use other constructor to ensure struct size is set
            BLUETOOTH_DEVICE_INFO bdi = new BLUETOOTH_DEVICE_INFO(device.ToInt64());

            byte[] btOobData = null;
            int result = NativeMethods.BluetoothAuthenticateDeviceEx(IntPtr.Zero, IntPtr.Zero,
                ref bdi, btOobData, authenticationRequirement);

            if (result != 0) {
                //determine error cause from "result"...
                // ERROR_INVALID_PARAMETER      87
                // WAIT_TIMEOUT                258
                // ERROR_NOT_AUTHENTICATED    1244
                Debug.WriteLine("PairRequest/BADEx failed with: " + result);
                return false;
            }
            return true;
#endif
        }
        #endregion

        #region Remove Device
        /// <summary>
        /// Remove the pairing with the specified device
        /// </summary>
        /// -
        /// <param name="device">Remote device with which to remove pairing.</param>
        /// -
        /// <returns>TRUE if device was successfully removed, else FALSE.</returns>
        public bool RemoveDevice(BluetoothAddress device)
        {
            if (device == null)
                throw new ArgumentNullException("device");
#if NETCF
            //remove stored PIN
            RevokePin(device);

            //purge registry data
            RegistryKey rk = Registry.LocalMachine.OpenSubKey(NativeMethods.ceRegistryRoot + "Device", true);
            if (rk != null)
            {
                //delete the device key and all contents
                try {
                    rk.DeleteSubKeyTree(device.ToString());
                } catch (ArgumentException) {
                    // Apparently it doesn't exist...
                    return false;
                }
                return true;
            }

            return false;
#else
            int result = NativeMethods.BluetoothRemoveDevice(device.ToByteArray());
            if (result == 0) {
                return true;
            }
            Debug.WriteLine("RemoveDevice/BRD failed with: " + result);
            return false;
#endif
        }
        #endregion


        #region Set Link Key
        /// <summary>
        /// <para><b>Not supported on Windows XP</b></para>
        /// </summary>
        /// <param name="device"></param>
        /// <param name="linkkey"></param>
        /// <remarks><para>On Windows CE platforms this calls <c>BthSetLinkKey</c>,
        /// its MSDN remarks say:
        /// </para>
        /// <para>&#x201C;The link key is persisted in registry until <c>BthRevokeLinkKey</c>
        /// is called.
        /// </para>
        /// <para>&#x201C;Typically, the Bluetooth stack manages link keys automatically,
        /// for example, it stores them when they are created. This function is useful
        /// primarily for backup purposes.
        /// </para>
        /// <para>&#x201C;While link key is stored, it will be automatically supplied
        /// once the link key request is issued by the authentication mechanism. If
        /// the link key is incorrect, the renegotiation that involves the PIN is
        /// initiated by the Bluetooth adapter, and the PIN code may be requested
        /// from the user.
        /// </para>
        /// <para>&#x201C;The link key length is 16 bytes. You cannot create link
        /// keys; they are generated by the Bluetooth hardware.&#x201D;
        /// </para>
        /// </remarks>
        /// <returns></returns>
        public bool SetLinkKey(BluetoothAddress device, Guid linkkey)
        {
            if (device == null)
                throw new ArgumentNullException("device");
#if NETCF
			int result = NativeMethods.BthSetLinkKey(device.ToByteArray(), ref linkkey);
			if(result!=0)
			{
				return false;
			}
			return true;
#else
            throw new PlatformNotSupportedException("Not supported on Windows XP");
#endif
        }
        #endregion

        #region Get PIN Request
        /// <summary>
        /// Retrieves the address of the Bluetooth peer device authentication that requires the PIN code.
        /// <para><b>Not supported on Windows XP</b></para>
        /// </summary>
        /// <remarks><para>On Windows CE platforms this calls <c>BthGetPINRequest</c>,
        /// its MSDN remarks say:
        /// </para>
        /// <para>&#x201C;There can be multiple requests outstanding. After the event
        /// that is provided by the UI handler is signaled, the UI handler must call
        /// this function multiple times until the call fails.&#x201D;
        /// </para>
        /// <para>See also 
        /// <see cref="M:InTheHand.Net.Bluetooth.BluetoothSecurity.RefusePinRequest(InTheHand.Net.BluetoothAddress)"/>
        /// and <see cref="M:InTheHand.Net.Bluetooth.BluetoothSecurity.AnswerPinRequest(InTheHand.Net.BluetoothAddress,System.String)"/>
        /// </para>
        /// </remarks>
        /// <returns><see cref="BluetoothAddress"/> of the remote device, or null if there is no outstanding PIN request.</returns>
        public BluetoothAddress GetPinRequest()
        {
#if NETCF
			byte[] buffer = new byte[6];
			//try and retrieve the next outstanding request
			int hresult = NativeMethods.BthGetPINRequest(buffer);

			if(hresult==0)
			{
				return new BluetoothAddress(buffer);
			}

			//on failure no address to return
			return null;
#else
            throw new PlatformNotSupportedException("Not supported on Windows XP");
#endif
        }
        #endregion

        #region Refuse PIN Request
        /// <summary>
        /// Refuses an outstanding PIN request.
        /// <para><b>Not supported on Windows XP</b></para>
        /// </summary>
        /// <param name="device">Address of the requesting device.</param>
        /// <remarks><para>On Windows CE platforms this calls <c>BthRefusePINRequest</c>,
        /// its MSDN remarks say:
        /// </para>
        /// <para>&#x201C;This function refuses an outstanding PIN request that is
        /// retrieved by <see cref="M:InTheHand.Net.Bluetooth.BluetoothSecurity.GetPinRequest()"/>
        /// function.&#x201D;
        /// </para>
        /// <para>See also 
        /// <see cref="M:InTheHand.Net.Bluetooth.BluetoothSecurity.GetPinRequest"/>
        /// and <see cref="M:InTheHand.Net.Bluetooth.BluetoothSecurity.AnswerPinRequest(InTheHand.Net.BluetoothAddress,System.String)"/>
        /// </para>
        /// </remarks>
        public bool RefusePinRequest(BluetoothAddress device)
        {
            if (device == null)
                throw new ArgumentNullException("device");
#if NETCF
			int hresult = NativeMethods.BthRefusePINRequest(device.ToByteArray());
			if(hresult==0)
			{
				return true;
			}
			return false;
#else
            throw new PlatformNotSupportedException("Not supported on Windows XP");
#endif
        }
        #endregion

    }
}
