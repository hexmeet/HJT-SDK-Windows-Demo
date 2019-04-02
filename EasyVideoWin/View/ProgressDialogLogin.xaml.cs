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
    /// Interaction logic for ProgressDialogLogin.xaml
    /// </summary>
    public partial class ProgressDialogLogin : Window
    {
        private static ProgressDialogLogin _instance = new ProgressDialogLogin();

        private ProgressDialogLogin()
        {
            InitializeComponent();
        }

        public static ProgressDialogLogin Instance
        {
            get
            {
                return _instance;
            }
        }

        public void ShowDialog(Window loginWindow)
        {
            if(loginWindow == null)
            {
                return;
            }
            this.Owner = loginWindow;
            this.Width = loginWindow.Width;
            this.Height = loginWindow.Height;
            this.Top = loginWindow.Top;
            this.Left = loginWindow.Left;
            base.Show();
        }

    }
}
