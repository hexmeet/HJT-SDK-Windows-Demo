using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasyVideoWin.Model.CloudModel.CloudRest
{
    public class ErrorMessageRest
    {
        public static int ERR_INVALID_TOKEN = 1001;

        public int errorCode { get; set; }
        public string errorInfo { get; set; }
        public List<string> args { get; set; }
    }
}
