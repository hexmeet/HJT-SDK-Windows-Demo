using EasyVideoWin.Helpers;
using EasyVideoWin.Model;
using log4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
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
    /// Interaction logic for PromptWindow.xaml
    /// </summary>
    public partial class PromptWindow : Window
    {
        private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private static readonly int SHOW_DURATION = 3000;
        
        private static PromptWindow _instance = new PromptWindow(null);

        private Timer _autoCloseTimer = null;
        private IMasterDisplayWindow _masterWindow = null;

        public static PromptWindow Instance
        {
            get
            {
                return _instance;
            }
        }

        private PromptWindow(Window owner)
        {
            InitializeComponent();
            InitParams(owner);
        }
        
        private void InitParams(Window owner)
        {
            this.SizeChanged += PromptWindow_SizeChanged;
            this.SizeToContent = SizeToContent.Width;
            this.Owner = owner;
        }
        
        private void StartAutoCloseTimer(int duration)
        {
            if (null == _autoCloseTimer)
            {
                _autoCloseTimer = new Timer();
                _autoCloseTimer.AutoReset = false;
                _autoCloseTimer.Elapsed += CloseWindow;
            }
            _autoCloseTimer.Enabled = false;
            if (duration > 0)
            {
                _autoCloseTimer.Interval = duration;
            }
            else
            {
                _autoCloseTimer.Interval = SHOW_DURATION;
            }
            _autoCloseTimer.Enabled = true;
        }

        private void CloseWindow(object sender, ElapsedEventArgs e)
        {
            Application.Current.Dispatcher.InvokeAsync(() => {
                CloseWindow();
            });
        }

        public void CloseWindow()
        {
            if (Visibility.Visible != this.Visibility)
            {
                return;
            }
            if (null != _autoCloseTimer && _autoCloseTimer.Enabled)
            {
                log.InfoFormat("Close window disable timer:{0}", _autoCloseTimer.Enabled);
                _autoCloseTimer.Enabled = false;
            }
            log.Info("Close window");
            this.Visibility = Visibility.Collapsed;
        }

        public void ShowPromptByTime(string content, int duration, IMasterDisplayWindow masterWin, Window activeWindow)
        {
            if (string.IsNullOrEmpty(content))
            {
                return;
            }
            _masterWindow = masterWin;
            log.Info("Show prompt by time");
            if (null != _autoCloseTimer && _autoCloseTimer.Enabled)
            {
                log.InfoFormat("Timer enabled:{0}", _autoCloseTimer.Enabled);
                _autoCloseTimer.Enabled = false;
            }

            //log.Info("Begin to show prompt content.");
            //this.label.Content = content;
            //this.Show();
            ////this.Top += (WindowDownRatio * ratio);
            //StartAutoCloseTimer(duration);
            //SetProperPosition();
            Application.Current.Dispatcher.InvokeAsync(() =>
            {
                // note: Put the code below the UI thread queue to make sure the execution after the last arrival item timeout. 
                log.Info("Begin to show prompt content.");
                this.label.Content = content;
                this.Show();
                //this.Top += (WindowDownRatio * ratio);
                StartAutoCloseTimer(duration);
                SetProperPosition();
                log.InfoFormat("window IsActive: {0}, null != activeWindow: {1}", this.IsActive, null != activeWindow);
                if (IsActive && null != activeWindow)
                {
                    log.Info("activeWindow, activate");
                    activeWindow.Activate();
                }
            });
        }

        private void PromptWindow_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            SetProperPosition();
        }

        private void SetProperPosition()
        {
            if (null == _masterWindow)
            {
                return;
            }
            Rect mainWindowRect = _masterWindow.GetWindowRect();
            double width = this.ActualWidth;
            if (width > mainWindowRect.Width)
            {
                width = mainWindowRect.Width;
            }
            double left = mainWindowRect.Left + Math.Round((mainWindowRect.Width - width) / 2);

            this.Left = left;
            this.Top = mainWindowRect.Top + (mainWindowRect.Height - this.ActualHeight) / 2;
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
        }
    }
}
