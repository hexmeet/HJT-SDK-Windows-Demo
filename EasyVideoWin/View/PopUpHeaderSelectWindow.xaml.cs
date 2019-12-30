using EasyVideoWin.CustomControls;
using EasyVideoWin.Helpers;
using EasyVideoWin.Model;
using EasyVideoWin.Model.CloudModel;
using log4net;
using System;
using System.ComponentModel;
using System.Drawing.Imaging;
using System.IO;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace EasyVideoWin.View
{
    /// <summary>
    /// Interaction logic for PopUpHeaderSelectWindow.xaml
    /// </summary>
    public partial class PopUpHeaderSelectWindow : Window
    {
        private readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        public IntPtr hwnd;
        private double _dpiX;
        private double _dpiY;
        private IMasterDisplayWindow _ownerWindow;

        public PopUpHeaderSelectWindow(IMasterDisplayWindow ownerWindow)
        {
            InitializeComponent();
            //disable move this window
            this.SourceInitialized += Window_SourceInitialized;
            GetDpi();

            _ownerWindow = ownerWindow;
            this.Owner = ownerWindow.GetWindow();

            updateImg("pack://application:,,,/Resources/Icons/uploadHeaderBackGround.jpg");
            EVSdkManager.Instance.EventUploadUserImageComplete += EVSdkWrapper_EventUploadUserImageComplete;
            EVSdkManager.Instance.EventError += EVSdkWrapper_EventError;
        }
        
        private void Window_SourceInitialized(object sender, EventArgs e)
        {
            hwnd = new WindowInteropHelper(this).Handle;
            HwndSource source = HwndSource.FromHwnd(hwnd);
            source.AddHook(WndProc);
        }

        const int WM_SYSCOMMAND = 0x0112;
        const int SC_MOVE = 0xF010;
        private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            switch (msg)
            {
                case WM_SYSCOMMAND:
                    int command = wParam.ToInt32() & 0xfff0;
                    if (command == SC_MOVE)
                    {
                        handled = true;
                    }
                    break;
                default:
                    break;
            }
            return IntPtr.Zero;
        }

        private void CloseBtn_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void updateImg(string imgPath)
        {
            BitmapImage uploadHeaderBackgroundPic = new BitmapImage();
            uploadHeaderBackgroundPic.BeginInit();
            uploadHeaderBackgroundPic.UriSource = new Uri(imgPath);
            uploadHeaderBackgroundPic.EndInit();
            viewPhotoDefaultImg.Source = uploadHeaderBackgroundPic; 
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            AdornerLayer layer = AdornerLayer.GetAdornerLayer(finalPic);
            _IMAdorner = new IMAdorner(finalPic, this);
            layer.Add(_IMAdorner);

            RefreshMonitorDpi();
        }

        #region -- image cut area --
        private Cursor oldCursor;
        private bool isMouseLeftButtonDown;
        private Point previousMousePoint;
        string filePath;
        private IMAdorner _IMAdorner;

        private void MasterImage_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            ContentControl img = sender as ContentControl;
            if (img == null) return;

            this.oldCursor = this.Cursor;
            this.Cursor = Cursors.SizeAll;

            img.CaptureMouse();
            this.isMouseLeftButtonDown = true;
            this.previousMousePoint = e.GetPosition(img);
        }

        private void MasterImage_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            ContentControl img = sender as ContentControl;
            if (img == null) return;

            this.Cursor = this.oldCursor;

            img.ReleaseMouseCapture();
            this.isMouseLeftButtonDown = false;
        }

        private void MasterImage_MouseMove(object sender, MouseEventArgs e)
        {
            ContentControl image = sender as ContentControl;

            if (image == null)
            {
                return;
            }

            if (this.isMouseLeftButtonDown && e.LeftButton == MouseButtonState.Pressed)
            {
                this.DoImageMove(image, e.GetPosition(image));
            }
        }

        private void DoImageMove(ContentControl image, Point position)
        {
            TransformGroup group = finalPic.FindResource("ImageCompareResources") as TransformGroup;
            TranslateTransform transform = group.Children[1] as TranslateTransform;
            transform.X += position.X - this.previousMousePoint.X;
            transform.Y += position.Y - this.previousMousePoint.Y;
            this.previousMousePoint = position;
        }

        private void MasterImage_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            ContentControl image = sender as ContentControl;
            if (image == null)
            {
                return;
            }

            TransformGroup group = finalPic.FindResource("ImageCompareResources") as TransformGroup;
            Point point = e.GetPosition(image);
            double scale = e.Delta * 0.001;
            ZoomImage(group, point, scale);
        }

        private static void ZoomImage(TransformGroup group, Point point, double scale)
        {
            Point pointToContent = group.Inverse.Transform(point);
            ScaleTransform transform = group.Children[0] as ScaleTransform;

            if (transform.ScaleX + scale < 0.5)
            {
                return;
            }

            transform.ScaleX += scale;
            transform.ScaleY += scale;
            TranslateTransform transform1 = group.Children[1] as TranslateTransform;
            transform1.X = -1 * ((pointToContent.X * transform.ScaleX) - point.X);
            transform1.Y = -1 * ((pointToContent.Y * transform.ScaleY) - point.Y);
        }

        private void UploadPic_OnClick(object sender, MouseButtonEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog dialog = new Microsoft.Win32.OpenFileDialog();
            String Desktop = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);

            //dialog.InitialDirectory = AppDomain.CurrentDomain.BaseDirectory + Desktop + "\\";
            dialog.InitialDirectory = "C:\\";
            dialog.Filter = "JPG|*.jpg";
            if (dialog.ShowDialog() == true)
            {
                string fullFileName = dialog.FileName;
                filePath = dialog.FileName;
                string fileName = Utils.GetFileNameFromPath(fullFileName);
                long fileSize = Utils.GetFileSize(fullFileName);
                if (fileSize > 1 * 1024 * 1024) // larger than 1M
                {
                    //MessageBox.Show(LanguageUtil.Instance.GetValueByKey("PIC_TOO_LARGE"));
                    MessageBoxTip tip = new MessageBoxTip(_ownerWindow);
                    tip.SetTitleAndMsg(LanguageUtil.Instance.GetValueByKey("PROMPT"), LanguageUtil.Instance.GetValueByKey("PIC_TOO_LARGE"), LanguageUtil.Instance.GetValueByKey("CONFIRM"));
                    tip.ShowDialog();
                    return;
                }
                //uploadTextBlock.Text = fullFileName;
                //uploadFileSize.Content = Utils.GetFileSize(fullFileName);
                //picPathStack.Visibility = Visibility.Visible;
                FilePathReal.Text = fullFileName;
                updateImg(fullFileName);
                finishPicGrid.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#f04848"));
            }
        }

        private void FinishPic_OnClick(object sender, MouseButtonEventArgs e)
        {
            if(FilePathReal.Text == "" || FilePathReal.Text == null)
            {
                return;
            }

            #region 裁剪图片
            System.Windows.Forms.Screen current_Screen = System.Windows.Forms.Screen.FromHandle(hwnd);

            double width = (Int16)Trans2RightPixelX(180);
            double height = (Int16)Trans2RightPixelY(180);
            int offset = (Int16)Trans2RightPixelY(17);
            int current_Screen_Physical_X = current_Screen.WorkingArea.X + (current_Screen.WorkingArea.Width - (int)width) / 2;
            int current_Screen_Physical_Y = current_Screen.WorkingArea.Y + (current_Screen.WorkingArea.Height - (int)height) / 2 - offset;

            double x = current_Screen_Physical_X;
            double y = current_Screen_Physical_Y;
            try
            {
                System.Drawing.Bitmap memoryImage = new System.Drawing.Bitmap((int)width, (int)height);
                System.Drawing.Size s = new System.Drawing.Size(memoryImage.Width, memoryImage.Height);
                System.Drawing.Graphics memoryGraphics = System.Drawing.Graphics.FromImage(memoryImage);
                memoryGraphics.CopyFromScreen((int)x, (int)y, 0, 0, s);

                string tempPath = Utils.GetCurConfigPath();
                string fullFileNamePath = tempPath + DateTime.Now.ToString("yyyyMMddHHmmss") + ".jpg";
                memoryImage.Save(fullFileNamePath, ImageFormat.Jpeg);

                EVSdkManager.Instance.UploadUserImage(fullFileNamePath);
                ProgressDialog.Instance.ShowDialog();
            }
            catch (Exception ex)
            {
                log.InfoFormat("Failed to upload avatar, exception: {0}", ex);
            }
            #endregion 裁剪图片
        }

        private void EVSdkWrapper_EventUploadUserImageComplete(string path)
        {
            log.Info("EventUploadUserImageComplete");
            Application.Current.Dispatcher.InvokeAsync(() => {
                ProgressDialog.Instance.Hide();
                this.DialogResult = true;
                this.Close();

                MessageBoxTip tip = new MessageBoxTip(_ownerWindow);
                tip.SetTitleAndMsg(LanguageUtil.Instance.GetValueByKey("PROMPT"), LanguageUtil.Instance.GetValueByKey("UPLOAD_PIC_SUCCESS"), LanguageUtil.Instance.GetValueByKey("CONFIRM"));
                tip.ShowDialog();
                try
                {
                    File.Delete(path);
                }
                catch (Exception ex)
                {
                    log.InfoFormat("Failed to local avatar for upload: {0}, exception: {1}", path, ex);
                }
            });
            log.Info("EventUploadUserImageComplete end");
        }

        private void EVSdkWrapper_EventError(ManagedEVSdk.Structs.EVErrorCli err)
        {
            log.Info("EventError");
            if (EVSdkManager.ACTION_UPLOADUSERIMAGE == err.action)
            {
                Application.Current.Dispatcher.InvokeAsync(() => {
                    ProgressDialog.Instance.Hide();
                    MessageBoxTip tip = new MessageBoxTip(_ownerWindow);
                    tip.SetTitleAndMsg(LanguageUtil.Instance.GetValueByKey("PROMPT"), LanguageUtil.Instance.GetValueByKey("UPLOAD_PIC_FAILURE"), LanguageUtil.Instance.GetValueByKey("CONFIRM"));
                    tip.ShowDialog();
                });
            }

            log.Info("EventError end");
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            EVSdkManager.Instance.EventUploadUserImageComplete -= EVSdkWrapper_EventUploadUserImageComplete;
            EVSdkManager.Instance.EventError -= EVSdkWrapper_EventError;
            base.OnClosing(e);
        }
        
        #endregion

        #region -- draw gray area --
        class IMAdorner : Adorner
        {
            PopUpHeaderSelectWindow _parent;
            public double x1;
            public double y1;
            public double width;
            public double height;

            public IMAdorner(UIElement adorned, PopUpHeaderSelectWindow parent)
                : base(adorned)
            {
                _parent = parent;
            }

            protected override void OnRender(DrawingContext dc)
            {
                base.OnRender(dc);

                Grid ic = this.AdornedElement as Grid;
                SolidColorBrush scb = new SolidColorBrush(Color.FromArgb(197, 173, 173, 173));

                x1 = ic.Width;
                y1 = ic.Height;
                width = height = (ic.Width - 100) / 2;
                //dc.DrawRectangle(new SolidColorBrush(), new Pen(scb, width), new Rect(0, 0, x1, y1));
                dc.DrawRectangle(new SolidColorBrush(), new Pen(scb, 60), new Rect(25, 30, 245, 245));
            }

            protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
            {
                _parent.MasterImage_MouseLeftButtonDown(_parent.TestContentControl1, e);
            }

            protected override void OnMouseLeftButtonUp(MouseButtonEventArgs e)
            {
                _parent.MasterImage_MouseLeftButtonUp(_parent.TestContentControl1, e);
            }

            protected override void OnMouseMove(MouseEventArgs e)
            {
                _parent.MasterImage_MouseMove(_parent.TestContentControl1, e);
            }

            protected override void OnMouseWheel(MouseWheelEventArgs e)
            {
                _parent.MasterImage_MouseWheel(_parent.TestContentControl1, e);
            }
        }
        #endregion

        #region -- DPI --
        private void GetDpi()
        {
            using (System.Drawing.Graphics g = System.Drawing.Graphics.FromHwnd(IntPtr.Zero))
            {
                _dpiX = g.DpiX;
                _dpiY = g.DpiY;
            }
        }

        private double Trans2RightPixelX(double basePixelX)
        {
            return _dpiX / 96d * basePixelX;
        }

        private double Trans2RightPixelY(double basePixelY)
        {
            return _dpiY / 96d * basePixelY;
        }

        protected void RefreshMonitorDpi()
        {
            try
            {
                // get the current DPI of the monitor of the window
                IntPtr monitor = DpiUtil.MonitorFromWindow(hwnd, DpiUtil.MONITOR_DEFAULTTONEAREST);
                uint _current_dpiX = 96;
                uint _current_dpiY = 96;
                DpiUtil.GetDpiForMonitor(monitor, (int)DpiUtil.DpiType.Effective, out _current_dpiX, out _current_dpiY);

                // update Width and Height based on the current DPI of the monitor
                UpdateWindowSize();

                _dpiX = _current_dpiX;
                _dpiY = _current_dpiY;
            }
            catch (DllNotFoundException e)
            {
                log.ErrorFormat("Can not load system dll file: {0}", e);
            }
        }

        private void UpdateWindowSize()
        {
            double old_Pos_Width = Math.Abs(this.Left) + this.Width;
            double old_Pos_Height = Math.Abs(this.Top) + this.Height;

            double new_Pos_Width = Math.Abs(this.Left) + this.Width;
            double new_Pos_Height = Math.Abs(this.Top) + this.Height;

            int _relScaleX = (int)(old_Pos_Width - new_Pos_Width);
            int _relScaleY = (int)(old_Pos_Height - new_Pos_Height);

            this.Left = this.Left + (_relScaleX / 2);
            this.Top = this.Top + (_relScaleY / 2);
        }
        #endregion
    }
}
