#if AUTOTEST

using EasyVideoWin.Helpers;
using EasyVideoWin.ManagedEVSdk.Structs;
using EasyVideoWin.Model;
using EasyVideoWin.Model.CloudModel;
using EasyVideoWin.View;
using EasyVideoWin.ViewModel;
using EasyVideoWin.Www.Api.Rest.DataStruct;
using EasyVideoWin.Www.Api.Rest.Resource;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasyVideoWin.www.api.rest
{
    public class ConfController : BaseApiController
    {
        private CallDialOut callDialOut = new CallDialOut();

        [HttpPut(Constants.MAKE_CALL_PATH)]
        public ActionResult MakeCall([FromBody] CallRequest callRequest)
        {
            if (LoginStatus.LoggedIn != LoginManager.Instance.CurrentLoginStatus)
            {
                return GetErrInfo(Constants.ErrorCode.NOT_LOGGED_IN);
            }

            callDialOut.StartVideoCall(false, false, callRequest.callee, callRequest.displayName, callRequest.password);
            return Content("");
        }

        [HttpPut(Constants.DROP_CALL_PATH)]
        public ActionResult DropCall()
        {
            if (CallStatus.Dialing != CallController.Instance.CurrentCallStatus
                && CallStatus.Connected != CallController.Instance.CurrentCallStatus
            )
            {
                return GetErrInfo(Constants.ErrorCode.NO_CONFERENCE);
            }

            CallController.Instance.TerminateCall();
            return Content("");
        }

        [HttpPut(Constants.ANONYMOUS_JOIN_CONF_PATH)]
        public ActionResult AnonymousJoinConf([FromBody] AnonymousJoinConfRequest anonymousJoinConfRequest)
        {
            Utils.SetDisplayNameInConf(anonymousJoinConfRequest.displayName);

            System.Windows.Application.Current.Dispatcher.Invoke(() =>
            {
                LoginManager.Instance.AnonymousJoinConference(
                                false
                                , LoginManager.Instance.EnableSecure
                                , anonymousJoinConfRequest.serverAddress
                                , LoginManager.Instance.ServerPort
                                , anonymousJoinConfRequest.callee
                                , anonymousJoinConfRequest.password
                                , !anonymousJoinConfRequest.turnOffCamera
                                , !anonymousJoinConfRequest.turnOffMicrophone);
            });

            return Content("");
        }

        [HttpGet(Constants.CALL_STATISTICS_PATH)]
        public ActionResult GetStatistics()
        {
            if (CallStatus.Connected != CallController.Instance.CurrentCallStatus)
            {
                return GetErrInfo(Constants.ErrorCode.NO_CONFERENCE);
            }

            EVStatsCli mediaStats = new EVStatsCli();
            EVSdkManager.Instance.GetStats(ref mediaStats);
            StatisticsInfo statisticsInfo = new StatisticsInfo();
            statisticsInfo.statistics = new CallStatistics();
            statisticsInfo.statistics.media_statistics = new MediaStatistics();
            statisticsInfo.statistics.media_statistics.ar = new List<StreamStatus>();
            statisticsInfo.statistics.media_statistics.@as = new List<StreamStatus>();
            statisticsInfo.statistics.media_statistics.cr = new List<StreamStatus>();
            statisticsInfo.statistics.media_statistics.cs = new List<StreamStatus>();
            statisticsInfo.statistics.media_statistics.pr = new List<StreamStatus>();
            statisticsInfo.statistics.media_statistics.ps = new List<StreamStatus>();
            for (var i = 0; i < mediaStats.size; ++i)
            {
                EVStreamStatsCli streamStats = mediaStats.stats[i];
                var restStatus = new StreamStatus()
                {
                    codec               = null != streamStats.payload_type ? streamStats.payload_type : "",
                    resolution          = EV_STREAM_TYPE_CLI.EV_STREAM_AUDIO == streamStats.type ? "" : streamStats.resolution.width + "x" + streamStats.resolution.height,
                    //pipeName            = "AR",
                    packetLost          = streamStats.cum_packet_loss,
                    packetLostRate      = streamStats.packet_loss_rate,
                    jitter              = 0,
                    frameRate           = EV_STREAM_TYPE_CLI.EV_STREAM_AUDIO == streamStats.type ? 0 : (int)streamStats.fps,
                    encrypted           = streamStats.is_encrypted,
                    rtp_actualBitRate   = (int)streamStats.real_bandwidth,
                    rtp_settingBitRate  = (int)streamStats.nego_bandwidth,
                    totalPacket         = streamStats.cum_packet
                };
                switch (streamStats.type)
                {
                    case EV_STREAM_TYPE_CLI.EV_STREAM_AUDIO:
                        if (EV_STREAM_DIR_CLI.EV_STREAM_DOWNLOAD == streamStats.dir)
                        {
                            restStatus.pipeName = "AR";
                            statisticsInfo.statistics.media_statistics.ar.Add(restStatus);
                        }
                        else
                        {
                            restStatus.pipeName = "AS";
                            statisticsInfo.statistics.media_statistics.@as.Add(restStatus);
                        }
                        break;
                    case EV_STREAM_TYPE_CLI.EV_STREAM_VIDEO:
                        if (EV_STREAM_DIR_CLI.EV_STREAM_DOWNLOAD == streamStats.dir)
                        {
                            restStatus.pipeName = "PR";
                            statisticsInfo.statistics.media_statistics.pr.Add(restStatus);
                        }
                        else
                        {
                            restStatus.pipeName = "PS";
                            statisticsInfo.statistics.media_statistics.ps.Add(restStatus);
                        }
                        break;
                    case EV_STREAM_TYPE_CLI.EV_STREAM_CONTENT:
                        if (EV_STREAM_DIR_CLI.EV_STREAM_DOWNLOAD == streamStats.dir)
                        {
                            restStatus.pipeName = "CR";
                            statisticsInfo.statistics.media_statistics.cr.Add(restStatus);
                        }
                        else
                        {
                            restStatus.pipeName = "CS";
                            statisticsInfo.statistics.media_statistics.cs.Add(restStatus);
                        }
                        break;
                    case EV_STREAM_TYPE_CLI.EV_STREAM_WHITE_BOARD:
                        if (EV_STREAM_DIR_CLI.EV_STREAM_DOWNLOAD == streamStats.dir)
                        {
                            restStatus.pipeName = "CR";
                            statisticsInfo.statistics.media_statistics.cr.Add(restStatus);
                        }
                        else
                        {
                            restStatus.pipeName = "CS";
                            statisticsInfo.statistics.media_statistics.cs.Add(restStatus);
                        }
                        break;
                }
            }
            
            statisticsInfo.statistics.signal_statistics = new SignalStatistics()
            {
                call_type = "SVC",
                call_rate = SettingManager.Instance.GetCurrentCallRate(),
                call_index = 0
            };

            return Json(statisticsInfo);
        }

        [HttpPut(Constants.AUDIO_MUTE_PATH)]
        public ActionResult SetAudioMute([FromBody] AudioMute audioMute)
        {
            if (CallStatus.Connected != CallController.Instance.CurrentCallStatus)
            {
                return GetErrInfo(Constants.ErrorCode.NO_CONFERENCE);
            }

            if (audioMute.muteOn)
            {
                CallController.Instance.EnableMic(false);
            }
            else
            {
                CallController.Instance.EnableMic(true);
            }

            return Content("");
        }

        [HttpPut(Constants.HAND_UP_PATH)]
        public ActionResult SetHandUp()
        {
            if (CallStatus.Connected != CallController.Instance.CurrentCallStatus)
            {
                return GetErrInfo(Constants.ErrorCode.NO_CONFERENCE);
            }

            EVSdkManager.Instance.RequestRemoteUnmute(true);
            return Content("");
        }

        [HttpPut(Constants.SVC_LAYOUT_MODE_PATH)]
        public ActionResult SetSvcLayoutMode([FromBody] SvcLayoutMode svcLayoutMode)
        {
            if (CallStatus.Connected != CallController.Instance.CurrentCallStatus)
            {
                return GetErrInfo(Constants.ErrorCode.NO_CONFERENCE);
            }

            if (1 == svcLayoutMode.mode || 2 == svcLayoutMode.mode)
            {
                System.Windows.Application.Current.Dispatcher.Invoke(() =>
                {
                    LayoutBackgroundWindow.Instance.LayoutOperationbar_SvcLayoutModeChanged(2 == svcLayoutMode.mode);
                });
            }
            return Content("");
        }

        [HttpGet(Constants.SVC_MEETING_PATH)]
        public ActionResult GetSvcMeetingInfo()
        {
            if (CallStatus.Connected != CallController.Instance.CurrentCallStatus)
            {
                return GetErrInfo(Constants.ErrorCode.NO_CONFERENCE);
            }

            LayoutInfo layoutInfo = new LayoutInfo()
            {
                windowIdx = new List<int>(),
                svcDeviceIds = new List<string>(),
                svcSuit = new List<string>(),
                isOnlyLocal = false
            };
            List<string> micMutedParticipants = new List<string>();
            var size = LayoutBackgroundWindow.Instance.LayoutIndication.sites_size;
            var sites = LayoutBackgroundWindow.Instance.LayoutIndication.sites;
            for (var i=0; i<size; ++i)
            {
                layoutInfo.windowIdx.Add(i);
                layoutInfo.svcDeviceIds.Add(sites[i].device_id.ToString());
                layoutInfo.svcSuit.Add(sites[i].name);
                if (sites[i].mic_muted)
                {
                    micMutedParticipants.Add(sites[i].device_id.ToString());
                }
            }
            
            layoutInfo.layoutMode = LayoutBackgroundWindow.Instance.IsSpeakerMode ? "Speaker" : "Gallery";
            layoutInfo.speakerName = LayoutBackgroundWindow.Instance.LayoutIndication.speaker_name;

            SvcMeetingInfo svcMeetingInfo = new SvcMeetingInfo()
            {
                deviceId = LoginManager.Instance.DeviceId.ToString(),
                micMutedParticipants = micMutedParticipants,
                layoutInfo = layoutInfo,
                isSvcLayoutEnable = true,
                gallerySpeakerIndex = LayoutBackgroundWindow.Instance.LayoutIndication.speaker_index,
                muteByMru = LayoutBackgroundWindow.Instance.IsRemoteMuted,
                svcReg = 2,
                wsConnect = 1
            };
            
            return Json(svcMeetingInfo);
        }

        [HttpPut(Constants.START_WHITEBOARD)]
        public ActionResult StartWhiteboard()
        {
            if (CallStatus.Connected != CallController.Instance.CurrentCallStatus)
            {
                return GetErrInfo(Constants.ErrorCode.NO_CONFERENCE);
            }

            System.Windows.Application.Current.Dispatcher.Invoke(() =>
            {
                LayoutBackgroundWindow.Instance.StartWhiteboard();
            });
            return Content("");
        }

        [HttpPut(Constants.STOP_WHITEBOARD)]
        public ActionResult StopWhiteboard()
        {
            if (CallStatus.Connected != CallController.Instance.CurrentCallStatus)
            {
                return GetErrInfo(Constants.ErrorCode.NO_CONFERENCE);
            }

            System.Windows.Application.Current.Dispatcher.Invoke(() =>
            {
                LayoutBackgroundWindow.Instance.ExitWhiteboard();
            });
            return Content("");
        }

        [HttpPut(Constants.START_CONTENT)]
        public ActionResult StartContent()
        {
            if (CallStatus.Connected != CallController.Instance.CurrentCallStatus)
            {
                return GetErrInfo(Constants.ErrorCode.NO_CONFERENCE);
            }

            System.Windows.Application.Current.Dispatcher.Invoke(() =>
            {
                LayoutBackgroundWindow.Instance.StartContent();
            });
            return Content("");
        }

        [HttpPut(Constants.STOP_CONTENT)]
        public ActionResult StopContent()
        {
            if (CallStatus.Connected != CallController.Instance.CurrentCallStatus)
            {
                return GetErrInfo(Constants.ErrorCode.NO_CONFERENCE);
            }

            System.Windows.Application.Current.Dispatcher.InvokeAsync(() =>
            {
                LayoutBackgroundWindow.Instance.ExitContent();
            });
            return Content("");
        }
    }

    class CallDialOut : DialOutModelBase
    {
        public void StartVideoCall(bool turnOffCamera, bool turnOffMicrophone, string confNumber, string displayName, string confPassword)
        {
            EVSdkManager.Instance.EnableCamera(!turnOffCamera);
            EVSdkManager.Instance.EnableMic(!turnOffMicrophone);
            System.Windows.Application.Current.Dispatcher.InvokeAsync(() =>
            {
                JoinConference(confNumber.Trim(), displayName, confPassword, (MainWindow)System.Windows.Application.Current.MainWindow);
            });
        }
    }
}

#endif