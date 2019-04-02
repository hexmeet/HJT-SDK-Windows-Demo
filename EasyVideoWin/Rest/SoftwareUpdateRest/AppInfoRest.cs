using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasyVideoWin.Rest.SoftwareUpdateRest
{
    public class AppInfoRest
    {
        public string VERSION { get; set; }
        public string DOWNLOAD_URL { get; set; }
        public string CLOUD_API_SERVER_ADDR { get; set; }
        public string CLOUD_API_SERVER_PROTOCOL { get; set; }
        public int CLOUD_API_SERVER_PORT { get; set; }
    }
}
