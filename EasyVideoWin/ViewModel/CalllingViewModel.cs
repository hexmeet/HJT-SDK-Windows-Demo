using EasyVideoWin.CustomControls;
using EasyVideoWin.Helpers;
using EasyVideoWin.Model;
using EasyVideoWin.View;
using log4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;


namespace EasyVideoWin.ViewModel
{
    class CalllingViewModel : ViewModelBase
    {
        #region -- Members --

        private readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private UserControl _dialingView;
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
                if (_currentView != value)
                {
                    _currentView = value;
                    OnPropertyChanged("CurrentView");
                }
            }
        }

        #endregion

        #region -- Constructor --
        public CalllingViewModel()
        {
            //_playerView = (UserControl)Activator.CreateInstance(typeof(EasyVideoWin.View.PlayerView));
            //_playerView = VideoView.Instance;
            _dialingView = (UserControl)Activator.CreateInstance(typeof(EasyVideoWin.View.DialingView));

            
            CallController.Instance.CallStatusChanged += OnCallStatusChanged;
        }

        #endregion

        #region -- Public Methods --


        #endregion

        #region -- Private Methods --

        private void OnCallStatusChanged(object sender, CallStatus status)
        {
            log.InfoFormat("OnCallStatusChanged start. {0}", status);
            switch (status)
            {
                case CallStatus.Dialing:
                case CallStatus.ConfIncoming:
                case CallStatus.P2pIncoming:
                    CurrentView = _dialingView;
                    CallController.Instance.PlayRingtone();
                    break;
                default:
                    CallController.Instance.StopRingtone();
                    break;

            }
            log.Info("OnCallStatusChanged end.");
        }
        
        #endregion
    }
}
