// 32feet.NET - Personal Area Networking for .NET
//
// Net.Bluetooth.Factory.CommonBluetoothInquiry
// 
// Copyright (c) 2008-2010 Alan J.McFarlane, All rights reserved.
// Copyright (c) 2008-2010 In The Hand Ltd, All rights reserved.
// This source code is licensed under the MIT License

using AsyncResultInquiry = InTheHand.Net.AsyncResult<System.Collections.Generic.List<System.UInt32>>;
using AR_Inquiry = InTheHand.Net.AsyncResult<System.Collections.Generic.List<InTheHand.Net.Bluetooth.Factory.IBluetoothDeviceInfo>,
    InTheHand.Net.Bluetooth.Factory.DiscoDevsParams>;
using List_IBluetoothDeviceInfo = System.Collections.Generic.List<InTheHand.Net.Bluetooth.Factory.IBluetoothDeviceInfo>;
//
using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics.CodeAnalysis;
using InTheHand.Net.Sockets;
using System.Threading;
using System.Diagnostics;

namespace InTheHand.Net.Bluetooth.Factory
{
    /// <exclude/>
    public abstract class CommonBluetoothInquiry<TInquiryEventItemType>
    {
        // !!!
#pragma warning disable 1591 // Missing XML comment for publicly visible type or member '
        //
        readonly object _lockInquiry = new object();
        List<IBluetoothDeviceInfo> _inquiryDevices;
        AsyncResult<List<IBluetoothDeviceInfo>, DiscoDevsParams> _inquiryAr;
        List<AR_Inquiry> _arInquiryFollowers;
        BluetoothClient.LiveDiscoveryCallback _liveDiscoHandler;
        object _liveDiscoState;

        //----
        protected CommonBluetoothInquiry()
        {
        }

        //----
        protected abstract IBluetoothDeviceInfo CreateDeviceInfo(TInquiryEventItemType item);

        protected virtual IBluetoothDeviceInfo CreateDeviceInfoFromManualNameLookup(IBluetoothDeviceInfo previous, string name)
        {
            throw new NotImplementedException();
        }

        //----
#if false // NOT tested
        internal void GotNameManually(BluetoothAddress addr, string name)
        {
            IBluetoothDeviceInfo prevDev;
            lock (_lockInquiry) {
                if (_inquiryDevices == null) {
                    return;
                }
                prevDev = _inquiryDevices.FindLast(x => x.DeviceAddress == addr);
                if (prevDev == null) {
                    // Device found by current inquiry.
                    return;
                }
            }//lock
            Debug.Assert(prevDev != null, "return above!");
            // Don't want to call stack code from the lock.
            var newDev = CreateDeviceInfoFromManualNameLookup(prevDev, name);
            lock (_lockInquiry) {
                var prevDev2 = _inquiryDevices.FindLast(x => x.DeviceAddress == addr);
                if (prevDev2 == prevDev) {
                    BluetoothDeviceInfo.AddUniqueDevice(_inquiryDevices, newDev);
                }
            }//lock
        }
#endif

        //----
        public void HandleInquiryResultInd(TInquiryEventItemType item)
        {
            IBluetoothDeviceInfo bdi = CreateDeviceInfo(item);
            BluetoothClient.LiveDiscoveryCallback liveDiscoHandler;
            object liveDiscoState;
            lock (_lockInquiry) {
                if (_inquiryDevices == null) {
                    //Debug.Assert(TestUtilities.IsUnderTestHarness(), "HandleDeviceResponded without DD i.e. m_inquiryDevices == null.");
                    return;
                }
                liveDiscoHandler = _liveDiscoHandler;
                liveDiscoState = _liveDiscoState;
                //
                DateTime t0 = _inquiryAr.BeginParameters.discoTime;
                bdi.SetDiscoveryTime(t0);
                //
                BluetoothDeviceInfo.AddUniqueDevice(_inquiryDevices, bdi);
            }
            if (liveDiscoHandler != null) {
                WaitCallback dlgt = delegate {
                    OnDeviceResponded(liveDiscoHandler, bdi, liveDiscoState);
                };
                ThreadPool.QueueUserWorkItem(dlgt);
            }
        }

        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic")]
        private void OnDeviceResponded(BluetoothClient.LiveDiscoveryCallback liveDiscoHandler,
            IBluetoothDeviceInfo bdi, object liveDiscoState)
        {
            liveDiscoHandler(bdi, liveDiscoState);
        }

        public void HandleInquiryComplete(int? reportedNumResponses)
        {
            Debug.WriteLine("CBI.HandleInquiryComplete");
            AR_Inquiry ar;
            List<AR_Inquiry> sacArFollowers = null;
            List_IBluetoothDeviceInfo deviceList;
            lock (_lockInquiry) {
                StopInquiry();
                ar = _inquiryAr;
                deviceList = _inquiryDevices;
                //Debug.Assert(ar != null || TestUtilities.IsUnderTestHarness(), "Inquiry_Complete but no outstanding operation (ar).");
                _inquiryAr = null;
                _liveDiscoHandler = null;
                _liveDiscoState = null;
                if (_arInquiryFollowers != null) {
                    sacArFollowers = _arInquiryFollowers;
                    _arInquiryFollowers = null;
                }
            }//lock
#if DEBUG
            if (deviceList != null && reportedNumResponses != null) {
                int countCollected = deviceList.Count;
                Debug.Assert(reportedNumResponses == countCollected,
                    "reportedNumResponses: " + reportedNumResponses + " NOT == countCollected: " + countCollected);
            }
#endif
            WaitCallback dlgt = delegate {
                OnInquiryComplete(ar, deviceList, sacArFollowers);
            };
            ThreadPool.QueueUserWorkItem(dlgt);
        }

        protected virtual void StopInquiry()
        {
            // 
            // Not allowed to call from the callback thread, unneccessary
            // anyway apparently- BtSdkError ret = NativeMethods.Btsdk_StopDeviceDiscovery();
        }

        static void OnInquiryComplete(AR_Inquiry sacAr,
            List_IBluetoothDeviceInfo sacResult, List<AR_Inquiry> sacArFollowers)
        {
            if (sacAr != null) {
                sacAr.SetAsCompleted(sacResult, false);
                if (sacArFollowers != null) {
                    foreach (AR_Inquiry ar in sacArFollowers)
                        ar.SetAsCompleted(sacResult, false);
                }
            }
        }

        //----
        public IAsyncResult BeginInquiry(int maxDevices, TimeSpan inquiryLength,
            AsyncCallback asyncCallback, Object state,
            BluetoothClient.LiveDiscoveryCallback liveDiscoHandler, object liveDiscoState,
            ThreadStart startInquiry,
            DiscoDevsParams args)
        {
            int fakeUseI = maxDevices;
            AR_Inquiry ar;
            AR_Inquiry sacAr = null;
            List_IBluetoothDeviceInfo sacResult = null;
            lock (_lockInquiry) {
                if (_inquiryAr != null) {
                    Debug.WriteLine("Merging concurrent DiscoverDevices call into the running one (will get same result list).");
                    // Just give any new request the same results as the outstanding Inquiry.
                    ar = new AR_Inquiry(asyncCallback, state, args);
                    if (_inquiryAr.IsCompleted) {
                        // This can never occur (is nulled before SAC'd), but leave in anyway...
                        sacAr = ar;
                        sacResult = _inquiryDevices;
                    } else {
                        if (_arInquiryFollowers == null)
                            _arInquiryFollowers = new List<AR_Inquiry>();
                        _arInquiryFollowers.Add(ar);
                    }
                } else { // New inquiry process.
                    ar = new AR_Inquiry(asyncCallback, state, args);
                    _inquiryAr = ar;
                    _arInquiryFollowers = null;
                    _inquiryDevices = new List_IBluetoothDeviceInfo();
                    _liveDiscoHandler = liveDiscoHandler;
                    _liveDiscoState = liveDiscoState;
                    bool siSuccess = false;
                    try {
                        startInquiry();
                        siSuccess = true;
                    } finally {
                        if (!siSuccess) { _inquiryAr = null; }
                    }
                    if (inquiryLength.CompareTo(TimeSpan.Zero) > 0) {
                        var tTmp = TimeSpan.FromMilliseconds(
                            checked(1.5 * inquiryLength.TotalMilliseconds));
                        System.Threading.ThreadPool.QueueUserWorkItem(InquiryTimeout_Runner,
                            new InquiryTimeoutParams(ar, tTmp));
                    }
                }
            }//lock
            if (sacAr != null) {
                sacAr.SetAsCompleted(sacResult, true);
            }
            return ar;
        }

        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic")]
        public List<IBluetoothDeviceInfo> EndInquiry(IAsyncResult ar)
        {
            var arI = (AsyncResult<List<IBluetoothDeviceInfo>>)ar;
            List<IBluetoothDeviceInfo> handles = arI.EndInvoke();
            return handles;
        }

        //----
        struct InquiryTimeoutParams
        {
            internal readonly IAsyncResult _ar;
            internal readonly TimeSpan _InquiryLength;

            public InquiryTimeoutParams(AR_Inquiry ar, TimeSpan inquiryLength)
            {
                _ar = ar;
                _InquiryLength = inquiryLength;
            }

            /// <summary>
            /// Get timeout value in Int32 milliseconds,
            /// as NETCF <c>WaitHandle.WaitOne</c> can't use TimeSpan.
            /// </summary>
            /// -
            /// <returns>An Int32 containing the timeout value in milliseconds.
            /// </returns>
            internal int InquiryLengthAsMiliseconds()
            {
                double ms0 = this._InquiryLength.TotalMilliseconds;
                int ms = checked((int)ms0);
                return ms;
            }
        }

        void InquiryTimeout_Runner(object state)
        {
            InquiryTimeoutParams args = (InquiryTimeoutParams)state;
            bool completed = args._ar.AsyncWaitHandle.WaitOne(args.InquiryLengthAsMiliseconds(), false);
            if (completed)
                return;
            // TODO ? InquiryTimeout_Runner: If power resumed then restart the timeout
            lock (_lockInquiry) {
                if (args._ar.IsCompleted) // It won the race to enter the lock.
                    return;
                object arDD = _inquiryAr;
                // TO-DO What if its in the 'followers' list.
                if (arDD == null) // etc
                    return;
                if (arDD != args._ar)
                    return;
                Utils.MiscUtils.Trace_WriteLine("Cancelling Inquiry due to timeout.");
                HandleInquiryComplete(null);
                Debug.Assert(_inquiryAr == null, "NOT m_arInquiry==null after (timed-out) completion.");
            }
        }

    }//class
}
