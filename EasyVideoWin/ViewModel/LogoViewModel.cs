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
    class LogoViewModel : ViewModelBase
    {
        #region -- Members --
        private string _conferenceNumber;
        
        #endregion

        #region -- Properties --
        
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
        
        #endregion
        //private Dispatcher _workerDispatcher;

        #region -- Constructor --

        public LogoViewModel()
        {
            CallController.Instance.PropertyChanged += OnCallControllerPropertyChanged;
        }

        #endregion

        #region -- Private Methods --
        
        private void OnCallControllerPropertyChanged(object sender, PropertyChangedEventArgs args)
        {
            if ("ConferenceNumber" == args.PropertyName)
            {
                ConferenceNumber = CallController.Instance.ConferenceNumber;
            }
        }
        
        #endregion

    }
}
