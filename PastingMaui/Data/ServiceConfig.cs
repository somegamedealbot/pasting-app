using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PastingMaui.Data
{
    public static class ServiceConfig
    {
        public static readonly string serviceUuidString = "6d191666-d987-4358-b547-85709795d965";
        public static Guid serviceUuid = Guid.Parse(serviceUuidString);
        public const UInt16 sdpServiceAttributeId = 0x100;
        public const byte SdpServiceNameAttributeType = (4 << 3) | 5;
        public static readonly string SdpServiceName = "Pasting";
        public static int typeSize = 1;
        public static int packetSize = sizeof(UInt32);

    }
}
