#if AUTOTEST

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasyVideoWin.Www.Api.Rest.DataStruct
{
    public class CallRequest
    {
        public string connId { get; set; }
        public string displayName { get; set; }
        public string callee { get; set; }
        public int callRate { get; set; }
        public string callType { get; set; }
        public string password { get; set; }
    }
}

#endif