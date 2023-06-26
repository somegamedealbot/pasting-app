using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PastingMaui.Data
{
    public interface IBTScan
    {
        public Func<Task> RefreshBTDevices
        {
            get;
            set;
        }
        ObservableCollection<IBTDevice> btDevicesCollection
        {
            get;
            set;
        }
        

        public void ScanDevices();

        public void StopScan();

        public void RestartScan();

        public void ConnectToDevice(IBTDevice device);

        public bool isScanning();

    }
}
