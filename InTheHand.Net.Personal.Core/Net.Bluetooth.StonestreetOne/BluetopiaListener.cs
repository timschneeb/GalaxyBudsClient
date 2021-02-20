// 32feet.NET - Personal Area Networking for .NET
//
// Net.Bluetooth.StonestreetOne.BluetopiaListener
// 
// Copyright (c) 2011 Alan J.McFarlane, All rights reserved.
// Copyright (c) 2011 In The Hand Ltd, All rights reserved.
// This source code is licensed under the In The Hand Community License - see License.txt

using System;
using System.Diagnostics;
using InTheHand.Net.Bluetooth.Factory;
using System.Text;

namespace InTheHand.Net.Bluetooth.StonestreetOne
{
    class BluetopiaListener : CommonBluetoothListener
    {
        readonly BluetopiaFactory _fcty;
        BluetopiaSdpCreator _sdpCtor;
        volatile bool _disposed;
        byte _port;
        BluetopiaRfcommStream _theFirstBorn;

        //--
        internal BluetopiaListener(BluetopiaFactory factory)
            : base(factory)
        {
            _fcty = factory;
        }

        //--
        protected override void SetupListener(BluetoothEndPoint bep, int scn, out BluetoothEndPoint liveLocalEP)
        {
            // No API in Bluetopia for one-off setup!!
            if (bep.HasPort) {
                _port = checked((byte)bep.Port);
            } else {
                _port = 11;
            }
            liveLocalEP = new BluetoothEndPoint(BluetoothAddress.None, BluetoothService.Empty, _port);
        }

        protected override CommonRfcommStream GetNewPort()
        {
            var p = new BluetopiaRfcommStream(_fcty);
            // (Note: The Port Id/Handle isn't known until DoOpenServer is called).
            if (_theFirstBorn == null) _theFirstBorn = p;
            return p;
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
            if (_sdpCtor != null) {
                _sdpCtor.DeleteServiceRecord();
                _sdpCtor = null;
            }
        }

        //--
        // The structs I created for creating SDP records are totally wrong,
        // so this doesn't work!!!!
        protected override void AddCustomServiceRecord(ref ServiceRecord fullServiceRecord, int livePort)
        {
            var livePortB = checked((byte)livePort);
            ServiceRecordHelper.SetRfcommChannelNumber(fullServiceRecord,
                livePortB);
            var sdpCtor = new BluetopiaSdpCreator(_fcty);
            sdpCtor.CreateServiceRecord(fullServiceRecord);
            _sdpCtor = sdpCtor;
        }

        protected override void AddSimpleServiceRecord(out ServiceRecord fullServiceRecord,
            int livePort, Guid serviceClass, string serviceName)
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

    }
}
