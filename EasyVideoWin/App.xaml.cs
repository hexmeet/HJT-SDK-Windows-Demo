using EasyVideoWin.CustomControls;
using EasyVideoWin.Helpers;
using EasyVideoWin.Model;
using log4net;
using System;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows;
using System.Windows.Threading;
using static EasyVideoWin.Helpers.Utils;

namespace EasyVideoWin
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private const string JOIN_CONF_REGEX = @"^--url={0}://(.+)/(.+)\?(.+)$";
        /// <summary>The event mutex name.</summary>
        private const string UniqueEventName = "1CDF4DAF-EB94-4D31-8A3D-08BF736FC7E2";

        /// <summary>The unique mutex name.</summary>
        private const string UniqueMutexName = "CA22CBFD-E39B-4DDF-BA37-EC25A9785AB6";

        /// <summary>The event wait handle.</summary>
        private EventWaitHandle eventWaitHandle;

        /// <summary>The mutex.</summary>
        private Mutex mutex;

        private string _anonymousJoinConfType;
        private string _anonymousJoinConfServerAddress;
        private string _anonymousJoinConfId;
        private string _anonymousJoinConfServerProtocol;
        private string _anonymousJoinConfPassword = "";
        private int _anonymousJoinConfServerPort = 80;
        
        /// <summary>The app on startup.</summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        private void CheckSingleInstance(string[] args)
        {
            bool isOwned;
            this.mutex = new Mutex(true, UniqueMutexName, out isOwned);
            this.eventWaitHandle = new EventWaitHandle(false, EventResetMode.AutoReset, UniqueEventName);

            // So, R# would not give a warning that this variable is not used.
            GC.KeepAlive(this.mutex);

            if (isOwned)
            {
                log4net.Config.XmlConfigurator.Configure();
                log.InfoFormat("=============== {0} is Starting =============", System.Reflection.Assembly.GetExecutingAssembly().GetName().Name);
                log.InfoFormat("=============== Version - {0}, process name: {1} =============", Utils.GetEdition(), Process.GetCurrentProcess().ProcessName);

                ParseStartupArgs(args, log);

                // Spawn a thread which will be waiting for our event
                var thread = new Thread(
                    () =>
                    {
                        while (this.eventWaitHandle.WaitOne())
                        {
                            OnReceiveEventHandle();
                        }
                    });

                // It is important mark it as background otherwise it will prevent app from exiting.
                thread.IsBackground = true;
                thread.Start();
                SaveJoinConfData();
                return;
            }

            HandleConflict(args);
        }

        private void OnReceiveEventHandle()
        {
            log.Info("Receive event handle");
            Current.Dispatcher.InvokeAsync((() => ((MainWindow)Current.MainWindow).BringToForeground()));

            // join conf
            string joinConfAddress = Utils.GetAnonymousJoinConfServerAddress();
            string joinConfId = Utils.GetAnonymousJoinConfId();
            string joinConfPassword = Utils.GetAnonymousJoinConfPassword();
            log.InfoFormat("Join conf address: {0}, conf id: {1}", joinConfAddress, joinConfId);
            if (string.IsNullOrEmpty(joinConfAddress) || string.IsNullOrEmpty(joinConfId))
            {
                log.Info("Empty conf address or conf id.");
                return;
            }

            LoginStatus status = LoginManager.Instance.CurrentLoginStatus;
            log.InfoFormat("Login status: {0}", status);
            if (LoginStatus.LoggedIn == status)
            {
                log.InfoFormat("Call status: {0}", CallController.Instance.CurrentCallStatus);
                if ((CallStatus.Idle == CallController.Instance.CurrentCallStatus || CallStatus.Ended == CallController.Instance.CurrentCallStatus))
                {
                    if (Utils.GetAnonymousLogoutAndAnonymousJoinConf())
                    {
                        log.Info("Logout and prepare to join conf anonymously.");
                        Application.Current.Dispatcher.InvokeAsync(() => {
                            CloseMessageTip();
                            LoginManager.Instance.Logout();
                        });
                    }
                    else
                    {
                        Utils.SetAnonymousJoinConfServerAddress("");
                        Utils.SetAnonymousJoinConfId("");
                        Utils.SetAnonymousJoinConfPassword("");
                        log.Info("Dial out to conf.");
                        Application.Current.Dispatcher.InvokeAsync(() => {
                            CloseMessageTip();
                            CallController.Instance.JoinConference(joinConfId, "", joinConfPassword);
                        });
                    }
                }
                else
                {
                    log.Info("In calling and do not dial out.");
                }
            }
            else if (LoginStatus.NotLogin == status || LoginStatus.LoginFailed == status)
            {
                log.Info("Not login and need anonymous join conf");
                Application.Current.Dispatcher.InvokeAsync(() => {
                    CloseMessageTip();
                    //LoginManager.Instance.ServiceType = Utils.ServiceTypeEnum.Enterprise;
                    //LoginManager.Instance.LoginProgress = LoginProgressEnum.EnterpriseJoinConf;
                    LoginManager.Instance.ServiceType = Utils.ServiceTypeEnum.None;
                    LoginManager.Instance.LoginProgress = LoginProgressEnum.Idle;
                    LoginManager.Instance.IsNeedAnonymousJoinConf = true;
                });
            }
        }

        // close message tip to avoid the modal dialog prevent some oprations such as the opration of video bar can not be displayed 
        private void CloseMessageTip()
        {
            MainWindow mainWindow = Application.Current.MainWindow as MainWindow;
            if (null != mainWindow)
            {
                mainWindow.CloseMessageBoxTip();
            }
        }

        private void HandleConflict(string[] args)
        {
            Utils.ParseCloudServerAddress();
            ILog conflictLogger = CreateConflictLogger();
            conflictLogger.Info("Another instance of the app is running.");
            ParseStartupArgs(args, conflictLogger);
            // save join conf args
            if (!string.IsNullOrEmpty(_anonymousJoinConfServerAddress))
            {
                conflictLogger.InfoFormat("join conf server address: {0}", _anonymousJoinConfServerAddress);
                bool saveConfArgs = true;
                LoginStatus loginStatus = Utils.GetLoginStatus();
                conflictLogger.InfoFormat("login status: {0}", loginStatus);
                if (LoginStatus.LoggingIn == loginStatus || LoginStatus.AnonymousLoggingIn == loginStatus)
                {
                    conflictLogger.Info("Another process of the application is logging in.");
                    //MessageBox.Show("Another process of the application is logging in. Please close it and rejoin the conference.");
                    MessageBox.Show(LanguageUtil.Instance.GetValueByKey("ANOTHER_APP_LOGGIN"));
                    saveConfArgs = false;
                }
                else if (LoginStatus.LoggedIn == loginStatus || LoginStatus.AnonymousLoggedIn == loginStatus)
                {
                    ServiceTypeEnum serviceType = Utils.GetServiceType();
                    string serverAddress = Utils.GetServerAddress(serviceType);
                    if (_anonymousJoinConfServerAddress.Equals(serverAddress))
                    {
                        bool isConfRunning = Utils.GetIsConfRunning();
                        if (isConfRunning)
                        {
                            string confId = Utils.GetRunningConfId();
                            if (!confId.Equals(_anonymousJoinConfId))
                            {
                                conflictLogger.InfoFormat("Another conference is running on the application. Running conf id:{0}, current conf id:{1}", confId, _anonymousJoinConfId);
                                //MessageBox.Show("Another conference is running on the application. Please terminate the conference and rejoin the conference.");
                                MessageBox.Show(LanguageUtil.Instance.GetValueByKey("ANOTHER_CONF_RUNNING"));
                            }
                            saveConfArgs = false;
                        }
                    }
                    else
                    {
                        conflictLogger.InfoFormat("The application has been logged in with another address. Logged server address: {0}, current address: {1}", serverAddress, _anonymousJoinConfServerAddress);
                        bool isConfRunning = Utils.GetIsConfRunning();
                        if (isConfRunning)
                        {
                            conflictLogger.InfoFormat("There is a conference running on the other server.");
                            MessageBox.Show(LanguageUtil.Instance.GetValueByKey("ANOTHER_CONF_RUNNING"));
                            saveConfArgs = false;
                        }
                        else
                        {
                            log.Info("Set SetAnonymousLogoutAndAnonymousJoinConf to true.");
                            //MessageBox.Show("The application has been logged in with another address. Please log out and rejoin the conference.");
                            //MessageBox.Show(LanguageUtil.Instance.GetValueByKey("APP_LOGGED_WITH_ANOTHER_ADDRESS"));
                            //saveConfArgs = false;
                            Utils.SetAnonymousLogoutAndAnonymousJoinConf(true);
                        }
                    }
                }
                if (saveConfArgs)
                {
                    conflictLogger.Info("Save join conf data");
                    SaveJoinConfData();
                }
            }

            conflictLogger.Info("notify another instance and exit.");
            // Notify other instance so it could bring itself to foreground.
            this.eventWaitHandle.Set();

            // Terminate this instance.
            //this.Shutdown();
            System.Environment.Exit(1);
        }

        private ILog CreateConflictLogger()
        {
            log4net.Appender.RollingFileAppender appender = new log4net.Appender.RollingFileAppender();
            appender.AppendToFile = true;
            appender.File = Utils.GetConflictLogFileName();
            appender.ImmediateFlush = true;
            appender.LockingModel = new log4net.Appender.FileAppender.MinimalLock();
            appender.RollingStyle = log4net.Appender.RollingFileAppender.RollingMode.Size;
            appender.MaxFileSize = 10 * 1024 * 1024;
            appender.MaxSizeRollBackups = 5;
            
            ///layout
            log4net.Layout.PatternLayout layout = new log4net.Layout.PatternLayout("%date [%02thread] %-5level %message%newline");

            appender.Layout = layout;
            appender.ActivateOptions();
            log4net.Repository.ILoggerRepository repository = log4net.LogManager.CreateRepository("ConflictRepository");

            log4net.Config.BasicConfigurator.Configure(repository, appender);
            ILog logger = log4net.LogManager.GetLogger(repository.Name, "ConflictLog");

            return logger;
        }

        private void SaveJoinConfData()
        {
            Utils.SetAnonymousJoinConfType(_anonymousJoinConfType);
            Utils.SetAnonymousJoinConfServerAddress(_anonymousJoinConfServerAddress);
            Utils.SetAnonymousJoinConfId(_anonymousJoinConfId);
            Utils.SetAnonymousJoinConfPassword(_anonymousJoinConfPassword);
            Utils.SetAnonymousJoinConfServerProtocol(_anonymousJoinConfServerProtocol);
            Utils.SetAnonymousJoinConfServerPort(_anonymousJoinConfServerPort);
        }

        void AppDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            ShowUnhandledException(e);

            //#if DEBUG   // In debug mode do not custom-handle the exception, let Visual Studio handle it

            //e.Handled = false;

            //#else

            //ShowUnhandledException(e);    

            //#endif     
        }

        void ShowUnhandledException(DispatcherUnhandledExceptionEventArgs e)
        {
            string procName = Process.GetCurrentProcess().ProcessName;
            using (PerformanceCounter pc = new PerformanceCounter("Process", "Private Bytes", procName))
            {
                log.InfoFormat("Current memory used: {0}", pc.NextValue());
            }

            log.Error("App ShowUnhandledException: " + e.Exception);
            e.Handled = true;

            string errorMessage = string.Format("An application error occurred.\n\nError: {0}",

            e.Exception.Message + (e.Exception.InnerException != null ? "\n" +
            e.Exception.InnerException.Message : null));
            
            try
            {
                MessageBoxTip tip;
                MainWindow mainWindow = Current.MainWindow as MainWindow;
                if (null == mainWindow)
                {
                    tip = new MessageBoxTip(null);
                }
                else
                {
                    tip = new MessageBoxTip(mainWindow.GetCurrentMainDisplayWindow());
                }
                
                tip.SetTitleAndMsg(LanguageUtil.Instance.GetValueByKey("PROMPT"), errorMessage, LanguageUtil.Instance.GetValueByKey("CONFIRM"));
                tip.ShowDialog();
            }
            catch(Exception ex)
            {
                log.Error("Failed to ShowUnhandledException, detail: " + ex);
            }

            CefSharpUtil.ShutdownCefSharp();
            View.LayoutBackgroundWindow.Instance.Dispose();
            Application.Current.Shutdown();
        }
        
        private void ParseStartupArgs(string[] args, ILog logger)
        {
            logger.InfoFormat("args length: {0}", args.Length);
            for (int i = 0; i < args.Length; i++)
            {
                logger.InfoFormat("Args[{0}]:{1}", i, args[i]);
            }

            if (args.Length > 0)
            {
                string joinConfRegex = string.Format(JOIN_CONF_REGEX, EasyVideoWin.Properties.Settings.Default.URLProtocol);
                Match mat = Regex.Match(args[0], joinConfRegex);
                if (mat.Success)
                {
                    logger.Info("The args[0] is the join conf url and parse the arg.");
                    _anonymousJoinConfServerAddress = mat.Groups[1].Value;
                    _anonymousJoinConfType = mat.Groups[2].Value;
                    string query = mat.Groups[3].Value;
                    string[] queries = query.Split('&');
                    for (int i = 0; i < queries.Length; ++i)
                    {
                        string[] items = queries[i].Split('=');
                        if (items.Length != 2)
                        {
                            continue;
                        }

                        if (items[0].Equals("confid"))
                        {
                            _anonymousJoinConfId = items[1];
                        }
                        else if (items[0].Equals("password"))
                        {
                            _anonymousJoinConfPassword = items[1];
                        }
                        else if (items[0].Equals("protocol"))
                        {
                            _anonymousJoinConfServerProtocol = items[1];
                        }
                        if (items[0].Equals("port"))
                        {
                            try
                            {
                                _anonymousJoinConfServerPort = int.Parse(items[1]);
                            }
                            catch(Exception e)
                            {
                                _anonymousJoinConfServerPort = 0;
                            }
                        }
                    }

                    if (0 == _anonymousJoinConfServerPort)
                    {
                        _anonymousJoinConfServerPort = "https" == _anonymousJoinConfServerProtocol ? 443 : 80;
                    }
                }
                else
                {
                    logger.Info("The args[0] is not the join conf url.");
                }
            }
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            try
            {
                // when startup up, then init language
                LanguageUtil.Instance.InitLanguage();
                CheckSingleInstance(e.Args);
                
                // Global exception handling  
                Application.Current.DispatcherUnhandledException += new DispatcherUnhandledExceptionEventHandler(AppDispatcherUnhandledException);
                
                this.ShutdownMode = System.Windows.ShutdownMode.OnMainWindowClose;
                base.OnStartup(e);
                log.Info("OnStartup end");
            }
            catch (Exception e1)
            {
                log.Error("App OnStartup: " + e1);
            }
        }
        
        static App()
        {
        //   DpiUtil.Load_DPI_Aware();
        }
    }
}
