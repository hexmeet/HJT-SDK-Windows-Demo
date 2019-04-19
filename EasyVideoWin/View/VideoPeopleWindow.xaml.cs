using EasyVideoWin.CustomControls;
using EasyVideoWin.Helpers;
using EasyVideoWin.Model;
using EasyVideoWin.ViewModel;
using log4net;
using System;
using System.Collections.Generic;
using System.ComponentModel;
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
using System.Windows.Shapes;

namespace EasyVideoWin.View
{
    /// <summary>
    /// Interaction logic for VideoPeopleWindow.xaml
    /// </summary>
    public partial class VideoPeopleWindow : FullScreenBaseWindow
    {
        #region -- Members --

        private readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private const double INITIAL_WIDTH = 1280d;
        private const double INITIAL_HEIGHT = 760d;
        private const double TITLEBAR_HEIGHT = 40d;

        private static VideoPeopleWindow _instance = null; // new VideoPeopleWindow();
        private VideoPeopleWindowViewModel _viewModel;

        #endregion

        #region -- Properties --

        public static VideoPeopleWindow Instance
        {
            get
            {
                return _instance;
            }
            set
            {
                _instance = value;
            }
        }

        public bool FirstShow { get; set; } = true;

        #endregion

        #region -- Constructor --

        public VideoPeopleWindow() : base(INITIAL_WIDTH, INITIAL_HEIGHT, TITLEBAR_HEIGHT, true)
        {
            InitializeComponent();

            log.InfoFormat("VideoPeopleWindow hash code:{0}", this.GetHashCode());

            this.Title = System.Reflection.Assembly.GetExecutingAssembly().GetName().Name;
            //CallController.Instance.CallStatusChanged += OnCallStatusChanged;

            _viewModel = this.DataContext as VideoPeopleWindowViewModel;
            
            this.IsVisibleChanged += VideoPeopleWindow_IsVisibleChanged;
            this.SizeChanged += VideoPeopleWindow_SizeChanged;
        }
        
        #endregion

        #region -- Public Method --

        public void Init()
        {
            this.Background = Brushes.White;
            this.contentPanel.Visibility = Visibility.Collapsed;
        }

        public void ResumeNormalSetting()
        {
            this.Background = Brushes.Black;
            contentPanel.Visibility = Visibility.Visible;
        }

        public void UpdatePresettingState()
        {
            _presettingState = WindowState;
        }

        public void Set2PresettingState()
        {
            ChangeWindowState(_presettingState);
            Activate();
        }

        public void ResetInitialSize()
        {
            this.Width = INITIAL_WIDTH;
            this.Height = INITIAL_HEIGHT;
        }

        #endregion

        #region -- Protected Method --

        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonDown(e);
            this.DragMove();
        }

        protected override void OnClosed(EventArgs e)
        {
            log.Info("Window on closed");
            base.OnClosed(e);
            Application.Current.MainWindow?.Close();
        }

        #endregion

        #region -- Private Method --

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            this.StateChanged += VideoPeopleWindow_StateChanged;
            LayoutBackgroundWindow.Instance.InitRelevantElements();
        }
        
        private void MinimizeMainWindow(object sender, RoutedEventArgs e)
        {
            MinimizeWindow();
        }

        private void MaximizeMainWindow(object sender, RoutedEventArgs e)
        {
            MaximizeWindow();
        }

        private void RestoreMainWindow(object sender, RoutedEventArgs e)
        {
            RestoreWindow();
        }

        private void ChangeWindowFullScreenState(object sender, RoutedEventArgs e)
        {
            SetFullScreen();
        }
        
        private void VideoPeopleWindow_StateChanged(object sender, EventArgs e)
        {
            this.minBtn.Visibility = FullScreenStatus ? Visibility.Collapsed : Visibility.Visible;
            this.maxBtn.Visibility = (FullScreenStatus || WindowState == WindowState.Maximized) ? Visibility.Collapsed : Visibility.Visible;
            this.restoreBtn.Visibility = (FullScreenStatus || WindowState != WindowState.Maximized) ? Visibility.Collapsed : Visibility.Visible;
            this.fullScreenBtn.Visibility = FullScreenStatus ? Visibility.Collapsed : Visibility.Visible;
            this.titleBar.Visibility = FullScreenStatus ? Visibility.Collapsed : Visibility.Visible;
        }

        private void VideoPeopleWindow_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (Visibility.Visible == this.Visibility)
            {
                VideoPeopleWindow_StateChanged(null, null);
            }
        }

        private void VideoPeopleWindow_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            log.InfoFormat("SizeChanged, new:{0}x{1}, previous:{2}x{3}", e.NewSize.Width, e.NewSize.Height, e.PreviousSize.Width, e.PreviousSize.Height);
        }

        #endregion
    }
}
