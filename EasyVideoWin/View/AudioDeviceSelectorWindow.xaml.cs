using EasyVideoWin.Model;
using EasyVideoWin.ViewModel;
using log4net;
using System.Windows;
using System.Windows.Controls;

namespace EasyVideoWin.View
{
    /// <summary>
    /// Interaction logic for AudioDeviceSelectorView.xaml
    /// </summary>
    public partial class AudioDeviceSelectorView : Window
    {
        private readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public AudioDeviceSelectorView()
        {
            InitializeComponent();
            CallController.Instance.CallStatusChanged += OnCallStatusChanged;
            log.Info("Audio device selector view constructed.");
        }

        private void closeBtn_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void OnCallStatusChanged(object sender, CallStatus status)
        {
            log.Info("OnCallStatusChanged start.");
            if (status == CallStatus.Ended)
            {
                App.Current.Dispatcher.InvokeAsync(() =>
                {
                    this.Close();
                });
            }
            log.Info("OnCallStatusChanged end.");
        }
    }
}
