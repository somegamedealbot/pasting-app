﻿using PastingMaui.Data;
using PastingMaui.Platforms.Windows;
using PastingMaui.Shared;
using Windows.Networking.Sockets;

namespace PastingMaui.Platforms
{
    internal class PastingApp : IPasting
    {

        Client appClient;
        Server appServer;

        BTDevice connectedDevice;
        IOHandler handler;
        public IToastService _toast_service;

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
            StartServer();
            // https://github.com/android/connectivity-samples/issues/263#issuecomment-1100650576
            // use for requsting bluetooth permission
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

        public Client client
        {
            get
            {
                return client;
            }
        }

        public Server server
        {
            get
            {
                return server;
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
            handler.OnReadThreadEnd += async (sender, args) =>
            {
                app._toast_service.
                    AddToast("Disconnected", "Successfully received a connection", Shared.ToastType.Alert);
                // Stop any writing here
                RemoveConnectedDevice();
                await appServer.StartServer(); // attempts to restart server for now
            };
            handler.OnWriteThreadEnd += (sender, args) =>
            {
                app._toast_service.
                    AddToast("Sent Data", "Successfully sent data", Shared.ToastType.Alert);
            };
        }
        public void SetConnectedDevice(BTDevice device, StreamSocket socket)
        {
            // might need to lock the object
            ConnectedToDevice = true;
            ConnectedDevice = device;
            handler = new IOHandler(device, socket);
            SetupReadWriteHandlers();
            OnUIChangeOnConnect.Invoke(this, null);
            if (handler.StartReadThread())
            {
                _toast_service.AddToast("Out of Memory", "Out of memory, Could not create new thread for receiving data", ToastType.Alert);
            }
        }

        public void DisconnectDevice()
        {
            handler.CloseConnection();
            //RemoveConnectedDevice();
        }

        public void StopServicesOnConnect()
        {
            appClient.StopScanning();
            appServer.StopServer();
        }

        public void RemoveConnectedDevice()
        {
            ConnectedToDevice = false;
            //ConnectedDevice.Dispose()
            ConnectedDevice = null;
            //handler.Dispose()
            handler = null;
            OnUIChangeOnDisconnect.Invoke(this, null);
        }

        public void StartClient()
        {
            appClient.ScanDevices();
        }

        public void StartScanningDevices()
        {
            appClient.ScanDevices();
        }

        public void StartServer()
        {
            Task.Run(() => appServer.InitServer()).Wait();
        }
    }
}
