#if AUTOTEST

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasyVideoWin.Www.Api.Rest.DataStruct
{
    public class LoginResult
    {
        public string token { get; set; }
        public string connId { get; set; }
        public string deviceName { get; set; }
    }
}

#endif