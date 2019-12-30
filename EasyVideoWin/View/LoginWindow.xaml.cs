using EasyVideoWin.CustomControls;
using EasyVideoWin.Helpers;
using EasyVideoWin.Model;
using log4net;
using System;
using System.Collections.Generic;
using System.IO;
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
    /// Interaction logic for LoginWindow.xaml
    /// </summary>
    public partial class LoginWindow : Window, IMasterDisplayWindow
    {
        #region -- Members --

        private readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private const double WINDOW_DESIGN_WIDTH = 530;
        private LoginManager _loginMgr = LoginManager.Instance;
        private IntPtr _handle;

        private static ManagedEVSdk.Structs.EV_LAYOUT_TYPE_CLI[] galleryLayoutCapaticy = {
            ManagedEVSdk.Structs.EV_LAYOUT_TYPE_CLI.EV_LAYOUT_TYPE_1
            , ManagedEVSdk.Structs.EV_LAYOUT_TYPE_CLI.EV_LAYOUT_TYPE_2H
            , ManagedEVSdk.Structs.EV_LAYOUT_TYPE_CLI.EV_LAYOUT_TYPE_4
       //     , ManagedEVSdk.Structs.EV_LAYOUT_TYPE_CLI.EV_LAYOUT_TYPE_6W
            , ManagedEVSdk.Structs.EV_LAYOUT_TYPE_CLI.EV_LAYOUT_TYPE_9
        //    , ManagedEVSdk.Structs.EV_LAYOUT_TYPE_CLI.EV_LAYOUT_TYPE_12W
            , ManagedEVSdk.Structs.EV_LAYOUT_TYPE_CLI.EV_LAYOUT_TYPE_16
        };

        private static ManagedEVSdk.Structs.EV_LAYOUT_TYPE_CLI[] speakerLayoutCapaticy = {
            ManagedEVSdk.Structs.EV_LAYOUT_TYPE_CLI.EV_LAYOUT_TYPE_1
            , ManagedEVSdk.Structs.EV_LAYOUT_TYPE_CLI.EV_LAYOUT_TYPE_5_1L_4R
            , ManagedEVSdk.Structs.EV_LAYOUT_TYPE_CLI.EV_LAYOUT_TYPE_8
        };

        #endregion

        #region -- Properties --

        #endregion

        #region -- Constructor --

        public LoginWindow()
        {
            log.Info("LoginWindow begin to construct");
            InitializeComponent();

            this.Loaded += Window_Loaded;
            LanguageUtil.Instance.LanguageChanged += OnLanguageChanged;

            if (LanguageUtil.Instance.CurrentLanguage != LanguageType.ZH_CN)
            {
                this.imgLogoLogin.Source = new BitmapImage(new Uri("pack://application:,,,/Resources/Icons/logo_login_en.png"));
            }
        }
        
        #endregion

        #region -- Public Method --

        #endregion

        #region -- Protected Method --

        protected override void OnClosed(EventArgs e)
        {
            log.Info("Window on closed");
            base.OnClosed(e);
            Application.Current.MainWindow?.Close();
        }

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
                        Utils.MySetWindowPos(hwnd, new Rect(rect.left, rect.top, rect.right - rect.left, rect.bottom - rect.top));
                    }
                    break;
            }
            return IntPtr.Zero;
        }

        private void CloseBtn_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
        
        private void MinBtn_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            log.InfoFormat("Loaded, visibility:{0}", this.Visibility);
            ProgressDialogLogin.Instance.ShowDialog(this);
            _loginMgr.LoginWindow = this;
            // init app below to show login window quickly
            Application.Current.Dispatcher.InvokeAsync(() => {
                InitAppParameters();
                
                HwndSource hwndSource = (HwndSource)HwndSource.FromVisual((Window)sender);
                hwndSource.AddHook(DragHook);
                
                _handle = new WindowInteropHelper(this).Handle;
                MainWindow mainWindow = (MainWindow)Application.Current.MainWindow;
                DisplayUtil.AdjustWindowPosition(this, mainWindow);
                mainWindow.InitData();
                ProgressDialogLogin.Instance.Hide();
                LoginManager.Instance.IsInitFinished = true;
                this.Activate();
            });
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

        private void Setting_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            SettingWindow win = new SettingWindow();
            win.Owner = this;
            win.ShowDialog();
        }

        private void OnLanguageChanged(object sender, LanguageType language)
        {
            if (LanguageUtil.Instance.CurrentLanguage != LanguageType.ZH_CN)
            {
                this.imgLogoLogin.Source = new BitmapImage(new Uri("pack://application:,,,/Resources/Icons/logo_login_en.png"));
            }
            else
            {
                this.imgLogoLogin.Source = new BitmapImage(new Uri("pack://application:,,,/Resources/Icons/logo_login.png"));
            }
        }
        
        private void InitAppParameters()
        {
            log.Info("Init app parameters");
            Utils.CreateConfigFilePath();

            if (Utils.GetAnonymousLogoutAndAnonymousJoinConf())
            {
                // clear the flag
                Utils.SetAnonymousLogoutAndAnonymousJoinConf(false);
            }

            CefSharpUtil.ClearCefSharpCacheAndLog();
            CefSharpUtil.InitCefSharpSetting();

            Application.Current.MainWindow = new MainWindow();
            InitEVSdk();
            View.VideoPeopleWindow.Instance = new View.VideoPeopleWindow();
            // set the window size to a small value to initialize the window in a smooth showing
            View.VideoPeopleWindow.Instance.Width = 10;
            View.VideoPeopleWindow.Instance.Height = 10;
            View.VideoPeopleWindow.Instance.Init();
            View.VideoPeopleWindow.Instance.Show();
            View.VideoPeopleWindow.Instance.Visibility = Visibility.Collapsed;
            View.VideoPeopleWindow.Instance.ResumeNormalSetting();
            
#if AUTOTEST
            log.Info("Start automation server.");
            www.WwwServer.StartServer();
#endif
        }

        private void InitEVSdk()
        {
            try
            {
                log.Info("Begin to init evsdk");
                bool first_run = false;
                string appDataFolder = Utils.GetConfigDataPath();
                if (!Directory.Exists(appDataFolder))
                {
                    // Try to create the directory.
                    Directory.CreateDirectory(appDataFolder);
                }

                if (!File.Exists(System.IO.Path.Combine(appDataFolder, "hexmeetrc")))
                {
                    log.Info("SDK Configuration doesn't exist. It must be the 1st run after installation.");
                    first_run = true;
                }

                EVSdkManager.Instance.CreateEVEngine();
                EVSdkManager.Instance.SetLog(ManagedEVSdk.Structs.EV_LOG_LEVEL_CLI.EV_LOG_LEVEL_MESSAGE, appDataFolder, "evsdk", 20 * 1024 * 1024);
                EVSdkManager.Instance.EnableLog(true);
                EVSdkManager.Instance.Initialize(appDataFolder, "hexmeetrc");
                EVSdkManager.Instance.EnablePreview(false);
                string rootCAPath = Utils.GetRootCAPath();
                EVSdkManager.Instance.SetRootCA(rootCAPath);
                EVSdkManager.Instance.EnableWhiteBoard(true);
                CallController.Instance.SetMaxRecvVideo(Utils.GetEnable4x4Layout() ? (uint)MaxRecvVideoLayout.Layout_4x4 : (uint)MaxRecvVideoLayout.Layout_3x3);
                CallController.Instance.SetPeopleHighFrameRate(Helpers.Utils.GetOpenHighFrameRateVideo());

                if (first_run)
                {
                    // Set the default call rate to 2M 
                    EVSdkManager.Instance.SetBandwidth(2048);
                    log.Info("Set Default Call Rate to 2M at 1st run");
                }

                string userAgentName = "HexMeet";
                // get correct product version for user agent.
                string version = System.Windows.Forms.Application.ProductVersion;
                EVSdkManager.Instance.SetUserAgent(userAgentName, version);

                EVSdkManager.Instance.SetLayoutCapacity(ManagedEVSdk.Structs.EV_LAYOUT_MODE_CLI.EV_LAYOUT_GALLERY_MODE, galleryLayoutCapaticy);
                EVSdkManager.Instance.SetLayoutCapacity(ManagedEVSdk.Structs.EV_LAYOUT_MODE_CLI.EV_LAYOUT_SPEAKER_MODE, speakerLayoutCapaticy);

                log.Info("Init sdk end.");
            }
            catch(Exception e)
            {
                log.ErrorFormat("Failed to init ev sdk, exception:{0}", e.Message);
                System.Environment.Exit(1);
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
