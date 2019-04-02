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
    /// Interaction logic for GeneralOperationButton.xaml
    /// </summary>
    public partial class GeneralOperationButton : UserControl
    {
        public string TitleText
        {
            get { return (string)GetValue(TitleTextProperty); }
            set { SetValue(TitleTextProperty, value); }
        }
        public static readonly DependencyProperty TitleTextProperty =
            DependencyProperty.Register("TitleText", typeof(string), typeof(GeneralOperationButton), new UIPropertyMetadata(null));

        public string ContentText
        {
            get { return (string)GetValue(ContentTextProperty); }
            set { SetValue(ContentTextProperty, value); }
        }
        public static readonly DependencyProperty ContentTextProperty =
            DependencyProperty.Register("ContentText", typeof(string), typeof(GeneralOperationButton), new UIPropertyMetadata(null));

        public string ContentTextlabel
        {
            get { return (string)GetValue(ContentTextlabelProperty); }
            set { SetValue(ContentTextlabelProperty, value); }
        }
        public static readonly DependencyProperty ContentTextlabelProperty =
            DependencyProperty.Register("ContentTextlabel", typeof(string), typeof(GeneralOperationButton), new UIPropertyMetadata(null));

        public ImageSource ContentImage
        {
            get { return (ImageSource)GetValue(ContentImageProperty); }
            set { SetValue(ContentImageProperty, value); }
        }
        public static readonly DependencyProperty ContentImageProperty =
            DependencyProperty.Register("ContentImage", typeof(ImageSource), typeof(GeneralOperationButton), new UIPropertyMetadata(null));

        public ICommand Command
        {
            get { return (ICommand)GetValue(CommandProperty); }
            set { SetValue(CommandProperty, value); }
        }
        public static readonly DependencyProperty CommandProperty =
            DependencyProperty.Register("Command", typeof(ICommand), typeof(GeneralOperationButton), new UIPropertyMetadata(null));

        public Visibility VisibilityTextBlock
        {
            get { return (Visibility)GetValue(VisibilityTextBlockProperty); }
            set { SetValue(VisibilityTextBlockProperty, value); }
        }
        public static readonly DependencyProperty VisibilityTextBlockProperty =
            DependencyProperty.Register("VisibilityTextBlock", typeof(Visibility), typeof(GeneralOperationButton), new UIPropertyMetadata(null));


        public event RoutedEventHandler Click;

        public GeneralOperationButton()
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
