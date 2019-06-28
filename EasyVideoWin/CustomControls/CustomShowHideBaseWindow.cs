using EasyVideoWin.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interop;

namespace EasyVideoWin.CustomControls
{
    public partial class CustomShowHideBaseWindow : Window
    {
        /*
        * For normal window, when call the function 'Show' of window, the window will go to top and the other window that has focus will
        * go to bottom in z order. So change the window hiding way that moving window to a invisible place.
        * For example, when show receiving content window, changing the svc layout, the receiving content window will go to the bottom.
        */
        #region -- Members --

        private IntPtr _handle = IntPtr.Zero;
        private const int HIDE_WINDOW_LEFT = -20000;
        private const int HIDE_WINDOW_TOP = -20000;
        protected bool _isWindowHidden = false; // In surface, when set the window hidden manually with -20000 in both left and top, but the actual value is not -20000 

        protected double _rightTop;
        protected double _rightLeft;

        #endregion

        #region -- Properties --
        
        public bool IsWindowHidden
        {
            get
            {
                return _isWindowHidden;
            }
        }

        public IntPtr Handle
        {
            get
            {
                if (IntPtr.Zero == _handle)
                {
                    _handle = new WindowInteropHelper(this).Handle;
                }
                return _handle;
            }
        }

        #endregion

        #region -- Constructor --

        public CustomShowHideBaseWindow()
        {
            this.Loaded += CustomShowHideBaseWindow_Loaded;
        }
        
        #endregion

        #region -- Protected Methods --

        virtual protected void OnWindowHidden()
        {

        }

        virtual protected void OnWindowSizeChanged(double width, double height)
        {

        }

        #endregion

        #region -- Public Methods --

        public bool SetProperWindowPos(double left, double top, double width, double height, bool forceUpdate = false)
        {
            if (IntPtr.Zero == _handle)
            {
                return false;
            }

            // check if the value is changed to avoid UI refresh for OOM
            if (this.Left == left && this.Top == top && this.Width == width && this.Height == height && !forceUpdate)
            {
                return false;
            }

            _isWindowHidden = false;
            if (forceUpdate)
            {
                this.Left   = left;
                this.Top    = top;
                this.Width  = width;
                this.Height = height;
            }
            else
            {
                if (this.Left != left)
                {
                    this.Left = left;
                }

                if (this.Top != top)
                {
                    this.Top = top;
                }

                if (this.Width != width)
                {
                    this.Width = width;
                }

                if (this.Height != height)
                {
                    this.Height = height;
                }
            }
            
            //Utils.SetWindowPosSmoothly(_handle, left, top, width, height);

            this._rightLeft = left;
            this._rightTop = top;

            OnWindowSizeChanged(width, height);

            return true;
        }

        public bool SetProperWindowPos(double left, double top)
        {
            if (IntPtr.Zero == _handle)
            {
                return false;
            }

            // check if the value is changed to avoid UI refresh for OOM
            if (this.Left == left && this.Top == top)
            {
                return false;
            }

            _isWindowHidden = false;

            if (this.Left != left)
            {
                this.Left = left;
            }

            if (this.Top != top)
            {
                this.Top = top;
            }
            
            //Utils.SetWindowPosSmoothly(_handle, left, top, this.Width, this.Height);

            this._rightLeft = left;
            this._rightTop = top;
            return true;
        }

        public void MoveWindowPos(double x, double y)
        {
            if (this._isWindowHidden)
            {
                this._rightLeft += x;
                this._rightTop += y;
                return;
            }

            SetProperWindowPos(this.Left + x, this.Top + y);
        }

        public bool SetProperWindowSize(double width, double height)
        {
            // check if the value is changed to avoid UI refresh for OOM
            if (width == this.Width && height == this.Height)
            {
                return false;
            }

            if (width != this.Width)
            {
                this.Width = width;
            }

            if (height != this.Height)
            {
                this.Height = height;
            }
            
            OnWindowSizeChanged(width, height);

            return true;
        }

        public void ShowWindow()
        {
            if (IntPtr.Zero == _handle)
            {
                return;
            }

            if (this.Left != this._rightLeft)
            {
                this.Left = this._rightLeft;
            }

            if (this.Top != this._rightTop)
            {
                this.Top = this._rightTop;
            }
            //Utils.SetWindowPosSmoothly(_handle, this._rightLeft, this._rightTop, this.Width, this.Height);

            this._isWindowHidden = false;
        }

        public void HideWindow()
        {
            if (_isWindowHidden)
            {
                // if window is hidden, don't hide it again to avoid the incorrect position.
                return;
            }
            this._isWindowHidden = true;
            this._rightLeft = this.Left;
            this._rightTop = this.Top;
            this.Left = HIDE_WINDOW_LEFT;
            this.Top = HIDE_WINDOW_TOP;
            
            OnWindowHidden();
        }

        #endregion

        #region -- Private Methods --

        private void CustomShowHideBaseWindow_Loaded(object sender, RoutedEventArgs e)
        {
            if (IntPtr.Zero == _handle)
            {
                _handle = new WindowInteropHelper(this).Handle;
            }
        }

        #endregion
    }
}
