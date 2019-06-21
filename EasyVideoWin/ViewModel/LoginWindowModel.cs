using EasyVideoWin.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using EasyVideoWin.Model;
using System.Windows.Controls;
using System.Net;
using log4net;
using System.Threading;
using System.Windows;
using EasyVideoWin.View;
using System.Windows.Media;
using EasyVideoWin.Model.CloudModel;

namespace EasyVideoWin.ViewModel
{
    class LoginWindowModel : ViewModelBase
    {
        #region -- Members --

        private readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private static System.Windows.Media.Brush PRODUCT_NAME_COLOR = new SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#4381FF"));
        private static System.Windows.Media.Brush OPERATION_NAME_COLOR = new SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#000000"));

        private UserControl _currentView;
        private UserControl _loginTypeView;
        private UserControl _loginOptionsView;
        private UserControl _loginInfoView;
        private UserControl _loginJoinConfView;
        private UserControl _loginAdvancedSettingView;
        private string _currentTitle;
        private System.Windows.Media.Brush _titleColor;

        private Visibility _titleVisibility;
        private Visibility _logoVisibility;

        #endregion

        #region -- Properties --

        public RelayCommand BackCommand { get; set; }
        
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
                
                OnPropertyChanged("CurrentTitle");
            }
        }

        public string CurrentTitle
        {
            get
            {
                return _currentTitle;
            }
            set
            {
                _currentTitle = value;
                OnPropertyChanged("CurrentTitle");
            }
        }

        public System.Windows.Media.Brush TitleColor
        {
            get
            {
                return _titleColor;
            }
            set
            {
                _titleColor = value;
                OnPropertyChanged("TitleColor");
            }
        }

        public Visibility BackVisibility
        {
            get
            {
                return LoginProgressEnum.Idle == LoginManager.Instance.LoginProgress ? Visibility.Collapsed : Visibility.Visible;
            }
        }
        
        public Visibility TitleVisibility
        {
            get
            {
                return _titleVisibility;
            }
            set
            {
                _titleVisibility = value;
                OnPropertyChanged("TitleVisibility");
            }
        }

        public Visibility LogoVisibility
        {
            get
            {
                return _logoVisibility;
            }
            set
            {
                _logoVisibility = value;
                OnPropertyChanged("LogoVisibility");
            }
        }

        #endregion

        #region -- Constructor --

        public LoginWindowModel()
        {
            BackCommand = new RelayCommand(Back);

            _loginTypeView              = (UserControl)Activator.CreateInstance(typeof(LoginTypeView));
            _loginOptionsView           = (UserControl)Activator.CreateInstance(typeof(LoginOptionsView));
            _loginInfoView              = (UserControl)Activator.CreateInstance(typeof(LoginInfoView));
            _loginJoinConfView          = (UserControl)Activator.CreateInstance(typeof(AnonymousJoinConfView));
            _loginAdvancedSettingView   = (UserControl)Activator.CreateInstance(typeof(LoginAdvancedSettingView));

            OnLoginProgressChanged();

            LoginManager.Instance.PropertyChanged += LoginManager_PropertyChanged;
        }

        #endregion

        #region -- Public Method --

        #endregion

        #region -- Private Method --

        private void LoginManager_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if ("LoginProgress" == e.PropertyName)
            {
                Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    OnLoginProgressChanged();
                });
            }
            else if ("CurrentLoginStatus" == e.PropertyName)
            {
                log.InfoFormat("Current login status changed, status={0}", LoginManager.Instance.CurrentLoginStatus);
            }
        }

        private void OnLoginProgressChanged()
        {
            switch (LoginManager.Instance.LoginProgress)
            {
                case LoginProgressEnum.Idle:
                    CurrentView = _loginTypeView;
                    //CurrentTitle = LanguageUtil.Instance.GetValueByKey("PRODUCT_NAME");
                    //TitleColor = PRODUCT_NAME_COLOR;
                    LogoVisibility = Visibility.Visible;
                    TitleVisibility = Visibility.Hidden;
                    break;
                case LoginProgressEnum.EnterpriseOptions:
                case LoginProgressEnum.CloudOptions:
                    CurrentView = _loginOptionsView;
                    //CurrentTitle = LanguageUtil.Instance.GetValueByKey("PRODUCT_NAME");
                    //TitleColor = PRODUCT_NAME_COLOR;
                    LogoVisibility = Visibility.Visible;
                    TitleVisibility = Visibility.Hidden;
                    break;
                case LoginProgressEnum.EnterpriseLogin:
                    CurrentView = _loginInfoView;
                    CurrentTitle = LanguageUtil.Instance.GetValueByKey("ENTERPRISE_LOGIN");
                    TitleColor = OPERATION_NAME_COLOR;
                    LogoVisibility = Visibility.Hidden;
                    TitleVisibility = Visibility.Visible;
                    break;
                case LoginProgressEnum.EnterpriseJoinConf:
                case LoginProgressEnum.CloudJoinConf:
                    CurrentView = _loginJoinConfView;
                    CurrentTitle = LanguageUtil.Instance.GetValueByKey("JOIN_CONFERENCE");
                    TitleColor = OPERATION_NAME_COLOR;
                    LogoVisibility = Visibility.Hidden;
                    TitleVisibility = Visibility.Visible;
                    break;
                case LoginProgressEnum.CloudLogin:
                    CurrentView = _loginInfoView;
                    CurrentTitle = LanguageUtil.Instance.GetValueByKey("CLOUD_LOGIN");
                    TitleColor = OPERATION_NAME_COLOR;
                    LogoVisibility = Visibility.Hidden;
                    TitleVisibility = Visibility.Visible;
                    break;
                case LoginProgressEnum.AdvancedSetting:
                    CurrentView = _loginAdvancedSettingView;
                    CurrentTitle = LanguageUtil.Instance.GetValueByKey("ADVANCED_SETTING");
                    TitleColor = OPERATION_NAME_COLOR;
                    LogoVisibility = Visibility.Hidden;
                    TitleVisibility = Visibility.Visible;
                    break;

            }
            
            OnPropertyChanged("BackVisibility");
        }

        private void Back(object parameter)
        {
            switch (LoginManager.Instance.LoginProgress)
            {
                case LoginProgressEnum.Idle:
                    break;
                case LoginProgressEnum.EnterpriseOptions:
                case LoginProgressEnum.CloudOptions:
                    LoginManager.Instance.LoginProgress = LoginProgressEnum.Idle;
                    break;
                case LoginProgressEnum.EnterpriseLogin:
                case LoginProgressEnum.EnterpriseJoinConf:
                    LoginManager.Instance.LoginProgress = LoginProgressEnum.EnterpriseOptions;
                    break;
                case LoginProgressEnum.CloudJoinConf:
                case LoginProgressEnum.CloudLogin:
                    LoginManager.Instance.LoginProgress = LoginProgressEnum.CloudOptions;
                    break;
                case LoginProgressEnum.AdvancedSetting:
                    LoginManager.Instance.LoginProgress = LoginManager.Instance.PreviousLoginProgress;
                    break;
            }
        }
        
        #endregion
    }
}
