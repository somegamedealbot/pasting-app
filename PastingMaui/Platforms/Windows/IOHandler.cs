using PastingMaui.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage.Streams;

namespace PastingMaui.Platforms.Windows
{
    internal class IOHandler
    {
        public async static void ReadLoop(DataReader reader, IBTScan scanner)
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

                    }
                    else
                    {

                    }
                }

                //if (disconnected)
                //{
                //    Disconnect(scanner);
                //    return;
                //    // notify user about the disconnection
                //}
            }
        }
    }
}
