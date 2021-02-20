using System;
using System.Diagnostics;
using InTheHand.Net.Bluetooth.Msft;
using InTheHand.Net.Sockets;
using System.Diagnostics.CodeAnalysis;

namespace InTheHand.Net.Bluetooth
{
    /// <summary>
    /// Provides data for an authentication event.
    /// </summary>
    /// -
    /// <remarks>
    /// <para>For usage information, see the class documentation at
    /// <see cref="T:InTheHand.Net.Bluetooth.BluetoothWin32Authentication"/> it includes
    /// an example, 
    /// also see the documentation on each of this class&#x2019;s properties.
    /// </para>
    /// </remarks>
    public sealed class BluetoothWin32AuthenticationEventArgs : EventArgs
    {
        readonly BluetoothDeviceInfo m_device;
        readonly BluetoothAuthenticationMethod _authMethod;
        readonly BluetoothAuthenticationRequirements _authReq;
        readonly BluetoothIoCapability _authIoCapa;
        readonly int? _numberOrPasskey;
        int? _responseNumberOrPasskey;
        string m_pin;
        bool? _confirmSsp;
        byte[] _oobC, _oobR;
        //
        bool m_callbackWithResult;
        readonly int m_attemptNumber;
        readonly int m_errorCode;

        //--------------------------------------------------------------

        /// <summary>
        /// Initialize an instance of <see cref="T:InTheHand.Net.Bluetooth.BluetoothWin32AuthenticationEventArgs"/>.
        /// </summary>
        public BluetoothWin32AuthenticationEventArgs()
        { }

        /// <summary>
        /// Initialize an instance of <see cref="T:InTheHand.Net.Bluetooth.BluetoothWin32AuthenticationEventArgs"/>.
        /// </summary>
        /// -
        /// <param name="device">The device information to store in the event args.
        /// </param>
        public BluetoothWin32AuthenticationEventArgs(BluetoothDeviceInfo device)
        {
            if (device == null) {
                throw new ArgumentNullException("device");
            }
            m_device = device;
            _authMethod = BluetoothAuthenticationMethod.Legacy;
            _authReq = BluetoothAuthenticationRequirements.MITMProtectionNotDefined;
            _authIoCapa = BluetoothIoCapability.Undefined;
            _numberOrPasskey = null; // Keep compiler happy on CF2 build.
        }

        internal BluetoothWin32AuthenticationEventArgs(BLUETOOTH_DEVICE_INFO device)
            : this(new BluetoothDeviceInfo(new WindowsBluetoothDeviceInfo(device)))
        {
        }

#if !NETCF && !NO_MSFT
        internal BluetoothWin32AuthenticationEventArgs(BLUETOOTH_DEVICE_INFO device,
                ref BLUETOOTH_AUTHENTICATION_CALLBACK_PARAMS? authCallbackParams)
            : this(device)
        {
            _authMethod = authCallbackParams.Value.authenticationMethod;
            _authReq = authCallbackParams.Value.authenticationRequirements;
            _authIoCapa = authCallbackParams.Value.ioCapability;
            const uint MaxSixDigitDecimal = 999999;
            Debug.Assert(authCallbackParams.Value.Numeric_Value_Passkey <= MaxSixDigitDecimal);
            _numberOrPasskey = checked((int)authCallbackParams.Value.Numeric_Value_Passkey);
        }
#endif

        internal BluetoothWin32AuthenticationEventArgs(int errorCode, BluetoothWin32AuthenticationEventArgs previousEa)
        {
            if (previousEa == null) {
                throw new ArgumentNullException("previousEa");
            }
            m_device = previousEa.Device;
            _authIoCapa = previousEa._authIoCapa;
            _authMethod = previousEa._authMethod;
            _authReq = previousEa._authReq;
            _numberOrPasskey = previousEa._numberOrPasskey;
            m_attemptNumber = previousEa.AttemptNumber + 1;
            //
            m_errorCode = errorCode;
        }

        //--------------------------------------------------------------
        /// <summary>
        /// Gets the device requiring an authentication response as a
        /// <see cref="T:InTheHand.Net.Sockets.BluetoothDeviceInfo"/>.
        /// </summary>
        public BluetoothDeviceInfo Device { get { return m_device; } }

        /// <summary>
        /// Gets a <see cref="T:InTheHand.Net.Bluetooth.BluetoothAuthenticationRequirements"/>
        /// enumeration value that specifies the 'Man in the Middle' protection
        /// required for authentication.
        /// </summary>
        public BluetoothAuthenticationRequirements AuthenticationRequirements
        {
            get { return _authReq; }
        }

        /// <summary>
        /// Gets a <see cref="T:InTheHand.Net.Bluetooth.BluetoothIoCapability"/>
        /// enumeration value that defines the input/output capabilities of the
        /// Bluetooth device.
        /// </summary>
        public BluetoothIoCapability IoCapability
        {
            get { return _authIoCapa; }
        }

        /// <summary>
        /// Gets a <see cref="T:InTheHand.Net.Bluetooth.BluetoothAuthenticationMethod"/>
        /// enumeration value that defines the authentication method utilized
        /// by the Bluetooth device.
        /// </summary>
        /// -
        /// <remarks>
        /// <para>The method to be used depends on the
        /// <see cref="P:InTheHand.Net.Bluetooth.BluetoothWin32AuthenticationEventArgs.AuthenticationRequirements"/>
        /// and the <see cref="P:InTheHand.Net.Bluetooth.BluetoothWin32AuthenticationEventArgs.IoCapability"/> on both machines.
        /// </para>
        /// <para>See <see href="http://32feet.codeplex.com/wikipage?title=BluetoothWin32Authentication"/>
        /// for how to respond to each of the authentication methods.
        /// </para>
        /// </remarks>
        public BluetoothAuthenticationMethod AuthenticationMethod
        {
            get { return _authMethod; }
        }

        /// <summary>
        /// Gets whether the <see cref="AuthenticationMethod"/> is 
        /// <see cref="F:InTheHand.Net.Bluetooth.BluetoothAuthenticationMethod.NumericComparison"/>
        /// and it is of subtype "JustWorks".
        /// </summary>
        /// -
        /// <remarks>
        /// <para>Gets whether the <see cref="AuthenticationMethod"/> is 
        /// <see cref="F:InTheHand.Net.Bluetooth.BluetoothAuthenticationMethod.NumericComparison"/>
        /// and it is of subtype "JustWorks".
        /// </para>
        /// <para>If true then a simple Yes/No answer from the user is adequate,
        /// Or if false then the <see cref="P:InTheHand.Net.Bluetooth.BluetoothWin32AuthenticationEventArgs.NumberOrPasskey"/>
        /// (or <see cref="P:InTheHand.Net.Bluetooth.BluetoothWin32AuthenticationEventArgs.NumberOrPasskeyAsString"/>)
        /// value should be displayed to the user(s) so that he/she/they can
        /// verify that the values displayed on both devices are the same.
        /// Is null if 
        /// <see cref="P:InTheHand.Net.Bluetooth.BluetoothWin32AuthenticationEventArgs.AuthenticationRequirements"/>
        /// is not 
        /// <see cref="F:InTheHand.Net.Bluetooth.BluetoothAuthenticationMethod.NumericComparison"/>.
        /// </para>
        /// </remarks>
        public bool? JustWorksNumericComparison
        {
            get
            {
                if (AuthenticationMethod != BluetoothAuthenticationMethod.NumericComparison)
                    return null;
                switch (AuthenticationRequirements) {
                    // MitmProtection NOT required
                    case BluetoothAuthenticationRequirements.MITMProtectionNotRequired:
                    case BluetoothAuthenticationRequirements.MITMProtectionNotRequiredBonding:
                    case BluetoothAuthenticationRequirements.MITMProtectionNotRequiredGeneralBonding:
                        return true;
                    // MitmProtection required
                    case BluetoothAuthenticationRequirements.MITMProtectionRequired:
                    case BluetoothAuthenticationRequirements.MITMProtectionRequiredBonding:
                    case BluetoothAuthenticationRequirements.MITMProtectionRequiredGeneralBonding:
                        return false;
                    default:
                        Debug.Assert(TestUtilities.IsUnderTestHarness(),
                            "Unknown AuthenticationRequirements value: " + AuthenticationRequirements);
                        return false;
                }
            }
        }

        /// <summary>
        /// Get the Numeric or Passcode value being used by the 
        /// SSP pairing event.
        /// </summary>
        /// -
        /// <value>Is a six digit number from 000000 to 999999,
        /// or <see langword="null"/> if not present.
        /// </value>
        /// -
        /// <remarks>
        /// <para>Will be present in the
        /// <see cref="F:InTheHand.Net.Bluetooth.BluetoothAuthenticationMethod.NumericComparison"/>,
        /// <see cref="P:InTheHand.Net.Bluetooth.BluetoothWin32AuthenticationEventArgs.JustWorksNumericComparison"/>,
        /// and <see cref="F:InTheHand.Net.Bluetooth.BluetoothAuthenticationMethod.PasskeyNotification"/>
        /// authentication methods only.
        /// </para>
        /// <para>Is a six digit number from 000000 to 999999.
        /// </para>
        /// </remarks>
        public int? NumberOrPasskey
        {
            get { return _numberOrPasskey; }
        }

        /// <summary>
        /// Gets the <see cref="P:InTheHand.Net.Bluetooth.BluetoothWin32AuthenticationEventArgs.NumberOrPasskey"/>
        /// formatted in its correct six decimal digits format.
        /// </summary>
        /// -
        /// <value>A <see cref="T:System.String"/> representing
        /// <see cref="P:InTheHand.Net.Bluetooth.BluetoothWin32AuthenticationEventArgs.NumberOrPasskey"/>
        /// formatted in its six decimal digits format,
        /// or <see langword="null"/> if 
        /// <see cref="P:InTheHand.Net.Bluetooth.BluetoothWin32AuthenticationEventArgs.NumberOrPasskey"/>
        /// is <see langword="null"/>.
        /// </value>
        public string NumberOrPasskeyAsString
        {
            get
            {
                if (NumberOrPasskey.HasValue) {
                    // Bluetooth Secure Simple Pairing User Interface Flow Whitepaper.pdf
                    // "[...] The Usability Expert Group
                    // recommends that Bluetooth Secure Simple Pairing with any association model that uses 6 digits be split
                    // into two groups of three separated by a single space when displayed to help minimize the usability
                    // impacts caused by the numeric entry length."
#if DEBUG
                    var zerosThrees = 0.ToString(SixDigitsFormatString);
                    Debug.Assert(zerosThrees.Length == 6);
#endif
                    var twoThrees = NumberOrPasskey.Value
                        .ToString(SixDigitsFormatString).Insert(3, " ");
                    Debug.Assert(twoThrees.Length == 7);
                    return twoThrees;
                } else
                    return null;
            }
        }

        /// <summary>
        /// Gets or sets the PIN string to be used to authenticate the specified device.
        /// </summary>
        /// -
        /// <remarks>
        /// <para>Is only used in the 
        /// <see cref="F:InTheHand.Net.Bluetooth.BluetoothAuthenticationMethod.Legacy"/>
        /// pairing method.
        /// </para>
        /// <para>On an authentication event, a PIN response is sent if the value 
        /// returned from the handler is not <see langword="null"/>.
        /// </para>
        /// </remarks>
        public string Pin
        {
            get { return m_pin; }
            set
            {
                // ????
                //if (CannotSendAnotherResponse_) {
                //    throw new InvalidOperationException();
                //}
                m_pin = value;
            }
        }

        /// <summary>
        /// Get or set whether we will respond positively, negatively or
        /// ignore the SSP pairing event.
        /// </summary>
        public bool? Confirm
        {
            get { return _confirmSsp; }
            set { _confirmSsp = value; }
        }

        /// <summary>
        /// Get or set what Numeric or Passcode value or whether no value
        /// will be used in responding to the SSP pairing event.
        /// </summary>
        /// -
        /// <remarks>
        /// <para>Is a number from 000000 to 999999, or null if not to be included
        /// in the response.
        /// </para>
        /// </remarks>
        public int? ResponseNumberOrPasskey
        {
            get { return _responseNumberOrPasskey; }
            set { _responseNumberOrPasskey = value; }
        }

        /// <summary>
        /// Creates a positive response to the
        /// <see cref="F:InTheHand.Net.Bluetooth.BluetoothAuthenticationMethod.OutOfBand"/>
        /// pairing event also providing optional security values.
        /// </summary>
        /// -
        /// <param name="c">An byte array of length 16 bytes, or null.
        /// A 128-bit cryptographic key used for two-way authentication.
        /// </param>
        /// <param name="r">An byte array of length 16 bytes, or null.
        /// A randomly generated number used for one-way authentication.
        /// If this number is not provided by the device initiating the OOB
        /// session, this value is 0.
        /// </param>
        public void ConfirmOob(byte[] c, byte[] r)
        {
            const string Must16bytes = "OOB C and R value must be length 16 (or not supplied).";
            if (!(c == null || c.Length == 16)) throw new ArgumentException(Must16bytes, "c");
            if (!(r == null || r.Length == 16)) throw new ArgumentException(Must16bytes, "r");
            //
            _oobC = c;
            _oobR = r;
            // Don't allow the user to make changes.
            Clone(ref _oobC);
            Clone(ref _oobR);
            //
            _confirmSsp = true;
        }

        private void Clone<T>(ref T[] arr) where T : struct
        {
            if (arr == null) return;
            arr = (T[])arr.Clone();
        }

        internal byte[] OobC { get { return _oobC; } }
        internal byte[] OobR { get { return _oobR; } }

        //****

        /// <summary>
        /// Gets or sets whether the callback is called again after the PIN response
        /// is sent.
        /// </summary>
        /// -
        /// <remarks><para>This is useful to see the error code returned by sending
        /// the PIN response. It can thus also be used to see the successful result 
        /// of sending the PIN response.  See the documentation on the 
        /// <see cref="T:InTheHand.Net.Bluetooth.BluetoothWin32Authentication"/> class.
        /// </para>
        /// </remarks>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Callback")]
        public bool CallbackWithResult
        {
            get { return m_callbackWithResult; }
            set { m_callbackWithResult = value; }
        }

        /// <summary>
        /// Gets how many attempts at sending a PIN have been tried.
        /// </summary>
        /// <remarks>
        /// When there&#x2019;s a new PIN request, the first time the callback is
        /// called this property will have value zero.  If the PIN is rejected and
        /// <see cref="P:InTheHand.Net.Bluetooth.BluetoothWin32AuthenticationEventArgs.CallbackWithResult"/>
        /// was set, then the callback will be recalled and this property will have
        /// value one, etc.
        /// </remarks>
        public int AttemptNumber { get { return m_attemptNumber; } }

        /// <summary>
        /// The Windows error code returned by the last PIN response attempt.
        /// </summary>
        /// -
        /// <remarks><para>A bad PIN/passcode value appears to result in a error code
        /// with value 1244, which is <see cref="F:InTheHand.Net.Bluetooth.BluetoothWin32Authentication.NativeErrorNotAuthenticated"/>.
        /// </para>
        /// <para>If one tries to respond to that failure with another passcode,
        /// then error 1167 <see cref="F:InTheHand.Net.Bluetooth.BluetoothWin32Authentication.NativeErrorDeviceNotConnected"/>
        /// results.  So it seems that only one attempt is possible.
        /// </para>
        /// </remarks>
        public int PreviousNativeErrorCode { get { return m_errorCode; } }

        /// <summary>
        /// The Windows error code returned by the last PIN response attempt,
        /// as an unsigned value.
        /// </summary>
        /// -
        /// <remarks>See <see cref="P:InTheHand.Net.Bluetooth.BluetoothWin32AuthenticationEventArgs.PreviousNativeErrorCode"/>.
        /// </remarks>
        /// -
        /// <seealso cref="P:InTheHand.Net.Bluetooth.BluetoothWin32AuthenticationEventArgs.PreviousNativeErrorCode"/>
        [CLSCompliant(false)] // -> PreviousErrorCode
        [SuppressMessage("Microsoft.Naming", "CA1720:IdentifiersShouldNotContainTypeNames", MessageId = "unsigned")]
        public uint PreviousNativeErrorCodeAsUnsigned
        {
            get { return unchecked((uint)m_errorCode); }
        }

        /// <summary>
        /// Gets whether it is not possible to send another PIN response.
        /// </summary>
        /// <remarks><para>For instance, in testing it appears that after one response
        /// the device becomes non-contactable, any PIN response returning error code
        /// <see cref="F:InTheHand.Net.Bluetooth.BluetoothWin32Authentication.NativeErrorDeviceNotConnected"/>.
        /// </para>
        /// </remarks>
        public bool CannotSendAnotherResponse
        {
            get
            {
                return m_errorCode == BluetoothWin32Authentication.NativeErrorDeviceNotConnected;
            }
        }

        /// <summary>
        /// A format string to display the Passkey or comparison Number as six decimal digits.
        /// </summary>
        public const string SixDigitsFormatString = "D6";

        /// <exclude/>
        private const string ErrorMessageSendingAnotherPinIsDisallowed_
            = "It is disallowed to send another PIN response in this case.";

    }//class
}
