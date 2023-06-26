using PastingMaui.Data;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PastingMaui.Platforms
{
    internal class Client : IClient
    {
        public Client() {
            
            devices = new ObservableCollection<IBTDevice>();
            removed_devices = new List<IBTDevice>();
            scanner = new BTScanner(devices, removed_devices);

        }

        private BTScanner scanner;
        private bool connectedToServer;
        private ObservableCollection<IBTDevice> devices;
        private List<IBTDevice> removed_devices;

        IBTScan IClient.scanner { 
            get { return scanner; }
        }

        public ObservableCollection<IBTDevice> discovered_devices
        {
            get { return devices; } 
        }

        public List<IBTDevice> devices_removed
        {
            get { return removed_devices; }
        }
    }
}
