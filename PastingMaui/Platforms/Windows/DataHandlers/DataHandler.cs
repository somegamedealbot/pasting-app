using PastingMaui.Platforms.Windows;
using PastingMaui.Shared;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using Windows.Storage.Streams;
using System;

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

        public async Task ReceiveData(Stream inStream, PacketInfo packet, Stream writeLocation)
        {
            byte[] buffer = new byte[IOHandler.bufferSize];
            uint totalReadCount = 0;
            int tempCount = 0;
            Paste paste = PastingApp.app.pasteManager.AddPaste(writeLocation);
            int readSize = packet.Size > buffer.Length ? buffer.Length : (int)packet.Size;
            try
            {
                while (packet.Size > totalReadCount && (tempCount += await inStream.ReadAsync(buffer.AsMemory(0, readSize))) != 0)
                {
                    totalReadCount += (uint)tempCount;
                    await writeLocation.WriteAsync(buffer.AsMemory(0, tempCount));
                    uint remaining = packet.Size - totalReadCount;
                    readSize = remaining > buffer.Length ? buffer.Length : (int)remaining;
                    // save text or file here
                }

            }
            catch (Exception)
            {
                throw;
            }

            paste.CompletePaste();

            // call to finished reading this file/txt
            //Task.Run(PastingApp.app)

        }

    }
}
