using System;
using InTheHand.Net.Bluetooth;
using InTheHand.Net.Bluetooth.AttributeIds;

namespace InTheHand.Net.Bluetooth
{


    /// <summary>
    /// Gets a list of enum-like classes containing SDP Service Attribute Id definitions 
    /// for a particular Service Class.
    /// </summary>
    /// -
    /// <remarks>
    /// See method 
    /// <see cref="M:InTheHand.Net.Bluetooth.MapServiceClassToAttributeIdList.GetAttributeIdEnumTypes(InTheHand.Net.Bluetooth.ServiceRecord)"/>.
    /// </remarks>
    public class MapServiceClassToAttributeIdList
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="T:InTheHand.Net.Bluetooth.MapServiceClassToAttributeIdList"/> class.
        /// </summary>
        public MapServiceClassToAttributeIdList()        
        { }

        /// <summary>
        /// Get a list of enum-like classes containing Service Attribute Id definitions 
        /// for the type of the Service Class contained in the given Service Record.
        /// </summary>
        /// -
        /// <param name="record">A <see cref="T:InTheHand.Net.Bluetooth.ServiceRecord"/>
        /// whose <see cref="F:InTheHand.Net.Bluetooth.AttributeIds.UniversalAttributeId.ServiceClassIdList"/>
        /// element will be retrieved, and its Service Class Id will used
        /// for the lookup.
        /// </param>
        /// -
        /// <returns>
        /// An array of <see cref="T:System.Type"/> each of which is a enum-like class 
        /// which defines the set of Service Attribute IDs used by a particular 
        /// Service Class e.g. ObjectPushProfile.
        /// An empty array will be returned if none of the Service Classes
        /// are known, or the record contains no 
        /// <see cref="F:InTheHand.Net.Bluetooth.AttributeIds.UniversalAttributeId.ServiceClassIdList"/>
        /// attribute, or it is invalid.
        /// <note>Currently only the first Service Class Id is looked-up.</note>
        /// </returns>
        /// -
        /// <exception cref="T:System.ArgumentNullException">
        /// <paramref name="record"/> is null.
        /// </exception>
        public Type[] GetAttributeIdEnumTypes(ServiceRecord record)
        {
            if (record == null) { throw new ArgumentNullException("record"); }
            //
            ServiceAttribute attr;
            try {
                attr = record.GetAttributeById(UniversalAttributeId.ServiceClassIdList);
            } catch (
#if V1
                ArgumentException
#else
                System.Collections.Generic.KeyNotFoundException
#endif
                    ex) {
                System.Diagnostics.Debug.Assert(ex.Message == ServiceRecord.ErrorMsgNoAttributeWithId);
                goto InvalidRecord;
            }
            ServiceElement element = attr.Value;
            if (element.ElementType != ElementType.ElementSequence) {
                goto InvalidRecord;
            }
            ServiceElement[] idElements = element.GetValueAsElementArray();
            //TODO ((GetServiceClassSpecificAttributeIdEnumDefiningType--foreach (ServiceElement curIdElem in idElements) {))
            if (idElements.Length != 0) {
                ServiceElement curIdElem = idElements[0];
                Type enumType = GetAttributeIdEnumType(curIdElem);
                if (enumType != null) {
                    return new Type[] { enumType };
                }
            }//else fall through...
        // None-matched, or invalid attribute format etc.
        InvalidRecord:
            return new Type[0];
        }

        /// <summary>
        /// Get the enum-like class containing the Service Attribute Id definitions 
        /// for the type of the Service Class contained in the given 
        /// <see cref="F:InTheHand.Net.Bluetooth.AttributeIds.UniversalAttributeId.ServiceClassIdList"/>
        /// (type <see cref="F:InTheHand.Net.Bluetooth.ElementTypeDescriptor.Uuid"/>) data element.
        /// </summary>
        /// -
        /// <param name="idElement">A <see cref="T:InTheHand.Net.Bluetooth.ServiceElement"/>
        /// of 'UUID' type containing the Service Class to search for.
        /// </param>
        /// -
        /// <returns>
        /// A <see cref="T:System.Type"/> object representing the enum-like class
        /// holding the Attribute Id definitions, or null if the Service Class is
        /// unknown or the element is not of <see cref="F:InTheHand.Net.Bluetooth.ElementTypeDescriptor.Uuid"/>
        /// type.
        /// </returns>
        /// -
        /// <exception cref="T:System.ArgumentNullException">
        /// <paramref name="idElement"/> is null.
        /// </exception>
        protected virtual Type GetAttributeIdEnumType(ServiceElement idElement)
        {
            if (idElement == null) { throw new ArgumentNullException("idElement"); }
            //
            if (idElement.ElementTypeDescriptor != ElementTypeDescriptor.Uuid) {
                return null;
            }
            Guid uuid = idElement.GetValueAsUuid();
            //
            return GetAttributeIdEnumType(uuid);
        }

        /// <overloads>
        /// Get the enum-like class containing the Service Attribute Id definitions 
        /// for the type of the Service Class specified.
        /// </overloads>
        /// -
        /// <summary>
        /// Get the enum-like class containing the Service Attribute Id definitions 
        /// for the type of the Service Class specified by UUID.
        /// </summary>
        /// -
        /// <param name="uuid">The Service Class to search for, as a <see cref="T:System.Guid"/>.
        /// </param>
        /// -
        /// <returns>
        /// A <see cref="T:System.Type"/> object representing the enum-like class
        /// holding the Attribute Id definitions, or null if the Service Class is
        /// unknown.
        /// </returns>
        protected virtual Type GetAttributeIdEnumType(Guid uuid)
        {
            foreach (ServiceClassToIdsMapRow cur in m_serviceClassToIdsMapTable) {
                if (uuid == cur.ServiceClassId) { return cur.AttributeIdEnumType; }
            }//for
            //
            return null;
        }

        private struct ServiceClassToIdsMapRow
        {
            public readonly Guid ServiceClassId;
            public readonly Type AttributeIdEnumType;

            public ServiceClassToIdsMapRow(Guid ServiceClassId,Type AttributeIdEnumType)
            {
                this.ServiceClassId = ServiceClassId;
                this.AttributeIdEnumType = AttributeIdEnumType;
            }
        }

        private ServiceClassToIdsMapRow[] m_serviceClassToIdsMapTable = {
            // Note BluetoothService.ServiceDiscoveryServer has a wrong value in 32feet.NET v2.1 and earlier!
            new ServiceClassToIdsMapRow(BluetoothService.ServiceDiscoveryServer, typeof(ServiceDiscoveryServerAttributeId)), //0x1000
            //
            new ServiceClassToIdsMapRow(BluetoothService.IrMCSync, typeof(ObexAttributeId)), //0x1104
            new ServiceClassToIdsMapRow(BluetoothService.ObexObjectPush, typeof(ObexAttributeId)),   //0x1105
            new ServiceClassToIdsMapRow(BluetoothService.ObexFileTransfer, typeof(ObexAttributeId)), //0x1106
            new ServiceClassToIdsMapRow(BluetoothService.IrMCSyncCommand, typeof(ObexAttributeId)),  //0x1107
            //
            // The Service Class ID used in the ServiceClassList seems to have changed 
            // between HSP version 1.1 and 1.2.
            new ServiceClassToIdsMapRow(BluetoothService.Headset, typeof(HeadsetProfileAttributeId)),  //0x1108
            new ServiceClassToIdsMapRow(BluetoothService.HeadsetHeadset, typeof(HeadsetProfileAttributeId)),  //0x1131
            //AUX-new Foo(BluetoothService.GenericAudio, typeof(HspAttributeId)), //0x1203
            //
            new ServiceClassToIdsMapRow(BluetoothService.Panu, typeof(PersonalAreaNetworkingProfileAttributeId)), //0x1115
            new ServiceClassToIdsMapRow(BluetoothService.Nap, typeof(PersonalAreaNetworkingProfileAttributeId)),  //0x1116
            new ServiceClassToIdsMapRow(BluetoothService.GN, typeof(PersonalAreaNetworkingProfileAttributeId)),   //0x1117
            //
            new ServiceClassToIdsMapRow(BluetoothService.DirectPrinting, typeof(BasicPrintingProfileAttributeId)),   //0x1118-
            new ServiceClassToIdsMapRow(BluetoothService.ReferencePrinting, typeof(BasicPrintingProfileAttributeId)),    //0x1119-
            new ServiceClassToIdsMapRow(BluetoothService.DirectPrintingReferenceObjects, typeof(BasicPrintingProfileAttributeId)),   //0x1120-
            new ServiceClassToIdsMapRow(BluetoothService.ReflectedUI, typeof(BasicPrintingProfileAttributeId)),  //0x1121-
            new ServiceClassToIdsMapRow(BluetoothService.BasicPrinting, typeof(BasicPrintingProfileAttributeId)),    //0x1122
            new ServiceClassToIdsMapRow(BluetoothService.PrintingStatus, typeof(BasicPrintingProfileAttributeId)),   //0x1123-
            //
            // The profile specific Attributes are on both ends
            new ServiceClassToIdsMapRow(BluetoothService.Handsfree, typeof(HandsFreeProfileAttributeId)),    //0x111E
            new ServiceClassToIdsMapRow(BluetoothService.HandsfreeAudioGateway, typeof(HandsFreeProfileAttributeId)),    //0x111F
            //AUX-new Foo(BluetoothService.GenericAudio, typeof(HfpAttributeId)),
            //
            new ServiceClassToIdsMapRow(BluetoothService.HumanInterfaceDevice, typeof(HidProfileAttributeId)),    //0x1124
            new ServiceClassToIdsMapRow(BluetoothService.PnPInformation, typeof(DeviceIdProfileAttributeId)),    //0x1200
        };

    }//class

}
