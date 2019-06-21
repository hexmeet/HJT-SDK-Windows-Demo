using EasyVideoWin.Helpers;
using log4net;
using System;
using System.Windows;
using System.Windows.Input;
using System.ComponentModel;

namespace EasyVideoWin.CustomControls
{
    /// <summary>
    /// Interaction logic for MessageBoxEx.xaml
    /// </summary>
    public partial class MessageBoxTip : Window
    {
        private readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private bool _resize = true;
        private IMasterDisplayWindow _backgroundWindow;

        public MessageBoxTip()
        {
            InitializeComponent();
        }
        
        public MessageBoxTip(IMasterDisplayWindow backgroundWindow) : this()
        {
            _backgroundWindow = backgroundWindow;
            if (null != backgroundWindow)
            {
                this.Owner = backgroundWindow.GetWindow();
            }
        }

        public MessageBoxTip(IMasterDisplayWindow backgroundWindow, Window owner) : this()
        {
            _backgroundWindow = backgroundWindow;
            if (null != owner)
            {
                this.Owner = owner;
            }
        }
        
        public void isResize(bool value)
        {
            _resize = value;
        }

        public void SetTitleAndMsg(string title, string msg, string btn)
        {
            TitleText.Text = title;
            MessageText.Text = msg.Replace($"\\n", "\n");
            ButtonText.Text = btn;
        }

        private void OnClick_Close(object sender, MouseButtonEventArgs e)
        {
            this.Close();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (_resize)
            {
                Rect rect = MessageBoxUtil.GetOwnerWindowRect(this, _backgroundWindow);
                if(0 != rect.Width)
                {
                    this.Width = rect.Width;
                    this.Height = rect.Height;
                    this.Left = rect.Left;
                    this.Top = rect.Top;
                }
            }
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);

            log.Info("MessageBoxTip Closed");
        }
    }
}
