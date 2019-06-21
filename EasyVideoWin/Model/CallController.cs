using System;
using System.ComponentModel;
using EasyVideoWin.Model.CloudModel;
using EasyVideoWin.Helpers;
using log4net;
using Microsoft.WindowsAPICodePack.ApplicationServices;
using EasyVideoWin.View;
using EasyVideoWin.CustomControls;

namespace EasyVideoWin.Model
{
    public enum CallStatus
    {
        Idle,
        Dialing,
        Connected,
        Ended
    }

    public enum ContentStreamStatus
    {
        Idle
        , ReceivingContentStarted
        , ReceivingWhiteBoardStarted
        , SendingContentStarted
        , SendingWhiteBoardStarted
    }

    public enum MaxRecvVideoLayout
    {
        Layout_3x3      = 9
        , Layout_4x4    = 16
    }

    public class ContentStreamInfo
    {
        public ContentStreamStatus LastStatus { get; set; }
        public ContentStreamStatus CurrentStatus { get; set; }
    }
    
    class CallController : INotifyPropertyChanged
    {
        // code for Signelton, replace this part with IOC container later
        private static CallController _instance = new CallController();

        private readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        
        private enum JoinConfTypeEnum
        {
            Normal
            , Direct
            , Location
        }

        private JoinConfTypeEnum _joinConfType;

        public bool IsConnected2WhiteBoard { get; set; } = false;
        public CloudModel.CloudRest.AcsRest acsInfo;
        public bool _retryToGetAcsInfo = false;
        
        private System.Media.SoundPlayer RingtonePlayer = new System.Media.SoundPlayer();

        private string _whiteBoardPresenter;
        public string WhiteBoardPresenter {
            get {
                return _whiteBoardPresenter;
            }
            set {
                _whiteBoardPresenter = value;
                OnPropertyChanged("WhiteBoardPresenter");
            }
        }

        //private Dispatcher _workerDispatcher;
        
        private CallStatus _callStatus = CallStatus.Idle;
        public CallStatus CurrentCallStatus
        {
            get
            {
                return _callStatus;
            }
            set
            {
                if (value != _callStatus)
                {
                    log.InfoFormat("Call status changed from {0} to {1}", _callStatus, value);
                    _callStatus = value;
                    CallStatusChanged?.Invoke(this, _callStatus);
                }
            }
        }

        private string  _conferenceServer;
        private uint    _conferencePort;
        private string  _conferenceDisplayName;
        private string  _conferenceNumber;
        public string ConferenceNumber {
            get
            {
                return _conferenceNumber;
            }
            set
            {
                _conferenceNumber = value;
                OnPropertyChanged("ConferenceNumber");
            }
        }
        
        private ContentStreamStatus _currentContentStreamStatus;
        public ContentStreamStatus CurrentContentStreamStatus
        {
            get
            {
                return _currentContentStreamStatus;
            }
            set
            {
                log.InfoFormat("change content stream status from {0} to {1}", _currentContentStreamStatus, value);
                if (_currentContentStreamStatus != value)
                {
                    var lastStatus = _currentContentStreamStatus;
                    _currentContentStreamStatus = value;
                    ContentStreamStatusChanged?.Invoke(this, new ContentStreamInfo() { LastStatus = lastStatus, CurrentStatus = value });
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private CallController()
        {
            EVSdkManager.Instance.EventCallConnected += EVSdkWrapper_EventCallConnected;
            EVSdkManager.Instance.EventCallEnd += EVSdkWrapper_EventCallEnd;
            EVSdkManager.Instance.EventContent += EVSdkWrapper_EventContent;
            
            PowerManager.IsMonitorOnChanged += new EventHandler(MonitorOnChanged);

            //Microsoft.Win32.SystemEvents.PowerModeChanged += SystemEvents_PowerModeChanged;
            //ManualResetEvent dispatcherReadyEvent = new ManualResetEvent(false);

            //Thread t = new Thread(new ThreadStart(() =>
            //{
            //    _workerDispatcher = Dispatcher.CurrentDispatcher;
            //    dispatcherReadyEvent.Set();
            //    Dispatcher.Run();
            //}));

            //t.IsBackground = true;
            //t.Start();

            //dispatcherReadyEvent.WaitOne();
        }
        
        //private void SystemEvents_PowerModeChanged(object sender, Microsoft.Win32.PowerModeChangedEventArgs e)
        private void MonitorOnChanged(object sender, EventArgs e)
        {
            if (PowerManager.IsMonitorOn)
            {
                return;
            }
            // stop sending content.
            if (CurrentCallStatus == CallStatus.Connected)
            {
                if (ContentStreamStatus.SendingContentStarted == CurrentContentStreamStatus || ContentStreamStatus.SendingWhiteBoardStarted == CurrentContentStreamStatus)
                {
                    StopContent();                    
                }
                log.Info("Terminate call for system power turned off monitor");
                // terminate call, white board will be cleared automatically.
                TerminateCall();
                App.Current.Dispatcher.InvokeAsync(() => {
                    MessageBoxTip tip;
                    if (
                           LoginProgressEnum.EnterpriseJoinConf == LoginManager.Instance.LoginProgress
                        || LoginProgressEnum.CloudJoinConf == LoginManager.Instance.LoginProgress
                    )
                    {
                        tip = new MessageBoxTip((IMasterDisplayWindow)LoginManager.Instance.LoginWindow);
                    }
                    else
                    {
                        tip = new MessageBoxTip((MainWindow)App.Current.MainWindow);
                    }
                    string errPrompt = LanguageUtil.Instance.GetValueByKey("MONITOR_OFF_CALL_HANGUP");
                    tip.SetTitleAndMsg(LanguageUtil.Instance.GetValueByKey("PROMPT"), errPrompt, LanguageUtil.Instance.GetValueByKey("CONFIRM"));
                    tip.ShowDialog();
                });
            }
        }
        
        public static CallController Instance
        {
            get
            {
                return _instance;
            }
        }
        
        public EventHandler<CallStatus> CallStatusChanged;
        public EventHandler<ContentStreamInfo> ContentStreamStatusChanged;

        private void EVSdkWrapper_EventCallConnected(ManagedEVSdk.Structs.EVCallInfoCli callInfo)
        {
            log.InfoFormat("EventCallConnected. Peer:{0}", callInfo.peer);
            ConferenceNumber = callInfo.peer;
            CurrentContentStreamStatus = ContentStreamStatus.Idle;
            CurrentCallStatus = CallStatus.Connected;
            log.Info("EventCallConnected end");
        }
        
        private void EVSdkWrapper_EventContent(ManagedEVSdk.Structs.EVContentInfoCli contentInfo)
        {
            log.InfoFormat("EventContent, enabled: {0}, direction: {1}, type: {2}, status:{3}", contentInfo.enabled, contentInfo.dir, contentInfo.type, contentInfo.status);
            if (!contentInfo.enabled)
            {
                CurrentContentStreamStatus = ContentStreamStatus.Idle;
            }
            else
            {
                if (ManagedEVSdk.Structs.EV_STREAM_DIR_CLI.EV_STREAM_UPLOAD == contentInfo.dir)
                {
                    if (ManagedEVSdk.Structs.EV_STREAM_TYPE_CLI.EV_STREAM_WHITE_BOARD == contentInfo.type)
                    {
                        CurrentContentStreamStatus = ContentStreamStatus.SendingWhiteBoardStarted;
                    }
                    else if (ManagedEVSdk.Structs.EV_STREAM_TYPE_CLI.EV_STREAM_CONTENT == contentInfo.type)
                    {
                        CurrentContentStreamStatus = ContentStreamStatus.SendingContentStarted;
                    }
                }
                else
                {
                    if (ManagedEVSdk.Structs.EV_STREAM_TYPE_CLI.EV_STREAM_WHITE_BOARD == contentInfo.type)
                    {
                        CurrentContentStreamStatus = ContentStreamStatus.ReceivingWhiteBoardStarted;
                    }
                    else if (ManagedEVSdk.Structs.EV_STREAM_TYPE_CLI.EV_STREAM_CONTENT == contentInfo.type)
                    {
                        CurrentContentStreamStatus = ContentStreamStatus.ReceivingContentStarted;
                    }
                }
            }
            log.Info("EventContent end.");
        }

        private void EVSdkWrapper_EventCallEnd(ManagedEVSdk.Structs.EVCallInfoCli callInfo)
        {
            log.Info("EventCallEnd.");
            if (   
                   null != callInfo.err 
                && callInfo.err.type == ManagedEVSdk.ErrorInfo.EV_ERROR_TYPE_CLI.EV_ERROR_TYPE_CALL
                && callInfo.err.code == (int)ManagedEVSdk.ErrorInfo.EV_CALL_ERROR_CLI.EV_CALL_INVALID_PASSWORD
            )
            {
                log.Info("password empty!");
                App.Current.Dispatcher.InvokeAsync(() =>
                {
                    InputConfPasswordPromptDialog passwordDialog = new InputConfPasswordPromptDialog();
                    passwordDialog.Owner = VideoPeopleWindow.Instance;

                    bool? result = passwordDialog.ShowDialog();
                    if (null != result && result.Value)
                    {
                        if (JoinConfTypeEnum.Direct == _joinConfType)
                        {
                            JoinConference(_conferenceServer, _conferencePort, ConferenceNumber, _conferenceDisplayName, passwordDialog.ConfPassword);
                        }
                        else if (JoinConfTypeEnum.Location == _joinConfType)
                        {
                            JoinConferenceWithLocation(_conferenceServer, _conferencePort, ConferenceNumber, _conferenceDisplayName, passwordDialog.ConfPassword);
                        }
                        else
                        {
                            JoinConference(ConferenceNumber, _conferenceDisplayName, passwordDialog.ConfPassword);
                        }
                    }
                    else
                    {
                        CurrentCallStatus = CallStatus.Ended;
                        if (LoginStatus.AnonymousLoggedIn == LoginManager.Instance.CurrentLoginStatus)
                        {
                            LoginManager.Instance.CurrentLoginStatus = LoginStatus.NotLogin;
                        }
                    }
                });
            }
            else
            {
                CurrentCallStatus = CallStatus.Ended;
                if (LoginStatus.AnonymousLoggedIn == LoginManager.Instance.CurrentLoginStatus)
                {
                    LoginManager.Instance.CurrentLoginStatus = LoginStatus.NotLogin;
                }
            }

            log.InfoFormat("EventCallEnd end.");
        }
        
        public void JoinConference(string confNumber, string displayName, string password)
        {
            log.InfoFormat("Join conference. conf number:{0}, display name:{1}", confNumber, displayName);

            _conferenceDisplayName = displayName;
            ConferenceNumber = confNumber;
            CurrentCallStatus = CallStatus.Dialing;

            bool rst = EVSdkManager.Instance.JoinConference(confNumber, displayName, password);
            if (!rst)
            {
                log.Info("Failed to join conference and change call status to ended.");
                CurrentCallStatus = CallStatus.Ended;
            }
        }

        public void JoinConference(string server, uint port, string conferenceNumber, string displayName, string password)
        {
            log.InfoFormat("Join conference. server:{0}, port:{1} conf number:{2}, display name:{3}", server, port, conferenceNumber, displayName);
            _joinConfType = JoinConfTypeEnum.Direct;
            _conferenceServer = server;
            _conferencePort = port;
            _conferenceDisplayName = displayName;
            ConferenceNumber = conferenceNumber;
            CurrentCallStatus = CallStatus.Dialing;
            bool rst = EVSdkManager.Instance.JoinConference(server, port, conferenceNumber, displayName, password);
            if (!rst)
            {
                log.Info("Failed to join conference directly and change call status to ended.");
                CurrentCallStatus = CallStatus.Ended;
            }
        }

        public void JoinConferenceWithLocation(string locationServer, uint port, string conferenceNumber, string displayName, string password)
        {
            log.InfoFormat("Join conference with location. location server:{0}, port:{1} conf number:{2}, display name:{3}", locationServer, port, conferenceNumber, displayName);
            _joinConfType = JoinConfTypeEnum.Location;
            _conferenceServer = locationServer;
            _conferencePort = port;
            _conferenceDisplayName = displayName;
            ConferenceNumber = conferenceNumber;
            CurrentCallStatus = CallStatus.Dialing;
            bool rst = EVSdkManager.Instance.JoinConferenceWithLocation(locationServer, port, conferenceNumber, displayName, password);
            if (!rst)
            {
                log.Info("Failed to join conference with location and change call status to ended.");
                CurrentCallStatus = CallStatus.Ended;
            }
        }

        public void TerminateCall()
        {
            log.Info("Stop the call!");
            EVSdkManager.Instance.LeaveConference();
        }
        
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        
        public void EnableMic(bool enable)
        {
            EVSdkManager.Instance.EnableMic(enable);
        }

        public void EnableCamera(bool enable)
        {
            EVSdkManager.Instance.EnableCamera(enable);
        }
        
        public void SetLocalVideoWindow(IntPtr handle)
        {
            EVSdkManager.Instance.SetLocalVideoWindow(handle);
        }

        public void SetRemoteContentWindow(IntPtr handle)
        {
            EVSdkManager.Instance.SetRemoteContentWindow(handle);
        }

        public void UpdateUserImage(string background, string front)
        {
            log.Info("Update user image");
            EVSdkManager.Instance.SetUserImage(background, front);
        }
        
        public void GetAcsInfo()
        {
            string boardRoom = ConferenceNumber;
            log.InfoFormat("Get acs info for white board on server:{0}, conference number:{1}",
                CloudApiManager.Instance.DoradoZoneAddress, boardRoom);
            acsInfo = CloudApiManager.Instance.GetAcsInfoByCallNumber(boardRoom, LoginManager.Instance.DeviceId, handleAcsErrMsg);
            int retryCount = 0;
            while ((acsInfo == null) && _retryToGetAcsInfo) 
            {
                retryCount++;
                log.InfoFormat("get acsInfo failed, retry count:{0}", retryCount); 
                System.Threading.Thread.Sleep(1500); //1.5s try one time.
                acsInfo = CloudApiManager.Instance.GetAcsInfoByCallNumber(boardRoom, LoginManager.Instance.DeviceId, handleAcsErrMsg);
                if (retryCount > 10)
                    _retryToGetAcsInfo = false;
            }
            log.InfoFormat("Get acs info from Dorado:{0}", (acsInfo !=null ? acsInfo.toString():null ));  
        }

        public void handleAcsErrMsg(EasyVideoWin.HttpUtils.RestResponse response)
        {
            String errPrompt = "";
            if (string.IsNullOrEmpty(response.Content))
            {
                errPrompt = response.StatusCode.ToString();
                log.ErrorFormat("Request to remote server error, status code:{0}", errPrompt);
            }
            else
            {
                log.Info("No error content for get acs info.");
            }
            _retryToGetAcsInfo = true;
            log.Error("Request ACS info failed, clear acsInfo.");
            acsInfo = null;
        }

        public void SetVideoWindowIds(IntPtr[] ids)
        {
            EVSdkManager.Instance.SetRemoteVideoWindow(ids, (uint)ids.Length);
        }
        
        public void PlayRingtone()
        {
            // "Resources/sounds/ringtone.wav"
            string path = AppDomain.CurrentDomain.BaseDirectory + "Resources\\sounds\\ringtone.wav";
            RingtonePlayer.SoundLocation = path;
            try
            {
                RingtonePlayer.PlayLooping();
            }
            catch (Exception e)
            {
                //System.Windows.Forms.MessageBox.Show(string.Format("Error:\n {0}", e.Message), "Error");
                CustomControls.MessageBoxTip tip = new CustomControls.MessageBoxTip(((MainWindow)System.Windows.Application.Current.MainWindow).GetCurrentMainDisplayWindow());
                tip.SetTitleAndMsg(LanguageUtil.Instance.GetValueByKey("PROMPT"), string.Format("Error:\n {0}", e.Message), LanguageUtil.Instance.GetValueByKey("CONFIRM"));
                tip.ShowDialog();
            }
        }

        public void StopRingtone()
        {
            RingtonePlayer.Stop();
        }

        public void SetPeopleHighFrameRate(bool highFrameRate)
        {
            EVSdkManager.Instance.EnableHighFPS(highFrameRate);
        }
        
        public void SendContent()
        {
            EVSdkManager.Instance.SendContent();
        }

        public void SendWhiteBoard()
        {
            EVSdkManager.Instance.SendWhiteBoard();
        }

        public void StopContent()
        {
            log.Info("Invoking stop content.");
            EVSdkManager.Instance.StopContent();
            CurrentContentStreamStatus = ContentStreamStatus.Idle;
        }
        
        public bool IsMicEnabled()
        {
            return EVSdkManager.Instance.MicEnabled();
        }

        public bool IsCameraEnabled()
        {
            return EVSdkManager.Instance.CameraEnabled();
        }

        public void SetMaxRecvVideo(uint num)
        {
            EVSdkManager.Instance.SetMaxRecvVideo(num);
        }
    }
}
