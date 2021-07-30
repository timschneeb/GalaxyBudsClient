using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Net.NetworkInformation;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Controls.Selection;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using GalaxyBudsClient.Interface.Dialogs;
using GalaxyBudsClient.Interface.Elements;
using GalaxyBudsClient.Interface.Items;
using GalaxyBudsClient.Model.Attributes;
using GalaxyBudsClient.Model.Firmware;
using GalaxyBudsClient.Platform;
using GalaxyBudsClient.Utils;
using GalaxyBudsClient.Utils.DynamicLocalization;
using Serilog;

namespace GalaxyBudsClient.Interface.Pages
{
    public class FirmwareSelectionPage : AbstractPage
    {
        public override Pages PageType => Pages.FirmwareSelect;
        
        public ObservableCollection<FirmwareRemoteBinary>? AvailableFirmwares
        {
            get => _firmwareBox.Items as ObservableCollection<FirmwareRemoteBinary>;
            set => _firmwareBox.Items = value;
        }

        public SelectionModel<FirmwareRemoteBinary>? Selection
        {
            get => _firmwareBox.Selection as SelectionModel<FirmwareRemoteBinary>;
            set => _firmwareBox.Selection = value;
        }
        
        public bool IsSearching
        {
            set => _pageHeader.LoadingSpinnerVisible = value;
            get => _pageHeader.LoadingSpinnerVisible;
        }

        private readonly ListBox _firmwareBox;
        private readonly PageHeader _pageHeader;
        private readonly Label _navBarNextLabel;
        private readonly Border _navBarNext;
        private readonly Border _navBarAdvanced;

        private ContextMenu? _advancedMenu;

        private readonly FirmwareRemoteClient _client = new FirmwareRemoteClient();

        public FirmwareSelectionPage()
        {
            AvaloniaXamlLoader.Load(this);
            _navBarNextLabel = this.FindControl<Label>("NavBarNextLabel");
            _navBarNext = this.FindControl<Border>("NavBarNext");
            _navBarAdvanced = this.FindControl<Border>("NavBarAdvanced");
            _pageHeader = this.FindControl<PageHeader>("PageHeader");
            _firmwareBox = this.FindControl<ListBox>("FirmwareList");

            AvailableFirmwares = new ObservableCollection<FirmwareRemoteBinary>();
            Selection = new SelectionModel<FirmwareRemoteBinary>();
            
            IsSearching = false;
            
            RefreshList();
            
            Loc.LanguageUpdated += UpdateStrings;
            UpdateStrings();
        }

        private void UpdateStrings()
        {
            _advancedMenu = MenuFactory.BuildContextMenu(new Dictionary<string, EventHandler<RoutedEventArgs>?>()
            {
                [Loc.Resolve("fw_select_from_disk")] = (sender, args) => SelectFromDisk()
            }, _navBarAdvanced);
        }

        private async void SelectFromDisk()
        {
            var result = await new QuestionBox()
            {
                Title = Loc.Resolve("cact_notice"),
                Description = Loc.Resolve("fw_select_external_note"),
            }.ShowDialog<bool>(MainWindow.Instance);

            if (result)
            {
                OpenFileDialog dlg = new OpenFileDialog {Filters = new List<FileDialogFilter>()
                {
                    new FileDialogFilter {Name = "Firmware binary", Extensions = new List<string>() {"bin"}},
                    new FileDialogFilter {Name = "All files", Extensions = new List<string>() {"*"}},
                }, AllowMultiple = false};
                string[]? paths = await dlg.ShowAsync(MainWindow.Instance);
            
                if (paths == null || paths.Length < 1)
                {
                    return;
                }
                
                await PrepareInstallation(await File.ReadAllBytesAsync(paths[0]), Path.GetFileName(paths[0]));
            }
        }
        
        public override void OnPageShown()
        {
            RefreshList();
        }

        private void BackButton_OnPointerPressed(object? sender, PointerPressedEventArgs e)
        {
            MainWindow.Instance.Pager.SwitchPage(Pages.System);
        }
        
        private async void Next_OnPointerPressed(object? sender, PointerPressedEventArgs e)
        {
            
            if (Selection == null || 
                Selection.Count <= 0 || 
                Selection.SelectedItem == null ||
                Selection.SelectedItem.Model != BluetoothImpl.Instance.ActiveModel)
            {
                await new MessageBox()
                {
                    Title = Loc.Resolve("error"),
                    Description = Loc.Resolve("fw_select_unsupported_selection")
                }.ShowDialog(MainWindow.Instance);
                return;
            }

            if (IsSearching)
            {
                Log.Warning("DeviceSelectionDialog: Refresh/download already in progress");
                return;
            }
            
            IsSearching = true;
            _navBarNextLabel.Content = Loc.Resolve("fw_select_downloading");

            byte[] binary;
            try
            {
                binary = await _client.DownloadFirmware(Selection.SelectedItem);
            }
            catch (NetworkInformationException ex)
            {
                IsSearching = false;
                _navBarNextLabel.Content = Loc.Resolve("fw_select_install");
                
                await new MessageBox()
                {
                    Title = Loc.Resolve("error"),
                    Description =
                        $"{Loc.Resolve("fw_select_net_error")}\n{Loc.Resolve("fw_select_http_error")} {ex.ErrorCode}"
                }.ShowDialog(MainWindow.Instance);

                Log.Error("FirmwareSelectionPage.Next: " + ex);
                return;
            }
            catch (Exception ex)
            {
                IsSearching = false;
                _navBarNextLabel.Content = Loc.Resolve("fw_select_install");

                await new MessageBox()
                {
                    Title = Loc.Resolve("error"),
                    Description =
                        $"{Loc.Resolve("fw_select_net_error")}\n\n{ex.Message}"
                }.ShowDialog(MainWindow.Instance);

                Log.Error("FirmwareSelectionPage.Next: " + ex);
                return;
            }
            
            IsSearching = false;
            _navBarNextLabel.Content = Loc.Resolve("fw_select_install");
            
            await PrepareInstallation(binary, Selection.SelectedItem.BuildName ?? Loc.Resolve("fw_select_unknown_build"));
        }

        private async Task PrepareInstallation(byte[] data, string buildName)
        {
            FirmwareBinary? binary = null;
            try
            {
                binary = new FirmwareBinary(data, buildName);
            }
            catch (FirmwareParseException ex)
            {
                await new MessageBox()
                {
                    Title = Loc.Resolve("fw_select_verify_fail"),
                    Description = ex.ErrorMessage
                }.ShowDialog(MainWindow.Instance);
                return;
            }
            
            var result = await new QuestionBox()
            {
                Title = string.Format(Loc.Resolve("fw_select_confirm"), binary.BuildName, BluetoothImpl.Instance.ActiveModel.GetDescription()),
                Description = Loc.Resolve("fw_select_confirm_desc"),
                MinWidth = 600,
                MaxWidth = 600,
            }.ShowDialog<bool>(MainWindow.Instance);

            if (result)
            {
                MainWindow.Instance.Pager.SwitchPage(Pages.FirmwareTransfer);
                await Task.Delay(400);
                ((FirmwareTransferPage) MainWindow.Instance.Pager.FindPage(Pages.FirmwareTransfer)!).BeginTransfer(binary);
            }
        }
        
        private async void RefreshList(bool user = false)
        {
            if (IsSearching)
            {
                Log.Warning("DeviceSelectionDialog: Refresh already in progress");
                return;
            }
            
            IsSearching = true;
            _navBarNext.IsVisible = false;
            _firmwareBox.IsVisible = false;
            AvailableFirmwares?.Clear();

            FirmwareRemoteBinary[] firmwareBins;
            try
            {
                firmwareBins = await _client.SearchForFirmware(this.FindControl<SwitchDetailListItem>("AllowDowngrade").IsChecked);
            }
            catch (NetworkInformationException ex)
            {
                IsSearching = false;
                if (user)
                {
                    await new MessageBox()
                    {
                        Title = Loc.Resolve("error"),
                        Description =
                            $"{Loc.Resolve("fw_select_net_index_error")}\n{Loc.Resolve("fw_select_http_error")} {ex.ErrorCode}"
                    }.ShowDialog(MainWindow.Instance);
                }
                Log.Error("FirmwareSelectionPage.RefreshList: " + ex);
                
                SetEmptyView(true);
                return;
            }
            catch (Exception ex)
            {
                IsSearching = false;
                if (user)
                {
                    await new MessageBox()
                    {
                        Title = Loc.Resolve("error"),
                        Description =
                            $"{Loc.Resolve("fw_select_net_index_error")}\n\n{ex.Message}"
                    }.ShowDialog(MainWindow.Instance);
                }
                
                Log.Error("FirmwareSelectionPage.RefreshList: " + ex);

                SetEmptyView(true);
                return;
            }

            firmwareBins
                .ToList()
                .ForEach(x => AvailableFirmwares?.Add(x));
            
            SetEmptyView(AvailableFirmwares?.Count <= 0);
            _firmwareBox.IsVisible = true;
            IsSearching = false;
        }

        private void SetEmptyView(bool b)
        {
            this.FindControl<Border>("FirmwareContainer").IsVisible = !b;
            this.FindControl<Border>("EmptyView").IsVisible = b;
        }

        private void Firmwares_OnSelectionChanged(object? sender, SelectionChangedEventArgs e)
        {
            _navBarNext.IsVisible = true;
        }

        private void AllowDowngrade_OnToggled(object? sender, bool e)
        {
            RefreshList(user: true);
        }

        private void Advanced_OnPointerPressed(object? sender, PointerPressedEventArgs e)
        {
            _advancedMenu?.Open(_navBarAdvanced);
        }
    }
}