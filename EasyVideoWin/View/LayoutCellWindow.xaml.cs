using EasyVideoWin.CustomControls;
using EasyVideoWin.Helpers;
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
    /// Interaction logic for LayoutCellWindow.xaml
    /// </summary>
    public partial class LayoutCellWindow : CustomShowHideBaseWindow
    {
        #region -- Members --

        public delegate void WindowMoveHandler(object sender, double x, double y);
        public event WindowMoveHandler OnWindowMoved;

        private Point _position = new Point(0, 0);

        public delegate void DpiChangedHandler(object sender);
        public event DpiChangedHandler DpiChanged;

        private string _cellName;

        #endregion

        #region -- Properties --

        public string CellName
        {
            get
            {
                return _cellName;
            }
            set
            {
                _cellName = value;
                Operationbar.CellName = _cellName;
            }
        }
        
        public ulong DeviceId { get; set; }

        public LayoutCellOperationbarWindow Operationbar { get; set; } = new LayoutCellOperationbarWindow();

        #endregion

        #region -- Constructor --

        public LayoutCellWindow()
        {
            InitializeComponent();
            this.Loaded += LayoutCellWindow_Loaded;
        }

        private void LayoutCellWindow_Loaded(object sender, RoutedEventArgs e)
        {
            Utils.HideWindowInAltTab(this.Handle);
            HwndSource hwndSource = (HwndSource)HwndSource.FromVisual((Window)sender);
            hwndSource.AddHook(DragHook);
        }

        #endregion

        #region -- Public Methods --

        #endregion

        #region -- Protected Methods --
        
        protected override void OnLocationChanged(EventArgs e)
        {
            base.OnLocationChanged(e);

            if (!double.IsNaN(_position.X) && !double.IsNaN(_position.Y))
            {
                double x = this.Left - _position.X;
                double y = this.Top - _position.Y;

                OnWindowMoved?.Invoke(this, x, y);
            }
            _position.X = this.Left;
            _position.Y = this.Top;
        }

        protected override void OnWindowHidden()
        {
            base.OnWindowHidden();
            Operationbar.HideWindow();
        }

        #endregion

        #region -- Private Methods --

        private IntPtr DragHook(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            switch ((Utils.WM)msg)
            {
                case Utils.WM.WM_DPICHANGED:
                    Utils.RECT rect = (Utils.RECT)Marshal.PtrToStructure(lParam, typeof(Utils.RECT));
                    if (rect.left != -32000 && rect.top != -32000)
                    {
                        Utils.MySetWindowPos(hwnd, new Rect(rect.left, rect.top, rect.right - rect.left, rect.bottom - rect.top));
                        DpiChanged?.Invoke(this);
                    }
                    break;
            }
            return IntPtr.Zero;
        }

        #endregion

    }
}
