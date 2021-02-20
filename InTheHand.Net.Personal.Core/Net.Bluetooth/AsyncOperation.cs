#if NETCF
using System;
using System.Threading;
using SendOrPostCallback = System.Threading.WaitCallback;

namespace InTheHand.Net.Bluetooth
{
    class AsyncOperation
    {
        System.Windows.Forms.Control m_synchronizeInvoke;
        object m_userSuppliedState;

        internal AsyncOperation(System.Windows.Forms.Control synchronizeInvoke, object userSuppliedState)
        {
            m_userSuppliedState = userSuppliedState;
            m_synchronizeInvoke = synchronizeInvoke;
        }

        public object UserSuppliedState
        {
            get { return m_userSuppliedState; }
        }

        internal void Post(SendOrPostCallback cb, object args)
        {
            if (m_synchronizeInvoke != null) {
                m_synchronizeInvoke.BeginInvoke(cb, args);
            } else {
                ThreadPool.QueueUserWorkItem(cb, args);
            }
        }

        internal void PostOperationCompleted(SendOrPostCallback cb, object args)
        {
            // to-do mark as complete -- we don't use that functionality.
            Post(cb, args);
        }

    }
}
#endif