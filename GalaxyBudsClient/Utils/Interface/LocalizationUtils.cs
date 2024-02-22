using System;
using System.IO;
using System.Reflection;
using System.Text;
using System.Xml;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Markup.Xaml.Styling;
using Avalonia.Media;
using GalaxyBudsClient.Model.Constants;
using GalaxyBudsClient.Platform;
using Serilog;

namespace GalaxyBudsClient.Utils.Interface
{
    namespace DynamicLocalization
    {
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
                var resource = Application.Current?.FindResource(resName);
                if (resource is string str)
                {
                    return str;
                }

                return $"<Missing resource: {resName}>";
            }
            
            public static FlowDirection ResolveFlowDirection()
            {
                return (Application.Current?.FindResource("IsRightToLeft") as bool?) == true ? FlowDirection.RightToLeft : FlowDirection.LeftToRight;
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
                if (SettingsProvider.Instance.Locale == Locales.custom && !IsTranslatorModeEnabled())
                {
                    lang = Locales.en.ToString();
                    SettingsProvider.Instance.Locale = Locales.en;
                }
                SetLanguageResourceDictionary($"{Program.AvaresUrl}/i18n/{lang}.xaml", external: false);
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
                            using var stream = new MemoryStream(Encoding.UTF8.GetBytes(File.ReadAllText(path)));
                            try
                            {
                                if (AvaloniaRuntimeXamlLoader.Load(stream) is ResourceDictionary dict)
                                {
                                    if (Application.Current != null)
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
                            if (Application.Current != null)
                                Application.Current.Resources.MergedDictionaries[langDictId] =
                                    new ResourceInclude((Uri?)null) { Source = new Uri(path) };
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