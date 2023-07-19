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
using Android.Runtime;
using static PastingMaui.Platforms.Client;
using Android.Widget;

namespace PastingMaui.Platforms
{
    [Service(Exported = false)]
    [IntentFilter(actions: new string[]{"SCAN_DEVICE", "ENABLE_BLUETOOTH_RESULT"})]
    public class BTScanner : Service, IBTScan, IDisposable
    {
        public static readonly string RescanDevicesAction = "RESCAN_DEVICES";
        public static readonly string SetupReceiversAction = "SETUP_RECEIVERS";
        public static readonly string ScanDevicesAction = "SCAN_DEVICES";
        public static readonly string ScanFinishedAction = "SCAN_FINISHED";
        public static readonly int ENABLE_BLUETOOTH_REQ_CODE = 567;
        public static readonly string EnableBluetoothResult = "ENABLE_BLUETOOTH_RESULT";
        public static readonly string BondedDevicesAction = "BONDED_DEVICES";
        public static readonly string StopScanAction = "STOP_SCAN";

        public static readonly string BondedDevicesKey = "bonded devices";

        public enum WatcherState
        {
            NOT_CREATED,
            IDLE,
            SCANNING,
            STOPPED
        }
        
        public static int REQUEST_CODE = 677;
        BluetoothManager manager;
        BluetoothAdapter adapter;// share the bluetooth adapter
        public WatcherState state;

        bool scan_success;
        public Func<Task> RefreshBTDevices
        {
            get;
            set;
        }

        public ObservableCollection<IBTDevice> btDevicesCollection
        {
            get
            {
                return PastingApp.getApp()?.appClient.discovered_devices;
            }
        }

        ObservableCollection<IBTDevice> IBTScan.btDevicesCollection { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public override void OnCreate()
        {
            base.OnCreate();
            // one-time creation put here
            manager = (BluetoothManager)MauiApplication.Context.GetSystemService(Context.BluetoothService);
            adapter = manager.Adapter;
            scan_success = false;
            state = WatcherState.NOT_CREATED;

            SetupReceivers();
            state = WatcherState.IDLE;
        }

        private void SetupReceivers()
        {
            // sets up action when a device is found
            this.RegisterReceiver(new DiscoveryAction(),
                new IntentFilter(BluetoothDevice.ActionFound));

            // sets up action when the discovery ends
            this.RegisterReceiver(new DiscoveryFinished(this),
                new IntentFilter(BluetoothAdapter.ActionDiscoveryFinished));
        }

        private void AdapterOnCheck()
        {
            // checks if adapter is on
            if (!adapter.IsEnabled)
            {

                // if needed
                Intent enableBT = new Intent(BluetoothAdapter.ActionRequestEnable);
                // task user to enable bluetooth
                MainActivity.GetMainActivity()?.StartActivityForResult(enableBT, ENABLE_BLUETOOTH_REQ_CODE);

                // return and wait for result passed from MainActivity
                return;

            }

        }

        public void GetBondedDevices() {
            List<IParcelable> btPairedDevices = new List<IParcelable>();

            ICollection<BluetoothDevice> bondedDevices = adapter.BondedDevices;
            ScanMode mode = adapter.ScanMode;

            using (IEnumerator<BluetoothDevice> enumerator = bondedDevices.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    btPairedDevices.Add(new BTDevice(enumerator.Current));
                }
            }

            Intent intent = new Intent();
            intent.PutParcelableArrayListExtra(BondedDevicesKey, btPairedDevices);
            intent.SetAction(BondedDevicesAction);
            SendBroadcast(intent);
        }

        private class DiscoveryFinished : BroadcastReceiver
        {

            BTScanner scanner;
            public DiscoveryFinished(BTScanner btScanner)
            {
                scanner = btScanner;
            }

            public override void OnReceive(Context context, Intent intent)
            {
                string action = intent.Action;
                if (BluetoothAdapter.ActionDiscoveryFinished.Equals(action))
                {
                    scanner.ScanFinished();
                }

            }
        }

        protected void ScanFinished() {

            // Once scan is finished, broadcast the data back and kill the thread
            Intent resultIntent = new Intent();
            resultIntent.SetAction(ScanFinishedAction);
            SendBroadcast(resultIntent);
            state = WatcherState.IDLE;

        }

        [return: GeneratedEnum]
        public override StartCommandResult OnStartCommand(Intent intent, [GeneratedEnum] StartCommandFlags flags, int startId)
        {
            StartCommandResult result = base.OnStartCommand(intent, flags, startId);

            if (intent.Action == ScanDevicesAction)
            {
                if (state == WatcherState.NOT_CREATED)
                {
                    AdapterOnCheck();
                }
                ScanDevices();

            }
            else if (intent.Action == StopScanAction)
            {

                StopScan();
                // unpack the receivers
                // set them up
            }
            else if (intent.Action == BluetoothAdapter.ActionRequestEnable)
            {
                bool enabledBT = intent.GetBooleanExtra("enabled", false);
                if (enabledBT)
                {
                    ScanDevices();
                }
            }

            return result;
        }

        public void ScanDevices()
        {
            // before discovering check if already discovering
            Toast toast;
            if (!adapter.IsDiscovering)
            {
                GetBondedDevices();
                state = WatcherState.SCANNING;
                scan_success = adapter.StartDiscovery();
                toast = Toast.MakeText(MainActivity.GetMainActivity(), "Scanning for devices", ToastLength.Short);
                //ThreadStart start = new ThreadStart(() =>
                //{
                //    scanSuccess = adapter.StartDiscovery();
                //    return;
                //});
                //Thread thread = new Thread(ScanFinished);
            }
            else
            {
                // Display toast saying already scanning
                toast = Toast.MakeText(MainActivity.GetMainActivity(), "Already currently scanning for devices", ToastLength.Short);

            }
            toast.Show();

        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            StopScan();
        }

        public void StopScan()
        {
            if (adapter.IsDiscovering)
            {
                adapter.CancelDiscovery();
                state = WatcherState.IDLE;
            }
        }

        public void RestartScan()
        {
            throw new NotImplementedException();
        }

        void IBTScan.ConnectToDevice(IBTDevice device)
        {
            Task.Run(async () => {
                await device.Connect(PastingApp.getApp().appClient);
            });
        }

        public bool isScanning()
        {
            throw new NotImplementedException();
        }

        public override IBinder OnBind(Intent intent)
        {
            return null;
        }
    }
}
