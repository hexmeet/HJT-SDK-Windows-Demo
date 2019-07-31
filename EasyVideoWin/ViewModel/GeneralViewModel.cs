using EasyVideoWin.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EasyVideoWin.View;
using EasyVideoWin.Model;
using EasyVideoWin.CustomControls;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.ComponentModel;
using System.Collections.ObjectModel;

namespace EasyVideoWin.ViewModel
{
    class GeneralViewModel : ViewModelBase
    {
        #region -- Members --

        private ObservableCollection<CallRateItem> _callRateSource = null;
        private CallRateItem _selectedCallRate;
        
        private ObservableCollection<LanguageItem> _languageSource = null;
        private LanguageItem _selectedLanguage;

        #endregion
        
        #region -- Properties --

        public RelayCommand DiagnosticCommand { get; set; }
        
        public ObservableCollection<CallRateItem> CallRateSource
        {
            get
            {
                if (null == _callRateSource)
                {
                    this._callRateSource = new ObservableCollection<CallRateItem>()
                    {
                        new CallRateItem() { Key = 384, Text = "384K" }
                        , new CallRateItem() { Key = 512, Text = "512K" }
                        , new CallRateItem() { Key = 768, Text = "768K" }
                        , new CallRateItem() { Key = 1024, Text = "1M" }
                        , new CallRateItem() { Key = 1536, Text = "1.5M" }
                        , new CallRateItem() { Key = 2048, Text = "2M" }
                    };
                }

                return _callRateSource;
            }
        }

        public CallRateItem SelectedCallRate
        {
            get
            {
                return _selectedCallRate;
            }
            set
            {
                if (_selectedCallRate != value)
                {
                    SettingManager.Instance.SetCallRate(value.Key);
                    _selectedCallRate = value;
                    OnPropertyChanged("SelectedCallRate");
                }
            }
        }
        
        public ObservableCollection<LanguageItem> LanguageSource
        {
            get
            {
                if (null == _languageSource)
                {
                    this._languageSource = new ObservableCollection<LanguageItem>()
                    {
                        new LanguageItem() { Key = LanguageType.EN_US, Text = LanguageUtil.Instance.GetValueByKey("COMBO_LANGUAGE_EN") }
                        , new LanguageItem() { Key = LanguageType.ZH_CN, Text = LanguageUtil.Instance.GetValueByKey("COMBO_LANGUAGE_CH") }
                    };
                }

                return _languageSource;
            }
        }

        public LanguageItem SelectedLanguage
        {
            get
            {
                return _selectedLanguage;
            }
            set
            {
                if (_selectedLanguage != value)
                {
                    LanguageUtil.Instance.UpdateLanguage(value.Key);
                    _selectedLanguage = value;
                    OnPropertyChanged("SelectedLanguage");
                }
            }
        }


        public string Account
        {
            get
            {
                return LoginManager.Instance.DisplayName;
            }
        }

        public bool FullScreenAfterStartup
        {
            get
            {
                return Helpers.Utils.GetFullScreenAfterStartup();
            }
            set
            {
                Helpers.Utils.SetFullScreenAfterStartup(value);
            }
        }

        public bool AutoStartup
        {
            get
            {
                return Helpers.Utils.GetAutoStartup();
            }
            set
            {
                Helpers.Utils.SetAutoStartup(value);
            }
        }

        public bool AutoAnswer
        {
            get
            {
                return Helpers.Utils.GetAutoAnswer();
            }
            set
            {
                Helpers.Utils.SetAutoAnswer(value);
            }
        }

        public bool AutoLogin
        {
            get
            {
                return Helpers.Utils.IsAutoLogin();
            }
            set
            {
                Helpers.Utils.SetAutoLogin(value);
            }
        }

        public string ScreenPciPath
        {
            get
            {
                return Helpers.Utils.GetScreenPicPath();
            }
            set
            {
                Helpers.Utils.SetScreenPicPath(value);
            }
        }

        public bool AutoCapture
        {
            get
            {
                return Helpers.Utils.GetAutoCaptureWin();
            }
            set
            {
                Helpers.Utils.SetAutoCaptureWin(value);
            }
        }
        
        public bool OpenHighFrameRateVideo
        {
            get
            {
                return Helpers.Utils.GetOpenHighFrameRateVideo();
            }
            set
            {
                
                Helpers.Utils.SetOpenHighFrameRateVideo(value);
                // Let SDK know the preferred fps.

                CallController.Instance.SetPeopleHighFrameRate(value);
            }
        }

        public bool AutoHidePartyName
        {
            get
            {
                return Helpers.Utils.GetAutoHidePartyName();
            }
            set
            {

                Helpers.Utils.SetAutoHidePartyName(value);
            }
        }

        public bool Enable4x4Layout
        {
            get
            {
                return Helpers.Utils.GetEnable4x4Layout();
            }
            set
            {
                CallController.Instance.SetMaxRecvVideo(value ? (uint)MaxRecvVideoLayout.Layout_4x4 : (uint)MaxRecvVideoLayout.Layout_3x3);
                Helpers.Utils.SetEnable4x4Layout(value);
            }
        }

        public bool DisablePrompt
        {
            get
            {
                return Helpers.Utils.GetDisablePrompt();
            }
            set
            {
                Helpers.Utils.SetDisablePrompt(value);
            }
        }

        public bool Support1080P
        {
            get
            {
                return EVSdkManager.Instance.HDEnabled();
            }
            set
            {
                EVSdkManager.Instance.EnableHD(value);
            }
        }

        #endregion

        #region -- Constructor --

        public GeneralViewModel()
        {
            DiagnosticCommand = new RelayCommand(Diagnostic);
        
            LoginManager.Instance.PropertyChanged += OnLoginManagerPropertyChanged;
        }

        #endregion

        #region -- Public Methods --

        public void InitData()
        {
            uint callRate = SettingManager.Instance.GetCurrentCallRate();
            for (var i=0; i<CallRateSource.Count; ++i)
            {
                if (CallRateSource.ElementAt(i).Key == callRate)
                {
                    SelectedCallRate = CallRateSource.ElementAt(i);
                    break;
                }
            }
            
            LanguageType curLanguage = LanguageUtil.Instance.CurrentLanguage;
            for (var i = 0; i < LanguageSource.Count; ++i)
            {
                if (LanguageSource.ElementAt(i).Key == curLanguage)
                {
                    SelectedLanguage = LanguageSource.ElementAt(i);
                    break;
                }
            }

            OnPropertyChanged("ScreenPciPath");
            OnPropertyChanged("AutoLogin");
            OnPropertyChanged("AutoAnswer");
            OnPropertyChanged("AutoCapture");
            OnPropertyChanged("OpenHighFrameRateVideo");
            OnPropertyChanged("AutoHidePartyName");
            OnPropertyChanged("Enable4x4Layout");
            OnPropertyChanged("DisablePrompt");
        }
        
        #endregion

        #region -- Private Methods --
        
        private void OnLoginManagerPropertyChanged(object sender, PropertyChangedEventArgs args)
        {
            if ("DisplayName" == args.PropertyName)
            {
                //Account = LoginManager.Instance.Name;
                OnPropertyChanged("Account");
            }
        }
        
        private void Diagnostic(object parameter)
        {
            System.Windows.Controls.UserControl ctrl = parameter as System.Windows.Controls.UserControl;
            IMasterDisplayWindow displayWin = Window.GetWindow(ctrl) as IMasterDisplayWindow;
            SettingManager.Instance.Diagnostic(displayWin);
        }
        
        #endregion
    }

    public class CallRateItem
    {
        public uint Key { get; set; }
        public string Text { get; set; }
    }

    public class LanguageItem
    {
        public LanguageType Key { get; set; }
        public string Text { get; set; }
    }
}
