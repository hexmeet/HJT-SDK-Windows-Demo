using EasyVideoWin.CustomControls;
using EasyVideoWin.Helpers;
using EasyVideoWin.Model;
using EasyVideoWin.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
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
using System.ComponentModel;
using log4net;
using System.Windows.Interop;

namespace EasyVideoWin.View
{
    /// <summary>
    /// Interaction logic for JoinConfWindow.xaml
    /// </summary>
    public partial class JoinConfWindow : Window, IMasterDisplayWindow
    {
        #region -- Members --

        private readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private const double WINDOW_DESIGN_WIDTH = 530;
        private LoginManager _loginMgr = LoginManager.Instance;
        private IntPtr _handle = IntPtr.Zero;

        #endregion

        #region -- Properties --

        #endregion

        #region -- Constructor --

        public JoinConfWindow(string confNumber, string confPassword)
        {
            log.Info("Begin join conf window construction.");
            InitializeComponent();
            log.Info("Component finished.");
            JoinConfWindowModel vm = this.DataContext as JoinConfWindowModel;
            vm.SetConfNumberPassword(confNumber, confPassword);

            CallController.Instance.CallStatusChanged += OnCallStatusChanged;
            log.Info("Join conf window constructed.");
        }

        #endregion

        #region -- Public Method --

        #endregion

        #region -- Protected Method --

        #endregion

        #region -- Private Method --

        protected override void OnClosing(CancelEventArgs e)
        {
            CallController.Instance.CallStatusChanged -= OnCallStatusChanged;
        }

        private IntPtr DragHook(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            switch ((Utils.WM)msg)
            {
                case Utils.WM.WM_DPICHANGED:
                    Utils.RECT rect = (Utils.RECT)Marshal.PtrToStructure(lParam, typeof(Utils.RECT));
                    if (rect.left != -32000 && rect.top != -32000)
                    {
                        Utils.MySetWindowPos(hwnd, new Rect(rect.left, rect.top, rect.right - rect.left, rect.bottom - rect.top));
                    }
                    break;
            }
            return IntPtr.Zero;
        }

        private void CloseBtn_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void MinBtn_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            log.Info("Adjust window postion begin");
            DisplayUtil.AdjustWindowPosition(this, VideoPeopleWindow.Instance);
            log.Info("Adjust window postion end");
        }

        private void BackGround_MouseMove(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                if (!(e.OriginalSource is TextBox))
                {
                    this.DragMove();
                }
            }
        }

        private void OnCallStatusChanged(object sender, CallStatus status)
        {
            log.Info("OnCallStatusChanged start.");
            switch (status)
            {
                case CallStatus.Dialing:
                case CallStatus.ConfIncoming:
                case CallStatus.P2pIncoming:
                case CallStatus.P2pOutgoing:
                case CallStatus.Connected:
                    App.Current.Dispatcher.InvokeAsync(() => {
                        this.Close();
                    });
                    break;
                default:
                    break;
            }
            
            log.Info("OnCallStatusChanged end.");
        }

        #endregion

        #region implement IMessageBoxOwner
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
            return 1; // this.Width / WINDOW_DESIGN_WIDTH;
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
