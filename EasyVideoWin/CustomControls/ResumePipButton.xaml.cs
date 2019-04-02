using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace EasyVideoWin.CustomControls
{
    /// <summary>
    /// Interaction logic for ResumePipButton.xaml
    /// </summary>
    public partial class ResumePipButton : UserControl
    {
        public ResumePipButton()
        {
            InitializeComponent();
        }

        public ICommand Command
        {
            get { return (ICommand)GetValue(CommandProperty); }
            set { SetValue(CommandProperty, value); }
        }
        public static readonly DependencyProperty CommandProperty =
            DependencyProperty.Register("Command", typeof(ICommand), typeof(ResumePipButton), new UIPropertyMetadata(null));

        public static readonly DependencyProperty CommandParameterProperty =
            DependencyProperty.Register("CommandParameter", typeof(object), typeof(ResumePipButton), new UIPropertyMetadata(null));
        public object CommandParameter
        {
            get { return (object)GetValue(CommandParameterProperty); }
            set { SetValue(CommandParameterProperty, value); }
        }

        public event RoutedEventHandler Click;

        private void button_Click(object sender, RoutedEventArgs e)
        {
            if (Click != null)
            {
                Click(this, e);
            }
        }

        public ImageSource NormalImage
        {
            get { return (ImageSource)GetValue(NormalImageProperty); }
            set { SetValue(NormalImageProperty, value); }
        }
        public static readonly DependencyProperty NormalImageProperty =
            DependencyProperty.Register("NormalImage", typeof(ImageSource), typeof(ResumePipButton), new UIPropertyMetadata(null));

        public ImageSource MouseOverImage
        {
            get { return (ImageSource)GetValue(MouseOverImageProperty); }
            set { SetValue(MouseOverImageProperty, value); }
        }
        public static readonly DependencyProperty MouseOverImageProperty =
            DependencyProperty.Register("MouseOverImage", typeof(ImageSource), typeof(ResumePipButton), new UIPropertyMetadata(null));

        public Brush NormalTextForeground
        {
            get { return (Brush)GetValue(NormalTextForegroundProperty); }
            set { SetValue(NormalTextForegroundProperty, value); }
        }
        public static readonly DependencyProperty NormalTextForegroundProperty =
            DependencyProperty.Register("NormalTextForeground", typeof(Brush), typeof(ResumePipButton), new UIPropertyMetadata(null));

        public Brush MouseOverTextForeground
        {
            get { return (Brush)GetValue(MouseOverTextForegroundProperty); }
            set { SetValue(MouseOverTextForegroundProperty, value); }
        }
        public static readonly DependencyProperty MouseOverTextForegroundProperty =
            DependencyProperty.Register("MouseOverTextForeground", typeof(Brush), typeof(ResumePipButton), new UIPropertyMetadata(null));

        public string ButtonText
        {
            get { return (string)GetValue(ButtonTextProperty); }
            set { SetValue(ButtonTextProperty, value); }
        }
        public static readonly DependencyProperty ButtonTextProperty =
            DependencyProperty.Register("ButtonText", typeof(string), typeof(ResumePipButton), new UIPropertyMetadata(null));
        
    }
}
