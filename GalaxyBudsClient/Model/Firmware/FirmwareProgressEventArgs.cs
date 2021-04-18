using System;

namespace GalaxyBudsClient.Model.Firmware
{
    public class FirmwareProgressEventArgs : EventArgs
    {
        public FirmwareProgressEventArgs(int percent, long currentEstimatedByteCount, long totalByteCount)
        {
            Percent = percent;
            CurrentEstimatedByteCount = currentEstimatedByteCount;
            TotalByteCount = totalByteCount;
        }

        public int Percent { get; }
        public long CurrentEstimatedByteCount { get; }
        public long TotalByteCount { get; }
    }
}