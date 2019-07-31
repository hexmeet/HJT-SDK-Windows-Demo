using EasyVideoWin.CustomControls;
using EasyVideoWin.Helpers;
using log4net;
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
        #region -- Members --

        private readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private const double CELL_NAME_ADJUT_HEIGHT_TO_WIN = 13;

        #endregion

        #region -- Constructor --

        public event PropertyChangedEventHandler PropertyChanged;

        private const int INITIAL_HEIGHT = 27;
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
                this.cellName.Width = double.NaN;
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

            this.SizeChanged += LayoutCellOperationbarWindow_SizeChanged;
            this.cellName.SizeChanged += CellName_SizeChanged;
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

        private void CellName_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            log.InfoFormat("CellName_SizeChanged, CellName: {0}, width: {1}, actualWidth: {2}, win width: {3}, win actualWidth: {4}"
                , cellName.Text
                , cellName.Width
                , cellName.ActualWidth
                , this.Width
                , this.ActualWidth
            );

            AdjustCellNameWidth();
        }

        private void LayoutCellOperationbarWindow_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            log.InfoFormat("LayoutCellOperationbarWindow_SizeChanged, CellName: {0}, width: {1}, actualWidth: {2}, win width: {3}, win actualWidth: {4}"
                , cellName.Text
                , cellName.Width
                , cellName.ActualWidth
                , this.Width
                , this.ActualWidth
            );

            double width = this.ActualWidth - CELL_NAME_ADJUT_HEIGHT_TO_WIN;
            if (cellName.ActualWidth < width && width > 0)
            {
                cellName.Width = double.NaN;
            }

            AdjustCellNameWidth();
        }

        private void AdjustCellNameWidth()
        {
            double width = this.ActualWidth - CELL_NAME_ADJUT_HEIGHT_TO_WIN;
            if (this.cellName.ActualWidth > width && width > 0)
            {
                this.cellName.Width = width;
            }
        }

        #endregion
    }
}
