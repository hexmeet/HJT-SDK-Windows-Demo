using System;

namespace EasyVideoWin.Model.CloudModel.CloudRest
{
    public class AcsRest
    {
        public String acsJsonWebToken { get; set; }
        public String internalEdgeIp { get; set; }
        public String externalEdgeIp { get; set; }

        public string toString()
        {
            return string.Format("acsJsonWebToken:{0}, interEdgeIp:{1}, externalEdgeIp:{2}",
                acsJsonWebToken, internalEdgeIp, externalEdgeIp );
        }
    }
}
