using EasyVideoWin.Helpers;
using EasyVideoWin.Model;
using log4net;
using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace EasyVideoWin.View
{
    /// <summary>
    /// Interaction logic for ContentControlToolBarView.xaml
    /// </summary>
    public partial class ContentControlToolBarView : Window
    {
        #region -- Members --
        private readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        [DllImport("user32.dll")]
        private static extern int GetWindowLong(IntPtr hWnd, int nIndex);
        [DllImport("user32.dll")]
        private static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

        private const int GWL_STYLE = -16;
        private const int WS_MAXIMIZEBOX = 0x10000;

        public delegate void ListenerClickHandler(object sender, RoutedEventArgs e);
        public event ListenerClickHandler ListenerEnableShareSoundClick = null;
        public event ListenerClickHandler ListenerDisableShareSoundClick = null;
        public event ListenerClickHandler ListenerStartCursorClick = null;
        public event ListenerClickHandler ListenerStopCursorClick = null;
        public event ListenerClickHandler ListenerPenSettingClick = null;
        public event ListenerClickHandler ListenerHighlighterClick = null;
        public event ListenerClickHandler ListenerEraseClick = null;
        public event ListenerClickHandler ListenerRevokeClick = null;
        public event ListenerClickHandler ListenerWallpaperClick = null;
        public event ListenerClickHandler ListenerClearClick = null;
        public event ListenerClickHandler ListenerSnapClick = null;
        public event ListenerClickHandler ListenerExitClick = null;

        public delegate void ListenerMoveHnadler(object sender, MouseButtonEventArgs e);
        public event ListenerMoveHnadler ListenerToolBarMove = null;
        private System.Windows.Media.Imaging.BitmapImage _eraserBitmapImage             = null;
        private System.Windows.Media.Imaging.BitmapImage _eraserClickBitmapImage        = null;
        private System.Windows.Media.Imaging.BitmapImage _eraserLeftClickBitmapImage    = null;
        
        public ShareContentMode ToolbarInContentMode { set; get; }

        #endregion
        #region -- Constructors --
        public ContentControlToolBarView(bool showShareSoundBtn)
        {
            log.Debug("ContentControlToolBarView construction");
            InitializeComponent();

            this.SourceInitialized += Window_SourceInitialized;
            this.Loaded += ContentControlToolBarView_Loaded;

            if (showShareSoundBtn)
            {
                RefreshShareSoundBtnStatus();
            }
            else
            {
                this.EnableShareSound.Visibility = Visibility.Collapsed;
                this.DisableShareSound.Visibility = Visibility.Collapsed;
            }
        }


        #endregion

        #region -- Properties --

        #endregion

        #region -- Public Methods --

        public void RefreshShareSoundBtnStatus()
        {
            bool enabled = CallController.Instance.ContentAudioEnabled();
            this.EnableShareSound.Visibility = enabled ? Visibility.Collapsed : Visibility.Visible;
            this.DisableShareSound.Visibility = enabled ? Visibility.Visible : Visibility.Collapsed;
        }

        #endregion

        #region -- Private Methods --

        private void EnableShareSound_Click(object sender, RoutedEventArgs e)
        {
            log.Info("ContentControlToolBarView EnableShareSound_Click");
            ListenerEnableShareSoundClick?.Invoke(sender, e);
        }

        private void DisableShareSound_Click(object sender, RoutedEventArgs e)
        {
            log.Info("ContentControlToolBarView DisableShareSound_Click");
            ListenerDisableShareSoundClick?.Invoke(sender, e);
        }

        private void ContentControlToolBarView_Loaded(object sender, RoutedEventArgs e)
        {
            Utils.SetSoftwareRender(this);
        }

        private void StartCursor_Click(object sender, RoutedEventArgs e)
        {
            log.Debug("ContentControlToolBarView StartCursor_Click");
            ListenerStartCursorClick?.Invoke(sender, e);
        }

        private void StopCursor_Click(object sender, RoutedEventArgs e)
        {
            log.Debug("ContentControlToolBarView StopCursor_Click");
            ListenerStopCursorClick?.Invoke(sender, e);
        }

        private void PenSetting_Click(object sender, RoutedEventArgs e)
        {
            log.Debug("ContentControlToolBarView PenSetting_Click");
            ResetDefaultButtonImage();
            if (StopCursor.IsVisible)
            {
                ListenerPenSettingClick?.Invoke(sender, e);
            }
            else if(ToolbarInContentMode == ShareContentMode.Whiteboard)
            {
                ListenerPenSettingClick?.Invoke(sender, e);
            }
        }

        private void Highlighter_Click(object sender, RoutedEventArgs e)
        {
            log.Debug("ContentControlToolBarView Highlighter_Click");
            ResetDefaultButtonImage();
            if (StopCursor.IsVisible)
            {
                ListenerHighlighterClick?.Invoke(sender, e);
            }
            else if (ToolbarInContentMode == ShareContentMode.Whiteboard)
            {
                ListenerHighlighterClick?.Invoke(sender, e);
            }
        }

        private void Erase_Click(object sender, RoutedEventArgs e)
        {
            log.Debug("ContentControlToolBarView Erase_Click");
            ResetDefaultButtonImage();
            if (this.ToolbarPanel.Orientation == System.Windows.Controls.Orientation.Vertical)
            {
                if (null == _eraserLeftClickBitmapImage)
                {
                    _eraserLeftClickBitmapImage = new System.Windows.Media.Imaging.BitmapImage(new Uri("pack://application:,,,/Resources/Icons/Content/icon_eraser_left_click.png"));
                }
                Erase.NormalImage       = _eraserLeftClickBitmapImage;
                Erase.MouseOverImage    = _eraserLeftClickBitmapImage;
                Erase.TextForeground    = Utils.SelectedForeGround;
            }
            else
            {
                if (null == _eraserClickBitmapImage)
                {
                    _eraserClickBitmapImage = new System.Windows.Media.Imaging.BitmapImage(new Uri("pack://application:,,,/Resources/Icons/Content/icon_eraser_click.png"));
                }
                Erase.NormalImage       = _eraserClickBitmapImage;
                Erase.MouseOverImage    = _eraserClickBitmapImage;
                Erase.TextForeground    = Utils.SelectedForeGround;
            }
            if (StopCursor.IsVisible)
            {
                ListenerEraseClick?.Invoke(sender, e);
            }
            else if (ToolbarInContentMode == ShareContentMode.Whiteboard)
            {
                ListenerEraseClick?.Invoke(sender, e);
            }
        }

        private void Revoke_Click(object sender, RoutedEventArgs e)
        {
            log.Debug("ContentControlToolBarView Revoke_Click");
            if (StopCursor.IsVisible)
            {
                ListenerRevokeClick?.Invoke(sender, e);
            }
            else if (ToolbarInContentMode == ShareContentMode.Whiteboard)
            {
                ListenerRevokeClick?.Invoke(sender, e);
            }
        }

        private void Wallpaper_Click(object sender, RoutedEventArgs e)
        {
            log.Debug("ContentControlToolBarView Wallpaper_Click");
            if (StopCursor.IsVisible)
            {
                ListenerWallpaperClick?.Invoke(sender, e);
            }
            else if (ToolbarInContentMode == ShareContentMode.Whiteboard)
            {
                ListenerWallpaperClick?.Invoke(sender, e);
            }
        }

        private void Clear_Click(object sender, RoutedEventArgs e)
        {
            log.Debug("ContentControlToolBarView Clear_Click");
            if (StopCursor.IsVisible)
            {
                ListenerClearClick?.Invoke(sender, e);
            }
            else if (ToolbarInContentMode == ShareContentMode.Whiteboard)
            {
                ListenerClearClick?.Invoke(sender, e);
            }
        }

        private void Snap_Click(object sender, RoutedEventArgs e)
        {
            log.Debug("ContentControlToolBarView Snap_Click");
            ListenerSnapClick?.Invoke(sender, e);
            using (System.Media.SoundPlayer RingtonePlayer = new System.Media.SoundPlayer())
            {
                string path = AppDomain.CurrentDomain.BaseDirectory + "Resources\\sounds\\camera.wav";
                RingtonePlayer.SoundLocation = path;
                RingtonePlayer.Play();
            }
        }

        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            log.Debug("ContentControlToolBarView Exit_Click");
            ListenerExitClick?.Invoke(sender, e);
        }

        private void ResetDefaultButtonImage()
        {
            if (null == _eraserBitmapImage)
            {
                _eraserBitmapImage = new System.Windows.Media.Imaging.BitmapImage(new Uri("pack://application:,,,/Resources/Icons/Content/icon_eraser.png"));
            }
            Erase.NormalImage       = _eraserBitmapImage;
            Erase.MouseOverImage    = _eraserBitmapImage;
            Erase.TextForeground    = Utils.DefaultForeGround;
        }

        private void Window_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            log.Debug("ContentControlToolBarView Window_MouseLeftButtonDown");
            Console.WriteLine("Window_MouseLeftButtonDown");
            base.OnMouseLeftButtonDown(e);
            this.DragMove();

            ListenerToolBarMove?.Invoke(sender, e);
        }

        private void Window_SourceInitialized(object sender, EventArgs e)
        {
            log.DebugFormat("ContentControlToolBarView Window_SourceInitialized {0}", this.GetHashCode());
            IntPtr hwnd = new System.Windows.Interop.WindowInteropHelper((Window)sender).Handle;
            int value = GetWindowLong(hwnd, GWL_STYLE);
            SetWindowLong(hwnd, GWL_STYLE, (int)(value & ~WS_MAXIMIZEBOX));

            System.Windows.Interop.HwndSource hwndSource = (System.Windows.Interop.HwndSource)System.Windows.Interop.HwndSource.FromVisual((Window)sender);
            hwndSource.AddHook(DragHook);
        }

        private IntPtr DragHook(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            switch ((Utils.WM)msg)
            {
                case Utils.WM.WM_DPICHANGED:
                    Utils.RECT rect = (Utils.RECT)Marshal.PtrToStructure(lParam, typeof(Utils.RECT));
                    if (CallController.Instance.CurrentCallStatus != CallStatus.Connected)
                    {
                        Utils.MySetWindowPos(hwnd, new System.Windows.Rect(rect.left, rect.top, rect.right - rect.left, rect.bottom - rect.top));
                    }
                    break;
            }
            return IntPtr.Zero;
        }

        #endregion
    }

    public enum ButtonStatus
    {
        None = 0,
        Selected = 1,
        Show = 2,
        Hide = 3,
    }
}
