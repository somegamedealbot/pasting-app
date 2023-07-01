using System;
using System.Collections.ObjectModel;
using Android;
using Android.App;
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

            ScanDevices();
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

            BroadcastReceiver receiver = new ScanComplete(RecieveDevicesData);
            Activity main = MainActivity.GetMainActivity();
            var recieverFlags = ContextCompat.ReceiverNotExported;
            IntentFilter filter = new IntentFilter(BTScanner.ScanDevicesAction);
            ContextCompat.RegisterReceiver(main, receiver, filter, recieverFlags);

            //filter.AddAction($"{MauiApplication.Context.ApplicationInfo.PackageName}.{BTScanner.ScanDevicesAction}");

            Intent intent = new Intent(main, typeof(BTScanner));
            intent.SetAction("SCAN_DEVICES");
            ComponentName name = main.StartService(intent);
        }

        public void RecieveDevicesData(List<BTDevice> devices)
        {
            Console.WriteLine("Data acquired");
            // on receieving device data
        }

        private BTScanner scanner;
        private bool connectedToServer;
        private ObservableCollection<IBTDevice> devices;
        private List<BTDevice> scannedDevices;

        [BroadcastReceiver(Exported = false)]
        protected class ScanComplete : BroadcastReceiver
        {

            private Action<List<BTDevice>> callback;

            public ScanComplete()
            {
            }

            public ScanComplete(Action<List<BTDevice>> method)
            {
                callback = method;
            }
            public override void OnReceive(Context context, Intent intent)
            {
                string action = intent.Action.ToString();
                
                if (action.Equals("scan_for_devices"))
                {
                    int version = DeviceInfo.Current.Version.Major;
                    List<BTDevice> found_devices = new List<BTDevice>();

                    if (version >= 33)
                    {
                        found_devices = intent.GetParcelableArrayListExtra("devices", Java.Lang.Class.FromType(typeof(BTDevice))).Cast<BTDevice>().ToList();
                    }
                    else
                    {
                        found_devices = intent.GetParcelableArrayListExtra("devices").Cast<BTDevice>().ToList();
                    }

                    callback(found_devices);

                }

            }
        }

    }
}
