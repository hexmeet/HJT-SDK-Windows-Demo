#include "../Include/EVEventObsever.h"
#include "../Include/EVSdkWrapper.h"

namespace EasyVideoWin {
namespace ManagedEVSdk {

EVEventObsever::EVEventObsever(int nId) : m_nId(nId)
{
    m_nId = nId;
}

EVEventObsever::~EVEventObsever()
{

}

void EVEventObsever::onError(ev::engine::EVError & err) {
    EVSdkWrapper::GetEVSdkWrapper(m_nId)->OnError(err);
}

void EVEventObsever::onWarn(ev::engine::EVWarn & warn) {
    EVSdkWrapper::GetEVSdkWrapper(m_nId)->OnWarn(warn);
}

void EVEventObsever::onNetworkState(bool reachable) {
    EVSdkWrapper::GetEVSdkWrapper(m_nId)->OnNetworkState(reachable);
}

void EVEventObsever::onNetworkQuality(float quality_rating)
{
    EVSdkWrapper::GetEVSdkWrapper(m_nId)->OnNetworkQuality(quality_rating);
}

void EVEventObsever::onLoginSucceed(ev::engine::EVUserInfo & user) {
    EVSdkWrapper::GetEVSdkWrapper(m_nId)->OnLoginSucceed(user);
}

void EVEventObsever::onRegister(bool registered) {
    EVSdkWrapper::GetEVSdkWrapper(m_nId)->OnRegister(registered);
}

void EVEventObsever::onCallIncoming(ev::engine::EVCallInfo & info) {
    EVSdkWrapper::GetEVSdkWrapper(m_nId)->OnCallIncoming(info);
}

void EVEventObsever::onCallConnected(ev::engine::EVCallInfo & info) {
    EVSdkWrapper::GetEVSdkWrapper(m_nId)->OnCallConnected(info);
}

void EVEventObsever::onCallPeerConnected(ev::engine::EVCallInfo & info) {
    EVSdkWrapper::GetEVSdkWrapper(m_nId)->OnCallPeerConnected(info);
}

void EVEventObsever::onCallEnd(ev::engine::EVCallInfo & info) {
    EVSdkWrapper::GetEVSdkWrapper(m_nId)->OnCallEnd(info);
}

void EVEventObsever::onContent(ev::engine::EVContentInfo & info) {
    EVSdkWrapper::GetEVSdkWrapper(m_nId)->OnContent(info);
}

void EVEventObsever::onDownloadUserImageComplete(const char * path) {
    EVSdkWrapper::GetEVSdkWrapper(m_nId)->OnDownloadUserImageComplete(path);
}

void EVEventObsever::onUploadUserImageComplete(const char * path) {
    EVSdkWrapper::GetEVSdkWrapper(m_nId)->OnUploadUserImageComplete(path);
}

void EVEventObsever::onLayoutIndication(ev::engine::EVLayoutIndication & layout) {
    EVSdkWrapper::GetEVSdkWrapper(m_nId)->OnLayoutIndication(layout);
}

void EVEventObsever::onLayoutSiteIndication(ev::engine::EVSite & site) {
    EVSdkWrapper::GetEVSdkWrapper(m_nId)->OnLayoutSiteIndication(site);
}

void EVEventObsever::onLayoutSpeakerIndication(ev::engine::EVLayoutSpeakerIndication & speaker) {
    EVSdkWrapper::GetEVSdkWrapper(m_nId)->OnLayoutSpeakerIndication(speaker);
}

void EVEventObsever::onMuteSpeakingDetected()
{
    EVSdkWrapper::GetEVSdkWrapper(m_nId)->OnMuteSpeakingDetected();
}

void EVEventObsever::onJoinConferenceIndication(ev::engine::EVCallInfo & info) {
    EVSdkWrapper::GetEVSdkWrapper(m_nId)->OnJoinConferenceIndication(info);
}

void EVEventObsever::onConferenceEndIndication(int seconds) {
    EVSdkWrapper::GetEVSdkWrapper(m_nId)->OnConferenceEndIndication(seconds);
}

void EVEventObsever::onRecordingIndication(ev::engine::EVRecordingInfo & state) {
    EVSdkWrapper::GetEVSdkWrapper(m_nId)->OnRecordingIndication(state);
}

void EVEventObsever::onMessageOverlay(ev::engine::EVMessageOverlay & msg) {
    EVSdkWrapper::GetEVSdkWrapper(m_nId)->OnMessageOverlay(msg);
}

void EVEventObsever::onWhiteBoardIndication(ev::engine::EVWhiteBoardInfo & info) {
    EVSdkWrapper::GetEVSdkWrapper(m_nId)->OnWhiteBoardIndication(info);
}

void EVEventObsever::onParticipant(int number) {
    EVSdkWrapper::GetEVSdkWrapper(m_nId)->OnParticipant(number);
}

void EVEventObsever::onMicMutedShow(int micMuted)
{
    EVSdkWrapper::GetEVSdkWrapper(m_nId)->OnMicMutedShow(micMuted);
}

void EVEventObsever::onPeerImageUrl(const char * imageUrl) {
    EVSdkWrapper::GetEVSdkWrapper(m_nId)->OnPeerImageUrl(imageUrl);
}

} // ManagedEVSdk
} // EasyVideoWin