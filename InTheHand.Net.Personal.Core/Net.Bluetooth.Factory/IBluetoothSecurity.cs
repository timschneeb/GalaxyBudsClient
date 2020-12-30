// 32feet.NET - Personal Area Networking for .NET
//
// InTheHand.Net.Bluetooth.Factory.IBluetoothSecurity
// 
// Copyright (c) 2003-2010 In The Hand Ltd, All rights reserved.
// This source code is licensed under the MIT License

using System;
using System.Diagnostics.CodeAnalysis;

namespace InTheHand.Net.Bluetooth.Factory
{
    /// <summary>
    /// Handles security between bluetooth devices.
    /// </summary>
    /// -
    /// <remarks>
    /// <para>Used by <see cref="P:InTheHand.Net.Bluetooth.BluetoothPublicFactory.BluetoothSecurity"/>.
    /// </para>
    /// </remarks>
    public interface IBluetoothSecurity
    {
        #region Pair/Un-pair
        /// <summary>
        /// Intiates pairing for a remote device.
        /// </summary>
        /// -
        /// <param name="device">Remote device with which to pair.</param>
        /// <param name="pin">Chosen PIN code, must be between 1 and 16 ASCII characters.</param>
        /// -
        /// <returns>Whether the operation was successful.</returns>
        bool PairRequest(BluetoothAddress device, string pin);

        /// <summary>
        /// Remove the pairing with the specified device
        /// </summary>
        /// -
        /// <param name="device">Remote device with which to remove pairing.</param>
        /// -
        /// <returns>TRUE if device was successfully removed, else FALSE.</returns>
        bool RemoveDevice(BluetoothAddress device);
        #endregion

        #region Save Pin/forget
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
        bool SetPin(BluetoothAddress device, string pin);

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
        bool RevokePin(BluetoothAddress device);
        #endregion

        //--------
        #region PinRequest/etc
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
        [SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate")]
        BluetoothAddress GetPinRequest();

        /// <summary>
        /// Refuses an outstanding PIN request.
        /// <para><b>Not supported on Windows XP</b></para>
        /// </summary>
        /// -
        /// <param name="device">Address of the requesting device.</param>
        /// -
        /// <returns><c>true</c> if the operation was successful; <c>false</c> otherwise.</returns>
        /// -
        /// <remarks><para>On Windows CE platforms this calls <c>BthRefusePINRequest</c>,
        /// its MSDN remarks say:
        /// </para>
        /// <para>&#x201C;This function refuses an outstanding PIN request that is
        /// retrieved by <see cref="M:InTheHand.Net.Bluetooth.BluetoothSecurity.GetPinRequest"/>
        /// function.&#x201D;
        /// </para>
        /// <para>See also 
        /// <see cref="M:InTheHand.Net.Bluetooth.BluetoothSecurity.GetPinRequest"/>
        /// and <see cref="M:InTheHand.Net.Bluetooth.BluetoothSecurity.AnswerPinRequest(InTheHand.Net.BluetoothAddress,System.String)"/>
        /// </para>
        /// </remarks>
        bool RefusePinRequest(BluetoothAddress device);
        #endregion

        #region SetLinkKey
        /// <summary>
        /// <para><b>Not supported on Windows XP</b></para>
        /// </summary>
        /// -
        /// <param name="device">The device whose Link Key to retrieve.</param>
        /// <param name="linkKey">The 16-byte Link Key to set.</param>
        /// -
        /// <returns><c>true</c> if the operation was successful; <c>false</c> otherwise.</returns>
        /// -
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
        bool SetLinkKey(BluetoothAddress device, Guid linkKey);
        #endregion
    }
}
