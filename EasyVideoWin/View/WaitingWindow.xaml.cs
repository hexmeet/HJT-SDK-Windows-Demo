using System;
using System.Collections.Generic;
using System.ComponentModel;
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
    /// Interaction logic for WaitingWindow.xaml
    /// </summary>
    public partial class WaitingWindow : Window
    {
        #region -- Members --

        System.Windows.Input.Cursor _appCursor = System.Windows.Input.Mouse.OverrideCursor;

        #endregion

        #region -- Properties --

        #endregion

        #region -- Constructor --

        public WaitingWindow(string promptInfo)
        {
            InitializeComponent();

            System.Windows.Input.Mouse.OverrideCursor = System.Windows.Input.Cursors.Wait;
            this.promptInfoTextBlock.Text = promptInfo;
        }

        #endregion

        #region -- Public Method --

        #endregion

        #region -- Protected Method --

        protected override void OnClosing(CancelEventArgs e)
        {
            System.Windows.Input.Mouse.OverrideCursor = _appCursor;
            base.OnClosing(e);
        }

        #endregion

        #region -- Private Method --


        #endregion
    }
}
