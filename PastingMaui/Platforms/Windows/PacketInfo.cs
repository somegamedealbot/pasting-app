using PastingMaui.Data;
using Windows.Storage.Streams;

namespace PastingMaui.Platforms.Android
{
    internal class PacketInfo : BasePacketInfo
    {

        public uint Size
        {
            get; private set;
        }

        public bool IsText
        {
            get; private set;
        }

        public static async Task<PacketInfo> ReadPacketInfo(DataReader reader)
        {
            int infoSize = sizeof(uint) + sizeof(bool);
            PacketInfo packet = new PacketInfo();

            try
            {
                await reader.LoadAsync((uint)infoSize);
                packet.IsText = reader.ReadBoolean(); // type of info
                packet.Size = reader.ReadUInt32();

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

        public static void SendPacketInfo()
        {

        }

        private PacketInfo() { }
    }
}
