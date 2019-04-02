using EasyVideoWin.Model;
using System;
using System.Collections.ObjectModel;
using System.Threading;
using System.Windows.Media;

namespace EasyVideoWin.ViewModel
{
    class VideoSettingViewModel : ViewModelBase
    {
        
        private ObservableCollection<string> _cameraValues;
        public ObservableCollection<string> CameraValues
        {
            get
            {
                return _cameraValues;
            }
            set
            {
                _cameraValues = value;
                OnPropertyChanged("CameraValues");
            }
        }

        private string _cameraSelectedValue;
        public String CameraSelectedValue
        {
            get
            {
                return _cameraSelectedValue;
            }
            set
            {
                if (_cameraSelectedValue != value)
                {
                    SettingManager.Instance.SetCamera(value);
                }
                _cameraSelectedValue = value;
                OnPropertyChanged("CameraSelectedValue");
            }
        }

        public VideoSettingViewModel()
        {
            
        }
        
        public void RefreshData()
        {
            CameraValues = SettingManager.Instance.GetCameras();
            _cameraSelectedValue = SettingManager.Instance.GetCurrentCamera();
            CameraSelectedValue = _cameraSelectedValue;

        }
    }
}
