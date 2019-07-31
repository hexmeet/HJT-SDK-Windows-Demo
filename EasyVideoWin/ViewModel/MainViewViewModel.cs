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
        private UserControl _loginUserView;
        private UserControl _currentView;
        private bool _conferenceViewEnabled;
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
        public RelayCommand ShowAddressBookCommand { get; set; }
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
                LoginUserViewEnabled = false;
                if (_conferenceView == _currentView)
                {
                    ConferenceViewEnabled = true;
                }
                else if (_loginUserView == _currentView)
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
        
        Visibility _failureVisibility;
        public Visibility RegisterFailureVisibility
        {
            get
            {
                return _failureVisibility;
            }
            set
            {
                _failureVisibility = value;
                OnPropertyChanged("RegisterFailureVisibility");
            }
        }

        Visibility _successVisibility;
        public Visibility RegisterSuccessVisibility
        {
            get
            {
                return _successVisibility;
            }
            set
            {
                _successVisibility = value;
                OnPropertyChanged("RegisterSuccessVisibility");
            }
        }

        public WriteableBitmap AvatarBmp
        {
            get
            {
                return LoginManager.Instance.AvatarBmp;
            }
        }

        #endregion

        #region -- Constructor --

        public MainViewViewModel()
        {
            ShowConferenceCommand = new RelayCommand(ShowConference);
            ShowLoginUserCommand = new RelayCommand(ShowLoginUser);
            
            _conferenceView = (UserControl)Activator.CreateInstance(typeof(EasyVideoWin.View.ConferenceView));
            _loginUserView = (UserControl)Activator.CreateInstance(typeof(EasyVideoWin.View.SettingView));
            
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
                            _conferenceView = (UserControl)Activator.CreateInstance(typeof(EasyVideoWin.View.ConferenceView));
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
                            var confView = _conferenceView as EasyVideoWin.View.ConferenceView;
                            confView.Dispose();
                            _conferenceView = null;
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
        }
        
        private void UpdateRegisterStatus(bool isRegistered)
        {
            log.InfoFormat("Register status changed, is registered {0}", isRegistered);
            RegisterSuccessVisibility = isRegistered ? Visibility.Visible : Visibility.Collapsed;
            RegisterFailureVisibility = isRegistered ? Visibility.Collapsed : Visibility.Visible;
        }

        private void ShowConference(object parameter)
        {
            CurrentView = _conferenceView;
        }
        
        private void ShowLoginUser(object parameter)
        {
            CurrentView = _loginUserView;
        }
        
        #endregion
    }
}
