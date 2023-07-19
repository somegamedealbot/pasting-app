using PastingMaui.Platforms.Android;
using System.Text;
using static PastingMaui.Platforms.PastingApp;

namespace PastingMaui.Platforms.Windows.DataHandlers
{
    internal class DataHandler
    {

        IOHandler IOHandler;

        public void SetIOHandler(IOHandler handler)
        {
            IOHandler = handler;
        }

        public void RemoveIOHandler()
        {
            IOHandler = null;
        }

        public async Task<string> GetClipboardData()
        {
            return Clipboard.HasText == true ? await Clipboard.GetTextAsync() : null;
        }

        public void SendPasteData()
        {
            string clipboardData = null;
            MainThread.InvokeOnMainThreadAsync(async () =>
            {
                clipboardData = await GetClipboardData();
            });

            if (clipboardData == null)
            {
                ToastMaker.MakeToast("No clipboard data found");
                return;
            }

            var convertedStream = new MemoryStream(Encoding.UTF8.GetBytes(clipboardData));

            PacketInfo packet = PacketInfo.SetPacketInfo((uint)clipboardData.Length, true);


            if (IOHandler == null)
            {
                throw new Exception("null IOHandler");
            }
            else
            {
                IOHandler.WriteStreamTo(packet, convertedStream);
            }

        }

        public async Task ReceiveData(Stream inStream, PacketInfo packet, Stream writeLocation)
        {
            byte[] buffer = new byte[IOHandler.bufferSize];
            uint totalReadCount = 0;
            int tempCount = 0;

            try
            {
                while ((tempCount += await inStream.ReadAsync(buffer.AsMemory(0, IOHandler.bufferSize))) != 0 &&
                                        packet.Size > totalReadCount)
                {
                    totalReadCount += (uint)tempCount;
                    await writeLocation.WriteAsync(buffer);
                    // save text or file here
                }

            }
            catch (Exception)
            {
                throw;
            }

            // call to finished reading this file/txt
            //Task.Run(PastingApp.app)

        }

    }
}
