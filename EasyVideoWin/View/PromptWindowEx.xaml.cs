using EasyVideoWin.Helpers;
using EasyVideoWin.Model;
using log4net;
using System.Timers;
using System.Windows;

namespace EasyVideoWin.View
{
    /// <summary>
    /// Interaction logic for PromptWindowEx.xaml
    /// </summary>
    public partial class PromptWindowEx : Window
    {
        private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private static readonly int SHOW_DURATION = 5000;

        public PromptWindowEx()
        {
            InitializeComponent();
            CallController.Instance.CallStatusChanged += OnCallStatusChanged;
        }

        private void OnCallStatusChanged(object sender, CallStatus status)
        {
            log.Info("OnCallStatusChanged start.");
            if (status == CallStatus.Ended)
            {
                App.Current.Dispatcher.Invoke(() =>
                {
                    this.Close();
                });
            }
            log.Info("OnCallStatusChanged end.");
        }

        private void CloseWindow(object sender, ElapsedEventArgs e)
        {
            App.Current.Dispatcher.Invoke(() =>
            {
                this.Close();
            });
        }

        private void StartAutoCloseTimer()
        {
            Timer timer = new Timer();
            timer.Interval = SHOW_DURATION;
            timer.AutoReset = false;
            timer.Elapsed += CloseWindow;
            timer.Enabled = true;
        }

        public void ShowOncePrompt(string content)
        {
            if (string.IsNullOrEmpty(content))
            {
                return;
            }

            this.label.Content = content;
            this.Show();
            StartAutoCloseTimer();
        }
    }
}
