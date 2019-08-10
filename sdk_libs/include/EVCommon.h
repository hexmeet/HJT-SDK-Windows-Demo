#ifndef EV_COMMON_H
#define EV_COMMON_H
#include<stdint.h>
#include <string>
#include <vector>

#ifdef _MSC_VER
#include <windows.h>
#define EV_API extern "C" __declspec(dllexport)
#define EV_CLASS_API __declspec(dllexport)
#elif defined(__APPLE__)
#define EV_API __attribute__((visibility("default"))) extern "C"
#define EV_CLASS_API 
#elif defined(__ANDROID__) || defined(__linux__)
#define EV_API extern "C" __attribute__((visibility("default")))
#define EV_CLASS_API 
#else
#define EV_API extern "C"
#define EV_CLASS_API 
#endif


namespace ev {
namespace common {

typedef enum _EV_ERROR_TYPE {
    EV_ERROR_TYPE_SDK = 0,
    EV_ERROR_TYPE_SERVER = 1,
    EV_ERROR_TYPE_LOCATE = 2,
    EV_ERROR_TYPE_CALL = 3,
    EV_ERROR_TYPE_AV_SERVER = 4,
    EV_ERROR_TYPE_UNKNOWN = 5
} EV_ERROR_TYPE;

typedef enum _EV_ERROR {
    EV_OK = 0,
    EV_NG = 1,
    EV_UNINITIALIZED = 2,
    EV_BAD_FORMAT = 3,
    EV_NOT_IN_CONF = 4,
    EV_BAD_PARAM = 5,
    EV_REGISTER_FAILED = 6,
    EV_INTERNAL_ERROR = 7,
    EV_SERVER_UNREACHABLE = 8,
    EV_SERVER_INVALID = 9,
    EV_CALL_DECLINED = 10,
    EV_CALL_BUSY = 11,
    EV_CALL_IO_ERROR = 12
} EV_ERROR;

}
}

namespace ev {
namespace engine {

//////////////////////////////
//  Common
//////////////////////////////
#define EV_LAYOUT_SIZE 16
#define EV_STREAM_SIZE 32

typedef enum _EV_CALL_TYPE {
	EV_CALL_UNKNOWN = 0,
	EV_CALL_SIP = 1,
	EV_CALL_H323 = 2,
	EV_CALL_SVC = 3
} EV_CALL_TYPE;

typedef enum _EV_CALL_DIR {
	EV_CALL_OUTGOING = 0,
	EV_CALL_INCOMING = 1
} EV_CALL_DIR;

typedef enum _EV_CALL_STATUS {
	EV_CALL_STATUS_SUCCESS = 0,
	EV_CALL_STATUS_ABORTED = 1,
	EV_CALL_STATUS_MISSED = 2,
	EV_CALL_STATUS_DECLINED = 3
} EV_CALL_STATUS;

typedef enum _EV_STREAM_TYPE {
	EV_STREAM_AUDIO = 0,
	EV_STREAM_VIDEO = 1,
	EV_STREAM_CONTENT = 2,
	EV_STREAM_WHITE_BOARD = 3
} EV_STREAM_TYPE;

typedef enum _EV_STREAM_DIR {
	EV_STREAM_UPLOAD = 0,
	EV_STREAM_DOWNLOAD = 1
} EV_STREAM_DIR;

typedef enum _EV_CONTENT_STATUS {
    EV_CONTENT_UNKNOWN = 0,
	EV_CONTENT_GRANTED = 1,
	EV_CONTENT_RELEASED = 2,
	EV_CONTENT_DENIED = 3,
	EV_CONTENT_REVOKED = 4
} EV_CONTENT_STATUS;

typedef enum _EV_CONTENT_MODE {
    EV_CONTENT_FULL_MODE = 0,
    EV_CONTENT_APPLICATION_MODE = 1
} EV_CONTENT_MODE;

typedef enum _EV_WHITE_BOARD_TYPE {
    EV_ACS_WHITE_BOARD = 0,
    EV_BELUGA_WHITE_BOARD = 1
} EV_WHITE_BOARD_TYPE;

class EV_CLASS_API EVError {
public:
    EVError() {
        clear();
    }

    void clear() {
        type = common::EV_ERROR_TYPE_UNKNOWN;
        action.clear();
        code = 0;
        msg.clear();
        args.clear();
    }

    common::EV_ERROR_TYPE type;
    std::string action;
    int code;
    std::string msg;
    std::vector<std::string> args;
};

typedef enum _EV_WARN {
    EV_WARN_NETWORK_POOR = 0,
    EV_WARN_NETWORK_VERY_POOR = 1,
    EV_WARN_BANDWIDTH_INSUFFICIENT = 2,
    EV_WARN_BANDWIDTH_VERY_INSUFFICIENT = 3,
    EV_WARN_NO_AUDIO_CAPTURE_CARD = 4
} EV_WARN;

class EV_CLASS_API EVWarn {
public:
    EV_WARN code;
    std::string msg; 
}; 

class EV_CLASS_API EVVideoSize {
public:
	int width;
    int height;
};

typedef enum _EV_ENCRYPT_TYPE {
    EV_ENCRYPT_SHA1 = 0,
    EV_ENCRYPT_AES = 1
} EV_ENCRYPT_TYPE;

//////////////////////////////
//  Log
//////////////////////////////

typedef enum _EV_LOG_LEVEL {
    EV_LOG_LEVEL_DEBUG = 0,
    EV_LOG_LEVEL_MESSAGE = 1,
    EV_LOG_LEVEL_WARNING = 2,
    EV_LOG_LEVEL_ERROR = 3,
    EV_LOG_LEVEL_FATAL = 4
} EV_LOG_LEVEL;

//////////////////////////////
//  Device
//////////////////////////////

typedef enum _EV_DEVICE_TYPE {
    EV_DEVICE_AUDIO_CAPTURE = 0,
    EV_DEVICE_AUDIO_PLAYBACK = 1,
    EV_DEVICE_VIDEO_CAPTURE = 2
} EV_DEVICE_TYPE;

class EV_CLASS_API EVDevice {
public:
    unsigned int id;
    EV_DEVICE_TYPE type;
    std::string name;
};

//////////////////////////////
//  Statistic
////////////////////////////// 

class EV_CLASS_API EVStreamStats {
public:
    EV_STREAM_TYPE type;
    EV_STREAM_DIR dir;
    std::string payload_type;
    float nego_bandwidth; //kbps
    float real_bandwidth; //kbps
    uint64_t cum_packet;
    float fps;  //Video only
    EVVideoSize resolution; //Video only
    uint64_t cum_packet_loss;
    float packet_loss_rate;
    bool is_encrypted;
    unsigned int ssrc;
    std::string name;
};

class EV_CLASS_API EVStats {
public:
    unsigned int size;
    EVStreamStats stats[EV_STREAM_SIZE];
};

//////////////////////////////
//  Call Log
////////////////////////////// 

class EV_CLASS_API EVCallLog {
public:
    EVCallLog() {
        id.clear();
        type = EV_CALL_UNKNOWN;
        dir = EV_CALL_OUTGOING;
        status = EV_CALL_STATUS_SUCCESS;
        displayName.clear();
        peer.clear();
        startTime = 0;
        duration = 0;
        isAudioOnly = FALSE;
    }
    std::string id;
    EV_CALL_TYPE type;
    EV_CALL_DIR dir;
    EV_CALL_STATUS status;
    std::string displayName;
    std::string peer;
    uint64_t startTime;
    uint64_t duration;
    bool isAudioOnly;
};

//////////////////////////////
//  Event
//////////////////////////////

class EV_CLASS_API EVUserInfo {
public:
    uint64_t userId;
    std::string username;
    std::string displayName;
    std::string org;
    std::string orgPortAllocMode;
    uint64_t orgPortCount;
    std::string email;
    std::string cellphone;
    std::string telephone;
    uint64_t deviceId;
    std::string dept;
    bool everChangedPasswd;
    std::string customizedH5UrlPrefix;
    std::string token;
    std::string doradoVersion;
    std::string specifiedUpgradingServerAddress;
    uint64_t serverTime;
    std::string callNumber;
    std::string appServerType;
    std::string urlSuffixForMobile;
    std::string urlSuffixForPC;
};

class EV_CLASS_API EVCallInfo {
public:
    EVCallInfo() {
        clear();
    }

    void clear() {
        isAudioOnly = FALSE;
        contentEnabled = TRUE;
        peer.clear();
        conference_number.clear();
        password.clear();
        err.clear();
        isBigConference = FALSE;
        isRemoteMuted = FALSE;
    }

    bool isAudioOnly;
    bool contentEnabled;
    std::string peer;
    std::string conference_number;
    std::string password;
    EVError err;
    bool isBigConference;
    bool isRemoteMuted;
};

class EV_CLASS_API EVContentInfo {
public:
    EVContentInfo() {
        clear();
    }

    void clear() {
        enabled = FALSE;
        dir = EV_STREAM_DOWNLOAD;
        type = EV_STREAM_CONTENT;
        status = EV_CONTENT_UNKNOWN;
        isBigConference = FALSE;
        isRemoteMuted = FALSE;
    }

    bool enabled;
    EV_STREAM_DIR dir;
    EV_STREAM_TYPE type;
    EV_CONTENT_STATUS status;
    bool isBigConference;
    bool isRemoteMuted;
};

class EV_CLASS_API EVWhiteBoardInfo {
public:
    EV_WHITE_BOARD_TYPE type;
    std::string authServer;
    std::string server;
};

class EV_CLASS_API IEVEventCallBack {
public:
    virtual void onError(EVError & err) {
        (void)err;
    }

    virtual void onWarn(EVWarn & warn) {
        (void)warn;
    }

    virtual void onLoginSucceed(EVUserInfo & user) {
        (void)user;
    }

    virtual void onDownloadUserImageComplete(const char * path) {
        (void)path;
    }

    virtual void onUploadUserImageComplete(const char * path) {
        (void)path;
    }

    virtual void onNetworkState(bool reachable) {
        (void)reachable;
    }

    virtual void onNetworkQuality(float quality_rating) {
        (void)quality_rating;
    }

    virtual void onProvision(bool applied) {
        (void)applied;
    }

    virtual void onCallConnected(EVCallInfo & info) {
        (void)info;
    }

    virtual void onCallEnd(EVCallInfo & info) {
        (void)info;
    }

    virtual void onContent(EVContentInfo & info) {
        (void)info;
    }

    virtual void onMuteSpeakingDetected() {
    } 

    virtual void onCallLogUpdated(EVCallLog & call_log) {
        (void)call_log;
    }

    virtual void onVideoPreviewFrame(void * frame, int size) {
        (void)frame;
        (void)size;
    }

    virtual void onContentPreviewFrame(void * frame, int size) {
        (void)frame;
        (void)size;
    }
};

class EV_CLASS_API IEVCommon {
public:
    //Log
    virtual void setLog(EV_LOG_LEVEL level, const char * log_path, const char * log_file_name, unsigned int max_file_size) = 0;
    virtual void enableLog(bool enable) = 0;
    virtual std::string compressLog() = 0;

    //init
    virtual int initialize(const char *config_path, const char * config_file_name) = 0;
    virtual int setRootCA(const char * root_ca_path) = 0;
    virtual int setUserImage(const char * background_file_path, const char * user_image_path) = 0;
    virtual int enableWhiteBoard(bool enable) = 0;
    virtual int setUserAgent(const char * company, const char * version) = 0;
    virtual int release() = 0;
    virtual int enableSecure(bool enable) = 0;
    virtual std::string encryptPassword(const char * password) = 0;
    virtual std::string encryptPassword(EV_ENCRYPT_TYPE type, const char * password) = 0;
    virtual std::string getSerialNumber() = 0;
    virtual std::string getPlatform() = 0;

    //Device
    virtual std::vector<EVDevice> getDevices(EV_DEVICE_TYPE type) = 0;
    virtual EVDevice getDevice(EV_DEVICE_TYPE type) = 0;
    virtual void setDevice(EV_DEVICE_TYPE type, unsigned int id) = 0;
    virtual int enableMicMeter(bool enable) = 0;
    virtual float getMicVolume() = 0;
    virtual int setDeviceRotation(int rotation) = 0;
    virtual int audioInterruption(int type) = 0;

    //Set Windows
    virtual int setLocalVideoWindow(void * id) = 0;
    virtual int setRemoteVideoWindow(void * id) = 0;
    virtual int setRemoteContentWindow(void * id) = 0;
    virtual int setLocalContentWindow(void * id, EV_CONTENT_MODE mode) = 0;
    virtual int setPreviewVideoWindow(void * id) = 0;
    virtual int zoomRemoteWindow(EV_STREAM_TYPE stream_type, float zoom_factor, 
    float x, float y) = 0;

    virtual void * getLocalVideoWindow() = 0;
    virtual void * getRemoteVideoWindow() = 0;
    virtual void * getRemoteContentWindow() = 0;
    virtual void * getLocalContentWindow() = 0;
    virtual void * getPreviewVideoWindow() = 0;

    //Login
    virtual int downloadUserImage(const char * path) = 0;
    virtual int uploadUserImage(const char * path) = 0;
    virtual int changePassword(const char * encrypted_oldpassword, const char * encrypted_newpassword) = 0;
    virtual int changeDisplayName(const char * display_name) = 0;
    virtual int getUserInfo(EVUserInfo & userinfo) = 0;
    virtual std::string getDisplayName() = 0;

    //Conference
    virtual int enablePreview(bool enable) = 0;
    virtual int setBandwidth(unsigned int kbps) = 0;
    virtual unsigned int getBandwidth() = 0;
    virtual bool cameraEnabled() = 0;
    virtual int enableCamera(bool enable) = 0;
    virtual int switchCamera() = 0;
    virtual bool micEnabled() = 0;
    virtual int enableMic(bool enable) = 0;
    virtual int enableSpeaker(bool enable) = 0;
    virtual bool remoteMuted() = 0;
    virtual int requestRemoteUnmute(bool val) = 0;
    virtual bool highFPSEnabled() = 0;
    virtual int enableHighFPS(bool enable) = 0;
    virtual int enableHD(bool enable) = 0;
    virtual bool HDEnabled() = 0;
    virtual float getNetworkQuality() = 0;
    virtual int getStats(EVStats & stats) = 0; 
    virtual int getCallInfo(EVCallInfo & call_info) = 0;
    virtual int getContentInfo(EVContentInfo & content_info) = 0;
    virtual int enablePreviewFrameCb(EV_STREAM_TYPE type, bool enable) = 0;

    //Send Content
    virtual int sendContent() = 0;
    virtual int sendWhiteBoard() = 0;
    virtual int stopContent() = 0;
    virtual int enableContentAudio(bool enable) = 0;
    virtual bool contentAudioEnabled() = 0;

    //Call Log
    virtual int setCallLogMaxSize(unsigned int num) = 0;
    virtual std::vector<EVCallLog> getCallLog() = 0;
    virtual int removeCallLog(const char * id) = 0;

    //Provision
    virtual int setProvision(const char * server, unsigned int port) = 0;
    virtual int clearProvision() = 0;

    //Codec
    virtual int enableHardDecoding(bool enable) = 0;
    virtual bool hardDecodingEnabled() = 0;
};

}
}


#endif
