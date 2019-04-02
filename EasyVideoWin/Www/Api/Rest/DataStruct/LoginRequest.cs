#if AUTOTEST

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasyVideoWin.Www.Api.Rest.DataStruct
{
    public class LoginRequest
    {
        public string serverAddress { get; set; }
        public string loginName { get; set; }
        public string loginPassword { get; set; }
    }
}

#endif