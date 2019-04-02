#if AUTOTEST

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasyVideoWin.Www.Api.Rest.DataStruct
{
    public class CallStatistics
    {
        public MediaStatistics media_statistics { get; set; }
        public SignalStatistics signal_statistics { get; set; }
    }
}

#endif