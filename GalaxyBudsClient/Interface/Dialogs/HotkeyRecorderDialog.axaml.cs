using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using FluentAvalonia.UI.Controls;
using GalaxyBudsClient.Interface.ViewModels.Dialogs;
using GalaxyBudsClient.Utils.Interface.DynamicLocalization;

namespace GalaxyBudsClient.Interface.Dialogs;

public partial class HotkeyRecorderDialog : UserControl
{
    private bool _recording;
    private TopLevel? _topLevel;
        
    public HotkeyRecorderDialog()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        _topLevel = TopLevel.GetTopLevel(this);
        if (_topLevel != null)
        {
            _topLevel.KeyUp += OnKeyUp;
            _topLevel.KeyDown += OnKeyDown;
        }
        base.OnAttachedToVisualTree(e);
    }
        
    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        if (_topLevel != null)
        {
            _topLevel.KeyUp -= OnKeyUp;
            _topLevel.KeyDown -= OnKeyDown;
        }
        base.OnDetachedFromVisualTree(e);
    }
        
    private void OnKeyUp(object? sender, KeyEventArgs e)
    {
        _recording = false;
        e.Handled = true;
    }

    private void OnKeyDown(object? sender, KeyEventArgs e)
    {
        if (DataContext is not HotkeyRecorderDialogViewModel vm)
            return;
            
        if (!_recording)
        {
            vm.Hotkeys.Clear();
        }

        if (vm.Hotkeys.Contains(e.Key))
            return;

        vm.Hotkeys.Add(e.Key);
        _recording = true;
        e.Handled = true;
    }
        
    public static async Task<List<Key>?> OpenDialogAsync()
    {
        var dialog = new ContentDialog
        {
            Title = Loc.Resolve("hotrec_content"),
            PrimaryButtonText = Loc.Resolve("okay"),
            CloseButtonText = Loc.Resolve("cancel")
        };

        var viewModel = new HotkeyRecorderDialogViewModel();
        dialog.Content = new HotkeyRecorderDialog
        {
            DataContext = viewModel
        };

        dialog.PrimaryButtonClick += OnPrimaryButtonClick;
        var result = await dialog.ShowAsync(MainWindow2.Instance);
        dialog.PrimaryButtonClick -= OnPrimaryButtonClick;
            
        return result == ContentDialogResult.None ? null : viewModel.Hotkeys.ToList();

        void OnPrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            var defer = args.GetDeferral();
            args.Cancel = viewModel.Hotkeys.Count == 0;

            if (args.Cancel)
            {
                var resultHint = new ContentDialog
                {
                    Content = Loc.Resolve("error"),
                    Title = Loc.Resolve("hotkey_edit_invalid"),
                    PrimaryButtonText = Loc.Resolve("window_close")
                };
                _ = resultHint.ShowAsync(MainWindow2.Instance)
                    .ContinueWith(_ => defer.Complete());
            }
            else
            {
                defer.Complete();
            }
        }
    }
}