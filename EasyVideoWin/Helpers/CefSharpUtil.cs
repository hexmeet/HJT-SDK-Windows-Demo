using log4net;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasyVideoWin.Helpers
{
    public static class CefSharpUtil
    {
        #region -- Members --

        private static ILog _log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private const string CEF_CACHE_PATH = "CefSharpCache";
        private const string CEF_LOG_NAME = "CEF.log";
        private static string[] _cacheFiles = {
            "blob_storage"
            , "Cache"
            , "GPUCache"
            , "Local Storage"
            , "000003.log"
            , "Cookies"
            , "Cookies-journal"
            , "CURRENT"
            , "DevToolsActivePort"
            , "LOCK"
            , "LOG"
            , "LOG.old"
            , "MANIFEST-000001"
            , "Visited Links"
        };


        #endregion

        #region -- Properties --

        #endregion

        #region -- Constructor --

        #endregion

        #region -- Public Method --

        public static void ClearCefSharpCacheAndLog()
        {
            ClearPreviousLeftCache();

            // clear current cache
            string cachePath = Path.Combine(Utils.GetConfigDataPath(), CEF_CACHE_PATH);
            try
            {
                if (Directory.Exists(cachePath))
                {
                    DirectoryInfo dirInfo = new DirectoryInfo(cachePath);
                    DeleteFileByDirectory(dirInfo);
                }
            }
            catch (Exception e)
            {
                _log.InfoFormat("Failed to delete current cef cache: {0}, exception:{1}", cachePath, e);
            }

            TruncateCefLog();
        }

        public static void InitCefSharpSetting()
        {
            if (!CefSharp.Cef.IsInitialized)
            {
                _log.Info("Init cef sharp setting.");
                CefSharp.Wpf.CefSettings settings = new CefSharp.Wpf.CefSettings
                {
                    RemoteDebuggingPort = 8888
                    , CachePath         = Path.Combine(Utils.GetConfigDataPath(), CEF_CACHE_PATH)
                };

                //the following line is required for CEF 63.0.0 
                CefSharp.CefSharpSettings.LegacyJavascriptBindingEnabled = true;
                CefSharp.CefSharpSettings.SubprocessExitIfParentProcessClosed = true;

                settings.LogFile = Path.Combine(Utils.GetConfigDataPath(), CEF_LOG_NAME);
                settings.WindowlessRenderingEnabled = true;
                CefSharp.Cef.Initialize(settings);
            }
        }

        public static void ShutdownCefSharp()
        {
            _log.Info("Shutdown cef sharp.");
            CefSharp.Cef.Shutdown();
        }

        #endregion

        #region -- Private Method --

        private static void DeleteFileByDirectory(DirectoryInfo info)
        {
            foreach (DirectoryInfo newInfo in info.GetDirectories())
            {
                DeleteFileByDirectory(newInfo);
            }

            foreach (FileInfo newInfo in info.GetFiles())
            {
                newInfo.Attributes = newInfo.Attributes & ~(FileAttributes.Archive | FileAttributes.ReadOnly | FileAttributes.Hidden);
                newInfo.Delete();
            }

            info.Attributes = info.Attributes & ~(FileAttributes.Archive | FileAttributes.ReadOnly | FileAttributes.Hidden);
            info.Delete();
        }

        private static void ClearPreviousLeftCache()
        {
            // For the cache stored the directory of app data in previous version, so delete them firstly
            string configPath = Utils.GetConfigDataPath();
            for (int i = 0; i < _cacheFiles.Length; ++i)
            {
                string fileName = Path.Combine(configPath, _cacheFiles[i]);
                try
                {
                    if (File.Exists(fileName))
                    {
                        File.Delete(fileName);
                    }
                    else if (Directory.Exists(fileName))
                    {
                        DirectoryInfo dirInfo = new DirectoryInfo(fileName);
                        DeleteFileByDirectory(dirInfo);
                    }
                }
                catch (Exception e)
                {
                    _log.InfoFormat("Failed to delete cef cache file of previous version: {0}, exception:{1}", fileName, e);
                }
            }
        }

        private static void TruncateCefLog()
        {
            string cefLog = Path.Combine(Utils.GetConfigDataPath(), CEF_LOG_NAME);
            long fileSize = Utils.GetFileSize(cefLog);
            if (fileSize > 3 * 1024 * 1024) // larger than 3M
            {
                try
                {
                    List<string> lns = new List<string>();
                    using (FileStream cefLogStream = new FileStream(cefLog, FileMode.Open))
                    {
                        // only keep the last 100K
                        cefLogStream.Seek(fileSize - 100 * 1024, SeekOrigin.Begin);
                        using (StreamReader sr = new StreamReader(cefLogStream))
                        {
                            sr.ReadLine(); // drop the half-backed line
                            string ln;
                            while (null != (ln = sr.ReadLine()))
                            {
                                lns.Add(ln);
                            }
                        }
                    }
                    using (StreamWriter sw = new StreamWriter(cefLog))
                    {
                        foreach (string ln in lns)
                        {
                            sw.WriteLine(ln);
                        }
                    }
                }
                catch (Exception e)
                {
                    _log.InfoFormat("Failed to handle cef log: {0}, exception: {1}", cefLog, e);
                }
            }
        }

        #endregion


    }
}
