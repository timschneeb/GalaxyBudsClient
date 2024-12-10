using System.Threading.Tasks;

namespace GalaxyBudsClient.Platform.Interfaces;

public interface IOfficialAppDetector
{
    public Task<bool> IsInstalledAsync();
}