using EasyVideoWin.Helpers;
using log4net;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace EasyVideoWin.View
{
    /// <summary>
    /// Interaction logic for BrushSelectWindow.xaml
    /// </summary>
    public partial class BrushSelectWindow : Window
    {
        private readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private int _color = (Int16)PenColor.Black;
        private int _penSize = 1;

        public delegate void ListenerBrushHandler(string penTypeSel, int colorTypeSel);
        public event ListenerBrushHandler ListenerBrush = null;

        public BrushSelectWindow()
        {
            InitializeComponent();
        }
        
        private void Color_Click(object sender, MouseButtonEventArgs e)
        {
            setColorToDefault();
            Grid grid = (Grid)sender;
            switch(grid.Name)
            {
                case "black":
                    color_black.Visibility = Visibility.Collapsed;
                    colorSelect_black.Visibility = Visibility.Visible;
                    _color = (Int16)PenColor.Black;
                    break;
                case "white":
                    color_white.Visibility = Visibility.Collapsed;
                    colorSelect_white.Visibility = Visibility.Visible;
                    _color = (Int16)PenColor.White;
                    break;
                case "blue":
                    color_blue.Visibility = Visibility.Collapsed;
                    colorSelect_blue.Visibility = Visibility.Visible;
                    _color = (Int16)PenColor.Blue;
                    break;
                case "green":
                    color_green.Visibility = Visibility.Collapsed;
                    colorSelect_green.Visibility = Visibility.Visible;
                    _color = (Int16)PenColor.Green;
                    break;
                case "yellow":
                    color_yellow.Visibility = Visibility.Collapsed;
                    colorSelect_yellow.Visibility = Visibility.Visible;
                    _color = (Int16)PenColor.Yellow;
                    break;
                case "orange":
                    color_orange.Visibility = Visibility.Collapsed;
                    colorSelect_orange.Visibility = Visibility.Visible;
                    _color = (Int16)PenColor.Orange;
                    break;
                case "red":
                    color_red.Visibility = Visibility.Collapsed;
                    colorSelect_red.Visibility = Visibility.Visible;
                    _color = (Int16)PenColor.Red;
                    break;
                case "purple":
                    color_purple.Visibility = Visibility.Collapsed;
                    colorSelect_purple.Visibility = Visibility.Visible;
                    _color = (Int16)PenColor.Purple;
                    break;
                default:
                    color_black.Visibility = Visibility.Collapsed;
                    colorSelect_black.Visibility = Visibility.Visible;
                    _color = (Int16)PenColor.Black;
                    break;
            }
            log.DebugFormat("Selected Color: {0}, value: {1}, pen size: {2}", grid.Name, _color, _penSize.ToString());

            ListenerBrush?.Invoke(_penSize.ToString(), _color);
        }

        private void setColorToDefault()
        {
            color_black.Visibility = Visibility.Visible;
            color_white.Visibility = Visibility.Visible;
            color_blue.Visibility = Visibility.Visible;
            color_green.Visibility = Visibility.Visible;
            color_yellow.Visibility = Visibility.Visible;
            color_orange.Visibility = Visibility.Visible;
            color_red.Visibility = Visibility.Visible;
            color_purple.Visibility = Visibility.Visible;

            colorSelect_black.Visibility = Visibility.Collapsed;
            colorSelect_white.Visibility = Visibility.Collapsed;
            colorSelect_blue.Visibility = Visibility.Collapsed;
            colorSelect_green.Visibility = Visibility.Collapsed;
            colorSelect_yellow.Visibility = Visibility.Collapsed;
            colorSelect_orange.Visibility = Visibility.Collapsed;
            colorSelect_red.Visibility = Visibility.Collapsed;
            colorSelect_purple.Visibility = Visibility.Collapsed;
        }

        private void Pensize_Click(object sender, MouseButtonEventArgs e)
        {
            setPensizeToDefault();
            Grid grid = (Grid)sender;
            switch (grid.Name)
            {
                case "size1":
                    size_1.Visibility = Visibility.Collapsed;
                    sizeSelect_1.Visibility = Visibility.Visible;
                    _penSize = 1;
                    break;
                case "size2":
                    size_2.Visibility = Visibility.Collapsed;
                    sizeSelect_2.Visibility = Visibility.Visible;
                    _penSize = 2;
                    break;
                case "size3":
                    size_3.Visibility = Visibility.Collapsed;
                    sizeSelect_3.Visibility = Visibility.Visible;
                    _penSize = 3;
                    break;
                case "size4":
                    size_4.Visibility = Visibility.Collapsed;
                    sizeSelect_4.Visibility = Visibility.Visible;
                    _penSize = 4;
                    break;
                default:
                    size_1.Visibility = Visibility.Collapsed;
                    sizeSelect_1.Visibility = Visibility.Visible;
                    _penSize = 1;
                    break;
            }
            log.DebugFormat("Selected pen size: {0}, color: {1}", _penSize.ToString(), _color);
            Utils.savePenTypeSelection(_penSize.ToString());
            ListenerBrush?.Invoke(_penSize.ToString(), _color);
        }

        private void setPensizeToDefault()
        {
            size_1.Visibility = Visibility.Visible;
            size_2.Visibility = Visibility.Visible;
            size_3.Visibility = Visibility.Visible;
            size_4.Visibility = Visibility.Visible;

            sizeSelect_1.Visibility = Visibility.Collapsed;
            sizeSelect_2.Visibility = Visibility.Collapsed;
            sizeSelect_3.Visibility = Visibility.Collapsed;
            sizeSelect_4.Visibility = Visibility.Collapsed;
        }

        public void initColor(int color)
        {
            switch (color)
            {
                case (Int16)PenColor.Black:
                    color_black.Visibility = Visibility.Collapsed;
                    colorSelect_black.Visibility = Visibility.Visible;
                    break;
                case (Int16)PenColor.White:
                    color_white.Visibility = Visibility.Collapsed;
                    colorSelect_white.Visibility = Visibility.Visible;
                    break;
                case (Int16)PenColor.Blue:
                    color_blue.Visibility = Visibility.Collapsed;
                    colorSelect_blue.Visibility = Visibility.Visible;
                    break;
                case (Int16)PenColor.Green:
                    color_green.Visibility = Visibility.Collapsed;
                    colorSelect_green.Visibility = Visibility.Visible;
                    break;
                case (Int16)PenColor.Yellow:
                    color_yellow.Visibility = Visibility.Collapsed;
                    colorSelect_yellow.Visibility = Visibility.Visible;
                    break;
                case (Int16)PenColor.Orange:
                    color_orange.Visibility = Visibility.Collapsed;
                    colorSelect_orange.Visibility = Visibility.Visible;
                    break;
                case (Int16)PenColor.Red:
                    color_red.Visibility = Visibility.Collapsed;
                    colorSelect_red.Visibility = Visibility.Visible;
                    break;
                case (Int16)PenColor.Purple:
                    color_purple.Visibility = Visibility.Collapsed;
                    colorSelect_purple.Visibility = Visibility.Visible;
                    break;
                default:
                    color_black.Visibility = Visibility.Collapsed;
                    colorSelect_black.Visibility = Visibility.Visible;
                    break;
            }
        }

        public void initSize(int size)
        {
            switch (size)
            {
                case 1:
                    size_1.Visibility = Visibility.Collapsed;
                    sizeSelect_1.Visibility = Visibility.Visible;
                    break;
                case 2:
                    size_2.Visibility = Visibility.Collapsed;
                    sizeSelect_2.Visibility = Visibility.Visible;
                    break;
                case 3:
                    size_3.Visibility = Visibility.Collapsed;
                    sizeSelect_3.Visibility = Visibility.Visible;
                    break;
                case 4:
                    size_4.Visibility = Visibility.Collapsed;
                    sizeSelect_4.Visibility = Visibility.Visible;
                    break;
                default:
                    size_1.Visibility = Visibility.Collapsed;
                    sizeSelect_1.Visibility = Visibility.Visible;
                    break;
            }
        }
    }

    public enum PenColor
    {
        Black = 0,
        White = 1,
        Blue = 2,
        Green = 3,
        Yellow = 4,
        Orange = 5,
        Red = 6,
        Purple = 7,
        Lake = 8,
        Pink = 9,
        Light_Blue = 10,
        Light_Green = 11,
    }
}
