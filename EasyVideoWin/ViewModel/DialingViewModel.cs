using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EasyVideoWin.Model;
using EasyVideoWin.Helpers;
using System.ComponentModel;
using System.Windows;
using System.Collections.ObjectModel;
using System.Threading;
using System.Windows.Threading;
using log4net;

namespace EasyVideoWin.ViewModel
{
    class DialingViewModel : ViewModelBase
    {
        #region -- Members --
        private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private string _conferenceNumber;
        private string _backgroundImage;
        private float _backgroundOpacity;
        private Visibility _headerImageVisibility;
        private string _foreground;
        
        #endregion

        #region -- Properties --

        public RelayCommand HangupCommand { get; private set; }
        
        public string ConferenceNumber
        {
            get
            {
                return _conferenceNumber;
            }
            private set
            {
                _conferenceNumber = value;
                OnPropertyChanged("ConferenceNumber");
            }
        }

        public string BackgroundImage
        {
            get
            {
                return _backgroundImage;
            }
            private set
            {
                _backgroundImage = value;
                OnPropertyChanged("BackgroundImage");
            }
        }

        public float BackgroundOpacity
        {
            get
            {
                return _backgroundOpacity;
            }
            private set
            {
                _backgroundOpacity = value;
                OnPropertyChanged("BackgroundOpacity");
            }
        }

        public Visibility HeaderImageVisibility
        {
            get
            {
                return _headerImageVisibility;
            }
            private set
            {
                _headerImageVisibility = value;
                OnPropertyChanged("HeaderImageVisibility");
            }
        }
        
        public string Foreground
        {
            get
            {
                return _foreground;
            }
            private set
            {
                _foreground = value;
                OnPropertyChanged("Foreground");
            }
        }

        #endregion
        //private Dispatcher _workerDispatcher;

        #region -- Constructor --

        public DialingViewModel()
        {
            HangupCommand = new RelayCommand(Hangup);

            SetDefaultSetting();
            
            CallController.Instance.PropertyChanged += OnCallControllerPropertyChanged;
            CallController.Instance.CallStatusChanged += OnCallStatusChanged;
            //ManualResetEvent dispatcherReadyEvent = new ManualResetEvent(false);

            //Thread t = new Thread(new ThreadStart(() =>
            //{
            //    _workerDispatcher = Dispatcher.CurrentDispatcher;
            //    dispatcherReadyEvent.Set();
            //    Dispatcher.Run();
            //}));

            //t.IsBackground = true;
            //t.Start();

            //dispatcherReadyEvent.WaitOne();
        }

        #endregion

        #region -- Private Methods --

        private void OnCallStatusChanged(object sender, CallStatus status)
        {
            log.Info("OnCallStatusChanged start.");
            switch (status)
            {
                case CallStatus.Ended:
                    SetDefaultSetting();
                    break;
                default:
                    break;
            }
            log.Info("OnCallStatusChanged end.");
        }

        private void OnCallControllerPropertyChanged(object sender, PropertyChangedEventArgs args)
        {
            if ("ConferenceNumber" == args.PropertyName)
            {
                OnConferenceNumberChanged(CallController.Instance.ConferenceNumber);
            }
        }

        private void OnConferenceNumberChanged(string conferenceNumber)
        {
            ConferenceNumber = conferenceNumber;
        }
        
        private void SetDefaultSetting()
        {
            BackgroundImage = "pack://application:,,,/Resources/Icons/background_default.png";
            BackgroundOpacity = 1f;
            //HeaderImageVisibility = Visibility.Collapsed;
            Foreground = "#FFFFFF";
        }
        
        private void Hangup(object parameter)
        {
            CallController.Instance.TerminateCall();
        }

        #endregion

    }
}
