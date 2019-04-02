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

namespace EasyVideoWin.ViewModel
{
    class LoginOptionsViewModel : ViewModelBase
    {
        #region -- Members --

        private string _currentTitle;

        #endregion

        #region -- Properties --

        public RelayCommand JoinConfViewCommand { get; set; }
        public RelayCommand LoginViewCommand { get; set; }

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

        public Visibility Application4TrialVisibility
        {
            get
            {
                return LoginProgressEnum.CloudOptions == LoginManager.Instance.LoginProgress ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        #endregion

        #region -- Constructor --

        public LoginOptionsViewModel()
        {
            JoinConfViewCommand = new RelayCommand(JoinConfView);
            LoginViewCommand = new RelayCommand(LoginView);

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
                Application.Current.Dispatcher.Invoke(() =>
                {
                    OnLoginProgressChanged();
                });
            }
        }

        private void OnLoginProgressChanged()
        {
            switch (LoginManager.Instance.LoginProgress)
            {
                case LoginProgressEnum.EnterpriseOptions:
                    CurrentTitle = LanguageUtil.Instance.GetValueByKey("ENTERPRISE_USER");
                    OnPropertyChanged("Application4TrialVisibility");
                    break;
                case LoginProgressEnum.CloudOptions:
                    CurrentTitle = LanguageUtil.Instance.GetValueByKey("CLOUD_USER");
                    OnPropertyChanged("Application4TrialVisibility");
                    break;
            }
        }

        private void JoinConfView(object parameter)
        {
            LoginManager.Instance.LoginProgress = LoginProgressEnum.EnterpriseOptions == LoginManager.Instance.LoginProgress
                                                    ? LoginProgressEnum.EnterpriseJoinConf : LoginProgressEnum.CloudJoinConf;
        }

        private void LoginView(object parameter)
        {
            LoginManager.Instance.LoginProgress = LoginProgressEnum.EnterpriseOptions == LoginManager.Instance.LoginProgress
                                                    ? LoginProgressEnum.EnterpriseLogin : LoginProgressEnum.CloudLogin;
        }

        #endregion
    }
}
