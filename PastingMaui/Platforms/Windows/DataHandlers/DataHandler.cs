using PastingMaui.Platforms.Windows;
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

            PacketInfo packet = PacketInfo.SetPacketInfo((uint)convertedStream.Length, true, null);


            if (IOHandler == null)
            {
                throw new Exception("null IOHandler");
            }
            else
            {
                IOHandler.WriteStreamTo(packet, convertedStream);
            }

        }

        public async Task SendFileData(FileStream fileStream, string fileName)
        {
            if (fileStream.Length > uint.MaxValue)
            {
                throw new Exception("File too big");
            }

            PacketInfo packet = PacketInfo.SetPacketInfo((uint)fileStream.Length, false, fileName);

            if (IOHandler == null)
            {
                throw new Exception("null IOHandler");
            }
            else
            {
                IOHandler.WriteStreamTo(packet, fileStream);
            }
        }

        public async Task ReceiveData(DataReader reader, PacketInfo packet, Stream writeLocation)
        {
            IBuffer buffer;
            uint remainingCount = packet.Size;
            Paste paste = PastingApp.app.pasteManager.AddPaste(writeLocation, packet);

            uint readSize = (uint)(IOHandler.bufferSize > packet.Size ? (int)packet.Size: IOHandler.bufferSize);

            try
            {
                while (remainingCount > 0) {
                    var readCount = await reader.LoadAsync(readSize);
                    remainingCount -= readCount;
                    buffer = reader.ReadBuffer(readSize);
                    var arr = buffer.ToArray();
                    writeLocation.Write(arr, 0, arr.Length);

                    readSize = IOHandler.bufferSize > remainingCount ? remainingCount : (uint)IOHandler.bufferSize;

                }

            }
            catch (Exception)
            {
                paste.InCompletePaste();
                //packet.Dispose()
                throw;
            }

            paste.CompletePaste();
            // notify the file is done

        }

    }
}
