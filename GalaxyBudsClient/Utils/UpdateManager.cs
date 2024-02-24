using System.Threading.Tasks;
using NetSparkleUpdater;
using NetSparkleUpdater.Enums;
using NetSparkleUpdater.SignatureVerifiers;

namespace GalaxyBudsClient.Utils
{
    public class UpdateManager
    {
        private static readonly object Padlock = new object();
        private static UpdateManager? _instance;
        public static UpdateManager Instance
        {
            get
            {
                lock (Padlock)
                {
                    return _instance ??= new UpdateManager();
                }
            }
        }

        public static void Init()
        {
            lock (Padlock)
            { 
                _instance ??= new UpdateManager();
            }
        }

        public SparkleUpdater Core { get; }

        private UpdateManager()
        {
            Core = new SparkleUpdater("https://timschneeberger.me/updates/galaxybudsclient/appcast.xml", new Ed25519Checker(SecurityMode.Unsafe))
            {
                SecurityProtocolType = System.Net.SecurityProtocolType.Tls12
            };
            Core.StartLoop(false, false);
        }

        public async Task<UpdateStatus> DoManualCheck()
        {
            var result = await Core.CheckForUpdatesAtUserRequest();
            if (result == null)
            {
                return UpdateStatus.CouldNotDetermine;
            }

            if (result.Status == UpdateStatus.UpdateAvailable)
            { 
                MainWindow.Instance.UpdatePage.SetUpdate(result.Updates, false);
            }
            
            return result.Status;
        }

        public async Task SilentCheck()
        {
            var result = await Core.CheckForUpdatesQuietly();

            if (result?.Status == UpdateStatus.UpdateAvailable)
            { 
                MainWindow.Instance.UpdatePage.SetUpdate(result.Updates, true);
            }
        }
    }
}