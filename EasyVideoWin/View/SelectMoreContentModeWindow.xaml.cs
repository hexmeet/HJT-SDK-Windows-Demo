using EasyVideoWin.Helpers;
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
using System.Windows.Shapes;

namespace EasyVideoWin.View
{
    /// <summary>
    /// SelectMoreContentModeWindow.xaml 的交互逻辑
    /// </summary>
    public partial class SelectMoreContentModeWindow : Window
    {
        #region -- Members --

        private readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public delegate void ListenerSelectedMoreContentHandler(MoreContentMode mode);
        public event ListenerSelectedMoreContentHandler ListenerSelectedMoreContent = null;
        private const int INITIAL_HEIGHT = 100;
        private const int INITIAL_WIDTH = 140;
        private bool _showSwitch2AudioMode;
        private bool _showRequestSpeak;
        private bool _showChangeName;

        #endregion

        #region -- Properties --

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

        public Visibility Switch2AudioModeVisibility
        {
            get
            {
                return _showSwitch2AudioMode ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        public Visibility RequestSpeakVisibility
        {
            get
            {
                return _showRequestSpeak ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        public Visibility ChangeNameVisibility
        {
            get
            {
                return _showChangeName ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        #endregion

        #region -- Constructor --

        public SelectMoreContentModeWindow(bool showSwitch2AudioMode, bool showRequestSpeak, bool showChangeName)
        {
            InitializeComponent();

            _showSwitch2AudioMode = showSwitch2AudioMode;
            _showRequestSpeak = showRequestSpeak;
            _showChangeName = showChangeName;

            this.switch2AudioModeGrid.SetBinding(Button.VisibilityProperty, new Binding("Switch2AudioModeVisibility") { Source = this });
            this.requestSpeakGrid.SetBinding(Button.VisibilityProperty, new Binding("RequestSpeakVisibility") { Source = this });
            this.changeNameGrid.SetBinding(Button.VisibilityProperty, new Binding("ChangeNameVisibility") { Source = this });
        }

        #endregion

        #region -- Public Method --



        #endregion

        #region -- Private Method --

        private void Switch2AudioModeBtn_Click(object sender, MouseButtonEventArgs e)
        {
            ListenerSelectedMoreContent(MoreContentMode.Switch2AudioMode);
            this.Hide();
        }

        private void RequestSpeakBtn_Click(object sender, MouseButtonEventArgs e)
        {
            ListenerSelectedMoreContent(MoreContentMode.RequestSpeak);
            this.Hide();
        }
        
        private void ChangeNameBtn_Click(object sender, MouseButtonEventArgs e)
        {
            ListenerSelectedMoreContent(MoreContentMode.ChangeDisplayName);
            this.Hide();
        }

        private void MouseEnter_ItemLabel(object sender, MouseEventArgs e)
        {
            System.Windows.Controls.Label lbl = sender as System.Windows.Controls.Label;
            if (null == lbl)
            {
                return;
            }
            lbl.Background = Utils.SelectedBackGround;
        }

        private void MouseLeave_ItemLabel(object sender, MouseEventArgs e)
        {
            System.Windows.Controls.Label lbl = sender as System.Windows.Controls.Label;
            if (null == lbl)
            {
                return;
            }
            lbl.Background = Utils.DefaultBackGround;
        }

        #endregion

    }

    public enum MoreContentMode
    {
        Switch2AudioMode        = 0
        , RequestSpeak          = 1
        , ChangeDisplayName     = 2
    }
}
