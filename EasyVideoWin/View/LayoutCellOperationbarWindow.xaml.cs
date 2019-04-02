using EasyVideoWin.CustomControls;
using EasyVideoWin.Helpers;
using System;
using System.Collections.Generic;
using System.ComponentModel;
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
    /// Interaction logic for LayoutCellOperationbarWindow.xaml
    /// </summary>
    public partial class LayoutCellOperationbarWindow : CustomShowHideBaseWindow, INotifyPropertyChanged
    {
        #region -- Constructor --

        public event PropertyChangedEventHandler PropertyChanged;

        private const int INITIAL_HEIGHT = 35;
        private Visibility _micMutedVisibility;
        private bool _isMicMuted;

        #endregion

        #region -- Properties --

        public string CellName
        {
            get
            {
                return this.cellName.Text;
            }
            set
            {
                this.cellName.Text = value;
            }
        }

        public int InitialHeight
        {
            get
            {
                return INITIAL_HEIGHT;
            }
        }

        public Visibility MicMutedVisibility
        {
            get
            {
                return _micMutedVisibility;
            }
            set
            {
                _micMutedVisibility = value;
                OnPropertyChanged("MicMutedVisibility");
            }
        }

        public bool IsMicMuted
        {
            get
            {
                return _isMicMuted;
            }
            set
            {
                _isMicMuted = value;
                MicMutedVisibility = _isMicMuted ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        #endregion

        #region -- Constructor --

        public LayoutCellOperationbarWindow()
        {
            InitializeComponent();

            this.imgMicMuted.SetBinding(Image.VisibilityProperty, new Binding("MicMutedVisibility") { Source = this });

            IsMicMuted = false;

            this.Height = INITIAL_HEIGHT;
            this.Loaded += LayoutCellOperationbarWindow_Loaded;
        }
        
        #endregion

        #region -- Public Methods --

        public void SetFontSize(double fontSize)
        {
            this.cellName.FontSize = fontSize;
        }

        public void SetTextMargin(Thickness thickness)
        {
            this.cellName.Margin = thickness;
        }

        #endregion

        #region -- Private Methods --

        private void LayoutCellOperationbarWindow_Loaded(object sender, RoutedEventArgs e)
        {
            Utils.HideWindowInAltTab(this.Handle);
        }

        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion
    }
}
