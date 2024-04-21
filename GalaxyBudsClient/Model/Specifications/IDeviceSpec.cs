using System;
using System.Collections.Generic;
using GalaxyBudsClient.Interface.Developer;
using GalaxyBudsClient.Message;
using GalaxyBudsClient.Model.Constants;
using GalaxyBudsClient.Model.Specifications.Touch;
using Serilog;

namespace GalaxyBudsClient.Model.Specifications;

public interface IDeviceSpec
{
    public Dictionary<Features, FeatureRule?> Rules { get; }
    public Models Device { get; }
    public string DeviceBaseName { get; }
    public ITouchMap TouchMap { get; }
    public Guid ServiceUuid { get; }
    public IEnumerable<TrayItemTypes> TrayShortcuts { get; }
    public string IconResourceKey { get; }
    public int MaximumAmbientVolume { get; }
    public byte StartOfMessage { get; }
    public byte EndOfMessage { get; }
        
    bool Supports(Features arg) => Supports(arg, null);
    
    public bool Supports(Features feature, int? extendedStatusRevision)
    {
        if (TranslatorTools.GrantAllFeaturesForTesting)
            return true;
            
        if (!Rules.TryGetValue(feature, out var value))
        {
            return false;
        }

        if (value == null)
        {
            return true;
        }

        if (extendedStatusRevision == null && DeviceMessageCache.Instance.ExtendedStatusUpdate?.Revision == null)
        {
            Log.Warning("IDeviceSpec: Cannot compare revision for {Feature}. No ExtendedStatusUpdate cached", feature);
            return true;
        }
        
        return (extendedStatusRevision != null && extendedStatusRevision >= value.MinimumExtendedStatusRevision) ||
               DeviceMessageCache.Instance.ExtendedStatusUpdate?.Revision >= value.MinimumExtendedStatusRevision;
    }
}
    
public class FeatureRule(
    int minimumExtendedStatusRevision, 
    int? minimumStatusRevision = null)
{
    public int MinimumExtendedStatusRevision { get; } = minimumExtendedStatusRevision;
    public int? MinimumStatusRevision { get; } = minimumStatusRevision;
}