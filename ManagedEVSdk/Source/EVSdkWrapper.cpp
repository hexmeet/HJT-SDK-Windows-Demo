
#include "../Include/EVSdkWrapper.h"
#include "../Include/EVEventObsever.h"
#include "../Include/Utils.h"

namespace EasyVideoWin {
namespace ManagedEVSdk {


EVSdkWrapper::EVSdkWrapper()
{
    m_pEVEventObsever = nullptr;
}

EVSdkWrapper::~EVSdkWrapper()
{
    if (nullptr != m_pEVEngine)
    {
        m_pEVEngine->unregisterEventHandler(m_pEVEventObsever);
        delete m_pEVEngine;
    }

    if (nullptr != m_pEVEventObsever) {
        delete m_pEVEventObsever;
    }
}

EVSdkWrapper^ EVSdkWrapper::CreateInstance()
{
    return gcnew EVSdkWrapper();
}

EVSdkWrapper^ EVSdkWrapper::GetEVSdkWrapper(int nId)
{
    return (EVSdkWrapper^)m_mapObseverIds[nId];
}

void EVSdkWrapper::CreateEVEngine()
{
    if (nullptr == m_mapObseverIds)
    {
        System::String^ str = "Obsever ids is null";
        OutputLog(str);
        m_mapObseverIds = gcnew System::Collections::Hashtable();
    }
    m_pEVEngine = createEVEngine();
    int id = ++m_nObseverIdx;
    m_pEVEventObsever = new EVEventObsever(id);
    m_mapObseverIds->Add(id, this);
    m_pEVEngine->registerEventHandler(m_pEVEventObsever);
	System::String^ str = "Create EVEngine";
    OutputLog(str);
}

//Log
void EVSdkWrapper::EVEngineSetLog(Structs::EV_LOG_LEVEL_CLI level, System::String^ logPath, System::String^ logFileName, unsigned int maxFileSize)
{
    std::string szLogPath = msclr::interop::marshal_as<std::string>(logPath);
    std::string szLogFileName = msclr::interop::marshal_as<std::string>(logFileName);
    m_pEVEngine->setLog(safe_cast<ev::engine::EV_LOG_LEVEL>(level), szLogPath.c_str(), szLogFileName.c_str(), maxFileSize);
}

void EVSdkWrapper::EVEngineEnableLog(bool enable)
{
    m_pEVEngine->enableLog(enable);
}

//init
int EVSdkWrapper::EVEngineInitialize(System::String^ configPath, System::String^ configFileName)
{
    std::string szConfigPath = msclr::interop::marshal_as<std::string>(configPath);
    std::string szConfigFileName = msclr::interop::marshal_as<std::string>(configFileName);
    return m_pEVEngine->initialize(szConfigPath.c_str(), szConfigFileName.c_str());
}

int EVSdkWrapper::EVEngineSetRootCA(System::String^ rootCaPath)
{
    std::string szRootCaPath = msclr::interop::marshal_as<std::string>(rootCaPath);
    return m_pEVEngine->setRootCA(szRootCaPath.c_str());
}

int EVSdkWrapper::EVEngineSetUserImage(System::String^ backgroundFilePath, System::String^ userImagePath)
{
    std::string szBackgroundFilePath = msclr::interop::marshal_as<std::string>(backgroundFilePath);
    std::string szUserImagePath = msclr::interop::marshal_as<std::string>(userImagePath);
    return m_pEVEngine->setUserImage(szBackgroundFilePath.c_str(), szUserImagePath.c_str());
}

int EVSdkWrapper::EVEngineEnableWhiteBoard(bool enable)
{
    return m_pEVEngine->enableWhiteBoard(enable);
}

int EVSdkWrapper::EVEngineSetUserAgent(System::String^ company, System::String^ version)
{
    std::string szCompany = msclr::interop::marshal_as<std::string>(company);
    std::string szVersion = msclr::interop::marshal_as<std::string>(version);
    return m_pEVEngine->setUserAgent(szCompany.c_str(), szVersion.c_str());
}

int EVSdkWrapper::EVEngineRelease()
{
    return m_pEVEngine->release();
}

//Login
int EVSdkWrapper::EVEngineEnableSecure(bool enable)
{
    return m_pEVEngine->enableSecure(enable);
}

//System::String^ EVSdkWrapper::EVEngineEncryptPassword(System::String^ password)
//{
//    std::string szPassword = msclr::interop::marshal_as<std::string>(password);
//    std::string szEnc = m_pEVEngine->encryptPassword(szPassword.c_str());
//    return msclr::interop::marshal_as<System::String ^>(szEnc);
//}

int EVSdkWrapper::EVEngineLogin(System::String^ server, unsigned int port, System::String^ username, System::String^ password)
{
    std::string szServer = msclr::interop::marshal_as<std::string>(server);
    std::string szUsername = msclr::interop::marshal_as<std::string>(username);
    std::string szPassword = msclr::interop::marshal_as<std::string>(password);
    std::string szEncryptedPassword = m_pEVEngine->encryptPassword(szPassword.c_str());
    return m_pEVEngine->login(szServer.c_str(), port, szUsername.c_str(), szEncryptedPassword.c_str());
}

int EVSdkWrapper::EVEngineLoginWithLocation(System::String^ locationServer, unsigned int port, System::String^ username, System::String^ password)
{
    std::string szLocationServer = msclr::interop::marshal_as<std::string>(locationServer);
    std::string szUsername = msclr::interop::marshal_as<std::string>(username);
    std::string szPassword = msclr::interop::marshal_as<std::string>(password);
    std::string szEncryptedPassword = m_pEVEngine->encryptPassword(szPassword.c_str());
    return m_pEVEngine->loginWithLocation(szLocationServer.c_str(), port, szUsername.c_str(), szEncryptedPassword.c_str());
}

int EVSdkWrapper::EVEngineLogout()
{
    return m_pEVEngine->logout();
}

int EVSdkWrapper::EVEngineDownloadUserImage(System::String^ path)
{
    std::string szPath = msclr::interop::marshal_as<std::string>(path);
    return m_pEVEngine->downloadUserImage(szPath.c_str());
}

int EVSdkWrapper::EVEngineUploadUserImage(System::String^ path)
{
    std::string szPath = msclr::interop::marshal_as<std::string>(path);
    return m_pEVEngine->uploadUserImage(szPath.c_str());
}

int EVSdkWrapper::EVEngineChangePassword(System::String^ oldPassword, System::String^ newPassword)
{
    std::string szOldPassword = msclr::interop::marshal_as<std::string>(oldPassword);
    std::string szEncryptedOldPassword = m_pEVEngine->encryptPassword(szOldPassword.c_str());
    std::string szNewPassword = msclr::interop::marshal_as<std::string>(newPassword);
    std::string szEncryptedNewPassword = m_pEVEngine->encryptPassword(szNewPassword.c_str());
    return m_pEVEngine->changePassword(szEncryptedOldPassword.c_str(), szEncryptedNewPassword.c_str());
}

int EVSdkWrapper::EVEngineChangeDisplayName(System::String^ displayName)
{
    char* pszName = Utils::ManagedStr2Utf8Char(displayName);
    int rst = m_pEVEngine->changeDisplayName(pszName);
    delete[] pszName;
    return rst;
}

int EVSdkWrapper::EVEngineGetUserInfo(Structs::EVUserInfoCli^ %userInfo)
{
    ev::engine::EVUserInfo evUserInfo;
    int rst = m_pEVEngine->getUserInfo(evUserInfo);
    if (ev::common::EV_ERROR::EV_OK == rst)
    {
        userInfo->Unmanaged2ManagedStruct(evUserInfo);
    }
    return rst;
}

System::String^ EVSdkWrapper::EVEngineGetDisplayName()
{
    std::string szDisplayName = m_pEVEngine->getDisplayName();
    return Utils::Utf8Str2ManagedStr(szDisplayName);
}

//Device
array<Structs::EVDeviceCli^>^ EVSdkWrapper::EVEngineGetDevices(Structs::EV_DEVICE_TYPE_CLI type)
{
    std::vector<ev::engine::EVDevice> evDevices = m_pEVEngine->getDevices(safe_cast<ev::engine::EV_DEVICE_TYPE>(type));
    array<Structs::EVDeviceCli^>^ cliDevices = gcnew array<Structs::EVDeviceCli^>(evDevices.size());
    for (auto i = 0; i < evDevices.size(); ++i)
    {
        Structs::EVDeviceCli^ pcliDevice = gcnew Structs::EVDeviceCli();
        pcliDevice->Unmanaged2ManagedStruct(evDevices[i]);
        cliDevices[i] = pcliDevice;
    }

    return cliDevices;
}

Structs::EVDeviceCli^ EVSdkWrapper::EVEngineGetDevice(Structs::EV_DEVICE_TYPE_CLI type)
{
    ev::engine::EVDevice evDevice = m_pEVEngine->getDevice(safe_cast<ev::engine::EV_DEVICE_TYPE>(type));
    Structs::EVDeviceCli^ pCliDevice = gcnew Structs::EVDeviceCli();
    pCliDevice->Unmanaged2ManagedStruct(evDevice);
    return pCliDevice;
}

void EVSdkWrapper::EVEngineSetDevice(Structs::EV_DEVICE_TYPE_CLI type, unsigned int id)
{
    m_pEVEngine->setDevice(safe_cast<ev::engine::EV_DEVICE_TYPE>(type), id);
}

int EVSdkWrapper::EVEngineEnableMicMeter(bool enable)
{
    return m_pEVEngine->enableMicMeter(enable);
}

float EVSdkWrapper::EVEngineGetMicVolume()
{
    return m_pEVEngine->getMicVolume();
}


//Set Windows
int EVSdkWrapper::EVEngineSetLocalVideoWindow(System::IntPtr id)
{
    return m_pEVEngine->setLocalVideoWindow(id.ToPointer());
}

int EVSdkWrapper::EVEngineSetRemoteVideoWindow(array<System::IntPtr>^ ids, unsigned int size)
{
    void** winIds = new void*[ids->Length];
    for (auto i = 0; i < ids->Length; ++i)
    {
        winIds[i] = ids[i].ToPointer();
    }
    int rst = m_pEVEngine->setRemoteVideoWindow(winIds, size);
    delete[] winIds;
    return rst;
}

int EVSdkWrapper::EVEngineSetRemoteContentWindow(System::IntPtr id)
{
    return m_pEVEngine->setRemoteContentWindow(id.ToPointer());
}

int EVSdkWrapper::EVEngineSetLocalContentWindow(System::IntPtr id, Structs::EV_CONTENT_MODE_CLI mode)
{
    return m_pEVEngine->setLocalContentWindow(id.ToPointer(), safe_cast<ev::engine::EV_CONTENT_MODE>(mode));
}

//Conference & Layout
int EVSdkWrapper::EVEngineEnablePreview(bool enable)
{
    return m_pEVEngine->enablePreview(enable);
}

int EVSdkWrapper::EVEngineSetBandwidth(unsigned int kbps)
{
    return m_pEVEngine->setBandwidth(kbps);
}

unsigned int EVSdkWrapper::EVEngineGetBandwidth() {
    return m_pEVEngine->getBandwidth();
}

int EVSdkWrapper::EVEngineSetMaxRecvVideo(unsigned int num)
{
    return m_pEVEngine->setMaxRecvVideo(num);
}

int EVSdkWrapper::EVEngineJoinConference(System::String^ conferenceNumber, System::String^ displayName, System::String^ password)
{
    std::string szConferenceNumber = msclr::interop::marshal_as<std::string>(conferenceNumber);
    char * pszName = Utils::ManagedStr2Utf8Char(displayName);
    std::string szPassword = msclr::interop::marshal_as<std::string>(password);
    int rst = m_pEVEngine->joinConference(szConferenceNumber.c_str(), pszName, szPassword.c_str());
    delete[] pszName;
    return rst;
}

int EVSdkWrapper::EVEngineJoinConference(System::String^ server, unsigned int port, System::String^ conferenceNumber, System::String^ displayName, System::String^ password)
{
    char * pszName = Utils::ManagedStr2Utf8Char(displayName);
    std::string szServer = msclr::interop::marshal_as<std::string>(server);
	std::string szConferenceNumber = msclr::interop::marshal_as<std::string>(conferenceNumber);
    std::string szPassword = msclr::interop::marshal_as<std::string>(password);
    int rst = m_pEVEngine->joinConference(szServer.c_str(), port, szConferenceNumber.c_str(), pszName, szPassword.c_str());
    delete[] pszName;
    return rst;
}

int EVSdkWrapper::EVEngineJoinConferenceWithLocation(System::String^ locationServer, unsigned int port, System::String^ conferenceNumber, System::String^ displayName, System::String^ password)
{
    char * pszName = Utils::ManagedStr2Utf8Char(displayName);
    std::string szLocationServer = msclr::interop::marshal_as<std::string>(locationServer);
    std::string szConferenceNumber = msclr::interop::marshal_as<std::string>(conferenceNumber);
    std::string szPassword = msclr::interop::marshal_as<std::string>(password);
    int rst = m_pEVEngine->joinConferenceWithLocation(szLocationServer.c_str(), port, szConferenceNumber.c_str(), pszName, szPassword.c_str());
    delete[] pszName;
    return rst;
}

int EVSdkWrapper::EVEngineLeaveConference()
{
    return m_pEVEngine->leaveConference();
}

bool EVSdkWrapper::EVEngineCameraEnabled()
{
    return m_pEVEngine->cameraEnabled();
}

int EVSdkWrapper::EVEngineEnableCamera(bool enable)
{
    return m_pEVEngine->enableCamera(enable);
}

bool EVSdkWrapper::EVEngineMicEnabled()
{
    return m_pEVEngine->micEnabled();
}

int EVSdkWrapper::EVEngineEnableMic(bool enable)
{
    return m_pEVEngine->enableMic(enable);
}

bool EVSdkWrapper::EVEngineRemoteMuted()
{
    return m_pEVEngine->remoteMuted();
}

int EVSdkWrapper::EVEngineRequestRemoteUnmute(bool val)
{
    return m_pEVEngine->requestRemoteUnmute(val);
}

bool EVSdkWrapper::EVEngineHighFPSEnabled()
{
    return m_pEVEngine->highFPSEnabled();
}

int EVSdkWrapper::EVEngineEnableHighFPS(bool enable)
{
    return m_pEVEngine->enableHighFPS(enable);
}

bool EVSdkWrapper::EVEngineHDEnabled()
{
    return m_pEVEngine->HDEnabled();
}

int EVSdkWrapper::EVEngineEnableHD(bool enable)
{
    return m_pEVEngine->enableHD(enable);
}

int EVSdkWrapper::EVEngineSetLayout(Structs::EVLayoutRequestCli^ layout)
{
    ev::engine::EVLayoutRequest evLayoutRequest;
    layout->Managed2UnmanagedStruct(evLayoutRequest);
    return m_pEVEngine->setLayout(evLayoutRequest);
}

float EVSdkWrapper::EVEngineGetNetworkQuality()
{
    return m_pEVEngine->getNetworkQuality();
}

int EVSdkWrapper::EVEngineGetStats(Structs::EVStatsCli^ %statsCli)
{
    ev::engine::EVStats evStats;
    evStats.size = 0;
    System::String^ logInfo = "Begin to getStats from sdk";
    OutputLog(logInfo);
    int rst = m_pEVEngine->getStats(evStats);
    logInfo = System::String::Format("getStats size:{0}", evStats.size);
    OutputLog(logInfo);
    if (ev::common::EV_ERROR::EV_OK == rst)
    {
        statsCli->Unmanaged2ManagedStruct(evStats);
    }
    return rst;
}


//Send Content
int EVSdkWrapper::EVEngineSendContent()
{
    return m_pEVEngine->sendContent();
}

int EVSdkWrapper::EVEngineSendWhiteBoard()
{
    return m_pEVEngine->sendWhiteBoard();
}

int EVSdkWrapper::EVEngineStopContent()
{
    return m_pEVEngine->stopContent();
}

void EVSdkWrapper::OnError(ev::engine::EVError& err)
{
    Structs::EVErrorCli^ errorCli = gcnew Structs::EVErrorCli();
    errorCli->Unmanaged2ManagedStruct(err);
    EventError(errorCli);
}

void EVSdkWrapper::OnWarn(ev::engine::EVWarn& warn)
{
    Structs::EVWarnCli^ warnCli = gcnew Structs::EVWarnCli();
    warnCli->Unmanaged2ManagedStruct(warn);
    EventWarn(warnCli);
}

void EVSdkWrapper::OnNetworkState(bool reachable)
{
    EventNetworkState(reachable);
}

void EVSdkWrapper::OnNetworkQuality(float rating)
{
    EventNetworkQuality(rating);
}

void EVSdkWrapper::OnLoginSucceed(ev::engine::EVUserInfo& user)
{
    Structs::EVUserInfoCli^ userInfoCli = gcnew Structs::EVUserInfoCli();
    userInfoCli->Unmanaged2ManagedStruct(user);
    EventLoginSucceed(userInfoCli);
}

void EVSdkWrapper::OnRegister(bool registered)
{
    EventRegister(registered);
}

void EVSdkWrapper::OnCallIncoming(ev::engine::EVCallInfo& info)
{
    //Structs::EVCallInfoCli^ callInfoCli = gcnew Structs::EVCallInfoCli();
    //callInfoCli->Unmanaged2ManagedStruct(info);
    //EventCallIncoming(callInfoCli);
}

void EVSdkWrapper::OnCallConnected(ev::engine::EVCallInfo& info)
{
    Structs::EVCallInfoCli^ callInfoCli = gcnew Structs::EVCallInfoCli();
    callInfoCli->Unmanaged2ManagedStruct(info);
    EventCallConnected(callInfoCli);
}

void EVSdkWrapper::OnCallEnd(ev::engine::EVCallInfo& info)
{
    Structs::EVCallInfoCli^ callInfoCli = gcnew Structs::EVCallInfoCli();
    callInfoCli->Unmanaged2ManagedStruct(info);
    EventCallEnd(callInfoCli);
}

void EVSdkWrapper::OnContent(ev::engine::EVContentInfo& info)
{
    Structs::EVContentInfoCli^ infoCli = gcnew Structs::EVContentInfoCli();
    infoCli->Unmanaged2ManagedStruct(info);
    EventContent(infoCli);
}

void EVSdkWrapper::OnDownloadUserImageComplete(const char * path)
{
    EventDownloadUserImageComplete(msclr::interop::marshal_as<System::String^>(path));
}

void EVSdkWrapper::OnUploadUserImageComplete(const char * path)
{
    EventUploadUserImageComplete(msclr::interop::marshal_as<System::String^>(path));
}

void EVSdkWrapper::OnLayoutIndication(ev::engine::EVLayoutIndication& layout)
{
    Structs::EVLayoutIndicationCli^ layoutCli = gcnew Structs::EVLayoutIndicationCli();
    layoutCli->Unmanaged2ManagedStruct(layout);
    EventLayoutIndication(layoutCli);
}

void EVSdkWrapper::OnLayoutSiteIndication(ev::engine::EVSite& site)
{
    Structs::EVSiteCli^ siteCli = gcnew Structs::EVSiteCli();
    siteCli->Unmanaged2ManagedStruct(site);
    EventLayoutSiteIndication(siteCli);
}

void EVSdkWrapper::OnLayoutSpeakerIndication(ev::engine::EVLayoutSpeakerIndication& speaker)
{
    Structs::EVLayoutSpeakerIndicationCli^ speakerCli = gcnew Structs::EVLayoutSpeakerIndicationCli();
    speakerCli->Unmanaged2ManagedStruct(speaker);
    EventLayoutSpeakerIndication(speakerCli);
}

void EVSdkWrapper::OnMuteSpeakingDetected()
{
    EventMuteSpeakingDetected();
}

void EVSdkWrapper::OnJoinConferenceIndication(ev::engine::EVCallInfo& info)
{
    Structs::EVCallInfoCli^ callInfoCli = gcnew Structs::EVCallInfoCli();
    callInfoCli->Unmanaged2ManagedStruct(info);
    EventJoinConferenceIndication(callInfoCli);
}

void EVSdkWrapper::OnConferenceEndIndication(int seconds)
{
    //EventConferenceEndIndication(seconds);
}

void EVSdkWrapper::OnRecordingIndication(ev::engine::EVRecordingInfo& state)
{
    Structs::EVRecordingInfoCli^ stateCli = gcnew Structs::EVRecordingInfoCli();
    stateCli->Unmanaged2ManagedStruct(state);
    EventRecordingIndication(stateCli);
}

void EVSdkWrapper::OnMessageOverlay(ev::engine::EVMessageOverlay& msg)
{
    Structs::EVMessageOverlayCli^ msgCli = gcnew Structs::EVMessageOverlayCli();
    msgCli->Unmanaged2ManagedStruct(msg);
    EventMessageOverlay(msgCli);
}

void EVSdkWrapper::OnWhiteBoardIndication(ev::engine::EVWhiteBoardInfo & info)
{
    Structs::EVWhiteBoardInfoCli^ infoCli = gcnew Structs::EVWhiteBoardInfoCli();
    infoCli->Unmanaged2ManagedStruct(info);
    EventWhiteBoardIndication(infoCli);
}

void EVSdkWrapper::OnParticipant(int number)
{
    EventParticipant(number);
}

void EVSdkWrapper::OutputLog(System::String^ %str)
{
	EventManagedLog(str);
}

} // ManagedEVSdk
} // EasyVideoWin