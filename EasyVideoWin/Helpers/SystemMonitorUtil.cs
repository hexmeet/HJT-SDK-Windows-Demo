using log4net;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasyVideoWin.Helpers
{
    class SystemMonitorUtil
    {
        #region -- Members --

        private static ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private static SystemMonitorUtil _instance = new SystemMonitorUtil();

        private PerformanceCounter _performanceCounter;
        private bool _initialized = false;

        #endregion

        #region -- Properties --

        public static SystemMonitorUtil Instance
        {
            get
            {
                return _instance;
            }
        }

        #endregion

        #region -- Constructor --

        private SystemMonitorUtil()
        {
            try
            {
                string procName = Process.GetCurrentProcess().ProcessName;
                _performanceCounter = new PerformanceCounter("Process", "Private Bytes", procName);
                _initialized = true;
                log.Info("Succeed to construct SystemMonitorUtil");
            }
            catch (Exception e)
            {
                log.InfoFormat("Failed to construct SystemMonitorUtil, exeption: {0}", e);
            }
        }

        #endregion

        #region -- Public Methods --

        public void OutputMemoryInfo()
        {
            if (!_initialized)
            {
                return;
            }
            log.InfoFormat("Process memory: {0}, GC memory: {1}", _performanceCounter.NextValue(), GC.GetTotalMemory(true));
        }

        public void CollectGarbage()
        {
            if (!_initialized)
            {
                return;
            }
            log.InfoFormat("Before collect garbage. Process memory: {0}, GC memory: {1}", _performanceCounter.NextValue(), GC.GetTotalMemory(true));
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
            log.InfoFormat("After garbage collected. Process memory: {0}, GC memory: {1}", _performanceCounter.NextValue(), GC.GetTotalMemory(true));
        }

        #endregion

        #region -- Private Methods --

        #endregion
    }
}
