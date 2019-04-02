using System;
using System.Collections.ObjectModel;
using EasyVideoWin.Helpers;
using EasyVideoWin.CustomControls;
using EasyVideoWin.View;
using log4net;
using Microsoft.WindowsAPICodePack.Dialogs;
using EasyVideoWin.ManagedEVSdk.Structs;
using System.Threading.Tasks;

namespace EasyVideoWin.Model
{
    class SettingManager
    {
        private readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private static SettingManager _instance = new SettingManager();
        public EventHandler<WaitingCollectLogView> CollectLogFinished;
        private bool _isLogUploadSuccess = false;

        public static SettingManager Instance
        {
            get
            {
                return _instance;
            }
        }

        private SettingManager()
        {

        }

        public ObservableCollection<string> GetMicrophones()
        {
            ObservableCollection<string> validValues = new ObservableCollection<string>();
            EVDeviceCli[] mics = EVSdkManager.Instance.GetDevices(EV_DEVICE_TYPE_CLI.EV_DEVICE_AUDIO_CAPTURE);
            for (var i=0; i<mics.Length; ++i)
            {
                validValues.Add(Utils.Utf8Byte2DefaultStr(mics[i].name));
            }
            
            return validValues;
        }

        public ObservableCollection<string> GetSpeakers()
        {
            ObservableCollection<string> validValues = new ObservableCollection<string>();
            EVDeviceCli[] speakers = EVSdkManager.Instance.GetDevices(EV_DEVICE_TYPE_CLI.EV_DEVICE_AUDIO_PLAYBACK);
            for (var i = 0; i < speakers.Length; ++i)
            {
                validValues.Add(Utils.Utf8Byte2DefaultStr(speakers[i].name));
            }

            return validValues;
        }

        public ObservableCollection<string> GetCameras()
        {
            ObservableCollection<string> validValues = new ObservableCollection<string>();
            EVDeviceCli[] cameras = EVSdkManager.Instance.GetDevices(EV_DEVICE_TYPE_CLI.EV_DEVICE_VIDEO_CAPTURE);
            for (var i = 0; i < cameras.Length; ++i)
            {
                validValues.Add(Utils.Utf8Byte2DefaultStr(cameras[i].name));
            }
            
            return validValues;
        }

        public void SetMicrophone(string microphoneName)
        {
            EVDeviceCli[] mics = EVSdkManager.Instance.GetDevices(EV_DEVICE_TYPE_CLI.EV_DEVICE_AUDIO_CAPTURE);
            for (var i=0; i < mics.Length; ++i)
            {
                if (microphoneName == Utils.Utf8Byte2DefaultStr(mics[i].name))
                {
                    EVSdkManager.Instance.SetDevice(EV_DEVICE_TYPE_CLI.EV_DEVICE_AUDIO_CAPTURE, mics[i].id);
                    return;
                }
            }
        }

        public void SetSpeaker(string speakerName)
        {
            EVDeviceCli[] speakers = EVSdkManager.Instance.GetDevices(EV_DEVICE_TYPE_CLI.EV_DEVICE_AUDIO_PLAYBACK);
            for (var i = 0; i < speakers.Length; ++i)
            {
                if (speakerName == Utils.Utf8Byte2DefaultStr(speakers[i].name))
                {
                    EVSdkManager.Instance.SetDevice(EV_DEVICE_TYPE_CLI.EV_DEVICE_AUDIO_PLAYBACK, speakers[i].id);
                    return;
                }
            }
        }

        public void SetCamera(string cameraName)
        {
            EVDeviceCli[] cameras = EVSdkManager.Instance.GetDevices(EV_DEVICE_TYPE_CLI.EV_DEVICE_VIDEO_CAPTURE);
            for (var i = 0; i < cameras.Length; ++i)
            {
                if (cameraName == Utils.Utf8Byte2DefaultStr(cameras[i].name))
                {
                    EVSdkManager.Instance.SetDevice(EV_DEVICE_TYPE_CLI.EV_DEVICE_VIDEO_CAPTURE, cameras[i].id);
                    return;
                }
            }
        }

        public string GetCurrentMicrophone()
        {
            EVDeviceCli mic = EVSdkManager.Instance.GetDevice(EV_DEVICE_TYPE_CLI.EV_DEVICE_AUDIO_CAPTURE);
            return null != mic ? Utils.Utf8Byte2DefaultStr(mic.name) : "";
        }

        public string GetCurrentSpeaker()
        {
            EVDeviceCli speaker = EVSdkManager.Instance.GetDevice(EV_DEVICE_TYPE_CLI.EV_DEVICE_AUDIO_PLAYBACK);
            return null != speaker ? Utils.Utf8Byte2DefaultStr(speaker.name) : "";
        }

        public string GetCurrentCamera()
        {
            EVDeviceCli camera = EVSdkManager.Instance.GetDevice(EV_DEVICE_TYPE_CLI.EV_DEVICE_VIDEO_CAPTURE);
            return null != camera ? Utils.Utf8Byte2DefaultStr(camera.name) : "";
        }

        public uint GetCurrentCallRate()
        {
            return EVSdkManager.Instance.GetBandwidth();
        }

        public void SetCallRate(uint callRate)
        {
            EVSdkManager.Instance.SetBandwidth(callRate);
        }

        public void CloseCamera()
        {
            EVSdkManager.Instance.EnablePreview(false);
        }

        public void OpenCamemra()
        {
            EVSdkManager.Instance.EnablePreview(true);
        }

        public void SetLocalVideoWindow(IntPtr handle)
        {
            EVSdkManager.Instance.SetLocalVideoWindow(handle);
        }
        
        public void EnableMicMeter(bool enable)
        {
            EVSdkManager.Instance.EnableMicMeter(enable);
        }

        public float GetMicVolume()
        {
            return EVSdkManager.Instance.GetMicVolume();
        }

        public void Diagnostic(IMasterDisplayWindow msgOwner)
        {
            WaitingCollectLogView waitingview = new WaitingCollectLogView();
            string appDataFolder = Utils.GetConfigDataPath();
            LogFileCollector collector = new LogFileCollector(appDataFolder);
            Task.Run(() => {
                collector.Collect();
                _isLogUploadSuccess = collector.UploadToOss();
                CollectLogFinished(this, waitingview);
            });
            
            waitingview.Owner = msgOwner.GetWindow();
            waitingview.Topmost = true;
            waitingview.ShowDialog();
            
            if (!_isLogUploadSuccess)
            {
                using (CommonOpenFileDialog dialog = new CommonOpenFileDialog())
                {
                    dialog.Title = LanguageUtil.Instance.GetValueByKey("CHOOSE_LOG_FILE_PATH");
                    dialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
                    dialog.IsFolderPicker = true;
                    if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
                    {
                        try
                        {
                            collector.SaveAsTo(dialog.FileName);
                        }
                        catch (Exception ex)
                        {
                            log.ErrorFormat("Error:\n {0}", ex.Message);
                            MessageBoxTip tip1 = new MessageBoxTip(msgOwner);
                            tip1.SetTitleAndMsg(LanguageUtil.Instance.GetValueByKey("PROMPT"), string.Format("Error:\n {0}", ex.Message),
                                LanguageUtil.Instance.GetValueByKey("CONFIRM"));
                            tip1.ShowDialog();
                            return;
                        }

                    }
                    else
                    {
                        // no save file, just exit.
                        return;
                    }
                }
            }

            MessageBoxTip tip2 = new MessageBoxTip(msgOwner);
            string msg = LanguageUtil.Instance.GetValueByKey("SAVE_LOG_FILE_SUCCESS");
            if (_isLogUploadSuccess)
            {
                msg = LanguageUtil.Instance.GetValueByKey("UPLOAD_LOG_FILE_SUCCESS");
            }
            tip2.SetTitleAndMsg(LanguageUtil.Instance.GetValueByKey("PROMPT"), msg,
            LanguageUtil.Instance.GetValueByKey("CONFIRM"));
            tip2.ShowDialog();
        }
    }
    
}
