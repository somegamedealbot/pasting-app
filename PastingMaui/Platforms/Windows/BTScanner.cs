using Windows.Devices.Enumeration;
using Windows.Foundation;
using System.Collections.ObjectModel;
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

        public DeviceWatcher BTDeviceWatcher
        {
            get; set;
        }
        public ObservableCollection<IBTDevice> btDevicesCollection
        {
            get;
            set;
        }

        Client client;

        // check for permissions and setup before scanning
        public BTScanner(ObservableCollection<IBTDevice> btDevices, Client appClient)
        {

            btDevicesCollection = btDevices;
            client = appClient;
            CreateWatcher();
        }

        private void CreateWatcher()
        {
            // watcher looks for bluetooth devices that are not paired yet
            string[] requestedProperties = { "System.Devices.Aep.DeviceAddress", "System.Devices.Aep.IsConnected", "System.Devices.Aep.CanPair", "System.Devices.Aep.IsPresent" };

            //string[] requestedProperties = { "System.Devices.Aep.DeviceAddress", "System.Devices.Aep.IsConnected, System.Devices.Aep.CanPair, System.Devices.Aep.IsPaired, System.Devices.Aep.IsPresent, System.Devices.Aep.IsConnected" };
            //bb7bb05e-5972-42b5-94fc-76eaa7084d49
            //var watcher = DeviceInformation.CreateWatcher("System.Devices.Aep.ProtocolId:=\"{e0cbf06c-cd8b-4647-bb8a-263b43f0f974}\" AND " +
            //    "System.Devices.AepService.ServiceClassId:=\"{00000003-0000-1000-8000-00805F9B34FB}\"", requestedProperties, DeviceInformationKind.AssociationEndpoint);

            var watcher = DeviceInformation.CreateWatcher("(System.Devices.Aep.ProtocolId:=\"{e0cbf06c-cd8b-4647-bb8a-263b43f0f974}\")",
                requestedProperties, DeviceInformationKind.AssociationEndpoint);

            BTDeviceWatcher = watcher;

          
            // rfcomm service ID: 00000003-0000-1000-8000-00805F9B34FB
            
            // handler when a new device is discovered
            // handler will run async if no await
            watcher.Added += new TypedEventHandler<DeviceWatcher, DeviceInformation>(async (watcher, deviceInfo) =>
            {
                // Do things here when a new device is 
                //if (deviceInfo.Properties.TryGetValue("System.Devices.Aep.IsPresent", out dynamic Boolean))
                //{
                //    if (Boolean == true)
                //    {
                await client.AddDevice(new BTDevice(deviceInfo));
                    //}
                //}
                //await RefreshBTDevices();
                //await RefreshBTDevices();

            });

            watcher.Removed += new TypedEventHandler<DeviceWatcher, DeviceInformationUpdate>(async (watcher, update) =>
            {
                // Do things when a device is removed from discovery

                //if (update.Properties.TryGetValue("System.Devices.Aep.IsPresent", out dynamic Boolean))
                //{
                //    if (Boolean == true)
                //    {
                await client.ActionOnDevices(() =>
                {
                    bool DeviceRemoved = false;
                    int index = 0;
                    while (!DeviceRemoved && index < btDevicesCollection.Count)
                    {

                        var device = btDevicesCollection[index];
                        if (device.Id.Equals(update.Id))
                        {
                            btDevicesCollection.Remove(device);
                            DeviceRemoved = !DeviceRemoved;
                        }
                        index++;

                    }
                    return Task.CompletedTask;
                });
                //    }
                //}

                //await RefreshBTDevices();

            });

            watcher.Updated += new TypedEventHandler<DeviceWatcher, DeviceInformationUpdate>(async (watcher, update) =>
            {
                // Update the specific device that updated with newly updated information

                //if (update.Properties.TryGetValue("System.Devices.Aep.IsPresent", out dynamic Boolean))
                //{
                //    if (Boolean == true)
                //    {
                await client.ActionOnDevices(() =>
                {
                    var tempEnumerableDevices = btDevicesCollection.Cast<BTDevice>();
                    // cast should not create new objects but objects should remain the same in original colleciton

                    foreach (BTDevice device in tempEnumerableDevices)
                    {
                        if (device.Id == update.Id)
                        {
                            device.DeviceInfo = update;
                            //device.UpdateDeviceInformation(update);
                            break;
                        }
                    }
                    //await RefreshBTDevices();
                    return Task.CompletedTask;
                });
                //    }
                //}

            });

            watcher.EnumerationCompleted += new TypedEventHandler<DeviceWatcher, object>(async (watcher, obj) =>
            {
                // when enumeration is done
                //await RefreshBTDevices();
            });

            watcher.Stopped += new TypedEventHandler<DeviceWatcher, object>(async (watcher, obj) =>
            {
                // when enumeration is stopped
                //btDevicesCollection.Clear();
                //await RefreshBTDevices();

            });
        }


        public void StopScan()
        {
            if (BTDeviceWatcher != null)
            {
                if (BTDeviceWatcher.Status == DeviceWatcherStatus.Started || 
                    BTDeviceWatcher.Status == DeviceWatcherStatus.EnumerationCompleted)
                {
                    BTDeviceWatcher.Stop();
                }
            }
        }

        public void RestartScan()
            // refresh button
        {
            if (BTDeviceWatcher != null)
            {
                StopScan();
                ScanDevices();
            }
        }

        public void ScanDevices()
        {
            if (isScanning())
            {
                return;
            }

            if (BTDeviceWatcher != null)
            {
                if (BTDeviceWatcher.Status != DeviceWatcherStatus.EnumerationCompleted && 
                    BTDeviceWatcher.Status != DeviceWatcherStatus.Started)
                {
                    BTDeviceWatcher.Start();
                }

            }
        }

        public void ConnectToDevice(IBTDevice device)
        {
            device.Connect(PastingApp.app.client);
        }

        public bool isScanning()
        {
            return BTDeviceWatcher.Status.Equals(DeviceWatcherStatus.Started);
        }

    }
}
