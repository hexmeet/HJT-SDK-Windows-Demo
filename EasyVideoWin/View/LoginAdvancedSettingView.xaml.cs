using EasyVideoWin.CustomControls;
using EasyVideoWin.Helpers;
using EasyVideoWin.Model;
using EasyVideoWin.Model.CloudModel;
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
    /// Interaction logic for LoginAdvancedSettingView.xaml
    /// </summary>
    public partial class LoginAdvancedSettingView : UserControl
    {
        #region -- Members --

        #endregion

        #region -- Properties --

        #endregion

        #region -- Constructor --

        public LoginAdvancedSettingView()
        {
            InitializeComponent();

            this.Loaded += LoginAdvancedSettingView_Loaded;
        }

        #endregion

        #region -- Public Method --

        #endregion

        #region -- Private Method --

        private void LoginAdvancedSettingView_Loaded(object sender, RoutedEventArgs e)
        {
            RefreshControlValue();
        }

        private void RefreshControlValue()
        {
            int nPort = Utils.GetServerPort();
            this.textBoxPort.Text = 0 == nPort ? "" : nPort.ToString();
            this.checkBoxUseHttps.IsChecked = Utils.GetUseHttps();
        }

        private void Ok_Click(object sender, RoutedEventArgs e)
        {
            bool valid = true;
            string szPort = this.textBoxPort.Text.Trim();
            if (string.IsNullOrEmpty(szPort))
            {
                szPort =  this.checkBoxUseHttps.IsChecked.HasValue && this.checkBoxUseHttps.IsChecked.Value ? "443" : "80";
            }
            int nPort;
            if (!int.TryParse(szPort, out nPort))
            {
                valid = false;
            }
            else
            {
                if (nPort < 1 || nPort > 65535)
                {
                    valid = false;
                }
            }

            if (!valid)
            {
                MessageBoxTip tip = new MessageBoxTip((IMasterDisplayWindow)LoginManager.Instance.LoginWindow);
                tip.SetTitleAndMsg(LanguageUtil.Instance.GetValueByKey("PROMPT"), LanguageUtil.Instance.GetValueByKey("VALID_PORT_VALUE_PROMPT"), LanguageUtil.Instance.GetValueByKey("CONFIRM"));
                tip.ShowDialog();
                return;
            }

            Utils.SetServerPort(nPort);
            Utils.SetUseHttps(this.checkBoxUseHttps.IsChecked.Value);

            LoginManager.Instance.LoginProgress = LoginManager.Instance.PreviousLoginProgress;
        }
        
        private void Port_LostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(this.textBoxPort.Text.Trim()))
            {
                this.textBoxPort.Tag = LanguageUtil.Instance.GetValueByKey("PORT_OPTIONAL");
            }
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            LoginManager.Instance.LoginProgress = LoginManager.Instance.PreviousLoginProgress;
        }

        private void UseHttps_Click(object sender, RoutedEventArgs e)
        {
            this.textBoxPort.Text = this.checkBoxUseHttps.IsChecked.Value ? "443" : "80";
        }

        #endregion
        
    }
}
