using EasyVideoWin.Helpers;
using EasyVideoWin.ViewModel;
using System;
using System.Windows;
using System.Windows.Interop;

namespace EasyVideoWin.View
{
    /// <summary>
    /// Interaction logic for SoftwareUpdateWindow.xaml
    /// </summary>
    public partial class SoftwareUpdateWindow : Window, IMasterDisplayWindow
    {
        #region -- Members --

        private static SoftwareUpdateWindow _instance = null;
        private SoftwareUpdateWindowModel _viewModel;
        private IntPtr _handle = IntPtr.Zero;

        #endregion

        #region -- Properties 

        public static SoftwareUpdateWindow Instance
        {
            get
            {
                if (null == _instance)
                {
                    _instance = new SoftwareUpdateWindow();
                }
                return _instance;
            }
        }
        
        public IntPtr Handle
        {
            get
            {
                return _handle;
            }
        }

        #endregion

        #region -- Constructor --

        public SoftwareUpdateWindow()
        {
            InitializeComponent();

            _viewModel = this.DataContext as SoftwareUpdateWindowModel;
            this.IsVisibleChanged += SoftwareUpdateWindow_IsVisibleChanged;
            this.Loaded += SoftwareUpdateWindow_Loaded;
        }
        
        #endregion

        #region -- Public Method --

        public void Reset(string version, string downloadUrl)
        {
            _viewModel.Reset(version, downloadUrl);
        }

        #endregion

        #region -- Protected Method --



        #endregion

        #region -- Private Method --

        private void CloseBtn_Click(object sender, RoutedEventArgs e)
        {
            _viewModel.Cancel();
            this.Visibility = Visibility.Collapsed;
        }
        
        private void ConfirmBtn_Click(object sender, RoutedEventArgs e)
        {
            if (_viewModel.IsLastestStep())
            {
                this.Visibility = Visibility.Collapsed;
            }
            // put the following codes to Application.Current.Dispatcher.InvokeAsync to hide the window quickly, or the window hide slowly
            Application.Current.Dispatcher.InvokeAsync(() =>
            {
                _viewModel.Confirm();
            });
        }

        private void SoftwareUpdateWindow_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (Visibility.Visible != this.Visibility)
            {
                if (null != SoftwareUpdateWindow.Instance.Owner)
                {
                    // set the owner active or the owner will hide when cancel the software update window.
                    SoftwareUpdateWindow.Instance.Owner.Activate();
                }
            }
        }

        private void SoftwareUpdateWindow_Loaded(object sender, RoutedEventArgs e)
        {
            if (IntPtr.Zero == _handle)
            {
                _handle = new WindowInteropHelper(this).Handle;
            }
        }
        
        #endregion

        #region -- IMessageBoxOwner --
        public double GetWidth()
        {
            return this.Width;
        }

        public double GetHeight()
        {
            return this.Height;
        }

        public double GetLeft()
        {
            return this.Left;
        }

        public double GetTop()
        {
            return this.Top;
        }

        public double GetSizeRatio()
        {
            return 1; // this.Width / _initialWidth;
        }

        public Window GetWindow()
        {
            return this;
        }

        public IntPtr GetHandle()
        {
            if (IntPtr.Zero == _handle)
            {
                _handle = new WindowInteropHelper(this).Handle;
            }
            return _handle;
        }

        public Rect GetWindowRect()
        {
            return new Rect(this.Left, this.Top, this.Width, this.Height);
        }

        public double GetInitialWidth()
        {
            return this.Width;
        }

        public double GetInitialHeight()
        {
            return this.Height;
        }

        #endregion
    }
}
