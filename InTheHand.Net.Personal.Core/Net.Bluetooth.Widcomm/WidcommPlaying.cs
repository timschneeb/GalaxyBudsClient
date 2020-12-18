// 32feet.NET - Personal Area Networking for .NET
//
// InTheHand.Net.Widcomm.WidcommSocketExceptions
// 
// Copyright (c) 2009-2010 In The Hand Ltd, All rights reserved.
// Copyright (c) 2009-2010 Alan J. McFarlane, All rights reserved.
// This source code is licensed under the MIT License

using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using InTheHand.Net.Bluetooth.Factory;

namespace InTheHand.Net.Bluetooth.Widcomm
{

    /// <summary>
    /// "Define common return code for new SDK functions that would normally return BOOL"
    /// </summary>
    /// -
    /// <remarks>"Added BTW and SDK 5.0.1.1100".
    /// </remarks>
    public enum SDK_RETURN_CODE
    {
        /// <summary>
        /// "The call was successful"
        /// </summary>
        Success,
        /// <summary>
        /// "Unspecified failure"
        /// </summary>
        Fail,
        /// <summary>
        /// "The API is not supported on the platform BTW stack version"
        /// </summary>
        NotSupported,
        /// <summary>
        /// "The API cannot complete at this time, but may be retried"
        /// </summary>
        Busy,
        /// <summary>
        /// "One of the API parameters was invalid"
        /// </summary>
        InvalidParam,
        /// <summary>
        /// "A necessary resource could not be obtained"
        /// </summary>
        ErrResources,
        /// <summary>
        /// "The operation timed out before completion"
        /// </summary>
        Timeout
    };


#pragma warning disable 1591 // warning CS1591: Missing XML comment for publicly visible type or member ...

    public enum RemoteDeviceState
    {
        Unknown,
        NotPresent,
        Present,
        Connected
    }


    public class WidcommPlaying
    {
        WidcommBluetoothFactory m_factory;

        public WidcommPlaying()
        {
            var f2 = BluetoothFactory.GetTheFactoryOfTypeOrDefault<WidcommBluetoothFactory>();
            // TODO (Remove this manual way after some time)
            foreach (BluetoothFactory f in BluetoothFactory.Factories) {
                WidcommBluetoothFactory wf = f as WidcommBluetoothFactory;
                if (wf != null) {
                    m_factory = wf;
                    break;
                }
            }
            Trace.Assert(f2 == m_factory);
            Trace.Assert(object.ReferenceEquals(f2, m_factory));
            if (m_factory == null)
                throw new InvalidOperationException("Widcomm stack not present.");
        }

        internal WidcommPlaying(WidcommBluetoothFactory factory)
        {
            m_factory = factory;
        }

        //--
        public RemoteDeviceState FindIfPresentOrConnected(BluetoothAddress addr)
        {
            return FindIfPresentOrConnected(WidcommUtils.FromBluetoothAddress(addr));
        }

        public RemoteDeviceState FindIfPresentOrConnected(byte[] bda)
        {
            WidcommBtInterface iface = m_factory.GetWidcommBtInterface();
            int start;
            start = Environment.TickCount;
            Utils.MiscUtils.Trace_WriteLine("FiPoC: gonna IsRemoteDeviceConnected");
            bool connected = iface.IsRemoteDeviceConnected(bda);
            int tc = Environment.TickCount - start;
            //
            start = Environment.TickCount;
            //TO-DO If this chap is slow, when connected==true we could exit before calling it.
            //(...But it should be quick in that case!)
            Utils.MiscUtils.Trace_WriteLine("FiPoC: gonna IsRemoteDevicePresent");
            SDK_RETURN_CODE present0 = iface.IsRemoteDevicePresent(bda);
            bool present = (present0 == SDK_RETURN_CODE.Success);
            int tp = Environment.TickCount - start;
            //--
#if NETCF
#else
            Debug.Assert(present0 == SDK_RETURN_CODE.NotSupported);
            Debug.Assert(connected == false);
            const int ExpectedMilliseconds = 100;
            Debug.Assert(tp < ExpectedMilliseconds, "slow Is-Present: " + tp);
            Debug.Assert(tc < ExpectedMilliseconds, "slow Is-Connected: " + tc);
#endif
            //--
            RemoteDeviceState state;
            if (connected) {
                state = RemoteDeviceState.Connected;
#if NETCF
                Debug.Assert(present0 == SDK_RETURN_CODE.Success, "present0: " + present0);
#else
                // On BTW IsConnected was implemented before IsPresent.
                Debug.Assert(present0 == SDK_RETURN_CODE.NotSupported
                    || present0 == SDK_RETURN_CODE.Success, "present0: " + present0);
#endif
            } else if (present0 == SDK_RETURN_CODE.NotSupported) {
                state = RemoteDeviceState.Unknown;
            } else if (present0 == SDK_RETURN_CODE.Success) {
                state = RemoteDeviceState.Present;
            } else {
                Debug.Assert(present0 == SDK_RETURN_CODE.Timeout, "present0: " + present0);
                state = RemoteDeviceState.NotPresent;
            }
            Utils.MiscUtils.Trace_WriteLine("FindIfPresentOrConnected: c={0} + p={1} => {2}",
                connected, present0, state);
            return state;
        }

        //--------
        public bool DoPowerDownUpReset
        {
            get { return m_factory.DoPowerDownUpReset; }
            set { m_factory.DoPowerDownUpReset = value; }
        }
    }
}
