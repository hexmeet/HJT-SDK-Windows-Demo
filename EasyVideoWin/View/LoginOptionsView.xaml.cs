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
    /// Interaction logic for LoginOptionsView.xaml
    /// </summary>
    public partial class LoginOptionsView : UserControl
    {
        public LoginOptionsView()
        {
            InitializeComponent();
        }

        private void Application4Trial_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            System.Diagnostics.Process.Start("http://www.hjtcloud.com/try");
            e.Handled = true;
        }
    }
}
