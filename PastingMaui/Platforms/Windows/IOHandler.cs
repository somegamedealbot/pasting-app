using PastingMaui.Data;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
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

        public IOHandler(StreamSocket socket)
        {
            bluetoothSocket = socket;
            writer = new DataWriter(socket.OutputStream);
            reader = new DataReader(socket.InputStream);
        }

        public IOHandler(BTDevice device, StreamSocket socket)
        {
            btDevice = device;
            bluetoothSocket = socket;
            writer = new DataWriter(socket.OutputStream);
            reader = new DataReader(socket.InputStream);
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

        public bool WriteStreamTo(int type, Stream data, uint dataSize)
        {
            writeThread = new Thread(async () =>
            {
                await WriteData(writer, type, data, dataSize);
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
                IBuffer buffer;
                bool disconnected = false;
                uint blockSize = 4096;
                uint initReadCount = 1 + sizeof(uint);
                uint tempCount = 0;
                uint remainingCount = 0;

                try
                {
                    await reader.LoadAsync(initReadCount);
                    int type = reader.ReadByte(); // type of info

                    if (type > 2)
                    {
                        // notify user bad packet
                    }

                    // 32 bit integer
                    uint dataSize = reader.ReadUInt32();


                    while ((tempCount += await reader.LoadAsync(blockSize)) != 0)
                    {
                        remainingCount = dataSize - tempCount;
                        buffer = reader.ReadBuffer(blockSize);

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
        public async Task WriteData(DataWriter writer, int type, Stream data, uint dataSize)
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
                writer.WriteBytes(buffer);
                totalWriteCount += (uint)writeSize;
                remainingCount -= (uint)writeSize;

                writeSize = (dataSize - writeSize < bufferSize) ? (int)dataSize : bufferSize;
                await data.WriteAsync(buffer.AsMemory(0, bufferSize));

            }

        }
    }
}
