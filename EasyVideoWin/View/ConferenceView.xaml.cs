using CefSharp;
using CefSharp.WinForms;
using EasyVideoWin.Helpers;
using EasyVideoWin.Model;
using EasyVideoWin.Model.CloudModel;
using EasyVideoWin.WinForms;
using log4net;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace EasyVideoWin.View
{
    /// <summary>
    /// Interaction logic for ConferenceView.xaml
    /// </summary>
    public partial class ConferenceView : UserControl, IDisposable
    {
        #region -- Members --

        private readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private const string CONF_INFO_URL = "{0}/webapp/#/conferences?token={1}&userId={2}&lang={3}&orgPortAllocMode={4}&orgPortCount={5}"; // 0: server address
        private ConferenceJsEvent _conferenceJsEvent = null;

        private EasyVideoWin.CustomControls.BrowserLoadingPanel _loadingView = new EasyVideoWin.CustomControls.BrowserLoadingPanel();
        private EasyVideoWin.CustomControls.BrowserLoadFailedPanel _loadFailedView = new EasyVideoWin.CustomControls.BrowserLoadFailedPanel();
        private System.Timers.Timer _browserLoadTimer = new System.Timers.Timer();

        #endregion

        #region -- Properties --

        public bool IsDisposed { get; private set; } = false;

        #endregion

        #region -- Constructor --

        public ConferenceView()
        {
            InitializeComponent();

            //this.Browser = (this.wfh.Child as WhiteBoardForm).browser;
            //this.Browser.RegisterJsObject("conferenceObj", new ConferenceJsEvent(), new BindingOptions() { CamelCaseJavascriptNames = false });
            //this.Browser.MenuHandler = new CustomMenuHandler();
            //this.Browser.RequestHandler = new ConfCefRequestHandler();
            //this.Browser.ConsoleMessage += OnBrowserConsoleMessage;
            //this.Browser.LoadError += Browser_LoadError;
            //this.Browser.IsBrowserInitializedChanged += Browser_IsBrowserInitializedChanged;

            this.browser.Visibility = Visibility.Collapsed;
            this.contentPresenter.Content = _loadingView;
            this.contentPresenter.Visibility = Visibility.Visible;

            _browserLoadTimer.Interval = 20 * 1000;
            _browserLoadTimer.AutoReset = false;
            _browserLoadTimer.Elapsed += BrowserLoadTimer_Elapsed;

            _loadFailedView.ReloadEvent += LoadFailedView_ReloadEvent;
            
            _conferenceJsEvent = new ConferenceJsEvent(this.browser, (MainWindow)Application.Current.MainWindow);
            _conferenceJsEvent.PageCreatedEvent += ConferenceJsEvent_PageCreatedEvent;
            this.browser.RegisterJsObject("conferenceObj", _conferenceJsEvent, new BindingOptions() { CamelCaseJavascriptNames = false });
            //this.browser.JavascriptObjectRepository.Register("conferenceObj", _conferenceJsEvent, true, new BindingOptions() { CamelCaseJavascriptNames = false });
            this.browser.MenuHandler = new CustomMenuHandler();
            this.browser.RequestHandler = new ConfCefRequestHandler();
            this.browser.ConsoleMessage += OnBrowserConsoleMessage;
            this.browser.LoadError += Browser_LoadError;
            this.browser.IsBrowserInitializedChanged += Browser_IsBrowserInitializedChanged;
            this.browser.Loaded += Browser_Loaded;

            this.Loaded += ConferenceView_Loaded;
            this.Unloaded += ConferenceView_Unloaded;
            LoginManager.Instance.PropertyChanged += LoginManager_PropertyChanged;
        }
        
        #endregion

        #region -- Public Method --

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected void Dispose(bool disposing)
        {
            if (!IsDisposed)
            {
                if (disposing)
                {
                    //Clean Up managed resources
                    LoginManager.Instance.PropertyChanged -= LoginManager_PropertyChanged;
                    Application.Current.Dispatcher.Invoke(() => {
                        this.browser.Dispose();
                    });
                }
                //Clean up unmanaged resources  
            }
            IsDisposed = true;
        }
        #endregion

        #region -- Private Method --

        private void BrowserLoadTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            Application.Current.Dispatcher.Invoke(() => {
                this.contentPresenter.Content = _loadFailedView;
            });
        }

        private void LoadFailedView_ReloadEvent(object sender, RoutedEventArgs e)
        {
            this.contentPresenter.Content = _loadingView;
            this.browser.Reload(true);
            _browserLoadTimer.Enabled = true;
        }

        private void ConferenceJsEvent_PageCreatedEvent()
        {
            _browserLoadTimer.Enabled = false;
            Application.Current.Dispatcher.Invoke(() => {
                this.contentPresenter.Visibility = Visibility.Collapsed;
                this.browser.Visibility = Visibility.Visible;
            });
        }

        private void Browser_IsBrowserInitializedChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if ("IsBrowserInitialized" == e.Property.Name)
            {
                bool initialized = (bool)(e.NewValue);
                if (!initialized)
                {
                    return;
                }
            }

            System.Timers.Timer timerBrowserReload = new System.Timers.Timer();
            timerBrowserReload.Elapsed += new System.Timers.ElapsedEventHandler((object source, System.Timers.ElapsedEventArgs args) =>
            {
                Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    // For unknow reason, the browser load a blank page in initialization, so load again after a deferred time
                    ((CefSharp.Wpf.ChromiumWebBrowser)sender).Load(GetConferenceUrl());
                    _browserLoadTimer.Enabled = true;
                });
            });
            timerBrowserReload.Interval = 10;
            timerBrowserReload.Enabled = true;
            timerBrowserReload.AutoReset = false;
        }

        private void Browser_LoadError(object sender, LoadErrorEventArgs e)
        {
            // ConnectionTimedOut  ConnectionRefused
            log.InfoFormat("Error occurred when loading conference view browser. Error code:{0}, failed url:{1}", e.ErrorCode, e.FailedUrl);
        }
        
        private void JoinConf_Click(object sender, RoutedEventArgs e)
        {
            //JoinConfWindow joinConfWindow = new JoinConfWindow();
            //joinConfWindow.ShowDialog();
        }

        private void ConferenceView_Loaded(object sender, RoutedEventArgs e)
        {
            if (this.browser.IsBrowserInitialized)
            {
                this.browser.Load(GetConferenceUrl());
                _browserLoadTimer.Enabled = true;
            }
        }
        
        private void ConferenceView_Unloaded(object sender, RoutedEventArgs e)
        {
            if (this.browser.IsBrowserInitialized)
            {
                //this.browser.Load(GetConferenceUrl());
                this.browser.Visibility = Visibility.Collapsed;
                _conferenceJsEvent.ClearConfListCache();
            }
        }

        private void Browser_Loaded(object sender, RoutedEventArgs e)
        {
            this.browser.Visibility = Visibility.Visible;
        }

        private void OnBrowserConsoleMessage(object sender, ConsoleMessageEventArgs args)
        {
            log.InfoFormat("Browser console log: {0}", args.Message);
        }

        private string GetConferenceUrl()
        {
            string address = Properties.Settings.Default.ConfInfoAddress;
            if (string.IsNullOrEmpty(address))
            {
                address = string.Format(
                    CONF_INFO_URL
                    , LoginManager.Instance.LoginUserInfo.customizedH5UrlPrefix
                    , LoginManager.Instance.LoginToken
                    , LoginManager.Instance.UserId
                    , LanguageUtil.Instance.GetCurrentWebLanguage()
                    , LoginManager.Instance.LoginUserInfo.orgPortAllocMode
                    , LoginManager.Instance.LoginUserInfo.orgPortCount
                );
            }

            log.InfoFormat("Conference info url: {0}", address);
            return address;
        }

        private void LoginManager_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if ("LoginToken" == e.PropertyName)
            {
                if (string.IsNullOrEmpty(LoginManager.Instance.LoginToken) || null == _conferenceJsEvent)
                {
                    return;
                }

                _conferenceJsEvent.UpdateToken(LoginManager.Instance.LoginToken);
            }
        }

        #endregion
    }

    public class ConferenceJsEvent
    {
        private readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private const string USER_DEFAULT_MAIL_KEY = @"Software\Microsoft\Windows\Shell\Associations\UrlAssociations\mailto\UserChoice";
        private const string FOXMAIL_PROG_ID = "foxmail.Url.mailto";
        
        public string MessageText = string.Empty;
        private CefSharp.Wpf.ChromiumWebBrowser _browser;
        public delegate void PageCreatedHandler();
        public event PageCreatedHandler PageCreatedEvent;

        private IMasterDisplayWindow _masterDisplayWindow;

        private ConferenceJsEvent()
        {

        }

        public ConferenceJsEvent(CefSharp.Wpf.ChromiumWebBrowser browser, IMasterDisplayWindow masterDisplayWindow)
        {
            this._browser = browser;
            this._masterDisplayWindow = masterDisplayWindow;
        }

        public void JoinConf(string confNumber, string confPassword)
        {
            log.Info("Join conf is called.");
            Application.Current.Dispatcher.InvokeAsync(() => {
                log.Info("Begin to show join conf dialog.");
                JoinConfWindow joinConfWindow = new JoinConfWindow(confNumber, confPassword);
                joinConfWindow.Owner = Application.Current.MainWindow;
                joinConfWindow.ShowDialog();
            });
            //string str = string.Format("conf number: {0}, display name: {1}", confNumber, confPassword);
            //MessageBox.Show(str);
        }

        public void TokenExpired()
        {
            log.Info("Browser report token expired.");
            LoginManager.Instance.UpdateLoginToken(true);
        }
        
        public void UpdateToken(string token)
        {
            if (null == this._browser.GetBrowser() || null == this._browser.GetBrowser().GetFrame(null))
            {
                return;
            }
            log.InfoFormat("Update token to browser, token={0}", token);
            string updateToken = "window.updateToken('" + token + "')";
            this._browser.GetBrowser().GetFrame(null).ExecuteJavaScriptAsync(updateToken);
        }

        public void Go2Reservation(string url)
        {
            Application.Current.Dispatcher.Invoke(() => {
                System.Diagnostics.Process.Start(url);
            });
        }

        public void ClearConfListCache()
        {
            if (null == this._browser.GetBrowser() || null == this._browser.GetBrowser().GetFrame(null))
            {
                return;
            }
            string cmd = "window.clearConfListCache()";
            this._browser.GetBrowser().GetFrame(null).ExecuteJavaScriptAsync(cmd);
        }

        public void ClearConfManagementCache()
        {
            if (null == this._browser.GetBrowser() || null == this._browser.GetBrowser().GetFrame(null))
            {
                return;
            }
            string cmd = "window.clearConfCtrlCache()";
            this._browser.GetBrowser().GetFrame(null).ExecuteJavaScriptAsync(cmd);
        }

        public void UpdateData()
        {
            if (null == this._browser.GetBrowser() || null == this._browser.GetBrowser().GetFrame(null))
            {
                return;
            }
            string cmd = "window.reloadDataByApp()";
            this._browser.GetBrowser().GetFrame(null).ExecuteJavaScriptAsync(cmd);
        }

        public void SendEmail(string email)
        {
            if (IsFoxmailAsDefaultMail())
            {
                System.Diagnostics.Process.Start("mailto:?");
                Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    const string mailtoFlag = "mailto:?";
                    const string subjectFlag = "subject=";
                    const string bodyFlag = "body=";
                    if (0 != email.IndexOf(mailtoFlag))
                    {
                        log.Info("mailto flag is not correct.");
                        return;
                    }

                    string mailToData = email.Substring(mailtoFlag.Length);
                    string subject = "";
                    string body = "";
                    string[] toDataItems = mailToData.Split('&');
                    for (var i = 0; i < toDataItems.Length; ++i)
                    {
                        if (0 == toDataItems[i].IndexOf(subjectFlag))
                        {
                            subject = toDataItems[i].Substring(subjectFlag.Length);
                        }
                        else if (0 == toDataItems[i].IndexOf(bodyFlag))
                        {
                            body = toDataItems[i].Substring(bodyFlag.Length);
                        }
                    }
                    body = body.Replace("%0A", Environment.NewLine).Replace("%26", "&");
                    EmailClientNotSupportPrompt prompt = new EmailClientNotSupportPrompt();
                    System.Windows.Rect mainWindowRect = _masterDisplayWindow.GetWindowRect();
                    prompt.Left = mainWindowRect.Left + (mainWindowRect.Width - prompt.Width) / 2;
                    prompt.Top = mainWindowRect.Top + (mainWindowRect.Height - prompt.Height) / 2;
                    prompt.WindowStartupLocation = WindowStartupLocation.Manual;
                    prompt.Owner = _masterDisplayWindow.GetWindow();
                    prompt.SetSubjectAndBody(subject, body);
                    prompt.ShowDialog();
                });
            }
            else
            {
                System.Diagnostics.Process.Start(email);
            }
        }

        public void PageCreated()
        {
            PageCreatedEvent?.Invoke();
        }

        public void showTest()
        {
            MessageBox.Show("this in C#.\n\r");
        }

        private bool IsFoxmailAsDefaultMail()
        {
            RegistryKey key = Registry.CurrentUser.OpenSubKey(USER_DEFAULT_MAIL_KEY, RegistryKeyPermissionCheck.ReadSubTree);
            if (null == key)
            {
                return false;
            }

            try
            {
                object defaultEmail = key.GetValue("ProgId");
                if (null == defaultEmail)
                {
                    return false;
                }

                string email = (string)defaultEmail;
                log.InfoFormat("default mail prog id:{0}", email);
                if (email.Equals(FOXMAIL_PROG_ID))
                {
                    return true;
                }
            }
            catch
            {
                return false;
            }

            return false;
        }
    }

    public class ConfCefRequestHandler : IRequestHandler
    {
        private readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public bool CanGetCookies(IWebBrowser chromiumWebBrowser, IBrowser browser, IFrame frame, IRequest request)
        {
            return true;
        }

        public bool CanSetCookie(IWebBrowser chromiumWebBrowser, IBrowser browser, IFrame frame, IRequest request, Cookie cookie)
        {
            return true;
        }

        public bool OnBeforeBrowse(IWebBrowser chromiumWebBrowser, IBrowser browser, IFrame frame, IRequest request, bool userGesture, bool isRedirect)
        {
            return false;
        }

        bool IRequestHandler.GetAuthCredentials(IWebBrowser browserControl, IBrowser browser, IFrame frame, bool isProxy, string host, int port, string realm, string scheme, IAuthCallback callback)
        {
            return false;
        }

        IResponseFilter IRequestHandler.GetResourceResponseFilter(IWebBrowser browserControl, IBrowser browser, IFrame frame, IRequest request, IResponse response)
        {
            return null;
        }

        CefReturnValue IRequestHandler.OnBeforeResourceLoad(IWebBrowser browserControl, IBrowser browser, IFrame frame, IRequest request, IRequestCallback callback)
        {
            return CefReturnValue.Continue;
        }

        bool IRequestHandler.OnCertificateError(IWebBrowser browserControl, IBrowser browser, CefErrorCode errorCode, string requestUrl, ISslInfo sslInfo, IRequestCallback callback)
        {
            if (browser.IsDisposed)
            {
                return false;
            }

            if (!callback.IsDisposed)
            {
                using (callback)
                {
                    //To allow certificate
                    callback.Continue(true);
                    return true;
                }
            }

            return false;
        }

        bool IRequestHandler.OnOpenUrlFromTab(IWebBrowser browserControl, IBrowser browser, IFrame frame, string targetUrl, WindowOpenDisposition targetDisposition, bool userGesture)
        {
            return false;
        }

        void IRequestHandler.OnPluginCrashed(IWebBrowser browserControl, IBrowser browser, string pluginPath)
        {
            throw new NotImplementedException();
        }

        bool IRequestHandler.OnProtocolExecution(IWebBrowser browserControl, IBrowser browser, string url)
        {
            return false;
        }

        bool IRequestHandler.OnQuotaRequest(IWebBrowser browserControl, IBrowser browser, string originUrl, long newSize, IRequestCallback callback)
        {
            return false;
        }

        void IRequestHandler.OnRenderProcessTerminated(IWebBrowser browserControl, IBrowser browser, CefTerminationStatus status)
        {
            if (browser.IsDisposed)
            {
                return;
            }

            log.Debug("the reder process terminated!");
            browser.Reload(true);
        }

        void IRequestHandler.OnRenderViewReady(IWebBrowser browserControl, IBrowser browser)
        {

        }

        void IRequestHandler.OnResourceLoadComplete(IWebBrowser browserControl, IBrowser browser, IFrame frame, IRequest request, IResponse response, UrlRequestStatus status, long receivedContentLength)
        {
            if (browser.IsDisposed || null == browser.MainFrame)
            {
                return;
            }

            try
            {
                browser.MainFrame.ExecuteJavaScriptAsync("document.body.style.overflow = 'hidden'");
            }
            catch(Exception e)
            {
                log.InfoFormat("Exception on OnResourceLoadComplete, error:{0}", e.Message);
            }
        }

        void IRequestHandler.OnResourceRedirect(IWebBrowser browserControl, IBrowser browser, IFrame frame, IRequest request, IResponse response, ref string newUrl)
        {

        }

        bool IRequestHandler.OnResourceResponse(IWebBrowser browserControl, IBrowser browser, IFrame frame, IRequest request, IResponse response)
        {
            if (browser.IsDisposed)
            {
                return false;
            }

            int statusCode = response.StatusCode;
            return false;
        }

        bool IRequestHandler.OnSelectClientCertificate(IWebBrowser browserControl, IBrowser browser, bool isProxy, string host, int port, X509Certificate2Collection certificates, ISelectClientCertificateCallback callback)
        {
            return false;
        }
    }
}
