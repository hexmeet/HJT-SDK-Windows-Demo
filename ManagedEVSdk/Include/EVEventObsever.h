#pragma once

#include "../../sdk_libs/include/EVCommon.h"
#include "../../sdk_libs/include/IEVEngine.h"

namespace EasyVideoWin {
namespace ManagedEVSdk {

public class EVEventObsever : public ev::engine::IEVEventHandler
{
public:
    EVEventObsever(int nId);
    ~EVEventObsever();

    virtual void onError(ev::engine::EVError & err);
    virtual void onWarn(ev::engine::EVWarn & warn);
    virtual void onNetworkState(bool reachable);
    virtual void onNetworkQuality(float quality_rating);
    virtual void onLoginSucceed(ev::engine::EVUserInfo & user);
    virtual void onRegister(bool registered);
    virtual void onCallIncoming(ev::engine::EVCallInfo & info);
    virtual void onCallConnected(ev::engine::EVCallInfo & info);
    virtual void onCallPeerConnected(ev::engine::EVCallInfo & info);
    virtual void onCallEnd(ev::engine::EVCallInfo & info);
    virtual void onContent(ev::engine::EVContentInfo & info);
    virtual void onDownloadUserImageComplete(const char * path);
    virtual void onUploadUserImageComplete(const char * path);
    virtual void onLayoutIndication(ev::engine::EVLayoutIndication & layout);
    virtual void onLayoutSiteIndication(ev::engine::EVSite & site);
    virtual void onLayoutSpeakerIndication(ev::engine::EVLayoutSpeakerIndication & speaker);
    virtual void onMuteSpeakingDetected();
    virtual void onJoinConferenceIndication(ev::engine::EVCallInfo & info);
    virtual void onConferenceEndIndication(int seconds);
    virtual void onRecordingIndication(ev::engine::EVRecordingInfo & state);
    virtual void onMessageOverlay(ev::engine::EVMessageOverlay & msg);
    virtual void onWhiteBoardIndication(ev::engine::EVWhiteBoardInfo & info);
    virtual void onParticipant(int number);
    virtual void onMicMutedShow(int micMuted);
    virtual void onPeerImageUrl(const char* imageUrl);

private:
    int     m_nId;
};

} // ManagedEVSdk
} // EasyVideoWin