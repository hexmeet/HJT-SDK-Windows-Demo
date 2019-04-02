using EasyVideoWin.Helpers;
using System;
using System.Timers;
using System.Windows;

namespace EasyVideoWin.View
{
    /// <summary>
    /// Interaction logic for SpeakingPrompt.xaml
    /// </summary>
    public partial class SpeakingPrompt : Window
    {
        private static readonly int SHOW_DURATION = 3000;
        
        private static Visibility _myVisibility = Visibility.Hidden;

        public SpeakingPrompt()
        {
            InitializeComponent();
            this.Owner = VideoPeopleWindow.Instance;
        }

        private void StartAutoCloseTimer()
        {
            Timer timer = new Timer();
            timer.Interval = SHOW_DURATION;
            timer.AutoReset = false;
            timer.Elapsed += CloseWindow;
            timer.Enabled = true;
        }

        private void CloseWindow(object sender, ElapsedEventArgs e)
        {
            App.Current.Dispatcher.Invoke(() =>
            {
                this.Close();
                _myVisibility = Visibility.Hidden;
            });
        }

        public void ShowPrompt()
        {
            if (_myVisibility == Visibility.Visible)
            {
                return;
            }
            
            this.Show();
            _myVisibility = Visibility.Visible;
            StartAutoCloseTimer();
        }
    }
}
