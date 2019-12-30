using EasyVideoWin.CustomControls;
using EasyVideoWin.Helpers;
using EasyVideoWin.Model;
using EasyVideoWin.View;
using log4net;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace EasyVideoWin.ViewModel
{
    class VideoPeopleWindowViewModel : LayoutTitlebarWindowViewModel
    {
        #region -- Members --

        private readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private const int CONF_ID_HISTORY_MAX_COUNT = 5;
        private LoginManager _loginMgr = LoginManager.Instance;
        private UserControl _callingView;
        private UserControl _logoView;
        
        private UserControl _currentView;

        #endregion

        #region -- Properties --
        
        public UserControl CurrentView
        {
            get
            {
                return _currentView;
            }
            set
            {
                if (value != _currentView)
                {
                    _currentView = value;
                    OnPropertyChanged("CurrentView");
                }
            }
        }

        public bool IsVisible
        {
            get
            {
                bool flag = (
                                   CallStatus.Dialing == CallController.Instance.CurrentCallStatus
                                || CallStatus.ConfIncoming == CallController.Instance.CurrentCallStatus
                                || CallStatus.P2pIncoming == CallController.Instance.CurrentCallStatus
                                || CallStatus.P2pOutgoing == CallController.Instance.CurrentCallStatus
                                || CallStatus.Connected == CallController.Instance.CurrentCallStatus
                            );
                log.InfoFormat("Video people window visible changed, visible:{0}", flag);
                return flag;
            }
        }


        #endregion

        #region -- Constructor --



        public VideoPeopleWindowViewModel()
        {
            CallController.Instance.CallStatusChanged += OnCallStatusChanged;
            LoginManager.Instance.PropertyChanged += OnLoginMgrPropertyChanged;

            if (
                   LoginStatus.LoggedIn == LoginManager.Instance.CurrentLoginStatus
                || LoginStatus.AnonymousLoggedIn == LoginManager.Instance.CurrentLoginStatus
            )
            {
                _callingView = (UserControl)Activator.CreateInstance(typeof(EasyVideoWin.View.CallingView));
                _logoView = (UserControl)Activator.CreateInstance(typeof(EasyVideoWin.View.LogoView));
                
                CurrentView = _callingView;
            }
        }

        #endregion

        #region -- Public Method --

        #endregion

        #region -- Private Method --
        
        private void OnCallStatusChanged(object sender, CallStatus status)
        {
            log.Info("OnCallStatusChanged start.");
            OnPropertyChanged("IsVisible");
            if (CallStatus.Idle == status || CallStatus.Ended == status)
            {
                Utils.SetIsConfRunning(false);
                Utils.SetRunningConfId("");
            }
            else
            {
                Utils.SetIsConfRunning(true);
                Utils.SetRunningConfId(CallController.Instance.ConferenceNumber);
            }

            switch (status)
            {
                case CallStatus.Dialing:
                case CallStatus.ConfIncoming:
                case CallStatus.P2pIncoming:
                    CurrentView = _callingView;
                    Application.Current.Dispatcher.InvokeAsync(() =>
                    {
                        LayoutBackgroundWindow.Instance.SetLocalVideoHandle();
                    });
                    break;
                case CallStatus.Connected:
                case CallStatus.P2pOutgoing:
                    Application.Current.Dispatcher.InvokeAsync(() =>
                    {
                        CurrentView = _logoView;
                        LayoutBackgroundWindow.Instance.InitSetting();
                        LayoutBackgroundWindow.Instance.ShowWindow(true);
                    });

                    if (CallStatus.Connected == status)
                    {
                        SaveCurrentConfId();
                    }

                    break;
            }
            log.Info("OnCallStatusChanged end.");
        }

        private void SaveCurrentConfId()
        {
            if (CallController.Instance.IsP2pCall)
            {
                log.InfoFormat("P2p call, do not save conf number to history list: {0}", CallController.Instance.ConferenceNumber);
                return;
            }
            log.InfoFormat("Save conf number to history list: {0}", CallController.Instance.ConferenceNumber);
            List<string> confIds = Utils.GetConfIdsSetting();
            confIds.RemoveAll(u => u == CallController.Instance.ConferenceNumber);
            confIds.Insert(0, CallController.Instance.ConferenceNumber);
            while (confIds.Count > CONF_ID_HISTORY_MAX_COUNT)
            {
                confIds.RemoveAt(confIds.Count - 1);
            }
            Utils.SetConfIdsSetting(confIds);
        }

        private void OnLoginMgrPropertyChanged(object sender, PropertyChangedEventArgs arg)
        {
            if ("CurrentLoginStatus" == arg.PropertyName)
            {
                // To show the main window quickly(especially when network is not fluent), make the operation after loginedIn as below
                LoginStatus status = LoginManager.Instance.CurrentLoginStatus;
                log.DebugFormat("VideoPeopleWindowViewModel received login status changed message, login status:{0}", status);
                OnPropertyChanged("IsVisible");
                if (LoginStatus.LoggedIn == status || LoginStatus.AnonymousLoggedIn == status)
                {
                    App.Current.Dispatcher.InvokeAsync(() =>
                    {
                        if (_callingView == null)
                        {
                            _callingView = (UserControl)Activator.CreateInstance(typeof(EasyVideoWin.View.CallingView));
                        }

                        if (_logoView == null)
                        {
                            _logoView = (UserControl)Activator.CreateInstance(typeof(EasyVideoWin.View.LogoView));
                        }
                        
                        CurrentView = _callingView;
                    });
                }
            }
        }
        
        #endregion


    }
}
