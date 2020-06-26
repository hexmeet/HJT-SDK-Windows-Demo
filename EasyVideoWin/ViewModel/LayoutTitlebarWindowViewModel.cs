using EasyVideoWin.Helpers;
using EasyVideoWin.ManagedEVSdk.Structs;
using EasyVideoWin.Model;
using EasyVideoWin.View;
using log4net;
using System;
using System.ComponentModel;
using System.Text;
using System.Windows;

namespace EasyVideoWin.ViewModel
{
    class LayoutTitlebarWindowViewModel : DialOutModelBase
    {
        #region -- Members --

        private readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private System.Timers.Timer _refreshCallDurationTimer = null;
        private TimeSpan _callConnectedTimeSpan;

        private string _conferenceNumber;
        private string _callDuration;
        private int _callQuality = 0;
        private string _callQualityImageSource;
        private Visibility _encryptionVisibility = Visibility.Collapsed;

        #endregion

        #region -- Properties --

        public RelayCommand MediaStatisticsCommand { get; set; }

        public string ConferenceNumber
        {
            get
            {
                return _conferenceNumber;
            }
            protected set
            {
                _conferenceNumber = value;
                OnPropertyChanged("ConferenceNumber");
            }
        }

        public string CallDuration
        {
            get
            {
                return _callDuration;
            }
            set
            {
                _callDuration = value;
                OnPropertyChanged("CallDuration");
            }
        }

        public int CallQuality
        {
            get
            {
                return _callQuality;
            }
            set
            {
                if (_callQuality != value)
                {
                    log.InfoFormat("Call quality changed from {0} to {1}", _callQuality, value);
                    _callQuality = value;
                    if (_callQuality < 1)
                    {
                        log.Info("Call quality is less than 1 and set it to 1");
                    }
                    if (_callQuality > 5)
                    {
                        log.Info("Call quality is larger than 5 and set it to 5");
                    }
                    Application.Current.Dispatcher.InvokeAsync(() => {
                        log.Info("Update call quality image source");
                        CallQualityImageSource = "pack://application:,,,/Resources/Icons/icon_call_quality_level" + _callQuality + ".png";
                    });
                }
            }
        }

        public string CallQualityImageSource
        {
            get
            {
                return _callQualityImageSource;
            }
            set
            {
                _callQualityImageSource = value;
                OnPropertyChanged("CallQualityImageSource");
            }
        }
            
        public Visibility EncryptionVisibility
        {
            get
            {
                return _encryptionVisibility;
            }
            set
            {
                _encryptionVisibility = value;
                OnPropertyChanged("EncryptionVisibility");
            }
        }

        #endregion

        #region -- Constructor --

        public LayoutTitlebarWindowViewModel()
        {
            MediaStatisticsCommand = new RelayCommand(MediaStatistics);

            CallController.Instance.CallStatusChanged += OnCallStatusChanged;
            EVSdkManager.Instance.EventNetworkQuality += EVSdkWrapper_EventNetworkQuality;
            CallController.Instance.PropertyChanged += OnCallControllerPropertyChanged;
        }
        
        #endregion

        #region -- Public Method --

        #endregion

        #region -- Private Method --

        private void OnCallStatusChanged(object sender, CallStatus status)
        {
            log.Info("OnCallStatusChanged start.");
            switch (status)
            {
                case CallStatus.Ended:
                case CallStatus.Idle:
                    StopRefreshCallDurationTimer();
                    ConferenceNumber = "";
                    EncryptionVisibility = Visibility.Collapsed;
                    break;
                case CallStatus.Dialing:
                    EncryptionVisibility = Visibility.Collapsed;
                    CallDuration = "00:00:00";
                    CallQuality = 5;
                    // ConferenceNumber = CallController.Instance.IsP2pCall ? CallController.Instance.PeerDisplayName : CallController.Instance.ConferenceNumber;
                    ConferenceNumber = CallController.Instance.IsP2pCall ? "" : CallController.Instance.ConferenceNumber;
                    log.InfoFormat("Update ConferenceNumber: {0}", ConferenceNumber);
                    break;
                case CallStatus.P2pIncoming:
                case CallStatus.ConfIncoming:
                    CallDuration = "00:00:00";
                    CallQuality = 5;
                    break;
                case CallStatus.Connected:
                    StartRefreshCallDurationTimer();
                    CheckEncryptionStatus();
                    break;
                default:
                    break;
            }
            
            log.Info("OnCallStatusChanged end.");
        }
        
        private void StartRefreshCallDurationTimer()
        {
            if (null == _refreshCallDurationTimer)
            {
                _refreshCallDurationTimer = new System.Timers.Timer();
                _refreshCallDurationTimer.Elapsed += OnTimerRefreshCallDuration;
                _refreshCallDurationTimer.AutoReset = true;
                _callConnectedTimeSpan = new TimeSpan(DateTime.Now.Ticks);
            }
            _refreshCallDurationTimer.Enabled = true;
        }

        private void StopRefreshCallDurationTimer()
        {
            if (null != _refreshCallDurationTimer)
            {
                _refreshCallDurationTimer.AutoReset = true;
                _refreshCallDurationTimer.Enabled = false;
                _callConnectedTimeSpan = new TimeSpan(DateTime.Now.Ticks);
                _refreshCallDurationTimer = null;
            }
        }

        private void OnTimerRefreshCallDuration(object source, System.Timers.ElapsedEventArgs e)
        {
            TimeSpan tsNow = new TimeSpan(DateTime.Now.Ticks);
            TimeSpan duration = tsNow.Subtract(_callConnectedTimeSpan).Duration();
            StringBuilder sb = new StringBuilder();
            int hour = duration.Days * 24 + duration.Hours;
            sb.Append(hour < 10 ? ("0" + hour.ToString()) : hour.ToString())
                .Append(":")
                .Append(duration.Minutes < 10 ? ("0" + duration.Minutes.ToString()) : duration.Minutes.ToString())
                .Append(":")
                .Append(duration.Seconds < 10 ? ("0" + duration.Seconds.ToString()) : duration.Seconds.ToString());
            CallDuration = sb.ToString();
        }
        
        private void MediaStatistics(object parameter)
        {
            if (null == MediaStatisticsView.Instance)
            {
                MediaStatisticsView.Instance = new MediaStatisticsView();
            }
            MediaStatisticsView.Instance.Shows();
            DisplayUtil.CenterWindowOnMasterWindowScreen(MediaStatisticsView.Instance, LayoutBackgroundWindow.Instance);
            // 2018/03/08 - Do NOT force media statistics window to be always topest 
            // MediaStatisticsView.Instance.Owner = _layoutBackgroundWindow.GetTopestWindow();
        }
        
        private void EVSdkWrapper_EventNetworkQuality(float qualityRating)
        {
            log.Info("EventNetworkQuality");
            CallQuality = (int)qualityRating;
            log.Info("EventNetworkQuality end");
        }

        private void CheckEncryptionStatus()
        {
            EVStatsCli mediaStats = new EVStatsCli();
            EVSdkManager.Instance.GetStats(ref mediaStats);

            if (mediaStats.size > 0)
            {
                EncryptionVisibility = mediaStats.stats[0].is_encrypted ? Visibility.Visible : Visibility.Collapsed;
            }
            else
            {
                EncryptionVisibility = Visibility.Collapsed;
            }
            log.InfoFormat("EncryptionVisibility: {0}", EncryptionVisibility);
        }

        private void OnCallControllerPropertyChanged(object sender, PropertyChangedEventArgs args)
        {
            if ("ConferenceNumber" == args.PropertyName)
            {
                ConferenceNumber = CallController.Instance.ConferenceNumber;
            }
        }

        #endregion

    }
}
