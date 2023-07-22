using PastingMaui.Data;

namespace PastingMaui.Platforms.Android
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

        public static async Task<PacketInfo> ReadPacketInfo(Stream stream, int bufferSize)
        {
            int infoSize = sizeof(uint) + sizeof(bool);
            byte[] buffer = new byte[infoSize];
            PacketInfo packet = new PacketInfo();
            try
            {
                await stream.ReadAsync(buffer, 0, infoSize);
                packet.isText = BitConverter.ToBoolean(buffer, 0);
                packet.Size = BitConverter.ToUInt32(buffer, sizeof(bool));

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
            BitConverter.GetBytes(Size).CopyTo(buffer, sizeof(int));
            return sizeof(int) + sizeof(uint);
        }

        public static void SendPacketInfo()
        {

        }

        private PacketInfo() { }
    }
}
