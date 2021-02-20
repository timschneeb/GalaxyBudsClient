// 32feet.NET - Personal Area Networking for .NET
//
// InTheHand.Windows.Forms.SelectBluetoothDeviceDialogForm
// 
// Copyright (c) 2003-2011 In The Hand Ltd, All rights reserved.
// This source code is licensed under the Microsoft Public License (Ms-PL) - see License.txt



using System;
using System.Collections;
#if NETCF
using InTheHand.ComponentModel;
#else
using System.ComponentModel;
#endif
using System.Net.Sockets;
#if !NO_WINFORMS
using System.Drawing;
using System.Windows.Forms;
#endif
using InTheHand.Net.Sockets;
using InTheHand.Net.Bluetooth;
using System.Runtime.InteropServices;
using System.Diagnostics;

namespace InTheHand.Windows.Forms
{
    /// <summary>
    /// Managed code dialog for Windows CE systems.
    /// </summary>
    internal sealed class SelectBluetoothDeviceForm 
#if ! NO_WINFORMS
        : System.Windows.Forms.Form, ISelectBluetoothDevice
#endif
    {
#if ! NO_WINFORMS
#if NETCF
        private const int LVS_EX_GRADIENT = 0x20000000;
        private const int LVS_EX_THEME = 0x02000000;
        private Label labelInfo;
        private const int LVM_SETEXTENDEDLISTVIEWSTYLE = 0x1036;
        private ImageList imageList1;
        //private const int GWL_EXSTYLE = -20;
        private int dpi = GetDeviceCaps(IntPtr.Zero, 88);

        [DllImport("coredll")]
        private static extern int SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);

        [DllImport("coredll", EntryPoint = "GetDeviceCaps", SetLastError = true)]
        private static extern int GetDeviceCaps(IntPtr hdc, int nIndex);
        //private static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

        //[DllImport("coredll")]
        //private static extern int GetWindowLong(IntPtr hWnd, int nIndex); 
#endif


        private System.Windows.Forms.MainMenu mainMenu1;
        private System.Windows.Forms.MenuItem mnuMenu;
        private System.Windows.Forms.MenuItem mnuSelect;
        private System.Windows.Forms.MenuItem mnuAgain;
        private System.Windows.Forms.ListView lvDevices;
        private System.Windows.Forms.MenuItem mnuCancel;

        public BluetoothDeviceInfo selectedDevice;
        private System.Windows.Forms.ColumnHeader clmDevice;
        bool fShowAuthenticated, fShowRemembered, fShowUnknown, fForceAuthentication, fDiscoverableOnly;
        Predicate<BluetoothDeviceInfo> _filterFn;
        private string info = string.Empty;
        volatile bool _closed;

        //----
        public SelectBluetoothDeviceForm(/*BLUETOOTH_SELECT_DEVICE_PARAMS p*/)
        {
            //
            // Required for Windows Form Designer support
            //
            InitializeComponent();
            //
            classOfDeviceFilter = new System.Collections.Generic.List<uint>();
            //
#if ! WinCE
            this.Size = new Size(400, 250);
#endif

            fShowAuthenticated = true;
            fShowRemembered = true;
            fShowUnknown = true;
            fForceAuthentication = false;

#if NETCF
            switch (dpi)
            {
                case 96:
                    imageList1.ImageSize = new Size(24, 24);
                    imageList1.Images.Add(new Icon(System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream("InTheHand.Net.Windows.Forms.1f00.ico"),24,24));
                    break;
                case 128:
                    imageList1.ImageSize = new Size(32, 32);
                    imageList1.Images.Add(new Icon(System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream("InTheHand.Net.Windows.Forms.1f00.ico"), 32, 32));
                    break;
                case 131:
                    imageList1.ImageSize = new Size(32, 32);
                    imageList1.Images.Add(new Icon(System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream("InTheHand.Net.Windows.Forms.1f00.ico"), 32, 32));
                    break;
                case 192:
                    imageList1.ImageSize = new Size(48, 48);
                    imageList1.Images.Add(new Icon(System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream("InTheHand.Net.Windows.Forms.1f00.ico"), 48, 48));
                    break;
            }
#endif
        }

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code
        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.Windows.Forms.ListViewItem listViewItem1 = new System.Windows.Forms.ListViewItem();
            this.lvDevices = new System.Windows.Forms.ListView();
            this.clmDevice = new System.Windows.Forms.ColumnHeader();
            this.mainMenu1 = new System.Windows.Forms.MainMenu();
            this.mnuSelect = new System.Windows.Forms.MenuItem();
            this.mnuMenu = new System.Windows.Forms.MenuItem();
            this.mnuAgain = new System.Windows.Forms.MenuItem();
            this.mnuCancel = new System.Windows.Forms.MenuItem();
#if NETCF
            this.imageList1 = new System.Windows.Forms.ImageList();
            this.labelInfo = new System.Windows.Forms.Label();
#endif
            this.SuspendLayout();
            // 
            // lvDevices
            // 
            this.lvDevices.Columns.Add(this.clmDevice);
            this.lvDevices.Font = new System.Drawing.Font("Tahoma", 12F, System.Drawing.FontStyle.Regular);
            this.lvDevices.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.None;
            listViewItem1.Text = "Scanning for Bluetooth devices...";
            this.lvDevices.Items.Add(listViewItem1);
            this.lvDevices.Location = new System.Drawing.Point(8, 32);
            this.lvDevices.Name = "lvDevices";
            this.lvDevices.Size = new System.Drawing.Size(100, 200);
#if NETCF
            this.lvDevices.SmallImageList = this.imageList1;
#endif
            this.lvDevices.TabIndex = 0;
            this.lvDevices.View = System.Windows.Forms.View.Details;
            this.lvDevices.ItemActivate += new System.EventHandler(this.selectDevice);
            // 
            // clmDevice
            // 
            this.clmDevice.Text = "ColumnHeader";
            this.clmDevice.Width = 100;
            // 
            // mainMenu1
            // 
            this.mainMenu1.MenuItems.Add(this.mnuSelect);
            this.mainMenu1.MenuItems.Add(this.mnuMenu);
            // 
            // mnuSelect
            // 
            this.mnuSelect.Enabled = false;
            this.mnuSelect.Text = "Select";
            this.mnuSelect.Click += new System.EventHandler(this.selectDevice);
            // 
            // mnuMenu
            // 
            this.mnuMenu.MenuItems.Add(this.mnuAgain);
            this.mnuMenu.MenuItems.Add(this.mnuCancel);
            this.mnuMenu.Text = "Menu";
            // 
            // mnuAgain
            // 
            this.mnuAgain.Enabled = false;
            this.mnuAgain.Text = "Search Again";
            this.mnuAgain.Click += new System.EventHandler(this.mnuAgain_Click);
            // 
            // mnuCancel
            // 
            this.mnuCancel.Text = "Cancel";
            this.mnuCancel.Click += new System.EventHandler(this.mnuCancel_Click);
#if NETCF
            // 
            // imageList1
            // 
            this.imageList1.ImageSize = new System.Drawing.Size(16, 16);
            // 
            // labelInfo
            // 
            this.labelInfo.Location = new System.Drawing.Point(3, 3);
            this.labelInfo.Name = "labelInfo";
            this.labelInfo.Size = new System.Drawing.Size(0, 20);
            this.labelInfo.Font = new Font("Tahoma", 9f, FontStyle.Bold);
            this.labelInfo.ForeColor = SystemColors.ActiveCaption;
#endif
            // 
            // SelectBluetoothDeviceForm
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Inherit;
            this.ClientSize = new System.Drawing.Size(240, 240);
            this.ControlBox = false;
#if NETCF
            this.Controls.Add(this.labelInfo);
#endif
            this.Controls.Add(this.lvDevices);
            this.Menu = this.mainMenu1;
            this.Name = "SelectBluetoothDeviceForm";
            this.Text = "Bluetooth";
            this.Load += new System.EventHandler(this.SelectBluetoothDeviceForm_Load);
            this.Resize += new System.EventHandler(this.SelectBluetoothDeviceForm_Resize);
            this.ResumeLayout(false);

        }
        #endregion

        private void mnuCancel_Click(object sender, System.EventArgs e)
        {
            _closed = true;
            Cursor.Current = Cursors.Default;
            this.DialogResult = DialogResult.Cancel;
        }

        private void SelectBluetoothDeviceForm_Load(object sender, System.EventArgs e)
        {
            Start();

#if NETCF
            //pick a font size based on dpi and use font smoothing
            /*Microsoft.WindowsCE.Forms.LogFont lf = new Microsoft.WindowsCE.Forms.LogFont();
            lf.FaceName = this.Font.Name;
            
            lf.Height = (int)(0.3 * dpi);
            lf.Quality = Microsoft.WindowsCE.Forms.LogFontQuality.AntiAliased;
            lvDevices.Font = Font.FromLogFont(lf);*/
#endif
        }

        void Start()
        {
            _closed = false;
            // TO-DO Dispose the BtCli/BtCmpnt
            //
            Cursor.Current = Cursors.WaitCursor;
            lvDevices.Items.Clear();
            this.mnuAgain.Enabled = false;
            StartDiscovery();
        }

        private void SelectBluetoothDeviceForm_Resize(object sender, System.EventArgs e)
        {
#if NETCF
            //do gradient view
            SendMessage(lvDevices.Handle, LVM_SETEXTENDEDLISTVIEWSTYLE, LVS_EX_GRADIENT | LVS_EX_THEME | 0x01800000, LVS_EX_GRADIENT | LVS_EX_THEME | 0x01800000);

			//set layout dynamically
			this.Bounds = new Rectangle(0,0,Screen.PrimaryScreen.WorkingArea.Width,Screen.PrimaryScreen.WorkingArea.Height);

            if (!string.IsNullOrEmpty(info))
            {
                labelInfo.Text = info;
                labelInfo.Bounds = new Rectangle((int)((3 / 96f) * dpi), (int)((3 / 96f) * dpi), this.Width - (int)((6 / 96f) * dpi), (int)((20 / 96f) * dpi));
                labelInfo.Visible = true;

                //fill the panel
                lvDevices.Bounds = new Rectangle(-1, (int)((24 / 96f) * dpi), this.Width + 2, this.Height - (int)((24 / 96f) * dpi) + 1);
            }
            else
            {
                labelInfo.Visible = false;
                //fill the panel
                lvDevices.Bounds = new Rectangle(-1, -1, this.Width + 2, this.Height + 2);
            }
#else
            lvDevices.Bounds = new Rectangle(-1, -1, this.Width + 2, this.Height + 2);
#endif

            lvDevices.Columns[0].Width = this.Width - 2;
        }

        #region Discovery
        private void StartDiscovery()
        {
            BluetoothClient bc = new BluetoothClient();
            BluetoothDeviceInfo[] devicesR;
            if (fShowAuthenticated || fShowRemembered) {
                devicesR = bc.DiscoverDevices(255, fShowAuthenticated, fShowRemembered, false, fDiscoverableOnly);
            } else {
                devicesR = new BluetoothDeviceInfo[0];
            }
            UpdateProgressChanged(null, new DiscoverDevicesEventArgs(devicesR, null));
            //
            if (fShowUnknown || fDiscoverableOnly) {
                var bco = new BluetoothComponent(bc);
                bco.DiscoverDevicesAsync(255, false, false, true, fDiscoverableOnly, bco);
                bco.DiscoverDevicesProgress += bco_DiscoverDevicesProgress;
                bco.DiscoverDevicesComplete += bco_DiscoverDevicesComplete;
            } else {
                UpdateCompleted(null, new DiscoverDevicesEventArgs(new BluetoothDeviceInfo[0], null));
            }
        }

        void bco_DiscoverDevicesProgress(object sender, DiscoverDevicesEventArgs e)
        {
            Debug.Assert(e.Error == null && !e.Cancelled, "Never raised by BtCompnt!");
            var dlgt = new EventHandler<DiscoverDevicesEventArgs>(UpdateProgressChanged);
            lvDevices.BeginInvoke(dlgt, this, e);
        }

        void bco_DiscoverDevicesComplete(object sender, DiscoverDevicesEventArgs e)
        {
            var dlgt = new EventHandler<DiscoverDevicesEventArgs>(UpdateCompleted);
            lvDevices.BeginInvoke(dlgt, this, e);
            //
            var bco = (BluetoothComponent)e.UserState;
            bco.Dispose();
        }
        #endregion

        void UpdateProgressChanged(object sender, DiscoverDevicesEventArgs e)
        {
            if (_closed) {
                return;
            }
            BluetoothDeviceInfo[] newDevices = e.Devices;
            newDevices = DoFiltering(newDevices);
            AppendDevices(newDevices);
        }

        private BluetoothDeviceInfo[] DoFiltering(BluetoothDeviceInfo[] newDevices)
        {
            if (classOfDeviceFilter.Count > 0) {
                System.Collections.Generic.List<BluetoothDeviceInfo> filteredDevices = new System.Collections.Generic.List<BluetoothDeviceInfo>();
                //check the class of device and only return matching devices
                foreach (BluetoothDeviceInfo bdi in newDevices) {
                    foreach (uint codFilter in classOfDeviceFilter) {
                        if ((codFilter & 0x1f00) == codFilter) {
                            //major only
                            if ((bdi.ClassOfDevice.Value & 0x1f00) == codFilter) {
                                filteredDevices.Add(bdi);
                                break;
                            }
                        } else if (((bdi.ClassOfDevice.Value & 0x001ffc) & codFilter) == codFilter) {
                            filteredDevices.Add(bdi);
                            break;
                        }
                    }
                }//for
                newDevices = filteredDevices.ToArray();
            }
            if (_filterFn != null) {
                Predicate<BluetoothDeviceInfo> filterFn = _filterFn;
                var filteredDevices = new System.Collections.Generic.List<BluetoothDeviceInfo>();
                foreach (var bdi in newDevices) {
                    var keep = filterFn(bdi);
                    if (keep) {
                        filteredDevices.Add(bdi);
                    } else { //DBG
                    }
                }//for
                newDevices = filteredDevices.ToArray();
            } else { //DBG
            }
            return newDevices;
        }

        private void UpdateCompleted(object sender, DiscoverDevicesEventArgs e)
        {
            if (_closed) {
                return;
            }
            // See if the background worker method failed
            if (e.Error != null) {
#if DEBUG
                MessageBox.Show("e.Error is set, must have been an exception!");
#endif
                System.Diagnostics.Debug.Assert(e.Error as PlatformNotSupportedException != null,
                    "When devices=null, expect PlatformNotSupportedException.");
                System.Diagnostics.Debug.Assert(e.Error.InnerException as SocketException != null,
                    "When devices=null, expect innerException SocketException.");
                ListViewItem lvi = new ListViewItem("Failed: " + e.Error);
                lvi.ImageIndex = -1;
                lvDevices.Items.Add(lvi);
            } else {
                BluetoothDeviceInfo[] newDevices = e.Devices;
                if (newDevices == null) {
#if DEBUG
                    MessageBox.Show("devices is null, must have been an exception!");
#endif
                } else {
                    newDevices = DoFiltering(newDevices);
                    AppendDevices(newDevices);
                }
            }

            mnuAgain.Enabled = true;

            lvDevices.Focus();

            Cursor.Current = Cursors.Default;
        }



        void AppendDevices(BluetoothDeviceInfo[] newDevices)
        {
            if (newDevices != null) {
                foreach (BluetoothDeviceInfo bdi in newDevices) {
                    BdiListViewItem lvi = new BdiListViewItem(bdi);
                    lvi.ImageIndex = 0;
                    lvDevices.Items.Add(lvi);
                }
                //
                if (lvDevices.Items.Count > 0) {
                    mnuSelect.Enabled = true;
                }
            }
        }

        class BdiListViewItem : ListViewItem
        {
            readonly BluetoothDeviceInfo m_bdi;

            public BdiListViewItem(BluetoothDeviceInfo device)
                : base(device.DeviceName)
            {
#if NETCF
                ImageIndex = 0;
#endif
                m_bdi = device;
            }

            public BluetoothDeviceInfo Device { get { return m_bdi; } }
        }

        private void selectDevice(object sender, System.EventArgs e)
        {
            if (lvDevices.SelectedIndices.Count > 0 && lvDevices.SelectedIndices[0] > -1 && lvDevices.SelectedIndices[0] < lvDevices.Items.Count) {
                _closed = true;
                Cursor.Current = Cursors.Default;
                //
                ListViewItem item0 = lvDevices.Items[lvDevices.SelectedIndices[0]];
                BdiListViewItem item = item0 as BdiListViewItem;
                Debug.Assert(item != null, "non BdiListViewItem selected!!");
                if (item != null) {
                    selectedDevice = item.Device;

                    if (fForceAuthentication) {
                        //authenticate with the selected device
                    }

                    this.DialogResult = DialogResult.OK;
                }
            }

        }

        private void mnuAgain_Click(object sender, System.EventArgs e)
        {
            Start();
        }

        public override void Refresh()
        {
            lvDevices.Items.Clear();
            fShowAuthenticated = true;
            fShowUnknown = true;
            fForceAuthentication = false;
            base.Refresh();
        }

        #region ISelectBluetoothDevice Members
        void ISelectBluetoothDevice.Reset()
        {
            this.Refresh();
        }

        bool InTheHand.Windows.Forms.ISelectBluetoothDevice.ShowAuthenticated
        {
            get { return fShowAuthenticated; }
            set { fShowAuthenticated = value; }
        }

        bool InTheHand.Windows.Forms.ISelectBluetoothDevice.ShowRemembered
        {
            get { return fShowRemembered; }
            set { fShowRemembered = value; }
        }

        bool InTheHand.Windows.Forms.ISelectBluetoothDevice.ShowUnknown
        {
            get { return fShowUnknown; }
            set { fShowUnknown = value; }
        }

        bool InTheHand.Windows.Forms.ISelectBluetoothDevice.ForceAuthentication
        {
            get { return fForceAuthentication; }
            set { fForceAuthentication = value; }
        }

        bool InTheHand.Windows.Forms.ISelectBluetoothDevice.ShowDiscoverableOnly
        {
            get { return fDiscoverableOnly; }
            set { fDiscoverableOnly = value; }
        }

        string InTheHand.Windows.Forms.ISelectBluetoothDevice.Info
        {
            get { return info; }
            set { info = value; }
        }
#if WinXP


        bool InTheHand.Windows.Forms.ISelectBluetoothDevice.AddNewDeviceWizard
        {
            get { return false; }
            set { throw new NotImplementedException("The method or operation is not implemented."); }
        }

        bool InTheHand.Windows.Forms.ISelectBluetoothDevice.SkipServicesPage
        {
            get { return true; }
            set { throw new NotImplementedException("The method or operation is not implemented."); }
        }
#endif

        private System.Collections.Generic.List<uint> classOfDeviceFilter;
        public void SetClassOfDevices(ClassOfDevice[] classOfDevices)
        {
            classOfDeviceFilter.Clear();
            foreach (ClassOfDevice cod in classOfDevices) {
                classOfDeviceFilter.Add(cod.Value);
            }
        }

        public void SetFilter(Predicate<InTheHand.Net.Sockets.BluetoothDeviceInfo> filterFn,
            SelectBluetoothDeviceDialog.PFN_DEVICE_CALLBACK msftFilterFn)
        {
            _filterFn = filterFn;
        }
        #endregion
#endif
    }
}


