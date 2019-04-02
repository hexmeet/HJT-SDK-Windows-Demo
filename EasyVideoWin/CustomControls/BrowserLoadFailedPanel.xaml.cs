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

namespace EasyVideoWin.CustomControls
{
    /// <summary>
    /// Interaction logic for BrowserLoadFailedPanel.xaml
    /// </summary>
    public partial class BrowserLoadFailedPanel : UserControl
    {
        public event RoutedEventHandler ReloadEvent;

        public BrowserLoadFailedPanel()
        {
            InitializeComponent();
        }

        private void Reload_Click(object sender, RoutedEventArgs e)
        {
            ReloadEvent?.Invoke(sender, e);
        }
    }
}
