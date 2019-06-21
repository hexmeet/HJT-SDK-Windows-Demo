#pragma once

#include <msclr\marshal_cppstd.h>

#include "../../sdk_libs/include/EVCommon.h"
#include "../../sdk_libs/include/IEVEngine.h"
#include "Structs.h"


namespace EasyVideoWin {
namespace ManagedEVSdk {

class EVEventObsever;

public ref class EVSdkWrapper sealed {
public:
    ~EVSdkWrapper();

    //static property EVSdkWrapper^ Instance {
    //    EVSdkWrapper^ get()
    //    {
    //        if (nullptr == m_instance)
    //        {
    //            m_instance = gcnew EVSdkWrapper();
    //        }
    //        return m_instance;
    //    }

    //    void set(EVSdkWrapper^ instance)
    //    {
    //        m_instance = instance;
    //    }
    //}

    static EVSdkWrapper^ CreateInstance();
    static EVSdkWrapper^ GetEVSdkWrapper(int nId);
    void CreateEVEngine();

    // event
    event System::Action<Structs::EVErrorCli^>^ EventError;
    event System::Action<Structs::EVWarnCli^>^ EventWarn;
    event System::Action<bool>^ EventNetworkState;
    event System::Action<float>^ EventNetworkQuality;
    event System::Action<Structs::EVUserInfoCli^>^ EventLoginSucceed;
    event System::Action<bool>^ EventRegister;
    //event System::Action<Structs::EVCallInfoCli^>^ EventCallIncoming;
    event System::Action<Structs::EVCallInfoCli^>^ EventCallConnected;
    event System::Action<Structs::EVCallInfoCli^>^ EventCallEnd;
    event System::Action<Structs::EVContentInfoCli^>^ EventContent;
    event System::Action<System::String^>^ EventDownloadUserImageComplete;
    event System::Action<System::String^>^ EventUploadUserImageComplete;
    event System::Action<Structs::EVLayoutIndicationCli^>^ EventLayoutIndication;
    event System::Action<Structs::EVSiteCli^>^ EventLayoutSiteIndication;
    event System::Action<Structs::EVLayoutSpeakerIndicationCli^>^ EventLayoutSpeakerIndication;
    event System::Action^ EventMuteSpeakingDetected;
    event System::Action<Structs::EVCallInfoCli^>^ EventJoinConferenceIndication;
    //event System::Action<int>^ EventConferenceEndIndication;
    event System::Action<Structs::EVRecordingInfoCli^>^ EventRecordingIndication;
    event System::Action<Structs::EVMessageOverlayCli^>^ EventMessageOverlay;
    event System::Action<Structs::EVWhiteBoardInfoCli^>^ EventWhiteBoardIndication;
    event System::Action<int>^ EventParticipant;

	event System::Action<System::String^>^ EventManagedLog;

    // ev engine
    //Log
    void EVEngineSetLog(Structs::EV_LOG_LEVEL_CLI level, System::String^ logPath, System::String^ logFileName, unsigned int maxFileSize);
    void EVEngineEnableLog(bool enable);
    //init
    int EVEngineInitialize(System::String^ configPath, System::String^ configFileName);
    int EVEngineSetRootCA(System::String^ rootCaPath);
    int EVEngineSetUserImage(System::String^ backgroundFilePath, System::String^ userImagePath);
    int EVEngineEnableWhiteBoard(bool enable);
    int EVEngineSetUserAgent(System::String^ company, System::String^ version);
    int EVEngineRelease();

    //Login
    int EVEngineEnableSecure(bool enable);
    //System::String^ EVEngineEncryptPassword(System::String^ password);
    int EVEngineLogin(System::String^ server, unsigned int port, System::String^ username, System::String^ encryptedPassword);
    int EVEngineLoginWithLocation(System::String^ locationServer, unsigned int port, System::String^ username, System::String^ password);
    int EVEngineLogout();
    int EVEngineDownloadUserImage(System::String^ path);
    int EVEngineUploadUserImage(System::String^ path);
    int EVEngineChangePassword(System::String^ oldPassword, System::String^ newPassword);
    int EVEngineChangeDisplayName(System::String^ displayName);
    int EVEngineGetUserInfo(Structs::EVUserInfoCli^ %userInfo);
    System::String^ EVEngineGetDisplayName();

    //Device
    array<Structs::EVDeviceCli^>^ EVEngineGetDevices(Structs::EV_DEVICE_TYPE_CLI type);
    Structs::EVDeviceCli^ EVEngineGetDevice(Structs::EV_DEVICE_TYPE_CLI type);
    void EVEngineSetDevice(Structs::EV_DEVICE_TYPE_CLI type, unsigned int id);
    int EVEngineEnableMicMeter(bool enable);
    float EVEngineGetMicVolume();

    //Set Windows
    int EVEngineSetLocalVideoWindow(System::IntPtr id);
    int EVEngineSetRemoteVideoWindow(array<System::IntPtr>^ ids, unsigned int size);
    int EVEngineSetRemoteContentWindow(System::IntPtr id);
    int EVEngineSetLocalContentWindow(System::IntPtr id, Structs::EV_CONTENT_MODE_CLI mode);
    
    //Conference & Layout
    int EVEngineEnablePreview(bool enable);
    int EVEngineSetBandwidth(unsigned int kbps);
    unsigned int EVEngineGetBandwidth();
    int EVEngineSetMaxRecvVideo(unsigned int num);
    int EVEngineJoinConference(System::String^ conferenceNumber, System::String^ displayName, System::String^ password);
    int EVEngineJoinConference(System::String^ server, unsigned int port, System::String^ conferenceNumber, System::String^ displayName, System::String^ password);
    int EVEngineJoinConferenceWithLocation(System::String^ locationServer, unsigned int port, System::String^ conferenceNumber, System::String^ displayName, System::String^ password);
    int EVEngineLeaveConference();
    bool EVEngineCameraEnabled();
    int EVEngineEnableCamera(bool enable);
    bool EVEngineMicEnabled();
    int EVEngineEnableMic(bool enable);
    bool EVEngineRemoteMuted();
    int EVEngineRequestRemoteUnmute(bool val);
    bool EVEngineHighFPSEnabled();
    int EVEngineEnableHighFPS(bool enable);
    int EVEngineSetLayout(Structs::EVLayoutRequestCli^ layout);
    float EVEngineGetNetworkQuality();
    int EVEngineGetStats(Structs::EVStatsCli^ %stats);

    //Send Content
    int EVEngineSendContent();
    int EVEngineSendWhiteBoard();
    int EVEngineStopContent();


    // obsever
    void OnError(ev::engine::EVError& err);
    void OnWarn(ev::engine::EVWarn& warn);
    void OnNetworkState(bool reachable);
    void OnNetworkQuality(float rating);
    void OnLoginSucceed(ev::engine::EVUserInfo& user);
    void OnRegister(bool registered);
    void OnCallIncoming(ev::engine::EVCallInfo& info);
    void OnCallConnected(ev::engine::EVCallInfo& info);
    void OnCallEnd(ev::engine::EVCallInfo& info);
    void OnContent(ev::engine::EVContentInfo& info);
    void OnDownloadUserImageComplete(const char * path);
    void OnUploadUserImageComplete(const char * path);
    void OnLayoutIndication(ev::engine::EVLayoutIndication& layout);
    void OnLayoutSiteIndication(ev::engine::EVSite& site);
    void OnLayoutSpeakerIndication(ev::engine::EVLayoutSpeakerIndication& speaker);
    void OnMuteSpeakingDetected();
    void OnJoinConferenceIndication(ev::engine::EVCallInfo& info);
    void OnConferenceEndIndication(int seconds);
    void OnRecordingIndication(ev::engine::EVRecordingInfo& state);
    void OnMessageOverlay(ev::engine::EVMessageOverlay& msg);
    void OnWhiteBoardIndication(ev::engine::EVWhiteBoardInfo & info);
    void OnParticipant(int number);
    


private:
    //static EVSdkWrapper^                        m_instance;

    EVSdkWrapper();

    ev::engine::IEVEngine*	                    m_pEVEngine;
    EVEventObsever*                             m_pEVEventObsever;

    static int                                  m_nObseverIdx = 0;
    static System::Collections::IDictionary^    m_mapObseverIds;
    
	void OutputLog(System::String^ %str);

};

} // ManagedEVSdk
} // EasyVideoWin
