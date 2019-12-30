using EasyVideoWin.Model;
using EasyVideoWin.Helpers;
using EasyVideoWin.ManagedEVSdk.Structs;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using log4net;


namespace EasyVideoWin.ViewModel
{
    class MediaStatisticsViewModel : INotifyPropertyChanged
    {
        private readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private Timer _refreshMediaStatisticsTimer = null;
        public List<MediaStatisticsItem> _mediaStatisticsItemList = new List<MediaStatisticsItem>();
        public List<MediaStatisticsItem> MediaStatisticsItemList
        {
            get
            {
                return this._mediaStatisticsItemList;
            }
            set
            {
                this._mediaStatisticsItemList = value;
                this.RaisePropertyChanged("MediaStatisticsItemList");
            }
        }

        public MediaStatisticsViewModel()
        {
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public void RaisePropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public void StartRefreshMediaStatistics()
        {
            if (null == _refreshMediaStatisticsTimer)
            {
                _refreshMediaStatisticsTimer = new Timer(1000);
                _refreshMediaStatisticsTimer.Elapsed += OnTimerRefreshMediaStatistics;
            }

            _refreshMediaStatisticsTimer.AutoReset = true;
            _refreshMediaStatisticsTimer.Enabled = true;
        }

        public void StopRefreshMediaStatistics()
        {
            _refreshMediaStatisticsTimer.Enabled = false;
        }

        private string TranslateResolution(EVVideoSizeCli videoSize)
        {
            if (null == videoSize)
            {
                return "-";
            }

            if (320 == videoSize.width && 180 == videoSize.height)
            {
                return "180p";
            }

            if (640 == videoSize.width && 360 == videoSize.height)
            {
                return "360p";
            }

            if (1280 == videoSize.width && 720 == videoSize.height)
            {
                return "720p";
            }

            if (1920 == videoSize.width && 1080 == videoSize.height)
            {
                return "1080p";
            }

            return string.Format("{0}x{1}", videoSize.width, videoSize.height);
        }

        private void OnTimerRefreshMediaStatistics(object source, ElapsedEventArgs e)
        {
            string notEncrypted     = LanguageUtil.Instance.GetValueByKey("NOT_ENCRYPTED");
            string encrypted        = LanguageUtil.Instance.GetValueByKey("ENCRYPTED");
            EVStatsCli mediaStats   = new EVStatsCli();
            EVSdkManager.Instance.GetStats(ref mediaStats);
            List<MediaStatisticsItem> tempList = new List<MediaStatisticsItem>();
            for (int i = 0; i < mediaStats.size; ++i)
            {
                EVStreamStatsCli streamStats = mediaStats.stats[i];
                MediaStatisticsItem item = new MediaStatisticsItem()
                {
                    Codec           = null != streamStats.payload_type ? streamStats.payload_type : "",
                    LineRate        = streamStats.nego_bandwidth.ToString(),
                    ActualLineRate  = Math.Round(streamStats.real_bandwidth, 1).ToString(),
                    Resolution      = EV_STREAM_TYPE_CLI.EV_STREAM_AUDIO == streamStats.type ? "-" : TranslateResolution(streamStats.resolution),
                    FrameRate       = EV_STREAM_TYPE_CLI.EV_STREAM_AUDIO == streamStats.type ? "-" : Math.Round(streamStats.fps, 1).ToString(),
                    PacketLossInfo  = streamStats.cum_packet_loss.ToString() + "(" + Math.Round(streamStats.packet_loss_rate, 1).ToString() + "%)",
                    Encrypted       = streamStats.is_encrypted ? encrypted : notEncrypted
                };

                switch (streamStats.type)
                {
                    case EV_STREAM_TYPE_CLI.EV_STREAM_AUDIO:
                        if (EV_STREAM_DIR_CLI.EV_STREAM_DOWNLOAD == streamStats.dir)
                        {
                            item.MediaOrientation = LanguageUtil.Instance.GetValueByKey("AUDIO_RECEPTION");
                        }
                        else
                        {
                            item.MediaOrientation = LanguageUtil.Instance.GetValueByKey("AUDIO_SENDING");
                        }
                        break;
                    case EV_STREAM_TYPE_CLI.EV_STREAM_VIDEO:
                        if (EV_STREAM_DIR_CLI.EV_STREAM_DOWNLOAD == streamStats.dir)
                        {
                            item.MediaOrientation = LanguageUtil.Instance.GetValueByKey("VIDEO_RECEPTION");
                        }
                        else
                        {
                            item.MediaOrientation = LanguageUtil.Instance.GetValueByKey("VIDEO_SENDING");
                        }
                        break;
                    case EV_STREAM_TYPE_CLI.EV_STREAM_CONTENT:
                        if (EV_STREAM_DIR_CLI.EV_STREAM_DOWNLOAD == streamStats.dir)
                        {
                            item.MediaOrientation = LanguageUtil.Instance.GetValueByKey("CONTENT_RECEPTION");
                        }
                        else
                        {
                            item.MediaOrientation = LanguageUtil.Instance.GetValueByKey("CONTENT_SENDING");
                        }
                        break;
                    case EV_STREAM_TYPE_CLI.EV_STREAM_WHITE_BOARD:
                        if (EV_STREAM_DIR_CLI.EV_STREAM_DOWNLOAD == streamStats.dir)
                        {
                            item.MediaOrientation = LanguageUtil.Instance.GetValueByKey("WHITE_BOARD_RECEPTION");
                        }
                        else
                        {
                            item.MediaOrientation = LanguageUtil.Instance.GetValueByKey("WHITE_BOARD_SENDING");
                        }
                        break;
                }
                tempList.Add(item);
            }
            MediaStatisticsItemList = tempList;
        }
    }

    public class MediaStatisticsItem : INotifyPropertyChanged
    {
        private string _mediaOrientation;
        public string MediaOrientation
        {
            get
            {
                return _mediaOrientation;
            }
            set
            {
                _mediaOrientation = value;
                NotifyPropertyChanged("MediaOrientation");
            }
        }

        private string _codec;
        public string Codec
        {
            get
            {
                return _codec;
            }
            set
            {
                _codec = value;
                NotifyPropertyChanged("Codec");
            }
        }

        private string _lineRate;
        public string LineRate
        {
            get
            {
                return _lineRate;
            }
            set
            {
                _lineRate = value;
                NotifyPropertyChanged("LineRate");
            }
        }

        private string _actualLineRate;
        public string ActualLineRate
        {
            get
            {
                return _actualLineRate;
            }
            set
            {
                _actualLineRate = value;
                NotifyPropertyChanged("ActualLineRate");
            }
        }

        private string _resolution;
        public string Resolution
        {
            get
            {
                return _resolution;
            }
            set
            {
                _resolution = value;
                NotifyPropertyChanged("Resolution");
            }
        }

        private string _frameRate;
        public string FrameRate
        {
            get
            {
                return _frameRate;
            }
            set
            {
                _frameRate = value;
                NotifyPropertyChanged("FrameRate");
            }
        }

        private string _packetLossInfo;
        public string PacketLossInfo
        {
            get
            {
                return _packetLossInfo;
            }
            set
            {
                _packetLossInfo = value;
                NotifyPropertyChanged("PacketLossInfo");
            }
        }
        

        private string _encrypted;
        public string Encrypted
        {
            get
            {
                return _encrypted;
            }
            set
            {
                _encrypted = value;
                NotifyPropertyChanged("Encrypted");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public void NotifyPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
