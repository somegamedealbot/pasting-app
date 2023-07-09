using Android.App;
using Android.Bluetooth;
using Android.Content;
using Android.Database;
using Android.OS;
using Android.Widget;
using AndroidX.VersionedParcelable;
using Java.Interop;
using Java.IO;
using Java.Nio;
using Java.Security;
using Java.Util;
using Org.Apache.Http.Client;
using PastingMaui.Data;
using PastingMaui.Platforms.Android;
using PastingMaui.Shared;
using System;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.ConstrainedExecution;
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
        System.IO.Stream inStream;
        System.IO.Stream outStream;
        string socketType = String.Empty;
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
            secure = true;
        }

        // connect to a device (server)
        public async Task<ToastData> Connect(IClient client)
        {
            socketType = secure ? "Secure" : "Insecure";
            UUID convertedUuid = UUID.FromString(ServiceConfig.serviceUuidString);
            AndroidToast toast;
            
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
                toast = AndroidToast.MakeText(MainActivity.GetMainActivity(), "Failed to connect: something went wrong when connecitng to sockets", ToastLength.Short);
                toast.Show();
                return new ToastData("Failed to connect", "Something went wrong when connecting socket", ToastType.Alert);
            }

            // send signal to cancel Discovery
            Activity main = MainActivity.GetMainActivity();
            Intent intent = new Intent(main, typeof(BTScanner));
            intent.SetAction(BTScanner.StopScanAction);
            ComponentName name = main.StartService(intent);
            try
            {
                socket.Connect();
            }
            catch(Exception e)
            {
                socket.Dispose();
                socket = null;
            }

            //try
            //{
            //    await socket.ConnectAsync(); // attempt connection

            //}
            //catch(Exception e)
            //{

            //}

            // have input and outputs streams
            if (socket.IsConnected)
            {
                inStream = socket.InputStream;
                outStream = socket.OutputStream;
                client.SetConnectedDevice(this);

            }
            else
            {
                return new ToastData("Failed to connect", $"Make sure that {device.Name} is currently running the app", ToastType.Alert);
            }

            toast = AndroidToast.MakeText(MainActivity.GetMainActivity(), $"Connected! Successfully connected to {device.Name}", ToastLength.Short);
            return new ToastData("Connected!", $"Successfully connected to {device.Name}", ToastType.Alert);

        }

        public void Disconnect(IBTScan scanner)
        {
            throw new NotImplementedException();
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

        public void Disconnect()
        {
            throw new NotImplementedException();
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
