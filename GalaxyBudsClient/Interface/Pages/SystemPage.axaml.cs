using System;
using System.IO;
using System.Numerics;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Threading;
using FluentAvalonia.UI.Controls;
using GalaxyBudsClient.Interface.Dialogs;
using GalaxyBudsClient.Interface.Services;
using GalaxyBudsClient.Interface.ViewModels.Pages;
using GalaxyBudsClient.Message;
using GalaxyBudsClient.Message.Decoder;
using GalaxyBudsClient.Platform;
using GalaxyBudsClient.Utils.Extensions;
using GalaxyBudsClient.Utils.Interface.DynamicLocalization;
using Serilog;
using Symbol = FluentIcons.Common.Symbol;

namespace GalaxyBudsClient.Interface.Pages;

public partial class SystemPage : BasePage<SystemPageViewModel>
{
    public SystemPage()
    {
        InitializeComponent();
        
        
        AddHandler(SettingsExpanderItem.ClickEvent, OnSettingsItemClicked);
    }

    private void OnSettingsItemClicked(object? sender, RoutedEventArgs e)
    {
        if (e.Source is not SettingsExpanderItem item)
            return;
        
        NavigationService.Instance.Navigate(item.Name);
    }

    private async void OnFactoryResetClicked(object? sender, RoutedEventArgs e)
    {
        var cd = new ContentDialog
        {
            Title = Loc.Resolve("factory_header"),
            Content = Loc.Resolve("factory_confirmation"),
            PrimaryButtonText = Loc.Resolve("continue_button"),
            SecondaryButtonText = Loc.Resolve("cancel"),
            DefaultButton = ContentDialogButton.Primary,
        };

        cd.PrimaryButtonClick += OnFactoryResetConfirmed;
        cd.SecondaryButtonClick += (_, _) => cd.Hide();
        await cd.ShowAsync(MainWindow2.Instance);
        cd.PrimaryButtonClick -= OnFactoryResetConfirmed;
    }

    private async void OnFactoryResetConfirmed(ContentDialog sender, ContentDialogButtonClickEventArgs args)
    {
        var cancelToken = new CancellationTokenSource();
        sender.Title = Loc.Resolve("system_waiting_for_device");
        sender.IsPrimaryButtonEnabled = false;
        sender.IsSecondaryButtonEnabled = false;
        
        var defer = args.GetDeferral();
        SppMessageHandler.Instance.ResetResponse += OnResetResponse;
        await BluetoothImpl.Instance.SendRequestAsync(SppMessage.MessageIds.RESET);
        
        // Wait for 10 seconds for the reset response
        await Task.Delay(10000, cancelToken.Token);
        
        SppMessageHandler.Instance.ResetResponse -= OnResetResponse;
        // If the reset response was not received, show an error message
        OnResetResponse(null, -1);
        return;
        
        void OnResetResponse(object? s, int code)
        {
            cancelToken.Cancel();
            defer.Complete();
            
            if (code != 0)
            {
                var info = code == -1 ? Loc.Resolve("system_no_response") : code.ToString();
                
                _ = new MessageBox
                {
                    Title = Loc.Resolve("factory_error_title"),
                    Description = Loc.Resolve("factory_error").Replace("{0}", info)
                }.ShowAsync(MainWindow2.Instance);
                return;
            }

            // TODO re-enter setup wizard?
            BluetoothImpl.Instance.UnregisterDevice();
        }
    }

    private async void OnPairingModeClicked(object? sender, RoutedEventArgs e)
    {
        await BluetoothImpl.Instance.SendRequestAsync(SppMessage.MessageIds.UNK_PAIRING_MODE);
        await BluetoothImpl.Instance.DisconnectAsync();
        await new MessageBox
        {
            Title = Loc.Resolve("connlost_disconnected"),
            Description = Loc.Resolve("pairingmode_done")
        }.ShowAsync();
    }
    
    private async void OnSelfTestClicked(object? sender, RoutedEventArgs e)
    {
        var closeButton = new TaskDialogButton(Loc.Resolve("cancel"), TaskDialogStandardResult.Close);
        var cancelToken = new CancellationTokenSource();

        var td = new TaskDialog
        {
            Header = Loc.Resolve("selftest_header"),
            Buttons = [closeButton],
            IconSource = new FluentIcons.Avalonia.Fluent.SymbolIconSource() { Symbol = Symbol.Beaker },
            Content = new TextBlock()
            {
                TextWrapping = TextWrapping.Wrap,
                Text = Loc.Resolve("system_waiting_for_device"),
                MaxWidth = 600
            },
            XamlRoot = MainWindow2.Instance,
            ShowProgressBar = true,
        };
            
        try
        {
            td.SetProgressBarState(100, TaskDialogProgressState.Indeterminate);
            _ = td.ShowAsync(true).ContinueWith(a =>
            {
                if (a.Result as TaskDialogStandardResult? == TaskDialogStandardResult.Close)
                    cancelToken.Cancel();
            }, cancelToken.Token);

            SppMessageHandler.Instance.SelfTestResponse += OnSelfTestResponse;
            await BluetoothImpl.Instance.SendRequestAsync(SppMessage.MessageIds.SELF_TEST);

            // Wait for 10 seconds for the self test response
            await Task.Delay(10000, cancelToken.Token);
        }
        catch (TaskCanceledException) {}
      
        SppMessageHandler.Instance.SelfTestResponse -= OnSelfTestResponse;

        // If the self test response was not received, show an error message
        if (td.IsVisible)
            OnSelfTestResponse(null, null);
        return;

        TaskDialogCommand ResultWithSideAsTaskItem(string testKey, bool lPass, bool rPass)
        {
            return new TaskDialogCommand
            {
                Text = Loc.Resolve(testKey),
                IconSource = new FluentIcons.Avalonia.Fluent.SymbolIconSource
                {
                    Symbol = !lPass || !rPass ? Symbol.Warning : Symbol.Checkmark
                },
                Description = $"{Loc.Resolve("left")}: {Loc.Resolve(lPass ? "selftest_pass" : "selftest_fail")}, " +
                              $"{Loc.Resolve("right")}: {Loc.Resolve(rPass ? "selftest_pass" : "selftest_fail")}",
                ClosesOnInvoked = false
            };
        }
        
        TaskDialogCommand ResultAsTaskItem(string testKey, bool pass)
        {
            return new TaskDialogCommand
            {
                Text = Loc.Resolve(testKey),
                IconSource = new FluentIcons.Avalonia.Fluent.SymbolIconSource
                {
                    Symbol = !pass ? Symbol.Warning : Symbol.Checkmark
                },
                Description = Loc.Resolve(pass ? "selftest_pass" : "selftest_fail"),
                ClosesOnInvoked = false
            };
        }
        
        void OnSelfTestResponse(object? s, SelfTestParser? parser)
        {
            cancelToken.Cancel();
            td.Buttons[0].Text = Loc.Resolve("window_close");

            if (parser == null)
            {
                td.SetProgressBarState(100, TaskDialogProgressState.Error);
                ((TextBlock)td.Content).Text = Loc.Resolve("system_no_response");
            }
            else
            {
                _ = new TaskDialog
                {
                    Header = Loc.Resolve(parser is not { AllChecks: true } ? "selftest_fail_long" : "selftest_pass_long"),
                    Buttons = td.Buttons,
                    IconSource = td.IconSource,
                    XamlRoot = MainWindow2.Instance,
                    Commands = new[]
                    {
                        ResultAsTaskItem("system_hwver", parser.HardwareVersion),
                        ResultAsTaskItem("system_swver", parser.SoftwareVersion),
                        ResultAsTaskItem("system_touchver", parser.TouchFirmwareVersion),
                        ResultWithSideAsTaskItem("system_btaddr", parser.LeftBluetoothAddress, parser.RightBluetoothAddress),
                        ResultWithSideAsTaskItem("system_proximity", parser.LeftProximity, parser.RightProximity),
                        ResultWithSideAsTaskItem("system_thermo", parser.LeftThermistor, parser.RightThermistor),
                        ResultWithSideAsTaskItem("system_adc_soc", parser.LeftAdcSOC, parser.RightAdcSOC),
                        ResultWithSideAsTaskItem("system_adc_voltage", parser.LeftAdcVCell, parser.RightAdcVCell),
                        ResultWithSideAsTaskItem("system_adc_current", parser.LeftAdcCurrent, parser.RightAdcCurrent),
                        ResultWithSideAsTaskItem("system_hall", parser.LeftHall, parser.RightHall),
                        ResultWithSideAsTaskItem("system_accel", parser.AllLeftAccelerator, parser.AllRightAccelerator)
                    }
                }.ShowAsync(true);
                td.Hide();
            }
        }
    }

    private async void OnSpatialSensorTestClicked(object? sender, RoutedEventArgs e)
    {
        var textBlock = new TextBlock
        {
            Text = Loc.Resolve("system_waiting_for_device"),
            TextWrapping = TextWrapping.Wrap,
            MaxWidth = 600
        };
        
        var cd = new ContentDialog
        {
            Title = Loc.Resolve("spatial_header"),
            Content = textBlock,
            CloseButtonText = Loc.Resolve("window_close")
        };

        using var sensorManager = new SpatialSensorManager();
        sensorManager.NewQuaternionReceived += OnNewQuaternionReceived;
        sensorManager.Attach();
        
        await cd.ShowAsync(MainWindow2.Instance);
        
        sensorManager.NewQuaternionReceived -= OnNewQuaternionReceived;
        sensorManager.Detach();
        return;
        
        void OnNewQuaternionReceived(object? s, Quaternion quat)
        {
            var rpy = quat.ToRollPitchYaw();
            textBlock.Text = $"{Loc.Resolve("spatial_dump_quaternion")}\n" +
                             $"X={quat.X}\nY={quat.Y}\nZ={quat.Z}\nW={quat.W}\n\n" + 
                             $"{Loc.Resolve("spatial_dump_rpy")}\n" +
                             $"Roll={rpy[0]}\nPitch={rpy[1]}\nYaw={rpy[2]}\n";
        }
    }
    
    private async void OnTraceDumpDownloadClicked(object? sender, RoutedEventArgs e)
    {
        var result = await new QuestionBox()
        {
            Title = Loc.Resolve("system_trace_core_dump"),
            Description = Loc.Resolve("coredump_dl_note"),
            ButtonText = Loc.Resolve("continue_button")
        }.ShowAsync();
        
        if (!result)
            return;
        
        // Begin download
        var closeButton = new TaskDialogButton(Loc.Resolve("cancel"), TaskDialogStandardResult.Close);
        var textBlock = new TextBlock
        {
            Text = string.Empty,
            TextWrapping = TextWrapping.Wrap,
            MaxWidth = 600
        };
        var td = new TaskDialog
        {
            Header = Loc.Resolve("coredump_header"),
            Buttons = [closeButton],
            IconSource = new FluentIcons.Avalonia.Fluent.SymbolIconSource { Symbol = Symbol.Bug },
            SubHeader = Loc.Resolve("coredump_dl_progress_prepare"),
            XamlRoot = MainWindow2.Instance,
            ShowProgressBar = true,
            Content = textBlock
        };
        td.SetProgressBarState(0, TaskDialogProgressState.Indeterminate);

        DeviceLogManager.Instance.ProgressUpdated += OnProgressUpdated;
        DeviceLogManager.Instance.Finished += OnFinished;
        BluetoothImpl.Instance.Disconnected += OnDisconnected;

        await DeviceLogManager.Instance.BeginDownloadAsync();
        await td.ShowAsync(true);
        
        BluetoothImpl.Instance.Disconnected -= OnDisconnected;
        DeviceLogManager.Instance.Finished -= OnFinished;
        DeviceLogManager.Instance.ProgressUpdated -= OnProgressUpdated;
        
        await DeviceLogManager.Instance.CancelDownload();
        return;
        
        void OnFinished(object? s, LogDownloadFinishedEventArgs ev)
        {
            Dispatcher.UIThread.Post(async () =>
            {
                td.SubHeader = Loc.Resolve("coredump_dl_progress_finished");
                td.SetProgressBarState(100, TaskDialogProgressState.Normal);

                var path = await MainWindow2.Instance.OpenFolderPickerAsync(
                    Loc.Resolve("coredump_dl_save_dialog_title"));
                if (string.IsNullOrEmpty(path))
                    return;

                ev.CoreDumpPaths.ForEach(x => CopyDump(x, path));
                ev.TraceDumpPaths.ForEach(x => CopyDump(x, path));

                await new MessageBox()
                {
                    Title = Loc.Resolve("coredump_dl_save_success_title"),
                    Description = string.Format(Loc.Resolve("coredump_dl_save_success"),
                        ev.CoreDumpPaths.Count + ev.TraceDumpPaths.Count, path)
                }.ShowAsync();
            }, DispatcherPriority.Normal);
        }

        void OnDisconnected(object? o, string s)
        {
            Dispatcher.UIThread.Post(() =>
            {
                td.SubHeader = Loc.Resolve($"connlost_disconnected");
                td.SetProgressBarState(100, TaskDialogProgressState.Error);
                textBlock.Text = string.Empty;
                closeButton.Text = Loc.Resolve("window_close");
            }, DispatcherPriority.Normal);
        }
        
        void OnProgressUpdated(object? s, LogDownloadProgressEventArgs ev)
        {
            Dispatcher.UIThread.Post(() =>
            {
                if (ev.DownloadType == LogDownloadProgressEventArgs.Type._Switching)
                {
                    td.SubHeader = Loc.Resolve($"coredump_dl_progress_prepare");
                    td.SetProgressBarState(0, TaskDialogProgressState.Indeterminate);
                    textBlock.Text = string.Empty;
                }
                else
                {
                    td.SubHeader = string.Format(Loc.Resolve($"coredump_dl_{(ev.DownloadType == LogDownloadProgressEventArgs.Type.Coredump ? "coredump" : "trace")}_progress"), 
                            Math.Ceiling((double)ev.CurrentByteCount / ev.TotalByteCount * 100));
                    textBlock.Text = string.Format(
                            Loc.Resolve("coredump_dl_progress_size"),
                            ev.CurrentByteCount.ToString(),
                            ev.TotalByteCount.ToString())
                        .Replace("(", "")
                        .Replace(")", "");
                    td.SetProgressBarState(Math.Ceiling(((double)ev.CurrentByteCount / ev.TotalByteCount) * 100), TaskDialogProgressState.Normal);
                }
            }, DispatcherPriority.Normal);
        }
        
        async void CopyDump(string source, string targetDir)
        {
            try
            {
                File.Copy(source, Path.Combine(targetDir, Path.GetFileName(source)));
            }
            catch (Exception ex)
            {
                Log.Error(ex, "CoreDump: Failed to copy dumps");
                await new MessageBox()
                {
                    Title = Loc.Resolve("coredump_dl_save_fail_title"),
                    Description = string.Format(Loc.Resolve("coredump_dl_save_fail"), ex.Message)
                }.ShowAsync();
            }
        }
    }
}