using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace EasyVideoWin.Helpers
{
    class DpiUtil
    {
        [DllImport("kernel32.dll")]
        private static extern IntPtr LoadLibrary(string libname);

        [DllImport("kernel32.dll")]
        private static extern void FreeLibrary(IntPtr hlib);

        [DllImport("user32.dll")]
        public static extern bool SetProcessDPIAware();

        [DllImport("SHCore.dll", SetLastError = true)]
        public static extern bool SetProcessDpiAwareness(PROCESS_DPI_AWARENESS awareness);

        [DllImport("SHCore.dll", SetLastError = true)]
        public static extern void GetProcessDpiAwareness(IntPtr hprocess, out PROCESS_DPI_AWARENESS awareness);

        [DllImport("User32.dll")]
        public static extern IntPtr MonitorFromPoint([In]System.Drawing.Point pt, [In]uint dwFlags);

        public const int MONITOR_DEFAULTTONEAREST = 2;
        [DllImport("User32")]
        public static extern IntPtr MonitorFromWindow(IntPtr handle, int flags);

        [DllImport("Shcore.dll")]
        public static extern IntPtr GetDpiForMonitor([In]IntPtr hmonitor, [In]DpiType dpiType, [Out]out uint dpiX, [Out]out uint dpiY);

        [DllImport("User32.DLL")]
        public static extern bool SetWindowPos(IntPtr hWnd, int hWndInsertAfter, int x, int y, int cx, int cy, uint uFlags);

        public enum PROCESS_DPI_AWARENESS
        {
            Process_DPI_Unaware = 0,
            Process_System_DPI_Aware = 1,
            Process_Per_Monitor_DPI_Aware = 2
        }

        public enum DpiType
        {
            Effective = 0,
            Angular = 1,
            Raw = 2
        }

        public static bool Exists_SHCore_Library()
        {
            bool exists = false;
            IntPtr hShcore = LoadLibrary("SHCore.dll");
            if (hShcore != IntPtr.Zero)
            {
                exists = true;
            }

            FreeLibrary(hShcore);
            return exists;
        }

        public static void Load_DPI_Aware()
        {
            bool exists = Exists_SHCore_Library();
            if (exists)
            {
                Set_Process_Per_Monitor_DPI_Aware();
            }
            else
            {
                SetProcessDPIAware();
            }
        }

        public static void Set_Process_System_DPI_Aware()
        {
            SetProcessDpiAwareness(PROCESS_DPI_AWARENESS.Process_System_DPI_Aware);

            PROCESS_DPI_AWARENESS awareness;
            GetProcessDpiAwareness(Process.GetCurrentProcess().Handle, out awareness);
            Console.WriteLine(awareness.ToString());
        }

        public static void Set_Process_Per_Monitor_DPI_Aware()
        {
            SetProcessDpiAwareness(PROCESS_DPI_AWARENESS.Process_Per_Monitor_DPI_Aware);

            PROCESS_DPI_AWARENESS awareness;
            GetProcessDpiAwareness(Process.GetCurrentProcess().Handle, out awareness);
            Console.WriteLine(awareness.ToString());
        }

        public static void GetDpiByScreen(System.Windows.Forms.Screen screen, out uint dpiX, out uint dpiY)
        {
            if(!Exists_SHCore_Library())
            {
                throw new DllNotFoundException("Dll file not find.");
            }
            var pnt = new System.Drawing.Point(screen.Bounds.Left + 1, screen.Bounds.Top + 1);
            var mon = MonitorFromPoint(pnt, 2/*MONITOR_DEFAULTTONEAREST*/);
            GetDpiForMonitor(mon, DpiType.Effective, out dpiX, out dpiY);
        }

        public static System.Windows.Forms.Screen GetScreenByPoint(int x, int y)
        {
            System.Drawing.Point drawingPoint = new System.Drawing.Point(x, y);
            System.Windows.Forms.Screen screen = System.Windows.Forms.Screen.FromPoint(drawingPoint);
            return screen;
        }

        public static System.Windows.Forms.Screen GetScreenByHandle(IntPtr hWnd)
        {
            System.Windows.Forms.Screen screen = System.Windows.Forms.Screen.FromHandle(hWnd);
            return screen;
        }

        public static double GetCurrentScreenAndPrimaryScreenRatio(System.Windows.Forms.Screen currentScreen)
        {
            try
            {
                uint primaryDpiX = 0;
                uint primaryDpiY = 0;
                DpiUtil.GetDpiByScreen(System.Windows.Forms.Screen.PrimaryScreen, out primaryDpiX, out primaryDpiY);

                uint currentScreenDpiX = 96;
                uint currentScreenDpiY = 96;
                DpiUtil.GetDpiByScreen(currentScreen, out currentScreenDpiX, out currentScreenDpiY);

                return primaryDpiX / currentScreenDpiX;
            }
            catch (Exception e)
            {

            }
            return 1;
        }

        public static void GetDpiRatioByHandle(IntPtr handle, out double ratioX, out double ratioY)
        {
            uint dpiX = 96;
            uint dpiY = 96;
            var currentScreen = GetScreenByHandle(handle);
            try
            {
                GetDpiByScreen(currentScreen, out dpiX, out dpiY);
            }
            catch (DllNotFoundException e)
            {
                throw e;
            }

            ratioX = (double)dpiX / 96d;
            ratioY = (double)dpiY / 96d;
        }
    }
}
