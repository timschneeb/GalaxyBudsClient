// 32feet.NET - Personal Area Networking for .NET
//
// InTheHand.Net.Widcomm.WidcommSocketExceptions
// 
// Copyright (c) 2008-2009 In The Hand Ltd, All rights reserved.
// Copyright (c) 2008-2009 Alan J. McFarlane, All rights reserved.
// This source code is licensed under the MIT License

using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace InTheHand.Net.Bluetooth.Widcomm
{
    class WidcommPortSingleThreader : IDisposable
    {
        Queue<StCommand> m_actions = new Queue<StCommand>();
        Thread m_thread;
        volatile bool _ended;

        public WidcommPortSingleThreader()
        {
            Thread t = new Thread(SingleThreader_Runner);
            t.Name = "32feetWidcommST";
#if !NETCF //SetApartmentState
            t.SetApartmentState(ApartmentState.STA);
#endif
            t.IsBackground = true;
            m_thread = t;
            m_thread.Start();
        }
#if NETCF
        ManualResetEvent _netcfEvent = new ManualResetEvent(false);
#endif

        public static bool IsWidcommSingleThread(WidcommPortSingleThreader st)
        {
            var x = Thread.CurrentThread == st.m_thread;
            return x;
        }

        internal T AddCommand<T>(T command) where T : StCommand
        {
            if (WidcommBtInterface.IsWidcommSingleThread(this)) {
                Debug.Assert(Thread.CurrentThread == m_thread);
                // Yikes the special thread is trying to offload some work!!
                Debug.Fail("Widcomm main thread calling itself!?!");
                throw new NotSupportedException("Internal error -- Widcomm main thread calling itself!?!");
            } else {
                Debug.Assert(Thread.CurrentThread != m_thread);
            }
            command.SetRunner(this);
            //TODO ?if (_ended) throw new InvalidOperationException("Not running!");
            lock (m_actions) {
                m_actions.Enqueue(command);
#if NETCF
                try {
                    _netcfEvent.Set();
                } catch (ObjectDisposedException) {
                    bool ended = _ended;
                    Utils.MiscUtils.Trace_WriteLine("!single-threader.AddCommand ObjectDisposedException"
                        + " (_ended: " + _ended + ")");
                }
#else
                Monitor.Pulse(m_actions);
#endif
            }
            return command;
        }

        void SingleThreader_Runner()
        {
            Debug.Assert(IsWidcommSingleThread(this), "double check the IsWidcommSingleThread property works.");
            try {
#if !NETCF
                while (true) {
                    StCommand cmd;
                    lock (m_actions) {
                        while (m_actions.Count == 0) {
                            Monitor.Wait(m_actions);
                        }
                        cmd = m_actions.Dequeue();
                    }//lock
                    if (cmd is ExitCommand) {
                        break;
                    }
                    cmd.Action();
                    // Clear the command, otherwise its port reference keeps the port,
                    // which keeps the WRCStream, thus its finalizer isn't called
                    // which means this thread says alive, which keeps the command...
                    cmd = null;
                }//while
#else
                while (true) {
                    int count;
                    lock (m_actions) {
                        count = m_actions.Count;
                    }// Like Wait() must leave the lock to let the writer in!
                    if (count == 0) {
                        _netcfEvent.WaitOne();
                    }
                    StCommand cmd;
                    lock (m_actions) {
                        _netcfEvent.Reset();
                        count = m_actions.Count;
                        if (count == 0) {
                            Debug.Fail("Somebody else is reading from the queue!!!");
                            continue;
                        }
                        cmd = m_actions.Dequeue();
                    }//lock
                    if (cmd is ExitCommand) {
                        break;
                    }
                    cmd.Action();
                    // Clear the command, otherwise its port reference keeps the port,
                    // which keeps the WRCStream, thus its finalizer isn't called
                    // which means this thread says alive, which keeps the command...
                    cmd = null;
                }//while
#endif
            } finally {
                _ended = true;
#if NETCF
                _netcfEvent.Close();
#endif
            }
        }

        internal abstract class StCommand
        {
            // TODO Pool ManualResetEvent instances in WidcommPortSingleThreader.
            ManualResetEvent _complete = new ManualResetEvent(false);
            Exception _error;
            WidcommPortSingleThreader _runner;

            internal void SetRunner(WidcommPortSingleThreader runner)
            {
                _runner = runner;
            }

            [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes",
                Justification = "We rethrow it on the caller.")]
            internal void Action()
            {
                try {
                    ActionCore();
                } catch (Exception ex) {
                    _error = ex;
                } finally {
                    try {
                        _complete.Set();
                    } catch (ObjectDisposedException) {
                        Utils.MiscUtils.Trace_WriteLine("!StCommand.Action ObjectDisposedException (" +
                            this.GetType().Name + ")");
                    }
                }
            }

            protected abstract void ActionCore();

            public Exception Error { get { return _error; } }

            internal void WaitCompletion()
            {
                WaitCompletion(true);
            }

            internal void WaitCompletion(bool shouldWait)
            {
                bool ended = _runner.IsEnded;
                if (shouldWait && !ended) {
                    _complete.WaitOne();
                    // Don't close this in the non-wait case as the completed
                    // code will set it and explode!  (We've wrapped that in a
                    // catch(ODEx) also).
                    // Note we skip the wait and close in the Finalization case,
                    // that's either at shutdown, or the consumer didn't Dispose
                    // so we're not too worried performance wise in either case.
                    _complete.Close();
                } else {
                }
                Exception ex = Error;
                if (ex != null) {
                    var ex2 = new System.Reflection.TargetInvocationException(
                        // NETCFv2 TargetInvocationException..ctor(Exception)
                        // does NOT set the InnerException property!!!  Use this
                        // constructor instead.
                        ex.Message == null ? "Widcomm WaitCompletion error." : ex.Message,
                        ex);
                    Debug.Assert(ex2.InnerException != null, "NOT ex2.InnerException != null");
                    Debug.Assert(ex2.Message == ex.Message, "NOT ex2.Message == ex.Message");
                    throw ex2;
                }
            }
        }

        internal sealed class ExitCommand : StCommand
        {
            protected override void ActionCore()
            { //NOP
            }
        }

        internal abstract class PortCommand : StCommand
        {
            IRfcommPort _port;
            protected IRfcommPort Port { get { return _port; } }

            public PortCommand(IRfcommPort port)
            {
                _port = port;
            }
        }

        internal sealed class PortWriteCommand : PortCommand
        {
            byte[] _data;
            ushort _lenToWrite, _lenWritten;
            PORT_RETURN_CODE _result;

            public PortWriteCommand(byte[] data, ushort lenToWrite, IRfcommPort port)
                : base(port)
            {
                _data = data;
                _lenToWrite = lenToWrite;
            }

            protected override void ActionCore()
            {
                _result = Port.Write(_data, _lenToWrite, out _lenWritten);
            }

            private new void WaitCompletion()
            {
                throw new NotSupportedException("Use WaitCompletion(out ushort lenWritten).");
            }

            internal PORT_RETURN_CODE WaitCompletion(out ushort lenWritten)
            {
                base.WaitCompletion();
                lenWritten = _lenWritten;
                return _result;
            }
        }

        internal sealed class PortCreateCommand : PortCommand
        {
            public PortCreateCommand(IRfcommPort port)
                : base(port)
            {
            }

            protected override void ActionCore()
            {
                Port.Create();
            }
        }

        internal sealed class OpenServerCommand : PortCommand
        {
            int _scn;
            PORT_RETURN_CODE _result;

            public OpenServerCommand(int scn, IRfcommPort port)
                : base(port)
            {
                _scn = scn;
            }

            public new PORT_RETURN_CODE WaitCompletion()
            {
                base.WaitCompletion();
                return _result;
            }

            protected override void ActionCore()
            {
                _result = Port.OpenServer(_scn);
            }
        }

        internal sealed class OpenClientCommand : PortCommand
        {
            int _scn;
            byte[] _address;
            PORT_RETURN_CODE _result;

            public OpenClientCommand(int scn, byte[] address, IRfcommPort port)
                : base(port)
            {
                _address = address;
                _scn = scn;
            }

            public new PORT_RETURN_CODE WaitCompletion()
            {
                base.WaitCompletion();
                return _result;
            }

            protected override void ActionCore()
            {
                _result = Port.OpenClient(_scn, _address);
            }
        }

        internal sealed class PortCloseCommand : PortCommand
        {
            PORT_RETURN_CODE _result = PORT_RETURN_CODE.NotSet;

            public PortCloseCommand(IRfcommPort port)
                : base(port)
            {
            }

            protected override void ActionCore()
            {
                _result = Port.Close();
            }

            public new PORT_RETURN_CODE WaitCompletion(bool shouldWait)
            {
                base.WaitCompletion(shouldWait);
                return _result;
            }
        }

        internal sealed class MiscNoReturnCommand : StCommand
        {
            ThreadStart _dlgt;

            public MiscNoReturnCommand(ThreadStart dlgt)
            {
                _dlgt = dlgt;
            }

            protected override void ActionCore()
            {
                _dlgt();
            }
        }

        internal sealed class MiscReturnCommand<TResult> : StCommand
        {
            Func<TResult> _dlgt;
            TResult _result;

            public MiscReturnCommand(Func<TResult> dlgt)
            {
                _dlgt = dlgt;
            }

            protected override void ActionCore()
            {
                _result = _dlgt();
            }

            public new TResult WaitCompletion()
            {
                base.WaitCompletion();
                return _result;
            }
        }

        #region IDisposable Members

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public void Dispose(bool disposing)
        {
            Utils.MiscUtils.Trace_WriteLine("WidcommPortSingleThreader.Dispose(b)");
            _ended = true; // Will also be set by the thread ending.
            // We can safely do this as Queue is not finalizable and our reference
            // keeps it alive.  The only danger could be if some blocked thread is
            // holding the lock BUT THAT'S ONLY US!
            AddCommand(new ExitCommand());
        }

        public bool IsEnded { get { return _ended; } }
        #endregion
    }
}
