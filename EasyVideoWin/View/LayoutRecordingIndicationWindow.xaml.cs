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
    /// Interaction logic for LayoutRecordingIndicationWindow.xaml
    /// </summary>
    public partial class LayoutRecordingIndicationWindow : CustomShowHideBaseWindow
    {
        #region -- Properties --

        public string TypeTitle
        {
            set
            {
                this.typeTitle.Text = value;
            }
        }
        
        #endregion

        #region -- Constructor --

        public LayoutRecordingIndicationWindow()
        {
            InitializeComponent();

            this.SizeToContent = SizeToContent.Width;
            this.Loaded += LayoutRecordingIndicationWindow_Loaded;
        }

        #endregion

        #region -- Public Methods --

        #endregion

        #region -- Private Methods --

        private void LayoutRecordingIndicationWindow_Loaded(object sender, RoutedEventArgs e)
        {
            Utils.HideWindowInAltTab(this.Handle);
        }

        #endregion
    }
}
