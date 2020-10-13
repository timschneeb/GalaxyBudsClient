using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Automation;
using Galaxy_Buds_Client.message;
using Galaxy_Buds_Client.model;
using Galaxy_Buds_Client.model.Constants;
using Galaxy_Buds_Client.util;
using InTheHand.Net;
using InTheHand.Net.Sockets;

namespace Galaxy_Buds_Client
{
    public sealed class BluetoothService
    {
        private static BluetoothService _instance = null;
        private static readonly object SingletonPadlock = new object();

        public static BluetoothService Instance
        {
            get
            {
                lock (SingletonPadlock)
                {
                    return _instance ?? (_instance = new BluetoothService());
                }
            }
        }

        private static readonly Guid ServiceUuidBuds = new Guid("{00001102-0000-1000-8000-00805f9b34fd}");
        private static readonly Guid ServiceUuidBudsPlus = new Guid("{00001101-0000-1000-8000-00805F9B34FB}");
        private static readonly Guid ServiceUuidBudsLive = new Guid("{00001101-0000-1000-8000-00805F9B34FB}");

        private Thread _btservice = null;

        private BluetoothEndPoint ep = null;
        private BluetoothClient cli = null;
        private readonly object _btpadlock = new object();

        private readonly Queue<SPPMessage> _transmitterQueue = new Queue<SPPMessage>();
        private readonly object _transmitpadlock = new object();

        public event EventHandler<byte[]> NewDataAvailable;
        public event EventHandler<SPPMessage> MessageReceived;
        public event EventHandler<SocketException> SocketException;
        public event EventHandler<InvalidDataException> InvalidDataException;
        public event EventHandler<PlatformNotSupportedException> PlatformNotSupportedException;

        public Model ActiveModel = Model.BudsPlus;
        public bool IsConnected => cli != null && cli.Connected;

        public BluetoothService()
        {
            _btservice = new Thread(BluetoothServiceLoop);
        }

        ~BluetoothService()
        {
            Disconnect();
        }

        public void CreateClient()
        {
            try
            {
                cli = new BluetoothClient();
            }
            catch (PlatformNotSupportedException e)
            {
                PlatformNotSupportedException?.Invoke(this, e);
            }
        }

        public void Connect(BluetoothAddress macAddress, Model deviceModel)
        {
            if (macAddress == null)
                return;

            lock (_btpadlock)
            {
                CreateClient();

                if (cli == null)
                    return;

                if (cli.Connected)
                    cli.Close();

                Guid serviceEntry = new Guid();
                switch (deviceModel)
                {
                    case Model.Buds:
                        serviceEntry = ServiceUuidBuds;
                        break;
                    case Model.BudsPlus:
                        serviceEntry = ServiceUuidBudsPlus;
                        break;
                    case Model.BudsLive:
                        serviceEntry = ServiceUuidBudsLive;
                        break;
                }

                ActiveModel = deviceModel;

                ep = new BluetoothEndPoint(macAddress, serviceEntry);

                try
                {
                    cli.Connect(ep);
                    if (_btservice == null)
                        _btservice = new Thread(BluetoothServiceLoop);
                    _btservice.Start();
                }
                catch (SocketException e)
                {
                    OnSocketException(e);
                }
            }
        }

        public void Disconnect()
        {
            _btservice?.Abort();
            _btservice = null;
            cli?.Close();
            cli = null;
        }

        public void SendAsync(SPPMessage msg)
        {
            lock (_transmitpadlock)
            {
                _transmitterQueue.Enqueue(msg);
            }
        }

        private void BluetoothServiceLoop()
        {
            Stream peerStream = null;

            while (true)
            {
                Thread.Sleep(50);

                if (cli == null || !cli.Connected)
                    continue;

                if (peerStream == null)
                {
                    lock (_btpadlock)
                    {
                        peerStream = cli.GetStream();
                    }
                }


                var available = cli.Available;


                if (available > 0 && peerStream.CanRead)
                {
                    byte[] buffer = new byte[available];
                    try
                    {
                        peerStream.Read(buffer, 0, available);
                    }
                    catch (SocketException e)
                    {
                        OnSocketException(e);
                    }


                    if (buffer.Length > 0)
                        OnNewDataAvailable(buffer);
                }

                lock (_transmitpadlock)
                {
                    if (peerStream.CanWrite && _transmitterQueue.Count > 0)
                    {
                        SPPMessage msg = _transmitterQueue.Dequeue();
                        byte[] raw = msg.EncodeMessage();
                        try
                        {
                            peerStream.Write(raw, 0, raw.Length);
                        }
                        catch (SocketException e)
                        {
                            OnSocketException(e);
                        }
                        catch (IOException e)
                        {
                            if (e.InnerException != null && e.InnerException.GetType() == typeof(SocketException))
                            {
                                OnSocketException(e.InnerException as SocketException);
                            }
                        }
                    }
                }
            }
        }

        private void OnNewDataAvailable(byte[] frame)
        {
            ArrayList data = new ArrayList(frame);
            do
            {
                try
                {
                    SPPMessage msg = SPPMessage.DecodeMessage(data.OfType<byte>().ToArray());

                    MessageReceived?.Invoke(this, msg);

                    if (msg.TotalPacketSize >= data.Count)
                        break;
                    data.RemoveRange(0, msg.TotalPacketSize);
                }
                catch (InvalidDataException e)
                {
                    InvalidDataException?.Invoke(this, e);
                }
            } while (data.Count > 0);

            NewDataAvailable?.Invoke(this, frame);
        }
        private void OnSocketException(SocketException e)
        {
            SocketException?.Invoke(this, e);
        }
    }
}
