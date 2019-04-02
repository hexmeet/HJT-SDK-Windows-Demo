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
    /// Interaction logic for ConfirmWin.xaml
    /// </summary>
    public partial class ConfirmWin : Window
    {
        public ConfirmWin()
        {
            InitializeComponent();
            loading.Visibility = Visibility.Collapsed;
            ConfirmBtn.Visibility = Visibility.Visible;
        }

        private void BackGround_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                this.DragMove();
                //Window.MouseMoveEvent();
            }
        }

        private void OnClick_OK(object sender, MouseButtonEventArgs e)
        {
            this.Close();
        }

        public void SetTitleAndMsg(string title,string msg)
        {
            TitleText.Text = title;
            MessageText.Text = msg;
            ConfirmBtn.Visibility = Visibility.Visible;
        }

        public void ShowLoading(bool isloading)
        {
            if(isloading)
            {
                loading.Visibility = Visibility.Visible;
                MessageText.Text = "";
                ConfirmBtn.Visibility = Visibility.Collapsed;
            }
            else
            {
                loading.Visibility = Visibility.Collapsed;
                MessageText.Text = "";
                ConfirmBtn.Visibility = Visibility.Visible;
            }
        }
    }
}
