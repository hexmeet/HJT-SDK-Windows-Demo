#pragma once

#include <msclr\marshal_cppstd.h>

#include "../../sdk_libs/include/EVCommon.h"
#include "../../sdk_libs/include/IEVEngine.h"
#include "ErrorInfo.h"

namespace EasyVideoWin {
namespace ManagedEVSdk {
namespace Structs {

static const int EV_LAYOUT_SIZE_CLI = 16;
static const unsigned int EV_STREAM_SIZE_CLI = 20;

[System::Serializable]
[System::FlagsAttribute]
public enum class EV_STREAM_TYPE_CLI
{
    EV_STREAM_AUDIO         = 0
    , EV_STREAM_VIDEO       = 1
    , EV_STREAM_CONTENT     = 2
    , EV_STREAM_WHITE_BOARD = 3
};

[System::Serializable]
[System::FlagsAttribute]
public enum class EV_STREAM_DIR_CLI
{
    EV_STREAM_UPLOAD        = 0
    , EV_STREAM_DOWNLOAD    = 1
};

[System::Serializable]
[System::FlagsAttribute]
public enum class EV_CONTENT_STATUS_CLI {
    EV_CONTENT_UNKNOWN      = 0
    , EV_CONTENT_GRANTED    = 1
    , EV_CONTENT_RELEASED   = 2
    , EV_CONTENT_DENIED     = 3
    , EV_CONTENT_REVOKED    = 4
};

[System::Serializable]
[System::FlagsAttribute]
public enum class EV_CONTENT_MODE_CLI
{
    EV_CONTENT_FULL_MODE            = 0
    , EV_CONTENT_APPLICATION_MODE   = 1
};

[System::Serializable]
[System::FlagsAttribute]
public enum class EV_WHITE_BOARD_TYPE_CLI
{
    EV_ACS_WHITE_BOARD = 0,
    EV_BELUGA_WHITE_BOARD = 1
};

[System::Serializable]
public ref struct EVErrorCli
{
    ErrorInfo::EV_ERROR_TYPE_CLI    type;
    System::String^                 action;
    int                             code;
    System::String^                 msg;
    array<System::String^>^         args;

    void Unmanaged2ManagedStruct(ev::engine::EVError& evErr);
};

[System::Serializable]
[System::FlagsAttribute]
public enum class EV_WARN_CLI {
    EV_WARN_NETWORK_POOR                    = 0
    , EV_WARN_NETWORK_VERY_POOR             = 1
    , EV_WARN_BANDWIDTH_INSUFFICIENT        = 2
    , EV_WARN_BANDWIDTH_VERY_INSUFFICIENT   = 3
    , EV_WARN_NO_AUDIO_CAPTURE_CARD         = 4
};

[System::Serializable]
public ref struct EVWarnCli {
public:
    EV_WARN_CLI         code;
    System::String^     msg;

    void Unmanaged2ManagedStruct(ev::engine::EVWarn& evWarn);
};

[System::Serializable]
public ref struct EVVideoSizeCli
{
    int                             width;
    int                             height;

    void Unmanaged2ManagedStruct(ev::engine::EVVideoSize& evVideoSize);
};

//////////////////////////////
//  Log
//////////////////////////////
[System::Serializable]
[System::FlagsAttribute]
public enum class EV_LOG_LEVEL_CLI
{
    EV_LOG_LEVEL_DEBUG = 0,
    EV_LOG_LEVEL_MESSAGE = 1,
    EV_LOG_LEVEL_WARNING = 2,
    EV_LOG_LEVEL_ERROR = 3,
    EV_LOG_LEVEL_FATAL = 4
};

//////////////////////////////
//  Device
//////////////////////////////

[System::Serializable]
[System::FlagsAttribute]
public enum class EV_DEVICE_TYPE_CLI
{
    EV_DEVICE_AUDIO_CAPTURE     = 0
    , EV_DEVICE_AUDIO_PLAYBACK  = 1
    , EV_DEVICE_VIDEO_CAPTURE   = 2
};

[System::Serializable]
public ref struct EVDeviceCli
{
    unsigned int            id;
    EV_DEVICE_TYPE_CLI      type;
    array<System::Byte>^    name;

    void Unmanaged2ManagedStruct(ev::engine::EVDevice& evDevice);
};

//////////////////////////////
//  Layout
//////////////////////////////

[System::Serializable]
[System::FlagsAttribute]
public enum class EV_LAYOUT_MODE_CLI {
    EV_LAYOUT_AUTO_MODE = 0,
    EV_LAYOUT_GALLERY_MODE = 1,
    EV_LAYOUT_SPEAKER_MODE = 2,
    EV_LAYOUT_SPECIFIED_MODE = 3
};

[System::Serializable]
[System::FlagsAttribute]
public enum class EV_LAYOUT_TYPE_CLI {
    EV_LAYOUT_TYPE_AUTO = -1,
    EV_LAYOUT_TYPE_1 = 101,
    EV_LAYOUT_TYPE_2H = 201,
    EV_LAYOUT_TYPE_2V = 202,
    EV_LAYOUT_TYPE_2H_2 = 203,
    EV_LAYOUT_TYPE_2V_2 = 204,
    EV_LAYOUT_TYPE_2_1IN1 = 205,
    EV_LAYOUT_TYPE_2_1L_1RS = 207,
    EV_LAYOUT_TYPE_3_1T_2B = 301,
    EV_LAYOUT_TYPE_3_2T_1B = 302,
    EV_LAYOUT_TYPE_3_1L_2R = 303,
    EV_LAYOUT_TYPE_3_2IN1 = 304,
    EV_LAYOUT_TYPE_1P2W = 305,
    EV_LAYOUT_TYPE_4 = 401,
    EV_LAYOUT_TYPE_4_3T_1B = 402,
    EV_LAYOUT_TYPE_4_1L_3R = 403,
    EV_LAYOUT_TYPE_4_1T_3B = 404,
    EV_LAYOUT_TYPE_4_3IN1 = 405,
    EV_LAYOUT_TYPE_5_1L_4R = 501,
    EV_LAYOUT_TYPE_5_4T_1B = 502,
    EV_LAYOUT_TYPE_5_1T_4B = 503,
    EV_LAYOUT_TYPE_6 = 601,
    EV_LAYOUT_TYPE_6W = 602,
    EV_LAYOUT_TYPE_2P4W = 603,
    EV_LAYOUT_TYPE_6CP = 604,
    EV_LAYOUT_TYPE_8 = 801,
    EV_LAYOUT_TYPE_9 = 901,
    EV_LAYOUT_TYPE_9_1IN_8OUT = 902,
    EV_LAYOUT_TYPE_9_8T_1B = 903,
    EV_LAYOUT_TYPE_9_1T_8B = 904,
    EV_LAYOUT_TYPE_10 = 1001,
    EV_LAYOUT_TYPE_2TP8B = 1002,
    EV_LAYOUT_TYPE_2CP4L4R = 1003,
    EV_LAYOUT_TYPE_12W = 1201,
    EV_LAYOUT_TYPE_13 = 1301,
    EV_LAYOUT_TYPE_1LTP12 = 1302,
    EV_LAYOUT_TYPE_16 = 1601,
    EV_LAYOUT_TYPE_1TLP16 = 1701,
    EV_LAYOUT_TYPE_1CP16 = 1702,
    EV_LAYOUT_TYPE_20 = 2001,
    EV_LAYOUT_TYPE_20_SQUARE = 2002,
    EV_LAYOUT_TYPE_1TLP20 = 2101,
    EV_LAYOUT_TYPE_1CP20 = 2102,
    EV_LAYOUT_TYPE_25 = 2501,
    EV_LAYOUT_TYPE_30 = 3001,
    EV_LAYOUT_TYPE_30_SQUARE = 3002,
    EV_LAYOUT_TYPE_36 = 3601
};

[System::Serializable]
[System::FlagsAttribute]
public enum class EV_LAYOUT_PAGE_CLI {
    EV_LAYOUT_CURRENT_PAGE = 0,
    EV_LAYOUT_PREV_PAGE = 1,
    EV_LAYOUT_NEXT_PAGE = 2
};

[System::Serializable]
public ref struct EVLayoutRequestCli
{
    EV_LAYOUT_MODE_CLI      mode;
    EV_LAYOUT_TYPE_CLI      max_type;
    EV_LAYOUT_PAGE_CLI      page;
    EVVideoSizeCli^         max_resolution;
    unsigned int            windows_size;
    array<System::IntPtr>^  windows;

    EVLayoutRequestCli()
    {
        windows = gcnew array<System::IntPtr>(EV_LAYOUT_SIZE_CLI);
    }

    void Managed2UnmanagedStruct(ev::engine::EVLayoutRequest& evLayoutRequest);
};


[System::Serializable]
public ref struct EVSiteCli {
    System::IntPtr      window;
    bool                is_local;
    System::String^     name;
    uint64_t            device_id;
    bool                mic_muted;
    bool                remote_muted;

    void Unmanaged2ManagedStruct(ev::engine::EVSite& site);
};

[System::Serializable]
public ref struct EVLayoutIndicationCli {
    EV_LAYOUT_MODE_CLI  mode;
    EV_LAYOUT_MODE_CLI  setting_mode;
    EV_LAYOUT_TYPE_CLI  type;
    bool                mode_settable;
    System::String^     speaker_name;
    int                 speaker_index;
    unsigned int        sites_size;
    array<EVSiteCli^>^  sites;
    
    EVLayoutIndicationCli()
    {
        sites = gcnew array<EVSiteCli^>(EV_LAYOUT_SIZE_CLI);
    }

    void Unmanaged2ManagedStruct(ev::engine::EVLayoutIndication& indication);
};

[System::Serializable]
public ref struct EVLayoutSpeakerIndicationCli {
    System::String^     speaker_name;
    int                 speaker_index;

    void Unmanaged2ManagedStruct(ev::engine::EVLayoutSpeakerIndication& indication);
};

//////////////////////////////
//  Statistic
////////////////////////////// 

[System::Serializable]
public ref struct EVStreamStatsCli {
    EV_STREAM_TYPE_CLI  type;
    EV_STREAM_DIR_CLI   dir;
    System::String^     payload_type;
    float               nego_bandwidth; //kbps
    float               real_bandwidth; //kbps
    uint64_t            cum_packet;
    float               fps;  //Video only
    EVVideoSizeCli^     resolution; //Video only
    uint64_t            cum_packet_loss;
    float               packet_loss_rate;
    bool                is_encrypted;
    unsigned int        ssrc;
    System::String^     name;

    void Unmanaged2ManagedStruct(ev::engine::EVStreamStats& evStreamStats);
};

[System::Serializable]
public ref struct EVStatsCli {
    unsigned int                size;
    array<EVStreamStatsCli^>^   stats;

    EVStatsCli()
    {
        size = 0;
        //stats = gcnew array<EVStreamStatsCli^>(EV_STREAM_SIZE_CLI);
    }

    void Unmanaged2ManagedStruct(ev::engine::EVStats& evStats);
};

//////////////////////////////
//  Event
//////////////////////////////

[System::Serializable]
public ref struct EVUserInfoCli
{
	System::String^         username;
    System::String^         displayName;
	System::String^         org;
	System::String^         email;
	System::String^         cellphone;
	System::String^         telephone;
	System::String^         dept;
	bool                    everChangedPasswd;
	System::String^         customizedH5UrlPrefix;
	System::String^         token;
    uint64_t                userId;
    uint64_t                deviceId;
    System::String^         orgPortAllocMode;
    uint64_t                orgPortCount;

	void Unmanaged2ManagedStruct(ev::engine::EVUserInfo& evUserInfo);
};

[System::Serializable]
public ref struct EVCallInfoCli {
    bool                isAudioOnly;
    bool                contentEnabled;
    System::String^     peer;
    System::String^     conference_number;
    System::String^     password;
    EVErrorCli^         err;

    void Unmanaged2ManagedStruct(ev::engine::EVCallInfo& evCallInfo);
};

[System::Serializable]
public ref struct EVContentInfoCli {
    bool                    enabled;
    EV_STREAM_DIR_CLI       dir;
    EV_STREAM_TYPE_CLI      type;
    EV_CONTENT_STATUS_CLI   status;

    void Unmanaged2ManagedStruct(ev::engine::EVContentInfo& info);
};

[System::Serializable]
[System::FlagsAttribute]
public enum class EV_RECORDING_STATE_CLI {
    EV_RECORDING_STATE_NONE = 0,
    EV_RECORDING_STATE_ON = 1,
    EV_RECORDING_STATE_PAUSE = 2
} ;

[System::Serializable]
public ref struct EVRecordingInfoCli {
public:
    EV_RECORDING_STATE_CLI  state;
    bool                    live;

    void Unmanaged2ManagedStruct(ev::engine::EVRecordingInfo& info);
};

[System::Serializable]
public ref struct EVMessageOverlayCli {
public:
    bool                    enable;
    System::String^         content;
    int                     displayRepetitions;
    int                     displaySpeed;
    int                     verticalBorder;
    int                     transparency;
    int                     fontSize;
    System::String^         foregroundColor;
    System::String^         backgroundColor;

    void Unmanaged2ManagedStruct(ev::engine::EVMessageOverlay& messageOverlay);
};

[System::Serializable]
public ref struct EVWhiteBoardInfoCli
{
public:
    EV_WHITE_BOARD_TYPE_CLI type;
    System::String^         authServer;
    System::String^         server;

    void Unmanaged2ManagedStruct(ev::engine::EVWhiteBoardInfo& whiteBoardInfo);
};

} // Structs
} // ManagedEVSdk
} // EasyVideoWin
