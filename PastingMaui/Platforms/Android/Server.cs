using Android.Bluetooth;
using Android.Content;
using Java.Util;
using PastingMaui.Data;
using PastingMaui.Platforms.Windows.DataHandlers;
using AndroidToast = Android.Widget.Toast;

namespace PastingMaui.Platforms
{
    internal class Server : IServer
    {
        public Server(Context context) { }

        readonly BluetoothAdapter adapter;
        readonly BluetoothManager manager;
        private BluetoothServerSocket socket;
        BTDevice device;
        DataHandler dataHandler;
        string socketType;
        bool isSecure;

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

    }
}
