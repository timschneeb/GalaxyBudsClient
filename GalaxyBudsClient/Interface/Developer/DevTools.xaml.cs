using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using Avalonia;
using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Input.Raw;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using GalaxyBudsClient.Bluetooth.Linux;
using GalaxyBudsClient.Interface.Dialogs;
using GalaxyBudsClient.Message;
using GalaxyBudsClient.Model;
using GalaxyBudsClient.Model.Constants;
using GalaxyBudsClient.Model.ViewModels;
using GalaxyBudsClient.Platform;
using GalaxyBudsClient.Utils;

namespace GalaxyBudsClient.Interface.Developer
{
    public sealed class DevTools : Window
    {
        private class ViewModel
        {
            public ViewModel()
            {
            }
            
            private readonly IReadOnlyList<SPPMessage.MessageIds> _msgIdCache
                = Enum.GetValues(typeof(SPPMessage.MessageIds)).Cast<SPPMessage.MessageIds>().ToList();
            private readonly IReadOnlyList<SPPMessage.MsgType> _msgTypeCache
                = Enum.GetValues(typeof(SPPMessage.MsgType)).Cast<SPPMessage.MsgType>().ToList();

            public IEnumerable MsgIdSource => _msgIdCache;
            public IEnumerable MsgTypeSource => _msgTypeCache;
        
            public readonly DataGridCollectionView MsgTableDataView = new DataGridCollectionView(new List<RecvMsgViewHolder>());
            public readonly DataGridCollectionView PropTableDataView = new DataGridCollectionView(new List<PropertyViewModel>());

            public List<RecvMsgViewHolder>? MsgTableDataSource =>
                MsgTableDataView.SourceCollection as List<RecvMsgViewHolder>;
            public List<PropertyViewModel>? PropTableDataSource =>
                PropTableDataView.SourceCollection as List<PropertyViewModel>;
        }

        private readonly List<byte> _cache = new List<byte>();

        private readonly DataGrid _msgTable;
        private readonly DataGrid _propTable;
        private readonly TextBox _hexDump;
        private readonly TextBox _hexPayloadSend;
        private readonly ComboBox _msgIdSend;
        private readonly ComboBox _msgTypeSend;

        private readonly List<FileDialogFilter> _filters = new List<FileDialogFilter>()
        {
            new FileDialogFilter {Name = "Hex dump", Extensions = new List<string>() {"bin", "hex"}},
            new FileDialogFilter {Name = "All files", Extensions = new List<string>() {"*"}},
        };

        private readonly ViewModel _vm = new ViewModel();
        
        public DevTools()
        {
            DataContext = _vm;
            AvaloniaXamlLoader.Load(this);
            this.AttachDevTools();

            _msgTable = this.FindControl<DataGrid>("MsgTable");
            _propTable = this.FindControl<DataGrid>("PropTable");
            _hexDump = this.FindControl<TextBox>("HexDump");
            _msgIdSend = this.FindControl<ComboBox>("SendMsgId"); 
            _msgTypeSend = this.FindControl<ComboBox>("SendMsgType");
            _hexPayloadSend = this.FindControl<TextBox>("SendMsgPayload");

            _msgTable.Items = _vm.MsgTableDataView;
            _propTable.Items = _vm.PropTableDataView;
            
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
                    _hexDump.Text = HexUtils.Dump(_cache.ToArray());

                    RecvMsgViewHolder holder = new RecvMsgViewHolder(SPPMessage.DecodeMessage(raw));
                    _vm.MsgTableDataSource?.Add(holder);
                    _vm.MsgTableDataView.Refresh();
                }
                catch(InvalidPacketException){}
            });
        }

        private void OnClosing(object? sender, CancelEventArgs e)
        {
            BluetoothImpl.Instance.NewDataReceived -= OnNewDataReceived;

            _cache.Clear();
            _hexDump.Clear();
            _vm.MsgTableDataSource?.Clear();
            _vm.MsgTableDataView.Refresh();
        }
        
        private void CopyPayload_OnClick(object? sender, RoutedEventArgs e)
        {
            var item = (RecvMsgViewHolder?)_msgTable.SelectedItem;
            if (item != null)
            {
                Application.Current?.Clipboard?.SetTextAsync(item.Payload);
            }
        }
       
        private void SendMsg_Click(object? sender, RoutedEventArgs e)
        {
            if (_msgIdSend.SelectedItem == null || _msgTypeSend.SelectedItem == null)
            {
                new MessageBox
                {
                    Title = "Error", 
                    Description = "Please fill all fields out."
                }.ShowDialog(this);
                return;
            }

            byte[] payload;
            try
            {
                payload = _hexPayloadSend.Text.HexStringToByteArray();
            }
            catch (ArgumentOutOfRangeException)
            {
                new MessageBox
                {
                    Title = "Invalid payload format",
                    Description = "Correct format: 00 01 FF E5 [...]"
                }.ShowDialog(this);
                return;
            }
            catch (FormatException)
            {
                new MessageBox
                {
                    Title = "Payload not hexadecimal",
                    Description = "Correct format: 00 01 FF E5 [...]"
                }.ShowDialog(this);
                return;
            }

            SPPMessage msg = new SPPMessage
            {
                Id = (SPPMessage.MessageIds) _msgIdSend.SelectedItem,
                Payload = payload,
                Type = (SPPMessage.MsgType) _msgTypeSend.SelectedItem
            };
            var _ = BluetoothImpl.Instance.SendAsync(msg);
        }
        
        private void Clear_OnClick(object? sender, RoutedEventArgs e)
        {
            _cache.Clear();
            _hexDump.Clear();
            _vm.MsgTableDataSource?.Clear();
            _vm.MsgTableDataView.Refresh();
        }

        private async void LoadDump_OnClick(object? sender, RoutedEventArgs e)
        {
            OpenFileDialog dlg = new OpenFileDialog {Filters = _filters, AllowMultiple = false};
            string[]? paths = await dlg.ShowAsync(this);
            
            if (paths == null || paths.Length < 1)
            {
                return;
            }

            ArrayList data;
            try
            {
                data = new ArrayList(await File.ReadAllBytesAsync(paths[0]));
                _cache.Clear();
                _cache.AddRange((byte[]) data.ToArray(typeof(byte)));
                _hexDump.Text = HexUtils.Dump(_cache.ToArray());
            }
            catch (Exception ex)
            {
                await new MessageBox
                {
                    Title = "Error while reading file", 
                    Description = ex.Message
                }.ShowDialog(this);
                return;
            }

            var msgs = new List<SPPMessage>();
            int failCount = 0;
            do
            {
                int msgSize = 0;
                SPPMessage? msg = null;
                try
                {
                    var raw = data.OfType<byte>().ToArray();

                    msg = SPPMessage.DecodeMessage(raw);
                    msgSize = msg.TotalPacketSize;
                    
                    msgs.Add(msg);

                    failCount = 0;
                }
                catch (InvalidPacketException)
                {
                    // Attempt to remove broken message, otherwise skip data block
                    var somIndex = 0;
                    for (int i = 1; i < data.Count; i++)
                    {
                        if ((BluetoothImpl.Instance.ActiveModel == Models.Buds &&
                            (byte)(data[i] ?? 0) == (byte)SPPMessage.Constants.SOM) ||
                            (BluetoothImpl.Instance.ActiveModel != Models.Buds &&
                             (byte)(data[i] ?? 0) == (byte)SPPMessage.Constants.SOMPlus))
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
            SaveFileDialog dlg = new SaveFileDialog {Filters = _filters};
            string? path = await dlg.ShowAsync(this);
            
            if (path == null)
            {
                return;
            }
            
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
                }.ShowDialog(this);             
            }
        }

        private void MsgTable_OnSelectionChanged(object? sender, SelectionChangedEventArgs e)
        {
            RecvMsgViewHolder? item = (RecvMsgViewHolder?)_msgTable.SelectedItem;
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
}