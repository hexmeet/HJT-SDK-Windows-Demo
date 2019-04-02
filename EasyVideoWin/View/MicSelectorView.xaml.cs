using EasyVideoWin.ViewModel;
using log4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace EasyVideoWin.View
{
    /// <summary>
    /// Interaction logic for MicSelector.xaml
    /// </summary>
    public partial class MicSelectorView : UserControl
    {
        private readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private MicrophoneViewModel _viewModel;

        public MicSelectorView()
        {
            InitializeComponent();
            _viewModel = this.DataContext as MicrophoneViewModel;
            log.Info("Mic speaker selector view constructed.");
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
