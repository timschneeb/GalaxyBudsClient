using System;
using Avalonia.Controls;
using Avalonia.Interactivity;
using GalaxyBudsClient.Interface.Developer;

namespace GalaxyBudsClient.Utils
{
    public static class DialogLauncher
    {
        private static DevTools? _devTools;
        private static TranslatorTools? _translatorTools;
        
        public static void ShowDevTools(Window? parent = null)
        {
            _devTools ??= new DevTools();
            try
            {
                if (parent == null)
                {
                    _devTools.Show();
                }
                else
                {
                    _devTools.Show(parent);
                }
            }
            catch (InvalidOperationException)
            {
                _devTools = new DevTools();
                if (parent == null)
                {
                    _devTools.Show();
                }
                else
                {
                    _devTools.Show(parent);
                }
            }
        }

        public static void ShowTranslatorTools(Window? parent = null)
        {
            _translatorTools ??= new TranslatorTools();
            try
            {
                if (parent == null)
                {
                    _translatorTools.Show();
                }
                else
                {
                    _translatorTools.Show(parent);
                }
            }
            catch (InvalidOperationException)
            {
                _translatorTools = new TranslatorTools();
                if (parent == null)
                {
                    _translatorTools.Show();
                }
                else
                {
                    _translatorTools.Show(parent);
                }
            }
        }
    }
}