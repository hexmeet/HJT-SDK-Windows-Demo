
#include "../Include/Structs.h"

#include <msclr/marshal_windows.h>

#include "../Include/Utils.h"

namespace EasyVideoWin {
namespace ManagedEVSdk {
namespace Structs {

void EVErrorCli::Unmanaged2ManagedStruct(ev::engine::EVError& evErr)
{
    type    = safe_cast<ErrorInfo::EV_ERROR_TYPE_CLI>(evErr.type);
    action  = msclr::interop::marshal_as<System::String^>(evErr.action);
    code    = evErr.code;
    msg     = msclr::interop::marshal_as<System::String^>(evErr.msg);
    args    = gcnew array<System::String^>(evErr.args.size());
    for (unsigned int i = 0; i < evErr.args.size(); ++i)
    {
        args[i] = msclr::interop::marshal_as<System::String^>(evErr.args[i]);
    }
}

void EVWarnCli::Unmanaged2ManagedStruct(ev::engine::EVWarn& evWarn)
{
    code    = safe_cast<EV_WARN_CLI>(evWarn.code);
    msg     = msclr::interop::marshal_as<System::String^>(evWarn.msg);
}

void EVVideoSizeCli::Unmanaged2ManagedStruct(ev::engine::EVVideoSize& evVideoSize)
{
    width   = evVideoSize.width;
    height  = evVideoSize.height;
}

void EVUserInfoCli::Unmanaged2ManagedStruct(ev::engine::EVUserInfo& evUserInfo)
{
	username                = msclr::interop::marshal_as<System::String^>(evUserInfo.username);
    displayName             = Utils::Utf8Str2ManagedStr(evUserInfo.displayName);
    org                     = Utils::Utf8Str2ManagedStr(evUserInfo.org);
	email                   = msclr::interop::marshal_as<System::String^>(evUserInfo.email);
	cellphone               = msclr::interop::marshal_as<System::String^>(evUserInfo.cellphone);
	telephone               = msclr::interop::marshal_as<System::String^>(evUserInfo.telephone);
    dept                    = Utils::Utf8Str2ManagedStr(evUserInfo.dept);
	everChangedPasswd       = evUserInfo.everChangedPasswd;
	customizedH5UrlPrefix   = msclr::interop::marshal_as<System::String^>(evUserInfo.customizedH5UrlPrefix);
	token                   = msclr::interop::marshal_as<System::String^>(evUserInfo.token);
    userId                  = evUserInfo.userId;
    deviceId                = evUserInfo.deviceId;
    orgPortAllocMode        = Utils::Utf8Str2ManagedStr(evUserInfo.orgPortAllocMode);
    orgPortCount            = evUserInfo.orgPortCount;
}

void EVCallInfoCli::Unmanaged2ManagedStruct(ev::engine::EVCallInfo& evCallInfo)
{
    isAudioOnly         = evCallInfo.isAudioOnly;
    contentEnabled      = evCallInfo.contentEnabled;
    peer                = msclr::interop::marshal_as<System::String^>(evCallInfo.peer);
    conference_number   = msclr::interop::marshal_as<System::String^>(evCallInfo.conference_number);
    password            = msclr::interop::marshal_as<System::String^>(evCallInfo.password);
    err                 = gcnew EVErrorCli();
    err->Unmanaged2ManagedStruct(evCallInfo.err);
}

void EVContentInfoCli::Unmanaged2ManagedStruct(ev::engine::EVContentInfo& info)
{
    enabled             = info.enabled;
    dir                 = safe_cast<EV_STREAM_DIR_CLI>(info.dir);
    type                = safe_cast<EV_STREAM_TYPE_CLI>(info.type);
    status              = safe_cast<EV_CONTENT_STATUS_CLI>(info.status);
}

void EVDeviceCli::Unmanaged2ManagedStruct(ev::engine::EVDevice& evDevice)
{
	id      = evDevice.id;
	type    = safe_cast<EV_DEVICE_TYPE_CLI>(evDevice.type);
	name = gcnew array<System::Byte>(evDevice.name.size());
    for (unsigned int i = 0; i < evDevice.name.size(); ++i)
    {
        name[i] = evDevice.name[i];
    }
}

void EVLayoutRequestCli::Managed2UnmanagedStruct(ev::engine::EVLayoutRequest& evLayoutRequest)
{
    evLayoutRequest.mode                    = safe_cast<ev::engine::EV_LAYOUT_MODE>(mode);
    evLayoutRequest.max_type                = safe_cast<ev::engine::EV_LAYOUT_TYPE>(max_type);
    evLayoutRequest.page                    = safe_cast<ev::engine::EV_LAYOUT_PAGE>(page);
    evLayoutRequest.max_resolution.width    = max_resolution->width;
    evLayoutRequest.max_resolution.height   = max_resolution->height;
    evLayoutRequest.windows_size            = windows_size;
    for (unsigned int i = 0; i < windows_size; ++i)
    {
        evLayoutRequest.windows[i] = windows[i].ToPointer();
    }
}

void EVSiteCli::Unmanaged2ManagedStruct(ev::engine::EVSite& site)
{
    HANDLE handle = (HANDLE)(site.window);
    window          = msclr::interop::marshal_as<System::IntPtr>(handle);
    is_local        = site.is_local;
    name            = EasyVideoWin::ManagedEVSdk::Utils::Utf8Str2ManagedStr(site.name);
    device_id       = site.device_id;
    mic_muted       = site.mic_muted;
    remote_muted    = site.remote_muted;
}

void EVLayoutIndicationCli::Unmanaged2ManagedStruct(ev::engine::EVLayoutIndication& indication)
{
    mode                = safe_cast<EV_LAYOUT_MODE_CLI>(indication.mode);
    setting_mode        = safe_cast<EV_LAYOUT_MODE_CLI>(indication.setting_mode);
    type                = safe_cast<EV_LAYOUT_TYPE_CLI>(indication.type);
    mode_settable       = indication.mode_settable;
    speaker_name        = Utils::Utf8Str2ManagedStr(indication.speaker_name);
    speaker_index       = indication.speaker_index;
    sites_size          = indication.sites_size;
    for (unsigned int i = 0; i < sites_size; ++i)
    {
        sites[i]        = gcnew EVSiteCli();
        sites[i]->Unmanaged2ManagedStruct(indication.sites[i]);
    }
}

void EVLayoutSpeakerIndicationCli::Unmanaged2ManagedStruct(ev::engine::EVLayoutSpeakerIndication& indication)
{
    speaker_name        = Utils::Utf8Str2ManagedStr(indication.speaker_name);
    speaker_index       = indication.speaker_index;
}

void EVStreamStatsCli::Unmanaged2ManagedStruct(ev::engine::EVStreamStats& evStreamStats)
{
    type                = safe_cast<Structs::EV_STREAM_TYPE_CLI>(evStreamStats.type);
    dir                 = safe_cast<Structs::EV_STREAM_DIR_CLI>(evStreamStats.dir);
    payload_type        = Utils::Utf8Str2ManagedStr(evStreamStats.payload_type);
    nego_bandwidth      = evStreamStats.nego_bandwidth;
    real_bandwidth      = evStreamStats.real_bandwidth;
    cum_packet          = evStreamStats.cum_packet;
    fps                 = evStreamStats.fps;
    resolution          = gcnew EVVideoSizeCli();
    resolution->Unmanaged2ManagedStruct(evStreamStats.resolution);
    cum_packet_loss     = evStreamStats.cum_packet_loss;
    packet_loss_rate    = evStreamStats.packet_loss_rate;
    is_encrypted        = evStreamStats.is_encrypted;
    ssrc                = evStreamStats.ssrc;
    name                = msclr::interop::marshal_as<System::String^>(evStreamStats.name);
}

void EVStatsCli::Unmanaged2ManagedStruct(ev::engine::EVStats& evStats)
{
    size = evStats.size;
    stats = gcnew array<EVStreamStatsCli^>(size);
    for (unsigned int i = 0; i < size; ++i)
    {
        stats[i] = gcnew EVStreamStatsCli();
        stats[i]->Unmanaged2ManagedStruct(evStats.stats[i]);
    }
}

void EVRecordingInfoCli::Unmanaged2ManagedStruct(ev::engine::EVRecordingInfo& info)
{
    state       = safe_cast<EV_RECORDING_STATE_CLI>(info.state);
    live        = info.live;
}

void EVMessageOverlayCli::Unmanaged2ManagedStruct(ev::engine::EVMessageOverlay& messageOverlay)
{
    enable                  = messageOverlay.enable;
    content                 = Utils::Utf8Str2ManagedStr(messageOverlay.content);
    displayRepetitions      = messageOverlay.displayRepetitions;
    displaySpeed            = messageOverlay.displaySpeed;
    verticalBorder          = messageOverlay.verticalBorder;
    transparency            = messageOverlay.transparency;
    fontSize                = messageOverlay.fontSize;
    foregroundColor         = msclr::interop::marshal_as<System::String^>(messageOverlay.foregroundColor);
    backgroundColor         = msclr::interop::marshal_as<System::String^>(messageOverlay.backgroundColor);
}

void EVWhiteBoardInfoCli::Unmanaged2ManagedStruct(ev::engine::EVWhiteBoardInfo& whiteBoardInfo)
{
    type                    = safe_cast<EV_WHITE_BOARD_TYPE_CLI>(whiteBoardInfo.type);
    authServer              = msclr::interop::marshal_as<System::String^>(whiteBoardInfo.authServer);
    server                  = msclr::interop::marshal_as<System::String^>(whiteBoardInfo.server);
}


} // Structs
} // ManagedEVSdk
} // EasyVideoWin