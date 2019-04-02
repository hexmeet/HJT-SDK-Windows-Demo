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
    /// Interaction logic for ConfManagementWindow.xaml
    /// </summary>
    public partial class ConfManagementWindow : Window, IDisposable, IMasterDisplayWindow
    {
        #region -- Members --

        private const double WINDOW_DESIGN_WIDTH = 662;
        private readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private const string CONF_MANAGEMENT_URL = "{0}/webapp/#/confControl?numericId={1}&token={2}&deviceId={3}&userId={4}&lang={5}"; // 0: server address

        private ConferenceJsEvent _conferenceJsEvent = null;

        private EasyVideoWin.CustomControls.BrowserLoadingPanel _loadingView = new EasyVideoWin.CustomControls.BrowserLoadingPanel();
        private EasyVideoWin.CustomControls.BrowserLoadFailedPanel _loadFailedView = new EasyVideoWin.CustomControls.BrowserLoadFailedPanel();
        private System.Timers.Timer _browserLoadTimer = new System.Timers.Timer();
        private IntPtr _handle;

        #endregion

        #region -- Properties --

        public bool IsDisposed { get; private set; } = false;

        #endregion

        #region -- Constructor --

        public ConfManagementWindow()
        {
            InitializeComponent();

            //this.Browser = (this.wfh.Child as WhiteBoardForm).browser;
            //this.Browser.MenuHandler = new CustomMenuHandler();
            //this.Browser.RequestHandler = new ConfCefRequestHandler();
            //this.Browser.ConsoleMessage += OnBrowserConsoleMessage;
            //this.Browser.LoadError += Browser_LoadError;
            //this.Browser.IsBrowserInitializedChanged += Browser_IsBrowserInitializedChanged;
            //this.Loaded += ConferenceManagement_Loaded;

            //this.browser.Visibility = Visibility.Collapsed;
            this.contentPresenter.Content = _loadingView;
            this.contentPresenter.Visibility = Visibility.Visible;

            _browserLoadTimer.Interval = 20 * 1000;
            _browserLoadTimer.AutoReset = false;
            _browserLoadTimer.Elapsed += BrowserLoadTimer_Elapsed;

            _loadFailedView.ReloadEvent += LoadFailedView_ReloadEvent;

            _conferenceJsEvent = new ConferenceJsEvent(this.browser, this);
            _conferenceJsEvent.PageCreatedEvent += ConferenceJsEvent_PageCreatedEvent;
            this.browser.RegisterJsObject("conferenceObj", _conferenceJsEvent, new BindingOptions() { CamelCaseJavascriptNames = false });
            //this.browser.JavascriptObjectRepository.Register("conferenceObj", _conferenceJsEvent, true, new BindingOptions() { CamelCaseJavascriptNames = false });
            this.browser.MenuHandler = new CustomMenuHandler();
            this.browser.RequestHandler = new ConfCefRequestHandler();
            this.browser.ConsoleMessage += OnBrowserConsoleMessage;
            this.browser.LoadError += Browser_LoadError;
            this.Loaded += ConferenceManagement_Loaded;
            this.IsVisibleChanged += ConfManagementWindow_IsVisibleChanged;
            this.browser.IsBrowserInitializedChanged += Browser_IsBrowserInitializedChanged;
            LoginManager.Instance.PropertyChanged += LoginManager_PropertyChanged;
        }

        #endregion

        #region -- Public Method --

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
                    //Clean Up managed resources
                    LoginManager.Instance.PropertyChanged -= LoginManager_PropertyChanged;
                    Application.Current.Dispatcher.Invoke(() => {
                        _loadFailedView.ReloadEvent -= LoadFailedView_ReloadEvent;
                        _conferenceJsEvent.PageCreatedEvent -= ConferenceJsEvent_PageCreatedEvent;

                        this.browser.ConsoleMessage -= OnBrowserConsoleMessage;
                        this.browser.LoadError -= Browser_LoadError;
                        this.Loaded -= ConferenceManagement_Loaded;
                        this.IsVisibleChanged -= ConfManagementWindow_IsVisibleChanged;
                        this.browser.IsBrowserInitializedChanged -= Browser_IsBrowserInitializedChanged;

                        this.browser.Dispose();
                    });
                }
                //Clean up unmanaged resources  
            }
            IsDisposed = true;
        }

        #endregion

        #region -- Protected Method --

        #endregion

        #region -- Private Method --

        private void BrowserLoadTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            Application.Current.Dispatcher.Invoke(() => {
                this.contentPresenter.Content = _loadFailedView;
            });
        }

        private void LoadFailedView_ReloadEvent(object sender, RoutedEventArgs e)
        {
            this.contentPresenter.Content = _loadingView;
            this.browser.Reload(true);
            _browserLoadTimer.Enabled = true;
        }

        private void ConferenceJsEvent_PageCreatedEvent()
        {
            _browserLoadTimer.Enabled = false;
            Application.Current.Dispatcher.Invoke(() => {
                this.contentPresenter.Visibility = Visibility.Collapsed;
                //this.browser.Visibility = Visibility.Visible;
            });
        }

        private void Browser_IsBrowserInitializedChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if ("IsBrowserInitialized" == e.Property.Name)
            {
                bool initialized = (bool)(e.NewValue);
                if (!initialized)
                {
                    return;
                }
            }

            System.Timers.Timer timerBrowserReload = new System.Timers.Timer();
            timerBrowserReload.Elapsed += new System.Timers.ElapsedEventHandler((object source, System.Timers.ElapsedEventArgs args) =>
            {
                Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    // For unknow reason, the browser load a blank page in initialization, so load again after a deferred time
                    ((CefSharp.Wpf.ChromiumWebBrowser)sender).Load(GetConferenceManagementUrl());
                    _browserLoadTimer.Enabled = true;
                });
            });
            timerBrowserReload.Interval = 10;
            timerBrowserReload.Enabled = true;
            timerBrowserReload.AutoReset = false;
        }

        private void Browser_LoadError(object sender, LoadErrorEventArgs e)
        {
            log.InfoFormat("Error occurred when loading conference management browser. Error code:{0}, failed url:{1}", e.ErrorCode, e.FailedUrl);
        }

        private void ConferenceManagement_Loaded(object sender, RoutedEventArgs e)
        {
            _handle = new WindowInteropHelper(this).Handle;
            HwndSource hwndSource = (HwndSource)HwndSource.FromVisual((Window)sender);
            hwndSource.AddHook(DragHook);
        }

        private void ConfManagementWindow_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (!this.browser.IsBrowserInitialized)
            {
                return;
            }

            if (Visibility.Visible == this.Visibility)
            {
                _conferenceJsEvent.UpdateData();
            }
            else
            {
                _conferenceJsEvent.ClearConfManagementCache();
            }
        }

        private void OnBrowserConsoleMessage(object sender, ConsoleMessageEventArgs args)
        {
            log.InfoFormat("Browser console log: {0}", args.Message);
        }

        private string GetConferenceManagementUrl()
        {
            string address = Properties.Settings.Default.ConfManagementAddress;
            if (string.IsNullOrEmpty(address))
            {
                address = string.Format(
                    CONF_MANAGEMENT_URL
                    , LoginManager.Instance.LoginUserInfo.customizedH5UrlPrefix
                    , CallController.Instance.ConferenceNumber
                    , LoginManager.Instance.LoginToken
                    , LoginManager.Instance.DeviceId
                    , LoginManager.Instance.UserId
                    , LanguageUtil.Instance.GetCurrentWebLanguage()
                );
            }
        
            log.InfoFormat("Conference management url: {0}", address);

            return address;
        }

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

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            DisplayUtil.AdjustWindowPosition(this, VideoPeopleWindow.Instance);
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

        private void LoginManager_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if ("LoginToken" == e.PropertyName)
            {
                if (string.IsNullOrEmpty(LoginManager.Instance.LoginToken) || null == _conferenceJsEvent)
                {
                    return;
                }

                _conferenceJsEvent.UpdateToken(LoginManager.Instance.LoginToken);
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
