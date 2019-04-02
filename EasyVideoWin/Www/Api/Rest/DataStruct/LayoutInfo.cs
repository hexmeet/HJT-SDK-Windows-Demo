#if AUTOTEST

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasyVideoWin.Www.Api.Rest.DataStruct
{
    public class LayoutInfo
    {
        public List<int> windowIdx { get; set; }
        public string layoutMode { get; set; }
        public string speakerName { get; set; }
        public List<string> svcDeviceIds { get; set; }
        public List<string> svcSuit { get; set; }
        public bool isOnlyLocal { get; set; }
    }
}

#endif