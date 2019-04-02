using log4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace EasyVideoWin.Helpers
{
    class WorkerThreadManager
    {
        private readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private Dispatcher _evSdkWorkDispatcher;
        
        private static WorkerThreadManager _instance = new WorkerThreadManager();

        public static WorkerThreadManager Instance
        {
            get
            {
                return _instance;
            }
        }
        
        public Dispatcher EVSdkWorkDispatcher
        {
            get
            {
                return _evSdkWorkDispatcher;
            }
        }
        
        public WorkerThreadManager()
        {
            CreateEvSdkWorkDispatcher();
        }
        
        private void CreateEvSdkWorkDispatcher()
        {
            ManualResetEvent dispatcherReadyEvent = new ManualResetEvent(false);

            Thread t = new Thread(new ThreadStart(() =>
            {
                _evSdkWorkDispatcher = Dispatcher.CurrentDispatcher;
                dispatcherReadyEvent.Set();
                Dispatcher.Run();
            }));

            t.IsBackground = true;
            t.Start();

            dispatcherReadyEvent.WaitOne();
        }
    }
}
