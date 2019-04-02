using EasyVideoWin.Model;
using System;
using System.Collections.ObjectModel;
using System.Threading;
using System.Windows.Media;

namespace EasyVideoWin.ViewModel
{
    class AudioSettingViewModel : ViewModelBase
    {
        private delegate void DelegateChangeMicrophoneVolume();
        public bool isOpenCheckMicrophoneVolume = false;

        private Brush _defaultForeGround = new SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#e1e1e1"));
        private Brush _selectedForeGround = new SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#00be82"));

        private ObservableCollection<string> _microphoneValues;
        public ObservableCollection<string> MicrophoneValues
        {
            get
            {
                return _microphoneValues;
            }
            set
            {
                _microphoneValues = value;
                OnPropertyChanged("MicrophoneValues");
            }
        }

        private string _microphoneSelectedValue;
        public String MicrophoneSelectedValue
        {
            get
            {
                return _microphoneSelectedValue;
            }
            set
            {
                if (_microphoneSelectedValue != value)
                {
                    isOpenCheckMicrophoneVolume = false;
                    SettingManager.Instance.EnableMicMeter(false);
                    SettingManager.Instance.SetMicrophone(value);

                    isOpenCheckMicrophoneVolume = true;
                    SettingManager.Instance.EnableMicMeter(true);
                    startCheckMicrophoneVolume();
                }
                _microphoneSelectedValue = value;
                OnPropertyChanged("MicrophoneSelectedValue");
            }
        }

        private ObservableCollection<string> _speakerValues;
        public ObservableCollection<string> SpeakerValues
        {
            get
            {
                return _speakerValues;
            }
            set
            {
                _speakerValues = value;
                OnPropertyChanged("SpeakerValues");
            }
        }

        private string _speakerSelectedValue;
        public String SpeakerSelectedValue
        {
            get
            {
                return _speakerSelectedValue;
            }
            set
            {
                if (_speakerSelectedValue != value)
                {
                    SettingManager.Instance.SetSpeaker(value);
                }
                _speakerSelectedValue = value;
                OnPropertyChanged("SpeakerSelectedValue");
            }
        }
        
        #region Microphone Volume
        public AudioSettingViewModel()
        {
            //init
            resetMicrophoneVolumeRange();
        }

        public void startCheckMicrophoneVolume()
        {
            Thread thread = new Thread(new ThreadStart(StartMicrophoneVolumeThread));
            thread.Start();
        }

        public void StartMicrophoneVolumeThread()
        {
            try
            {
                while (isOpenCheckMicrophoneVolume)
                {
                    float tempVolume = SettingManager.Instance.GetMicVolume();
                    if (tempVolume > 0)
                    {
                        decimal tempNumber = ((decimal)tempVolume * 100) / 4;
                        if (tempNumber > 0)
                        {
                            setMicrophoneVolumeRange((Int16)Math.Ceiling(tempNumber));
                        }
                    }

                    Thread.Sleep(200);
                    resetMicrophoneVolumeRange();
                }
            }
            catch (Exception e)
            {

            }
        }

        private void resetMicrophoneVolumeRange()
        {
            MicrophoneVolume1 = _defaultForeGround;
            MicrophoneVolume2 = _defaultForeGround;
            MicrophoneVolume3 = _defaultForeGround;
            MicrophoneVolume4 = _defaultForeGround;
            MicrophoneVolume5 = _defaultForeGround;
            MicrophoneVolume6 = _defaultForeGround;
            MicrophoneVolume7 = _defaultForeGround;
            MicrophoneVolume8 = _defaultForeGround;
            MicrophoneVolume9 = _defaultForeGround;
            MicrophoneVolume10 = _defaultForeGround;
            MicrophoneVolume11 = _defaultForeGround;
            MicrophoneVolume12 = _defaultForeGround;
            MicrophoneVolume13 = _defaultForeGround;
            MicrophoneVolume14 = _defaultForeGround;
            MicrophoneVolume15 = _defaultForeGround;
            MicrophoneVolume16 = _defaultForeGround;
            MicrophoneVolume17 = _defaultForeGround;
            MicrophoneVolume18 = _defaultForeGround;
            MicrophoneVolume19 = _defaultForeGround;
            MicrophoneVolume20 = _defaultForeGround;
            MicrophoneVolume21 = _defaultForeGround;
            MicrophoneVolume22 = _defaultForeGround;
            MicrophoneVolume23 = _defaultForeGround;
            MicrophoneVolume24 = _defaultForeGround;
            MicrophoneVolume25 = _defaultForeGround;
        }

        private void setMicrophoneVolumeRange(int tempNumber)
        {
            switch (tempNumber)
            {
                case 1:
                    MicrophoneVolume1 = _selectedForeGround;
                    break;
                case 2:
                    MicrophoneVolume1 = _selectedForeGround;
                    MicrophoneVolume2 = _selectedForeGround;
                    break;
                case 3:
                    MicrophoneVolume1 = _selectedForeGround;
                    MicrophoneVolume2 = _selectedForeGround;
                    MicrophoneVolume3 = _selectedForeGround;
                    break;
                case 4:
                    MicrophoneVolume1 = _selectedForeGround;
                    MicrophoneVolume2 = _selectedForeGround;
                    MicrophoneVolume3 = _selectedForeGround;
                    MicrophoneVolume4 = _selectedForeGround;
                    break;
                case 5:
                    MicrophoneVolume1 = _selectedForeGround;
                    MicrophoneVolume2 = _selectedForeGround;
                    MicrophoneVolume3 = _selectedForeGround;
                    MicrophoneVolume4 = _selectedForeGround;
                    MicrophoneVolume5 = _selectedForeGround;
                    break;
                case 6:
                    MicrophoneVolume1 = _selectedForeGround;
                    MicrophoneVolume2 = _selectedForeGround;
                    MicrophoneVolume3 = _selectedForeGround;
                    MicrophoneVolume4 = _selectedForeGround;
                    MicrophoneVolume5 = _selectedForeGround;
                    MicrophoneVolume6 = _selectedForeGround;
                    break;
                case 7:
                    MicrophoneVolume1 = _selectedForeGround;
                    MicrophoneVolume2 = _selectedForeGround;
                    MicrophoneVolume3 = _selectedForeGround;
                    MicrophoneVolume4 = _selectedForeGround;
                    MicrophoneVolume5 = _selectedForeGround;
                    MicrophoneVolume6 = _selectedForeGround;
                    MicrophoneVolume7 = _selectedForeGround;
                    break;
                case 8:
                    MicrophoneVolume1 = _selectedForeGround;
                    MicrophoneVolume2 = _selectedForeGround;
                    MicrophoneVolume3 = _selectedForeGround;
                    MicrophoneVolume4 = _selectedForeGround;
                    MicrophoneVolume5 = _selectedForeGround;
                    MicrophoneVolume6 = _selectedForeGround;
                    MicrophoneVolume7 = _selectedForeGround;
                    MicrophoneVolume8 = _selectedForeGround;
                    break;
                case 9:
                    MicrophoneVolume1 = _selectedForeGround;
                    MicrophoneVolume2 = _selectedForeGround;
                    MicrophoneVolume3 = _selectedForeGround;
                    MicrophoneVolume4 = _selectedForeGround;
                    MicrophoneVolume5 = _selectedForeGround;
                    MicrophoneVolume6 = _selectedForeGround;
                    MicrophoneVolume7 = _selectedForeGround;
                    MicrophoneVolume8 = _selectedForeGround;
                    MicrophoneVolume9 = _selectedForeGround;
                    break;
                case 10:
                    MicrophoneVolume1 = _selectedForeGround;
                    MicrophoneVolume2 = _selectedForeGround;
                    MicrophoneVolume3 = _selectedForeGround;
                    MicrophoneVolume4 = _selectedForeGround;
                    MicrophoneVolume5 = _selectedForeGround;
                    MicrophoneVolume6 = _selectedForeGround;
                    MicrophoneVolume7 = _selectedForeGround;
                    MicrophoneVolume8 = _selectedForeGround;
                    MicrophoneVolume9 = _selectedForeGround;
                    MicrophoneVolume10 = _selectedForeGround;
                    break;
                case 11:
                    MicrophoneVolume1 = _selectedForeGround;
                    MicrophoneVolume2 = _selectedForeGround;
                    MicrophoneVolume3 = _selectedForeGround;
                    MicrophoneVolume4 = _selectedForeGround;
                    MicrophoneVolume5 = _selectedForeGround;
                    MicrophoneVolume6 = _selectedForeGround;
                    MicrophoneVolume7 = _selectedForeGround;
                    MicrophoneVolume8 = _selectedForeGround;
                    MicrophoneVolume9 = _selectedForeGround;
                    MicrophoneVolume10 = _selectedForeGround;
                    MicrophoneVolume11 = _selectedForeGround;
                    break;
                case 12:
                    MicrophoneVolume1 = _selectedForeGround;
                    MicrophoneVolume2 = _selectedForeGround;
                    MicrophoneVolume3 = _selectedForeGround;
                    MicrophoneVolume4 = _selectedForeGround;
                    MicrophoneVolume5 = _selectedForeGround;
                    MicrophoneVolume6 = _selectedForeGround;
                    MicrophoneVolume7 = _selectedForeGround;
                    MicrophoneVolume8 = _selectedForeGround;
                    MicrophoneVolume9 = _selectedForeGround;
                    MicrophoneVolume10 = _selectedForeGround;
                    MicrophoneVolume11 = _selectedForeGround;
                    MicrophoneVolume12 = _selectedForeGround;
                    break;
                case 13:
                    MicrophoneVolume1 = _selectedForeGround;
                    MicrophoneVolume2 = _selectedForeGround;
                    MicrophoneVolume3 = _selectedForeGround;
                    MicrophoneVolume4 = _selectedForeGround;
                    MicrophoneVolume5 = _selectedForeGround;
                    MicrophoneVolume6 = _selectedForeGround;
                    MicrophoneVolume7 = _selectedForeGround;
                    MicrophoneVolume8 = _selectedForeGround;
                    MicrophoneVolume9 = _selectedForeGround;
                    MicrophoneVolume10 = _selectedForeGround;
                    MicrophoneVolume11 = _selectedForeGround;
                    MicrophoneVolume12 = _selectedForeGround;
                    MicrophoneVolume13 = _selectedForeGround;
                    break;
                case 14:
                    MicrophoneVolume1 = _selectedForeGround;
                    MicrophoneVolume2 = _selectedForeGround;
                    MicrophoneVolume3 = _selectedForeGround;
                    MicrophoneVolume4 = _selectedForeGround;
                    MicrophoneVolume5 = _selectedForeGround;
                    MicrophoneVolume6 = _selectedForeGround;
                    MicrophoneVolume7 = _selectedForeGround;
                    MicrophoneVolume8 = _selectedForeGround;
                    MicrophoneVolume9 = _selectedForeGround;
                    MicrophoneVolume10 = _selectedForeGround;
                    MicrophoneVolume11 = _selectedForeGround;
                    MicrophoneVolume12 = _selectedForeGround;
                    MicrophoneVolume13 = _selectedForeGround;
                    MicrophoneVolume14 = _selectedForeGround;
                    break;
                case 15:
                    MicrophoneVolume1 = _selectedForeGround;
                    MicrophoneVolume2 = _selectedForeGround;
                    MicrophoneVolume3 = _selectedForeGround;
                    MicrophoneVolume4 = _selectedForeGround;
                    MicrophoneVolume5 = _selectedForeGround;
                    MicrophoneVolume6 = _selectedForeGround;
                    MicrophoneVolume7 = _selectedForeGround;
                    MicrophoneVolume8 = _selectedForeGround;
                    MicrophoneVolume9 = _selectedForeGround;
                    MicrophoneVolume10 = _selectedForeGround;
                    MicrophoneVolume11 = _selectedForeGround;
                    MicrophoneVolume12 = _selectedForeGround;
                    MicrophoneVolume13 = _selectedForeGround;
                    MicrophoneVolume14 = _selectedForeGround;
                    MicrophoneVolume15 = _selectedForeGround;
                    break;
                case 16:
                    MicrophoneVolume1 = _selectedForeGround;
                    MicrophoneVolume2 = _selectedForeGround;
                    MicrophoneVolume3 = _selectedForeGround;
                    MicrophoneVolume4 = _selectedForeGround;
                    MicrophoneVolume5 = _selectedForeGround;
                    MicrophoneVolume6 = _selectedForeGround;
                    MicrophoneVolume7 = _selectedForeGround;
                    MicrophoneVolume8 = _selectedForeGround;
                    MicrophoneVolume9 = _selectedForeGround;
                    MicrophoneVolume10 = _selectedForeGround;
                    MicrophoneVolume11 = _selectedForeGround;
                    MicrophoneVolume12 = _selectedForeGround;
                    MicrophoneVolume13 = _selectedForeGround;
                    MicrophoneVolume14 = _selectedForeGround;
                    MicrophoneVolume15 = _selectedForeGround;
                    MicrophoneVolume16 = _selectedForeGround;
                    break;
                case 17:
                    MicrophoneVolume1 = _selectedForeGround;
                    MicrophoneVolume2 = _selectedForeGround;
                    MicrophoneVolume3 = _selectedForeGround;
                    MicrophoneVolume4 = _selectedForeGround;
                    MicrophoneVolume5 = _selectedForeGround;
                    MicrophoneVolume6 = _selectedForeGround;
                    MicrophoneVolume7 = _selectedForeGround;
                    MicrophoneVolume8 = _selectedForeGround;
                    MicrophoneVolume9 = _selectedForeGround;
                    MicrophoneVolume10 = _selectedForeGround;
                    MicrophoneVolume11 = _selectedForeGround;
                    MicrophoneVolume12 = _selectedForeGround;
                    MicrophoneVolume13 = _selectedForeGround;
                    MicrophoneVolume14 = _selectedForeGround;
                    MicrophoneVolume15 = _selectedForeGround;
                    MicrophoneVolume16 = _selectedForeGround;
                    MicrophoneVolume17 = _selectedForeGround;
                    break;
                case 18:
                    MicrophoneVolume1 = _selectedForeGround;
                    MicrophoneVolume2 = _selectedForeGround;
                    MicrophoneVolume3 = _selectedForeGround;
                    MicrophoneVolume4 = _selectedForeGround;
                    MicrophoneVolume5 = _selectedForeGround;
                    MicrophoneVolume6 = _selectedForeGround;
                    MicrophoneVolume7 = _selectedForeGround;
                    MicrophoneVolume8 = _selectedForeGround;
                    MicrophoneVolume9 = _selectedForeGround;
                    MicrophoneVolume10 = _selectedForeGround;
                    MicrophoneVolume11 = _selectedForeGround;
                    MicrophoneVolume12 = _selectedForeGround;
                    MicrophoneVolume13 = _selectedForeGround;
                    MicrophoneVolume14 = _selectedForeGround;
                    MicrophoneVolume15 = _selectedForeGround;
                    MicrophoneVolume16 = _selectedForeGround;
                    MicrophoneVolume17 = _selectedForeGround;
                    MicrophoneVolume18 = _selectedForeGround;
                    break;
                case 19:
                    MicrophoneVolume1 = _selectedForeGround;
                    MicrophoneVolume2 = _selectedForeGround;
                    MicrophoneVolume3 = _selectedForeGround;
                    MicrophoneVolume4 = _selectedForeGround;
                    MicrophoneVolume5 = _selectedForeGround;
                    MicrophoneVolume6 = _selectedForeGround;
                    MicrophoneVolume7 = _selectedForeGround;
                    MicrophoneVolume8 = _selectedForeGround;
                    MicrophoneVolume9 = _selectedForeGround;
                    MicrophoneVolume10 = _selectedForeGround;
                    MicrophoneVolume11 = _selectedForeGround;
                    MicrophoneVolume12 = _selectedForeGround;
                    MicrophoneVolume13 = _selectedForeGround;
                    MicrophoneVolume14 = _selectedForeGround;
                    MicrophoneVolume15 = _selectedForeGround;
                    MicrophoneVolume16 = _selectedForeGround;
                    MicrophoneVolume17 = _selectedForeGround;
                    MicrophoneVolume18 = _selectedForeGround;
                    MicrophoneVolume19 = _selectedForeGround;
                    break;
                case 20:
                    MicrophoneVolume1 = _selectedForeGround;
                    MicrophoneVolume2 = _selectedForeGround;
                    MicrophoneVolume3 = _selectedForeGround;
                    MicrophoneVolume4 = _selectedForeGround;
                    MicrophoneVolume5 = _selectedForeGround;
                    MicrophoneVolume6 = _selectedForeGround;
                    MicrophoneVolume7 = _selectedForeGround;
                    MicrophoneVolume8 = _selectedForeGround;
                    MicrophoneVolume9 = _selectedForeGround;
                    MicrophoneVolume10 = _selectedForeGround;
                    MicrophoneVolume11 = _selectedForeGround;
                    MicrophoneVolume12 = _selectedForeGround;
                    MicrophoneVolume13 = _selectedForeGround;
                    MicrophoneVolume14 = _selectedForeGround;
                    MicrophoneVolume15 = _selectedForeGround;
                    MicrophoneVolume16 = _selectedForeGround;
                    MicrophoneVolume17 = _selectedForeGround;
                    MicrophoneVolume18 = _selectedForeGround;
                    MicrophoneVolume19 = _selectedForeGround;
                    MicrophoneVolume20 = _selectedForeGround;
                    break;
                case 21:
                    MicrophoneVolume1 = _selectedForeGround;
                    MicrophoneVolume2 = _selectedForeGround;
                    MicrophoneVolume3 = _selectedForeGround;
                    MicrophoneVolume4 = _selectedForeGround;
                    MicrophoneVolume5 = _selectedForeGround;
                    MicrophoneVolume6 = _selectedForeGround;
                    MicrophoneVolume7 = _selectedForeGround;
                    MicrophoneVolume8 = _selectedForeGround;
                    MicrophoneVolume9 = _selectedForeGround;
                    MicrophoneVolume10 = _selectedForeGround;
                    MicrophoneVolume11 = _selectedForeGround;
                    MicrophoneVolume12 = _selectedForeGround;
                    MicrophoneVolume13 = _selectedForeGround;
                    MicrophoneVolume14 = _selectedForeGround;
                    MicrophoneVolume15 = _selectedForeGround;
                    MicrophoneVolume16 = _selectedForeGround;
                    MicrophoneVolume17 = _selectedForeGround;
                    MicrophoneVolume18 = _selectedForeGround;
                    MicrophoneVolume19 = _selectedForeGround;
                    MicrophoneVolume20 = _selectedForeGround;
                    MicrophoneVolume21 = _selectedForeGround;
                    break;
                case 22:
                    MicrophoneVolume1 = _selectedForeGround;
                    MicrophoneVolume2 = _selectedForeGround;
                    MicrophoneVolume3 = _selectedForeGround;
                    MicrophoneVolume4 = _selectedForeGround;
                    MicrophoneVolume5 = _selectedForeGround;
                    MicrophoneVolume6 = _selectedForeGround;
                    MicrophoneVolume7 = _selectedForeGround;
                    MicrophoneVolume8 = _selectedForeGround;
                    MicrophoneVolume9 = _selectedForeGround;
                    MicrophoneVolume10 = _selectedForeGround;
                    MicrophoneVolume11 = _selectedForeGround;
                    MicrophoneVolume12 = _selectedForeGround;
                    MicrophoneVolume13 = _selectedForeGround;
                    MicrophoneVolume14 = _selectedForeGround;
                    MicrophoneVolume15 = _selectedForeGround;
                    MicrophoneVolume16 = _selectedForeGround;
                    MicrophoneVolume17 = _selectedForeGround;
                    MicrophoneVolume18 = _selectedForeGround;
                    MicrophoneVolume19 = _selectedForeGround;
                    MicrophoneVolume20 = _selectedForeGround;
                    MicrophoneVolume21 = _selectedForeGround;
                    MicrophoneVolume22 = _selectedForeGround;
                    break;
                case 23:
                    MicrophoneVolume1 = _selectedForeGround;
                    MicrophoneVolume2 = _selectedForeGround;
                    MicrophoneVolume3 = _selectedForeGround;
                    MicrophoneVolume4 = _selectedForeGround;
                    MicrophoneVolume5 = _selectedForeGround;
                    MicrophoneVolume6 = _selectedForeGround;
                    MicrophoneVolume7 = _selectedForeGround;
                    MicrophoneVolume8 = _selectedForeGround;
                    MicrophoneVolume9 = _selectedForeGround;
                    MicrophoneVolume10 = _selectedForeGround;
                    MicrophoneVolume11 = _selectedForeGround;
                    MicrophoneVolume12 = _selectedForeGround;
                    MicrophoneVolume13 = _selectedForeGround;
                    MicrophoneVolume14 = _selectedForeGround;
                    MicrophoneVolume15 = _selectedForeGround;
                    MicrophoneVolume16 = _selectedForeGround;
                    MicrophoneVolume17 = _selectedForeGround;
                    MicrophoneVolume18 = _selectedForeGround;
                    MicrophoneVolume19 = _selectedForeGround;
                    MicrophoneVolume20 = _selectedForeGround;
                    MicrophoneVolume21 = _selectedForeGround;
                    MicrophoneVolume22 = _selectedForeGround;
                    MicrophoneVolume23 = _selectedForeGround;
                    break;
                case 24:
                    MicrophoneVolume1 = _selectedForeGround;
                    MicrophoneVolume2 = _selectedForeGround;
                    MicrophoneVolume3 = _selectedForeGround;
                    MicrophoneVolume4 = _selectedForeGround;
                    MicrophoneVolume5 = _selectedForeGround;
                    MicrophoneVolume6 = _selectedForeGround;
                    MicrophoneVolume7 = _selectedForeGround;
                    MicrophoneVolume8 = _selectedForeGround;
                    MicrophoneVolume9 = _selectedForeGround;
                    MicrophoneVolume10 = _selectedForeGround;
                    MicrophoneVolume11 = _selectedForeGround;
                    MicrophoneVolume12 = _selectedForeGround;
                    MicrophoneVolume13 = _selectedForeGround;
                    MicrophoneVolume14 = _selectedForeGround;
                    MicrophoneVolume15 = _selectedForeGround;
                    MicrophoneVolume16 = _selectedForeGround;
                    MicrophoneVolume17 = _selectedForeGround;
                    MicrophoneVolume18 = _selectedForeGround;
                    MicrophoneVolume19 = _selectedForeGround;
                    MicrophoneVolume20 = _selectedForeGround;
                    MicrophoneVolume21 = _selectedForeGround;
                    MicrophoneVolume22 = _selectedForeGround;
                    MicrophoneVolume23 = _selectedForeGround;
                    MicrophoneVolume24 = _selectedForeGround;
                    break;
                case 25:
                    MicrophoneVolume1 = _selectedForeGround;
                    MicrophoneVolume2 = _selectedForeGround;
                    MicrophoneVolume3 = _selectedForeGround;
                    MicrophoneVolume4 = _selectedForeGround;
                    MicrophoneVolume5 = _selectedForeGround;
                    MicrophoneVolume6 = _selectedForeGround;
                    MicrophoneVolume7 = _selectedForeGround;
                    MicrophoneVolume8 = _selectedForeGround;
                    MicrophoneVolume9 = _selectedForeGround;
                    MicrophoneVolume10 = _selectedForeGround;
                    MicrophoneVolume11 = _selectedForeGround;
                    MicrophoneVolume12 = _selectedForeGround;
                    MicrophoneVolume13 = _selectedForeGround;
                    MicrophoneVolume14 = _selectedForeGround;
                    MicrophoneVolume15 = _selectedForeGround;
                    MicrophoneVolume16 = _selectedForeGround;
                    MicrophoneVolume17 = _selectedForeGround;
                    MicrophoneVolume18 = _selectedForeGround;
                    MicrophoneVolume19 = _selectedForeGround;
                    MicrophoneVolume20 = _selectedForeGround;
                    MicrophoneVolume21 = _selectedForeGround;
                    MicrophoneVolume22 = _selectedForeGround;
                    MicrophoneVolume23 = _selectedForeGround;
                    MicrophoneVolume24 = _selectedForeGround;
                    MicrophoneVolume25 = _selectedForeGround;
                    break;
            }
        }

        private Brush _microphoneVolume1;
        public Brush MicrophoneVolume1
        {
            get { return _microphoneVolume1; }
            set { _microphoneVolume1 = value; OnPropertyChanged("MicrophoneVolume1"); }
        }
        private Brush _microphoneVolume2;
        public Brush MicrophoneVolume2
        {
            get { return _microphoneVolume2; }
            set { _microphoneVolume2 = value; OnPropertyChanged("MicrophoneVolume2"); }
        }
        private Brush _microphoneVolume3;
        public Brush MicrophoneVolume3
        {
            get { return _microphoneVolume3; }
            set { _microphoneVolume3 = value; OnPropertyChanged("MicrophoneVolume3"); }
        }
        private Brush _microphoneVolume4;
        public Brush MicrophoneVolume4
        {
            get { return _microphoneVolume4; }
            set { _microphoneVolume4 = value; OnPropertyChanged("MicrophoneVolume4"); }
        }
        private Brush _microphoneVolume5;
        public Brush MicrophoneVolume5
        {
            get { return _microphoneVolume5; }
            set { _microphoneVolume5 = value; OnPropertyChanged("MicrophoneVolume5"); }
        }
        private Brush _microphoneVolume6;
        public Brush MicrophoneVolume6
        {
            get { return _microphoneVolume6; }
            set { _microphoneVolume6 = value; OnPropertyChanged("MicrophoneVolume6"); }
        }
        private Brush _microphoneVolume7;
        public Brush MicrophoneVolume7
        {
            get { return _microphoneVolume7; }
            set { _microphoneVolume7 = value; OnPropertyChanged("MicrophoneVolume7"); }
        }
        private Brush _microphoneVolume8;
        public Brush MicrophoneVolume8
        {
            get { return _microphoneVolume8; }
            set { _microphoneVolume8 = value; OnPropertyChanged("MicrophoneVolume8"); }
        }
        private Brush _microphoneVolume9;
        public Brush MicrophoneVolume9
        {
            get { return _microphoneVolume9; }
            set { _microphoneVolume9 = value; OnPropertyChanged("MicrophoneVolume9"); }
        }
        private Brush _microphoneVolume10;
        public Brush MicrophoneVolume10
        {
            get { return _microphoneVolume10; }
            set { _microphoneVolume10 = value; OnPropertyChanged("MicrophoneVolume10"); }
        }
        private Brush _microphoneVolume11;
        public Brush MicrophoneVolume11
        {
            get { return _microphoneVolume11; }
            set { _microphoneVolume11 = value; OnPropertyChanged("MicrophoneVolume11"); }
        }
        private Brush _microphoneVolume12;
        public Brush MicrophoneVolume12
        {
            get { return _microphoneVolume12; }
            set { _microphoneVolume12 = value; OnPropertyChanged("MicrophoneVolume12"); }
        }
        private Brush _microphoneVolume13;
        public Brush MicrophoneVolume13
        {
            get { return _microphoneVolume13; }
            set { _microphoneVolume13 = value; OnPropertyChanged("MicrophoneVolume13"); }
        }
        private Brush _microphoneVolume14;
        public Brush MicrophoneVolume14
        {
            get { return _microphoneVolume14; }
            set { _microphoneVolume14 = value; OnPropertyChanged("MicrophoneVolume14"); }
        }
        private Brush _microphoneVolume15;
        public Brush MicrophoneVolume15
        {
            get { return _microphoneVolume15; }
            set { _microphoneVolume15 = value; OnPropertyChanged("MicrophoneVolume15"); }
        }
        private Brush _microphoneVolume16;
        public Brush MicrophoneVolume16
        {
            get { return _microphoneVolume16; }
            set { _microphoneVolume16 = value; OnPropertyChanged("MicrophoneVolume16"); }
        }
        private Brush _microphoneVolume17;
        public Brush MicrophoneVolume17
        {
            get { return _microphoneVolume17; }
            set { _microphoneVolume17 = value; OnPropertyChanged("MicrophoneVolume17"); }
        }
        private Brush _microphoneVolume18;
        public Brush MicrophoneVolume18
        {
            get { return _microphoneVolume18; }
            set { _microphoneVolume18 = value; OnPropertyChanged("MicrophoneVolume18"); }
        }
        private Brush _microphoneVolume19;
        public Brush MicrophoneVolume19
        {
            get { return _microphoneVolume19; }
            set { _microphoneVolume19 = value; OnPropertyChanged("MicrophoneVolume19"); }
        }
        private Brush _microphoneVolume20;
        public Brush MicrophoneVolume20
        {
            get { return _microphoneVolume20; }
            set { _microphoneVolume20 = value; OnPropertyChanged("MicrophoneVolume20"); }
        }
        private Brush _microphoneVolume21;
        public Brush MicrophoneVolume21
        {
            get { return _microphoneVolume21; }
            set { _microphoneVolume21 = value; OnPropertyChanged("MicrophoneVolume20"); }
        }
        private Brush _microphoneVolume22;
        public Brush MicrophoneVolume22
        {
            get { return _microphoneVolume22; }
            set { _microphoneVolume22 = value; OnPropertyChanged("MicrophoneVolume20"); }
        }
        private Brush _microphoneVolume23;
        public Brush MicrophoneVolume23
        {
            get { return _microphoneVolume23; }
            set { _microphoneVolume23 = value; OnPropertyChanged("MicrophoneVolume20"); }
        }
        private Brush _microphoneVolume24;
        public Brush MicrophoneVolume24
        {
            get { return _microphoneVolume24; }
            set { _microphoneVolume24 = value; OnPropertyChanged("MicrophoneVolume20"); }
        }
        private Brush _microphoneVolume25;
        public Brush MicrophoneVolume25
        {
            get { return _microphoneVolume25; }
            set { _microphoneVolume25 = value; OnPropertyChanged("MicrophoneVolume20"); }
        }
        #endregion

        public void RefreshData()
        {
            //string[] microhones = DeviceManager.Instance.GetMicrophones();
            //MicrophoneValues = new ObservableCollection<string>(microhones);
            MicrophoneValues = SettingManager.Instance.GetMicrophones();
            _microphoneSelectedValue = SettingManager.Instance.GetCurrentMicrophone();
            MicrophoneSelectedValue = _microphoneSelectedValue;

            SpeakerValues = SettingManager.Instance.GetSpeakers();
            _speakerSelectedValue = SettingManager.Instance.GetCurrentSpeaker();
            SpeakerSelectedValue = _speakerSelectedValue;
        }
    }
}
