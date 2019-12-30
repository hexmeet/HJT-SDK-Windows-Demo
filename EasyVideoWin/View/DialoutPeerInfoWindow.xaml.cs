using EasyVideoWin.CustomControls;
using EasyVideoWin.Helpers;
using EasyVideoWin.Model;
using log4net;
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
    /// Interaction logic for DialoutPeerInfoWindow.xaml
    /// </summary>
    public partial class DialoutPeerInfoWindow : CustomShowHideBaseWindow
    {
        #region -- Properties --

        private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        #endregion

        #region -- Constructor --

        public DialoutPeerInfoWindow()
        {
            InitializeComponent();

            //this.SizeToContent = SizeToContent.Width;
            this.Loaded += DialoutPeerInfoWindow_Loaded;
        }
        
        #endregion

        #region -- Public Methods --

        public void UpdateAvatarImage(string avatarUrl, string displayName)
        {
            if (string.IsNullOrEmpty(avatarUrl))
            {
                log.Warn("Failed to UpdateAvatarImage for null or empty");
            }
            else
            {
                try {
                    this.avatarImg.Source = new BitmapImage(new Uri(avatarUrl));
                }
                catch(Exception e)
                {
                    log.WarnFormat("Exception for UpdateAvatarImage: {0}", e);
                }
            }
            this.peerDisplayName.Text = displayName;
        }

        #endregion

        #region -- Private Methods --

        private void DialoutPeerInfoWindow_Loaded(object sender, RoutedEventArgs e)
        {
            Utils.HideWindowInAltTab(this.Handle);
        }

        private void Hangup_Click(object sender, RoutedEventArgs e)
        {
            log.Info("Hangup_Click");
            CallController.Instance.TerminateCall();
        }

        #endregion
    }
}
