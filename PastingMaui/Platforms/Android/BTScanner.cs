using System;
using System.Collections.ObjectModel;
using PastingMaui.Data;
using Android.Bluetooth;
using Android.Content;
using Android.App;
using Android.OS;
using Microsoft.Maui.Devices;
using AndroidX.Core.App;
using AndroidX.Core.Content;
using Android;
using Android.Content.PM;
using System.Runtime.InteropServices;
using Java.Util;
using Java.Nio;
using System.Runtime.CompilerServices;

namespace PastingMaui.Platforms
{

    public class BTScanner : Activity, IBTScan, IDisposable
    {

        enum WatcherState
        {
            IN_PROGRESS,
            STOPPED
        }
        
        public static int REQUEST_CODE = 677;
        BluetoothManager manager;
        BluetoothAdapter adapter;// share the bluetooth adapter
        Context appContext;
        WatcherState state;
        public Func<Task> RefreshBTDevices
        {
            get;
            set;
        }

        public ObservableCollection<IBTDevice> btDevicesCollection
        {
            get;
            set;
        }

        ObservableCollection<IBTDevice> btDevices;

        List<BTDevice> devices;

        public BTScanner(ObservableCollection<IBTDevice> devices, Context context)
        {
            manager = (BluetoothManager)context.GetSystemService(Context.BluetoothService);
            adapter = manager.Adapter;
            btDevicesCollection = devices;
            btDevices = devices;
            
        }

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            manager = (BluetoothManager)this.GetSystemService(Context.BluetoothService);
            adapter = manager.Adapter;
            //btDevicesCollection = Intent.GetParcelableExtra("devices");
            //btDevices = devices;
            devices = new List<BTDevice>();
        }

        protected override void OnStart() // this can either be in the app or here or the server
        {
            base.OnStart();
            // check if bluetooth is enabled to start
            if (adapter == null)
            {
                CreateWatcher();
            }

            if (!adapter.IsEnabled)
            {
                Intent enableBT = new Intent(BluetoothAdapter.ActionRequestEnable);
                StartActivityForResult(enableBT, 3);
            }

        }

        public void StartWatcher()
        {
            if (adapter == null)
            {
                CreateWatcher();
            }

            if (!adapter.IsEnabled)
            {
                Intent enableBT = new Intent(BluetoothAdapter.ActionRequestEnable);
                StartActivityForResult(enableBT, 3);
            }
        }

        private void CreateWatcher()
        {
            // sets up action when a device is found
            this.RegisterReceiver(new DiscoveryAction(devices, this), 
                new IntentFilter(BluetoothDevice.ActionFound));

            // sets up action when the discovery ends
            this.RegisterReceiver(new DiscoveryAction(devices, this),
                new IntentFilter(BluetoothAdapter.ActionDiscoveryFinished));

            ScanDevices();

            ICollection<BluetoothDevice> bondedDevices = adapter.BondedDevices;

            using (IEnumerator<BluetoothDevice> enumerator = bondedDevices.GetEnumerator())
            {
                do
                {
                    btDevicesCollection.Add(new BTDevice(enumerator.Current));
                } 
                while (enumerator.MoveNext());
            }

        }

        private class DiscoveryAction : BroadcastReceiver
        {

            List<BTDevice> devices;
            BTScanner scanner;

            public DiscoveryAction(List<BTDevice> btDevices, BTScanner activity)
            {
                devices = btDevices;
                scanner = activity;
            }

            public override void OnReceive(Context context, Intent intent)
            {
                string action = intent.Action;
                if (BluetoothDevice.ActionFound.Equals(action))
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
                        devices.Add(new BTDevice(device));
                    }

                }
                else if (BluetoothAdapter.ActionDiscoveryFinished.Equals(action))
                {
                    scanner.ScanFinished();
                }

            }
        }

        protected void ScanFinished() {
            Intent resultIntent = new Intent();
            // create 2 lists based on java, return those
            resultIntent.PutParcelableArrayListExtra("devices", devices.Cast<IParcelable>().ToList());
            //Bundle bundle = new Bundle();
            //bundle.PutParcelableArrayList("devices", (IList<IParcelable>)devices);
            //resultIntent.PutExtra("DEVICE_DATA", bundle);
            SetResult(Result.Ok, resultIntent);

        }

        public void ScanDevices()
        {
            // before discovering check if already discovering


            if (!adapter.IsDiscovering)
            {
                adapter.StartDiscovery();
            }
            else
            {
                // display toast here
            }



        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            StopScan();
            // maybe remove the collection of devices?
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            StopScan();
        }

        public void StopScan()
        {
            if (!adapter.IsDiscovering)
            {
                adapter.CancelDiscovery();
            }
        }

        public void RestartScan()
        {
            throw new NotImplementedException();
        }

        void IBTScan.ConnectToDevice(IBTDevice device)
        {
            Task.Run(async () => {
                await device.Connect(this);
            });
        }

        public bool isScanning()
        {
            throw new NotImplementedException();
        }
    }
}
