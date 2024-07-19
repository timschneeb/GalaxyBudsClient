using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Android.Content;
using Android.Content.PM;
using GalaxyBudsClient.Platform.Interfaces;

#pragma warning disable CA1416

namespace GalaxyBudsClient.Android;

[SuppressMessage("ReSharper", "UnusedType.Global")]
public class OfficialAppDetector(Context context) : IOfficialAppDetector
{
    private readonly string[] _packages =
    [
        "com.samsung.accessory.fridaymgr",
        "com.samsung.accessory.popcornmgr",
        "com.samsung.accessory.neobeanmgr",
        "com.samsung.accessory.berrymgr",
        "com.samsung.accessory.zenithmgr",
        "com.samsung.accessory.pearlmgr",
        "com.samsung.accessory.paranmgr",
        "com.samsung.accessory.jellymgr"
    ];
    
    public Task<bool> IsInstalledAsync()
    {
        foreach (var package in _packages)
        {
            try
            {
                context.PackageManager?.GetPackageInfo(package, 0);
                return Task.FromResult(true);
            }
            catch (PackageManager.NameNotFoundException)
            {
                // ignored
            }
        }
        
        return Task.FromResult(false);
    }
}