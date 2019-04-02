#if AUTOTEST

using EasyVideoWin.Www.Utils;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasyVideoWin.Www.Api.Rest.DataStruct
{
    public class Constants
    {
        public const string LOGIN_PATH                  = "/api/rest/login";
        public const string LOGOUT_PATH                 = "/api/rest/logout";
        public const string MAKE_CALL_PATH              = "/api/rest/system/make_call";
        public const string DROP_CALL_PATH              = "/api/rest/system/drop_call";
        public const string ANONYMOUS_JOIN_CONF_PATH    = "/api/rest/system/anonymous_join_conf";
        public const string CALL_SETTING_PATH           = "/api/rest/system/call_setting";
        public const string SVC_STATUS_PATH             = "/api/rest/svc/status";
        public const string CALL_STATISTICS_PATH        = "/api/rest/system/get_statistics";
        public const string AUDIO_MUTE_PATH             = "/api/rest/system/toggle_mute";
        public const string SVC_LAYOUT_MODE_PATH        = "/api/rest/system/set_svc_layout_mode";
        public const string HAND_UP_PATH                = "/api/rest/system/hand_up";
        public const string SVC_MEETING_PATH            = "/api/rest/system/svc_meeting";
        public const string START_WHITEBOARD            = "/api/rest/system/whiteboard/start";
        public const string STOP_WHITEBOARD             = "/api/rest/system/whiteboard/stop";
        public const string START_CONTENT               = "/api/rest/system/content/start";
        public const string STOP_CONTENT                = "/api/rest/system/content/stop";

        public enum ErrorCode
        {
            [Description("Success")]
            OK = 0,
            [Description("Failed to operate and UI has prompt the reason.")]
            OPERATION_FAILED_AND_UI_PROMPT_REASON = 4000,
            [Description("The device is in use, please try later.")]
            DEVICE_IN_USE = 4001,
            [Description("The device is not logged in.")]
            NOT_LOGGED_IN = 4002,
            [Description("There is no conference.")]
            NO_CONFERENCE = 4003
        }

        
    }
}

#endif