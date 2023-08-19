using PastingMaui.Data;
using PastingMaui.Platforms.Windows;
using PastingMaui.Platforms.Windows.DataHandlers;
using System.Runtime.InteropServices.WindowsRuntime;
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
        DataHandler dataHandler;

        Thread readThread;
        Thread writeThread;

        public static int bufferSize = 4096;

        public IOHandler(BTDevice device, StreamSocket socket, DataHandler handler)
        {
            btDevice = device;
            bluetoothSocket = socket;
            reader = new(socket.InputStream);
            writer = new(socket.OutputStream);
            dataHandler = handler;
        }

        public async void CloseConnection()
        {

            try
            {
                await bluetoothSocket.CancelIOAsync();

            }
            catch (Exception ex)
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


        public async Task ReadLoop(DataReader inStream)
        {

            // now connected to the client

            // display socket statistics using socket.Information

            while (true)
            {

                try
                {
                    PacketInfo packet = await PacketInfo.ReadPacketInfo(inStream);
                    Stream writeLocation = null;
                    if (packet.IsText)
                    {
                        writeLocation = new MemoryStream();
                    }
                    else
                    {
                        // setup file location here
                    }

                    await dataHandler.ReceiveData(inStream, packet, writeLocation);


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
            var infoSize = packet.SetupBuffer(buffer);
            IBuffer convertedBuffer = buffer.AsBuffer();

            writer.WriteBuffer(buffer.AsBuffer(), 0, (uint)infoSize);

            if (packet.Size < bufferSize)
            {
                writeSize = (int)packet.Size;
            }
            else
            {
                writeSize = bufferSize;
            }

            while (totalWriteCount < packet.Size)
            {
                data.Read(buffer, 0, writeSize);
                if (writeSize < bufferSize)
                {
                    writer.WriteBuffer(convertedBuffer, 0, (uint)writeSize);
                }
                else
                {
                    writer.WriteBuffer(convertedBuffer);
                }
                await writer.StoreAsync();
                totalWriteCount += (uint)writeSize;
                remainingCount -= (uint)writeSize;
                writeSize = (remainingCount < bufferSize) ? (int)remainingCount : bufferSize;
                //await data.WriteAsync(buffer.AsMemory(0, bufferSize));

            }

        }
    }
}