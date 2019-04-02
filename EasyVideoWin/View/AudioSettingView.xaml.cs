using EasyVideoWin.Model;
using NAudio.Wave;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;

namespace EasyVideoWin.View
{
    /// <summary>
    /// Interaction logic for AudioSettingView.xaml
    /// </summary>
    public partial class AudioSettingView : UserControl
    {
        private EasyVideoWin.ViewModel.AudioSettingViewModel _audioSettingViewModel;
        private WaveOutEvent _waveOut = null;

        public AudioSettingView()
        {
            InitializeComponent();

            _audioSettingViewModel = this.DataContext as EasyVideoWin.ViewModel.AudioSettingViewModel;
            _audioSettingViewModel.RefreshData();
            this.IsVisibleChanged += new DependencyPropertyChangedEventHandler(UserControl_IsVisibleChanged);
        }

        private void UserControl_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if((bool)e.OldValue == false && (bool)e.NewValue == true)
            {
                _audioSettingViewModel.isOpenCheckMicrophoneVolume = true;
                RefreshData();

                SettingManager.Instance.EnableMicMeter(true);
                _audioSettingViewModel.startCheckMicrophoneVolume();
            }
            else if((bool)e.OldValue == true && (bool)e.NewValue == false)
            {
                _audioSettingViewModel.isOpenCheckMicrophoneVolume = false;
                SettingManager.Instance.EnableMicMeter(false);
                DisposeWave();
            }
        }
        
        private void RefreshData()
        {
            _audioSettingViewModel.RefreshData();
        }
        
        #region audio
        private void DetectSpeaker()
        {
            string selectedSpeaker = SettingManager.Instance.GetCurrentSpeaker();
            if (selectedSpeaker != null && !selectedSpeaker.Equals(""))
            {
                int waveOutDevices = WaveOut.DeviceCount;
                for (int i = 0; i < WaveOut.DeviceCount; i++)
                {
                    var cap = WaveOut.GetCapabilities(i);
                    if (selectedSpeaker.Contains(cap.ProductName))
                    {
                        _waveOut.DeviceNumber = i;
                        return;
                    }
                }
            }
        }

        private void OpenLoudspeaker_Click(object sender, RoutedEventArgs e)
        {
            DisposeWave();
            //init
            _waveOut = new WaveOutEvent();
            //set output device
            DetectSpeaker();
            //sound file
            string filePath = @AppDomain.CurrentDomain.BaseDirectory + "Resources\\sounds\\speaker.wav";
            WaveFileReader reader = new WaveFileReader(filePath);
            _waveOut.Init(reader);
            _waveOut.Play();
        }

        private void DisposeWave()
        {
            if (_waveOut != null)
            {
                if (_waveOut.PlaybackState == NAudio.Wave.PlaybackState.Playing)
                {
                    _waveOut.Stop();
                    _waveOut.Dispose();
                    _waveOut = null;
                }
            }
        }
        #endregion
    }
}
