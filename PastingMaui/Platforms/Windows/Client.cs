using PastingMaui.Data;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Threading.Tasks;

namespace PastingMaui.Platforms
{
    internal class Client : IClient
    {

        private BTScanner scanner;
        private bool connectedToServer;
        private ObservableCollection<IBTDevice> devices;
        private List<IBTDevice> removed_devices;

        IBTScan IClient.scanner { 
            get { return scanner; }
        }

        public BTScanner deviceScanner
        {
            get { return scanner; }
        }

        public ObservableCollection<IBTDevice> discovered_devices
        {
            get { return devices; } 
        }

        NotifyCollectionChangedEventHandler handler;

        public Func<Task> RefreshBTDevices
        {
            set
            {
                if (handler != null)
                {
                    discovered_devices.CollectionChanged -= handler;
                }
                handler = async (sender, e) => {
                    await value();
                };
                discovered_devices.CollectionChanged += handler;
            }
        }
        public SemaphoreSlim deviceListSemaphore
        {
            get; private set;
        }

        IBTDevice IClient.ConnectedDevice
        {
            get
            {
                return ConnectedDevice;
            }
        }

        bool isScanning;
        public bool IsScanning { get { return isScanning; } }

        public IBTDevice ConnectedDevice;

        public async Task ActionOnDevices(Func<Task> task)
        {
            deviceListSemaphore.Wait();
            await task.Invoke(); // does whatever task that the list
            deviceListSemaphore.Release();
        }
        public Client() {
            
            devices = new ObservableCollection<IBTDevice>();
            removed_devices = new List<IBTDevice>();
            deviceListSemaphore = new SemaphoreSlim(1, 1);
            
        }

        public void ScanDevices()
        {
            scanner ??= new BTScanner(devices, this);
            if (!scanner.isScanning())
            {
                discovered_devices.Clear();
                scanner.ScanDevices();
                isScanning = true;
            }
        }

        public async Task AddDevice(BTDevice device)
        {
            deviceListSemaphore.Wait();
            devices.Add(device); // does whatever task that the list
            deviceListSemaphore.Release();
        }

        public void StopScanning()
        {
            scanner.StopScan();
            isScanning = false;
        }

        public void SetConnectedDevice(IBTDevice device)
        {
            ConnectedDevice = device;
        }

        public void RemoveConnectedDevice()
        {
            //ConnectedDevice.Dispose();
            ConnectedDevice = null;
        }
    }
}
