using PastingMaui.Platforms.Android;
using PastingMaui.Shared;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using Windows.Storage.Streams;

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

        public async Task SendPasteData()
        {
            string clipboardData = null;
            await MainThread.InvokeOnMainThreadAsync(async () =>
            {
                clipboardData = await GetClipboardData();
            });

            if (clipboardData == null)
            {
                PastingApp.app._toast_service.AddToast("No clipboard data", "No clipboard data found", Shared.ToastType.Alert); 
                return;
            }

            var convertedStream = new MemoryStream(Encoding.UTF8.GetBytes(clipboardData));

            PacketInfo packet = PacketInfo.SetPacketInfo((uint)convertedStream.Length, true);


            if (IOHandler == null)
            {
                throw new Exception("null IOHandler");
            }
            else
            {
                IOHandler.WriteStreamTo(packet, convertedStream);
            }

        }

        public async Task ReceiveData(DataReader reader, PacketInfo packet, Stream writeLocation)
        {
            IBuffer buffer;
            uint tempCount = 0;
            uint remainingCount = 0;
            Paste paste = new Paste(writeLocation);
            try
            {
                while ((tempCount += await reader.LoadAsync((uint)IOHandler.bufferSize)) != 0
                            && remainingCount > 0)
                {
                    remainingCount = packet.Size - tempCount;
                    buffer = reader.ReadBuffer((uint)IOHandler.bufferSize);
                    var arr = buffer.ToArray();
                    writeLocation.Write(arr, 0, arr.Length);

                    //using (var bufStream = buffer.AsStream())
                    //{
                    //    bufStream.CopyTo(writeLocation);
                    //}
                }

            }
            catch (Exception)
            {
                throw;
            }

            DisplayPasteData(packet, writeLocation);
            // notify the file is done

        }

        public void DisplayPasteData(PacketInfo packet, Stream writeLocation)
        {

        }

    }
}
