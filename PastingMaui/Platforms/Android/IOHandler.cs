using Android.Bluetooth;
using PastingMaui.Data;
using PastingMaui.Platforms.Windows.DataHandlers;

namespace PastingMaui.Platforms.Android
{
    public class IOHandler : IIOHandler
    {

        BTDevice btDevice;
        BluetoothSocket bluetoothSocket;
        Stream outStream;
        Stream inStream;
        DataHandler dataHandler;

        Thread readThread;
        Thread writeThread;

        public static int bufferSize = 4096;

        public IOHandler(BTDevice device, BluetoothSocket socket, DataHandler data)
        {
            btDevice = device;
            bluetoothSocket = socket;
            outStream = socket.OutputStream;
            inStream = socket.InputStream;
            dataHandler = data;
        }

        public bool CloseConnection()
        {
            if (bluetoothSocket is not null)
            {
                // stop thread loops
                // close streams and socket
                inStream.Close();
                outStream.Close();
                bluetoothSocket.Close();
                bluetoothSocket.Dispose();
                bluetoothSocket = null;
                return true;
            }
            return false;
        }

        public bool StartReadThread()
        {
            readThread = new Thread(async () =>
            {
                await ReadLoop(inStream);
                CallOnReadEnd();
            });
            try { readThread.Start(); }
            catch {
                // Out of memory warning
                return false;
            }
            return true;

        }

        public bool WriteStreamTo(PacketInfo info, Stream data)
        {
            writeThread = new Thread(async () =>
            {
                await WriteData(outStream, info, data);
                CallOnWriteEnd();
            });
            try { writeThread.Start(); } catch
            {
                // Out of memory warning
                return false;
            }
            return true;
        }

        public async Task ReadLoop(Stream inStream) // make new thread for ReadLoop?
        {
            while (true)
            {

                try
                {
                    PacketInfo packet = await PacketInfo.ReadPacketInfo(inStream, bufferSize);
                    Stream writeLocation = null;
                    if (!packet.IsText)
                    {
                        writeLocation = new MemoryStream();
                    }
                    else
                    {
                        //writeLocation = File.Create() file path here
                    }

                    await dataHandler.ReceiveData(inStream, packet, writeLocation);

                }
                catch(Exception ex)
                {
                    return;
                }
            }
            // cancel any writing
           
        }

        public async Task WriteData(Stream outStream, PacketInfo packet, Stream data)
        {
            byte[] buffer = new byte[bufferSize];
            uint totalWriteCount = 0;
            uint remainingCount = packet.Size;
            int writeSize = 0;

            buffer.AsMemory(0, 4096);
            var infoSize = packet.SetupBuffer(buffer);

            outStream.Write(buffer, 0, infoSize);

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
                outStream.Write(buffer, 0, writeSize);
                outStream.Flush();
                //await writer.StoreAsync();
                totalWriteCount += (uint)writeSize;
                remainingCount -= (uint)writeSize;
                writeSize = (packet.Size - writeSize < bufferSize) ? (int)packet.Size : bufferSize;
                //await data.WriteAsync(buffer.AsMemory(0, bufferSize));

            }

        }

    }
}
