using EasyVideoWin.Helpers;
using EasyVideoWin.ViewModel;
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
    /// Interaction logic for IncomingCallPrompt.xaml
    /// </summary>
    public partial class IncomingCallPromptDialog : Window
    {
        #region -- Members --

        

        #endregion

        #region -- Properties --

        public bool IsAccept{ get; set; }
        
        #endregion

        #region -- Constructor --

        public IncomingCallPromptDialog()
        {
            InitializeComponent();
        }

        #endregion

        #region -- Public Methods --

        public void SetPromptInfo(string callFrom, string confNumber)
        {
            string prompt = "";
            if (string.IsNullOrEmpty(callFrom))
            {
                prompt = LanguageUtil.Instance.GetValueByKey("INVITED_JOIN_CONF_PROMPT");
                prompt = string.Format(prompt, confNumber);
            }
            else
            {
                prompt = LanguageUtil.Instance.GetValueByKey("ADMIN_INVITE_YOU_JOIN_CONF_PROMPT");
                prompt = string.Format(prompt, callFrom, confNumber);
            }
            
            this.promptContent.Text = prompt;
        }

        #endregion

        #region -- Private Methods --

        private void Accept_Click(object sender, RoutedEventArgs e)
        {
            this.IsAccept = true;
            this.Visibility = Visibility.Collapsed;
        }


        private void Close_Click(object sender, RoutedEventArgs e)
        {
            this.IsAccept = false;
            this.Visibility = Visibility.Collapsed;
        }

        #endregion


    }
}
