using EasyVideoWin.Helpers;
using System;
using System.Windows;
using System.Windows.Input;

namespace EasyVideoWin.CustomControls
{
    /// <summary>
    /// Interaction logic for MessageBoxEx.xaml
    /// </summary>
    public partial class MessageBoxConfirm : Window
    {
        private bool _resize = true;
        private IMasterDisplayWindow _ownerWindow;
        public event EventHandler ConfirmEvent;
        
        private MessageBoxConfirm()
        {
            InitializeComponent();
        }

        public MessageBoxConfirm(IMasterDisplayWindow ownerWindow) : this()
        {
            _ownerWindow = ownerWindow;
            if (null != ownerWindow)
            {
                this.Owner = ownerWindow.GetWindow();
            }
        }

        public void SetTitleAndMsg(string title, string msg)
        {
            TitleText.Text = title;
            MessageText.Text = msg;
        }

        public void isResize(bool value)
        {
            _resize = value;
        }

        private void CloseBtn_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void ConfirmBtn_Click(object sender, RoutedEventArgs e)
        {
            ConfirmEvent?.Invoke(this, new EventArgs());
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (_resize)
            {
                Rect rect = MessageBoxUtil.GetOwnerWindowRect(this, _ownerWindow);
                if (0 != rect.Width)
                {
                    this.Width = rect.Width;
                    this.Height = rect.Height;
                    this.Left = rect.Left;
                    this.Top = rect.Top;
                }
            }
        }
    }
}
