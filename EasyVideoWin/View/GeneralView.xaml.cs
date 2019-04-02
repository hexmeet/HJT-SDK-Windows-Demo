using EasyVideoWin.Helpers;
using EasyVideoWin.Model;
using EasyVideoWin.ViewModel;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace EasyVideoWin.View
{
    /// <summary>
    /// Interaction logic for GeneralView.xaml
    /// </summary>
    public partial class GeneralView : UserControl
    {

        #region -- Constructor --

        public GeneralView()
        {
            InitializeComponent();
            this.IsVisibleChanged += GeneralView_IsVisibleChanged;
        }

        #endregion

        #region -- Private Methods --
        
        private void ChangeScreenPicPath(object sender, RoutedEventArgs e)
        {
            using (System.Windows.Forms.FolderBrowserDialog dlgFolder = new System.Windows.Forms.FolderBrowserDialog())
            {
                dlgFolder.Description = LanguageUtil.Instance.GetValueByKey("SELECT_SCREEN_PIC_PATH"); //App.Lang["HEXMEET_LOGCOLLECT_SAVE_PACKAGE"] as string;// "Please select a folder to save the log package.";
                if (dlgFolder.ShowDialog(this as System.Windows.Forms.IWin32Window) == System.Windows.Forms.DialogResult.OK)
                {
                    string destFolder = dlgFolder.SelectedPath;
                    if (destFolder != null && !destFolder.Equals(""))
                    {
                        Helpers.Utils.SetScreenPicPath(destFolder);
                        ScreenPicPathText.Text = destFolder;
                    }
                }
            }
        }
        
        private void GeneralView_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (Visibility.Visible == this.Visibility)
            {
                GeneralViewModel vm = this.DataContext as GeneralViewModel;
                vm.InitData();
            }
        }
        
        #endregion


    }


}
