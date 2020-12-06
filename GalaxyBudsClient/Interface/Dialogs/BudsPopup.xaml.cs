using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using GalaxyBudsClient.Model.Constants;
using GalaxyBudsClient.Utils;
using GalaxyBudsClient.Utils.DynamicLocalization;

namespace GalaxyBudsClient.Interface.Dialogs
{
    public sealed class BudsPopup : Window
    {
        private Label _header;
        
        private bool _isHeaderHidden;
        public bool HideHeader
        {
            get => _isHeaderHidden;
            set
            {
                _isHeaderHidden = value;
                if (value)
                {
                    Height = 205 - 35;
                    //HeaderRow.Height = new GridLength(0);
                }
                else
                {
                    Height = 205;
                    //HeaderRow.Height = new GridLength(35);
                }
            }
        }

        public EventHandler ClickedEventHandler { get; set; }

        public PopupPlacement PopupPlacement { get; set; }
        
        public BudsPopup(){}
        
        public BudsPopup(Models model, int left, int right, int box)
        {
            AvaloniaXamlLoader.Load(this);
            this.AttachDevTools();

            _header = this.FindControl<Label>("Header");
            
            string mod = "";
            if (model == Models.BudsPlus) mod = "+";
            else if (model == Models.BudsLive) mod = " Live";

            string name = Environment.UserName.Split(' ')[0];

            /*string title = SettingsProvider.Instance.ConnectionPopupCustomTitle == ""
                ? Loc.Resolve("connpopup_title")
                : SettingsProvider.Instance.ConnectionPopupCustomTitle;

            _header.Content = string.Format(title, name, mod);
            UpdateContent(left, right, box);*/
            
            //SPPMessageHandler.Instance.StatusUpdate += Instance_OnStatusUpdate;
        }
    }
}