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
    public class AuthenticationController : BaseApiController
    {
        [HttpPost(Constants.LOGIN_PATH)]
        public ActionResult Login([FromBody] LoginRequest loginRequest)
        {
            if (
                   LoginStatus.LoggedIn == LoginManager.Instance.CurrentLoginStatus
                || LoginStatus.LoggingIn == LoginManager.Instance.CurrentLoginStatus
                || LoginStatus.AnonymousLoggedIn == LoginManager.Instance.CurrentLoginStatus
                || LoginStatus.AnonymousLoggingIn == LoginManager.Instance.CurrentLoginStatus
            )
            {
                return GetErrInfo(Constants.ErrorCode.DEVICE_IN_USE);
            }

            if (LoginStatus.LoginFailed == LoginManager.Instance.CurrentLoginStatus)
            {
                LoginManager.Instance.CurrentLoginStatus = LoginStatus.NotLogin;
            }

            if (!LoginManager.Instance.Login(loginRequest.loginName, loginRequest.loginPassword, loginRequest.serverAddress))
            {
                return GetErrInfo(Constants.ErrorCode.OPERATION_FAILED_AND_UI_PROMPT_REASON);
            }

            LoginManager.Instance.LoginResetEvent.WaitOne();

            if (LoginStatus.LoggedIn != LoginManager.Instance.CurrentLoginStatus)
            {
                return GetErrInfo(Constants.ErrorCode.OPERATION_FAILED_AND_UI_PROMPT_REASON);
            }

            return Json(
                new LoginResult()
                {
                    token       = LoginManager.Instance.LoginToken,
                    connId      = "",
                    deviceName  = LoginManager.Instance.DisplayName
                }
            );
        }

        [HttpPut(Constants.LOGOUT_PATH)]
        public ActionResult Logout()
        {
            if (LoginStatus.LoggedIn != LoginManager.Instance.CurrentLoginStatus)
            {
                return GetErrInfo(Constants.ErrorCode.NOT_LOGGED_IN);
            }

            LoginManager.Instance.Logout();
            return Content("");
        }
    }
}

#endif