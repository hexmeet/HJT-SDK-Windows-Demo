#if AUTOTEST

using EasyVideoWin.Www.Api.Rest.DataStruct;
using EasyVideoWin.Www.Utils;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using static EasyVideoWin.Www.Api.Rest.DataStruct.Constants;

namespace EasyVideoWin.Www.Api.Rest.Resource
{
    public class BaseApiController : Controller
    {
        public ActionResult GetErrInfo(ErrorCode errCode)
        {
            this.Response.StatusCode = (int)HttpStatusCode.BadRequest;
            var errInfo = new ErrorInfo()
            {
                errorCode = (int)errCode,
                errorMessage = errCode.GetDescription()
            };
            return Json(errInfo);
        }
    }
}

#endif