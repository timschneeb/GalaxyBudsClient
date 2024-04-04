using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using Avalonia;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Platform;
using Avalonia.Platform.Storage;
using Avalonia.Threading;
using AvaloniaHex.Document;
using DynamicData;
using GalaxyBudsClient.Interface.Dialogs;
using GalaxyBudsClient.Interface.ViewModels.Developer;
using GalaxyBudsClient.Message;
using GalaxyBudsClient.Model;
using GalaxyBudsClient.Model.Constants;
using GalaxyBudsClient.Platform;
using GalaxyBudsClient.Utils;
using GalaxyBudsClient.Utils.Extensions;

namespace GalaxyBudsClient.Interface.Developer;

public partial class DevTools : StyledWindow.StyledWindow
{
    private readonly List<FilePickerFileType> _filters =
    [
        new FilePickerFileType("Hex dump") { Patterns = new List<string>() { "*.bin", "*.hex" } },
        new FilePickerFileType("All files") { Patterns = new List<string>() { "*" } }
    ];

    private readonly DevToolsViewModel _vm = new();
        
    public DevTools()
    {
        InitializeComponent();

        var monoFonts = new FontFamily(null, $"{Program.AvaresUrl}/Resources/Fonts/RobotoMono-Regular.ttf#Roboto Mono,Consolas,Hack,Monospace,monospace");
        HexEditor.FontFamily = monoFonts;
        
        DataContext = _vm;
        
        Closing += OnClosing;
        BluetoothImpl.Instance.NewDataReceived += OnNewDataReceived;
    }

    private void OnNewDataReceived(object? sender, byte[] raw)
    {
        Dispatcher.UIThread.Post(() =>
        { 
            try
            {
                HexEditor.Document ??= new InMemoryBinaryDocument();
                HexEditor.Document.InsertBytes(HexEditor.Document.Length, new ReadOnlySpan<byte>(raw));
                
                if(_vm.IsAutoscrollEnabled)
                    HexEditor.Caret.Location = new BitLocation(HexEditor.Document.Length - 1);
                HexEditor.HexView.InvalidateVisualLines();

                var holder = new MessageViewHolder(SppMessage.Decode(raw, BluetoothImpl.ActiveModel));
                _vm.MsgTableDataSource.Add(holder);
                _vm.MsgTableDataView.Refresh();
                
                if(_vm.IsAutoscrollEnabled)
                    MsgTable.ScrollIntoView(holder, null);
            }
            catch(InvalidPacketException){}
        });
    }

    private void OnClosing(object? sender, CancelEventArgs e)
    {
        BluetoothImpl.Instance.NewDataReceived -= OnNewDataReceived;

        HexEditor.Document = null;
        _vm.MsgTableDataSource.Clear();
        _vm.MsgTableDataView.Refresh();
    }
        
    private void CopyPayload_OnClick(object? sender, RoutedEventArgs e)
    {
        var item = (MessageViewHolder?)MsgTable.SelectedItem;
        if (item != null)
        {
            GetTopLevel(this)?.Clipboard?.SetTextAsync(item.Payload);
        }
    }
       
    private void SendMsg_Click(object? sender, RoutedEventArgs e)
    {
        if (SendMsgId.SelectedItem == null || SendMsgType.SelectedItem == null)
        {
            _ = new MessageBox
            {
                Title = "Error", 
                Description = "Please fill all fields out."
            }.ShowAsync(this);
            return;
        }

        byte[] payload;
        try
        {
            payload = SendMsgPayload.Text.HexStringToByteArray();
        }
        catch (Exception)
        {
            _ = new MessageBox
            {
                Title = "Invalid payload format",
                Description = "Correct format: 00 01 FF E5 [...]"
            }.ShowAsync(this);
            return;
        }

        var msg = new SppMessage
        {
            Id = (MsgIds?) SendMsgId.SelectedItem ?? MsgIds.UNKNOWN_0,
            Payload = payload,
            Type = (MsgTypes?) SendMsgType.SelectedItem ?? MsgTypes.Request
        };
        _ = BluetoothImpl.Instance.SendAsync(msg);
    }
        
    private void Clear_OnClick(object? sender, RoutedEventArgs e)
    {
        HexEditor.Document?.RemoveBytes(0, HexEditor.Document.Length);
        _vm.MsgTableDataSource.Clear();
        _vm.MsgTableDataView.Refresh();
    }

    private async void LoadDump_OnClick(object? sender, RoutedEventArgs e)
    {
        var file = await this.OpenFilePickerAsync(_filters);
        if (file == null)
            return;

        var model = await new EnumChoiceBox<Models> { Title = "Choose model for dump..." }.OpenDialogAsync(this);
        if(model == null)
            return;
        
        var content = await File.ReadAllBytesAsync(file);
        try
        {
            HexEditor.Document ??= new InMemoryBinaryDocument();
            HexEditor.Document.RemoveBytes(0, HexEditor.Document.Length);
            HexEditor.Document.InsertBytes(HexEditor.Document.Length, new ReadOnlySpan<byte>(content));
        }
        catch (Exception ex)
        {
            _ = new MessageBox
            {
                Title = "Error while reading file", 
                Description = ex.Message
            }.ShowAsync(this);
            return;
        }

        try
        {
            _vm.MsgTableDataSource.AddRange(
                SppMessage.DecodeRawChunk([..content], model.Value)
                    .Select(m => new MessageViewHolder(m)));
            _vm.MsgTableDataView.Refresh();
        }
        catch (InvalidPacketException ex)
        {
            _ = new MessageBox
            {
                Title = "Error while decoding message", 
                Description = $"Error code: {ex.ErrorCode}\n\n{ex.Message}"
            }.ShowAsync(this);
        }
    }

    private async void SaveDump_OnClick(object? sender, RoutedEventArgs e)
    {
        var path = await this.SaveFilePickerAsync(_filters, "*.bin", "dump.bin");
        if (path == null)
            return;
            
        try
        {
            await using var fs = new FileStream(path, FileMode.Create, FileAccess.Write);
            HexEditor.Document?.WriteAllToStream(fs);
        }
        catch (Exception ex)
        {
            await new MessageBox
            {
                Title = "Error while saving file", 
                Description = ex.Message
            }.ShowAsync(this);             
        }
    }
}