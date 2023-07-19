using Android.Bluetooth;
using Android.Content;
using Android.Runtime;
using Java.Util;
using PastingMaui.Data;
using PastingMaui.Platforms.Android;
using AndroidToast = Android.Widget.Toast;

namespace PastingMaui.Platforms
{
    internal class Server : IServer
    {
        public Server(Context context) { }

        BluetoothAdapter adapter;
        BluetoothManager manager;
        private BluetoothServerSocket socket;
        IOHandler handler;
        string socketType = string.Empty;
        BTDevice device;
        bool isSecure;

        private class PastingProfile : Java.Lang.Object, IBluetoothProfile
        {
            public IList<BluetoothDevice> ConnectedDevices => throw new NotImplementedException();

            [return: GeneratedEnum]
            public ProfileState GetConnectionState(BluetoothDevice device)
            {
                throw new NotImplementedException();
            }

            public IList<BluetoothDevice> GetDevicesMatchingConnectionStates([GeneratedEnum] ProfileState[] states)
            {
                throw new NotImplementedException();
            }
        }
        IBluetoothProfile profile;

        public Server() {
            manager = ((BluetoothManager)MauiApplication.Context.GetSystemService(Context.BluetoothService));
            adapter = manager.Adapter;
            isSecure = true;
            Task.Run(() => InitServer());
        }

        public async Task InitServer()
        {
            await StartServer();
        }

        public async Task StartServer()
        {
            if (socket is not null) {
                return;
            }
            try
            {
                socketType = "Secure";
                UUID uuid = UUID.FromString(ServiceConfig.serviceUuidString);
                if (isSecure)
                {
                    
                    socket = adapter
                        .ListenUsingRfcommWithServiceRecord(ServiceConfig.SdpServiceName, uuid);
                       // get uuids here to see if its registered
                }
                else
                {
                    socket = adapter
                        .ListenUsingInsecureRfcommWithServiceRecord(ServiceConfig.SdpServiceName, uuid);
                }

            }
            catch (Exception e)
            {
                if (socket != null)
                {
                    socket.Dispose();
                    socket = null;
                    socketType = string.Empty;
                    throw;
                }
                // Write exception here
            }

            await AcceptConnection();

        }

        public async Task AcceptConnection()
        {
            BluetoothSocket btSocket = await socket.AcceptAsync(); // temporary, when the rest of ui is make use the method with a timeout

            if (btSocket.IsConnected)
            {
                PastingApp.app.StopServicesOnConnect();
                device = new BTDevice(btSocket.RemoteDevice);
                SetIOHandler(btSocket, device);
                //handler.StartReadThread();
                PastingApp.app.SetConnectedDevice(device, btSocket);
                PastingApp.ToastMaker.MakeAndShowToast("Connected to device");
            }
        }

        public void StopServer()
        {
            socket.Close();
            socket = null;
            //handler?.Dispose();
        }

        private void SetIOHandler(BluetoothSocket socket, BTDevice btDevice)
        {
            //handler?.Dispose();
            handler = new IOHandler(btDevice, socket);
        }

    }
}
