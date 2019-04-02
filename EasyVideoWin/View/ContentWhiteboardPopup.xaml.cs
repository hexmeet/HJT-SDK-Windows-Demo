using EasyVideoWin.Helpers;
using System;
using System.Windows;
using System.Windows.Input;

namespace EasyVideoWin.View
{
    /// <summary>
    /// Interaction logic for ContentWhiteboardPopup.xaml
    /// </summary>
    public partial class ContentWhiteboardPopup : Window
    {
        public delegate void ListenerWhiteboardPopupMouseLeftButtonDownHandler();
        public event ListenerWhiteboardPopupMouseLeftButtonDownHandler WhiteboardPopupMouseLeftButtonDown = null;

        public ContentWhiteboardPopup()
        {
            InitializeComponent();
            this.Loaded += ContentWhiteboardPopup_Loaded;
        }

        private void ContentWhiteboardPopup_Loaded(object sender, RoutedEventArgs e)
        {
            Utils.SetSoftwareRender(this);
        }

        private void Window_OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            WhiteboardPopupMouseLeftButtonDown?.Invoke();
        }
    }
}
