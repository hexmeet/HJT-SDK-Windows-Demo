#if AUTOTEST

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasyVideoWin.Www.Api.Rest.DataStruct
{
    public class AudioMute
    {
        public string connId { get; set; }
        public bool muteOn { get; set; }
    }
}

#endif