// 32feet.NET - Personal Area Networking for .NET
//
// InTheHand.Net.Bluetooth.Msft.BTHNS_SETBLOB
// 
// Copyright (c) 2003-2006,2011 In The Hand Ltd, All rights reserved.
// This source code is licensed under the MIT License

using System;
using System.Runtime.InteropServices;
using InTheHand.Runtime.InteropServices;

namespace InTheHand.Net.Bluetooth.Msft
{

	// <summary>
	// This structure is passed to the BthNsSetService function through the lpqsRegInfo->lpBlob member and contains information on the new service.
	// </summary>
	internal class BTHNS_SETBLOB : BTHNS_BLOB
	{

		private const int BTH_SDP_VERSION = 1;

		private GCHandle pVersionHandle;
		private GCHandle pRecordHandle;

		/* ULONG __RPC_FAR *pSdpVersion;
		ULONG __RPC_FAR *pRecordHandle;
		ULONG Reserved[ 4 ];
		ULONG fSecurity;
		ULONG fOptions;
		ULONG ulRecordLength;
		UCHAR pRecord[ 1 ];
	
		public IntPtr pRecordHandle;
		private uint fSecurity;
		private uint fOptions;
		public uint ulRecordLength;
		public byte pRecord;*/

        //
        // * Win32
        //typedef struct _BTH_SET_SERVICE {
        //        PULONG pSdpVersion;
        //        HANDLE *pRecordHandle;
        //        ULONG fCodService;    // COD_SERVICE_* bits
        //        ULONG Reserved[5];    // Reserved by system.  Must be zero.                
        //        ULONG ulRecordLength; // length of pRecord which follows
        //        UCHAR pRecord[1];     // SDP record as defined by bluetooth spec
        //}
        // * WinCE
        //struct _BTHNS_SETBLOB
        //{
        //    ULONG __RPC_FAR *pSdpVersion;
        //    ULONG __RPC_FAR *pRecordHandle;
        //    ULONG Reserved[ 4 ];
        //    ULONG fSecurity;
        //    ULONG fOptions;
        //    ULONG ulRecordLength;
        //    UCHAR pRecord[ 1 ];
        //}


		public BTHNS_SETBLOB(byte[] record)
		{
            BTHNS_SETBLOB.AssertCheckLayout();
			//create data buffer
			m_data = new byte[StructLength_36 + record.Length];
			pVersionHandle = GCHandle.Alloc(BTH_SDP_VERSION, GCHandleType.Pinned);
			pRecordHandle = GCHandle.Alloc((IntPtr)0, GCHandleType.Pinned);
			IntPtr vaddr = pVersionHandle.AddrOfPinnedObject();
			IntPtr haddr = pRecordHandle.AddrOfPinnedObject();
			Marshal32.WriteIntPtr(m_data, Offset_pSdpVersion_0, vaddr);
			Marshal32.WriteIntPtr(m_data, Offset_pRecordHandle_4, haddr);
			BitConverter.GetBytes(record.Length).CopyTo(m_data, Offset_ulRecordLength_32);

			//copy sdp record
			Buffer.BlockCopy(record, 0, m_data, StructLength_36, record.Length);
		}

		public IntPtr Handle
		{
			get
			{
				IntPtr pHandle = Marshal32.ReadIntPtr(m_data, Offset_pRecordHandle_4);
				return Marshal32.ReadIntPtr(pHandle, 0);	
			}
			set
			{
				IntPtr pHandle = Marshal32.ReadIntPtr(m_data, Offset_pRecordHandle_4);
				Marshal32.WriteIntPtr(pHandle, 0, value);
			}
		}

        public uint CodService
        {
            get
            {
                return BitConverter.ToUInt32(m_data, Offset_fCodService_8);
            }
            set
            {
                BitConverter.GetBytes(value).CopyTo(m_data, Offset_fCodService_8);
            }
        }

		#region IDisposable Members

		protected override void Dispose(bool disposing)
		{
			try
			{
				if(pVersionHandle.IsAllocated)
				{
					pVersionHandle.Free();
				}

				if(pRecordHandle.IsAllocated)
				{
					pRecordHandle.Free();
				}

			}
			finally
			{
				base.Dispose(disposing);
			}
		}
		#endregion

        #region Field Offsets
        private static readonly int Offset_pSdpVersion_0 = 0;
        private static readonly int Offset_pRecordHandle_4 = 1 * IntPtr.Size;
        private static readonly int Offset_fCodService_8 = 2 * IntPtr.Size;
        private static readonly int Offset_ulRecordLength_32 = 2 * IntPtr.Size + 6 * 4;
        private static readonly int StructLength_36 = 2 * IntPtr.Size + 7 * 4;

        static bool s_doneAssert;

        [System.Diagnostics.Conditional("DEBUG")]
        public static void AssertCheckLayout()
        {
            if (s_doneAssert)
                return;
            s_doneAssert = true;
#if !NETCF
            if (IntPtr.Size == 4) {
                System.Diagnostics.Debug.Assert(Offset_pSdpVersion_0 == 0, "Offset_pSdpVersion_0");
                System.Diagnostics.Debug.Assert(Offset_pRecordHandle_4 == 4, "Offset_pRecordHandle_4");
                System.Diagnostics.Debug.Assert(Offset_fCodService_8 == 8, "Offset_fCodService_8");
                System.Diagnostics.Debug.Assert(Offset_ulRecordLength_32 == 32, "Offset_ulRecordLength_32");
                System.Diagnostics.Debug.Assert(StructLength_36 == 36, "StructLength_36");
            } else {
                System.Diagnostics.Debug.Assert(Offset_pSdpVersion_0 == 0, "Offset_pSdpVersion_0");
                System.Diagnostics.Debug.Assert(Offset_pRecordHandle_4 == 8, "Offset_pRecordHandle_4");
                System.Diagnostics.Debug.Assert(Offset_fCodService_8 == 16, "Offset_fCodService_8");
                System.Diagnostics.Debug.Assert(Offset_ulRecordLength_32 == 40, "Offset_ulRecordLength_32");
                System.Diagnostics.Debug.Assert(StructLength_36 == 44, "StructLength_36");
            }
#endif
        }
        #endregion
    }
}
