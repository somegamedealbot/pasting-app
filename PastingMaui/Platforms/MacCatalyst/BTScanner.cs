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
        BTScanner()
        {
            btDevicesCollection = MauiProgram.BTDevices;
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

        public bool isScanning()
        {
            throw new NotImplementedException();
        }
    }
}
