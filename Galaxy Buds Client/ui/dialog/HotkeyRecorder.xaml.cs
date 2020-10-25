using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using WindowsInput.Native;
using Galaxy_Buds_Client.util;

namespace Galaxy_Buds_Client.ui.dialog
{
    /// <summary>
    /// Interaction logic for HotkeyRecorder.xaml
    /// </summary>
    public partial class HotkeyRecorder : Window
    {
        public String HotkeyString
        {
            get
            {
                bool first = true;
                StringBuilder sb = new StringBuilder();

                if (Hotkeys == null)
                    return "null";

                foreach (var key in Hotkeys)
                {
                    if (!first)
                    {
                        sb.Append("+");
                    }
                    sb.Append(key);

                    first = false;
                }

                return sb.ToString();
            }
        }

        public List<Key> Hotkeys { get; private set; }

        public List<VirtualKeyCode> HotKeysVirtualCode
        {
            get
            {
                List<VirtualKeyCode> a = new List<VirtualKeyCode>();
                foreach (var hotkey in Hotkeys)
                {
                    a.Add((VirtualKeyCode)KeyInterop.VirtualKeyFromKey(hotkey));
                }
                return a;
            }
        }
        public List<int> HotKeysVirtualCodeRaw
        {
            get
            {
                List<int> a = new List<int>();
                foreach (var hotkey in Hotkeys)
                {
                    a.Add(KeyInterop.VirtualKeyFromKey(hotkey));
                }
                return a;
            }
        }

        private bool _recording = false;

        public HotkeyRecorder()
        {
            InitializeComponent();
        }

        private void Cancel_OnClick(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void Apply_OnClick(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }

        private void HotkeyRecorder_OnKeyUp(object sender, KeyEventArgs e)
        {
            _recording = false;
            e.Handled = true;
        }

        private void HotkeyRecorder_OnKeyDown(object sender, KeyEventArgs e)
        {
            if (!_recording || Hotkeys == null)
            {
                Hotkeys = null;
                Hotkeys = new List<Key>();
            }

            if (Hotkeys.Contains(e.Key))
                return;

            Hotkeys.Add(e.Key);

            _recording = true;

            KeyString.Content = HotkeyString;
            e.Handled = true;
        }

    }
}
