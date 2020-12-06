using System;
using GalaxyBudsClient.Message;

namespace GalaxyBudsClient
{
    /** STUB **/
    public class BluetoothService
    {
        private static BluetoothService? _instance;
        private static readonly object SingletonPadlock = new object();

        public static BluetoothService Instance
        {
            get
            {
                lock (SingletonPadlock)
                {
                    return _instance ??= new BluetoothService();
                }
            }
        }

        public Model.Constants.Models ActiveModel = Model.Constants.Models.BudsPlus;
        
        public event EventHandler<byte[]>? NewDataAvailable;
        public event EventHandler<SPPMessage>? MessageReceived;
        
        public void SendAsync(SPPMessage msg){}
    }
}