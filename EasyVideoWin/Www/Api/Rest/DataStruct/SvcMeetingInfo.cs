#if AUTOTEST

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasyVideoWin.Www.Api.Rest.DataStruct
{
    public class SvcMeetingInfo
    {
        public string deviceId { get; set; }
        public List<string> micMutedParticipants { get; set; }
        public LayoutInfo layoutInfo { get; set; }
        public bool isSvcLayoutEnable { get; set; }
        public int gallerySpeakerIndex { get; set; }
        public bool muteByMru { get; set; }
        public int svcReg { get; set; }
        public int wsConnect { get; set; }
    }
}

#endif