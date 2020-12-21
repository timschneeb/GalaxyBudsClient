using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Markup;
using System.Xml;
using Serilog;

namespace ThePBone.Interop.Win32.TrayIcon
{
    internal static class ResourceLoader
    {
        private static readonly string TempDir = Path.Combine(Path.GetTempPath(), "TempResourcesGBC");
        
        internal static T FindResource<T>(string name)
        {
            return (T)Application.Current.FindResource(name);
        }

         internal static T LoadXamlFromManifest<T>(string resource)
         {
            var stream = Assembly.GetExecutingAssembly()
                .GetManifestResourceStream(resource);
            string path = Path.Combine(TempDir, $"Resource_{DateTime.Now.Ticks}_{resource}");

            try
            {
                Directory.CreateDirectory(TempDir);


                stream?.Seek(0, SeekOrigin.Begin);

                using (FileStream fs = new FileStream(path, FileMode.Create))
                {
                    stream?.CopyTo(fs);
                    fs.Flush();
                }

            }
            catch (Exception ex)
            {
                Log.Error("Win32.TrayIcon: unable to unpack resources: " + ex.Message);
            }

            using (var xmlReader = XmlReader.Create(File.OpenText(path)))
                return (T)XamlReader.Load(xmlReader);
        }

         internal static void ClearCache()
         {
             try
             {
                 Directory.Delete(TempDir, true);
             }
             catch (Exception ex)
             {
                 Log.Error("Win32.TrayIcon: unable to clean cache: " + ex.Message);
             }
         }
    }
}
