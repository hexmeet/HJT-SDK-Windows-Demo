using EasyVideoWin.CustomControls;
using EasyVideoWin.Helpers;
using log4net;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace EasyVideoWin.View
{
    /// <summary>
    /// Interaction logic for MessageOverlayWindow.xaml
    /// </summary>
    public partial class MessageOverlayWindow : CustomShowHideBaseWindow, INotifyPropertyChanged
    {
        #region -- Constructor --

        private readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public event PropertyChangedEventHandler PropertyChanged;

        private const int INITIAL_HEIGHT = 50;

        private double _rollSpeedMargin;
        #endregion

        #region -- Properties --

        public string ContentText
        {
            get
            {
                return this.contentTextBlock.Text;
            }
            set
            {
                this.contentTextBlock.Text = value;
            }
        }

        public int InitialHeight
        {
            get
            {
                return INITIAL_HEIGHT;
            }
        }

        #endregion

        #region -- Constructor --

        public MessageOverlayWindow()
        {
            InitializeComponent();
            
            this.Height = INITIAL_HEIGHT;
            this.Loaded += MessageOverlayWindow_Loaded;
            this.contentTextBlock.SizeChanged += ContentTextBlock_SizeChanged;
        }
        
        #endregion

        #region -- Public Methods --

        public void SetFontSize(double fontSize)
        {
            this.contentTextBlock.FontSize = fontSize * 2;
        }

        public void SetTextMargin(Thickness thickness)
        {
        //    this.cellName.Margin = thickness;
        }

        public void SetBackground(string background)
        {
            this.backgroundDockPanel.Background = new SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString(background));
        }

        public void SetForeground(string foreground)
        {
            this.contentTextBlock.Foreground = new SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString(foreground));
        }

        public void SetTransparency(int transparency)
        {
            this.backgroundDockPanel.Opacity = 1 - (double)transparency / 100.0;
        }

        public bool RollContent()
        {
            double left = this.contentTextBlock.Margin.Left - _rollSpeedMargin;
            //if (   (left > 0 && left > (this.contentDockPanel.ActualWidth - this.contentTextBlock.ActualWidth))
            //    || left < -this.contentTextBlock.ActualWidth)
            {
                // adjust the value below to make constant speed
                left += _rollSpeedMargin / 2;
            }

            if (this.contentTextBlock.ActualWidth > this.contentDockPanel.ActualWidth)
            {
                if (left < 0 && -left > (this.contentTextBlock.ActualWidth - this.contentDockPanel.ActualWidth) && -left < (this.contentTextBlock.ActualWidth))
                {
                    // adjust the value below to make constant speed when roll into the middle
                    left -= _rollSpeedMargin / 2;
                }
            }
            else
            {
                if (
                       (left > 0 && left < (this.contentDockPanel.ActualWidth - this.contentTextBlock.ActualWidth))
                    || (left < 0 && -left < this.contentTextBlock.ActualWidth)
                )
                {
                    left -= _rollSpeedMargin / 2;
                }
            }

            this.contentTextBlock.Margin = new Thickness(left, 0, 0, 0);
            double distance = distance = this.contentDockPanel.ActualWidth / 2 + this.contentTextBlock.ActualWidth;
            if (left < 0 && left < -distance)
            {
                // roll to end
                return false;
            }

            return true;
        }

        public void ResetRollContentInitialPos()
        {
            this.contentTextBlock.Margin = new Thickness(this.Width, 0, 0, 0);
        }

        public void SetContentTextBlock2DefaultPos()
        {
            this.contentTextBlock.Margin = new Thickness(0, 0, 0, 0);
        }

        public void SetRollSpeed(int speed)
        {
            switch (speed)
            {
                case 10:
                    _rollSpeedMargin = 26;
                    break;
                case 9:
                    _rollSpeedMargin = 13;
                    break;
                case 8:
                    _rollSpeedMargin = 8;
                    break;
                case 7:
                    _rollSpeedMargin = 6;
                    break;
                case 6:
                    _rollSpeedMargin = 4.8;
                    break;
                case 5:
                    _rollSpeedMargin = 4;
                    break;
                case 4:
                    _rollSpeedMargin = 3.5;
                    break;
                case 3:
                    _rollSpeedMargin = 3.1;
                    break;
                case 2:
                    _rollSpeedMargin = 2.6;
                    break;
                case 1:
                    _rollSpeedMargin = 2.4;
                    break;
                default:
                    break;

            }
            
        }

        #endregion

        #region -- Private Methods --

        private void ContentTextBlock_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (e.HeightChanged)
            {
                this.Height = e.NewSize.Height;
            }
        }

        private void MessageOverlayWindow_Loaded(object sender, RoutedEventArgs e)
        {
            Utils.HideWindowInAltTab(this.Handle);
        }

        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        override protected void OnWindowSizeChanged(double width, double height)
        {
            this.backgroundDockPanel.Width = this.Width;
            this.contentDockPanel.Width = this.Width;
            this.contentDockPanel.Margin = new Thickness(-width, 0, 0, 0);
        }

        #endregion
    }
}
