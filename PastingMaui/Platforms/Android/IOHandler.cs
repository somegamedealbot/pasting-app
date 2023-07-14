using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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

        public bool WriteStreamTo(int type, Stream data, uint dataSize)
        {
            writeThread = new Thread(async () =>
            {
                await WriteData(outStream, type, data, dataSize);
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
                bool initialRead = true;
                int bufferSize = 4096;
                byte[] buffer = new byte[bufferSize];
                uint totalReadCount = 0;
                int tempCount = 0;
                uint dataSize = 0;

                try
                {
                    while ((tempCount += await inStream.ReadAsync(buffer.AsMemory(0, bufferSize))) != 0)
                    {
                        totalReadCount += (uint)tempCount;
                        if (initialRead)
                        {
                            int type = BitConverter.ToInt32(buffer, 0);
                            dataSize = BitConverter.ToUInt32(buffer, sizeof(int));
                            int preDataSize = sizeof(uint) + 1;
                            totalReadCount -= (uint)preDataSize;
                            initialRead = false;
                            // make sure to check for file signature for the first run through

                            // decide to save to file or display data
                        }



                    }
                }
                catch(Exception ex)
                {
                    return;
                }
            }
            // cancel any writing
           
        }

        public async Task WriteData(Stream outStream, int type, Stream data, uint dataSize)
        {
            int bufferSize = 4096;
            byte[] buffer = new byte[bufferSize];
            uint totalWriteCount = 0;
            uint remainingCount = dataSize;
            int writeSize = 0;

            buffer.AsMemory(0, 4096);
            BitConverter.GetBytes(type).CopyTo(buffer, 0);
            BitConverter.GetBytes(dataSize).CopyTo(buffer, sizeof(int));
            writeSize += sizeof(int) + sizeof(uint);

            await data.WriteAsync(buffer.AsMemory(writeSize,
                bufferSize - writeSize));

            if (dataSize - writeSize < bufferSize)
            {
                writeSize += (int)dataSize;
            }
            else
            {
                writeSize += bufferSize - writeSize;
            }

            while (totalWriteCount < dataSize)
            {
                await outStream.WriteAsync(buffer.AsMemory(0, writeSize));
                totalWriteCount += (uint)writeSize;
                remainingCount -= (uint)writeSize;

                writeSize = (dataSize - writeSize < bufferSize) ? (int)dataSize : bufferSize;
                await data.WriteAsync(buffer.AsMemory(0,
                bufferSize));

            }

        }

    }
}
