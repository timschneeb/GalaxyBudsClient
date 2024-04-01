using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using Avalonia.Threading;
using GalaxyBudsClient.Interface.Dialogs;
using GalaxyBudsClient.Interface.ViewModels.Developer;
using GalaxyBudsClient.Message;
using GalaxyBudsClient.Model;
using GalaxyBudsClient.Platform;
using GalaxyBudsClient.Utils;
using GalaxyBudsClient.Utils.Extensions;

namespace GalaxyBudsClient.Interface.Developer;

public partial class DevTools : StyledWindow.StyledWindow
{
    private readonly List<byte> _cache = [];

    private readonly List<FilePickerFileType> _filters =
    [
        new FilePickerFileType("Hex dump") { Patterns = new List<string>() { "*.bin", "*.hex" } },
        new FilePickerFileType("All files") { Patterns = new List<string>() { "*" } }
    ];

    private readonly DevToolsViewModel _vm = new();
        
    public DevTools()
    {
        InitializeComponent();
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
                _cache.AddRange(raw);
                HexDump.Text = HexUtils.Dump(_cache.ToArray());

                var holder = new MessageViewHolder(SppMessage.DecodeMessage(raw, BluetoothImpl.ActiveModel));
                _vm.MsgTableDataSource.Add(holder);
                _vm.MsgTableDataView.Refresh();
                    
                HexDump.CaretIndex = int.MaxValue;
                MsgTable.ScrollIntoView(holder, null);
            }
            catch(InvalidPacketException){}
        });
    }

    private void OnClosing(object? sender, CancelEventArgs e)
    {
        BluetoothImpl.Instance.NewDataReceived -= OnNewDataReceived;

        _cache.Clear();
        HexDump.Clear();
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
        catch (ArgumentOutOfRangeException)
        {
            _ = new MessageBox
            {
                Title = "Invalid payload format",
                Description = "Correct format: 00 01 FF E5 [...]"
            }.ShowAsync(this);
            return;
        }
        catch (FormatException)
        {
            _ = new MessageBox
            {
                Title = "Payload not hexadecimal",
                Description = "Correct format: 00 01 FF E5 [...]"
            }.ShowAsync(this);
            return;
        }

        var msg = new SppMessage
        {
            Id = (MsgIds?) SendMsgId.SelectedItem ?? MsgIds.UNKNOWN_0,
            Payload = payload,
            Type = (MsgTypes?) SendMsgType.SelectedItem ?? MsgTypes.INVALID
        };
        _ = BluetoothImpl.Instance.SendAsync(msg);
    }
        
    private void Clear_OnClick(object? sender, RoutedEventArgs e)
    {
        _cache.Clear();
        HexDump.Clear();
        _vm.MsgTableDataSource.Clear();
        _vm.MsgTableDataView.Refresh();
    }

    private async void LoadDump_OnClick(object? sender, RoutedEventArgs e)
    {
        var file = await this.OpenFilePickerAsync(_filters);
        if (file == null)
            return;
            
        ArrayList data;
        try
        {
            data = new ArrayList(await File.ReadAllBytesAsync(file));
            _cache.Clear();
            _cache.AddRange((byte[]) data.ToArray(typeof(byte)));
            HexDump.Text = HexUtils.Dump(_cache.ToArray());
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

        var msgs = new List<SppMessage>();
        do
        {
            SppMessage msg;
            try
            {
                var raw = data.OfType<byte>().ToArray();
                msg = SppMessage.DecodeMessage(raw, BluetoothImpl.ActiveModel); // TODO: Implement model select dialog
                msgs.Add(msg);
            }
            catch (InvalidPacketException ex)
            {
                _ = new MessageBox
                {
                    Title = "Error while decoding message", 
                    Description = $"{ex.ErrorCode} {ex.Message}"
                }.ShowAsync(this);
                break;
            }

            if (msg.TotalPacketSize >= data.Count)
            {
                data.Clear();
                break;
            }

            data.RemoveRange(0, msg.TotalPacketSize);

            if (ByteArrayUtils.IsBufferZeroedOut(data))
            {
                /* No more data remaining */
                break;
            }
        } while (data.Count > 0);

        foreach (var holder in msgs.Select(m => new MessageViewHolder(m)))
        {
            _vm.MsgTableDataSource.Add(holder);
        }
        _vm.MsgTableDataView.Refresh();
    }

    private async void SaveDump_OnClick(object? sender, RoutedEventArgs e)
    {
        var path = await this.SaveFilePickerAsync(_filters, "*.bin", "dump.bin");
        if (path == null)
            return;
            
        try
        {
            await using var fs = new FileStream(path, FileMode.Create, FileAccess.Write);
            fs.Write(_cache.ToArray(), 0, _cache.ToArray().Length);
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