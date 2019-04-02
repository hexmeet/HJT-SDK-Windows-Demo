using EasyVideoWin.CustomControls;
using EasyVideoWin.Helpers;
using EasyVideoWin.Model;
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
    /// Interaction logic for LayoutTitlebarWindow.xaml
    /// </summary>
    public partial class LayoutTitlebarWindow : CustomShowHideBaseWindow
    {
        #region -- Members --
        
        #endregion

        #region -- Properties --

        #endregion

        #region -- Constructor --

        public LayoutTitlebarWindow()
        {
            InitializeComponent();

            this.Loaded += LayoutTitlebarWindow_Loaded;
        }

        #endregion

        #region -- Public Method --

        #endregion

        #region -- Private Method --

        private void LayoutTitlebarWindow_Loaded(object sender, RoutedEventArgs e)
        {
            Utils.HideWindowInAltTab(this.Handle);
        }

        private void ExitFullScreenBtn_Click(object sender, RoutedEventArgs e)
        {
            VideoPeopleWindow.Instance.RestoreWindow();
        }
        
        #endregion
    }
}
