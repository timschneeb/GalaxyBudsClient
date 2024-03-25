using System;
using System.Collections.Generic;
using System.IO;
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
                return Application.Current?.FindResource("IsRightToLeft") as bool? == true ? FlowDirection.RightToLeft : FlowDirection.LeftToRight;
            }

            public static void Load()
            {
                var lang = Settings.Instance.Locale.ToString();
                if (lang.EndsWith('_'))
                    lang = lang.TrimEnd('_');

                switch (Settings.Instance.Locale)
                {
                    case Locales.custom when IsTranslatorModeEnabled():
                        SetLanguageResourceDictionary(GetTranslatorModeFile(), true);
                        NotifyObservers();
                        return;
                    case Locales.custom when !IsTranslatorModeEnabled():
                        lang = Locales.en.ToString();
                        Settings.Instance.Locale = Locales.en;
                        break;
                }

                SetLanguageResourceDictionary($"{Program.AvaresUrl}/i18n/{lang}.xaml", false);
                NotifyObservers();
            }

            private static void NotifyObservers()
            {
                foreach (var item in SLangList)
                {
                    var value = Resolve(item.Key);
                    foreach (var item1 in item.Value)
                    {
                        if (item1.TryGetTarget(out var target))
                        { 
                            target.OnNext(value);
                        }
                    }
                }
            }
            
            private static void SetLanguageResourceDictionary(string path, bool external)
            {
                try
                {
                    var langDictId = ResourceIndexer.Find("Loc-");

                    if (langDictId == -1)
                    {
                        const string msg = "Neither custom language nor fallback resource found. " +
                                           "Unwanted side-effects may occur.";
                        Log.Error("Localization: {Msg}", msg);
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
                                var msg = $"An external resource dictionary contains syntax errors.\n\nPlease check line {ex.LineNumber}, column {ex.LinePosition} in the affected XAML file.\n\n{path}";
                                ErrorDetected?.Invoke("XAML syntax error", msg);
                                Log.Error("Localization: XAML syntax error. Line {Line}, column {Position}", ex.LineNumber, ex.LinePosition);
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
                    Log.Error("Localization: IOError while loading locales. Details: {Message}", e.Message);
                }
                
                LanguageUpdated?.Invoke();
            }
            
            private static readonly Dictionary<string, List<WeakReference<IObserver<string>>>> SLangList = [];

            public static IDisposable AddObserverForKey(string key, IObserver<string> observer)
            {
                if (SLangList.TryGetValue(key, out var list))
                {
                    list.Add(new WeakReference<IObserver<string>>(observer));
                }
                else
                {
                    list = [new WeakReference<IObserver<string>>(observer)];
                    SLangList.Add(key, list);
                }
                var value = Resolve(key);
                observer.OnNext(value);
                return new Unsubscribable(list, observer);
            }

            private class Unsubscribable(List<WeakReference<IObserver<string>>> observers, IObserver<string> observer) : IDisposable
            {
                public void Dispose()
                {
                    foreach (var item in observers.ToArray())
                    {
                        if (!item.TryGetTarget(out var target)
                            || target == observer)
                        {
                            observers.Remove(item);
                        }
                    }
                }
            }
        }
    }
}