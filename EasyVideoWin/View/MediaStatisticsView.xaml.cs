using System.Windows;
using EasyVideoWin.ViewModel;
using EasyVideoWin.Model;
using EasyVideoWin.Helpers;
using System.Windows.Input;
using System;
using System.Windows.Interop;
using System.Runtime.InteropServices;
using log4net;

namespace EasyVideoWin.View
{
    /// <summary>
    /// Interaction logic for MediaStatistics.xaml
    /// </summary>
    public partial class MediaStatisticsView : Window
    {
        private readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private static MediaStatisticsView _instance = null;

        public static MediaStatisticsView Instance
        {
            get
            {
                // Note: don't instantiate below to avoid necessarily instantiate e.g. in layoutbackgroundwindow
                //if(_instance == null)
                //{
                //    _instance = new MediaStatisticsView();
                //}
                return _instance;
            }
            set
            {
                _instance = value;
            }
        }

        public void Shows()
        {
            this.Show();
            this.Focus();
        }

        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonDown(e);
            this.DragMove();
        }

        protected override void OnClosed(EventArgs e)
        {
            CallController.Instance.CallStatusChanged -= OnCallStatusChanged;
            MediaStatisticsViewModel mediaStatisticsViewModel = this.DataContext as MediaStatisticsViewModel;
            if (null != mediaStatisticsViewModel)
            {
                mediaStatisticsViewModel.StopRefreshMediaStatistics();
            }
            _instance = null;
            base.OnClosed(e);
        }

        public MediaStatisticsView()
        {
            InitializeComponent();

            MediaStatisticsViewModel mediaStatisticsViewModel = this.DataContext as MediaStatisticsViewModel;
            if (null != mediaStatisticsViewModel)
            {
                mediaStatisticsViewModel.StartRefreshMediaStatistics();
            }

            CallController.Instance.CallStatusChanged += OnCallStatusChanged;
            this.SourceInitialized += Window_SourceInitialized;
        }
        
        private void OnCloseBtnClick(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void OnCallStatusChanged(object sender, CallStatus status)
        {
            log.Info("OnCallStatusChanged start.");
            if (status == CallStatus.Ended)
            {
                Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    this.Close();
                });
            }
            log.Info("OnCallStatusChanged end.");
        }
        
        private void Window_SourceInitialized(object sender, EventArgs ea)
        {
            HwndSource hwndSource = (HwndSource)HwndSource.FromVisual((Window)sender);
            hwndSource.AddHook(DragHook);
        }

        private IntPtr DragHook(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            if(((WM)msg == WM.WM_DPICHANGED))
            {
                RECT rect = (RECT)Marshal.PtrToStructure(lParam, typeof(RECT));
                Utils.MySetWindowPos(hwnd, new Rect(rect.left, rect.top, rect.right - rect.left, rect.bottom - rect.top));
            }
            return IntPtr.Zero;
        }

        internal enum WM
        {
            WM_DPICHANGED = 0x02E0,
        }

        internal struct RECT
        {
            public int left;
            public int top;
            public int right;
            public int bottom;
        }
    }
}
