using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using Android;
using Android.App;
using Android.Bluetooth;
using Android.Content;
using Android.Content.PM;
using Android.Media;
using Android.OS;
using Android.Runtime;
using AndroidX.Core.App;
using AndroidX.Core.Content;
using Java.Lang;
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using PastingMaui.Data;
using PastingMaui.Platforms.Android.CustomPerms;

namespace PastingMaui.Platforms
{
    public class Client : IClient
    {
#if DEBUG
        string debugPrefix = "[Client]";
        #endif
        private static int BLUETOOTH_REQUEST_CODE = 786;

        Activity mainActivity;

        PermissionStatus bluetoothStatus;

        public Func<Task> RefreshBTDevices
        {
            set
            {
                if (handler != null) {
                    discovered_devices.CollectionChanged -= handler;
                }
                handler = async (sender, e) => {
                    await value();
                };
                discovered_devices.CollectionChanged += handler;
            }
        }

        NotifyCollectionChangedEventHandler handler;

        public IBTDevice ConnectedDevice; 

        IBTScan IClient.scanner
        {
            get { return scanner; }
        }

        public ObservableCollection<IBTDevice> discovered_devices
        {
            get { return devices; }
        }

        public SemaphoreSlim deviceListSemaphore
        {
            get; private set;
        }
        IBTDevice IClient.ConnectedDevice { get { return ConnectedDevice; } }

        static bool isScanning;
        bool IClient.IsScanning { get { return isScanning; }}


        public void SetConnectedDevice(IBTDevice device)
        {
            ConnectedDevice = device;
        }

        public async Task ActionOnDevices(Func<Task> task)
        {
            await deviceListSemaphore.WaitAsync();
            await task.Invoke(); // does whatever task that the list
            deviceListSemaphore.Release();
        }

        public void SetupReceivers()
        {
            BroadcastReceiver receiver = new ScanExtras(RecieveBondedDevicesData);
            var recieverFlags = ContextCompat.ReceiverNotExported;
            IntentFilter filter = new (BTScanner.BondedDevicesAction);
            filter.AddAction(BTScanner.ScanFinishedAction);
            ContextCompat.RegisterReceiver(MainActivity.GetMainActivity(), receiver, filter, recieverFlags);
        }

        public Client()
        {
            devices = new ObservableCollection<IBTDevice>();
            scannedDevices = new List<BTDevice>();
            mainActivity = Microsoft.Maui.ApplicationModel.Platform.CurrentActivity;
            bluetoothStatus = PermissionStatus.Unknown;
            deviceListSemaphore = new SemaphoreSlim(1, 1);

            SetupReceivers();
        }

        public static void StopScanning()
        {
            Activity main = MainActivity.GetMainActivity();
            Intent intent = new Intent(main, typeof(BTScanner));
            intent.SetAction(BTScanner.StopScanAction);
            ComponentName name = main.StartService(intent);
            isScanning = false;
        }

        public async void ScanDevices() {

            // ask for permission here first

            //if (DeviceInfo.Version.Major < 33)

            bluetoothStatus = await Permissions.CheckStatusAsync<BluetoothPerms>();

            if (bluetoothStatus != PermissionStatus.Granted) { 
                
                if (Permissions.ShouldShowRationale<BluetoothPerms>())
                {
                    // display prompt here
                }
                
                await Permissions.RequestAsync<BluetoothPerms>();
            }

            //else
            //{
            //    return;
            //}
            Activity main = MainActivity.GetMainActivity();

            //filter.AddAction($"{MauiApplication.Context.ApplicationInfo.PackageName}.{BTScanner.ScanDevicesAction}");
            Intent intent = new Intent(main, typeof(BTScanner));
            intent.SetAction(BTScanner.ScanDevicesAction);


            discovered_devices.Clear(); // clears all devices after refresh
            
            
            ComponentName name = main.StartService(intent);
            isScanning = true;
            
            
        }

        public void RecieveBondedDevicesData(List<BTDevice> devices)
        {
            Console.WriteLine("Data acquired");
            deviceListSemaphore.Wait();
            devices.ForEach(d => discovered_devices.Add(d)); // should have an event that updates the UI
            deviceListSemaphore.Release();
            // on receieving device data
        }

        public void RemoveConnectedDevice()
        {
            ConnectedDevice = ConnectedDevice == null ? throw new System.Exception("Cannot remove null Connected Device") : null;
        }

        private BTScanner scanner;
        private bool connectedToServer;
        private ObservableCollection<IBTDevice> devices;
        private List<BTDevice> scannedDevices;

        protected class ScanExtras : BroadcastReceiver
        {

            private Action<List<BTDevice>> callback;

            public ScanExtras()
            {
            }

            public ScanExtras(Action<List<BTDevice>> method)
            {
                callback = method;
            }
            public override void OnReceive(Context context, Intent intent)
            {
                string action = intent.Action.ToString();
                
                if (action.Equals(BTScanner.BondedDevicesAction))
                {
                    int version = DeviceInfo.Current.Version.Major;
                    List<BTDevice> found_devices = new List<BTDevice>();

                    if (version >= 33)
                    {
                        found_devices = intent.GetParcelableArrayListExtra(BTScanner.BondedDevicesKey,
                           Java.Lang.Class.FromType(typeof(BTDevice))).Cast<BTDevice>().ToList();
                    }
                    else
                    {
                        var test = intent.GetParcelableArrayListExtra(BTScanner.BondedDevicesKey);

                        found_devices = intent.GetParcelableArrayListExtra(BTScanner.BondedDevicesKey)
                            .Cast<BTDevice>().ToList();
                    }

                    callback(found_devices);

                }
                else if (action.Equals(BTScanner.ScanFinishedAction))
                {
                    isScanning = false;
                    Console.WriteLine("BTScan finished");
                }

            }
        }

        public class DiscoveryAction : BroadcastReceiver
        {
            private Client client;
            public DiscoveryAction()
            {
                client = PastingApp.getApp()?.appClient;
            }

            public override void OnReceive(Context context, Intent intent)
            {
                string action = intent.Action;
                if (BluetoothDevice.ActionFound.Equals(BluetoothDevice.ActionFound))
                {
                    int version = DeviceInfo.Current.Version.Major;
                    BluetoothDevice device;
                    if (version >= 33)
                    {
                        device = (BluetoothDevice)intent.GetParcelableExtra(action, Class);
                    }
                    else
                    {
                        device = (BluetoothDevice)intent.GetParcelableExtra(action);
                    }

                    if (device != null && device.BondState != Bond.Bonded)
                    {
                        client.deviceListSemaphore.Wait();
                        client.devices.Add(new BTDevice(device));
                        client.deviceListSemaphore.Release();
                    }

                }

            }
        }

    }
}
