using EasyVideoWin.Helpers;
using EasyVideoWin.Model;
using EasyVideoWin.Model.CloudModel;
using EasyVideoWin.Model.CloudModel.CloudRest;
using log4net;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
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
    /// Interaction logic for LoginTypeView.xaml
    /// </summary>
    public partial class LoginTypeView : UserControl
    {
        #region -- Members --

        private readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private const string DXJC_AES_KEY = "yzr9dy8x60hv7z82";
        private const string DXJC_AES_IV = "0000000000000000";

        #endregion

        #region -- Properties --

        #endregion

        #region -- Constructor --

        public LoginTypeView()
        {
            InitializeComponent();

            LoginManager.Instance.PropertyChanged += LoginManager_PropertyChanged;
        }

        #endregion

        #region -- Public Method --

        #endregion

        #region -- Private Method --

        private void LoginManager_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if ("IsInitFinished" == e.PropertyName)
            {
                if (LoginManager.Instance.IsInitFinished)
                {
                    if (!LoginManager.Instance.IsNeedAnonymousJoinConf)
                    {
                        return;
                    }

                    string anonymousJoinConfToken = Helpers.Utils.GetAnonymousJoinConfToken();
                    if (string.IsNullOrEmpty(anonymousJoinConfToken))
                    {
                        Application.Current.Dispatcher.InvokeAsync(() => {
                            ShowJoinConfDisplayNameWindow();
                        });
                    }
                    else
                    {
                        LoginManager.Instance.LoginByToken(
                                         Helpers.Utils.GetAnonymousJoinConfServerAddress()
                                        , Helpers.Utils.GetAnonymousJoinConfServerProtocol()
                                        , Helpers.Utils.GetAnonymousJoinConfServerPort()
                                        , anonymousJoinConfToken
                                    );
                    }
                }
            }
            else if ("IsNeedAnonymousJoinConf" == e.PropertyName)
            {
                log.InfoFormat("Need anonymous join conf:{0}", LoginManager.Instance.IsNeedAnonymousJoinConf);
                if (!LoginManager.Instance.IsNeedAnonymousJoinConf)
                {
                    return;
                }
                string anonymousJoinConfToken = Helpers.Utils.GetAnonymousJoinConfToken();
                if (string.IsNullOrEmpty(anonymousJoinConfToken))
                {
                    Application.Current.Dispatcher.InvokeAsync(() => {
                        ShowJoinConfDisplayNameWindow();
                    });
                }
                else
                {
                    LoginManager.Instance.LoginByToken(
                                     Helpers.Utils.GetAnonymousJoinConfServerAddress()
                                    , Helpers.Utils.GetAnonymousJoinConfServerProtocol()
                                    , Helpers.Utils.GetAnonymousJoinConfServerPort()
                                    , anonymousJoinConfToken
                                );
                }
            }
        }

        private void ShowJoinConfDisplayNameWindow()
        {
            if (Visibility.Visible == JoinConfDisplayNameWindow.Instance.Visibility)
            {
                return;
            }
            //LoginManager.Instance.LoginWindow.Visibility = Visibility.Collapsed;
            JoinConfDisplayNameWindow.Instance.RefreshJoinConfData();
            JoinConfDisplayNameWindow.Instance.Owner = LoginManager.Instance.LoginWindow;
            JoinConfDisplayNameWindow.Instance.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            JoinConfDisplayNameWindow.Instance.ShowDialog();
        }
        
        #endregion
    }
}
