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
    class LoginInfoViewModel : ViewModelBase
    {
        #region -- Members --

        #endregion

        #region -- Properties --

        public Visibility ServerVisibility
        {
            get
            {
                return LoginProgressEnum.EnterpriseLogin == LoginManager.Instance.LoginProgress ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        public Visibility RegisterVisibility
        {
            get
            {
                // do not show register info currently
                //return LoginProgressEnum.CloudLogin == LoginManager.Instance.LoginProgress ? Visibility.Visible : Visibility.Collapsed;
                return Visibility.Collapsed;
            }
        }

        #endregion

        #region -- Constructor --

        public LoginInfoViewModel()
        {
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
                switch (LoginManager.Instance.LoginProgress)
                {
                    case LoginProgressEnum.EnterpriseLogin:
                    case LoginProgressEnum.CloudLogin:
                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            OnPropertyChanged("ServerVisibility");
                            OnPropertyChanged("RegisterVisibility");
                        });
                        break;
                    default:
                        break;
                }

            }
        }

        #endregion
    }
}
