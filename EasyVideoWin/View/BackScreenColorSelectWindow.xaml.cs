using EasyVideoWin.CustomControls;
using EasyVideoWin.Helpers;
using log4net;
using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace EasyVideoWin.View
{
    /// <summary>
    /// Interaction logic for BackScreenColorSelectWindow.xaml
    /// </summary>
    public partial class BackScreenColorSelectWindow : Window
    {
        #region -- Members --
        private readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private int _backScreenType = (Int16)BackScreenColorEnum.Transparency;

        public static System.Windows.Media.Brush DefaultBackGround = new SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#E5000000"));
        private Brush _selectedForeGround = new SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#ffffff"));
        private Brush _selectedBackGround = new SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#2f2f2f"));

        public delegate void ListenerBackGroundSelectedHandler(int type);
        public event ListenerBackGroundSelectedHandler ListenerBackGroundSelected = null;

        public delegate void ListenerUploadBackGroundHandler(string filePath);
        public event ListenerUploadBackGroundHandler ListenerUploadBackGround = null;

        public ShareContentMode ToolbarInContentMode { set; get; }

        #endregion

        #region -- Properties --

        #endregion

        #region -- Constructors --
        public BackScreenColorSelectWindow()
        {
            try
            {
                InitializeComponent();
            }
            catch (Exception ex)
            {
                log.DebugFormat("Failed to init back screen select window, error message: {0}", ex.Message);
            }
        }
        #endregion

        #region -- Public Methods --

        public void InitData()
        {
            if(ToolbarInContentMode == ShareContentMode.Whiteboard)
            {
                local_selected.Visibility = Visibility.Visible;
                null_selected.Visibility = Visibility.Collapsed;
            }
            string backcolor = Utils.getBackColorSelection();
            _backScreenType = backcolor == "" ? 0 : Convert.ToInt32(backcolor);
            SetColor(_backScreenType);
            
            this.Topmost = true;
        }

        public int GetBackScreenType()
        {
            return _backScreenType;
        }
        #endregion
        //private void BackGround_MouseMove(object sender, MouseEventArgs e)
        //{
        //    if (e.LeftButton == MouseButtonState.Pressed)
        //    {
        //        this.DragMove();
        //        //Window.MouseMoveEvent();
        //    }
        //}

        #region -- Private Methods --
        private void BlackColor_Click(object sender, MouseButtonEventArgs e)
        {
            _backScreenType = (Int16)BackScreenColorEnum.Black;
            SetColor(_backScreenType);
            log.DebugFormat("Selected back screen Color: {0}, value: {1}", "black", _backScreenType);
            Utils.saveBackColorTypeSelection(_backScreenType.ToString());

            //this.Close();
            ListenerBackGroundSelected?.Invoke(_backScreenType);
            this.Hide();
        }

        private void WhiteColor_Click(object sender, MouseButtonEventArgs e)
        {
            _backScreenType = (Int16)BackScreenColorEnum.White;
            SetColor(_backScreenType);
            log.DebugFormat("Selected back screen Color: {0}, value: {1}", "white", _backScreenType);
            Utils.saveBackColorTypeSelection(_backScreenType.ToString());

            //this.Close();
            ListenerBackGroundSelected?.Invoke(_backScreenType);
            this.Hide();
        }

        private void NullColor_Click(object sender, MouseButtonEventArgs e)
        {
            _backScreenType = (Int16)BackScreenColorEnum.Transparency;
            SetColor(_backScreenType);
            log.DebugFormat("Selected back screen Color: {0}, value: {1}", "default color", _backScreenType);
            Utils.saveBackColorTypeSelection(_backScreenType.ToString());

            //this.Close();
            ListenerBackGroundSelected?.Invoke(_backScreenType);
            this.Hide();
        }

        private void LocalColor_Click(object sender, MouseButtonEventArgs e)
        {
            log.DebugFormat("Load upload back ground image window");
            this.Hide();
            Microsoft.Win32.OpenFileDialog dialog = new Microsoft.Win32.OpenFileDialog();
            dialog.InitialDirectory = "C:\\";
            dialog.Filter = "(*.jpg;*.png)|*.jpg;*.png";
            if (dialog.ShowDialog() == true)
            {
                string fullFileName = dialog.FileName;
                //double fileSize = Utils.GetFileSize(fullFileName);
                //if (fileSize > 1024 * 5)
                //{
                //    MessageBoxTip tip = new MessageBoxTip();
                //    tip.Owner = System.Windows.Application.Current.MainWindow;
                //    tip.SetTitleAndMsg(LanguageUtil.Instance.GetValueByKey("PROMPT"), LanguageUtil.Instance.GetValueByKey("PIC_TOO_LARGE"), LanguageUtil.Instance.GetValueByKey("CONFIRM"));
                //    tip.ShowDialog();
                //    return;
                //}

                if (!string.IsNullOrEmpty(fullFileName))
                {
                    ListenerUploadBackGround?.Invoke(fullFileName);
                }
            }
        }

        private void SetColor(int color)
        {
            switch (color)
            {
                case (Int16)BackScreenColorEnum.Black:
                    black_selected.Background = _selectedBackGround;
                    white_selected.Background = DefaultBackGround;
                    null_selected.Background = DefaultBackGround;

                    Black_Text.Foreground = _selectedForeGround;
                    White_Text.Foreground = Utils.DefaultForeGround;
                    Transpanrent_Text.Foreground = Utils.DefaultForeGround;
                    break;

                case (Int16)BackScreenColorEnum.White:
                    black_selected.Background = DefaultBackGround;
                    white_selected.Background = _selectedBackGround;
                    null_selected.Background = DefaultBackGround;

                    Black_Text.Foreground = Utils.DefaultForeGround;
                    White_Text.Foreground = _selectedForeGround;
                    Transpanrent_Text.Foreground = Utils.DefaultForeGround;
                    break;

                case (Int16)BackScreenColorEnum.Transparency:
                    black_selected.Background = DefaultBackGround;
                    white_selected.Background = DefaultBackGround;
                    null_selected.Background = _selectedBackGround;

                    Black_Text.Foreground = Utils.DefaultForeGround;
                    White_Text.Foreground = Utils.DefaultForeGround;
                    Transpanrent_Text.Foreground = _selectedForeGround;
                    break;

                default:
                    black_selected.Background = DefaultBackGround;
                    white_selected.Background = DefaultBackGround;
                    null_selected.Background = _selectedBackGround;

                    Black_Text.Foreground = Utils.DefaultForeGround;
                    White_Text.Foreground = Utils.DefaultForeGround;
                    Transpanrent_Text.Foreground = _selectedForeGround;
                    break;
            }
        }
        #endregion
    }

    public enum BackScreenColorEnum
    {
        Black = 0,
        White = 1,
        Transparency = 2,
        Local = 3,
    }
}
