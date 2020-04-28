using EasyVideoWin.CustomControls;
using EasyVideoWin.Helpers;
using EasyVideoWin.Model;
using log4net;
using System;
using System.Collections.Generic;
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
    /// Interaction logic for UrlJoinConfDisplayNameWindow.xaml
    /// </summary>
    public partial class JoinConfDisplayNameWindow : Window, IMasterDisplayWindow
    {
        #region -- Members --

        private readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private static JoinConfDisplayNameWindow _instance = null;

        private bool        _isJoinDirectly;
        private string      _joinConfProtocol;
        private string      _joinConfAddress;
        private int         _joinConfPort;
        private string      _joinConfId;
        private string      _joinConfPassword;
        private IntPtr      _handle;

        #endregion

        #region -- Properties --

        public static JoinConfDisplayNameWindow Instance
        {
            get
            {
                if (null == _instance)
                {
                    _instance = new JoinConfDisplayNameWindow();
                }

                return _instance;
            }
        }

        #endregion

        #region -- Constructor --

        public JoinConfDisplayNameWindow()
        {
            InitializeComponent();
        }

        #endregion

        #region -- Public Method --
        
        public void RefreshJoinConfData()
        {
            _isJoinDirectly = "join" == Utils.GetAnonymousJoinConfType();
            _joinConfProtocol = Utils.GetAnonymousJoinConfServerProtocol();
            _joinConfAddress = Utils.GetAnonymousJoinConfServerAddress();
            _joinConfPort = Utils.GetAnonymousJoinConfServerPort();
            _joinConfId = Utils.GetAnonymousJoinConfId();
            _joinConfPassword = Utils.GetAnonymousJoinConfPassword();
            this.textBlockJoiningMeeting.Text = string.Format(LanguageUtil.Instance.GetValueByKey("JOINING_MEETING"), _joinConfId);
            this.textBoxDisplayName.Text = Utils.GetDisplayNameInConf();
            this.turnOffCamera.IsChecked = Utils.GetDisableCameraOnJoinConf();
            this.turnOffMicrophone.IsChecked = Utils.GetDisableMicOnJoinConf();
        }

        public void CloseWindow()
        {
            Application.Current.Dispatcher.InvokeAsync(() => {
                this.Visibility = Visibility.Collapsed;
            });
        }

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
            ClearJoinConfData();
            this.Visibility = Visibility.Collapsed;
        }

        private void Join_Click(object sender, RoutedEventArgs e)
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
                log.InfoFormat("The lenght of name exceeds {0}.", Utils.DISPLAY_NAME_MAX_LENGTH);
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

            Utils.SetDisplayNameInConf(szNameDisplayedInConf);
            bool enableCamera = null == this.turnOffCamera.IsChecked || !this.turnOffCamera.IsChecked.Value;
            bool enableMicrophone = null == this.turnOffMicrophone.IsChecked || !this.turnOffMicrophone.IsChecked.Value;
            Utils.SetDisableCameraOnJoinConf(!enableCamera);
            Utils.SetDisableMicOnJoinConf(!enableMicrophone);
            Application.Current.Dispatcher.InvokeAsync(() => {
                LoginManager.Instance.AnonymousJoinConference(
                    _isJoinDirectly
                    , ("https" == _joinConfProtocol.ToLower())
                    , _joinConfAddress
                    , (uint)_joinConfPort
                    , _joinConfId
                    , _joinConfPassword
                    , enableCamera
                    , enableMicrophone
                );
            });
            ClearJoinConfData();
        }

        private void ClearJoinConfData()
        {
            Utils.SetAnonymousJoinConfType("");
            Utils.SetAnonymousJoinConfServerProtocol("");
            log.Info("SetAnonymousJoinConfServerAddress empty");
            Utils.SetAnonymousJoinConfServerAddress("");
            Utils.SetAnonymousJoinConfId("");
            Utils.SetAnonymousJoinConfPassword("");
            Utils.SetAnonymousJoinConfServerPort(0);
            LoginManager.Instance.IsNeedAnonymousJoinConf = false;
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
            return 1;
        }

        #endregion

    }
}
