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
    class AnonymousJoinConfViewModel : DialOutModelBase
    {
        #region -- Members --

        #endregion

        #region -- Properties --

        public Visibility ServerVisibility
        {
            get
            {
                return LoginProgressEnum.EnterpriseJoinConf == LoginManager.Instance.LoginProgress ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        #endregion

        #region -- Constructor --

        public AnonymousJoinConfViewModel()
        {
            LoginManager.Instance.PropertyChanged += LoginManager_PropertyChanged;
        }

        #endregion

        #region -- Public Method --

        public void StartJoinConference(string confNumber, string displayName, string confPassword, Helpers.IMasterDisplayWindow ownerWindow)
        {
            JoinConference(confNumber.Trim(), displayName, confPassword, ownerWindow);
        }

        #endregion

        #region -- Private Method --

        private void LoginManager_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if ("LoginProgress" == e.PropertyName)
            {
                switch (LoginManager.Instance.LoginProgress)
                {
                    case LoginProgressEnum.EnterpriseJoinConf:
                    case LoginProgressEnum.CloudJoinConf:
                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            OnPropertyChanged("ServerVisibility");
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
