using EasyVideoWin.CustomControls;
using EasyVideoWin.Helpers;
using EasyVideoWin.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasyVideoWin.ViewModel
{
    class DialOutModelBase : ViewModelBase
    {
        protected void JoinConference(string conferenceNumber, string displayName, string password, IMasterDisplayWindow ownerWindow)
        {
            if (string.IsNullOrEmpty(conferenceNumber.Trim()))
            {
                return;
            }
            
            if (LoginManager.Instance.CurrentLoginStatus == LoginStatus.LoggedIn && !LoginManager.Instance.IsRegistered)
            {
                if (null == ownerWindow)
                {
                    ownerWindow = (MainWindow)System.Windows.Application.Current.MainWindow;
                }
                MessageBoxTip tip = new MessageBoxTip(ownerWindow);
                tip.SetTitleAndMsg(LanguageUtil.Instance.GetValueByKey("PROMPT"), LanguageUtil.Instance.GetValueByKey("FAIL_TO_CALL_FOR_REGISTER_FAILURE"), LanguageUtil.Instance.GetValueByKey("CONFIRM"));
                tip.ShowDialog();
                return;
            }

            CallController.Instance.UpdateUserImage(Utils.GetSuspendedVideoBackground(), Utils.GetCurrentAvatarPath());
            CallController.Instance.JoinConference(conferenceNumber, displayName, password);
        }        
    }
}
