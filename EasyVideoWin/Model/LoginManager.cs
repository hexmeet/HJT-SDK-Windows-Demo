﻿using EasyVideoWin.CustomControls;
using EasyVideoWin.Helpers;
using EasyVideoWin.ManagedEVSdk.Structs;
using EasyVideoWin.Model.CloudModel;
using EasyVideoWin.Model.CloudModel.CloudRest;
using EasyVideoWin.View;
using log4net;
using Newtonsoft.Json;
using System;
using System.ComponentModel;
using System.IO;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;

namespace EasyVideoWin.Model
{
    public enum LoginStatus
    {
        NotLogin,
        LoggedIn,
        LoggingIn,
        LoginFailed,
        AnonymousLoggedIn,
        AnonymousLoggingIn
    }

    public enum LoginProgressEnum
    {
        Idle
        , EnterpriseOptions
        , EnterpriseLogin
        , EnterpriseJoinConf
        , CloudOptions
        , CloudLogin
        , CloudJoinConf
        , AdvancedSetting
    }
    
    class LoginManager : INotifyPropertyChanged
    {
        private static LoginManager _instance = new LoginManager();
        private readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private const string CLIENT_USER_AGENT     = "windows-client";
        private AutoResetEvent _loginResetEvent = new AutoResetEvent(false);

        private const string DXJC_AES_KEY = "yzr9dy8x60hv7z82";
        private const string DXJC_AES_IV = "0000000000000000";

        private LoginManager()
        {
            log.InfoFormat("LoginManager begin to construct, CloudOnly: {0}", Properties.Settings.Default.CloudOnly);
            _loginStatus = LoginStatus.NotLogin;

            //Utils.ParseCloudServerAddress();
            string joinConfAddress = Utils.GetAnonymousJoinConfServerAddress();
            if (EasyVideoWin.Properties.Settings.Default.CloudOnly)
            {
                ServiceType = Utils.ServiceTypeEnum.Cloud;
            }
            else
            {
                ServiceType = Utils.GetServiceType();
            }
            if (!string.IsNullOrEmpty(joinConfAddress))
            {
                // anonymous join conf
                log.Info("Anonymous join conf or link p2p call");
                string joinConfContactId = Utils.GetAnonymousJoinConfContactId();
                string joinConfContactAlias = Utils.GetAnonymousJoinConfContactAlias();
                if (!string.IsNullOrEmpty(joinConfContactId) || !string.IsNullOrEmpty(joinConfContactAlias))
                {
                    log.InfoFormat("Link P2p call and try to auto login, joinConfAddress: {0}, CloudServerDomain: {2}", joinConfAddress, Utils.CloudServerDomain);
                    if (Properties.Settings.Default.CloudOnly && joinConfAddress != Utils.CloudServerDomain)
                    {
                        Utils.ClearAnonymousJoinConfData();
                        log.Info("Can not link p2p call for CloudOnly and invalid server address");
                        //MessageBox.Show(LanguageUtil.Instance.GetValueByKey("CANNOT_CALL_FOR_INVALID_SERVER_ADDRESS"));
                        SetProperLoginStatus(false);
                    }
                    else
                    {
                        if (joinConfAddress == Utils.CloudServerDomain)
                        {
                            ServiceType = Utils.ServiceTypeEnum.Cloud;
                        }
                        Utils.SetServerAddress(ServiceType, joinConfAddress);
                        SetProperLoginStatus(true);
                    }
                }
                else
                {
                    log.Info("Anonymous join conf");
                    ServiceType = Properties.Settings.Default.CloudOnly ? Utils.ServiceTypeEnum.Cloud : Utils.ServiceTypeEnum.None;
                    IsNeedAnonymousJoinConf = true;
                }
            }
            else
            {
                log.Info("Normal startup and set proper login status");
                SetProperLoginStatus(false);
            }

            log.InfoFormat("New login manager instance, hash code: {0}", this.GetHashCode());
            EVSdkManager.Instance.EventLoginSucceed += EVSdkWrapper_EventLoginSucceed;
            EVSdkManager.Instance.EventRegister += EVSdkWrapper_EventRegister;
            EVSdkManager.Instance.EventDownloadUserImageComplete += EVSdkWrapper_EventDownloadUserImageComplete;
        }
        
        public static LoginManager Instance
        {
            get
            {
                return _instance;
            }
        }

        public AutoResetEvent LoginResetEvent
        {
            get
            {
                return _loginResetEvent;
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private bool _isNeedRelogin = false;
        public bool IsNeedRelogin
        {
            get
            {
                return _isNeedRelogin;
            }
            set
            {
                if (_isNeedRelogin != value)
                {
                    _isNeedRelogin = value;
                    OnPropertyChanged("IsNeedRelogin");
                }
            }
        }

        public Window LoginWindow { get; set; }

        private LoginStatus _loginStatus;
        public LoginStatus CurrentLoginStatus
        {
            get
            {
                return _loginStatus;
            }
            set
            {
                if (_loginStatus == value)
                {
                    return;
                }

                Utils.SetLoginStatus(value);
                if (LoginStatus.NotLogin == value || LoginStatus.LoginFailed == value)
                {
                    LoginToken = "";
                    UpdateAvatarBmp(null); // release the used image or deleting it will be failed.
                    IsRegistered = false;
                }

                log.InfoFormat("Change login status from {0} to {1}", _loginStatus, value);
                _loginStatus = value;
                OnPropertyChanged("CurrentLoginStatus");
            }
        }

        private LoginProgressEnum _loginProgress;
        public LoginProgressEnum LoginProgress
        {
            get
            {
                return _loginProgress;
            }
            set
            {
                log.InfoFormat("Set LoginProgress from {0} to {1}", _loginProgress, value);
                if (_loginProgress != value)
                {
                    PreviousLoginProgress = _loginProgress;
                    _loginProgress = value;
                    OnPropertyChanged("LoginProgress");
                }
            }
        }

        public LoginProgressEnum PreviousLoginProgress { get; set; }

        private Utils.ServiceTypeEnum _serviceType;
        public Utils.ServiceTypeEnum ServiceType
        {
            get
            {
                return _serviceType;
            }
            set{
                log.InfoFormat("ServiceType changing from {0} to {1}", _serviceType, value);
                if (EasyVideoWin.Properties.Settings.Default.CloudOnly)
                {
                    if (Utils.ServiceTypeEnum.Cloud != value)
                    {
                        log.WarnFormat("Set error value to ServiceType for CloudOnly, value: {0}", value);
                    }
                    else
                    {
                        if (
                               LoginProgressEnum.CloudOptions != LoginProgress
                            && LoginProgressEnum.CloudLogin != LoginProgress
                            && LoginProgressEnum.CloudJoinConf != LoginProgress
                        )
                        {
                            log.InfoFormat("Invaid LoginProgress for CloudOnly and set to valid value, LoginProgress: {0}", LoginProgress);
                            LoginProgress = LoginProgressEnum.CloudOptions;
                        }
                    }
                }
                _serviceType = value;
            }
        }

        public bool _isNeedAutoLogin;
        public bool IsNeedAutoLogin
        {
            get
            {
                return _isNeedAutoLogin;
            }
            set
            {
                if (_isNeedAutoLogin != value)
                {
                    _isNeedAutoLogin = value;
                    OnPropertyChanged("IsNeedAutoLogin");
                }
            }
        }

        public bool _isNeedAnonymousJoinConf;
        public bool IsNeedAnonymousJoinConf
        {
            get
            {
                return _isNeedAnonymousJoinConf;
            }
            set
            {
                if (_isNeedAnonymousJoinConf != value)
                {
                    _isNeedAnonymousJoinConf = value;
                    OnPropertyChanged("IsNeedAnonymousJoinConf");
                }
            }
        }
        
        public EVUserInfoCli _loginUserInfo;
        public EVUserInfoCli LoginUserInfo
        {
            get
            {
                return _loginUserInfo;
            }
            set
            {
                _loginUserInfo = value;
                if (null == _loginUserInfo)
                {
                    log.Info("_loginUserInfo is null");
                }
                else
                {
                    if (null == _loginUserInfo.featureSupport)
                    {
                        log.Info("_loginUserInfo.featureSupport is null");
                    }
                    else
                    {
                        log.InfoFormat("_loginUserInfo.featureSupport, chatInConference: {0}, contactWebPage: {1}, p2pCall: {2}, sitenameIsChangeable: {3}, switchingToAudioConference: {4}"
                            , _loginUserInfo.featureSupport.chatInConference
                            , _loginUserInfo.featureSupport.contactWebPage
                            , _loginUserInfo.featureSupport.p2pCall
                            , _loginUserInfo.featureSupport.sitenameIsChangeable
                            , _loginUserInfo.featureSupport.switchingToAudioConference);
                    }
                }
                OnPropertyChanged("LoginUserInfo");
            }
        }
        
        private string _loginToken;
        public string LoginToken
        {
            set
            {
                if (_loginToken == value)
                {
                    return;
                }
                _loginToken = value;
                OnPropertyChanged("LoginToken");
            }
            get
            {
                return _loginToken;
            }
        }

        private ulong _userId;
        public ulong UserId
        {
            get
            {
                return _userId;
            }
            set
            {
                _userId = value;
                OnPropertyChanged("UserId");
            }
        }
        
        private WriteableBitmap _avatarBmp;
        public WriteableBitmap AvatarBmp
        {
            get
            {
                return _avatarBmp;
            }
            set
            {
                if (null == value && null == _avatarBmp)
                {
                    return;
                }

                _avatarBmp = value;
                OnPropertyChanged("AvatarBmp");
            }
        }
        
        public ulong DeviceId
        {
            get
            {
                if (LoginStatus.LoggedIn == CurrentLoginStatus || LoginStatus.AnonymousLoggedIn == CurrentLoginStatus)
                {
                    return LoginUserInfo.deviceId;
                }
                
                return 0;
            }
        }

        public bool _isRegistered = false;
        public bool IsRegistered
        {
            get
            {
                return _isRegistered;
            }
            set
            {
                if (_isRegistered == value)
                {
                    return;
                }

                _isRegistered = value;
                OnPropertyChanged("IsRegistered");
            }
        }
        
        public bool _isInitFinished = false;
        public bool IsInitFinished
        {
            get
            {
                return _isInitFinished;
            }
            set
            {
                if (_isInitFinished == value)
                {
                    return;
                }
                _isInitFinished = value;
                OnPropertyChanged("IsInitFinished");
            }
        }

        public bool EnableSecure
        {
            get
            {
                if (IsUrlLoginMode)
                {
                    return UrlLoginEnableSecure;
                }

                if (
                       LoginProgressEnum.CloudOptions == LoginProgress
                    || LoginProgressEnum.CloudLogin == LoginProgress
                    || LoginProgressEnum.CloudJoinConf == LoginProgress
                )
                {
                    return Utils.CloudServerUseHttps;
                }
                else
                {
                    return Utils.GetUseHttps();
                }
            }
        }

        public uint ServerPort
        {
            get
            {
                if (IsUrlLoginMode)
                {
                    return UrlLoginPort;
                }

                if (
                       LoginProgressEnum.CloudOptions == LoginProgress
                    || LoginProgressEnum.CloudLogin == LoginProgress
                    || LoginProgressEnum.CloudJoinConf == LoginProgress
                )
                {
                    return (uint)Utils.CloudServerPort;
                }
                else
                {
                    return (uint)Utils.GetServerPort();
                }
            }
        }

        private bool IsUrlLoginMode { get; set; }
        private bool UrlLoginEnableSecure { get; set; }
        private uint UrlLoginPort { get; set; }
        
        public void DownloadLoginUserAvatar()
        {
            string tempPath = Path.Combine(Utils.GetConfigDataPath(), LoginToken + "_header.jpg");
            log.InfoFormat("Begin to download avatar to {0}", tempPath);
            EVSdkManager.Instance.DownloadUserImage(tempPath);
        }

        private void EVSdkWrapper_EventDownloadUserImageComplete(string path)
        {
            log.InfoFormat("EventDownloadUserImageComplete. path={0}", path);
            string avatarPath = Utils.GetCurrentAvatarPath();
            try
            {
                File.Copy(path, avatarPath, true);
                UpdateAvatarBmp(avatarPath);
                File.Delete(path);
            }
            catch(Exception e)
            {
                log.InfoFormat("Exception for copy avatar or delete temp avatar, exception:{0}", e);
            }
            finally
            {
                log.Info("EventDownloadUserImageComplete end");
            }
        }
        
        private string _displayName;
        public string DisplayName
        {
            set
            {
                _displayName = value;
                OnPropertyChanged("DisplayName");
            }
            get
            {
                return _displayName;
            }
        }

        private string _userame;
        public string Username
        {
            set
            {
                _userame = value;
                OnPropertyChanged("Username");
            }
            get
            {
                return _userame;
            }
        }
        
        public void AutoLoginToProperServiceType()
        {
            switch (ServiceType)
            {
                case Utils.ServiceTypeEnum.Enterprise:
                    LoginProgress = LoginProgressEnum.EnterpriseLogin;
                    IsNeedAutoLogin = true;
                    break;
                case Utils.ServiceTypeEnum.Cloud:
                    LoginProgress = LoginProgressEnum.CloudLogin;
                    IsNeedAutoLogin = true;
                    break;
            }
        }

        public void SaveCurrentLoginInfo()
        {
            Utils.SetPreServiceType(ServiceType);
            if (Utils.ServiceTypeEnum.Enterprise == ServiceType)
            {
                Utils.SetPreEnterpriseServerAddress(Utils.GetServerAddress(ServiceType));
            }
            Utils.SetPreUseHttps(Utils.GetUseHttps());
            Utils.SetPreServerPort(Utils.GetServerPort());
        }

        public void ResumePreLoginInfo()
        {
            Utils.ServiceTypeEnum preServiceType = Utils.GetPreServiceType();
            Utils.SetServiceType(preServiceType);
            if (Utils.ServiceTypeEnum.Enterprise == preServiceType)
            {
                Utils.SetServerAddress(preServiceType, Utils.GetPreEnterpriseServerAddress());
            }
            Utils.SetUseHttps(Utils.GetPreUseHttps());
            Utils.SetServerPort(Utils.GetPreServerPort());
            ServiceType = preServiceType;
            if (Utils.ServiceTypeEnum.Enterprise == preServiceType)
            {
                LoginProgress = LoginProgressEnum.EnterpriseLogin;
            }
            else
            {
                LoginProgress = LoginProgressEnum.CloudLogin;
            }
        }

        public void TryAutoLogin()
        {
            log.Info("TryAutoLogin");
            string joinConfAddress = Utils.GetAnonymousJoinConfServerAddress();
            string joinConfContactId = Utils.GetAnonymousJoinConfContactId();
            string joinConfContactAlias = Utils.GetAnonymousJoinConfContactAlias();
            bool isLinkP2pCall = !string.IsNullOrEmpty(joinConfAddress) && (!string.IsNullOrEmpty(joinConfContactId) || !string.IsNullOrEmpty(joinConfContactAlias));
            if (!Utils.IsAutoLogin() && !isLinkP2pCall)
            {
                log.Info("Do not try to login for IsAutoLogin and isLinkP2pCall are false");
                return;
            }

            LoginWithConfigInfo();
        }

        public void LoginWithConfigInfo()
        {
            log.Info("Begin to login with config info");
            string userName = Utils.GetUserName(ServiceType);
            string serverAddress;
            string password = Utils.GetPassword(ServiceType);
            serverAddress = Utils.GetServerAddress(ServiceType);

            if (string.IsNullOrWhiteSpace(userName) || string.IsNullOrWhiteSpace(serverAddress) || string.IsNullOrWhiteSpace(password))
            {
                log.Info("Can not login with config info for invalid value.");
                return;
            }

            Login(userName, password, serverAddress);
        }

        public void Logout()
        {
            log.Info("Logout app.");
            if (LoginStatus.LoggedIn == CurrentLoginStatus)
            {
                EVSdkManager.Instance.Logout();
            }
            
            CurrentLoginStatus = LoginStatus.NotLogin;
        }
        
        private string _serverDomainAddress = "";
        private uint _domainPort = 80;
        private bool _useHttps = false;
        public string LoginPassword { get; set; }
        
        public bool Login(string userName, string pwd, string domainAddress, bool isPasswdEncrypted = false)
        {
            log.InfoFormat("Login, domainAddress: {0}, username: {1}, password: {2}: isPasswordEncrypted: {3}", domainAddress, userName, pwd, isPasswdEncrypted);
            if(CurrentLoginStatus != LoginStatus.NotLogin)
            {
                log.InfoFormat("Can not login for login status is: {0}", CurrentLoginStatus);
                return false;
            }

            if (string.IsNullOrEmpty(pwd) || string.IsNullOrEmpty(userName) || string.IsNullOrEmpty(domainAddress))
            {
                log.Info("Can not login for login status is not invalid login parameters");
                return false;
            }

            IsUrlLoginMode = isPasswdEncrypted;

            EVSdkManager.Instance.EnableSecure(EnableSecure);
            _serverDomainAddress = domainAddress;
            LoginPassword = pwd;
            CurrentLoginStatus = LoginStatus.LoggingIn;

            Task.Run(() => {
                // When click button login in login dialog, the LoginWithLocation may block the UI thread and ProgressDialogLogin can not be displayed.
                // So exec LoginWithLocation in another thread.
                if (isPasswdEncrypted)
                {
                    EVSdkManager.Instance.LoginWithLocationByEncPasswd(domainAddress, ServerPort, userName, pwd);
                }
                else
                {
                    EVSdkManager.Instance.LoginWithLocation(domainAddress, ServerPort, userName, pwd);
                }
            });

            Application.Current.Dispatcher.InvokeAsync(() =>
            {
                log.Info("Show ProgressDialogLogin");
                DisplayUtil.SetWindowCenterAndOwner(ProgressDialogLogin.Instance, (IMasterDisplayWindow)LoginWindow);
                ProgressDialogLogin.Instance.ShowDialog();
            });

            return true;
        }

        public bool UrlLogin(string serverAddress, uint port, bool isSecure, string username, string encryptedPasswd)
        {
            UrlLoginEnableSecure = isSecure;
            UrlLoginPort = port;
            if (LoginStatus.LoginFailed == CurrentLoginStatus)
            {
                CurrentLoginStatus = LoginStatus.NotLogin;
            }
            return Login(username, encryptedPasswd, serverAddress, true);
        }

        public void LoginByToken(string serverAddress, string protocol, int port, string token)
        {
            if (Utils.ServiceTypeEnum.Cloud == ServiceType)
            {
                LoginProgress = LoginProgressEnum.CloudLogin;
            }
            else
            {
                LoginProgress = LoginProgressEnum.EnterpriseLogin;
            }
            Utils.SetAnonymousLogoutAndAnonymousJoinConf(false);
            log.InfoFormat("LoginByToken, serverAddress: {0}, protocol: {1}, port: {2}", serverAddress, protocol, port);
            IsNeedAnonymousJoinConf = false;
            Application.Current.Dispatcher.InvokeAsync(() =>
            {
                log.Info("Show ProgressDialogLogin");
                ProgressDialogLogin.Instance.ShowDialog();
            });
            LoginResultByTokenRest dxjcLoginInfo = CloudApiManager.Instance.GetDxjcLoginInfo(serverAddress, protocol, port, token, (response) =>
            {
                try
                {
                    Utils.ClearAnonymousJoinConfData();
                    ErrorMessageRest errMsg = JsonConvert.DeserializeObject<ErrorMessageRest>(response.Content);
                    log.ErrorFormat("loginByToken failed, errorCode: {0}, errorInfo: {0}", errMsg.errorCode, errMsg.errorInfo);
                }
                catch (Exception e)
                {
                    log.InfoFormat("Exception for handle error of loginByToken, exception:{0}", e);
                }
            });
            if (null == dxjcLoginInfo)
            {
                Utils.ClearAnonymousJoinConfData();
                log.Info("GetDxjcLoginInfo response null");
                return;
            }
            log.InfoFormat("url login info, token: {0}, dxjckey: {1},", dxjcLoginInfo.token, dxjcLoginInfo.dxjckey);
            string plainKey = CryptoUtil.AESDecrypt(dxjcLoginInfo.dxjckey, DXJC_AES_KEY, DXJC_AES_IV, false, PaddingMode.None);
            LoginUserRest loginInfo = JsonConvert.DeserializeObject<LoginUserRest>(plainKey);
            log.InfoFormat("url login user: {0}", loginInfo.username);

            Application.Current.Dispatcher.InvokeAsync(() =>
            {
                ProgressDialogLogin.Instance.Hide();
            });
            if (string.IsNullOrEmpty(loginInfo.username) || string.IsNullOrEmpty(loginInfo.password))
            {
                log.Info("Invalid url login username or password");
                Utils.ClearAnonymousJoinConfData();
                return;
            }
            bool rst = UrlLogin(
                Utils.GetAnonymousJoinConfServerAddress()
                , (uint)Utils.GetAnonymousJoinConfServerPort()
                , "https" == Utils.GetAnonymousJoinConfServerProtocol()
                , loginInfo.username
                , loginInfo.password
                );
            if (!rst)
            {
                log.Info("Failed to UrlLogin and clear data");
                Utils.ClearAnonymousJoinConfData();
            }
        }

        public void OnLoggingInFailed(bool forceLogout)
        {
            if (forceLogout)
            {
                EVSdkManager.Instance.Logout();
            }
            CurrentLoginStatus = LoginStatus.LoginFailed;
            if (IsUrlLoginMode)
            {
                Utils.ClearAnonymousJoinConfData();
            }
            _loginResetEvent.Set();
            Application.Current.Dispatcher.InvokeAsync(() =>
            {
                ProgressDialogLogin.Instance.Hide();
                LoginWindow.Activate();
            });
        }

        private void EVSdkWrapper_EventLoginSucceed(ManagedEVSdk.Structs.EVUserInfoCli userInfo)
        {
            log.InfoFormat("EventLoginSucceed, username:{0}, token:{1}", userInfo.username, userInfo.token);
            LoginToken = userInfo.token;
            DisplayName = userInfo.displayName;
            Username = userInfo.username;
            UserId = userInfo.userId;
            LoginUserInfo = userInfo;

            if (LoginStatus.LoggingIn == CurrentLoginStatus)
            {
                log.InfoFormat("Login succeed and current login status is LoggingIn. It should download avatar.");
                // update avatar
                string avatarPath = Utils.GetCurrentAvatarPath();
                try
                {
                    if (File.Exists(avatarPath))
                    {
                        log.Info("Current avatar file is existed and delete it for the new one.");
                        UpdateAvatarBmp(null);
                        File.Delete(avatarPath);
                    }
                }
                catch (Exception e)
                {
                    log.InfoFormat("Failed to delete avatar, exception:{0}", e);
                }
                DownloadLoginUserAvatar();
            }

            CurrentLoginStatus = (LoginStatus.AnonymousLoggingIn == CurrentLoginStatus || LoginStatus.AnonymousLoggedIn == CurrentLoginStatus) ? LoginStatus.AnonymousLoggedIn : LoginStatus.LoggedIn;
            log.InfoFormat("Login successfully. Set login status to {0}.Domain address={1}, username={2}, token={3}", CurrentLoginStatus, _serverDomainAddress, Username, LoginToken);
            Utils.SetIsConfRunning(false);
            Utils.SetRunningConfId("");

            if (!IsUrlLoginMode)
            {
                LoginSuccessToSaveData(_serverDomainAddress);
                if (LoginStatus.AnonymousLoggedIn == CurrentLoginStatus)
                {
                    Utils.SetServerPort((int)_domainPort);
                    Utils.SetUseHttps(_useHttps);
                }
                else
                {
                    Utils.SetPassword(ServiceType, LoginPassword);
                    Utils.SetUserName(ServiceType, userInfo.username);
                }
            }

            _loginResetEvent.Set();
            Application.Current.Dispatcher.InvokeAsync(() =>
            {
                ProgressDialogLogin.Instance.Hide();
                LoginWindow.Activate();
            });
            log.Info("EventLoginSucceed end");
        }
        
        public void AnonymousJoinConference(bool isJoinDirectly, bool useHttps, string domainAddress, uint domainPort, string confNumber, string confPassword, bool enableCamera, bool enableMicrophone, ManagedEVSdk.Structs.EV_SVC_CONFERENCE_NAME_TYPE_CLI nameType=ManagedEVSdk.Structs.EV_SVC_CONFERENCE_NAME_TYPE_CLI.EV_SVC_CONFERENCE_NAME_ID)
        {
            if (LoginStatus.NotLogin != CurrentLoginStatus && LoginStatus.LoginFailed != CurrentLoginStatus)
            {
                log.InfoFormat("Do not anonymous join conference, login status:{0}", CurrentLoginStatus);
                return;
            }

            if (string.IsNullOrEmpty(domainAddress))
            {
                log.InfoFormat("Do not anonymous join conference for the ip of parsed domain address is empty, domain address:{0}", domainAddress);
                return;
            }

            log.InfoFormat("Anonymous join conference, is directly:{0}", isJoinDirectly);
            EVSdkManager.Instance.EnableCamera(enableCamera);
            EVSdkManager.Instance.EnableMic(enableMicrophone);

            _serverDomainAddress = domainAddress;
            _domainPort = domainPort;
            _useHttps = useHttps;
            CurrentLoginStatus = LoginStatus.AnonymousLoggingIn;
            EVSdkManager.Instance.EnableSecure(useHttps);
            
            UpdateValidDisplayName();
            CallController.Instance.UpdateUserImage(Utils.GetSuspendedVideoBackground(), Utils.GetDefaultUserAvatar());
            CallController.Instance.JoinConferenceWithLocation(domainAddress, domainPort, confNumber, nameType, DisplayName, confPassword);
        }
        
        private void UpdateValidDisplayName()
        {
            string displayName = Utils.GetDisplayNameInConf();
            if (string.IsNullOrEmpty(displayName))
            {
                displayName = Dns.GetHostName();
            }
            DisplayName = displayName;
        }

        private void LoginSuccessToSaveData(string serverAddress)
        {
            Utils.SetServerAddress(ServiceType, serverAddress);
            Utils.SetServiceType(ServiceType);
        }
        
        public void UpdateLoginToken(bool logoutWhenFailed = false)
        {
            if (LoginStatus.LoggedIn != CurrentLoginStatus && LoginStatus.AnonymousLoggedIn != CurrentLoginStatus)
            {
                log.Info("Can't update login token for not login.");
                return;
            }

            log.Info("Update login token.");

            EVUserInfoCli userInfo = new EVUserInfoCli();
            EVSdkManager.Instance.GetUserInfo(ref userInfo);
            if (null == userInfo)
            {
                log.Info("Failed to update login token");
                if (
                       logoutWhenFailed
                    && CallStatus.ConfIncoming != CallController.Instance.CurrentCallStatus
                    && CallStatus.P2pIncoming != CallController.Instance.CurrentCallStatus
                    && CallStatus.P2pOutgoing != CallController.Instance.CurrentCallStatus
                    && CallStatus.Connected != CallController.Instance.CurrentCallStatus
                )
                {
                    CurrentLoginStatus = LoginStatus.NotLogin;
                    log.Info("Failed to update token and logout.");
                }
                return;
            }
            
            LoginToken = userInfo.token;
            log.InfoFormat("Updated token value:{0}", LoginToken);
            DisplayName = userInfo.displayName;
        }
        
        private bool CanAutoLogin(bool forceLogin)
        {
            log.InfoFormat("CanAutoLogin, forceLogin: {0}", forceLogin);
            if (Utils.ServiceTypeEnum.None == ServiceType)
            {
                log.Info("Service type is none, can not auto login");
                return false;
            }

            string userName = Utils.GetUserName(ServiceType);
            string serverAddress = Utils.GetServerAddress(ServiceType);
            string password = Utils.GetPassword(ServiceType);
            bool isAutoLogin = Utils.IsAutoLogin() || forceLogin;
            bool validPassword = !string.IsNullOrWhiteSpace(password);
            if (!string.IsNullOrWhiteSpace(userName) && !string.IsNullOrWhiteSpace(serverAddress) && validPassword && isAutoLogin)
            {
                return true;
            }

            log.InfoFormat("Can not auto login, user name:{0}, server address: {1}, valid password: {2}, auto login config: {3}", userName, serverAddress, validPassword, isAutoLogin);
            return false;
        }

        private void EVSdkWrapper_EventRegister(bool registered)
        {
            log.InfoFormat("EventRegister:{0}", registered);
            IsRegistered = registered;
            log.Info("EventRegister end");
        }

        private void UpdateAvatarBmp(string path)
        {
            Application.Current.Dispatcher.InvokeAsync(() => {
                AvatarBmp = GetAvatarWriteableBitmap(path);
            });
        }

        private WriteableBitmap GetAvatarWriteableBitmap(string path)
        {
            log.Info("Gat avatar writable bitmap");
            if (!File.Exists(path))
            {
                log.Info("Failed to get avatar writable bitmap for path not existed");
                return null;
            }

            if (null == path)
            {
                log.Info("Failed to get avatar writable bitmap for null path");
                return null;
            }
            try
            {
                System.Drawing.Image sourceImage = System.Drawing.Image.FromFile(path);
                int imageWidth = sourceImage.Width, imageHeight = sourceImage.Height;

                System.Drawing.Bitmap sourceBmp = new System.Drawing.Bitmap(sourceImage, imageWidth, imageHeight);
                IntPtr hBitmap = sourceBmp.GetHbitmap();
                BitmapSource bitmapSource = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(hBitmap, IntPtr.Zero, Int32Rect.Empty,
                BitmapSizeOptions.FromEmptyOptions());
                bitmapSource.Freeze();
                WriteableBitmap writeableBmp = new WriteableBitmap(bitmapSource);
                sourceImage.Dispose();
                sourceBmp.Dispose();
                return writeableBmp;
            }
            catch(Exception e)
            {
                log.InfoFormat("Failed to get avatar writeable bitmap, exception:{0}", e);
                return null;
            }
        }

        private void SetProperLoginStatus(bool forceLogin)
        {
            if (CanAutoLogin(forceLogin))
            {
                log.Info("Can auto login.");
                AutoLoginToProperServiceType();
            }
            else
            {
                log.Info("Can not auto login.");
                switch (ServiceType)
                {
                    case Utils.ServiceTypeEnum.None:
                        LoginProgress = LoginProgressEnum.Idle;
                        break;
                    case Utils.ServiceTypeEnum.Enterprise:
                        LoginProgress = LoginProgressEnum.EnterpriseOptions;
                        break;
                    case Utils.ServiceTypeEnum.Cloud:
                        LoginProgress = LoginProgressEnum.CloudOptions;
                        break;
                }
            }
        }

        public void AutoLogin4LinkP2pCall(string serverAddress)
        {
            log.InfoFormat("AutoLogin4LinkP2pCall, serverAddress: {0}, CloudOnly: {1}, CloudServerDomain: {2}", serverAddress, Properties.Settings.Default.CloudOnly, Utils.CloudServerDomain);
            if (EasyVideoWin.Properties.Settings.Default.CloudOnly || Utils.CloudServerDomain == serverAddress)
            {
                ServiceType = Utils.ServiceTypeEnum.Cloud;
            }
            else
            {
                ServiceType = Utils.ServiceTypeEnum.Enterprise;
            }
            
            Utils.SetServerAddress(ServiceType, serverAddress);
            switch (ServiceType)
            {
                case Utils.ServiceTypeEnum.Enterprise:
                    LoginProgress = LoginProgressEnum.EnterpriseLogin;
                    break;
                case Utils.ServiceTypeEnum.Cloud:
                    LoginProgress = LoginProgressEnum.CloudLogin;
                    break;
            }
            IsNeedRelogin = true;
        }

    }
}
