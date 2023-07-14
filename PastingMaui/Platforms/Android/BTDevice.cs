using Android.App;
using Android.Bluetooth;
using Android.Content;
using Android.OS;
using Android.Widget;
using Java.Interop;
using Java.Util;
using PastingMaui.Data;
using PastingMaui.Platforms.Android;
using PastingMaui.Shared;
using AndroidToast = Android.Widget.Toast;

namespace PastingMaui.Platforms
{
    public class BTDevice : Java.Lang.Object, IParcelable, IBTDevice
    {
        public BluetoothDevice device
        {
            get; private set;
        }

        public string Id
        {
            get
            {
                return device.Alias;
            }
        }

        public string Name
        {
            get
            {
                return device.Name;
            }
        }

        public string Type
        {
            get
            {
                return device.Address.ToString();
            }
        }

        private BluetoothSocket socket;
        bool secure;
        private bool disposedValue;

        public BTDevice(BluetoothDevice btDevice) {
            device = btDevice;
            secure = true;
        }

        [ExportField("CREATOR")]
        public static IParcelableCreator InitCreator()
        {
            return new BTDeviceCreator();
        }

        public BTDevice(Parcel parcel)
        {
            // read BluetoothDevice
            device = parcel.ReadTypedObject(BluetoothDevice.Creator).JavaCast<BluetoothDevice>();
            secure = true; // turn back to secure
        }

        public void RunOnUIThread(Action action)
        {
            MainActivity.GetMainActivity().RunOnUiThread(action);
        }

        // connect to a device (server)
        public async Task<ToastData> Connect(IClient client)
        {
            UUID convertedUuid = UUID.FromString(ServiceConfig.serviceUuidString);
            
            try
            {
                if (secure)
                {
                    socket = device.CreateRfcommSocketToServiceRecord(convertedUuid);
                }
                else
                {
                    socket = device.CreateInsecureRfcommSocketToServiceRecord(convertedUuid);
                }
            }
            catch (Exception e)
            {
                socket.Dispose();
                socket = null;
                RunOnUIThread(() => PastingApp.ToastMaker.MakeAndShowToast("Failed to make Server: something went wrong when creating a listening socket"));
                //toast = AndroidToast.MakeText(MainActivity.GetMainActivity(), "Failed to connect: something went wrong when connecitng to sockets", ToastLength.Short);
                //toast.Show();
                return new ToastData("Failed to connect", "Something went wrong when connecting socket", ToastType.Alert);
            }

            // send signal to cancel Discovery
            PastingApp.app.StopServicesOnConnect();

            try
            {
                socket.Connect();
            }
            catch(Exception e)
            {
                socket.Dispose();
                socket = null;
                return new ToastData("Failed to connect", $"Make sure that {device.Name} is currently running the app", ToastType.Alert);
            }
            // have input and outputs streams
            if (socket.IsConnected)
            {
                client.SetConnectedDevice(this);
                RunOnUIThread(() => 
                    PastingApp.app.SetConnectedDevice(this, socket));
            }
            else
            {
                PastingApp.app.StartServices();
                return new ToastData("Failed to connect", $"Make sure that {device.Name} is currently running the app", ToastType.Alert);
                // restart services
            }
            RunOnUIThread(
                () => PastingApp.ToastMaker.MakeAndShowToast($"Connected! Successfully connected to {device.Name}"));
            return new ToastData("Connected!", $"Successfully connected to {device.Name}", ToastType.Alert);

        }

        public void Disconnect(IClient client)
        {
            // operations for the disconnection

            client.RemoveConnectedDevice();
            PastingApp.app.RemoveConnectedDevice();
        }

        public bool IsConnected()
        {
            throw new NotImplementedException();
        }

        public bool Pair()
        {
            throw new NotImplementedException();
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            if (disposing)
            {
                device.Dispose();
            }
        }

        public int DescribeContents()
        {
            return 0;
        }

        public void WriteToParcel(Parcel dest, [global::Android.Runtime.GeneratedEnum] ParcelableWriteFlags flags)
        {
            dest.WriteTypedObject(device, flags);
        }

    }

    public class BTDeviceCreator : Java.Lang.Object, IParcelableCreator
    {
        public Java.Lang.Object CreateFromParcel(Parcel source)
        {
            return new BTDevice(source).JavaCast<Java.Lang.Object>();
        }

        public Java.Lang.Object[] NewArray(int size)
        {
            return Array.ConvertAll(new BTDevice[size], (device) =>
            {
                return device.JavaCast<Java.Lang.Object>();
            });
        }
    }

}
