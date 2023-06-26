using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using Microsoft.Maui.Devices;
using PastingMaui.Data;

namespace PastingMaui.Platforms
{
    internal class BTScanner : IBTScan
    {
        public Func<Task> RefreshBTDevices
        {
            get;
            set;
        }


        // check for permissions and setup before scanning
        BTScanner(ObservableCollection<IBTDevice> btDevices, List<IBTDevice> removedDevices)
        {
            btDevicesCollection = btDevices;
        }

        public ObservableCollection<IBTDevice> btDevicesCollection { 
            get => throw new NotImplementedException(); set => throw new NotImplementedException(); 
        }

        public void ScanDevices()
        {
            throw new NotImplementedException();
        }

        public void StopScan()
        {
            throw new NotImplementedException();
        }

        public void RestartScan()
        {
            throw new NotImplementedException();
        }

        public void ConnectToDevice(IBTDevice device)
        {
            throw new NotImplementedException();
        }
    }
}
