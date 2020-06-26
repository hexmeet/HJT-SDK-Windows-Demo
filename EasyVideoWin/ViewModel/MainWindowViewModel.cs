using EasyVideoWin.CustomControls;
using EasyVideoWin.Helpers;
using EasyVideoWin.HttpUtils;
using EasyVideoWin.Model;
using EasyVideoWin.View;
using log4net;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace EasyVideoWin.ViewModel
{
    class MainWindowViewModel : DialOutModelBase
    {
        private readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        static extern EXECUTION_STATE SetThreadExecutionState(EXECUTION_STATE esFlags);

        private System.Timers.Timer _timerCheckSystemStatus;

        private const int EV_CALL_BYE_EP_NO_PACKET_RECEIVED = 11;       // ep can not receive stream from mru and teminate after 30 second
        private const int EV_CALL_BYE_MRU_NORMAL = 100;                 // conf is ended
        private const int EV_CALL_BYE_MRU_OPERATOR_DISCONNECT = 101;    // conf is ended or hung up by host 会议结束或被主持人挂断
        private const int EV_CALL_BYE_MRU_NO_PACKET_RECEIVED = 102;     // mru can not receive stream from ep and teminate after 30 second
        
        private static Dictionary<int, string> _sdkSelfErrorInfo = new Dictionary<int, string>
        {
            //{0,      "OK"}
            {(int)ManagedEVSdk.ErrorInfo.EV_ERROR_CLI.EV_NG,                    "SDK_NOT_GOOD"}
            , {(int)ManagedEVSdk.ErrorInfo.EV_ERROR_CLI.EV_UNINITIALIZED,       "SDK_UNINITIALIZED"}
            , {(int)ManagedEVSdk.ErrorInfo.EV_ERROR_CLI.EV_BAD_FORMAT,          "SDK_BAD_FORMAT"}
            , {(int)ManagedEVSdk.ErrorInfo.EV_ERROR_CLI.EV_NOT_IN_CONF,         "SDK_NOT_IN_CONF"}
            , {(int)ManagedEVSdk.ErrorInfo.EV_ERROR_CLI.EV_BAD_PARAM,           "SDK_BAD_PARAM"}
            //, {6,   "SDK_REGISTER_FAILED"}
            , {(int)ManagedEVSdk.ErrorInfo.EV_ERROR_CLI.EV_INTERNAL_ERROR,      "SDK_INTERNAL_ERROR"}
            , {(int)ManagedEVSdk.ErrorInfo.EV_ERROR_CLI.EV_SERVER_UNREACHABLE,  "SERVER_UNREACHABLE"}
            , {(int)ManagedEVSdk.ErrorInfo.EV_ERROR_CLI.EV_SERVER_INVALID,      "INVALID_SERVER"}
        };

        private static Dictionary<int, string> _locationErrorInfo = new Dictionary<int, string>
        {
            //{10000,     "LOCATION_FAILED_LOG_IN"}
            //, {10001,   "LOCATION_FAILED_LOG_IN"}
            //, {10002,   "LOCATION_FAILED_LOG_IN"}
            //, {10003,   "LOCATION_FAILED_LOG_IN"}
            //, {10004,   "LOCATION_FAILED_LOG_IN"}
            //, {10005,   "LOCATION_FAILED_LOG_IN"}
            //, {10006,   "LOCATION_FAILED_LOG_IN"}
            {10007,   "LOCATION_INVALID_USER_NAME_PASSWORD"}
            , {10008,   "LOCATION_INVALID_CONFERENCE_ID"}
            //, {10009,   "LOCATION_FAILED_LOG_IN"}
        };

        private static Dictionary<int, string> _callErrorInfo = new Dictionary<int, string>
        {
            {2999,      "SERVICE_EXCEPTION"}
            , {11,      "NETWORK_UNAVAILABLE"}
            , {100,     "MEETING_ENDED"}
            , {101,     "MEETING_ENDED"}
            , {102,     "NETWORK_UNAVAILABLE"}
            , {1001,    "INVALID_NUMERICID"}
            , {1003,    "INVALID_USERNAME"}
            , {1005,    "INVALID_USERID"}
            , {1007,    "INVALID_DEVICEID"}
            , {1009,    "INVALID_ENDPOINT"}
            , {2001,    "SERVER_UNLICENSED"}
            , {2002,    "SERVER_LICENSE_EXPIRED"}
            , {2003,    "NOT_FOUND_SUITABLE_MRU"}
            , {2005,    "NEITHER_TEMPLATE_NOR_ONGOING_NOR_BINDED_ROOM"}
            , {2007,    "LOCK_TIMEOUT"}
            , {2009,    "TEMPLATE_CONF_WITHOUT_CONFROOM"}
            , {2011,    "ROOM_EXPIRED"}
            , {2013,    "ROOM_COMPLETELY_FULL"}
            //, {2015,    "INVALID_PASSWORD"}
            , {2017,    "NO_TIME_SPACE_TO_ACTIVATE_ROOM"}
            , {2019,    "NOT_FOUND_SIGNALING_IP"}
            , {2021,    "MIXED_MRU_COMPLETELY_FULL"}
            , {2023,    "CONF_PORT_COUNT_USED_UP"}
            , {2024,    "ORG_PORT_COUNT_USED_UP"}
            , {2025,    "HAISHEN_PORT_COUNT_USED_UP"}
            , {2027,    "EXCEEDED_GATEWAY_AUDIO_PORT_COUNT_IN_LICENSE"}
            , {2029,    "EXCEEDED_GATEWAY_VIDEO_PORT_COUNT_IN_LICENSE"}
            , {2031,    "ONLY_ROOM_OWNER_CAN_ACTIVATE_ROOM"}
            , {2033,    "NOT_ALLOW_ANONYMOUS_PARTY"}
            //, {2043,    "LOCAL_ZONE_NOT_STARTED"} do not prompt the error
            //, {2045,    "LOCAL_ZONE_STOPPED"} do not prompt the error
            , {4049,    "CALL_FAILED_FOR_PARTY_IN_CALL"}
            , {4051,    "PARTY_OFFLINE"}
            , {4055,    "FAILED_JOIN_CONF_FOR_LOCKED"}
            , {4057,    "CALL_ROOM_BUSY"}
        };

        public static readonly int SERVER_ERROR_LOGIN_FAILED_MORE_THAN_5_TIMES = 1101;
        private static Dictionary<int, string> _serverErrorInfo = new Dictionary<int, string>
        {
            {1000,      "API_VERSION_NOT_SUPPORTED"}
            , {1001,    "INVALID_TOKEN"}
            , {1002,    "INVALID_PARAMETER"}
            , {1003,    "INVALID_DEVICESN"}
            , {1004,    "INVALID_MEDIA_TYPE"}
            , {1005,    "PERMISSION_DENIED"}
            , {1006,    "WRONG_FIELD_NAME"}
            , {1007,    "INTERNAL_SYSTEM_ERROR"}
            , {1008,    "OPERATION_FAILED"}
            , {1009,    "GET_FAILED"}
            , {1010,    "NOT_SUPPORTED"}
            , {1011,    "REDIS_LOCK_TIMEOUT"}
            //, {1019,    "LOCAL_ZONE_STOPPED"} do not prompt the error
            , {1100,    "INVALID_USER"}
            , {1101,    "LOGIN_FAILED_MORE_THAN_5_TIMES"}
            , {1102,    "ACCOUNT_TEMPORARILY_LOCKED"}
            , {1103,    "ACCOUNT_DISABLED"}
            , {1104,    "NO_USERNAME"}
            , {1105,    "EMAIL_MISMATCH"}
            , {1106,    "COMPANY_ADMINISTRATOR_NOT_IN_ANY_COMPANY"}
            , {1112,    "NOT_AVAILABLE_FOR_ADMINISTRATORS"}
            , {1200,    "FILE_UPLOAD_FAILED"}
            , {1201,    "INVALID_LICENSE"}
            , {1202,    "INVALID_IMPORT_USER_FILE"}
            , {1300,    "INVALID_TIME_SERVICE_ADDRESS"}
            , {1301,    "FAILED_UPDATE_SYSTEM_PROPERTIES"}
            , {1400,    "CONF_NOT_EXISTS"}
            , {1401,    "NUMERICID_CONFLICTS"}
            , {1402,    "CONF_UPDATING_IN_PROGRESS"}
            , {1403,    "CONF_DELETING_IN_PROGRESS"}
            , {1404,    "CONF_TERMINATING_IN_PROGRESS"}
            , {1405,    "CONF_LAUNCHING_IN_PROGRESS"}
            , {1406,    "CONF_NOT_IN_APPROVED_STATUS"}
            , {1407,    "CONF_NUMERICID_ONGOING"}
            , {1409,    "CONF_NOT_APPROVED_OR_ONGOING"}
            , {1410,    "PARTICIPANT_NOT_EXISTS_IN_CONF"}
            , {1412,    "NUMERICID_ALREADY_IN_USE"}
            , {1415,    "INVALID_CONF_TIME"}
            , {1418,    "INVALID_CONF_ID"}
            , {1421,    "NOT_FOUND_SUITABLE_MRU"}
            , {1422,    "NOT_FOUND_SUITABLE_GATEWAY"}
            , {1424,    "FAILED_TO_CONNECT_MRU"}
            , {1427,    "NOT_ALLOW_DUPLICATED_NAME"}
            , {1430,    "NOT_FOUND_CONF_IN_REDIS"}
            , {1431,    "NOT_IN_LECTURER_MODE"}
            , {1433,    "FAILED_TO_MUTE_ALL_PARTICIPANTS"}
            , {1436,    "FAILED_TO_CONNECT_PARTICIPANT"}
            , {1439,    "FAILED_TO_DISCONNECT_PARTICIPANT"}
            , {1442,    "FAILED_TO_CHANGE_LAYOUT"}
            , {1445,    "FAILED_TO_SET_SUBTITLE"}
            , {1448,    "FAILED_TO_MUTE_PARTICIPANT_AUDIO"}
            , {1451,    "FAILED_TO_DELETE_PARTICIPANT"}
            , {1454,    "FAILED_TO_INVITE_AVC_ENDPOINT"}
            , {1455,    "FAILED_TO_INVITE_SVC_ENDPOINTS"}
            , {1456,    "CONF_ROOM_COMPLETELY_FULL"}
            , {1457,    "TIMEOUT_TO_GENERATE_NUMERICID"}
            , {1460,    "NOT_FOUND_PROFILE_NAMED_SVC"}
            , {1463,    "FAILED_TO_PROLONG_CONF"}

            , {1500,    "INVALID_MEETING_CONTROL_REQUEST"}
            , {1600,    "NAME_IN_USE"}
            , {1601,    "EMPTY_ENDPOINT_NAME"}
            , {1602,    "EMPTY_ENDPOINT_CALL_MODE"}
            , {1603,    "EMPTY_ENDPOINT_SIP_USERNAME"}
            , {1604,    "EMPTY_ENDPOINT_SIP_PASSWORD"}
            , {1605,    "EMPTY_ENDPOINT_ADDRESS"}
            , {1606,    "INVALID_SIP_USERNAME"}
            , {1607,    "INVALID_IP_ADDRESS"}
            , {1608,    "ENDPOINT_NOT_EXIST"}
            , {1609,    "E164_IN_USE"}
            , {1610,    "ENDPOINT_DEVICE_SN_EXIST"}
            , {1611,    "SIP_USERNAME_REGISTERED"}
            , {1612,    "ENDPOINT_E164_INVALID"}
            , {1613,    "NOT_FOUND_ENDPOINT_DEVICE_SN"}
            , {1614,    "NOT_FOUND_ENDPOINT_PROVISION_TEMPLATE"}
            , {1615,    "DEVICE_SN_EXISTS"}
            , {1700,    "CAN_NOT_DELETE_USER_IN_RESERVED_MEETING"}
            , {1701,    "EMPTY_USER_PASSWORD"}
            , {1702,    "EMPTY_USERNAME"}
            , {1703,    "EMPTY_USER_DISPLAY_NAME"}
            , {1704,    "INVALID_USER_EMAIL"}
            , {1705,    "INVALID_CELLPHONE_NUMBER"}
            , {1706,    "ORIGINAL_PASSWORD_WRONG"}
            , {1707,    "DUPLICATE_EMAIL_NAME"}
            , {1708,    "DUPLICATE_CELLPHONE_NUMBER"}
            , {1709,    "DUPLICATE_USERNAME"}
            , {1710,    "INVALID_CONF_ROOM_MAX_CAPACITY"}
            , {1711,    "SHOULD_ASSIGN_DEPARTMENT_TO_DEPARTMENT_ADMINISTRATOR"}
            , {1712,    "EMPTY_USER_EMAIL"}
            , {1713,    "EMPTY_USER_CELLPHONE_NUMBER"}
            , {1714,    "NOT_ORGANIZATION_ADMINISTRATOR"}
            , {1800,    "COMPANY_NOT_EXIST"}
            , {1801,    "SHORT_NAME_OF_COMPANY_USED"}
            , {1802,    "FULL_NAME_OF_COMPANY_USED"}
            , {1803,    "COMPANY_NOT_EMPTY"}
            , {1804,    "EMPTY_COMPANY_SHORT_NAME"}
            , {1900,    "AGENT_IN_USE"}
            , {1901,    "SHORT_NAME_IN_USE"}
            , {1902,    "FULL_NAME_IN_USE"}
            , {1903,    "AGENT_NOT_EXIST"}
            , {1904,    "AGENT_NOT_EMPTY"}
            , {2000,    "CONF_ROOM_EXPIRED"}
            , {2003,    "NOT_FOUND_SUITABLE_ROOM"}
            , {2006,    "CONF_ROOM_IN_USE"}
            , {2009,    "CONF_ROOM_NUMBER_IN_USE"}
            , {2012,    "CONF_ROOM_CAPACITY_EXCEEDS_LIMIT"}
            , {2015,    "INVALID_CONF_ROOM_CAPACITY"}
            , {2018,    "INVALID_CONF_ROOM_NUMBER"}
            , {2021,    "ROOM_NOT_EXISTS"}
            , {2100,    "CAN_NOT_DELETE_DEPARTMENT_WITH_SUBORDINATE_DEPARTMENT"}
            , {2101,    "CAN_NOT_DELETE_DEPARTMENT_WITH_USERS_OR_ENDPOINTS"}
            , {2200,    "INVALID_ACS_CONFIGURATION"}
            , {2035,    "TRIAL_EXPIRED"}
        };

        [FlagsAttribute]
        public enum EXECUTION_STATE : uint
        {
            ES_AWAYMODE_REQUIRED = 0x00000040,
            ES_CONTINUOUS = 0x80000000,
            ES_DISPLAY_REQUIRED = 0x00000002,
            ES_SYSTEM_REQUIRED = 0x00000001
            // Legacy flag, should not be used.
            // ES_USER_PRESENT = 0x00000004
        }

        private LoginManager _loginMgr = LoginManager.Instance;
        private MainView _mainView = null;
        
        private UserControl _currentView;
        public UserControl CurrentView
        {
            get
            {
                if (null == _currentView)
                {
                    log.Info("Get current view, but the view is null.");
                }
                else
                {
                    log.InfoFormat("Get current view, hash code: {0}", _currentView.GetHashCode());
                }
                return _currentView;
            }
            set
            {
                if (value != _currentView)
                {
                    _currentView = value;
                    if (null == _currentView)
                    {
                        log.Info("Update current view to null");
                    }
                    else
                    {
                        log.InfoFormat("Update current view, hash code: {0}", _currentView.GetHashCode());
                    }
                    
                    OnPropertyChanged("CurrentView");
                }
            }
        }
        
        public MainWindowViewModel()
        {
            log.Info("MainWindowViewModel begin to construct");
            try
            {
                _timerCheckSystemStatus = new System.Timers.Timer();
                _timerCheckSystemStatus.Interval = 3 * 1000;
                _timerCheckSystemStatus.AutoReset = true;
                _timerCheckSystemStatus.Elapsed += TimerCheckSystemStatus_Elapsed;
                _timerCheckSystemStatus.Enabled = true;
            }
            catch(Exception e)
            {
                log.Error(e.Message);
                System.Environment.Exit(1);
            }

            CallController.Instance.CallStatusChanged += OnCallStatusChanged;
            LoginManager.Instance.PropertyChanged += OnLoginMgrPropertyChanged;
            
            if (
                   LoginStatus.LoggedIn == LoginManager.Instance.CurrentLoginStatus
                || LoginStatus.AnonymousLoggedIn == LoginManager.Instance.CurrentLoginStatus
            )
            {
                _mainView = new MainView();
                if (LoginStatus.AnonymousLoggedIn == LoginManager.Instance.CurrentLoginStatus)
                {
                }
                else
                {
                    CurrentView = _mainView;
                }
            }

            EVSdkManager.Instance.EventError += EVSdkWrapper_EventError;
            log.Info("MainWindowViewModel constructed");
        }

        private void EVSdkWrapper_EventError(ManagedEVSdk.Structs.EVErrorCli err)
        {
            log.InfoFormat("EventError, type:{0}, code:{1}, msg:{2}, ation:{3}", err.type, err.code, err.msg, err.action);
            if (
                   (LoginStatus.NotLogin == LoginManager.Instance.CurrentLoginStatus || LoginStatus.LoginFailed == LoginManager.Instance.CurrentLoginStatus)
                && EVSdkManager.ACTION_ONCALLEND != err.action
            )
            {
                log.InfoFormat("Do not show the error message for the login status is {0}. EventError end", LoginManager.Instance.CurrentLoginStatus);
                return;
            }

            if (EVSdkManager.ACTION_UPLOADUSERIMAGE == err.action)
            {
                log.Info("Do not handle error message whose action is uploadUserImage. EventError end");
                return;
            }

            if (LoginStatus.LoggedIn == LoginManager.Instance.CurrentLoginStatus || LoginStatus.AnonymousLoggedIn == LoginManager.Instance.CurrentLoginStatus)
            {
                if (ManagedEVSdk.ErrorInfo.EV_ERROR_TYPE_CLI.EV_ERROR_TYPE_SDK == err.type)
                {
                    log.InfoFormat("Do not handle sdk error, since the login status is:{0}. EventError end", LoginManager.Instance.CurrentLoginStatus);
                    return;
                }

                if (EVSdkManager.ACTION_LOGIN == err.action || EVSdkManager.ACTION_LOGINWITHLOCATION == err.action)
                {
                    log.InfoFormat("Do not handle message:{0}, since the login status is:{1}. EventError end", err.action, LoginManager.Instance.CurrentLoginStatus);
                    return;
                }
            }

            LoginStatus loginStatus = LoginManager.Instance.CurrentLoginStatus;
            if (LoginStatus.LoggingIn == LoginManager.Instance.CurrentLoginStatus || LoginStatus.AnonymousLoggingIn == LoginManager.Instance.CurrentLoginStatus)
            {
                log.InfoFormat("Current login status:{0}. Login failed.", LoginManager.Instance.CurrentLoginStatus);
                LoginManager.Instance.OnLoggingInFailed(LoginStatus.LoggingIn == LoginManager.Instance.CurrentLoginStatus);
            }
            
            string errPrompt = null;
            switch (err.type)
            {
                case ManagedEVSdk.ErrorInfo.EV_ERROR_TYPE_CLI.EV_ERROR_TYPE_SDK:
                    if (_sdkSelfErrorInfo.ContainsKey(err.code))
                    {
                        errPrompt = LanguageUtil.Instance.GetValueByKey(_sdkSelfErrorInfo[err.code]);
                    }
                    break;
                case ManagedEVSdk.ErrorInfo.EV_ERROR_TYPE_CLI.EV_ERROR_TYPE_LOCATE:
                    if (_locationErrorInfo.ContainsKey(err.code))
                    {
                        errPrompt = LanguageUtil.Instance.GetValueByKey(_locationErrorInfo[err.code]);
                    }
                    else
                    {
                        if (10009 == err.code)
                        {
                            if (LoginStatus.AnonymousLoggingIn == loginStatus)
                            {
                                errPrompt = LanguageUtil.Instance.GetValueByKey("INVALID_MEETING_ID");
                            }
                            else if (LoginStatus.LoggingIn == loginStatus)
                            {
                                errPrompt = LanguageUtil.Instance.GetValueByKey("INVALID_USER");
                            }
                            else
                            {
                                errPrompt = string.Format(LanguageUtil.Instance.GetValueByKey("LOCATION_FAILED_LOG_IN"), err.code);
                            }
                        }
                        else
                        {
                            errPrompt = string.Format(LanguageUtil.Instance.GetValueByKey("LOCATION_FAILED_LOG_IN"), err.code);
                        }
                    }
                    break;
                case ManagedEVSdk.ErrorInfo.EV_ERROR_TYPE_CLI.EV_ERROR_TYPE_CALL:
                    if (_callErrorInfo.ContainsKey(err.code))
                    {
                        errPrompt = LanguageUtil.Instance.GetValueByKey(_callErrorInfo[err.code]);
                        if (EV_CALL_BYE_MRU_NORMAL == err.code || EV_CALL_BYE_MRU_OPERATOR_DISCONNECT == err.code || EV_CALL_BYE_EP_NO_PACKET_RECEIVED == err.code || EV_CALL_BYE_MRU_NO_PACKET_RECEIVED == err.code)
                        {
                            if (!CallController.Instance.IsPreviousP2pCall)
                            {
                                errPrompt = CallController.Instance.ConferenceNumber + "\n" + errPrompt;
                            }
                        }
                    }
                    else
                    {
                        if (err.code == (int)ManagedEVSdk.ErrorInfo.EV_CALL_ERROR_CLI.EV_CALL_INVALID_PASSWORD)
                        {
                            log.InfoFormat("Ignore the error code:{0}", err.code);
                            return;
                        }
                        errPrompt = "Received unknown call error, code:" + err.code;
                    }
                    break;
                case ManagedEVSdk.ErrorInfo.EV_ERROR_TYPE_CLI.EV_ERROR_TYPE_SERVER:
                    if (_serverErrorInfo.ContainsKey(err.code))
                    {
                        errPrompt = LanguageUtil.Instance.GetValueByKey(_serverErrorInfo[err.code]);
                        if (SERVER_ERROR_LOGIN_FAILED_MORE_THAN_5_TIMES == err.code && null != err.args && err.args.Length > 0)
                        {
                            errPrompt = string.Format(errPrompt, err.args[0]);
                        }
                    }
                    else
                    {
                        errPrompt = "Received unknown server error, code:" + err.code;
                    }
                    break;
                default:
                    errPrompt = string.Format("SDK error, type:{0}, code:{1}, msg:{2}", err.type, err.code, err.msg);
                    break;
            }
            if (!string.IsNullOrEmpty(errPrompt))
            {
                Application.Current.Dispatcher.InvokeAsync(() => {
                    MainWindow mainWindow = Application.Current.MainWindow as MainWindow;
                    if (null == mainWindow)
                    {
                        log.Info("Can not show error message for main window is null");
                        return;
                    }
                    mainWindow.ShowPromptTip(errPrompt, string.Format("SDK error, type:{0}, code:{1}", err.type, err.code));
                });
            }
            else
            {
                log.Info("Can not show error prompt for the prompt string is empty.");
            }
            log.Info("EventError end");
        }
        
        private void OnCallStatusChanged(object sender, CallStatus status)
        {
            log.Info("OnCallStatusChanged start.");
            UpdateWindowVisibility();
            switch (status)
            {
                case CallStatus.Idle:
                    // do noting. Note: do not remove the branch, or go to default
                    break;
                case CallStatus.Dialing:
                case CallStatus.ConfIncoming:
                case CallStatus.P2pIncoming:
                case CallStatus.P2pOutgoing:
                case CallStatus.Connected:
                    Application.Current.Dispatcher.InvokeAsync(() => {
                        JoinConfDisplayNameWindow.Instance.CloseWindow();
                    });
                    break;
                default: // Ended, PeerDeclined, PeerCancelled, TimeoutSelfCancelled
                    log.Info("Call ended and change current view to main view.");
                    if (LoginStatus.LoggedIn == LoginManager.Instance.CurrentLoginStatus)
                    {
                        CurrentView = _mainView;
                    }

                    Application.Current.Dispatcher.InvokeAsync(() => {
                        log.Info("CallStatus.Ended");
                        // maybe VideoPeopleWindow.Instance.Set2PresettingState() in UpdateWindowVisibility(), so hide LayoutBackgroundWindow in the following
                        if (Visibility.Visible != LayoutBackgroundWindow.Instance.Visibility)
                        {
                            log.Info("LayoutBackgroundWindow is not visible, so OnCallEnded");
                            LayoutBackgroundWindow.Instance.OnCallEnded();
                        }
                        else
                        {
                            LayoutBackgroundWindow.Instance.HideWindow(true);
                        }
                    });

                    SetThreadExecutionState(EXECUTION_STATE.ES_CONTINUOUS);

                    log.Info("Call ended and collect garbage");
                    SystemMonitorUtil.Instance.CollectGarbage();
                    break;
            }

            log.Info("OnCallStatusChanged end.");
        }

        private void OnLoginMgrPropertyChanged(object sender, PropertyChangedEventArgs arg)
        {
            if ("CurrentLoginStatus" == arg.PropertyName)
            {
                UpdateWindowVisibility();
                // To show the main window quickly(especially when network is not fluent), make the operation after loginedIn as below
                LoginStatus status = LoginManager.Instance.CurrentLoginStatus;
                log.DebugFormat("MainWindowViewModel received login status changed message, login status:{0}", status);
                if (LoginStatus.LoggedIn == status || LoginStatus.AnonymousLoggedIn == status)
                {
                    App.Current.Dispatcher.InvokeAsync(() =>
                    {
                        if (_mainView == null)
                        {
                            _mainView = new MainView();
                        }
                        
                        if (LoginStatus.AnonymousLoggedIn == status)
                        {
                            CurrentView = null;
                        }
                        else
                        {
                            CurrentView = _mainView;
                            //string joinConfAddress = Utils.GetAnonymousJoinConfServerAddress();
                            //string joinConfContactId = Utils.GetAnonymousJoinConfContactId();
                            //if (!string.IsNullOrEmpty(joinConfAddress) && !string.IsNullOrEmpty(joinConfContactId))
                            //{
                            //    Utils.SetAnonymousJoinConfServerAddress("");
                            //    Utils.SetAnonymousJoinConfContactId("");
                            //    CallController.Instance.P2pCallPeer(joinConfContactId, null, joinConfContactId);
                            //}
                        }
                    });
                }
                else if (LoginStatus.NotLogin == status)
                {
                    if (Utils.GetAnonymousLogoutAndAnonymousJoinConf())
                    {
                        log.Info("Login status changed to NotLogin and LogoutAndAnonymousJoinConf is true.");
                        string joinConfAddress = Utils.GetAnonymousJoinConfServerAddress();
                        string joinConfId = Utils.GetAnonymousJoinConfId();
                        string joinConfAlias = Utils.GetAnonymousJoinConfAlias();
                        if (!string.IsNullOrEmpty(joinConfAddress) && (!string.IsNullOrEmpty(joinConfId) || !string.IsNullOrEmpty(joinConfAlias)))
                        {
                            log.Info("Login status changed to NotLogin and begin to join conf anonymously.");
                            LoginManager.Instance.SaveCurrentLoginInfo();
                            Application.Current.Dispatcher.InvokeAsync(() => {
                                LoginManager.Instance.ServiceType = Properties.Settings.Default.CloudOnly ? Utils.ServiceTypeEnum.Cloud : Utils.ServiceTypeEnum.Enterprise;
                                LoginManager.Instance.LoginProgress = Properties.Settings.Default.CloudOnly ? LoginProgressEnum.CloudJoinConf : LoginProgressEnum.EnterpriseJoinConf;
                                LoginManager.Instance.IsNeedAnonymousJoinConf = true;
                            });
                        }
                        else
                        {
                            if (CallStatus.Ended == CallController.Instance.CurrentCallStatus)
                            {
                                log.Info("Call ended and need to login.");
                                LoginManager.Instance.ResumePreLoginInfo();
                                LoginManager.Instance.IsNeedRelogin = true;
                            }
                            log.Info("Clear flag for logout and anonymous join conf.");
                            Utils.SetAnonymousLogoutAndAnonymousJoinConf(false);
                        }
                    }
                    //else if (Utils.GetAnonymousLogoutAndLinkP2pCall())
                    //{
                    //    log.Info("Login status changed to NotLogin and GetAnonymousLogoutAndLinkP2pCall is true.");
                    //    string joinConfAddress = Utils.GetAnonymousJoinConfServerAddress();
                    //    string joinConfContactId = Utils.GetAnonymousJoinConfContactId();
                    //    if (!string.IsNullOrEmpty(joinConfAddress) && !string.IsNullOrEmpty(joinConfContactId))
                    //    {
                    //        log.Info("Login status changed to NotLogin and begin to login for link p2p call.");
                    //        LoginManager.Instance.SaveCurrentLoginInfo();
                    //        Application.Current.Dispatcher.InvokeAsync(() => {
                    //            LoginManager.Instance.AutoLogin4LinkP2pCall(joinConfAddress);
                    //            LoginManager.Instance.IsNeedRelogin = true;
                    //        });
                    //    }

                    //    Utils.SetAnonymousLogoutAndLinkP2pCall(false);
                    //}
                }
                else if (LoginStatus.LoginFailed == status)
                {
                    string joinConfAddress = Utils.GetAnonymousJoinConfServerAddress();
                    string joinConfContactId = Utils.GetAnonymousJoinConfContactId();
                    string joinConfContactAlias = Utils.GetAnonymousJoinConfContactAlias();
                    if (!string.IsNullOrEmpty(joinConfAddress) && (!string.IsNullOrEmpty(joinConfContactId) || !string.IsNullOrEmpty(joinConfContactAlias)))
                    {
                        log.Info("LoginFailed, clear link p2p call info");
                        Utils.ClearAnonymousJoinConfData();
                    }
                }
            }

            Application.Current.Dispatcher.InvokeAsync(() => {
                // in the thread to make sure _mainView is constructed.
                if (null != _mainView)
                {
                    // call this to ensure the order for login status
                    _mainView.LoginManager_PropertyChanged(sender, arg);
                }
            });
        }
        
        private void UpdateWindowVisibility()
        {
            LoginStatus loginStatus = LoginManager.Instance.CurrentLoginStatus;
            CallStatus callStatus = CallController.Instance.CurrentCallStatus;
            log.InfoFormat("Update window visibility, login status: {0}, call status: {1}", loginStatus, callStatus);
            bool isInCall =    CallStatus.Dialing == callStatus
                            || CallStatus.ConfIncoming == callStatus
                            || CallStatus.P2pIncoming == callStatus
                            || CallStatus.P2pOutgoing == callStatus
                            || CallStatus.Connected == callStatus;
            bool isLoginWindowVisible = 
                   LoginStatus.NotLogin == loginStatus
                || LoginStatus.LoggingIn == loginStatus
                || LoginStatus.LoginFailed == loginStatus
                || LoginStatus.AnonymousLoggingIn == loginStatus
                || (LoginStatus.AnonymousLoggedIn == loginStatus && !isInCall);
            bool isMainWindowVisible = (LoginStatus.LoggedIn == loginStatus) && !isInCall;
            bool isVideoPeopleWindowVisible = isInCall;
            Application.Current.Dispatcher.InvokeAsync(() => {
                if (null != LoginManager.Instance.LoginWindow)
                {
                    log.InfoFormat("Login window visible:{0}", isLoginWindowVisible);
                    Visibility visibility = isLoginWindowVisible ? Visibility.Visible : Visibility.Collapsed;
                    if (visibility != LoginManager.Instance.LoginWindow.Visibility)
                    {
                        LoginManager.Instance.LoginWindow.Visibility = visibility;
                        if (
                               Visibility.Visible == SoftwareUpdateWindow.Instance.Visibility
                            && Visibility.Visible == LoginManager.Instance.LoginWindow.Visibility
                        )
                        {
                            LoginWindow loginWindow = LoginManager.Instance.LoginWindow as LoginWindow;
                            DisplayUtil.SetWindowCenterAndOwner(SoftwareUpdateWindow.Instance, loginWindow);
                            log.Info("Set soft update window owner to login window");
                        }
                    }
                    else
                    {
                        log.InfoFormat("Login window has been visibility: {0}", visibility);
                    }
                }

                if (null != Application.Current.MainWindow)
                {
                    log.InfoFormat("Main window visible:{0}", isMainWindowVisible);
                    Visibility visibility = isMainWindowVisible ? Visibility.Visible : Visibility.Collapsed;
                    if (visibility != Application.Current.MainWindow.Visibility)
                    {
                        Application.Current.MainWindow.Visibility = visibility;
                        if (
                               Visibility.Visible == SoftwareUpdateWindow.Instance.Visibility
                            && Visibility.Visible == Application.Current.MainWindow.Visibility
                            && Application.Current.MainWindow.IsLoaded
                        )
                        {
                            MainWindow mainWindow = Application.Current.MainWindow as MainWindow;
                            DisplayUtil.SetWindowCenterAndOwner(SoftwareUpdateWindow.Instance, mainWindow);
                            log.Info("Set soft update window owner to main window");
                        }
                    }
                    else
                    {
                        log.InfoFormat("Main window has been visibility: {0}", visibility);
                    }
                }

                if (null != VideoPeopleWindow.Instance)
                {
                    log.InfoFormat("Video people window visible:{0}", isVideoPeopleWindowVisible);
                    Visibility visibility = isVideoPeopleWindowVisible ? Visibility.Visible : Visibility.Collapsed;
                    if (visibility != VideoPeopleWindow.Instance.Visibility)
                    {
                        log.InfoFormat(
                            "Update VideoPeopleWindow visible, FirstShow:{0}, ActualWidth:{1}, ActualHeight:{2}, WindowState: {3}"
                            , VideoPeopleWindow.Instance.FirstShow
                            , VideoPeopleWindow.Instance.ActualWidth
                            , VideoPeopleWindow.Instance.ActualHeight
                            , VideoPeopleWindow.Instance.WindowState
                        );
                        Visibility currentStatus = VideoPeopleWindow.Instance.Visibility;
                        if (Visibility.Visible == currentStatus && !isVideoPeopleWindowVisible && WindowState.Minimized == VideoPeopleWindow.Instance.WindowState)
                        {
                            // in the case: sending content and the window is minimized, then disconnect it in remote. it should be resume to preseting state and then hidden.
                            VideoPeopleWindow.Instance.Set2PresettingState();
                        }
                        VideoPeopleWindow.Instance.Visibility = isVideoPeopleWindowVisible ? Visibility.Visible : Visibility.Collapsed;
                        if (   isVideoPeopleWindowVisible
                            && (VideoPeopleWindow.Instance.FirstShow || 0 == VideoPeopleWindow.Instance.ActualWidth || 0 == VideoPeopleWindow.Instance.ActualHeight)
                        )
                        {
                            log.Info("Reset initial size for VideoPeopleWindow");
                            // resume window size and center the screen on first show
                            VideoPeopleWindow.Instance.FirstShow = false;
                            VideoPeopleWindow.Instance.ResetInitialSize();
                            DisplayUtil.CenterWindowOnMasterWindowScreen(View.VideoPeopleWindow.Instance, LoginManager.Instance.LoginWindow as LoginWindow);
                        }
                        //VideoPeopleWindow.Instance.Visibility = isVideoPeopleWindowVisible ? Visibility.Visible : Visibility.Collapsed;
                        if (Visibility.Visible == VideoPeopleWindow.Instance.Visibility && currentStatus != Visibility.Visible)
                        {
                            VideoPeopleWindow.Instance.AdjustWindowSize();
                        }
                        log.InfoFormat("VideoPeopleWindow, left: {0}, top: {1}", VideoPeopleWindow.Instance.Left, VideoPeopleWindow.Instance.Top);
                        if (
                               Visibility.Visible == SoftwareUpdateWindow.Instance.Visibility
                            && Visibility.Visible == VideoPeopleWindow.Instance.Visibility
                        )
                        {
                            DisplayUtil.SetWindowCenterAndOwner(SoftwareUpdateWindow.Instance, VideoPeopleWindow.Instance);
                            log.Info("Set soft update window owner to video people window");
                        }
                    }
                    else
                    {
                        log.InfoFormat("Video people window has been visibility: {0}", visibility);
                    }
                }
            });
        }
        
        public void StartJoinConference(string confNumber, string displayName, string password, IMasterDisplayWindow ownerWindow)
        {
            JoinConference(confNumber, displayName, password, ownerWindow);
        }

        private void TimerCheckSystemStatus_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            SystemMonitorUtil.Instance.OutputMemoryInfo();
        }

        public void CheckSoftwareUpdate(bool showPrompt)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(Properties.Settings.Default.SoftwareUpdateAddress)
                .Append("/")
                .Append(Properties.Settings.Default.SoftwareUpdateFlag)
                .Append("/")
                .Append(Properties.Settings.Default.SoftwareUpdateBundleId)
                .Append("/");
            if (!string.IsNullOrEmpty(Properties.Settings.Default.SoftwareUpdateAppInfoPrefix))
            {
                sb.Append(Properties.Settings.Default.SoftwareUpdateAppInfoPrefix).Append(".");
            }
            sb.Append(Properties.Settings.Default.SoftwareUpdateAppInfo);
            string swUpdateUrl =  sb.ToString();
            log.InfoFormat("Software update url:{0}", swUpdateUrl);
            WaitingWindow waitingWindow = null;
            if (showPrompt)
            {
                Application.Current.Dispatcher.InvokeAsync(() => {
                    waitingWindow = new WaitingWindow(LanguageUtil.Instance.GetValueByKey("CHECKING_UPDATE"));
                    IMasterDisplayWindow masterWindow = ((MainWindow)Application.Current.MainWindow).GetCurrentMainDisplayWindow();
                    if (null != masterWindow)
                    {
                        waitingWindow.Owner = masterWindow.GetWindow();
                    }
                    waitingWindow.ShowDialog();
                });
            }
            Task.Run(() => {
                RestClient restClient = new RestClient();
                RestResponse response = null;
                try
                {
                    response = restClient.GetObject(swUpdateUrl);
                    if (response.StatusCode >= HttpStatusCode.BadRequest)
                    {
                        log.InfoFormat("Failed to get app info for software update, status code: {0}", response.StatusCode);
                        if (showPrompt)
                        {
                            Application.Current.Dispatcher.InvokeAsync(() => {
                                if (null != waitingWindow)
                                {
                                    waitingWindow.Close();
                                }
                                ShowNotFoundNewVersion();
                            });
                        }
                        
                        return;
                    }

                    Rest.SoftwareUpdateRest.AppInfoRest appInfo = JsonConvert.DeserializeObject<Rest.SoftwareUpdateRest.AppInfoRest>(response.Content);
                    log.InfoFormat("Software update info, version:{0}, url:{1}", appInfo.VERSION, appInfo.DOWNLOAD_URL);
                    string currentVersion = Utils.GetEdition();
                    log.InfoFormat("Current app version: {0}", currentVersion);
                    string[] arrAppInfo = appInfo.VERSION.Split('.');
                    string[] arrCurrentVersion = currentVersion.Split('.');
                    int len = arrAppInfo.Length > arrCurrentVersion.Length ? arrAppInfo.Length : arrCurrentVersion.Length;
                    bool newVersionFound = false;
                    for (int i=0; i<len; ++i)
                    {
                        int newItem = int.Parse(arrAppInfo[i]);
                        int currentItem = int.Parse(arrCurrentVersion[i]);
                        if (newItem > currentItem)
                        {
                            newVersionFound = true;
                            break;
                        }
                        else if (newItem < currentItem)
                        {
                            break;
                        }
                    }

                    if (!newVersionFound)
                    {
                        if (showPrompt)
                        {
                            Application.Current.Dispatcher.InvokeAsync(() => {
                                if (null != waitingWindow)
                                {
                                    waitingWindow.Close();
                                }
                                ShowNotFoundNewVersion();
                            });
                        }
                        return;
                    }

                    Application.Current.Dispatcher.InvokeAsync(() => {
                        if (null != waitingWindow)
                        {
                            waitingWindow.Close();
                        }
                        SoftwareUpdateWindow.Instance.Reset(appInfo.VERSION, appInfo.DOWNLOAD_URL);
                        IMasterDisplayWindow masterWindow = ((MainWindow)Application.Current.MainWindow).GetCurrentMainDisplayWindow();
                        DisplayUtil.SetWindowCenterAndOwner(SoftwareUpdateWindow.Instance, masterWindow);
                        if (null != masterWindow)
                        {
                            log.InfoFormat("Set soft update window owner to window:{0}", masterWindow.GetHashCode());
                        }

                        SoftwareUpdateWindow.Instance.Show();
                        if (null != SoftwareUpdateWindow.Instance.Owner)
                        {
                            // set the owner active or the owner will hide when cancel the software update window.
                            SoftwareUpdateWindow.Instance.Owner.Activate();
                        }
                    });
                }
                catch (Exception ex)
                {
                    log.ErrorFormat("Failed to get app info for software update, exception:{0}", ex);
                    if (showPrompt)
                    {
                        Application.Current.Dispatcher.InvokeAsync(() => {
                            if (null != waitingWindow)
                            {
                                waitingWindow.Close();
                            }
                            ShowNotFoundNewVersion();
                        });
                    }
                }
            });
        }

        public void RefreshSoftwareUpdateWindow()
        {
            if (
                   Visibility.Visible == SoftwareUpdateWindow.Instance.Visibility
                && Visibility.Visible == Application.Current.MainWindow.Visibility
            )
            {
                MainWindow mainWindow = Application.Current.MainWindow as MainWindow;
                DisplayUtil.SetWindowCenterAndOwner(SoftwareUpdateWindow.Instance, mainWindow);
            }
        }

        private void ShowNotFoundNewVersion()
        {
            MessageBoxTip tip = new MessageBoxTip(((MainWindow)Application.Current.MainWindow).GetCurrentMainDisplayWindow());
            tip.SetTitleAndMsg(
                LanguageUtil.Instance.GetValueByKey("PROMPT")
                , LanguageUtil.Instance.GetValueByKey("NOT_FOUND_NEW_VERSION")
                , LanguageUtil.Instance.GetValueByKey("CONFIRM")
            );
            tip.ShowDialog();
        }
    }
}
