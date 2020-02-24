using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EasyVideoWin.Model;
using EasyVideoWin.Helpers;
using System.ComponentModel;
using System.Windows;
using System.Collections.ObjectModel;
using System.Threading;
using System.Windows.Threading;
using log4net;

namespace EasyVideoWin.ViewModel
{
    class DialingViewModel : ViewModelBase
    {
        #region -- Members --
        private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private string _conferenceNumber;
        private string _backgroundImage;
        private float _backgroundOpacity;
        private Visibility _headerImageVisibility;
        private string _foreground;
        private Visibility _videoAnswerVisibility;
        private Visibility _confNumberVisibility;
        private string _peerDisplayName;
        private string _peerImageUrl;
        private string _invitingInfo;
        #endregion

        #region -- Properties --

        public RelayCommand HangupCommand { get; private set; }
        public RelayCommand VideoAnswerCommand { get; private set; }

        public string ConferenceNumber
        {
            get
            {
                return _conferenceNumber;
            }
            private set
            {
                _conferenceNumber = value;
                OnPropertyChanged("ConferenceNumber");
            }
        }

        public string BackgroundImage
        {
            get
            {
                return _backgroundImage;
            }
            private set
            {
                _backgroundImage = value;
                OnPropertyChanged("BackgroundImage");
            }
        }

        public float BackgroundOpacity
        {
            get
            {
                return _backgroundOpacity;
            }
            private set
            {
                _backgroundOpacity = value;
                OnPropertyChanged("BackgroundOpacity");
            }
        }

        public Visibility HeaderImageVisibility
        {
            get
            {
                return _headerImageVisibility;
            }
            private set
            {
                _headerImageVisibility = value;
                OnPropertyChanged("HeaderImageVisibility");
            }
        }
        
        public string Foreground
        {
            get
            {
                return _foreground;
            }
            private set
            {
                _foreground = value;
                OnPropertyChanged("Foreground");
            }
        }

        public Visibility VideoAnswerVisibility
        {
            get
            {
                return _videoAnswerVisibility;
            }
            private set
            {
                _videoAnswerVisibility = value;
                OnPropertyChanged("VideoAnswerVisibility");
            }
        }

        public Visibility ConfNumberVisibility
        {
            get
            {
                return _confNumberVisibility;
            }
            private set
            {
                _confNumberVisibility = value;
                OnPropertyChanged("ConfNumberVisibility");
            }
        }

        public string PeerDisplayName
        {
            get
            {
                return _peerDisplayName;
            }
            private set
            {
                _peerDisplayName = value;
                OnPropertyChanged("PeerDisplayName");
            }
        }

        public string PeerImageUrl
        {
            get
            {
                return _peerImageUrl;
            }
            private set
            {
                if (_peerImageUrl != value)
                {
                    _peerImageUrl = value;
                    OnPropertyChanged("PeerImageUrl");
                }
            }
        }

        public string InvitingInfo
        {
            get
            {
                return _invitingInfo;
            }
            private set
            {
                _invitingInfo = value;
                OnPropertyChanged("InvitingInfo");
            }
        }

        #endregion
        //private Dispatcher _workerDispatcher;

        #region -- Constructor --

        public DialingViewModel()
        {
            HangupCommand = new RelayCommand(Hangup);
            VideoAnswerCommand = new RelayCommand(VideoAnswer);

            SetDefaultSetting();
            
            CallController.Instance.CallStatusChanged += OnCallStatusChanged;
            EVSdkManager.Instance.EventPeerImageUrl += EVSdkManager_EventPeerImageUrl;
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
        
        #endregion

        #region -- Private Methods --

        private void OnCallStatusChanged(object sender, CallStatus status)
        {
            log.Info("OnCallStatusChanged start.");
            if (CallStatus.Ended == status || CallStatus.Idle == status)
            {
                log.Info("Call Ended or status, empty ConferenceNumber");
                ConferenceNumber = "";
                if (CallStatus.Ended == status)
                {
                    SetDefaultSetting();
                }
                PeerImageUrl = "";
            }
            else if (CallStatus.ConfIncoming == status || CallStatus.P2pIncoming == status)
            {
                VideoAnswerVisibility = Visibility.Visible;
                ConfNumberVisibility = Visibility.Collapsed;
                PeerDisplayName = CallController.Instance.CallInfo.peer;
                if (CallStatus.P2pIncoming == status)
                {
                    InvitingInfo = LanguageUtil.Instance.GetValueByKey("INVITING_YOU");
                }
                else if (CallStatus.ConfIncoming == status)
                {
                    if (string.IsNullOrEmpty(PeerDisplayName))
                    {
                        InvitingInfo = string.Format(LanguageUtil.Instance.GetValueByKey("INVITED_JOIN_CONF_PROMPT"), CallController.Instance.CallInfo.conference_number);
                    }
                    else
                    {
                        InvitingInfo = string.Format(LanguageUtil.Instance.GetValueByKey("ADMIN_INVITE_YOU_JOIN_CONF_PROMPT"), CallController.Instance.CallInfo.conference_number);
                    }
                }
            }
            else if (CallStatus.Dialing == status)
            {
                if (CallController.Instance.IsP2pCall)
                {
                    ConferenceNumber = CallController.Instance.PeerDisplayName;
                }
                else
                {
                    ConferenceNumber = CallController.Instance.ConferenceNumber;
                }
                log.InfoFormat("Dialing, set ConferenceNumber: {0}", ConferenceNumber);
                VideoAnswerVisibility = Visibility.Collapsed;
                ConfNumberVisibility = Visibility.Visible;
                PeerDisplayName = "";
            }
            
            log.Info("OnCallStatusChanged end.");
        }
        
        private void SetDefaultSetting()
        {
            BackgroundImage = "pack://application:,,,/Resources/Icons/background_default.png";
            BackgroundOpacity = 1f;
            //HeaderImageVisibility = Visibility.Collapsed;
            Foreground = "#FFFFFF";
        }
        
        private void Hangup(object parameter)
        {
            log.Info("Hangup");
            
            if (CallStatus.ConfIncoming == CallController.Instance.CurrentCallStatus || CallStatus.P2pIncoming == CallController.Instance.CurrentCallStatus)
            {
                CallController.Instance.CurrentCallStatus = CallStatus.Idle;
                CallController.Instance.DeclineIncommingCall(CallController.Instance.CallInfo.conference_number);
            }
            else
            {
                CallController.Instance.TerminateCall();
            }
        }

        private void VideoAnswer(object parameter)
        {
            log.Info("VideoAnswer");
            CallController.Instance.JoinConference(CallController.Instance.CallInfo.conference_number, LoginManager.Instance.DisplayName, CallController.Instance.CallInfo.password, ManagedEVSdk.Structs.EV_SVC_CALL_TYPE_CLI.EV_SVC_CALL_CONF);
        }

        private void EVSdkManager_EventPeerImageUrl(string imageUrl)
        {
            log.InfoFormat("EVSdkManager_EventPeerImageUrl: {0}", imageUrl);
            PeerImageUrl = imageUrl;
        }

        #endregion

    }
}
