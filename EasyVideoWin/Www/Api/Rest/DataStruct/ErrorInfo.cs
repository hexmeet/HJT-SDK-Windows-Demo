#if AUTOTEST

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasyVideoWin.Www.Api.Rest.DataStruct
{
    public class ErrorInfo
    {
        public int errorCode { get; set; }
        public string errorMessage { get; set; }
    }
}

#endif