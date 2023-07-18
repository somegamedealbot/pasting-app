using Android.Bluetooth;
using PastingMaui.Data;

namespace PastingMaui.Platforms.Android
{
    public class IOHandler : IIOHandler
    {

        BTDevice btDevice;
        BluetoothSocket bluetoothSocket;
        Stream outStream;
        Stream inStream;

        Thread readThread;
        Thread writeThread;

        public static int bufferSize = 4096;

        public IOHandler(BluetoothSocket socket)
        {
            bluetoothSocket = socket;
            outStream = socket.OutputStream;
            inStream = socket.InputStream;
        }

        public IOHandler(BTDevice device, BluetoothSocket socket)
        {
            btDevice = device;
            bluetoothSocket = socket;
            outStream = socket.OutputStream;
            inStream = socket.InputStream;
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
            int bufferSize = 4096;
            byte[] buffer = new byte[bufferSize];
            uint totalWriteCount = 0;
            uint remainingCount = packet.Size;
            int writeSize = 0;

            buffer.AsMemory(0, 4096);
            BitConverter.GetBytes(packet.IsText).CopyTo(buffer, 0);
            BitConverter.GetBytes(packet.Size).CopyTo(buffer, sizeof(int));
            var infoSize = sizeof(int) + sizeof(uint);

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

        //public async Task WriteData(Stream outStream, PacketInfo packetInfo, Stream data)
        //{
        //    int bufferSize = 4096;
        //    byte[] buffer = new byte[bufferSize];
        //    uint totalWriteCount = 0;
        //    uint remainingCount = packetInfo.Size;
        //    int writeSize = 0;

        //    buffer.AsMemory(0, 4096);
        //    BitConverter.GetBytes(packetInfo.IsText).CopyTo(buffer, 0);
        //    BitConverter.GetBytes(packetInfo.Size).CopyTo(buffer, sizeof(int));
        //    writeSize += sizeof(int) + sizeof(uint);

        //    await data.WriteAsync(buffer.AsMemory(writeSize,
        //        bufferSize - writeSize));

        //    if (packetInfo.Size - writeSize < bufferSize)
        //    {
        //        writeSize += (int)packetInfo.Size;
        //    }
        //    else
        //    {
        //        writeSize += bufferSize - writeSize;
        //    }

        //    while (totalWriteCount < packetInfo.Size)
        //    {
        //        await outStream.WriteAsync(buffer.AsMemory(0, writeSize));
        //        totalWriteCount += (uint)writeSize;
        //        remainingCount -= (uint)writeSize;

        //        writeSize = (packetInfo.Size - writeSize < bufferSize) ? (int)packetInfo.Size : bufferSize;
        //        await data.WriteAsync(buffer.AsMemory(0,
        //        bufferSize));

        //    }

        //}

    }
}
