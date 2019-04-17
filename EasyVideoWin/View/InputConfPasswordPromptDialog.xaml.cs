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
    public partial class InputConfPasswordPromptDialog : Window
    {
        #region -- Members --

        

        #endregion

        #region -- Properties --

        public string ConfPassword
        {
            get
            {
                return this.textBoxPassword.Text;
            }
        }
        
        #endregion

        #region -- Constructor --

        public InputConfPasswordPromptDialog()
        {
            InitializeComponent();
            this.IsVisibleChanged += InputConfPasswordPromptDialog_IsVisibleChanged;
        }
        
        #endregion

        #region -- Public Methods --

        #endregion

        #region -- Private Methods --

        private void Accept_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
        }


        private void Close_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
        }

        private void InputConfPasswordPromptDialog_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (Visibility.Visible == this.Visibility)
            {
                this.textBoxPassword.Focus();
            }
        }

        #endregion


    }
}
