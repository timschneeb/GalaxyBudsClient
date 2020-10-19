using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Galaxy_Buds_Client.model.Constants;

namespace Galaxy_Buds_Client.util
{
    using Microsoft.Win32;
    using System;
    using System.Globalization;
    using System.IO;
    using System.Threading;
    using System.Windows;
    namespace DynamicLocalization
    {
        public static class Loc
        {

            public static string GetTranslatorModeFile()
            {
                return Path.Combine(
                    Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? string.Empty,
                    "custom_language.xaml");
            }
            public static bool IsTranslatorModeEnabled()
            {
                string s = GetTranslatorModeFile();
                return File.Exists(s);
            }

            public static string GetString(string resName)
            {
                string str;
                try
                {
                    str = Application.Current.FindResource(resName) as string;
                }
                catch (ResourceReferenceKeyNotFoundException e)
                {
                    Console.WriteLine($@"CRITICAL: String key not found. Resetting to english! ({e.Message})");
                    SetLanguageResourceDictionary($"pack://application:,,,/i18N/en.xaml");

                    Properties.Settings.Default.CurrentLocale = Locale.en;
                    Properties.Settings.Default.Save();

                    try
                    {
                        str = Application.Current.FindResource(resName) as string;
                    }
                    catch (ResourceReferenceKeyNotFoundException e2)
                    {
                        Console.WriteLine($@"CRITICAL: String key also not found in english. Returning error string. ({e2.Message})");
                        return "<Resource missing>";
                    }
                }

                return str;
            }

            public static void Load()
            {

                string lang = Properties.Settings.Default.CurrentLocale.ToString(); //System.Threading.Thread.CurrentThread.CurrentCulture.TwoLetterISOLanguageName.ToLower();

                if (Properties.Settings.Default.CurrentLocale == Locale.custom && IsTranslatorModeEnabled())
                {
                    SetLanguageResourceDictionary(GetTranslatorModeFile());
                    return;
                }
                else if (Properties.Settings.Default.CurrentLocale == Locale.custom && !IsTranslatorModeEnabled())
                {
                    lang = Locale.en.ToString();
                    Properties.Settings.Default.CurrentLocale = Locale.en;
                    Properties.Settings.Default.Save();
                }

                SetLanguageResourceDictionary($"pack://application:,,,/i18N/{lang}.xaml");
            }

            /// <summary>  
            /// Sets or replaces the ResourceDictionary by dynamically loading  
            /// a Localization ResourceDictionary from the file path passed in.  
            /// </summary>
            /// <param name="inFile"></param>  
            private static void SetLanguageResourceDictionary(String inFile)
            {
                try
                {
                    // Read in ResourceDictionary File  
                    var languageDictionary = new ResourceDictionary();

                    try
                    {
                        languageDictionary.Source = new Uri(inFile);
                    }
                    catch (System.Windows.Markup.XamlParseException e)
                    {
                        MessageBox.Show(
                            $"XAML Syntax error. Failed to parse language file:\n" +
                            $"{e.Message}", "Failed to parse language file", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }

                    // Remove any previous Localization dictionaries loaded  
                    int langDictId = -1;
                    bool defaultReached = false;
                    for (int i = 0; i < Application.Current.Resources.MergedDictionaries.Count; i++)
                    {
                        var md = Application.Current.Resources.MergedDictionaries[i];
                        // Make sure your Localization ResourceDictionarys have the ResourceDictionaryName  
                        // key and that it is set to a value starting with "Loc-".  

                        if (md.Contains("ResourceDictionaryName"))
                        {
                            if (md["ResourceDictionaryName"].ToString().StartsWith("Loc-"))
                            {
                                if (md["ResourceDictionaryName"].ToString() == "Loc-en" && !defaultReached)
                                {
                                    defaultReached = true;
                                    continue;
                                }
                                langDictId = i;
                                break;
                            }
                        }
                    }
                    if (langDictId == -1)
                    {
                        // Add in newly loaded Resource Dictionary  
                        Application.Current.Resources.MergedDictionaries.Add(languageDictionary);
                    }
                    else
                    {
                        // Replace the current langage dictionary with the new one  
                        Application.Current.Resources.MergedDictionaries[langDictId] = languageDictionary;
                    }
                }
                catch (IOException e)
                {
                    Console.WriteLine(@"Locale not supported, falling back to english...");
                }
            }
        }
    }
}
