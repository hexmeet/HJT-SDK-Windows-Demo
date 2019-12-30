using EasyVideoWin.Helpers;
using log4net;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Interop;

namespace EasyVideoWin.CustomControls
{
    public class FullScreenBaseWindow : Window, IMasterDisplayWindow, INotifyPropertyChanged, IDisposable
    {
        #region -- Members --

        private readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public delegate void TitleBarWindowMoveHandler(double x, double y);
        public event TitleBarWindowMoveHandler TitleBarWindowMove;
        public delegate void DpiChangedHandler();
        public event DpiChangedHandler DpiChanged;

        protected WindowState _presettingState;
        public event PropertyChangedEventHandler PropertyChanged;

        private int _maxWindowWidth = 0;
        private int _maxWindowHeight = 0;
        private DateTime _displaySettingChangedTime = DateTime.Now;

        private Point _position = new Point(0, 0);

        private IntPtr _handle;
        
        private readonly double _initialWidth;
        private readonly double _initialHeight;
        private readonly double _titlebarHeight;

        // for rssize keeping aspect ratio
        private double _aspectRatio;
        private bool? _adjustingHeight = null;
        private bool _maximized = false;
        private bool _maxWinWhenLoaded = false;
        private bool _fullScreenStatus = false;
        private bool _notAdjustSizeOnLoaded = false;

        #endregion

        #region -- Properties --

        public double InitialWidth
        {
            get
            {
                return _initialWidth;
            }
        }

        public double InitialHeight
        {
            get
            {
                return _initialHeight;
            }
        }

        public double TitlebarHeight
        {
            get
            {
                return _titlebarHeight;
            }
        }

        public bool FullScreenStatus
        {
            get
            {
                return _fullScreenStatus;
            }
            set
            {
                if (_fullScreenStatus != value)
                {
                    _fullScreenStatus = value;
                    OnPropertyChanged("FullScreenStatus");
                }
            }
        }
        public double DpiX { get; set; }
        public double DpiY { get; set; }

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
            set
            {
                _handle = value;
            }
        }

        public bool IsDisposed { get; set; } = false;

        #endregion

        #region -- Constructor --

        public FullScreenBaseWindow()
        {
            this.Loaded += FullScreenBaseWindow_Loaded;
        }

        public FullScreenBaseWindow(double initialWidth, double initialHeight, double titlebarHeight = 0, bool notAdjustSizeOnLoaded = false)
        {
            _initialWidth = initialWidth;
            _initialHeight = initialHeight;
            _titlebarHeight = titlebarHeight;
            _notAdjustSizeOnLoaded = notAdjustSizeOnLoaded;

            this.Loaded += FullScreenBaseWindow_Loaded;
        }
        
        #endregion

        #region -- Public Method --

        public void ChangeWindowState(WindowState newState)
        {
            if (newState == WindowState.Maximized)
            {
                _maximized = true;
            }
            else if (newState == WindowState.Normal)
            {
                _maximized = false;
            }
            
            WindowState = newState;
        }

        public Rect GetWindowRect()
        {
            double top = WindowExtensions.ActualTop(this, Handle);
            double left = WindowExtensions.ActualLeft(this, Handle);
            double width = this.Width;
            double height = this.Height;
            if (WindowState.Maximized == this.WindowState)
            {
                left = left / (this.DpiX / 96d);
                top = top / (this.DpiY / 96d);

                if (this.ActualWidth > this.Width)
                {
                    width = this.ActualWidth;
                    height = this.ActualHeight;
                }
            }
            
            return new Rect(left, top, width, height);
        }

        public void MinimizeWindow()
        {
            if (!FullScreenStatus && WindowState.Minimized == this.WindowState)
            {
                log.InfoFormat("Window has been Minimized, hash: {0}", this.GetHashCode());
                return;
            }
            FullScreenStatus = false;
            ChangeWindowState(WindowState.Minimized);
        }

        public void MaximizeWindow()
        {
            if (!FullScreenStatus && WindowState.Maximized == this.WindowState)
            {
                log.InfoFormat("Window has been Maximized, hash: {0}", this.GetHashCode());
                return;
            }
            FullScreenStatus = false;
            ChangeWindowState(WindowState.Maximized);
        }

        public void RestoreWindow()
        {
            if (!FullScreenStatus && WindowState.Normal == this.WindowState)
            {
                log.InfoFormat("Window has been Normal, hash: {0}", this.GetHashCode());
                return;
            }
            FullScreenStatus = false;
            ChangeWindowState(WindowState.Normal);
        }

        public void SetFullScreen()
        {
            if (FullScreenStatus && WindowState.Maximized == this.WindowState)
            {
                log.InfoFormat("Window has been Full Screen, hash: {0}", this.GetHashCode());
                return;
            }
            //reset normal when window is max; and set full screen(window is max too) 
            if (this.WindowState == WindowState.Maximized && !FullScreenStatus)
            {
                log.Info("first reset window to normal, then set window to max in full screen");
                FullScreenStatus = false;
                ChangeWindowState(WindowState.Normal);
            }

            FullScreenStatus = true;
            ChangeWindowState(WindowState.Maximized);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion

        #region -- Protected Method --

        protected override void OnLocationChanged(EventArgs e)
        {
            base.OnLocationChanged(e);

            if (!double.IsNaN(_position.X) && !double.IsNaN(_position.Y))
            {
                double x = this.Left - _position.X;
                double y = this.Top - _position.Y;

                TitleBarWindowMove?.Invoke(x, y);
            }
            _position.X = this.Left;
            _position.Y = this.Top;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!IsDisposed)
            {
                if (disposing)
                {
                    Microsoft.Win32.SystemEvents.DisplaySettingsChanged -= new System.EventHandler(DisplaySettingsChanged);
                }
                //Clean up unmanaged resources
            }
            IsDisposed = true;
        }

        #endregion

        #region -- Private Method --

        private void FullScreenBaseWindow_Loaded(object sender, RoutedEventArgs e)
        {
            HwndSource hwndSource = (HwndSource)HwndSource.FromVisual((Window)sender);
            hwndSource.AddHook(DragHook);
            this.StateChanged += FullScreenBaseWindow_StateChanged;

            _aspectRatio = _initialWidth / (_initialHeight - _titlebarHeight);

            Handle = new WindowInteropHelper(this).Handle;

            DisplayUtil.CheckScreenOrientation(Handle);
            Microsoft.Win32.SystemEvents.DisplaySettingsChanged += new System.EventHandler(DisplaySettingsChanged);

            DpiX = 96d;
            DpiY = 96d;
            UpdateDpiValue();
            if (!_notAdjustSizeOnLoaded)
            {
                AdjustWindowSize();
            }
        }

        private void FullScreenBaseWindow_StateChanged(object sender, EventArgs e)
        {
            log.InfoFormat("Window state changed, state: {0}, _maximized: {1}, hash code: {2}, name: {3}", this.WindowState, _maximized, this.GetHashCode(), this.Name);
            if (WindowState.Maximized == this.WindowState && !_maximized)
            {
                _maximized = true;
            }
        }

        private IntPtr DragHook(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            //if(this.WindowState == WindowState.Maximized)
            //{
            //    return IntPtr.Zero;
            //}

            //if (0x0200 == msg || 0x0084 == msg || 0x0020 == msg || 0x00A0 == msg)
            //{
            //    return IntPtr.Zero;
            //}

            //log.DebugFormat("Main Window DragHook, msg={0:X000}, hwnd={1:X000}", msg, hwnd);

            //if (0x0005 == msg)
            //{
            //    int width = LOWORD((uint)lParam);
            //    int height = HIWORD((uint)lParam);
            //    log.DebugFormat("Main window receive msg WM_SIZE, width={0}, height={1}", width, height);
            //}

            switch ((Utils.WM)msg)
            {
                case Utils.WM.WM_GETMINMAXINFO:
                    {
                        //    log.DebugFormat("WM.WM_GETMINMAXINFO, window state:{0}", this.WindowState);
                        WmGetMinMaxInfo(hwnd, lParam);
                        handled = true;
                    }
                    break;
                case Utils.WM.WM_SYSCOMMAND:
                    {
                        //    log.DebugFormat("receive syscommand: {0:X000}, window state:{1}", (int)wParam, this.WindowState);
                        if (wParam == new IntPtr(0xF030)) // Maximize event - SC_MAXIMIZE from Winuser.h
                        {
                            //    log.Debug("WM.WM_SYSCOMMAND -- wParam == new IntPtr(0xF030)");
                            _maximized = true;
                        }
                        else if (wParam == new IntPtr(0xF120) || wParam == new IntPtr(0xF020)) // SC_RESTORE or SC_MINIMIZE
                        {
                            //    log.Debug("WM.WM_SYSCOMMAND -- wParam == new IntPtr(0xF120) || wParam == new IntPtr(0xF020)");
                            // do nothing
                        }
                        else
                        {
                            //    log.DebugFormat("WM.WM_SYSCOMMAND -- else, wParam={0:X000}", wParam);
                            _maximized = false;
                        }
                    }
                    break;
                case Utils.WM.WINDOWPOSCHANGING:
                    {
                        if (FullScreenStatus)
                        {
                            return IntPtr.Zero;
                        }
                        Utils.WINDOWPOS pos = (Utils.WINDOWPOS)Marshal.PtrToStructure(lParam, typeof(Utils.WINDOWPOS));
                        //log.DebugFormat("WM.WINDOWPOSCHANGING, window state:{0}", this.WindowState);
                        if (_maximized)
                        {
                            //log.DebugFormat("WM.WINDOWPOSCHANGING -- _maximized, position:x={0}, y={1}, cx={2}, cy={3}, width={4}, height={5}", pos.x, pos.y, pos.cx, pos.cy, this.Width, this.Height);
                            if (pos.x > -32000 && pos.y > -32000 && 0 != pos.cx && 0 != pos.cy)
                            {
                                if ((pos.cx != _maxWindowWidth || pos.cy != _maxWindowHeight) && (_maxWindowWidth > 0 && _maxWindowHeight > 0))
                                {
                                    //The size of window is not equal to the maximize and set to the maximize.
                                    log.DebugFormat("The size of window is not equal to the maximize and set to the maximize. pos.cx={0}, pos.cy={1}, maxCx={2}, maxCy={3}", pos.cx, pos.cy, _maxWindowWidth, _maxWindowHeight);
                                    pos.cx = _maxWindowWidth;
                                    pos.cy = _maxWindowHeight;
                                    Marshal.StructureToPtr(pos, lParam, true);
                                    handled = true;
                                }
                            }

                            return IntPtr.Zero;
                        }

                        if (_maxWinWhenLoaded)
                        {
                            _maxWinWhenLoaded = false;
                            FullScreenStatus = false;
                            ChangeWindowState(WindowState.Maximized);
                            return IntPtr.Zero;
                        }

                        if ((pos.flags & (int)Utils.SWP_NOMOVE) != 0)
                        {
                            //log.Debug("WM.WINDOWPOSCHANGING -- (pos.flags & (int)SWP.NOMOVE) != 0");
                            return IntPtr.Zero;
                        }

                        Window wnd = (Window)HwndSource.FromHwnd(hwnd).RootVisual;
                        if (wnd == null)
                        {
                            //log.Debug("WM.WINDOWPOSCHANGING -- wnd == null");
                            return IntPtr.Zero;
                        }

                        if ((pos.cx == _maxWindowWidth && pos.cy == _maxWindowHeight) && (_maxWindowWidth > 0 && _maxWindowHeight > 0))
                        {
                            log.DebugFormat("The window maybe is maximized.");
                            return IntPtr.Zero;
                        }

                        if ((pos.cx > _maxWindowWidth || pos.cy > _maxWindowHeight) && (_maxWindowWidth > 0 && _maxWindowHeight > 0))
                        {
                            // The window size exceed the max size. Maybe the window is maximized by dragging the window to the top
                            log.Debug("The window size exceed the max size. Maybe the window is maximized by dragging the window to the top");
                            pos.cx = _maxWindowWidth;
                            pos.cy = _maxWindowHeight;
                            Marshal.StructureToPtr(pos, lParam, true);
                            handled = true;
                            return IntPtr.Zero;
                        }

                        if ((pos.cx == _maxWindowWidth || pos.cy == _maxWindowHeight) && (_maxWindowWidth > 0 && _maxWindowHeight > 0))
                        {
                            log.Debug("The window maybe is dragged to the right of left of screen to half max.");
                            handled = true;
                            return IntPtr.Zero;
                        }

                        //log.DebugFormat("Main window original position, x={0}, y={1}, cx={2}, cy={3}", pos.x, pos.y, pos.cx, pos.cy);


                        // determine what dimension is changed by detecting the mouse position relative to the 
                        // window bounds. if gripped in the corner, either will work.
                        if (!_adjustingHeight.HasValue)
                        {
                            Point p = Utils.GetMousePosition();

                            double diffWidth = Math.Min(Math.Abs(p.X - pos.x), Math.Abs(p.X - pos.x - pos.cx));
                            double diffHeight = Math.Min(Math.Abs(p.Y - pos.y), Math.Abs(p.Y - pos.y - pos.cy));

                            _adjustingHeight = diffHeight > diffWidth;
                        }

                        if (_adjustingHeight.Value)
                        {
                            //    log.DebugFormat("Adjust pos.cy, original pos.cy:{0}", pos.cy);
                            pos.cy = (int)((pos.cx / _aspectRatio) + _titlebarHeight * (DpiY / 96d)); // adjusting height to width change
                            //    log.DebugFormat("Changed pos.cy:{0}", pos.cy);
                        }
                        else
                        {
                            //    log.DebugFormat("Adjust pos.cx, original pos.cx:{0}", pos.cx);
                            pos.cx = (int)((pos.cy - _titlebarHeight * (DpiY / 96d)) * _aspectRatio); // adjusting width to heigth change
                            //    log.DebugFormat("Changed pos.cx:{0}", pos.cx);
                        }
                        //log.InfoFormat("Main window changed position, x={0}, y={1}, cx={2}, cy={3}", pos.x, pos.y, pos.cx, pos.cy);
                        Marshal.StructureToPtr(pos, lParam, true);
                        handled = true;
                    }
                    break;
                case Utils.WM.EXITSIZEMOVE:
                    //log.Debug("WM.EXITSIZEMOVE");
                    _adjustingHeight = null; // reset adjustment dimension and detect again next time window is resized
                    break;
                case Utils.WM.WM_DPICHANGED:
                    Utils.RECT rect = (Utils.RECT)Marshal.PtrToStructure(lParam, typeof(Utils.RECT));
                    if (rect.left != -32000 && rect.top != -32000)
                    {
                        //sync trigger system event(DisplaySettingChanged and DpiChanged)
                        //if first trigger DisplaySettingChanged, after trigger DpiChanged, skip DpiChanged event;because DpiChanged position is wrong; 
                        if ((DateTime.Now - _displaySettingChangedTime).TotalSeconds >= 2)
                        {
                            UpdateDpiValue();
                            Utils.MySetWindowPos(hwnd, new Rect(rect.left, rect.top, rect.right - rect.left, rect.bottom - rect.top));
                            DpiChanged?.Invoke();
                        }
                    }
                    else
                    {
                        log.DebugFormat("--------------neglect dpichanged, because rect.left={0}, rect.top={1}", rect.left, rect.top);
                    }
                    break;
            }

            return IntPtr.Zero;
        }

        private void UpdateDpiValue()
        {
            using (System.Drawing.Graphics g = System.Drawing.Graphics.FromHwnd(Handle))
            {
                DpiX = g.DpiX;
                DpiY = g.DpiY;
            }
            log.InfoFormat("Update main window dpi value, DpiX:{0}, DpiY:{1}", DpiX, DpiY);
        }

        public void AdjustWindowSize()
        {
            if (this._maximized)
            {
                return;
            }

            try
            {
                WinInfo winInfo = GetWindowInfo();
                uint dpiX = 0;
                uint dpiY = 0;
                Screen currentScreen = DpiUtil.GetScreenByHandle(Handle);
                DpiUtil.GetDpiByScreen(currentScreen, out dpiX, out dpiY);
                double ratioX = ((double)dpiX / 96d);
                double ratioY = ((double)dpiY / 96d);
                double maxCx = (double)winInfo.maxCx / ratioX;
                double maxCy = (double)winInfo.maxCy / ratioY;
                log.InfoFormat("Adjust window size, width={0}, height={1}, maxCx={2}, maxCy={3}, left={4}, top={5}, WindowState={6}, hash code={7}", this.Width, this.Height, maxCx, maxCy, this.Left, this.Top, this.WindowState, this.GetHashCode());
                // when the screen size is less than the specified size of main window, then set the size of main window
                // to a proper size.
                if (_initialWidth >= maxCx || _initialHeight >= maxCy)
                {
                    double ratio = _initialWidth / (_initialHeight - _titlebarHeight);
                    if ((maxCx / (maxCy - _titlebarHeight)) > ratio)
                    {
                        this.Height = maxCy;
                        this.Width = (maxCy - _titlebarHeight) * ratio;
                    }
                    else
                    {
                        this.Width = maxCx;
                        this.Height = maxCx / ratio + _titlebarHeight;
                    }

                    Utils.MySetWindowPos(Handle, new Rect(currentScreen.Bounds.Left + (currentScreen.Bounds.Width - this.Width * ratioX) / 2, currentScreen.Bounds.Top + (currentScreen.Bounds.Height - this.Height * ratioY) / 2, this.Width * ratioX, this.Height * ratioY));

                    log.InfoFormat("window size is adjusted, width={0}, height={1}, hash code={2}", this.Width, this.Height, this.GetHashCode());
                    //_maxWinWhenLoaded = true;
                    //FullScreenStatus = false;
                    //ChangeWindowState(WindowState.Maximized);
                }
            }
            catch (DllNotFoundException e)
            {
                log.Debug("Cannot adjust window size for dll not found.");
            }
        }

        private WinInfo GetWindowInfo()
        {
            Screen currentScreen = DpiUtil.GetScreenByHandle(Handle);
            System.Drawing.Rectangle workArea = currentScreen.WorkingArea;
            WinInfo winInfo = new WinInfo();
            winInfo.maxCx = Math.Abs(workArea.Right - workArea.Left);
            winInfo.maxCy = Math.Abs(workArea.Bottom - workArea.Top);

            return winInfo;
        }

        public struct WinInfo
        {
            public int maxCx;
            public int maxCy;
        }


        private void DisplaySettingsChanged(object sender, EventArgs e)
        {
            //resolve any screen orientation change
            DisplayUtil.CheckScreenOrientation(Handle);
            //end

            double oldLeft = this.Left;
            double oldTop = this.Top;

            //resolve this problem: if screen resolution change, video windows show improperly
            //this.ChangeWindowState(WindowState.Normal);//if window is minimized, codes below will not get right result
            this.RestoreWindow(); //if window is minimized, codes below will not get right result
            Screen screen = DpiUtil.GetScreenByHandle(Handle);

            double ratio = 1;
            try
            {
                uint dpiX = 0;
                uint dpiY = 0;
                DpiUtil.GetDpiByScreen(screen, out dpiX, out dpiY);
                ratio = dpiX / 96d;
                if (ratio <= 0)
                {
                    ratio = 1;
                }
            }
            catch (DllNotFoundException ex)
            {
                ratio = 1;
                log.Warn("Cannot adjust window size for : " + ex.Message);
            }

            if (this.Left >= screen.Bounds.Left / ratio && this.Left < ((screen.Bounds.Left + screen.Bounds.Width) / ratio))
            {
                //window is not cross screen, nothing todo
            }
            else
            {
                //reset default size when window cross screen
                double height = screen.Bounds.Height / 1.2d;
                double width = height * _initialWidth / _initialHeight;

                Utils.MySetWindowPos(Handle, new Rect(screen.Bounds.Left + (screen.Bounds.Width - width) / 2, screen.Bounds.Top + (screen.Bounds.Height - height) / 2, width, height));
                _displaySettingChangedTime = DateTime.Now;
                //do this trick to refresh layout of all the video windows
                this.MinimizeWindow();
                this.RestoreWindow();
                //end
            }
        }

        private void WmGetMinMaxInfo(IntPtr hwnd, IntPtr lParam)
        {
            if (this.WindowState == WindowState.Maximized && !FullScreenStatus)
            {
                Utils.MINMAXINFO mmi = (Utils.MINMAXINFO)Marshal.PtrToStructure(lParam, typeof(Utils.MINMAXINFO));

                // Adjust the maximized size and position to fit the work area of the correct monitor
                Screen currentScreen = System.Windows.Forms.Screen.FromHandle(hwnd);
                System.Drawing.Rectangle workArea = currentScreen.WorkingArea;
                System.Drawing.Rectangle monitorArea = currentScreen.Bounds;
                mmi.ptMaxPosition.x = Math.Abs(workArea.Left - monitorArea.Left);
                mmi.ptMaxPosition.y = Math.Abs(workArea.Top - monitorArea.Top);
                mmi.ptMaxSize.x = Math.Abs(workArea.Right - workArea.Left);
                mmi.ptMaxSize.y = Math.Abs(workArea.Bottom - workArea.Top);
                //log.DebugFormat("mmi.ptMaxPosition={0}:{1}, mmi.ptMaxSize: {2} x {3}", mmi.ptMaxPosition.x, mmi.ptMaxPosition.y, mmi.ptMaxSize.x, mmi.ptMaxSize.y);
                _maxWindowWidth = mmi.ptMaxSize.x;
                _maxWindowHeight = mmi.ptMaxSize.y;
                Marshal.StructureToPtr(mmi, lParam, true);
            }
        }

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion

        #region -- IMasterDisplayWindow --

        public Window GetWindow()
        {
            return this;
        }

        public IntPtr GetHandle()
        {
            return Handle;
        }

        public double GetInitialWidth()
        {
            return _initialWidth;
        }

        public double GetInitialHeight()
        {
            return _initialHeight;
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

        #endregion
    }
}
