#if NETCF && !FX3_5
using System;

namespace InTheHand.Net.Bluetooth
{
    /// <exclude/>
    public abstract class AsyncCompletedEventArgs : EventArgs
    {
        Exception m_error;
        bool m_cancelled;
        Object m_userState;

        internal AsyncCompletedEventArgs(Exception error, bool cancelled, Object userState)
        {
            m_error = error;
            m_cancelled = cancelled;
            m_userState = userState;
        }

        /// <exclude/>
        protected virtual void RaiseExceptionIfNecessary()
        {
            if (m_error != null)
                throw m_error;
            if (m_cancelled)
                throw new InvalidOperationException("Cancelled");
        }

        /// <exclude/>
        public Exception Error { get { return m_error; } }
        /// <exclude/>
        public bool Cancelled { get { return m_cancelled; } }
        /// <exclude/>
        public object UserState { get { return m_userState; } }
    }

}
#endif