#if AUTOTEST

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasyVideoWin.Www.Api.Rest.DataStruct
{
    public class StatisticsInfo
    {
        public CallStatistics statistics { get; set; }
        public string errorMessage { get; set; }
        public int errorCode { get; set; }
    }
}

#endif