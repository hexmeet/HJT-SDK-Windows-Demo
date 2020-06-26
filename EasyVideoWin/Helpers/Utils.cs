using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows;
using System.Windows.Media.Imaging;
using System.Windows.Interop;
using System.Drawing;
using System.Windows.Forms;
using System.Windows.Media;
using System.Net;
using System.Text.RegularExpressions;
using EasyVideoWin.Model;
using log4net;

namespace EasyVideoWin.Helpers
{
    public class Utils
    {
        private static ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        public const string CONFIG_TITLE = "Config";
        public const string RUNNING_STATUS_TITLE = "RunningStatus";
        public const string ANONYMOUS_JOIN_CONFERENCE_TITLE = "AnonymousJoinConferenceSetting";
        public const string CONFERENCE_IDS_SETTING_TITLE = "ConferenceIdsSetting";
        public const string DEFAULT_CONFIG_PATH = "EasyVideo\\Config\\";
        public const string DEFAULT_CONFIG_NAME = "EasyVideo\\Config\\config.ini";
        public const string DEFAULT_IMG_TMP_INK = "EasyVideo\\SnapShot\\tmpInk.bmp";
        public const string DEFAULT_IMG_TMP_SCREEN = "EasyVideo\\SnapShot\\tmpScreen.bmp";
        public const string DEFAULT_IMG_TMP_SCREEN_PATH = "EasyVideo\\SnapShot\\";
        public const string DEFAULT_BACKGROUND_IMG_FILE_NAME = "Resources\\Icons\\background.jpg";
        public const string AUDIO_MODE_BACKGROUND_IMG_FILE_NAME = "Resources\\Icons\\bg_audio-mode.jpg";
        private const string ROOT_CA_FILE_NAME = "rootca.pem";
        public const string DEFAULT_USER_HEADER_FILE_NAME = "Resources\\Icons\\default_user_header.jpg";
        private const string CONFLICT_LOG_NAME = "App-conflict.log";
        private const string CEF_CACHE_PATH = "CefCache";
        private const string CEF_LOG_NAME = "CEF.log";
        public const string INSTALL_PACKAGE_PATH = "Install";
        public static readonly string SIP_SETTING = "SipSetting";
        private const string CONFIG_DISABLE_CAMERA_ON_JOIN_CONF         = "DisableCameraOnJoinConf";
        private const string CONFIG_DISABLE_MIC_ON_JOIN_CONF            = "DisableMicOnJoinConf";

        public static readonly string AES_KEY = "JA56!*?>afa%^fgFACD$#$<:'F$&klac";
        public static readonly string AES_IV = ";lGFF56?>{]')*}A";
        public static readonly string LANGUAGE = "language";
        public static readonly string INVALID_DISPLAY_NAME_REGEX = "[\"<>]+";
        public static readonly int DISPLAY_NAME_MAX_LENGTH = 32;

        public static System.Windows.Media.Brush DefaultForeGround = new SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#919191"));
        public static System.Windows.Media.Brush SelectedForeGround = new SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#ffffff"));
        public static System.Windows.Media.Brush DefaultBackGround = new SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#000000"));
        public static System.Windows.Media.Brush SelectedBackGround = new SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#4381ff"));

        private static string _screenPicturePath = null;

        [DllImport("kernel32")]
        private static extern bool WritePrivateProfileString(string section, string key, string val, string filePath);
        [DllImport("kernel32")]
        private static extern uint GetPrivateProfileString(string section, string key, string def, StringBuilder retVal, int size, string filePath);
        [DllImport("user32.dll")]
        private static extern IntPtr MonitorFromWindow(IntPtr handle, Int32 flags);
        private const string SOFTWARE_RUN_REGISTRY_KEY = @"Software\Microsoft\Windows\CurrentVersion\Run";
        private static readonly string AUTO_RUN_APP_NAME = 
            System.Reflection.Assembly.GetExecutingAssembly().GetName().Name;

        [DllImport("User32.DLL", SetLastError = true)]
        private static extern bool SetWindowPos(IntPtr hWnd, int hWndInsertAfter, int x, int y, int cx, int cy, uint uFlags);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);

        [DllImport("kernel32.dll")]
        public static extern uint GetLastError();

        [DllImport("user32.dll")]
        private static extern int GetWindowLong(IntPtr hWnd, int nIndex);
        [DllImport("user32.dll")]
        private static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

        private const string DOWNLOAD_FOLDER_GUID = "{374DE290-123F-4565-9164-39C4925E467B}";
        private const uint KNOWN_FOLDER_FLAGS_DONT_VERIFY = 0x00004000;
        [DllImport("Shell32.dll")]
        private static extern int SHGetKnownFolderPath([MarshalAs(UnmanagedType.LPStruct)]Guid rfid, uint dwFlags, IntPtr hToken, out IntPtr ppszPath);

        public const uint SWP_NOMOVE = 0x0002;
        protected const uint SWP_NOZORDER = 0x0004;
        protected const uint SWP_NOREDRAW = 0x0008;
        protected const uint SWP_NOACTIVATE = 0x0010;
        protected const uint SWP_NOOWNERZORDER = 0x0200;
        protected const uint SWP_NOSENDCHANGING = 0x0400;
        protected const uint SWP_DEFERERASE = 0x2000;
        protected const uint SWP_ASYNCWINDOWPOS = 0x4000;
        protected const uint SWP_SHOWWINDOW = 0x0040;
        protected const int HWND_TOP = 0;
        protected const int HWND_TOPMOST = -1;
        protected const int HWND_NOTOPMOST = -2;

        private const int GWL_EXSTYLE = (-20);
        private const int WS_EX_TOOLWINDOW = 0x0080;
        private const int WS_EX_APPWINDOW = 0x40000;
        
        public enum ServiceTypeEnum
        {
            None
            , Enterprise
            , Cloud
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct WINDOWPOS
        {
            public IntPtr hwnd;
            public IntPtr hwndInsertAfter;
            public int x;
            public int y;
            public int cx;
            public int cy;
            public int flags;
        }

        public enum WM
        {
            WM_SIZE             = 0x0005,
            WINDOWPOSCHANGING   = 0x0046,
            WM_WINDOWPOSCHANGED = 0x0047,
            EXITSIZEMOVE        = 0x0232,
            WM_SYSCOMMAND       = 0x0112,
            WM_GETMINMAXINFO    = 0x0024,
            WM_DPICHANGED       = 0x02E0,
            WM_CHAR             = 0x0102
        }

        public struct RECT
        {
            public int left;
            public int top;
            public int right;
            public int bottom;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct MINMAXINFO
        {
            public POINT ptReserved;
            public POINT ptMaxSize;
            public POINT ptMaxPosition;
            public POINT ptMinTrackSize;
            public POINT ptMaxTrackSize;
        };

        [StructLayout(LayoutKind.Sequential)]
        public struct POINT
        {
            /// <summary>
            /// x coordinate of point.
            /// </summary>
            public int x;
            /// <summary>
            /// y coordinate of point.
            /// </summary>
            public int y;

            /// <summary>
            /// Construct a point of coordinates (x,y).
            /// </summary>
            public POINT(int x, int y)
            {
                this.x = x;
                this.y = y;
            }
        }

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool GetCursorPos(ref Win32Point pt);

        [StructLayout(LayoutKind.Sequential)]
        internal struct Win32Point
        {
            public Int32 X;
            public Int32 Y;
        };

        public static System.Windows.Point GetMousePosition() // mouse position relative to screen
        {
            Win32Point w32Mouse = new Win32Point();
            GetCursorPos(ref w32Mouse);
            return new System.Windows.Point(w32Mouse.X, w32Mouse.Y);
        }
        
        public static bool MySetWindowPos(IntPtr hWnd, Rect rect)
        {
            return SetWindowPos(hWnd, 0, (int)rect.Left, (int)rect.Top, (int)rect.Width, (int)rect.Height, SWP_NOZORDER | SWP_NOACTIVATE);
        }

        public static bool SetWindow2Top(Window window)
        {
            //uint dpiX = 96;
            //uint dpiY = 96;
            //IntPtr handle = new WindowInteropHelper(window).Handle;
            //var currentScreen = DpiUtil.GetScreenByHandle(handle);
            //try
            //{
            //    DpiUtil.GetDpiByScreen(currentScreen, out dpiX, out dpiY);
            //}
            //catch(DllNotFoundException e)
            //{
            //    log.ErrorFormat("Can not load windows dll: {0}", e);
            //}

            //double ratioX = (double)dpiX / 96d;
            //double ratioY = (double)dpiY / 96d;
            //return SetWindowPos(
            //    handle
            //    , HWND_TOP
            //    , (int)(window.Left * ratioX)
            //    , (int)(window.Top * ratioY)
            //    , (int)(window.Width * ratioX)
            //    , (int)(window.Height * ratioY)
            //    , SWP_NOACTIVATE
            //);

            IntPtr handle = new WindowInteropHelper(window).Handle;
            return SetWindow2Top(handle);
        }

        public static bool SetWindow2Top(IntPtr handle)
        {
            if (IntPtr.Zero == handle)
            {
                return false;
            }
            RECT rect;
            GetWindowRect(handle, out rect);
            return SetWindowPos(
                handle
                , HWND_TOP
                , (int)rect.left
                , (int)rect.top
                , (int)(rect.right - rect.left)
                , (int)(rect.bottom - rect.top)
                , SWP_NOACTIVATE
            );
        }

        public static bool SetWindow2NoTopMost(Window window)
        {
            //uint dpiX = 96;
            //uint dpiY = 96;
            //IntPtr handle = new WindowInteropHelper(window).Handle;
            //var currentScreen = DpiUtil.GetScreenByHandle(handle);
            //try
            //{
            //    DpiUtil.GetDpiByScreen(currentScreen, out dpiX, out dpiY);
            //}
            //catch (DllNotFoundException e)
            //{
            //    log.ErrorFormat("Can not load windows dll: {0}", e);
            //}

            //double ratioX = (double)dpiX / 96d;
            //double ratioY = (double)dpiY / 96d;
            //return SetWindowPos(
            //    new WindowInteropHelper(window).Handle
            //    , HWND_NOTOPMOST
            //    , (int)(window.Left * ratioX)
            //    , (int)(window.Top * ratioY)
            //    , (int)(window.Width * ratioX)
            //    , (int)(window.Height * ratioY)
            //    , SWP_NOACTIVATE
            //);

            IntPtr handle = new WindowInteropHelper(window).Handle;
            return SetWindow2NoTopMost(handle);
        }

        public static bool SetWindow2NoTopMost(IntPtr handle)
        {
            RECT rect;
            GetWindowRect(handle, out rect);
            return SetWindowPos(
                handle
                , HWND_NOTOPMOST
                , (int)rect.left
                , (int)rect.top
                , (int)(rect.right - rect.left)
                , (int)(rect.bottom - rect.top)
                , SWP_NOACTIVATE
            );
        }

        public static bool SetWindowPosSmoothly(IntPtr handle, double left, double top, double width, double height)
        {
            double ratioX = 1;
            double ratioY = 1;
            try
            {
                DpiUtil.GetDpiRatioByHandle(handle, out ratioX, out ratioY);
            }
            catch (DllNotFoundException e)
            {
                log.ErrorFormat("Can not load windows dll: {0}", e);
            }

            return SetWindowPos(
                                  handle
                                , 0
                                , (int)(left * ratioX)
                                , (int)(top * ratioY)
                                , (int)(width * ratioX)
                                , (int)(height * ratioY)
                                , SWP_NOZORDER | SWP_NOACTIVATE | SWP_NOREDRAW | SWP_DEFERERASE
                                );
        }

        public static bool SetWindow2TopMost(Window window)
        {
            IntPtr handle = new WindowInteropHelper(window).Handle;
            return SetWindow2TopMost(handle);
        }

        public static bool SetWindow2TopMost(IntPtr handle)
        {
            if (IntPtr.Zero == handle)
            {
                return false;
            }
            RECT rect;
            GetWindowRect(handle, out rect);
            return SetWindowPos(
                handle
                , HWND_TOPMOST
                , (int)rect.left
                , (int)rect.top
                , (int)(rect.right - rect.left)
                , (int)(rect.bottom - rect.top)
                , SWP_SHOWWINDOW
            );
        }

        public static bool SetWindowPosTopMost(IntPtr handle, int left, int top, int width, int height)
        {
            return SetWindowPos(
                                  handle
                                , HWND_TOPMOST
                                , left
                                , top
                                , width
                                , height
                                , SWP_SHOWWINDOW
                                );
        }

        public static Bitmap GetScreenSnapshot()
        {
            Rectangle rc = SystemInformation.VirtualScreen;
            Bitmap bitmap = new Bitmap(rc.Width, rc.Height, System.Drawing.Imaging.PixelFormat.Format32bppArgb);

            using (Graphics g = Graphics.FromImage(bitmap))
            {
                g.CopyFromScreen(rc.X, rc.Y, 0, 0, rc.Size, CopyPixelOperation.SourceCopy);
            }

            return bitmap;
        }

        public static bool IniWriteValue(string Section, string Key, string Value, string filePath)
        {
            CreateFile(filePath, "");
            return WritePrivateProfileString(Section, Key, Value, filePath);
        }

        public static string IniReadValue(string Section, string Key, string filePath)
        {
            StringBuilder temp = new StringBuilder(1024);
            uint i = GetPrivateProfileString(Section, Key, "", temp, 1024, filePath);
            return temp.ToString();
        }

        public static bool CreateFile(string filePath, string content)
        {
            if (!File.Exists(filePath))
            {
                //no this file, create the folrder and file
                string[] pathDir = filePath.Split('\\');
                string strFolder = "";
                
                foreach (string tmp in pathDir)
                {
                    if (!tmp.Contains('.'))
                    {
                        strFolder += tmp + "\\";
                    }

                }

                try
                {
                    // Determine whether the directory exists.
                    if (!Directory.Exists(strFolder))
                    {
                        // Try to create the directory.
                        Directory.CreateDirectory(strFolder);
                    }

                    //file create
                    using(StreamWriter sw = new StreamWriter(filePath))
                    {
                        sw.Write(content);
                        sw.Flush();
                        sw.Close();
                    }
                }
                catch (Exception e)
                {
                    return false;
                }
                return true;
            }

            return true;
        }

        public static bool ExistINIFile(string filePath)
        {
            return File.Exists(filePath);
        }

        public static void SavePwd(string pwd, string Section, string Key, string filePath)
        {
            //encrypt
            string encryptPwd = "";
            try
            {
                encryptPwd = CryptoUtil.Encrypt(pwd);
            }
            catch (Exception e)
            {
                return;
            }

            //save
            IniWriteValue(Section, Key, encryptPwd, filePath);
        }

        public static string ReadPwd(string Section, string Key, string filePath)
        {
            //read from ini
            string encPwd = IniReadValue(Section, Key, filePath);
            if (encPwd != "")
            {

                //decrypt
                try  //add try catch to avoid crash
                {
                    //decrypt
                    return CryptoUtil.Decrypt(encPwd);
                }
                catch (Exception e)
                {
                    return "";
                }

            }
            return "";

        }
        
        public static void savePenTypeSelection(string colorType)
        {
            //save
            Utils.IniWriteValue(Utils.CONFIG_TITLE, "pen", colorType, Utils.GetCurConfigFile());
        }

        public static string getPenTypeSelection()
        {
            //save
            return Utils.IniReadValue(Utils.CONFIG_TITLE, "pen", Utils.GetCurConfigFile());
        }

        public static void saveBackColorTypeSelection(string colorType)
        {
            //save
            Utils.IniWriteValue(Utils.CONFIG_TITLE, "backcolor", colorType, Utils.GetCurConfigFile());
        }

        public static string getBackColorSelection()
        {
            //save
            return Utils.IniReadValue(Utils.CONFIG_TITLE, "backcolor", Utils.GetCurConfigFile());
        }
        
        #region Generate Time millineSec
        public static string GenerateMilSec()
        {
            return DateTime.Now.Ticks.ToString();
        }

        public static string HexMeetTime()
        {
            DateTime now = DateTime.Now;
            string formatTime = now.ToString("yyyy-MM-dd_HH.mm.ss");
            StringBuilder sb = new StringBuilder();
            sb.Append(System.Reflection.Assembly.GetExecutingAssembly().GetName().Name).Append("_");
            sb.Append(LanguageUtil.Instance.GetValueByKey("SCREEN_SHOT"));
            sb.Append("_").Append(formatTime);
            return sb.ToString();
        }
        
        #endregion


        #region Get system Path
        public static string GetCurrentPath()
        {
            return System.Environment.CurrentDirectory.ToString();
        }

        public static string GetConfigDataPath()
        {
            string sysPath = GetAppDataPath();
            if (string.IsNullOrEmpty(sysPath))
            {
                //try to get current path
                sysPath = GetCurrentPath();
            }

            return sysPath;
        }

        public static void CreateConfigFilePath()
        {
            string sysPath = GetConfigDataPath();
            string configPath = Path.Combine(sysPath, DEFAULT_CONFIG_PATH);
            if (!Directory.Exists(configPath))
            {
                Directory.CreateDirectory(configPath);
            }
        }

        public static string GetCurConfigFile()
        {
            string sysPath = GetConfigDataPath();
            string configPath = Path.Combine(sysPath, DEFAULT_CONFIG_NAME);
            return configPath;
        }

        public static string GetCurConfigPath()
        {
            string sysPath = GetConfigDataPath();
            string configPath = Path.Combine(sysPath, DEFAULT_CONFIG_PATH);
            return configPath;
        }

        public static string GetCurTmpScreenPath()
        {
            return GetConfigDataPath();
        }
       
        private static string GetAppDataPath()
        {
            string appDataFolder = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            appDataFolder = Path.Combine(appDataFolder, System.Reflection.Assembly.GetExecutingAssembly().GetName().Name);

            try
            {
                // Determine whether the directory exists.
                if (!Directory.Exists(appDataFolder))
                {
                    // Try to create the directory.
                    Directory.CreateDirectory(appDataFolder);
                }

            }
            catch (Exception e)
            {
                return "";
            }

            return appDataFolder;
        }

        public static string GetAppInstalledPath()
        {
            string appFileName = System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName;
            //RegistryKey key = Registry.CurrentUser.OpenSubKey(SOFTWARE_RUN_REGISTRY_KEY, true);
            //if (null != key)
            //{
            //    object runPath = key.GetValue(AUTO_RUN_APP_NAME);
            //    if (runPath != null)
            //    {
            //        appFileName = (string)runPath;
            //    }
                    
            //}

            int index = appFileName.LastIndexOf(AUTO_RUN_APP_NAME);
            if ( index > 0)
                return appFileName.Substring(0, index);

            return "";

        }

        public static string GetCurrentAvatarPath()
        {
            return Path.Combine(GetConfigDataPath(), "header.jpg");
        }

        public static string GetDefaultUserAvatar()
        {
            string path = GetAppInstalledPath();
            return Path.Combine(path, DEFAULT_USER_HEADER_FILE_NAME);
        }

        public static string GetSuspendedVideoBackground()
        {
            string path = GetAppInstalledPath();
            return Path.Combine(path, DEFAULT_BACKGROUND_IMG_FILE_NAME);
        }

        public static string GetAudioModeBackground()
        {
            string path = GetAppInstalledPath();
            return Path.Combine(path, AUDIO_MODE_BACKGROUND_IMG_FILE_NAME);
        }

        public static string GetRootCAPath()
        {
            string path = GetAppInstalledPath();
            return Path.Combine(path, ROOT_CA_FILE_NAME);
        }

        public static string GetInstallPackagePath()
        {
            string sysPath = GetConfigDataPath();
            string installPath = Path.Combine(sysPath, INSTALL_PACKAGE_PATH);
            
            try
            {
                // Determine whether the directory exists.
                if (!Directory.Exists(installPath))
                {
                    // Try to create the directory.
                    Directory.CreateDirectory(installPath);
                }
            }
            catch (Exception e)
            {
                return "";
            }

            return installPath;
        }

        public static string GetConflictLogFileName()
        {
            return Path.Combine(GetConfigDataPath(), CONFLICT_LOG_NAME);
        }

        #endregion

        public static bool IsAutoLogin()
        {
            bool isAutoLogin = false;
            string strAutologinState = Utils.IniReadValue(Utils.CONFIG_TITLE, "RememberMe", Utils.GetCurConfigFile());
            if(strAutologinState == "" )
            {
                isAutoLogin = true;
            }
            else if(strAutologinState == "0")
            {
                isAutoLogin = false;
            }
            else
            {
                isAutoLogin = true;
            }
            return isAutoLogin;
        }

        public static void SetAutoLogin(bool isAutoLogin)
        {
            if(isAutoLogin)
            {
                Utils.IniWriteValue(Utils.CONFIG_TITLE, "RememberMe", "1", Utils.GetCurConfigFile());
            }
            else
            {
                Utils.IniWriteValue(Utils.CONFIG_TITLE, "RememberMe", "0", Utils.GetCurConfigFile());
            }
            
        }

        public static string GetServerAddress(ServiceTypeEnum serviceType)
        {
            if (ServiceTypeEnum.Enterprise == serviceType)
            {
                return Utils.IniReadValue(Utils.CONFIG_TITLE, "EnterpriseServerAddress", Utils.GetCurConfigFile());
            }
            else if (ServiceTypeEnum.Cloud == serviceType)
            {
                return CloudServerDomain;
            }
            else
            {
                return "";
            }
        }

        public static void ParseCloudServerAddress()
        {
            string address = Properties.Settings.Default.CloudLocationServerAddress;
            const string https = "https";
            CloudServerUseHttps = address.Substring(0, https.Length) == https;
            const string prefix = "://";
            int pos = address.IndexOf(prefix);
            string domain;
            int port = CloudServerUseHttps ? 443 : 80;
            if (pos >= 0)
            {
                domain = address.Substring(pos + prefix.Length);
                pos = domain.IndexOf(":");
                if (pos >= 0)
                {
                    string szPort = domain.Substring(pos+1);
                    int.TryParse(szPort, out port);
                    domain = domain.Substring(0, pos);
                }
            }
            else
            {
                domain = address;
            }
            CloudServerDomain = domain;
            CloudServerPort = port;
        }

        public static bool CloudServerUseHttps { get; set; }
        public static int CloudServerPort { get; set; }
        public static string CloudServerDomain { get; set; }

        public static void SetServerAddress(ServiceTypeEnum serviceType, string serverAddress)
        {
            if (ServiceTypeEnum.Enterprise == serviceType)
            {
                Utils.IniWriteValue(Utils.CONFIG_TITLE, "EnterpriseServerAddress", serverAddress, Utils.GetCurConfigFile());
            }
        }

        public static string GetUserName(ServiceTypeEnum serviceType)
        {
            if (ServiceTypeEnum.Enterprise == serviceType)
            {
                return Utils.IniReadValue(Utils.CONFIG_TITLE, "EnterpriseUser", Utils.GetCurConfigFile());
            }
            else if (ServiceTypeEnum.Cloud == serviceType)
            {
                return Utils.IniReadValue(Utils.CONFIG_TITLE, "CloudUser", Utils.GetCurConfigFile());
            }
            else
            {
                return "";
            }
        }

        public static void SetUserName(ServiceTypeEnum serviceType, string userName)
        {
            if (ServiceTypeEnum.Enterprise == serviceType)
            {
                Utils.IniWriteValue(Utils.CONFIG_TITLE, "EnterpriseUser", userName, Utils.GetCurConfigFile());
            }
            else if (ServiceTypeEnum.Cloud == serviceType)
            {
                Utils.IniWriteValue(Utils.CONFIG_TITLE, "CloudUser", userName, Utils.GetCurConfigFile());
            }
        }

        public static string GetPassword(ServiceTypeEnum serviceType)
        {
            if (ServiceTypeEnum.Enterprise == serviceType)
            {
                return Utils.ReadPwd(Utils.CONFIG_TITLE, "EnterprisePassword", Utils.GetCurConfigFile());
            }
            else if (ServiceTypeEnum.Cloud == serviceType)
            {
                return Utils.ReadPwd(Utils.CONFIG_TITLE, "CloudPassword", Utils.GetCurConfigFile());
            }
            else
            {
                return "";
            }
        }

        public static void SetPassword(ServiceTypeEnum serviceType, string pwd)
        {
            if (ServiceTypeEnum.Enterprise == serviceType)
            {
                Utils.SavePwd(pwd, Utils.CONFIG_TITLE, "EnterprisePassword", Utils.GetCurConfigFile());
            }
            else if (ServiceTypeEnum.Cloud == serviceType)
            {
                Utils.SavePwd(pwd, Utils.CONFIG_TITLE, "CloudPassword", Utils.GetCurConfigFile());
            }
        }

        public static void SetServiceType(ServiceTypeEnum serviceType)
        {
            Utils.IniWriteValue(Utils.CONFIG_TITLE, "ServiceType", serviceType.ToString(), Utils.GetCurConfigFile());
        }

        public static ServiceTypeEnum GetServiceType()
        {
            string szServiceType = Utils.IniReadValue(Utils.CONFIG_TITLE, "ServiceType", Utils.GetCurConfigFile());
            return ParseServiceType(szServiceType);
        }

        public static ServiceTypeEnum ParseServiceType(string szServiceType)
        {
            if (string.IsNullOrEmpty(szServiceType))
            {
                return ServiceTypeEnum.None;
            }
            else
            {
                ServiceTypeEnum serviceType;
                try
                {
                    serviceType = (ServiceTypeEnum)Enum.Parse(typeof(ServiceTypeEnum), szServiceType);
                }
                catch (ArgumentException e)
                {
                    log.InfoFormat("ArgumentException for parse service type:{0}, error: {1}", szServiceType, e);
                    return ServiceTypeEnum.None;
                }

                return serviceType;
            }
        }

        public static void SetPreServiceType(ServiceTypeEnum serviceType)
        {
            Utils.IniWriteValue(Utils.CONFIG_TITLE, "PreServiceType", serviceType.ToString(), Utils.GetCurConfigFile());
        }

        public static ServiceTypeEnum GetPreServiceType()
        {
            string szServiceType = Utils.IniReadValue(Utils.CONFIG_TITLE, "PreServiceType", Utils.GetCurConfigFile());
            return ParseServiceType(szServiceType);
        }

        public static void SetPreEnterpriseServerAddress(string serverAddress)
        {
            Utils.IniWriteValue(Utils.CONFIG_TITLE, "PreEnterpriseServerAddress", serverAddress, Utils.GetCurConfigFile());
        }

        public static string GetPreEnterpriseServerAddress()
        {
            return Utils.IniReadValue(Utils.CONFIG_TITLE, "PreEnterpriseServerAddress", Utils.GetCurConfigFile());
        }

        public static int GetPreServerPort()
        {
            string str = Utils.IniReadValue(Utils.CONFIG_TITLE, "PreServerPort", Utils.GetCurConfigFile());
            if (string.IsNullOrEmpty(str))
            {
                return GetPreUseHttps() ? 443 : 80;
            }

            return int.Parse(str);
        }

        public static void SetPreServerPort(int port)
        {
            Utils.IniWriteValue(Utils.CONFIG_TITLE, "PreServerPort", port.ToString(), Utils.GetCurConfigFile());
        }

        public static bool GetPreUseHttps()
        {
            string str = Utils.IniReadValue(Utils.CONFIG_TITLE, "PreUseHttps", Utils.GetCurConfigFile());
            return "TRUE" == str.ToUpper();
        }

        public static void SetPreUseHttps(bool useHttps)
        {
            Utils.IniWriteValue(Utils.CONFIG_TITLE, "PreUseHttps", useHttps.ToString(), Utils.GetCurConfigFile());
        }

        public static void SetLoginStatus(LoginStatus loginStatus)
        {
            Utils.IniWriteValue(Utils.CONFIG_TITLE, "LoginStatus", loginStatus.ToString(), Utils.GetCurConfigFile());
        }

        public static LoginStatus GetLoginStatus()
        {
            string szLoginStatus = Utils.IniReadValue(Utils.CONFIG_TITLE, "LoginStatus", Utils.GetCurConfigFile());
            if (string.IsNullOrEmpty(szLoginStatus))
            {
                return LoginStatus.NotLogin;
            }
            else
            {
                LoginStatus loginStatus;
                try
                {
                    loginStatus = (LoginStatus)Enum.Parse(typeof(LoginStatus), szLoginStatus);
                }
                catch (ArgumentException e)
                {
                    log.InfoFormat("ArgumentException for parse login status:{0}, error: {1}", szLoginStatus, e);
                    return LoginStatus.NotLogin;
                }

                return loginStatus;
            }
        }

        public static void SetDisplayNameInConf(string displayName)
        {
            // base64 display name for there are messy codes in some windows
            byte[] bytes = Encoding.UTF8.GetBytes(displayName);
            string base64Str = Convert.ToBase64String(bytes);
            Utils.IniWriteValue(Utils.CONFIG_TITLE, "DisplayNameUtf8", base64Str, Utils.GetCurConfigFile());
        }

        public static string GetDisplayNameInConf()
        {
            string base64DisplayNameUtf8 = Utils.IniReadValue(Utils.CONFIG_TITLE, "DisplayNameUtf8", Utils.GetCurConfigFile());
            if (!string.IsNullOrEmpty(base64DisplayNameUtf8))
            {
                try
                {
                    byte[] bytes = Convert.FromBase64String(base64DisplayNameUtf8);
                    string str = Encoding.UTF8.GetString(bytes);
                    return str;
                }
                catch (Exception e)
                {
                    log.InfoFormat("Failed to format DisplayNameUtf8 in config file, display name:{0}, exception:{1}", base64DisplayNameUtf8, e);
                    return "";
                }
            }
            else
            {
                string base64DisplayName = Utils.IniReadValue(Utils.CONFIG_TITLE, "DisplayName", Utils.GetCurConfigFile());
                if (!string.IsNullOrEmpty(base64DisplayName))
                {
                    try
                    {
                        byte[] bytes = Convert.FromBase64String(base64DisplayName);
                        string str = Encoding.Unicode.GetString(bytes);
                        SetDisplayNameInConf(str);
                        return str;
                    }
                    catch (Exception e)
                    {
                        log.InfoFormat("Failed to format DisplayName in config file, display name:{0} exception:{1}", base64DisplayName, e);
                        return "";
                    }
                }

                // compatible with the old config
                string displayName = Utils.IniReadValue(Utils.CONFIG_TITLE, "DisplayNameInConf", Utils.GetCurConfigFile());
                if (!string.IsNullOrEmpty(displayName))
                {
                    SetDisplayNameInConf(displayName);
                    Utils.IniWriteValue(Utils.CONFIG_TITLE, "DisplayNameInConf", null, Utils.GetCurConfigFile());
                    return displayName;
                }

                return "";
            }           
        }

        public static int GetServerPort()
        {
            string str = Utils.IniReadValue(Utils.CONFIG_TITLE, "ServerPort", Utils.GetCurConfigFile());
            if (string.IsNullOrEmpty(str))
            {
                return GetUseHttps() ? 443 : 80;
            }

            return int.Parse(str);
        }

        public static void SetServerPort(int port)
        {
            Utils.IniWriteValue(Utils.CONFIG_TITLE, "ServerPort", port.ToString(), Utils.GetCurConfigFile());
        }

        public static bool GetUseHttps()
        {
            string str = Utils.IniReadValue(Utils.CONFIG_TITLE, "UseHttps", Utils.GetCurConfigFile());
            return "1" == str;
        }

        public static void SetUseHttps(bool useHttps)
        {
            if (useHttps)
            {
                Utils.IniWriteValue(Utils.CONFIG_TITLE, "UseHttps", "1", Utils.GetCurConfigFile());
            }
            else
            {
                Utils.IniWriteValue(Utils.CONFIG_TITLE, "UseHttps", "0", Utils.GetCurConfigFile());
            }
        }

        public static void SetDisableCameraOnJoinConf(bool disabled)
        {
            IniWriteValue(CONFIG_TITLE, CONFIG_DISABLE_CAMERA_ON_JOIN_CONF, disabled.ToString(), GetCurConfigFile());
        }

        public static bool GetDisableCameraOnJoinConf()
        {
            string value = Utils.IniReadValue(CONFIG_TITLE, CONFIG_DISABLE_CAMERA_ON_JOIN_CONF, GetCurConfigFile());
            return "TRUE" == value.ToUpper();
        }

        public static void SetDisableMicOnJoinConf(bool disabled)
        {
            IniWriteValue(CONFIG_TITLE, CONFIG_DISABLE_MIC_ON_JOIN_CONF, disabled.ToString(), GetCurConfigFile());
        }

        public static bool GetDisableMicOnJoinConf()
        {
            string value = Utils.IniReadValue(CONFIG_TITLE, CONFIG_DISABLE_MIC_ON_JOIN_CONF, GetCurConfigFile());
            return "FALSE" != value.ToUpper();
        }

        public static bool GetIsConfRunning()
        {
            string str = Utils.IniReadValue(Utils.RUNNING_STATUS_TITLE, "IsConfRunning", Utils.GetCurConfigFile());
            return "1" == str;
        }

        public static void SetIsConfRunning(bool isConfRunning)
        {
            if (isConfRunning)
            {
                Utils.IniWriteValue(Utils.RUNNING_STATUS_TITLE, "IsConfRunning", "1", Utils.GetCurConfigFile());
            }
            else
            {
                Utils.IniWriteValue(Utils.RUNNING_STATUS_TITLE, "IsConfRunning", "0", Utils.GetCurConfigFile());
            }
        }

        public static string GetRunningConfId()
        {
            return Utils.IniReadValue(Utils.RUNNING_STATUS_TITLE, "ConfId", Utils.GetCurConfigFile());
        }

        public static void SetRunningConfId(string confId)
        {
            Utils.IniWriteValue(Utils.RUNNING_STATUS_TITLE, "ConfId", confId, Utils.GetCurConfigFile());
        }

        public static string GetAnonymousJoinConfType()
        {
            return Utils.IniReadValue(ANONYMOUS_JOIN_CONFERENCE_TITLE, "Type", Utils.GetCurConfigFile());
        }

        public static void SetAnonymousJoinConfType(string type)
        {
            Utils.IniWriteValue(ANONYMOUS_JOIN_CONFERENCE_TITLE, "Type", type, Utils.GetCurConfigFile());
        }

        public static string GetAnonymousJoinConfServerAddress()
        {
            return Utils.IniReadValue(ANONYMOUS_JOIN_CONFERENCE_TITLE, "ServerAddress", Utils.GetCurConfigFile());
        }

        public static void SetAnonymousJoinConfServerAddress(string address)
        {
            Utils.IniWriteValue(ANONYMOUS_JOIN_CONFERENCE_TITLE, "ServerAddress", address, Utils.GetCurConfigFile());
        }

        public static string GetAnonymousJoinConfId()
        {
            return Utils.IniReadValue(ANONYMOUS_JOIN_CONFERENCE_TITLE, "ConfId", Utils.GetCurConfigFile());
        }

        public static void SetAnonymousJoinConfId(string confId)
        {
            Utils.IniWriteValue(ANONYMOUS_JOIN_CONFERENCE_TITLE, "ConfId", confId, Utils.GetCurConfigFile());
        }

        public static string GetAnonymousJoinConfAlias()
        {
            return Utils.IniReadValue(ANONYMOUS_JOIN_CONFERENCE_TITLE, "ConfAlias", Utils.GetCurConfigFile());
        }

        public static void SetAnonymousJoinConfAlias(string confAlias)
        {
            Utils.IniWriteValue(ANONYMOUS_JOIN_CONFERENCE_TITLE, "ConfAlias", confAlias, Utils.GetCurConfigFile());
        }

        public static string GetAnonymousJoinConfContactId()
        {
            return Utils.IniReadValue(ANONYMOUS_JOIN_CONFERENCE_TITLE, "ContactId", Utils.GetCurConfigFile());
        }

        public static void SetAnonymousJoinConfContactId(string contactId)
        {
            Utils.IniWriteValue(ANONYMOUS_JOIN_CONFERENCE_TITLE, "ContactId", contactId, Utils.GetCurConfigFile());
        }

        public static string GetAnonymousJoinConfContactAlias()
        {
            return Utils.IniReadValue(ANONYMOUS_JOIN_CONFERENCE_TITLE, "ContactAlias", Utils.GetCurConfigFile());
        }

        public static void SetAnonymousJoinConfContactAlias(string contactAlias)
        {
            Utils.IniWriteValue(ANONYMOUS_JOIN_CONFERENCE_TITLE, "ContactAlias", contactAlias, Utils.GetCurConfigFile());
        }

        public static string GetAnonymousJoinConfContactName()
        {
            return Utils.IniReadValue(ANONYMOUS_JOIN_CONFERENCE_TITLE, "ContactName", Utils.GetCurConfigFile());
        }

        public static void SetAnonymousJoinConfContactName(string contactName)
        {
            Utils.IniWriteValue(ANONYMOUS_JOIN_CONFERENCE_TITLE, "ContactName", contactName, Utils.GetCurConfigFile());
        }

        public static string GetAnonymousJoinConfDisplayName()
        {
            return Utils.IniReadValue(ANONYMOUS_JOIN_CONFERENCE_TITLE, "DisplayName", Utils.GetCurConfigFile());
        }

        public static void SetAnonymousJoinConfDisplayName(string displayName)
        {
            Utils.IniWriteValue(ANONYMOUS_JOIN_CONFERENCE_TITLE, "DisplayName", displayName, Utils.GetCurConfigFile());
        }

        public static string GetAnonymousJoinConfPassword()
        {
            return Utils.IniReadValue(ANONYMOUS_JOIN_CONFERENCE_TITLE, "ConfPassword", Utils.GetCurConfigFile());
        }

        public static void SetAnonymousJoinConfPassword(string pwd)
        {
            Utils.IniWriteValue(ANONYMOUS_JOIN_CONFERENCE_TITLE, "ConfPassword", pwd, Utils.GetCurConfigFile());
        }

        public static string GetAnonymousJoinConfServerProtocol()
        {
            return Utils.IniReadValue(ANONYMOUS_JOIN_CONFERENCE_TITLE, "ServerProtocol", Utils.GetCurConfigFile());
        }

        public static void SetAnonymousJoinConfServerProtocol(string protocol)
        {
            Utils.IniWriteValue(ANONYMOUS_JOIN_CONFERENCE_TITLE, "ServerProtocol", protocol, Utils.GetCurConfigFile());
        }

        public static int GetAnonymousJoinConfServerPort()
        {
            string str = Utils.IniReadValue(ANONYMOUS_JOIN_CONFERENCE_TITLE, "ServerPort", Utils.GetCurConfigFile());
            if (string.IsNullOrEmpty(str))
            {
                return 80;
            }

            return int.Parse(str);
        }

        public static void SetAnonymousJoinConfServerPort(int port)
        {
            Utils.IniWriteValue(ANONYMOUS_JOIN_CONFERENCE_TITLE, "ServerPort", port.ToString(), Utils.GetCurConfigFile());
        }
        
        public static bool GetAnonymousLogoutAndAnonymousJoinConf()
        {
            string value = Utils.IniReadValue(ANONYMOUS_JOIN_CONFERENCE_TITLE, "LogoutAndAnonymousJoinConf", Utils.GetCurConfigFile());
            return "TRUE" == value.ToUpper();
        }

        public static void SetAnonymousLogoutAndAnonymousJoinConf(bool logoutAndAnonymousJoinConf)
        {
            Utils.IniWriteValue(ANONYMOUS_JOIN_CONFERENCE_TITLE, "LogoutAndAnonymousJoinConf", logoutAndAnonymousJoinConf.ToString(), Utils.GetCurConfigFile());
        }

        public static bool GetAnonymousLogoutAndLinkP2pCall()
        {
            string value = Utils.IniReadValue(ANONYMOUS_JOIN_CONFERENCE_TITLE, "LogoutAndLinkP2pCall", Utils.GetCurConfigFile());
            return "TRUE" == value.ToUpper();
        }

        public static void SetAnonymousLogoutAndLinkP2pCall(bool logoutAndAnonymousJoinConf)
        {
            Utils.IniWriteValue(ANONYMOUS_JOIN_CONFERENCE_TITLE, "LogoutAndLinkP2pCall", logoutAndAnonymousJoinConf.ToString(), Utils.GetCurConfigFile());
        }

        public static string GetAnonymousJoinConfToken()
        {
            return Utils.IniReadValue(ANONYMOUS_JOIN_CONFERENCE_TITLE, "Token", Utils.GetCurConfigFile());
        }
        
        public static void SetAnonymousJoinConfToken(string token)
        {
            Utils.IniWriteValue(ANONYMOUS_JOIN_CONFERENCE_TITLE, "Token", token, Utils.GetCurConfigFile());
        }

        public static void ClearAnonymousJoinConfData()
        {
            log.Info("ClearAnonymousJoinConfData");
            SetAnonymousJoinConfType("");
            SetAnonymousJoinConfServerProtocol("");
            SetAnonymousJoinConfServerAddress("");
            SetAnonymousJoinConfId("");
            SetAnonymousJoinConfAlias("");
            SetAnonymousJoinConfDisplayName("");
            SetAnonymousJoinConfContactId("");
            SetAnonymousJoinConfContactAlias("");
            SetAnonymousJoinConfContactName("");
            SetAnonymousJoinConfPassword("");
            SetAnonymousJoinConfServerPort(0);
            SetAnonymousJoinConfToken("");
        }

        public static string GetAcsServerAddressFromConfig()
        {
            string addr = Utils.IniReadValue(Utils.CONFIG_TITLE, "Acs", Utils.GetCurConfigFile());
            if(string.IsNullOrEmpty(addr))
            {
                // we close the default aliyun's ACS server for Customer deliver started.
                //addr = WHITEBOARD_DEFAULT_SERVER;
                log.Info("there is not ACS server configed in config file");
            }

            return addr;
        }

        public static void SetConfIdsSetting(List<string> confIds)
        {
            int count = confIds.Count;
            Utils.IniWriteValue(Utils.CONFERENCE_IDS_SETTING_TITLE, "ConfIdsCount", count.ToString(), Utils.GetCurConfigFile());
            for (int i=0; i<count; ++i)
            {
                Utils.IniWriteValue(Utils.CONFERENCE_IDS_SETTING_TITLE, "ConfIds_" + i, confIds[i], Utils.GetCurConfigFile());
            }
        }

        public static List<string> GetConfIdsSetting()
        {
            List<string> list = new List<string>();
            string str = Utils.IniReadValue(Utils.CONFERENCE_IDS_SETTING_TITLE, "ConfIdsCount", Utils.GetCurConfigFile());
            int count = 0;
            try
            {
                count = int.Parse(str);
            }
            catch(Exception e)
            {

            }

            for (int i=0; i<count; ++i)
            {
                string confId = Utils.IniReadValue(Utils.CONFERENCE_IDS_SETTING_TITLE, "ConfIds_" + i, Utils.GetCurConfigFile());
                if (string.IsNullOrEmpty(confId))
                {
                    continue;
                }
                list.Add(confId);
            }

            return list;
        }

        public static string GetEdition()
        {
            return System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();
        }

        public static void SetFullScreenAfterStartup(bool fullScreen)
        {
            Utils.IniWriteValue(Utils.CONFIG_TITLE, "FullScreenAfterStartup", fullScreen.ToString(), Utils.GetCurConfigFile());
        }

        public static bool GetFullScreenAfterStartup()
        {
            string value = Utils.IniReadValue(Utils.CONFIG_TITLE, "FullScreenAfterStartup", Utils.GetCurConfigFile());
            return "TRUE" == value.ToUpper();
        }

        public static bool GetAutoAnswer()
        {
            string value = Utils.IniReadValue(Utils.CONFIG_TITLE, "AntoAnswer", Utils.GetCurConfigFile());
            return "TRUE" == value.ToUpper();
        }

        public static void SetAutoAnswer(bool antoAnswer)
        {
            Utils.IniWriteValue(Utils.CONFIG_TITLE, "AntoAnswer", antoAnswer.ToString(), Utils.GetCurConfigFile());
        }

        public static bool SetAutoStartup(bool autoStartup)
        {
            string appFileName = System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName;
            RegistryKey key = Registry.CurrentUser.OpenSubKey(SOFTWARE_RUN_REGISTRY_KEY, true);
            if (null == key)
            {
                key = Registry.CurrentUser.CreateSubKey(SOFTWARE_RUN_REGISTRY_KEY);
            }

            if (autoStartup)
            {
                try
                {
                    key.SetValue(AUTO_RUN_APP_NAME, appFileName);
                    key.Close();
                }
                catch
                {
                    return false;
                }
            }
            else
            {
                try
                {
                    key.DeleteValue(AUTO_RUN_APP_NAME);
                    key.Close();
                }
                catch
                {
                    return false;
                }
            }

            return true;
        }

        public static bool GetAutoStartup()
        {
            string appFileName = System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName;
            RegistryKey key = Registry.CurrentUser.OpenSubKey(SOFTWARE_RUN_REGISTRY_KEY, true);
            if (null == key)
            {
                return false;
            }

            try
            {
                object runPath = key.GetValue(AUTO_RUN_APP_NAME);
                if (null == runPath)
                {
                    return false;
                }

                return appFileName.Equals((string)runPath);
            }
            catch
            {
                return false;
            }
            
        }

        public static void SetAutoCaptureWin(bool autoCaptureWin)
        {
            Utils.IniWriteValue(Utils.CONFIG_TITLE, "autoCaptureWin", autoCaptureWin.ToString(), Utils.GetCurConfigFile());
        }

        public static bool GetAutoCaptureWin()
        {
            string value = Utils.IniReadValue(Utils.CONFIG_TITLE, "autoCaptureWin", Utils.GetCurConfigFile());
            /*
            Default value is off
            if(string.IsNullOrEmpty(value))
            {
                return true;
            }
            */
            return "TRUE" == value.ToUpper();
        }

        public static void SetOpenNewWinInContentStream(bool openNewWinInContentStream)
        {
            Utils.IniWriteValue(Utils.CONFIG_TITLE, "openNewWinInContentStream", openNewWinInContentStream.ToString(), Utils.GetCurConfigFile());
        }

        public static bool GetOpenNewWinInContentStream()
        {
            string value = Utils.IniReadValue(Utils.CONFIG_TITLE, "openNewWinInContentStream", Utils.GetCurConfigFile());
            return "TRUE" == value.ToUpper();
        }
        
        public static string GetPortInConfigFile()
        {
            return Utils.IniReadValue(Utils.CONFIG_TITLE, "port", Utils.GetCurConfigFile());
        }

        public static void ClearPortInConfigFile()
        {
            Utils.IniWriteValue(Utils.CONFIG_TITLE, "port", string.Empty, Utils.GetCurConfigFile());
        }
        
        public static void SetOpenHighFrameRateVideo(bool openHighFrameRateVideo)
        {
            Utils.IniWriteValue(Utils.CONFIG_TITLE, "openHighFrameRateVideo", openHighFrameRateVideo.ToString(), Utils.GetCurConfigFile());
        }

        public static bool GetOpenHighFrameRateVideo()
        {
            string value = Utils.IniReadValue(Utils.CONFIG_TITLE, "openHighFrameRateVideo", Utils.GetCurConfigFile());
            return "FALSE" != value.ToUpper();
        }

        public static string GetScreenPicPath()
        {
            if (!string.IsNullOrEmpty(_screenPicturePath))
            {
                return _screenPicturePath;
            }

            _screenPicturePath = Utils.IniReadValue(Utils.CONFIG_TITLE, "ScreenPicPath", Utils.GetCurConfigFile());
            if (string.IsNullOrEmpty(_screenPicturePath))
            {
                IntPtr outPath;
                int result = SHGetKnownFolderPath(new Guid(DOWNLOAD_FOLDER_GUID), KNOWN_FOLDER_FLAGS_DONT_VERIFY, new IntPtr(0), out outPath);
                if (result >= 0)
                {
                    _screenPicturePath = Marshal.PtrToStringUni(outPath);
                    Marshal.FreeCoTaskMem(outPath);
                }
                else
                {
                    log.Info("Failed to get folder download and set the default screen folder to desktop.");
                    _screenPicturePath = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
                }

            }
            return _screenPicturePath;
        }

        public static void SetScreenPicPath(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                path = "";
            }

            Utils.IniWriteValue(Utils.CONFIG_TITLE, "ScreenPicPath", path, Utils.GetCurConfigFile());
            _screenPicturePath = path;
        }

        public static string GetFileNameFromPath(string strPathFile)
        {

            if (strPathFile == null || strPathFile == "")
            {
                return "";
            }
            string strFileName = "";


            string[] sVar = strPathFile.Split(new char[] { '\\' });
            if (sVar.Length > 0)
            {
                strFileName = sVar[sVar.Length - 1];
            }

            if (strFileName.Contains(".mp4"))
            {
                return strFileName;
            }
            return "";
        }

        public static long GetFileSize(string fullPath)
        {
            FileInfo file = new FileInfo(fullPath);
            if (file != null && File.Exists(fullPath))
            {
                //check file info
                return file.Length;
            }

            return 0;
        }
        
        public static string GenerateScreenPictureName()
        {
            string finalPath = Path.Combine(GetScreenPicPath(), HexMeetTime() + ".png");
            return finalPath;
        }

        public static string GetCurPicTmpPath()
        {
            CreateTmpImgFolder();
            string imgScreenPath = GetCurTmpScreenPath() + "\\" + DEFAULT_IMG_TMP_SCREEN_PATH;
            return imgScreenPath;
        }
        
        public static bool CreateTmpImgFolder()
        {
            string strFolder = GetCurTmpScreenPath() + "\\" + "EasyVideo\\SnapShot\\";
            try
            {
                // Determine whether the directory exists.
                if (!Directory.Exists(strFolder))
                {
                    // Try to create the directory.
                    Directory.CreateDirectory(strFolder);
                }
            }
            catch (Exception e)
            {
                return false;
            }
            return true;
        }
        
        public static string AESEncrypt(string plainStr)
        {
            return CryptoUtil.AESEncrypt(plainStr, AES_KEY, AES_IV, false);
        }

        public static string AESDecrypt(string encryptStr)
        {
            return CryptoUtil.AESDecrypt(encryptStr, AES_KEY, AES_IV, false);
        }
        
        public static void SaveLanguage(string language)
        {
            //save
            Utils.IniWriteValue(Utils.CONFIG_TITLE, LANGUAGE, language, Utils.GetCurConfigFile());
        }

        public static string GetLanguage()
        {
            //get
            return Utils.IniReadValue(Utils.CONFIG_TITLE, LANGUAGE, Utils.GetCurConfigFile());
        }
        
        public static int HideWindowInAltTab(Window window)
        {
            IntPtr hwnd = new System.Windows.Interop.WindowInteropHelper(window).Handle;
            //return SetWindowLong(hwnd, GWL_EXSTYLE, (GetWindowLong(hwnd, GWL_EXSTYLE) | WS_EX_TOOLWINDOW) & ~WS_EX_APPWINDOW);
            return HideWindowInAltTab(hwnd);
        }

        public static int HideWindowInAltTab(IntPtr hwnd)
        {
            return SetWindowLong(hwnd, GWL_EXSTYLE, (GetWindowLong(hwnd, GWL_EXSTYLE) | WS_EX_TOOLWINDOW));
        }

        public static void SetSoftwareRender(Visual visual)
        {
            // Software render to avoid OOM
            System.Windows.Interop.HwndSource hwndSource = (System.Windows.Interop.HwndSource)PresentationSource.FromVisual(visual);
            HwndTarget hwndTarget = hwndSource.CompositionTarget;
            hwndTarget.RenderMode = System.Windows.Interop.RenderMode.SoftwareOnly;

            if (RenderMode.SoftwareOnly != RenderOptions.ProcessRenderMode)
            {
                RenderOptions.ProcessRenderMode = RenderMode.SoftwareOnly;
            }
        }
        
        public static void SetAutoHidePartyName(bool autoHidePartyName)
        {
            Utils.IniWriteValue(Utils.CONFIG_TITLE, "AutoHidePartyName", autoHidePartyName.ToString(), Utils.GetCurConfigFile());
        }

        public static bool GetAutoHidePartyName()
        {
            string value = Utils.IniReadValue(Utils.CONFIG_TITLE, "AutoHidePartyName", Utils.GetCurConfigFile());
            return "TRUE" == value.ToUpper();
        }

        public static void SetDisablePrompt(bool disablePrompt)
        {
            Utils.IniWriteValue(Utils.CONFIG_TITLE, "DisablePrompt", disablePrompt.ToString(), Utils.GetCurConfigFile());
        }

        public static bool GetDisablePrompt()
        {
            string value = Utils.IniReadValue(Utils.CONFIG_TITLE, "DisablePrompt", Utils.GetCurConfigFile());
            return "TRUE" == value.ToUpper();
        }

        public static void SetEnable4x4Layout(bool enable4x4Layout)
        {
            Utils.IniWriteValue(Utils.CONFIG_TITLE, "Enable4x4Layout", enable4x4Layout.ToString(), Utils.GetCurConfigFile());
        }

        public static bool GetEnable4x4Layout()
        {
            string value = Utils.IniReadValue(Utils.CONFIG_TITLE, "Enable4x4Layout", Utils.GetCurConfigFile());
            return "TRUE" == value.ToUpper();
        }

        public static void SetFullScreenOnCallConnected(bool fullScreenOnCallConnected)
        {
            Utils.IniWriteValue(Utils.CONFIG_TITLE, "FullScreenOnCallConnected", fullScreenOnCallConnected.ToString(), Utils.GetCurConfigFile());
        }

        public static bool GetFullScreenOnCallConnected()
        {
            string value = Utils.IniReadValue(Utils.CONFIG_TITLE, "FullScreenOnCallConnected", Utils.GetCurConfigFile());
            return "TRUE" == value.ToUpper();
        }

        public static string Utf8Byte2DefaultStr(byte[] bufUtf8)
        {
            log.Info("Utf8Byte2DefaultStr");
            if (null == bufUtf8)
            {
                return "";
            }
            
            byte[] bufferUnicode = Encoding.Convert(Encoding.UTF8, Encoding.Unicode, bufUtf8, 0, bufUtf8.Length);
            string strUnicode = Encoding.Unicode.GetString(bufferUnicode, 0, bufferUnicode.Length);
            log.InfoFormat("Default string: {0}", strUnicode);
            return strUnicode;
        }
    }   
}
