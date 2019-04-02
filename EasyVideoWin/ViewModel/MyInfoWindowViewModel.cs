using EasyVideoWin.Helpers;
using EasyVideoWin.ManagedEVSdk.Structs;
using EasyVideoWin.Model;
using log4net;
using System.Text;
using System.Windows.Media.Imaging;

namespace EasyVideoWin.ViewModel
{
    class MyInfoWindowViewModel : ViewModelBase
    {
        #region -- Members --

        private readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private string _userDisplayName;
        private string _cellphone;
        private string _telephone;
        private string _email;
        private string _department;
        private string _organization;

        #endregion

        #region -- Properties --
        
        public WriteableBitmap AvatarBmp
        {
            get
            {
                return LoginManager.Instance.AvatarBmp;
            }
        }

        public string UserDisplayName
        {
            get
            {
                return _userDisplayName;
            }
            set
            {
                _userDisplayName = value;
                OnPropertyChanged("UserDisplayName");
            }
        }

        public string Cellphone
        {
            get
            {
                return _cellphone;
            }
            set
            {
                _cellphone = value;
                OnPropertyChanged("Cellphone");
            }
        }

        public string Telephone
        {
            get
            {
                return _telephone;
            }
            set
            {
                _telephone = value;
                OnPropertyChanged("Telephone");
            }
        }

        public string Email
        {
            get
            {
                return _email;
            }
            set
            {
                _email = value;
                OnPropertyChanged("Email");
            }
        }

        public string Department
        {
            get
            {
                return _department;
            }
            set
            {
                _department = value;
                OnPropertyChanged("Department");
            }
        }

        public string Organization
        {
            get
            {
                return _organization;
            }
            set
            {
                _organization = value;
                OnPropertyChanged("Organization");
            }
        }
        #endregion

        #region -- Constructor --

        public MyInfoWindowViewModel()
        {
            LoginManager.Instance.PropertyChanged += LoginManager_PropertyChanged;
        }

        #endregion

        #region -- Public Method --

        public void RefreshData()
        {
            EVUserInfoCli userInfo = new EVUserInfoCli();
            EVSdkManager.Instance.GetUserInfo(ref userInfo);
            if (null == userInfo)
            {
                log.Info("Failed to get user info.");
                return;
            }
            UserDisplayName = userInfo.displayName;
            Cellphone = userInfo.cellphone;
            Telephone = userInfo.telephone;
            Email = userInfo.email;
            Department = userInfo.dept;
            Organization = userInfo.org;
        }

        #endregion

        #region -- Private Method --

        private void LoginManager_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if ("AvatarBmp" == e.PropertyName)
            {
                OnPropertyChanged("AvatarBmp");
            }
        }

        #endregion
    }
}
