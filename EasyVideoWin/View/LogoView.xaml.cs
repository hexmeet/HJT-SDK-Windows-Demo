﻿using EasyVideoWin.Helpers;
using log4net;
using System;
using System.Collections.Generic;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace EasyVideoWin.View
{
    /// <summary>
    /// Interaction logic for LogoView.xaml
    /// </summary>
    public partial class LogoView : UserControl
    {
        private readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private Window _window;
        private bool _firstLoad;

        public LogoView()
        {
            InitializeComponent();

            this._firstLoad = true;

            ImageBrush b = new ImageBrush();
            b.ImageSource = new BitmapImage(new Uri("pack://application:,,,/Resources/Icons/logo_preview.png"));
            b.Stretch = Stretch.Fill;
            this.Background = b;

            this.Loaded += LogoView_Loaded;
        }

        private void LogoView_Loaded(object sender, RoutedEventArgs e)
        {
            if (this._firstLoad)
            {
                _window = Window.GetWindow(this);
                AdjustWindowSize();
                _window.SizeChanged += Window_SizeChanged;
                //VideoView.Instance.SetContentWindowHandle();
            }

            _firstLoad = false;
        }

        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            AdjustWindowSize();
        }

        private void AdjustWindowSize()
        {
            if (_window.ActualHeight < 0)
            {
                log.WarnFormat("Invalid _window.ActualHeight: {0}", _window.ActualHeight);
                return;
            }
            this.Height = _window.ActualHeight;
            Rect mainWindowRect = VideoPeopleWindow.Instance.GetWindowRect();
            double titlebarHeight = VideoPeopleWindow.Instance.FullScreenStatus ? 0 : VideoPeopleWindow.Instance.TitlebarHeight;
            if ((mainWindowRect.Width / (mainWindowRect.Height - titlebarHeight)) > (16.0 / 9.0))
            {
                double height = mainWindowRect.Height - titlebarHeight;
                if (height < 0)
                {
                    log.WarnFormat("Invalid height: {0}", height);
                    height = 720;
                }
                this.Height = height;
                this.Width = this.Height * 16.0 / 9.0;
            }
            else
            {
                double width = mainWindowRect.Width;
                if (width < 0)
                {
                    log.WarnFormat("Invalid width: {0}", width);
                    width = 1280;
                }
                this.Width = width;
                this.Height = this.Width * 9.0 / 16.0;
            }
            this.bgDecoration.Height = (mainWindowRect.Height - titlebarHeight - this.Height) / 2;
        }
    }
}
