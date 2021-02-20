using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text;
using System.Xml;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Markup.Xaml.MarkupExtensions;
using GalaxyBudsClient.Model.Constants;
using GalaxyBudsClient.Platform;
using Serilog;

namespace GalaxyBudsClient.Utils
{
    namespace DynamicLocalization
    {
        class XamlLoaderShim : AvaloniaXamlLoader.IRuntimeXamlLoader
        {
            public object Load(Stream stream, Assembly localAsm, object o, Uri baseUri, bool designMode) 
                => AvaloniaRuntimeXamlLoader.Load(stream, localAsm, o, baseUri, designMode);
        }
        
        public static class Loc
        {
            public static Action</* Title */string,/* Content */string>? ErrorDetected { set; get; }
            
            public static event Action? LanguageUpdated;
            public static string GetTranslatorModeFile()
            {
                return PlatformUtils.CombineDataPath("custom_language.xaml");
            }

            public static bool IsTranslatorModeEnabled()
            {
                return File.Exists(GetTranslatorModeFile());
            }

            public static string Resolve(string resName)
            {
                var resource = Application.Current.FindResource(resName);
                if (resource is string str)
                {
                    return str;
                }

                return $"<Missing resource: {resName}>";
            }

            public static void Load()
            {

                string lang = SettingsProvider.Instance.Locale.ToString();
                if (lang.EndsWith("_"))
                    lang = lang.TrimEnd('_');

                if (SettingsProvider.Instance.Locale == Locales.custom && IsTranslatorModeEnabled())
                {    
                    SetLanguageResourceDictionary(GetTranslatorModeFile(), external: true);
                    return;
                }
                else if (SettingsProvider.Instance.Locale == Locales.custom && !IsTranslatorModeEnabled())
                {
                    lang = Locales.en.ToString();
                    SettingsProvider.Instance.Locale = Locales.en;
                }

                SetLanguageResourceDictionary($"avares://GalaxyBudsClient/i18n/{lang}.xaml", external: false);
            }
            
            private static void SetLanguageResourceDictionary(string path, bool external)
            {
                try
                {
                    int langDictId = ResourceIndexer.Find("Loc-");

                    if (langDictId == -1)
                    {
                        string msg = "Neither custom language nor fallback resource found. " +
                                     "Unwanted side-effects may occur.";
                        Log.Error($"Localization: {msg}");
                        ErrorDetected?.Invoke("Unable to resolve resource", msg);
                    }
                    else
                    {
                        // Replace the current language dictionary with the new one  
                        if (external)
                        {
                            if (AvaloniaLocator.Current.GetService<AvaloniaXamlLoader.IRuntimeXamlLoader>() == null)
                                AvaloniaLocator.CurrentMutable.Bind<AvaloniaXamlLoader.IRuntimeXamlLoader>()
                                    .ToConstant(new XamlLoaderShim());
                    
                            var loader = AvaloniaLocator.Current.GetService<AvaloniaXamlLoader.IRuntimeXamlLoader>();
      
                            using var stream = new MemoryStream(Encoding.UTF8.GetBytes(File.ReadAllText(path)));
                            try
                            {
                                if (loader.Load(stream, null, null, null, false) is ResourceDictionary dict)
                                {
                                    Application.Current.Resources.MergedDictionaries[langDictId] = dict;
                                }
                                else
                                {
                                    const string msg = "Custom language file is not a resource dictionary";
                                    ErrorDetected?.Invoke("XAML error", msg);
                                    Log.Error($"Localization: {msg}");
                                }
                            }
                            catch (XmlException ex)
                            {
                                string msg = $"An external resource dictionary contains syntax errors.\n\nPlease check line {ex.LineNumber}, column {ex.LinePosition} in the affected XAML file.\n\n{path}";
                                ErrorDetected?.Invoke("XAML syntax error", msg);
                                Log.Error($"Localization: XAML syntax error. Line {ex.LineNumber}, column {ex.LinePosition}.");
                            }
                        }
                        else
                        {
                            Application.Current.Resources.MergedDictionaries[langDictId] = new ResourceInclude {Source = new Uri(path)};
                        }
                    }
                }
                catch (IOException e)
                {
                    ErrorDetected?.Invoke("IO-Exception", e.Message);
                    Log.Error($"Localization: IOError while loading locales. Details: {e.Message}");
                }
                
                LanguageUpdated?.Invoke();
            }
        }
    }
}