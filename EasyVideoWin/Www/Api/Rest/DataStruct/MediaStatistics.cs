#if AUTOTEST

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasyVideoWin.Www.Api.Rest.DataStruct
{
    public class MediaStatistics
    {
        public List<StreamStatus> ar { get; set; }
        public List<StreamStatus> @as { get; set; }
        public List<StreamStatus> cr { get; set; }
        public List<StreamStatus> cs { get; set; }
        public List<StreamStatus> pr { get; set; }
        public List<StreamStatus> ps { get; set; }
    }
}

#endif