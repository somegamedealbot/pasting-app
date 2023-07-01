using Android.Bluetooth;
using Android.Database;
using Android.Media;
using Android.OS;
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
using System.Runtime.CompilerServices;
using System.Runtime.ConstrainedExecution;

namespace PastingMaui.Platforms
{
    public class BTDevice : Java.Lang.Object, IParcelable, IBTDevice
    {
        public BluetoothDevice device
        {
            get; private set;
        }

        public string Id => throw new NotImplementedException();

        public string Name => throw new NotImplementedException();

        public string Type => throw new NotImplementedException();

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
        }

        // connect to a device (server)
        public Task<ToastData> Connect(IBTScan scanner)
        {

            socketType = secure ? "Secure" : "Insecure";
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
                return Task.FromResult(new ToastData("Failed to connect", "Something went wrong when connecting socket", ToastType.Alert));
            }

            // have input and outputs streams
            if (socket != null)
            {
                inStream = socket.InputStream;
                outStream = socket.OutputStream;
            }

            return Task.FromResult(new ToastData("Connected!", $"Successfully connected to {device.Name}", ToastType.Alert));

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
