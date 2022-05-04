using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml.MarkupExtensions;

namespace GalaxyBudsClient.Utils
{
    public static class ResourceIndexer
    {
        public static int Find(string prefix)
        {
            if (Application.Current == null)
            {
                return -1;
            }
            
            var dictId = -1;
            for (var i = Application.Current.Resources.MergedDictionaries.Count - 1; i >= 0; i--)
            {
                var md = Application.Current.Resources.MergedDictionaries[i];
                if (md is ResourceInclude include)
                {
                    if (include.Loaded.TryGetResource("ResourceDictionaryName", out var name))
                    {
                        if (name?.ToString() == null)
                        {
                            break;
                        }
                            
                        if (name.ToString()!.StartsWith(prefix))
                        {
                            dictId = i;
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
                            break;
                        }
                            
                        if (name.ToString()!.StartsWith(prefix))
                        {
                            dictId = i;
                            break;
                        }
                    }
                }
            }

            return dictId;
        }
    }
}