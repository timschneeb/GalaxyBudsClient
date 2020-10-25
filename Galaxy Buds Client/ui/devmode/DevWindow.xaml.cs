using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using Galaxy_Buds_Client.message;
using Galaxy_Buds_Client.model;
using Galaxy_Buds_Client.util;

namespace Galaxy_Buds_Client.ui.devmode
{
    /// <summary>
    /// Interaktionslogik für DevWindow.xaml
    /// </summary>
    /// 
    public partial class DevWindow : Window
    {
        public readonly SynchronizationContext Context = SynchronizationContext.Current;
        private readonly List<byte> _cache = new List<byte>();

        public DevWindow()
        {
            InitializeComponent();
            this.Closing += OnClosing;

            BluetoothService.Instance.NewDataAvailable += BluetoothServiceNewDataAvailable;
            BluetoothService.Instance.MessageReceived += BluetoothServiceMessageReceived;
        }

        private void BluetoothServiceMessageReceived(object sender, SPPMessage e)
        {
            Dispatcher.BeginInvoke((Action) (() =>
            {
                RecvMsgViewHolder holder = new RecvMsgViewHolder(e);
                recvTable.Items.Add(holder);
            }));
        }

        private void OnClosing(object sender, CancelEventArgs e)
        {
            BluetoothService.Instance.NewDataAvailable -= BluetoothServiceNewDataAvailable;
            BluetoothService.Instance.MessageReceived -= BluetoothServiceMessageReceived;

            _cache.Clear();
            hexDump.Clear();
            recvTable.Items.Clear();
        }

        private void BluetoothServiceNewDataAvailable(object sender, byte[] e)
        {
            Dispatcher.BeginInvoke((Action)(() =>
           {
               _cache.AddRange(e);
               hexDump.Text = Hex.Dump(_cache.ToArray());
           }));
        }

        private void MenuParseDump_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog
            {
                DefaultExt = ".hex",
                Filter = "Hex dump|*.hex|All files|*.*"
            };
            bool? result = dlg.ShowDialog();

            // Process file dialog box results
            if (result == true)
            {
                try
                {
                    ArrayList data = new ArrayList(File.ReadAllBytes(dlg.FileName));
                    
                    Dispatcher.BeginInvoke((Action)(() =>
                    {
                        _cache.Clear();
                        _cache.AddRange((byte[])data.ToArray(typeof(byte)));
                        hexDump.Text = Hex.Dump(_cache.ToArray());

                        do
                        {
                            try
                            {
                                SPPMessage msg = SPPMessage.DecodeMessage((byte[])data.ToArray(typeof(byte)));
                                RecvMsgViewHolder holder = new RecvMsgViewHolder(msg);
                                recvTable.Items.Add(holder);

                                if (msg.TotalPacketSize >= data.Count)
                                    break;
                                data.RemoveRange(0, msg.TotalPacketSize);
                            }
                            catch (InvalidDataException ex)
                            {
                                Console.WriteLine("Error while parsing dump: " + ex.Message);
                            }
                        } while (data.Count > 0);
                    }));
                }
                catch (Exception ex)
                {
                    MessageBox.Show(this, "Error", ex.Message, MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void MenuDumpFile_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.SaveFileDialog dlg = new Microsoft.Win32.SaveFileDialog
            {
                FileName = "dump", DefaultExt = ".hex", Filter = "Hex dump|*.hex"
            };
            bool? result = dlg.ShowDialog();

            // Process save file dialog box results
            if (result == true)
            {
                try
                {
                    using (var fs = new FileStream(dlg.FileName, FileMode.Create, FileAccess.Write))
                    {
                        fs.Write(_cache.ToArray(), 0, _cache.ToArray().Length);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(this, "Error", ex.Message, MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void MenuClear_Click(object sender, RoutedEventArgs e)
        {
            _cache.Clear();
            hexDump.Clear();
            recvTable.Items.Clear();
        }
        private void SendMsg_Click(object sender, RoutedEventArgs e)
        {
            if (SendMsgId.SelectedValue == null || SendMsgType.SelectedValue == null)
            {
                MessageBox.Show("Please fill all fields out.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            SPPMessage msg = new SPPMessage
            {
                Id = (SPPMessage.MessageIds) SendMsgId.SelectedValue,
                Payload = SendMsgPayload.Text.HexStringToByteArray(),
                Type = (SPPMessage.MsgType) SendMsgType.SelectedValue
            };
            BluetoothService.Instance.SendAsync(msg);
        }

        private void recvTable_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            RecvMsgViewHolder item = (RecvMsgViewHolder)recvTable.SelectedItem;
            if (item == null || item.Message == null)
            {
                recvMsgProperties.Items.Clear();
            }
            else
            {
                var parser = item.Message.BuildParser();
                if (parser != null)
                {
                    recvMsgProperties.Items.Clear();
                    foreach (var pair in parser.ToStringMap())
                    {
                        var viewmodel = new PropViewModel(pair.Key, pair.Value);
                        recvMsgProperties.Items.Add(viewmodel);
                    }
                }
                else
                {
                    recvMsgProperties.Items.Clear();
                }
            }
        }

        private void CopySelectedCell_OnExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            RecvMsgViewHolder item = (RecvMsgViewHolder)recvTable.SelectedItem;
            if (item != null)
            {
                Clipboard.SetText(item.Payload);
            }
        }
    }
}
