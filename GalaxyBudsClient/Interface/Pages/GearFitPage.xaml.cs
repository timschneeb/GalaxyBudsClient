using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using GalaxyBudsClient.Interface.Elements;
using GalaxyBudsClient.Message;
using GalaxyBudsClient.Message.Decoder;
using GalaxyBudsClient.Model;
using GalaxyBudsClient.Model.Constants;
using GalaxyBudsClient.Platform;
using GalaxyBudsClient.Utils.Interface.DynamicLocalization;
using Serilog;

namespace GalaxyBudsClient.Interface.Pages
{
    public class GearFitPage : AbstractPage
    {
        public override Pages PageType => Pages.GearFitTest;

        private readonly ScanButton _scanButton;
        private readonly Image _iconLeft;
        private readonly Image _iconRight;
        private readonly Label _statusLeft;
        private readonly Label _statusRight;
        private readonly Grid _warningContainer;
        private bool _lastWarning;
        
        public GearFitPage()
        {   
            AvaloniaXamlLoader.Load(this);

            _scanButton = this.GetControl<ScanButton>("ScanButton");
            _iconLeft = this.GetControl<Image>("LeftIcon");
            _iconRight = this.GetControl<Image>("RightIcon");
            _statusLeft = this.GetControl<Label>("LeftStatus");
            _statusRight = this.GetControl<Label>("RightStatus");
            _warningContainer = this.GetControl<Grid>("EarbudWarningContainer");
            
            SppMessageHandler.Instance.BaseUpdate += (_, update) => UpdateDashboard(update);
            SppMessageHandler.Instance.FitTestResult += (_, update) => ShowResults(update);
            _scanButton.ScanningStatusChanged += ScanButton_OnScanningStatusChanged;
        }

        private async void ScanButton_OnScanningStatusChanged(object? sender, bool e)
        {
            if (e)
            {
                _statusLeft.Content = "";
                _statusRight.Content = "";
                await BluetoothImpl.Instance.SendRequestAsync(SppMessage.MessageIds.CHECK_THE_FIT_OF_EARBUDS, 1);
            }
            else
            {
                await BluetoothImpl.Instance.SendRequestAsync(SppMessage.MessageIds.CHECK_THE_FIT_OF_EARBUDS, 0);
            }
        }
        
        public void UpdateDashboard(IBasicStatusUpdate ext)
        {
            _lastWarning = _warningContainer.IsVisible = ext.WearState != WearStates.Both;
        }

        public override void OnPageShown()
        {
            _warningContainer.IsVisible = _lastWarning;
            _statusLeft.Content = "";
            _statusRight.Content = "";
            
            if (DeviceMessageCache.Instance.DebugGetAllData == null)
            {
                UpdateIcons(0,0);
            }
            else
            {
                UpdateIcons(DeviceMessageCache.Instance.DebugGetAllData.LeftAdcSOC,
                    DeviceMessageCache.Instance.DebugGetAllData.RightAdcSOC);
            }
        }

        public override void OnPageHidden()
        {
            _scanButton.IsSearching = false;
            _ = BluetoothImpl.Instance.SendRequestAsync(SppMessage.MessageIds.CHECK_THE_FIT_OF_EARBUDS, 0);
        }

        private void ShowResults(FitTestParser result)
        {
            _scanButton.IsSearching = false;
            _ = BluetoothImpl.Instance.SendRequestAsync(SppMessage.MessageIds.CHECK_THE_FIT_OF_EARBUDS, 0);
            _statusLeft.Content = GetTextFor(result.Left);
            _statusRight.Content = GetTextFor(result.Right);
        }

        private static string GetTextFor(FitTestParser.Result result)
        {
            return result switch
            {
                FitTestParser.Result.Bad => Loc.Resolve("gft_bad"),
                FitTestParser.Result.Good => Loc.Resolve("gft_good"),
                FitTestParser.Result.TestFailed => Loc.Resolve("gft_fail"),
                _ => result.ToString()
            };
        }

        private void UpdateIcons(double left, double right)
        {
            var isLeftOnline = left > 0;
            var isRightOnline = right > 0;

            var type = BluetoothImpl.Instance.DeviceSpec.IconResourceKey;
            
            var leftSourceName = $"Left{type}{(isLeftOnline ? "Connected" : "Disconnected")}";
            _iconLeft.Source = (IImage?)Application.Current?.FindResource(leftSourceName);
            var rightSourceName = $"Right{type}{(isRightOnline ? "Connected" : "Disconnected")}";
            _iconRight.Source = (IImage?)Application.Current?.FindResource(rightSourceName);
        }

        private void BackButton_OnPointerPressed(object? sender, PointerPressedEventArgs e)
        {
            MainWindow.Instance.Pager.SwitchPage(Pages.Advanced);
        }
		
    }
}