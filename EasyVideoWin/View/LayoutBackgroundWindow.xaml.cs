using EasyVideoWin.Helpers;
using EasyVideoWin.Model;
using log4net;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
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
using System.ComponentModel;

namespace EasyVideoWin.View
{
    /// <summary>
    /// Interaction logic for LayoutBackgroundWindow.xaml
    /// </summary>
    public partial class LayoutBackgroundWindow : Window, IDisposable, IMasterDisplayWindow
    {
        #region -- Members --
        
        public class PartyInfo
        {
            public bool IsMicMuted { get; set; }
        }

        private enum IndependentActiveWindowType
        {
            Empty
            , ReceiveContentWindow
            , WhiteBoardWindow
            , MediaStatistics
        }

        private readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        public static LayoutBackgroundWindow _instance = new LayoutBackgroundWindow();

        private const string SPEAKER_BORDER_COLOR                   = "#ffae00";
        private const string LOVAL_VIDEO_BORDER_COLOR               = "#4381ff";
        private const int LAYOUT_OPERATIONBAR_HEIGHT                = 68;
        private const int MESSAGE_OVERLAY_HEIGHT                    = 64;
        private const int SMALL_VIDEO_WINDOW_WIDTH                  = 176;
        private const int SMALL_VIDEO_WINDOW_HEIGHT                 = 99;
        private const int MAX_LAYOUT_CELL_COUNT                     = 16;
        private const int NORMAL_CELLS_SECTION_BAR_HEIGHT           = 26;
        private const int NORMAL_CELLS_SHOW_MAX_COUNT               = 5;
        private const int CANCEL_FOCUS_VIDEO_WINDOW_LEFT            = 20;
        private const int CANCEL_FOCUS_VIDEO_WINDOW_TOP             = 28;
        private LayoutCellWindow[] _layoutCells                     = new LayoutCellWindow[MAX_LAYOUT_CELL_COUNT];
        private LayoutCellWindow _localVideoCell                    = new LayoutCellWindow();
        private LayoutNormalCellsSectionWindow _normalCellsSection  = new LayoutNormalCellsSectionWindow();
        private LayoutTitlebarWindow _titlebar                      = new LayoutTitlebarWindow();
        private LayoutCellOperationbarWindow _baseWindow            = new LayoutCellOperationbarWindow();
        private LayoutOperationbarWindow _layoutOperationbar        = new LayoutOperationbarWindow();
        private LayoutCellOperationbarWindow _localVideoCellOperationbar = new LayoutCellOperationbarWindow();
        private LayoutCellWindow _speakerModeMainCell;
        private LayoutCellWindow[] _layoutNormalCells;
        private LayoutCellWindow _currentSpeakerCell;
        private LayoutCellWindow _movingLayoutCell = null;
        private bool _isSpeakerMode = true;
        private int _sourceInitializedCellsCount = 0;
        private Dictionary<IntPtr, LayoutCellWindow> _dictLayoutCellHandle = new Dictionary<IntPtr, LayoutCellWindow>();
        private int _layoutSpeakerIdx;
        private IntPtr _localVideoCellHandle = IntPtr.Zero;
        private int _normalCellsStartIdx = 0;
        private SolidColorBrush _speakerBorderBrush;
        private SolidColorBrush _localVideoBorderBrush;

        private System.Timers.Timer _timerHideOperationbar  = new System.Timers.Timer();
        private long _timeTicksLastMove                     = DateTime.Now.Ticks;
        
        private const double CELL_BORDER_LENGTH = 2;
        private Border _speakerBorder = new Border();
        private double _cellBorderWidth;
        private double _cellBorderHeight;
        private bool _showNormalCellsSection = true;
        private bool _showNormalCellsNavigateUp = false;
        private bool _showNormalCellsNavigateDown = false;
        private bool _isFocusVideoMode = false;
        private LayoutCancelFocusVideoWindow _cancelFocusVideoWindow = new LayoutCancelFocusVideoWindow();
        private LayoutRecordingIndicationWindow _recordingIndicationWindow = new LayoutRecordingIndicationWindow();
        private ManagedEVSdk.Structs.EVRecordingInfoCli _recordingIndication = new ManagedEVSdk.Structs.EVRecordingInfoCli();
        private MessageOverlayWindow _messageOverlayWindow = new MessageOverlayWindow();
        private ConfManagementWindow _confManagermentWindow = null; // new ConfManagementWindow();
        
        private System.Timers.Timer _timerMessageOverlayRoll = new System.Timers.Timer();
        private int _messageOverlayDisplayRepetitions;
        private int _messageOverlayDisplayRolledCount;
        private int _messageOverlayVerticalBorder;

        private long _timeTicksLastMouseLeftButtonDown;
        private System.Windows.Point _moveStartPoint;
        private bool _moveDetected;

        private Rect _videoPeopleWindowRect;

        private bool _disableChangeLayout = false;
        private IndependentActiveWindowType _independentActiveWindow;
        private bool _autoHidePartyName;
        private IntPtr m_hNotifyDevNode = IntPtr.Zero;
        private System.Timers.Timer _updateAudioDevicesTimer;
        private IntPtr _handle;
        private ModalPromptDlg _modalPromptDlg = null;
        private bool _disablePrompt = false;

        #endregion

        #region -- Properties --

        public static LayoutBackgroundWindow Instance
        {
            get
            {
                return _instance;
            }
        }

        public bool IsSpeakerMode
        {
            get
            {
                return _isSpeakerMode;
            }
        }

        public ManagedEVSdk.Structs.EVLayoutIndicationCli LayoutIndication { get; set; } = new ManagedEVSdk.Structs.EVLayoutIndicationCli();
        
        public bool IsRemoteMuted { get; set; }

        public bool IsDisposed { get; private set; } = false;
        
        public LayoutOperationbarWindow LayoutOperationbarWindow
        {
            get
            {
                return _layoutOperationbar;
            }
        }

        #endregion

        #region -- Constructor --

        private LayoutBackgroundWindow()
        {
            InitializeComponent();

            VideoPeopleWindow.Instance.TitleBarWindowMove += OnTitleBarWindowMove;
            VideoPeopleWindow.Instance.StateChanged += new EventHandler(OnVideoPeopleWindowStateChanged);
            VideoPeopleWindow.Instance.PropertyChanged += VideoPeopleWindow_PropertyChanged;
            this.SourceInitialized += LayoutBackgroundWindow_SourceInitialized;
            
            _speakerBorderBrush = new SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString(SPEAKER_BORDER_COLOR));
            _localVideoBorderBrush = new SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString(LOVAL_VIDEO_BORDER_COLOR));
            this.sectionGrid.Children.Add(this._speakerBorder);
            _speakerBorder.Visibility = Visibility.Collapsed;
            _speakerBorder.VerticalAlignment = VerticalAlignment.Top;
            _speakerBorder.BorderThickness = new Thickness(20);
            _speakerBorder.BorderBrush = _speakerBorderBrush;

            LayoutIndication = new ManagedEVSdk.Structs.EVLayoutIndicationCli();
            IsRemoteMuted = false;

            _layoutOperationbar.EventInitScreen += LayoutOperationbar_EventInitScreen;
            _layoutOperationbar.EventRequestSpeaker += LayoutOperationbar_EventRequestSpeaker;

            this.Activated += LayoutBackgroundWindow_Activated;
            this.Deactivated += LayoutBackgroundWindow_Deactivated;
        }
        

        #endregion

        #region -- Public Methods --

        public void InitRelevantElements()
        {
            _layoutOperationbar.InitContentWindow();

            this.MouseMove += LayoutBackgroundWindow_MouseMove;
            this.MouseDown += LayoutBackgroundWindow_MouseDown;
            this.Width = 0;
            this.Height = 0;
            this.Show();

            double width;
            double height;

            for (int i = 0; i < _layoutCells.Length; ++i)
            {
                var cell = _layoutCells[i] = new LayoutCellWindow();
                cell.Activated += LayoutCell_Activated;
                cell.MouseMove += LayoutCell_MouseMove;
                cell.MouseDown += LayoutCell_MouseDown;
                cell.SourceInitialized += Cell_SourceInitialized;
                cell.OnWindowMoved += LayoutCell_OnWindowMoved;
                cell.DpiChanged += LayoutCell_DpiChanged;
                cell.MouseLeftButtonUp += LayoutCell_MouseLeftButtonUp;
                cell.MouseDoubleClick += LayoutCell_MouseDoubleClick;
                cell.SetProperWindowSize(0, 0);
                cell.Operationbar.Activated += LayoutCell_Activated;
                width = cell.Operationbar.Width;
                height = cell.Operationbar.Height;
                cell.Operationbar.SetProperWindowSize(0, 0);
                cell.Show();
                cell.Operationbar.Show();
                cell.HideWindow();
                cell.Operationbar.SetProperWindowSize(width, height);
                cell.Operationbar.Owner = cell;
            }
            
            width = _cancelFocusVideoWindow.Width;
            height = _cancelFocusVideoWindow.Height;
            _cancelFocusVideoWindow.SetProperWindowSize(0, 0);
            _cancelFocusVideoWindow.Show();
            _cancelFocusVideoWindow.HideWindow();
            _cancelFocusVideoWindow.SetProperWindowSize(width, height);
            _cancelFocusVideoWindow.MouseLeftButtonDown += CancelFocusVideoWindow_MouseLeftButtonDown;
            
            _titlebar.MouseMove += Titlebar_MouseMove;
            _titlebar.Owner = this;
            _titlebar.Show();
            _titlebar.HideWindow();
            //_titlebar.Hide();

            _baseWindow.Owner = this;
            _baseWindow.Show();
            _baseWindow.HideWindow();

            width = _recordingIndicationWindow.Width;
            height = _recordingIndicationWindow.Height;
            _recordingIndicationWindow.SetProperWindowSize(0, 0);
            _recordingIndicationWindow.Show();
            _recordingIndicationWindow.HideWindow();
            _recordingIndicationWindow.SetProperWindowSize(width, height);
            _recordingIndicationWindow.Owner = _baseWindow;

            width = _messageOverlayWindow.Width;
            height = MESSAGE_OVERLAY_HEIGHT;
            _messageOverlayWindow.SetProperWindowSize(0, 0);
            _messageOverlayWindow.Show();
            _messageOverlayWindow.SetTextMargin(new Thickness(10));
            _messageOverlayWindow.SetFontSize(16d);
            _messageOverlayWindow.SetProperWindowSize(width, height);
            _messageOverlayWindow.HideWindow();
            _messageOverlayWindow.Owner = _baseWindow;
            _timerMessageOverlayRoll.Interval = 10;
            _timerMessageOverlayRoll.Elapsed += TimerMessageOverlayRoll_Elapsed;
            
            _localVideoCell.SourceInitialized += LocalVideoCell_SourceInitialized;
            _localVideoCell.MouseMove += LayoutCell_MouseMove;
            //_localVideoCell.MouseDown += LayoutCell_MouseDown;
            _localVideoCell.OnWindowMoved += LayoutCell_OnWindowMoved;
            _localVideoCell.MouseEnter += LocalVideoCell_MouseEnter;
            _localVideoCell.MouseLeave += LocalVideoCell_MouseLeave;
            _localVideoCell.DpiChanged += LayoutCell_DpiChanged;
            _localVideoCell.MouseLeftButtonUp += LayoutCell_MouseLeftButtonUp;
            _localVideoCell.SetProperWindowSize(0, 0);
            _localVideoCell.CellName = LanguageUtil.Instance.GetValueByKey("ME");
            _localVideoCell.Operationbar.Activated += LayoutCell_Activated;
            width = _localVideoCell.Operationbar.Width;
            height = _localVideoCell.Operationbar.Height;
            _localVideoCell.Operationbar.SetProperWindowSize(0, 0);
            _localVideoCell.Show();
            _localVideoCell.Operationbar.Show();
            _localVideoCell.HideWindow();
            _localVideoCell.Operationbar.SetProperWindowSize(width, height);
            _localVideoCell.Operationbar.Owner = _localVideoCell;

            _layoutOperationbar.PropertyChanged += LayoutOperationbar_PropertyChanged;
            _layoutOperationbar.MouseMove += LayoutOperationbar_MouseMove;
            _layoutOperationbar.SvcLyoutModeChanged += LayoutOperationbar_SvcLayoutModeChanged;
            _layoutOperationbar.ShowNormalCellsChanged += LayoutOperationbar_ShowNormalCellsChanged;
            _layoutOperationbar.ConfManagementChanged += LayoutOperationbar_ConfManagementChanged;
            _layoutOperationbar.SetProperWindowSize(0, 0);
            _layoutOperationbar.Show();
            _layoutOperationbar.HideWindow();
                        
            _localVideoCellOperationbar.Activated += LayoutCell_Activated;
            _localVideoCellOperationbar.SetProperWindowSize(0, 0);
            _localVideoCellOperationbar.Show();
            _localVideoCellOperationbar.HideWindow();
            _localVideoCellOperationbar.Hide();


            _timerHideOperationbar.Elapsed += new System.Timers.ElapsedEventHandler((object source, System.Timers.ElapsedEventArgs args) =>
            {
                Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    log.Info("Handle timer operation bar begin");
                    _titlebar.HideWindow();
                    if (_autoHidePartyName)
                    {
                        HideAllCellOperationbars();
                    }
                    _localVideoCellOperationbar.HideWindow();
                    _layoutOperationbar.HideWindow();
                    UpdateLayoutOperationbarsPosition();

                    _timerHideOperationbar.Stop();
                    log.Info("Handle timer operation bar end");
                });

            });

            _timerHideOperationbar.Interval = 5 * 1000;
            //_timerHideOperationbar.Enabled = true;
            _timerHideOperationbar.AutoReset = true;
            
            _normalCellsSection.Show();
            _normalCellsSection.HideWindow();
            _normalCellsSection.InitDecorationBorder(NORMAL_CELLS_SHOW_MAX_COUNT);
            _normalCellsSection.OnWindowMoved += NormalCellsSection_OnWindowMoved;
            _normalCellsSection.CellsNavigateChanged += NormalCellsSection_CellsNavigateChanged;
            _normalCellsSection.CustomWindowStateChanged += NormalCellsSection_CustomWindowStateChanged;
            _normalCellsSection.DpiChanged += NormalCellsSection_DpiChanged;
            _normalCellsSection.MouseMove += NormalCellsSection_MouseEnter;
            _normalCellsSection.MouseLeave += NormalCellsSection_MouseLeave;
            log.InfoFormat("_normalCellsSection, IsWindowHidden: {0}, left: {1}, top: {2}", _normalCellsSection.IsWindowHidden, _normalCellsSection.Left, _normalCellsSection.Top);
            
            _titlebar.Owner = _messageOverlayWindow;
            _layoutOperationbar.Owner = _messageOverlayWindow;
            this.Hide();

         
        }
        
        public void ShowWindow(bool bFirst = false, bool isDpiChanged = false)
        {
            log.InfoFormat("Show window, isFirst:{0}, isDpiChanged:{1}", bFirst, isDpiChanged);
            _videoPeopleWindowRect = VideoPeopleWindow.Instance.GetWindowRect();
            
            double height = 0;
            double width = 0;
            double top = 0;
            double left = 0;
            if ((_videoPeopleWindowRect.Width / (_videoPeopleWindowRect.Height - VideoPeopleWindow.Instance.TitlebarHeight)) > (16.0 / 9.0))
            {
                height = _videoPeopleWindowRect.Height - VideoPeopleWindow.Instance.TitlebarHeight;
                width = height * 16.0 / 9.0;
            }
            else
            {
                width = _videoPeopleWindowRect.Width;
                height = width * 9.0 / 16.0;
            }

            if (WindowState.Maximized == VideoPeopleWindow.Instance.WindowState && VideoPeopleWindow.Instance.FullScreenStatus)
            {
                if ((_videoPeopleWindowRect.Width / _videoPeopleWindowRect.Height) > (16.0 / 9.0))
                {
                    height = _videoPeopleWindowRect.Height;
                    width = height * 16.0 / 9.0;
                }
                else
                {
                    width = _videoPeopleWindowRect.Width;
                    height = width * 9.0 / 16.0;
                }
                top += _videoPeopleWindowRect.Top + (_videoPeopleWindowRect.Height - height) / 2;
            }
            else
            {
                top = _videoPeopleWindowRect.Top + VideoPeopleWindow.Instance.TitlebarHeight;
                top += (_videoPeopleWindowRect.Height - VideoPeopleWindow.Instance.TitlebarHeight - height) / 2;
            }

            left = _videoPeopleWindowRect.Left + (_videoPeopleWindowRect.Width - width) / 2;
            this.Width = width;
            this.Height = height;
            this.Left = left;
            this.Top = top;

            this.Owner = VideoPeopleWindow.Instance;
            this.WindowState = WindowState.Normal;
            this.Show();

            if (bFirst)
            {
                SetInitialPosOfNormalCellsSection();
            }

            SetRecordingIndicationWindow2ProperPos();

            if (string.IsNullOrEmpty(_messageOverlayWindow.ContentText))
            {
                _messageOverlayWindow.HideWindow();
            }
            else
            {
                SetMessageOverlayWindow2ProperPos();
            }

            ChangeSvcLayoutMode(_isSpeakerMode, isDpiChanged);

            SetCancelFocusVideoWindow2ProperPos();
            
            _layoutOperationbar.HideWindow();
            if (_autoHidePartyName)
            {
                if (_isSpeakerMode)
                {
                    if (null != _speakerModeMainCell && !_speakerModeMainCell.Operationbar.IsWindowHidden)
                    {
                        _speakerModeMainCell.Operationbar.HideWindow();
                    }
                }
                else
                {
                    HideAllCellOperationbars();
                }
            }
            else
            {
                ShowAllLayoutCellOperationbars();
            }
            
            log.Info("Show window end.");
        }
        
        public void HideWindow(bool isCallEnded = false)
        {
            log.InfoFormat("Begin to hide windows related. Visibility: {0}, WindowState: {1}", this.Visibility, this.WindowState);

            _cancelFocusVideoWindow.HideWindow();
            _recordingIndicationWindow.HideWindow();
            for (int i = 0; i < _layoutCells.Length; ++i)
            {
                _layoutCells[i].HideWindow();
            }

            _localVideoCell.HideWindow();
            _titlebar.HideWindow();
            _layoutOperationbar.HideWindow();
            _localVideoCellOperationbar.HideWindow();
            _normalCellsSection.HideWindow();
            _messageOverlayWindow.HideWindow();
            
            if (isCallEnded)
            {
                OnCallEnded();
            }

            this.Hide();
            log.InfoFormat("Hide executed for layout background window, visibility:{0}", this.Visibility);
        }
        
        public void OnCallEnded()
        {
            log.InfoFormat("Call ended and set the controls to default. visibility:{0}", this.Visibility);
            // set to speaker mode and clear the layout cell to avoid to see the last frame when reconnect
            _isSpeakerMode = true;
            _isFocusVideoMode = false;
            _disableChangeLayout = false;
            _cancelFocusVideoWindow.HideWindow();
            _recordingIndicationWindow.HideWindow();
            _recordingIndication.state = ManagedEVSdk.Structs.EV_RECORDING_STATE_CLI.EV_RECORDING_STATE_NONE;
            PromptWindow.Instance.CloseWindow();
            _messageOverlayWindow.ContentText = "";
            _messageOverlayWindow.HideWindow();
            if (null != _confManagermentWindow)
            {
                if (Visibility.Visible == _confManagermentWindow.Visibility)
                {
                    _confManagermentWindow.Close();
                }
                _confManagermentWindow.Dispose();
                _confManagermentWindow = null;
            }
            if (null != _modalPromptDlg && Visibility.Visible == _modalPromptDlg.Visibility)
            {
                _modalPromptDlg.Owner = null;
                _modalPromptDlg.Visibility = Visibility.Collapsed;
            }
            log.InfoFormat("Before hide layout cells, window visibility:{0}", this.Visibility);
            for (var i = 0; i < _layoutCells.Length; ++i)
            {
                log.InfoFormat("hide layout cell:{0}, visibility:{1}", i, _layoutCells[i].Visibility);
                _layoutCells[i].Owner = null;
                _layoutCells[i].HideWindow();
                //clear the layout cell to avoid to see the last frame when reconnect
                _layoutCells[i].Hide();
                _layoutCells[i].Show();
            }
            log.InfoFormat("After hide layout cells, window visibility:{0}", this.Visibility);
            _normalCellsSection.SetProperWindowSize(0, 0);
            _layoutSpeakerIdx = -1;
            LayoutIndication = new ManagedEVSdk.Structs.EVLayoutIndicationCli();
            IsRemoteMuted = false;
            _layoutOperationbar.TreatmentOnCallEnded();
            this.Owner = null;
            _localVideoCell.Operationbar.IsMicMuted = false;

            log.Info("Call ended and collect garbage");
            SystemMonitorUtil.Instance.CollectGarbage();
        }

        public void SetLocalVideoHandle()
        {
            if (IntPtr.Zero == _localVideoCellHandle)
            {
                _localVideoCellHandle = _localVideoCell.Handle;
            }
            CallController.Instance.SetLocalVideoWindow(_localVideoCellHandle);
        }

        public void InitSetting()
        {
            _autoHidePartyName = Utils.GetAutoHidePartyName();

            _layoutOperationbar.ShowNormalCellsBtnVisibility = Visibility.Collapsed;
            _showNormalCellsSection = true;
            if (null == _confManagermentWindow)
            {
                _confManagermentWindow = new ConfManagementWindow();
            }

            // update the string according the language setting
            _localVideoCell.CellName = EVSdkManager.Instance.GetDisplayName();
            _disablePrompt = Utils.GetDisablePrompt();
            log.InfoFormat("InitSetting, _disablePrompt: {0}", _disablePrompt);
        }

        public void StartWhiteboard()
        {
            _layoutOperationbar.StartWhiteboard();
        }

        public void ExitWhiteboard()
        {
            _layoutOperationbar.ExitWhiteboard();
        }

        public void StartContent()
        {
            _layoutOperationbar.StartContent();
        }

        public void ExitContent()
        {
            _layoutOperationbar.ExitContent();
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected void Dispose(bool disposing)
        {
            if (!IsDisposed)
            {
                if (disposing)
                {
                    UnregisterNotification();
                }
                //Clean up unmanaged resources  
            }
            IsDisposed = true;
        }

        #endregion

        #region -- Private Methods --

        private void LayoutBackgroundWindow_SourceInitialized(object sender, EventArgs e)
        {
            VideoPeopleWindow.Instance.DpiChanged += MainWindow_DpiChanged;
            
            Utils.HideWindowInAltTab(this);

            this.IsVisibleChanged                           += LayoutBackgroundWindow_IsVisibleChanged;
            EVSdkManager.Instance.EventRecordingIndication  += EVSdkWrapper_EventRecordingIndication;
            EVSdkManager.Instance.EventMessageOverlay       += EVSdkWrapper_EventMessageOverlay;
            EVSdkManager.Instance.EventLayoutSiteIndication += EVSdkWrapper_EventLayoutSiteIndication;
            EVSdkManager.Instance.EventWarn                 += EVSdkWrapper_EventWarn;
            EVSdkManager.Instance.EventContent              += EVSdkWrapper_EventContent;
            EVSdkManager.Instance.EventMuteSpeakingDetected += EVSdkWrapper_EventMuteSpeakingDetected;

            HwndSource hwndSource = (HwndSource)HwndSource.FromVisual((Window)sender);
            hwndSource.AddHook(DragHook);
            RegisterNotification(new Guid(DbtUtil.GUID_DEVINTERFACE_AUDIO_DEVICE));
        }

        private void EVSdkWrapper_EventMuteSpeakingDetected()
        {
            if (_disablePrompt)
            {
                log.Info("Received EventMuteSpeakingDetected, but prompt disabled.");
                return;
            }

            log.Info("EventMuteSpeakingDetected");
            Application.Current.Dispatcher.InvokeAsync(() =>
            {
                log.Info("EventMuteSpeakingDetected - show prompt");
                string prompt = LanguageUtil.Instance.GetValueByKey("MIC_MUTED");
                ShowPromptWindow(prompt, 3000);
            });
        }

        private void EVSdkWrapper_EventContent(ManagedEVSdk.Structs.EVContentInfoCli contentInfo)
        {
            log.InfoFormat("EventContent, status:{0}", contentInfo.status);
            if (ManagedEVSdk.Structs.EV_CONTENT_STATUS_CLI.EV_CONTENT_DENIED == contentInfo.status)
            {
                Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    string prompt = LanguageUtil.Instance.GetValueByKey("SEND_CONTENT_DENIED");
                    ShowPromptWindow(prompt, 5000);
                });
            }
            
            log.Info("EventContent end");
        }

        private void LayoutBackgroundWindow_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            log.InfoFormat("Visible changed, visibility:{0}", this.Visibility);
            
            if (   
                   Visibility.Visible == this.Visibility
                && CallStatus.Connected != CallController.Instance.CurrentCallStatus
            )
            {
                string info = null;
                // set true for get file name and line number
                StackTrace st = new StackTrace(true);
                // Get call current stacks  
                StackFrame[] sf = st.GetFrames();
                for (int i = 0; i < sf.Length; ++i)
                {
                    info = info + "\r\n" + " FileName=" + sf[i].GetFileName() + " fullname=" + sf[i].GetMethod().DeclaringType.FullName + " function=" + sf[i].GetMethod().Name + " FileLineNumber=" + sf[i].GetFileLineNumber();
                }

                // for unknown reason, the window changed to visible by system invoke
                log.InfoFormat("For unknown reason, the window changed to visible by system invoke when not Connected or Streaming. Stack info:{0}", info);
                //this.Hide();
            }
        }

        private void MainWindow_DpiChanged()
        {
            if (CallStatus.Connected != CallController.Instance.CurrentCallStatus)
            {
                return;
            }

            ShowWindow(false, true);
        }
        
        private void OnVideoPeopleWindowStateChanged(object sender, EventArgs e)
        {
            if (CallStatus.Connected != CallController.Instance.CurrentCallStatus)
            {
                return;
            }

            log.InfoFormat("Received video people window state changed, state:{0}, call status:{1}", VideoPeopleWindow.Instance.WindowState, CallController.Instance.CurrentCallStatus);

            switch (VideoPeopleWindow.Instance.WindowState)
            {
                case WindowState.Minimized:
                    HideWindow();
                    break;
                case WindowState.Maximized:
                    ShowWindow();
                    break;
                case WindowState.Normal:
                    ShowWindow();
                    break;
                default:
                    break;
            }
        }
        
        private void LayoutBackgroundWindow_MouseMove(object sender, MouseEventArgs e)
        {
            if (!CheckMoveValidity(e.GetPosition(this)))
            {
                return;
            }

            if (_layoutOperationbar.IsWindowHidden)
            {
                log.InfoFormat("LayoutBackgroundWindow_MouseMove, show layout operation bar, visibility:{0}", this.Visibility);
            }

            SetShowStatus4LayoutOperationbar(true);
        }

        private void LayoutBackgroundWindow_MouseDown(object sender, MouseButtonEventArgs e)
        {
            log.InfoFormat("LayoutBackgroundWindow_MouseDown, _layoutOperationbar.IsWindowHidden: {0}", _layoutOperationbar.IsWindowHidden);
            if (_layoutOperationbar.IsWindowHidden)
            {
                log.Info("LayoutBackgroundWindow_MouseDown, show layout operation bar.");
            }

            SetShowStatus4LayoutOperationbar(_layoutOperationbar.IsWindowHidden);
        }
        
        private void LayoutCell_MouseDown(object sender, MouseButtonEventArgs e)
        {
            long nowTicks = DateTime.Now.Ticks;
            if (new TimeSpan(nowTicks - _timeTicksLastMouseLeftButtonDown).TotalMilliseconds < ToolWindowSwitchHelper.MOUSE_LEFT_BUTTON_DOWN_TIME_INTERVAL_IN_MILLISECONDS)
            {
                return;
            }

            _timeTicksLastMouseLeftButtonDown = nowTicks;

            LayoutCellWindow cell = sender as LayoutCellWindow;
            if (_isSpeakerMode)
            {
                if (_speakerModeMainCell != cell)
                {
                    return;
                }
            }

            if (_layoutOperationbar.IsWindowHidden)
            {
                log.Info("LayoutCell_MouseDown, show layout operation bar.");
            }
            SetShowStatus4LayoutOperationbar(_layoutOperationbar.IsWindowHidden);
            if (!_layoutOperationbar.IsWindowHidden)
            {
                ShowLayoutCellOperationbar(cell);
            }
        }

        private void LayoutCell_MouseMove(object sender, MouseEventArgs e)
        {
            if (!CheckMoveValidity(e.GetPosition(this)))
            {
                return;
            }
            
            LayoutCellWindow cell = sender as LayoutCellWindow;
            if (_isSpeakerMode)
            {
                if (_speakerModeMainCell == cell)
                {
                    if (_layoutOperationbar.IsWindowHidden)
                    {
                        log.Info("LayoutCell_MouseMove, show layout operation bar.");
                    }
                    SetShowStatus4LayoutOperationbar(true);
                }
            }
            else
            {
                if (_layoutOperationbar.IsWindowHidden)
                {
                    log.Info("LayoutCell_MouseMove, show layout operation bar.");
                }
                SetShowStatus4LayoutOperationbar(true);
            }

            ShowLayoutCellOperationbar(cell);
            
            SetTopWindows();

            //if (_isSpeakerMode)
            //{
            //    if (_speakerModeMainCell != cell)
            //    {
            //        return;
            //    }
            //}

            //SetShowStatus4LayoutOperationbar(true);
        }
        
        private void ShowLayoutCellOperationbar(LayoutCellWindow cell, bool forceUpdate = false)
        {
            if (null == cell)
            {
                return;
            }
            if (!cell.Operationbar.IsWindowHidden && !forceUpdate)
            {
                return;
            }

            if (_autoHidePartyName)
            {
                HideAllCellOperationbars();
            }

            uint sitesSize = 0;
            if (null != LayoutIndication)
            {
                sitesSize = LayoutIndication.sites_size;
            }

            if (cell != _localVideoCell)
            {
                var i = 0;
                for (; i < sitesSize; ++i)
                {
                    if (LayoutIndication.sites[i].device_id == cell.DeviceId)
                    {
                        cell.Operationbar.IsMicMuted = LayoutIndication.sites[i].mic_muted;
                        break;
                    }
                }

                if (i >= sitesSize)
                {
                    cell.Operationbar.IsMicMuted = false;
                }
            }
                        
            if (string.IsNullOrEmpty(cell.Operationbar.CellName))
            {
                cell.Operationbar.HideWindow();
                return;
            }
            else
            {
                cell.Operationbar.ShowWindow();
            } 

            SetLayoutCellOperationbar2ProperPos(cell);
        }

        private void SetLayoutCellOperationbar2ProperPos(LayoutCellWindow cell)
        {
            double left = cell.Left;
            double top = 0;
            double width = cell.Width - 3;
            double height = cell.Operationbar.InitialHeight;
            int cellBottom = (int)(cell.Top + cell.Height);
            // maybe cellBottom is only larger than (_layoutOperationbar.Top + _layoutOperationbar.Height) 0.xx, so convert to int 
            if (cellBottom > (int)_layoutOperationbar.Top && cellBottom <= (int)(_layoutOperationbar.Top + _layoutOperationbar.Height))
            {
                top = _layoutOperationbar.Top - cell.Operationbar.Height;
                //    log.Info("Cell operation bar should be at the top of layout operation bar, cell:" + cell.CellId);
            }
            else
            {
                top = cell.Top + cell.Height - cell.Operationbar.Height;
            }
            if (cell.Operationbar.SetProperWindowPos(left, top, width, height))
            {
                log.InfoFormat("Set the cell operation bar to proper position, cell name: {0}, left: {1}, top: {2}, width: {3}, height: {4}"
                    , cell.CellName, cell.Operationbar.Left, cell.Operationbar.Top, cell.Operationbar.Width, cell.Operationbar.Height);
            }
            else
            {
                log.InfoFormat("The position of cell operation bar is not changed, cell name={0}", cell.CellName);
            }
            Utils.SetWindow2NoTopMost(cell.Operationbar.Handle);
        }

        private bool CheckMoveValidity(Point point)
        {
            bool valid = false;
            long nowTicks = DateTime.Now.Ticks;
            if (new TimeSpan(nowTicks - _timeTicksLastMove).TotalMilliseconds > ToolWindowSwitchHelper.MOUSE_MOVE_TIME_INTERVAL_IN_MILLISECONDS)
            {
                _moveStartPoint = point; // e.Location;
                _moveDetected = false;
            }
            else
            {
                if (_moveDetected)
                {
                    valid = true;
                }
                else
                {
                    Math.Pow(2d, 3d);
                    double distanceSquare = Math.Pow(point.X - _moveStartPoint.X, 2d) + Math.Pow(point.Y - _moveStartPoint.Y, 2d);
                    if (distanceSquare > Math.Pow(ToolWindowSwitchHelper.MOUSE_MOVE_MIN_DISTANCE, 2d))
                    {
                        valid = true;
                        _moveDetected = true;
                    }
                }
            }
            _timeTicksLastMove = nowTicks;

            return valid;
        }

        private void LocalVideoCell_SourceInitialized(object sender, EventArgs e)
        {
            SetLocalVideoHandle();
        }

        private void Cell_SourceInitialized(object sender, EventArgs e)
        {
            ++_sourceInitializedCellsCount;
            if (MAX_LAYOUT_CELL_COUNT == _sourceInitializedCellsCount)
            {
                IntPtr[] handles = new IntPtr[MAX_LAYOUT_CELL_COUNT];
                for (int i=0; i< MAX_LAYOUT_CELL_COUNT; ++i)
                {
                    var handle = _layoutCells[i].Handle;
                    handles[i] = handle;
                    _dictLayoutCellHandle.Add(handle, _layoutCells[i]);
                }
                CallController.Instance.SetVideoWindowIds(handles);

                EVSdkManager.Instance.EventLayoutIndication += EVSdkWrapper_EventLayoutIndication;
                EVSdkManager.Instance.EventLayoutSpeakerIndication += EVSdkWrapper_EventLayoutSpeakerIndication;
            }
        }
        
        private void LayoutOperationbar_MouseMove(object sender, MouseEventArgs e)
        {
            RestartTimerHideOperationbar();
        }

        private void Titlebar_MouseMove(object sender, MouseEventArgs e)
        {
            RestartTimerHideOperationbar();
        }
        
        private void SetShowStatus4LayoutOperationbar(bool show)
        {
            if (show)
            {
                SetProperPosition4LayoutOperationbarAndTitlebar();
                if (VideoPeopleWindow.Instance.FullScreenStatus)
                {
                    _titlebar.ShowWindow();
                }
                _layoutOperationbar.ShowWindow();
                _timerHideOperationbar.Stop();
                _timerHideOperationbar.Start();
            }
            else
            {
                _titlebar.HideWindow();
                _layoutOperationbar.HideWindow();
                if (_autoHidePartyName)
                {
                    if (_isSpeakerMode)
                    {
                        if (!_speakerModeMainCell.Operationbar.IsWindowHidden)
                        {
                            _speakerModeMainCell.Operationbar.HideWindow();
                        }
                    }
                    else
                    {
                        HideAllCellOperationbars();
                    }
                }
            }
            UpdateLayoutOperationbarsPosition();
        }

        private void RestartTimerHideOperationbar()
        {
            _timerHideOperationbar.Stop();
            _timerHideOperationbar.Start();
        }

        private void LayoutCell_Activated(object sender, EventArgs e)
        {
            SetTopWindows();
        }
        
        private bool HasActiveWindow()
        {
            if (WindowState.Minimized == VideoPeopleWindow.Instance.WindowState)
            {
                return true;
            }

            bool flag = _layoutOperationbar.IsReceiveContentWinActive
                    || (null != MediaStatisticsView.Instance && MediaStatisticsView.Instance.IsActive)
                    || (null != _confManagermentWindow && _confManagermentWindow.IsActive)
                    //|| (null != SoftwareUpdateWindow.Instance && SoftwareUpdateWindow.Instance.IsActive)
                    || _layoutOperationbar.IsWhiteboardActive;
                    
            return flag;
        }

        private void SaveIndependenetActiveWindow()
        {
            if (_layoutOperationbar.IsReceiveContentWinActive)
            {
                _independentActiveWindow = IndependentActiveWindowType.ReceiveContentWindow;
                _layoutOperationbar.SetOwner4ReceiveContent(_layoutOperationbar);
                return;
            }

            if (_layoutOperationbar.IsWhiteboardActive)
            {
                _independentActiveWindow = IndependentActiveWindowType.WhiteBoardWindow;
                _layoutOperationbar.SetOwner4WhiteBoardWindow(_layoutOperationbar);
                return;
            }

            if (null != MediaStatisticsView.Instance && MediaStatisticsView.Instance.IsActive)
            {
                _independentActiveWindow = IndependentActiveWindowType.MediaStatistics;
                MediaStatisticsView.Instance.Owner = _layoutOperationbar;
                return;
            }
        }

        private void ResumeIndependenetActiveWindow()
        {
            switch (_independentActiveWindow)
            {
                case IndependentActiveWindowType.Empty:
                    break;
                case IndependentActiveWindowType.ReceiveContentWindow:
                    _layoutOperationbar.SetOwner4ReceiveContent(null);
                    _layoutOperationbar.SetReceiveContentWindowActivate();
                    break;
                case IndependentActiveWindowType.WhiteBoardWindow:
                    _layoutOperationbar.SetOwner4WhiteBoardWindow(null);
                    _layoutOperationbar.SetWhiteBoardWindowActivate();
                    break;
                case IndependentActiveWindowType.MediaStatistics:
                    if (null != MediaStatisticsView.Instance)
                    {
                        MediaStatisticsView.Instance.Owner = null;
                        MediaStatisticsView.Instance.Activate();
                    }
                    break;
            }
            _independentActiveWindow = IndependentActiveWindowType.Empty;
        }

        private void SetTopWindows()
        {
            if (HasActiveWindow())
            {
                return;
            }
            
            // set windows top in order
            if (_isSpeakerMode)
            {
                Utils.SetWindow2Top(_normalCellsSection.Handle);
                SetSoftwareUpdateWindowTop();
                Utils.SetWindow2Top(_layoutOperationbar.Handle);
                Utils.SetWindow2Top(_messageOverlayWindow.Handle);
            }
            else
            {
                Utils.SetWindow2Top(_titlebar.Handle);
                Utils.SetWindow2Top(_layoutOperationbar.Handle);
                Utils.SetWindow2Top(_normalCellsSection.Handle);
                SetSoftwareUpdateWindowTop();
                Utils.SetWindow2Top(_messageOverlayWindow.Handle);
            }
            
        }

        private void SetSoftwareUpdateWindowTop()
        {
            if (Visibility.Visible != SoftwareUpdateWindow.Instance.Visibility)
            {
                return;
            }

            if (_layoutOperationbar != SoftwareUpdateWindow.Instance.Owner && Visibility.Visible == VideoPeopleWindow.Instance.Visibility)
            {
                SoftwareUpdateWindow.Instance.Owner = _layoutOperationbar;
                log.Info("Set soft update window owner to layout operation bar");
            }
            log.Info("Set SoftwareUpdateWindow to top");
            Utils.SetWindow2Top(SoftwareUpdateWindow.Instance);
        }
        
        private void SetLayout4GalleryMode(bool isDpiChanged = false)
        {
            if (null == LayoutIndication)
            {
                return;
            }

            int speakerIdx                          = _layoutSpeakerIdx;
            uint sitesLength                        = LayoutIndication.sites_size;
            ManagedEVSdk.Structs.EVSiteCli[] sites  = LayoutIndication.sites;
            log.Info("Set layout for gallery mode.");
            if (_isFocusVideoMode)
            {
                _isFocusVideoMode = false;
                _cancelFocusVideoWindow.HideWindow();
            }

            int row = 0;
            int col = 0;
            if (1 == sitesLength)
            {
                row = 1;
                col = 1;
            }
            else if (2 == sitesLength )
            {
                row = 1;
                col = 2;
            }
            else if (sitesLength >= 3 && sitesLength <= 4)
            {
                row = 2;
                col = 2;
            }
            else if (sitesLength >= 5 && sitesLength <= 6)
            {
                row = 2;
                col = 3;
            }
            else if (sitesLength >= 7 && sitesLength <= 9)
            {
                row = 3;
                col = 3;
            }
            else if (sitesLength > 9 && sitesLength <= 12)
            {
                row = 3;
                col = 4;
            }
            else
            {
                row = 4;
                col = 4;
            }

            double cellWidth = 0;
            double cellHeight = 0;
            double revisedTop = 0;
            double ratio = 1; // this.Width / Utils.MAIN_WINDOW_DESIGN_WIDTH;
            
            if (1 == sitesLength)
            {
                // don't set border for 1X1 layout
                _cellBorderWidth = 0;
            }
            else
            {
                _cellBorderWidth = CELL_BORDER_LENGTH;
            }
            cellWidth = (this.Width - _cellBorderWidth * (col + 1)) / col;
            cellHeight = cellWidth * 9.0 / 16.0;
            
            _cellBorderHeight = (this.Height - cellHeight * row) / (row + 1);

            if (row != col)
            {
                revisedTop = (this.Height - cellHeight * row) / 2;
                _cellBorderHeight = CELL_BORDER_LENGTH;
            }

            Dictionary<LayoutCellWindow, int> usedSites = new Dictionary<LayoutCellWindow, int>();
            for (var i=0; i<sitesLength; ++i)
            {
                var cell = _dictLayoutCellHandle[sites[i].window];
                cell.Owner = this;
                cell.WindowState = WindowState.Normal;
                cell.CellName = sites[i].name;
                cell.DeviceId = sites[i].device_id;
                if (WindowState.Minimized == VideoPeopleWindow.Instance.WindowState)
                {
                    cell.HideWindow();
                }
                else
                {
                    var currentWidth = cellWidth;
                    var currentHeight = cellHeight;
                    int posJ = i % col;
                    int posI = (i - posJ) / col;
                    var currentLeft = this.Left + currentWidth * posJ + _cellBorderWidth * (posJ + 1);
                    var currentTop = this.Top + revisedTop * ratio + currentHeight * posI + _cellBorderHeight * (posI + 1);
                    if (cell.SetProperWindowPos(currentLeft, currentTop, currentWidth, currentHeight))
                    {
                        log.InfoFormat("Set gallery cell position, name:{0}, left:{1}, top:{2}, width:{3}, height:{4}", cell.CellName, currentLeft, currentTop, currentWidth, currentHeight);
                    }
                    else
                    {
                        log.InfoFormat("The position of cell is not changed, cell name={0}", cell.CellName);
                    }
                }

                if (i == speakerIdx)
                {
                    SetSpeakerCellBorder(cell);
                }
                Utils.SetWindow2NoTopMost(cell.Handle);
                usedSites.Add(cell, 0);
            }

            if (speakerIdx < 0)
            {
                HideSpeakerCellBorder();
            }

            for (int i = 0; i < _layoutCells.Length; ++i)
            {
                var cell = _layoutCells[i];
                cell.MouseEnter -= LayoutCell_MouseEnter;
                cell.MouseLeave -= LayoutCell_MouseLeave;
                cell.Operationbar.MouseEnter -= LayoutCell_MouseEnter;
                cell.Operationbar.MouseLeave -= LayoutCell_MouseLeave;
                if (!usedSites.ContainsKey(cell))
                {
                    _layoutCells[i].HideWindow();
                }
            }

            _layoutNormalCells = new LayoutCellWindow[1];
            _layoutNormalCells[0] = _localVideoCell;
            if (!isDpiChanged)
            {
                ShowNormalCells();
            }
            
            //SetNormalCellBorder(_layoutNormalCells[0], 0, ratio);
            if (!HasActiveWindow())
            {
                _layoutNormalCells[0].ShowWindow();
            }

            if (_autoHidePartyName)
            {
                HideAllCellOperationbars();
            }
            else
            {
                UpdateLayoutOperationbarsPosition();
            }
            
            SetTopWindows();

            log.Info("SetLayout4GalleryMode end.");
        }

        private void HideAllCellOperationbars()
        {
            for (var i=0; i<_layoutCells.Length; ++i)
            {
                if (!_layoutCells[i].Operationbar.IsWindowHidden)
                {
                    _layoutCells[i].Operationbar.HideWindow();
                }
            }

            if (!_localVideoCell.Operationbar.IsMicMuted)
            {
                _localVideoCell.Operationbar.HideWindow();
            }
        }

        private void LayoutCell_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            _movingLayoutCell = sender as LayoutCellWindow;
            if (MouseButtonState.Pressed == e.LeftButton)
            {
                _movingLayoutCell.DragMove();
            }
            else
            {
                _movingLayoutCell = null;
            }
        }

        private void LayoutCell_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            _movingLayoutCell = null;
        }

        private void LayoutCell_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (!_isSpeakerMode)
            {
                return;
            }
            LayoutCellWindow cell = sender as LayoutCellWindow;
            if (_speakerModeMainCell == cell)
            {
                return;
            }

            for (int i=0; i< _dictLayoutCellHandle.Count; ++i)
            {
                var item = _dictLayoutCellHandle.ElementAt(i);
                if (item.Value == cell)
                {
                    _isFocusVideoMode = true;
                    // put _cancelFocusVideoWindow above _messageOverlayWindow to make sure to operate the _cancelFocusVideoWindow
                    _cancelFocusVideoWindow.Owner = _messageOverlayWindow; // this._speakerModeMainCell;
                    SetCancelFocusVideoWindow2ProperPos();
                    SvcSetLayout(ManagedEVSdk.Structs.EV_LAYOUT_PAGE_CLI.EV_LAYOUT_CURRENT_PAGE, _isSpeakerMode, item.Key);
                    break;
                }
            }
        }

        private void CancelFocusVideoWindow_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            _isFocusVideoMode = false;
            _cancelFocusVideoWindow.HideWindow();
            SvcSetLayout(ManagedEVSdk.Structs.EV_LAYOUT_PAGE_CLI.EV_LAYOUT_CURRENT_PAGE, _isSpeakerMode, IntPtr.Zero);
        }

        private void LayoutCell_OnWindowMoved(object sender, double x, double y)
        {
            LayoutCellWindow cell = sender as LayoutCellWindow;
            if (cell == _movingLayoutCell)
            {
                _normalCellsSection.MoveWindowPos(x, y);
            }

            if (!cell.Operationbar.IsWindowHidden)
            {
                cell.Operationbar.MoveWindowPos(x, y);
            }
        }

        private void LayoutCell_DpiChanged(object sender)
        {
            LayoutCellWindow currentCell = sender as LayoutCellWindow;
            if (currentCell == _movingLayoutCell)
            {
                int idx = 0;
                for (var i=0; i< _layoutNormalCells.Length; ++i)
                {
                    if (_layoutNormalCells[i] == currentCell)
                    {
                        idx = i;
                        break;
                    }
                }
                double ratio = GetNormalCellsSectionRatio();
                double left = currentCell.Left - CELL_BORDER_LENGTH * ratio;
                double top = currentCell.Top - (SMALL_VIDEO_WINDOW_HEIGHT + CELL_BORDER_LENGTH) * ratio * idx - (CELL_BORDER_LENGTH + NORMAL_CELLS_SECTION_BAR_HEIGHT) * ratio; //_normalCellsSection.Top + SMALL_VIDEO_WINDOW_HEIGHT * ratio * i + (i + 1) * 2 * ratio;
                _normalCellsSection.SetProperWindowPos(left, top);

                _normalCellsSection.ResetDecorationBorders();
                left = currentCell.Left;
                for (var i = 0; i < _layoutNormalCells.Length; ++i)
                {
                    LayoutCellWindow cell = _layoutNormalCells[i];
                    SetNormalCellBorder(cell, i);
                    if (cell == currentCell)
                    {
                        continue;
                    }

                    top = _normalCellsSection.Top + (SMALL_VIDEO_WINDOW_HEIGHT + CELL_BORDER_LENGTH) * ratio * i + (CELL_BORDER_LENGTH + NORMAL_CELLS_SECTION_BAR_HEIGHT) * ratio;
                    cell.SetProperWindowPos(left, top);
                }
            }
        }


        private void NormalCellsSection_OnWindowMoved(double x, double y)
        {
            if (null == _layoutNormalCells)
            {
                return;
            }

            for (int i = 0; i < _layoutNormalCells.Length; ++i)
            {
                var cell = _layoutNormalCells[i];
                if (_autoHidePartyName && !cell.Operationbar.IsWindowHidden && _localVideoCell != cell)
                {
                    cell.Operationbar.HideWindow();
                }
                if (_movingLayoutCell == cell)
                {
                    continue;
                }
                cell.MoveWindowPos(x, y);
            }
        }

        private void LayoutOperationbar_ShowNormalCellsChanged(object sender, EventArgs e)
        {
            _layoutOperationbar.ShowNormalCellsBtnVisibility = Visibility.Collapsed;
            _showNormalCellsSection = true;
            ShowNormalCells();
        }


        private void LayoutOperationbar_ConfManagementChanged(object sender, EventArgs e)
        {
            if (null == _confManagermentWindow)
            {
                _confManagermentWindow = new ConfManagementWindow();
            }
            //_confManagermentWindow.Owner = _normalCellsSection;
            //_confManagermentWindow.ShowDialog();
            //if (null != _confManagermentWindow)
            //{
            //    // maybe call ended and _confManagermentWindow is disposed.
            //    _confManagermentWindow.Owner = null;
            //}

            if (Visibility.Visible == _confManagermentWindow.Visibility)
            {
                _confManagermentWindow.Activate();
            }
            else
            {
                _confManagermentWindow.Show();
            }
        }

        private void NormalCellsSection_CustomWindowStateChanged(WindowState windowState)
        {
            if (WindowState.Minimized == windowState)
            {
                _normalCellsSection.HideWindow();

                for (var i=0; i< _layoutNormalCells.Length; ++i)
                {
                    _layoutNormalCells[i].HideWindow();
                }
                _layoutOperationbar.ShowNormalCellsBtnVisibility = Visibility.Visible;
                _showNormalCellsSection = false;
            }
        }

        private void NormalCellsSection_DpiChanged()
        {
            ShowNormalCells();
        }

        private void NormalCellsSection_CellsNavigateChanged(bool isUp)
        {
            //if (isUp)
            //{
            //    --_normalCellsStartIdx;
            //}
            //else
            //{
            //    ++_normalCellsStartIdx;
            //}

            ManagedEVSdk.Structs.EV_LAYOUT_PAGE_CLI page = isUp ? ManagedEVSdk.Structs.EV_LAYOUT_PAGE_CLI.EV_LAYOUT_PREV_PAGE
                                                                : ManagedEVSdk.Structs.EV_LAYOUT_PAGE_CLI.EV_LAYOUT_NEXT_PAGE;
            SvcSetLayout(page, _isSpeakerMode, IntPtr.Zero);
        }

        private void SetInitialPosOfNormalCellsSection()
        {
            int showCount = NORMAL_CELLS_SHOW_MAX_COUNT;
            double width = SMALL_VIDEO_WINDOW_WIDTH + CELL_BORDER_LENGTH * 2;
            double height = (NORMAL_CELLS_SECTION_BAR_HEIGHT + SMALL_VIDEO_WINDOW_HEIGHT * showCount + CELL_BORDER_LENGTH * (showCount + 1));
            
            double left = this.Left + this.Width - width * 1.5;
            double top = this.Top + (this.Height - height) / 4;
            height = NORMAL_CELLS_SECTION_BAR_HEIGHT + SMALL_VIDEO_WINDOW_HEIGHT + CELL_BORDER_LENGTH * 2;
            _normalCellsSection.SetProperWindowPos(left, top, width, height);
            _normalCellsSection.HideToolbar();
        }

        private double GetNormalCellsSectionRatio()
        {
            return _normalCellsSection.Width / (SMALL_VIDEO_WINDOW_WIDTH + CELL_BORDER_LENGTH * 2);
        }

        private void HideLayoutCell(LayoutCellWindow cell)
        {
            cell.MouseEnter -= LayoutCell_MouseEnter;
            cell.MouseLeave -= LayoutCell_MouseLeave;
            cell.Operationbar.MouseEnter -= LayoutCell_MouseEnter;
            cell.Operationbar.MouseLeave -= LayoutCell_MouseLeave;
            cell.Operationbar.HideWindow();
            cell.HideWindow();
        }

        private void ShowNormalCells()
        {
            if (null == _layoutNormalCells)
            {
                return;
            }
            log.InfoFormat("Show normal cells, _showNormalCellsSection: {0}", _showNormalCellsSection);

            // remove the useless evnets
            _localVideoCell.MouseLeftButtonDown -= LayoutCell_MouseLeftButtonDown;
            for (int i = 0; i < _layoutCells.Length; ++i)
            {
                _layoutCells[i].MouseLeftButtonDown -= LayoutCell_MouseLeftButtonDown;
            }

            if (!_showNormalCellsSection)
            {
                HideLayoutCell(_localVideoCell);

                if (_isSpeakerMode)
                {
                    for (int i = 1; i < _layoutCells.Length; ++i)
                    {
                        var cell = _layoutCells[i];
                        HideLayoutCell(cell);
                    }
                }

                return;
            }

            double ratio = GetNormalCellsSectionRatio();
            
            log.Info("Reset normal cells decoration borders.");
            _normalCellsSection.ResetDecorationBorders();
            
            //if (_normalCellsStartIdx >= _layoutNormalCells.Length)
            //{
            //    _normalCellsStartIdx = _layoutNormalCells.Length - 1;
            //}

            //if (_normalCellsStartIdx < 0)
            //{
            //    _normalCellsStartIdx = 0;
            //}

            //if (_normalCellsStartIdx > 0)
            //{
            //    _localVideoCell.HideWindow();
            //}

            //_normalCellsSection.CellsNavigateUpVisibility = _normalCellsStartIdx > 0 ? Visibility.Visible : Visibility.Collapsed;
            _showNormalCellsNavigateUp = _layoutNormalCells.Length > 1;
            int showCount = _layoutNormalCells.Length; // - _normalCellsStartIdx;
            double height = 0;
            if (showCount >= NORMAL_CELLS_SHOW_MAX_COUNT)
            {
                showCount = NORMAL_CELLS_SHOW_MAX_COUNT;
                _showNormalCellsNavigateDown = true;
            }
            else
            {
                _showNormalCellsNavigateDown = false;
            }
            
            double width = (SMALL_VIDEO_WINDOW_WIDTH + CELL_BORDER_LENGTH * 2) * ratio;
            height += (NORMAL_CELLS_SECTION_BAR_HEIGHT * 2 + SMALL_VIDEO_WINDOW_HEIGHT * showCount + CELL_BORDER_LENGTH * (showCount + 1)) * ratio;
            _normalCellsSection.SetProperWindowSize(width, height);
            _normalCellsSection.Owner = _layoutOperationbar;
            _normalCellsSection.ShowWindow();
            log.InfoFormat("_normalCellsSection position, left: {0}, top: {1}, ratio: {2}", _normalCellsSection.Left, _normalCellsSection.Top, ratio);

            if (!HasActiveWindow())
            {
                log.Info("Set normal cells section to top");
                Utils.SetWindow2Top(_normalCellsSection.Handle);
                SetSoftwareUpdateWindowTop();
            }

            Dictionary<LayoutCellWindow, int> usedSites = new Dictionary<LayoutCellWindow, int>();
            if (_isSpeakerMode)
            {
                usedSites.Add(_speakerModeMainCell, 0);
            }
            log.Info("Set normal cells to proper position.");
            for (int i=0; i<showCount; ++i)
            {
                LayoutCellWindow cell = _layoutNormalCells[_normalCellsStartIdx+i];
                SetNormalCellBorder(cell, i);
                cell.Owner = _normalCellsSection;
                cell.MouseEnter += LayoutCell_MouseEnter;
                cell.MouseLeave += LayoutCell_MouseLeave;
                cell.Operationbar.MouseEnter += LayoutCell_MouseEnter;
                cell.Operationbar.MouseLeave += LayoutCell_MouseLeave;
                width = SMALL_VIDEO_WINDOW_WIDTH * ratio;
                height = SMALL_VIDEO_WINDOW_HEIGHT * ratio;
                double left = _normalCellsSection.Left + CELL_BORDER_LENGTH * ratio;
                double top = _normalCellsSection.Top + (SMALL_VIDEO_WINDOW_HEIGHT + CELL_BORDER_LENGTH) * ratio * i + (CELL_BORDER_LENGTH + NORMAL_CELLS_SECTION_BAR_HEIGHT) * ratio; //_normalCellsSection.Top + SMALL_VIDEO_WINDOW_HEIGHT * ratio * i + (i + 1) * 2 * ratio;
                log.InfoFormat("Prepare to set cell position, name={0}, set to: left={1}, top={2}, current: left={3}, top={4}", cell.CellName, left, top, cell.Left, cell.Top);
                // force update the cell position in gallery mode for maybe the position of cell is not correct when change the position fo normal cell section and change window state from max to normal
                if (cell.SetProperWindowPos(left, top, width, height, !_isSpeakerMode))
                {
                    log.InfoFormat("Set normal cell position, name: {0}, left:{1}, top:{2}, width:{3}, height:{4}", cell.CellName, left, top, width, height);
                }
                else
                {
                    log.InfoFormat("The position of cell is not changed, cell name={0}", cell.CellName);
                }
                
                if (!_autoHidePartyName)
                {
                    ShowLayoutCellOperationbar(cell, true);
                }
                cell.MouseLeftButtonDown += LayoutCell_MouseLeftButtonDown;
                if (!HasActiveWindow())
                {
                    log.Info("Set normal cell to top");
                    Utils.SetWindow2Top(cell.Handle);
                }
                else
                {
                    log.Info("Set normal cell to non top");
                    Utils.SetWindow2NoTopMost(cell.Handle);
                }
                usedSites.Add(cell, 0);
            }
            log.Info("Hide not used cells for normal cells section.");
            if (_isSpeakerMode)
            {
                for (int i = 0; i < _layoutCells.Length; ++i)
                {
                    var cell = _layoutCells[i];
                    if (!usedSites.ContainsKey(cell))
                    {
                        HideLayoutCell(cell);
                    }
                }
            }

            if (_localVideoCell.Operationbar.IsMicMuted)
            {
                ShowLayoutCellOperationbar(_localVideoCell, true);
            }
            log.Info("Show normal cells end.");
        }
        
        private void SetLayout4SpeakerMode(bool isDpiChanged = false)
        {
            if (null == LayoutIndication)
            {
                return;
            }
            //for (int i = 0; i < _layoutCells.Length; ++i)
            //{
            //    _layoutCells[i].Hide();
            //}

            //if (sitesLength <= 0)
            //{
            //    return;
            //}
            int speakerIdx                          = _layoutSpeakerIdx;
            uint sitesLength                        = LayoutIndication.sites_size;
            ManagedEVSdk.Structs.EVSiteCli[] sites  = LayoutIndication.sites;
            log.Info("Set layout for speaker mode.");
            _speakerModeMainCell = _dictLayoutCellHandle[sites[0].window];
            _speakerModeMainCell.CellName = sites[0].name;
            _speakerModeMainCell.DeviceId = sites[0].device_id;
            _speakerModeMainCell.MouseEnter -= LayoutCell_MouseEnter;
            _speakerModeMainCell.MouseLeave -= LayoutCell_MouseLeave;
            _speakerModeMainCell.Operationbar.MouseEnter -= LayoutCell_MouseEnter;
            _speakerModeMainCell.Operationbar.MouseLeave -= LayoutCell_MouseLeave;
            if (0 == speakerIdx)
            {
                _currentSpeakerCell = _speakerModeMainCell;
            }
            
            _layoutNormalCells = new LayoutCellWindow[sitesLength];
            _layoutNormalCells[0] = _localVideoCell;
            for (var i = 1; i < sitesLength; ++i)
            {
                _layoutNormalCells[i] = _dictLayoutCellHandle[sites[i].window];
                _layoutNormalCells[i].CellName = sites[i].name;
                _layoutNormalCells[i].DeviceId = sites[i].device_id;
                _layoutNormalCells[i].BorderThickness = new Thickness(0);
                if (i == speakerIdx)
                {
                    _currentSpeakerCell = _layoutNormalCells[i];
                }
            }
            
            // don't set border for the main cell of speak mode 
            //_cellBorderWidth = CELL_BORDER_LENGTH;
            _cellBorderWidth = 0;
            double cellWidth = this.Width - _cellBorderWidth * 2;
            double cellHeight = cellWidth * 9.0 / 16.0;
            _cellBorderHeight = (this.Height - cellHeight) / 2.0;
            _speakerModeMainCell.Owner = this;
            if (WindowState.Minimized == VideoPeopleWindow.Instance.WindowState)
            {
                _speakerModeMainCell.HideWindow();
            }
            else
            {
                double left = this.Left + _cellBorderWidth;
                double top = this.Top + _cellBorderHeight;
                if (_speakerModeMainCell.SetProperWindowPos(left, top, cellWidth, cellHeight))
                {
                    log.InfoFormat("Set speaker main cell pos, name:{0}, left:{1}, top:{2}, width:{3}, height:{4}", _speakerModeMainCell.CellName, left, top, cellWidth, cellHeight);
                }
                else
                {
                    log.Info("The position of speaker main cell is not changed.");
                }
                if (!_autoHidePartyName)
                {
                    ShowLayoutCellOperationbar(_speakerModeMainCell, true);
                }
            }
            // currently do not show speaker border for main cell
            //if (0 == speakerIdx)
            //{
            //    SetSpeakerCellBorder(_speakerModeMainCell);
            //}
            //else if (speakerIdx < 0)
            //{
            //    HideSpeakerCellBorder();
            //}
            HideSpeakerCellBorder();
            
            if (!isDpiChanged)
            {
                ShowNormalCells();
            }
            
        }

        private void SetNormalCellBorder(LayoutCellWindow normalCell, int idx)
        {
            double sizeRatio = GetNormalCellsSectionRatio();
            Border border = null;
            if (normalCell == _localVideoCell)
            {
                border = _normalCellsSection.GetValidBorder();
                border.BorderBrush = _localVideoBorderBrush;
            }
            else if (normalCell == _currentSpeakerCell)
            {
                border = _normalCellsSection.GetValidBorder();
                border.BorderBrush = _speakerBorderBrush;
            }

            if (null != border)
            {
                border.Width = (SMALL_VIDEO_WINDOW_WIDTH + CELL_BORDER_LENGTH * 2) * sizeRatio;
                border.Height = (SMALL_VIDEO_WINDOW_HEIGHT + CELL_BORDER_LENGTH * 2) * sizeRatio;
                double top = NORMAL_CELLS_SECTION_BAR_HEIGHT * sizeRatio + (SMALL_VIDEO_WINDOW_HEIGHT + CELL_BORDER_LENGTH) * sizeRatio * idx;
                border.Margin = new Thickness(0, top, 0, 0);
                border.Visibility = Visibility.Visible;
            }
        }
        
        private void EVSdkWrapper_EventLayoutIndication(ManagedEVSdk.Structs.EVLayoutIndicationCli layoutIndication)
        {
            log.Info("EventLayoutIndication");
            LayoutIndication    = layoutIndication;
            _layoutSpeakerIdx   = layoutIndication.speaker_index;
            Application.Current.Dispatcher.InvokeAsync(() =>
            {
                log.InfoFormat("Layout indication, is speaker mode: {0}, sites length: {1}, setting_mode: {2}, mode: {3}", _isSpeakerMode, LayoutIndication.sites_size, layoutIndication.setting_mode, layoutIndication.mode);
                if (LayoutIndication.sites_size <= 0)
                {
                    log.Info("sites size is not larger than 0. EventLayoutIndication end");
                    return;
                }

                for (var i=0; i< LayoutIndication.sites_size; ++i)
                {
                    log.InfoFormat("Site device_id: {0}, name: {1}, mic_muted: {2}", LayoutIndication.sites[i].device_id, LayoutIndication.sites[i].name, LayoutIndication.sites[i].mic_muted);
                }

                _isSpeakerMode = ManagedEVSdk.Structs.EV_LAYOUT_MODE_CLI.EV_LAYOUT_GALLERY_MODE != layoutIndication.mode;
                _layoutOperationbar.IsSpeakerMode = _isSpeakerMode;
                _speakerBorder.Visibility = Visibility.Collapsed;

                if (_isSpeakerMode)
                {
                    SetLayout4SpeakerMode();
                }
                else
                {
                    SetLayout4GalleryMode();
                }

                OnDisableLayoutChanged(!LayoutIndication.mode_settable);
                log.InfoFormat("Handle Layout indication end, IsActive: {0}", this.IsActive);
            });
            log.Info("EventLayoutIndication end");
        }

        private void EVSdkWrapper_EventLayoutSpeakerIndication(ManagedEVSdk.Structs.EVLayoutSpeakerIndicationCli speakerIndication)
        {
            log.InfoFormat("EventLayoutSpeakerIndication, speaker idx: {0}, name: {1}", speakerIndication.speaker_index, speakerIndication.speaker_name);
            if (speakerIndication.speaker_index > LayoutIndication.sites_size)
            {
                log.Info("Can not update speaker for speaker index is larger than sites size. EventLayoutSpeakerIndication end");
                return;
            }

            Application.Current.Dispatcher.InvokeAsync(() =>
            {
                log.Info("Handle speaker indication begin.");
                _speakerBorder.Visibility = Visibility.Collapsed;
                if (_isSpeakerMode)
                {
                    _normalCellsSection.ResetDecorationBorders();
                    if (0 == speakerIndication.speaker_index)
                    {
                        _currentSpeakerCell = _speakerModeMainCell;
                        SetSpeakerCellBorder(_speakerModeMainCell);
                    }
                    else if (speakerIndication.speaker_index < 0)
                    {
                        HideSpeakerCellBorder();
                        _currentSpeakerCell = null;
                    }
                    else
                    {
                        _currentSpeakerCell = _dictLayoutCellHandle[LayoutIndication.sites[speakerIndication.speaker_index].window];
                    }

                    for (var i = 0; i < 4 && i < _layoutNormalCells.Length; ++i)
                    {
                        SetNormalCellBorder(_layoutNormalCells[i], i);
                    }

                    _layoutSpeakerIdx = speakerIndication.speaker_index;
                }
                else
                {
                    _layoutSpeakerIdx = speakerIndication.speaker_index;
                    if (_layoutSpeakerIdx < 0)
                    {
                        HideSpeakerCellBorder();
                    }
                    else
                    {
                        var speakerCell = _dictLayoutCellHandle[LayoutIndication.sites[_layoutSpeakerIdx].window];
                        SetSpeakerCellBorder(speakerCell);
                    }
                }
                log.Info("EventLayoutSpeakerIndication end.");
            });
        }
        
        private void SetSpeakerCellBorder( LayoutCellWindow speakerCell)
        {
            _speakerBorder.BorderThickness = new Thickness(_cellBorderWidth + 2, _cellBorderHeight + 2, _cellBorderWidth + 2, _cellBorderHeight + 2);
            _speakerBorder.Width = speakerCell.Width + _cellBorderWidth * 2;
            _speakerBorder.Height = speakerCell.Height + _cellBorderHeight * 2;
            var borderLeft = speakerCell.Left - this.Left - _cellBorderWidth;
            var borderTop = speakerCell.Top - this.Top - _cellBorderHeight;
            _speakerBorder.Margin = new Thickness(borderLeft, borderTop, 0, 0);
            _speakerBorder.Visibility = Visibility.Visible;
        }

        private void HideSpeakerCellBorder()
        {
            _speakerBorder.Visibility = Visibility.Collapsed;
        }

        private void OnTitleBarWindowMove(double x, double y)
        {
            if (Visibility.Visible != this.Visibility)
            {
                return;
            }

            this.Left += x;
            this.Top += y;

            _videoPeopleWindowRect = VideoPeopleWindow.Instance.GetWindowRect();
            double left = _videoPeopleWindowRect.Left + (_videoPeopleWindowRect.Width - this.Width) / 2;
            if (left != this.Left)
            {
                log.InfoFormat("The left is not fit to the main window and need to adjust it. Fixed left:{0}, current left:{1}", left, this.Left);
                this.Left = left;
            }

            double top = this.Top;
            switch (VideoPeopleWindow.Instance.WindowState)
            {
                case WindowState.Maximized:
                    if (VideoPeopleWindow.Instance.FullScreenStatus)
                    {
                        top = _videoPeopleWindowRect.Top;
                    }
                    else
                    {
                        top = _videoPeopleWindowRect.Top + VideoPeopleWindow.Instance.TitlebarHeight;
                    }

                    break;
                case WindowState.Normal:
                    top = _videoPeopleWindowRect.Top + VideoPeopleWindow.Instance.TitlebarHeight;
                    break;
                default:
                    break;
            }

            if (top != this.Top)
            {
                log.InfoFormat("The top is not fit to the main window and need to adjust it. Fixed top:{0}, current top:{1}", top, this.Top);
                this.Top = top;
            }

            if (_isSpeakerMode)
            {
                if (null != _speakerModeMainCell)
                {
                    if (_autoHidePartyName && !_speakerModeMainCell.Operationbar.IsWindowHidden)
                    {
                        _speakerModeMainCell.Operationbar.HideWindow();
                    }
                    _speakerModeMainCell.MoveWindowPos(x, y);
                }
                
            }
            else
            {
                for (int i = 0; i<_layoutCells.Length; ++i)
                {
                    if (_autoHidePartyName && !_layoutCells[i].Operationbar.IsWindowHidden)
                    {
                        _layoutCells[i].Operationbar.HideWindow();
                    }
                    _layoutCells[i].MoveWindowPos(x, y);
                }
            }

            bool isHidden = _layoutOperationbar.IsWindowHidden;
            _layoutOperationbar.HideWindow();
            if (!isHidden)
            {
                UpdateLayoutOperationbarsPosition();
            }
            //_layoutOperationbar.MoveWindowPos(x, y);
            //_normalCellsSection.MoveWindowPos(x, y);
            _cancelFocusVideoWindow.MoveWindowPos(x, y);
            _recordingIndicationWindow.MoveWindowPos(x, y);
            _messageOverlayWindow.MoveWindowPos(x, y);

            //ShowToolWindow();
            //HideShareToolWindow?.Invoke();
        }
        
        public void LayoutOperationbar_SvcLayoutModeChanged(bool isSpeakerMode, bool isDpiChanged=false)
        {
            if (_disableChangeLayout)
            {
                ShowPromptWindow(LanguageUtil.Instance.GetValueByKey("LAYOUT_CHANGE_DISABLED"), 5000);
                return;
            }

            //ChangeSvcLayoutMode(isSpeakerMode, isDpiChanged);
            if (_isSpeakerMode != isSpeakerMode)
            {
                SvcSetLayout(ManagedEVSdk.Structs.EV_LAYOUT_PAGE_CLI.EV_LAYOUT_CURRENT_PAGE, isSpeakerMode, IntPtr.Zero);
            }

            if (!_autoHidePartyName)
            {
                ShowAllLayoutCellOperationbars();
            }
        }

        private void ChangeSvcLayoutMode(bool isSpeakerMode, bool isDpiChanged)
        {
            _layoutOperationbar.IsSpeakerMode = isSpeakerMode;
            if (LayoutIndication.sites_size <= 0)
            {
                for (var i = 0; i < _layoutCells.Length; ++i)
                {
                    _layoutCells[i].HideWindow();
                }
                return;
            }
            if (_isSpeakerMode != isSpeakerMode)
            {
                _layoutSpeakerIdx = -1;
                SvcSetLayout(ManagedEVSdk.Structs.EV_LAYOUT_PAGE_CLI.EV_LAYOUT_CURRENT_PAGE, isSpeakerMode, IntPtr.Zero);
            }
            _isSpeakerMode = isSpeakerMode;
            _speakerBorder.Visibility = Visibility.Collapsed;
            if (_isSpeakerMode)
            {
                SetLayout4SpeakerMode(isDpiChanged);
            }
            else
            {
                SetLayout4GalleryMode(isDpiChanged);
            }
        }

        private void SvcSetLayout(ManagedEVSdk.Structs.EV_LAYOUT_PAGE_CLI page, bool isSpeakerMode, IntPtr focusVideoWindow)
        {
            ManagedEVSdk.Structs.EVLayoutRequestCli layoutRequest = new ManagedEVSdk.Structs.EVLayoutRequestCli();
            if (isSpeakerMode)
            {
                layoutRequest.mode = ManagedEVSdk.Structs.EV_LAYOUT_MODE_CLI.EV_LAYOUT_SPEAKER_MODE;
                layoutRequest.max_type = ManagedEVSdk.Structs.EV_LAYOUT_TYPE_CLI.EV_LAYOUT_TYPE_5_4T_1B;
            }
            else
            {
                layoutRequest.mode = ManagedEVSdk.Structs.EV_LAYOUT_MODE_CLI.EV_LAYOUT_GALLERY_MODE;
                layoutRequest.max_type = Utils.GetEnable4x4Layout() ? ManagedEVSdk.Structs.EV_LAYOUT_TYPE_CLI.EV_LAYOUT_TYPE_16 : ManagedEVSdk.Structs.EV_LAYOUT_TYPE_CLI.EV_LAYOUT_TYPE_9;
            }
            layoutRequest.page = page;
            layoutRequest.max_resolution = new ManagedEVSdk.Structs.EVVideoSizeCli() { width = 0, height = 0 };
            layoutRequest.windows_size = IntPtr.Zero == focusVideoWindow ? 0u : 1u;
            layoutRequest.windows = new IntPtr[layoutRequest.windows_size];
            if (layoutRequest.windows_size > 0)
            {
                layoutRequest.windows[0] = focusVideoWindow;
            }
            for (int i = 1; i < layoutRequest.windows_size; ++i)
            {
                layoutRequest.windows[i] = IntPtr.Zero;
            }

            EVSdkManager.Instance.SetLayout(layoutRequest);
        }

        private void EVSdkWrapper_EventMessageOverlay(ManagedEVSdk.Structs.EVMessageOverlayCli messageOverlay)
        {
            log.Info("EventMessageOverlay");
            if (null == messageOverlay)
            {
                log.Info("Received message overlay, but the object is null. EventMessageOverlay end");
                return;
            }
            
            log.InfoFormat("Received message overlay. on:{0}", messageOverlay.enable);
            SetMessageOverlay(messageOverlay);
            log.Info("EventMessageOverlay end");
        }

        private void EVSdkWrapper_EventLayoutSiteIndication(ManagedEVSdk.Structs.EVSiteCli site)
        {
            log.Info("EventLayoutSiteIndication");
            if (site.is_local)
            {
                if (IsRemoteMuted != site.remote_muted)
                {
                    OnPartyRemoteMutedChanged(site.remote_muted);
                    IsRemoteMuted = site.remote_muted;
                }
                log.Info("Local site. EventLayoutSiteIndication end");
                return;
            }

            for (var i=0; i<LayoutIndication.sites_size; ++i)
            {
                if (LayoutIndication.sites[i].device_id == site.device_id)
                {
                    if (LayoutIndication.sites[i].mic_muted != site.mic_muted)
                    {
                        OnPartyMicMutedChanged(site.device_id, site.mic_muted);
                    }
                    LayoutIndication.sites[i] = site;
                    break;
                }
            }
            log.Info("EventLayoutSiteIndication end");
        }
        
        private void SetMessageOverlay(ManagedEVSdk.Structs.EVMessageOverlayCli messageOverlay)
        {
            Application.Current.Dispatcher.InvokeAsync(() => {
                log.Info("Handle message overlay begin");
                if (messageOverlay.enable)
                {
                    if (string.IsNullOrEmpty(messageOverlay.content))
                    {
                        log.Info("Message overlay is enabled, but content is empty.");
                        _messageOverlayWindow.ContentText = "";
                        _messageOverlayWindow.HideWindow();
                        _timerMessageOverlayRoll.Enabled = false;
                    }
                    else
                    {
                        _messageOverlayWindow.ContentText = messageOverlay.content;
                        _messageOverlayWindow.SetFontSize(messageOverlay.fontSize);
                        _messageOverlayWindow.SetBackground(messageOverlay.backgroundColor);
                        _messageOverlayWindow.SetForeground(messageOverlay.foregroundColor);
                        _messageOverlayWindow.SetTransparency(messageOverlay.transparency);
                        _messageOverlayVerticalBorder = messageOverlay.verticalBorder;
                        //_messageOverlayWindow.Owner = _titlebar;
                        SetMessageOverlayWindow2ProperPos();
                        if (!HasActiveWindow())
                        {
                            log.Info("Set messageOverlayWindow to top");
                            Utils.SetWindow2Top(_messageOverlayWindow.Handle);
                        }
                        if (messageOverlay.displaySpeed > 0)
                        {
                            log.Info("Begin to roll message overlay.");
                            _messageOverlayWindow.ResetRollContentInitialPos();
                            _messageOverlayWindow.SetRollSpeed(messageOverlay.displaySpeed);
                            _messageOverlayDisplayRepetitions = messageOverlay.displayRepetitions;
                            _messageOverlayDisplayRolledCount = 0;
                            _timerMessageOverlayRoll.Enabled = true;
                            _timerMessageOverlayRoll.AutoReset = true;
                        }
                        else
                        {
                            _messageOverlayWindow.SetContentTextBlock2DefaultPos();
                            _timerMessageOverlayRoll.Enabled = false;
                        }
                    }
                }
                else
                {
                    _timerMessageOverlayRoll.Enabled = false;
                    _messageOverlayWindow.ContentText = "";
                    _messageOverlayWindow.HideWindow();
                }
                log.Info("Handle message overlay end");
            });
        }

        private void SetMessageOverlayWindow2ProperPos()
        {
            if (WindowState.Minimized == VideoPeopleWindow.Instance.WindowState)
            {
                return;
            }

            double left = this.Left;
            double width = this.Width;
            double height = _messageOverlayWindow.Height; // _messageOverlayWindow.InitialHeight * ratio;
            double top = this.Top + (this.Height - height) * ((100.0 - _messageOverlayVerticalBorder) / 100.0);

            _messageOverlayWindow.SetProperWindowPos(left, top, width, height);
        }
        
        private void TimerMessageOverlayRoll_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            Application.Current.Dispatcher.InvokeAsync(() => {
                if (!_messageOverlayWindow.RollContent())
                {
                    ++_messageOverlayDisplayRolledCount;
                    if (_messageOverlayDisplayRepetitions > 0 && _messageOverlayDisplayRolledCount >= _messageOverlayDisplayRepetitions)
                    {
                        log.Info("Message overlay rolled finished.");
                        _messageOverlayWindow.ContentText = "";
                        _timerMessageOverlayRoll.Enabled = false;
                        _messageOverlayWindow.HideWindow();
                        return;
                    }

                    _messageOverlayWindow.ResetRollContentInitialPos();
                }
            });
        }

        private void LayoutOperationbar_EventInitScreen()
        {
            Application.Current.Dispatcher.InvokeAsync(() =>
            {
                ShowPromptWindow(LanguageUtil.Instance.GetValueByKey("INIT_SCREEN"), 3000);
            });
        }

        private void LayoutOperationbar_EventRequestSpeaker()
        {
            Application.Current.Dispatcher.InvokeAsync(() => {
                ShowPromptWindow(LanguageUtil.Instance.GetValueByKey("SENDING_REQUEST_SPEAK"), 3000);
            });
        }

        private void ShowPromptWindow(string promptContent, int duration)
        {
            log.Info("ShowPromptWindow");
            if (WindowState.Minimized == VideoPeopleWindow.Instance.WindowState)
            {
                log.Info("Do not show prompt window for video people window is minimized.");
                return;
            }

            if (CallStatus.Connected != CallController.Instance.CurrentCallStatus)
            {
                log.Info("Do not show prompt window for call status is not connected.");
                return;
            }

            PromptWindow promptWindow = PromptWindow.Instance;

            //SaveIndependenetActiveWindow();
            //promptWindow.Owner = this;
            //promptWindow.ShowPromptByTime(promptContent, duration, VideoPeopleWindow.Instance);
            //ResumeIndependenetActiveWindow();
            //promptWindow.Owner = _layoutOperationbar;

            promptWindow.CloseWindow();
            IMasterDisplayWindow masterWin = null;
            Window activeWin = null;
            if (_layoutOperationbar.IsReceiveContentWinActive)
            {
                var win = _layoutOperationbar.GetReceiveContentWindow();
                masterWin = win;
                activeWin = win;
                promptWindow.Owner = win;
                log.Info("ReceiveContentWindow is activated.");
            }
            else if (_layoutOperationbar.IsWhiteboardActive)
            {
                var win = _layoutOperationbar.GetContentWhiteboardWindow();
                masterWin = win;
                activeWin = win;
                promptWindow.Owner = win;
                log.Info("ContentWhiteboardWindow is activated.");
            }
            else if (null != MediaStatisticsView.Instance && MediaStatisticsView.Instance.IsActive)
            {
                masterWin = VideoPeopleWindow.Instance;
                activeWin = MediaStatisticsView.Instance;
                promptWindow.Owner = activeWin;
                log.Info("MediaStatisticsView is activated.");
            }
            else if (null != _confManagermentWindow && _confManagermentWindow.IsActive)
            {
                masterWin = _confManagermentWindow;
                activeWin = _confManagermentWindow;
                promptWindow.Owner = _confManagermentWindow;
                log.Info("_confManagermentWindow is activated.");
            }
            else
            {
                log.InfoFormat("VideoPeopleWindow as owner of prompt window. this.IsActive: {0}", this.IsActive);
                masterWin = VideoPeopleWindow.Instance;
                promptWindow.Owner = _layoutOperationbar;
                if (VideoPeopleWindow.Instance.IsActive || this.IsActive)
                {
                    log.Info("VideoPeopleWindow is activated.");
                    activeWin = VideoPeopleWindow.Instance;
                }
            }

            promptWindow.ShowPromptByTime(promptContent, duration, masterWin, activeWin);
        }

        private void OnPartyRemoteMutedChanged(bool isMuted)
        {
            Application.Current.Dispatcher.InvokeAsync(() => {
                log.Info("Handle party retmote muted changed begin");
                if (isMuted)
                {
                    _layoutOperationbar.RequestSpeakVisibility = Visibility.Visible;
                    ShowPromptWindow(LanguageUtil.Instance.GetValueByKey("CHAIRMAN_MUTE_ENDPOINT_INFO"), 7000);
                }
                else
                {
                    _layoutOperationbar.RequestSpeakVisibility = Visibility.Collapsed;
                    ShowPromptWindow(LanguageUtil.Instance.GetValueByKey("CHAIRMAN_UNMUTE_ENDPOINT_INFO"), 5000);
                }
                log.Info("Handle party retmote muted changed end");
            });
        }

        private void OnPartyMicMutedChanged(ulong deviceId, bool isMicMuted)
        {
            if (deviceId == LoginManager.Instance.DeviceId)
            {
                return;
            }
            log.Info("OnPartyMicMutedChanged");
            Application.Current.Dispatcher.InvokeAsync(() => {
                log.InfoFormat("Begin to handle mic muted changed, login device id:{0}", LoginManager.Instance.DeviceId);
                for (var j = 0; j < _layoutCells.Length; ++j)
                {
                    if (_layoutCells[j].DeviceId == deviceId)
                    {
                        _layoutCells[j].Operationbar.IsMicMuted = isMicMuted;
                        break;
                    }
                }

                log.InfoFormat("Handled mic muted, party device id:{0}, isMicMuted:{1}", deviceId, isMicMuted);
            });
        }
        
        private void OnDisableLayoutChanged(bool disable)
        {
            _disableChangeLayout = disable;
            if (disable)
            {
                if (_isFocusVideoMode)
                {
                    log.Info("Handle party disable changed begin");
                    _isFocusVideoMode = false;
                    _cancelFocusVideoWindow.HideWindow();
                    log.Info("Handle party disable changed end");
                }

                if (!_isSpeakerMode)
                {
                    _isSpeakerMode = true;
                    _layoutOperationbar.IsSpeakerMode = true;
                }
            }
        }

        private void EVSdkWrapper_EventWarn(ManagedEVSdk.Structs.EVWarnCli warn)
        {
            log.InfoFormat("EventWarn, code:{0}, msg:{1}", warn.code, warn.msg);
            if (ManagedEVSdk.Structs.EV_WARN_CLI.EV_WARN_NO_AUDIO_CAPTURE_CARD != warn.code && _disablePrompt)
            {
                return;
            }

            Application.Current.Dispatcher.InvokeAsync(() =>
            {
                if (ManagedEVSdk.Structs.EV_WARN_CLI.EV_WARN_NO_AUDIO_CAPTURE_CARD == warn.code)
                {
                    if (CallStatus.Connected != CallController.Instance.CurrentCallStatus)
                    {
                        log.Info("Do not show no mic window for call status is not connected.");
                        return;
                    }

                    if (null == _modalPromptDlg)
                    {
                        _modalPromptDlg = new ModalPromptDlg();
                    }
                    if (Visibility.Visible == _modalPromptDlg.Visibility)
                    {
                        return;
                    }
                    _modalPromptDlg.Owner = _layoutOperationbar;
                    _modalPromptDlg.ShowDialog(
                        VideoPeopleWindow.Instance
                        , LanguageUtil.Instance.GetValueByKey("PROMPT")
                        , LanguageUtil.Instance.GetValueByKey("NO_MICROPHONE")
                        , LanguageUtil.Instance.GetValueByKey("GOT_IT")
                    );
                    if (null != _modalPromptDlg)
                    {
                        _modalPromptDlg.Owner = null;
                    }
                }
                else
                {
                    string prompt = null;
                    switch (warn.code)
                    {
                        case ManagedEVSdk.Structs.EV_WARN_CLI.EV_WARN_NETWORK_POOR:
                            prompt = LanguageUtil.Instance.GetValueByKey("NETWORK_POOR");
                            break;
                        case ManagedEVSdk.Structs.EV_WARN_CLI.EV_WARN_NETWORK_VERY_POOR:
                            prompt = LanguageUtil.Instance.GetValueByKey("NETWORK_VERY_POOR");
                            break;
                        case ManagedEVSdk.Structs.EV_WARN_CLI.EV_WARN_BANDWIDTH_INSUFFICIENT:
                            prompt = LanguageUtil.Instance.GetValueByKey("BANDWIDTH_INSUFFICIENT");
                            break;
                        case ManagedEVSdk.Structs.EV_WARN_CLI.EV_WARN_BANDWIDTH_VERY_INSUFFICIENT:
                            prompt = LanguageUtil.Instance.GetValueByKey("BANDWIDTH_VERY_INSUFFICIENT");
                            break;
                    }

                    if (string.IsNullOrEmpty(prompt))
                    {
                        log.Info("Empty prompt. EventWarn end");
                        return;
                    }

                    ShowPromptWindow(prompt, 10000);
                }
            });
            log.Info("EventWarn end");
        }
        
        private void VideoPeopleWindow_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if ("FullScreenStatus" == e.PropertyName)
            {
                if (!VideoPeopleWindow.Instance.FullScreenStatus)
                {
                    _titlebar.HideWindow();
                }
            }
        }

        private void EVSdkWrapper_EventRecordingIndication(ManagedEVSdk.Structs.EVRecordingInfoCli recordingInfo)
        {
            log.InfoFormat("EventRecordingIndication, state={0}, is live={1}", recordingInfo.state, recordingInfo.live);
            _recordingIndication = recordingInfo;
            if (ManagedEVSdk.Structs.EV_RECORDING_STATE_CLI.EV_RECORDING_STATE_ON == recordingInfo.state)
            {
                Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    _recordingIndicationWindow.TypeTitle = recordingInfo.live ? "LIVE" : "REC";
                    SetRecordingIndicationWindow2ProperPos();
                    log.Info("Show recording flag.");
                });
            }
            else
            {
                Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    _recordingIndicationWindow.HideWindow();
                    SetCancelFocusVideoWindow2ProperPos();
                    log.Info("Hide recording flag.");
                });
            }
            log.Info("EventRecordingIndication end");
        }

        private void SetRecordingIndicationWindow2ProperPos()
        {
            if (WindowState.Minimized == VideoPeopleWindow.Instance.WindowState)
            {
                return;
            }

            if (ManagedEVSdk.Structs.EV_RECORDING_STATE_CLI.EV_RECORDING_STATE_ON != _recordingIndication.state)
            {
                return;
            }

            _recordingIndicationWindow.SetProperWindowPos(this.Left + CANCEL_FOCUS_VIDEO_WINDOW_LEFT, this.Top + CANCEL_FOCUS_VIDEO_WINDOW_TOP);
            SetCancelFocusVideoWindow2ProperPos();
        }

        private void SetCancelFocusVideoWindow2ProperPos()
        {
            if (!_isFocusVideoMode)
            {
                return;
            }

            if (_recordingIndicationWindow.IsWindowHidden)
            {
                _cancelFocusVideoWindow.SetProperWindowPos(this.Left + CANCEL_FOCUS_VIDEO_WINDOW_LEFT, this.Top + CANCEL_FOCUS_VIDEO_WINDOW_TOP);
            }
            else
            {
                double width = _recordingIndicationWindow.Width;
                if (_recordingIndicationWindow.ActualWidth > width)
                {
                    width = _recordingIndicationWindow.ActualWidth;
                }
                double left = _recordingIndicationWindow.Left + width + 14;
                _cancelFocusVideoWindow.SetProperWindowPos(left, this.Top + CANCEL_FOCUS_VIDEO_WINDOW_TOP);
            }
        }

        private void SetProperPosition4LayoutOperationbarAndTitlebar()
        {
            Rect mainWindowRect = VideoPeopleWindow.Instance.GetWindowRect();

            // if the following values for layout operation bar don't be int, the position will be not correct when change main window between min/restore operation in max window style.
            var width = Math.Round(mainWindowRect.Width);
            var height = LAYOUT_OPERATIONBAR_HEIGHT;
            var left = Math.Round(mainWindowRect.Left);
            double top = 0;
            if (WindowState.Maximized == VideoPeopleWindow.Instance.WindowState)
            {
                // move the operation bar to the bottom of window when maximized
                if (VideoPeopleWindow.Instance.ActualHeight > VideoPeopleWindow.Instance.Height)
                {
                    top = Math.Round(VideoPeopleWindow.Instance.ActualHeight - height);
                }
                else
                {
                    top = Math.Round(VideoPeopleWindow.Instance.Height - height);
                }
                // adjust the top value when the secondary screen is on the top of primary one.
                top += mainWindowRect.Top;
            }
            else
            {
                top = Math.Round(this.Top + this.Height - height);
            }
            if (_layoutOperationbar.SetProperWindowPos(left, top, width, height))
            {
                log.InfoFormat("Set _layoutOperationbar position, left:{0}, top:{1}, width:{2}, height:{3}", left, top, width, height);
            }
            if (!_autoHidePartyName)
            {
                if (_isSpeakerMode)
                {
                    ShowLayoutCellOperationbar(_speakerModeMainCell, true);
                }
                else
                {
                    ShowAllLayoutCellOperationbars();
                }
            }
            if (VideoPeopleWindow.Instance.FullScreenStatus)
            {
                if (_titlebar.SetProperWindowPos(left, mainWindowRect.Top, width, VideoPeopleWindow.Instance.TitlebarHeight))
                {
                    log.InfoFormat("Set _titlebar position, left:{0}, top:{1}, width:{2}, height:{3}", left, mainWindowRect.Top, width, VideoPeopleWindow.Instance.TitlebarHeight);
                }
            }
        }

        private void ShowAllLayoutCellOperationbars()
        {
            for (var i=0; i<_layoutCells.Length; ++i)
            {
                if (!_layoutCells[i].IsWindowHidden)
                {
                    ShowLayoutCellOperationbar(_layoutCells[i], true);
                }
                else
                {
                    if (!_layoutCells[i].Operationbar.IsWindowHidden)
                    {
                        _layoutCells[i].Operationbar.HideWindow();
                    }
                }
            }
        }

        private void UpdateLayoutOperationbarsPosition()
        {
            if (!_autoHidePartyName)
            {
                if (_isSpeakerMode)
                {
                    ShowLayoutCellOperationbar(_speakerModeMainCell, true);
                }
                else
                {
                    ShowAllLayoutCellOperationbars();
                }
            }

        }

        private void LayoutCell_MouseLeave(object sender, MouseEventArgs e)
        {
            _normalCellsSection.HideToolbar();
        }

        private void LayoutCell_MouseEnter(object sender, MouseEventArgs e)
        {
            _normalCellsSection.ShowToolbar(_showNormalCellsNavigateUp, _showNormalCellsNavigateDown);
        }

        private void NormalCellsSection_MouseLeave(object sender, MouseEventArgs e)
        {
            _normalCellsSection.HideToolbar();
        }

        private void NormalCellsSection_MouseEnter(object sender, MouseEventArgs e)
        {
            _normalCellsSection.ShowToolbar(_showNormalCellsNavigateUp, _showNormalCellsNavigateDown);
        }

        private void LocalVideoCell_MouseLeave(object sender, MouseEventArgs e)
        {
            _normalCellsSection.HideToolbar();
        }

        private void LocalVideoCell_MouseEnter(object sender, MouseEventArgs e)
        {
            _normalCellsSection.ShowToolbar(_showNormalCellsNavigateUp, _showNormalCellsNavigateDown);
        }

        private IntPtr DragHook(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            // Intercept the WM_DEVICECHANGE message
            if (msg == DbtUtil.WM_DEVICECHANGE)
            {
                // Get the message event type
                int nEventType = wParam.ToInt32();

                // Check for devices being disconnected
                if (nEventType == DbtUtil.DBT_DEVICEREMOVECOMPLETE)
                {
                    DbtUtil.DEV_BROADCAST_HDR hdr = new DbtUtil.DEV_BROADCAST_HDR();

                    // Convert lparam to DEV_BROADCAST_HDR structure
                    Marshal.PtrToStructure(lParam, hdr);

                    if (hdr.dbch_devicetype == DbtUtil.DBT_DEVTYP_DEVICEINTERFACE)
                    {
                        log.Info("Audio device is removed.");
                        // call the function in sdk to avoid the call is terminated when usb mic is removed.
                        StartUpdateAudioDevicesTimer();
                    }
                }
            }
            return IntPtr.Zero;
        }
        
        private void RegisterNotification(Guid guid)
        {
            DbtUtil.DEV_BROADCAST_DEVICEINTERFACE devIF = new DbtUtil.DEV_BROADCAST_DEVICEINTERFACE();
            IntPtr devIFBuffer;

            // Set to HID GUID
            devIF.dbcc_size = Marshal.SizeOf(devIF);
            devIF.dbcc_devicetype = DbtUtil.DBT_DEVTYP_DEVICEINTERFACE;
            devIF.dbcc_reserved = 0;
            devIF.dbcc_classguid = guid;

            // Allocate a buffer for DLL call
            devIFBuffer = Marshal.AllocHGlobal(devIF.dbcc_size);

            // Copy devIF to buffer
            Marshal.StructureToPtr(devIF, devIFBuffer, true);

            // Register for HID device notifications
            m_hNotifyDevNode = DbtUtil.RegisterDeviceNotification((new WindowInteropHelper(this)).Handle, devIFBuffer, DbtUtil.DEVICE_NOTIFY_WINDOW_HANDLE);

            // Copy buffer to devIF
            Marshal.PtrToStructure(devIFBuffer, devIF);

            // Free buffer
            Marshal.FreeHGlobal(devIFBuffer);
        }

        // Unregister device notification
        private void UnregisterNotification()
        {
            if (IntPtr.Zero == m_hNotifyDevNode)
            {
                return;
            }
            log.Info("Unregister notification");
            DbtUtil.UnregisterDeviceNotification(m_hNotifyDevNode);
            m_hNotifyDevNode = IntPtr.Zero;
        }

        private void StartUpdateAudioDevicesTimer()
        {
            if (null == _updateAudioDevicesTimer)
            {
                _updateAudioDevicesTimer = new System.Timers.Timer();
                _updateAudioDevicesTimer.Interval = 2 * 1000;
                _updateAudioDevicesTimer.AutoReset = false;
                _updateAudioDevicesTimer.Elapsed += UpdateAudioDevicesTimer_Elapsed;
            }

            _updateAudioDevicesTimer.Start();
        }

        private void UpdateAudioDevicesTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            log.Info("Update mic and speaker.");
            ManagedEVSdk.Structs.EVDeviceCli currentSpeaker     = EVSdkManager.Instance.GetDevice(ManagedEVSdk.Structs.EV_DEVICE_TYPE_CLI.EV_DEVICE_AUDIO_PLAYBACK);
            ManagedEVSdk.Structs.EVDeviceCli currentMic         = EVSdkManager.Instance.GetDevice(ManagedEVSdk.Structs.EV_DEVICE_TYPE_CLI.EV_DEVICE_AUDIO_CAPTURE);
            ManagedEVSdk.Structs.EVDeviceCli[] speakers         = EVSdkManager.Instance.GetDevices(ManagedEVSdk.Structs.EV_DEVICE_TYPE_CLI.EV_DEVICE_AUDIO_PLAYBACK);
            ManagedEVSdk.Structs.EVDeviceCli[] mics             = EVSdkManager.Instance.GetDevices(ManagedEVSdk.Structs.EV_DEVICE_TYPE_CLI.EV_DEVICE_AUDIO_CAPTURE);
            if (null == currentSpeaker || string.IsNullOrEmpty(Utils.Utf8Byte2DefaultStr(currentSpeaker.name)))
            {
                log.Info("Invalid current speaker.");
                if (null != speakers && speakers.Length > 0)
                {
                    log.InfoFormat("Set the speaker to the first in list, id:{0}, name:{1}", speakers[0].id, speakers[0].name);
                    EVSdkManager.Instance.SetDevice(ManagedEVSdk.Structs.EV_DEVICE_TYPE_CLI.EV_DEVICE_AUDIO_PLAYBACK, speakers[0].id);
                }
                else
                {
                    log.Info("Invalid speaker list and do not set current speaker.");
                }
            }
            else
            {
                bool found = false;
                for (var i = 0; i < speakers.Length; ++i)
                {
                    if (currentSpeaker.id == speakers[i].id)
                    {
                        found = true;
                        break;
                    }
                }
                if (!found)
                {
                    if (null != speakers && speakers.Length > 0)
                    {
                        log.InfoFormat("Current speaker is not found. Set the speaker to the first in list, id:{0}, name:{1}", speakers[0].id, speakers[0].name);
                        EVSdkManager.Instance.SetDevice(ManagedEVSdk.Structs.EV_DEVICE_TYPE_CLI.EV_DEVICE_AUDIO_PLAYBACK, speakers[0].id);
                    }
                }
            }
            if (null == currentMic || string.IsNullOrEmpty(Utils.Utf8Byte2DefaultStr(currentMic.name)))
            {
                log.Info("Invalid current mic.");
                if (null != mics && mics.Length > 0)
                {
                    log.InfoFormat("Set the mic to the first in list, id:{0}, name:{1}", mics[0].id, mics[0].name);
                    EVSdkManager.Instance.SetDevice(ManagedEVSdk.Structs.EV_DEVICE_TYPE_CLI.EV_DEVICE_AUDIO_CAPTURE, mics[0].id);
                }
                else
                {
                    log.Info("Invalid mic list and do not set current mic.");
                }
            }
            else
            {
                bool found = false;
                for (var i = 0; i < mics.Length; ++i)
                {
                    if (currentMic.id == mics[i].id)
                    {
                        found = true;
                        break;
                    }
                }
                if (!found)
                {
                    if (null != mics && mics.Length > 0)
                    {
                        log.InfoFormat("Current mic is not found. Set the mic to the first in list, id:{0}, name:{1}", mics[0].id, mics[0].name);
                        EVSdkManager.Instance.SetDevice(ManagedEVSdk.Structs.EV_DEVICE_TYPE_CLI.EV_DEVICE_AUDIO_CAPTURE, mics[0].id);
                    }
                }
            }
        }

        private void LayoutOperationbar_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if ("IsAudioMuted" == e.PropertyName)
            {
                Application.Current.Dispatcher.InvokeAsync(() => {
                    _localVideoCell.Operationbar.IsMicMuted = _layoutOperationbar.IsAudioMuted;
                    if (_localVideoCell.Operationbar.IsMicMuted)
                    {
                        _localVideoCell.Operationbar.ShowWindow();
                    }
                });
            }
        }

        private void LayoutBackgroundWindow_Deactivated(object sender, EventArgs e)
        {
            log.InfoFormat("LayoutBackgroundWindow_Deactivated, IsActive: {0}", this.IsActive);
        }

        private void LayoutBackgroundWindow_Activated(object sender, EventArgs e)
        {
            log.InfoFormat("LayoutBackgroundWindow_Activated, IsActive: {0}", this.IsActive);
        }

        public Window GetWindow()
        {
            return this;
        }

        public IntPtr GetHandle()
        {
            if (IntPtr.Zero == _handle)
            {
                _handle = new WindowInteropHelper(this).Handle;
            }
            return _handle;
        }

        public Rect GetWindowRect()
        {
            return new Rect(this.Left, this.Top, this.Width, this.Height);
        }

        public double GetInitialWidth()
        {
            return this.Width;
        }

        public double GetInitialHeight()
        {
            return this.Height;
        }

        public double GetWidth()
        {
            return this.Width;
        }

        public double GetHeight()
        {
            return this.Height;
        }

        public double GetLeft()
        {
            return this.Left;
        }

        public double GetTop()
        {
            return this.Top;
        }

        public double GetSizeRatio()
        {
            return 1.0;
        }

        #endregion
    }
}
