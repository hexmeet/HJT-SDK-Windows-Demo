#if AUTOTEST

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasyVideoWin.Www.Api.Rest.DataStruct
{
    public class AnonymousJoinConfRequest
    {
        public string serverAddress { get; set; }
        public string displayName { get; set; }
        public string callee { get; set; }
        public string password { get; set; }
        public bool turnOffCamera { get; set; }
        public bool turnOffMicrophone { get; set; }
    }
}

#endif