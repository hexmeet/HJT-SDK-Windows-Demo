using EasyVideoWin.CustomControls;
using EasyVideoWin.Helpers;
using EasyVideoWin.Model;
using EasyVideoWin.Model.CloudModel;
using EasyVideoWin.ViewModel;
using log4net;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace EasyVideoWin.View
{
    /// <summary>
    /// Interaction logic for SvcLoginJoinConfView.xaml
    /// </summary>
    public partial class AnonymousJoinConfView : UserControl
    {
        #region -- Members --

        private readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private AnonymousJoinConfViewModel _viewModel = new AnonymousJoinConfViewModel();
        private const string VALID_CONF_ID = "^(\\d)+$";
        private string _confPassword;

        #endregion

        #region -- Properties --

        #endregion

        #region -- Constructor --

        public AnonymousJoinConfView()
        {
            InitializeComponent();

            this._viewModel = this.DataContext as AnonymousJoinConfViewModel;
            
            this.Loaded += SvcLoginJoinConfView_Loaded;
        }
        
        #endregion

        #region -- Public Method --

        public void SetConfNumberPassword(string confNumber, string confPassword)
        {
            this.comboBoxConfId.Text = confNumber;
            _confPassword = confPassword;
            if (!string.IsNullOrEmpty(confNumber))
            {
                this.comboBoxConfId.IsEnabled = false;
            }
        }

        #endregion

        #region -- Private Method --

        private void SvcLoginJoinConfView_Loaded(object sender, RoutedEventArgs e)
        {
            RefreshControlValue();
            UpdateMoreButton();
            //TryAnonymousJoinConf();
            LoginManager.Instance.PropertyChanged += LoginManager_PropertyChanged;
        }

        private void RefreshControlValue()
        {
            if (LoginStatus.LoggedIn != LoginManager.Instance.CurrentLoginStatus && LoginProgressEnum.AdvancedSetting == LoginManager.Instance.PreviousLoginProgress)
            {
                // don't change the join conf dialog or login server values after set advanced setting
                return;
            }

            this.textBoxServerAddress.Text = Utils.GetServerAddress(LoginManager.Instance.ServiceType);
            this.textBoxNameDisplayedInConf.Text = Utils.GetDisplayNameInConf();

            List<string> confIds = Utils.GetConfIdsSetting();
            List<ConfIdInfo> listConfIdInfo = new List<ConfIdInfo>();
            for (int i = 0; i < confIds.Count; ++i)
            {
                listConfIdInfo.Add(new ConfIdInfo { ConfId = confIds[i] });
            }

            this.comboBoxConfId.ItemsSource = listConfIdInfo;
            if (string.IsNullOrEmpty(this.comboBoxConfId.Text) && listConfIdInfo.Count > 0)
            {
                this.comboBoxConfId.SelectedIndex = 0;
            }

            this.turnOffCamera.IsChecked = Utils.GetDisableCameraOnJoinConf();
            this.turnOffMicrophone.IsChecked = Utils.GetDisableMicOnJoinConf();

            log.InfoFormat("Update control values, conf id count:{0}, display name:{1}", confIds.Count, this.textBoxNameDisplayedInConf.Text);
        }

        private void Join_Click(object sender, RoutedEventArgs e)
        {
            log.Info("Join conference click");
            bool valid = true;
            string szServerAddress;
            if (LoginProgressEnum.EnterpriseJoinConf == LoginManager.Instance.LoginProgress)
            {
                szServerAddress = this.textBoxServerAddress.Text.Trim();
                if (string.IsNullOrEmpty(szServerAddress))
                {
                    this.textBoxServerAddress.Tag = LanguageUtil.Instance.GetValueByKey("SERVER");
                    valid = false;
                }
            }
            else
            {
                szServerAddress = Utils.CloudServerDomain;
            }

            string szConfId = this.comboBoxConfId.Text.Trim();
            if (string.IsNullOrEmpty(szConfId))
            {
                valid = false;
            }

            string szNameDisplayedInConf = this.textBoxNameDisplayedInConf.Text.Trim();
            if (string.IsNullOrEmpty(szNameDisplayedInConf))
            {
                this.textBoxNameDisplayedInConf.Tag = LanguageUtil.Instance.GetValueByKey("NAME_DISPLAYED_IN_CONF");
            }
            
            if (!valid)
            {
                log.Info("Null value.");
                return;
            }

            Window window = Window.GetWindow(this);
            IMasterDisplayWindow ownerWindow = window as IMasterDisplayWindow;
            if (null == ownerWindow)
            {
                ownerWindow = (MainWindow)Application.Current.MainWindow;
            }

            if (!Regex.IsMatch(szConfId, VALID_CONF_ID))
            {
                log.Info("Invalid conf id.");
                MessageBoxTip tip = new MessageBoxTip(ownerWindow);
                tip.SetTitleAndMsg(
                    LanguageUtil.Instance.GetValueByKey("PROMPT")
                    , LanguageUtil.Instance.GetValueByKey("INVALID_CONFERENCE_ID")
                    , LanguageUtil.Instance.GetValueByKey("CONFIRM")
                );
                tip.ShowDialog();
                return;
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

            log.Info("Correct value and go ahead.");
            Utils.SetDisplayNameInConf(szNameDisplayedInConf);
            bool enableCamera = null == this.turnOffCamera.IsChecked || !this.turnOffCamera.IsChecked.Value;
            bool enableMicrophone = null == this.turnOffMicrophone.IsChecked || !this.turnOffMicrophone.IsChecked.Value;
            Utils.SetDisableCameraOnJoinConf(!enableCamera);
            Utils.SetDisableMicOnJoinConf(!enableMicrophone);

            // join the conference
            if (LoginStatus.LoggedIn == LoginManager.Instance.CurrentLoginStatus)
            {
                EVSdkManager.Instance.EnableCamera(enableCamera);
                EVSdkManager.Instance.EnableMic(enableMicrophone);
                if (string.IsNullOrEmpty(szNameDisplayedInConf))
                {
                    szNameDisplayedInConf = LoginManager.Instance.DisplayName;
                }
                log.Info("Join conf -- start video call.");
                this._viewModel.StartJoinConference(szConfId, szNameDisplayedInConf, _confPassword, ownerWindow);
            }
            else if (LoginStatus.NotLogin == LoginManager.Instance.CurrentLoginStatus || LoginStatus.LoginFailed == LoginManager.Instance.CurrentLoginStatus)
            {
                log.Info("Join conf -- anonymous.");
                LoginManager.Instance.AnonymousJoinConference(
                    false
                    , LoginManager.Instance.EnableSecure
                    , szServerAddress
                    , LoginManager.Instance.ServerPort
                    , szConfId
                    , ""
                    , enableCamera
                    , enableMicrophone
                );
            }
        }

        private void ServerAddress_LostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(this.textBoxServerAddress.Text.Trim()))
            {
                this.textBoxServerAddress.Tag = LanguageUtil.Instance.GetValueByKey("SERVER");
            }
        }
        
        private void NameDisplayedInConf_LostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(this.textBoxNameDisplayedInConf.Text.Trim()))
            {
                this.textBoxNameDisplayedInConf.Tag = LanguageUtil.Instance.GetValueByKey("NAME_DISPLAYED_IN_CONF");
            }
        }

        private void LoginManager_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if ("LoginProgress" == e.PropertyName)
            {
                if (LoginProgressEnum.EnterpriseJoinConf == LoginManager.Instance.LoginProgress)
                {
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        RefreshControlValue();
                        this.comboBoxConfId.Margin = new Thickness(0, 6, 0, 0);
                        UpdateMoreButton();
                    });
                }
                else if(LoginProgressEnum.CloudJoinConf == LoginManager.Instance.LoginProgress)
                {
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        RefreshControlValue();
                        this.comboBoxConfId.Margin = new Thickness(0, 40, 0, 0);
                        UpdateMoreButton();
                    });
                }
            }
            else if ("CurrentLoginStatus" == e.PropertyName)
            {
                Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    UpdateMoreButton();
                });
            }
            //else if ("IsNeedAnonymousJoinConf" == e.PropertyName)
            //{
            //    TryAnonymousJoinConf();
            //}
        }

        private void UpdateMoreButton()
        {
            this.moreDockPanel.Visibility = (LoginStatus.LoggedIn == LoginManager.Instance.CurrentLoginStatus || LoginProgressEnum.EnterpriseJoinConf != LoginManager.Instance.LoginProgress)
                                              ? Visibility.Collapsed : Visibility.Visible;
        }

        //private void TryAnonymousJoinConf()
        //{
        //    log.InfoFormat("Need anonymous join conf:{0}", LoginManager.Instance.IsNeedAnonymousJoinConf);
        //    if (!LoginManager.Instance.IsNeedAnonymousJoinConf)
        //    {
        //        return;
        //    }

        //    bool enableCamera = null == this.turnOffCamera.IsChecked || !this.turnOffCamera.IsChecked.Value;
        //    bool enableMicrophone = null == this.turnOffMicrophone.IsChecked || !this.turnOffMicrophone.IsChecked.Value;
        //    bool isJoinDirectly = "join" == Utils.GetAnonymousJoinConfType();
        //    string protocol = Utils.GetAnonymousJoinConfServerProtocol();
        //    string joinConfAddress = Utils.GetAnonymousJoinConfServerAddress();
        //    int port = Utils.GetAnonymousJoinConfServerPort();
        //    string confId = Utils.GetAnonymousJoinConfId();
        //    string confPassword = Utils.GetAnonymousJoinConfPassword();
        //    Utils.SetAnonymousJoinConfType("");
        //    Utils.SetAnonymousJoinConfServerProtocol("");
        //    Utils.SetAnonymousJoinConfServerAddress("");
        //    Utils.SetAnonymousJoinConfId("");
        //    Utils.SetAnonymousJoinConfPassword("");
        //    Utils.SetAnonymousJoinConfServerPort(0);
        //    Application.Current.Dispatcher.InvokeAsync(() => {
        //        LoginManager.Instance.AnonymousJoinConference(
        //            isJoinDirectly
        //            , ("https" == protocol.ToLower())
        //            , joinConfAddress
        //            , (uint)port
        //            , confId
        //            , confPassword
        //            , enableCamera
        //            , enableMicrophone
        //        );
        //    });
        //    LoginManager.Instance.IsNeedAnonymousJoinConf = false;
        //}

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            string confId = this.comboBoxConfId.Text;
            List<ConfIdInfo> confIdList = this.comboBoxConfId.ItemsSource as List<ConfIdInfo>;
            WpfImageButton closeButton = sender as WpfImageButton;
            confIdList.RemoveAll(u => u.ConfId == closeButton.Tag.ToString());

            
            this.comboBoxConfId.ItemsSource = null;
            this.comboBoxConfId.ItemsSource = confIdList;
            this.comboBoxConfId.Text = confId;
            List<string> list = new List<string>();
            for (int i=0; i<confIdList.Count; ++i)
            {
                list.Add(confIdList[i].ConfId);
            }
            Utils.SetConfIdsSetting(list);
        }

        private void More_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            LoginManager.Instance.LoginProgress = LoginProgressEnum.AdvancedSetting;
        }

        #endregion

    }

    public class ConfIdInfo
    {
        public string ConfId { get; set; }
    }
}
