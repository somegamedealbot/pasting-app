using PastingMaui.Data;
using PastingMaui.Platforms.Windows;
using Windows.Devices.Bluetooth;
using Windows.Devices.Bluetooth.Rfcomm;
using Windows.Networking.Sockets;
using Windows.Storage.Streams;
using WindowsStreams = Windows.Storage.Streams;

namespace PastingMaui.Platforms
{
    internal class Server : IServer
    {
        // move the uuid later
        RfcommServiceProvider provider;
        StreamSocketListener socketListener;
        StreamSocket socket;
        BluetoothDevice btDevice;
        BTDevice deviceInfo;

        IOHandler handler;

        bool ServerRunning;
        bool ServerEnabled;

        public class OnReadThreadEndArgs : EventArgs
        { 
        }

        public Server() { }

        private delegate void OnReadThreadEndHandler(object sender, OnReadThreadEndArgs e);

        private event OnReadThreadEndHandler OnReadThreadEnd = (sender, e) =>
        {
            // put things here when the read thread is disconnected
        };

        public async Task InitServer()
        {
            try {
                provider = await RfcommServiceProvider.CreateAsync(RfcommServiceId.FromUuid(ServiceConfig.serviceUuid));

            } 
            catch (Exception ex) {
                // bluetooth isn't on currently for this machine
                // warn user here with ui
                return;
            }

            InitSDPAttributes(provider);

            await StartServer();

            // started to listen to connections
        }

        public async Task StartServer()
        {
            if (ServerRunning) // server already running
            {
                return;
            }

            await StartListeningSocket();

            // advertising
            try
            {
                provider.StartAdvertising(socketListener, true);
            }
            catch (Exception ex)
            {
                // notify usre about the error here with ui
                PastingApp.app._toast_service.
                    AddToast("Error Starting Server", "Could not start advertising device", Shared.ToastType.Alert);
            }
        }

        public async Task StartListeningSocket()
        {
            socketListener = new StreamSocketListener();
            socketListener.ConnectionReceived += SocketConnect;

            await socketListener.BindServiceNameAsync(provider.ServiceId.AsString(), SocketProtectionLevel.BluetoothEncryptionAllowNullAuthentication);
        }

        public void SetupReadWriteHandlers()
        {
            handler.OnReadThreadEnd += (sender, args) =>
            {
                PastingApp.app._toast_service.
                    AddToast("Disconnected", "Successfully received a connection", Shared.ToastType.Alert);
            };
            handler.OnWriteThreadEnd += (sender, args) =>
            {
                PastingApp.app._toast_service.
                    AddToast("Sent Data", "Successfully sent data", Shared.ToastType.Alert);
            };
        }

        void InitSDPAttributes(RfcommServiceProvider provider)
        {
            var writer = new DataWriter();
            
            writer.WriteByte(ServiceConfig.SdpServiceNameAttributeType);
            writer.WriteByte((byte)ServiceConfig.SdpServiceName.Length);

            writer.UnicodeEncoding = WindowsStreams.UnicodeEncoding.Utf8;
            writer.WriteString(ServiceConfig.SdpServiceName);

            provider.SdpRawAttributes.Add(ServiceConfig.sdpServiceAttributeId, writer.DetachBuffer());
        }

        public void StopServer() {
            provider.StopAdvertising();

        }

        private void SetHandler(StreamSocket socket, BTDevice device)
        {
            handler = new IOHandler(device, socket);
        }

        /*
         * Packet Layout
         * 1 byte for type -> 2 bytes for size -> n bytes for data
         * */
        async void SocketConnect(StreamSocketListener listener, StreamSocketListenerConnectionReceivedEventArgs args)
        {
            socketListener.Dispose();

            try
            {
                socket = args.Socket;
            }

            catch (Exception ex)
            {
                // notify error

                // disconnect from client
                PastingApp.app._toast_service.
                    AddToast("Failed connection from client", "Error connecting to client", Shared.ToastType.Alert);
                socket.Dispose();
                return;
            }

            // after verifying socket, change the UI

            // loop to read content sent
            btDevice = await BluetoothDevice.FromHostNameAsync(socket.Information.RemoteHostName);
            deviceInfo = new BTDevice(btDevice.DeviceInformation);

            PastingApp.app.StopServicesOnConnect();

            PastingApp.app.SetConnectedDevice(deviceInfo, socket);
            SetHandler(socket, deviceInfo);
            SetupReadWriteHandlers();

        }
    }
}
