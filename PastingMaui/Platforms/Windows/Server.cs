using Microsoft.Maui.Controls.PlatformConfiguration;
using Microsoft.VisualBasic;
using PastingMaui.Data;
using PastingMaui.Platforms.Windows;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.ApplicationModel.DataTransfer;
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
        DataWriter dataWriter;
        DataReader dataReader;

        Thread readThread;
        Thread writeThread;

        public class OnReadThreadEndArgs : EventArgs
        { 
        }

        public Server() { }

        private delegate void OnReadThreadEndHandler(object sender, OnReadThreadEndArgs e);

        private event OnReadThreadEndHandler OnReadThreadEnd = (sender, e) =>
        {
            // put things here when the read thread is disconnected
        };
        //private void OnReadThreadEnd(EventArgs e)
        //{
        //    ReadThreadEnd?.Invoke(this, e);
        //}

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
            
            // sort out sockets
            socketListener = new StreamSocketListener();
            socketListener.ConnectionReceived += SocketConnect;

            await socketListener.BindServiceNameAsync(provider.ServiceId.AsString(), SocketProtectionLevel.BluetoothEncryptionAllowNullAuthentication);

            InitSDPAttributes(provider);

            // advertising
            try
            {
                provider.StartAdvertising(socketListener, true);
            }
            catch (Exception ex)
            {
                // notify usre about the error here with ui
            }

            // started to listen to connections
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

        void Disconnect()
        {
            // remove connection
            // reset all fields
            if (provider != null)
            {
                provider.StopAdvertising();
                provider = null;
            }

            if (socketListener != null)
            {
                socketListener.Dispose();
                socketListener = null; 
            }

            if (socket != null)
            {
                socket.Dispose();
                socket = null;
            }

            if (dataWriter != null)
            {
                dataWriter.DetachStream();
                dataWriter = null;
            }

            if (dataReader != null)
            {
                dataReader.DetachStream();
                dataReader = null;
            }

            // disconnected from client
        }

        async void sendInfo(IBuffer buffer, int type)
        {
            if (buffer.Length > 0)
            {
                if (dataWriter != null)
                {
                    try
                    {
                        // write
                        dataWriter.WriteByte((byte)type);
                        dataWriter.WriteBuffer(buffer);

                        await dataWriter.StoreAsync();
                    }
                    catch(Exception ex)
                    {
                        // handle exception here
                    }

                }
                else
                {
                    // no client is connected
                }

            }


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
                Disconnect();
                

                return;
            }

            // after verifying socket, change the UI

            // loop to read content sent
            var device = await BluetoothDevice.FromHostNameAsync(socket.Information.RemoteHostName);

            dataWriter = new DataWriter(socket.OutputStream);
            dataReader = new DataReader(socket.InputStream);

            // now connected to the client

            // assign new thread to read info
            readThread = new Thread(() =>
            {
                IOHandler.ReadLoop(dataReader, PastingApp.app.client.deviceScanner);
                OnReadThreadEnd?.Invoke(this, null);
            });

            writeThread = new Thread(() =>
            {
                // write loop
            });
            readThread.Start();
            writeThread.Start();

            // display socket statistics using socket.Information

            //while (true)
            //{
            //    IBuffer buffer;
            //    bool disconnected = false;

            //    try
            //    {
            //        byte type = dataReader.ReadByte(); // type of info

            //        // 32 bit integer
            //        int dataSize = dataReader.ReadInt32();
            //        var readCount = await dataReader.LoadAsync((uint)dataSize);


            //        if (readCount < dataSize)
            //        {
            //            // data was cut off, or connection was lost

            //            // cut connection
            //            disconnected = true;

            //            // notify user that connection was broken
            //        }

            //        buffer = dataReader.ReadBuffer(readCount);

            //        // save file to folder location
            //    }
            //    catch (Exception ex)
            //    {
            //        // handle exception here 
            //    }

            //    if (disconnected)
            //    {
            //        Disconnect();

            //        // notify user about the disconnection
            //    }

            //}
            
        }
    }
}
