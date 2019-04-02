using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace EasyVideoWin.CustomControls
{
    /// <summary>
    /// Interaction logic for LayoutOperationbarVerticalButton.xaml
    /// </summary>
    public partial class LayoutOperationbarVerticalButton : UserControl
    {
        public LayoutOperationbarVerticalButton()
        {
            InitializeComponent();
        }

        public ICommand Command
        {
            get { return (ICommand)GetValue(CommandProperty); }
            set { SetValue(CommandProperty, value); }
        }
        public static readonly DependencyProperty CommandProperty =
            DependencyProperty.Register("Command", typeof(ICommand), typeof(LayoutOperationbarVerticalButton), new UIPropertyMetadata(null));

        public static readonly DependencyProperty CommandParameterProperty =
            DependencyProperty.Register("CommandParameter", typeof(object), typeof(LayoutOperationbarVerticalButton), new UIPropertyMetadata(null));
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
            DependencyProperty.Register("NormalImage", typeof(ImageSource), typeof(LayoutOperationbarVerticalButton), new UIPropertyMetadata(null));

        public string ButtonText
        {
            get { return (string)GetValue(ButtonTextProperty); }
            set { SetValue(ButtonTextProperty, value); }
        }
        public static readonly DependencyProperty ButtonTextProperty =
            DependencyProperty.Register("ButtonText", typeof(string), typeof(LayoutOperationbarVerticalButton), new UIPropertyMetadata(null));

        public string ExtraInfoText
        {
            get { return (string)GetValue(ExtraInfoTextProperty); }
            set { SetValue(ExtraInfoTextProperty, value); }
        }
        public static readonly DependencyProperty ExtraInfoTextProperty =
            DependencyProperty.Register("ExtraInfoText", typeof(string), typeof(LayoutOperationbarVerticalButton), new UIPropertyMetadata(null));
    }
}
