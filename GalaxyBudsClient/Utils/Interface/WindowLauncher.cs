using System;
using Avalonia.Controls;
using GalaxyBudsClient.Interface.Developer;

namespace GalaxyBudsClient.Utils.Interface
{
    public static class WindowLauncher
    {
        private static DevTools? _devTools;
        private static TranslatorTools? _translatorTools;

        public static void ShowDevTools(Window? parent = null) => ShowAsSingleInstance(ref _devTools, parent);
        public static void ShowTranslatorTools(Window? parent = null) => ShowAsSingleInstance(ref _translatorTools, parent);
        
        private static void ShowAsSingleInstance<T>(ref T? target, Window? parent = null) where T : Window, new()
        {
            target ??= new T();
            try
            {
                Show(target);
            }
            catch (InvalidOperationException)
            {
                // Old window object has been closed and cannot be reused 
                target = new T();
                Show(target);
            }
            return;

            void Show(T target)
            {
                if (parent == null)
                    target.Show();
                else
                    target.Show(parent);
            }
        }
    }
}