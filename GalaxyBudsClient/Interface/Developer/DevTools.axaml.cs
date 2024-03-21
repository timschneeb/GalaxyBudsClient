using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using Avalonia.Threading;
using GalaxyBudsClient.Interface.Dialogs;
using GalaxyBudsClient.Interface.ViewModels;
using GalaxyBudsClient.Message;
using GalaxyBudsClient.Model;
using GalaxyBudsClient.Model.Constants;
using GalaxyBudsClient.Platform;
using GalaxyBudsClient.Utils;
using GalaxyBudsClient.Utils.Extensions;

namespace GalaxyBudsClient.Interface.Developer;

public partial class DevTools : Window
{
    private class ViewModel
    {
        private readonly IReadOnlyList<SppMessage.MessageIds> _msgIdCache
            = Enum.GetValues(typeof(SppMessage.MessageIds)).Cast<SppMessage.MessageIds>().ToList();
        private readonly IReadOnlyList<SppMessage.MsgType> _msgTypeCache
            = Enum.GetValues(typeof(SppMessage.MsgType)).Cast<SppMessage.MsgType>().ToList();

        public IEnumerable MsgIdSource => _msgIdCache;
        public IEnumerable MsgTypeSource => _msgTypeCache;
        
        public readonly DataGridCollectionView MsgTableDataView = new(new List<RecvMsgViewHolder>());
        public readonly DataGridCollectionView PropTableDataView = new(new List<PropertyViewModel>());

        public List<RecvMsgViewHolder>? MsgTableDataSource =>
            MsgTableDataView.SourceCollection as List<RecvMsgViewHolder>;
        public List<PropertyViewModel>? PropTableDataSource =>
            PropTableDataView.SourceCollection as List<PropertyViewModel>;
    }

    private readonly List<byte> _cache = [];

    private readonly List<FilePickerFileType> _filters =
    [
        new FilePickerFileType("Hex dump") { Patterns = new List<string>() { "*.bin", "*.hex" } },
        new FilePickerFileType("All files") { Patterns = new List<string>() { "*" } }
    ];

    private readonly ViewModel _vm = new();
        
    public DevTools()
    {
        InitializeComponent();
        DataContext = _vm;

        MsgTable.ItemsSource = _vm.MsgTableDataView;
        PropTable.ItemsSource = _vm.PropTableDataView;
            
        Closing += OnClosing;
        BluetoothService.Instance.NewDataReceived += OnNewDataReceived;
    }

    private void OnNewDataReceived(object? sender, byte[] raw)
    {
        Dispatcher.UIThread.Post(() =>
        { 
            try
            {
                _cache.AddRange(raw);
                HexDump.Text = HexUtils.Dump(_cache.ToArray());

                var holder = new RecvMsgViewHolder(SppMessage.DecodeMessage(raw));
                _vm.MsgTableDataSource?.Add(holder);
                _vm.MsgTableDataView.Refresh();
                    
                HexDump.CaretIndex = int.MaxValue;
                MsgTable.ScrollIntoView(holder, null);
            }
            catch(InvalidPacketException){}
        });
    }

    private void OnClosing(object? sender, CancelEventArgs e)
    {
        BluetoothService.Instance.NewDataReceived -= OnNewDataReceived;

        _cache.Clear();
        HexDump.Clear();
        _vm.MsgTableDataSource?.Clear();
        _vm.MsgTableDataView.Refresh();
    }
        
    private void CopyPayload_OnClick(object? sender, RoutedEventArgs e)
    {
        var item = (RecvMsgViewHolder?)MsgTable.SelectedItem;
        if (item != null)
        {
            GetTopLevel(this)?.Clipboard?.SetTextAsync(item.Payload);
        }
    }
       
    private void SendMsg_Click(object? sender, RoutedEventArgs e)
    {
        if (MsgTable.SelectedItem == null || MsgTable.SelectedItem == null)
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
            Id = (SppMessage.MessageIds?) SendMsgId.SelectedItem ?? SppMessage.MessageIds.UNKNOWN_0,
            Payload = payload,
            Type = (SppMessage.MsgType?) SendMsgType.SelectedItem ?? SppMessage.MsgType.INVALID
        };
        _ = BluetoothService.Instance.SendAsync(msg);
    }
        
    private void Clear_OnClick(object? sender, RoutedEventArgs e)
    {
        _cache.Clear();
        HexDump.Clear();
        _vm.MsgTableDataSource?.Clear();
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
        var failCount = 0;
        do
        {
            var msgSize = 0;
            try
            {
                var raw = data.OfType<byte>().ToArray();

                var msg = SppMessage.DecodeMessage(raw);
                msgSize = msg.TotalPacketSize;
                    
                msgs.Add(msg);

                failCount = 0;
            }
            catch (InvalidPacketException)
            {
                // Attempt to remove broken message, otherwise skip data block
                var somIndex = 0;
                for (var i = 1; i < data.Count; i++)
                {
                    if ((BluetoothService.ActiveModel == Models.Buds &&
                         (byte)(data[i] ?? 0) == (byte)SppMessage.Constants.SOM) ||
                        (BluetoothService.ActiveModel != Models.Buds &&
                         (byte)(data[i] ?? 0) == (byte)SppMessage.Constants.SOMPlus))
                    {
                        somIndex = i;
                        break;
                    }
                }

                msgSize = somIndex;
                    
                if (failCount > 5)
                {
                    // Abandon data block
                    break;
                }
                    
                failCount++;
            }

            if (msgSize >= data.Count)
            {
                data.Clear();
                break;
            }

            data.RemoveRange(0, msgSize);

            if (ByteArrayUtils.IsBufferZeroedOut(data))
            {
                /* No more data remaining */
                break;
            }
        } while (data.Count > 0);

        foreach (var holder in msgs.Select(m => new RecvMsgViewHolder(m)))
        {
            _vm.MsgTableDataSource?.Add(holder);
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

    private void MsgTable_OnSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        var item = (RecvMsgViewHolder?)MsgTable.SelectedItem;
        if (item?.Message == null)
        {
            _vm.PropTableDataSource?.Clear();
        }
        else
        {
            var parser = item.Message.BuildParser();
            if (parser != null)
            {
                _vm.PropTableDataSource?.Clear();
                foreach (var (key, value) in parser.ToStringMap())
                {
                    _vm.PropTableDataSource?.Add(new PropertyViewModel(key, value));
                }
            }
            else
            {
                _vm.PropTableDataSource?.Clear();
            }
        }
            
        _vm.PropTableDataView.Refresh();
    }
}