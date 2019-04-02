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
    /// Interaction logic for EmailClientNotSupportPrompt.xaml
    /// </summary>
    public partial class EmailClientNotSupportPrompt : Window
    {
        #region -- Members --

        #endregion

        #region -- Properties --

        #endregion

        #region -- Constructor --

        public EmailClientNotSupportPrompt()
        {
            InitializeComponent();
        }

        #endregion

        #region -- Public Method --

        public void SetSubjectAndBody(string subject, string body)
        {
            this.subject.Text = subject;
            this.body.Text = body;
        }

        #endregion

        #region -- Private Method --

        private void CloseBtn_Click(object sender, RoutedEventArgs e)
        {
            this.Visibility = Visibility.Collapsed;
        }

        #endregion

    }
}
