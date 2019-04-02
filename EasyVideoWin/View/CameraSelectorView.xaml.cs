using EasyVideoWin.ViewModel;
using log4net;
using System.Windows.Controls;

namespace EasyVideoWin.View
{
    /// <summary>
    /// Interaction logic for CameraSelectorView.xaml
    /// </summary>
    public partial class CameraSelectorView : UserControl
    {
        private readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private CameraSelectorViewModel _viewModel;
        public CameraSelectorView()
        {
            InitializeComponent();
            _viewModel = this.DataContext as CameraSelectorViewModel;
            log.Info("Camera speaker selector view constructed.");
        }

        private void SelectItem(object sender, SelectionChangedEventArgs args)
        {
            ListBox listBox = sender as ListBox;
            if (listBox.HasItems)
            {
                _viewModel.SelectItem((sender as ListBox).SelectedIndex);
            }
        }
    }
}
