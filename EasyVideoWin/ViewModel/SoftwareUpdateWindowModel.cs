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
using EasyVideoWin.Model.CloudModel;
using System.IO;

namespace EasyVideoWin.ViewModel
{
    class SoftwareUpdateWindowModel : ViewModelBase
    {
        #region -- Members --

        private readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        
        private enum SoftwareUpdateProgressEnum
        {
            NewVersionFound
            , Downloading
            , Downloaded
        }

        private SoftwareUpdateProgressEnum _softwareUpdateProgress;
        private string _promptContent;
        private double _downloadPercent;
        private string _downloadCount;
        private Visibility _downloadPercentVisibility;
        private Visibility _downloadCountVisibility;
        private string _closeButtonText;
        private string _confirmButtonText;
        private Visibility _confirmButtonVisibility;
        private string _version;
        private string _downloadUrl;
        private string _fullDownloadedFileName;
        private WebClient _webClient = null;
        private bool _cancelDownload = false;

        #endregion

        #region -- Properties --

        public string PromptContent
        {
            get
            {
                return _promptContent;
            }
            set
            {
                _promptContent = value;
                OnPropertyChanged("PromptContent");
            }
        }

        public double DownloadPercent
        {
            get
            {
                return _downloadPercent;
            }
            set
            {
                _downloadPercent = value;
                OnPropertyChanged("DownloadPercent");
            }
        }

        public string DownloadCount
        {
            get
            {
                return _downloadCount;
            }
            set
            {
                _downloadCount = value;
                OnPropertyChanged("DownloadCount");
            }
        }

        public Visibility DownloadPercentVisibility
        {
            get
            {
                return _downloadPercentVisibility;
            }
            set
            {
                _downloadPercentVisibility = value;
                OnPropertyChanged("DownloadPercentVisibility");
            }
        }

        public Visibility DownloadCountVisibility
        {
            get
            {
                return _downloadCountVisibility;
            }
            set
            {
                _downloadCountVisibility = value;
                OnPropertyChanged("DownloadCountVisibility");
            }
        }

        public string CloseButtonText
        {
            get
            {
                return _closeButtonText;
            }
            set
            {
                _closeButtonText = value;
                OnPropertyChanged("CloseButtonText");
            }
        }

        public string ConfirmButtonText
        {
            get
            {
                return _confirmButtonText;
            }
            set
            {
                _confirmButtonText = value;
                OnPropertyChanged("ConfirmButtonText");
            }
        }

        public Visibility ConfirmButtonVisibility
        {
            get
            {
                return _confirmButtonVisibility;
            }
            set
            {
                _confirmButtonVisibility = value;
                OnPropertyChanged("ConfirmButtonVisibility");
            }
        }

        private SoftwareUpdateProgressEnum SoftwareUpdateProgress
        {
            get
            {
                return _softwareUpdateProgress;
            }
            set
            {
                _softwareUpdateProgress = value;
                OnSoftwareUpdateProgressChanged();
            }
        }

        #endregion

        #region -- Constructor --

        public SoftwareUpdateWindowModel()
        {
            SoftwareUpdateProgress = SoftwareUpdateProgressEnum.NewVersionFound;
        }

        #endregion

        #region -- Public Method --

        public void Reset(string version, string downloadUrl)
        {
            _version = version;
            _downloadUrl = downloadUrl;
            SoftwareUpdateProgress = SoftwareUpdateProgressEnum.NewVersionFound;
        }

        public void Cancel()
        {
            switch (SoftwareUpdateProgress)
            {
                case SoftwareUpdateProgressEnum.Downloading:
                    CancelDownloading();
                    break;
                case SoftwareUpdateProgressEnum.Downloaded:
                    RemoveDownloadedSoftware();
                    break;
            }
        }

        public bool IsLastestStep()
        {
            return SoftwareUpdateProgressEnum.Downloaded == SoftwareUpdateProgress;
        }

        public void Confirm()
        {
            switch (SoftwareUpdateProgress)
            {
                case SoftwareUpdateProgressEnum.NewVersionFound:
                    DownloadSoftware();
                    break;
                case SoftwareUpdateProgressEnum.Downloaded:
                    InstallSoftware();
                    break;
                default:
                    break;
            }
        }


        #endregion

        #region -- Private Method --

        private void DownloadSoftware()
        {
            CancelDownloading();
            _cancelDownload = false;
            SoftwareUpdateProgress = SoftwareUpdateProgressEnum.Downloading;
            
            if (null == _webClient)
            {
                _webClient = new WebClient();
                _webClient.DownloadFileCompleted += Client_DownloadFileCompleted;
                _webClient.DownloadProgressChanged += Client_DownloadProgressChanged;
            }

            int pos = _downloadUrl.LastIndexOf("/");
            string fileName = _downloadUrl.Substring(pos + 1);
            _fullDownloadedFileName = Path.Combine(Utils.GetInstallPackagePath(), fileName);
            log.InfoFormat("Download install package to: {0}", _fullDownloadedFileName);
            if (File.Exists(_fullDownloadedFileName))
            {
                File.Delete(_fullDownloadedFileName);
            }

            try
            {
                _webClient.DownloadFileAsync(new Uri(_downloadUrl), _fullDownloadedFileName, fileName);
            }
            catch (Exception ex)
            {
                log.InfoFormat("Failed to download install package, err:{0}", ex.Message);
            }
        }

        private void Client_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            DownloadPercent = e.ProgressPercentage;
            StringBuilder sb = new StringBuilder();
            sb.Append((e.BytesReceived / (1024.0 * 1024.0)).ToString("0.00"))
                .Append("MB / ")
                .Append((e.TotalBytesToReceive / (1024.0 * 1024.0)).ToString("0.00"))
                .Append("MB");
            DownloadCount = sb.ToString();
        }

        private void Client_DownloadFileCompleted(object sender, AsyncCompletedEventArgs e)
        {
            log.InfoFormat("Download file completed. Canclled:{0}", _cancelDownload);
            if (_cancelDownload)
            {
                RemoveDownloadedSoftware();
            }
            SoftwareUpdateProgress = SoftwareUpdateProgressEnum.Downloaded;
        }

        private void InstallSoftware()
        {
            System.Diagnostics.Process.Start(_fullDownloadedFileName);
        }

        private void CancelDownloading()
        {
            if (null != _webClient && _webClient.IsBusy)
            {
                log.Info("Cacel download install package.");
                _webClient.CancelAsync();
                _cancelDownload = true;
            //    RemoveDownloadedSoftware();
            }
        }
        
        private void RemoveDownloadedSoftware()
        {
            log.Info("Remove downloaded install package.");
            if (File.Exists(_fullDownloadedFileName))
            {
                try
                {
                    File.Delete(_fullDownloadedFileName);
                    log.Info("Install package deleted.");
                }
                catch (Exception e)
                {
                    log.InfoFormat("Failed to remove downloaded install package, exception:{0}", e);
                }
            }
        }

        private void OnSoftwareUpdateProgressChanged()
        {
            switch (SoftwareUpdateProgress)
            {
                case SoftwareUpdateProgressEnum.NewVersionFound:
                    PromptContent = string.Format(LanguageUtil.Instance.GetValueByKey("NEW_VERSION_SOFTWARE_FOUND"), _version);
                    DownloadPercentVisibility = Visibility.Collapsed;
                    DownloadCountVisibility = Visibility.Collapsed;
                    ConfirmButtonVisibility = Visibility.Visible;
                    CloseButtonText = LanguageUtil.Instance.GetValueByKey("CANCEL");
                    ConfirmButtonText = LanguageUtil.Instance.GetValueByKey("DOWNLOAD");
                    break;
                case SoftwareUpdateProgressEnum.Downloading:
                    PromptContent = LanguageUtil.Instance.GetValueByKey("DOWNLOADING_NEW_VERSION_SOFTWARE");
                    DownloadPercentVisibility = Visibility.Visible;
                    DownloadCountVisibility = Visibility.Visible;
                    ConfirmButtonVisibility = Visibility.Collapsed;
                    CloseButtonText = LanguageUtil.Instance.GetValueByKey("CANCEL");
                    break;
                case SoftwareUpdateProgressEnum.Downloaded:
                    PromptContent = LanguageUtil.Instance.GetValueByKey("INSTALL_LATEST_SOFTWARE");
                    DownloadPercentVisibility = Visibility.Visible;
                    DownloadCountVisibility = Visibility.Collapsed;
                    ConfirmButtonVisibility = Visibility.Visible;
                    CloseButtonText = LanguageUtil.Instance.GetValueByKey("NO");
                    ConfirmButtonText = LanguageUtil.Instance.GetValueByKey("YES");
                    break;
            }
        }
        
        #endregion
    }
}
