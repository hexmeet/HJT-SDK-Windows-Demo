using log4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EasyVideoWin.ManagedEVSdk;
using EasyVideoWin.ManagedEVSdk.Structs;
using EasyVideoWin.Helpers;
using EasyVideoWin.Enums;

namespace EasyVideoWin.Model
{
    public class EVSdkManager
    {
        #region -- Members --

        private readonly ILog _log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        static private EVSdkManager _instance = new EVSdkManager();

        public const string ACTION_UPLOADUSERIMAGE          = "uploadUserImage";
        public const string ACTION_CHANGEDISPLAYNAME        = "changeDisplayName";
        public const string ACTION_LOGIN                    = "login";
        public const string ACTION_LOGINWITHLOCATION        = "loginWithLocation";
        public const string ACTION_ONCALLEND = "onCallEnd";

        public event Action<EVErrorCli> EventError;
        public event Action<EVWarnCli> EventWarn;

        public event Action<bool> EventNetworkState;
        public event Action<float> EventNetworkQuality;

        public event Action<EVUserInfoCli> EventLoginSucceed;
        public event Action<bool> EventRegister;

        //public event Action<EVCallInfoCli> EventCallIncoming;
        public event Action<EVCallInfoCli> EventCallConnected;
        public event Action<EVCallInfoCli> EventCallPeerConnected;
        public event Action<EVCallInfoCli> EventCallEnd;
        public event Action<EVContentInfoCli> EventContent;

        public event Action<string> EventDownloadUserImageComplete;
        public event Action<string> EventUploadUserImageComplete;

        public event Action<EVLayoutIndicationCli> EventLayoutIndication;
        public event Action<EVSiteCli> EventLayoutSiteIndication;
        public event Action<EVLayoutSpeakerIndicationCli> EventLayoutSpeakerIndication;
        public event Action EventMuteSpeakingDetected;
        public event Action<EVCallInfoCli> EventJoinConferenceIndication;
        //public event Action<int> EventConferenceEndIndication;
        public event Action<EVRecordingInfoCli> EventRecordingIndication;
        public event Action<EVMessageOverlayCli> EventMessageOverlay;
        public event Action<EVWhiteBoardInfoCli> EventWhiteBoardIndication;
        public event Action<int> EventParticipant;
        public event Action<bool> EventRemoteMicMuted;
        public event Action<string> EventPeerImageUrl;

        public event Action<string> EventManagedLog;
        
        #endregion

        #region -- Properties --

        static public EVSdkManager Instance
        {
            get
            {
                return _instance;
            }
        }

        public EVSdkWrapper EVSdkWrapper { get; set; }

        #endregion

        #region -- Constructor --

        private EVSdkManager()
        {
            this.EVSdkWrapper = EVSdkWrapper.CreateInstance();
            this.EVSdkWrapper.EventManagedLog += EVSdkWrapper_EventManagedLog;

            this.EVSdkWrapper.EventError += EVSdkWrapper_EventError;
            this.EVSdkWrapper.EventWarn += EVSdkWrapper_EventWarn;

            this.EVSdkWrapper.EventNetworkState += EVSdkWrapper_EventNetworkState;
            this.EVSdkWrapper.EventNetworkQuality += EVSdkWrapper_EventNetworkQuality;

            this.EVSdkWrapper.EventLoginSucceed += EVSdkWrapper_EventLoginSucceed;
            this.EVSdkWrapper.EventRegister += EVSdkWrapper_EventRegister;

            //this.EVSdkWrapper.EventCallIncoming += EVSdkWrapper_EventCallIncoming;
            this.EVSdkWrapper.EventCallConnected += EVSdkWrapper_EventCallConnected;
            this.EVSdkWrapper.EventCallPeerConnected += EVSdkWrapper_EventCallPeerConnected;
            this.EVSdkWrapper.EventCallEnd += EVSdkWrapper_EventCallEnd;
            this.EVSdkWrapper.EventContent += EVSdkWrapper_EventContent;

            this.EVSdkWrapper.EventDownloadUserImageComplete += EVSdkWrapper_EventDownloadUserImageComplete;
            this.EVSdkWrapper.EventUploadUserImageComplete += EVSdkWrapper_EventUploadUserImageComplete;

            this.EVSdkWrapper.EventLayoutIndication += EVSdkWrapper_EventLayoutIndication;
            this.EVSdkWrapper.EventLayoutSiteIndication += EVSdkWrapper_EventLayoutSiteIndication;
            this.EVSdkWrapper.EventLayoutSpeakerIndication += EVSdkWrapper_EventLayoutSpeakerIndication;
            this.EVSdkWrapper.EventMuteSpeakingDetected += EVSdkWrapper_EventMuteSpeakingDetected;
            this.EVSdkWrapper.EventJoinConferenceIndication += EVSdkWrapper_EventJoinConferenceIndication;
            //this.EVSdkWrapper.EventConferenceEndIndication += EVSdkWrapper_EventConferenceEndIndication;
            this.EVSdkWrapper.EventRecordingIndication += EVSdkWrapper_EventRecordingIndication;
            this.EVSdkWrapper.EventMessageOverlay += EVSdkWrapper_EventMessageOverlay;
            this.EVSdkWrapper.EventWhiteBoardIndication += EVSdkWrapper_EventWhiteBoardIndication;
            this.EVSdkWrapper.EventParticipant += EVSdkWrapper_EventParticipant;
            this.EVSdkWrapper.EventMicMutedShow += EVSdkWrapper_EventMicMutedShow;
            this.EVSdkWrapper.EventPeerImageUrl += EVSdkWrapper_EventPeerImageUrl;
        }
        
        #endregion

        #region -- Public Method --

        public void CreateEVEngine()
        {
            this.EVSdkWrapper.CreateEVEngine();
        }
        
        //Log
        public void SetLog(ManagedEVSdk.Structs.EV_LOG_LEVEL_CLI level, string logPath, string logFileName, uint maxFileSize)
        {
            _log.InfoFormat("SetLog, level={0}, path={1}, file name={2}, max size={3}", level, logPath, logFileName, maxFileSize);
            this.EVSdkWrapper.EVEngineSetLog(level, logPath, logFileName, maxFileSize);
            _log.Info("SetLog done");
        }

        public void EnableLog(bool enable)
        {
            _log.InfoFormat("EnableLog: {0}", enable);
            this.EVSdkWrapper.EVEngineEnableLog(enable);
            _log.Info("EnableLog done");
        }

        //init
        public void Initialize(string configPath, string configFileName)
        {
            _log.InfoFormat("Initialize, config path:{0}, config file name:{1}", configPath, configFileName);
            int rst = this.EVSdkWrapper.EVEngineInitialize(configPath, configFileName);
            if ((int)ManagedEVSdk.ErrorInfo.EV_ERROR_CLI.EV_OK != rst)
            {
                _log.InfoFormat("Failed to Initialize, result:{0}, config path:{1}, config file name:{2}", rst, configPath, configFileName);
            }
            _log.Info("Initialize done");
        }

        public void SetRootCA(string rootCaPath)
        {
            _log.InfoFormat("SetRootCA, path:{0}", rootCaPath);
            int rst = this.EVSdkWrapper.EVEngineSetRootCA(rootCaPath);
            if ((int)ManagedEVSdk.ErrorInfo.EV_ERROR_CLI.EV_OK != rst)
            {
                _log.InfoFormat("Failed to SetRootCA, result:{0}, root ca path:{1}", rst, rootCaPath);
            }
            _log.Info("SetRootCA done");
        }

        public void SetUserImage(string backgroundFilePath, string userImagePath)
        {
            _log.InfoFormat("SetUserImage, background file path:{0}, user image path:{1}", backgroundFilePath, userImagePath);
            int rst = this.EVSdkWrapper.EVEngineSetUserImage(backgroundFilePath, userImagePath);
            if ((int)ManagedEVSdk.ErrorInfo.EV_ERROR_CLI.EV_OK != rst)
            {
                _log.InfoFormat("Failed to SetUserImage, result:{0}, background file path:{1}, user image path:{2}", rst, backgroundFilePath, userImagePath);
            }
            _log.Info("SetUserImage done");
        }

        public void EnableWhiteBoard(bool enable)
        {
            _log.InfoFormat("EnableWhiteBoard:{0}", enable);
            int rst = this.EVSdkWrapper.EVEngineEnableWhiteBoard(enable);
            if ((int)ManagedEVSdk.ErrorInfo.EV_ERROR_CLI.EV_OK != rst)
            {
                _log.InfoFormat("Failed to EnableWhiteBoard, result:{0}, enable:{1}", rst, enable);
            }
            _log.Info("EnableWhiteBoard done");
        }

        public void SetUserAgent(string company, string version)
        {
            _log.InfoFormat("SetUserAgent, company={0}, version={1}", company, version);
            int rst = this.EVSdkWrapper.EVEngineSetUserAgent(company, version);
            if ((int)ManagedEVSdk.ErrorInfo.EV_ERROR_CLI.EV_OK != rst)
            {
                _log.InfoFormat("Failed to SetUserAgent, result:{0}, company:{1}, version:{2}", rst, company, version);
            }
            _log.Info("SetUserAgent done");
        }

        public void Release()
        {
            _log.Info("Release");
            int rst = this.EVSdkWrapper.EVEngineRelease();
            if ((int)ManagedEVSdk.ErrorInfo.EV_ERROR_CLI.EV_OK != rst)
            {
                _log.InfoFormat("Failed to Release, result:{0}", rst);
            }

            _log.Info("Release done");
        }

        // Login
        public void EnableSecure(bool enable)
        {
            _log.InfoFormat("EnableSecure: {0}", enable);
            int rst = this.EVSdkWrapper.EVEngineEnableSecure(enable);
            if ((int)ManagedEVSdk.ErrorInfo.EV_ERROR_CLI.EV_OK != rst)
            {
                _log.InfoFormat("Failed to EnableSecure, result:{0}, enable:{1}", rst, enable);
            }
            _log.Info("EnableSecure done");
        }
        
        //public string EncryptPassword(string password)
        //{
        //    return this.EVSdkWrapper.EVEngineEncryptPassword(password);
        //}

        public void Login(string server, uint port, string username, string password)
        {
            _log.InfoFormat("Login, server:{0}, port:{1}, username:{2}", server, port, username);
            int rst = this.EVSdkWrapper.EVEngineLogin(server, port, username, password);
            if ((int)ManagedEVSdk.ErrorInfo.EV_ERROR_CLI.EV_OK != rst)
            {
                _log.InfoFormat("Failed to Login, result:{0}, server:{1}, port:{2}, username:{3}", rst, server, port, username);
            }
            _log.Info("Login done");
        }

        public void LoginWithLocation(string locationServer, uint port, string username, string password)
        {
            _log.InfoFormat("LoginWithLocation, location server:{0}, port:{1}, username:{2}", locationServer, port, username);
            int rst = this.EVSdkWrapper.EVEngineLoginWithLocation(locationServer, port, username, password);
            if ((int)ManagedEVSdk.ErrorInfo.EV_ERROR_CLI.EV_OK != rst)
            {
                _log.InfoFormat("Failed to LoginWithLocation, result:{0}, location server:{1}, port:{2}, username:{3}", rst, locationServer, port, username);
            }
            _log.Info("LoginWithLocation done");
        }

        public void Logout()
        {
            _log.Info("Logout");
            int rst = this.EVSdkWrapper.EVEngineLogout();
            if ((int)ManagedEVSdk.ErrorInfo.EV_ERROR_CLI.EV_OK != rst)
            {
                _log.InfoFormat("Failed to logout, result:{0}", rst);
            }
            _log.Info("Logout done");
        }

        public void DownloadUserImage(string path)
        {
            _log.InfoFormat("DownloadUserImage, path={0}", path);
            int rst = this.EVSdkWrapper.EVEngineDownloadUserImage(path);
            if ((int)ManagedEVSdk.ErrorInfo.EV_ERROR_CLI.EV_OK != rst)
            {
                _log.InfoFormat("Failed to DownloadUserImage, result:{0}, path:{1}", rst, path);
            }
            _log.Info("DownloadUserImage done");
        }

        public void UploadUserImage(string path)
        {
            _log.InfoFormat("UploadUserImage, path={0}", path);
            int rst = this.EVSdkWrapper.EVEngineUploadUserImage(path);
            if ((int)ManagedEVSdk.ErrorInfo.EV_ERROR_CLI.EV_OK != rst)
            {
                _log.InfoFormat("Failed to UploadUserImage, result:{0}, path:{1}", rst, path);
            }
            _log.Info("UploadUserImage done");
        }

        public bool ChangePassword(string oldPassword, string newPassword)
        {
            _log.Info("ChangePassword");
            int rst = this.EVSdkWrapper.EVEngineChangePassword(oldPassword, newPassword);
            _log.Info("ChangePassword done");
            if ((int)ManagedEVSdk.ErrorInfo.EV_ERROR_CLI.EV_OK != rst)
            {
                _log.InfoFormat("Failed to ChangePassword, result:{0}", rst);
                return false;
            }
            return true;
        }

        public bool ChangeDisplayName(string displayName)
        {
            _log.InfoFormat("ChangeDisplayName:{0}", displayName);
            int rst = this.EVSdkWrapper.EVEngineChangeDisplayName(displayName);
            _log.Info("ChangeDisplayName done");
            if ((int)ManagedEVSdk.ErrorInfo.EV_ERROR_CLI.EV_OK != rst)
            {
                _log.InfoFormat("Failed to ChangeDisplayName, result:{0}, display name:{1}", rst, displayName);
                return false;
            }
            return true;
        }

        public void GetUserInfo(ref ManagedEVSdk.Structs.EVUserInfoCli userinfo)
        {
            _log.Info("GetUserInfo");
            int rst = this.EVSdkWrapper.EVEngineGetUserInfo(ref userinfo);
            if ((int)ManagedEVSdk.ErrorInfo.EV_ERROR_CLI.EV_OK != rst)
            {
                _log.InfoFormat("Failed to GetUserInfo, result:{0}", rst);
                userinfo = null;
            }
            _log.Info("GetUserInfo done");
        }

        public string GetDisplayName()
        {
            _log.Info("GetDisplayName");
            string displayName = this.EVSdkWrapper.EVEngineGetDisplayName();
            _log.Info("GetDisplayName done");
            return displayName;
        }

        //Device
        public ManagedEVSdk.Structs.EVDeviceCli[] GetDevices(ManagedEVSdk.Structs.EV_DEVICE_TYPE_CLI type)
        {
            _log.InfoFormat("GetDevices: {0}", type);
            EVDeviceCli[] devices = this.EVSdkWrapper.EVEngineGetDevices(type);
            _log.InfoFormat("GetDevices done, size:{0}", devices.Length);
            return devices;
        }

        public ManagedEVSdk.Structs.EVDeviceCli GetDevice(ManagedEVSdk.Structs.EV_DEVICE_TYPE_CLI type)
        {
            _log.InfoFormat("GetDevice: {0}", type);
            EVDeviceCli device = this.EVSdkWrapper.EVEngineGetDevice(type);
            _log.Info("GetDevice done");
            return device;
        }

        public void SetDevice(ManagedEVSdk.Structs.EV_DEVICE_TYPE_CLI type, uint id)
        {
            _log.InfoFormat("SetDevice, type={0}, id={1}", type, id);
            this.EVSdkWrapper.EVEngineSetDevice(type, id);
            _log.Info("SetDevice done");
        }

        public void EnableMicMeter(bool enable)
        {
            _log.InfoFormat("EnableMicMeter:{0}", enable);
            int rst = this.EVSdkWrapper.EVEngineEnableMicMeter(enable);
            if ((int)ManagedEVSdk.ErrorInfo.EV_ERROR_CLI.EV_OK != rst)
            {
                _log.InfoFormat("Failed to EnableMicMeter, enable:{0}, result:{1}", enable, rst);
            }
            _log.Info("EnableMicMeter done");
        }

        public float GetMicVolume()
        {
            _log.Info("GetMicVolume");
            float value = this.EVSdkWrapper.EVEngineGetMicVolume();
            _log.InfoFormat("GetMicVolume done: {0}", value);
            return value;
        }

        //Set Windows
        public void SetLocalVideoWindow(IntPtr id)
        {
            _log.InfoFormat("SetLocalVideoWindow, id={0}", id);
            int rst = this.EVSdkWrapper.EVEngineSetLocalVideoWindow(id);
            if ((int)ManagedEVSdk.ErrorInfo.EV_ERROR_CLI.EV_OK != rst)
            {
                _log.InfoFormat("Failed to SetLocalVideoWindow, id:{0}, result:{1}", id, rst);
            }
            _log.Info("SetLocalVideoWindow done");
        }

        public void SetRemoteVideoWindow(IntPtr[] ids, uint size)
        {
            _log.InfoFormat("SetRemoteVideoWindow, size={0}", size);
            int rst = this.EVSdkWrapper.EVEngineSetRemoteVideoWindow(ids, size);
            if ((int)ManagedEVSdk.ErrorInfo.EV_ERROR_CLI.EV_OK != rst)
            {
                _log.InfoFormat("Failed to SetRemoteVideoWindow, size:{0}, result:{1}", size, rst);
            }
            _log.Info("SetRemoteVideoWindow done");
        }

        public void SetRemoteContentWindow(IntPtr id)
        {
            _log.InfoFormat("SetRemoteContentWindow, id={0}", id);
            int rst = this.EVSdkWrapper.EVEngineSetRemoteContentWindow(id);
            if ((int)ManagedEVSdk.ErrorInfo.EV_ERROR_CLI.EV_OK != rst)
            {
                _log.InfoFormat("Failed to SetRemoteContentWindow, id:{0}, result:{1}", id, rst);
            }
            _log.Info("SetRemoteContentWindow done");
        }

        public void SetLocalContentWindow(IntPtr id, ManagedEVSdk.Structs.EV_CONTENT_MODE_CLI mode)
        {
            _log.InfoFormat("SetLocalContentWindow, id={0}, mode={1}", id, mode);
            int rst = this.EVSdkWrapper.EVEngineSetLocalContentWindow(id, mode);
            if ((int)ManagedEVSdk.ErrorInfo.EV_ERROR_CLI.EV_OK != rst)
            {
                _log.InfoFormat("Failed to SetLocalContentWindow, id:{0}, mode={1} result:{2}", id, mode, rst);
            }
            _log.Info("SetLocalContentWindow done");
        }
        
        //Conference & Layout
        public void EnablePreview(bool enable)
        {
            _log.InfoFormat("EnablePreview:{0}", enable);
            int rst = this.EVSdkWrapper.EVEngineEnablePreview(enable);
            if ((int)ManagedEVSdk.ErrorInfo.EV_ERROR_CLI.EV_OK != rst)
            {
                _log.InfoFormat("Failed to EnablePreview, enable:{0}, result:{1}", enable, rst);
            }
            _log.Info("EnablePreview done");
        }

        public void SetBandwidth(uint kbps)
        {
            _log.InfoFormat("SetBandwidth:{0}", kbps);
            int rst = this.EVSdkWrapper.EVEngineSetBandwidth(kbps);
            if ((int)ManagedEVSdk.ErrorInfo.EV_ERROR_CLI.EV_OK != rst)
            {
                _log.InfoFormat("Failed to SetBandwidth, bandwidth:{0}, result:{1}", kbps, rst);
            }
            _log.Info("SetBandwidth done");
        }

        public uint GetBandwidth()
        {
            _log.Info("GetBandwidth");
            uint bw = this.EVSdkWrapper.EVEngineGetBandwidth();
            _log.InfoFormat("GetBandwidth done: {0}", bw);
            return bw;
        }

        public void SetMaxRecvVideo(uint num)
        {
            _log.InfoFormat("SetMaxRecvVideo:{0}", num);
            int rst = this.EVSdkWrapper.EVEngineSetMaxRecvVideo(num);
            if ((int)ManagedEVSdk.ErrorInfo.EV_ERROR_CLI.EV_OK != rst)
            {
                _log.InfoFormat("Failed to SetMaxRecvVideo, number:{0}, result:{1}", num, rst);
            }
            _log.Info("SetMaxRecvVideo done");
        }

        public void SetLayoutCapacity(ManagedEVSdk.Structs.EV_LAYOUT_MODE_CLI mode, ManagedEVSdk.Structs.EV_LAYOUT_TYPE_CLI[] types)
        {
            _log.InfoFormat("SetLayoutCapacity: {0}", mode);
            int rst = this.EVSdkWrapper.EVEngineSetLayoutCapacity(mode, types);
            if ((int)ManagedEVSdk.ErrorInfo.EV_ERROR_CLI.EV_OK != rst)
            {
                _log.InfoFormat("Failed to SetLayoutCapacity, result: {0}", rst);
            }
            _log.Info("SetLayoutCapacity done");
        }

        public bool JoinConference(string conferenceNumber, string displayName, string password, EV_SVC_CALL_TYPE_CLI type)
        {
            _log.InfoFormat("JoinConference, conf number:{0}, display name:{1}", conferenceNumber, displayName);
            if (null == displayName)
            {
                _log.Info("display name is null and set it to empty");
                displayName = "";
            }
            if (null == password)
            {
                _log.Info("password is null and set it to empty");
                password = "";
            }
            int rst = this.EVSdkWrapper.EVEngineJoinConference(conferenceNumber, displayName, password, type);
            _log.Info("JoinConference done");
            if ((int)ManagedEVSdk.ErrorInfo.EV_ERROR_CLI.EV_OK != rst)
            {
                _log.InfoFormat("Failed to JoinConference, conference number:{0}, display name:{1}, result:{2}", conferenceNumber, displayName, rst);
                return false;
            }
            return true;
        }

        public bool JoinConference(string server, uint port, string conferenceNumber, string displayName, string password)
        {
            _log.InfoFormat("JoinConference with server directly, conf number:{0}, display name:{1}", conferenceNumber, displayName);
            if (null == displayName)
            {
                _log.Info("display name is null and set it to empty");
                displayName = "";
            }
            if (null == password)
            {
                _log.Info("password is null and set it to empty");
                password = "";
            }
            int rst = this.EVSdkWrapper.EVEngineJoinConference(server, port, conferenceNumber, displayName, password);
            _log.Info("JoinConference with server directly done");
            if ((int)ManagedEVSdk.ErrorInfo.EV_ERROR_CLI.EV_OK != rst)
            {
                _log.InfoFormat("Failed to JoinConference, server:{0}, port:{1} conference number:{2}, display name:{3}, result:{4}"
                    , server
                    , port
                    , conferenceNumber
                    , displayName
                    , rst);
                return false;
            }
            return true;
        }

        public bool JoinConferenceWithLocation(string locationServer, uint port, string conferenceNumber, string displayName, string password)
        {
            _log.InfoFormat("JoinConferenceWithLocation, location server:{0}, port:{1}, conf number:{2} display name:{3}", locationServer, port, conferenceNumber, displayName);
            if (null == displayName)
            {
                _log.Info("display name is null and set it to empty");
                displayName = "";
            }
            if (null == password)
            {
                _log.Info("password is null and set it to empty");
                password = "";
            }
            int rst = this.EVSdkWrapper.EVEngineJoinConferenceWithLocation(locationServer, port, conferenceNumber, displayName, password);
            _log.Info("JoinConferenceWithLocation done");
            if ((int)ManagedEVSdk.ErrorInfo.EV_ERROR_CLI.EV_OK != rst)
            {
                _log.InfoFormat("Failed to JoinConferenceWithLocation, location server:{0}, port:{1} conference number:{2}, display name:{3}, result:{4}"
                    , locationServer
                    , port
                    , conferenceNumber
                    , displayName
                    , rst );
                return false;
            }
            return true;
        }

        public void LeaveConference()
        {
            _log.Info("LeaveConference");
            int rst = this.EVSdkWrapper.EVEngineLeaveConference();
            if ((int)ManagedEVSdk.ErrorInfo.EV_ERROR_CLI.EV_OK != rst)
            {
                _log.InfoFormat("Failed to LeaveConference, result:{0}", rst);
            }
            _log.Info("LeaveConference done");
        }
        
        public void DeclineIncommingCall(string confNumber)
        {
            _log.InfoFormat("DeclineIncommingCall, confNumber: {0}", confNumber);
            int rst = this.EVSdkWrapper.EVEngineDeclineIncommingCall(confNumber);
            if ((int)ManagedEVSdk.ErrorInfo.EV_ERROR_CLI.EV_OK != rst)
            {
                _log.InfoFormat("Failed to DeclineIncommingCall, result:{0}", rst);
            }
            _log.Info("DeclineIncommingCall done");
        }

        public bool CameraEnabled()
        {
            _log.Info("CameraEnabled");
            bool enabled = this.EVSdkWrapper.EVEngineCameraEnabled();
            _log.InfoFormat("CameraEnabled done:{0}", enabled);
            return enabled;
        }

        public void EnableCamera(bool enable)
        {
            _log.InfoFormat("EnableCamera:{0}", enable);
            int rst = this.EVSdkWrapper.EVEngineEnableCamera(enable);
            if ((int)ManagedEVSdk.ErrorInfo.EV_ERROR_CLI.EV_OK != rst)
            {
                _log.InfoFormat("Failed to EnableCamera, enable:{0}, result:{1}", enable, rst);
            }
            _log.Info("EnableCamera done");
        }

        public bool MicEnabled()
        {
            _log.Info("MicEnabled");
            bool enabled =  this.EVSdkWrapper.EVEngineMicEnabled();
            _log.InfoFormat("MicEnabled done:{0}", enabled);
            return enabled;
        }

        public void EnableMic(bool enable)
        {
            _log.InfoFormat("EnableMic:{0}", enable);
            int rst = this.EVSdkWrapper.EVEngineEnableMic(enable);
            if ((int)ManagedEVSdk.ErrorInfo.EV_ERROR_CLI.EV_OK != rst)
            {
                _log.InfoFormat("Failed to EnableMic, enable:{0}, result:{1}", enable, rst);
            }
            _log.Info("EnableMic done");
        }

        public bool RemoteMuted()
        {
            _log.Info("RemoteMuted");
            bool muted = this.EVSdkWrapper.EVEngineRemoteMuted();
            _log.InfoFormat("RemoteMuted: {0}", muted);
            return muted;
        }

        public void RequestRemoteUnmute(bool val)
        {
            _log.InfoFormat("RequestRemoteUnmute:{0}", val);
            int rst = this.EVSdkWrapper.EVEngineRequestRemoteUnmute(val);
            if ((int)ManagedEVSdk.ErrorInfo.EV_ERROR_CLI.EV_OK != rst)
            {
                _log.InfoFormat("Failed to RequestRemoteUnmute, value:{0}, result:{1}", val, rst);
            }
            _log.Info("RequestRemoteUnmute done");
        }

        public bool HighFPSEnabled()
        {
            _log.Info("HighFPSEnabled");
            bool rst = this.EVSdkWrapper.EVEngineHighFPSEnabled();
            _log.InfoFormat("HighFPSEnabled done: {0}", rst);
            return rst;
        }

        public void EnableHighFPS(bool enable)
        {
            _log.InfoFormat("EnableHighFPS: {0}", enable);
            int rst = this.EVSdkWrapper.EVEngineEnableHighFPS(enable);
            if ((int)ManagedEVSdk.ErrorInfo.EV_ERROR_CLI.EV_OK != rst)
            {
                _log.InfoFormat("Failed to EnableHighFPS, enable:{0}, result:{1}", enable, rst);
            }
            _log.Info("EnableHighFPS done");
        }

        public bool HDEnabled()
        {
            _log.Info("Get HDEnabled");
            bool rst = this.EVSdkWrapper.EVEngineHDEnabled();
            _log.InfoFormat("HDEnabled: {0}", rst);
            return rst;
        }

        public void EnableHD(bool enable)
        {
            _log.InfoFormat("EnableHD: {0}", enable);
            int rst = this.EVSdkWrapper.EVEngineEnableHD(enable);
            if ((int)ManagedEVSdk.ErrorInfo.EV_ERROR_CLI.EV_OK != rst)
            {
                _log.InfoFormat("Failed to EnableHD, enable:{0}, result:{1}", enable, rst);
            }
            _log.Info("EnableHD done");
        }

        public void SetLayout(ManagedEVSdk.Structs.EVLayoutRequestCli layout)
        {
            _log.InfoFormat("SetLayout, mode: {0}, max_type: {1}", layout.mode, layout.max_type);
            int rst = this.EVSdkWrapper.EVEngineSetLayout(layout);
            if ((int)ManagedEVSdk.ErrorInfo.EV_ERROR_CLI.EV_OK != rst)
            {
                _log.InfoFormat("Failed to SetLayout, result:{0}", rst);
            }
            _log.Info("SetLayout done");
        }
        
        public void GetStats(ref ManagedEVSdk.Structs.EVStatsCli stats)
        {
            _log.Info("GetStats");
            int rst = this.EVSdkWrapper.EVEngineGetStats(ref stats);
            if ((int)ManagedEVSdk.ErrorInfo.EV_ERROR_CLI.EV_OK != rst)
            {
                _log.InfoFormat("Failed to GetStats, result:{0}", rst);
            }
            _log.Info("GetStats done");
        }

        public void SetVideoActive(MediaModeType mediaMode)
        {
            _log.InfoFormat("SetVideoActive: {0}", mediaMode);
            int rst = this.EVSdkWrapper.EVEngineSetVideoActive((int)mediaMode);
            if ((int)ManagedEVSdk.ErrorInfo.EV_ERROR_CLI.EV_OK != rst)
            {
                _log.InfoFormat("Failed to SetVideoActive, result:{0}", rst);
            }
            _log.Info("SetVideoActive done");
        }

        public MediaModeType VideoActive()
        {
            _log.Info("Get VideoActive");
            int rst = this.EVSdkWrapper.EVEngineVideoActive();
            _log.InfoFormat("VideoActive: {0}", rst);
            return (MediaModeType)rst;
        }

        public bool SetInConfDisplayName(string displayName)
        {
            _log.InfoFormat("SetInConfDisplayName: {0}", displayName);
            int rst = this.EVSdkWrapper.EVEngineSetInConfDisplayName(displayName);
            _log.InfoFormat("SetInConfDisplayName done: {0}", rst);

            return (int)ManagedEVSdk.ErrorInfo.EV_ERROR_CLI.EV_OK == rst;
        }

        //Send Content
        public void SendContent()
        {
            _log.Info("SendContent");
            int rst = this.EVSdkWrapper.EVEngineSendContent();
            if ((int)ManagedEVSdk.ErrorInfo.EV_ERROR_CLI.EV_OK != rst)
            {
                _log.InfoFormat("Failed to SendContent, result:{0}", rst);
            }
            _log.Info("SendContent done");
        }

        public void SendWhiteBoard()
        {
            _log.Info("SendWhiteBoard");
            int rst = this.EVSdkWrapper.EVEngineSendWhiteBoard();
            if ((int)ManagedEVSdk.ErrorInfo.EV_ERROR_CLI.EV_OK != rst)
            {
                _log.InfoFormat("Failed to SendWhiteBoard, result:{0}", rst);
            }
            _log.Info("SendWhiteBoard done");
        }

        public void StopContent()
        {
            _log.Info("StopContent");
            int rst = this.EVSdkWrapper.EVEngineStopContent();
            if ((int)ManagedEVSdk.ErrorInfo.EV_ERROR_CLI.EV_OK != rst)
            {
                _log.InfoFormat("Failed to StopContent, result:{0}", rst);
            }
            _log.Info("StopContent done");
        }

        public void EnableContentAudio(bool enable)
        {
            _log.Info("EnableContentAudio");
            int rst = this.EVSdkWrapper.EVEngineEnableContentAudio(enable);
            if ((int)ManagedEVSdk.ErrorInfo.EV_ERROR_CLI.EV_OK != rst)
            {
                _log.InfoFormat("Failed to EnableContentAudio, result:{0}", rst);
            }
            _log.Info("EnableContentAudio done");
        }

        public bool ContentAudioEnabled()
        {
            _log.Info("ContentAudioEnabled");
            bool rst = this.EVSdkWrapper.EVEngineContentAudioEnabled();
            _log.InfoFormat("ContentAudioEnabled done: {0}", rst);
            return rst;
        }

        #endregion

        #region -- Private Method --

        private void EVSdkWrapper_EventManagedLog(string logStr)
        {
            _log.InfoFormat("Managed Sdk -- {0}", logStr);
        }

        private void EVSdkWrapper_EventError(EVErrorCli error)
        {
            WorkerThreadManager.Instance.EVSdkWorkDispatcher.InvokeAsync(() => {
                EventError?.Invoke(error);
            });
        }

        private void EVSdkWrapper_EventWarn(EVWarnCli warn)
        {
            WorkerThreadManager.Instance.EVSdkWorkDispatcher.InvokeAsync(() => {
                EventWarn?.Invoke(warn);
            });
        }

        private void EVSdkWrapper_EventNetworkState(bool reachable)
        {
            WorkerThreadManager.Instance.EVSdkWorkDispatcher.InvokeAsync(() => {
                EventNetworkState?.Invoke(reachable);
            });
        }

        private void EVSdkWrapper_EventNetworkQuality(float rating)
        {
            WorkerThreadManager.Instance.EVSdkWorkDispatcher.InvokeAsync(() => {
                EventNetworkQuality?.Invoke(rating);
            });
        }

        private void EVSdkWrapper_EventLoginSucceed(EVUserInfoCli userInfo)
        {
            WorkerThreadManager.Instance.EVSdkWorkDispatcher.InvokeAsync(() => {
                EventLoginSucceed?.Invoke(userInfo);
            });
        }

        private void EVSdkWrapper_EventRegister(bool registered)
        {
            WorkerThreadManager.Instance.EVSdkWorkDispatcher.InvokeAsync(() => {
                EventRegister?.Invoke(registered);
            });
        }

        //private void EVSdkWrapper_EventCallIncoming(EVCallInfoCli callInfo)
        //{
        //    WorkerThreadManager.Instance.EVSdkWorkDispatcher.InvokeAsync(() => {
        //        EventCallIncoming?.Invoke(callInfo);
        //    });
        //}

        private void EVSdkWrapper_EventCallConnected(EVCallInfoCli callInfo)
        {
            _log.InfoFormat("EventCallConnected: {0}", GetCallInfoString(callInfo));
            WorkerThreadManager.Instance.EVSdkWorkDispatcher.InvokeAsync(() => {
                EventCallConnected?.Invoke(callInfo);
            });
        }


        private void EVSdkWrapper_EventCallPeerConnected(EVCallInfoCli callInfo)
        {
            _log.InfoFormat("EventCallPeerConnected: {0}", GetCallInfoString(callInfo));
            WorkerThreadManager.Instance.EVSdkWorkDispatcher.InvokeAsync(() => {
                EventCallPeerConnected?.Invoke(callInfo);
            });
        }

        private void EVSdkWrapper_EventCallEnd(EVCallInfoCli callInfo)
        {
            _log.InfoFormat("EventCallEnd: {0}", GetCallInfoString(callInfo));
            WorkerThreadManager.Instance.EVSdkWorkDispatcher.InvokeAsync(() => {
                EventCallEnd?.Invoke(callInfo);
            });
        }

        private void EVSdkWrapper_EventContent(EVContentInfoCli contentInfo)
        {
            WorkerThreadManager.Instance.EVSdkWorkDispatcher.InvokeAsync(() => {
                EventContent?.Invoke(contentInfo);
            });
        }


        private void EVSdkWrapper_EventDownloadUserImageComplete(string path)
        {
            WorkerThreadManager.Instance.EVSdkWorkDispatcher.InvokeAsync(() => {
                EventDownloadUserImageComplete?.Invoke(path);
            });
        }

        private void EVSdkWrapper_EventUploadUserImageComplete(string path)
        {
            WorkerThreadManager.Instance.EVSdkWorkDispatcher.InvokeAsync(() => {
                EventUploadUserImageComplete?.Invoke(path);
            });
        }

        private void EVSdkWrapper_EventLayoutIndication(EVLayoutIndicationCli layoutIndication)
        {
            WorkerThreadManager.Instance.EVSdkWorkDispatcher.InvokeAsync(() => {
                EventLayoutIndication?.Invoke(layoutIndication);
            });
        }

        private void EVSdkWrapper_EventLayoutSiteIndication(EVSiteCli site)
        {
            WorkerThreadManager.Instance.EVSdkWorkDispatcher.InvokeAsync(() => {
                EventLayoutSiteIndication?.Invoke(site);
            });
        }

        private void EVSdkWrapper_EventLayoutSpeakerIndication(EVLayoutSpeakerIndicationCli speakerIndication)
        {
            WorkerThreadManager.Instance.EVSdkWorkDispatcher.InvokeAsync(() => {
                EventLayoutSpeakerIndication?.Invoke(speakerIndication);
            });
        }

        private void EVSdkWrapper_EventMuteSpeakingDetected()
        {
            WorkerThreadManager.Instance.EVSdkWorkDispatcher.InvokeAsync(() =>
            {
                EventMuteSpeakingDetected?.Invoke();
            });
        }

        private void EVSdkWrapper_EventJoinConferenceIndication(EVCallInfoCli callInfo)
        {
            _log.InfoFormat("EventJoinConferenceIndication: {0}", GetCallInfoString(callInfo));
            WorkerThreadManager.Instance.EVSdkWorkDispatcher.InvokeAsync(() => {
                EventJoinConferenceIndication?.Invoke(callInfo);
            });
        }

        //private void EVSdkWrapper_EventConferenceEndIndication(int confEndIndication)
        //{
        //    WorkerThreadManager.Instance.EVSdkWorkDispatcher.InvokeAsync(() => {
        //        EventConferenceEndIndication?.Invoke(confEndIndication);
        //    });
        //}

        private void EVSdkWrapper_EventRecordingIndication(EVRecordingInfoCli recordingInfo)
        {
            WorkerThreadManager.Instance.EVSdkWorkDispatcher.InvokeAsync(() => {
                EventRecordingIndication?.Invoke(recordingInfo);
            });
        }

        private void EVSdkWrapper_EventMessageOverlay(EVMessageOverlayCli messageOverlay)
        {
            WorkerThreadManager.Instance.EVSdkWorkDispatcher.InvokeAsync(() => {
                EventMessageOverlay?.Invoke(messageOverlay);
            });
        }

        private void EVSdkWrapper_EventWhiteBoardIndication(EVWhiteBoardInfoCli whiteBoardInfo)
        {
            WorkerThreadManager.Instance.EVSdkWorkDispatcher.InvokeAsync(() => {
                EventWhiteBoardIndication?.Invoke(whiteBoardInfo);
            });
        }

        private void EVSdkWrapper_EventParticipant(int number)
        {
            WorkerThreadManager.Instance.EVSdkWorkDispatcher.InvokeAsync(() => {
                EventParticipant?.Invoke(number);
            });
        }

        private void EVSdkWrapper_EventMicMutedShow(int micMuted)
        {
            WorkerThreadManager.Instance.EVSdkWorkDispatcher.InvokeAsync(() => {
                EventRemoteMicMuted?.Invoke(0 == micMuted);
            });
        }

        private void EVSdkWrapper_EventPeerImageUrl(string imageUrl)
        {
            WorkerThreadManager.Instance.EVSdkWorkDispatcher.InvokeAsync(() => {
                EventPeerImageUrl?.Invoke(imageUrl);
            });
        }

        private string GetCallInfoString(EVCallInfoCli callInfo)
        {
            if (null == callInfo)
            {
                return "";
            }

            StringBuilder sb = new StringBuilder();
            sb.Append("conference_number: ")
                .Append(callInfo.conference_number)
                .Append(", contentEnabled: ")
                .Append(callInfo.contentEnabled)
                .Append(", isAudioOnly: ")
                .Append(callInfo.isAudioOnly)
                .Append(", isBigConference: ")
                .Append(callInfo.isBigConference)
                .Append(", isRemoteMuted: ")
                .Append(callInfo.isRemoteMuted)
                .Append(", peer: ")
                .Append(callInfo.peer)
                .Append(", svcCallType: ")
                .Append(callInfo.svcCallType)
                .Append(", svcCallAction: ")
                .Append(callInfo.svcCallAction);
            if (null != callInfo.err)
            {
                sb.Append(", err.type: ")
                    .Append(callInfo.err.type)
                    .Append(", err.code: ")
                    .Append(callInfo.err.code);
            }

            return sb.ToString();
        }

        #endregion
    }
}
