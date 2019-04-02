using System;
using System.Drawing;
using System.Windows.Media.Imaging;
using System.Windows.Media;
using System.Runtime.InteropServices;
using System.Drawing.Imaging;
using System.Windows.Forms;
using System.Windows;
using log4net;

namespace EasyVideoWin.Helpers
{
    class MonitorCapture
    {
        [DllImport("user32.dll")]
        public static extern IntPtr GetWindowDC(IntPtr hwnd);
        [DllImport("gdi32.dll")]
        public static extern IntPtr CreateCompatibleDC(IntPtr hdc);
        [DllImport("gdi32.dll")]
        public static extern IntPtr CreateCompatibleBitmap(IntPtr hdc, int nWidth, int nHeight);
        [DllImport("gdi32.dll")]
        public static extern IntPtr SelectObject(IntPtr hdc, IntPtr hgdiobj);
        [DllImport("user32.dll")]
        public static extern bool PrintWindow(IntPtr hwnd, IntPtr hdcBlt, UInt32 nFlags);
        [DllImport("gdi32.dll")]
        public static extern int DeleteDC(IntPtr hdc);
        [DllImport("gdi32.dll")]
        public static extern IntPtr DeleteObject(IntPtr hObject);
        [DllImport("user32.dll")]
        public static extern IntPtr ReleaseDC(IntPtr hWnd, IntPtr hDC);

        private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public static int SaveMonitorPic(System.Windows.Forms.Screen screen, String path)
        {
            int width = screen.Bounds.Width;
            int height = screen.Bounds.Height;
            log.InfoFormat("Create monitor picture, width: {0}, height: {1}", width, height);
            using (Bitmap memoryImage = new Bitmap(width, height))
            {
                System.Drawing.Size s = new System.Drawing.Size(memoryImage.Width, memoryImage.Height);
                using (Graphics memoryGraphics = Graphics.FromImage(memoryImage))
                {
                    memoryGraphics.CopyFromScreen(screen.Bounds.X, screen.Bounds.Y, 0, 0, s);
                }

                string str = path;
                try
                {
                    if (String.IsNullOrEmpty(str))
                    {
                        str = string.Format(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + @"\Screenshot.png");
                    }
                }
                catch (Exception er)
                {
                    log.Info("Failed to create image path: " + er.Message);
                    return -1;
                }

                // Save it! 
                log.Info("Saving the image...");
                memoryImage.Save(str);
            }            
            
            return 0;
        }

        public static int SaveWindowPicByHandle(Window win, IntPtr hWnd, String path)
        {
            IntPtr hscrdc = GetWindowDC(hWnd);
            Control control = Control.FromChildHandle(hWnd);
            IntPtr hbitmap;
            if (control != null)
            {
                hbitmap = CreateCompatibleBitmap(hscrdc, control.Width, control.Height);
            }
            else
            {
                uint primaryDpiX = 96;
                uint primaryDpiY = 96;
                try
                {
                    DpiUtil.GetDpiByScreen(Screen.PrimaryScreen, out primaryDpiX, out primaryDpiY);
                }
                catch (Exception e)
                {

                }
                
                hbitmap = CreateCompatibleBitmap(hscrdc, Convert.ToInt32(win.ActualWidth * primaryDpiX/96), Convert.ToInt32(win.ActualHeight * primaryDpiY/96));
            }
            
            IntPtr hmemdc = CreateCompatibleDC(hscrdc);
            SelectObject(hmemdc, hbitmap);

            UInt32 systemFlag = 0;
            if (System.Environment.OSVersion.Version.Major > 6 || (System.Environment.OSVersion.Version.Major == 6 && System.Environment.OSVersion.Version.Minor > 1))
            {
                systemFlag = 2;
            }

            bool re = PrintWindow(hWnd, hmemdc, systemFlag);
            Bitmap bmp = null;
            if (re)
            {
                bmp = Bitmap.FromHbitmap(hbitmap);
            }
            DeleteObject(hbitmap);
            DeleteDC(hmemdc);
            ReleaseDC(hWnd, hscrdc);
            bmp.Save(path, ImageFormat.Png);
            return 0;
        }
    }
}
