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
    class LoginTypeViewModel : ViewModelBase
    {
        #region -- Members --

        #endregion

        #region -- Properties --

        public RelayCommand EnterpriseUserCommand { get; set; }
        public RelayCommand CloudUserCommand { get; set; }

        #endregion

        #region -- Constructor --

        public LoginTypeViewModel()
        {
            EnterpriseUserCommand = new RelayCommand(EnterpriseUser);
            CloudUserCommand = new RelayCommand(CloudUser);
        }

        #endregion

        #region -- Public Method --

        #endregion

        #region -- Private Method --

        private void EnterpriseUser(object parameter)
        {
            LoginManager.Instance.LoginProgress = LoginProgressEnum.EnterpriseOptions;
            LoginManager.Instance.ServiceType = Utils.ServiceTypeEnum.Enterprise;
        }

        private void CloudUser(object parameter)
        {
            LoginManager.Instance.LoginProgress = LoginProgressEnum.CloudOptions;
            LoginManager.Instance.ServiceType = Utils.ServiceTypeEnum.Cloud;
        }

        #endregion
    }
}
