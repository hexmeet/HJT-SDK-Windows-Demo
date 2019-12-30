using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.ComponentModel;
using EasyVideoWin.Model;
using System.Threading;
using EasyVideoWin.CustomControls;
using EasyVideoWin.Helpers;
using log4net;
using System.Windows.Input;
using EasyVideoWin.Model.CloudModel;
using System.Windows.Media.Imaging;

namespace EasyVideoWin.View
{
    /// <summary>
    /// Interaction logic for MainControl.xaml
    /// </summary>
    public partial class MainView : UserControl, INotifyPropertyChanged
    {
        private static ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private MainWindow _window;
        private bool _firstLoad;
        private const string MY_SELF_SERVICE = "{0}/mymeetings?token={1}"; // 0: server address, 1: token value

        public event PropertyChangedEventHandler PropertyChanged;

        public WindowState CurrentWindowState
        {
            get
            {
                return null != _window ? _window.WindowState : WindowState.Normal;
            }
        }

        public Visibility MinButtonVisibility
        {
            get
            {
                if (_window != null && _window.FullScreenStatus)
                {
                    return Visibility.Collapsed;
                }
                else
                {
                    return Visibility.Visible;
                }
            }
        }

        public Visibility CloseButtonVisibility
        {
            get
            {
                if (_window != null && _window.FullScreenStatus)
                {
                    return Visibility.Collapsed;
                }
                else
                {
                    return Visibility.Visible;
                }
            }
        }

        public Visibility MaxButtonVisibility
        {
            get
            {
                if(_window != null && !_window.FullScreenStatus)
                {
                    return WindowState.Maximized == CurrentWindowState ? Visibility.Collapsed : Visibility.Visible;
                }
                else
                {
                    return Visibility.Collapsed;
                }
            }
        }

        public Visibility RestoreButtonVisibility
        {
            get
            {
                if(_window != null && !_window.FullScreenStatus)
                {
                    return WindowState.Maximized == CurrentWindowState ? Visibility.Visible : Visibility.Collapsed;
                }
                else
                {
                    return Visibility.Collapsed;
                }
            }
        }

        public Visibility FullScreenVisibility
        {
            get
            {
                if (_window != null && _window.FullScreenStatus)
                {
                    return Visibility.Collapsed;
                }
                else
                {
                    return Visibility.Visible;
                }
            }
        }

        public Visibility ExitFullScreenVisibility
        {
            get
            {
                if (_window != null && _window.FullScreenStatus)
                {
                    return Visibility.Visible;
                }
                else
                {
                    return Visibility.Collapsed;
                }

            }
        }

        public MainView()
        {
            InitializeComponent();

            this._firstLoad = true;

            this.Loaded += MainView_Loaded;
            this.minButton.SetBinding(Button.VisibilityProperty, new Binding("MinButtonVisibility") { Source = this });
            this.maxButton.SetBinding(Button.VisibilityProperty, new Binding("MaxButtonVisibility") { Source = this });
            this.restoreButton.SetBinding(Button.VisibilityProperty, new Binding("RestoreButtonVisibility") { Source = this });
            //this.fullScreenBtn.SetBinding(Button.VisibilityProperty, new Binding("FullScreenVisibility") { Source = this });
            this.fullScreenBtn.Visibility = Visibility.Collapsed;
            this.exitFullScreenBtn.SetBinding(Button.VisibilityProperty, new Binding("ExitFullScreenVisibility") { Source = this });
            this.closeButton.SetBinding(Button.VisibilityProperty, new Binding("CloseButtonVisibility") { Source = this });

            //_window = (MainWindow)Application.Current.MainWindow; ;
            //_window.StateChanged += Window_StateChanged;
            
            LanguageUtil.Instance.LanguageChanged += OnLanguageChanged;
            CallController.Instance.CallStatusChanged += OnCallStatusChanged;
            LoginManager.Instance.PropertyChanged += LoginManager_PropertyChanged;

            if (LanguageUtil.Instance.CurrentLanguage != LanguageType.ZH_CN)
            {
                this.imgLogoHome.Source = new BitmapImage(new Uri("pack://application:,,,/Resources/Icons/logo_home_en.png"));
            }
        }
        
        private void MainView_Loaded(object sender, RoutedEventArgs e)
        {
            if (this._firstLoad)
            {
                _window = (MainWindow)Window.GetWindow(this);
                _window.StateChanged += Window_StateChanged;
                this.Height = _window.ActualHeight;
                _window.SizeChanged += Window_SizeChanged;
                //VideoView.Instance.SetContentWindowHandle();
            }

            ChangeButtonProperty();

            bool fullScreen = Helpers.Utils.GetFullScreenAfterStartup();
            if (fullScreen && this._firstLoad)
            {
                //_window.ChangeWindowState(WindowState.Maximized);
                FullScreen_Click(sender, e);                
            }

            _firstLoad = false;
        }

        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            this.Height = _window.ActualHeight;
        }

        private void Window_StateChanged(object sender, EventArgs e)
        {
            //OnPropertyChanged("CurrentWindowState");
            ChangeButtonProperty();
        }

        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void ChangeButtonProperty()
        {
            OnPropertyChanged("MinButtonVisibility");
            OnPropertyChanged("MaxButtonVisibility");
            OnPropertyChanged("RestoreButtonVisibility");
            OnPropertyChanged("FullScreenVisibility");
            OnPropertyChanged("ExitFullScreenVisibility");
            OnPropertyChanged("CloseButtonVisibility");
        }

        private void MinWindow_Click(object sender, RoutedEventArgs e)
        {
            _window.MinimizeWindow();
        }

        private void MaxWindow_Click(object sender, RoutedEventArgs e)
        {
            _window.MaximizeWindow();
        }

        private void RestoreWindow_Click(object sender, RoutedEventArgs e)
        {
            _window.RestoreWindow();
        }

        private void FullScreen_Click(object sender, RoutedEventArgs e)
        {
            _window.SetFullScreen();
        }

        private void ExitFullScreen_Click(object sender, RoutedEventArgs e)
        {
            _window.RestoreWindow();
        }

        private void CloseWindow_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxConfirm messageBoxEx = new MessageBoxConfirm((MainWindow)Application.Current.MainWindow);
            messageBoxEx.SetTitleAndMsg(LanguageUtil.Instance.GetValueByKey("PROMPT"), LanguageUtil.Instance.GetValueByKey("ARE_YOU_SURE_TO_CLOSE_APP"));
            messageBoxEx.ConfirmEvent += PromptWindowConfirmEvent;

            messageBoxEx.ShowDialog();
        }

        private void PromptWindowConfirmEvent(object sender, EventArgs e)
        {
            //make signaling correctly.
            LoginManager.Instance.Logout();
            // let SDK finish work async.
            Thread.Sleep(500);

            Application.Current.MainWindow.Close();
        }

        private void MySelfService_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            string address = string.Format(
                    MY_SELF_SERVICE
                    , LoginManager.Instance.LoginUserInfo.customizedH5UrlPrefix
                    , LoginManager.Instance.LoginToken
                );
            System.Diagnostics.Process.Start(address);
            e.Handled = true;
        }

        private void OnLanguageChanged(object sender, LanguageType language)
        {
            if (LanguageUtil.Instance.CurrentLanguage != LanguageType.ZH_CN)
            {
                this.imgLogoHome.Source = new BitmapImage(new Uri("pack://application:,,,/Resources/Icons/logo_home_en.png"));
            }
            else
            {
                this.imgLogoHome.Source = new BitmapImage(new Uri("pack://application:,,,/Resources/Icons/logo_home.png"));
            }
        }

        private void Avatar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            MyInfoWindow.Instance.Owner = Window.GetWindow(this);
            MyInfoWindow.Instance.ShowDialog();
            e.Handled = true;
        }

        private void OnCallStatusChanged(object sender, CallStatus status)
        {
            log.Info("OnCallStatusChanged start.");
            Application.Current.Dispatcher.InvokeAsync(() => {
                switch (status)
                {
                    case CallStatus.Dialing:
                    case CallStatus.ConfIncoming:
                    case CallStatus.P2pIncoming:
                    case CallStatus.P2pOutgoing:
                    case CallStatus.Connected:
                        if (Visibility.Visible == MyInfoWindow.Instance.Visibility)
                        {
                            MyInfoWindow.Instance.Visibility = Visibility.Collapsed;
                        }
                        
                        break;
                    default:
                        break;
                }
            });
            log.Info("OnCallStatusChanged end.");
        }

        private void LoginManager_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if ("CurrentLoginStatus" == e.PropertyName)
            {
                if (LoginStatus.LoggedIn != LoginManager.Instance.CurrentLoginStatus)
                {
                    Application.Current.Dispatcher.InvokeAsync(() => {
                        if (Visibility.Visible == MyInfoWindow.Instance.Visibility)
                        {
                            MyInfoWindow.Instance.Visibility = Visibility.Collapsed;
                        }
                    });
                }
            }
        }

    }

}
