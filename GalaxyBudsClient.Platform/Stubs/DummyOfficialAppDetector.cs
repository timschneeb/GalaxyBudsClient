using System.Threading.Tasks;
using GalaxyBudsClient.Platform.Interfaces;

namespace GalaxyBudsClient.Platform.Stubs;

public class DummyOfficialAppDetector : IOfficialAppDetector
{
    public Task<bool> IsInstalledAsync() => Task.FromResult(false);
}