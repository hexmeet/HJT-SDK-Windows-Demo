using System;
using System.Drawing;
using System.Windows.Media.Imaging;
using System.Windows.Media;
using System.Runtime.InteropServices;
using System.Drawing.Imaging;
using System.Windows.Forms;
using System.Windows;
using log4net;
using System.IO;

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
            log.InfoFormat("Save snap window picture to path:{0}", path);
            string dir = Path.GetDirectoryName(path);
            if (!Directory.Exists(dir))
            {
                log.Info("The snap window picture for the specified path is not existed.");
            }
            if (File.Exists(path))
            {
                log.Info("The snap window picture for the file is existed.");
            }
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

                hbitmap = CreateCompatibleBitmap(hscrdc, Convert.ToInt32(win.ActualWidth * primaryDpiX / 96), Convert.ToInt32(win.ActualHeight * primaryDpiY / 96));
            }

            IntPtr hmemdc = CreateCompatibleDC(hscrdc);
            SelectObject(hmemdc, hbitmap);

            UInt32 systemFlag = 0;
            if (System.Environment.OSVersion.Version.Major > 6 || (System.Environment.OSVersion.Version.Major == 6 && System.Environment.OSVersion.Version.Minor > 1))
            {
                systemFlag = 2;
            }

            log.InfoFormat("OS Version, major:{0}, minor:{1}, system flag:{2}", System.Environment.OSVersion.Version.Major, System.Environment.OSVersion.Version.Minor, systemFlag);
            bool re = PrintWindow(hWnd, hmemdc, systemFlag);
            if (re)
            {
                //using(Bitmap bmp = Bitmap.FromHbitmap(hbitmap))
                //{
                //    DeleteObject(hbitmap);
                //    DeleteDC(hmemdc);
                //    ReleaseDC(hWnd, hscrdc);
                //    bmp.Save(path, ImageFormat.Png);
                //}

                BitmapSource nImage = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(
                                hbitmap, IntPtr.Zero, System.Windows.Int32Rect.Empty, System.Windows.Media.Imaging.BitmapSizeOptions.FromEmptyOptions());
                log.InfoFormat("Snapped image, PixelWidth:{0}, PixelHeight:{1}, Width:{2}, Height:{3}", nImage.PixelWidth, nImage.PixelHeight, nImage.Width, nImage.Height);
                System.Windows.Media.Imaging.PngBitmapEncoder encoder = new System.Windows.Media.Imaging.PngBitmapEncoder();
                encoder.Frames.Add(System.Windows.Media.Imaging.BitmapFrame.Create(nImage));
                try
                {
                    using (FileStream stream = new FileStream(path, FileMode.Create))
                    {
                        encoder.Save(stream);
                    }
                }
                catch (FileNotFoundException e)
                {
                    log.InfoFormat("Failed to save screen picture, exception:{0}", e);
                    return -2;
                }
                finally
                {
                    log.Info("Save picture successfully and release resource.");
                    DeleteObject(hbitmap);
                    DeleteDC(hmemdc);
                    ReleaseDC(hWnd, hscrdc);
                }
            }
            else
            {
                log.Info("Failed to print window by handle");
                DeleteObject(hbitmap);
                DeleteDC(hmemdc);
                ReleaseDC(hWnd, hscrdc);
                return -1;
            }
            return 0;
        }
    }
}
