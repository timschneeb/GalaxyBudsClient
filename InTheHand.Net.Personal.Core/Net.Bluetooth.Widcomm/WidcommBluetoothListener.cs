// 32feet.NET - Personal Area Networking for .NET
//
// InTheHand.Net.Widcomm.WidcommBluetoothListener
// 
// Copyright (c) 2008-2010 In The Hand Ltd, All rights reserved.
// Copyright (c) 2008-2010 Alan J. McFarlane, All rights reserved.
// This source code is licensed under the MIT License

using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using InTheHand.Net.Bluetooth.Factory;

namespace InTheHand.Net.Bluetooth.Widcomm
{
    /// <exclude/>
    [SuppressMessage("Microsoft.Design", "CA1001:TypesThatOwnDisposableFieldsShouldBeDisposable",
        Justification = "Stop methods is the XxxListener-pattern's Dispose.")]
#if TEST_EARLY
    public 
#endif
    sealed class WidcommBluetoothListener : CommonBluetoothListener
    {
        readonly WidcommBluetoothFactoryBase m_factory;
        volatile WidcommRfcommInterface m_RfCommIf; // This has no Finalizer
        ISdpService m_sdpService;

        internal WidcommBluetoothListener(WidcommBluetoothFactoryBase factory)
            :base(factory)
        {
            m_factory = factory;
            GC.SuppressFinalize(this); // Finalization only needed for IRfcommIf.
        }

        void SetupRfcommIf()
        {
            IRfCommIf rfCommIf = m_factory.GetWidcommRfCommIf();
            m_RfCommIf = new WidcommRfcommInterface(rfCommIf);
            rfCommIf.Create();
            GC.ReRegisterForFinalize(this);
        }

        //----
        protected override void SetupListener(BluetoothEndPoint bep, int requestedScn, out BluetoothEndPoint liveLocalEP)
        {
            SetupRfcommIf();
            int scn = m_RfCommIf.SetScnForLocalServer(bep.Service, requestedScn);
            BTM_SEC secLevel = WidcommUtils.ToBTM_SEC(true, m_authenticate, m_encrypt);
            m_RfCommIf.SetSecurityLevelServer(secLevel,
                new byte[] { (byte)'h', (byte)'a', (byte)'c', (byte)'k', (byte)'S', (byte)'v', (byte)'r', }
                );
            liveLocalEP = new BluetoothEndPoint(BluetoothAddress.None, BluetoothService.Empty, scn);
        }

        protected override void AddCustomServiceRecord(ref ServiceRecord fullServiceRecord, int livePort)
        {
            var livePortB = checked((byte)livePort);
            ServiceRecordHelper.SetRfcommChannelNumber(fullServiceRecord,
                livePortB);
            m_sdpService = SdpService.CreateCustom(fullServiceRecord, m_factory);
        }

        protected override void AddSimpleServiceRecord(out ServiceRecord fullServiceRecord,
            int livePort, Guid serviceClass, string serviceName)
        {
            var livePortB = checked((byte)livePort);
            m_sdpService = SdpService.CreateRfcomm(
                serviceClass,
                serviceName, livePortB, m_factory);
            ServiceRecordBuilder bldrDummy = new ServiceRecordBuilder();
            bldrDummy.AddServiceClass(serviceClass);
            bldrDummy.ServiceName = serviceName;
            fullServiceRecord = bldrDummy.ServiceRecord;
            ServiceRecordHelper.SetRfcommChannelNumber(fullServiceRecord,
                livePortB);
        }

        //----
        protected override bool IsDisposed
        {
            get { return (m_RfCommIf == null); }
        }

        //----
        protected override void OtherDispose(bool disposing)
        {
            WidcommRfcommInterface rfCommIf = m_RfCommIf;
            m_RfCommIf = null;
            Debug.Assert(IsDisposed, "NOT IsDisposed, but just set it so!");
            if (rfCommIf == null) {
                //Debug.Fail("WidcommBluetoothListener not started (disposing: " +
                //    disposing + ")."); // TODO (Remove this before release).
                return;
            }
            rfCommIf.Dispose(disposing);
        }

        protected override void OtherDisposeMore()
        {
            if (m_sdpService != null)
                m_sdpService.Dispose();
        }

        //----
        protected override CommonRfcommStream GetNewPort()
        {
            CommonRfcommStream port = m_factory.GetWidcommRfcommStreamWithoutRfcommIf();
            return port;
        }

    }
}
