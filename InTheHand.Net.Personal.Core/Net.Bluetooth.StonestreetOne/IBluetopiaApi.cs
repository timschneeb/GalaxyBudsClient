// 32feet.NET - Personal Area Networking for .NET
//
// Net.Bluetooth.StonestreetOne.IBluetopiaApi
// 
// Copyright (c) 2010-2011 Alan J.McFarlane, All rights reserved.
// Copyright (c) 2010-2011 In The Hand Ltd, All rights reserved.
// This source code is licensed under the In The Hand Community License - see License.txt

using BD_ADDR_BY_VALUE = System.Int64;
//
using System;

namespace InTheHand.Net.Bluetooth.StonestreetOne
{
    interface IBluetopiaApi
    {
        #region BSCAPI.h
        /// <summary>
        /// 
        /// </summary>
        /// -
        /// <remarks>
        /// <para>
        /// The following function is responsible for Initializing a Bluetooth
        /// Protocol Stack for the specified Bluetooth Device (using the      
        /// specified HCI Transport).  This function *MUST* be called (and    
        /// complete successfully) before any function in this module can be  
        /// called.
        /// </para>
        ///<para>Once this function completes the specified Bluetooth 
        /// Protocol Stack ID will remain valid for the specified Bluetooth   
        /// Device until the Bluetooth Stack is Closed via a call to the      
        /// BSC_Shutdown() function.                                          
        ///          </para>
        /// </remarks>
        /// -
        /// <param name="HCI_DriverInformation">
        /// The first parameter specifies the Bluetooth HCI Driver   
        /// Transport Information to use when opening the Bluetooth Device    
        /// and the second parameter specifies Flags that are to be used to   
        /// alter the base Bluetooth Protocol Stack Functionality.  The HCI   
        /// Driver Information parameter is NOT optional and must specify a   
        /// valid Bluetooth HCI Driver transport provided by this Protocol    
        /// Stack Implementation.  
        /// </param>
        /// <param name="Flags">
        /// The flags parameter should be zero unless  
        /// altered functionality is required.
        /// * NOTE * The Bit Mask values for the Flags Parameter are defined  
        ///          at the top of this file.                                 
        /// </param>
        /// -
        /// <returns>
        /// Upon successfuly completion,  
        /// this function will return a positive, non-zero, return value.     
        /// This value will be used as input to functions provided by the     
        /// Bluetooth Protocol Stack that require a Bluetooth Protocol Stack  
        /// ID (functions that operate directly on a Bluetooth Device).  If   
        /// this function fails, the return value will be a negative return   
        /// code which specifies the error (see error codes defined           
        /// elsewhere).  
        /// </returns>
        int BSC_Initialize(ref Structs.HCI_DriverInformation__HCI_COMMDriverInformation HCI_DriverInformation,
            /*unsigned long*/StackConsts.BSC_INITIALIZE_FLAGs Flags);
        int BSC_Initialize(byte[] HCI_DriverInformation,
            /*unsigned long*/StackConsts.BSC_INITIALIZE_FLAGs Flags);

        void BSC_Shutdown(uint BluetoothStackID);

        BluetopiaError BSC_Read_RSSI(uint BluetoothStackID, BD_ADDR_BY_VALUE BD_ADDR, out sbyte RSSI);

        #endregion

        #region GAPAPI.h
        BluetopiaError GAP_Authenticate_Remote_Device(uint BluetoothStackID, BD_ADDR_BY_VALUE BD_ADDR,
            NativeMethods.GAP_Event_Callback GAP_Event_Callback, uint CallbackParameter);
        BluetopiaError GAP_Register_Remote_Authentication(uint BluetoothStackID,
            NativeMethods.GAP_Event_Callback GAP_Event_Callback, uint CallbackParameter);
        BluetopiaError GAP_Authentication_Response(uint BluetoothStackID, BD_ADDR_BY_VALUE BD_ADDR,
            ref Structs.GAP_Authentication_Information GAP_Authentication_Information);

        BluetopiaError GAP_Initiate_Bonding(uint BluetoothStackID, BD_ADDR_BY_VALUE BD_ADDR,
            StackConsts.GAP_Bonding_Type GAP_Bonding_Type,
            NativeMethods.GAP_Event_Callback GAP_Event_Callback, uint CallbackParameter);

        BluetopiaError GAP_Query_Local_BD_ADDR(uint BluetoothStackID, /*"BD_ADDR_t *"*/byte[] BD_ADDR);
        BluetopiaError GAP_Query_Local_Device_Name(uint BluetoothStackID, int NameBufferLength, byte[] NameBuffer);
        BluetopiaError GAP_Query_Class_Of_Device(uint BluetoothStackID, out uint/*"Class_of_Device_t*"*/ Class_of_Device);
        //
        BluetopiaError GAP_Query_Connectability_Mode(uint BluetoothStackID,
            out StackConsts.GAP_Connectability_Mode GAP_Connectability_Mode);
        BluetopiaError GAP_Set_Connectability_Mode(uint BluetoothStackID,
            StackConsts.GAP_Connectability_Mode GAP_Connectability_Mode);
        BluetopiaError GAP_Query_Discoverability_Mode(uint BluetoothStackID,
            out StackConsts.GAP_Discoverability_Mode GAP_Discoverability_Mode,
            out uint Max_Discoverable_Time);
        BluetopiaError GAP_Set_Discoverability_Mode(uint BluetoothStackID,
            StackConsts.GAP_Discoverability_Mode GAP_Discoverability_Mode, uint Max_Discoverable_Time);
        BluetopiaError GAP_Set_Pairability_Mode(uint BluetoothStackID,
            StackConsts.GAP_Pairability_Mode GAP_Pairability_Mode);
        //
        BluetopiaError GAP_Query_Remote_Device_Name(uint BluetoothStackID, BD_ADDR_BY_VALUE BD_ADDR,
            NativeMethods.GAP_Event_Callback GAP_Event_Callback, uint CallbackParameter);
        //

        /// <summary>
        /// The following function is provided to allow a mechanism for       
        /// Starting an Inquiry Scan Procedure.
        /// </summary>
        /// -
        /// <remarks>
        /// <para>/// * NOTE * Only ONE Inquiry can be performed at any given time.     
        ///          Calling this function while an outstanding Inquiry is    
        ///          in progress will fail.  The caller can call the          
        ///          GAP_Cancel_Inquiry() function to cancel a currently      
        ///          executing Inquiry procedure.                             
        /// </para>
        /// <para>* NOTE * The Minimum and Maximum Inquiry Parameters are optional  
        ///          and, if specified, represent the Minimum and Maximum     
        ///          Periodic Inquiry Periods.  The caller should set BOTH    
        ///          of these values to zero if a simple Inquiry procedure    
        ///          is to be used (Non-Periodic).  If these two parameters   
        ///          are specified, then then these two parameters must       
        ///          satisfy the following formula:                           
        ///                                                                   
        ///          MaximumPeriodLength > MinimumPeriodLength > InquiryLength
        /// </para>
        /// <para>* NOTE * All Inquiry Period Time parameters are specified in      
        ///          seconds.                                                 
        /// </para>
        /// </remarks>
        /// -
        /// <param name="BluetoothStackID">
        /// The first parameter to this  
        /// function is the Bluetooth Protocol Stack of the Bluetooth Device  
        /// that is to perform the Inquiry.
        /// </param>
        /// <param name="GAP_Inquiry_Type">
        /// The second parameter is the Type 
        /// of Inquiry to perform.
        /// </param>
        /// <param name="MinimumPeriodLength">
        /// The third and fourth parameters are the   
        /// Minimum and Maximum Period Lengths (only valid in case a Periodic 
        /// Inquiry is to be performed).
        /// </param>
        /// <param name="MaximumPeriodLength">
        /// </param>
        /// <param name="InquiryLength">
        /// The fifth parameter is the Length   
        /// of Time to perform the Inquiry.
        /// </param>
        /// <param name="MaximumResponses">
        /// The sixth parameter is the       
        /// Number of Responses to Wait for.
        /// </param>
        /// <param name="GAP_Event_Callback">
        /// The final two parameters        
        /// represent the Callback Function (and parameter) that is to be     
        /// called when the specified Inquiry has completed.
        /// </param>
        /// <param name="CallbackParameter">
        /// </param>
        /// -
        /// <returns>
        /// This function   
        /// returns zero if successful, or a negative return error code if    
        /// an Inquiry was unable to be performed.                            
        /// </returns>
        BluetopiaError GAP_Perform_Inquiry(uint BluetoothStackID, StackConsts.GAP_Inquiry_Type GAP_Inquiry_Type,
            uint MinimumPeriodLength, uint MaximumPeriodLength,
            uint InquiryLength, uint MaximumResponses,
            NativeMethods.GAP_Event_Callback GAP_Event_Callback, uint CallbackParameter);

        BluetopiaError GAP_Query_Local_Out_Of_Band_Data(uint BluetoothStackID,
            IntPtr/*"Simple_Pairing_Hash_t *"*/SimplePairingHash, IntPtr/*"Simple_Pairing_Randomizer_t *"*/SimplePairingRandomizer);
        #endregion

        #region HCIAPI.h
        // Function 'HCI_Version_Supported' is cheaper, it does not access the controller.
        BluetopiaError HCI_Read_Local_Version_Information(uint BluetoothStackID,
            out StackConsts.HCI_ERROR_CODE StatusResult,
            out HciVersion HCI_VersionResult, out ushort HCI_RevisionResult,
            out LmpVersion LMP_VersionResult, out Manufacturer Manufacturer_NameResult, out ushort LMP_SubversionResult);
        BluetopiaError HCI_Read_Local_Supported_Features(uint BluetoothStackID,
            out StackConsts.HCI_ERROR_CODE StatusResult, byte[] LMP_FeaturesResult);
        #endregion

        #region SPPAPI.h
        /// The following function is responsible for Opening a Remote Serial 
        /// Port on the specified Remote Device.  This function accepts the   
        /// Bluetooth Stack ID of the Bluetooth Stack which is to open the    
        /// Serial Connection as the first parameter.  The second parameter   
        /// specifies the Board Address (NON NULL) of the Remote Bluetooth    
        /// Device to connect with.  The next parameter specifies the Remote  
        /// Server Channel ID to connect.  The final two parameters specify   
        /// the SPP Event Callback function, and callback parameter,          
        /// respectively, of the SPP Event Callback that is to process any    
        /// further interaction with the specified Remote Port (Opening       
        /// Status, Data Writes, etc).  This function returns a non-zero,     
        /// positive, value if successful, or a negative return error code if 
        /// this function is unsuccessful.  If this function is successful,   
        /// the return value will represent the Serial Port ID that can be    
        /// passed to all other functions that require it.  Once a Serial Port
        /// is opened, it can only be closed via a call to the                
        /// SPP_Close_Port() function (passing the return value from this     
        /// function).    
        /// <summary>
        /// 
        /// </summary>
        int SPP_Open_Remote_Port(uint BluetoothStackID, BD_ADDR_BY_VALUE BD_ADDR, uint ServerPort,
            NativeMethods.SPP_Event_Callback SPP_Event_Callback, uint CallbackParameter);
        //__DLLMODEF__ int BTPSAPI SPP_Open_Remote_Port(unsigned int BluetoothStackID, BD_ADDR_t BD_ADDR, unsigned int ServerPort, SPP_Event_Callback_t SPP_Event_Callback, unsigned long CallbackParameter);

        BluetopiaError SPP_Close_Port(uint BluetoothStackID, uint SerialPortID);

        int SPP_Open_Server_Port(uint BluetoothStackID, uint ServerPort,
            NativeMethods.SPP_Event_Callback SPP_Event_Callback, uint CallbackParameter);

        BluetopiaError SPP_Close_Server_Port(uint BluetoothStackID, uint SerialPortID);

        BluetopiaError SPP_Register_SDP_Record(uint BluetoothStackID, uint SerialPortID,
            ref Structs.SPP_SDP_Service_Record SDPServiceRecord,
            byte[] ServiceNameUtf8, out uint SDPServiceRecordHandle);

        /// <summary>
        /// The following function is responsible for Reading Serial Data from
        /// the specified Serial Connection.  
        /// </summary>
        /// -
        /// <param name="BluetoothStackID">
        /// The input parameters to this function are
        /// the Bluetooth Stack ID of the Bluetooth Stack that the second     
        /// parameter is valid for (Serial Port Identifier)
        /// </param>
        /// <param name="SerialPortID">
        /// The SerialPortID that is passed 
        /// to this function MUST have been established by either Accepting   
        /// a Serial Port Connection (callback from the SPP_Open_Server_Port()
        /// function) or by initiating a Serial Port Connection (via calling  
        /// the SPP_Open_Remote_Port() function and having the remote side    
        /// accept the Connection).
        /// </param>
        /// <param name="DataBufferSize">
        /// the Size of the  
        /// Data Buffer to be used for reading and a pointer to the Data      
        /// Buffer.
        /// </param>
        /// <param name="DataBuffer">The data.
        /// </param>
        /// -
        /// <returns>
        /// This function returns the number of data bytes that were 
        /// successfully read (zero if there were no Data Bytes ready to be   
        /// read), or a negative return error code if unsuccessful.           
        /// </returns>
        int SPP_Data_Read(uint BluetoothStackID, uint SerialPortID, ushort DataBufferSize, byte[] DataBuffer);


        /// <summary>
        /// The following function is responsible for Sending Serial Data to  
        /// the specified Serial Connection.
        /// </summary>
        /// -
        /// <remarks>
        /// <para>
        /// The SerialPortID that is passed 
        /// to this function MUST have been established by either Accepting   
        /// a Serial Port Connection (callback from the SPP_Open_Server_Port()
        /// function) or by initiating a Serial Port Connection (via calling  
        /// the SPP_Open_Remote_Port() function and having the remote side    
        /// accept the Connection).  
        /// </para>
        /// <para>
        /// * NOTE * If this function is unable to send all of the data that  
        ///          was specified (via the DataLength parameter), this       
        ///          function will return the number of bytes that were       
        ///          actually sent (zero or more, but less than the DataLength
        ///          parameter value).  When this happens (and ONLY when      
        ///          this happens), the user can expect to be notified when   
        ///          the Serial Port is able to send data again via the       
        ///          etPort_Transmit_Buffer_Empty_Indication SPP Event.  This 
        ///          will allow the user a mechanism to know when the Transmit
        ///          Buffer is empty so that more data can be sent.           
        /// </para>
        /// </remarks>
        /// -
        /// <param name="BluetoothStackID">
        /// The input parameters to this function are
        /// the Bluetooth Stack ID of the Bluetooth Stack
        /// </param>
        /// <param name="SerialPortID">
        /// the second     
        /// parameter is valid for (Serial Port Identifier)
        /// </param>
        /// <param name="DataLength">
        /// the Length of    
        /// the Data to send
        /// </param>
        /// <param name="DataBuffer">
        /// a pointer to the Data Buffer to Send.
        /// </param>
        /// -
        /// <returns>
        /// This  
        /// function returns the number of data bytes that were successfully  
        /// sent, or a negative return error code if unsuccessful.
        /// </returns>
        int SPP_Data_Write(uint BluetoothStackID, uint SerialPortID, ushort DataLength, byte[] DataBuffer);
        #endregion

        #region SDPAPI.h
        int SDP_Service_Search_Attribute_Request(uint BluetoothStackID,
            BD_ADDR_BY_VALUE BD_ADDR,
            uint NumberServiceUUID, Structs.SDP_UUID_Entry[] SDP_UUID_Entry,
            uint NumberAttributeListElements, Structs.SDP_Attribute_ID_List_Entry[] AttributeIDList,
            NativeMethods.SDP_Response_Callback SDP_Response_Callback, uint CallbackParameter);
        //__DLLMODEF__ int BTPSAPI SDP_Service_Search_Attribute_Request(unsigned int BluetoothStackID, BD_ADDR_t BD_ADDR, unsigned int NumberServiceUUID, SDP_UUID_Entry_t SDP_UUID_Entry[], unsigned int NumberAttributeListElements, SDP_Attribute_ID_List_Entry_t AttributeIDList[], SDP_Response_Callback_t SDP_Response_Callback, unsigned long CallbackParameter);

        int SDP_Create_Service_Record(uint BluetoothStackID,
            uint NumberServiceClassUUID, Structs.SDP_UUID_Entry_Bytes[] SDP_UUID_Entry);

        /// <summary>
        /// The following function is responsible for Deleting a SDP Service
        /// Record that was added with the SDP_Create_Service_Record()
        /// function.
        /// </summary>
        /// -
        /// <remarks>
        /// <para>
        /// This function deletes the specified SDP Service Record
        /// and deletes ALL SDP Attributes that are associated with the
        /// specified Service Record.
        /// </para>
        /// </remarks>
        /// -
        /// <param name="BluetoothStackID">
        /// The Bluetooth Stack ID
        /// of the Bluetooth Protocol Stack that the SDP Server resides on.
        /// </param>
        /// <param name="Service_Record_Handle">
        /// The SDP Service Record Handle to delete from the specified
        /// SDP Server. The second parameter to this function is obtained
        /// via a successful call to the SDP_Create_Service_Record()
        /// function.
        /// </param>
        /// -
        /// <returns>
        /// This function returns zero if the
        /// specified Service Record was able to be deleted successfully, or
        /// a negative return error code if the Service Record was NOT able to
        /// be deleted successfully.  If this function completes successfully
        /// the Service Record is NO longer valid on the specified SDP
        /// Server.
        /// </returns>
        BluetopiaError SDP_Delete_Service_Record(uint BluetoothStackID, uint Service_Record_Handle);

        BluetopiaError SDP_Add_Attribute(uint BluetoothStackID, uint Service_Record_Handle,
            ushort Attribute_ID, ref Structs.SDP_Data_Element__Struct SDP_Data_Element);
        [Obsolete("For test")]
        BluetopiaError SDP_Add_Attribute(uint BluetoothStackID, uint Service_Record_Handle,
            ushort Attribute_ID, Structs.SDP_Data_Element__Class SDP_Data_Element);
        #endregion
    }
}
