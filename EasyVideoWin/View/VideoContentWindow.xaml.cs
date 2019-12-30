using EasyVideoWin.CustomControls;
using EasyVideoWin.Helpers;
using EasyVideoWin.Model;
using log4net;
using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Interop;

namespace EasyVideoWin.View
{
    /// <summary>
    /// Interaction logic for VideoContentView.xaml
    /// </summary>
    public partial class VideoContentWindow : FullScreenBaseWindow, INotifyPropertyChanged
    {
        private readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private static VideoContentWindow _instance = new VideoContentWindow();
        private const double INITIAL_WIDTH = 1280d;
        private const double INITIAL_HEIGHT = 760d;

        public VideoContentWindow() : base(INITIAL_WIDTH, INITIAL_HEIGHT)
        {
            InitializeComponent();

            log.InfoFormat("VideoContentWindow hash code:{0}", this.GetHashCode());

            StateChanged += WindowState_Changed;
            // Loaded event can't received for every call's receiving content.
            // so have to use the visible changed event, help title button display right.
            this.IsVisibleChanged += VideoContentWindow_IsVisibleChanged;

            this.WindowState = WindowState.Normal;
            CallController.Instance.CallStatusChanged += OnCallStatusChanged;
            
            this.Loaded += VideoContentWindow_Loaded;
        }

        private void VideoContentWindow_Loaded(object sender, RoutedEventArgs e)
        {
            Utils.SetSoftwareRender(this);
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            this.Hide();
            e.Cancel = true;
        }
        
        public static VideoContentWindow Instance
        {
            get
            {
                return _instance;
            }
        }

        protected override void OnClosed(EventArgs e)
        {
            CallController.Instance.CallStatusChanged -= OnCallStatusChanged;
            base.OnClosed(e);
        }

        private void ChangeWindowMinState(object sender, RoutedEventArgs e)
        {
            MinimizeWindow();
        }

        private void ChangeWindowMaxState(object sender, RoutedEventArgs e)
        {
            MaximizeWindow();
        }
        
        private void ChangeWindowNormalState(object sender, RoutedEventArgs e)
        {
            RestoreWindow();
        }

        private void ChangeWindowFullScreenState(object sender, RoutedEventArgs e)
        {
            SetFullScreen();
        }
        
        private void ChangeWindowExitFullScreenState(object sender, RoutedEventArgs e)
        {
            RestoreWindow();
        }

        private void VideoContentWindow_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            ChangeToolbarButtonsStatus();
        }

        private void WindowState_Changed(object sender, EventArgs e)
        {
            ChangeToolbarButtonsStatus();
        }

        private void ChangeToolbarButtonsStatus()
        {
            this.minBtn.Visibility = FullScreenStatus ? Visibility.Collapsed : Visibility.Visible;
            this.maxBtn.Visibility = (FullScreenStatus || WindowState == WindowState.Maximized) ? Visibility.Collapsed : Visibility.Visible;
            this.restoreBtn.Visibility = (FullScreenStatus || WindowState != WindowState.Maximized) ? Visibility.Collapsed : Visibility.Visible;
            this.fullScreenBtn.Visibility = FullScreenStatus ? Visibility.Collapsed : Visibility.Visible;
            this.exitFullScreenBtn.Visibility = FullScreenStatus ? Visibility.Visible : Visibility.Collapsed;
        }

        private void Window_OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (this.WindowState != WindowState.Maximized)
            {
                this.DragMove();
            }
        }
        
        private void OnCallStatusChanged(object sender, CallStatus status)
        {
            log.Info("OnCallStatusChanged start.");
            switch (status)
            {
                case CallStatus.Ended:
                    Application.Current.Dispatcher.InvokeAsync(() =>
                    {
                        if (Visibility.Collapsed == this.Visibility)
                        {
                            log.Info("Window has been collapsed.");
                            return;
                        }

                        log.Info("Call ended and hide video content window.");
                        // add this to help make the window state right later.
                        this.ChangeWindowNormalState(null, null);
                        this.Visibility = Visibility.Collapsed;
                        log.Info("Window collapsed.");
                    });
                    break;
            }
            log.Info("OnCallStatusChanged end.");
        }

    }
}
