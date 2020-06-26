using EasyVideoWin.Helpers;
using EasyVideoWin.Model;
using log4net;
using System;
using System.Threading.Tasks;
using System.Timers;
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
            //LoginManager.Instance.PropertyChanged += LoginManager_PropertyChanged;

            // update current register status for UI display
            UpdateRegisterStatus(LoginManager.Instance.IsRegistered);
        }

        #endregion

        #region -- Private Method --

        // do not use listen event from login manager for main view is not constructed yet when logined.
        public void LoginManager_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if ("CurrentLoginStatus" == e.PropertyName)
            {
                log.InfoFormat("CurrentLoginStatus changed to: {0}", LoginManager.Instance.CurrentLoginStatus);
                if (LoginStatus.LoggedIn == LoginManager.Instance.CurrentLoginStatus)
                {
                    Application.Current.Dispatcher.InvokeAsync(() =>
                    {
                        log.Info("LoggedIn, init _conferenceView and _contactView");
                        if (null == _conferenceView)
                        {
                            _conferenceView = new View.WebBrowserWrapperView(Enums.WebBrowserUrlType.CONFERENCES, (MainWindow)Application.Current.MainWindow);
                        }

                        if (null == _contactView)
                        {
                            _contactView = new View.WebBrowserWrapperView(Enums.WebBrowserUrlType.CONTACTS, (MainWindow)Application.Current.MainWindow, false);
                        }
                        CurrentView = _conferenceView;
                        
                        string joinConfAddress = Utils.GetAnonymousJoinConfServerAddress();
                        string joinConfContactId = Utils.GetAnonymousJoinConfContactId();
                        string joinConfContactAlias = Utils.GetAnonymousJoinConfContactAlias();
                        string joinConfId = Utils.GetAnonymousJoinConfId();
                        string joinConfAlias = Utils.GetAnonymousJoinConfAlias();
                        log.InfoFormat("LoggedIn, joinConfAddress: {0}, joinConfContactId: {1}, joinConfId: {2}, joinConfContactAlias: {3}, joinConfAlias: {4}", joinConfAddress, joinConfContactId, joinConfId, joinConfContactAlias, joinConfAlias);
                        if (!string.IsNullOrEmpty(joinConfAddress) && (!string.IsNullOrEmpty(joinConfContactId) || !string.IsNullOrEmpty(joinConfContactAlias)))
                        {
                            log.Info("Valid link p2p call");
                            string contactName = System.Web.HttpUtility.UrlDecode(Utils.GetAnonymousJoinConfContactName());
                            log.InfoFormat("contactName: {0}", contactName);
                            Utils.ClearAnonymousJoinConfData();

                            // p2p call in 1 second to make sure the content view in main view show correctly.
                            // or the content view can not be initialized when relogin and then p2p call
                            Timer timer = new Timer();
                            timer.Interval = 1000;
                            timer.AutoReset = false;
                            timer.Elapsed += (object sender2, ElapsedEventArgs e2) =>
                            {
                                if (!string.IsNullOrEmpty(joinConfContactId))
                                {
                                    CallController.Instance.P2pCallPeer(joinConfContactId, null, contactName);
                                }
                                else
                                {
                                    CallController.Instance.P2pCallPeerByUserName(joinConfContactAlias, null, contactName);
                                }
                            };
                            timer.Start();
                        }
                        if (!string.IsNullOrEmpty(joinConfAddress) && (!string.IsNullOrEmpty(joinConfId) || !string.IsNullOrEmpty(joinConfAlias)))
                        {
                            log.Info("Valid url join conf");
                            string joinConfPasswd = Utils.GetAnonymousJoinConfPassword();
                            Utils.ClearAnonymousJoinConfData();

                            // url join call in 1 second to make sure the content view in main view show correctly.
                            // or the content view can not be initialized when relogin and then p2p call
                            Timer timer = new Timer();
                            timer.Interval = 1000;
                            timer.AutoReset = false;
                            timer.Elapsed += (object sender2, ElapsedEventArgs e2) =>
                            {
                                if (!string.IsNullOrEmpty(joinConfId))
                                {
                                    CallController.Instance.JoinConference(joinConfId, LoginManager.Instance.DisplayName, joinConfPasswd);
                                }
                                else
                                {
                                    CallController.Instance.JoinConference(joinConfAlias, ManagedEVSdk.Structs.EV_SVC_CONFERENCE_NAME_TYPE_CLI.EV_SVC_CONFERENCE_NAME_ALIAS, LoginManager.Instance.DisplayName, joinConfPasswd);
                                }
                            };
                            timer.Start();
                        }
                    });
                }
                else if (LoginStatus.NotLogin == LoginManager.Instance.CurrentLoginStatus)
                {
                    // destruct confrence view object to release the browser
                    Application.Current.Dispatcher.InvokeAsync(() => {
                        log.Info("NotLogin, destroy _conferenceView and _contactView");
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

                    // to ensure destroy the view and then relogin
                    if (Utils.GetAnonymousLogoutAndLinkP2pCall())
                    {
                        log.Info("Login status changed to NotLogin and GetAnonymousLogoutAndLinkP2pCall is true.");
                        string joinConfAddress = Utils.GetAnonymousJoinConfServerAddress();
                        string joinConfContactId = Utils.GetAnonymousJoinConfContactId();
                        string joinConfContactAlias = Utils.GetAnonymousJoinConfContactAlias();
                        if (!string.IsNullOrEmpty(joinConfAddress) && (!string.IsNullOrEmpty(joinConfContactId) || !string.IsNullOrEmpty(joinConfContactAlias)))
                        {
                            log.Info("Login status changed to NotLogin and begin to login for link p2p call.");
                            LoginManager.Instance.SaveCurrentLoginInfo();
                            Application.Current.Dispatcher.InvokeAsync(() => {
                                log.Info("Begin AutoLogin4LinkP2pCall");
                                LoginManager.Instance.AutoLogin4LinkP2pCall(joinConfAddress);
                            });
                        }

                        Utils.SetAnonymousLogoutAndLinkP2pCall(false);
                    }
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
            log.Info("ShowConference");
            CurrentView = _conferenceView;
        }

        private void ShowContact(object parameter)
        {
            log.Info("ShowContact");
            CurrentView = _contactView;
        }

        private void ShowLoginUser(object parameter)
        {
            log.Info("ShowLoginUser");
            CurrentView = _settingView;
        }
        
        #endregion
    }
}
