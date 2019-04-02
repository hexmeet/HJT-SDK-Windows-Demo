#if AUTOTEST

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasyVideoWin.Www.Api.Rest.DataStruct
{
    public class StreamStatus
    {
        public string codec { get; set; }
        public string resolution { get; set; }
        public string pipeName { get; set; }
        public ulong packetLost { get; set; }
        public float packetLostRate { get; set; }
        public int jitter { get; set; }
        public int frameRate { get; set; }
        public bool encrypted { get; set; }
        public int rtp_actualBitRate { get; set; }
        public int rtp_settingBitRate { get; set; }
        public ulong totalPacket { get; set; }
    }
}

#endif