// 32feet.NET - Personal Area Networking for .NET
//
// Net.Bluetooth.StonestreetOne.BluetopiaUtils
// 
// Copyright (c) 2010 Alan J.McFarlane, All rights reserved.
// Copyright (c) 2010 In The Hand Ltd, All rights reserved.
// This source code is licensed under the In The Hand Community License - see License.txt

using System;
using System.Diagnostics;
using System.Net.Sockets;
using System.Text;

using InTheHand.Net.Sockets;
using System.Globalization;

namespace InTheHand.Net.Bluetooth.StonestreetOne
{
    static class BluetopiaUtils
    {
        #region Return code handling
        internal static bool IsSuccess(BluetopiaError ret)
        {
            return (ret >= 0);
        }

        internal static bool IsSuccessZeroIsIllegal(BluetopiaError ret)
        {
            return (ret > 0);
        }

        [DebuggerNonUserCode]
        internal static void Assert(BluetopiaError ret, string descr)
        {
            Debug.Assert(IsSuccess(ret), "Bluetopia error: "
                + ErrorToString(ret)
                + ", at: " + descr);
        }

        [DebuggerNonUserCode]
        internal static void WriteLineIfError(BluetopiaError ret, string descr)
        {
            if (!IsSuccess(ret))
                Utils.MiscUtils.Trace_WriteLine("Bluetopia error: "
                    + ErrorToString(ret)
                    + ", at: " + descr);
        }

        internal static string ErrorToString(BluetopiaError ret)
        {
            return ret + "=(" + ((int)ret).ToString(CultureInfo.InvariantCulture) + ")";
        }

        [DebuggerNonUserCode]
        internal static void CheckAndThrow(BluetopiaError ret, string descr)
        {
            if (ret >= 0)
                return;
            Throw(ret, descr);
        }
        [DebuggerNonUserCode]
        internal static void CheckAndThrowZeroIsIllegal(BluetopiaError ret, string descr)
        {
            Debug.Assert(ret != 0 || TestUtilities.IsUnderTestHarness(), "Zero is also invalid per the docs.");
            if (ret > 0)
                return;
            Throw(ret, descr);
        }
        [DebuggerNonUserCode]
        internal static void Throw(BluetopiaError ret, string descr)
        {
            //default to NotSocket unless match specific error below
            SocketError err = SocketError.NotSocket;

            switch (ret) {
                // TODO ! Match BlueSoleil error codes to SocketExceptions.
                // Note need different errors for GetServiceRecords, handle here or in GSR itself?
                case BluetopiaError.INVALID_PARAMETER:
                case BluetopiaError.INVALID_BLUETOOTH_STACK_ID:
                    err = SocketError.InvalidArgument;
                    break;
                case BluetopiaError.DLL_INITIALIZATION_ERROR:
                case BluetopiaError.HCI_INITIALIZATION_ERROR:
                case BluetopiaError.GAP_INITIALIZATION_ERROR:
                case BluetopiaError.SCO_INITIALIZATION_ERROR:
                case BluetopiaError.L2CAP_INITIALIZATION_ERROR:
                case BluetopiaError.RFCOMM_INITIALIZATION_ERROR:
                case BluetopiaError.SDP_INITIALIZATION_ERROR:
                case BluetopiaError.SPP_INITIALIZATION_ERROR:
                case BluetopiaError.GOEP_INITIALIZATION_ERROR:
                case BluetopiaError.OTP_INITIALIZATION_ERROR:
                    err = SocketError.SystemNotReady;
                    break;
                case BluetopiaError.UNSUPPORTED_HCI_VERSION:
                    err = SocketError.VersionNotSupported;
                    break;
                case BluetopiaError.ATTEMPTING_CONNECTION_TO_DEVICE:
                    err = SocketError.ConnectionRefused;
                    break;
                case BluetopiaError.INVALID_CONNECTION_STATE:
                    err = SocketError.NotConnected;
                    break;
                case BluetopiaError.CONNECTION_TO_DEVICE_LOST:
                    err = SocketError.ConnectionAborted;
                    break;
                case BluetopiaError.INTERNAL_ERROR:
                    err = SocketError.Fault;
                    break;
                case BluetopiaError.INSUFFICIENT_BUFFER_SPACE:
                case BluetopiaError.SPP_BUFFER_FULL:
                    err = SocketError.NoBufferSpaceAvailable;
                    break;
                case BluetopiaError.UNSUPPORTED_PLATFORM_ERROR:
                    err = SocketError.OperationNotSupported;
                    break;
                case BluetopiaError.RFCOMM_UNABLE_TO_ADD_CONNECTION_INFORMATION:
                    err = SocketError.AddressAlreadyInUse;
                    break;
            }

            Debug.WriteLine("Bluetopia Throw " + ret + ", at: " + descr
                + ", (" + err + ")");
            throw new BluetopiaSocketException(ret, (int)err);
        }
        #endregion

        //--
        #region Address Conversion
        [DebuggerStepThrough]
        internal static BluetoothAddress ToBluetoothAddress(byte[] address_)
        {
            return BluetoothAddress.CreateFromLittleEndian(address_);
        }

        [DebuggerStepThrough]
        internal static long BluetoothAddressAsInteger(BluetoothAddress address)
        {
            var intStruct = address.ToInt64();
#if DEBUG
            //Debug.Assert(i[6] == 0);
            //Debug.Assert(i[7] == 0);
            var intAsBytes = BitConverter.GetBytes(intStruct);
            var asBytes = address.ToByteArrayLittleEndian();
            Debug.Assert(intAsBytes[6] == 0);
            Debug.Assert(intAsBytes[7] == 0);
            for (int i = 0; i < 6; ++i) {
                Debug.Assert(intAsBytes[i] == asBytes[i]);
            }
            //Debug.Assert(i[5] == address.ToByteArrayBigEndian()[0]);
#endif
            return intStruct;
        }
        #endregion

        //--
        #region Name conversion
        internal static string FromNameString(byte[] arr, int? bufferLen)
        {
            int idx = Array.IndexOf<byte>(arr, 0);
            int len = idx != -1 ? idx : arr.Length;
            Debug.Assert(!bufferLen.HasValue || bufferLen.Value <= arr.Length,
                "bufferLen :" + bufferLen + ", arr.Length: " + arr.Length);
            if (bufferLen.HasValue && bufferLen.Value < len) {
                len = bufferLen.Value;
            }
            Debug.Assert(len <= arr.Length, "len: " + len + ", arr.Length: " + arr.Length);
            var txt = Encoding.UTF8.GetString(arr, 0, len);
            return txt;
        }

        internal static string FromNameString(byte[] arr)
        {
            return FromNameString(arr, null);
        }
        #endregion

        //----
        #region CoD conversion
        internal static ClassOfDevice ToClassOfDevice(byte[] p)
        {
            uint codI = (uint)p[0] + 256 * ((uint)p[1] + 256 * (uint)p[2]);
            var cod = new ClassOfDevice(codI);
            return cod;
        }
        #endregion

    }
}
