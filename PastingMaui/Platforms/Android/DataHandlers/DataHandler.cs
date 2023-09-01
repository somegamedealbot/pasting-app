using PastingMaui.Platforms.Android;
using PastingMaui.Shared;
using System.Text;
using static PastingMaui.Platforms.PastingApp;
using System;

namespace PastingMaui.Platforms.Windows.DataHandlers
{
    public class DataHandler
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

            PacketInfo packet = PacketInfo.SetPacketInfo((uint)clipboardData.Length, true, null);


            if (IOHandler == null)
            {
                throw new Exception("null IOHandler");
            }
            else
            {
                IOHandler.WriteStreamTo(packet, convertedStream);

                // after done notify here
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

        public async Task ReceiveData(Stream inStream, PacketInfo packet, Stream writeLocation)
        {
            byte[] buffer = new byte[IOHandler.bufferSize];
            uint totalReadCount = 0;
            int tempCount = 0;
            Paste paste = PastingApp.app.pasteManager.AddPaste(writeLocation, packet);
            int readSize = packet.Size > buffer.Length ? buffer.Length : (int)packet.Size;
            try
            {
                while (packet.Size > totalReadCount && (tempCount = await inStream.ReadAsync(buffer.AsMemory(0, readSize))) != 0)
                {
                    totalReadCount += (uint)tempCount;
                    await writeLocation.WriteAsync(buffer.AsMemory(0, tempCount));
                    uint remaining = packet.Size - totalReadCount;
                    readSize = remaining > IOHandler.bufferSize ? IOHandler.bufferSize : (int)remaining;
                    // save text or file here
                }

            }
            catch (Exception)
            {
                paste.InCompletePaste();
                //packet.Dispose()
                throw;
            }

            paste.CompletePaste();

            // call to finished reading this file/txt
            //Task.Run(PastingApp.app)

        }

    }
}
