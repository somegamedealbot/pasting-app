using System;
using System.Collections.ObjectModel;
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
            get;
            set;
        }

        IBTScan IClient.scanner
        {
            get { return scanner; }
        }

        public ObservableCollection<IBTDevice> discovered_devices
        {
            get { return devices; }
        }

        public Client()
        {
            devices = new ObservableCollection<IBTDevice>();
            scannedDevices = new List<BTDevice>();
            mainActivity = Microsoft.Maui.ApplicationModel.Platform.CurrentActivity;
            bluetoothStatus = PermissionStatus.Unknown;
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

            BroadcastReceiver receiver = new ScanExtras(RecieveBondedDevicesData);
            Activity main = MainActivity.GetMainActivity();
            var recieverFlags = ContextCompat.ReceiverNotExported;
            IntentFilter filter = new IntentFilter(BTScanner.BondedDevicesAction);
            filter.AddAction(BTScanner.ScanFinishedAction);
            ContextCompat.RegisterReceiver(main, receiver, filter, recieverFlags);

            //filter.AddAction($"{MauiApplication.Context.ApplicationInfo.PackageName}.{BTScanner.ScanDevicesAction}");

            Intent intent = new Intent(main, typeof(BTScanner));
            intent.SetAction(BTScanner.ScanDevicesAction);
            ComponentName name = main.StartService(intent);
        }

        public void RecieveBondedDevicesData(List<BTDevice> devices)
        {
            Console.WriteLine("Data acquired");
            devices.ForEach(d => discovered_devices.Add(d)); // should have an event that updates the UI 
            // on receieving device data
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
                        client.devices.Add(new BTDevice(device));
                    }

                }

            }
        }

    }
}
