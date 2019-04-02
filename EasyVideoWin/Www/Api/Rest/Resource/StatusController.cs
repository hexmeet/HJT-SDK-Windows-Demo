#if AUTOTEST

using EasyVideoWin.Model;
using EasyVideoWin.Model.CloudModel;
using EasyVideoWin.Www.Api.Rest.DataStruct;
using EasyVideoWin.Www.Api.Rest.Resource;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using static EasyVideoWin.Www.Api.Rest.DataStruct.Constants;

namespace EasyVideoWin.www.api.rest.Resource
{
    public class StatusController : BaseApiController
    {
        [HttpGet(Constants.SVC_STATUS_PATH)]
        public ActionResult GetSvcStatus()
        {
            if (LoginStatus.LoggedIn != LoginManager.Instance.CurrentLoginStatus)
            {
                return GetErrInfo(Constants.ErrorCode.NOT_LOGGED_IN);
            }
            
            return Json(
                new SvcStatus()
                {
                    serverAddress   = CloudApiManager.Instance.DoradoZoneAddress,
                    status          = LoginManager.Instance.IsRegistered ? "REGISTERED" : "UNREGISTERED"
                }
            );
        }
        
    }
}

#endif