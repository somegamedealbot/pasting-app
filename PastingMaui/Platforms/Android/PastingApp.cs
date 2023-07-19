using PastingMaui.Data;
using PastingMaui.Platforms.Android;
using PastingMaui.Shared;
using Application = Android.App.Application;
using AndroidToast = Android.Widget.Toast;
using AndroidToastLength = Android.Widget.ToastLength;
using Android.Bluetooth;
using PastingMaui.Platforms.Windows.DataHandlers;

namespace PastingMaui.Platforms
{
    internal class PastingApp : IPasting
    {

        public class ToastMaker
        {
            public static AndroidToast MakeToast(string msg)
            {
                return AndroidToast.MakeText(MainActivity.GetMainActivity(), msg, AndroidToastLength.Short);
            }

            public static void MakeAndShowToast(string msg)
            {
                AndroidToast.MakeText(MainActivity.GetMainActivity(), msg, AndroidToastLength.Short).Show();
            }
        }

        public Client appClient;
        private IToastService _toast_service;
        public Server appServer;
        private IOHandler ioHandler;
        private DataHandler dataHandler;

        public event EventHandler OnUIChangeOnConnect;
        public event EventHandler OnUIChangeOnDisconnect;

        public static PastingApp app
        {
            get; private set;
        }

        public PastingApp(IToastService _service)
        {
            _toast_service = _service;
            app = this;
            appClient = new Client();
            appServer = new Server();
            dataHandler = new DataHandler();
        }
        
        IServer IPasting.server {
            get
            {
                return appServer;
            }
        }
        IClient IPasting.client {
            get
            {
                return appClient;
            }
        }

        public bool ConnectedToDevice
        {
            get; private set;
        }

        public IBTDevice ConnectedDevice
        {
            get; private set;
        }

        private void SetupReadWriteHandlers()
        {
            ioHandler.OnReadThreadEnd += (sender, args) =>
            {
                app._toast_service.
                    AddToast("Disconnected", "Successfully received a connection", Shared.ToastType.Alert);
                RemoveConnectedDevice();
                Task.Run(() => appServer.StartServer()); // attempts to restart server for now
            };
            ioHandler.OnWriteThreadEnd += (sender, args) =>
            {
                app._toast_service.
                    AddToast("Sent Data", "Successfully sent data", Shared.ToastType.Alert);
            };
        }

        public void SetConnectedDevice(BTDevice device, BluetoothSocket socket)
        {
            ConnectedToDevice = true;
            ConnectedDevice = device;
            ioHandler = new IOHandler(device, socket);
            SetupReadWriteHandlers();
            dataHandler.SetIOHandler(ioHandler);
            OnUIChangeOnConnect?.Invoke(this, null);
            if (ioHandler.StartReadThread())
            {
                // notify that there is no more ram
                AndroidToast.MakeText(MainActivity.GetMainActivity(), "Out of Memory: Could not create read Thread", AndroidToastLength.Short);
                //_toast_service.AddToast(new ToastData("Error establishing connection", "Out of Memory", ToastType.Alert));
            }
        }

        public void RemoveConnectedDevice()
        {
            ConnectedToDevice = ConnectedToDevice == true ? false : throw new Exception("Bad Connected Device State");
            ConnectedDevice = null;
            ioHandler.CloseConnection();
            dataHandler.RemoveIOHandler();
            OnUIChangeOnDisconnect?.Invoke(this, null);
            // dispose here
            //ioHandler.Dispose();
            //ConnectedToDevice.Dispose()
        }

        public void StopServicesOnConnect()
        {
            Client.StopScanning();
            appServer.StopServer();
        }

        public void StartServices()
        {
            StartScanningDevices();
            Task.Run(appServer.StartServer);
        }

        public void StartClient()
        {
        }

        public void StartServer()
        {

        }

        public Task SendPasteData()
        {
            dataHandler.SendPasteData();
            return Task.CompletedTask;
        }

        public static PastingApp getApp()
        {
            return app ?? null;
        }

        public void StartScanningDevices()
        {
            Task.Run(() => appClient.ScanDevices());
        }

        public void DisconnectDevice()
        {
            //ioHandler.Dispose();
            ioHandler.CloseConnection();
        }
    }
}
