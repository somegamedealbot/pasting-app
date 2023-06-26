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
using PastingMaui.Data;

namespace PastingMaui.Platforms
{
    [Activity(Permission = Manifest.Permission.BluetoothConnect)]
    public class Client : Activity, IClient
    {
#if DEBUG
        string debugPrefix = "[Client]";
        #endif
        Context appContext;
        private static int BLUETOOTH_REQUEST_CODE = 786;

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

        //public Client(Context context)
        //{
        //    appContext = context;
        //    devices = new ObservableCollection<IBTDevice>();
        //    scannedDevices = new List<BTDevice>();
        //    //scanner = new BTScanner(devices, removed_devices, context);

        //}

        private BTScanner scanner;
        private bool connectedToServer;
        private ObservableCollection<IBTDevice> devices;
        private List<BTDevice> scannedDevices;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            devices = new ObservableCollection<IBTDevice>();
            //scanner = new BTScanner(devices, appContext);
            scannedDevices = new List<BTDevice>();
        }

        protected override void OnStart()
        {
            base.OnStart();
            CheckAndReqPermission();
        }

        protected override void OnResume()
        {
            base.OnResume();
            //CheckAndReqPermission();
        }

        protected override void OnStop()
        {
            base.OnStop();
        }

        protected override void OnDestroy()
        {
            scanner.Dispose();
        }

        public bool CheckBluetoothPermission()
        {
            return ContextCompat.CheckSelfPermission(this, Manifest.Permission.BluetoothConnect).Equals(Permission.Granted);
        }

        public void CheckAndReqPermission()
        {
            if (!CheckBluetoothPermission())
            {
                var perms = new string[] {
                    Manifest.Permission.Bluetooth,
                    Manifest.Permission.BluetoothAdmin
                };
                ActivityCompat.RequestPermissions(this, perms, BLUETOOTH_REQUEST_CODE);
            }
            else
            {
                //scanner.ScanDevices();
                Intent intent = new Intent(this, Java.Lang.Class.FromType(typeof(BTScanner)));
                StartActivityForResult(intent, BTScanner.REQUEST_CODE);
            }
        }

        public override void OnRequestPermissionsResult(int code, string[] permissions, Permission[] grantResults)
        {
            base.OnRequestPermissionsResult(code, permissions, grantResults);
            if (code == BLUETOOTH_REQUEST_CODE)
            {
                if (grantResults.Length != 0 && grantResults[0].Equals(Permission.Granted))
                {
#if DEBUG
                    Console.WriteLine("Bluetooth Permission Granted");
#endif
                    // start the scanning
                    Intent intent = new Intent(this, Java.Lang.Class.FromType(typeof(BTScanner)));
                    StartActivityForResult(intent, BTScanner.REQUEST_CODE);
                }
            }
        }

        protected override void OnActivityResult(int requestCode, [GeneratedEnum] Result resultCode, Intent data)
        {
            base.OnActivityResult(requestCode, resultCode, data);
            if (requestCode == BTScanner.REQUEST_CODE)
            {

                // handle result here get device data
                Bundle bundle = data.GetBundleExtra("DEVICE_DATA");
                int version = DeviceInfo.Current.Version.Major;

                if (version > 33)
                {
                    scannedDevices = data.GetParcelableArrayListExtra("devices", Java.Lang.Class.FromType(typeof(BTDevice))).Cast<BTDevice>().ToList();
                }
                else
                {
                    scannedDevices = data.GetParcelableArrayListExtra("devices").Cast<BTDevice>().ToList();
                }

            }

        }

        private class ScanComplete : BroadcastReceiver
        {

            public ScanComplete()
            {

            }

            public override void OnReceive(Context context, Intent intent)
            {
                throw new NotImplementedException();
            }
        }


    }
}
