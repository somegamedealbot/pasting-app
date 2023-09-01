using Microsoft.Maui.Storage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PastingMaui.Data
{
    public class BasePacketInfo
    {

        public uint Size
        {
            get; protected set;
        }

        public bool IsText
        {
            get; protected set;
        }

        public string FileName
        {
            get; protected set;
        }

        public string WriteLocation
        {
            get; set;
        }

        public static BasePacketInfo SetPacketInfo(BasePacketInfo packetInfo, uint size, bool isText, string fileName)
        {
            packetInfo.Size = size;
            packetInfo.IsText = isText;
            packetInfo.FileName = fileName;
            return packetInfo;
        }

        public int SetupBuffer(byte[] buffer)
        {
            int byteCount = 0;
            BitConverter.GetBytes(IsText).CopyTo(buffer, 0);
            BitConverter.GetBytes(Size).CopyTo(buffer, sizeof(bool));
            byteCount += sizeof(bool) + sizeof(uint);

            if (FileName != null)
            {
                BitConverter.GetBytes(FileName.Length).CopyTo(buffer, byteCount);
                byteCount += sizeof(int);

                var charsInBytes = Encoding.UTF8.GetBytes(FileName.ToCharArray());
                charsInBytes.CopyTo(buffer, byteCount);
                byteCount += charsInBytes.Length;
            }
            else
            {
                BitConverter.GetBytes(0).CopyTo(buffer, byteCount);
                byteCount += sizeof(int);
            }

            return byteCount;

        }

    }
}
