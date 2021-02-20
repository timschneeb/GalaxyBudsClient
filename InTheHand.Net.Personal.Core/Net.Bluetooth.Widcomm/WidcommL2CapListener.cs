using System;
using InTheHand.Net.Bluetooth.Factory;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace InTheHand.Net.Bluetooth.Widcomm
{
    sealed class WidcommL2CapListener : CommonBluetoothListener
    {
        public static CommonBluetoothListener Create()
        {
            WidcommBluetoothFactoryBase wf = WidcommBluetoothFactory.GetWidcommIfExists();
            var lsnr = new WidcommL2CapListener(wf);
            return lsnr;
        }

        //public static CommonBluetoothListener Create(BluetoothEndPoint bind)
        //{
        //    var lsnr = Create();
        //    lsnr.Construct(bind);
        //    return lsnr;
        //}

        //======
        readonly WidcommBluetoothFactoryBase m_factory;
        volatile WidcommRfcommInterface m_RfCommIf; // This has no Finalizer
        IRfCommIf _rfCommIf__tmp;
        ISdpService m_sdpService;


        [SuppressMessage("Microsoft.Usage", "CA1816:CallGCSuppressFinalizeCorrectly",
            Justification = "Finalization only needed once IRfcommIf created, then calls ReRegisterForFinalize.")]
        internal WidcommL2CapListener(WidcommBluetoothFactoryBase factory)
            : base(factory)
        {
            m_factory = factory;
            GC.SuppressFinalize(this); // Finalization only needed for IRfcommIf.
        }

        void SetupRfcommIf()
        {
            IRfCommIf rfCommIf = WidcommL2CapClient.GetWidcommL2CapIf(m_factory);
            _rfCommIf__tmp = rfCommIf;
            m_RfCommIf = new WidcommRfcommInterface(rfCommIf);
            rfCommIf.Create();
            GC.ReRegisterForFinalize(this);
        }

        //----
        protected override void VerifyPortIsInRange(BluetoothEndPoint bep)
        {
            //TODO if (bep.Port < BluetoothEndPoint.MinPsm || bep.Port > BluetoothEndPoint.MaxPsm)
            //    throw new ArgumentOutOfRangeException("bep", "Channel Number must be in the range xxx1 to xxx30.");
        }

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
            var livePort16 = checked((UInt16)livePort);
            ServiceRecordHelper.SetL2CapPsmNumber(fullServiceRecord,
                livePort16);
            m_sdpService = SdpService.CreateCustom(fullServiceRecord, m_factory);
        }

        protected override void AddSimpleServiceRecord(out ServiceRecord fullServiceRecord,
            int livePort, Guid serviceClass, string serviceName)
        {
#if SdpService_CreateL2Cap
            var livePort16 = checked((UInt16)livePort);
            //m_sdpService = SdpService.CreateL2Cap(
            //    serviceClass,
            //    serviceName, livePort16, m_factory);
#endif
            ServiceRecordBuilder bldrDummy = new ServiceRecordBuilder();
            bldrDummy.ProtocolType = BluetoothProtocolDescriptorType.L2Cap;
            bldrDummy.AddServiceClass(serviceClass);
            bldrDummy.ServiceName = serviceName;
            fullServiceRecord = bldrDummy.ServiceRecord;
#if SdpService_CreateL2Cap
            ServiceRecordHelper.SetL2CapChannelNumber(fullServiceRecord,
                livePort16);
#else
            AddCustomServiceRecord(ref fullServiceRecord, livePort);
#endif
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
            // HACK !! skipping l2capif.Dispose/Destroy/CL2CapIf.Destroy !!");
            Utils.MiscUtils.Trace_WriteLine("!! skipping l2capif.Dispose/Destroy/CL2CapIf.Destroy !!");
            //->rfCommIf.Dispose(disposing);
        }

        protected override void OtherDisposeMore()
        {
            if (m_sdpService != null)
                m_sdpService.Dispose();
        }

        //----
        protected override CommonRfcommStream GetNewPort()
        {
            CommonRfcommStream port = WidcommL2CapClient.GetWidcommL2CapStreamWithThisIf(
                m_factory, _rfCommIf__tmp);
            return port;
        }

        protected override IBluetoothClient GetBluetoothClientForListener(
            CommonRfcommStream strm)
        {
            return WidcommL2CapClient.factory_DoGetBluetoothClientForListener(
                m_factory, strm);
        }
    }
}
