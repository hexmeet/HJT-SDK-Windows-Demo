using EasyVideoWin.CustomControls;
using EasyVideoWin.Helpers;
using EasyVideoWin.ManagedEVSdk.Structs;
using EasyVideoWin.Model;
using EasyVideoWin.Model.CloudModel;
using EasyVideoWin.Model.CloudModel.CloudRest;
using EasyVideoWin.ViewModel;
using log4net;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace EasyVideoWin.View
{
    /// <summary>
    /// Interaction logic for MyInfoWindow.xaml
    /// </summary>
    public partial class MyInfoWindow : Window, IMasterDisplayWindow
    {
        #region -- Members --

        private readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private static MyInfoWindow _instance = new MyInfoWindow();
        private MessageBoxConfirm _logoutPromptMessageBox = null;
        private IntPtr _handle = IntPtr.Zero;
        MyInfoWindowViewModel _viewModel;

        #endregion

        #region -- Properties 

        public static MyInfoWindow Instance
        {
            get
            {
                return _instance;
            }
        }

        #endregion

        #region -- Constructor --

        public MyInfoWindow()
        {
            InitializeComponent();

            _viewModel = this.DataContext as MyInfoWindowViewModel;
            this.IsVisibleChanged += MyInfoWindow_IsVisibleChanged;
            EVSdkManager.Instance.EventError += EVSdkWrapper_EventError;
        }

        #endregion

        #region -- Public Method --

        #endregion

        #region -- Protected Method --

        protected override void OnClosing(CancelEventArgs e)
        {
            EVSdkManager.Instance.EventError -= EVSdkWrapper_EventError;
            base.OnClosing(e);
        }

        #endregion

        #region -- Private Method --

        private void CloseBtn_Click(object sender, RoutedEventArgs e)
        {
            this.Visibility = Visibility.Collapsed;
        }

        private void MyInfoWindow_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (Visibility.Visible == this.Visibility)
            {
                this.displayNameReadOnlyPanel.Visibility = Visibility.Visible;
                this.displayNameEditPanel.Visibility = Visibility.Collapsed;
                _viewModel.RefreshData();
            }
            else
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    if (null != _logoutPromptMessageBox)
                    {
                        _logoutPromptMessageBox.Close();
                        _logoutPromptMessageBox = null;
                    }
                });
            }
        }

        private void Avatar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            PopUpHeaderSelectWindow uploadWin = new PopUpHeaderSelectWindow(this);
            bool? success = uploadWin.ShowDialog();

            if (!success.Value)
            {
                return;
            }

            LoginManager.Instance.DownloadLoginUserAvatar();
        }

        private void ChangePassword_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            ChangePasswordWindow.Instance.Owner = this;
            ChangePasswordWindow.Instance.ShowDialog();
        }

        private void Logout_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (null == _logoutPromptMessageBox)
            {
                _logoutPromptMessageBox = new MessageBoxConfirm(this);
            }

            _logoutPromptMessageBox.SetTitleAndMsg(LanguageUtil.Instance.GetValueByKey("PROMPT"), LanguageUtil.Instance.GetValueByKey("ARE_YOU_SURE_TO_LOGOUT_APP"));
            _logoutPromptMessageBox.ConfirmEvent += new EventHandler(delegate (object sender1, EventArgs e1) {
                log.Info("Confirm to logout");
                LoginManager.Instance.Logout();
                Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    if (null != _logoutPromptMessageBox)
                    {
                        log.Info("Close logout prompt window.");
                        _logoutPromptMessageBox.Close();
                        _logoutPromptMessageBox = null;
                    }
                });
            });
            _logoutPromptMessageBox.ShowDialog();
            _logoutPromptMessageBox = null;
        }

        private void EditDisplayName_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.displayNameReadOnlyPanel.Visibility = Visibility.Collapsed;
            this.displayNameEditPanel.Visibility = Visibility.Visible;
        }

        private void SaveDisplayName_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            string newName = displayNameTextBox.Text.Trim();
            log.InfoFormat("Prepare to change display name to {0}", newName);
            if (string.IsNullOrEmpty(newName))
            {
                MessageBoxTip tip = new MessageBoxTip(this);
                tip.SetTitleAndMsg(LanguageUtil.Instance.GetValueByKey("PROMPT"), LanguageUtil.Instance.GetValueByKey("EMPTY_NAME"), LanguageUtil.Instance.GetValueByKey("CONFIRM"));
                tip.ShowDialog();
                return;
            }
            else
            {
                if (Regex.IsMatch(newName, Utils.INVALID_DISPLAY_NAME_REGEX))
                {
                    MessageBoxTip tip = new MessageBoxTip(this);
                    tip.SetTitleAndMsg(LanguageUtil.Instance.GetValueByKey("PROMPT"), LanguageUtil.Instance.GetValueByKey("UPDATE_USER_NAME"), LanguageUtil.Instance.GetValueByKey("CONFIRM"));
                    tip.ShowDialog();
                    return;
                }
            }

            if (newName.Length > 20)
            {
                MessageBoxTip tip = new MessageBoxTip(this);
                tip.SetTitleAndMsg(
                    LanguageUtil.Instance.GetValueByKey("PROMPT")
                    , LanguageUtil.Instance.GetValueByKey("NAME_MAX_LENGTH")
                    , LanguageUtil.Instance.GetValueByKey("CONFIRM")
                );
                tip.ShowDialog();
                return;
            }

            Task.Run(() => {
                bool result = EVSdkManager.Instance.ChangeDisplayName(newName);
                if (result)
                {
                    Application.Current.Dispatcher.InvokeAsync(() => {
                        UpdateUserDisplayName();
                        
                        this.displayNameReadOnlyPanel.Visibility = Visibility.Visible;
                        this.displayNameEditPanel.Visibility = Visibility.Collapsed;
                    });
                }
            })
            .ContinueWith((t) => {
                Application.Current.Dispatcher.InvokeAsync(() => {
                    ProgressDialog.Instance.Hide();
                });
            });

            ProgressDialog.Instance.ShowDialog();
        }

        private void EVSdkWrapper_EventError(ManagedEVSdk.Structs.EVErrorCli err)
        {
            log.Info("EventError");
            if (EVSdkManager.ACTION_CHANGEDISPLAYNAME == err.action)
            {
                UpdateUserDisplayName();
            }
            
            log.Info("EventError end");
        }

        private void UpdateUserDisplayName()
        {
            EVUserInfoCli userInfo = new EVUserInfoCli();
            EVSdkManager.Instance.GetUserInfo(ref userInfo);
            if (null == userInfo)
            {
                log.Info("Failed to get user info. Do not update display name.");
                return;
            }
            _viewModel.UserDisplayName = userInfo.displayName;
            LoginManager.Instance.DisplayName = userInfo.displayName;
        }

        #endregion

        #region -- IMessageBoxOwner --
        public double GetWidth()
        {
            return this.Width;
        }

        public double GetHeight()
        {
            return this.Height;
        }

        public double GetLeft()
        {
            return this.Left;
        }

        public double GetTop()
        {
            return this.Top;
        }

        public double GetSizeRatio()
        {
            return 1; // this.Width / _initialWidth;
        }

        public Window GetWindow()
        {
            return this;
        }

        public IntPtr GetHandle()
        {
            if (IntPtr.Zero == _handle)
            {
                _handle = new WindowInteropHelper(this).Handle;
            }
            return _handle;
        }

        public Rect GetWindowRect()
        {
            return new Rect(this.Left, this.Top, this.Width, this.Height);
        }

        public double GetInitialWidth()
        {
            return this.Width;
        }

        public double GetInitialHeight()
        {
            return this.Height;
        }

        #endregion
    }
}
