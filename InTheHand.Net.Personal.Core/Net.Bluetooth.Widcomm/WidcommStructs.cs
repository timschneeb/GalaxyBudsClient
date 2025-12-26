// 32feet.NET - Personal Area Networking for .NET
//
// InTheHand.Net.Widcomm.WidcommSocketExceptions
// 
// Copyright (c) 2008-2009 In The Hand Ltd, All rights reserved.
// Copyright (c) 2008-2009 Alan J. McFarlane, All rights reserved.
// This source code is licensed under the MIT License

using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;
using System.Diagnostics;

namespace InTheHand.Net.Bluetooth.Widcomm
{
    static class WidcommStructs
    {
#if WinXP
        internal const int StructLayoutPack = 1;    // <BtIfClasses.h> has: #pragma pack (1)
#else
        internal const int StructLayoutPack = 0;    // Use platform default.
#endif

        internal const int BD_ADDR_LEN = 6;                   /* Device address length */
        internal const int BD_NAME_LEN = 248;
        internal const int DEV_CLASS_LEN = 3;
    }

    //------------------------------------------------------------------
#if WinXP
    [StructLayout(LayoutKind.Sequential, Pack = WidcommStructs.StructLayoutPack)]
#endif
    struct REM_DEV_INFO
    {
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = WidcommStructs.BD_ADDR_LEN)]
        internal byte[] bda;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = WidcommStructs.DEV_CLASS_LEN)]
        internal byte[] dev_class;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = WidcommStructs.BD_NAME_LEN)]
        internal byte[] bd_name;
        internal bool b_paired;
        internal bool b_connected;
    }

    //------------------------------------------------------------------
#if WinXP
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
#endif
    internal struct DEV_VER_INFO
    {
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = WidcommStructs.BD_ADDR_LEN)]
        internal byte[] bd_addr;           //   0  +6   |   0   +6
        internal readonly byte hci_version;         //   6  +1   |   6   +1 +
        internal readonly UInt16 hci_revision;      //   7  +2   |   8   +2
        internal readonly byte lmp_version;         //   9  +1   |  10   +1 +
        internal readonly UInt16 manufacturer;      //  10  +2   |  12   +2
        internal readonly UInt16 lmp_sub_version;   //  12  +2   |  14   +2
        //                   total size:       14       |  16
        //                                     Win32    |  WinCE

        internal DEV_VER_INFO(HciVersion fake)
            : this()
        {
            Debug.Assert(fake == HciVersion.Unknown, "This ctor only used to fill with UNKNOWN values.");
            SetManufacturerAndVersionsToUnknown(
                out manufacturer, out hci_version, out lmp_version);
        }

        internal static void SetManufacturerAndVersionsToUnknown(
            out UInt16 manufacturer, out byte hci_version, out byte lmp_version)
        {
            manufacturer = unchecked((UInt16)checked((short)Manufacturer.Unknown));
            hci_version = (byte)HciVersion.Unknown;
            lmp_version = (byte)LmpVersion.Unknown;
        }

        //
        public override string ToString()
        {
            return $"[DEV_VER_INFO: {{{BitConverter.ToString(bd_addr)}}}, {hci_version}, {hci_revision}, {lmp_version}, {manufacturer}, {lmp_sub_version}]";
        }
    }

    //------------------------------------------------------------------
    struct tBT_CONN_STATS
    {
        public readonly UInt32 bIsConnected;
        public readonly Int32 rssi;
        public readonly UInt32 bytesSent;
        public readonly UInt32 bytesRcvd;
        public readonly UInt32 duration;
    }


    enum WBtRc : uint
    {
        WBT_SUCCESS,
        WBT_ERROR,
        WBT_NO_SECURITY_API_OBJECT,
        WBT_SECURITY_ERR_CREATE_API,
        WBT_SECURITY_ERR_CANNOT_CREATE_API,
        WBT_SECURITY_ERR_INVALID_DISPATCH,

        WBT_NO_BTM_API_OBJECT,
        WBT_BTM_ERR_CANNOT_CREATE_API,
        WBT_BTM_ERR_INVALID_DISPATCH,

        WBT_NO_GAP_API_OBJECT,

        WBT_NO_FTP_API_OBJECT,
        WBT_FTP_ERR_CANNOT_CREATE_API,
        WBT_FTP_ERR_INVALID_DISPATCH,

        WBT_NO_SPP_API_OBJECT,
        WBT_SPP_ERR_CANNOT_CREATE_API,
        WBT_SPP_ERR_INVALID_DISPATCH,
        WBT_SPP_ERR_NO_API,
        WBT_SPP_ERR_NO_DISPATCH,
        WBT_SPP_ERROR,

        WBT_NO_DUN_API_OBJECT,
        WBT_DUN_ERR_CREATE_API,
        WBT_DUN_ERR_CANNOT_CREATE_API,
        WBT_DUN_ERR_INVALID_DISPATCH,
        WBT_DUN_ERR_NO_API,
        WBT_DUN_ERR_NO_DISPATCH,
        WBT_DUN_ERROR,
        
        WBT_NO_FAX_API_OBJECT,
        WBT_FAX_ERR_CREATE_API,
        WBT_FAX_ERR_CANNOT_CREATE_API,
        WBT_FAX_ERR_INVALID_DISPATCH,
        WBT_FAX_ERR_NO_API,
        WBT_FAX_ERR_NO_DISPATCH,
        WBT_FAX_ERROR,

        WBT_NO_LAP_API_OBJECT,
        WBT_LAP_ERR_CREATE_API,
        WBT_LAP_ERR_CANNOT_CREATE_API,
        WBT_LAP_ERR_INVALID_DISPATCH,
        WBT_LAP_ERR_NO_API,
        WBT_LAP_ERR_NO_DISPATCH,
        WBT_LAP_ERROR,  

        WBT_NO_OPP_API_OBJECT,
        WBT_OPP_ERR_CREATE_API,
        WBT_OPP_ERR_CANNOT_CREATE_API,
        WBT_OPP_ERR_INVALID_DISPATCH,
        WBT_OPP_ERROR,  

        WBT_NO_SYNC_API_OBJECT,
        WBT_SYNC_ERR_CANNOT_CREATE_API,
        WBT_SYNC_ERR_INVALID_DISPATCH,
        WBT_SYNC_ERR_NO_API,
        WBT_SYNC_ERROR,  

        WBT_NO_TRACE_API_OBJECT,
        WBT_TRACE_ERR_CANNOT_CREATE_API,
        WBT_TRACE_ERR_INVALID_DISPATCH,
        WBT_TRACE_ERR_NO_API,
        WBT_TRACE_ERR_MALLOC_SINK,
        WBT_TRACE_ERR_INVALID_SINK,
        WBT_TRACE_ERROR,  

        WBT_NO_HSP_API_OBJECT,
        WBT_HSP_ERR_CANNOT_CREATE_API,
        WBT_HSP_ERR_INVALID_DISPATCH,
        WBT_NO_HAG_API_OBJECT,
        WBT_HAG_ERR_CANNOT_CREATE_API,
        WBT_HAG_ERR_INVALID_DISPATCH,

        WBT_EXCEPTION,
        WBT_OLE_EXCEPTION,                      // Most commonly, receiving this error means the connection to
                                                // the COM server "btstackserver.exe" was lost
        WBT_C_EXCEPTION,
        WBT_COM_EXCEPTION,

        WBT_NO_HCRP_API_OBJECT,
        WBT_HCRP_ERR_CREATE_API,
        WBT_HCRP_ERR_CANNOT_CREATE_API,
        WBT_HCRP_ERR_INVALID_DISPATCH,
        WBT_HCRP_ERR_NO_API,
        WBT_HCRP_ERR_NO_DISPATCH,
        WBT_HCRP_ERROR,
        
        WBT_NO_VDP_API_OBJECT,
        WBT_VDP_ERR_CANNOT_CREATE_API,
        WBT_VDP_ERR_INVALID_DISPATCH,

        /* Errors returned from COM server */
        WBT_COM_ERR_BASE = 0x81000000,

        WBT_BUSY,                               // Busy with another operation
        WBT_ERR_SERVER_DOWN,
        WBT_ERR_ALLOC_SCN_FAILED,               
        WBT_ERR_SDP_FULL,                       
        WBT_ERR_RFCOMM_CONNECTION_FAILED,       
        WBT_ERR_SERVICE_NOT_FOUND,              
        WBT_ERR_UNKNOWN_PORT,                   
        WBT_ERR_TRANSPORT_NOT_FOUND,            
        WBT_ERR_PPP_START_FAILED,               
        WBT_ERR_RAS_NOT_LOADED,                 
        WBT_ERR_RAS_CONNECTION_SETUP_FAILED,    
        WBT_ERR_NO_MODEM_FOUND,                 
        WBT_ERR_LANACCESS_START_FAILED,         
        WBT_ERR_NOT_ENOUGH_MEMORY,              // 
        WBT_ERR_BAD_SCN,                        // invalid scn for connection
        WBT_ERR_ALLOC_SCN,                      // unable to allocate SCN
        WBT_ERR_CREATE_SDP_RECORD,              // unable to create SDP record
        WBT_ERR_BAD_CONFIG,
	    WBT_DIALING,                            // NOT an error, the stack is about to put up the dial dialog
        WBT_ERR_BOTH_DUN_LAP_UNSUPPORTED,       // simultaneous DUN & LAP not supported in this OS
        WBT_ERR_NO_AUDIO_PRESENT,               // BT Audio is not installed to start HSP or HAG
        WBT_ERR_COM_PORT_IN_USE,                // COM port used for connection is already in use

	    /* COM server errors returned from GapStartInquiry() */
        /*#define*/ __WBT_ERR_START_INQUIRY_OFFSET = 0x81000020,
	    WBT_ERR_BTM_BUSY = __WBT_ERR_START_INQUIRY_OFFSET + 2, /* Device busy with another command  */
	    WBT_ERR_BTM_NO_RESOURCES,               /* No resources to issue command     */
	    WBT_ERR_BTM_MODE_UNSUPPORTED,           /* Request for 1 or more unsupported modes */
	    WBT_ERR_BTM_ILLEGAL_VALUE,              /* Illegal parameter value           */
	    WBT_ERR_BTM_WRONG_MODE,                 /* No device found  */
	    WBT_ERR_BTM_LICENSE_ERROR,				/* BTW license unauthorized */

        /* Generic HCI errors passed to the application */
        WBT_ERR_GEN_HCI = 0x810000F0,

        WBT_ERR_AUTH_FAILURE,           // Authentication failed, redo pairing
        WBT_ERR_PAIRING_NOT_ALLOWED,    // Pairing is not allowed by peer device
        WBT_ERR_RESPONCE_TIMEOUT,       // Timeout occured during pairing
        WBT_ERR_PAGE_TIMEOUT,           // Device is not available
        WBT_ERR_NOT_SUPPPORTED,         // Pairing not supported
        WBT_ERR_REPEATED_ATTEMPTS,      // Repeated attempts

        /* FTP error codes returned from COM server */
        WBT_FTP_ERR_BASE = 0x81000100,

        WBT_FTP_ERR_BAD_ADDR,            /* bad BD_ADDR */
        WBT_FTP_ERR_HANDLE,              /* bad handle */
        WBT_FTP_ERR_NOT_UNIQUE_DEV,      /* specified device is not unique (i.e. multiple
                                         ** matching entries
                                         */
        WBT_FTP_ERR_NO_HANDLES,          /* reached session limit;  no handle available */
        WBT_FTP_ERR_BAD_STATE,           /* could not handle request in present state */
        WBT_FTP_ERR_EXISTS,              /* file exists */
        WBT_FTP_ERR_BAD_REQUEST,         /* invalid request */
        WBT_FTP_ERR_NOT_FOUND,           /* no such file */
        WBT_FTP_ERR_NO_SERVICE,          /* could not find the specified FTP server */
        WBT_FTP_ERR_DISCONNECT,          /* connection lost */
        WBT_FTP_ERR_READ,                /* read error */
        WBT_FTP_ERR_WRITE,               /* write error */
        WBT_FTP_ERR_MEM,                 /* could not allocate memory */
        WBT_FTP_ERR_OBEX_AUTH,           /* OBEX Authentication required */
        WBT_FTP_ERR_DENIED,              /* request could not be honored */
    //    WBT_FTP_ERR_BAD_LOCAL_FOLDER,    /* bad local folder */
        WBT_FTP_ERR_CONNECT,             /* error establishing connection */
        WBT_FTP_ERR_PERM,                /* incorrect permissions */
        WBT_FTP_ERR_SERVER_INTERNAL,     /* server error */
        WBT_FTP_ERR_NO_SERVER_SUPPORT,   /* server does not support the operation */
        WBT_FTP_ERR_NOT_INITIALIZED,     /* not initialized */
        WBT_FTP_ERR_PARAM,               /* bad parameter */
        WBT_FTP_ERR_PERMISSIONS,         /* incorrect file permissions */
	    WBT_FTP_ERR_FS_RESOURCES,        /* out of file system resources (handles, disk space, etc) */
	    WBT_FTP_ERR_SHARING,             /* file sharing violation */
        WBT_FTP_ERR_TIMEOUT,             /* timeout */
        WBT_FTP_ERR_PATH_TOO_BIG,

        /* OPP error codes returned from COM server */
        WBT_OPP_ERR_BASE = 0x81000120,

        WBT_OPP_ERR_BAD_ADDR,             /* bad BD_ADDR */
        WBT_OPP_ERR_HANDLE,               /* bad handle */
        WBT_OPP_ERR_NOT_UNIQUE,           /* specified device is not unique (i.e. multiple
                                          ** matching entries
                                          */
        WBT_OPP_ERR_NO_HANDLES,           /* reached session limit;  no handle available */
        WBT_OPP_ERR_BAD_STATE,            /* could not handle request in present state */
        WBT_OPP_ERR_EXISTS,               /* file exists */
        WBT_OPP_ERR_BAD_REQUEST,          /* invalid request */
        WBT_OPP_ERR_NOT_FOUND,            /* no such file */
        WBT_OPP_ERR_NO_SERVICE,           /* could not find the specified FTP server */
        WBT_OPP_ERR_DISCONNECT,           /* connection lost */
        WBT_OPP_ERR_READ,                 /* read error */
        WBT_OPP_ERR_WRITE,                /* write error */
        WBT_OPP_ERR_OBEX_AUTH,            /* OBEX Authentication required */
        WBT_OPP_ERR_DENIED,               /* request could not be honored */
        WBT_OPP_ERR_DATA_NOT_SUPPORTED,   /* server does not support the requested data */
        WBT_OPP_ERR_CONNECT,              /* error establishing connection */
        WBT_OPP_ERR_NOT_INITIALIZED,      /* not initialized */
        WBT_OPP_ERR_PARAM,                /* bad parameter */
	    WBT_OPP_ERR_BAD_INBOX,            /* inbox is not valid */
	    WBT_OPP_ERR_BAD_NAME,             /* bad name for object */
	    WBT_OPP_ERR_PERMISSIONS,          /* prohibited by file permissions */
	    WBT_OPP_ERR_SHARING,              /* file sharing violation */
	    WBT_OPP_ERR_FS_RESOURCES,         /* out of file system resources (handles, disk space, etc) */
        WBT_OPP_ERR_NO_DATATYPES_ACCEPTED,/* No OPP datatypes are checked for acceptance in the cpanel */
   	    WBT_OPP_ERR_PIM,                  /* problem with PIM */
        WBT_OPP_ERR_PARSE,                /* problem parsing object */
	    WBT_OPP_ERR_TIMEOUT,
        WBT_OPP_ERR_PATH_TOO_BIG,

        /* SYNC error codes returned from COM server */
        WBT_SYNC_ERR_BASE = 0x81000160,

        WBT_SYNC_ERR_NO_HANDLES,		   /* no handle available */
	    WBT_SYNC_ERR_HANDLE,			   /* bad handle */
	    WBT_SYNC_ERR_NOT_INITIALIZED,      /* not initialized */
	    WBT_SYNC_ERR_OBJECT_NOT_FOUND,	   /* an object to be deleted or modified was not there */
	    WBT_SYNC_ERR_ALREADY_STARTED,	   /* sync is already started */
	    WBT_SYNC_ERR_OPENING_FILE,		   /* couldn't open a file */
	    WBT_SYNC_ERR_SERVICE_UNAVAILABLE,  /* the IrMC server was not available */
	    WBT_SYNC_ERR_OBEX,				   /* general obex error */
	    WBT_SYNC_ERR_OBEX_CLOSED,		   /* the obex session was lost */
	    WBT_SYNC_ERR_OBEX_ABORTED,		   /* the obex session was aborted */
	    WBT_SYNC_ERR_BAD_OBEX_RESPONSE,	   /* there was a bad obex response to a request */
	    WBT_SYNC_ERR_PIM_NOT_PRESENT,	   /* there is no PIM present */
	    WBT_SYNC_ERR_ALREADY_SYNCHING,	   /* machine is already sychronizing */
    //    WBT_SYNC_ERR_SAVE_IN_PIM_DISABLED, /* "Save Objects in PIM" is not checked in the cpanel */
        WBT_SYNC_ERR_RETIRED1,              /* this code has been retired */
	    WBT_SYNC_ERR_BAD_FOLDER,           /* work folder is not there & could not be created */
        WBT_SYNC_ERR_IRMC_SERVER,          /* IrMC Server error */
        WBT_SYNC_ERR_TIMEOUT,              /* timeout */
        WBT_SYNC_ERR_PIM,                  /* PIM caused an error */
        WBT_SYNC_ABORTED,                  /* the sync session was aborted */

        /* SDP service discovery error codes returned from COM server */
        WBT_SDP_ERR_BASE = 0x81000180,

        WBT_SDP_ERR_CONNECT_ERR,		    /* Could not connect to device */
        WBT_SDP_ERR_CONNECT_REJ,            /* Remote device rejected the connection */
        WBT_SDP_ERR_SECURITY,		        /* Security failed */
        WBT_SDP_ERR_BAD_RECORD,             /* Remote Service Record Error */

        /* PAN error codes */
        WBT_PAN_ERR_BASE = 0x81000190,
        WBT_PAN_LOCAL_NAP_EXISTS,           /* somebody already connected to us as a NAP, disconnect first */
        WBT_PAN_REMOTE_NAP_EXISTS,          /* we are already connected to NAP, disconnect first */
        WBT_PAN_LOCAL_GN_EXISTS,            /* somebody already connected to GN, can not connect to NAP */
        WBT_PAN_INVALID_REQ,                /* invalid request from app */
        WBT_PAN_WRONG_ROLE,                 /* PAN is in invalid role state */
        WBT_PAN_NAP_BOTH_SIDES,             /* NAP is setup on both sides */
        
        /* General connection error codes */
        WBT_CONN_ERR_BASE = 0x810001A0,

        WBT_CONN_DISCONNECTED,               /* An established connection has disconnected */
        WBT_CONN_CONNECT_ERROR,              /* Could not connect to remote */
        WBT_CONN_SECURITY_ERROR,             /* Security error during connection */
        WBT_CONN_LOCAL_CLOSED,               /* Local application opened and closed the COM port */
        WBT_CONN_LOCAL_TIMEOUT,               /* Local connection has not been started within timeout */

        /* BIP error codes returned from COM server */
        WBT_BIP_ERR_BASE = 0x810001B0,

        WBT_BIP_ERR_NO_HANDLES,           /* reached session limit;  no handle available */
        WBT_BIP_ERR_HANDLE,               /* bad handle */
        WBT_BIP_ERR_NOT_INITIALIZED,      /* not initialized */
        WBT_BIP_ERR_PARAM,                /* bad parameter */

        /* Print error codes returned from COM server */
        WBT_PRINT_ERR_BASE = 0x810001C0,

        WBT_PRINT_ERR_NO_PRINTER_FOUND,   /* no printer found to serve */

        /* Audio on demand error codes returned from COM server to Tray */
        WBT_AUDIO_DEMAND_ERR_BASE = 0x810001D0,
        WBT_AUDIO_DEMAND_HEADSET_NOT_CONFIGURED,
        WBT_AUDIO_DEMAND_HEADSET_NOT_CONNECTED,
        WBT_AUDIO_DEMAND_HEADSET_FAILED_TO_CONNECT,
        WBT_HEADSET_CONNECT_PENDING,
        WBT_AUDIO_FEATURE_NOT_SUPPORTED,

        /* BPP error codes */
        WBT_BPP_ERR_BASE = 0x820001B0,

        WBT_BPP_ERR_NO_HANDLES,           /* reached session limit;  no handle available */
        WBT_BPP_ERR_HANDLE,               /* bad handle */
        WBT_BPP_ERR_NOT_INITIALIZED,      /* not initialized */
        WBT_BPP_ERR_PARAM,                /* bad parameter */

        /* Voice Recognition error/status codes */
        WBT_VOICE_RECO_ERR_BASE = 0x820001C0,
        WBT_VOICE_RECO_STATUS_STARTED,  /* Started Voice recognition on local connection successfully */
        WBT_VOICE_RECO_STATUS_FAILED,   /* Failed to start Voice recognition on local connection */
        WBT_VOICE_RECO_STATUS_STOPPED,  /* Stopped Voice recognition on local connection */
        WBT_VOICE_RECO_STATUS_DIAL_COMMAND,     /* Dail Command received */
        WBT_VOICE_RECO_STATUS_ONE,              /* User said one */
        WBT_VOICE_RECO_STATUS_TWO,              /* User said two */
        WBT_VOICE_RECO_STATUS_THREE,            /* User said three */
        WBT_VOICE_RECO_STATUS_FOUR,             /* User said four */
        WBT_VOICE_RECO_STATUS_FIVE,             /* User said five */
        WBT_VOICE_RECO_STATUS_SIX,              /* User said six */
        WBT_VOICE_RECO_STATUS_SEVEN,            /* User said seven */
        WBT_VOICE_RECO_STATUS_EIGHT,            /* User said eight */
        WBT_VOICE_RECO_STATUS_NINE,             /* User said nine */
        WBT_VOICE_RECO_STATUS_ZERO,             /* User said zero */    
        WBT_VOICE_RECO_STATUS_CALL_COMMAND,    /* Call Command received */    
        WBT_VOICE_RECO_STATUS_MOVE_PROGRESS,    /* Dial timer ticked one */
        WBT_VOICE_RECO_STATUS_DIAL_DONE        /* Dial number done */
    
}
}
