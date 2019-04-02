using log4net;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace EasyVideoWin.View
{
    /// <summary>
    /// Interaction logic for BrushHighlighterWindow.xaml
    /// </summary>
    public partial class BrushHighlighterWindow : Window
    {
        private readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private int _color = (Int16)PenColor.Yellow;
        private int _penSize = 3;

        public delegate void ListenerHighlighterHandler(int penTypeSel, int colorTypeSel);
        public event ListenerHighlighterHandler ListenerHighlighter = null;

        public BrushHighlighterWindow()
        {
            InitializeComponent();
        }

        private void Color_Click(object sender, MouseButtonEventArgs e)
        {
            setColorToDefault();
            Grid grid = (Grid)sender;
            switch (grid.Name)
            {
                case "yellow":
                    color_yellow.Visibility = Visibility.Collapsed;
                    colorSelect_yellow.Visibility = Visibility.Visible;
                    _color = (Int16)PenColor.Yellow;
                    break;
                case "green":
                    color_green.Visibility = Visibility.Collapsed;
                    colorSelect_green.Visibility = Visibility.Visible;
                    _color = (Int16)PenColor.Green;
                    break;
                case "lake":
                    color_lake.Visibility = Visibility.Collapsed;
                    colorSelect_lake.Visibility = Visibility.Visible;
                    _color = (Int16)PenColor.Lake;
                    break;
                case "purple":
                    color_purple.Visibility = Visibility.Collapsed;
                    colorSelect_purple.Visibility = Visibility.Visible;
                    _color = (Int16)PenColor.Purple;
                    break;
                case "orange":
                    color_orange.Visibility = Visibility.Collapsed;
                    colorSelect_orange.Visibility = Visibility.Visible;
                    _color = (Int16)PenColor.Orange;
                    break;
                case "light_green":
                    color_light_green.Visibility = Visibility.Collapsed;
                    colorSelect_light_green.Visibility = Visibility.Visible;
                    _color = (Int16)PenColor.Light_Green;
                    break;
                case "light_blue":
                    color_light_blue.Visibility = Visibility.Collapsed;
                    colorSelect_light_blue.Visibility = Visibility.Visible;
                    _color = (Int16)PenColor.Light_Blue;
                    break;
                case "pink":
                    color_pink.Visibility = Visibility.Collapsed;
                    colorSelect_pink.Visibility = Visibility.Visible;
                    _color = (Int16)PenColor.Pink;
                    break;
                default:
                    color_yellow.Visibility = Visibility.Collapsed;
                    colorSelect_yellow.Visibility = Visibility.Visible;
                    _color = (Int16)PenColor.Yellow;
                    break;
            }
            log.DebugFormat("Selected Highlighter Color: {0}, value: {1}, pen size: {2}", grid.Name, _color, _penSize.ToString());

            ListenerHighlighter?.Invoke(_penSize, _color);
        }

        private void setColorToDefault()
        {
            color_yellow.Visibility = Visibility.Visible;
            color_purple.Visibility = Visibility.Visible;
            color_lake.Visibility = Visibility.Visible;
            color_green.Visibility = Visibility.Visible;
            color_orange.Visibility = Visibility.Visible;
            color_light_green.Visibility = Visibility.Visible;
            color_light_blue.Visibility = Visibility.Visible;
            color_pink.Visibility = Visibility.Visible;

            colorSelect_yellow.Visibility = Visibility.Collapsed;
            colorSelect_purple.Visibility = Visibility.Collapsed;
            colorSelect_lake.Visibility = Visibility.Collapsed;
            colorSelect_green.Visibility = Visibility.Collapsed;
            colorSelect_orange.Visibility = Visibility.Collapsed;
            colorSelect_light_green.Visibility = Visibility.Collapsed;
            colorSelect_light_blue.Visibility = Visibility.Collapsed;
            colorSelect_pink.Visibility = Visibility.Collapsed;
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
            log.DebugFormat("Selected Highlighter pen size: {0}, color: {1}", _penSize.ToString(), _color);
            ListenerHighlighter?.Invoke(_penSize, _color);
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
                case (Int16)PenColor.Yellow:
                    color_yellow.Visibility = Visibility.Collapsed;
                    colorSelect_yellow.Visibility = Visibility.Visible;
                    break;
                case (Int16)PenColor.Purple:
                    color_purple.Visibility = Visibility.Collapsed;
                    colorSelect_purple.Visibility = Visibility.Visible;
                    break;
                case (Int16)PenColor.Lake:
                    color_lake.Visibility = Visibility.Collapsed;
                    colorSelect_lake.Visibility = Visibility.Visible;
                    break;
                case (Int16)PenColor.Green:
                    color_green.Visibility = Visibility.Collapsed;
                    colorSelect_green.Visibility = Visibility.Visible;
                    break;
                case (Int16)PenColor.Orange:
                    color_orange.Visibility = Visibility.Collapsed;
                    colorSelect_orange.Visibility = Visibility.Visible;
                    break;
                case (Int16)PenColor.Light_Green:
                    color_light_green.Visibility = Visibility.Collapsed;
                    colorSelect_light_green.Visibility = Visibility.Visible;
                    break;
                case (Int16)PenColor.Light_Blue:
                    color_light_blue.Visibility = Visibility.Collapsed;
                    colorSelect_light_blue.Visibility = Visibility.Visible;
                    break;
                case (Int16)PenColor.Pink:
                    color_pink.Visibility = Visibility.Collapsed;
                    colorSelect_pink.Visibility = Visibility.Visible;
                    break;
                default:
                    color_yellow.Visibility = Visibility.Collapsed;
                    colorSelect_yellow.Visibility = Visibility.Visible;
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
}
