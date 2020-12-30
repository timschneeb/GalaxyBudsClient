using System;
using System.Collections.Generic;
using System.Text;
using InTheHand.Net.Bluetooth.Factory;

namespace InTheHand.Net.Bluetooth
{
    class NullBtListener : CommonBluetoothListener
    {
        internal enum LsnrSetting
        {
            None,
            ErrorOnOpenServer,
            ConnectsImmediately,
            ErrorConnectsImmediately,
        }

        //----
        volatile bool _disposed;
        volatile bool _disposedMore;
        Queue<LsnrSetting> _portSettings = new Queue<LsnrSetting>();

        internal NullBtListener(NullBluetoothFactory fcty)
            : base(fcty)
        {
        }

        //----
        internal void AddPortSettings(LsnrSetting[] settings)
        {
            foreach (var cur in settings) {
                _portSettings.Enqueue(cur);
            }
        }

        //----
        protected override CommonRfcommStream GetNewPort()
        {
            var set = _portSettings.Dequeue();
            var command = new LsnrCommands();
            switch (set) {
                case LsnrSetting.None:
                    break;
                case LsnrSetting.ErrorOnOpenServer:
                    command.NextOpenServerShouldFail = true;
                    break;
                case LsnrSetting.ConnectsImmediately:
                    command.NextPortShouldConnectImmediately = true;
                    break;
                case LsnrSetting.ErrorConnectsImmediately:
                    throw new NotImplementedException();
                //  break;
                default:
                    throw new ArgumentException("Unknown LsnrSetting value: " + set);
            }
            return new NullRfcommStream(command);
        }

        //----
        protected override void SetupListener(BluetoothEndPoint bep, int scn, out BluetoothEndPoint liveLocalEP)
        {
            liveLocalEP = new BluetoothEndPoint(BluetoothAddress.None, Guid.Empty, 25);
        }

        protected override void AddCustomServiceRecord(ref ServiceRecord fullServiceRecord, int livePort)
        {
        }

        protected override void AddSimpleServiceRecord(out ServiceRecord fullServiceRecord, int livePort, Guid serviceClass, string serviceName)
        {
            fullServiceRecord = CreateSimpleServiceRecord(serviceClass, serviceName);
            AddCustomServiceRecord(ref fullServiceRecord, livePort);
        }

        //TODO move to base class.  Maybe also make 'virtual' AddSimpleServiceRecord method.
        static ServiceRecord CreateSimpleServiceRecord(Guid serviceClass, string serviceName)
        {
            ServiceRecordBuilder bldr = new ServiceRecordBuilder();
            bldr.AddServiceClass(serviceClass);
            bldr.ServiceName = serviceName;
            var simpleSR = bldr.ServiceRecord;
            return simpleSR;
        }


        protected override bool IsDisposed
        {
            get { return _disposed; }
        }

        protected override void OtherDispose(bool disposing)
        {
            _disposed = true;
            // Anything else to do here?
        }

        protected override void OtherDisposeMore()
        {
            _disposedMore = true;
        }

        internal bool AllDisposed { get { return _disposed && _disposedMore; } }

    }


    internal class LsnrCommands
    {
        public bool NextOpenServerShouldFail { get; set; }
        public bool NextPortShouldConnectImmediately { get; set; }

        public LsnrCommands Clone()
        {
            var copy = new LsnrCommands
            {
                NextPortShouldConnectImmediately = this.NextPortShouldConnectImmediately,
                NextOpenServerShouldFail = this.NextOpenServerShouldFail
            };
            return copy;
        }
    }

}
