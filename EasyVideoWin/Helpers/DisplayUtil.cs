using log4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Interop;

namespace EasyVideoWin.Helpers
{
    class DisplayUtil
    {
        private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        // PInvoke declaration for EnumDisplaySettings Win32 API
        [DllImport("user32.dll", CharSet = CharSet.Ansi)]
        private static extern int EnumDisplaySettings(
         string lpszDeviceName,
         int iModeNum,
         ref DEVMODE lpDevMode);

        // PInvoke declaration for ChangeDisplaySettings Win32 API
        [DllImport("user32.dll", CharSet = CharSet.Ansi)]
        private static extern int ChangeDisplaySettings(
         ref DEVMODE lpDevMode,
         int dwFlags);

        // PInvoke declaration for ChangeDisplaySettings Win32 API
        [DllImport("user32.dll", CharSet = CharSet.Ansi)]
        private static extern int ChangeDisplaySettingsEx(
         string lpszDeviceName,
         ref DEVMODE lpDevMode,
         IntPtr hwnd,
         int dwFlags,
         IntPtr lParam);

        [DllImport("user32.dll", CharSet = CharSet.Ansi)]
        private static extern int EnumDisplayDevices(
            string lpszDevice,
            int iDevNum,
            ref DISPLAY_DEVICE lpDisplayDevice,
            int dwFlags);

        // constants
        private const int ENUM_CURRENT_SETTINGS = -1;
        private const int DMDO_DEFAULT = 0;
        private const int DMDO_90 = 1;
        private const int DMDO_180 = 2;
        private const int DMDO_270 = 3;
        private const int EDD_GET_DEVICE_INTERFACE_NAME = 1;

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
        private struct DEVMODE
        {
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
            public string dmDeviceName;

            public short dmSpecVersion;
            public short dmDriverVersion;
            public short dmSize;
            public short dmDriverExtra;
            public int dmFields;
            public int dmPositionX;
            public int dmPositionY;
            public int dmDisplayOrientation;
            public int dmDisplayFixedOutput;
            public short dmColor;
            public short dmDuplex;
            public short dmYResolution;
            public short dmTTOption;
            public short dmCollate;

            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
            public string dmFormName;

            public short dmLogPixels;
            public short dmBitsPerPel;
            public int dmPelsWidth;
            public int dmPelsHeight;
            public int dmDisplayFlags;
            public int dmDisplayFrequency;
            public int dmICMMethod;
            public int dmICMIntent;
            public int dmMediaType;
            public int dmDitherType;
            public int dmReserved1;
            public int dmReserved2;
            public int dmPanningWidth;
            public int dmPanningHeight;
        };

        private static DEVMODE CreateDevmode()
        {
            DEVMODE dm = new DEVMODE();
            dm.dmDeviceName = new String(new char[32]);
            dm.dmFormName = new String(new char[32]);
            dm.dmSize = (short)Marshal.SizeOf(dm);
            return dm;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
        private struct DISPLAY_DEVICE
        {
            public int cb;

            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
            public string DeviceName;

            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
            public string DeviceString;

            public int StateFlags;

            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
            public string DeviceID;

            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
            public string DeviceKey;
        }

        private static DISPLAY_DEVICE CreateDisplayDevice()
        {
            DISPLAY_DEVICE displayDevice = new DISPLAY_DEVICE();
            displayDevice.cb = (int)Marshal.SizeOf(displayDevice);
            displayDevice.DeviceName = new String(new char[32]);
            displayDevice.DeviceString = new String(new char[128]);
            displayDevice.DeviceID = new String(new char[128]);
            displayDevice.DeviceKey = new String(new char[128]);
            return displayDevice;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
        public struct RECT
        {
            public int left;
            public int top;
            public int right;
            public int bottom;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
        public struct MONITORINFOEX
        {
            public int cbSize;
            public RECT rcMonitor;
            public RECT rcWork;
            public int dwFlags;

            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
            public string szDevice;
        }

        [DllImport("user32.dll", CharSet = CharSet.Ansi)]
        public static extern int GetMonitorInfo(IntPtr hmonitor, ref MONITORINFOEX lpmi);

        public static MONITORINFOEX CreateMonitorInfo()
        {
            MONITORINFOEX monitorInfo = new MONITORINFOEX();
            monitorInfo.cbSize = (int)Marshal.SizeOf(monitorInfo);
            monitorInfo.rcMonitor = new RECT();
            monitorInfo.rcWork = new RECT();
            monitorInfo.szDevice = new String(new char[32]);
            return monitorInfo;
        }

        public static Rect CheckScreenRect(IntPtr windowHandle)
        {
            Rect rect = new Rect();
            if (windowHandle == IntPtr.Zero)
            {
                return rect;
            }

            IntPtr hMonitor = DpiUtil.MonitorFromWindow(windowHandle, DpiUtil.MONITOR_DEFAULTTONEAREST);
            MONITORINFOEX monitorInfo = DisplayUtil.CreateMonitorInfo();
            int result = DisplayUtil.GetMonitorInfo(hMonitor, ref monitorInfo);
            if (result == 0)
            {
                log.Error("failed to get monitor info");
                return rect;
            }

            DisplayUtil.DEVMODE dm = DisplayUtil.CreateDevmode();
            if (0 != DisplayUtil.EnumDisplaySettings(
                monitorInfo.szDevice,
                DisplayUtil.ENUM_CURRENT_SETTINGS,
                ref dm))
            {
                rect = new Rect(dm.dmPositionX, dm.dmPositionY, dm.dmPelsWidth, dm.dmPelsHeight);
                log.WarnFormat("CheckScreenRect: {0}, {1}, {2}, {3}, {4}", monitorInfo.szDevice, dm.dmPositionX, dm.dmPositionY, dm.dmPelsWidth, dm.dmPelsHeight);
            }
            return rect;
        }

        public static void CheckScreenOrientation(IntPtr windowHandle)
        {
            if (windowHandle == IntPtr.Zero)
            {
                return;
            }

            IntPtr hMonitor = DpiUtil.MonitorFromWindow(windowHandle, DpiUtil.MONITOR_DEFAULTTONEAREST);
            MONITORINFOEX monitorInfo = DisplayUtil.CreateMonitorInfo();
            int result = DisplayUtil.GetMonitorInfo(hMonitor, ref monitorInfo);
            if (result == 0)
            {
                log.Error("failed to get monitor info");
                return;
            }

            DisplayUtil.DEVMODE dm = DisplayUtil.CreateDevmode();
            if (0 != DisplayUtil.EnumDisplaySettings(
                monitorInfo.szDevice,
                DisplayUtil.ENUM_CURRENT_SETTINGS,
                ref dm))
            {
                // swap width and height
                int temp = dm.dmPelsHeight;
                dm.dmPelsHeight = dm.dmPelsWidth;
                dm.dmPelsWidth = temp;

                int iRet = 0;
                // determine new orientation
                switch (dm.dmDisplayOrientation)
                {
                    case DisplayUtil.DMDO_DEFAULT:
                        break;
                    case DisplayUtil.DMDO_270:
                        dm.dmDisplayOrientation = DisplayUtil.DMDO_180;
                        iRet = DisplayUtil.ChangeDisplaySettingsEx(monitorInfo.szDevice, ref dm, IntPtr.Zero, 0, IntPtr.Zero);
                        if (iRet != 0)
                        {
                            log.Error("ChangeDisplaySettings() failed, return value=" + iRet);
                        }
                        break;
                    case DisplayUtil.DMDO_180:
                        break;
                    case DisplayUtil.DMDO_90:
                        dm.dmDisplayOrientation = DisplayUtil.DMDO_DEFAULT;
                        iRet = DisplayUtil.ChangeDisplaySettingsEx(monitorInfo.szDevice, ref dm, IntPtr.Zero, 0, IntPtr.Zero);
                        if (iRet != 0)
                        {
                            log.Error("ChangeDisplaySettings() failed, return value=" + iRet);
                        }
                        break;
                    default:
                        // unknown orientation value
                        // add exception handling here
                        break;
                }
            }
        }

        /*
         * adjust window size if it is not on primary screen
         * */
        public static void AdjustWindowPosition(Window window, IMasterDisplayWindow masterWindow)
        {
            log.Info("Adjust window pos begin");
            bool needAdjust = false;
            IntPtr handle = new WindowInteropHelper(window).Handle;
            Screen screen = DpiUtil.GetScreenByHandle(handle);
            if (!screen.Primary)
            {
                try
                {
                    needAdjust = true;

                    uint dpiX = 0;
                    uint dpiY = 0;
                    DpiUtil.GetDpiByScreen(screen, out dpiX, out dpiY);
                    uint primaryDpiX = 0;
                    uint primaryDpiY = 0;
                    DpiUtil.GetDpiByScreen(Screen.PrimaryScreen, out primaryDpiX, out primaryDpiY);
                    double ratio = (double)dpiX / (double)primaryDpiX;
                    if (ratio > 1)
                    {
                        window.Width *= ratio;
                        window.Height *= ratio;
                        log.Info("adjust window size according to non primary screen's dpi");
                    }
                }
                catch (Exception e)
                {
                    log.ErrorFormat("fail to adjust window size according to non primary screen's dpi: {0}", e);
                }
            }

            if (screen.Bounds.Height / window.Height < 1.2)
            {
                needAdjust = true;
                window.Height = screen.Bounds.Height / 1.2d;
                window.Width = window.Height * masterWindow.GetInitialWidth() / masterWindow.GetInitialHeight();
                log.Info("adjust window size according to screen resolution");
            }

            try
            {
                if (needAdjust)
                {
                    Utils.MySetWindowPos(handle, new Rect(screen.Bounds.Left + (screen.Bounds.Width - window.Width) / 2, screen.Bounds.Top + (screen.Bounds.Height - window.Height) / 2, window.Width, window.Height));
                }
            }
            catch (Exception e)
            {
                log.ErrorFormat("fail to adjust window size and position: {0}", e);
            }
            log.Info("Adjust window pos end");
        }

        private static Screen GetSuitableScreen(IMasterDisplayWindow masterWindow)
        {
            if (System.Windows.Forms.Screen.AllScreens.Length == 1)
            {
                return System.Windows.Forms.Screen.PrimaryScreen;
            }
            else
            {
                // there are more than one screens.
                Screen rejectiveWinScreen = DpiUtil.GetScreenByHandle(masterWindow.GetHandle());
                Screen[] screens = System.Windows.Forms.Screen.AllScreens;
                foreach (System.Windows.Forms.Screen s in screens)
                {
                    if (!s.DeviceName.Equals(rejectiveWinScreen.DeviceName))
                    {
                        return s;
                    }
                }
            }
            return null;
        }

        public static System.Windows.Forms.Screen SetWndOnSuitableScreen(Window window, IMasterDisplayWindow masterWindow)
        {
            log.InfoFormat("set screen for window: {0}", window.Title);
            Screen screen = GetSuitableScreen(masterWindow);
            if (screen == null)
            {
                return null;
            }
            log.InfoFormat("target screen is {0}", screen.DeviceName);
            //IntPtr handle = new WindowInteropHelper(window).Handle;

            Screen currentScreen = DpiUtil.GetScreenByPoint((int)window.Left, (int)window.Top);
            if (screen.Equals(currentScreen))
            {
                // already in the suitable screen, do nothing.
                screen = currentScreen;
            }
            log.DebugFormat("current screen is {0}", currentScreen.DeviceName);

            System.Windows.Forms.Screen masterWindowScreen = DpiUtil.GetScreenByHandle(masterWindow.GetHandle());
            double currentRatio = 1;
            double primaryRatio = 1;
            uint currentScreenDpiX = 96;
            uint currentScreenDpiY = 96;
            uint primaryDpiX = 96;
            uint primaryDpiY = 96;
            try
            {
                DpiUtil.GetDpiByScreen(screen, out currentScreenDpiX, out currentScreenDpiY);
                currentRatio = (double)currentScreenDpiX / 96;
                
                DpiUtil.GetDpiByScreen(System.Windows.Forms.Screen.PrimaryScreen, out primaryDpiX, out primaryDpiY);
                primaryRatio = (double)primaryDpiX / 96;
            }
            catch (DllNotFoundException e)
            {
                log.ErrorFormat("Can not load windows dll: {0}", e);
            }


            log.DebugFormat("```````1```{0}, {1}, {2}, {3}", window.Left, window.Top, window.Width, window.Height);


            if (masterWindow.GetWindow().WindowState == WindowState.Minimized)
            {
                masterWindow.GetWindow().WindowState = WindowState.Normal;
            }

            double tempWidth = window.Width;
            double tempHeight = window.Height;

            if (currentScreen.DeviceName.Equals(System.Windows.Forms.Screen.PrimaryScreen.DeviceName))
            {
                log.DebugFormat("currentScreen in Primary");
                window.Left = (screen.Bounds.Left / primaryRatio + (screen.Bounds.Width / primaryRatio - tempWidth) / 2);
                window.Top = (screen.Bounds.Top / primaryRatio + (screen.Bounds.Height / primaryRatio - tempHeight) / 2);
            }
            else
            {
                log.DebugFormat("currentScreen in extend");
                window.Left = (screen.Bounds.Left * currentRatio + (screen.Bounds.Width * currentRatio - tempWidth) / 2);
                window.Top = (screen.Bounds.Top * currentRatio + (screen.Bounds.Height * currentRatio - tempHeight) / 2);
            }
            
            log.Info("Set window on suitable screen end.");
            return screen;
        }

        public static void SetWindowCenterAndOwner(Window window, IMasterDisplayWindow masterWindow, Window owner = null)
        {
            if (null == masterWindow)
            {
                log.Info("Master window is null and can not center window");
                return;
            }
            System.Windows.Rect masterWindowRect = masterWindow.GetWindowRect();
            window.Left = masterWindowRect.Left + (masterWindowRect.Width - window.Width) / 2;
            window.Top = masterWindowRect.Top + (masterWindowRect.Height - window.Height) / 2;
            window.WindowStartupLocation = WindowStartupLocation.Manual;
            window.Owner = null == owner ? masterWindow.GetWindow() : owner;
        }

        public static void CenterWindowOnMasterWindowScreen(Window window, IMasterDisplayWindow masterWindow)
        {
            log.InfoFormat("Center window on master window screen, window: widht={0}, height={1}", window.Width, window.Height);
            if (null == masterWindow)
            {
                log.Info("Master window is null and can not center window");
                return;
            }

            Screen screen = DpiUtil.GetScreenByHandle(masterWindow.GetHandle());
            
            double currentRatio = 1;
            uint currentScreenDpiX = 96;
            uint currentScreenDpiY = 96;
            try
            {
                DpiUtil.GetDpiByScreen(screen, out currentScreenDpiX, out currentScreenDpiY);
                currentRatio = (double)currentScreenDpiX / 96;
            }
            catch (DllNotFoundException e)
            {
                log.ErrorFormat("Can not load windows dll: {0}", e);
            }

            //if (masterWindow.GetWindow().WindowState == WindowState.Minimized)
            //{
            //    masterWindow.GetWindow().WindowState = WindowState.Normal;
            //}

            double tempWidth = window.Width;
            double tempHeight = window.Height;

            if (screen.DeviceName.Equals(System.Windows.Forms.Screen.PrimaryScreen.DeviceName))
            {
                log.InfoFormat("Set window on primary screen, screen: left={0}, top: {1}, width: {2}, height: {3}, ratio: {4}"
                    , screen.Bounds.Left, screen.Bounds.Top, screen.Bounds.Width, screen.Bounds.Height, currentRatio
                );
                window.Left = (screen.Bounds.Left / currentRatio + (screen.Bounds.Width / currentRatio - tempWidth) / 2);
                window.Top = (screen.Bounds.Top / currentRatio + (screen.Bounds.Height / currentRatio - tempHeight) / 2);
            }
            else
            {
                log.InfoFormat("Set window on extend screen, screen: left={0}, top: {1}, width: {2}, height: {3}, ratio: {4}"
                    , screen.Bounds.Left, screen.Bounds.Top, screen.Bounds.Width, screen.Bounds.Height, currentRatio
                );
                window.Left = (screen.Bounds.Left * currentRatio + (screen.Bounds.Width * currentRatio - tempWidth) / 2);
                window.Top = (screen.Bounds.Top * currentRatio + (screen.Bounds.Height * currentRatio - tempHeight) / 2);
            }

            log.InfoFormat("window is centered, left: {0}, top: {1}", window.Left, window.Top);
        }
    }
}
