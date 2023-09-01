using PastingMaui.Data;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Storage.Streams;
using WinRT;

namespace PastingMaui.Platforms.Windows
{
    public class PacketInfo : BasePacketInfo
    {

        private PacketInfo() { }

        public static async Task<PacketInfo> ReadPacketInfo(DataReader reader)
        {
            int infoSize = sizeof(uint) + sizeof(bool) + sizeof(int);
            PacketInfo packet = new();

            try
            {
                await reader.LoadAsync((uint)infoSize);
                var buffer = reader.ReadBuffer((uint)infoSize).ToArray();
                int byteCount = 0;
                packet.IsText = BitConverter.ToBoolean(buffer, 0);
                byteCount += sizeof(bool);
                packet.Size = BitConverter.ToUInt32(buffer, byteCount);
                byteCount += sizeof(uint);

                var fileNameSize = BitConverter.ToInt32(buffer, byteCount);
                byteCount += sizeof(int);

                if (fileNameSize > 0)
                {
                    await reader.LoadAsync((uint)fileNameSize);
                    var fileNameBuffer = reader.ReadBuffer((uint)fileNameSize).ToArray();
                    packet.FileName = BitConverter.ToString(buffer, byteCount, fileNameSize);
                }

            }
            catch (Exception)
            {
                throw;
            }

            return packet;
        }

        public static PacketInfo SetPacketInfo(uint size, bool isText, string fileName)
        {
            var packet = SetPacketInfo(new PacketInfo(), size, isText, fileName).As<PacketInfo>();
            return packet;
        }

        public new int SetupBuffer(byte[] buffer)
        {
            return base.SetupBuffer(buffer);
        }

    }
}
