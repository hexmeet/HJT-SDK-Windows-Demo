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
    /// Interaction logic for ChangeDisplayNameWindow.xaml
    /// </summary>
    public partial class ChangeDisplayNameWindow : Window, IMasterDisplayWindow
    {
        #region -- Members --

        private readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private IntPtr _handle = IntPtr.Zero;
        public event EventHandler CloseEvent;
        public event Action<string> ConfirmEvent;

        #endregion

        #region -- Properties 

        
        #endregion

        #region -- Constructor --

        public ChangeDisplayNameWindow()
        {
            InitializeComponent();

            this.IsVisibleChanged += ChangeDisplayNameWindow_IsVisibleChanged;
            this.textBoxDisplayName.Text = EVSdkManager.Instance.GetDisplayName();
        }

        #endregion

        #region -- Public Method --

        #endregion

        #region -- Protected Method --



        #endregion

        #region -- Private Method --

        private void NameDisplayedInConf_LostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(this.textBoxDisplayName.Text.Trim()))
            {
                this.textBoxDisplayName.Tag = LanguageUtil.Instance.GetValueByKey("NAME_DISPLAYED_IN_CONF");
            }
        }

        private void CloseBtn_Click(object sender, RoutedEventArgs e)
        {
            CloseEvent?.Invoke(this, new EventArgs());
            this.Close();
        }
        
        private void ConfirmBtn_Click(object sender, RoutedEventArgs e)
        {
            string szNameDisplayedInConf = this.textBoxDisplayName.Text.Trim();
            if (string.IsNullOrEmpty(szNameDisplayedInConf))
            {
                this.textBoxDisplayName.Tag = LanguageUtil.Instance.GetValueByKey("NAME_DISPLAYED_IN_CONF");
            }

            Window window = Window.GetWindow(this);
            IMasterDisplayWindow ownerWindow = window as IMasterDisplayWindow;
            if (null == ownerWindow)
            {
                ownerWindow = (MainWindow)Application.Current.MainWindow;
            }

            if (szNameDisplayedInConf.Length > Utils.DISPLAY_NAME_MAX_LENGTH)
            {
                log.InfoFormat("The length of name exceeds {0}.", Utils.DISPLAY_NAME_MAX_LENGTH);
                MessageBoxTip tip = new MessageBoxTip(ownerWindow);
                tip.SetTitleAndMsg(
                    LanguageUtil.Instance.GetValueByKey("PROMPT")
                    , string.Format(LanguageUtil.Instance.GetValueByKey("NAME_DISPLAYED_IN_CONF_MAX_LENGTH"), Utils.DISPLAY_NAME_MAX_LENGTH)
                    , LanguageUtil.Instance.GetValueByKey("CONFIRM")
                );
                tip.ShowDialog();
                return;
            }

            if (Regex.IsMatch(szNameDisplayedInConf, Utils.INVALID_DISPLAY_NAME_REGEX))
            {
                log.Info("Invalid char in name.");
                MessageBoxTip tip = new MessageBoxTip(ownerWindow);
                tip.SetTitleAndMsg(
                    LanguageUtil.Instance.GetValueByKey("PROMPT")
                    , LanguageUtil.Instance.GetValueByKey("NAME_DISPLAYED_IN_CONF_INCORRECT_PROMPT")
                    , LanguageUtil.Instance.GetValueByKey("CONFIRM")
                );
                tip.ShowDialog();
                return;
            }

            ConfirmEvent?.Invoke(szNameDisplayedInConf);
        }

        private void ChangeDisplayNameWindow_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (Visibility.Visible == this.Visibility)
            {
                this.textBoxDisplayName.Focus();
            }
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
