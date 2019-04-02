#if AUTOTEST

using EasyVideoWin.Model;
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
    public class SettingController : BaseApiController
    {
        [HttpGet(Constants.CALL_SETTING_PATH)]
        public ActionResult GetCallSetting()
        {
            if (LoginStatus.LoggedIn != LoginManager.Instance.CurrentLoginStatus)
            {
                return GetErrInfo(Constants.ErrorCode.NOT_LOGGED_IN);
            }
            
            return Json(
                new CallSetting()
                {
                    callRate = SettingManager.Instance.GetCurrentCallRate(),
                    autoAnswer = Helpers.Utils.GetAutoAnswer(),
                    encryptionMode = "",
                    signalingPreference = ""
                }
            );
        }

        [HttpPut(Constants.CALL_SETTING_PATH)]
        public ActionResult SetCallSetting([FromBody] CallSetting callSetting)
        {
            if (LoginStatus.LoggedIn != LoginManager.Instance.CurrentLoginStatus)
            {
                return GetErrInfo(Constants.ErrorCode.NOT_LOGGED_IN);
            }

            Helpers.Utils.SetAutoAnswer(callSetting.autoAnswer);
            SettingManager.Instance.SetCallRate(callSetting.callRate);
            return Content("");
        }
    }
}

#endif