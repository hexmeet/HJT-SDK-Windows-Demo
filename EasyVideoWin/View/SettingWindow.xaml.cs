using CefSharp;
using CefSharp.WinForms;
using EasyVideoWin.CustomControls;
using EasyVideoWin.Helpers;
using EasyVideoWin.Model;
using EasyVideoWin.Model.CloudModel;
using EasyVideoWin.WinForms;
using log4net;
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
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace EasyVideoWin.View
{
    /// <summary>
    /// Interaction logic for SettingWindow.xaml
    /// </summary>
    public partial class SettingWindow : Window, IMasterDisplayWindow
    {
        #region -- Members --

        private readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private SettingView _settingView = new SettingView();
        private IntPtr _handle;

        #endregion

        #region -- Properties --

        public bool IsDisposed { get; private set; } = false;

        #endregion

        #region -- Constructor --

        public SettingWindow()
        {
            InitializeComponent();
            
            this.contentPresenter.Content = _settingView;
            
        }
        
        #endregion

        #region -- Public Method --
        
        #endregion

        #region -- Protected Method --

        #endregion

        #region -- Private Method --
        
        private IntPtr DragHook(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            switch ((Utils.WM)msg)
            {
                case Utils.WM.WM_DPICHANGED:
                    Utils.RECT rect = (Utils.RECT)Marshal.PtrToStructure(lParam, typeof(Utils.RECT));
                    if (rect.left != -32000 && rect.top != -32000)
                    {
                        Utils.MySetWindowPos(hwnd, new System.Windows.Rect(rect.left, rect.top, rect.right - rect.left, rect.bottom - rect.top));
                    }
                    break;
            }
            return IntPtr.Zero;
        }
        
        private void CloseBtn_Click(object sender, RoutedEventArgs e)
        {
            this.Visibility = Visibility.Collapsed;
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
            return _handle;
        }

        public Rect GetWindowRect()
        {
            double dpiX;
            double dpiY;
            using (System.Drawing.Graphics g = System.Drawing.Graphics.FromHwnd(_handle))
            {
                dpiX = g.DpiX;
                dpiY = g.DpiY;
            }

            double top = WindowExtensions.ActualTop(this, _handle);
            double left = WindowExtensions.ActualLeft(this, _handle);
            double width = this.Width;
            double height = this.Height;
            if (WindowState.Maximized == this.WindowState)
            {
                left = left / (dpiX / 96d);
                top = top / (dpiY / 96d);

                if (this.ActualWidth > this.Width)
                {
                    width = this.ActualWidth;
                    height = this.ActualHeight;
                }
            }

            return new Rect(left, top, width, height);
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
