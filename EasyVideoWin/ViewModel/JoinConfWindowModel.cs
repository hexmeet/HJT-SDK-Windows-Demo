using EasyVideoWin.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using EasyVideoWin.Model;
using System.Windows.Controls;
using System.Net;
using log4net;
using System.Threading;
using System.Windows;
using EasyVideoWin.View;
using System.Windows.Media;

namespace EasyVideoWin.ViewModel
{
    class JoinConfWindowModel : ViewModelBase
    {
        #region -- Members --

        private readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private static System.Windows.Media.Brush OPERATION_NAME_COLOR = new SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#000000"));

        private UserControl _currentView;
        private UserControl _anonymousJoinConfView;
        private string _currentTitle;
        private System.Windows.Media.Brush _titleColor;
        #endregion

        #region -- Properties --
        
        public UserControl CurrentView
        {
            get
            {
                return _currentView;
            }
            set
            {
                if (_currentView != value)
                {
                    if (null != _currentView)
                    {
                        _currentView.Visibility = Visibility.Collapsed;
                    }
                    _currentView = value;
                    _currentView.Visibility = Visibility.Visible;
                    OnPropertyChanged("CurrentView");
                }
                
                OnPropertyChanged("CurrentTitle");
            }
        }

        public string CurrentTitle
        {
            get
            {
                return _currentTitle;
            }
            set
            {
                _currentTitle = value;
                OnPropertyChanged("CurrentTitle");
            }
        }

        public System.Windows.Media.Brush TitleColor
        {
            get
            {
                return _titleColor;
            }
            set
            {
                _titleColor = value;
                OnPropertyChanged("TitleColor");
            }
        }
        
        #endregion

        #region -- Constructor --

        public JoinConfWindowModel()
        {
            _anonymousJoinConfView = (UserControl)Activator.CreateInstance(typeof(EasyVideoWin.View.AnonymousJoinConfView));

            CurrentView = _anonymousJoinConfView;
            CurrentTitle = LanguageUtil.Instance.GetValueByKey("JOIN_CONFERENCE");
            TitleColor = OPERATION_NAME_COLOR;
        }

        #endregion

        #region -- Public Method --

        public void SetConfNumberPassword(string confNumber, string confPassword)
        {
            AnonymousJoinConfView joinConfView = _anonymousJoinConfView as AnonymousJoinConfView;
            joinConfView.SetConfNumberPassword(confNumber, confPassword);
        }

        #endregion

        #region -- Private Method --



        #endregion
    }
}
