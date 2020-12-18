using System;
using System.Diagnostics.CodeAnalysis;
using InTheHand.Net.Bluetooth.Factory;

namespace InTheHand.Net.Bluetooth
{
    /// <summary>
    /// Handles security between bluetooth devices.
    /// </summary>
    public 
#if !V1
        static 
#endif
        class BluetoothSecurity
    {
#if V1
        private BluetoothSecurity()
        { }
#endif

        static IBluetoothSecurity _impl;

        //--------
        static IBluetoothSecurity GetImpl()
        {
            if (_impl == null) {
                _impl = BluetoothFactory.Factory.DoGetBluetoothSecurity();
                if (_impl == null)
                    throw new InvalidOperationException("Null IBluetoothSecurity returned.");
                //Utils.MiscUtils.Trace_WriteLine("IBluetoothSecurity m_impl = " + m_impl.GetType().FullName);
            }
            return _impl;
        }

        //--------

        //TODO PairRequest XmlDocs for XP and CE pre 5.0.
        /// <summary>
        /// Intiates pairing for a remote device.
        /// </summary>
        /// <param name="device">Remote device with which to pair.</param>
        /// <param name="pin">Chosen PIN code, must be between 1 and 16 ASCII characters.
        /// Or <c>null</c> to initiate pairing to be handled for instance by
        /// <see cref="T:InTheHand.Net.Bluetooth.BluetoothWin32Authentication"/>.
        /// </param>
        /// -
        /// <remarks>
        /// <para>These days most devices are using Bluetooth version 2.1's 
        /// Secure Simple Pairing (SSP) which does not use a PIN code. So instead use this
        /// function only to initiate pairing by passing <c>null</c> as <paramref name="pin"/>.
        /// To handle pairing, at least on desktop Windows with the Microsoft Bluetooth stack,
        /// use class <see cref="T:InTheHand.Net.Bluetooth.BluetoothWin32Authentication"/>
        /// and respond to the pairing callback method as required; for most of the SSP
        /// pairing methods that is to set <see cref="P:InTheHand.Net.Bluetooth.BluetoothWin32AuthenticationEventArgs.Confirm">BluetoothWin32AuthenticationEventArgs.Confirm</see>
        /// to <c>true</c>, and for traditional pairing instead set <see cref="P:InTheHand.Net.Bluetooth.BluetoothWin32AuthenticationEventArgs.Pin">BluetoothWin32AuthenticationEventArgs.Pin</see>
        /// to the PIN code.
        /// </para>
        /// <para>On Windows CE platforms this calls <c>BthPairRequest</c>,
        /// its MSDN remarks say:
        /// </para>
        /// <para>&#x201C;BthPairRequest passes the parameters to the <c>BthSetPIN</c>
        /// function and creates an ACL connection. Once the connection is established,
        /// it calls the <c>BthAuthenticate</c> function to authenticate the device.&#x201D;
        /// </para>
        /// <para>On Windows XP/Vista platforms this calls <c>BluetoothAuthenticateDevice</c>.
        /// If the pin argument is set to null pairing is initiated and if 
        /// there is no instance of class <see cref="T:InTheHand.Net.Bluetooth.BluetoothWin32Authentication"/>
        /// then a Wizard is displayed to accept a PIN from the user.
        /// </para>
        /// <para>See also 
        /// <see cref="M:InTheHand.Net.Bluetooth.BluetoothSecurity.SetPin(InTheHand.Net.BluetoothAddress,System.String)"/>
        /// </para>
        /// </remarks>
        /// <returns>Whether the operation was successful.</returns>
        public static bool PairRequest(BluetoothAddress device, string pin)
        {
            return GetImpl().PairRequest(device, pin);
        }

        /// <summary>
        /// Remove the pairing with the specified device
        /// </summary>
        /// -
        /// <param name="device">Remote device with which to remove pairing.</param>
        /// -
        /// <returns>TRUE if device was successfully removed, else FALSE.</returns>
        public static bool RemoveDevice(BluetoothAddress device)
        {
            if (device == null)
                throw new ArgumentNullException("device");
            return GetImpl().RemoveDevice(device);
        }

        //----

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
        public static bool SetPin(BluetoothAddress device, string pin)
        {
            if (device == null)
                throw new ArgumentNullException("device");
            if (pin == null)
                throw new ArgumentNullException("pin");
            return GetImpl().SetPin(device, pin);
        }

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
        public static bool RevokePin(BluetoothAddress device)
        {
            if (device == null)
                throw new ArgumentNullException("device");
            return GetImpl().RevokePin(device);
        }

        //--------

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
        public static bool SetLinkKey(BluetoothAddress device, Guid linkKey)
        {
            if (device == null)
                throw new ArgumentNullException("device");
            return GetImpl().SetLinkKey(device, linkKey);
        }

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
        public static BluetoothAddress GetPinRequest()
        {
            return GetImpl().GetPinRequest();
        }

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
        public static bool RefusePinRequest(BluetoothAddress device)
        {
            if (device == null)
                throw new ArgumentNullException("device");
            return GetImpl().RefusePinRequest(device);
        }
    }
}
