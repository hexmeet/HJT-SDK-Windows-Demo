using EasyVideoWin.CustomControls;
using EasyVideoWin.Enums;
using EasyVideoWin.Helpers;
using EasyVideoWin.Model;
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
    /// LayoutExitAudioModeWindow.xaml 的交互逻辑
    /// </summary>
    public partial class LayoutExitAudioModeWindow : CustomShowHideBaseWindow
    {
        #region -- Members --

        public event Action EventSwitch2VideoMode;

        #endregion

        #region -- Properties --

        #endregion

        #region -- Constructor --
        
        public LayoutExitAudioModeWindow()
        {
            InitializeComponent();

            this.Loaded += LayoutExitAudioModeWindow_Loaded;
        }
        
        private void SwitchVideo_Click(object sender, RoutedEventArgs e)
        {
            CallController.Instance.Switch2VideoMode();
            EventSwitch2VideoMode?.Invoke();
        }

        #endregion

        #region -- Public Methods --

        #endregion

        #region -- Private Methods --

        private void LayoutExitAudioModeWindow_Loaded(object sender, RoutedEventArgs e)
        {
            Utils.HideWindowInAltTab(this.Handle);
        }

        private void SwitchVideo_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            CallController.Instance.Switch2VideoMode();
            EventSwitch2VideoMode?.Invoke();
        }

        #endregion

    }
}
