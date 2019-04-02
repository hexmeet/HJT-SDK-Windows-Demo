using EasyVideoWin.View;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasyVideoWin.ViewModel
{
    class WaitingCollectLogViewModel : ViewModelBase
    {
        System.Windows.Input.Cursor appCursor = System.Windows.Input.Mouse.OverrideCursor;
        public WaitingCollectLogViewModel()
        {
            isCollecting = true;
            Model.SettingManager.Instance.CollectLogFinished += this.OnCollectLogFinished;
            System.Windows.Input.Mouse.OverrideCursor = System.Windows.Input.Cursors.Wait;
        }

        public bool isCollecting { get; set; }

        private void OnCollectLogFinished(object sender, WaitingCollectLogView arg)
        {
            isCollecting = false;
            OnPropertyChanged("isCollecting");            
            App.Current.Dispatcher.Invoke(()=> {
                System.Windows.Input.Mouse.OverrideCursor = appCursor;
                arg.Close();
            });
        }

    }
}
