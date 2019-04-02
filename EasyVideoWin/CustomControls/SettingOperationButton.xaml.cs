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
    /// Interaction logic for SettingOperationButton.xaml
    /// </summary>
    public partial class SettingOperationButton : UserControl
    {
        public string TitleText
        {
            get { return (string)GetValue(TitleTextProperty); }
            set { SetValue(TitleTextProperty, value); }
        }
        public static readonly DependencyProperty TitleTextProperty =
            DependencyProperty.Register("TitleText", typeof(string), typeof(SettingOperationButton), new UIPropertyMetadata(null));

        public ImageSource FlagImage
        {
            get { return (ImageSource)GetValue(FlagImageProperty); }
            set { SetValue(FlagImageProperty, value); }
        }
        public static readonly DependencyProperty FlagImageProperty =
            DependencyProperty.Register("FlagImage", typeof(ImageSource), typeof(SettingOperationButton), new UIPropertyMetadata(null));

        public bool IsChecked
        {
            get { return (bool)GetValue(IsCheckedProperty); }
            set { SetValue(IsCheckedProperty, value); }
        }
        public static readonly DependencyProperty IsCheckedProperty =
            DependencyProperty.Register("IsChecked", typeof(bool), typeof(SettingOperationButton), new UIPropertyMetadata(null));


        public event RoutedEventHandler Click;

        public SettingOperationButton()
        {
            InitializeComponent();
        }

        private void button_Click(object sender, RoutedEventArgs e)
        {
            if (Click != null)
            {
                Click(this, e);
            }
        }
    }
}
