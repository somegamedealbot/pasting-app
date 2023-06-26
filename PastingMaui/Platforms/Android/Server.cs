using Android.Bluetooth;
using Android.Content;
using Java.Util;
using PastingMaui.Data;
using PastingMaui.Platforms.Android;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PastingMaui.Platforms
{
    internal class Server : IServer
    {
        public Server(Context context) { }

        BluetoothAdapter adapter;
        private BluetoothServerSocket socket;
        System.IO.Stream inStream;
        System.IO.Stream outStream;
        string socketType = string.Empty;
        bool isSecure;

        public Server(BluetoothAdapter btAdapter, bool secure) {
            adapter = btAdapter;
            isSecure = secure; 
            InitServer(secure);
        }

        public void InitServer(bool secure)
        {
            // start listening for connections
            try
            {
                socketType = secure ? "Secure" : "Insecure";
                UUID uuid = UUID.FromString(ServiceConfig.serviceUuidString);
                if (secure)
                {
                    socket = adapter
                        .ListenUsingRfcommWithServiceRecord(ServiceConfig.SdpServiceName, uuid);
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

                }
                // Write exception here
            }

            // send here to read loop
            // write the reader to be used either in the server or the client
            // it can executed on another thread (optional)

            Thread thread = new Thread(new ThreadStart(async () =>
            {
                await IOHandler.ReadLoop(inStream);
            }));

            thread.Start();

            // same with writer

        }

        public Task InitServer()
        {
            throw new NotImplementedException();
        }

        public void sendData()
        {
            Thread thread = new Thread(new ThreadStart(async () =>
            {
                //await IOHandler.WriteData(outStream, isSecure, data, Size);

            }));
        }

    }
}
