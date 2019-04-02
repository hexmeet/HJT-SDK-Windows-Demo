using EasyVideoWin.CustomControls;
using EasyVideoWin.Helpers;
using System;
using System.Collections.Generic;
using System.ComponentModel;
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
    /// Interaction logic for LayoutNormalCellsSectionWindow.xaml
    /// </summary>
    public partial class LayoutNormalCellsSectionWindow : CustomShowHideBaseWindow, INotifyPropertyChanged
    {
        #region -- Members --

        public delegate void WindowMoveHandler(double x, double y);
        public event WindowMoveHandler OnWindowMoved;

        public delegate void CellsNavigateChangedHandler(bool isUp);
        public event CellsNavigateChangedHandler CellsNavigateChanged;
        public delegate void CustomWindowStateChangedHandler(WindowState windowState);
        public event CustomWindowStateChangedHandler CustomWindowStateChanged;

        private Border[] _decorationBorder = new Border[0];
        private int _emptyDecorationBorderIdx = 0;
        private Point _position = new Point(0, 0);
        public event PropertyChangedEventHandler PropertyChanged;

        public delegate void DpiChangedHandler();
        public event DpiChangedHandler DpiChanged;

        private SolidColorBrush _toolbarNormalColorBrush = new SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#313131"));

        #endregion

        #region -- Properties --

        
        #endregion

        #region -- Constructor --

        public LayoutNormalCellsSectionWindow()
        {
            InitializeComponent();
            
            this.Loaded += LayoutNormalCellsSectionWindow_Loaded;
        }
        
        #endregion

        #region -- Public Methods --

        public void InitDecorationBorder(int count)
        {
            _decorationBorder = new Border[count];
            _emptyDecorationBorderIdx = 0;
            for (int i=0; i< _decorationBorder.Length; ++i)
            {
                _decorationBorder[i] = new Border();
                this.sectionGrid.Children.Add(_decorationBorder[i]);
                _decorationBorder[i].Visibility = Visibility.Collapsed;
                _decorationBorder[i].VerticalAlignment = VerticalAlignment.Top;
                _decorationBorder[i].BorderThickness = new Thickness(2);
            }
        }

        public void ResetDecorationBorders()
        {
            _emptyDecorationBorderIdx = 0;
            for (int i = 0; i < _decorationBorder.Length; ++i)
            {
                _decorationBorder[i].Visibility = Visibility.Collapsed;
            }
        }

        public Border GetValidBorder()
        {
            if (_emptyDecorationBorderIdx >= _decorationBorder.Length)
            {
                return null;
            }

            return _decorationBorder[_emptyDecorationBorderIdx++];
        }

        public void HideToolbar()
        {
            if (Visibility.Collapsed == this.btnMinWindow.Visibility)
            {
                return;
            }
            this.topToolbarGrid.Background = Brushes.Transparent;
            this.btnCellsNavigateUp.Visibility = Visibility.Collapsed;
            this.btnMinWindow.Visibility = Visibility.Collapsed;
            this.bottomToolbarGrid.Background = Brushes.Transparent;
            this.btnCellsNavigateDown.Visibility = Visibility.Collapsed;
        }

        public void ShowToolbar(bool showNavigateUp, bool showNavigateDown)
        {
            if (showNavigateUp && Visibility.Visible != this.btnCellsNavigateUp.Visibility)
            {
                this.btnCellsNavigateUp.Visibility = Visibility.Visible;
            }

            if (Visibility.Visible == this.btnMinWindow.Visibility)
            {
                return;
            }

            this.topToolbarGrid.Background = _toolbarNormalColorBrush;
            this.btnMinWindow.Visibility = Visibility.Visible;
            if (showNavigateDown && Visibility.Visible != this.btnCellsNavigateDown.Visibility)
            {
                this.bottomToolbarGrid.Background = _toolbarNormalColorBrush;
                this.btnCellsNavigateDown.Visibility = Visibility.Visible;
            }
        }

        #endregion

        #region -- Protected Methods --

        protected override void OnWindowSizeChanged(double width, double height)
        {
            this.sectionStackPanel.Width = this.sectionGrid.Width = width;
            this.sectionStackPanel.Height = this.sectionGrid.Height = height;
            this.topBar.Height = this.bottomBar.Height = new GridLength(width / 180 * 26);
            this.btnCellsNavigateUp.Width = this.btnCellsNavigateUp.Height =
                this.btnMinWindow.Width = this.btnMinWindow.Height =
                this.btnCellsNavigateDown.Width = this.btnCellsNavigateDown.Height =
                width / 180 * 14;
        }

        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonDown(e);
            this.DragMove();
        }

        protected override void OnLocationChanged(EventArgs e)
        {
            base.OnLocationChanged(e);

            if (!double.IsNaN(_position.X) && !double.IsNaN(_position.Y))
            {
                double x = this.Left - _position.X;
                double y = this.Top - _position.Y;

                OnWindowMoved?.Invoke(x, y);
            }
            _position.X = this.Left;
            _position.Y = this.Top;
            if (!_isWindowHidden)
            {
                _rightLeft = this.Left;
                _rightTop = this.Top;
            }
            
        }

        #endregion

        #region -- Private Methods --

        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void LayoutNormalCellsSectionWindow_Loaded(object sender, RoutedEventArgs e)
        {
            Utils.HideWindowInAltTab(this.Handle);
            HwndSource hwndSource = (HwndSource)HwndSource.FromVisual((Window)sender);
            hwndSource.AddHook(DragHook);
        }

        private void BtnCellsNavigateUp_Click(object sender, RoutedEventArgs e)
        {
            CellsNavigateChanged?.Invoke(true);
        }

        private void BtnCellsNavigateDown_Click(object sender, RoutedEventArgs e)
        {
            CellsNavigateChanged?.Invoke(false);
        }

        private void BtnMinWindow_Click(object sender, RoutedEventArgs e)
        {
            CustomWindowStateChanged?.Invoke(WindowState.Minimized);
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
                        DpiChanged?.Invoke();
                    }
                    break;
            }
            return IntPtr.Zero;
        }

        #endregion
    }
}
