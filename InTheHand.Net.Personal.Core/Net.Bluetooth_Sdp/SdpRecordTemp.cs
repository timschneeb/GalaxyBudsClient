// 32feet.NET
//
// InTheHand.Net.Bluetooth.SdpRecordTemp
// 
// Copyright (c) 2003-2006 In The Hand Ltd, All rights reserved.
// This source code is licensed under the In The Hand Community License - see License.txt

using System;
#if ! V1
using List_ServiceAttribute = System.Collections.Generic.List<InTheHand.Net.Bluetooth.ServiceAttribute>;
#else
using List_ServiceAttribute = System.Collections.ArrayList;
#endif
using InTheHand.Net.Bluetooth.AttributeIds;

namespace InTheHand.Net.Bluetooth
{
	/// <summary>
	/// Dummy SDP record into which we can insert the Guid and channel
	/// </summary>
	internal class SdpRecordTemp
	{
		byte[] m_data;
        ServiceRecord m_record;
        readonly int m_expectedRecordLength = 41;

		public SdpRecordTemp()
		{
			m_data = new byte[] {0x35,0x27,0x09,0x00,0x01,0x35,0x11,0x1c,
									0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,
									0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,
									0x09,0x00,0x04,0x35,0x0c,0x35,0x03,0x19,
									0x01,0x00,0x35,0x05,0x19,0x00,0x03,0x08,
									0x00};
            m_record = CreateRecord();
            m_expectedRecordLength = 41;
        }

		public Guid Service
		{
			get
			{
                // HACK SdpRecordTemp.get_Service is not callable, it is effectively private!
                Guid uuid = GetRecordService(m_record);
                //old
                byte[] guidbytes = new byte[16];
                Buffer.BlockCopy(m_data, 8, guidbytes, 0, 16);
                Guid hostGuid = new Guid(guidbytes);
                Guid netGuid = Sockets.BluetoothListener.HostToNetworkOrder(hostGuid);
                if (uuid != netGuid) {
                    System.Diagnostics.Debug.Fail("Internal error--Service UUID not equal.");
                }
                //
                return netGuid;
			}
			set
			{
                SetRecordService(m_record, value);
                //old
                Guid hostGuid = Sockets.BluetoothListener.HostToNetworkOrder(value);
                Buffer.BlockCopy(hostGuid.ToByteArray(), 0, m_data, 8, 16);
			}
		}

		public byte Channel
		{
			get
			{
                // HACK SdpRecordTemp.get_Channel is not callable, it is effectively private!
                int value = ServiceRecordHelper.GetRfcommChannelNumber(m_record);
                byte channel = (byte)value;
                //old
                byte orig_channel = m_data[m_data.Length - 1];
                if (orig_channel != channel) {
                    System.Diagnostics.Debug.Fail("Internal error--Channel Number not equal.");
                }
                //
                return channel;
			}
			set
			{
                ServiceRecordHelper.SetRfcommChannelNumber(m_record, value);
                m_data[m_data.Length - 1] = value;
			}
		}

		public byte[] ToByteArray()
		{
            byte[] newRecordBytes = new byte[m_expectedRecordLength];
            int length = new ServiceRecordCreator().CreateServiceRecord(m_record, newRecordBytes);
            System.Diagnostics.Debug.Assert(length == m_expectedRecordLength);
            //old
            for (int i = 0; i < m_data.Length; ++i) {
                if (m_data[i] != newRecordBytes[i]) {
                    System.Diagnostics.Debug.Fail(String.Format(
                        "Internal error--Records different at index {0} was 0x{1} is 0x{2}.",
                        i, m_data[i], newRecordBytes[i]));
                }
            }//for
			return newRecordBytes;
		}

        //--------------------------------------------------------------
        private static ServiceRecord CreateRecord()
        {
            List_ServiceAttribute attrs = new List_ServiceAttribute();
            ServiceElement element;
            //
            element = new ServiceElement(ElementType.ElementSequence,
                new ServiceElement(ElementType.Uuid128, Guid.Empty));
            attrs.Add(new ServiceAttribute(UniversalAttributeId.ServiceClassIdList, element));
            //
            element = ServiceRecordHelper.CreateRfcommProtocolDescriptorList();
            attrs.Add(new ServiceAttribute(UniversalAttributeId.ProtocolDescriptorList, element));
            //
            ServiceRecord record = new ServiceRecord(attrs);
            return record;
        }

        private static void SetRecordService(ServiceRecord m_record, Guid uuid128)
        {
            ServiceAttribute attr = m_record.GetAttributeById(UniversalAttributeId.ServiceClassIdList);
            ServiceElement element = (ServiceElement)attr.Value.GetValueAsElementList()[0];
            element.SetValue(uuid128);
        }

        private Guid GetRecordService(ServiceRecord m_record)
        {
            ServiceAttribute attr = m_record.GetAttributeById(UniversalAttributeId.ServiceClassIdList);
            ServiceElement element = (ServiceElement)attr.Value.GetValueAsElementList()[0];
            Guid uuid128 = (Guid)element.Value;
            return uuid128;
        }

    }//class
}
