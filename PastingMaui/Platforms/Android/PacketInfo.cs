using PastingMaui.Data;
using static System.Net.Mime.MediaTypeNames;
using System;
using Microsoft.Maui.Controls.PlatformConfiguration;
using System.Text;

namespace PastingMaui.Platforms.Android
{
    public class PacketInfo : BasePacketInfo
    {

        public static async Task<PacketInfo> ReadPacketInfo(Stream stream, int bufferSize)
        {
            int infoSize = sizeof(uint) + sizeof(bool) + sizeof(int);
            byte[] buffer = new byte[infoSize];
            PacketInfo packet = new();

            try
            {
                await stream.ReadAsync(buffer.AsMemory(0, infoSize));
                //await stream.ReadAsync(buffer.AsMemory(0, infoSize));
                int byteCount = 0;

                packet.IsText = BitConverter.ToBoolean(buffer, 0);
                byteCount += sizeof(bool);
                packet.Size = BitConverter.ToUInt32(buffer, 1);
                byteCount += sizeof(uint);

                var fileNameSize = BitConverter.ToInt32(buffer, byteCount);
                byteCount += sizeof(int);

                if (fileNameSize > 0)
                {
                    byte[] fileNameBuffer = new byte[fileNameSize];
                    await stream.ReadAsync(fileNameBuffer.AsMemory(0, fileNameSize));
                    packet.FileName = Encoding.UTF8.GetString(fileNameBuffer);
                }

            }
            catch (Exception)
            {
                throw;
            }

            return packet;
        }

        public static new PacketInfo SetPacketInfo(uint size, bool isText, string fileName)
        {
            return (PacketInfo)SetPacketInfo(new PacketInfo(), size, isText, fileName);
        }

        public new int SetupBuffer(byte[] buffer)
        {
            return base.SetupBuffer(buffer);
        }

        private PacketInfo() { }
    }
}
