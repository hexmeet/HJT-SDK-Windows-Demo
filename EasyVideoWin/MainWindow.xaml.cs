using System;
using System.Windows;
using System.Windows.Input;
using EasyVideoWin.View;
using EasyVideoWin.ViewModel;
using EasyVideoWin.Helpers;
using System.Runtime.InteropServices;
using System.Diagnostics;
using EasyVideoWin.Model;
using log4net;
using System.Threading;
using EasyVideoWin.CustomControls;

namespace EasyVideoWin
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : FullScreenBaseWindow
    {
        private readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private const double INITIAL_WIDTH = 720d;
        private const double INITIAL_HEIGHT = 520d;

        [DllImport("kernel32.dll")]
        static extern void ExitProcess(uint uExitCode);

        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool TerminateProcess(IntPtr hProcess, uint uExitCode);

        private LoginWindow _loginWindow;
        private bool _notLogin = true;
        
        private MainWindowViewModel _viewModel;
        
        private IncomingCallPromptDialog _incomingCallPromptDialog;
        private System.Timers.Timer _incomingCallPromptTimer = null;
        private bool _flashWindowStarted = false;
        private MessageBoxTip _messageBoxTip = null;
        private string _messageTipExtraInfo = null;

        public MainWindow():base(INITIAL_WIDTH, INITIAL_HEIGHT)
        {
            log.Info("MainWindow begin to construct");
            InitializeComponent();

            log.InfoFormat("MainWindow hash code:{0}", this.GetHashCode());

            this.Title = System.Reflection.Assembly.GetExecutingAssembly().GetName().Name;

            //_notLogin = false;

            CallController.Instance.CallStatusChanged += OnCallStatusChanged;

            _viewModel = this.DataContext as MainWindowViewModel;

            LoginManager.Instance.PropertyChanged += LoginManager_PropertyChanged;
        }
        
        private void OnCallStatusChanged(object sender, CallStatus status)
        {
            log.Info("OnCallStatusChanged start.");
            App.Current.Dispatcher.InvokeAsync(() => {
                log.Info("Begin to handle call status in UI thread.");
                if (CallStatus.Connected == status)
                {
                    StopFlashWindow();
                    if (null != _incomingCallPromptDialog)
                    {
                        _incomingCallPromptDialog.Visibility = Visibility.Collapsed;
                    }
                    SystemSleepManager.PreventSleep();
                }
                else
                {
                    StopFlashWindow();
                    if (CallStatus.Ended == status)
                    {
                        SystemSleepManager.ResumeSleep();
                    }
                }

                log.Info("Handled call status in UI thread.");
            });

            log.Info("OnCallStatusChanged end.");
        }

        public void BringToForeground()
        {
            log.Info("Bring to foreground");
            Window winToBeBroughtToFront;
            if (_notLogin && null != LoginManager.Instance.LoginWindow)
            {
                winToBeBroughtToFront = LoginManager.Instance.LoginWindow;
            }
            else
            {
                if (null != VideoPeopleWindow.Instance && Visibility.Visible == VideoPeopleWindow.Instance.Visibility)
                {
                    winToBeBroughtToFront = VideoPeopleWindow.Instance;
                }
                else
                {
                    winToBeBroughtToFront = this;
                }
            }

            if (null != LoginManager.Instance.LoginWindow)
            {
            //    LoginManager.Instance.LoginWindow.WindowState = WindowState.Normal;
                LoginManager.Instance.LoginWindow.Topmost = true;
                LoginManager.Instance.LoginWindow.Topmost = false;
            }
            //this.WindowState = WindowState.Normal;
            this.Topmost = true;
            this.Topmost = false;
            if (null != VideoPeopleWindow.Instance)
            {
            //    VideoPeopleWindow.Instance.WindowState = WindowState.Normal;
                VideoPeopleWindow.Instance.Topmost = true;
                VideoPeopleWindow.Instance.Topmost = false;
            }

            if (winToBeBroughtToFront.WindowState == WindowState.Minimized || winToBeBroughtToFront.Visibility == Visibility.Hidden)
            {
                FullScreenBaseWindow fullWin = winToBeBroughtToFront as FullScreenBaseWindow;
                log.InfoFormat("The current window is minimized and should change to normal. FullScreenBaseWindow: {0}", null != fullWin);
                if (null != fullWin)
                {
                    fullWin.RestoreWindow();
                }
                else
                {
                    winToBeBroughtToFront.WindowState = WindowState.Normal;
                }
                winToBeBroughtToFront.Show();
            }

            // According to some sources these steps gurantee that an app will be brought to foreground.
            winToBeBroughtToFront.Activate();
            winToBeBroughtToFront.Topmost = true;
            winToBeBroughtToFront.Topmost = false;
            winToBeBroughtToFront.Focus();
        }
        
        public void InitData()
        {
            _viewModel.CheckSoftwareUpdate(false);
        }

        public void CheckSoftwareUpdate()
        {
            _viewModel.CheckSoftwareUpdate(true);
        }

        public IMasterDisplayWindow GetCurrentMainDisplayWindow()
        {
            if (null != LoginManager.Instance.LoginWindow && Visibility.Visible == LoginManager.Instance.LoginWindow.Visibility)
            {
                log.Info("Main display window: LoginWindow");
                LoginWindow loginWindow = LoginManager.Instance.LoginWindow as LoginWindow;
                return loginWindow;
            }
            else if (Visibility.Visible == this.Visibility && this.IsLoaded)
            {
                log.Info("Main display window: MainWindow");
                return this;
            }
            //else if (Visibility.Visible == LayoutBackgroundWindow.Instance.Visibility)
            //{
            //    return LayoutBackgroundWindow.Instance;
            //}
            else if (Visibility.Visible == VideoPeopleWindow.Instance.Visibility)
            {
                log.Info("Main display window: VideoPeopleWindow");
                return VideoPeopleWindow.Instance;
            }

            log.Info("No visible window currently.");
            return null;
        }

        public void CloseMessageBoxTip()
        {
            if (null != _messageBoxTip && _messageBoxTip.IsLoaded)
            {
                // only keep the last prompt message to show
                log.InfoFormat("Prompt message arrived, but previous message is still displayed, close it. Message -- {0}", _messageTipExtraInfo);
                _messageBoxTip.Close();
                _messageBoxTip = null;
            }
        }

        public void ShowPromptTip(string prompt, string extraInfo)
        {
            IMasterDisplayWindow masterWindow = GetCurrentMainDisplayWindow();
            if (null == masterWindow)
            {
                log.Info("Can not show error message for master window is null");
                return;
            }

            CloseMessageBoxTip();
            _messageBoxTip = new MessageBoxTip();
            _messageTipExtraInfo = extraInfo;
            Window tipOwner = null;
            if (
                   null != (masterWindow as VideoPeopleWindow)
                && LayoutBackgroundWindow.Instance.IsLoaded
                && LayoutBackgroundWindow.Instance.LayoutOperationbarWindow.IsLoaded
            )
            {
                tipOwner = LayoutBackgroundWindow.Instance.LayoutOperationbarWindow;
            }
            log.InfoFormat("ShowPromptTip(MessageBoxTip), tipOwner is null:{0}", null == tipOwner);
            DisplayUtil.SetWindowCenterAndOwner(_messageBoxTip, masterWindow, tipOwner);
            _messageBoxTip.SetTitleAndMsg(LanguageUtil.Instance.GetValueByKey("PROMPT"), prompt, LanguageUtil.Instance.GetValueByKey("CONFIRM"));
            _messageBoxTip.ShowDialog();
            _messageBoxTip = null;
            _messageTipExtraInfo = "";
        }
        
        protected override void OnClosed(EventArgs e)
        {
            log.Info("OnClosed");
            CallController.Instance.CallStatusChanged -= OnCallStatusChanged;
            EVSdkManager.Instance.EventJoinConferenceIndication -= EVSdkWrapper_EventJoinConferenceIndication;
            if (CallController.Instance.CurrentCallStatus != CallStatus.Idle && CallController.Instance.CurrentCallStatus != CallStatus.Ended)
            {
                CallController.Instance.TerminateCall();
            }
            base.OnClosed(e);

            Thread.Sleep(500);//should give this time to terminate call

            log.Info("Release resource");
            EVSdkManager.Instance.Release();
            CefSharpUtil.ShutdownCefSharp();
            View.LayoutBackgroundWindow.Instance.Dispose();
            TerminateProcess(Process.GetCurrentProcess().Handle, 1);
        }
        
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            EVSdkManager.Instance.EventJoinConferenceIndication += EVSdkWrapper_EventJoinConferenceIndication;
            _viewModel.RefreshSoftwareUpdateWindow();
        }

        private void EVSdkWrapper_EventJoinConferenceIndication(ManagedEVSdk.Structs.EVCallInfoCli callInfo)
        {
            log.Info("EventJoinConferenceIndication");
            if (!(LoginStatus.LoggedIn == LoginManager.Instance.CurrentLoginStatus && (CallStatus.Ended == CallController.Instance.CurrentCallStatus || CallStatus.Idle == CallController.Instance.CurrentCallStatus)))
            {
                log.Info("Login status or call status is not expected. EventJoinConferenceIndication end");
                return;
            }
            Application.Current.Dispatcher.InvokeAsync(() =>
            {
                if (Utils.GetAutoAnswer())
                {
                    _viewModel.StartJoinConference(callInfo.conference_number, callInfo.password, this);
                    log.Info("Auto answer. EventJoinConferenceIndication end");
                    return;
                }

                if (null == _incomingCallPromptDialog)
                {
                    _incomingCallPromptDialog = new IncomingCallPromptDialog();
                }

                if (Visibility.Visible == _incomingCallPromptDialog.Visibility || _incomingCallPromptDialog.IsActive)
                {
                    // There is an incoming call and another call is coming.
                    log.Info("Another call coming. EventJoinConferenceIndication end");
                    return;
                }

                _incomingCallPromptDialog.SetPromptInfo(callInfo.peer, callInfo.conference_number);
                _incomingCallPromptDialog.Owner = this;
                StartIncomingCallPromptTimer();
                _incomingCallPromptDialog.ShowDialog();
                StopIncomingCallPromptTimer();
                if (_incomingCallPromptDialog.IsAccept)
                {
                    _viewModel.StartJoinConference(callInfo.conference_number, callInfo.password, this);
                }
            });
            log.Info("EventJoinConferenceIndication end");
        }
        
        private void StartIncomingCallPromptTimer()
        {
            if (null == _incomingCallPromptTimer)
            {
                _incomingCallPromptTimer = new System.Timers.Timer();
                _incomingCallPromptTimer.Elapsed += OnTimerIncomingCallPrompt;
                _incomingCallPromptTimer.AutoReset = false;
                _incomingCallPromptTimer.Interval = 30 * 1000;
            }

            StartFlashWindow();
            CallController.Instance.PlayRingtone();
            _incomingCallPromptTimer.Enabled = true;
        }

        private void OnTimerIncomingCallPrompt(object source, System.Timers.ElapsedEventArgs e)
        {
            if (Visibility.Visible == _incomingCallPromptDialog.Visibility)
            {
                System.Windows.Application.Current.Dispatcher.Invoke(() =>
                {
                    _incomingCallPromptDialog.IsAccept = false;
                    _incomingCallPromptDialog.Visibility = Visibility.Collapsed;
                });
            }
        }

        private void StopIncomingCallPromptTimer()
        {
            CallController.Instance.StopRingtone();

            StopFlashWindow();
            _incomingCallPromptTimer.Enabled = false;
        }

        private void LoginManager_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if ("CurrentLoginStatus" == e.PropertyName)
            {
                LoginStatus status = LoginManager.Instance.CurrentLoginStatus;
                _notLogin =  LoginStatus.NotLogin == status || LoginStatus.LoginFailed == status || LoginStatus.LoggingIn == status;
            }
            
        }

        private void StartFlashWindow()
        {
            if (_flashWindowStarted)
            {
                return;
            }
            FlashWindow.Start(Handle);
            _flashWindowStarted = true;
        }

        private void StopFlashWindow()
        {
            if (!_flashWindowStarted)
            {
                return;
            }
            FlashWindow.Stop(Handle);
            _flashWindowStarted = false;
        }

        #region -- Protected Method --

        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonDown(e);
            this.DragMove();
        }

        #endregion
    }
}
