using System;
using System.Collections.Generic;
using System.Text;
using InTheHand.Net.Bluetooth.Factory;
using System.Threading;

namespace InTheHand.Net.Bluetooth
{
    class NullRfcommStream : CommonRfcommStream
    {
        readonly LsnrCommands _cmds;

        internal NullRfcommStream(LsnrCommands cmds)
        {
            _cmds = cmds;
        }

        //----
        internal void Do_HandleCONNECTED()
        {
            HandleCONNECTED("testconnected");
        }


        //----


        protected override void RemovePortRecords()
        {
        }

        protected override void DoOtherPreDestroy(bool disposing)
        {
        }

        protected override void DoPortClose(bool disposing)
        {
        }

        protected override void DoPortDestroy(bool disposing)
        {
        }

        protected override void DoOtherSetup(BluetoothEndPoint bep, int scn)
        {
        }

        protected override void AddPortRecords()
        {
        }

        protected override void DoOpenClient(int scn, BluetoothAddress addressToConnect)
        {
        }

        protected override void DoOpenServer(int scn)
        {
            if (_cmds.NextPortShouldConnectImmediately) {
                ThreadPool.QueueUserWorkItem(delegate
                {
                    Do_HandleCONNECTED();
                });
            } else if (_cmds.NextOpenServerShouldFail) {
                throw new MulticastNotSupportedException("DoOpenServer FAILURE!");
            }
        }

        protected override bool TryBondingIf_inLock(BluetoothAddress addressToConnect, int ocScn, out Exception err)
        {
            err = null;
            return false;
        }

        protected override void DoWrite(byte[] p_data, ushort len_to_write, out ushort p_len_written)
        {
            // 'Write' all the data
            p_len_written = len_to_write;
        }
    }
}
