using EasyVideoWin.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using EasyVideoWin.CustomControls;
using EasyVideoWin.Helpers;

namespace EasyVideoWin.View
{
    /// <summary>
    /// Interaction logic for SettingView.xaml
    /// </summary>
    public partial class SettingView : UserControl
    {
        private List<SettingOperationButton> _navigationButtons = new List<SettingOperationButton>();
        private SettingViewModel _settingViewModel;
        
        public SettingView()
        {
            InitializeComponent();

            _navigationButtons.Add(this.GeneralButton);
            _navigationButtons.Add(this.AudioButton);
            _navigationButtons.Add(this.VideoButton);
            _navigationButtons.Add(this.AboutButton);

            _settingViewModel = (SettingViewModel)this.DataContext;

            //default selected MyInfoButton
            OnNavigationChanged(this.GeneralButton);
        }
        
        private void General_Click(object sender, RoutedEventArgs e)
        {
            _settingViewModel.SetGeneralCommand.Execute(null);
            OnNavigationChanged(this.GeneralButton);
        }

        private void Audio_Click(object sender, RoutedEventArgs e)
        {
            _settingViewModel.AudioCommand.Execute(null);
            OnNavigationChanged(this.AudioButton);
        }

        private void Video_Click(object sender, RoutedEventArgs e)
        {
            _settingViewModel.VideoCommand.Execute(null);
            OnNavigationChanged(this.VideoButton);
        }

        private void ShowAbout_Click(object sender, RoutedEventArgs e)
        {
            _settingViewModel.ShowAboutCommand.Execute(null);
            OnNavigationChanged(this.AboutButton);
            
        }

        private void OnNavigationChanged(SettingOperationButton activatedButton)
        {
            foreach(SettingOperationButton button in _navigationButtons)
            {
                button.IsChecked = activatedButton == button;
            }
        }
        
    }
}
