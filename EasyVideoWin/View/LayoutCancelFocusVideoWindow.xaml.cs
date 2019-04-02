using EasyVideoWin.CustomControls;
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
    /// Interaction logic for LayoutCancelFocusVideoWindow.xaml
    /// </summary>
    public partial class LayoutCancelFocusVideoWindow : CustomShowHideBaseWindow
    {
        #region -- Properties --

        
        #endregion

        #region -- Constructor --

        public LayoutCancelFocusVideoWindow()
        {
            InitializeComponent();

            this.SizeToContent = SizeToContent.Width;
            this.Loaded += LayoutCancelFocusVideoWindow_Loaded;
        }
        
        #endregion

        #region -- Public Methods --

        #endregion

        #region -- Private Methods --

        private void LayoutCancelFocusVideoWindow_Loaded(object sender, RoutedEventArgs e)
        {
            Utils.HideWindowInAltTab(this.Handle);
        }

        #endregion
    }
}
