// 32feet.NET - Personal Area Networking for .NET
//
// InTheHand.Net.Bluetooth.StoneStreetOne.BluetopiaSecurity
// 
// Copyright (c) 2009-2011 In The Hand Ltd, All rights reserved.
// This source code is licensed under the In The Hand Community License - see License.txt

using System;

using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using InTheHand.Net.Bluetooth.Factory;
using System.Runtime.InteropServices;
using System.Threading;
using System.Diagnostics.CodeAnalysis;

namespace InTheHand.Net.Bluetooth.StonestreetOne
{
    sealed class BluetopiaSecurity : IBluetopiaSecurity
    {
        //--
        readonly BluetopiaFactory _factory;
        readonly NativeMethods.GAP_Event_Callback _AuthenticateCallback;
        readonly Dictionary<BluetoothAddress, PinPairItem> _pins = new Dictionary<BluetoothAddress, PinPairItem>();
        readonly Dictionary<BluetoothAddress, byte[]> _keys = new Dictionary<BluetoothAddress, byte[]>();

        //--
        internal BluetopiaSecurity(BluetopiaFactory factory)
        {
            Debug.Assert(factory != null);
            _factory = factory;
            _AuthenticateCallback = new NativeMethods.GAP_Event_Callback(HandleAuthenticate_Callback);
        }

        void IBluetopiaSecurity.InitStack()
        {
            var ret = _factory.Api.GAP_Set_Pairability_Mode(_factory.StackId,
                StackConsts.GAP_Pairability_Mode.PairableMode);
            BluetopiaUtils.CheckAndThrow(ret, "GAP_Set_Pairability_Mode");
            ret = _factory.Api.GAP_Register_Remote_Authentication(
                _factory.StackId, _AuthenticateCallback, 0);
            BluetopiaUtils.CheckAndThrow(ret, "GAP_Register_Remote_Authentication");
        }

        //--
        /// <summary>
        /// Adds the address+pin record.
        /// </summary>
        /// <remarks>Only throws with bad pin.
        /// </remarks>
        /// <param name="device">device</param>
        /// <param name="pin">PIN, must less that 16-byte UTF-8.
        /// What about null PIN?????
        /// </param>
        /// <returns>The newly added item</returns>
        PinPairItem SetPinPairItem_willLock(BluetoothAddress device, string pin)
        {
            byte[] pinBytes = Structs.GAP_Authentication_Information
                .PinToByteArray(pin);
            //
            PinPairItem item;
            lock (_pins) {
                if (!_pins.TryGetValue(device, out item)) {
                    _pins.Add(device, new PinPairItem());
                    item = _pins[device];
                }
                // Set the new pin. May overwrite the old pin!
                item._pin = pinBytes;
            }
            return item;
        }

        PinPairItem GetPinPairItem_willLock(BluetoothAddress device)
        {
            lock (_pins) {
                PinPairItem item;
                if (_pins.TryGetValue(device, out item)) {
                    return item;
                } else {
                    return null;
                }
            }//lock
        }

        /// <summary>
        /// Removes the item.
        /// </summary>
        /// <remarks>We check only the <paramref name="device"/> and <paramref name="waitComplete"/>.
        /// </remarks>
        /// <param name="device"></param>
        /// <param name="pin"></param>
        /// <param name="waitComplete"></param>
        /// <returns><c>null</c> if not present or someone has overwritten it</returns>
        PinPairItem RemovePinPairItem__mustByInLock(BluetoothAddress device, string pin, ManualResetEvent waitComplete)
        {
            PinPairItem item;
            if (!_pins.TryGetValue(device, out item)) {
                return null;
            }
            if (item._eventForPairRequest == waitComplete) {
                _pins.Remove(device);
                return item;
            } else {
                return null;
            }
        }

        private void AddOrUpdateLinkKey_willLock(BluetoothAddress device, byte[] key)
        {
            lock (_pins) {
                bool was = _keys.Remove(device);
                _keys.Add(device, (byte[])key.Clone());
            }
        }


        //--
        #region IBluetoothSecurity Members

        public bool PairRequest(BluetoothAddress device, string pin)
        {
            // Verify and store the pin
            // TODO null PIN
            PinPairItem pairItem = SetPinPairItem_willLock(device, pin);
            // Add event etc.
            ManualResetEvent waitComplete;
            lock (_pins) {
                if (pairItem._eventForPairRequest != null) {
                    // HACK handle overlapping requests.  Fail the former?
                    pairItem.success = false;
                    pairItem._eventForPairRequest.Set();
                }
                waitComplete = new ManualResetEvent(false);
                pairItem._eventForPairRequest = waitComplete;
            }
            // Run
            BluetopiaError ret;
            var foo = false;
            if (foo) {
                ret = _factory.Api.GAP_Authenticate_Remote_Device(
                    _factory.StackId, BluetopiaUtils.BluetoothAddressAsInteger(device), _AuthenticateCallback, 0);
            } else {
                ret = _factory.Api.GAP_Initiate_Bonding(_factory.StackId,
                    BluetopiaUtils.BluetoothAddressAsInteger(device),
                    StackConsts.GAP_Bonding_Type.Dedicated,
                    _AuthenticateCallback, 0);
            }
            if (!BluetopiaUtils.IsSuccess(ret)) {
                return false;
            }
            // Need to wait for completion.
            const int timeout = 3 * 60 * 1000;
            waitComplete.WaitOne(timeout, false);
            lock (_pins) {
                RemovePinPairItem__mustByInLock(device, pin, waitComplete);
                return pairItem.success == true;
            }
        }

        public bool RemoveDevice(BluetoothAddress device)
        {
            throw new NotSupportedException("No API! Just delete in Registry??");
        }

        bool IBluetoothSecurity.SetPin(BluetoothAddress device, string pin)
        {
            var item = SetPinPairItem_willLock(device, pin);
            Debug.Assert(item != null, "what happened!!");
            return true;
        }

        bool IBluetoothSecurity.RevokePin(BluetoothAddress device)
        {
            lock (_pins) {
                var item = RemovePinPairItem__mustByInLock(device, null, null);
                return (item != null);
            }
        }

        BluetoothAddress IBluetoothSecurity.GetPinRequest()
        {
            throw new NotImplementedException();
        }

        bool IBluetoothSecurity.RefusePinRequest(BluetoothAddress device)
        {
            throw new NotImplementedException();
        }

        bool IBluetoothSecurity.SetLinkKey(BluetoothAddress device, Guid linkKey)
        {
            // Could be supported, but would anyone use it...
            throw new NotImplementedException();
        }

        #endregion

        #region Auth Callback Handler
        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes",
            Justification = "Mustn't let exception return to stack.")]
        private void HandleAuthenticate_Callback(uint BluetoothStackID,
            ref Structs.GAP_Event_Data GAP_Event_Data, uint CallbackParameter)
        {
            // Don't allow an exception to travel back to the BTPS stack!
            try {
                HandleAuthenticate_Callback2(BluetoothStackID,
                    ref GAP_Event_Data, CallbackParameter);
            } catch (Exception ex) {
                Utils.MiscUtils.Trace_WriteLine("Exception in our HandleAuthenticate_Callback2: " + ex);
            }
        }
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "BluetoothStackID")]
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "CallbackParameter")]
        private void HandleAuthenticate_Callback2(uint BluetoothStackID,
            ref Structs.GAP_Event_Data GAP_Event_Data, uint CallbackParameter)
        {
            //Debug.WriteLine("Authenticate_Callback Event_Data_Type: " + GAP_Event_Data.Event_Data_Type);
            Debug.Assert(GAP_Event_Data.Event_Data_Type == StackConsts.GAP_Event_Type.Authentication,
                "Unexpected Authenticate_Callback Event_Data_Type: " + GAP_Event_Data.Event_Data_Type);
            if (GAP_Event_Data.Event_Data_Type == StackConsts.GAP_Event_Type.Authentication) {
                var data = (Structs.GAP_Authentication_Event_Data__Status)
                    Marshal.PtrToStructure(GAP_Event_Data.pData,
                        typeof(Structs.GAP_Authentication_Event_Data__Status));
                var addr8 = new byte[8];
                data._Remote_Device.CopyTo(addr8, 0);
                var addrI = BitConverter.ToInt64(addr8, 0);
                var addr = BluetopiaUtils.ToBluetoothAddress(addr8);
#if DEBUG
                var addrI2 = BluetopiaUtils.BluetoothAddressAsInteger(addr);
                Trace.Assert(addrI == addrI2, "addrI: + " + addrI + " != addrI2: " + addrI2);
#endif
                Debug.WriteLine("Authenticate_Callback: type: " + data._GAP_Authentication_Event_Type
                    + ", addr: " + addr.ToString());
                if (data._GAP_Authentication_Event_Type == StackConsts.GAP_Authentication_Event_Type
                        .AuthenticationStatus) {
                    Debug.WriteLine("  Status: " + data.GetAuthenticationStatus(_factory.ApiVersion) + ")");
                }
                //
                PinPairItem ppItem;
                byte[] key = null;
                ppItem = GetPinPairItem_willLock(addr); // Got Pin?
                // Use LinkKey if not PairRequest active.
                if (ppItem == null || ppItem._eventForPairRequest == null) {
                    lock (_pins) { // Got LinkKey?
                        var got = _keys.TryGetValue(addr, out key);
                        Debug.Assert(!got || (key != null));
                    }
                }
                if (ppItem == null && key == null) {
                    Debug.WriteLine("  No Pin or LinkKey for that device, exiting.");
                    return;
                }
                Debug.WriteLine("  Have Pin: " + (ppItem != null) + ", LinkKey: " + (key != null));
                //
                BluetopiaError ret;
                switch (data._GAP_Authentication_Event_Type) {
                    case StackConsts.GAP_Authentication_Event_Type.LinkKeyRequest:
                        if (key == null) {
                            ret = RespondWithNoLinkKey(addrI);
                        } else {
                            ret = RespondWithLinkKey(addrI, key);
                        }
                        break;
                    case StackConsts.GAP_Authentication_Event_Type.PINCodeRequest:
                        if (ppItem == null) {
                            break;
                        }
                        // Respond with Pin
                        Debug.Assert(ppItem != null, "Would have exited above if not a known device.");
                        var rspndrInfo = new ResponderInfo { AddrI = addrI, PPItem = ppItem };
                        ret = RespondWithPinCode(rspndrInfo);
                        break;
                    case StackConsts.GAP_Authentication_Event_Type.AuthenticationStatus:
                        if (ppItem == null) {
                            break;
                        }
                        // Success or Fail??
                        Debug.Assert(ppItem != null, "Would have exited above if not a known device.");
                        lock (_pins) {
                            ppItem.status = data.GetAuthenticationStatus(_factory.ApiVersion);
                            ppItem.success = (ppItem.status
                                == StackConsts.HCI_ERROR_CODE.NO_ERROR);
                            ppItem._eventForPairRequest.Set();
                        }
                        break;
                    case StackConsts.GAP_Authentication_Event_Type.LinkKeyCreation:
                        // Store the LinkKey for the next time we connect -- the stack doesn't!
                        var authInfoKey = (Structs.GAP_Authentication_Event_Data__LinkKey)
                            Marshal.PtrToStructure(GAP_Event_Data.pData,
                                typeof(Structs.GAP_Authentication_Event_Data__LinkKey));
                        AddOrUpdateLinkKey_willLock(addr, authInfoKey.GetLinkKey(_factory.ApiVersion));
                        break;
                }//switch
            }
        }

        private BluetopiaError RespondWithNoLinkKey(long addrI)
        {
            BluetopiaError ret;
            var rsp = new Structs.GAP_Authentication_Information(
                     StackConsts.GAP_Authentication_Type_t.atLinkKey);
            Debug.Assert(rsp._Authentication_Data_Length == 0, "!Null is zero len.");
            Debug.WriteLine("  Sending Auth Response: -ve (no linkkey)");
            ret = _factory.Api.GAP_Authentication_Response(
                _factory.StackId, addrI, ref rsp);
            Debug.Assert(BluetopiaUtils.IsSuccess(ret),
                "GAP_Authentication_Response Negative=Non-data ret: " + ret);
            return ret;
        }

        private BluetopiaError RespondWithLinkKey(long addrI, byte[] key)
        {
            BluetopiaError ret;
            var rsp = new Structs.GAP_Authentication_Information(
                     StackConsts.GAP_Authentication_Type_t.atLinkKey,
                     key, _factory.ApiVersion);
            Debug.Assert(rsp._Authentication_Data_Length != 0, "Zero len LinkKeys not allowed.");
            Debug.WriteLine("  Sending Auth Response: LinkKey)");
            Debug.WriteLine("    len: " + rsp._Authentication_Data_Length);
            ret = _factory.Api.GAP_Authentication_Response(
                _factory.StackId, addrI, ref rsp);
            Debug.Assert(BluetopiaUtils.IsSuccess(ret),
                "GAP_Authentication_Response Negative=Non-data ret: " + ret);
            return ret;
        }

        class ResponderInfo
        {
            public long AddrI { get; set; }
            public PinPairItem PPItem { get; set; }
        }

        //private void RespondWithPinCode_Runner(object state)
        //{
        //    RespondWithPinCode((ResponderInfo)state);
        //}

        private BluetopiaError RespondWithPinCode(ResponderInfo rspInfo)
        {
            BluetopiaError ret;
            var rsp = new Structs.GAP_Authentication_Information(
                     StackConsts.GAP_Authentication_Type_t.atPINCode,
                     rspInfo.PPItem._pin, _factory.ApiVersion);
            //, "Zero len Pins not allowed.
            Debug.WriteLine("  Sending Auth Response: PinCode");
            Debug.WriteLine("    " + rsp.DebugToString());
            ret = _factory.Api.GAP_Authentication_Response(
                _factory.StackId, rspInfo.AddrI, ref rsp);
            Debug.Assert(BluetopiaUtils.IsSuccess(ret),
                "GAP_Authentication_Response: " + ret);
            return ret;
        }
        #endregion

        private class PinPairItem
        {
            internal byte[] _pin;
            internal ManualResetEvent _eventForPairRequest;
            //
            internal bool? success;
            internal StackConsts.HCI_ERROR_CODE? status;
        }
    }
}
