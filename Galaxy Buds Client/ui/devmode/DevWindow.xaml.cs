using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Net.Sockets;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
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
            //this.Closing += OnClosing;

            BluetoothService.Instance.NewDataAvailable += BluetoothServiceNewDataAvailable;
            BluetoothService.Instance.MessageReceived += BluetoothServiceMessageReceived;
        }

        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);
            if (!Properties.Settings.Default.DevModeWarningShown)
            {
                MessageBox.Show("This is the original tool I created to help me reverse-engineer the Bluetooth protocol of the Buds.\n" +
                                "I still decided to include it in the final version for debugging reasons (and in case someone wanted to play with it).\n" +
                                "However, be careful when sending custom messages to your Buds and do not send FOTA messages (Firmware Over-The-Air) unless you know what you are doing."
                    , "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                Properties.Settings.Default.DevModeWarningShown = true;
                Properties.Settings.Default.Save();
            }
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
            BluetoothService.Instance.Disconnect();
        }

        private void BluetoothServiceNewDataAvailable(object sender, byte[] e)
        {
            Dispatcher.BeginInvoke((Action)(() =>
           {
               _cache.AddRange(e);
               hexDump.Text = Hex.Dump(_cache.ToArray());
           }));
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

    }
}
