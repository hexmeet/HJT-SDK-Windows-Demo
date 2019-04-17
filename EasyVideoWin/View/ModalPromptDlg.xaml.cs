using EasyVideoWin.Helpers;
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
    /// Interaction logic for ModalPromptDlg.xaml
    /// </summary>
    public partial class ModalPromptDlg : Window
    {
        #region -- Members --

        private IMasterDisplayWindow _masterDisplayWindow;

        #endregion

        #region -- Properties --

        #endregion

        #region -- Constructor --

        public ModalPromptDlg()
        {
            InitializeComponent();
            this.SizeChanged += ModalPromptDlg_SizeChanged;
        }

        #endregion

        #region -- Public Method --

        public void ShowDialog(IMasterDisplayWindow masterDisplayWindow, string title, string content, string okText)
        {
            _masterDisplayWindow = masterDisplayWindow;
            this.TitleLabel.Content = title;
            this.ContentTextBlock.Text = content;
            this.OkTextButton.ButtonContent = okText;
            SetProperPosition();
            this.ShowDialog();
        }

        #endregion

        #region -- Private Method --

        private void ModalPromptDlg_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            SetProperPosition();
        }

        private void SetProperPosition()
        {
            if (null == _masterDisplayWindow)
            {
                return;
            }

            Rect mainWindowRect = _masterDisplayWindow.GetWindowRect();
            double width = this.ActualWidth;
            if (width > mainWindowRect.Width)
            {
                width = mainWindowRect.Width;
            }
            double left = mainWindowRect.Left + Math.Round((mainWindowRect.Width - width) / 2);

            this.Left = left;
            this.Top = mainWindowRect.Top + (mainWindowRect.Height - this.ActualHeight) / 2;
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Visibility = Visibility.Collapsed;
        }

        private void Ok_Click(object sender, RoutedEventArgs e)
        {
            this.Visibility = Visibility.Collapsed;
        }

        #endregion

    }
}
