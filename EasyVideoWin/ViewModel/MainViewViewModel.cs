using EasyVideoWin.Helpers;
using EasyVideoWin.Model;
using log4net;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace EasyVideoWin.ViewModel
{
    class MainViewViewModel : ViewModelBase
    {
        #region -- Members --

        private readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private UserControl _conferenceView;
        private UserControl _contactView;
        private UserControl _settingView;
        private UserControl _currentView;
        private bool _conferenceViewEnabled;
        private bool _contactViewEnabled;
        private bool _loginUserViewEnabled;
        
        #endregion

        #region -- Properties --

        public string UserDisplayName
        {
            get
            {
                return LoginManager.Instance.DisplayName;
            }
        }

        public RelayCommand ShowConferenceCommand { get; set; }
        public RelayCommand ShowContactCommand { get; set; }
        public RelayCommand ShowLoginUserCommand { get; set; }
        
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
                    if (null != _currentView)
                    {
                        _currentView.Visibility = Visibility.Collapsed;
                    }
                    _currentView = value;
                    _currentView.Visibility = Visibility.Visible;
                    OnPropertyChanged("CurrentView");
                }
                
                ConferenceViewEnabled = false;
                ContactViewEnabled = false;
                LoginUserViewEnabled = false;
                if (_conferenceView == _currentView)
                {
                    ConferenceViewEnabled = true;
                }
                if (_contactView == _currentView)
                {
                    ContactViewEnabled = true;
                }
                else if (_settingView == _currentView)
                {
                    LoginUserViewEnabled = true;
                }
            }
        }

        public bool ConferenceViewEnabled
        {
            get
            {
                return _conferenceViewEnabled;
            }
            set
            {
                _conferenceViewEnabled = value;
                OnPropertyChanged("ConferenceViewEnabled");
            }
        }

        public bool ContactViewEnabled
        {
            get
            {
                return _contactViewEnabled;
            }
            set
            {
                _contactViewEnabled = value;
                OnPropertyChanged("ContactViewEnabled");
            }
        }

        public bool LoginUserViewEnabled
        {
            get
            {
                return _loginUserViewEnabled;
            }
            set
            {
                _loginUserViewEnabled = value;
                OnPropertyChanged("LoginUserViewEnabled");
            }
        }

        private string _registerStatusText;
        public string RegisterStatusText
        {
            get
            {
                return _registerStatusText;
            }
            set
            {
                if (_registerStatusText != value)
                {
                    _registerStatusText = value;
                    OnPropertyChanged("RegisterStatusText");
                }
            }
        }

        private string _registerStatusImg;
        public string RegisterStatusImg
        {
            get
            {
                return _registerStatusImg;
            }
            set
            {
                if (_registerStatusImg != value)
                {
                    _registerStatusImg = value;
                    OnPropertyChanged("RegisterStatusImg");
                }
            }
        }

        public WriteableBitmap AvatarBmp
        {
            get
            {
                return LoginManager.Instance.AvatarBmp;
            }
        }

        public Visibility ContactsVisibility
        {
            get
            {
                return null != LoginManager.Instance.LoginUserInfo && LoginManager.Instance.LoginUserInfo.featureSupport.contactWebPage
                        ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        #endregion

        #region -- Constructor --

        public MainViewViewModel()
        {
            ShowConferenceCommand = new RelayCommand(ShowConference);
            ShowContactCommand = new RelayCommand(ShowContact);
            ShowLoginUserCommand = new RelayCommand(ShowLoginUser);
            
            _conferenceView = new View.WebBrowserWrapperView(Enums.WebBrowserUrlType.CONFERENCES, (MainWindow)Application.Current.MainWindow);
            _contactView = new View.WebBrowserWrapperView(Enums.WebBrowserUrlType.CONTACTS, (MainWindow)Application.Current.MainWindow, false);
            _settingView = (UserControl)Activator.CreateInstance(typeof(EasyVideoWin.View.SettingView));
            
            CurrentView = _conferenceView;
            LoginManager.Instance.PropertyChanged += LoginManager_PropertyChanged;

            // update current register status for UI display
            UpdateRegisterStatus(LoginManager.Instance.IsRegistered);
        }

        #endregion

        #region -- Private Method --
        
        private void LoginManager_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if ("CurrentLoginStatus" == e.PropertyName)
            {
                if (LoginStatus.LoggedIn == LoginManager.Instance.CurrentLoginStatus)
                {
                    Application.Current.Dispatcher.InvokeAsync(() =>
                    {
                        if (null == _conferenceView)
                        {
                            _conferenceView = new View.WebBrowserWrapperView(Enums.WebBrowserUrlType.CONFERENCES, (MainWindow)Application.Current.MainWindow);
                        }

                        if (null == _contactView)
                        {
                            _contactView = new View.WebBrowserWrapperView(Enums.WebBrowserUrlType.CONTACTS, (MainWindow)Application.Current.MainWindow, false);
                        }
                        CurrentView = _conferenceView;
                    });
                }
                else if (LoginStatus.NotLogin == LoginManager.Instance.CurrentLoginStatus)
                {
                    // destruct confrence view object to release the browser
                    Application.Current.Dispatcher.InvokeAsync(() => {
                        if (null != _conferenceView)
                        {
                            (_conferenceView as IDisposable)?.Dispose();
                            _conferenceView = null;
                        }

                        if (null != _contactView)
                        {
                            (_contactView as IDisposable)?.Dispose();
                            _contactView = null;
                        }
                    });
                }
            }
            else if ("DisplayName" == e.PropertyName)
            {
                OnPropertyChanged("UserDisplayName");
            }
            else if ("AvatarBmp" == e.PropertyName)
            {
                OnPropertyChanged("AvatarBmp");
            }
            else if ("IsRegistered" == e.PropertyName)
            {
                UpdateRegisterStatus(LoginManager.Instance.IsRegistered);
            }
            else if ("LoginUserInfo" == e.PropertyName)
            {
                OnPropertyChanged("ContactsVisibility");
            }
        }
        
        private void UpdateRegisterStatus(bool isRegistered)
        {
            log.InfoFormat("UpdateRegisterStatus, isRegistered: {0}", isRegistered);
            RegisterStatusText = isRegistered ? LanguageUtil.Instance.GetValueByKey("REGISTER_SUCCESS") : LanguageUtil.Instance.GetValueByKey("REGISTER_FAILURE");
            RegisterStatusImg = isRegistered ? "pack://application:,,,/Resources/Icons/icon_register_success.png" : "pack://application:,,,/Resources/Icons/icon_register_failure.png";
        }

        private void ShowConference(object parameter)
        {
            CurrentView = _conferenceView;
        }

        private void ShowContact(object parameter)
        {
            CurrentView = _contactView;
        }

        private void ShowLoginUser(object parameter)
        {
            CurrentView = _settingView;
        }
        
        #endregion
    }
}
