using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Windows;
using EasyVideoWin.Model;
using EasyVideoWin.Helpers;
using log4net;

namespace EasyVideoWin.ViewModel
{
    class SelectShareContentModeWindowViewModel : ViewModelBase
    {
        private readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public SelectShareContentModeWindowViewModel()
        {

        }

        public bool _interactionWhiteboardBtnVisible = true;
        public Visibility InteractionWhiteboardBtnVisibility
        {
            get
            {
                bool isAcsConnected = CallController.Instance.IsConnected2WhiteBoard;
                bool isWhiteBoardStared =    ContentStreamStatus.ReceivingWhiteBoardStarted == CallController.Instance.CurrentContentStreamStatus
                                          || ContentStreamStatus.SendingWhiteBoardStarted == CallController.Instance.CurrentContentStreamStatus;
                log.InfoFormat("white board is connected with ACS:{0}, white board is started:{1}",
                        isAcsConnected, isWhiteBoardStared);
                return (!isAcsConnected || isWhiteBoardStared) ? Visibility.Collapsed : Visibility.Visible;
            }
        }

        public Visibility ScreenShareBtnVisibility
        {
            get
            {
                return CallController.Instance.CurrentContentStreamStatus == ContentStreamStatus.SendingContentStarted
                        ? Visibility.Collapsed : Visibility.Visible;  
            }
        }
    }
}
