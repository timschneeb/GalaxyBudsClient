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
    public static class ResourceLoader
    {
        internal static T FindResource<T>(string name)
        {
            return (T)Application.Current.FindResource(name);
        }

         internal static T LoadXamlFromManifest<T>(string resource)
         {
             var stream = Assembly.GetExecutingAssembly()
                 .GetManifestResourceStream(resource);

             if (stream == null)
             {
                 throw new InvalidOperationException();
             }

             using (var sr = new StreamReader(stream))
                 using (var xmlReader = XmlReader.Create(sr))
                    return (T)XamlReader.Load(xmlReader);
         }
    }
}
