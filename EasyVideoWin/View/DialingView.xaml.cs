using EasyVideoWin.Helpers;
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
    /// Interaction logic for DialingView.xaml
    /// </summary>
    public partial class DialingView : UserControl
    {
        public DialingView()
        {
            InitializeComponent();

            //ImageBrush b = new ImageBrush();
            //b.ImageSource = new BitmapImage(new Uri("pack://application:,,,/Resources/Icons/background_default.png"));
            //b.Stretch = Stretch.Fill;
            //this.Background = b;
        }
    }
}
