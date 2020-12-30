// 32feet.NET - Personal Area Networking for .NET
//
// InTheHand.Net.StonestreetOne.BluetopiaPlaying.cs
// 
// Copyright (c) 2011 In The Hand Ltd, All rights reserved.
// Copyright (c) 2011 Alan J. McFarlane, All rights reserved.
// This source code is licensed under the In The Hand Community License - see License.txt

using System;
using System.Collections.Generic;
using System.Text;
using InTheHand.Net.Bluetooth.Factory;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace InTheHand.Net.Bluetooth.StonestreetOne
{
#pragma warning disable 1591 // Missing XmlDocs
    public class BluetopiaPlaying
    {
        readonly BluetopiaFactory _fcty;
        ushort? _hConn;
        readonly NativeMethods.HCI_Event_Callback_t _hciCb;


        public BluetopiaPlaying()
        {
            _hciCb = FnCallback;
            _fcty = BluetoothFactory.GetTheFactoryOfTypeOrDefault<BluetopiaFactory>();
            if (_fcty == null)
                throw new InvalidOperationException("No Bluetopia stack.");
            var ret = NativeMethods.HCI_Register_Event_Callback(_fcty.StackId,
                _hciCb, 0);
            BluetopiaUtils.CheckAndThrow(ret, "HCI_Register_Event_Callback");
        }

        void FnCallback(uint BluetoothStackID,
            ref Structs.HCI_Event_Data_t hciEventData, uint CallbackParameter)
        {
            try {
                FnCallback2(BluetoothStackID,
                    ref hciEventData, CallbackParameter);
            } catch (Exception ex) {
                Utils.MiscUtils.Trace_WriteLine("Exception from our HCI_ _Event_Callback!!!: " + ex);
            }
        }
        void FnCallback2(uint BluetoothStackID,
            ref Structs.HCI_Event_Data_t hciEventData, uint CallbackParameter)
        {
            Debug.WriteLine("HCI Callback: " + hciEventData.Event_Data_Type);
            if (hciEventData.Event_Data_Type == StackConsts.HCI_Event_Type_t.etConnection_Complete_Event) {
                var data = (Structs.HCI_Connection_Complete_Event_Data_t)Marshal.PtrToStructure(
                    hciEventData.pData, typeof(Structs.HCI_Connection_Complete_Event_Data_t));
                if (data.Link_Type == StackConsts.HCI_LINK_TYPE.ACL_CONNECTION) {
                    _hConn = data.Connection_Handle;
                }
            } else if (hciEventData.Event_Data_Type == StackConsts.HCI_Event_Type_t.etVendor_Specific_Debug_Event) {
                var data = new byte[hciEventData.Event_Data_Size];
                var pData = hciEventData.pData;
                Marshal.Copy(pData, data, 0, data.Length);
                Debug.WriteLine("vendor event data: " + BitConverter.ToString(data, 0));
            }
        }

        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Hci")]
        public void HciCreateConnection(BluetoothAddress address)
        {
            byte psrm = 0; byte psm = 0; ushort co = 0;
            byte ars = 1;
            ushort pt = 0xFFFF;
            //
            StackConsts.HCI_ERROR_CODE statusCode;
            var ret = NativeMethods.HCI_Create_Connection(_fcty.StackId,
                BluetopiaUtils.BluetoothAddressAsInteger(address), pt,
                psrm, psm, co, ars, out statusCode);
            BluetopiaUtils.CheckAndThrow(ret, "HCI_Create_Connection");
            Debug.WriteLine("HCI_Create_Connection status: " + statusCode);
        }

        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Hci")]
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Sco")]
        public void HciAddScoConnection()
        {
            var pt = StackConsts.HCI_PACKET_SCO_TYPE__AllThree;
            //
            StackConsts.HCI_ERROR_CODE statusCode;
            var ret = NativeMethods.HCI_Add_SCO_Connection(_fcty.StackId,
                _hConn.Value, pt, out statusCode);
            BluetopiaUtils.CheckAndThrow(ret, "HCI_Add_SCO_Connection");
            Debug.WriteLine("HCI_Add_SCO_Connection status: " + statusCode);
        }

        //----
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Hci")]
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Ocf")]
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Ogf")]
        [CLSCompliant(false)]
        public int HciSendRawCommand(
            Byte commandOgf, ushort commandOcf, Byte commandLength, Byte[] commandData)
        {
            Byte statusResult, lengthResult;
            var bufferResult = new byte[2000];
            Boolean waitForResponse = false;
            //
            // It doesn't appear that this parameter is an IN/OUT param but set
            // it anyway.  Presumably the library assumes that it's at least
            // 255 bytes long.
            // For the test we've done -- CSR BCCMD -- we see no output --
            // because they don't use HCI CommandComplete.
            lengthResult = 255;
            var ret = NativeMethods.HCI_Send_Raw_Command(_fcty.StackId,
                commandOgf, commandOcf, commandLength, commandData,
                out statusResult, ref lengthResult, bufferResult,
                waitForResponse);
            return (int)ret;
        }

    }
}
