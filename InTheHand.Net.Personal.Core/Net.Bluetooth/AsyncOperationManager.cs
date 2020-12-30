#if NETCF
using System;
using System.Windows.Forms;
using System.Threading;

namespace InTheHand.Net.Bluetooth
{
    static class AsyncOperationManager
    {
        static LocalDataStoreSlot _slotCachedAutoSyncCtx;

        static AsyncOperationManager()
        {
            _slotCachedAutoSyncCtx = Thread.AllocateDataSlot();
        }

        public static AsyncOperation CreateOperation(object userState)
        {
            System.Windows.Forms.Control c = GetSyncCtx();
            return new AsyncOperation(c, userState);
        }

        //public static bool AssumeWinFormsSynchronizationContext { get; set; }
        readonly static bool AssumeWinFormsSynchronizationContext = true;

        static Control GetSyncCtx()
        {
            //if (_manualSyncCtx != null) { return _manualSyncCtx; }
            if (AssumeWinFormsSynchronizationContext) {
                // Create a Control on which to marshal.  There better be a
                // message-loop or the event will never be raised.
                // For performance (finalization etc) try and create only one
                // [per message-loop -- which should be one is most apps!!].
                // (No need for thread safety as we ARE thread-static!)
                Control cached = (Control)Thread.GetData(_slotCachedAutoSyncCtx);
                if (cached != null)
                    return cached;
                var c = new Control();
                Thread.SetData(_slotCachedAutoSyncCtx, c);
                return c;
            }
            return null;
        }

    }

}
#endif
