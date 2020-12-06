using System;
using System.Collections;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using GalaxyBudsClient.Interface.Dialogs;
using GalaxyBudsClient.Interface.Pages;
using GalaxyBudsClient.Model.Constants;
using GalaxyBudsClient.Utils;
using GalaxyBudsClient.Utils.DynamicLocalization;

namespace GalaxyBudsClient.Interface.Developer
{
    public sealed class TranslatorTools : Window
    {
        private class ViewModel
        {
            public IEnumerable PagesSource => Enum.GetValues(typeof(AbstractPage.Pages)).Cast<AbstractPage.Pages>().ToList();
            public IEnumerable LocaleSource => Enum.GetValues(typeof(Locales)).Cast<Locales>().ToList();
            public IEnumerable ModelSource => Enum.GetValues(typeof(Model.Constants.Models)).Cast<Model.Constants.Models>().ToList();
        }

        private readonly CheckBox _ignoreConnLoss;
        private readonly ComboBox _pages;
        private readonly ComboBox _locales;
        private readonly TextBox _xamlPath;

        private readonly ViewModel _vm = new ViewModel();

        public TranslatorTools()
        {
            DataContext = _vm;
            AvaloniaXamlLoader.Load(this);
            this.AttachDevTools();

            _ignoreConnLoss = this.FindControl<CheckBox>("IgnoreConnLoss");
            _pages = this.FindControl<ComboBox>("Pages");
            _locales = this.FindControl<ComboBox>("Locales");
            _xamlPath = this.FindControl<TextBox>("XamlPath");

            _locales.SelectedItem = SettingsProvider.Instance.Locale;
            _xamlPath.Text = Loc.GetTranslatorModeFile();

            Loc.ErrorDetected += (title, content) =>
            {
                new MessageBox
                {
                    Title = title, 
                    Description = content
                }.ShowDialog(this);
            };
        }

        private void GoToPage_OnClick(object? sender, RoutedEventArgs e)
        {
            if (_pages.SelectedItem is AbstractPage.Pages page)
            {
                MainWindow.Instance.Pager.SwitchPage(page);
            }
        }

        private void ReloadXaml_OnClick(object? sender, RoutedEventArgs e)
        {
            if (_locales.SelectedItem is Locales locale)
            {
                SettingsProvider.Instance.Locale = locale;
            }
            Loc.Load();
        }

        private void Model_OnSelectionChanged(object? sender, SelectionChangedEventArgs e)
        {
            
        }

        private void IgnoreConnLoss_OnChecked(object? sender, RoutedEventArgs e)
        {
            
        }
    }
}