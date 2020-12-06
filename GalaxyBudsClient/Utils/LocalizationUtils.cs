using System;
using System.IO;
using System.Reflection;
using System.Text;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Markup.Xaml.MarkupExtensions;
using GalaxyBudsClient.Model.Constants;
using Serilog;
using XamlLoadException = XamlX.XamlLoadException;

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
                return Path.Combine(
                    Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? string.Empty,
                    "custom_language.xaml");
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

                return "<Missing resource>";
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
                    int langDictId = -1;
                    for (var i = Application.Current.Resources.MergedDictionaries.Count - 1; i >= 0; i--)
                    {
                        var md = Application.Current.Resources.MergedDictionaries[i];
                        if (md is ResourceInclude include)
                        {
                            if (include.Loaded.TryGetResource("ResourceDictionaryName", out var name))
                            {
                                if (name?.ToString() == null)
                                {
                                    return;
                                }
                                
                                if (name.ToString()!.StartsWith("Loc-"))
                                {
                                    langDictId = i;
                                    break;
                                }
                            }
                        }
                        else if (md is ResourceDictionary dict)
                        {
                            if (dict.TryGetResource("ResourceDictionaryName", out var name))
                            {
                                if (name?.ToString() == null)
                                {
                                    return;
                                }
                                
                                if (name.ToString()!.StartsWith("Loc-"))
                                {
                                    langDictId = i;
                                    break;
                                }
                            }
                        }

                    }

                    if (langDictId == -1)
                    {
                        Log.Error("Localization: Neither custom language nor fallback resource found. " +
                                  "Unwanted side-effects may occur.");
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
                            catch (XamlLoadException ex)
                            {
                                string msg = $"Custom language file contains syntax errors. Please check line {ex.LineNumber}, column {ex.LinePosition} in custom_language.xaml.";
                                ErrorDetected?.Invoke("XAML syntax error", msg);
                                Log.Error($"Localization: XAML syntax error. {msg}");
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