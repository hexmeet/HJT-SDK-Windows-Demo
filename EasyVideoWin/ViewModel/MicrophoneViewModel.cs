using EasyVideoWin.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasyVideoWin.ViewModel
{
    class MicrophoneViewModel : AudioDeviceModel
    {
        protected override string GetCurrentDevice()
        {
            return SettingManager.Instance.GetCurrentMicrophone();
        }

        protected override ObservableCollection<string> GetDevices()
        {
            return SettingManager.Instance.GetMicrophones();
        }

        protected override void SelectDevice(string device)
        {
            SettingManager.Instance.SetMicrophone(device);
        }
    }
}
