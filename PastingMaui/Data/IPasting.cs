using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PastingMaui.Data
{
    public interface IPasting
    {

        public IServer server
        {
            get;
        }

        public event EventHandler OnUIChangeOnConnect;

        public event EventHandler OnUIChangeOnDisconnect;

        public event EventHandler UIChangeOnConnect
        {
            add { OnUIChangeOnConnect += value; }
            remove { OnUIChangeOnConnect -= value; }
        }

        public event EventHandler UIChangeOnDisconnect { 
            add { OnUIChangeOnDisconnect += value; }
            remove { OnUIChangeOnDisconnect -= value; }
        }

        public void SetUIConnectHandler(EventHandler handler)
        {
            UIChangeOnConnect += handler;
        }
        public void SetUIDisconnectHandler(EventHandler handler)
        {
            UIChangeOnDisconnect += handler;
        }

        public IClient client
        {
            get;
        }

        public bool ConnectedToDevice { get; }

        public IBTDevice ConnectedDevice { get; }

        public void StartServer();

        public void StartClient();

        public void StartScanningDevices();

        public void DisconnectDevice();

    }
}
