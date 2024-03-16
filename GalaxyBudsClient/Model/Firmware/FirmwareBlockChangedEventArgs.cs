using System;

namespace GalaxyBudsClient.Model.Firmware
{
    public class FirmwareBlockChangedEventArgs(
        int segmentId,
        int offset,
        int offsetEnd,
        byte packetCount,
        int segmentSize,
        int segmentCrc32)
        : EventArgs
    {
        public int SegmentId { get; } = segmentId;
        public int Offset { get; } = offset;
        public int OffsetEnd { get; } = offsetEnd;
        public byte PacketCount { get; } = packetCount;
        public int SegmentSize { get; } = segmentSize;
        public int SegmentCrc32 { get; } = segmentCrc32;
    }
}