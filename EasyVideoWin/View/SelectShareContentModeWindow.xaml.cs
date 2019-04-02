using EasyVideoWin.Helpers;
using EasyVideoWin.Model;
using EasyVideoWin.ViewModel;
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
using System.Windows.Shapes;

namespace EasyVideoWin.View
{
    /// <summary>
    /// Interaction logic for SelectShareContentModeWindow.xaml
    /// </summary>
    public partial class SelectShareContentModeWindow : Window
    {
        public delegate void ListenerSelectedShareContentHandler(ShareContentMode mode);
        public event ListenerSelectedShareContentHandler ListenerSelectedShareContent = null;
        private SelectShareContentModeWindowViewModel _viewModel = null;
        private const int INITIAL_HEIGHT = 100;
        private const int INITIAL_WIDTH = 136;

        public int InitialHeight
        {
            get
            {
                return INITIAL_HEIGHT;
            }
        }

        public int InitialWidth
        {
            get
            {
                return INITIAL_WIDTH;
            }
        }

        public SelectShareContentModeWindow()
        {
            InitializeComponent();
            _viewModel = this.DataContext as SelectShareContentModeWindowViewModel;

        //    CallController.Instance.CallStatusChanged += OnCallStatusChanged;           
        }

        private void InteractionWhiteboardBtn_Click(object sender, MouseButtonEventArgs e)
        {
            ListenerSelectedShareContent(ShareContentMode.Whiteboard);
            this.Hide();
        }

        private void ScreenShareBtn_Click(object sender, MouseButtonEventArgs e)
        {
            ListenerSelectedShareContent(ShareContentMode.ScreenShare);
            this.Hide();
        }

        //private void OnCallStatusChanged(object sender, CallStatus status)
        //{
        //    if (status == CallStatus.Ended)
        //    {
        //        App.Current.Dispatcher.Invoke(() =>
        //        {
        //            this.Close();
        //        });
        //    }
        //}

        private void Window_MouseMove_Whiteboard(object sender, MouseEventArgs e)
        {
            this.InteractionWhiteboard.Background = Utils.SelectedBackGround;
        }

        private void Window_MouseMove_ScreenShare(object sender, MouseEventArgs e)
        {
            this.ScreenShare.Background = Utils.SelectedBackGround;
        }

        private void Window_MouseLeave_Whiteboard(object sender, MouseEventArgs e)
        {
            this.InteractionWhiteboard.Background = Utils.DefaultBackGround;
        }

        private void Window_MouseLeave_ScreenShare(object sender, MouseEventArgs e)
        {
            this.ScreenShare.Background = Utils.DefaultBackGround;
        }        
    }

    public enum ShareContentMode
    {
        Whiteboard = 1,
        ScreenShare = 2
    }
}
