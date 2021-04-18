using System;

namespace GalaxyBudsClient.Model.Firmware
{
    public class FirmwareBlockChangedEventArgs : EventArgs
    {
        public FirmwareBlockChangedEventArgs(int segmentId, int offset, int offsetEnd, byte packetCount, int segmentSize, int segmentCrc32)
        {
            SegmentId = segmentId;
            Offset = offset;
            OffsetEnd = offsetEnd;
            PacketCount = packetCount;
            SegmentSize = segmentSize;
            SegmentCrc32 = segmentCrc32;
        }

        public int SegmentId { get; }
        public int Offset { get; }
        public int OffsetEnd { get; }
        public byte PacketCount { get; }
        public int SegmentSize { get; }
        public int SegmentCrc32 { get; }
    }
}