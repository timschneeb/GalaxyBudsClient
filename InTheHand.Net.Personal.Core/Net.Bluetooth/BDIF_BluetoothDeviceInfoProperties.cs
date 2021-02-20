using System;
using System.Collections.Generic;
using System.Text;

namespace InTheHand.Net.Bluetooth
{
    /// <summary>
    /// Specifies properties of a remote Bluetooth Device.
    /// </summary>
    /// -
    /// -
    /// <remarks>
    /// <para>Supported only by the Microsoft stack on desktop Windows.
    /// </para>
    /// <para>Originally from Win32 "bthdef.h" and used by struct
    /// BTH_DEVICE_INFO.flags. The flags are named BDIF_**.
    /// </para>
    /// </remarks>
    [Flags]
    public enum BluetoothDeviceInfoProperties // Really: uint
    {
        /// <summary>
        /// The address member contains valid data.
        /// </summary>
        Address = (0x00000001),
        /// <summary>
        /// The classOfDevice member contains valid data.
        /// </summary>
        Cod = (0x00000002),
        /// <summary>
        /// The name member contains valid data.
        /// </summary>
        Name = (0x00000004),
        /// <summary>
        /// The device is a remembered and authenticated device.
        /// The BDIF_PERSONAL flag is always set when this flag is set.
        /// </summary>
        Paired = (0x00000008),
        /// <summary>
        /// The device is a remembered device. If this flag is set and
        /// the BDIF_PAIRED flag is not set, the device is not authenticated.
        /// </summary>
        Personal = (0x00000010),
        /// <summary>
        /// The remote Bluetooth device is currently connected to the local radio.
        /// </summary>
        Connected = (0x00000020),

        //
        // Support added in KB942567
        // #if (NTDDI_VERSION > NTDDI_VISTASP1 || \
        //    (NTDDI_VERSION == NTDDI_VISTASP1 && defined(VISTA_KB942567)))
        //
#pragma warning disable 1591 // Missing XmlDocs
        /// <summary>
        /// [Vista SP1]
        /// </summary>
        ShortName = 0x00000040,
        Visible = 0x00000080,
        SspSupported = 0x00000100,
        SspPaired = 0x00000200,
        SspMitmProtected = 0x00000400,
        Rssi = 0x00001000,
        /// <summary>
        /// 
        /// </summary>
        Eir = 0x00002000,

        //
        // Windows 8
        // #if  (NTDDI_VERSION >= NTDDI_WIN8, // >= WIN8
        //
        /// <summary>
        /// Bluetooth Basic Rate &#x2014; i.e. traditional Bluetooth [Windows 8]
        /// </summary>
        BR = 0x00004000,
        /// <summary>
        /// Bluetooth Low Energy
        /// </summary>
        LE = 0x00008000,
        LEPaired = 0x00010000,
        LEPersonal = 0x00020000,
        LEMitmProtected = 0x00040000,
    }


    static class BDIFMasks
    {
        public const BluetoothDeviceInfoProperties AllOrig
            = BluetoothDeviceInfoProperties.Address | BluetoothDeviceInfoProperties.Cod
            | BluetoothDeviceInfoProperties.Name | BluetoothDeviceInfoProperties.Paired
            | BluetoothDeviceInfoProperties.Personal | BluetoothDeviceInfoProperties.Connected;
        public const BluetoothDeviceInfoProperties AllKb942567
            = AllOrig | BluetoothDeviceInfoProperties.ShortName
            | BluetoothDeviceInfoProperties.Visible | BluetoothDeviceInfoProperties.SspSupported
            | BluetoothDeviceInfoProperties.SspPaired | BluetoothDeviceInfoProperties.SspMitmProtected
            | BluetoothDeviceInfoProperties.Rssi | BluetoothDeviceInfoProperties.Eir;
        public const BluetoothDeviceInfoProperties AllWindows8
            = AllKb942567 | BluetoothDeviceInfoProperties.BR
            | BluetoothDeviceInfoProperties.LE | BluetoothDeviceInfoProperties.LEPaired
            | BluetoothDeviceInfoProperties.LEPersonal | BluetoothDeviceInfoProperties.LEMitmProtected;
    }
}