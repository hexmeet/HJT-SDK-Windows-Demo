#if AUTOTEST

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasyVideoWin.Www.Api.Rest.DataStruct
{
    public class SignalStatistics
    {
        public string call_type { get; set; }
        public uint call_rate { get; set; }
        public int call_index { get; set; }
        
    }
}

#endif