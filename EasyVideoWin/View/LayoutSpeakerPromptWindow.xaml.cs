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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace EasyVideoWin.View
{
    /// <summary>
    /// LayoutSpeakerPromptWindow.xaml 的交互逻辑
    /// </summary>
    public partial class LayoutSpeakerPromptWindow : CustomShowHideBaseWindow
    {
        #region -- Members --

        private string _speakerName;

        #endregion

        #region -- Properties --

        public string SpeakerName
        {
            get
            {
                return _speakerName;
            }
            set
            {
                _speakerName = value;
                speakingPromptTextBlock.Text = string.Format(LanguageUtil.Instance.GetValueByKey("SPEAKER_SPEAKING"), value);
            }
        }

        #endregion

        #region -- Constructor --
        public LayoutSpeakerPromptWindow()
        {
            InitializeComponent();

            this.SizeToContent = SizeToContent.Width;
            this.Loaded += LayoutSpeakerPromptWindow_Loaded;
        }

        #endregion

        #region -- Public Methods --

        #endregion

        #region -- Private Methods --
        private void LayoutSpeakerPromptWindow_Loaded(object sender, RoutedEventArgs e)
        {
            Utils.HideWindowInAltTab(this.Handle);
        }
        #endregion
    }
}
