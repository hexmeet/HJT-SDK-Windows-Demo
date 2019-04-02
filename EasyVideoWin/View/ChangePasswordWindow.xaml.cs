using EasyVideoWin.CustomControls;
using EasyVideoWin.Helpers;
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
    /// Interaction logic for ChangePasswordWindow.xaml
    /// </summary>
    public partial class ChangePasswordWindow : Window, IMasterDisplayWindow
    {
        #region -- Members --

        private readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private static ChangePasswordWindow _instance = new ChangePasswordWindow();
        private const string INVALID_NAME_REGEX = "[\"<>]+";
        private IntPtr _handle = IntPtr.Zero;

        #endregion

        #region -- Properties 

        public static ChangePasswordWindow Instance
        {
            get
            {
                return _instance;
            }
        }

        #endregion

        #region -- Constructor --

        public ChangePasswordWindow()
        {
            InitializeComponent();

            this.IsVisibleChanged += ChangePasswordWindow_IsVisibleChanged;
        }
        
        #endregion

        #region -- Public Method --

        #endregion

        #region -- Protected Method --



        #endregion

        #region -- Private Method --

        private void CloseBtn_Click(object sender, RoutedEventArgs e)
        {
            this.Visibility = Visibility.Collapsed;
        }

        private void ChangePasswordWindow_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (Visibility.Visible == this.Visibility)
            {
                ClearPasswordContent();
            }
        }

        private void ClearPasswordContent()
        {
            this.oldPasswordBox.Password = "";
            this.newPassowrdBox.Password = "";
            this.confirmPassowrdBox.Password = "";
        }

        private void ConfirmBtn_Click(object sender, RoutedEventArgs e)
        {
            string oldPwd = this.oldPasswordBox.Password.Trim();
            string newPwd1 = this.newPassowrdBox.Password.Trim();
            string newPwd2 = this.confirmPassowrdBox.Password.Trim();

            if (oldPwd == "" || oldPwd.Length < 4 || oldPwd.Length > 16)
            {
                MessageBoxTip tip = new MessageBoxTip(this);
                tip.SetTitleAndMsg(LanguageUtil.Instance.GetValueByKey("PROMPT"), LanguageUtil.Instance.GetValueByKey("CHECK_INPUT_OLD_PASSWORD"), LanguageUtil.Instance.GetValueByKey("CONFIRM"));
                tip.ShowDialog();
                return;
            }

            if (newPwd1 == "" || newPwd1.Length < 4 || newPwd1.Length > 16)
            {
                MessageBoxTip tip = new MessageBoxTip(this);
                tip.SetTitleAndMsg(LanguageUtil.Instance.GetValueByKey("PROMPT"), LanguageUtil.Instance.GetValueByKey("CHECK_INPUT_NEW_PASSWORD"), LanguageUtil.Instance.GetValueByKey("CONFIRM"));
                tip.ShowDialog();
                return;
            }
            if (newPwd2 == "" || newPwd2.Length < 4 || newPwd2.Length > 16)
            {
                MessageBoxTip tip = new MessageBoxTip(this);
                tip.SetTitleAndMsg(LanguageUtil.Instance.GetValueByKey("PROMPT"), LanguageUtil.Instance.GetValueByKey("CHECK_INPUT_CONFIRM_PASSWORD"), LanguageUtil.Instance.GetValueByKey("CONFIRM"));
                tip.ShowDialog();
                return;
            }

            if (newPwd1 != newPwd2)
            {
                MessageBoxTip tip = new MessageBoxTip(this);
                tip.SetTitleAndMsg(LanguageUtil.Instance.GetValueByKey("PROMPT"), LanguageUtil.Instance.GetValueByKey("NEW_PASSWORD_DIFFERENT"), LanguageUtil.Instance.GetValueByKey("CONFIRM"));
                tip.ShowDialog();
                return;
            }

            if (oldPwd == newPwd2)
            {
                MessageBoxTip tip = new MessageBoxTip(this);
                tip.SetTitleAndMsg(LanguageUtil.Instance.GetValueByKey("PROMPT"), LanguageUtil.Instance.GetValueByKey("THE_SAME_NEW_AND_OLD_PASSWORD"), LanguageUtil.Instance.GetValueByKey("CONFIRM"));
                tip.ShowDialog();
                return;
            }
            
            Task.Run(() => {
                bool result = EVSdkManager.Instance.ChangePassword(oldPwd, newPwd1);
                if (result)
                {
                    Application.Current.Dispatcher.InvokeAsync(() => {
                        log.Info("Succeed to change password.");
                        Utils.SetPassword(LoginManager.Instance.ServiceType, newPwd1);
                        LoginManager.Instance.LoginPassword = newPwd1;

                        MessageBoxTip tip = new MessageBoxTip(this);
                        tip.SetTitleAndMsg(LanguageUtil.Instance.GetValueByKey("PROMPT"), LanguageUtil.Instance.GetValueByKey("CHANGE_PASSWORD_SUCCESS"), LanguageUtil.Instance.GetValueByKey("CONFIRM"));
                        tip.ShowDialog();
                        ClearPasswordContent();
                        CloseBtn_Click(null, null);
                    });
                }
                else
                {
                    log.Info("Failed to change password.");
                }
            })
            .ContinueWith((t) => {
                Application.Current.Dispatcher.InvokeAsync(() => {
                    ProgressDialog.Instance.Hide();
                });
            });
            
            ProgressDialog.Instance.ShowDialog();
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
