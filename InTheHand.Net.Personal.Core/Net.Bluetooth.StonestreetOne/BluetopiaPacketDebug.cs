// 32feet.NET - Personal Area Networking for .NET
//
// Net.Bluetooth.StonestreetOne.BluetopiaPacketDebug
// 
// Copyright (c) 2011 Alan J.McFarlane, All rights reserved.
// Copyright (c) 2011 In The Hand Ltd, All rights reserved.
// This source code is licensed under the In The Hand Community License - see License.txt

using System;
using System.Collections.Generic;
using System.Text;
using InTheHand.Net.Bluetooth.StonestreetOne;
using System.IO;
using System.Runtime.InteropServices;
using System.Diagnostics;
using Utils;
using System.Diagnostics.CodeAnalysis;

namespace InTheHand.Net.Bluetooth.StonestreetOne
{
    class BluetopiaPacketDebug : IDisposable
    {
        private static void Stream_WriteInt32(Stream dst, Int32 value)
        {
            byte[] arr = BitConverter.GetBytes(value);
            dst.Write(arr, 0, arr.Length);
        }

        private static void Stream_WriteInt32(Stream dst, UInt32 value)
        {
            Stream_WriteInt32(dst, unchecked((Int32)value));
        }

        private static void Stream_WriteInt16(Stream dst, Int16 value)
        {
            byte[] arr = BitConverter.GetBytes(value);
            dst.Write(arr, 0, arr.Length);
        }

        //=========================================
        readonly BluetopiaFactory _fcty;
        readonly NativeMethods.BSC_Debug_Callback_t _Handler;
#if RAW_FILE
        Stream _dest;
#endif
        Stream _destPcap;
        readonly bool WithPhdr = true;
        const int OffsetOfData = 8;
        byte[] _sharedBuf = new byte[3];
        //
        const long HundredNsInMsec = 10000;
        readonly long _tickTimeOffset;

        internal BluetopiaPacketDebug(BluetopiaFactory fcty)
            : this(fcty, File.Create(@"\packets.pcap"))
        {
        }

        internal BluetopiaPacketDebug(BluetopiaFactory fcty, Stream pcapFile)
        {
            _fcty = fcty;
            _Handler = HandleDebug;
            var ret = NativeMethods.BSC_RegisterDebugCallback(_fcty.StackId, _Handler, 0);
            BluetopiaUtils.Assert(ret, "BSC_RegisterDebugCallback");
            //
            var hundNs = DateTime.UtcNow.Ticks;
            var msec = Environment.TickCount;
            _tickTimeOffset = hundNs - msec * HundredNsInMsec;
#if RAW_FILE
            _dest = File.Create(@"\packets.dat");
#endif
            _destPcap = pcapFile;
            WritePcapGlobalHeader(_destPcap);
            //
#if DEBUG
            byte testIsByteRange = checked((byte)StackConsts.HCI_PacketType.__HCIAdditional);
            _sharedBuf[0] = testIsByteRange; // shut-up compiler
#if !NETCF // :-(
            Debug.Assert(new IntPtr(OffsetOfData) == Marshal.OffsetOf(
                typeof(Structs.HCI_Packet), "_startOf_HCIPacketData"));
#endif
#endif
        }

        #region IDisposable Members
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (disposing) {
#if RAW_FILE
                if (_dest != null) _dest.Close();
#endif
                if (_destPcap != null) _destPcap.Close();
            }
        }
        #endregion

#if RAW_FILE
        byte[] PadTemplate = { 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20,
                        0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20 /*0x20*/ };
#endif

        void HandleDebug(uint BluetoothStackID, Boolean PacketSent,
            IntPtr pHCIPacket, uint CallbackParameter)
        {
            var dt = DateTime.UtcNow;
            var hciPacket = (Structs.HCI_Packet)Marshal.PtrToStructure(
                pHCIPacket, typeof(Structs.HCI_Packet));
            var t = hciPacket._HCIPacketType;
            int originalLen = (int)hciPacket._HCIPacketLength;
            int snapShotLen;
            if (ShouldLogPacket(t, originalLen, out snapShotLen)) {
                byte[] data = ReadData(pHCIPacket, hciPacket, snapShotLen);
#if RAW_FILE
                WriteToRawFile(_dest, PacketSent, pHCIPacket, hciPacket, t,
                    data, snapShotLen, originalLen);
#endif
                WritePacketToPcap(_destPcap, dt, t, data, snapShotLen, originalLen,
                    PacketSent);
            }
        }

        private static bool ShouldLogPacket(StackConsts.HCI_PacketType t,
            int originalLen, out int snapShotLen)
        {
            if (t == StackConsts.HCI_PacketType.HCICommandPacket
                    || t == StackConsts.HCI_PacketType.HCIEventPacket) {
                snapShotLen = originalLen;
                return true;
            }
            if (t == StackConsts.HCI_PacketType.HCIACLDataPacket) {
                const int MaxSnap = 300; //----32;
                snapShotLen = Math.Min(MaxSnap, originalLen);
                return true;
            }
            snapShotLen = -1;
            return false;
        }

        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "hciPacket")]
        private byte[] ReadData(IntPtr pHCIPacket, Structs.HCI_Packet hciPacket, int len)
        {
            byte[] data = GetBufferOfAtLeast(len);
            var pData = Pointers.Add(pHCIPacket, OffsetOfData);
            Marshal.Copy(pData, data, 0, len);
            return data;
        }

#if RAW_FILE
        private void WriteToRawFile(Stream dst, Boolean PacketSent,
            IntPtr pHCIPacket, Structs.HCI_Packet hciPacket,
            StackConsts.HCI_PacketType t,
            byte[] data, int snapShotLen, int originalLen)
        {
            dst.WriteByte(PacketSent ? (byte)1 : (byte)0);
            dst.WriteByte((byte)t);
            Stream_WriteInt32(dst, snapShotLen);
            Stream_WriteInt32(dst, snapShotLen);
            var count = 1 + 1 + 4 + 4;
            //
            dst.Write(data, 0, snapShotLen);
            count += snapShotLen;
            //
            var over16 = count % 16;
            if (over16 != 0) {
                dst.Write(PadTemplate, 0, 16 - over16);
            }
        }
#endif

        private byte[] GetBufferOfAtLeast(int len)
        {
            if (_sharedBuf.Length < len) {
                _sharedBuf = new byte[len];
            }
            return _sharedBuf;
        }

        #region pcap
        // http://wiki.wireshark.org/Development/LibpcapFileFormat 2011-May-06
        //typedef struct pcap_hdr_s {
        //        guint32 magic_number;   /* magic number */
        //        guint16 version_major;  /* major version number */
        //        guint16 version_minor;  /* minor version number */
        //        gint32  thiszone;       /* GMT to local correction */
        //        guint32 sigfigs;        /* accuracy of timestamps */
        //        guint32 snaplen;        /* max length of captured packets, in octets */
        //        guint32 network;        /* data link type */
        //} pcap_hdr_t;
        //
        //readonly byte[] NOT_USED_PcapGlobalheader = {
        //    // The writer uses native byte-ordering and the reader can tell from
        //    // the magic number what byte-ordering it being used.  We will write
        //    // in little-endian
        //    0xd4,0xc3,0xb2,0xa1, // magic_number = 0xa1b2c3d4
        //    0x02,0, // version
        //    0x04,0, //  -"-
        //    0,0,0,0, // thiszone -- we'll use UTC
        //    0,0,0,0, // sigfigs -- not used
        //    0xFF,0xFF,0,0, // TODO write the correct maximum snapshot length.
        //    // http://www.tcpdump.org/linktypes.html
        //    //LINKTYPE_BLUETOOTH_HCI_H4 187 DLT_BLUETOOTH_HCI_H4 Bluetooth HCI UART transport layer; the frame contains an HCI packet indicator byte, as specified by Volume 4, part A of the Core Version 4.0 of the Bluetooth specifications, followed by an HCI packet of the specified packet type, as specified by Volume 2, Part E of the same document.  
        //    0xBB,0,0,0, // LINKTYPE_BLUETOOTH_HCI_H4 = 187 = 0xBB
        //};
        const int LINKTYPE_BLUETOOTH_HCI_H4 = 0xBB;
        const int LINKTYPE_BLUETOOTH_HCI_H4_WITH_PHDR = 0xC9;

        void WritePcapGlobalHeader(Stream dst)
        {
            Stream_WriteInt32(dst, 0xa1b2c3d4); // magic_number
            Stream_WriteInt16(dst, 2); // version
            Stream_WriteInt16(dst, 4); //  -"-
            Stream_WriteInt32(dst, 0); // thiszone -- we'll use UTC
            Stream_WriteInt32(dst, 0); // sigfigs -- not used
            Stream_WriteInt32(dst, 65535); // TO-DO write the correct maximum snapshot length.
            int networkId = WithPhdr ? LINKTYPE_BLUETOOTH_HCI_H4_WITH_PHDR
                : LINKTYPE_BLUETOOTH_HCI_H4;
            Stream_WriteInt32(dst, networkId);
        }

        void WritePcapPacketHeader(Stream dst, DateTime dt, int snapShotLen, int originalLen)
        {
            //typedef struct pcaprec_hdr_s {
            //        guint32 ts_sec;         /* timestamp seconds */
            //        guint32 ts_usec;        /* timestamp microseconds */
            //        guint32 incl_len;       /* number of octets of packet saved in file */
            //        guint32 orig_len;       /* actual length of packet */
            //} pcaprec_hdr_t;
            uint secs;
            int muSecs;
            secs = ToUnixTime(dt, out muSecs);
            //
            Stream_WriteInt32(dst, secs);
            Stream_WriteInt32(dst, muSecs);
            Stream_WriteInt32(dst, snapShotLen);
            Stream_WriteInt32(dst, originalLen);
        }

        private static void WritePhdr(Stream dst, bool directionIsSent)
        {
            // The second version of the file format has a direction indicator.
            // https://bugs.wireshark.org/bugzilla/show_bug.cgi?id=1751
            // libpcap_read_bt_pseudoheader()
            int dir = directionIsSent ? 0 : 1;
            dir = System.Net.IPAddress.HostToNetworkOrder(dir);
            Stream_WriteInt32(dst, dir);
        }

        void WritePacketToPcap(Stream dst, DateTime dt,
            StackConsts.HCI_PacketType h4PacketType,
            byte[] packet, int snapShotLen, int originalLen,
            bool directionIsSent)
        {
            Debug.Assert(snapShotLen <= originalLen);
            const int SizeOfH4Header = 1;
            const int SizeOfPHdr = 4;
            int optionalSizeOfPHdr = (WithPhdr ? SizeOfPHdr : 0);
            WritePcapPacketHeader(dst, dt,
                snapShotLen + SizeOfH4Header + optionalSizeOfPHdr,
                originalLen + SizeOfH4Header + optionalSizeOfPHdr);
            if (WithPhdr) {
                WritePhdr(dst, directionIsSent);
            }
            dst.WriteByte((byte)h4PacketType);
            dst.Write(packet, 0, snapShotLen);
        }
        #endregion

        #region Time
        readonly DateTime UnixEpoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        private uint ToUnixTime__(DateTime dt, out int usecs)
        {
            var fromEpoch = dt - UnixEpoch;
            //
            const int TicksInSecond = 10000000; // 100ns
            long lSecs, lNsecs;
            // No DivRem in NETCF!!! lSecs = Math.DivRem(fromEpoch.Ticks, TicksInSecond, out lNsecs);
            lSecs = fromEpoch.Ticks / TicksInSecond;
            lNsecs = fromEpoch.Ticks % TicksInSecond;
            long lUsecs = lNsecs / 1000;
            long lMsecsDBG = (lUsecs / 1000) * 1000;
            //
            Debug.Assert(lUsecs < 1000000);
            usecs = (int)lUsecs;
            return (uint)lSecs;
        }

#if !DateTime_Ticks___NoFractionsOnNetcf
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "dt")]
#endif
        private uint ToUnixTime(DateTime dt, out int usecs)
        {
            // Arghhhhhhh!!! "In Windows CE, time is specific only to the
            //   second. You can get a more precise time span measurement,
            //   for example, in milliseconds, by using the TickCount
            //   property."
            //
#if DateTime_Ticks___NoFractionsOnNetcf
            return ToUnixTime__(dt)
#endif
            //
            long tickTime = unchecked((uint)Environment.TickCount) * HundredNsInMsec
                + _tickTimeOffset;
            var dt2 = new DateTime(tickTime, DateTimeKind.Utc);
            return ToUnixTime__(dt2, out usecs);
        }
        #endregion
    }//class
}
