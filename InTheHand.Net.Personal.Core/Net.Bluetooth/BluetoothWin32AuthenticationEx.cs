// 32feet.NET - Personal Area Networking for .NET
//
// InTheHand.Net.Bluetooth.BLUETOOTH_AUTHENTICATE_RESPONSE
// 
// Copyright (c) 2003-2010 In The Hand Ltd, All rights reserved.
// This source code is licensed under the MIT License

#if WinXP
using System;
using System.Runtime.InteropServices;
using InTheHand.Net.Sockets;
using InTheHand.Net.Bluetooth.Msft;

namespace InTheHand.Net.Bluetooth
{
    /// <summary>
    /// The BLUETOOTH_AUTHENTICATION_CALLBACK_PARAMS structure contains specific configuration information about the Bluetooth device responding to an authentication request.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    internal struct BLUETOOTH_AUTHENTICATION_CALLBACK_PARAMS
    {
        /// <summary>
        /// A BLUETOOTH_DEVICE_INFO structure that contains information about a Bluetooth device.
        /// </summary>
        internal BLUETOOTH_DEVICE_INFO deviceInfo;
        /// <summary>
        /// A BLUETOOTH_AUTHENTICATION_METHOD enumeration that defines the authentication method utilized by the Bluetooth device.
        /// </summary>
        internal BluetoothAuthenticationMethod authenticationMethod;
        /// <summary>
        /// A BLUETOOTH_IO_CAPABILITY enumeration that defines the input/output capabilities of the Bluetooth device.
        /// </summary>
        internal BluetoothIoCapability ioCapability;
        /// <summary>
        /// A AUTHENTICATION_REQUIREMENTS specifies the 'Man in the Middle' protection required for authentication.
        /// </summary>
        internal BluetoothAuthenticationRequirements authenticationRequirements;

        //union{
        //    ULONG   Numeric_Value;
        //    ULONG   Passkey;
        //};
        /// <summary>
        /// A ULONG value used for Numeric Comparison authentication.
        /// or
        /// A ULONG value used as the passkey used for authentication.
        /// </summary>
        internal UInt32 Numeric_Value_Passkey;

        private void ShutupCompiler()
        {
            deviceInfo = new BLUETOOTH_DEVICE_INFO();
            authenticationMethod = BluetoothAuthenticationMethod.Legacy;
            ioCapability = BluetoothIoCapability.Undefined;
            authenticationRequirements = BluetoothAuthenticationRequirements.MITMProtectionNotDefined;
            Numeric_Value_Passkey = 0;
        }
    }

    /*[StructLayout(LayoutKind.Sequential)]
    internal struct BLUETOOTH_AUTHENTICATE_RESPONSE
    {
        internal Int64 bthAddressRemote; //BLUETOOTH_ADDRESS bthAddressRemote;
        internal BLUETOOTH_AUTHENTICATION_METHOD authMethod;
        
        //union{
        //    BLUETOOTH_PIN_INFO pinInfo;
        //    BLUETOOTH_OOB_DATA_INFO oobInfo;
        //    BLUETOOTH_NUMERIC_COMPARISON_INFO numericCompInfo;
        //    BLUETOOTH_PASSKEY_INFO passkeyInfo;
        //};
        //[MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
        //public byte[] union;

        public int negativeResponse;

        private void ShutupCompiler()
        {
            bthAddressRemote = 0;
            authMethod = BLUETOOTH_AUTHENTICATION_METHOD.Legacy;
            //union = null;
            negativeResponse = 0;
        }
    }*/

    [StructLayout(LayoutKind.Sequential)]
    internal struct BLUETOOTH_AUTHENTICATE_RESPONSE__PIN_INFO // see above
    {
        internal Int64 bthAddressRemote;
        internal BluetoothAuthenticationMethod authMethod;
        internal BLUETOOTH_PIN_INFO pinInfo;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 12)]
        private byte[] _padding;
        internal byte negativeResponse;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct BLUETOOTH_AUTHENTICATE_RESPONSE__OOB_DATA_INFO // see above
    {
        internal Int64 bthAddressRemote;
        internal BluetoothAuthenticationMethod authMethod;
        internal BLUETOOTH_OOB_DATA_INFO oobInfo;
        internal byte negativeResponse;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct BLUETOOTH_AUTHENTICATE_RESPONSE__NUMERIC_COMPARISON_PASSKEY_INFO // see above
    {
        internal Int64 bthAddressRemote;
        internal BluetoothAuthenticationMethod authMethod;
        internal UInt32 numericComp_passkey;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 28)]
        private byte[] _padding;
        internal byte negativeResponse;
    }

    /// <summary>
    /// The BLUETOOTH_PIN_INFO structure contains information used for authentication via PIN.
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Size = 20)]
    internal struct BLUETOOTH_PIN_INFO
    {
        internal const int BTH_MAX_PIN_SIZE = 16;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = BTH_MAX_PIN_SIZE)]
        internal byte[] pin;
        internal Int32 pinLength;
    }

    /// <summary>
    /// The BLUETOOTH_OOB_DATA_INFO structure contains data used to authenticate prior to establishing an Out-of-Band device pairing.
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Size = 32)]
    internal struct BLUETOOTH_OOB_DATA_INFO
    {
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
        internal byte[] C;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
        internal byte[] R;

        private void ShutupCompiler()
        {
            C = R = null;
        }
    }

    // These are so simple that well use one outer struct with a UInt32 which can be used by both methods
    //
    // /// <summary>
    // /// The BLUETOOTH_NUMERIC_COMPARISON_INFO structure contains the numeric value used for authentication via numeric comparison. 
    // /// </summary>
    // [StructLayout(LayoutKind.Sequential, Size = 4)]
    // internal struct BLUETOOTH_NUMERIC_COMPARISON_INFO
    //
    // /// <summary>
    // /// The BLUETOOTH_PASSKEY_INFO structure contains the passkey used for authentication via passkey.
    // /// </summary>
    // [StructLayout(LayoutKind.Sequential, Size = 4)]
    //internal struct BLUETOOTH_PASSKEY_INFO


}
#endif
