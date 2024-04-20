using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Input;
using System.Xml;
using System.Xml.Linq;
using Avalonia.Media;
using GalaxyBudsClient.Generated.I18N;
using GalaxyBudsClient.Model;
using GalaxyBudsClient.Model.Config;
using GalaxyBudsClient.Model.Constants;
using GalaxyBudsClient.Platform;
using Serilog;

namespace GalaxyBudsClient.Utils.Interface;

public static class Loc
{
    public static Action</* Title */string,/* Content */string>? ErrorDetected { set; get; }
            
    public static event Action? LanguageUpdated;
    public static string TranslatorModeFile => PlatformUtils.CombineDataPath("custom_language.xaml");
    public static ICommand ReloadCommand { get; } = new MiniCommand(_ => Load());
    
    private static readonly Dictionary<string, List<WeakReference<IObserver<string>>>> Observers = [];
    private static readonly Dictionary<string, string> FallbackStrings = [];
    private static Dictionary<string, string> _strings = [];

    static Loc()
    {
        // Load English fallback strings
        LoadInternalLanguage("en", ref FallbackStrings);
    }
    
    public static string Resolve(string resName) => 
        ResolveOrDefault(resName) ?? $"{resName}";
    
    public static string? ResolveOrDefault(string resName) =>
        _strings.TryGetValue(resName, out var result) ? result : 
        FallbackStrings.TryGetValue(resName, out var fallbackResult) ? fallbackResult : null;
            
    public static FlowDirection ResolveFlowDirection()
    {
        var rtlSetting = ResolveOrDefault("IsRightToLeft");
        return rtlSetting != null ? 
            (string.Equals(rtlSetting, "True", StringComparison.OrdinalIgnoreCase) ? FlowDirection.RightToLeft : FlowDirection.LeftToRight) : 
            FlowDirection.LeftToRight;
    }
    
    public static bool IsTranslatorModeEnabled => File.Exists(TranslatorModeFile);
    
    public static void Load()
    {
        var lang = Settings.Data.Locale.ToString();
        if (lang.EndsWith('_'))
            lang = lang.TrimEnd('_');
        
        switch (Settings.Data.Locale)
        {
            case Locales.custom when IsTranslatorModeEnabled:
                LoadExternalLanguage(TranslatorModeFile, ref _strings);
                return;
            case Locales.custom when !IsTranslatorModeEnabled:
                lang = Locales.en.ToStringFast();
                Settings.Data.Locale = Locales.en;
                break;
        }

        LoadInternalLanguage(lang, ref _strings);
    }

    private static void LoadInternalLanguage(string langCode, ref Dictionary<string, string> targetDictionary)
    {
        var dict = LocalizationDictionaries.GetByLangCode(langCode);
        if(dict == null)
            Log.Error($"Localization: Internal language dictionary for '{langCode}' not found.");
        else
            targetDictionary = dict;
        
        LanguageUpdated?.Invoke();
        NotifyObservers();
    }
    
    private static void LoadExternalLanguage(string path, ref Dictionary<string, string> targetDictionary)
    {
        try
        {
            using var stream = File.OpenRead(path);
            try
            {
                targetDictionary.Clear();
                
                using var reader = new StreamReader(stream);
                var doc = XDocument.Parse(reader.ReadToEnd());
                var nodes = doc.Root?.Nodes();
                if(nodes == null)
                    return;
                
                foreach (var node in nodes)
                {
                    if (node is not XElement element) 
                        continue;

                    var key = element.Attributes().FirstOrDefault(x => x.Name.LocalName == "Key");
                    if (key == null)
                        Log.Warning("Localization: x:Key attribute not found for XAML element of type {TypeName}", element.Name.LocalName);
                    else
                        targetDictionary.Add(key.Value, element.Value);
                }
            }
            catch (XmlException ex)
            {
                ErrorDetected?.Invoke("XAML syntax error", 
                    $"""
                     An external resource dictionary contains syntax errors.
                     Please check line {ex.LineNumber}, column {ex.LinePosition} in the affected XAML file.

                     {path}
                     """);
                
                Log.Error("Localization: XAML syntax error. Line {Line}, column {Position}", ex.LineNumber, ex.LinePosition);
            }
        }
        catch (Exception e)
        {
            ErrorDetected?.Invoke(e.GetType().Name, e.Message);
            Log.Error("Localization: {Ex} while loading language. Details: {Message}", e.GetType().FullName, e.Message);
        }
                
        LanguageUpdated?.Invoke();
        NotifyObservers();
    }

    private static void NotifyObservers()
    {
        foreach (var observerForKey in Observers)
        {
            var resolvedString = Resolve(observerForKey.Key);
            foreach (var weakReference in observerForKey.Value)
            {
                if (weakReference.TryGetTarget(out var observer))
                { 
                    observer.OnNext(resolvedString);
                }
            }
        }
    }
    
    public static IDisposable AddObserverForKey(string key, IObserver<string> observer)
    {
        if (Observers.TryGetValue(key, out var list))
        {
            list.Add(new WeakReference<IObserver<string>>(observer));
        }
        else
        {
            list = [new WeakReference<IObserver<string>>(observer)];
            Observers.Add(key, list);
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