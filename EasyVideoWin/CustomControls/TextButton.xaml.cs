using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace EasyVideoWin.CustomControls
{
    /// <summary>
    /// Interaction logic for TextButton.xaml
    /// </summary>
    public partial class TextButton : UserControl
    {
        public TextButton()
        {
            InitializeComponent();
        }

        public ICommand Command
        {
            get { return (ICommand)GetValue(CommandProperty); }
            set { SetValue(CommandProperty, value); }
        }
        public static readonly DependencyProperty CommandProperty =
            DependencyProperty.Register("Command", typeof(ICommand), typeof(TextButton), new UIPropertyMetadata(null));

        public static readonly DependencyProperty CommandParameterProperty =
            DependencyProperty.Register("CommandParameter", typeof(object), typeof(TextButton), new UIPropertyMetadata(null));
        public object CommandParameter
        {
            get { return (object)GetValue(CommandParameterProperty); }
            set { SetValue(CommandParameterProperty, value); }
        }
        
        public string ButtonContent
        {
            get { return (string)GetValue(ButtonContentProperty); }
            set { SetValue(ButtonContentProperty, value); }
        }
        public static readonly DependencyProperty ButtonContentProperty =
            DependencyProperty.Register("ButtonContent", typeof(string), typeof(TextButton), new UIPropertyMetadata(null));

        public double ButtonWidth
        {
            get { return (double)GetValue(ButtonWidthProperty); }
            set { SetValue(ButtonWidthProperty, value); }
        }
        public static readonly DependencyProperty ButtonWidthProperty =
            DependencyProperty.Register("ButtonWidth", typeof(double), typeof(TextButton), new UIPropertyMetadata(null));

        public double ButtonHeight
        {
            get { return (double)GetValue(ButtonHeightProperty); }
            set { SetValue(ButtonHeightProperty, value); }
        }
        public static readonly DependencyProperty ButtonHeightProperty =
            DependencyProperty.Register("ButtonHeight", typeof(double), typeof(TextButton), new UIPropertyMetadata(null));
        
        public Brush NormalForegroundColor
        {
            get { return (Brush)GetValue(NormalForegroundColorProperty); }
            set { SetValue(NormalForegroundColorProperty, value); }
        }
        public static readonly DependencyProperty NormalForegroundColorProperty =
            DependencyProperty.Register("NormalForegroundColor", typeof(Brush), typeof(TextButton), new UIPropertyMetadata(null));

        public Brush HoverForegroundColor
        {
            get { return (Brush)GetValue(HoverForegroundColorProperty); }
            set { SetValue(HoverForegroundColorProperty, value); }
        }
        public static readonly DependencyProperty HoverForegroundColorProperty =
            DependencyProperty.Register("HoverForegroundColor", typeof(Brush), typeof(TextButton), new UIPropertyMetadata(null));

        public Brush ClickForegroundColor
        {
            get { return (Brush)GetValue(ClickForegroundColorProperty); }
            set { SetValue(ClickForegroundColorProperty, value); }
        }
        public static readonly DependencyProperty ClickForegroundColorProperty =
            DependencyProperty.Register("ClickForegroundColor", typeof(Brush), typeof(TextButton), new UIPropertyMetadata(null));

        public Brush NormalBackgroundColor
        {
            get { return (Brush)GetValue(NormalBackgroundColorProperty); }
            set { SetValue(NormalBackgroundColorProperty, value); }
        }
        public static readonly DependencyProperty NormalBackgroundColorProperty =
            DependencyProperty.Register("NormalBackgroundColor", typeof(Brush), typeof(TextButton), new UIPropertyMetadata(null));

        public Brush HoverBackgroundColor
        {
            get { return (Brush)GetValue(HoverBackgroundColorProperty); }
            set { SetValue(HoverBackgroundColorProperty, value); }
        }
        public static readonly DependencyProperty HoverBackgroundColorProperty =
            DependencyProperty.Register("HoverBackgroundColor", typeof(Brush), typeof(TextButton), new UIPropertyMetadata(null));

        public Brush ClickBackgroundColor
        {
            get { return (Brush)GetValue(ClickBackgroundColorProperty); }
            set { SetValue(ClickBackgroundColorProperty, value); }
        }
        public static readonly DependencyProperty ClickBackgroundColorProperty =
            DependencyProperty.Register("ClickBackgroundColor", typeof(Brush), typeof(TextButton), new UIPropertyMetadata(null));

        public bool IsDefault
        {
            get { return (bool)GetValue(IsDefaultProperty); }
            set { SetValue(IsDefaultProperty, value); }
        }
        public static readonly DependencyProperty IsDefaultProperty =
            DependencyProperty.Register("IsDefault", typeof(bool), typeof(TextButton), new UIPropertyMetadata(null));

        public event RoutedEventHandler Click;

        private void button_Click(object sender, RoutedEventArgs e)
        {
            if (Click != null)
            {
                Click(this, e);
            }
        }

        private void Btn_MouseEnter(object sender, MouseEventArgs e)
        {
            if (null != HoverForegroundColor)
            {
                this.btn.Foreground = HoverForegroundColor;
            }
        }

        private void Btn_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (null != ClickForegroundColor)
            {
                this.btn.Foreground = ClickForegroundColor;
            }
        }

        private void Btn_MouseLeave(object sender, MouseEventArgs e)
        {
            this.btn.Foreground = NormalForegroundColor;
        }
        
    }
}
