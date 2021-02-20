// 32feet.NET - Personal Area Networking for .NET
//
// InTheHand.Net.Bluetooth.Widcomm.WidcommBluetoothFactoryBase
// 
// Copyright (c) 2008-2010 In The Hand Ltd, All rights reserved.
// This source code is licensed under the MIT License

using System;
using System.Collections.Generic;
using System.Text;
using InTheHand.Net.Sockets;
using System.Diagnostics;
using InTheHand.Net.Bluetooth.Factory;
using System.Threading;

namespace InTheHand.Net.Bluetooth.Widcomm
{
    abstract class WidcommBluetoothFactoryBase : BluetoothFactory
    {
        internal abstract WidcommBtInterface GetWidcommBtInterface();
        internal abstract WidcommRfcommStreamBase GetWidcommRfcommStream();
        internal abstract WidcommRfcommStreamBase GetWidcommRfcommStreamWithoutRfcommIf();
        internal abstract IRfcommPort GetWidcommRfcommPort();
        internal abstract IRfCommIf GetWidcommRfCommIf();
        internal abstract ISdpService GetWidcommSdpService();
        internal abstract void EnsureLoaded();
        //
        internal abstract WidcommPortSingleThreader GetSingleThreader();
        internal abstract bool IsWidcommSingleThread();

        #region Resource Cleanup records
        internal void AddPort(WidcommRfcommStreamBase port)
        {
            lock (_livePorts) {
                _livePorts.Add(port);
            }
        }

        internal void RemovePort(WidcommRfcommStreamBase port)
        {
            bool found;
            lock (_livePorts) {
                found = _livePorts.Remove(port);
            }
            Debug.WriteLine(WidcommUtils.GetTime4Log()
                + ": RemovePort found it: " + found);
        }

        internal WidcommRfcommStreamBase[] GetPortList()
        {
            lock (_livePorts) {
                var a = _livePorts.ToArray();
                Debug.WriteLine(WidcommUtils.GetTime4Log() + ": GetPortList returned " + a.Length + " ports.");
                return a;
            }
        }

        List<WidcommRfcommStreamBase> _livePorts = new List<WidcommRfcommStreamBase>();
        #endregion

        [Obsolete("_untested_")]
        internal virtual void AddThingsToKeepAlive<TObject>(TObject o)
            where TObject : class
        { }
    }

    sealed class WidcommBluetoothFactory : WidcommBluetoothFactoryBase
    {
        static IBtIf s_btIf;
        static WidcommBtInterface s_btInterface;
        static WidcommBluetoothSecurity m_btSecurity;
        static WidcommPortSingleThreader _st;
        internal volatile bool _seenStackDownEvent;
        List<object> _thingsToKeepAlive = new List<object>();

        public WidcommBluetoothFactory()
        {
            DoPowerDownUpReset = true;
            //throw if device doesn't support the stack
#if NETCF
            bool supp = System.IO.File.Exists("\\Windows\\BTSdkCE50.dll");
            if(!supp)
                supp = System.IO.File.Exists("\\Windows\\BTSdkCE30.dll");

            if (!supp)
                throw new PlatformNotSupportedException("Broadcom Bluetooth stack not supported (radio).");
#endif
            EnsureLoaded();
            // Check radio is connected.
            this.GetPrimaryRadio();
        }

        public bool DoPowerDownUpReset { get; set; }

        internal override void EnsureLoaded()
        {
            Debug.WriteLine(WidcommUtils.GetTime4Log() + ": WcBtFcty EnsureLoaded ENTRY (for v.L)");
            if (_st != null) {
                if (WidcommBtInterface.IsWidcommSingleThread(_st)) { // DEBUG
                }
            }
            lock (typeof(WidcommBluetoothFactory)) {
                Debug.WriteLine(WidcommUtils.GetTime4Log() + ": WcBtFcty EnsureLoaded   IN lock");
                // In case Widcomm is forced to shutdown when CBtIf is extant we monitor
                // for the stack-down event so we can force reload next time
                // we're used.
                var seenStackDownEvent = _seenStackDownEvent;
                _seenStackDownEvent = false;
                if (seenStackDownEvent) {
                    Debug.WriteLine(WidcommUtils.GetTime4Log() + ": WcBtFcty seenStackDownEvent");
                    if (!DoPowerDownUpReset) {
                        Utils.MiscUtils.Trace_WriteLine("Ignoring stack/radio shutdown due to DoPowerDownUpReset=false.");
                    } else {
                        Utils.MiscUtils.Trace_WriteLine("Restarting due to stack/radio shutdown.");
                        bool respectLocks = true;
                        // -- Shutdown --
                        ThreadStart doDispose = () => Dispose(true, respectLocks);
                        var st = GetSingleThreader();
                        if (st != null && !WidcommBtInterface.IsWidcommSingleThread(st)) {
                            respectLocks = false;
                            var c = st.AddCommand(new WidcommPortSingleThreader.MiscNoReturnCommand(
                                doDispose));
                            c.WaitCompletion();
                            MemoryBarrier();
                        } else {
                            doDispose();
                        }
                        Debug.WriteLine(WidcommUtils.GetTime4Log() + ": WcBtFcty done Dispose");
                        Debug.Assert(s_btIf == null, "After Dispose but NOT s_btIf == null");
                        Debug.Assert(s_btInterface == null, "After Dispose but NOT s_btInterface == null");
                        Thread.Sleep(5000);
                    }
                }
                //-- Create --
                // CBtIf: MUST ONLY be ONE of these, and must be FIRST created!
                //   "Only one object of this class should be instantiated by the application."
                //   "This class must be instantiated before any other DK classes are used"
                if (s_btIf == null) {
#if KILL_SINGLE_THREAD_AT_DISPOSAL
                    Debug.Assert(_st == null);
#endif
                    if (_st == null) _st = new WidcommPortSingleThreader();
                    Debug.Assert(GetSingleThreader() != null);
                    IBtIf btIf;
                    Func<IBtIf> f = () => new WidcommBtIf(this);
                    var st = GetSingleThreader();
                    if (st != null && !WidcommBtInterface.IsWidcommSingleThread(st)) {
                        var c = st.AddCommand(new WidcommPortSingleThreader.MiscReturnCommand<IBtIf>(
                            f));
                        btIf = c.WaitCompletion();
                    } else {
                        btIf = f();
                    }
                    Debug.WriteLine(WidcommUtils.GetTime4Log() + ": WcBtFcty done new WidcommBtIf");
                    if (st != null) {
                        btIf = new WidcommStBtIf(this, btIf);
                        Utils.MiscUtils.Trace_WriteLine("IBtIf using WidcommStBtIf.");
                    }
                    Debug.Assert(s_btInterface == null);
                    WidcommBtInterface btInterface = new WidcommBtInterface(btIf, this);
                    // Don't set these until we're sure that initialisation has 
                    // all completed successfully.
                    s_btIf = btIf;
                    s_btInterface = btInterface;
                } else {
                    Debug.Assert(s_btInterface != null, "One set but not the other!");
                }
            }//lock
            Debug.WriteLine(WidcommUtils.GetTime4Log() + ": WcBtFcty EnsureLoaded EXIT");
        }

        //-----
        protected override void Dispose(bool disposing)
        {
            Dispose(disposing, true);
        }

        private void Dispose(bool disposing, bool respectLocks)
        {
            IDisposable iface;
            IDisposable st;
            //lock (typeof(WidcommBluetoothFactory)) 
            bool gotLock = false;
            try {
                if (respectLocks) {
                    Monitor.Enter(typeof(WidcommBluetoothFactory));
                    gotLock = true;
                }
                iface = (IDisposable)s_btInterface;
                s_btIf = null;
                s_btInterface = null;
                m_btSecurity = null;
#if KILL_SINGLE_THREAD_AT_DISPOSAL
                st = _st;
                _st = null;
#else
                st = null;
#endif
                if (disposing) {
                    _thingsToKeepAlive.Clear();
                }
            } finally {
                if (gotLock) {
                    Monitor.Exit(typeof(WidcommBluetoothFactory));
                }
            }
            if (iface != null)
                iface.Dispose();
            if (st != null)  // Must NOT dispose this first!
                st.Dispose();
        }

        //-----
        [Obsolete("_untested_")]
        internal override void AddThingsToKeepAlive<TObject>(TObject o)
        {
            lock (typeof(WidcommBluetoothFactory)) {
                _thingsToKeepAlive.Add(o);
            }
        }

        //-----
        internal override WidcommPortSingleThreader GetSingleThreader()
        {
            return _st;
        }

        internal override bool IsWidcommSingleThread()
        {
            var st = GetSingleThreader();
            return WidcommPortSingleThreader.IsWidcommSingleThread(st);
        }

        //-----
        protected override IBluetoothClient GetBluetoothClient()
        {
            EnsureLoaded();
            return new WidcommBluetoothClient(this);
        }

        protected override IBluetoothClient GetBluetoothClient(System.Net.Sockets.Socket acceptedSocket)
        {
            throw new NotSupportedException("Cannot create a BluetoothClient from a Socket on the Widcomm stack.");
        }

        protected override IBluetoothClient GetBluetoothClientForListener(CommonRfcommStream strm)
        {
            return new WidcommBluetoothClient((WidcommRfcommStreamBase)strm, this);
        }

        protected override IBluetoothClient GetBluetoothClient(BluetoothEndPoint localEP)
        {
            EnsureLoaded();
            return new WidcommBluetoothClient(localEP, this);
        }

        protected override IL2CapClient GetL2CapClient()
        {
            EnsureLoaded();
            return new WidcommL2CapClient(this);
        }

        //----------------
        protected override IBluetoothListener GetBluetoothListener()
        {
            EnsureLoaded();
            return new WidcommBluetoothListener(this);
        }

        //----------------
        internal override WidcommBtInterface GetWidcommBtInterface()
        {
            EnsureLoaded();
            return s_btInterface;
        }

        internal override WidcommRfcommStreamBase GetWidcommRfcommStream()
        {
            EnsureLoaded();
            return new WidcommRfcommStream(GetWidcommRfcommPort(), GetWidcommRfCommIf(), this);
        }

        internal override WidcommRfcommStreamBase GetWidcommRfcommStreamWithoutRfcommIf()
        {
            EnsureLoaded();
            return new WidcommRfcommStream(GetWidcommRfcommPort(), null, this);
        }

        internal override IRfcommPort GetWidcommRfcommPort()
        {
            EnsureLoaded();
            // Handling of single threadedness is done within WidcommRfcommStream.
            return new WidcommRfcommPort();
        }

        internal override IRfCommIf GetWidcommRfCommIf()
        {
            EnsureLoaded();
            IRfCommIf inst = new WidcommRfCommIf();
            if (GetSingleThreader() != null) {
                inst = new WidcommStRfCommIf(this, inst);
                Utils.MiscUtils.Trace_WriteLine("IRfCommIf using WidcommStRfCommIf.");
            }
            return inst;
        }

        protected override IBluetoothDeviceInfo GetBluetoothDeviceInfo(BluetoothAddress address)
        {
            EnsureLoaded();
            return WidcommBluetoothDeviceInfo.CreateFromGivenAddress(address, this);
        }

        //----------------
        protected override IBluetoothRadio GetPrimaryRadio()
        {
            EnsureLoaded();
            return new WidcommBluetoothRadio(this);
        }

        protected override IBluetoothRadio[] GetAllRadios()
        {
            EnsureLoaded();
            // Widcomm supports only one radio.
            return new IBluetoothRadio[] { GetPrimaryRadio() };
        }

        //----------------
        internal override ISdpService GetWidcommSdpService()
        {
            EnsureLoaded();
            return new SdpService();
        }

        //----------------
        protected override IBluetoothSecurity GetBluetoothSecurity()
        {
            EnsureLoaded();
            if (m_btSecurity == null) {
                m_btSecurity = new WidcommBluetoothSecurity(this);
            }
            return m_btSecurity;
        }

        //----------------
        internal static WidcommBluetoothFactoryBase GetWidcommIfExists()
        {
            foreach (var cur in BluetoothFactory.Factories) {
                var curWf = cur as WidcommBluetoothFactoryBase;
                if (curWf != null) {
                    return curWf;
                }
            }
            throw new InvalidOperationException("No Widcomm.");
        }

        //----------------
        private static void MemoryBarrier()
        {
#if ! PocketPC
            Thread.MemoryBarrier();
#endif
        }

    }
}
