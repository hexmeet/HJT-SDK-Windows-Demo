using EasyVideoWin.Helpers;
using EasyVideoWin.Model;
using log4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace EasyVideoWin.View
{
    /// <summary>
    /// Interaction logic for LoginInfoView.xaml
    /// </summary>
    public partial class LoginInfoView : UserControl
    {
        #region -- Members --
        private readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        #endregion

        #region -- Properties --

        #endregion

        #region -- Constructor --

        public LoginInfoView()
        {
            InitializeComponent();

            this.Loaded += LoginInfoView_Loaded;
        }

        #endregion

        #region -- Public Method --

        #endregion

        #region -- Private Method --

        private void LoginInfoView_Loaded(object sender, RoutedEventArgs e)
        {
            RefreshControlValue();
            OnLoginProgressChanged();
            LoginManager.Instance.PropertyChanged += LoginManager_PropertyChanged;
        }

        private void RefreshControlValue()
        {
            if (LoginProgressEnum.AdvancedSetting == LoginManager.Instance.PreviousLoginProgress)
            {
                return;
            }

            this.textBoxServerAddress.Text = Utils.GetServerAddress(LoginManager.Instance.ServiceType);
            this.textBoxNumberEmail.Text = Utils.GetUserName(LoginManager.Instance.ServiceType);
            this.passwordBoxPassword.Password = Utils.GetPassword(LoginManager.Instance.ServiceType);
        }

        private void Login_Click(object sender, RoutedEventArgs e)
        {
            if (LoginStatus.NotLogin != LoginManager.Instance.CurrentLoginStatus && LoginStatus.LoginFailed != LoginManager.Instance.CurrentLoginStatus)
            {
                return;
            }
            
            bool valid = true;
            string szServerAddress;
            if (LoginProgressEnum.EnterpriseLogin == LoginManager.Instance.LoginProgress)
            {
                szServerAddress = this.textBoxServerAddress.Text.Trim();
                if (string.IsNullOrEmpty(szServerAddress))
                {
                    this.textBoxServerAddress.Tag = LanguageUtil.Instance.GetValueByKey("SERVER");
                    valid = false;
                }
            }
            else
            {
                szServerAddress = Utils.CloudServerDomain;
            }

            string szNumberEmail = this.textBoxNumberEmail.Text.Trim();
            if (string.IsNullOrEmpty(szNumberEmail))
            {
                this.textBoxNumberEmail.Tag = LanguageUtil.Instance.GetValueByKey("NUMBER_EMAIL");
                valid = false;
            }

            string szPassword = this.passwordBoxPassword.Password.Trim();
            if (string.IsNullOrEmpty(szPassword))
            {
                this.passwordBoxPassword.Tag = LanguageUtil.Instance.GetValueByKey("PASSWORD");
                valid = false;
            }

            if (!valid)
            {
                return;
            }

            LoginManager.Instance.CurrentLoginStatus = LoginStatus.NotLogin;
            LoginManager.Instance.Login(szNumberEmail, szPassword, szServerAddress);
        }
        
        private void ServerAddress_LostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(this.textBoxServerAddress.Text.Trim()))
            {
                this.textBoxServerAddress.Tag = LanguageUtil.Instance.GetValueByKey("SERVER");
            }
        }

        private void NumberEmail_LostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(this.textBoxNumberEmail.Text.Trim()))
            {
                this.textBoxNumberEmail.Tag = LanguageUtil.Instance.GetValueByKey("NUMBER_EMAIL");
            }
        }

        private void Password_LostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(this.passwordBoxPassword.Password.Trim()))
            {
                this.passwordBoxPassword.Tag = LanguageUtil.Instance.GetValueByKey("PASSWORD");
            }
        }

        private void LoginManager_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if ("LoginProgress" == e.PropertyName)
            {
                Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    OnLoginProgressChanged();
                });
            }
            else if ("IsInitFinished" == e.PropertyName)
            {
                if (LoginManager.Instance.IsInitFinished)
                {
                    TryAutoLogin();
                }
            }
            else if ("IsNeedRelogin" == e.PropertyName)
            {
                if (LoginManager.Instance.IsNeedRelogin)
                {
                    LoginManager.Instance.IsNeedRelogin = false;
                    LoginManager.Instance.LoginWithConfigInfo();
                }
            }
        }

        private void OnLoginProgressChanged()
        {
            switch (LoginManager.Instance.LoginProgress)
            {
                case LoginProgressEnum.EnterpriseLogin:
                    RefreshControlValue();
                    this.textBoxNumberEmail.Margin = new Thickness(0, 6, 0, 0);
                    break;
                case LoginProgressEnum.CloudLogin:
                    RefreshControlValue();
                    this.textBoxNumberEmail.Margin = new Thickness(0, 40, 0, 0);
                    break;
                default:
                    break;
            }
        }

        private void TryAutoLogin()
        {
            if (!LoginManager.Instance.IsNeedAutoLogin)
            {
                log.Info("Try auto login but IsNeedAutoLogin is false.");
                return;
            }

            LoginManager.Instance.IsNeedAutoLogin = false;
            LoginManager.Instance.TryAutoLogin();
        }

        #endregion


    }
}
