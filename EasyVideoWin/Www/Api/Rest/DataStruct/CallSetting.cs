#if AUTOTEST

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasyVideoWin.Www.Api.Rest.DataStruct
{
    public class CallSetting
    {
        public uint callRate { get; set; }
        public bool autoAnswer { get; set; }
        public string encryptionMode { get; set; }
        public string signalingPreference { get; set; }
    }
}

#endif