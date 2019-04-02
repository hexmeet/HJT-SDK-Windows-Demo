using System.Windows;
using System.Windows.Controls;

namespace EasyVideoWin.View
{
    /// <summary>
    /// Interaction logic for CallingView.xaml
    /// </summary>
    public partial class CallingView : UserControl
    {
        private Window _window;
        private bool _firstLoad;

        public CallingView()
        {
            InitializeComponent();

            this._firstLoad = true;
            this.Loaded += CallingView_Loaded;
        }

        private void CallingView_Loaded(object sender, RoutedEventArgs e)
        {
            if (this._firstLoad)
            {
                _window = Window.GetWindow(this);
                AdjustWindowSize();
                _window.SizeChanged += Window_SizeChanged;
            }

            this._firstLoad = false;
        }

        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            AdjustWindowSize();
        }

        private void AdjustWindowSize()
        {
            Rect mainWindowRect = VideoPeopleWindow.Instance.GetWindowRect();
            this.Height = mainWindowRect.Height;
        }
    }
}
