using EasyVideoWin.Helpers;
using EasyVideoWin.Model;
using log4net;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace EasyVideoWin.View
{
    /// <summary>
    /// Interaction logic for ScreenSelectionView.xaml
    /// </summary>
    public partial class ScreenSelectionView : Window
    {
        private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private List<Screen> _screenList = new List<Screen>();

        private int screenSelIndex = 0;

        public Screen SelectedScreen { get; set; }
        private Brush _defaultBackGround = new SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#f4f4f4"));
        private Brush _selectedBorderGround = new SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#589bf9"));
        private Brush _selectedBackGround = new SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#313131"));

        public ScreenSelectionView()
        {
            InitializeComponent();

            //initScreen();
            //SetImgSources();

        }

        private void SetImgSources(Screen tmpScreen, int index)
        {
            log.InfoFormat("Create screen image, index:{0}", index);
            string curTmpPicPath = Utils.GetCurPicTmpPath();
            curTmpPicPath += "tmp"+ index.ToString() + ".png";
            
            int result = MonitorCapture.SaveMonitorPic(tmpScreen, curTmpPicPath);

            switch (index)
            {
                case 0:
                    {
                        //screen0Img.Source = new BitmapImage(new Uri(curTmpPicPath, UriKind.Absolute));
                        screen0Img.Source = GetBitmapImage(curTmpPicPath);
                    }
                    break;
                case 1:
                    {
                        screen1Img.Source = GetBitmapImage(curTmpPicPath);
                    }
                    break;
                case 2:
                    {
                        screen2Img.Source = GetBitmapImage(curTmpPicPath);
                    }
                    break;
                case 3:
                    {
                        //screen3Img.Source = GetBitmapImage(curTmpPicPath);
                    }
                    break;
                case 4:
                    {
                        //screen4Img.Source = GetBitmapImage(curTmpPicPath);
                    }
                    break;
                case 5:
                    {
                        //screen5Img.Source = GetBitmapImage(curTmpPicPath);
                    }
                    break;
                default:
                    {
                        //screen1Img.Source = new BitmapImage(new Uri(curTmpPicPath, UriKind.Absolute));
                    }
                    break;
            }
            
        }

       public void InitScreen()
        {
            screen00.Visibility = Visibility.Hidden;
            screen01.Visibility = Visibility.Hidden;
            screen02.Visibility = Visibility.Hidden;
            //screen03.Visibility = Visibility.Hidden;
            //screen04.Visibility = Visibility.Hidden;
            //screen05.Visibility = Visibility.Hidden;
            screenSelIndex = 0;
            int screenNum = getAllScreens();

            _screenList = new List<Screen>();

            if (screenNum > 0)
            {
                for (int i = 0; i < screenNum; i++)
                {
                    Screen s = Screen.AllScreens[i];
                    //Window win = Window.GetWindow(this);
                    
                    _screenList.Add(Screen.AllScreens[i]);
                    SetImgSources(s,i);
                }

                if(screenNum == 1)
                {
                    screen00.Visibility = Visibility.Visible;
                }
                else if (screenNum == 2)
                {
                    screen00.Visibility = Visibility.Visible;
                    screen01.Visibility = Visibility.Visible;
                }
                else if (screenNum == 3)
                {
                    screen00.Visibility = Visibility.Visible;
                    screen01.Visibility = Visibility.Visible;
                    screen02.Visibility = Visibility.Visible;
                }
                //else if (screenNum == 4)
                //{
                //    screen00.Visibility = Visibility.Visible;
                //    screen01.Visibility = Visibility.Visible;
                //    screen02.Visibility = Visibility.Visible;
                //    screen03.Visibility = Visibility.Visible;
                //}
                //else if (screenNum == 5)
                //{
                //    screen00.Visibility = Visibility.Visible;
                //    screen01.Visibility = Visibility.Visible;
                //    screen02.Visibility = Visibility.Visible;
                //    screen03.Visibility = Visibility.Visible;
                //    screen04.Visibility = Visibility.Visible;
                //}
                //else if (screenNum > 5)
                //{
                //    screen00.Visibility = Visibility.Visible;
                //    screen01.Visibility = Visibility.Visible;
                //    screen02.Visibility = Visibility.Visible;
                //    screen03.Visibility = Visibility.Visible;
                //    screen04.Visibility = Visibility.Visible;
                //    screen05.Visibility = Visibility.Visible;
                //}
            }
        }

        private int getAllScreens()
        {
            int screenNumber = Screen.AllScreens.Length;
            if(screenNumber > 3)
            {
                screenNumber = 3;
            }
            return screenNumber;
        }

       

        private void BackGround_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                this.DragMove();
                //Window.MouseMoveEvent();
            }
        }

        private void SaveBtn_Click(object sender, RoutedEventArgs e)
        {
            //ScreenWaitingView waitingView = new ScreenWaitingView();
            //waitingView.Show();
            //waitingView.SetScreenSelected(_screenList[screenSelIndex]);
            SelectedScreen = _screenList[screenSelIndex];
            this.Close();
        }

       

        private void CloseBtn_Click(object sender, RoutedEventArgs e)
        {
            SelectedScreen = null;
            this.Close();
        }

        private void camera1Check_Click(object sender, MouseButtonEventArgs e)
        {
            grid0.BorderBrush = _selectedBorderGround;
            grid0.Background = _selectedBackGround;
            grid1.BorderBrush = _defaultBackGround;
            grid1.Background = _defaultBackGround;
            grid2.BorderBrush = _defaultBackGround;
            grid2.Background = _defaultBackGround;
            //grid3.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#f4f4f4"));
            //grid4.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#f4f4f4"));
            //grid5.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#f4f4f4"));
            screenSelIndex = 0;
        }

        private void camera2Check_Click(object sender, MouseButtonEventArgs e)
        {
            grid0.BorderBrush = _defaultBackGround;
            grid0.Background = _defaultBackGround;
            grid1.BorderBrush = _selectedBorderGround;
            grid1.Background = _selectedBackGround;
            grid2.BorderBrush = _defaultBackGround;
            grid0.Background = _defaultBackGround;
            grid2.Background = _defaultBackGround;
            //grid3.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#f4f4f4"));
            //grid4.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#f4f4f4"));
            //grid5.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#f4f4f4"));
            screenSelIndex = 1;
        }

        private void camera3Check_Click(object sender, MouseButtonEventArgs e)
        {
            grid0.BorderBrush = _defaultBackGround;
            grid0.Background = _defaultBackGround;
            grid1.BorderBrush = _defaultBackGround;
            grid1.Background = _defaultBackGround;
            grid2.BorderBrush = _selectedBorderGround;
            grid2.Background = _selectedBackGround;
            //grid3.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#f4f4f4"));
            //grid4.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#f4f4f4"));
            //grid5.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#f4f4f4"));
            screenSelIndex = 2;
        }

        //private void camera4Check_Click(object sender, MouseButtonEventArgs e)
        //{
        //    grid0.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#f4f4f4"));
        //    grid1.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#f4f4f4"));
        //    grid2.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#f4f4f4"));
        //    grid3.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#515151"));
        //    grid4.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#f4f4f4"));
        //    grid5.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#f4f4f4"));
        //    screenSelIndex = 3;
        //}

        //private void camera5Check_Click(object sender, MouseButtonEventArgs e)
        //{
        //    grid0.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#f4f4f4"));
        //    grid1.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#f4f4f4"));
        //    grid2.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#f4f4f4"));
        //    grid3.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#f4f4f4"));
        //    grid4.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#515151"));
        //    grid5.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#f4f4f4"));
        //    screenSelIndex = 4;
        //}

        //private void camera6Check_Click(object sender, MouseButtonEventArgs e)
        //{
        //    grid0.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#f4f4f4"));
        //    grid1.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#f4f4f4"));
        //    grid2.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#f4f4f4"));
        //    grid3.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#f4f4f4"));
        //    grid4.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#f4f4f4"));
        //    grid5.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#515151"));
        //    screenSelIndex = 5;
        //}

        public static BitmapImage GetBitmapImage(string path)
        {
            BitmapImage bitmap = new BitmapImage();
            bitmap.BeginInit();

            bitmap.CacheOption = BitmapCacheOption.OnLoad;
            bitmap.StreamSource = new System.IO.MemoryStream(File.ReadAllBytes(path));
            bitmap.EndInit();
            bitmap.Freeze();
            return bitmap;
        }
    }
}
