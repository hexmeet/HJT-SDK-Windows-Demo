using EasyVideoWin.Helpers;
using EasyVideoWin.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;


namespace EasyVideoWin.ViewModel
{
    class SettingViewModel : ViewModelBase
    {
        private UserControl _generalView;
        private UserControl _audioView;
        private UserControl _videoView;
        private UserControl _aboutView;
        public RelayCommand SetGeneralCommand { get; set; }
        public RelayCommand AudioCommand { get; set; }
        public RelayCommand VideoCommand { get; set; }
        public RelayCommand ShowAboutCommand { get; set; }

        private UserControl _currentView;
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

        public SettingViewModel()
        {
            SetGeneralCommand = new RelayCommand(SetGeneral);
            AudioCommand = new RelayCommand(SetAudio);
            VideoCommand = new RelayCommand(SetVideo);
            ShowAboutCommand = new RelayCommand(ShowAbout);

            _generalView = (UserControl)Activator.CreateInstance(typeof(EasyVideoWin.View.GeneralView));
            _audioView = (UserControl)Activator.CreateInstance(typeof(EasyVideoWin.View.AudioSettingView));
            _videoView = (UserControl)Activator.CreateInstance(typeof(EasyVideoWin.View.VideoSettingView));
            _aboutView = (UserControl)Activator.CreateInstance(typeof(EasyVideoWin.View.AboutView));
            
            CurrentView = _generalView;
        }
        
        private void SetGeneral(object parameter)
        {
            CurrentView = _generalView;
        }

        private void SetAudio(object parameter)
        {
            CurrentView = _audioView;
        }

        private void SetVideo(object parameter)
        {
            CurrentView = _videoView;
        }

        private void ShowAbout(object parameter)
        {
            CurrentView = _aboutView;
        }
    }
}
