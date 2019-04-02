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
    /// Interaction logic for ProgressDialog.xaml
    /// </summary>
    public partial class ProgressDialog : Window
    {
        private static ProgressDialog instance = new ProgressDialog();

        private ProgressDialog()
        {
            InitializeComponent();
            this.Owner = Application.Current.MainWindow;
        }

        public static ProgressDialog Instance
        {
            get
            {
                return instance;
            }
        }

        public new void ShowDialog()
        {
            MainWindow window = (MainWindow)Application.Current.MainWindow;
            Rect mainWindowRect = window.GetWindowRect();
            this.Width = mainWindowRect.Width;
            this.Height = mainWindowRect.Height;
            this.Top = mainWindowRect.Top;
            this.Left = mainWindowRect.Left;
            base.ShowDialog();
        }
    }
}
