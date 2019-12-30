using CefSharp;
using EasyVideoWin.CustomControls;
using EasyVideoWin.Enums;
using EasyVideoWin.Helpers;
using EasyVideoWin.Model;
using EasyVideoWin.Model.CloudModel;
using EasyVideoWin.Model.CloudModel.CloudRest;
using log4net;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace EasyVideoWin.View
{
    /// <summary>
    /// Interaction logic for LayoutOperationbarWindow.xaml
    /// </summary>
    public partial class LayoutOperationbarWindow : CustomShowHideBaseWindow, INotifyPropertyChanged
    {
        #region -- Members --
        private readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public delegate void SvcLyoutModeChangedHandler(LayoutModeType layoutMode, bool isDpiChanged = false);
        public event SvcLyoutModeChangedHandler SvcLyoutModeChanged;
        public event PropertyChangedEventHandler PropertyChanged;
        public event EventHandler ShowNormalCellsChanged;
        public event EventHandler ConfManagementChanged;
        public event Action EventInitScreen;
        public event Action EventRequestSpeaker;
        public event Action EventSwitch2AudioMode;
        public event Action<string> EventDisplayNameChanged;

        private LayoutModeType _layoutMode = LayoutModeType.NEW_SPEAKER_MODE;
        private bool _isAudioMuted;
        private bool _isLocalVideoSuspended;

        private string _whiteboardUrl = "";
        private string _whiteboardServerAddr = "";
        private ContentWhiteboard _contentWhiteboard = null;
        private SelectShareContentModeWindow _selectShareContentModeWindow = null;
        private SelectMoreContentModeWindow _selectMoreContentModeWindow = null;
        private System.Windows.Forms.Screen _selectedScreen = null;
        private CallStatus _callStatus = CallStatus.Idle;
        private bool _isShowSelectedContentMode = false;
        private bool _isShowSelectedMoreContentMode = false;
        private BackgroundWorker _loadWhiteBoardWindowWorker;
        private string _boardRoom = "";
        private const string ACS_URL = "{0}/acs/index.html?jwt={1}";
        private const string ACS_TEST_URL = "{0}/acs/pseudoAuth?uname={1}&room={2}&role=chair+person";
        private VideoContentWindow _receiveContentWin = null;
        private ContentControlView _sendContentWin = null;
        private bool _isNormalCellShown;
        private CancellationTokenSource _ctsSetPresenter;
        private System.Timers.Timer _checkWhiteBoardConnectionTimer;
        private bool _isRemoteMuted = false;
        private MediaModeType _mediaMode = MediaModeType.VIDEO_NORMAL;
        private bool _isChangeDisplayNameWinShown = false;

        private const int INITIAL_WIDTH = 1280;
        private ulong _presenterDeviceId;
        private BitmapImage _galleryImage = new System.Windows.Media.Imaging.BitmapImage(new Uri("pack://application:,,,/Resources/Icons/LayoutOperationbar/icon_gallery.png"));
        private BitmapImage _newSpeakerImage = new System.Windows.Media.Imaging.BitmapImage(new Uri("pack://application:,,,/Resources/Icons/LayoutOperationbar/icon_speaker.png"));
        private BitmapImage _traditionalSpeakerImage = new System.Windows.Media.Imaging.BitmapImage(new Uri("pack://application:,,,/Resources/Icons/LayoutOperationbar/icon_traditional_speaker.png"));
        
        #endregion

        #region -- Properties --

        public bool IsNormalCellShown
        {
            get
            {
                return _isNormalCellShown;
            }
            set
            {
                _isNormalCellShown = value;
                OnPropertyChanged("ShowNormalCellsBtnVisibility");
            }
        }

        public Visibility ShowNormalCellsBtnVisibility
        {
            get
            {
                return !_isNormalCellShown && MediaModeType.VIDEO_NORMAL == MediaMode ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        public bool IsAudioMuted
        {
            set
            {
                _isAudioMuted = value;
                log.InfoFormat("IsAudioMuted changed to: {0}", value);
                OnPropertyChanged("IsAudioMuted");
                OnPropertyChanged("MuteAudioVisibility");
                OnPropertyChanged("UnmuteAudioVisibility");
            }
            get
            {
                return _isAudioMuted;
            }
        }

        public Visibility MuteAudioVisibility
        {
            get
            {
                return IsAudioMuted ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        public Visibility UnmuteAudioVisibility
        {
            get
            {
                log.Info("UnmuteAudioVisibility is called.");
                return IsAudioMuted ? Visibility.Collapsed : Visibility.Visible;
            }
        }

        public bool IsLocalVideoSuspended
        {
            get
            {
                return _isLocalVideoSuspended;
            }
            set
            {
                _isLocalVideoSuspended = value;
                OnPropertyChanged("SuspendVideoVisibility");
                OnPropertyChanged("ResumeVideoVisibility");
            }
        }

        public Visibility SuspendVideoVisibility
        {
            get
            {
                return IsLocalVideoSuspended && MediaModeType.VIDEO_NORMAL == MediaMode ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        public Visibility ResumeVideoVisibility
        {
            get
            {
                return !IsLocalVideoSuspended && MediaModeType.VIDEO_NORMAL == MediaMode ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        public LayoutModeType LayoutMode
        {
            get
            {
                return _layoutMode;
            }
            set
            {
                switch (value)
                {
                    case LayoutModeType.GALLERY_MODE:
                        this.switchLayoutBtn.NormalImage = _galleryImage;
                        break;
                    case LayoutModeType.NEW_SPEAKER_MODE:
                        this.switchLayoutBtn.NormalImage = _newSpeakerImage;
                        break;
                    case LayoutModeType.TRADITIONAL_SPEAKER_MODE:
                        this.switchLayoutBtn.NormalImage = _traditionalSpeakerImage;
                        break;
                }
                _layoutMode = value;
            }
        }

        public Visibility ShowShareVisibility
        {
            get
            {
                if (MediaModeType.VIDEO_NORMAL != MediaMode)
                {
                    return Visibility.Collapsed;
                }

                bool isStartWhiteBoard = ContentStreamStatus.ReceivingWhiteBoardStarted == CallController.Instance.CurrentContentStreamStatus
                                         || ContentStreamStatus.SendingWhiteBoardStarted == CallController.Instance.CurrentContentStreamStatus;
                if (
                       null != _receiveContentWin
                    && ((isStartWhiteBoard && !_receiveContentWin.IsVisible) || (!isStartWhiteBoard && _receiveContentWin.IsVisible))
                )
                {
                    return Visibility.Visible;
                }
                else
                {
                    return Visibility.Collapsed;
                }
            }
        }

        public Visibility ShareModeVisibility
        {
            get
            {
                return MediaModeType.VIDEO_NORMAL == MediaMode ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        public Visibility SwitchLayoutVisibility
        {
            get
            {
                return MediaModeType.VIDEO_NORMAL == MediaMode ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        public Visibility MoreModeVisibility
        {
            get
            {
                bool showAudioMode =       null != LoginManager.Instance.LoginUserInfo
                                        && LoginManager.Instance.LoginUserInfo.featureSupport.switchingToAudioConference
                                        && MediaModeType.VIDEO_NORMAL == MediaMode;
                bool showChangeDisplayName = null != LoginManager.Instance.LoginUserInfo && LoginManager.Instance.LoginUserInfo.featureSupport.sitenameIsChangeable;
                log.InfoFormat("MoreModeVisibility, IsRemoteMuted: {0}, showAudioMode: {1}, showChangeDisplayName: {2}, MediaMode: {3}", IsRemoteMuted, showAudioMode, showChangeDisplayName, MediaMode);
                return IsRemoteMuted || showAudioMode || showChangeDisplayName ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        public bool IsReceiveContentWinActive
        {
            get
            {
                return _receiveContentWin.IsActive;
            }
        }

        public bool IsWhiteboardActive
        {
            get
            {
                return null != _contentWhiteboard && _contentWhiteboard.IsWindowActive;
            }
        }

        public string AcsServerAddress { get; set; }
        
        public bool IsRemoteMuted
        {
            get
            {
                return _isRemoteMuted;
            }
            set
            {
                _isRemoteMuted = value;
                OnPropertyChanged("MoreModeVisibility");
            }
        }
        public MediaModeType MediaMode
        {
            get
            {
                return _mediaMode;
            }
            set
            {
                _mediaMode = value;

                OnPropertyChanged("ShowNormalCellsBtnVisibility");
                OnPropertyChanged("ShareModeVisibility");
                OnPropertyChanged("SwitchLayoutVisibility");
                OnPropertyChanged("ShowShareVisibility");
                OnPropertyChanged("MoreModeVisibility");
                CheckLocalVideoStatus();

            }
        }

        public bool HasActiveSubDialog
        {
            get
            {
                return _isChangeDisplayNameWinShown;
            }
        }

        #endregion

        #region -- Constructor --

        public LayoutOperationbarWindow()
        {
            InitializeComponent();

            this.showNormalCellsBtn.SetBinding(Button.VisibilityProperty, new Binding("ShowNormalCellsBtnVisibility") { Source = this });
            this.unmuteAudioBtn.SetBinding(Button.VisibilityProperty, new Binding("UnmuteAudioVisibility") { Source = this });
            this.muteAudioBtn.SetBinding(Button.VisibilityProperty, new Binding("MuteAudioVisibility") { Source = this });
            this.resumeVideoBtn.SetBinding(Button.VisibilityProperty, new Binding("ResumeVideoVisibility") { Source = this });
            this.suspendVideoBtn.SetBinding(Button.VisibilityProperty, new Binding("SuspendVideoVisibility") { Source = this });
            this.shareModeBtn.SetBinding(Button.VisibilityProperty, new Binding("ShareModeVisibility") { Source = this });
            this.switchLayoutBtn.SetBinding(Button.VisibilityProperty, new Binding("SwitchLayoutVisibility") { Source = this });
            this.moreModeBtn.SetBinding(Button.VisibilityProperty, new Binding("MoreModeVisibility") { Source = this });
            this.showShareBtn.SetBinding(Button.VisibilityProperty, new Binding("ShowShareVisibility") { Source = this });
            
            CallController.Instance.CallStatusChanged += OnCallStatusChanged;
            CallController.Instance.ContentStreamStatusChanged += OnContentStreamStatusChanged;
            
            this.Loaded += LayoutOperationbarWindow_Loaded;
            EVSdkManager.Instance.EventWhiteBoardIndication += EVSdkWrapper_EventWhiteBoardIndication;
            EVSdkManager.Instance.EventParticipant += EVSdkWrapper_EventParticipant;
            EVSdkManager.Instance.EventRemoteMicMuted += EVSdkWrapper_EventRemoteMicMuted;
            LoginManager.Instance.PropertyChanged += LoginManager_PropertyChanged;
        }
        
        #endregion

        #region -- Public Methods --

        public VideoContentWindow GetReceiveContentWindow()
        {
            return _receiveContentWin;
        }

        // can not use properties for full stack error reason
        public ContentWhiteboard GetContentWhiteboardWindow()
        {
            return _contentWhiteboard;
        }

        public void InitContentWindow()
        {
            log.Info("Init content window");
            // content window
            _receiveContentWin = VideoContentWindow.Instance;
            _receiveContentWin.Show();
            //_receiveContentWin.Visibility = Visibility.Collapsed;
            _receiveContentWin.Hide();

            CallController.Instance.SetRemoteContentWindow(_receiveContentWin.contentVideoWnd.Handle);
        }

        public void StartWhiteboard()
        {
            SendWhiteboard();
        }

        public void ExitWhiteboard()
        {
            _contentWhiteboard.ExitWhiteboard();
        }

        public void StartContent()
        {
            CloseWhiteboard();
            SetScreenSelected(System.Windows.Forms.Screen.AllScreens[0]);
        }

        public void ExitContent()
        {
            CallController.Instance.StopContent();
        }

        public void SetWhiteBoardWindowActivate()
        {
            _contentWhiteboard.Activate();
        }

        public void SetOwner4WhiteBoardWindow(Window owner)
        {
            _contentWhiteboard.Owner = owner;
        }

        public void SetReceiveContentWindowActivate()
        {
            _receiveContentWin.Activate();
        }

        public void SetOwner4ReceiveContent(Window owner)
        {
            _receiveContentWin.Owner = owner;
        }
        
        public void OnConnect2WhiteBoard()
        {
            StopCheckWhiteBoardConnectionTimer();
            log.InfoFormat("On connect to white board, content stream status: {0}, white board visibility: {1}"
                , CallController.Instance.CurrentContentStreamStatus
                , _contentWhiteboard.Visibility );
            if (
                   ContentStreamStatus.ReceivingWhiteBoardStarted == CallController.Instance.CurrentContentStreamStatus
                && Visibility.Visible != _contentWhiteboard.Visibility
            )
            {
                log.Info("Content stream status is started and start white board mode is true, show white board");
                Application.Current.Dispatcher.InvokeAsync(() => {
                    OnPropertyChanged("ShowShareVisibility");
                    ShowWhiteBoard();
                });
            }
        }

        public void TreatmentOnCallEnded()
        {
            // the function will be invoked by layoutbackgroundwindow, maybe the call status changed arrived layoutbackground window
            // before this object, so the video window disppeared, but the white board window will dispear later about 3 seconds.
            ////if (null != _contentWhiteboard && !_contentWhiteboard.IsDisposed && Visibility.Visible == _contentWhiteboard.Visibility)
            ////{
            ////    _contentWhiteboard.Hide();
            ////}
        }

        public void UnmuteAudio()
        {
            MuteAudioBtn_Click(this, null);
        }

        #endregion

        #region -- Private Methods --

        private void CreateWhiteboard()
        {
            log.Info("Create white board");
            if (null != _contentWhiteboard)
            {
                return;
            }
            CloudApiManager.Instance.Token = LoginManager.Instance.LoginToken;
            _contentWhiteboard = new ContentWhiteboard();
            _contentWhiteboard.Closing += ContentWhiteboard_Closing;
            //listener white board,show or hide white board window
            CallbackObjectForJs callBackObj = new CallbackObjectForJs();
            callBackObj.MainModel = this;
            _contentWhiteboard.Browser.RegisterJsObject("callbackObj", callBackObj);
            
            //reload acs when exit
            _contentWhiteboard.ListeneWhiteBoardExit += new ContentWhiteboard.ListenerWhiteBoardExitHandler(ReLoadWhiteBoard);
            _contentWhiteboard.Browser.IsBrowserInitializedChanged += Browser_IsBrowserInitializedChanged;
        }

        private void ContentWhiteboard_Closing(object sender, CancelEventArgs e)
        {
            if (   CallStatus.Connected == CallController.Instance.CurrentCallStatus
                && (ContentStreamStatus.SendingWhiteBoardStarted == CallController.Instance.CurrentContentStreamStatus || ContentStreamStatus.ReceivingWhiteBoardStarted == CallController.Instance.CurrentContentStreamStatus)
            )
            {
                log.Info("Call status is Connected and content stream status is SendingWhiteBoardStarted, but content white board window is closing. Maybe the white board window is closed through task bar. Quit app.");
                e.Cancel = true;
                Application.Current.MainWindow.Close();
            }
        }

        private void Browser_IsBrowserInitializedChanged(object sender, IsBrowserInitializedChangedEventArgs e)
        {
            log.InfoFormat("Content white board browser initialized changed to: {0}", e.IsBrowserInitialized);
            if (e.IsBrowserInitialized)
            {
                LoadWhiteBoard();
            }
        }

        private void ReleaseWhiteBoard()
        {
            log.Info("Release white board");
            if (null != _contentWhiteboard)
            {
                _contentWhiteboard.Closing -= ContentWhiteboard_Closing;
                StopCheckWhiteBoardConnectionTimer();
                _contentWhiteboard.Browser.IsBrowserInitializedChanged -= Browser_IsBrowserInitializedChanged;
                _contentWhiteboard.TreatmentOnCallEnded();
                CallController.Instance.CurrentContentStreamStatus = ContentStreamStatus.Idle;
                _contentWhiteboard.ListeneWhiteBoardExit -= ReLoadWhiteBoard;
                _contentWhiteboard.Dispose();
                _contentWhiteboard = null;
            }

            if (CallController.Instance.IsConnected2WhiteBoard)
            {
                CallController.Instance.IsConnected2WhiteBoard = false;
            }
        }

        private void LayoutOperationbarWindow_Loaded(object sender, RoutedEventArgs e)
        {
            Utils.HideWindowInAltTab(this.Handle);
        }

        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void OnCallStatusChanged(object sender, CallStatus status)
        {
            log.Info("OnCallStatusChanged start.");
            switch (status)
            {
                case CallStatus.Dialing:
                    break;
                case CallStatus.Connected:
                    CheckMicStatus();
                    CheckLocalVideoStatus();
                    break;
                case CallStatus.Ended:
                    CancelLoadWhiteBoardWindowWorker();

                    Application.Current.Dispatcher.InvokeAsync(() => {
                        CloseSendContentWin();
                        ReleaseWhiteBoard();
                    });
                    
                    RestoreIconState();
                    HideSelectShareContentModeWindow();
                    HideSelectMoreContentModeWindow();

                    break;
            }

            _callStatus = status;
            log.Info("OnCallStatusChanged end.");
        }

        private void CloseSendContentWin()
        {
            if (null != _sendContentWin)
            {
                _sendContentWin.Loaded -= SendContentWin_Loaded;
                _sendContentWin.Dispose();
                _sendContentWin.Close();
            }
        }
        
        private void OnContentStreamStatusChanged(object sender, ContentStreamInfo contentStreamInfo)
        {
            log.Info("OnContentStreamStatusChanged start.");
            Application.Current.Dispatcher.InvokeAsync(() =>
            {
                log.InfoFormat("Content stream status changed, current status: {0}, last status: {1}", contentStreamInfo.CurrentStatus, contentStreamInfo.LastStatus);

                if (ContentStreamStatus.SendingWhiteBoardStarted ==  contentStreamInfo.LastStatus || ContentStreamStatus.ReceivingWhiteBoardStarted == contentStreamInfo.LastStatus)
                {
                    HideWhiteBoard();
                }

                switch (contentStreamInfo.CurrentStatus)
                {
                    case ContentStreamStatus.SendingContentStarted:
                        OnSendingContentStarted(contentStreamInfo);
                        break;
                    case ContentStreamStatus.SendingWhiteBoardStarted:
                        OnSendingWhiteBoardStarted(contentStreamInfo);
                        break;
                    case ContentStreamStatus.Idle:
                        OnContentIdle();
                        break;
                    case ContentStreamStatus.ReceivingContentStarted:
                        OnReceivingContentStarted();
                        break;
                    case ContentStreamStatus.ReceivingWhiteBoardStarted:
                        OnReceivingWhiteBoard();
                        break;
                }
            });
            log.Info("OnContentStreamStatusChanged end.");
        }
        
        private void OnSendingContentStarted(ContentStreamInfo contentStreamInfo)
        {
            log.Info("Send screen content");
            VideoPeopleWindow.Instance.UpdatePresettingState();
            SetSendContentWindowHandle(contentStreamInfo);

            if (Visibility.Visible == _receiveContentWin.Visibility)
            {
                _receiveContentWin.Hide();
            }
        }

        private void OnSendingWhiteBoardStarted(ContentStreamInfo contentStreamInfo)
        {
            ShowWhiteBoard();
            VideoPeopleWindow.Instance.UpdatePresettingState();
            _contentWhiteboard.Initiator = true;
            IntPtr hWnd = _contentWhiteboard.Handle;
            log.InfoFormat("send white board, white board window handle : {0}", hWnd);
            EVSdkManager.Instance.SetLocalContentWindow(hWnd, ManagedEVSdk.Structs.EV_CONTENT_MODE_CLI.EV_CONTENT_APPLICATION_MODE);
            
            if (Visibility.Visible == _receiveContentWin.Visibility)
            {
                _receiveContentWin.Hide();
            }
        }

        private void OnContentIdle()
        {
            if (null != _contentWhiteboard)
            {
                _contentWhiteboard.Initiator = false;
            }
            if (VideoPeopleWindow.Instance.WindowState == WindowState.Minimized)
            {
                log.InfoFormat("ContentStream ended, reset main window to presetting state");
                VideoPeopleWindow.Instance.Set2PresettingState();
            }

            CloseSendContentWin();

            log.Info("Set content related status to idle.");

            _receiveContentWin.Hide();
            OnPropertyChanged("ShowShareVisibility");
        }

        private void OnReceivingContentStarted()
        {
            log.Info("OnReceivingContentStarted start");
            CloseSendContentWin();

            if (VideoPeopleWindow.Instance.WindowState == WindowState.Minimized)
            {
                log.Info("ReceivingContentStarted, reset main window to presetting state");
                VideoPeopleWindow.Instance.Set2PresettingState();
            }

            _receiveContentWin.Show();
            OnPropertyChanged("ShowShareVisibility");
            if (WindowState.Normal != _receiveContentWin.WindowState)
            {
                // restore window to avoid max to max when receive content more than one time.
                _receiveContentWin.RestoreWindow();
            }
            DisplayUtil.SetWndOnSuitableScreen(_receiveContentWin, VideoPeopleWindow.Instance);
            _receiveContentWin.AdjustWindowSize();
            //change the content window to max size to get clearer content.
            //_receiveContentWin.TitleConfNumberLabel.Content = CallController.Instance.IsP2pCall ? CallController.Instance.PeerDisplayName : CallController.Instance.ConferenceNumber;
            _receiveContentWin.TitleConfNumberLabel.Content = CallController.Instance.IsP2pCall ? "" : CallController.Instance.ConferenceNumber;

            // do not max the content window to max to avoid max handy once time.
            //if (VideoPeopleWindow.Instance.WindowState == WindowState.Maximized && VideoPeopleWindow.Instance.FullScreenStatus)
            //{
            //    _receiveContentWin.SetFullScreen();
            //    log.Info("Receive content window is set to full screen");
            //}
            //else
            //{
            //    _receiveContentWin.MaximizeWindow();
            //    log.Info("Receive content window is set to max");
            //}
            log.Info("OnReceivingContentStarted end");
        }

        private void OnReceivingWhiteBoard()
        {
            log.Info("Receiving white board started.");
            CloseSendContentWin();

            if (null == _contentWhiteboard)
            {
                log.Info("Can not show receiving white board for white board is null.");
                return;
            }

            if (VideoPeopleWindow.Instance.WindowState == WindowState.Minimized)
            {
                log.Info("ReceivingContentStarted, reset video people window to presetting state");
                VideoPeopleWindow.Instance.Set2PresettingState();
            }

            _contentWhiteboard.Initiator = false;
            if (CallController.Instance.IsConnected2WhiteBoard)
            {
                OnPropertyChanged("ShowShareVisibility");
                ShowWhiteBoard();
            }
            else
            {
                log.Info("Not connect to white board and show white board later.");
            }
        }

        private void ShowWhiteBoard()
        {
            log.Info("show white board, show toolbar");
            _contentWhiteboard.Width = _contentWhiteboard.InitialWidth;
            _contentWhiteboard.Height = _contentWhiteboard.InitialHeight;
            _contentWhiteboard.Show();
            if (WindowState.Normal != _contentWhiteboard.WindowState)
            {
                // restore window to avoid max to max when receive content more than one time.
                _contentWhiteboard.RestoreWindow();
            }
            System.Windows.Forms.Screen screen = DisplayUtil.SetWndOnSuitableScreen(_contentWhiteboard, VideoPeopleWindow.Instance);
            _contentWhiteboard.AdjustWindowSize();

            log.InfoFormat("show board, width: {0}, height:{1}", _contentWhiteboard.Width, _contentWhiteboard.Height);

            // let white board show on top                    
            _contentWhiteboard.ShowOrHideToolbar(true, screen);

            string cmd = "window.isHighDpiDevice(\'" + _contentWhiteboard.IsHighDpiDevice.ToString() + "\')";
            _contentWhiteboard.Browser.GetBrowser().MainFrame.ExecuteJavaScriptAsync(cmd);

            // do not max the content window to max to avoid max handy once time.
            //if (VideoPeopleWindow.Instance.WindowState == WindowState.Maximized && !VideoPeopleWindow.Instance.FullScreenStatus)
            //{
            //    _contentWhiteboard.MaximizeWindow();
            //}
            //else if (VideoPeopleWindow.Instance.WindowState == WindowState.Maximized && VideoPeopleWindow.Instance.FullScreenStatus)
            //{
            //    _contentWhiteboard.SetFullScreen();
            //}

            _contentWhiteboard.Activate();
            _contentWhiteboard.Focus();
            log.Info("Show white board end");
        }

        private void CheckMicStatus()
        {
            IsAudioMuted = !CallController.Instance.IsMicEnabled();
        }

        private void CheckLocalVideoStatus()
        {
            IsLocalVideoSuspended = !CallController.Instance.IsCameraEnabled();
        }

        private void UnmuteAudioBtn_Click(object sender, RoutedEventArgs e)
        {
            log.Info("Invoked mute mic.");
            CallController.Instance.EnableMic(false);
            bool enabled = CallController.Instance.IsMicEnabled();
            if (enabled)
            {
                log.Info("Failed to mute mic.");
                return;
            }
            IsAudioMuted = true;
        }

        private void MuteAudioBtn_Click(object sender, RoutedEventArgs e)
        {
            log.Info("Invoked unmute mic.");
            CallController.Instance.EnableMic(true);
            bool enabled = CallController.Instance.IsMicEnabled();
            if (!enabled)
            {
                log.Info("Failed to unmute mic.");
                return;
            }
            IsAudioMuted = false;
        }

        private void ResumeVideoBtn_Click(object sender, RoutedEventArgs e)
        {
            CallController.Instance.EnableCamera(false);
            bool enabled = CallController.Instance.IsCameraEnabled();
            if (enabled)
            {
                log.Info("Failed to enable camera.");
                return;
            }
            IsLocalVideoSuspended = true;
        }

        private void SuspendVideoBtn_Click(object sender, RoutedEventArgs e)
        {
            CallController.Instance.EnableCamera(true);
            bool enabled = CallController.Instance.IsCameraEnabled();
            if (!enabled)
            {
                log.Info("Failed to disable camera.");
                return;
            }
            IsLocalVideoSuspended = false;
        }

        private void HangupBtn_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                e.Handled = true;
            }
        }

        private void ConfManagementBtn_Click(object sender, RoutedEventArgs e)
        {
            ConfManagementChanged?.Invoke(sender, new EventArgs());
        }
        
        private void SwitchLayoutBtn_Click(object sender, RoutedEventArgs e)
        {
            switch (LayoutMode)
            {
                case LayoutModeType.GALLERY_MODE:
                    SvcLyoutModeChanged?.Invoke(LayoutModeType.NEW_SPEAKER_MODE);
                    break;
                case LayoutModeType.NEW_SPEAKER_MODE:
                    SvcLyoutModeChanged?.Invoke(LayoutModeType.TRADITIONAL_SPEAKER_MODE);
                    break;
                case LayoutModeType.TRADITIONAL_SPEAKER_MODE:
                    SvcLyoutModeChanged?.Invoke(LayoutModeType.GALLERY_MODE);
                    break;
            }
            
        }

        private void MoreMode_Click(object sender, RoutedEventArgs e)
        {
            if (_isShowSelectedMoreContentMode)
            {
                log.Info("close SelectMoreContentMode Window");
                HideSelectMoreContentModeWindow();
            }
            else
            {
                log.Info("init and show SelectMoreContentMode Window");
                if (_selectMoreContentModeWindow == null)
                {
                    bool showSwitch2AudioMode = LoginManager.Instance.LoginUserInfo.featureSupport.switchingToAudioConference && MediaModeType.VIDEO_NORMAL == MediaMode;
                    _selectMoreContentModeWindow = new SelectMoreContentModeWindow(showSwitch2AudioMode, IsRemoteMuted, LoginManager.Instance.LoginUserInfo.featureSupport.sitenameIsChangeable);
                }

                _selectMoreContentModeWindow.Owner = this;
                _selectMoreContentModeWindow.Top = this.Top - _selectMoreContentModeWindow.Height - 5;
                Vector vectorNormal = VisualTreeHelper.GetOffset(this.normalButtonsDockPanel);
                Vector vectorMore = VisualTreeHelper.GetOffset(this.moreModeBtn);
                _selectMoreContentModeWindow.Left = this.Left + vectorNormal.X + vectorMore.X + this.moreModeBtn.ActualWidth / 2 - _selectMoreContentModeWindow.Width / 2;

                _selectMoreContentModeWindow.Show();
                _selectMoreContentModeWindow.ListenerSelectedMoreContent += new SelectMoreContentModeWindow.ListenerSelectedMoreContentHandler(SelectedMoreContentMode);
                _isShowSelectedMoreContentMode = true;
            }
        }

        private void ShowShare_Click(object sender, RoutedEventArgs e)
        {
            bool isStartWhiteBoard =    ContentStreamStatus.ReceivingWhiteBoardStarted == CallController.Instance.CurrentContentStreamStatus
                                     || ContentStreamStatus.SendingWhiteBoardStarted == CallController.Instance.CurrentContentStreamStatus;
            if (isStartWhiteBoard)
            {
                log.Info("Show white board.");
                _contentWhiteboard.Show();
                _contentWhiteboard.Activate();
                _contentWhiteboard.Focus();
                _contentWhiteboard.ControlToolBar.Show();
                if (_contentWhiteboard.WindowState == WindowState.Minimized)
                {
                    _contentWhiteboard.WindowState = WindowState.Normal;
                }
            }
            else if (!isStartWhiteBoard && _receiveContentWin.IsVisible)
            {
                log.Info("Show receive content window.");
                _receiveContentWin.Show();
                _receiveContentWin.Activate();
                _receiveContentWin.Focus();
                if (_receiveContentWin.WindowState == WindowState.Minimized)
                {
                    _receiveContentWin.WindowState = WindowState.Normal;
                }
            }
        }

        private void ShareMode_Click(object sender, RoutedEventArgs e)
        {
            if (_isShowSelectedContentMode)
            {
                log.Info("close SelectShareContentMode Window");
                HideSelectShareContentModeWindow();
            }
            else
            {
                log.Info("init and show SelectShareContentMode Window");
                if (_selectShareContentModeWindow == null)
                {
                    _selectShareContentModeWindow = new SelectShareContentModeWindow();
                }

                _selectShareContentModeWindow.Owner = this;
                _selectShareContentModeWindow.Top = this.Top - _selectShareContentModeWindow.Height - 5;
                Vector vectorNormal = VisualTreeHelper.GetOffset(this.normalButtonsDockPanel);
                Vector vectorSendShare = VisualTreeHelper.GetOffset(this.shareModeBtn);

                _selectShareContentModeWindow.Left = this.Left + vectorNormal.X + vectorSendShare.X + this.shareModeBtn.ActualWidth / 2 - _selectShareContentModeWindow.Width / 2;

                _selectShareContentModeWindow.Show();
                _selectShareContentModeWindow.ListenerSelectedShareContent += new SelectShareContentModeWindow.ListenerSelectedShareContentHandler(SelectedShareContentMode);
                _isShowSelectedContentMode = true;
            }
        }

        private void SettingBtn_Click(object sender, RoutedEventArgs e)
        {
            log.Info("Show device setting");
            System.Windows.Rect mainWindowRect = VideoPeopleWindow.Instance.GetWindowRect();
            Window audioDeviceView = new AudioDeviceSelectorView();
            audioDeviceView.Left = mainWindowRect.Left + (mainWindowRect.Width - audioDeviceView.Width) / 2;
            audioDeviceView.Top = mainWindowRect.Top + (mainWindowRect.Height - audioDeviceView.Height) / 2;
            audioDeviceView.WindowStartupLocation = WindowStartupLocation.Manual;
            audioDeviceView.Owner = this;
            audioDeviceView.ShowDialog();
        }

        private void HangupBtn_Click(object sender, RoutedEventArgs e)
        {
            log.Info("HangupBtn_Click");
            CallController.Instance.TerminateCall();
        }

        private void SendWhiteboard()
        {
            log.Info("Send Whiteboard");
            if (CallController.Instance.CurrentContentStreamStatus == ContentStreamStatus.SendingContentStarted)
            {
                // content video is sending out.
                CallController.Instance.StopContent();
            }

            // put show white board invoke to the quee of Application.Current.Dispatcher for CallController.Instance.StopSendContent()
            // will trigger content status to idle that may be later with sending content status for white board.
            Application.Current.Dispatcher.InvokeAsync(() => {
                //the first user start board, clear the current board before start
                const string clearCommand = "window.clearProject()";
                _contentWhiteboard.Browser.GetBrowser().MainFrame.ExecuteJavaScriptAsync(clearCommand);
                const string start = "window.startBoard()";
                _contentWhiteboard.Browser.GetBrowser().MainFrame.ExecuteJavaScriptAsync(start);
                ////string cmd = "window.isHighDpiDevice(\'" + _contentWhiteboard.IsHighDpiDevice.ToString() + "\')";
                ////_contentWhiteboard.Browser.GetBrowser().MainFrame.ExecuteJavaScriptAsync(cmd);

                CallController.Instance.WhiteBoardPresenter = "";
                log.InfoFormat("Show white board, device id: {0}", LoginManager.Instance.DeviceId);
                GetWhiteBoardPresenterName(LoginManager.Instance.DeviceId);
                OnPropertyChanged("ShowShareVisibility");
                
                // start content sending
                CallController.Instance.SendWhiteBoard();
            });
        }
        
        public void GetWhiteBoardPresenterName(ulong deviceId)
        {
            if (null != _ctsSetPresenter)
            {
                _ctsSetPresenter.Cancel();
                _ctsSetPresenter = null;
            }

            if (0 == deviceId)
            {
                log.Info("deviceId is 0 and do not GetWhiteBoardPresenterName");
                return;
            }

            _presenterDeviceId = deviceId;
            _ctsSetPresenter = new CancellationTokenSource();
            Task.Run(() => {
                if (null == _ctsSetPresenter || _ctsSetPresenter.Token.IsCancellationRequested)
                {
                    return;
                }

                // when token is invalid and update token, then update presenter name. in the case, the result of first thread may be later than the thread of updated token
                bool invalidToken = false;
                CancellationToken cancellationToken = _ctsSetPresenter.Token;
                CloudApiManager.Instance.Token = LoginManager.Instance.LoginToken;
                RestPartNameInfo partNameInfo = CloudApiManager.Instance.GetPartyName(CallController.Instance.ConferenceNumber, deviceId, (response) => {
                    try
                    {
                        ErrorMessageRest errMsg = JsonConvert.DeserializeObject<ErrorMessageRest>(response.Content);
                        log.ErrorFormat("GetWhiteBoardPresenterName failed, errorCode: {0}, errorInfo: {0}", errMsg.errorCode, errMsg.errorInfo);
                        if (errMsg.errorCode == ErrorMessageRest.ERR_INVALID_TOKEN)
                        {
                            invalidToken = true;
                            LoginManager.Instance.UpdateLoginToken();
                        }
                    }
                    catch(Exception e)
                    {
                        log.InfoFormat("Exception for handle error of GetWhiteBoardPresenterName, exception:{0}", e);
                    }
                });

                if (cancellationToken.IsCancellationRequested || invalidToken)
                {
                    log.Info("GetWhiteBoardPresenterName cancelled.");
                    return;
                }

                if (null != partNameInfo)
                {
                    CallController.Instance.WhiteBoardPresenter = partNameInfo.name;
                    log.InfoFormat("Got WhiteBoardPresenter: {0}", partNameInfo.name);
                }
                else
                {
                    CallController.Instance.WhiteBoardPresenter = LoginManager.Instance.DisplayName;
                    log.InfoFormat("Failed to get WhiteBoardPresenter, show display name: {0}", LoginManager.Instance.DisplayName);
                }
                _presenterDeviceId = 0;
            });
        }
        
        private void HideWhiteBoard()
        {
            if (null == _contentWhiteboard || Visibility.Visible != _contentWhiteboard.Visibility)
            {
                return;
            }

            Application.Current.Dispatcher.Invoke(() =>
            {
                /*
                 * To get the cursor from the content white board
                 * Scenario: 1) A send white board, 2) B receive white board, 3) click on the white board in touch device in B side
                 *           3) A stop white board, 4) the cursor will disappear on the B side.
                 */
                Mouse.UpdateCursor();
                _contentWhiteboard.ControlToolBar.Activate();

                log.Info("hide ContentWhiteboard, auto capture window to image, reload browser, hide toolbar");
                _contentWhiteboard.AutoSaveWindowToImage();
                _contentWhiteboard.Visibility = Visibility.Collapsed;
                // Reload here for not disconnected but for clear cache in browser.
                ReLoadWhiteBoard("");
                OnPropertyChanged("ShowShareVisibility");

                _contentWhiteboard.Initiator = false;
            });
        }
        
        private void ReLoadWhiteBoard(string str)
        {
            OnPropertyChanged("ShowShareVisibility");
            _contentWhiteboard.ShowOrHideToolbar(false, null);
            log.InfoFormat("ACS: reload whiteboard: {0}", string.Format(_whiteboardUrl, CallController.Instance.ConferenceNumber));
            // TODO: has to reload for now. however we should
            // revisit here later to have the logic refined. 
            // Reload may cause unexpected board reopen in some 
            // race condition.
            _contentWhiteboard.Browser.Reload(true);

        }

        private void SetScreenSelected(System.Windows.Forms.Screen selScreen)
        {
            this.Activate();
            EventInitScreen?.Invoke();
            _selectedScreen = selScreen;
            log.Info("Screen selected and start send content.");
            CallController.Instance.SendContent();
            //ContentView.SetWindowPosition();
            //ContentView.ResolveScreenOrientation();
            //ContentView.SetToolBarPositionLogical();  
        }

        private void CloseWhiteboard()
        {
            log.Info("CloseWhiteboard");

            ContentStreamStatus contentStatus = CallController.Instance.CurrentContentStreamStatus;
            if (ContentStreamStatus.SendingWhiteBoardStarted == contentStatus)
            {
                CallController.Instance.StopContent();
            }

            if (ContentStreamStatus.SendingWhiteBoardStarted != contentStatus && ContentStreamStatus.ReceivingWhiteBoardStarted != contentStatus)
            {
                return;
            }

            _contentWhiteboard.DisconnectingWhiteBoard();
            _contentWhiteboard.AutoSaveWindowToImage();

            ReLoadWhiteBoard("");
            _contentWhiteboard.Visibility = Visibility.Collapsed;
            OnPropertyChanged("ShowShareVisibility");
        }

        private void RestoreIconState()
        {
            OnPropertyChanged("ShowShareVisibility");
        }

        private void SelectedMoreContentMode(MoreContentMode mode)
        {
            if (MoreContentMode.Switch2AudioMode == mode)
            {
                Switch2AudioMode();
            }
            else if (MoreContentMode.RequestSpeak == mode)
            {
                RequestSpeak();
            }
            else if (MoreContentMode.ChangeDisplayName == mode)
            {
                ChangeDisplayName();
            }
            HideSelectMoreContentModeWindow();
        }

        private void Switch2AudioMode()
        {
            log.Info("SwitchAudioMode");
            CallController.Instance.Switch2AudioMode();
            EventSwitch2AudioMode?.Invoke();
        }
        
        private void RequestSpeak()
        {
            log.Info("RequestSpeak");
            EventRequestSpeaker?.Invoke();
            EVSdkManager.Instance.RequestRemoteUnmute(true);
        }

        private void ChangeDisplayName()
        {
            ChangeDisplayNameWindow dlg = new ChangeDisplayNameWindow();
            dlg.Owner = this;
            dlg.ConfirmEvent += (displayName) =>
            {
                this.Activate();
                if (EVSdkManager.Instance.SetInConfDisplayName(displayName))
                {
                    EventDisplayNameChanged?.Invoke(displayName);
                }
                else
                {
                    log.Info("Failed to ChangeDisplayName");
                }
                _isChangeDisplayNameWinShown = false;
                dlg.Close();
            };
            dlg.CloseEvent += (sender, e) =>
            {
                this.Activate();
                _isChangeDisplayNameWinShown = false;
            };
            _isChangeDisplayNameWinShown = true;
            dlg.ShowDialog();
        }

        private void SelectedShareContentMode(ShareContentMode mode)
        {
            if (ShareContentMode.ScreenShare == mode)
            {
                InitScreenShare();
            }
            else if (ShareContentMode.Whiteboard == mode && !_contentWhiteboard.IsVisible)
            {
                SendWhiteboard();
            }
            HideSelectShareContentModeWindow();
        }

        private void InitScreenShare()
        {
            if (ContentStreamStatus.SendingContentStarted == CallController.Instance.CurrentContentStreamStatus)
            {
                return;
            }

            SelectScreen();
        }

        private void SelectScreen()
        {
            int screenNum = System.Windows.Forms.Screen.AllScreens.Length;
            //if (screenNum == 1)
            //{
            //    CloseWhiteboard();
            //    SetScreenSelected(System.Windows.Forms.Screen.AllScreens[0]);
            //}
            //else
            {
                ScreenSelectionView _screenSel = new ScreenSelectionView();
                System.Windows.Rect mainWindowRect = VideoPeopleWindow.Instance.GetWindowRect();
                _screenSel.Left = mainWindowRect.Left + (mainWindowRect.Width - _screenSel.Width) / 2;
                _screenSel.Top = mainWindowRect.Top + (mainWindowRect.Height - _screenSel.Height) / 2;
                _screenSel.WindowStartupLocation = WindowStartupLocation.Manual;
                _screenSel.Owner = this;
                _screenSel.SelectedScreen = null;
                _screenSel.InitScreen();
                _screenSel.ShowDialog();
                if (null != _screenSel.SelectedScreen)
                {
                    CloseWhiteboard();
                    SetScreenSelected(_screenSel.SelectedScreen);
                }
            }
        }

        private void SetSendContentWindowHandle(ContentStreamInfo contentStreamInfo)
        {
            _sendContentWin = new ContentControlView();
            _sendContentWin.Loaded += SendContentWin_Loaded;
            _sendContentWin.SetSelectedScreen(_selectedScreen);
            _sendContentWin.Show();
            
            System.Windows.Forms.Screen screen = DpiUtil.GetScreenByHandle(VideoPeopleWindow.Instance.Handle);
            if (screen == null || screen.Equals(_selectedScreen))
            {
                VideoPeopleWindow.Instance.ChangeWindowState(WindowState.Minimized);
            }
            else if (ContentStreamStatus.ReceivingContentStarted == contentStreamInfo.LastStatus || ContentStreamStatus.ReceivingWhiteBoardStarted == contentStreamInfo.LastStatus)
            {
                //do this trick to refresh all the windows
                VideoPeopleWindow.Instance.ChangeWindowState(WindowState.Minimized);
                VideoPeopleWindow.Instance.ChangeWindowState(WindowState.Normal);
            }
        }

        private void SendContentWin_Loaded(object sender, RoutedEventArgs e)
        {
            if (null == _sendContentWin)
            {
                log.Info("Content window loaded, but the object reference is null.");
                return;
            }
            _sendContentWin.Handle = new WindowInteropHelper(_sendContentWin).Handle;
            _sendContentWin.SetProperPosition();
            log.InfoFormat("Send video content, handle: {0}", _sendContentWin.Handle);
            EVSdkManager.Instance.SetLocalContentWindow(_sendContentWin.Handle, ManagedEVSdk.Structs.EV_CONTENT_MODE_CLI.EV_CONTENT_FULL_MODE);
            log.Info("Local content window handle has been set!");
        }

        public bool IsWhiteBoardShowed()
        {
            return _contentWhiteboard.IsVisible;
        }

        private void CancelLoadWhiteBoardWindowWorker()
        {
            if (_loadWhiteBoardWindowWorker != null && _loadWhiteBoardWindowWorker.WorkerSupportsCancellation)
            {
                _loadWhiteBoardWindowWorker.CancelAsync();
                _loadWhiteBoardWindowWorker.Dispose();
            }
        }

        private void EVSdkWrapper_EventWhiteBoardIndication(ManagedEVSdk.Structs.EVWhiteBoardInfoCli whiteBoardInfo)
        {
            log.InfoFormat("EventWhiteBoardIndication, type: {0}, auth address: {1}, server: {2}"
                , whiteBoardInfo.type
                , whiteBoardInfo.authServer
                , whiteBoardInfo.server  );

            if (ManagedEVSdk.Structs.EV_WHITE_BOARD_TYPE_CLI.EV_ACS_WHITE_BOARD != whiteBoardInfo.type || string.IsNullOrEmpty(whiteBoardInfo.server))
            {
                log.Info("Invalid paramter. EventWhiteBoardIndication end");
                return;
            }
            
            Application.Current.Dispatcher.InvokeAsync(() => {
                log.Info("Begin to handle EventWhiteBoardIndication in UI thread");
                CloudApiManager.Instance.DoradoZoneAddress = whiteBoardInfo.authServer;
                CloudApiManager.Instance.AcsServerAddress = whiteBoardInfo.server;
                AcsServerAddress = whiteBoardInfo.server;
                ReInitWhiteBoard();
            });
            log.Info("EventWhiteBoardIndication end");
        }

        private void ReInitWhiteBoard()
        {
            if (null != _contentWhiteboard)
            {
                log.Info("Content white board has been constructed! Maybe the request arrived during conference ongoing for re-register.");
                return;
            }

            CreateWhiteboard();
            log.InfoFormat("_contentWhiteboard.IsActive: {0}, this.IsActive: {1}, LayoutBackgroundWindow.Instance.IsActive: {2}", _contentWhiteboard.IsActive, this.IsActive, LayoutBackgroundWindow.Instance.IsActive);
        }

        private void LoadWhiteBoard()
        {
            log.Info("LoadWhiteBoard");
            Application.Current.Dispatcher.InvokeAsync(() => {
                // workaround: active VideoPeopleWindow. the VideoPeopleWindow is not active in the following case:
                // 1. join conf with mic muted, 2. speak to show mute prompt, 3. when prompt disapeared, the VideoPeopleWindow is not acivated and show back
                // note: if not load whiteboard, this issue is not occurred.
                if (!VideoPeopleWindow.Instance.IsActive)
                {
                    VideoPeopleWindow.Instance.Activate();
                    VideoPeopleWindow.Instance.Topmost = true;
                    VideoPeopleWindow.Instance.Topmost = false;
                    VideoPeopleWindow.Instance.Focus();
                }
            });

            CancelLoadWhiteBoardWindowWorker();

            _loadWhiteBoardWindowWorker = new BackgroundWorker();
            _loadWhiteBoardWindowWorker.DoWork += new DoWorkEventHandler((sender, e) =>
            {
                CallController.Instance.GetAcsInfo();
            });
            _loadWhiteBoardWindowWorker.RunWorkerCompleted += new RunWorkerCompletedEventHandler((sender, e) =>
            {
                log.Info("Load white board worker completed.");
                if (e.Cancelled)
                {
                    log.Info("Load white board worker is cancelled.");
                    return;
                }
                else if (e.Error != null)
                {
                    log.ErrorFormat("failed to GetAcsInfo, exception: {0}", e.Error);
                }

                log.Info("Got acs info and begin to load white board");
                if (null == _contentWhiteboard)
                {
                    log.Info("_contentWhiteboard is null, don't load the white board");
                    return;
                }
                
                Application.Current.Dispatcher.InvokeAsync(() => {
                    // check _contentWhiteboard again in UI thread to avoid the object is released
                    if (null == _contentWhiteboard)
                    {
                        log.Info("_contentWhiteboard is null, don't load the white board");
                        return;
                    }
                    log.InfoFormat("Begin to load white board url., _contentWhiteboard.IsActive: {0}", _contentWhiteboard.IsActive);
                    // real deploy we use nginx or SLB as reverse proxy port as 80, so could be skip it here.
                    if (CallController.Instance.acsInfo != null)
                    {
                        if (string.IsNullOrEmpty(AcsServerAddress))
                        {
                            log.InfoFormat("Acs server address is invalid and use dorado zone address:{0}", CloudApiManager.Instance.DoradoZoneAddress);
                            _whiteboardServerAddr = CloudApiManager.Instance.DoradoZoneAddress;
                        }
                        else
                        {
                            _whiteboardServerAddr = AcsServerAddress;
                        }
                    }
                    else
                    {
                        _whiteboardServerAddr = Utils.GetAcsServerAddressFromConfig();
                        log.InfoFormat("ACS info is null, use the config address: {0}", _whiteboardServerAddr);
                    }

                    _contentWhiteboard.WhiteBoardServerAddr = _whiteboardServerAddr;
                    if (string.IsNullOrEmpty(_whiteboardServerAddr))
                    {
                        log.Info("whiteboard server is not configured");
                        return;
                    }

                    if (CallController.Instance.acsInfo != null && CallController.Instance.acsInfo.acsJsonWebToken != null)
                    {
                        _whiteboardUrl = string.Format(
                            ACS_URL
                            , _whiteboardServerAddr
                            , CallController.Instance.acsInfo.acsJsonWebToken
                        );
                    }
                    else
                    {
                        _boardRoom = CallController.Instance.ConferenceNumber;
                        _whiteboardUrl = string.Format(
                            ACS_TEST_URL
                            , _whiteboardServerAddr
                            , Utils.GetUserName(LoginManager.Instance.ServiceType)
                            , _boardRoom
                        );
                    }
                    log.InfoFormat("Load white board browser: {0}", _whiteboardUrl);
                    _contentWhiteboard.Browser.Load(_whiteboardUrl);
                    _contentWhiteboard.getJwtParam();
                    StartCheckWhiteBoardConnectionTimer();
                    log.InfoFormat("Load white board url end. _contentWhiteboard.IsActive: {0}", _contentWhiteboard.IsActive);
                });

                log.Info("Load white board worker completed end.");

                (sender as BackgroundWorker).Dispose();
            });
            _loadWhiteBoardWindowWorker.WorkerSupportsCancellation = true;
            _loadWhiteBoardWindowWorker.RunWorkerAsync();
        }

        private void StartCheckWhiteBoardConnectionTimer()
        {
            if (null == _checkWhiteBoardConnectionTimer)
            {
                _checkWhiteBoardConnectionTimer = new System.Timers.Timer();
                _checkWhiteBoardConnectionTimer.Interval = 10 * 1000;
                _checkWhiteBoardConnectionTimer.AutoReset = false;
                _checkWhiteBoardConnectionTimer.Elapsed += CheckWhiteBoardConnectionTimer_Elapsed;
            }

            _checkWhiteBoardConnectionTimer.Start();
        }

        private void CheckWhiteBoardConnectionTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            if (!CallController.Instance.IsConnected2WhiteBoard)
            {
                log.Info("Timeout for receive white board connected. Reconnection to white board.");
                LoadWhiteBoard();
            }
        }

        private void StopCheckWhiteBoardConnectionTimer()
        {
            log.Info("stop check white board connection timer");
            if (null == _checkWhiteBoardConnectionTimer)
            {
                return;
            }
            _checkWhiteBoardConnectionTimer.Stop();
        }

        private void HideSelectShareContentModeWindow()
        {
            Application.Current.Dispatcher.InvokeAsync(() => {
                if (!_isShowSelectedContentMode)
                {
                    return;
                }
                log.Info("Hide SelectShareContentModeWindow");
                _isShowSelectedContentMode = false;
                if (null == _selectShareContentModeWindow)
                {
                    return;
                }
                _selectShareContentModeWindow.ListenerSelectedShareContent -= new SelectShareContentModeWindow.ListenerSelectedShareContentHandler(SelectedShareContentMode);
                //_selectShareContentModeWindow.Hide();
                _selectShareContentModeWindow.Close();
                _selectShareContentModeWindow = null;
            });
        }

        private void HideSelectMoreContentModeWindow()
        {
            Application.Current.Dispatcher.InvokeAsync(() => {
                if (!_isShowSelectedMoreContentMode)
                {
                    return;
                }
                log.Info("Hide SelectMoreContentModeWindow");
                _isShowSelectedMoreContentMode = false;
                if (null == _selectMoreContentModeWindow)
                {
                    return;
                }
                _selectMoreContentModeWindow.ListenerSelectedMoreContent -= new SelectMoreContentModeWindow.ListenerSelectedMoreContentHandler(SelectedMoreContentMode);
                //_selectMoreContentModeWindow.Hide();
                _selectMoreContentModeWindow.Close();
                _selectMoreContentModeWindow = null;
            });
        }

        private void ShowNormalCellsBtn_Click(object sender, RoutedEventArgs e)
        {
            ShowNormalCellsChanged?.Invoke(sender, new EventArgs());
        }

        private void EVSdkWrapper_EventParticipant(int number)
        {
            Application.Current.Dispatcher.InvokeAsync(() => {
                this.confManagementBtn.ExtraInfoText = number.ToString();
            });
        }

        private void EVSdkWrapper_EventRemoteMicMuted(bool micMuted)
        {
            log.InfoFormat("EVSdkWrapper_EventRemoteMicMuted: {0}", micMuted);
            Application.Current.Dispatcher.InvokeAsync(() => {
                IsAudioMuted = !CallController.Instance.IsMicEnabled();
            });
        }

        private void LoginManager_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if ("LoginToken" == e.PropertyName)
            {
                if (string.IsNullOrEmpty(LoginManager.Instance.LoginToken))
                {
                    return;
                }

                log.Info("LoginToken updated and GetWhiteBoardPresenterName");
                GetWhiteBoardPresenterName(_presenterDeviceId);
            }
            else if ("LoginUserInfo" == e.PropertyName)
            {
                OnPropertyChanged("MoreModeVisibility");
            }
        }

        #endregion

        #region -- Protected Methods --

        override protected void OnWindowHidden()
        {
            HideSelectShareContentModeWindow();
            HideSelectMoreContentModeWindow();
        }

        protected override void OnClosed(EventArgs e)
        {
            CallController.Instance.CallStatusChanged -= OnCallStatusChanged;
            CallController.Instance.ContentStreamStatusChanged -= OnContentStreamStatusChanged;
            EVSdkManager.Instance.EventWhiteBoardIndication -= EVSdkWrapper_EventWhiteBoardIndication;

            base.OnClosed(e);
        }

        #endregion
    }


    class CallbackObjectForJs
    {
        private readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        public LayoutOperationbarWindow MainModel { set; get; }

        public void showMessage(string message)
        {
            MessageBox.Show(message);
        }

        public void showBoard(string board_master)
        {
            log.InfoFormat("ACS: showBoard call back, presenter is {0}", board_master);
            
            // when the current role is the receiver, we can not get the sender's device id, so we get the party name by the below method
            ulong deviceId;
            ulong.TryParse(board_master, out deviceId);
            log.InfoFormat("showBoard is invoked, device id: {0}", deviceId);
            this.MainModel.GetWhiteBoardPresenterName(deviceId);
        }

        public void hideBoard()
        {
            ////log.Info("Hide board");
            ////if (this.mainModel.IsWhiteBoardShowed())
            ////{
            ////    // in later hideBoard() reload will connect to the acs again.
            ////    CallController.Instance.IsConnected2WhiteBoard = false;
            ////    this.mainModel.hideBoard();
            ////}
        }

        public void boardConnected()
        {
            log.Info("white board connected.");
            if (!CallController.Instance.IsConnected2WhiteBoard)
            {
                CallController.Instance.IsConnected2WhiteBoard = true;
                this.MainModel.OnConnect2WhiteBoard();
            }
        }
    }
    
}

