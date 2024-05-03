using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using GalaxyBudsClient.Model.Constants;

namespace GalaxyBudsClient.Model.Database;

public class HistoryRecord
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }
    
    public PlacementStates PlacementL { get; set; }
    public PlacementStates PlacementR { get; set; }
    
    public int? BatteryL { get; set; }
    public int? BatteryR { get; set; }
    public int? BatteryCase { get; set; }
    
    public bool? IsChargingL { get; set; }
    public bool? IsChargingR { get; set; }
    public bool? IsChargingCase { get; set; }
    
    public DevicesInverted? HostDevice { get; set; }
    public NoiseControlModes? NoiseControlMode { get; set; }

    public DateTime Timestamp { get; set; }

    private sealed class HistoryRecordEqualityComparer : IEqualityComparer<HistoryRecord>
    {
        public bool Equals(HistoryRecord? x, HistoryRecord? y)
        {
            if (ReferenceEquals(x, y)) return true;
            if (ReferenceEquals(x, null)) return false;
            if (ReferenceEquals(y, null)) return false;
            if (x.GetType() != y.GetType()) return false;
            return x.PlacementL == y.PlacementL && x.PlacementR == y.PlacementR && 
                   x.BatteryL == y.BatteryL && x.BatteryR == y.BatteryR && x.BatteryCase == y.BatteryCase &&
                   x.IsChargingL == y.IsChargingL && x.IsChargingR == y.IsChargingR && x.IsChargingCase == y.IsChargingCase && 
                   x.HostDevice == y.HostDevice && x.NoiseControlMode == y.NoiseControlMode;
        }

        public int GetHashCode(HistoryRecord obj)
        {
            var hashCode = new HashCode();
            hashCode.Add((int)obj.PlacementL);
            hashCode.Add((int)obj.PlacementR);
            hashCode.Add(obj.BatteryL);
            hashCode.Add(obj.BatteryR);
            hashCode.Add(obj.BatteryCase);
            hashCode.Add(obj.IsChargingL);
            hashCode.Add(obj.IsChargingR);
            hashCode.Add(obj.IsChargingCase);
            hashCode.Add(obj.HostDevice);
            hashCode.Add(obj.NoiseControlMode);
            return hashCode.ToHashCode();
        }
    }

    public static IEqualityComparer<HistoryRecord> HistoryRecordComparer { get; } = new HistoryRecordEqualityComparer();
}