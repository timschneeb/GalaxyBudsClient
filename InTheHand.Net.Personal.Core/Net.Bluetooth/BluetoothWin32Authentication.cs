using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using InTheHand.Net.Bluetooth.Msft;
using InTheHand.Win32;
using System.Diagnostics.CodeAnalysis;

namespace InTheHand.Net.Bluetooth
{

    /// <summary>
    /// Provides Bluetooth authentication services on desktop Windows.
    /// </summary>
    /// -
    /// <remarks>
    /// <note>This class is supported on desktop Windows and with the Microsoft
    /// stack only.
    /// </note>
    /// <para>This class can be used in one of two ways.  Firstly
    /// an instance can be created specifying one device that is being connected
    /// to and the PIN string to use for it.  (That form is used internally by
    /// <see cref="T:InTheHand.Net.Sockets.BluetoothClient"/> to support
    /// its <see cref="M:InTheHand.Net.Sockets.BluetoothClient.SetPin(System.String)"/> method).
    /// </para>
    /// <para>Secondly it can also be used a mode where a user supplied
    /// callback will be called when any device requires authentication,
    /// the callback includes a parameter of type 
    /// <see cref="T:InTheHand.Net.Bluetooth.BluetoothWin32AuthenticationEventArgs"/>.
    /// Various authentication methods are available in Bluetooth version
    /// 2.1 and later.  Which one is being used is indicated by the
    /// <see cref="P:InTheHand.Net.Bluetooth.BluetoothWin32AuthenticationEventArgs.AuthenticationMethod"/>
    /// property.
    /// If it is <see cref="F:InTheHand.Net.Bluetooth.BluetoothAuthenticationMethod.Legacy"/>
    /// then the callback method should set the
    /// <see cref="P:InTheHand.Net.Bluetooth.BluetoothWin32AuthenticationEventArgs.Pin"/>
    /// property.
    /// </para>
    /// <para>
    /// For the other authentication methods 
    /// e.g. <see cref="F:InTheHand.Net.Bluetooth.BluetoothAuthenticationMethod.NumericComparison"/>
    /// or <see cref="F:InTheHand.Net.Bluetooth.BluetoothAuthenticationMethod.OutOfBand"/>
    /// the callback method should use one or more of the other properties and
    /// methods e.g.
    /// <see cref="P:InTheHand.Net.Bluetooth.BluetoothWin32AuthenticationEventArgs.NumberOrPasskey"/>,
    /// <see cref="P:InTheHand.Net.Bluetooth.BluetoothWin32AuthenticationEventArgs.Confirm"/>,
    /// <see cref="P:InTheHand.Net.Bluetooth.BluetoothWin32AuthenticationEventArgs.ResponseNumberOrPasskey"/>,
    /// <see cref="M:InTheHand.Net.Bluetooth.BluetoothWin32AuthenticationEventArgs.ConfirmOob(System.Byte[],System.Byte[])"/>
    /// etc.
    /// </para>
    /// <para>
    /// See the example below for a 'Legacy' method handler.
    /// The callback mode can be configured to do a callback after the 
    /// &#x2018;send PIN&#x2019; action, this allows one to see if it was successful 
    /// etc.  An example sequence where the PIN was <strong>incorrect</strong> is as follows.
    /// </para>
    /// <code lang="none">
    ///Authenticate one device -- with wrong passcode here the first two times.
    ///Passcode respectively: 'BAD-x', 'BAD-y', '9876'
    ///Making PC discoverable
    ///Hit Return to complete
    ///Authenticating 0017E464CF1E wm_alan1
    ///  Attempt# 0, Last error code 0
    ///  Sending "BAD-x"
    ///Authenticating 0017E464CF1E wm_alan1
    ///  Attempt# 1, Last error code 1244
    ///  Sending "BAD-y"
    ///Authenticating 0017E464CF1E wm_alan1
    ///  Attempt# 2, Last error code 1167
    ///  Sending "9876"
    ///Authenticating 0017E464CF1E wm_alan1
    ///  Attempt# 3, Last error code 1167
    ///etc
    ///</code>
    /// <para>
    /// That is we see the error code of <c>1244=NativeErrorNotAuthenticated</c>
    /// once, and then the peer device disappears (<c>1167=NativeErrorDeviceNotConnected</c>).
    /// I suppose that's a security feature -- its stops an attacker
    /// from trying again and again with different passcodes.
    ///
    /// Anyway the result of that is that is it <strong>not</strong> worth repeating 
    /// the callback after the device disappears.  The code now enforces this.  With 
    /// <see cref="P:InTheHand.Net.Bluetooth.BluetoothWin32AuthenticationEventArgs.CallbackWithResult"/> 
    /// set to <c>true</c>, if the result of the previous attempt was &#x2018;success&#x2019; 
    /// or &#x2018;device not connected&#x2019; then any new PIN set in the callback 
    /// won&#x2019;t be used and thus the callback won&#x2019;t be called again 
    /// for that authentication attempt.
    /// </para>
    /// <para>A successful authentication process can thus be detected by checking if
    /// <code>e.PreviousNativeErrorCode == NativeErrorSuccess &amp;&amp; e.AttemptNumber != 0</code>
    /// </para>
    /// <para>
    /// </para>
    /// <para>The instance will continue receiving authentication requests
    /// until it is disposed or garbage collected, so keep a reference to it
    /// whilst it should be active and call 
    /// <see cref="M:InTheHand.Net.Bluetooth.BluetoothWin32Authentication.Dispose"/>
    /// when you&#x2019;re finished.
    /// </para>
    /// </remarks>
    /// -
    /// <example>
    /// If one wants to respond to PIN requests for one device with a known PIN then
    /// use the simple form which is initialized with an address and PIN.
    /// <code lang="C#">
    /// BluetoothWin32Authentication authenticator
    ///     = new BluetoothWin32Authentication(remoteEP.Address, m_pin);
    /// // when the peer is expected to require pairing, perhaps do some work.
    /// authenticator.Dispose();
    /// </code>
    /// 
    /// If one wants to see the PIN request, perhaps to be able to check the type
    /// of the peer by its address then use the form here which requests callbacks.
    /// (Note that this code assumes that 'Legacy' PIN-based pairing is being
    /// used; setting the Pin property will presumably have no effect if the
    /// authentication method being used is one of the v2.1 SSP forms).
    /// <code lang="VB.NET">
    /// Using pairer As New BluetoothWin32Authentication(AddressOf Win32AuthCallbackHandler)
    ///     Console.WriteLine("Hit Return to stop authenticating")
    ///     Console.ReadLine()
    /// End Using
    /// ...
    /// 
    /// Sub Win32AuthCallbackHandler(ByVal sender As Object, ByVal e As InTheHand.Net.Bluetooth.BluetoothWin32AuthenticationEventArgs)
    ///    ' Note we assume here that 'Legacy' pairing is being used,
    ///    ' and thus we only set the Pin property!
    ///    Dim address As String = e.Device.DeviceAddress.ToString()
    ///    Console.WriteLine("Received an authentication request from address " + address)
    ///    
    ///    ' compare the first 8 hex numbers, this is just a special case because in the
    ///    ' used scenario the model of the devices can be identified by the first 8 hex
    ///    ' numbers, the last 4 numbers being the device specific part.
    ///    If address.Substring(0, 8).Equals("0099880D") OrElse _
    ///            address.Substring(0, 8).Equals("0099880E") Then
    ///        ' send authentication response
    ///        e.Pin = "5276"
    ///    ElseIf (address.Substring(0, 8).Equals("00997788")) Then
    ///        ' send authentication response
    ///        e.Pin = "ásdfghjkl"
    ///    End If
    /// End Sub
    /// </code>
    /// </example>
    //[System.Security.Permissions.SecurityPermission(System.Security.Permissions.SecurityAction.Demand, UnmanagedCode = true)]
    public partial class BluetoothWin32Authentication : IDisposable
    {
        /// <summary>
        /// Windows&#x2019; ERROR_SUCCESS
        /// </summary>
        /// <remarks><see cref="P:InTheHand.Net.Bluetooth.BluetoothWin32AuthenticationEventArgs.PreviousNativeErrorCode"/>
        /// </remarks>
        public const int NativeErrorSuccess = 0;

        /// <summary>
        /// Windows&#x2019; ERROR_NOT_AUTHENTICATED
        /// </summary>
        /// <remarks><see cref="P:InTheHand.Net.Bluetooth.BluetoothWin32AuthenticationEventArgs.PreviousNativeErrorCode"/>
        /// </remarks>
        public const int NativeErrorNotAuthenticated = (int)Win32Error.ERROR_NOT_AUTHENTICATED;

        /// <summary>
        /// Windows&#x2019; ERROR_DEVICE_NOT_CONNECTED
        /// </summary>
        /// <remarks><see cref="P:InTheHand.Net.Bluetooth.BluetoothWin32AuthenticationEventArgs.PreviousNativeErrorCode"/>
        /// </remarks>
        public const int NativeErrorDeviceNotConnected = (int)Win32Error.ERROR_DEVICE_NOT_CONNECTED;

        // This class is XP only, but XmlDocs generation is from CF2 only, so we need
        // to compile on that platform too.  So, leave the skeleton with no innards.
#if WinXP
        //
        bool m_hasKB942567 = true; // "Windows Vista Feature Pack for Wireless" etc.
        readonly IntPtr m_radioHandle = IntPtr.Zero;
        BluetoothAuthenticationRegistrationHandle m_regHandle;
        NativeMethods.BluetoothAuthenticationCallback m_callback; // Stop gc of callback thunk.
        NativeMethods.BluetoothAuthenticationCallbackEx m_callbackEx; // Stop gc of callback thunk.
        //
        readonly BluetoothAddress m_remoteAddress;
        readonly String m_pin;
        readonly EventHandler<BluetoothWin32AuthenticationEventArgs> m_userCallback;
#endif

        //--------------------------------------------------------------

        /// <overloads>
        /// Initializes a new instance of the <see cref="T:InTheHand.Net.Bluetooth.BluetoothWin32Authentication"/> class.
        /// </overloads>
        /// -
        /// <summary>
        /// Initializes a new instance of the <see cref="T:InTheHand.Net.Bluetooth.BluetoothWin32Authentication"/> class,
        /// to respond to a specific address with a specific PIN string.
        /// </summary>
        /// -
        /// <remarks>
        /// <para>The instance will continue receiving authentication requests
        /// until it is disposed or garbage collected, so keep a reference to it
        /// whilst it should be active, and call 
        /// <see cref="M:InTheHand.Net.Bluetooth.BluetoothWin32Authentication.Dispose"/>
        /// when you&#x2019;re finished.
        /// </para>
        /// </remarks>
        /// -
        /// <param name="remoteAddress">The address of the device to authenticate,
        /// as a <see cref="T:InTheHand.Net.BluetoothAddress"/>.
        /// </param>
        /// <param name="pin">The PIN string to use for authentication, as a
        /// <see cref="T:System.String"/>.
        /// </param>
        public BluetoothWin32Authentication(BluetoothAddress remoteAddress, String pin)
        {
#if ! WinXP
            throw new PlatformNotSupportedException("BluetoothWin32Authentication is Win32 only.");
#else
            if (remoteAddress == null) {
                throw new ArgumentNullException("remoteAddress");
            }
            if (remoteAddress.ToInt64() == 0) {
                throw new ArgumentNullException("remoteAddress", "A non-blank address must be specified.");
            }
            if (pin == null) {
                throw new ArgumentNullException("pin");
            }
            m_remoteAddress = remoteAddress;
            m_pin = pin;
            Register(remoteAddress);
#endif
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:InTheHand.Net.Bluetooth.BluetoothWin32Authentication"/> class,
        /// to call a specified handler when any device requires authentication.
        /// </summary>
        /// -
        /// <remarks>
        /// <para>See the example below.
        /// </para>
        /// <para>The callback mode can be configured to do a callback after the 
        /// &#x2018;send PIN&#x2019;action, this allows one to see if it was successful 
        /// etc.  An example sequence where the PIN was <strong>incorrect</strong> is as follows.
        /// </para>
        /// <code lang="none">
        ///Authenticate one device -- with wrong passcode here the first two times.
        ///Passcode respectively: 'BAD-x', 'BAD-y', '9876'
        ///Making PC discoverable
        ///Hit Return to complete
        ///Authenticating 0017E464CF1E wm_alan1
        ///  Attempt# 0, Last error code 0
        ///  Sending "BAD-x"
        ///Authenticating 0017E464CF1E wm_alan1
        ///  Attempt# 1, Last error code 1244
        ///  Sending "BAD-y"
        ///Authenticating 0017E464CF1E wm_alan1
        ///  Attempt# 2, Last error code 1167
        ///  Sending "9876"
        ///Authenticating 0017E464CF1E wm_alan1
        ///  Attempt# 3, Last error code 1167
        ///etc
        ///</code>
        /// <para>
        /// That is we see the error code of <c>1244=NativeErrorNotAuthenticated</c>
        /// once, and then the peer device disappears (<c>1167=NativeErrorDeviceNotConnected</c>).
        /// I suppose that's a security feature -- its stops an attacker
        /// from trying again and again with different passcodes.
        ///
        /// Anyway the result of that is that is it <strong>not</strong> worth repeating 
        /// the callback after the device disappears.  The code now enforces this.  With 
        /// <see cref="P:InTheHand.Net.Bluetooth.BluetoothWin32AuthenticationEventArgs.CallbackWithResult"/> 
        /// set to <c>true</c>, if the result of the previous attempt was &#x2018;success&#x2019; 
        /// or &#x2018;device not connected&#x2019; then any new PIN set in the callback 
        /// won&#x2019;t be used and thus the callback won&#x2019;t be called again 
        /// for that authentication attempt.
        /// </para>
        /// <para>A successful authentication process can thus be detected by setting
        /// <c>CallbackWithResult=true</c> and checking in the callback if
        /// <code>  e.PreviousNativeErrorCode == NativeErrorSuccess &amp;&amp; e.AttemptNumber != 0</code>
        /// </para>
        /// <para>
        /// </para>
        /// <para>The instance will continue receiving authentication requests
        /// until it is disposed or garbage collected, so keep a reference to it
        /// whilst it should be active, and call 
        /// <see cref="M:InTheHand.Net.Bluetooth.BluetoothWin32Authentication.Dispose"/>
        /// when you&#x2019;re finished.
        /// </para>
        /// </remarks>
        /// -
        /// <param name="handler">A reference to a handler function that can respond
        /// to authentication requests.
        /// </param>
        /// -
        /// <example>
        /// <code lang="VB.NET">
        /// Using pairer As New BluetoothWin32Authentication(AddressOf Win32AuthCallbackHandler)
        ///     Console.WriteLine("Hit Return to stop authenticating")
        ///     Console.ReadLine()
        /// End Using
        /// ...
        /// 
        /// Sub Win32AuthCallbackHandler(ByVal sender As Object, ByVal e As InTheHand.Net.Bluetooth.BluetoothWin32AuthenticationEventArgs)
        ///    Dim address As String = e.Device.DeviceAddress.ToString()
        ///    Console.WriteLine("Received an authentication request from address " + address)
        ///    
        ///    ' compare the first 8 hex numbers, this is just a special case because in the
        ///    ' used scenario the model of the devices can be identified by the first 8 hex
        ///    ' numbers, the last 4 numbers being the device specific part.
        ///    If address.Substring(0, 8).Equals("0099880D") OrElse _
        ///            address.Substring(0, 8).Equals("0099880E") Then
        ///        ' send authentication response
        ///        e.Pin = "5276"
        ///    ElseIf (address.Substring(0, 8).Equals("00997788")) Then
        ///        ' send authentication response
        ///        e.Pin = "ásdfghjkl"
        ///    End If
        /// End Sub
        /// </code>
        /// </example>
        public BluetoothWin32Authentication(EventHandler<BluetoothWin32AuthenticationEventArgs> handler)
        {
#if ! WinXP
            throw new PlatformNotSupportedException("BluetoothWin32Authentication is Win32 only.");
#else
            m_userCallback = handler;
            Register(new BluetoothAddress(0)); // All devices
#endif
        }

#if WinXP
        private void Register(BluetoothAddress remoteAddress)
        {
            System.Diagnostics.Debug.Assert(m_pin == null ^ m_userCallback == null);
            //
            m_callback = new NativeMethods.BluetoothAuthenticationCallback(NativeCallback);
            m_callbackEx = new NativeMethods.BluetoothAuthenticationCallbackEx(NativeCallback);
            BLUETOOTH_DEVICE_INFO bdi = new BLUETOOTH_DEVICE_INFO(remoteAddress);
            UInt32 ret;
            if (m_hasKB942567) {
                try {
                    ret = NativeMethods.BluetoothRegisterForAuthenticationEx(
                        ref bdi, out m_regHandle, m_callbackEx, IntPtr.Zero);
                } catch (EntryPointNotFoundException) {
                    m_hasKB942567 = false;
                    ret = NativeMethods.BluetoothRegisterForAuthentication(
                        ref bdi, out m_regHandle, m_callback, IntPtr.Zero);
                }
            } else {
                ret = NativeMethods.BluetoothRegisterForAuthentication(
                    ref bdi, out m_regHandle, m_callback, IntPtr.Zero);
            }
            int gle = Marshal.GetLastWin32Error();
            System.Diagnostics.Debug.Assert(ret == NativeErrorSuccess,
                "BluetoothRegisterForAuthentication failed, GLE="
                + gle.ToString() + "=0x" + gle.ToString("X"));
            if (ret != NativeErrorSuccess) {
                throw new System.ComponentModel.Win32Exception(gle);
            }
            m_regHandle.SetObjectToKeepAlive(m_callback, m_callbackEx);
        }

        //--------------------------------------------------------------
        private bool NativeCallback(IntPtr param, ref BLUETOOTH_AUTHENTICATION_CALLBACK_PARAMS pAuthCallbackParams)
        {
            Debug.WriteLine("BtRegForAuthEx Callback:");
            Debug.WriteLine(string.Format(System.Globalization.CultureInfo.InvariantCulture,
                "authParams: addr: 0x{0:X12},\n  meth: '{1}'\n  capa: '{2}'\n  requ: '{3}'\n  n/pk: 0x{4:X}={4}",
                pAuthCallbackParams.deviceInfo.Address,
                pAuthCallbackParams.authenticationMethod,
                pAuthCallbackParams.ioCapability,
                pAuthCallbackParams.authenticationRequirements,
                pAuthCallbackParams.Numeric_Value_Passkey));
            //if (pAuthCallbackParams.authenticationMethod
            //        == BluetoothAuthenticationMethod.Legacy) {
            BLUETOOTH_DEVICE_INFO bdi = pAuthCallbackParams.deviceInfo;
            BLUETOOTH_AUTHENTICATION_CALLBACK_PARAMS? refParams = pAuthCallbackParams;
            NativeCallback(pAuthCallbackParams.authenticationMethod, param, ref bdi, true, ref refParams);
            //}
            return false;
        }

        private bool NativeCallback(IntPtr param, ref BLUETOOTH_DEVICE_INFO bdi)
        {
            Debug.WriteLine("BtRegForAuth[NO-Ex] Callback.");
            BLUETOOTH_AUTHENTICATION_CALLBACK_PARAMS? nullRefParams = null;
            return NativeCallback(BluetoothAuthenticationMethod.Legacy,
                param, ref bdi, false, ref nullRefParams);
        }

        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "param")]
        private bool NativeCallback(BluetoothAuthenticationMethod method,
            IntPtr param, ref BLUETOOTH_DEVICE_INFO bdi, bool versionEx,
            ref BLUETOOTH_AUTHENTICATION_CALLBACK_PARAMS? pAuthCallbackParams)
        {
            System.Diagnostics.Debug.Assert(m_pin == null ^ m_userCallback == null);
            //
            System.Diagnostics.Debug.WriteLine(String.Format(
                    System.Globalization.CultureInfo.InvariantCulture,
                    "AuthenticateResponder callback (for {0}): 0x{1:X} 0x{2:X}",
                    m_remoteAddress, param, bdi.Address));
            //
            String pin;
            Int32 ret;
            if (m_pin != null) {
                // Pre-specified case.
                System.Diagnostics.Debug.Assert(bdi.Address == m_remoteAddress.ToInt64(),
                    "Should only get callback for the single device.");
                //TODO if (bdi.Address != m_remoteAddress.ToInt64()) {
                //    return false;
                //}
                pin = m_pin;
                if (versionEx) {
                    // TODO Want to send a positive response here to NumericComparison??
                    ret = BluetoothSendAuthenticationResponseExPin(ref bdi, pin);
                } else {
                    ret = NativeMethods.BluetoothSendAuthenticationResponse(
                        m_radioHandle, ref bdi, pin);
                }
            } else if (method == BluetoothAuthenticationMethod.Legacy) {
                // Callback case.
                System.Diagnostics.Debug.Assert(m_userCallback != null);
                BluetoothWin32AuthenticationEventArgs e
                    = new BluetoothWin32AuthenticationEventArgs(bdi);
                while (true) {
                    // Callback the user code
                    OnAuthentication(e);
                    // Don't proceed if no (null) passcode given, or
                    // if the last attempt was successful, or
                    // the decvice has disppeared.
                    if (e.Pin == null) {
                        ret = NativeErrorSuccess;
                        break;
                    }
                    if (e.PreviousNativeErrorCode == NativeErrorSuccess && e.AttemptNumber != 0) {
                        Debug.Assert(e.CallbackWithResult, "NOT CbWR but here (A#)!!");
                        ret = NativeErrorSuccess;
                        break;
                    }
                    if (e.PreviousNativeErrorCode == NativeErrorDeviceNotConnected) {
                        Debug.Assert(e.CallbackWithResult, "NOT CbWR but here (DNC)!!");
                        // When I try this (against Win2k+Belkin and iPaq hx2190,
                        // both apparently with Broadcom) I see:
                        //[[
                        //Authenticate one device -- with wrong passcode here the first two times.
                        //Passcode respectively: 'BAD-x', 'BAD-y', '9876'
                        //Making PC discoverable
                        //Hit Return to complete
                        //Authenticating 0017E464CF1E wm_alan1
                        //  Attempt# 0, Last error code 0
                        //Using '0.23672947484847'
                        //Authenticating 0017E464CF1E wm_alan1
                        //  Attempt# 1, Last error code 1244
                        //Using '0.54782851764365'
                        //Authenticating 0017E464CF1E wm_alan1
                        //  Attempt# 2, Last error code 1167
                        //Using '9876'
                        //Authenticating 0017E464CF1E wm_alan1
                        //  Attempt# 3, Last error code 1167
                        //etc
                        //]]
                        // That is we see the error code of 1244=ErrorNotAuthenticated
                        // once, and then the peer device disappears (1167=ErrorDeviceNotConnected).
                        // I suppose that's a security feature -- its stops an attacker
                        // from trying again and again with different passcodes.
                        //
                        // Anyway the result of that is that is it NOT worth repeating
                        // the callback after the device disappears.
                        ret = NativeErrorSuccess;
                        break;
                    }
                    pin = e.Pin;
                    System.Diagnostics.Debug.WriteLine(String.Format(System.Globalization.CultureInfo.InvariantCulture,
                        "BW32Auth SendAuthRsp pin {0}", pin));
                    if (versionEx) {
                        ret = BluetoothSendAuthenticationResponseExPin(ref bdi, pin);
                    } else {
                        ret = NativeMethods.BluetoothSendAuthenticationResponse(
                            m_radioHandle, ref bdi, pin);
                    }
                    if (ret != NativeErrorSuccess) {
                        System.Diagnostics.Trace.WriteLine(String.Format(
                            System.Globalization.CultureInfo.InvariantCulture,
                            "    BluetoothSendAuthenticationResponse failed: {0}=0x{0:X}", ret));
                    }
                    // Have to callback the user code after the attempt?
                    BluetoothWin32AuthenticationEventArgs lastEa = e;
                    if (!lastEa.CallbackWithResult) {
                        break;
                    }
                    e = new BluetoothWin32AuthenticationEventArgs(ret, lastEa);
                }
            } else if (method == BluetoothAuthenticationMethod.NumericComparison
                    || method == BluetoothAuthenticationMethod.Passkey
                    || method == BluetoothAuthenticationMethod.PasskeyNotification
                    || method == BluetoothAuthenticationMethod.OutOfBand
                    ) {
                // Callback case.
                System.Diagnostics.Debug.Assert(m_userCallback != null);
                BluetoothWin32AuthenticationEventArgs e
                    = new BluetoothWin32AuthenticationEventArgs(bdi, ref pAuthCallbackParams);
                while (true) {
                    // Callback the user code
                    OnAuthentication(e);
                    // Check if after e.CallbackWithResult...
                    if (e.PreviousNativeErrorCode == NativeErrorSuccess && e.AttemptNumber != 0) {
                        Debug.Assert(e.CallbackWithResult, "NOT CbWR but here (A#)!!");
                        ret = NativeErrorSuccess;
                        break;
                    }
                    if (e.PreviousNativeErrorCode == NativeErrorDeviceNotConnected) {
                        Debug.Assert(e.CallbackWithResult, "NOT CbWR but here (DNC)!!");
                        ret = NativeErrorSuccess;
                        break;
                    }
                    bool? confirm = e.Confirm;
                    System.Diagnostics.Debug.WriteLine(String.Format(System.Globalization.CultureInfo.InvariantCulture,
                        "BW32Auth SendAuthRspEx-NumComparison {0}", confirm));
                    if (confirm == null) {
                        ret = NativeErrorSuccess;
                        break;
                    }
                    if (method != BluetoothAuthenticationMethod.OutOfBand) {
                        ret = BluetoothSendAuthenticationResponseExNumCompPasskey(ref bdi, confirm, e);
                    } else {
                        ret = BluetoothSendAuthenticationResponseExOob(ref bdi, confirm, e);
                    }
                    if (ret != NativeErrorSuccess) {
                        System.Diagnostics.Trace.WriteLine(String.Format(
                            System.Globalization.CultureInfo.InvariantCulture,
                            "    BluetoothSendAuthenticationResponseEx failed: {0}=0x{0:X}", ret));
                    }
                    // Have to callback the user code after the attempt?
                    BluetoothWin32AuthenticationEventArgs lastEa = e;
                    if (!lastEa.CallbackWithResult) {
                        break;
                    }
                    e = new BluetoothWin32AuthenticationEventArgs(ret, lastEa);
                }
            } else {
                Debug.Fail("Unsupported auth method: " + method);
                ret = NativeErrorSuccess;
            }
            //
            if (ret != NativeErrorSuccess) {
                System.Diagnostics.Trace.WriteLine(String.Format(
                    System.Globalization.CultureInfo.InvariantCulture,
                    "BluetoothSendAuthenticationResponse failed: {0}=0x{0:X}", ret));
            }
            return true; // "The return value from this function is ignored by the system."
        }

        private int BluetoothSendAuthenticationResponseExPin(ref BLUETOOTH_DEVICE_INFO bdi, String pin)
        {
            Int32 ret;
            BLUETOOTH_AUTHENTICATE_RESPONSE__PIN_INFO rsp = new BLUETOOTH_AUTHENTICATE_RESPONSE__PIN_INFO();
            rsp.authMethod = BluetoothAuthenticationMethod.Legacy;
            rsp.bthAddressRemote = bdi.Address;
            rsp.pinInfo.pin = new byte[BLUETOOTH_PIN_INFO.BTH_MAX_PIN_SIZE];
            byte[] buf = System.Text.Encoding.UTF8.GetBytes(pin);
            int len = Math.Min(BLUETOOTH_PIN_INFO.BTH_MAX_PIN_SIZE, buf.Length);
            Array.Copy(buf, rsp.pinInfo.pin, len);
            rsp.pinInfo.pinLength = len;
            ret = NativeMethods.BluetoothSendAuthenticationResponseEx(
                m_radioHandle, ref rsp);
            return ret;
        }

        private int BluetoothSendAuthenticationResponseExNumCompPasskey(ref BLUETOOTH_DEVICE_INFO bdi,
            bool? confirm, BluetoothWin32AuthenticationEventArgs e)
        {
            if (!confirm.HasValue) {
                return NativeErrorSuccess;
            }
            Int32 ret;
            var rsp = new BLUETOOTH_AUTHENTICATE_RESPONSE__NUMERIC_COMPARISON_PASSKEY_INFO();
            rsp.negativeResponse = 1; // Default to NEGATIVE, really set below.
            rsp.authMethod = e.AuthenticationMethod;
            if (!(e.AuthenticationMethod == BluetoothAuthenticationMethod.NumericComparison
                    || e.AuthenticationMethod == BluetoothAuthenticationMethod.Passkey
                    || e.AuthenticationMethod == BluetoothAuthenticationMethod.PasskeyNotification)) {
                Debug.Fail("Bad call!!! method is: " + e.AuthenticationMethod);
                return NativeErrorNotAuthenticated;
            }
            rsp.bthAddressRemote = bdi.Address;
            switch (confirm) {
                case true:
                    rsp.negativeResponse = 0;
                    // Set the response number/passcode value
                    if (e.ResponseNumberOrPasskey.HasValue) {
                        rsp.numericComp_passkey = checked((uint)e.ResponseNumberOrPasskey.Value);
                    }
                    break;
                case false:
                case null:
                    Debug.Assert(confirm != null, "Should have exited above when non-response.");
                    rsp.negativeResponse = 1;
                    break;
            }
            ret = NativeMethods.BluetoothSendAuthenticationResponseEx(
                m_radioHandle, ref rsp);
            return ret;
        }

        private int BluetoothSendAuthenticationResponseExOob(ref BLUETOOTH_DEVICE_INFO bdi,
            bool? confirm, BluetoothWin32AuthenticationEventArgs e)
        {
            if (!confirm.HasValue) {
                return NativeErrorSuccess;
            }
            Int32 ret;
            var rsp = new BLUETOOTH_AUTHENTICATE_RESPONSE__OOB_DATA_INFO();
            rsp.negativeResponse = 1; // Default to NEGATIVE, really set below.
            rsp.authMethod = e.AuthenticationMethod;
            if (!(e.AuthenticationMethod == BluetoothAuthenticationMethod.OutOfBand)) {
                Debug.Fail("Bad call!!! method is: " + e.AuthenticationMethod);
                return NativeErrorNotAuthenticated;
            }
            rsp.bthAddressRemote = bdi.Address;
            switch (confirm) {
                case true:
                    rsp.negativeResponse = 0;
                    // Set the oob values
                    // (Testing shows that P/Invoke disallowed only incorrect 
                    // lengthinline arrays, null arrays are ok).
                    if (e.OobC != null) rsp.oobInfo.C = e.OobC;
                    if (e.OobR != null) rsp.oobInfo.R = e.OobR;
                    break;
                case false:
                case null:
                    Debug.Assert(confirm != null, "Should have exited above when non-response.");
                    rsp.negativeResponse = 1;
                    break;
            }
            ret = NativeMethods.BluetoothSendAuthenticationResponseEx(
                m_radioHandle, ref rsp);
            return ret;
        }

        /// <summary>
        /// Calls the authentication callback handler.
        /// </summary>
        /// -
        /// <param name="e">An instance of <see cref="T:InTheHand.Net.Bluetooth.BluetoothWin32AuthenticationEventArgs"/> 
        /// containing the details of the authentication callback.
        /// </param>
        protected virtual void OnAuthentication(BluetoothWin32AuthenticationEventArgs e)
        {
            EventHandler<BluetoothWin32AuthenticationEventArgs> callback = m_userCallback;
            if (callback != null) {
                m_userCallback(this, e);
            }
        }
#endif
        //--------------------------------------------------------------

        #region IDisposable Members
        /// <summary>
        /// Release the unmanaged resources used by the <see cref="T:InTheHand.Net.Bluetooth.BluetoothWin32Authentication"/>.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Release the unmanaged resources used by the <see cref="T:InTheHand.Net.Bluetooth.BluetoothWin32Authentication"/>,
        /// and optionally disposes of the managed resources.
        /// </summary>
        protected virtual void Dispose(bool disposing)
        {
#if WinXP
            if (disposing) {
                if (m_regHandle != null) {
                    m_regHandle.Dispose();
                }
            }
#endif
        }
        #endregion

    }//class


#if WinXP
    // Or should we derive from Microsoft.Win32.SafeHandles.SafeHandleMinusOneIsInvalid?
    sealed internal class BluetoothAuthenticationRegistrationHandle
        : Microsoft.Win32.SafeHandles.SafeHandleZeroOrMinusOneIsInvalid
    {
        object m_objectToKeepAlive, m_objectToKeepAlive2;
#if DEBUG
        WeakReference m_weakRef, m_weakRef2;
#endif

        public BluetoothAuthenticationRegistrationHandle()
            : base(true)
        { }

        protected override bool ReleaseHandle()
        {
#if false // was: DEBUG -- fails in Finalizer case IIRC
            System.Diagnostics.Debug.Assert((m_weakRef == null) == (m_objectToKeepAlive == null),
                "Only one of the two objects set.");
            if (m_weakRef != null) {
                bool isAlive = m_weakRef.IsAlive;
                try {
                    System.Diagnostics.Debug.Assert(isAlive
                        || AppDomain.CurrentDomain.IsFinalizingForUnload(),
                        "m_weakRef.IsAlive");
                } catch (ObjectDisposedException) { } // SafeHandle->CriticalFinalizer
            }
#endif
            bool success = NativeMethods.BluetoothUnregisterAuthentication(handle);
            int gle = Marshal.GetLastWin32Error();
            System.Diagnostics.Debug.Assert(success,
                "BluetoothUnregisterAuthentication returned false, GLE="
                + gle.ToString() + "=0x" + gle.ToString("X"));
            return success;
        }

        internal void SetObjectToKeepAlive(object objectToKeepAlive, object objectToKeepAlive2)
        {
            m_objectToKeepAlive = objectToKeepAlive;
            m_objectToKeepAlive2 = objectToKeepAlive2;
#if DEBUG
            m_weakRef = new WeakReference(m_objectToKeepAlive);
            m_weakRef2 = new WeakReference(m_objectToKeepAlive);
#endif
        }

    }//class
#endif
}
