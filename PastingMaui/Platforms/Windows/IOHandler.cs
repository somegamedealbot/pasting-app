using PastingMaui.Data;
using PastingMaui.Platforms.Android;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;
using Windows.Networking.Sockets;
using Windows.Storage.Streams;

namespace PastingMaui.Platforms.Windows
{
    internal class IOHandler : IIOHandler
    {
        BTDevice btDevice;
        StreamSocket bluetoothSocket;
        DataReader reader;
        DataWriter writer;

        Thread readThread;
        Thread writeThread;

        public static int bufferSize = 4096;

        public IOHandler(StreamSocket socket)
        {
            bluetoothSocket = socket;
            reader = new DataReader(socket.InputStream);
            writer = new DataWriter(socket.OutputStream);
        }

        public IOHandler(BTDevice device, StreamSocket socket)
        {
            btDevice = device;
            bluetoothSocket = socket;
            reader = new DataReader(socket.InputStream);
            writer = new DataWriter(socket.OutputStream);
        }

        public async void CloseConnection()
        {

            try
            {
                await bluetoothSocket.CancelIOAsync();

            }
            catch(Exception ex)
            {

            }
            if (writer != null)
            {
                writer = null;
            }

            if (reader != null)
            {
                reader = null;
            }

            bluetoothSocket.Dispose();
            // the method after this should consider disposing this handler
        }

        public bool StartReadThread()
        {
            readThread = new Thread(async () =>
            {
                await ReadLoop(reader);
                CallOnReadEnd();
            });
            try { readThread.Start(); }
            catch
            {
                // Out of memory warning
                return false;
            }
            return true;

        }

        public bool WriteStreamTo(PacketInfo packet, Stream data)
        {
            writeThread = new Thread(async () =>
            {
                await WriteData(writer, packet, data);
                CallOnWriteEnd();
            });
            try { writeThread.Start(); }
            catch
            {
                // Out of memory warning
                return false;
            }
            return true;
        }


        public async Task ReadLoop(DataReader reader)
        {

            // now connected to the client

            // display socket statistics using socket.Information

            while (true)
            {

                try
                {
                    PacketInfo packet = await PacketInfo.ReadPacketInfo(reader);
                    Stream writeLocation;
                    if (packet.IsText)
                    {
                        writeLocation = new MemoryStream();
                    }
                    else
                    {

                    }


                    // save file to folder location
                }
                catch (Exception ex)
                {
                    // handle exception here 
                    if ((uint)ex.HResult == 0x80072745) // disconnect by remote device
                    {
                        // cancel any write operation
                        //PastingApp.app.RemoveConnectedDevice();
                    }
                    else if ((uint)ex.HResult == 0x8000000B)
                    {
                        //PastingApp.app.RemoveConnectedDevice();
                    }
                    else
                    {

                    }
                    return;
                }

                //if (disconnected)
                //{
                //    Disconnect(scanner);
                //    return;
                //    // notify user about the disconnection
                //}
            }
        }
        public async Task WriteData(DataWriter writer, PacketInfo packet, Stream data)
        {
            int bufferSize = 4096;
            byte[] buffer = new byte[bufferSize];
            uint totalWriteCount = 0;
            uint remainingCount = packet.Size;
            int writeSize = 0;

            buffer.AsMemory(0, 4096);
            BitConverter.GetBytes(packet.IsText).CopyTo(buffer, 0);
            BitConverter.GetBytes(packet.Size).CopyTo(buffer, sizeof(int));
            var infoSize = sizeof(int) + sizeof(uint);

            writer.WriteBuffer(buffer.AsBuffer(), 0, (uint)infoSize);

            if (packet.Size < bufferSize)
            {
                writeSize += (int)packet.Size;
            }
            else
            {
                writeSize += bufferSize - writeSize;
            }

            while (totalWriteCount < packet.Size)
            {
                data.Read(buffer, 0, writeSize);
                writer.WriteBytes(buffer);
                await writer.StoreAsync();
                totalWriteCount += (uint)writeSize;
                remainingCount -= (uint)writeSize;
                writeSize = (packet.Size - writeSize < bufferSize) ? (int)packet.Size : bufferSize;
                //await data.WriteAsync(buffer.AsMemory(0, bufferSize));

            }

        }
    }
}
