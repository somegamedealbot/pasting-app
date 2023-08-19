using PastingMaui.Data;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Storage.Streams;

namespace PastingMaui.Platforms.Windows
{
    public class PacketInfo : BasePacketInfo
    {

        public uint Size
        {
            get; private set;
        }

        public bool IsText
        {
            get; private set;
        }

        public void setPacketSize(uint size)
        {
            Size = size;
        }

        public void setIsText(bool text)
        {
            isText = text;
        }

        public static async Task<PacketInfo> ReadPacketInfo(DataReader reader)
        {
            int infoSize = sizeof(uint) + sizeof(bool);
            PacketInfo packet = new();

            try
            {
                await reader.LoadAsync((uint)infoSize);
                var buffer = reader.ReadBuffer((uint)infoSize).ToArray();
                packet.IsText = BitConverter.ToBoolean(buffer, 0);
                packet.Size = BitConverter.ToUInt32(buffer, 1);

            }
            catch (Exception)
            {
                throw;
            }

            return packet;
        }

        public static PacketInfo SetPacketInfo(uint size, bool isText)
        {
            PacketInfo packetInfo = new PacketInfo();
            packetInfo.Size = size;
            packetInfo.IsText = isText;
            return packetInfo;
        }

        public int SetupBuffer(byte[] buffer)
        {
            BitConverter.GetBytes(IsText).CopyTo(buffer, 0);
            BitConverter.GetBytes(Size).CopyTo(buffer, sizeof(bool));
            return sizeof(bool) + sizeof(uint);
        }

        public static void SendPacketInfo()
        {

        }

        private PacketInfo() { }
    }
}
