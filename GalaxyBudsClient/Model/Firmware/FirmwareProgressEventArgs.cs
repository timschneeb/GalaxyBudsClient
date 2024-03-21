using System;

namespace GalaxyBudsClient.Model.Firmware;

public class FirmwareProgressEventArgs(int percent, long currentEstimatedByteCount, long totalByteCount)
    : EventArgs
{
    public int Percent { get; } = percent;
    public long CurrentEstimatedByteCount { get; } = currentEstimatedByteCount;
    public long TotalByteCount { get; } = totalByteCount;
}