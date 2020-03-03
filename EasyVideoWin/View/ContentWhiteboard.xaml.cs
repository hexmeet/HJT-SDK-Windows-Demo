using CefSharp;
using CefSharp.WinForms;
using EasyVideoWin.CustomControls;
using EasyVideoWin.Helpers;
using EasyVideoWin.Model;
using EasyVideoWin.Model.CloudModel;
using EasyVideoWin.WinForms;
using log4net;
using Newtonsoft.Json;
using System;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Resources;
using static EasyVideoWin.View.BackScreenColorSelectWindow;
using static EasyVideoWin.View.BrushHighlighterWindow;
using static EasyVideoWin.View.BrushSelectWindow;

namespace EasyVideoWin.View
{
    /// <summary>
    /// Interaction logic for ContentWhiteboard.xaml
    /// </summary>
    public partial class ContentWhiteboard : FullScreenBaseWindow, INotifyPropertyChanged
    {
        #region -- Members --

        private readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private const double INITIAL_WIDTH = 1280d;
        private const double INITIAL_HEIGHT = 760d;
        private const double TITLEBAR_HEIGHT = 40d;

        private int _selectedColor = (Int16)PenColor.Black;
        private int _penType = 1;
        private ButtonStatus _selectedPenAsTarget = ButtonStatus.None;
        private int _selectedHighligherColor = (Int16)PenColor.Yellow;
        private int _selectedHighlighterSize = 3;
        private ButtonStatus _selectedHighlighterAsTarget = ButtonStatus.None;

        private InkCanvasCursorEnum _inkCanvasCursorStatus;
        private ContentControlToolBarView _controlToolBar = null;
        private BrushSelectWindow _brushWin = null;
        private BackScreenColorSelectWindow _backScreenWin = null;
        private ContentWhiteboardPopup _whiteboardPopup = null;
        private BrushHighlighterWindow _highlighterWin = null;

        public delegate void ListenerWhiteBoardExitHandler(string str);
        public event ListenerWhiteBoardExitHandler ListeneWhiteBoardExit = null;

        private const string acsUploadUrl = "{0}/acs/upload";
        //private const string acsStartUrl = "{0}/acs/api/started/{1}";

        #endregion
        
        #region -- Properties --

        public string JwtParam { set; get; }
        public bool IsHighDpiDevice { set; get; } = false;
        public bool IsConnectedWBServer { get; set; } = false;

        public ChromiumWebBrowser Browser { set; get; }

        public string WhiteBoardServerAddr { get; set; }
        
        public ContentControlToolBarView ControlToolBar
        {
            get
            {
                return _controlToolBar;
            }
        }

        public bool IsWindowActive
        {
            get
            {
                return IsActive || _controlToolBar.IsActive || _brushWin.IsActive || _backScreenWin.IsActive || _highlighterWin.IsActive;
            }
        }
        
        #endregion

        #region -- Constructors --
        public ContentWhiteboard() : base(INITIAL_WIDTH, INITIAL_HEIGHT, TITLEBAR_HEIGHT)
        {
            InitializeComponent();

            log.InfoFormat("ContentWhiteboard hash code:{0}", this.GetHashCode());

            StateChanged += WindowState_Changed;
            this.IsVisibleChanged += ContentWhiteBoard_IsVisibleChanged;

            this.WindowState = WindowState.Normal;

            //this.browser = (this.wfh.Child as WhiteBoardForm).browser;
            this.Browser = (this.wfh.Child as WhiteBoardForm).browser;
            this.Browser.MenuHandler = new CustomMenuHandler();
            //this.browser.DownloadHandler = new DownloadHandler();
            CefRequestHandler requestHdlr = new CefRequestHandler();
            requestHdlr._board = this;
            this.Browser.RequestHandler = requestHdlr;


            InkCanvasCursorStatus = InkCanvasCursorEnum.PEN;
            _brushWin = new BrushSelectWindow();
            _backScreenWin = new BackScreenColorSelectWindow();
            _controlToolBar = new ContentControlToolBarView(false);
            _highlighterWin = new BrushHighlighterWindow();
            _whiteboardPopup = new ContentWhiteboardPopup();
            _whiteboardPopup.Background = new SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#01FFFFFF"));
            _whiteboardPopup.Visibility = Visibility.Collapsed;

            InitToolbar();

            TitleConfNumberLabel.Text = CallController.Instance.IsP2pCall ? "" : CallController.Instance.ConferenceNumber;

            CallController.Instance.PropertyChanged += OnWhiteBoardPresenterChanged;

            _brushWin.ListenerBrush += new ListenerBrushHandler(SetPenAndColor);
            _highlighterWin.ListenerHighlighter += new ListenerHighlighterHandler(SetHighlighterPenAndColor);
            _backScreenWin.ListenerBackGroundSelected += new ListenerBackGroundSelectedHandler(SetBackGroundByType);
            _backScreenWin.ListenerUploadBackGround += new ListenerUploadBackGroundHandler(ChangeBackgroundImage);

            _controlToolBar.ListenerPenSettingClick += new ContentControlToolBarView.ListenerClickHandler(PenSetting_Click);
            _controlToolBar.ListenerHighlighterClick += new ContentControlToolBarView.ListenerClickHandler(Highlighter_Click);
            _controlToolBar.ListenerEraseClick += new ContentControlToolBarView.ListenerClickHandler(Erase_Click);
            _controlToolBar.ListenerRevokeClick += new ContentControlToolBarView.ListenerClickHandler(Revoke_Click);
            _controlToolBar.ListenerWallpaperClick += new ContentControlToolBarView.ListenerClickHandler(Wallpaper_Click);
            _controlToolBar.ListenerClearClick += new ContentControlToolBarView.ListenerClickHandler(Clear_Click);
            _controlToolBar.ListenerSnapClick += new ContentControlToolBarView.ListenerClickHandler(Snap_Click);
            _controlToolBar.ListenerExitClick += new ContentControlToolBarView.ListenerClickHandler(Exit_Click);
            _controlToolBar.ListenerToolBarMove += new ContentControlToolBarView.ListenerMoveHnadler(ToolBarMove_Click);
            _whiteboardPopup.WhiteboardPopupMouseLeftButtonDown += new ContentWhiteboardPopup.ListenerWhiteboardPopupMouseLeftButtonDownHandler(WhiteboardPopup_MouseLeftButtonDown);
            this.Browser.ConsoleMessage += OnBrowserConsoleMessage;
        }

        //protected override void OnClosing(CancelEventArgs e)
        //{
        //    log.Info("OnClosing");
        //    this.Hide();
        //    HidePopupWindow(true, true, true);
        //    _controlToolBar.Hide();
        //    //e.Cancel = true;
        //}

        private void OnWhiteBoardPresenterChanged(object sender, PropertyChangedEventArgs e)
        {
            if ("WhiteBoardPresenter" == e.PropertyName)
            {
                if (CallController.Instance.IsP2pCall)
                {
                    return;
                }
                Application.Current.Dispatcher.Invoke(() =>
                {
                    TitleConfNumberLabel.Text = string.Format(LanguageUtil.Instance.GetValueByKey("S_WHITEBOARD"), CallController.Instance.WhiteBoardPresenter);
                });
            }
        }
        #endregion

        #region -- Public --
        private bool _initiator = false;
        public bool Initiator
        {
            get
            {
                return _initiator;
            }
            set
            {
                if (_initiator != value)
                {
                    _initiator = value;
                    OnPropertyChanged("InitiatorButtonVisibility");
                }
            }
        }
        public InkCanvasCursorEnum InkCanvasCursorStatus
        {
            get
            {
                return _inkCanvasCursorStatus;
            }
            set
            {
                _inkCanvasCursorStatus = value;
                log.Debug("Whiteboard InkCanvasCursorStatus : " + _inkCanvasCursorStatus);

                OnPropertyChanged("InitiatorButtonVisibility");
            }
        }
        
        public Visibility InitiatorButtonVisibility
        {
            get
            {
                //Only Presenter can quit board etc.
                return Initiator ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        public void ShowOrHideToolbar(bool isShow, System.Windows.Forms.Screen currentScreen)
        {
            log.DebugFormat("show or hide toolbar: {0}", isShow);
            if (isShow)
            {
                _controlToolBar.Show();
                SetToolBarPositionLogical(currentScreen);

                //CallController.Instance.DialingAddr;
                _whiteboardPopup.Show();
                //DisplayUtil.AdjustWindowPosition(this);
                //ToolBar_ChangePosition();


                SelectedPenAndColor();
                WhiteboardPopup_ChangePosition();
            }
            else
            {
                _controlToolBar.Hide();
                _whiteboardPopup.Hide();
                HidePopupWindow(true, true, true);
            }

        }
        
        protected override void Dispose(bool disposing)
        {
            if (!IsDisposed)
            {
                if (disposing)
                {
                    base.Dispose(disposing);
                    //Clean Up managed resources
                    CallController.Instance.PropertyChanged -= OnWhiteBoardPresenterChanged;
                    Application.Current.Dispatcher.Invoke(() => {
                        this.Browser.Dispose();
                    });
                }
                //Clean up unmanaged resources  
            }
            IsDisposed = true;
        }

        #endregion

        #region -- Private --
        
        private void InitToolbar()
        {
            log.Debug("Whiteboard InitToolbar");
            _controlToolBar.StartCursor.Visibility  = Visibility.Collapsed;
            _controlToolBar.StopCursor.Visibility   = Visibility.Collapsed;
            _controlToolBar.PenSetting.Visibility   = Visibility.Visible;
            _controlToolBar.Highlighter.Visibility  = Visibility.Visible;
            _controlToolBar.Erase.Visibility        = Visibility.Visible;
            _controlToolBar.Revoke.Visibility       = Visibility.Visible;
            _controlToolBar.Wallpaper.SetBinding(Button.VisibilityProperty, new Binding("InitiatorButtonVisibility") { Source = this });
            _controlToolBar.Clear.SetBinding(Button.VisibilityProperty, new Binding("InitiatorButtonVisibility") { Source = this });
            _controlToolBar.Exit.SetBinding(Button.VisibilityProperty, new Binding("InitiatorButtonVisibility") { Source = this });
        }

        private void SetToolBarPositionLogical(System.Windows.Forms.Screen currentScreen)
        {
            log.Debug("Whiteboard SetToolBarPositionLogical");
            //this.Topmost = true;
            _whiteboardPopup.Owner = this;
            if (currentScreen == null)
            {
                currentScreen = System.Windows.Forms.Screen.PrimaryScreen;
            }

            System.Windows.Forms.Screen mainWindowScreen = DpiUtil.GetScreenByHandle(VideoPeopleWindow.Instance.Handle);
            double primaryRatio = 1;
            uint currentScreenDpiX = 96;
            uint currentScreenDpiY = 96;
            uint primaryDpiX = 96;
            uint primaryDpiY = 96;
            try
            {
                DpiUtil.GetDpiByScreen(currentScreen, out currentScreenDpiX, out currentScreenDpiY);

                DpiUtil.GetDpiByScreen(System.Windows.Forms.Screen.PrimaryScreen, out primaryDpiX, out primaryDpiY);
                primaryRatio = (double)primaryDpiX / 96;
            }
            catch (DllNotFoundException ex)
            {
                log.ErrorFormat("Can not load windows dll: {0}", ex);
            }

            double toolBarWidth = _controlToolBar.Width;
            if (!currentScreen.DeviceName.Equals(System.Windows.Forms.Screen.PrimaryScreen.DeviceName))
            {
                _controlToolBar.Left = currentScreen.WorkingArea.Left / primaryRatio + (currentScreen.WorkingArea.Width / primaryRatio - toolBarWidth) / 2;
                _controlToolBar.Top = currentScreen.WorkingArea.Top / primaryRatio + 20;
            }
            else
            {
                _controlToolBar.Left = currentScreen.WorkingArea.Left / ((double)currentScreenDpiX / 96) + (currentScreen.WorkingArea.Width / ((double)currentScreenDpiX / 96) - toolBarWidth) / 2;
                _controlToolBar.Top = currentScreen.WorkingArea.Top / ((double)currentScreenDpiX / 96) + 20;
            }

            _controlToolBar.Owner = this;
            //_controlToolBar.Topmost = true;
            //_controlToolBar.Show();
            _controlToolBar.ToolbarInContentMode = ShareContentMode.Whiteboard;

            if (!currentScreen.DeviceName.Equals(mainWindowScreen.DeviceName) && !currentScreen.DeviceName.Equals(System.Windows.Forms.Screen.PrimaryScreen.DeviceName))
            {
                log.DebugFormat("ChangePopupWindowSizeByRatio _controlToolBar before: {0}, {1}, {2}, {3}", _controlToolBar.Left, _controlToolBar.Top, Math.Round(_controlToolBar.Width, 0), Math.Round(_controlToolBar.Height, 0));

                if (primaryDpiX > currentScreenDpiX)
                {
                    _controlToolBar.Width = 520 / primaryRatio;
                    _controlToolBar.Height = 70 / primaryRatio;
                }
                else if (primaryDpiX < currentScreenDpiX)
                {
                    _controlToolBar.Width = 520 * ((double)currentScreenDpiX / primaryDpiX);
                    _controlToolBar.Height = 70 * ((double)currentScreenDpiY / primaryDpiY);
                }

                //Utils.MySetWindowPos(new WindowInteropHelper(this._controlToolBar).Handle, new System.Windows.Rect(Math.Round(_controlToolBar.Left, 0), Math.Round(_controlToolBar.Top, 0), Math.Round(_controlToolBar.Width, 0), Math.Round(_controlToolBar.Height, 0)));
                log.DebugFormat("ChangePopupWindowSizeByRatio _controlToolBar after : {0}, {1}, {2}, {3}", _controlToolBar.Left, _controlToolBar.Top, _controlToolBar.Width, _controlToolBar.Height);
            }

        }

        private void OnBrowserConsoleMessage(object sender, ConsoleMessageEventArgs args)
        {
            log.InfoFormat("ACS browser console log: {0}", args.Message);
        }

        #region -- pen --
        private void PenSetting_Click(object sender, RoutedEventArgs e)
        {
            log.DebugFormat("Show pen window in content Whiteboard view: {0}", this.GetHashCode());
            HidePopupWindow(false, true, true);
            //set to default highlighter
            _selectedHighlighterAsTarget = ButtonStatus.None;
            ClickButtonChangeBackImage(0, 2);

            InkCanvasCursorStatus = InkCanvasCursorEnum.PEN;
            switch (_selectedPenAsTarget)
            {
                case ButtonStatus.None:
                    SelectedPenAndColor();
                    break;
                case ButtonStatus.Selected:
                    _selectedPenAsTarget = ButtonStatus.Show;
                    ClickButtonChangeBackImage(1, 1);
                    ShowPenWindow();
                    break;
                case ButtonStatus.Show:
                    _selectedPenAsTarget = ButtonStatus.Hide;
                    ClickButtonChangeBackImage(1, 1);
                    if (_brushWin.IsVisible)
                    {
                        _brushWin.Hide();
                    }
                    break;
                case ButtonStatus.Hide:
                    _selectedPenAsTarget = ButtonStatus.Show;
                    ClickButtonChangeBackImage(1, 1);
                    ShowPenWindow();
                    break;
            }
        }

        private void ShowPenWindow()
        {
            WhiteboardPopup_ChangeBackgroundTransparent(false);
            //init selected color
            _brushWin.initColor(_selectedColor);
            _brushWin.initSize(_penType);

            _brushWin.Owner = _controlToolBar;
            //_brushWin.Topmost = true;
            try
            {
                double screenLogicHeight = this.Height;
                double screenLogicWidth = this.Width;
                double screenLogicX = this.Left;
                double screenLogicY = this.Top;

                double toolbarLogicY = _controlToolBar.Top;
                double toolbarLogicX = _controlToolBar.Left;
                double toolbarLogicHeight = _controlToolBar.Height;
                double toolbarLogicWidth = _controlToolBar.Width;

                System.Windows.Forms.Screen currentScreen = DpiUtil.GetScreenByHandle(Handle);
                System.Windows.Forms.Screen mainWindowScreen = DpiUtil.GetScreenByHandle(VideoPeopleWindow.Instance.Handle);
                double primaryRatio = 1;
                uint currentScreenDpiX = 96;
                uint currentScreenDpiY = 96;
                uint primaryDpiX = 96;
                uint primaryDpiY = 96;
                try
                {
                    DpiUtil.GetDpiByScreen(currentScreen, out currentScreenDpiX, out currentScreenDpiY);

                    DpiUtil.GetDpiByScreen(System.Windows.Forms.Screen.PrimaryScreen, out primaryDpiX, out primaryDpiY);
                    primaryRatio = (double)primaryDpiX / 96;
                }
                catch (DllNotFoundException e)
                {
                    log.ErrorFormat("Can not load windows dll: {0}", e);
                }

                if (!currentScreen.DeviceName.Equals(System.Windows.Forms.Screen.PrimaryScreen.DeviceName))
                {
                    if (primaryDpiX > currentScreenDpiX)
                    {
                        _brushWin.Width = 200 / primaryRatio;
                        _brushWin.Height = 200 / primaryRatio;
                    }
                    else if (primaryDpiX < currentScreenDpiX)
                    {
                        _brushWin.Width = 200 * ((double)currentScreenDpiX / primaryDpiX);
                        _brushWin.Height = 200 * ((double)currentScreenDpiY / primaryDpiY);
                    }
                }
                else
                {
                    _brushWin.Width = 200;
                    _brushWin.Height = 200;
                }

                System.Windows.Point point = _controlToolBar.PenSetting.PointToScreen(new System.Windows.Point(0, 0));
                if (currentScreen.DeviceName.Equals(mainWindowScreen.DeviceName))
                {
                    point.X = point.X / ((double)currentScreenDpiX / 96);
                    point.Y = point.Y / ((double)currentScreenDpiY / 96);
                }
                else
                {
                    if (primaryRatio >= 1)
                    {
                        point.X = point.X / primaryRatio;
                        point.Y = point.Y / primaryRatio;
                    }
                    else
                    {
                        point.X = point.X * primaryRatio;
                        point.Y = point.Y * primaryRatio;
                    }
                }

                if (_controlToolBar.ToolbarPanel.Orientation == Orientation.Horizontal)
                {
                    if (toolbarLogicY > (screenLogicY + screenLogicHeight * 0.6))
                    {
                        _brushWin.Left = point.X - _brushWin.Width / 2;
                        _brushWin.Top = toolbarLogicY - _brushWin.Height;
                    }
                    else
                    {
                        _brushWin.Top = toolbarLogicY + toolbarLogicHeight;
                        _brushWin.Left = point.X - _brushWin.Width / 2;
                    }
                }

                _brushWin.Show();
            }
            catch (Exception ex)
            {
                log.DebugFormat("Exception for show pen selection window in Whiteboard, message:", ex.Message);
            }
        }

        private void SelectedPenAndColor()
        {
            _selectedPenAsTarget = ButtonStatus.Selected;
            SetPenAndColor(_penType.ToString(), _selectedColor);
            ClickButtonChangeBackImage(1, 1);
            //deselected highlighter
            _selectedHighlighterAsTarget = ButtonStatus.None;
            ClickButtonChangeBackImage(0, 2);
            //reset eraser selected status
            _controlToolBar.Erase.NormalImage = new System.Windows.Media.Imaging.BitmapImage(new Uri("pack://application:,,,/Resources/Icons/Content/icon_eraser.png"));
            _controlToolBar.Erase.MouseOverImage = new System.Windows.Media.Imaging.BitmapImage(new Uri("pack://application:,,,/Resources/Icons/Content/icon_eraser.png"));
            _controlToolBar.Erase.TextForeground = Utils.DefaultForeGround;
        }
        #endregion
        #region -- highlighter --
        private void Highlighter_Click(object sender, RoutedEventArgs e)
        {
            log.DebugFormat("Show highlighter window in content view: {0}", this.GetHashCode());
            HidePopupWindow(true, true, false);
            //set to default pen
            _selectedPenAsTarget = ButtonStatus.None;
            ClickButtonChangeBackImage(0, 1);

            switch (_selectedHighlighterAsTarget)
            {
                case ButtonStatus.None:
                    SelectedHighlightrAndColor();
                    break;
                case ButtonStatus.Selected:
                    _selectedHighlighterAsTarget = ButtonStatus.Show;
                    ShowHighlighterWindow();
                    ClickButtonChangeBackImage(1, 2);
                    break;
                case ButtonStatus.Show:
                    _selectedHighlighterAsTarget = ButtonStatus.Hide;
                    ClickButtonChangeBackImage(1, 2);
                    if (_highlighterWin.IsVisible)
                    {
                        _highlighterWin.Hide();
                    }
                    break;
                case ButtonStatus.Hide:
                    _selectedHighlighterAsTarget = ButtonStatus.Show;
                    ClickButtonChangeBackImage(1, 2);
                    ShowHighlighterWindow();
                    break;
            }
        }

        private void ShowHighlighterWindow()
        {
            WhiteboardPopup_ChangeBackgroundTransparent(false);
            //init selected color
            _highlighterWin.initColor(_selectedHighligherColor);
            _highlighterWin.initSize(_selectedHighlighterSize);

            _highlighterWin.Owner = _controlToolBar;
            //_highlighterWin.Topmost = true;
            try
            {
                double screenLogicHeight = this.Height;
                double screenLogicWidth = this.Width;
                double screenLogicX = this.Left;
                double screenLogicY = this.Top;

                double toolbarLogicY = _controlToolBar.Top;
                double toolbarLogicX = _controlToolBar.Left;
                double toolbarLogicHeight = _controlToolBar.Height;
                double toolbarLogicWidth = _controlToolBar.Width;

                System.Windows.Forms.Screen currentScreen = DpiUtil.GetScreenByHandle(Handle);
                System.Windows.Forms.Screen mainWindowScreen = DpiUtil.GetScreenByHandle(VideoPeopleWindow.Instance.Handle);
                double primaryRatio = 1;
                uint currentScreenDpiX = 96;
                uint currentScreenDpiY = 96;
                uint primaryDpiX = 96;
                uint primaryDpiY = 96;
                try
                {
                    DpiUtil.GetDpiByScreen(currentScreen, out currentScreenDpiX, out currentScreenDpiY);

                    DpiUtil.GetDpiByScreen(System.Windows.Forms.Screen.PrimaryScreen, out primaryDpiX, out primaryDpiY);
                    primaryRatio = (double)primaryDpiX / 96;
                }
                catch (DllNotFoundException e)
                {
                    log.ErrorFormat("Can not load windows dll: {0}", e);
                }

                if (!currentScreen.DeviceName.Equals(System.Windows.Forms.Screen.PrimaryScreen.DeviceName))
                {
                    if (primaryDpiX > currentScreenDpiX)
                    {
                        _highlighterWin.Width = 200 / primaryRatio;
                        _highlighterWin.Height = 200 / primaryRatio;
                    }
                    else if (primaryDpiX < currentScreenDpiX)
                    {
                        _highlighterWin.Width = 200 * ((double)currentScreenDpiX / primaryDpiX);
                        _highlighterWin.Height = 200 * ((double)currentScreenDpiY / primaryDpiY);
                    }
                }
                else
                {
                    _highlighterWin.Width = 200;
                    _highlighterWin.Height = 200;
                }

                System.Windows.Point point = _controlToolBar.Highlighter.PointToScreen(new System.Windows.Point(0, 0));
                if (currentScreen.DeviceName.Equals(mainWindowScreen.DeviceName))
                {
                    point.X = point.X / ((double)currentScreenDpiX / 96);
                    point.Y = point.Y / ((double)currentScreenDpiY / 96);
                }
                else
                {
                    if (primaryRatio >= 1)
                    {
                        point.X = point.X / primaryRatio;
                        point.Y = point.Y / primaryRatio;
                    }
                    else
                    {
                        point.X = point.X * primaryRatio;
                        point.Y = point.Y * primaryRatio;
                    }
                }

                if (_controlToolBar.ToolbarPanel.Orientation == Orientation.Horizontal)
                {
                    if (toolbarLogicY > (screenLogicY + screenLogicHeight * 0.6))
                    {
                        _highlighterWin.Left = point.X - _highlighterWin.Width / 2;
                        _highlighterWin.Top = toolbarLogicY - _highlighterWin.Height;
                    }
                    else
                    {
                        _highlighterWin.Left = point.X - _highlighterWin.Width / 2;
                        _highlighterWin.Top = toolbarLogicY + toolbarLogicHeight;
                    }
                }

                _highlighterWin.Show();
            }
            catch (Exception ex)
            {
                log.DebugFormat("Exception for show highlighter pen selection window, message:", ex.Message);
            }
        }

        private void SelectedHighlightrAndColor()
        {
            _selectedHighlighterAsTarget = ButtonStatus.Selected;
            SetHighlighterPenAndColor(_selectedHighlighterSize, _selectedHighligherColor);
            ClickButtonChangeBackImage(1, 2);
        }
        #endregion

        private void ChangeSelectedBrushColor(int selectedColor)
        {
            this._selectedColor = selectedColor;
        }

        private void SetPenAndColor(string penTypeSel, int colorTypeSel)
        {
            log.DebugFormat("Change pen style size: {0}, color: {1} in content Whiteboard view: {2}", penTypeSel, _selectedColor, this.GetHashCode());
            ChangeSelectedBrushColor(colorTypeSel);

            if (!string.IsNullOrEmpty(penTypeSel))
            {
                _penType = Convert.ToInt16(penTypeSel);
            }
            //pen size value : 1,2,3,4
            int tempPenType = (_penType == 2 ? 2 : _penType == 3 ? 3 : _penType == 4 ? 4 : 1);

            const string pencilCommand = "window.pencil('pencil')";
            this.Browser.GetBrowser().GetFrame(null).ExecuteJavaScriptAsync(pencilCommand);

            string linewightCommand = "window.lineWight(" + tempPenType + ")";
            this.Browser.GetBrowser().GetFrame(null).ExecuteJavaScriptAsync(linewightCommand);

            if (this.Browser.CanExecuteJavascriptInMainFrame)
            {
                int r, g, b;
                switch (_selectedColor)
                {
                    case (Int16)PenColor.Black:
                        {
                            r = 0; g = 0; b = 0;
                        }
                        break;
                    case (Int16)PenColor.White:
                        {
                            r = 255; g = 255; b = 255;
                        }
                        break;
                    case (Int16)PenColor.Blue:
                        {
                            r = 0; g = 0; b = 255;
                        }
                        break;
                    case (Int16)PenColor.Green:
                        {
                            r = 0; g = 249; b = 0;
                        }
                        break;
                    case (Int16)PenColor.Yellow:
                        {
                            r = 255; g = 252; b = 0;
                        }
                        break;
                    case (Int16)PenColor.Orange:
                        {
                            r = 255; g = 133; b = 23;
                        }
                        break;
                    case (Int16)PenColor.Red:
                        {
                            r = 255; g = 0; b = 0;
                        }
                        break;
                    case (Int16)PenColor.Purple:
                        {
                            r = 255; g = 64; b = 255;
                        }
                        break;
                    default:
                        {
                            r = 0; g = 0; b = 0;
                        }
                        break;
                }

                string changeColorCommand = "window.changeColor('{\"r\":" + r + ", \"g\":" + g + ", \"b\":" + b + ",\"a\":1}')";
                log.DebugFormat("color: {0}", changeColorCommand);
                this.Browser.GetBrowser().GetFrame(null).ExecuteJavaScriptAsync(changeColorCommand);
            }
            else
            {
                System.Windows.MessageBox.Show("can't execute Js now!");
            }
        }

        private void SetHighlighterPenAndColor(int penTypeSel, int colorTypeSel)
        {
            log.DebugFormat("Change highlighter pen style size: {0}, color: {1} in content view: {2}", penTypeSel, _selectedColor, this.GetHashCode());
            _selectedHighligherColor = colorTypeSel;
            _selectedHighlighterSize = penTypeSel;

            //pen size value : 1,2,3,4
            int tempPenType = penTypeSel;

            const string pencilCommand = "window.pencil('nitePen')";
            this.Browser.GetBrowser().GetFrame(null).ExecuteJavaScriptAsync(pencilCommand);

            string linewightCommand = "window.lineWight(" + tempPenType + ")";
            this.Browser.GetBrowser().GetFrame(null).ExecuteJavaScriptAsync(linewightCommand);

            if (this.Browser.CanExecuteJavascriptInMainFrame)
            {
                int r, g, b;
                switch (_selectedHighligherColor)
                {
                    case (Int16)PenColor.Yellow:
                        {
                            r = 255; g = 252; b = 0;
                        }
                        break;
                    case (Int16)PenColor.Green:
                        {
                            r = 0; g = 249; b = 0;
                        }
                        break;

                    case (Int16)PenColor.Lake:
                        {
                            r = 0; g = 253; b = 255;
                        }
                        break;
                    case (Int16)PenColor.Purple:
                        {
                            r = 255; g = 64; b = 255;
                        }
                        break;
                    case (Int16)PenColor.Orange:
                        {
                            r = 255; g = 133; b = 23;
                        }
                        break;
                    case (Int16)PenColor.Light_Green:
                        {
                            r = 170; g = 228; b = 100;
                        }
                        break;
                    case (Int16)PenColor.Light_Blue:
                        {
                            r = 169; g = 216; b = 255;
                        }
                        break;
                    case (Int16)PenColor.Pink:
                        {
                            r = 255; g = 172; b = 213;
                        }
                        break;
                    default:
                        {
                            r = 255; g = 252; b = 0;
                        }
                        break;
                }

                string changeColorCommand = "window.changeColor('{\"r\":" + r + ", \"g\":" + g + ", \"b\":" + b + ",\"a\":1}')";
                log.DebugFormat("color: {0}", changeColorCommand);
                this.Browser.GetBrowser().GetFrame(null).ExecuteJavaScriptAsync(changeColorCommand);
            }
            else
            {
                System.Windows.MessageBox.Show("can't execute Js now!");
            }
        }

        private void SetBackGroundByType(int backScreenType)
        {
            log.DebugFormat("Change back ground style: {0} in content Whiteboard view: {1}", backScreenType, this.GetHashCode());
            string tempBackScreen = "";
            if (backScreenType == (Int16)BackScreenColorEnum.Black)
            {
                tempBackScreen = "black";
            }
            else if (backScreenType == (Int16)BackScreenColorEnum.White)
            {
                tempBackScreen = "white";
            }

            if (!string.IsNullOrEmpty(tempBackScreen))
            {
                string command = "window.changeBackground('" + tempBackScreen + "')";
                this.Browser.GetBrowser().GetFrame(null).ExecuteJavaScriptAsync(command);
            }
        }

        private void Wallpaper_Click(object sender, RoutedEventArgs e)
        {
            if (_backScreenWin.IsVisible)
            {
                _backScreenWin.Hide();
                return;
            }
            WhiteboardPopup_ChangeBackgroundTransparent(false);
            log.DebugFormat("Show Wallpaper window in content Whiteboard view: {0}", this.GetHashCode());
            HidePopupWindow(true, false, true);

            _backScreenWin.ToolbarInContentMode = ShareContentMode.Whiteboard;
            _backScreenWin.InitData();
            _backScreenWin.Owner = _controlToolBar;
            //_backScreenWin.Topmost = true;

            double screenLogicHeight = this.Height;
            double screenLogicWidth = this.Width;
            double screenLogicX = this.Left;
            double screenLogicY = this.Top;

            double toolbarLogicY = _controlToolBar.Top;
            double toolbarLogicX = _controlToolBar.Left;
            double toolbarLogicHeight = _controlToolBar.Height;
            double toolbarLogicWidth = _controlToolBar.Width;

            System.Windows.Forms.Screen currentScreen = DpiUtil.GetScreenByHandle(Handle);
            System.Windows.Forms.Screen mainWindowScreen = DpiUtil.GetScreenByHandle(VideoPeopleWindow.Instance.Handle);
            double primaryRatio = 1;
            uint currentScreenDpiX = 96;
            uint currentScreenDpiY = 96;
            uint primaryDpiX = 96;
            uint primaryDpiY = 96;
            try
            {
                DpiUtil.GetDpiByScreen(currentScreen, out currentScreenDpiX, out currentScreenDpiY);

                DpiUtil.GetDpiByScreen(System.Windows.Forms.Screen.PrimaryScreen, out primaryDpiX, out primaryDpiY);
                primaryRatio = (double)primaryDpiX / 96;
            }
            catch (DllNotFoundException ex)
            {
                log.ErrorFormat("Can not load windows dll: {0}", ex);
            }

            if (!currentScreen.DeviceName.Equals(System.Windows.Forms.Screen.PrimaryScreen.DeviceName))
            {
                if (primaryDpiX > currentScreenDpiX)
                {
                    _backScreenWin.Width = 190 / primaryRatio;
                    _backScreenWin.Height = 200 / primaryRatio;
                }
                else if (primaryDpiX < currentScreenDpiX)
                {
                    _backScreenWin.Width = 190 * ((double)currentScreenDpiX / primaryDpiX);
                    _backScreenWin.Height = 200 * ((double)currentScreenDpiY / primaryDpiY);
                }
            }
            else
            {
                _backScreenWin.Width = 190;
                _backScreenWin.Height = 200;
            }

            System.Windows.Point point = _controlToolBar.Wallpaper.PointToScreen(new System.Windows.Point(0, 0));
            if (currentScreen.DeviceName.Equals(mainWindowScreen.DeviceName))
            {
                point.X = point.X / ((double)currentScreenDpiX / 96);
                point.Y = point.Y / ((double)currentScreenDpiY / 96);
            }
            else
            {
                if (primaryRatio >= 1)
                {
                    point.X = point.X / primaryRatio;
                    point.Y = point.Y / primaryRatio;
                }
                else
                {
                    point.X = point.X * primaryRatio;
                    point.Y = point.Y * primaryRatio;
                }
            }

            if (_controlToolBar.ToolbarPanel.Orientation == Orientation.Horizontal)
            {
                if (toolbarLogicY > (screenLogicY + screenLogicHeight * 0.6))
                {
                    _backScreenWin.Top = toolbarLogicY - _backScreenWin.Height;
                    _backScreenWin.Left = point.X - _backScreenWin.Width / 2;
                }
                else
                {
                    _backScreenWin.Top = toolbarLogicY + toolbarLogicHeight;
                    _backScreenWin.Left = point.X - _backScreenWin.Width / 2;
                }
            }

            _backScreenWin.Show();
        }

        private void Revoke_Click(object sender, RoutedEventArgs e)
        {
            log.DebugFormat("Show revoke boolbar in content Whiteboard view: {0}", this.GetHashCode());
            const string command = "window.undoLastOperation()";
            this.Browser.GetBrowser().GetFrame(null).ExecuteJavaScriptAsync(command);
        }

        private void Erase_Click(object sender, RoutedEventArgs e)
        {
            log.DebugFormat("Show reaser icon in content Whiteboard view: {0}", this.GetHashCode());
            HidePopupWindow(true, true, true);
            //set to default pen and highlighter
            _selectedPenAsTarget = ButtonStatus.None;
            ClickButtonChangeBackImage(0, 1);
            _selectedHighlighterAsTarget = ButtonStatus.None;
            ClickButtonChangeBackImage(0, 2);

            InkCanvasCursorStatus = InkCanvasCursorEnum.ERASER;

            const string command = "window.eraser()";
            this.Browser.GetBrowser().GetFrame(null).ExecuteJavaScriptAsync(command);
        }

        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            log.Info("ACS Whiteboard Exit_Click");
            ExitWhiteboard();
        }

        public void ExitWhiteboard()
        {
            HidePopupWindow(true, true, true);
            //MessageBoxConfirm confirm = new MessageBoxConfirm();
            //confirm.Owner = Application.Current.MainWindow;
            //confirm.isResize(false);
            //confirm.SetTitleAndMsg(LanguageUtil.Instance.GetValueByKey("PROMPT"), LanguageUtil.Instance.GetValueByKey("ARE_YOU_SURE_TO_STOP_SHARING"), LanguageUtil.Instance.GetValueByKey("CANCEL"), LanguageUtil.Instance.GetValueByKey("CONFIRM"));
            //confirm.GridButtonText2.PreviewMouseLeftButtonDown += new MouseButtonEventHandler(delegate (object sender1, MouseButtonEventArgs e1) {
            // stop content sending first, otherwise sdk will hung.
            //System.Threading.ThreadPool.GetAvailableThreads
            ThreadPool.QueueUserWorkItem(o => {
                AutoSaveWindowToImage();
                CallController.Instance.StopContent();
                disconn();
                Initiator = false;
                log.Info("ACS Whiteboard disconnect");
                Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    this.Visibility = Visibility.Collapsed;
                    ListeneWhiteBoardExit?.Invoke("exit");
                    //confirm.Close();
                });
            });

            //});
            //confirm.Width *= _ratio;
            //confirm.Height *= _ratio;
            //confirm.ShowDialog() ;
        }

        private void Snap_Click(object sender, RoutedEventArgs e)
        {
            log.Debug("Whiteboard Snap_Click");
            HidePopupWindow(true, true, true);
            string savePicPath = Utils.GenerateScreenPictureName();
            //System.Windows.Forms.Screen s = System.Windows.Forms.Screen.AllScreens[0];
            //int result = MonitorCapture.SaveMonitorPic(s, savePicPath);
            int result = MonitorCapture.SaveWindowPicByHandle(this, Handle, savePicPath);
            MessageBoxTip alert = new MessageBoxTip(this);
            if (result == 0)
            {
                //msgBox.SetTitleAndMsg(LanguageUtil.Instance.GetValueByKey("PROMPT"), LanguageUtil.Instance.GetValueByKey("THE_FILE_IS_SAVED_TO") + ":\n" + Utils.GetScreenPicPath());
                alert.SetTitleAndMsg(LanguageUtil.Instance.GetValueByKey("PROMPT"), LanguageUtil.Instance.GetValueByKey("THE_FILE_IS_SAVED_TO") + ":\n" + Utils.GetScreenPicPath(), LanguageUtil.Instance.GetValueByKey("CONFIRM"));
            }
            else if (-2 == result)
            {
                alert.SetTitleAndMsg(LanguageUtil.Instance.GetValueByKey("PROMPT"), LanguageUtil.Instance.GetValueByKey("PATH_PERMISSION_FOR_SNAP_PICTURE"), LanguageUtil.Instance.GetValueByKey("CONFIRM"));
            }
            else
            {
                //msgBox.SetTitleAndMsg(LanguageUtil.Instance.GetValueByKey("PROMPT"), LanguageUtil.Instance.GetValueByKey("THE_PICTURE_SAVE_FAILED"));
                alert.SetTitleAndMsg(LanguageUtil.Instance.GetValueByKey("PROMPT"), LanguageUtil.Instance.GetValueByKey("THE_PICTURE_SAVE_FAILED"), LanguageUtil.Instance.GetValueByKey("CONFIRM"));
            }
            alert.isResize(false);

            System.Windows.Forms.Screen currentScreen = DpiUtil.GetScreenByHandle(Handle);
            System.Windows.Forms.Screen mainWindowScreen = DpiUtil.GetScreenByHandle(VideoPeopleWindow.Instance.Handle);
            double diffScreenRatio = 1;
            uint currentScreenDpiX = 96;
            uint currentScreenDpiY = 96;
            uint mainScreenDpiX = 96;
            uint mainScreenDpiY = 96;
            try
            {
                DpiUtil.GetDpiByScreen(currentScreen, out currentScreenDpiX, out currentScreenDpiY);

                DpiUtil.GetDpiByScreen(mainWindowScreen, out mainScreenDpiX, out mainScreenDpiY);
                diffScreenRatio = (double)mainScreenDpiX / (double)currentScreenDpiX;
            }
            catch (DllNotFoundException ex)
            {
                log.ErrorFormat("Can not load windows dll: {0}", ex);
            }

            if (diffScreenRatio >= 1)
            {
                alert.Width = 360 / diffScreenRatio;
                alert.Height = 180 / diffScreenRatio;
            }
            else
            {
                alert.Width = 360;
                alert.Height = 180;
            }
            alert.Show();
        }

        private void ChangeBackgroundImage(string filePath)
        {
            log.InfoFormat("ACS Whiteboard ChangeBackgroundImage by upload, {0}", filePath);
            try
            {
                using (Bitmap memoryImage = new Bitmap(filePath))
                {
                    using (Bitmap resized = new Bitmap(memoryImage, new System.Drawing.Size(1280, 720)))
                    {
                        using (MemoryStream ms = new MemoryStream())
                        {
                            resized.Save(ms, memoryImage.RawFormat);
                            ms.Seek(0, SeekOrigin.Begin);

                            // real deploy we use nginx or SLB as reverse proxy port as 80, so could be skip it here.
                            string WHITEBOARD_UPLOAD_URL = string.Format(acsUploadUrl, WhiteBoardServerAddr);

                            // the size of image uploaded can not exceed 5M
                            if (ms.Length > 5 * 1024 * 1024)
                            {
                                MessageBoxTip tip = new MessageBoxTip(this);
                                tip.SetTitleAndMsg(LanguageUtil.Instance.GetValueByKey("PROMPT"), LanguageUtil.Instance.GetValueByKey("PIC_1280_720_LARGE"), LanguageUtil.Instance.GetValueByKey("CONFIRM"));
                                tip.ShowDialog();
                                return;
                            }
                            bool ret = Upload(WHITEBOARD_UPLOAD_URL, ms, JwtParam);
                            if (ret)
                            {
                                log.DebugFormat("Whiteboard ChangeBackgroundImage by upload success");
                                const string command = "window.changeBackground('img')";
                                this.Browser.GetBrowser().GetFrame(null).ExecuteJavaScriptAsync(command);
                            }
                            else
                            {
                                log.ErrorFormat("ACS Whiteboard ChangeBackgroundImage by upload failture");
                            }
                        }
                    }
                }
                
            }
            catch (Exception e)
            {
                log.ErrorFormat("ACS Whiteboard ChangeBackgroundImage by upload, {0}, {1}", filePath, e);
            }
        }

        private bool Upload(string url, Stream imageStream, string jwtParam)
        {
            using (HttpContent fileStreamContent = new StreamContent(imageStream))
            {
                Uri baseAddress = new Uri(WhiteBoardServerAddr);
                CookieContainer cookieContainer = new CookieContainer();
                using (HttpClientHandler handler = new HttpClientHandler() { CookieContainer = cookieContainer })
                using (HttpClient client = new HttpClient(handler))
                using (MultipartFormDataContent formData = new MultipartFormDataContent())
                {
                    cookieContainer.Add(baseAddress, new System.Net.Cookie("jwt", jwtParam));

                    formData.Add(fileStreamContent, "file", "background.jpg");
                    HttpResponseMessage response = client.PostAsync(url, formData).Result;
                    if (!response.IsSuccessStatusCode)
                    {
                        return false;
                    }
                    return true;
                }
            }
        }

        private void Clear_Click(object sender, RoutedEventArgs e)
        {
            log.DebugFormat("Clean pen strokes collection in Whiteboard HidePopupWindow view: {0}", this.GetHashCode());
            HidePopupWindow(true, true, true);
            if (this.Browser.CanExecuteJavascriptInMainFrame)
            {
                const string command = "window.clearProject()";
                this.Browser.GetBrowser().GetFrame(null).ExecuteJavaScriptAsync(command);
            }
            else
            {
                System.Windows.MessageBox.Show("can't execute Js now!");
            }
        }

        private void ToolBarMove_Click(object sender, MouseButtonEventArgs e)
        {
            log.DebugFormat("Move toolbar position in content Whiteboard view: {0}", this.GetHashCode());
            System.Windows.Forms.Screen screen = DpiUtil.GetScreenByHandle(Handle);
            double screenPhysicalX = screen.Bounds.X;
            double screenPhysicalY = screen.Bounds.Y;
            double screenPhysicalWidth = screen.Bounds.Width;
            double screenPhysicalHeight = screen.Bounds.Height;

            double toolBarHeight = _controlToolBar.ActualHeight;
            double toolBarWidth = _controlToolBar.ActualWidth;
            double toolbarLogicY = _controlToolBar.Top;
            double toolbarLogicX = _controlToolBar.Left;

            double primaryRatio = 1;
            uint currentScreendpiX = 96;
            uint currentScreendpiY = 96;
            uint primaryDpiX = 96;
            uint primaryDpiY = 96;
            try
            {
                DpiUtil.GetDpiByScreen(System.Windows.Forms.Screen.PrimaryScreen, out primaryDpiX, out primaryDpiY);
                primaryRatio = (double)primaryDpiX / 96;

                DpiUtil.GetDpiByScreen(screen, out currentScreendpiX, out currentScreendpiY);
            }
            catch (Exception e1)
            {
                log.Error(e1);
            }

            if (System.Windows.Forms.Screen.PrimaryScreen.DeviceName.Equals(screen.DeviceName))
            {
                log.DebugFormat("PrimaryScreen");
                if (toolbarLogicX < screenPhysicalX / primaryRatio)
                {
                    _controlToolBar.Left = screenPhysicalX / primaryRatio;
                    log.DebugFormat("---Whiteboard ToolBar left:{0}", _controlToolBar.Left);
                }
                else if ((toolbarLogicX + toolBarWidth) > (screenPhysicalX + screenPhysicalWidth) / primaryRatio)
                {
                    _controlToolBar.Left = (screenPhysicalX + screenPhysicalWidth) / primaryRatio - toolBarWidth;
                    log.DebugFormat("---Whiteboard ToolBar right:{0}", _controlToolBar.Left);
                }

                if (_controlToolBar.Top + _controlToolBar.Height >= (screenPhysicalY + screenPhysicalHeight) / primaryRatio)
                {
                    _controlToolBar.Top = (screenPhysicalY + screenPhysicalHeight) / primaryRatio - 100;
                    log.DebugFormat("---Whiteboard ToolBar bottom:{0}", _controlToolBar.Top);
                }
                else if (_controlToolBar.Top < screenPhysicalY / primaryRatio)
                {
                    _controlToolBar.Top = screenPhysicalY / primaryRatio + 20;
                    log.DebugFormat("---Whiteboard ToolBar top:{0}", _controlToolBar.Top);
                }
            }
            else
            {
                log.DebugFormat("extend Screen");
                if (toolbarLogicX < screenPhysicalX / primaryRatio)
                {
                    _controlToolBar.Left = screenPhysicalX / primaryRatio;
                    log.DebugFormat("---Whiteboard ToolBar left:{0}", _controlToolBar.Left);
                }
                else if ((toolbarLogicX + toolBarWidth) > (screenPhysicalX + screenPhysicalWidth) / primaryRatio)
                {
                    _controlToolBar.Left = (screenPhysicalX + screenPhysicalWidth) / primaryRatio - toolBarWidth;
                    log.DebugFormat("---Whiteboard ToolBar right:{0}", _controlToolBar.Left);
                }

                if (_controlToolBar.Top + _controlToolBar.Height >= (screenPhysicalY + screenPhysicalHeight) / primaryRatio)
                {
                    if (primaryDpiX > currentScreendpiX)
                    {
                        _controlToolBar.Top = (screenPhysicalY + screenPhysicalHeight - 100) / primaryRatio;
                    }
                    else if (primaryDpiX < currentScreendpiX)
                    {
                        _controlToolBar.Top = (screenPhysicalY + screenPhysicalHeight - 100) / ((double)currentScreendpiX / primaryDpiX);
                    }

                    log.DebugFormat("---Whiteboard ToolBar bottom:{0}", _controlToolBar.Top);
                }
                else if (_controlToolBar.Top < screenPhysicalY / primaryRatio)
                {
                    _controlToolBar.Top = screenPhysicalY / primaryRatio + 20;
                    log.DebugFormat("---Whiteboard ToolBar top:{0}", _controlToolBar.Top);
                }
            }
        }

        private void HidePopupWindow(Boolean brush, Boolean backScreen, Boolean highLighter)
        {
            log.Info("Whiteboard HidePopupWindow");
            Application.Current.Dispatcher.Invoke(() => {
                if (brush && _brushWin.IsVisible)
                {
                    _brushWin.Hide();
                }
                if (backScreen && _backScreenWin.IsVisible)
                {
                    _backScreenWin.Hide();
                }
                if (highLighter && _highlighterWin.IsVisible)
                {
                    _highlighterWin.Hide();
                }
            });
        }
        
        public void TreatmentOnCallEnded()
        {
            bool isStartWhiteBoard =    ContentStreamStatus.ReceivingWhiteBoardStarted == CallController.Instance.CurrentContentStreamStatus
                                     || ContentStreamStatus.SendingWhiteBoardStarted == CallController.Instance.CurrentContentStreamStatus;
            if (isStartWhiteBoard)
            {
                AutoSaveWindowToImage();
            }

            const string command = "window.disconnect()";
            IBrowser browser = this.Browser.GetBrowser();
            browser.MainFrame.ExecuteJavaScriptAsync(command);
        }

        public void DisconnectingWhiteBoard()
        {
            Application.Current.Dispatcher.Invoke(() => {
                Initiator = false;
                disconn();
                log.Info("ACS: whiteboard disconnected");
                this.Visibility = Visibility.Collapsed;
                this.ShowOrHideToolbar(false, null);
                this.Browser.Reload(true);

            });
        }

        public void AutoSaveWindowToImage()
        {
            if (Utils.GetAutoCaptureWin() && this.Visibility == Visibility.Visible)
            {
                log.DebugFormat("Whiteboard auto capture image");
                Application.Current.Dispatcher.Invoke(() => {
                    if (this.WindowState == WindowState.Minimized)
                    {
                        this.WindowState = WindowState.Normal;
                    }
                });
                
                string savePicPath = Utils.GenerateScreenPictureName();
                try
                {
                    Application.Current.Dispatcher.Invoke(() => {
                        MonitorCapture.SaveWindowPicByHandle(this, Handle, savePicPath);
                    });
                }
                catch (Exception e)
                {
                    log.Error("Whiteboard auto capture image, ", e);
                }
            }
        }

        private void ClickButtonChangeBackImage(int taps, int penType)
        {
            //selected
            if (taps == 1)
            {
                if (penType == 1)
                {
                    _controlToolBar.PenSetting.TextForeground = Utils.SelectedForeGround;
                    switch (_selectedColor)
                    {
                        case (Int16)PenColor.Black:
                            _controlToolBar.PenSetting.NormalImage = new System.Windows.Media.Imaging.BitmapImage(new Uri("pack://application:,,,/Resources/Icons/Content/Pen/Type/black_click.png"));
                            _controlToolBar.PenSetting.MouseOverImage = new System.Windows.Media.Imaging.BitmapImage(new Uri("pack://application:,,,/Resources/Icons/Content/Pen/Type/black_click.png"));
                            break;
                        case (Int16)PenColor.White:
                            _controlToolBar.PenSetting.NormalImage = new System.Windows.Media.Imaging.BitmapImage(new Uri("pack://application:,,,/Resources/Icons/Content/Pen/Type/white_click.png"));
                            _controlToolBar.PenSetting.MouseOverImage = new System.Windows.Media.Imaging.BitmapImage(new Uri("pack://application:,,,/Resources/Icons/Content/Pen/Type/white_click.png"));
                            break;
                        case (Int16)PenColor.Blue:
                            _controlToolBar.PenSetting.NormalImage = new System.Windows.Media.Imaging.BitmapImage(new Uri("pack://application:,,,/Resources/Icons/Content/Pen/Type/blue_click.png"));
                            _controlToolBar.PenSetting.MouseOverImage = new System.Windows.Media.Imaging.BitmapImage(new Uri("pack://application:,,,/Resources/Icons/Content/Pen/Type/blue_click.png"));
                            break;
                        case (Int16)PenColor.Green:
                            _controlToolBar.PenSetting.NormalImage = new System.Windows.Media.Imaging.BitmapImage(new Uri("pack://application:,,,/Resources/Icons/Content/Pen/Type/green_click.png"));
                            _controlToolBar.PenSetting.MouseOverImage = new System.Windows.Media.Imaging.BitmapImage(new Uri("pack://application:,,,/Resources/Icons/Content/Pen/Type/green_click.png"));
                            break;
                        case (Int16)PenColor.Yellow:
                            _controlToolBar.PenSetting.NormalImage = new System.Windows.Media.Imaging.BitmapImage(new Uri("pack://application:,,,/Resources/Icons/Content/Pen/Type/yellow_click.png"));
                            _controlToolBar.PenSetting.MouseOverImage = new System.Windows.Media.Imaging.BitmapImage(new Uri("pack://application:,,,/Resources/Icons/Content/Pen/Type/yellow_click.png"));
                            break;
                        case (Int16)PenColor.Orange:
                            _controlToolBar.PenSetting.NormalImage = new System.Windows.Media.Imaging.BitmapImage(new Uri("pack://application:,,,/Resources/Icons/Content/Pen/Type/orange_click.png"));
                            _controlToolBar.PenSetting.MouseOverImage = new System.Windows.Media.Imaging.BitmapImage(new Uri("pack://application:,,,/Resources/Icons/Content/Pen/Type/orange_click.png"));
                            break;
                        case (Int16)PenColor.Red:
                            _controlToolBar.PenSetting.NormalImage = new System.Windows.Media.Imaging.BitmapImage(new Uri("pack://application:,,,/Resources/Icons/Content/Pen/Type/red_click.png"));
                            _controlToolBar.PenSetting.MouseOverImage = new System.Windows.Media.Imaging.BitmapImage(new Uri("pack://application:,,,/Resources/Icons/Content/Pen/Type/red_click.png"));
                            break;
                        case (Int16)PenColor.Purple:
                            _controlToolBar.PenSetting.NormalImage = new System.Windows.Media.Imaging.BitmapImage(new Uri("pack://application:,,,/Resources/Icons/Content/Pen/Type/purple_click.png"));
                            _controlToolBar.PenSetting.MouseOverImage = new System.Windows.Media.Imaging.BitmapImage(new Uri("pack://application:,,,/Resources/Icons/Content/Pen/Type/purple_click.png"));
                            break;
                        default:
                            _controlToolBar.PenSetting.NormalImage = new System.Windows.Media.Imaging.BitmapImage(new Uri("pack://application:,,,/Resources/Icons/Content/Pen/Type/black_click.png"));
                            _controlToolBar.PenSetting.MouseOverImage = new System.Windows.Media.Imaging.BitmapImage(new Uri("pack://application:,,,/Resources/Icons/Content/Pen/Type/black_click.png"));
                            break;
                    }
                }
                else if (penType == 2)
                {
                    _controlToolBar.Highlighter.TextForeground = Utils.SelectedForeGround;
                    switch (_selectedHighligherColor)
                    {
                        case (Int16)PenColor.Yellow:
                            _controlToolBar.Highlighter.NormalImage = new System.Windows.Media.Imaging.BitmapImage(new Uri("pack://application:,,,/Resources/Icons/Content/Pen/Type/h_yellow_click.png"));
                            _controlToolBar.Highlighter.MouseOverImage = new System.Windows.Media.Imaging.BitmapImage(new Uri("pack://application:,,,/Resources/Icons/Content/Pen/Type/h_yellow_click.png"));
                            break;
                        case (Int16)PenColor.Purple:
                            _controlToolBar.Highlighter.NormalImage = new System.Windows.Media.Imaging.BitmapImage(new Uri("pack://application:,,,/Resources/Icons/Content/Pen/Type/h_purple_click.png"));
                            _controlToolBar.Highlighter.MouseOverImage = new System.Windows.Media.Imaging.BitmapImage(new Uri("pack://application:,,,/Resources/Icons/Content/Pen/Type/h_purple_click.png"));
                            break;
                        case (Int16)PenColor.Lake:
                            _controlToolBar.Highlighter.NormalImage = new System.Windows.Media.Imaging.BitmapImage(new Uri("pack://application:,,,/Resources/Icons/Content/Pen/Type/h_lake_click.png"));
                            _controlToolBar.Highlighter.MouseOverImage = new System.Windows.Media.Imaging.BitmapImage(new Uri("pack://application:,,,/Resources/Icons/Content/Pen/Type/h_lake_click.png"));
                            break;
                        case (Int16)PenColor.Green:
                            _controlToolBar.Highlighter.NormalImage = new System.Windows.Media.Imaging.BitmapImage(new Uri("pack://application:,,,/Resources/Icons/Content/Pen/Type/h_green_click.png"));
                            _controlToolBar.Highlighter.MouseOverImage = new System.Windows.Media.Imaging.BitmapImage(new Uri("pack://application:,,,/Resources/Icons/Content/Pen/Type/h_green_click.png"));
                            break;
                        case (Int16)PenColor.Orange:
                            _controlToolBar.Highlighter.NormalImage = new System.Windows.Media.Imaging.BitmapImage(new Uri("pack://application:,,,/Resources/Icons/Content/Pen/Type/h_orange_click.png"));
                            _controlToolBar.Highlighter.MouseOverImage = new System.Windows.Media.Imaging.BitmapImage(new Uri("pack://application:,,,/Resources/Icons/Content/Pen/Type/h_orange_click.png"));
                            break;
                        case (Int16)PenColor.Light_Green:
                            _controlToolBar.Highlighter.NormalImage = new System.Windows.Media.Imaging.BitmapImage(new Uri("pack://application:,,,/Resources/Icons/Content/Pen/Type/h_light-green_click.png"));
                            _controlToolBar.Highlighter.MouseOverImage = new System.Windows.Media.Imaging.BitmapImage(new Uri("pack://application:,,,/Resources/Icons/Content/Pen/Type/h_light-green_click.png"));
                            break;
                        case (Int16)PenColor.Light_Blue:
                            _controlToolBar.Highlighter.NormalImage = new System.Windows.Media.Imaging.BitmapImage(new Uri("pack://application:,,,/Resources/Icons/Content/Pen/Type/h_light-blue_click.png"));
                            _controlToolBar.Highlighter.MouseOverImage = new System.Windows.Media.Imaging.BitmapImage(new Uri("pack://application:,,,/Resources/Icons/Content/Pen/Type/h_light-blue_click.png"));
                            break;
                        case (Int16)PenColor.Pink:
                            _controlToolBar.Highlighter.NormalImage = new System.Windows.Media.Imaging.BitmapImage(new Uri("pack://application:,,,/Resources/Icons/Content/Pen/Type/h_pink_click.png"));
                            _controlToolBar.Highlighter.MouseOverImage = new System.Windows.Media.Imaging.BitmapImage(new Uri("pack://application:,,,/Resources/Icons/Content/Pen/Type/h_pink_click.png"));
                            break;
                        default:
                            _controlToolBar.Highlighter.NormalImage = new System.Windows.Media.Imaging.BitmapImage(new Uri("pack://application:,,,/Resources/Icons/Content/Pen/Type/h_yellow_click.png"));
                            _controlToolBar.Highlighter.MouseOverImage = new System.Windows.Media.Imaging.BitmapImage(new Uri("pack://application:,,,/Resources/Icons/Content/Pen/Type/h_yellow_click.png"));
                            break;
                    }
                }
            }
            else
            {
                if (penType == 1)
                {
                    _controlToolBar.PenSetting.TextForeground = Utils.DefaultForeGround;
                    switch (_selectedColor)
                    {
                        case (Int16)PenColor.Black:
                            _controlToolBar.PenSetting.NormalImage = new System.Windows.Media.Imaging.BitmapImage(new Uri("pack://application:,,,/Resources/Icons/Content/Pen/Type/black_default.png"));
                            _controlToolBar.PenSetting.MouseOverImage = new System.Windows.Media.Imaging.BitmapImage(new Uri("pack://application:,,,/Resources/Icons/Content/Pen/Type/black_default.png"));
                            break;
                        case (Int16)PenColor.White:
                            _controlToolBar.PenSetting.NormalImage = new System.Windows.Media.Imaging.BitmapImage(new Uri("pack://application:,,,/Resources/Icons/Content/Pen/Type/white_default.png"));
                            _controlToolBar.PenSetting.MouseOverImage = new System.Windows.Media.Imaging.BitmapImage(new Uri("pack://application:,,,/Resources/Icons/Content/Pen/Type/white_default.png"));
                            break;
                        case (Int16)PenColor.Blue:
                            _controlToolBar.PenSetting.NormalImage = new System.Windows.Media.Imaging.BitmapImage(new Uri("pack://application:,,,/Resources/Icons/Content/Pen/Type/blue_default.png"));
                            _controlToolBar.PenSetting.MouseOverImage = new System.Windows.Media.Imaging.BitmapImage(new Uri("pack://application:,,,/Resources/Icons/Content/Pen/Type/blue_default.png"));
                            break;
                        case (Int16)PenColor.Green:
                            _controlToolBar.PenSetting.NormalImage = new System.Windows.Media.Imaging.BitmapImage(new Uri("pack://application:,,,/Resources/Icons/Content/Pen/Type/green_default.png"));
                            _controlToolBar.PenSetting.MouseOverImage = new System.Windows.Media.Imaging.BitmapImage(new Uri("pack://application:,,,/Resources/Icons/Content/Pen/Type/green_default.png"));
                            break;
                        case (Int16)PenColor.Yellow:
                            _controlToolBar.PenSetting.NormalImage = new System.Windows.Media.Imaging.BitmapImage(new Uri("pack://application:,,,/Resources/Icons/Content/Pen/Type/yellow_default.png"));
                            _controlToolBar.PenSetting.MouseOverImage = new System.Windows.Media.Imaging.BitmapImage(new Uri("pack://application:,,,/Resources/Icons/Content/Pen/Type/yellow_default.png"));
                            break;
                        case (Int16)PenColor.Orange:
                            _controlToolBar.PenSetting.NormalImage = new System.Windows.Media.Imaging.BitmapImage(new Uri("pack://application:,,,/Resources/Icons/Content/Pen/Type/orange_default.png"));
                            _controlToolBar.PenSetting.MouseOverImage = new System.Windows.Media.Imaging.BitmapImage(new Uri("pack://application:,,,/Resources/Icons/Content/Pen/Type/orange_default.png"));
                            break;
                        case (Int16)PenColor.Red:
                            _controlToolBar.PenSetting.NormalImage = new System.Windows.Media.Imaging.BitmapImage(new Uri("pack://application:,,,/Resources/Icons/Content/Pen/Type/red_default.png"));
                            _controlToolBar.PenSetting.MouseOverImage = new System.Windows.Media.Imaging.BitmapImage(new Uri("pack://application:,,,/Resources/Icons/Content/Pen/Type/red_default.png"));
                            break;
                        case (Int16)PenColor.Purple:
                            _controlToolBar.PenSetting.NormalImage = new System.Windows.Media.Imaging.BitmapImage(new Uri("pack://application:,,,/Resources/Icons/Content/Pen/Type/purple_default.png"));
                            _controlToolBar.PenSetting.MouseOverImage = new System.Windows.Media.Imaging.BitmapImage(new Uri("pack://application:,,,/Resources/Icons/Content/Pen/Type/purple_default.png"));
                            break;
                        default:
                            _controlToolBar.PenSetting.NormalImage = new System.Windows.Media.Imaging.BitmapImage(new Uri("pack://application:,,,/Resources/Icons/Content/Pen/Type/black_default.png"));
                            _controlToolBar.PenSetting.MouseOverImage = new System.Windows.Media.Imaging.BitmapImage(new Uri("pack://application:,,,/Resources/Icons/Content/Pen/Type/black_default.png"));
                            break;
                    }
                }
                else if (penType == 2)
                {
                    _controlToolBar.Highlighter.TextForeground = Utils.DefaultForeGround;
                    switch (_selectedHighligherColor)
                    {
                        case (Int16)PenColor.Yellow:
                            _controlToolBar.Highlighter.NormalImage = new System.Windows.Media.Imaging.BitmapImage(new Uri("pack://application:,,,/Resources/Icons/Content/Pen/Type/h_yellow_default.png"));
                            _controlToolBar.Highlighter.MouseOverImage = new System.Windows.Media.Imaging.BitmapImage(new Uri("pack://application:,,,/Resources/Icons/Content/Pen/Type/h_yellow_default.png"));
                            break;
                        case (Int16)PenColor.Purple:
                            _controlToolBar.Highlighter.NormalImage = new System.Windows.Media.Imaging.BitmapImage(new Uri("pack://application:,,,/Resources/Icons/Content/Pen/Type/h_purple_default.png"));
                            _controlToolBar.Highlighter.MouseOverImage = new System.Windows.Media.Imaging.BitmapImage(new Uri("pack://application:,,,/Resources/Icons/Content/Pen/Type/h_purple_default.png"));
                            break;
                        case (Int16)PenColor.Lake:
                            _controlToolBar.Highlighter.NormalImage = new System.Windows.Media.Imaging.BitmapImage(new Uri("pack://application:,,,/Resources/Icons/Content/Pen/Type/h_lake_default.png"));
                            _controlToolBar.Highlighter.MouseOverImage = new System.Windows.Media.Imaging.BitmapImage(new Uri("pack://application:,,,/Resources/Icons/Content/Pen/Type/h_lake_default.png"));
                            break;
                        case (Int16)PenColor.Green:
                            _controlToolBar.Highlighter.NormalImage = new System.Windows.Media.Imaging.BitmapImage(new Uri("pack://application:,,,/Resources/Icons/Content/Pen/Type/h_green_default.png"));
                            _controlToolBar.Highlighter.MouseOverImage = new System.Windows.Media.Imaging.BitmapImage(new Uri("pack://application:,,,/Resources/Icons/Content/Pen/Type/h_green_default.png"));
                            break;
                        case (Int16)PenColor.Orange:
                            _controlToolBar.Highlighter.NormalImage = new System.Windows.Media.Imaging.BitmapImage(new Uri("pack://application:,,,/Resources/Icons/Content/Pen/Type/h_orange_default.png"));
                            _controlToolBar.Highlighter.MouseOverImage = new System.Windows.Media.Imaging.BitmapImage(new Uri("pack://application:,,,/Resources/Icons/Content/Pen/Type/h_orange_default.png"));
                            break;
                        case (Int16)PenColor.Light_Green:
                            _controlToolBar.Highlighter.NormalImage = new System.Windows.Media.Imaging.BitmapImage(new Uri("pack://application:,,,/Resources/Icons/Content/Pen/Type/h_light-green_default.png"));
                            _controlToolBar.Highlighter.MouseOverImage = new System.Windows.Media.Imaging.BitmapImage(new Uri("pack://application:,,,/Resources/Icons/Content/Pen/Type/h_light-green_default.png"));
                            break;
                        case (Int16)PenColor.Light_Blue:
                            _controlToolBar.Highlighter.NormalImage = new System.Windows.Media.Imaging.BitmapImage(new Uri("pack://application:,,,/Resources/Icons/Content/Pen/Type/h_light-blue_default.png"));
                            _controlToolBar.Highlighter.MouseOverImage = new System.Windows.Media.Imaging.BitmapImage(new Uri("pack://application:,,,/Resources/Icons/Content/Pen/Type/h_light-blue_default.png"));
                            break;
                        case (Int16)PenColor.Pink:
                            _controlToolBar.Highlighter.NormalImage = new System.Windows.Media.Imaging.BitmapImage(new Uri("pack://application:,,,/Resources/Icons/Content/Pen/Type/h_pink_default.png"));
                            _controlToolBar.Highlighter.MouseOverImage = new System.Windows.Media.Imaging.BitmapImage(new Uri("pack://application:,,,/Resources/Icons/Content/Pen/Type/h_pink_default.png"));
                            break;
                        default:
                            _controlToolBar.Highlighter.NormalImage = new System.Windows.Media.Imaging.BitmapImage(new Uri("pack://application:,,,/Resources/Icons/Content/Pen/Type/h_yellow_default.png"));
                            _controlToolBar.Highlighter.MouseOverImage = new System.Windows.Media.Imaging.BitmapImage(new Uri("pack://application:,,,/Resources/Icons/Content/Pen/Type/h_yellow_default.png"));
                            break;
                    }
                }
            }
        }

        #region -- white board popup window event--

        private void WhiteboardPopup_MouseLeftButtonDown()
        {
            log.DebugFormat("WhiteboardPopup MouseLeftButtonDown");
            if (_brushWin.IsVisible)
            {
                log.DebugFormat("Hide brush screen in content Whiteboard view: {0}", this.GetHashCode());
                SelectedPenAndColor();
                _brushWin.Hide();
            }
            if (_backScreenWin.IsVisible)
            {
                log.DebugFormat("Hide background screen in content Whiteboard view: {0}", this.GetHashCode());
                _backScreenWin.Hide();
            }
            if (_highlighterWin.IsVisible)
            {
                log.DebugFormat("Hide highlighter screen in content Whiteboard view: {0}", this.GetHashCode());
                SelectedHighlightrAndColor();
                _highlighterWin.Hide();
            }

            WhiteboardPopup_ChangeBackgroundTransparent(true);
        }

        private void WhiteboardPopup_ChangeBackgroundTransparent(bool type)
        {
            log.InfoFormat("WhiteboardPopup ChangeBackground is hidden: {0}", type);
            if (type)
            {
            //    _whiteboardPopup.Background = System.Windows.Media.Brushes.Transparent;
                _whiteboardPopup.Hide();
            }
            else
            {
            //    _whiteboardPopup.Background = new SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#01FFFFFF"));
                _whiteboardPopup.Show();
            }
        }

        private void WhiteboardPopup_ChangePosition()
        {
            log.DebugFormat("WhiteboardPopup ChangePosition");
            log.InfoFormat("WhiteboardPopup before x:{0},y:{1},w:{2},h:{3}", _whiteboardPopup.Left, _whiteboardPopup.Top, _whiteboardPopup.Width, _whiteboardPopup.Height);
            _whiteboardPopup.Left = this.ActualLeft(Handle);
            _whiteboardPopup.Top = this.ActualTop(Handle) + this.WhiteboardTitle.ActualHeight + 10;
            _whiteboardPopup.Height = this.WhiteboardContent.ActualHeight;
            _whiteboardPopup.Width = this.WhiteboardContent.ActualWidth;
            log.InfoFormat("WhiteboardPopup after x:{0},y:{1},w:{2},h:{3}", _whiteboardPopup.Left, _whiteboardPopup.Top, _whiteboardPopup.Width, _whiteboardPopup.Height);
            _whiteboardPopup.Hide();
        }

        private void ToolBar_ChangePosition()
        {
            log.DebugFormat("ToolBar_ChangePosition");
            _controlToolBar.Top = this.ActualTop(Handle) + 40;
            _controlToolBar.Left = (this.Width - _controlToolBar.Width) / 2 + this.ActualLeft(Handle);
        }
        #endregion
        
        private void ChangeWhiteboardWindowMinState(object sender, RoutedEventArgs e)
        {
            MinimizeWindow();
        }

        private void ChangeWhiteboardWindowMaxState(object sender, RoutedEventArgs e)
        {
            MaximizeWindow();
            WhiteboardPopup_ChangePosition();
        }
        
        private void ChangeWhiteboardWindowNormalState(object sender, RoutedEventArgs e)
        {
            RestoreWindow();
            WhiteboardPopup_ChangePosition();
        }
        
        private void ChangeWhiteboardWindowFullScreenState(object sender, RoutedEventArgs e)
        {
            SetFullScreen();
            WhiteboardPopup_ChangePosition();
        }
        
        private void ContentWhiteBoard_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            ChangeToolbarButtonsStatus();
        }

        private void WindowState_Changed(object sender, EventArgs e)
        {
            ChangeToolbarButtonsStatus();
        }

        private void ChangeToolbarButtonsStatus()
        {
            this.minBtn.Visibility              = FullScreenStatus ? Visibility.Collapsed : Visibility.Visible;
            this.maxBtn.Visibility              = (FullScreenStatus || WindowState == WindowState.Maximized) ? Visibility.Collapsed : Visibility.Visible;
            this.restoreBtn.Visibility          = (FullScreenStatus || WindowState != WindowState.Maximized) ? Visibility.Collapsed : Visibility.Visible;
            this.fullScreenBtn.Visibility       = FullScreenStatus ? Visibility.Collapsed : Visibility.Visible;
            this.exitFullScreenBtn.Visibility   = FullScreenStatus ? Visibility.Visible : Visibility.Collapsed;
        }
        
        private void ChangeWhiteboardWindowExitFullScreenState(object sender, RoutedEventArgs e)
        {
            log.DebugFormat("ChangeWhiteboardWindowExitFullScreenState");
            RestoreWindow();
            WhiteboardPopup_ChangePosition();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            log.Debug("Whiteboard Window_Loaded");
            Utils.SetSoftwareRender(this);

            System.Windows.Forms.Screen screen = DpiUtil.GetScreenByHandle(Handle);
            if (screen.Bounds.Width > 1920)
            {
                IsHighDpiDevice = true;
            }

            SetToolBarPositionLogical(screen);
        }

        private void Title_OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            log.DebugFormat("Title_OnMouseLeftButtonDown");
            if (this.WindowState != WindowState.Maximized)
            {
                this.DragMove();
                WhiteboardPopup_ChangePosition();
            }
        }

        private void disconn()
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                if (this.WindowState != WindowState.Normal)
                {
                    RestoreWindow();
                }
            });

            log.Info("Whiteboard disconnect");
            const string command = "window.disconnect()";
            IBrowser browser = this.Browser.GetBrowser();
            browser.MainFrame.ExecuteJavaScriptAsync(command);
            CallController.Instance.IsConnected2WhiteBoard = false;
        }

        private void Window_ChangeSize(object sender, SizeChangedEventArgs e)
        {
            log.InfoFormat("Whiteboard Window_ChangeSize, width:{0}", this.Width);
            //this.Height = this.Width * 9 / 16;
            // WhiteboardContent.Height = this.Width * 9 / 16;
            WhiteboardPopup_ChangePosition();

            //ToolBar_ChangePosition();
        }
        
        public void getJwtParam()
        {
            log.Info("getJwtParam");
            if (CallController.Instance.acsInfo != null && !string.IsNullOrEmpty(CallController.Instance.acsInfo.acsJsonWebToken))
            {
                this.JwtParam = CallController.Instance.acsInfo.acsJsonWebToken;
            }
            else
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    Task<JavascriptResponse> task = this.Browser.GetMainFrame().EvaluateScriptAsync("(function() {return window.jwtParam; })();", null);
                    task.ContinueWith(t =>
                    {
                        if (!t.IsFaulted)
                        {
                            JavascriptResponse response = t.Result;
                            this.JwtParam = (string)response.Result;
                        }
                    }, TaskScheduler.FromCurrentSynchronizationContext());
                });

            }
        }

        #endregion

        private void wfh_ChildChanged(object sender, System.Windows.Forms.Integration.ChildChangedEventArgs e)
        {

        }

    }

    public class CustomMenuHandler : IContextMenuHandler
    {
        public void OnBeforeContextMenu(IWebBrowser browserControl, IBrowser browser, IFrame frame, IContextMenuParams parameters, IMenuModel model)
        {
            model.Clear();
        }

        public bool OnContextMenuCommand(IWebBrowser browserControl, IBrowser browser, IFrame frame, IContextMenuParams parameters, CefMenuCommand commandId, CefEventFlags eventFlags)
        {
            return false;
        }

        public void OnContextMenuDismissed(IWebBrowser browserControl, IBrowser browser, IFrame frame)
        {

        }

        public bool RunContextMenu(IWebBrowser browserControl, IBrowser browser, IFrame frame, IContextMenuParams parameters, IMenuModel model, IRunContextMenuCallback callback)
        {
            return false;
        }
    }

    public class DownloadHandler : IDownloadHandler
    {

        private readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public void OnBeforeDownload(IBrowser browser, DownloadItem downloadItem, IBeforeDownloadCallback callback)
        {
            log.Debug("get before download event.");
        }

        public void OnBeforeDownload(IWebBrowser chromiumWebBrowser, IBrowser browser, DownloadItem downloadItem, IBeforeDownloadCallback callback)
        {
            log.Debug("On before download!");
        }

        public void OnDownloadUpdated(IBrowser browser, DownloadItem downloadItem, IDownloadItemCallback callback)
        {
            log.Debug("Get download update event!");
        }

        public void OnDownloadUpdated(IWebBrowser chromiumWebBrowser, IBrowser browser, DownloadItem downloadItem, IDownloadItemCallback callback)
        {
            log.Debug("On download updated!");
        }
    }

    public class CefRequestHandler : IRequestHandler
    {
        public ContentWhiteboard _board { get; set; }

        private readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        bool IRequestHandler.GetAuthCredentials(IWebBrowser browserControl, IBrowser browser, IFrame frame, bool isProxy, string host, int port, string realm, string scheme, IAuthCallback callback)
        {
            return false;
        }

        IResponseFilter IRequestHandler.GetResourceResponseFilter(IWebBrowser browserControl, IBrowser browser, IFrame frame, IRequest request, IResponse response)
        {
            return null;
        }
        
        CefReturnValue IRequestHandler.OnBeforeResourceLoad(IWebBrowser browserControl, IBrowser browser, IFrame frame, IRequest request, IRequestCallback callback)
        {
            return CefReturnValue.Continue;
        }

        bool IRequestHandler.OnCertificateError(IWebBrowser browserControl, IBrowser browser, CefErrorCode errorCode, string requestUrl, ISslInfo sslInfo, IRequestCallback callback)
        {
            if (browser.IsDisposed)
            {
                return false;
            }

            if (!callback.IsDisposed)
            {
                using (callback)
                {
                    //To allow certificate
                    callback.Continue(true);
                    return true;
                }
            }

            return false;
        }

        bool IRequestHandler.OnOpenUrlFromTab(IWebBrowser browserControl, IBrowser browser, IFrame frame, string targetUrl, WindowOpenDisposition targetDisposition, bool userGesture)
        {
            return false;
        }

        void IRequestHandler.OnPluginCrashed(IWebBrowser browserControl, IBrowser browser, string pluginPath)
        {
            throw new NotImplementedException();
        }

        bool IRequestHandler.OnProtocolExecution(IWebBrowser browserControl, IBrowser browser, string url)
        {
            return false;
        }

        bool IRequestHandler.OnQuotaRequest(IWebBrowser browserControl, IBrowser browser, string originUrl, long newSize, IRequestCallback callback)
        {
            return false;
        }

        void IRequestHandler.OnRenderProcessTerminated(IWebBrowser browserControl, IBrowser browser, CefTerminationStatus status)
        {
            if (browser.IsDisposed)
            {
                return;
            }

            log.Debug("the reder process terminated!");
            browser.Reload(true);
        }

        void IRequestHandler.OnRenderViewReady(IWebBrowser browserControl, IBrowser browser)
        {

        }

        void IRequestHandler.OnResourceLoadComplete(IWebBrowser browserControl, IBrowser browser, IFrame frame, IRequest request, IResponse response, UrlRequestStatus status, long receivedContentLength)
        {
            if (browser.IsDisposed || null == browser.MainFrame)
            {
                return;
            }

            if ((null != _board) && (_board.JwtParam == null))
            {
                _board.getJwtParam();
            }

            // check is disposed again to avoid disposed during the above function
            if (browser.IsDisposed || null == browser.MainFrame)
            {
                return;
            }

            try
            {
                // hide scroll bar.
                browser.MainFrame.ExecuteJavaScriptAsync("document.body.style.overflow = 'hidden'");
            }
            catch (Exception e)
            {
                log.InfoFormat("Exception on OnResourceLoadComplete, error:{0}", e.Message);
            }
            
        }

        void IRequestHandler.OnResourceRedirect(IWebBrowser browserControl, IBrowser browser, IFrame frame, IRequest request, IResponse response, ref string newUrl)
        {

        }

        bool IRequestHandler.OnResourceResponse(IWebBrowser browserControl, IBrowser browser, IFrame frame, IRequest request, IResponse response)
        {
            int statusCode = response.StatusCode;
            return false;
        }

        bool IRequestHandler.OnSelectClientCertificate(IWebBrowser browserControl, IBrowser browser, bool isProxy, string host, int port, X509Certificate2Collection certificates, ISelectClientCertificateCallback callback)
        {
            return false;
        }

        public bool OnBeforeBrowse(IWebBrowser chromiumWebBrowser, IBrowser browser, IFrame frame, IRequest request, bool userGesture, bool isRedirect)
        {
            return false;
        }

        public bool CanGetCookies(IWebBrowser chromiumWebBrowser, IBrowser browser, IFrame frame, IRequest request)
        {
            return true;
        }

        public bool CanSetCookie(IWebBrowser chromiumWebBrowser, IBrowser browser, IFrame frame, IRequest request, CefSharp.Cookie cookie)
        {
            return true;
        }
    }

    public class AcsStartResponse
    {
        public bool isstarted { get; set; }
    }
}
