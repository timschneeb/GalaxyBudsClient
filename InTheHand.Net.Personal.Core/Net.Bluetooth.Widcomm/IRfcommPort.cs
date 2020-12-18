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

namespace InTheHand.Net.Bluetooth.Widcomm
{
    internal interface IRfcommPort
    {
        // To get to its HandleDataReceive and HandleEvent methods.
        void SetParentStream(WidcommRfcommStreamBase parent);
        void Create();
        void Destroy();
        //
        PORT_RETURN_CODE OpenClient(int scn, byte[] address);
        PORT_RETURN_CODE OpenServer(int scn);
        PORT_RETURN_CODE Write(byte[] p_data, UInt16 len_to_write, out UInt16 p_len_written);
        PORT_RETURN_CODE Close();
        bool IsConnected(out BluetoothAddress p_remote_bdaddr);
        string DebugId { get;}
    }


    internal enum PORT_RETURN_CODE
    {
        SUCCESS,
        UNKNOWN_ERROR,
        ALREADY_OPENED,         // Client tried to open port to existing DLCI/BD_ADDR
        NOT_OPENED,             // Function called before conn opened, or after closed
        LINE_ERR,               // Line error
        START_FAILED,           // Connection attempt failed
        PAR_NEG_FAILED,         // Parameter negotiation failed, currently only MTU
        PORT_NEG_FAILED,        // Port negotiation failed
        PEER_CONNECTION_FAILED, // Connection ended by remote side
        PEER_TIMEOUT,           // Timeout by remote side.
        INVALID_PARAMETER,      // One or more of the function parameters were invalid.
        //
        NotSet = 0x8000,
    }


    /// <summary>
    /// Define RFCOMM Port events that registered application can receive in the callback
    /// </summary>
    [Flags]
    internal enum PORT_EV : uint
    {
        /// <summary>
        /// Any Character received
        /// </summary>
        RXCHAR = 0x00000001,
        /// <summary>
        /// Received certain character
        /// </summary>
        RXFLAG = 0x00000002,
        /// <summary>
        /// Transmitt Queue Empty
        /// </summary>
        TXEMPTY = 0x00000004,
        /// <summary>
        /// CTS changed state
        /// </summary>
        CTS = 0x00000008,
        /// <summary>
        /// DSR changed state
        /// </summary>
        DSR = 0x00000010,
        /// <summary>
        /// RLSD changed state
        /// </summary>
        RLSD = 0x00000020,
        /// <summary>
        /// Ring signal detected
        /// </summary>
        BREAK = 0x00000040,
        /// <summary>
        /// Line status error occurred
        /// </summary>
        ERR = 0x00000080,
        /// <summary>
        /// Ring signal detected
        /// </summary>
        RING = 0x00000100,
        /// <summary>
        /// CTS state
        /// </summary>
        CTSS = 0x00000400,
        /// <summary>
        /// DSR state
        /// </summary>
        DSRS = 0x00000800,
        /// <summary>
        /// RLSD state
        /// </summary>
        RLSDS = 0x00001000,
        /// <summary>
        /// receiver buffer overrun
        /// </summary>
        OVERRUN = 0x00002000,
        /// <summary>
        /// Any character transmitted
        /// </summary>
        TXCHAR = 0x00004000,

        /// <summary>
        /// RFCOMM connection established
        /// </summary>
        CONNECTED = 0x00000200,
        /// <summary>
        /// Was not able to establish connection; or disconnected
        /// </summary>
        CONNECT_ERR = 0x00008000,
        /// <summary>
        /// flow control enabled flag changed by remote
        /// </summary>
        FC = 0x00010000,
        /// <summary>
        /// flow control status true = enabled
        /// </summary>
        FCS = 0x00020000,

        //
        //// To register for RFCOMM events application should provide bitmask with 
        //// corresponding bit set
        ////
        //#define PORT_MASK_ALL             (PORT_EV_RXCHAR | PORT_EV_TXEMPTY | PORT_EV_CTS | \
        //                                   PORT_EV_DSR | PORT_EV_RLSD | PORT_EV_BREAK | \
        //                                   PORT_EV_ERR | PORT_EV_RING | PORT_EV_CONNECT_ERR | \
        //                                   PORT_EV_DSRS | PORT_EV_CTSS | PORT_EV_RLSDS | \
        //                                   PORT_EV_RXFLAG | PORT_EV_TXCHAR | PORT_EV_OVERRUN | \
        //                                   PORT_EV_CONNECTED | PORT_EV_FC | PORT_EV_FCS)
    }
}
