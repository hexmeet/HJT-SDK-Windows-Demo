using CefSharp;
using CefSharp.WinForms;
using EasyVideoWin.CustomControls;
using EasyVideoWin.Helpers;
using EasyVideoWin.Model;
using EasyVideoWin.Model.CloudModel;
using EasyVideoWin.WinForms;
using log4net;
using System;
using System.Collections.Generic;
using System.ComponentModel;
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

namespace EasyVideoWin.View
{
    /// <summary>
    /// Interaction logic for ConfManagementWindow.xaml
    /// </summary>
    public partial class ConfManagementWindow : FullScreenBaseWindow
    {
        #region -- Members --

        private const double WINDOW_DESIGN_WIDTH = 662;
        private readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        
        private WebBrowserWrapperView _confCtrlView = null;
        private UserControl _currentView;

        private const double INITIAL_WIDTH = 662d;
        private const double INITIAL_HEIGHT = 570d;

        #endregion

        #region -- Properties --

        public UserControl CurrentView
        {
            get
            {
                return _currentView;
            }
            set
            {
                if (_currentView != value)
                {
                    _currentView = value;
                    OnPropertyChanged("CurrentView");
                }
            }
        }

        #endregion

        #region -- Constructor --

        public ConfManagementWindow() : base(INITIAL_WIDTH, INITIAL_HEIGHT)
        {
            InitializeComponent();
            this.DataContext = this;

            this.IsVisibleChanged += ConfManagementWindow_IsVisibleChanged;
            
            this.StateChanged += ConfManagementWindow_StateChanged;

            CallController.Instance.CallStatusChanged += OnCallStatusChanged;
            _confCtrlView = new EasyVideoWin.View.WebBrowserWrapperView(Enums.WebBrowserUrlType.CONF_MANAGEMENT, this);
            CurrentView = _confCtrlView;
        }
        
        #endregion

        #region -- Public Method --
        
        override protected void Dispose(bool disposing)
        {
            if (!IsDisposed)
            {
                base.Dispose(disposing);
                if (disposing)
                {
                    //Clean Up managed resources
                    Application.Current.Dispatcher.Invoke(() => {
                        this.IsVisibleChanged -= ConfManagementWindow_IsVisibleChanged;
                    });
                }
                //Clean up unmanaged resources  
            }
            IsDisposed = true;
        }

        #endregion

        #region -- Protected Method --

        
        #endregion

        #region -- Private Method --
        
        private void ConfManagementWindow_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            ConfManagementWindow_StateChanged(null, new EventArgs());
            
            if (Visibility.Visible == this.Visibility)
            {
                _confCtrlView.UpdateData();
            }
            else
            {
                _confCtrlView.ClearConfManagementCache();
            }
        }
        

        private void ConfManagementWindow_StateChanged(object sender, EventArgs e)
        {
            if (WindowState.Maximized == WindowState)
            {
                this.maxButton.Visibility = Visibility.Collapsed;
                this.restoreButton.Visibility = Visibility.Visible;
            }
            else if (WindowState.Normal == WindowState)
            {
                this.maxButton.Visibility = Visibility.Visible;
                this.restoreButton.Visibility = Visibility.Collapsed;
            }
        }

        private void MinWindow_Click(object sender, RoutedEventArgs e)
        {
            MinimizeWindow();
        }

        private void MaxWindow_Click(object sender, RoutedEventArgs e)
        {
            MaximizeWindow();
        }

        private void RestoreWindow_Click(object sender, RoutedEventArgs e)
        {
            RestoreWindow();
        }

        private void CloseBtn_Click(object sender, RoutedEventArgs e)
        {
            this.Visibility = Visibility.Collapsed;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            DisplayUtil.AdjustWindowPosition(this, VideoPeopleWindow.Instance);
        }

        private void BackGround_MouseMove(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                if (!(e.OriginalSource is TextBox))
                {
                    this.DragMove();
                }
            }
        }

        private void OnCallStatusChanged(object sender, CallStatus status)
        {
            log.Info("OnCallStatusChanged");
            switch (status)
            {
                case CallStatus.Connected:
                    Application.Current.Dispatcher.InvokeAsync(() =>
                    {
                        if (null != _confCtrlView)
                        {
                            _confCtrlView.Dispose();
                        }
                        
                        _confCtrlView = new EasyVideoWin.View.WebBrowserWrapperView(Enums.WebBrowserUrlType.CONF_MANAGEMENT, this);
                        CurrentView = _confCtrlView;
                    });
                    break;
                case CallStatus.Ended:
                    Application.Current.Dispatcher.InvokeAsync(() => {
                        if (null != _confCtrlView)
                        {
                            _confCtrlView.Dispose();
                            _confCtrlView = null;
                        }
                    });
                    break;
            }

            log.Info("OnCallStatusChanged done");
        }

        #endregion
        

    }
}
