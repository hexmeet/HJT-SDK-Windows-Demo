using EasyVideoWin.CustomControls;
using EasyVideoWin.Helpers;
using EasyVideoWin.Helpers.InkCanvasEditingModes;
using EasyVideoWin.Model;
using log4net;
using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Resources;
using static EasyVideoWin.Helpers.DisplayUtil;
using static EasyVideoWin.View.BackScreenColorSelectWindow;
using static EasyVideoWin.View.BrushHighlighterWindow;
using static EasyVideoWin.View.BrushSelectWindow;

namespace EasyVideoWin.View
{
    /// <summary>
    /// Interaction logic for ContentControlView.xaml
    /// </summary>
    public partial class ContentControlView : Window, INotifyPropertyChanged, IMasterDisplayWindow, IDisposable
    {
        #region -- Members --

        private readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private System.Windows.Forms.Screen _selScreen = null;

        //private IntPtr _curHandle;
        DrawingAttributes _inkDA = new DrawingAttributes();

        private Cursor _selectedPen;
        private Cursor _eraser;
        private Cursor _highLighter;
        private bool _isOpenHighlighterPen = false;
        private int _selectedColor = (Int16)PenColor.Black;
        private int _selectedSize = 1;
        private ButtonStatus _selectedPenAsTarget = ButtonStatus.None;
        private int _selectedHighligherColor = (Int16)PenColor.Yellow;
        private int _selectedHighlighterSize = 3;
        private ButtonStatus _selectedHighlighterAsTarget = ButtonStatus.None;

        private BrushSelectWindow _brushWin = null;
        private BrushHighlighterWindow _highlighterWin = null;
        private BackScreenColorSelectWindow _backScreenWin = null;
        private ContentControlToolBarView _controlToolBar = null;

        private InkCanvasCursorEnum _inkCanvasCursorStatus;

        private SolidColorBrush _approximateTransparentBrush;

        public event PropertyChangedEventHandler PropertyChanged;

        //revike last draw
        private CommandStack _cmdStack;
        private int _editingOperationCount;
        private float _ratio = 1;

        #endregion

        #region -- Properties --

        public IntPtr Handle { get; set; } = IntPtr.Zero;

        public InkCanvasCursorEnum InkCanvasCursorStatus
        {
            get
            {
                return _inkCanvasCursorStatus;
            }
            set
            {

                _inkCanvasCursorStatus = value;
                log.Debug("ContentControlView InkCanvasCursorStatus : " + _inkCanvasCursorStatus);
                switch (_inkCanvasCursorStatus)
                {
                    case InkCanvasCursorEnum.DEFAULT:
                        this.inkCanvas1.Cursor = null;
                        break;
                    case InkCanvasCursorEnum.PEN:
                        this.inkCanvas1.Cursor = _selectedPen;
                        break;
                    case InkCanvasCursorEnum.ERASER:
                        this.inkCanvas1.Cursor = _eraser;
                        break;
                    default:
                        this.inkCanvas1.Cursor = null;
                        break;
                }

                OnPropertyChanged("StartCursorVisibility");
                OnPropertyChanged("StopCursorVisibility");
            }
        }

        public Visibility StartCursorVisibility
        {
            get
            {
                log.Debug("ContentControlView StartCursorVisibility: " + (InkCanvasCursorEnum.DEFAULT == InkCanvasCursorStatus ? Visibility.Visible : Visibility.Collapsed));
                return InkCanvasCursorEnum.DEFAULT == InkCanvasCursorStatus ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        public Visibility StopCursorVisibility
        {
            get
            {
                log.Debug("ContentControlView StopCursorVisibility: " + (InkCanvasCursorEnum.DEFAULT == InkCanvasCursorStatus ? Visibility.Collapsed : Visibility.Visible));
                return InkCanvasCursorEnum.DEFAULT == InkCanvasCursorStatus ? Visibility.Collapsed : Visibility.Visible;
            }
        }

        public bool IsDisposed { get; private set; } = false;

        #endregion

        #region -- Constructors --

        public ContentControlView()
        {
            InitializeComponent();

            _brushWin = new BrushSelectWindow();
            _backScreenWin = new BackScreenColorSelectWindow();
            _controlToolBar = new ContentControlToolBarView(true);
            _highlighterWin = new BrushHighlighterWindow();

            object color = ColorConverter.ConvertFromString("#01FFFFFF");
            _approximateTransparentBrush = new SolidColorBrush((Color)color);

            InitCursorInfo();
            InitToolbar();

            this.MaxHeight = SystemParameters.MaximumWindowTrackHeight;
            this.inkCanvas1.Background = Brushes.Transparent;

            //inkCanvas1.EditingMode = InkCanvasEditingMode.Ink;

            _inkDA.Width = 10;
            _inkDA.Height = 10;
            //inkDA.Color = Color.FromArgb(255, 255, 255, 255);
            _inkDA.Color = Color.FromRgb(0, 0, 0);
            this.inkCanvas1.DefaultDrawingAttributes = _inkDA;

            //ShowHideGrayBackground(false);

            // init background type to transparent
            Utils.saveBackColorTypeSelection("2");
            SetBackGroundByType(2);

            this.Background = Brushes.Transparent;
            InkCanvasCursorStatus = InkCanvasCursorEnum.DEFAULT;

            _brushWin.Closed += PenWin_Closed;
            _backScreenWin.Closed += BackScreenWin_Closed;

            //UndoRedo
            _cmdStack = new CommandStack(inkCanvas1.Strokes);
            inkCanvas1.MouseUp += new MouseButtonEventHandler(inkCanvas1_MouseUp);
            inkCanvas1.Strokes.StrokesChanged += Strokes_StrokesChanged;
            inkCanvas1.SelectionMoving += new InkCanvasSelectionEditingEventHandler(inkCanvas1_SelectionMovingOrResizing);
            inkCanvas1.SelectionResizing += new InkCanvasSelectionEditingEventHandler(inkCanvas1_SelectionMovingOrResizing);

            _brushWin.ListenerBrush += new ListenerBrushHandler(SetPenAndColor);
            _highlighterWin.ListenerHighlighter += new ListenerHighlighterHandler(SetHighlighterPenAndColor);
            _backScreenWin.ListenerBackGroundSelected += new ListenerBackGroundSelectedHandler(SetBackGroundByType);

            _controlToolBar.ListenerEnableShareSoundClick += new ContentControlToolBarView.ListenerClickHandler(EnableShareSound_Click);
            _controlToolBar.ListenerDisableShareSoundClick += new ContentControlToolBarView.ListenerClickHandler(DisableShareSound_Click);
            _controlToolBar.ListenerStartCursorClick += new ContentControlToolBarView.ListenerClickHandler(StartCursor_Click);
            _controlToolBar.ListenerStopCursorClick += new ContentControlToolBarView.ListenerClickHandler(StopCursor_Click);
            _controlToolBar.ListenerPenSettingClick += new ContentControlToolBarView.ListenerClickHandler(PenSetting_Click);
            _controlToolBar.ListenerHighlighterClick += new ContentControlToolBarView.ListenerClickHandler(Highlighter_Click);
            _controlToolBar.ListenerEraseClick += new ContentControlToolBarView.ListenerClickHandler(Erase_Click);
            _controlToolBar.ListenerRevokeClick += new ContentControlToolBarView.ListenerClickHandler(Revoke_Click);
            _controlToolBar.ListenerWallpaperClick += new ContentControlToolBarView.ListenerClickHandler(Wallpaper_Click);
            _controlToolBar.ListenerClearClick += new ContentControlToolBarView.ListenerClickHandler(Clear_Click);
            _controlToolBar.ListenerSnapClick += new ContentControlToolBarView.ListenerClickHandler(Snap_Click);
            _controlToolBar.ListenerExitClick += new ContentControlToolBarView.ListenerClickHandler(Exit_Click);
            _controlToolBar.ListenerToolBarMove += new ContentControlToolBarView.ListenerMoveHnadler(ToolBarMove_Click);

        }

        #endregion

        #region -- Public Methods --
        public void SetSelectedScreen(System.Windows.Forms.Screen screenSel)
        {
            _selScreen = screenSel;
        }

        private void ResolveScreenOrientation()
        {
            DisplayUtil.CheckScreenOrientation(Handle);
            Microsoft.Win32.SystemEvents.DisplaySettingsChanged += new System.EventHandler(DisplaySettingsChanged);
        }

        private void SetWindowPosition()
        {
            if(null == _selScreen || IntPtr.Zero == Handle)
            {
                return;
            }

            System.Drawing.Rectangle bounds = _selScreen.Bounds;

            bool result = Utils.SetWindowPosTopMost(Handle, bounds.X, bounds.Y, bounds.Width, bounds.Height);
            log.InfoFormat("Set WindowPosition->{0},x:{1},y:{2},width:{3},height:{4}", result, bounds.X, bounds.Y, bounds.Width, bounds.Height);
            if (!result)
            {
                log.Error("Set WindowPosition GetLastError: " + Utils.GetLastError());
                log.Error("Set WindowPosition GetLastWin32Error: " + Marshal.GetLastWin32Error());
                result = Utils.SetWindowPosTopMost(Handle, bounds.X, bounds.Y, bounds.Width, bounds.Height);
                log.InfoFormat("Set WindowPosition again->{0},x:{1},y:{2},width:{3},height:{4}", result, bounds.X, bounds.Y, bounds.Width, bounds.Height);
            }

            // sometimes the window size is not correct in high dpi devices, so check the screen size and adjust it
            IntPtr hMonitor = DpiUtil.MonitorFromWindow(Handle, DpiUtil.MONITOR_DEFAULTTONEAREST);
            MONITORINFOEX monitorInfo = DisplayUtil.CreateMonitorInfo();
            int rst = DisplayUtil.GetMonitorInfo(hMonitor, ref monitorInfo);
            if (rst == 0)
            {
                log.Error("failed to get monitor info");
                return;
            }
            int width = monitorInfo.rcMonitor.right - monitorInfo.rcMonitor.left;
            int height = monitorInfo.rcMonitor.bottom - monitorInfo.rcMonitor.top;
            log.InfoFormat("Check if adjust window size, screen size: {0}x{1}", width, height);
            if (bounds.Width < width || bounds.Height < height)
            {
                log.Info("Window size is not less than screen and adjust the window size");
                Utils.SetWindowPosTopMost(Handle, bounds.X, bounds.Y, width, height);
            }
        }

        public void SetWindowPositionExByRect(Rect rect)
        {
            bool result = Utils.SetWindowPosTopMost(Handle, (int)rect.X, (int)rect.Y, (int)rect.Width, (int)rect.Height);
            log.InfoFormat("Set SetWindowPositionExByRect->{0},x:{1},y:{2},width:{3},height:{4}", result, rect.X, rect.Y, rect.Width, rect.Height);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected void Dispose(bool disposing)
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

        #region -- Private Methods --

        private void BackScreenWin_Closed(object sender, EventArgs e)
        {
            _backScreenWin.Closed -= BackScreenWin_Closed;
            if (IsDisposed)
            {
                return;
            }
            _backScreenWin = new BackScreenColorSelectWindow();
            _backScreenWin.Closed += BackScreenWin_Closed;
        }

        private void PenWin_Closed(object sender, EventArgs e)
        {
            _brushWin.Closed -= PenWin_Closed;
            if (IsDisposed)
            {
                return;
            }
            _brushWin = new BrushSelectWindow();
            _brushWin.Closed += PenWin_Closed;
        }

        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void InitCursorInfo()
        {
            log.DebugFormat("Init cursor info(pen and eraser) in content view: {0}", this.GetHashCode());
            this.inkCanvas1.UseCustomCursor = true;

            //StreamResourceInfo sriPen = System.Windows.Application.GetResourceStream(new Uri("pack://application:,,,/Resources/Icons/Content/Pen/Draw/draw.cur"));
            //_selectedPen = new Cursor(sriPen.Stream);
            Stream iconstream = GetCursorFromCur(new Uri("pack://application:,,,/Resources/Icons/Content/Pen/Draw/draw.cur"), 8, 8);
            _selectedPen = new Cursor(iconstream);

            StreamResourceInfo sriEraser = System.Windows.Application.GetResourceStream(new Uri("pack://application:,,,/Resources/Icons/Content/icon_eraser.cur"));
            _eraser = new Cursor(sriEraser.Stream);

            iconstream = GetCursorFromCur(new Uri("pack://application:,,,/Resources/Icons/Content/Pen/Draw/highlighter.cur"), 8, 8);
            _highLighter = new Cursor(iconstream);
        }

        private Stream GetCursorFromCur(Uri uri, byte hotspotx, byte hotspoty)
        {
            StreamResourceInfo sri = Application.GetResourceStream(uri);

            Stream s = sri.Stream;

            byte[] buffer = new byte[s.Length];

            s.Read(buffer, 0, (int)s.Length);

            MemoryStream ms = new MemoryStream();

            buffer[2] = 2; // change to CUR file type
            buffer[10] = hotspotx;
            buffer[12] = hotspoty;

            ms.Write(buffer, 0, (int)s.Length);

            ms.Position = 0;

            return ms;
        }

        private void InitToolbar()
        {
            log.DebugFormat("ContentControlView InitToolbar: {0}", _controlToolBar.GetHashCode() );
            _controlToolBar.StartCursor.SetBinding(Button.VisibilityProperty, new Binding("StartCursorVisibility") { Source = this });
            _controlToolBar.StopCursor.SetBinding(Button.VisibilityProperty, new Binding("StopCursorVisibility") { Source = this });
            _controlToolBar.PenSetting.SetBinding(Button.VisibilityProperty, new Binding("StopCursorVisibility") { Source = this });
            _controlToolBar.Highlighter.SetBinding(Button.VisibilityProperty, new Binding("StopCursorVisibility") { Source = this });
            _controlToolBar.Erase.SetBinding(Button.VisibilityProperty, new Binding("StopCursorVisibility") { Source = this });
            _controlToolBar.Revoke.SetBinding(Button.VisibilityProperty, new Binding("StopCursorVisibility") { Source = this });
            _controlToolBar.Wallpaper.SetBinding(Button.VisibilityProperty, new Binding("StopCursorVisibility") { Source = this });
            _controlToolBar.Clear.SetBinding(Button.VisibilityProperty, new Binding("StopCursorVisibility") { Source = this });
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            log.Debug("ContentControlView Exit_Click");
            HidePopupWindow(true, true, true);
            MessageBoxConfirm confirm = new MessageBoxConfirm(this);
            confirm.Owner = this;
            confirm.isResize(false);
            confirm.SetTitleAndMsg(LanguageUtil.Instance.GetValueByKey("PROMPT"), LanguageUtil.Instance.GetValueByKey("ARE_YOU_SURE_TO_STOP_SHARING"));
            confirm.ConfirmEvent += new EventHandler(delegate (object sender1, EventArgs e1) {
                CallController.Instance.StopContent();

                log.Info("Click exit button, close content view");
                confirm.Close();
            });
            confirm.Width *= _ratio;
            confirm.Height *= _ratio;
            confirm.Show();
        }

        private void EnableShareSound_Click(object sender, RoutedEventArgs e)
        {
            log.Info("EnableShareSound_Click");
            CallController.Instance.EnableContentAudio(true);
            _controlToolBar.RefreshShareSoundBtnStatus();
        }

        private void DisableShareSound_Click(object sender, RoutedEventArgs e)
        {
            log.Info("DisableShareSound_Click");
            CallController.Instance.EnableContentAudio(false);
            _controlToolBar.RefreshShareSoundBtnStatus();
        }

        private void StartCursor_Click(object sender, RoutedEventArgs e)
        {
            log.Debug("ContentControlView StartCursor_Click");
            InkCanvasCursorStatus = InkCanvasCursorEnum.PEN;
            string backcolor = Utils.getBackColorSelection();
            int backScreenType = backcolor == "" ? 2 : Convert.ToInt32(backcolor);
            SetBackGroundByType(backScreenType);
            log.DebugFormat("Start cursor click, back screen color: {0}, value: {1}", backcolor, backScreenType);
            SelectedPenAndColor();
        }

        private void StopCursor_Click(object sender, RoutedEventArgs e)
        {
            log.DebugFormat("Stop cursor click in content view: {0}", this.GetHashCode());
            HidePopupWindow(true, true, true);
            if (this.Background != Brushes.White && this.Background != Brushes.Black)
            {
                this.Background = Brushes.Transparent;
            }

            InkCanvasCursorStatus = InkCanvasCursorEnum.DEFAULT;
            this.inkCanvas1.EditingMode = InkCanvasEditingMode.None;
        }

        #region -- pen --
        private void PenSetting_Click(object sender, RoutedEventArgs e)
        {
            log.DebugFormat("click pen window in content view: {0}", _selectedPenAsTarget);
            HidePopupWindow(false, true, true);
            //set to default highlighter
            _selectedHighlighterAsTarget = ButtonStatus.None;
            ClickButtonChangeBackImage(0, 2);

            if (!string.IsNullOrEmpty(Utils.getPenTypeSelection()))
                _selectedSize = Convert.ToInt16(Utils.getPenTypeSelection());
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
            //init back ground
            log.DebugFormat("show pen window in content view");
            string backcolor = Utils.getBackColorSelection();
            int backScreenType = backcolor == "" ? 2 : Convert.ToInt32(backcolor);
            SetBackGroundByType(backScreenType);
            //init selected color
            _brushWin.initColor(_selectedColor);
            _brushWin.initSize(_selectedSize);

            _brushWin.Owner = this;
            _brushWin.Topmost = true;
            try
            {
                double screenLogicHeight = this.Height;
                double screenLogicWidth = this.Width;
                double screenLogicX = this.Left;
                double screenLogicY = this.Top;

                double toolbarLogicY = _controlToolBar.Top;
                double toolbarLogicX = _controlToolBar.Left;

                if (_controlToolBar.ToolbarPanel.Orientation == Orientation.Horizontal)
                {
                    if (toolbarLogicY > (screenLogicY + screenLogicHeight * 0.6))
                    {
                        _brushWin.Top = toolbarLogicY - 195 * _ratio;
                        _brushWin.Left = toolbarLogicX - 10 * _ratio;
                    }
                    else
                    {
                        _brushWin.Top = toolbarLogicY + 65 * _ratio;
                        _brushWin.Left = toolbarLogicX - 10 * _ratio;
                    }
                }
                else
                {
                    if (toolbarLogicX > (screenLogicX + screenLogicWidth * 0.6))
                    {
                        _brushWin.Top = toolbarLogicY + 50 * _ratio;
                        _brushWin.Left = toolbarLogicX - 195 * _ratio;
                    }
                    else
                    {
                        _brushWin.Top = toolbarLogicY + 50 * _ratio;
                        _brushWin.Left = toolbarLogicX + 65 * _ratio;
                    }
                }
                _brushWin.Show();
            }
            catch (Exception ex)
            {
                log.DebugFormat("Exception for show pen selection window, message:", ex.Message);
            }
        }

        private void SelectedPenAndColor()
        {
            SetPenAndColor(Utils.getPenTypeSelection(), _selectedColor);
            ClickButtonChangeBackImage(1, 1);
            _selectedPenAsTarget = ButtonStatus.Selected;
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
                    ClickButtonChangeBackImage(1, 2);
                    ShowHighlighterWindow();
                    break;
                case ButtonStatus.Show:
                    _selectedHighlighterAsTarget = ButtonStatus.Hide;
                    ClickButtonChangeBackImage(1, 2);
                    if(_highlighterWin.IsVisible)
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
            log.DebugFormat("show Highlighter window in content view");
            //init selected color
            _highlighterWin.initColor(_selectedHighligherColor);
            _highlighterWin.initSize(_selectedHighlighterSize);

            _highlighterWin.Owner = this;
            _highlighterWin.Topmost = true;
            try
            {
                double screenLogicHeight = this.Height;
                double screenLogicWidth = this.Width;
                double screenLogicX = this.Left;
                double screenLogicY = this.Top;

                double toolbarLogicY = _controlToolBar.Top;
                double toolbarLogicX = _controlToolBar.Left;

                if (_controlToolBar.ToolbarPanel.Orientation == Orientation.Horizontal)
                {
                    if (toolbarLogicY > (screenLogicY + screenLogicHeight * 0.6))
                    {
                        _highlighterWin.Top = toolbarLogicY - 195 * _ratio;
                        _highlighterWin.Left = toolbarLogicX + 40 * _ratio;
                    }
                    else
                    {
                        _highlighterWin.Top = toolbarLogicY + 65 * _ratio;
                        _highlighterWin.Left = toolbarLogicX + 40 * _ratio;
                    }
                }
                else
                {
                    if (toolbarLogicX > (screenLogicX + screenLogicWidth * 0.6))
                    {
                        _highlighterWin.Top = toolbarLogicY + 110 * _ratio;
                        _highlighterWin.Left = toolbarLogicX - 195 * _ratio;
                    }
                    else
                    {
                        _highlighterWin.Top = toolbarLogicY + 110 * _ratio;
                        _highlighterWin.Left = toolbarLogicX + 65 * _ratio;
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
            SetHighlighterPenAndColor(_selectedHighlighterSize, _selectedHighligherColor);
            ClickButtonChangeBackImage(1, 2);
            _selectedHighlighterAsTarget = ButtonStatus.Selected;
        }
        #endregion

        private void Revoke_Click(object sender, RoutedEventArgs e)
        {
            log.DebugFormat("Show revoke boolbar in content view: {0}", this.GetHashCode());
            HidePopupWindow(true, true, true);

            if (_cmdStack != null)
            {
                if (_cmdStack.CanUndo)
                {
                    _cmdStack.Undo();
                }
            }
        }

        private void Erase_Click(object sender, RoutedEventArgs e)
        {
            log.DebugFormat("Show reaser icon in content view: {0}", this.GetHashCode());
            HidePopupWindow(true, true, true);
            InkCanvasCursorStatus = InkCanvasCursorEnum.ERASER;
            this.inkCanvas1.EditingMode = InkCanvasEditingMode.EraseByPoint;
            this.inkCanvas1.EraserShape = new EllipseStylusShape(40, 40);

            //set to default pen and highlighter
            _selectedPenAsTarget = ButtonStatus.None;
            ClickButtonChangeBackImage(0, 1);
            _selectedHighlighterAsTarget = ButtonStatus.None;
            ClickButtonChangeBackImage(0, 2);
        }

        private void Wallpaper_Click(object sender, RoutedEventArgs e)
        {
            log.DebugFormat("Show Wallpaper window in content view: {0}", this.GetHashCode());
            HidePopupWindow(true, false, true);
            //ShowHideGrayBackground(true);

            _backScreenWin.InitData();
            _backScreenWin.Owner = this;
            _backScreenWin.Topmost = true;
            try
            {
                double screenLogicHeight = this.Height;
                double screenLogicWidth = this.Width;
                double screenLogicX = this.Left;
                double screenLogicY = this.Top;

                double toolbarLogicY = _controlToolBar.Top;
                double toolbarLogicX = _controlToolBar.Left;

                if (_controlToolBar.ToolbarPanel.Orientation == Orientation.Horizontal)
                {
                    if (toolbarLogicY > (screenLogicY + screenLogicHeight * 0.6))
                    {
                        _backScreenWin.Top = toolbarLogicY - 195 * _ratio;
                        _backScreenWin.Left = toolbarLogicX + 230 * _ratio;
                    }
                    else
                    {
                        _backScreenWin.Top = toolbarLogicY + 65 * _ratio;
                        _backScreenWin.Left = toolbarLogicX + 230 * _ratio;
                    }
                }
                else
                {
                    if (toolbarLogicX > (screenLogicX + screenLogicWidth * 0.6))
                    {
                        _backScreenWin.Top = toolbarLogicY + 280 * _ratio;
                        _backScreenWin.Left = toolbarLogicX - 185 * _ratio;
                    }
                    else
                    {
                        _backScreenWin.Top = toolbarLogicY + 280 * _ratio;
                        _backScreenWin.Left = toolbarLogicX + 65 * _ratio;
                    }
                }

                _backScreenWin.Show();
            }
            catch (Exception ex)
            {
                log.DebugFormat("Exception for show back screen selection window, message:", ex.Message);
            }
            //_backScreenWin = null;
            //ShowHideGrayBackground(false);


            //int backScreenType = Convert.ToInt32(Utils.getBackColorSelection());
            //SetBackGroundByType(backScreenType);
            //SetPenAndColor(Utils.getPenTypeSelection(), Utils.getOpacityTypeSelection());
        }

        private void Clear_Click(object sender, RoutedEventArgs e)
        {
            log.DebugFormat("Clean pen strokes collection in content view: {0}", this.GetHashCode());
            HidePopupWindow(true, true, true);
            this.inkCanvas1.Strokes.Clear();
        }

        private void Snap_Click(object sender, RoutedEventArgs e)
        {
            log.Debug("ContentControlView Snap_Click");
            HidePopupWindow(true, true, true);
            string savePicPath = Utils.GenerateScreenPictureName();
            int result = MonitorCapture.SaveMonitorPic(_selScreen, savePicPath);
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
            alert.Width *= _ratio;
            alert.Height *= _ratio;
            alert.Show();
        }

        private void ToolBarMove_Click(object sender, MouseButtonEventArgs e)
        {
            double screenLogicX = this.Left;
            double screenLogicY = this.Top;
            double screenLogicWidth = this.Width;
            double screenLogicHeight = this.Height;

            double controlToolBarHeight = _controlToolBar.ActualHeight;
            double controlToolBarWidth = _controlToolBar.ActualWidth;
            //double maxControlToolBarLength = controlToolBarHeight > controlToolBarWidth ? controlToolBarHeight : controlToolBarWidth;
            double maxControlToolBarLength = 600 * _ratio;
            double toolbarLogicY = _controlToolBar.Top;
            double toolbarLogicX = _controlToolBar.Left;

            if ((screenLogicWidth + screenLogicX) < (toolbarLogicX + maxControlToolBarLength))
            {
                _controlToolBar.Left = screenLogicX + screenLogicWidth - 80;
                if (toolbarLogicY < screenLogicY)
                {
                    _controlToolBar.Top = screenLogicY + 10;
                }
                else if (toolbarLogicY + maxControlToolBarLength > screenLogicHeight)
                {
                    _controlToolBar.Top = Math.Abs(screenLogicY + screenLogicHeight - maxControlToolBarLength);
                }
                //right
                _controlToolBar.ToolbarPanel.Orientation = Orientation.Vertical;
                Thickness thick = new Thickness(0, 5, 0, 5);
                _controlToolBar.ToolbarPanel.Margin = thick;
                _controlToolBar.Width = 70 * _ratio;
                _controlToolBar.Height = 650 * _ratio;
                _controlToolBar.HorizontalAlignment = HorizontalAlignment.Right;
                _controlToolBar.VerticalAlignment = VerticalAlignment.Center;

            }
            else if ((toolbarLogicX - screenLogicX) < 1)
            {
                _controlToolBar.Left = screenLogicX + 10;
                if (toolbarLogicY < screenLogicY)
                {
                    _controlToolBar.Top = screenLogicY + 10;
                }
                else if (toolbarLogicY + maxControlToolBarLength > screenLogicHeight)
                {
                    _controlToolBar.Top = Math.Abs(screenLogicY + screenLogicHeight - maxControlToolBarLength);
                }
                //left
                _controlToolBar.ToolbarPanel.Orientation = Orientation.Vertical;
                Thickness thick = new Thickness(0, 5, 0, 5);
                _controlToolBar.ToolbarPanel.Margin = thick;
                _controlToolBar.Width = 70 * _ratio;
                _controlToolBar.Height = 650 * _ratio;
                _controlToolBar.HorizontalAlignment = HorizontalAlignment.Left;
                _controlToolBar.VerticalAlignment = VerticalAlignment.Center;
            }
            else if (toolbarLogicY < screenLogicY + 10)
            {
                _controlToolBar.Top = toolbarLogicY + 1;
                if (toolbarLogicX < screenLogicX + 10)
                {
                    _controlToolBar.Left = screenLogicX + 10;
                }
                else if (toolbarLogicX + maxControlToolBarLength > screenLogicX + screenLogicWidth)
                {
                    _controlToolBar.Left = Math.Abs(screenLogicX + screenLogicWidth - maxControlToolBarLength);
                }
                //top
                _controlToolBar.ToolbarPanel.Orientation = Orientation.Horizontal;
                Thickness thick = new Thickness(5, 0, 5, 0);
                _controlToolBar.ToolbarPanel.Margin = thick;
                _controlToolBar.Width = 520 * _ratio;
                _controlToolBar.Height = 70 * _ratio;
                _controlToolBar.HorizontalAlignment = HorizontalAlignment.Center;
                _controlToolBar.VerticalAlignment = VerticalAlignment.Top;
            }
            else if ((screenLogicY + screenLogicHeight) < (toolbarLogicY + maxControlToolBarLength))
            {
                _controlToolBar.Top = screenLogicY + screenLogicHeight - 80;
                if (toolbarLogicX < screenLogicX + 10)
                {
                    _controlToolBar.Left = screenLogicX + 10;
                }
                else if (toolbarLogicX + maxControlToolBarLength > screenLogicX + screenLogicWidth)
                {
                    _controlToolBar.Left = Math.Abs(screenLogicX + screenLogicWidth - maxControlToolBarLength);
                }
                //button
                _controlToolBar.ToolbarPanel.Orientation = Orientation.Horizontal;
                Thickness thick = new Thickness(5, 0, 5, 0);
                _controlToolBar.ToolbarPanel.Margin = thick;
                _controlToolBar.Width = 520 * _ratio;
                _controlToolBar.Height = 70 * _ratio;
                _controlToolBar.HorizontalAlignment = HorizontalAlignment.Center;
                _controlToolBar.VerticalAlignment = VerticalAlignment.Bottom;

            }

            log.InfoFormat("_controlToolBar position, left: {0}, top: {1}, width: {2}, height: {3}", _controlToolBar.Left, _controlToolBar.Top, _controlToolBar.Width, _controlToolBar.Height);
        }
        
        private void SetInkCanvasTransparent()
        {
            this.inkCanvas1.Opacity = 1d;
            object color = ColorConverter.ConvertFromString("#01FFFFFF");
            this.Background = _approximateTransparentBrush;
        }

        private void HidePopupWindow(Boolean brush, Boolean backScreen, Boolean highLighter)
        {
            if (brush && _brushWin.IsVisible)
            {
                _brushWin.Hide();
            }
            if (backScreen && _backScreenWin.IsVisible)
            {
                _backScreenWin.Hide();
            }
            if(highLighter && _highlighterWin.IsVisible)
            {
                _highlighterWin.Hide();
            }
        }

        private void Window_Loaded(Object s, EventArgs e)
        {
            Utils.SetSoftwareRender(this);
        }

        public void SetProperPosition()
        {
            SetWindowPosition();
            ResolveScreenOrientation();
            SetToolBarPositionLogical();
        }

        public void SetToolBarPositionLogical()
        {
            log.Debug("ContentControlView SetToolBarPositionLogical");
            this.Topmost = true;
            double inkCanvasWidth = this.Width;
            //_ratio = (float)inkCanvasWidth / 1920;
            //if (_ratio <= 0.5)
            //{
            //    _ratio = 0.6f;
            //}
            //else if (_ratio <= 0.6)
            //{
            //    _ratio = 0.7f;
            //}
            //else if (_ratio <= 0.7)
            //{
            //    _ratio = 0.8f;
            //}
            //else if (_ratio < 0.8)
            //{
            //    _ratio = 0.9f;
            //}
            _controlToolBar.Width = 520 * _ratio;
            _controlToolBar.Height = 70 * _ratio;
            double toolBarWidth = _controlToolBar.Width;
            _controlToolBar.Left = this.Left + (inkCanvasWidth - toolBarWidth) / 2;
            _controlToolBar.Top = this.Top + 10;
            _controlToolBar.Owner = this;
            _controlToolBar.Topmost = true;
            _controlToolBar.Show();
            log.DebugFormat("toolbar : " + _controlToolBar.GetHashCode());

            _backScreenWin.Width *= _ratio;
            _backScreenWin.Height *= _ratio;

            _brushWin.Width *= _ratio;
            _brushWin.Height *= _ratio;

            _highlighterWin.Width *= _ratio;
            _highlighterWin.Height *= _ratio;
        }

        public void SetToolBarPositionLogicalEx()
        {
            log.Debug("ContentControlView SetToolBarPositionLogicalEx");
            this.Topmost = true;
            double inkCanvasWidth = this.Width;
            _ratio = (float)inkCanvasWidth / 1920;
            if (_ratio < 0.8)
            {
                _ratio = 0.9f;
            }
            _controlToolBar.Width = 520 * _ratio;
            _controlToolBar.Height = 70 * _ratio;
            double toolBarWidth = _controlToolBar.Width;
            _controlToolBar.Left = this.Left + (inkCanvasWidth - toolBarWidth) / 2;
            _controlToolBar.Top = this.Top + 10;
            _controlToolBar.Topmost = true;

            _backScreenWin.Width *= _ratio;
            _backScreenWin.Height *= _ratio;

            _brushWin.Width *= _ratio;
            _brushWin.Height *= _ratio;
        }

        private void SetBackgroundDark(bool needDack)
        {
            if (needDack)
            {
                this.inkCanvas1.Background = Brushes.Black;
                this.inkCanvas1.Opacity = 0.5;
            }
            else
            {
                this.inkCanvas1.Opacity = 1;
            }
        }

        //private void ShowHideGrayBackground(bool isShow)
        //{
        //    if(isShow)
        //    {
        //        //grayBackground.Visibility = Visibility.Visible;
        //        //this.DetailedToolbar.Background = Brushes.Gray;
        //        //this.DetailedToolbar.Opacity = 0.5d;
        //    }
        //    else
        //    {
        //        //grayBackground.Visibility = Visibility.Collapsed;
        //        //this.DetailedToolbar.Background = Brushes.White;
        //        //this.DetailedToolbar.Opacity = 1d;
        //    }
        //}

        private void ChangeSelectedBrushColor(int selectedColor)
        {
            this._selectedColor = selectedColor;
        }

        private void SetPenAndColor(string penTypeSel, int colorTypeSel)
        {
            log.DebugFormat("Change pen style size: {0}, color: {1} in content view: {2}", penTypeSel, _selectedColor, this.GetHashCode());
            ChangeSelectedBrushColor(colorTypeSel);
            int penType = 1;
            if (!string.IsNullOrEmpty(penTypeSel))
                penType = Convert.ToInt16(penTypeSel);

            if (penType > 0)
            {
                _inkDA.Width = (penType == 1 ? 2 : penType == 2 ? 5 : penType == 3 ? 10 : penType == 4 ? 15 : 2);
                _inkDA.Height = (penType == 1 ? 2 : penType == 2 ? 5 : penType == 3 ? 10 : penType == 4 ? 15 : 2);
            }

            switch (_selectedColor)
            {
                case (Int16)PenColor.Black:
                    {
                        //black
                        _inkDA.Color = (Color)System.Windows.Media.ColorConverter.ConvertFromString("#000000");
                    }
                    break;
                case (Int16)PenColor.White:
                    {
                        //white
                        _inkDA.Color = (Color)System.Windows.Media.ColorConverter.ConvertFromString("#ffffff");
                    }
                    break;
                case (Int16)PenColor.Blue:
                    {
                        //blue
                        _inkDA.Color = (Color)System.Windows.Media.ColorConverter.ConvertFromString("#0000ff");
                    }
                    break;
                case (Int16)PenColor.Green:
                    {
                        //green
                        _inkDA.Color = (Color)System.Windows.Media.ColorConverter.ConvertFromString("#00f900");
                    }
                    break;
                case (Int16)PenColor.Yellow:
                    {
                        //yellow
                        _inkDA.Color = (Color)System.Windows.Media.ColorConverter.ConvertFromString("#fffc00");
                    }
                    break;
                case (Int16)PenColor.Orange:
                    {
                        //orange
                        _inkDA.Color = (Color)System.Windows.Media.ColorConverter.ConvertFromString("#ff8517");
                    }
                    break;
                case (Int16)PenColor.Red:
                    {
                        //red
                        _inkDA.Color = (Color)System.Windows.Media.ColorConverter.ConvertFromString("#ff0000");
                    }
                    break;
                case (Int16)PenColor.Purple:
                    {
                        //purple
                        _inkDA.Color = (Color)System.Windows.Media.ColorConverter.ConvertFromString("#ff40ff");
                    }
                    break;
                default:
                    {
                        //default
                        _inkDA.Color = (Color)System.Windows.Media.ColorConverter.ConvertFromString("#000000");
                    }
                    break;
            }

            _isOpenHighlighterPen = false;
            _inkDA.IsHighlighter = false;
            _inkDA.StylusTip = StylusTip.Ellipse;
            _inkDA.IgnorePressure = false;
            this.inkCanvas1.DefaultDrawingAttributes = _inkDA;
            this.inkCanvas1.EditingMode = InkCanvasEditingMode.Ink;
            this.inkCanvas1.Cursor = _selectedPen;

            string backcolor = Utils.getBackColorSelection();
            int backScreenType = backcolor == "" ? 2 : Convert.ToInt32(backcolor);
            SetBackGroundByType(backScreenType);
        }

        private void SetHighlighterPenAndColor(int penTypeSel, int colorTypeSel)
        {
            log.DebugFormat("Change highlighter pen style size: {0}, color: {1} in content view: {2}", penTypeSel, _selectedColor, this.GetHashCode());
            _selectedHighligherColor = colorTypeSel;
            _selectedHighlighterSize = penTypeSel;
            if (penTypeSel > 0)
            {
                _inkDA.Width = (penTypeSel == 1 ? 5 : penTypeSel == 2 ? 10 : penTypeSel == 3 ? 10 : penTypeSel == 4 ? 10 : 5);
                _inkDA.Height = (penTypeSel == 1 ? 5 : penTypeSel == 2 ? 10 : penTypeSel == 3 ? 20 : penTypeSel == 4 ? 30 : 5);
            }

            switch (_selectedHighligherColor)
            {
                case (Int16)PenColor.Yellow:
                    {
                        //yellow
                        _inkDA.Color = (Color)System.Windows.Media.ColorConverter.ConvertFromString("#fffc00");
                    }
                    break;
                case (Int16)PenColor.Green:
                    {
                        //green
                        _inkDA.Color = (Color)System.Windows.Media.ColorConverter.ConvertFromString("#00f900");
                    }
                    break;
                case (Int16)PenColor.Lake:
                    {
                        //lake
                        _inkDA.Color = (Color)System.Windows.Media.ColorConverter.ConvertFromString("#00fdff");
                    }
                    break;
                case (Int16)PenColor.Purple:
                    {
                        //purple
                        _inkDA.Color = (Color)System.Windows.Media.ColorConverter.ConvertFromString("#ff40ff");
                    }
                    break;
                case (Int16)PenColor.Orange:
                    {
                        //Orange
                        _inkDA.Color = (Color)System.Windows.Media.ColorConverter.ConvertFromString("#ff8517");
                    }
                    break;
                case (Int16)PenColor.Light_Green:
                    {
                        //Light_Green
                        _inkDA.Color = (Color)System.Windows.Media.ColorConverter.ConvertFromString("#aae464");
                    }
                    break;
                case (Int16)PenColor.Light_Blue:
                    {
                        //Light_Blue
                        _inkDA.Color = (Color)System.Windows.Media.ColorConverter.ConvertFromString("#a9d8ff");
                    }
                    break;
                case (Int16)PenColor.Pink:
                    {
                        //Pink
                        _inkDA.Color = (Color)System.Windows.Media.ColorConverter.ConvertFromString("#ffacd5");
                    }
                    break;
                default:
                    {
                        //default
                        _inkDA.Color = (Color)System.Windows.Media.ColorConverter.ConvertFromString("#fffc00");
                    }
                    break;
            }

            _isOpenHighlighterPen = true;
            _inkDA.IsHighlighter = true;
            _inkDA.StylusTip = StylusTip.Rectangle;
            _inkDA.IgnorePressure = true;
            this.inkCanvas1.DefaultDrawingAttributes = _inkDA;
            this.inkCanvas1.EditingMode = InkCanvasEditingMode.Ink;
            this.inkCanvas1.Cursor = _highLighter;
        }

        private void SetBackGroundByType(int backScreenType)
        {
            log.DebugFormat("Change back ground style: {0} in content view: {1}", backScreenType, this.GetHashCode());
            switch (backScreenType)
            {
                case (Int16)BackScreenColorEnum.Black:
                    {
                        this.Background = Brushes.Black;
                        this.Opacity = 1;
                    }
                    break;
                case (Int16)BackScreenColorEnum.White:
                    {
                        this.Background = Brushes.White;
                        this.Opacity = 1;
                    }
                    break;
                case (Int16)BackScreenColorEnum.Transparency:
                default:
                    {
                        //this.inkCanvas1.EditingMode = InkCanvasEditingMode.Ink;
                        SetInkCanvasTransparent();
                    }
                    break;
            }
        }
       
        private void Window_MouseButtonDown(object sender, MouseButtonEventArgs e)
        {
            log.Debug("ContentControlView Window_MouseButtonDown");
            if (e.LeftButton == MouseButtonState.Pressed && _controlToolBar.StopCursor.IsVisible)
            {
                if (_brushWin.IsVisible)
                {
                    log.DebugFormat("Hide brush screen in content view: {0}", this.GetHashCode());
                    _brushWin.Hide();
                }
                if (_backScreenWin.IsVisible)
                {
                    log.DebugFormat("Hide background screen in content view: {0}", this.GetHashCode());
                    _backScreenWin.Hide();
                }
                if(_highlighterWin.IsVisible)
                {
                    log.DebugFormat("Hide highlighter screen in content view: {0}", this.GetHashCode());
                    _highlighterWin.Hide();
                }

                if (this.inkCanvas1.EditingMode == InkCanvasEditingMode.EraseByPoint)
                {
                    //do nothing for now
                }
                else if(_isOpenHighlighterPen)
                {
                    SelectedHighlightrAndColor();
                }
                else
                {
                    SelectedPenAndColor();
                }
            }
        }

        private void DisplaySettingsChanged(object sender, EventArgs e)
        {
            //when screen disconnected, new WindowInteropHelper(this).Handle return 0;
            log.Debug("ContentControlView DisplaySettingsChanged");
            DisplayUtil.CheckScreenOrientation(Handle);
            Rect rect = DisplayUtil.CheckScreenRect(Handle);
            if (rect.Height > 0 && rect.Width > 0)
            {
                SetWindowPositionExByRect(rect);
            }
            else
            {
                SetWindowPosition();
            }

            //SetToolBarPositionLogical();
            SetToolBarPositionLogicalEx();
        }

        private void ClickButtonChangeBackImage(int taps, int penType)
        {
            //selected
            if (taps == 1)
            {
                if (penType == 1)
                {
                    _controlToolBar.PenSetting.TextForeground = Utils.SelectedForeGround;
                    if(_controlToolBar.ToolbarPanel.Orientation == Orientation.Horizontal)
                    {
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
                    else
                    {
                        switch (_selectedColor)
                        {
                            case (Int16)PenColor.Black:
                                _controlToolBar.PenSetting.NormalImage = new System.Windows.Media.Imaging.BitmapImage(new Uri("pack://application:,,,/Resources/Icons/Content/Pen/Type/black_left_click.png"));
                                _controlToolBar.PenSetting.MouseOverImage = new System.Windows.Media.Imaging.BitmapImage(new Uri("pack://application:,,,/Resources/Icons/Content/Pen/Type/black_left_click.png"));
                                break;
                            case (Int16)PenColor.White:
                                _controlToolBar.PenSetting.NormalImage = new System.Windows.Media.Imaging.BitmapImage(new Uri("pack://application:,,,/Resources/Icons/Content/Pen/Type/white_left_click.png"));
                                _controlToolBar.PenSetting.MouseOverImage = new System.Windows.Media.Imaging.BitmapImage(new Uri("pack://application:,,,/Resources/Icons/Content/Pen/Type/white_left_click.png"));
                                break;
                            case (Int16)PenColor.Blue:
                                _controlToolBar.PenSetting.NormalImage = new System.Windows.Media.Imaging.BitmapImage(new Uri("pack://application:,,,/Resources/Icons/Content/Pen/Type/blue_left_click.png"));
                                _controlToolBar.PenSetting.MouseOverImage = new System.Windows.Media.Imaging.BitmapImage(new Uri("pack://application:,,,/Resources/Icons/Content/Pen/Type/blue_left_click.png"));
                                break;
                            case (Int16)PenColor.Green:
                                _controlToolBar.PenSetting.NormalImage = new System.Windows.Media.Imaging.BitmapImage(new Uri("pack://application:,,,/Resources/Icons/Content/Pen/Type/green_left_click.png"));
                                _controlToolBar.PenSetting.MouseOverImage = new System.Windows.Media.Imaging.BitmapImage(new Uri("pack://application:,,,/Resources/Icons/Content/Pen/Type/green_left_click.png"));
                                break;
                            case (Int16)PenColor.Yellow:
                                _controlToolBar.PenSetting.NormalImage = new System.Windows.Media.Imaging.BitmapImage(new Uri("pack://application:,,,/Resources/Icons/Content/Pen/Type/yellow_left_click.png"));
                                _controlToolBar.PenSetting.MouseOverImage = new System.Windows.Media.Imaging.BitmapImage(new Uri("pack://application:,,,/Resources/Icons/Content/Pen/Type/yellow_left_click.png"));
                                break;
                            case (Int16)PenColor.Orange:
                                _controlToolBar.PenSetting.NormalImage = new System.Windows.Media.Imaging.BitmapImage(new Uri("pack://application:,,,/Resources/Icons/Content/Pen/Type/orange_left_click.png"));
                                _controlToolBar.PenSetting.MouseOverImage = new System.Windows.Media.Imaging.BitmapImage(new Uri("pack://application:,,,/Resources/Icons/Content/Pen/Type/orange_left_click.png"));
                                break;
                            case (Int16)PenColor.Red:
                                _controlToolBar.PenSetting.NormalImage = new System.Windows.Media.Imaging.BitmapImage(new Uri("pack://application:,,,/Resources/Icons/Content/Pen/Type/red_left_click.png"));
                                _controlToolBar.PenSetting.MouseOverImage = new System.Windows.Media.Imaging.BitmapImage(new Uri("pack://application:,,,/Resources/Icons/Content/Pen/Type/red_left_click.png"));
                                break;
                            case (Int16)PenColor.Purple:
                                _controlToolBar.PenSetting.NormalImage = new System.Windows.Media.Imaging.BitmapImage(new Uri("pack://application:,,,/Resources/Icons/Content/Pen/Type/purple_left_click.png"));
                                _controlToolBar.PenSetting.MouseOverImage = new System.Windows.Media.Imaging.BitmapImage(new Uri("pack://application:,,,/Resources/Icons/Content/Pen/Type/purple_left_click.png"));
                                break;
                            default:
                                _controlToolBar.PenSetting.NormalImage = new System.Windows.Media.Imaging.BitmapImage(new Uri("pack://application:,,,/Resources/Icons/Content/Pen/Type/black_left_click.png"));
                                _controlToolBar.PenSetting.MouseOverImage = new System.Windows.Media.Imaging.BitmapImage(new Uri("pack://application:,,,/Resources/Icons/Content/Pen/Type/black_left_click.png"));
                                break;
                        }
                    }
                }
                else if (penType == 2)
                {
                    _controlToolBar.Highlighter.TextForeground = Utils.SelectedForeGround;
                    if (_controlToolBar.ToolbarPanel.Orientation == Orientation.Horizontal)
                    {
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
                    else
                    {
                        switch (_selectedHighligherColor)
                        {
                            case (Int16)PenColor.Yellow:
                                _controlToolBar.Highlighter.NormalImage = new System.Windows.Media.Imaging.BitmapImage(new Uri("pack://application:,,,/Resources/Icons/Content/Pen/Type/h_yellow_left_click.png"));
                                _controlToolBar.Highlighter.MouseOverImage = new System.Windows.Media.Imaging.BitmapImage(new Uri("pack://application:,,,/Resources/Icons/Content/Pen/Type/h_yellow_left_click.png"));
                                break;
                            case (Int16)PenColor.Purple:
                                _controlToolBar.Highlighter.NormalImage = new System.Windows.Media.Imaging.BitmapImage(new Uri("pack://application:,,,/Resources/Icons/Content/Pen/Type/h_purple_left_click.png"));
                                _controlToolBar.Highlighter.MouseOverImage = new System.Windows.Media.Imaging.BitmapImage(new Uri("pack://application:,,,/Resources/Icons/Content/Pen/Type/h_purple_left_click.png"));
                                break;
                            case (Int16)PenColor.Lake:
                                _controlToolBar.Highlighter.NormalImage = new System.Windows.Media.Imaging.BitmapImage(new Uri("pack://application:,,,/Resources/Icons/Content/Pen/Type/h_lake_left_click.png"));
                                _controlToolBar.Highlighter.MouseOverImage = new System.Windows.Media.Imaging.BitmapImage(new Uri("pack://application:,,,/Resources/Icons/Content/Pen/Type/h_lake_left_click.png"));
                                break;
                            case (Int16)PenColor.Green:
                                _controlToolBar.Highlighter.NormalImage = new System.Windows.Media.Imaging.BitmapImage(new Uri("pack://application:,,,/Resources/Icons/Content/Pen/Type/h_green_left_click.png"));
                                _controlToolBar.Highlighter.MouseOverImage = new System.Windows.Media.Imaging.BitmapImage(new Uri("pack://application:,,,/Resources/Icons/Content/Pen/Type/h_green_left_click.png"));
                                break;
                            case (Int16)PenColor.Orange:
                                _controlToolBar.Highlighter.NormalImage = new System.Windows.Media.Imaging.BitmapImage(new Uri("pack://application:,,,/Resources/Icons/Content/Pen/Type/h_orange_left_click.png"));
                                _controlToolBar.Highlighter.MouseOverImage = new System.Windows.Media.Imaging.BitmapImage(new Uri("pack://application:,,,/Resources/Icons/Content/Pen/Type/h_orange_left_click.png"));
                                break;
                            case (Int16)PenColor.Light_Green:
                                _controlToolBar.Highlighter.NormalImage = new System.Windows.Media.Imaging.BitmapImage(new Uri("pack://application:,,,/Resources/Icons/Content/Pen/Type/h_light-green_left_click.png"));
                                _controlToolBar.Highlighter.MouseOverImage = new System.Windows.Media.Imaging.BitmapImage(new Uri("pack://application:,,,/Resources/Icons/Content/Pen/Type/h_light-green_left_click.png"));
                                break;
                            case (Int16)PenColor.Light_Blue:
                                _controlToolBar.Highlighter.NormalImage = new System.Windows.Media.Imaging.BitmapImage(new Uri("pack://application:,,,/Resources/Icons/Content/Pen/Type/h_light-blue_left_click.png"));
                                _controlToolBar.Highlighter.MouseOverImage = new System.Windows.Media.Imaging.BitmapImage(new Uri("pack://application:,,,/Resources/Icons/Content/Pen/Type/h_light-blue_left_click.png"));
                                break;
                            case (Int16)PenColor.Pink:
                                _controlToolBar.Highlighter.NormalImage = new System.Windows.Media.Imaging.BitmapImage(new Uri("pack://application:,,,/Resources/Icons/Content/Pen/Type/h_pink_left_click.png"));
                                _controlToolBar.Highlighter.MouseOverImage = new System.Windows.Media.Imaging.BitmapImage(new Uri("pack://application:,,,/Resources/Icons/Content/Pen/Type/h_pink_left_click.png"));
                                break;
                            default:
                                _controlToolBar.Highlighter.NormalImage = new System.Windows.Media.Imaging.BitmapImage(new Uri("pack://application:,,,/Resources/Icons/Content/Pen/Type/h_yellow_left_click.png"));
                                _controlToolBar.Highlighter.MouseOverImage = new System.Windows.Media.Imaging.BitmapImage(new Uri("pack://application:,,,/Resources/Icons/Content/Pen/Type/h_yellow_left_click.png"));
                                break;
                        }
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

        #endregion

        #region -- Undo/redo --

        //Track when mouse or stylus goes up to increment the editingOperationCount for undo / redo
        private void inkCanvas1_MouseUp(object sender, MouseButtonEventArgs e)
        {
            _editingOperationCount++;

            if (this.inkCanvas1.Cursor == Cursors.None)
            {
                InkCanvasCursorStatus = InkCanvasCursorEnum.PEN;
            }
        }

        private void Strokes_StrokesChanged(object sender, System.Windows.Ink.StrokeCollectionChangedEventArgs e)
        {
            //if (e.Added.Count > 0)
            //{
            //    log.DebugFormat("Cache pen strokes collection in content view: {0}", this.GetHashCode());
            //    _addedList.Push(e.Added);
            //}

            StrokeCollection added = new StrokeCollection(e.Added);
            IndexAllStrokes(e.Added);
            StrokeCollection removed = new StrokeCollection(e.Removed);

            if (e.Removed.Count > 0)
            {
                int rem = e.Removed[0].GetPropertyDataIds().Count();
                if (rem > 0)
                {
                    int index1 = (int)e.Removed[0].GetPropertyData(CommandItem.STROKE_INDEX_PROPERTY);
                    //MessageBox.Show(string.Format("Removed {0}", index1));
                }
            }

            CommandItem item = new StrokesAddedOrRemovedCI(_cmdStack, inkCanvas1.EditingMode, added, removed, _editingOperationCount);
            _cmdStack.Enqueue(item);
        }

        private void IndexAllStrokes(StrokeCollection added)
        {
            foreach (Stroke stroke in added)
            {
                //Remember original index of each stroke in collection
                int strokeIndex = inkCanvas1.Strokes.IndexOf(stroke);
                stroke.AddPropertyData(CommandItem.STROKE_INDEX_PROPERTY, strokeIndex);
            }
        }

        private void inkCanvas1_SelectionMovingOrResizing(object sender, InkCanvasSelectionEditingEventArgs e)
        {
            // Enforce stroke bounds to positive territory.
            Rect newRect = e.NewRectangle; Rect oldRect = e.OldRectangle;

            if (newRect.Top < 0d || newRect.Left < 0d)
            {
                Rect newRect2 =
                    new Rect(newRect.Left < 0d ? 0d : newRect.Left,
                                newRect.Top < 0d ? 0d : newRect.Top,
                                newRect.Width,
                                newRect.Height);

                e.NewRectangle = newRect2;
            }
            CommandItem item = new SelectionMovedOrResizedCI(_cmdStack, inkCanvas1.GetSelectedStrokes(), newRect, oldRect, _editingOperationCount);
            _cmdStack.Enqueue(item);
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
            return 1;
        }

        public Window GetWindow()
        {
            return this;
        }

        public IntPtr GetHandle()
        {
            return Handle;
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

    public enum InkCanvasCursorEnum
    {
        DEFAULT
        , PEN
        , ERASER
    }
}
