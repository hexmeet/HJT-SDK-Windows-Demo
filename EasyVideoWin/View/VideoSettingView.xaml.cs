using EasyVideoWin.Model;
using NAudio.Wave;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;

namespace EasyVideoWin.View
{
    /// <summary>
    /// Interaction logic for VideoSettingView.xaml
    /// </summary>
    public partial class VideoSettingView : UserControl
    {
        private EasyVideoWin.ViewModel.VideoSettingViewModel _videoSettingViewModel;
        
        public VideoSettingView()
        {
            InitializeComponent();

            _videoSettingViewModel = this.DataContext as EasyVideoWin.ViewModel.VideoSettingViewModel;
            _videoSettingViewModel.RefreshData();
            this.IsVisibleChanged += new DependencyPropertyChangedEventHandler(UserControl_IsVisibleChanged);
        }

        private void UserControl_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if((bool)e.OldValue == false && (bool)e.NewValue == true)
            {
                RefreshData();
            }
            else if((bool)e.OldValue == true && (bool)e.NewValue == false)
            {
                SettingManager.Instance.CloseCamera();
            }
        }
        
        private void RefreshData()
        {
            _videoSettingViewModel.RefreshData();
            SettingManager.Instance.SetLocalVideoWindow(this.localPreviewWnd.Handle);
            SettingManager.Instance.OpenCamemra();
        }
    }
}
