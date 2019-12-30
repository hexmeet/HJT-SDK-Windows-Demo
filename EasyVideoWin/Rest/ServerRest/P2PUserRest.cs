using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasyVideoWin.Rest.ServerRest
{
    public class P2PUserRest
    {
        public long userId { get; set; }
        public string displayName { get; set; }
        public string imageUrl { get; set; }
        public int type { get; set; }
    }
}
