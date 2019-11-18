#ifndef IEV_ENGINE_H
#define IEV_ENGINE_H
#include "EVCommon.h"

namespace ev {
namespace common {

//Error code from server
typedef enum _EV_SERVER_ERROR {
    EV_SERVER_API_VERSION_NOT_SUPPORTED = 1000,
    EV_SERVER_INVALID_TOKEN = 1001,
    EV_SERVER_INVALID_PARAMETER = 1002,
    EV_SERVER_INVALID_DEVICESN = 1003,
    EV_SERVER_INVALID_MEDIA_TYPE = 1004,
    EV_SERVER_PERMISSION_DENIED = 1005,
    EV_SERVER_WRONG_FIELD_NAME = 1006,
    EV_SERVER_INTERNAL_SYSTEM_ERROR = 1007,
    EV_SERVER_OPERATION_FAILED = 1008,
    EV_SERVER_GET_FAILED = 1009,
    EV_SERVER_NOT_SUPPORTED = 1010,
    EV_SERVER_REDIS_LOCK_TIMEOUT = 1011,
    EV_SERVER_LOCAL_ZONE_STOPPED = 1019,
    EV_SERVER_INVALID_USER_NAME_PASSWORD = 1100,
    EV_SERVER_LOGIN_FAILED_MORE_THAN_5_TIMES = 1101,
    EV_SERVER_ACCOUNT_TEMPORARILY_LOCKED = 1102,
    EV_SERVER_ACCOUNT_DISABLED = 1103,
    EV_SERVER_NO_USERNAME = 1104,
    EV_SERVER_EMAIL_MISMATCH = 1105,
    EV_SERVER_COMPANY_ADMINISTRATOR_NOT_IN_ANY_COMPANY = 1106,
    EV_SERVER_FILE_UPLOAD_FAILED = 1200,
    EV_SERVER_INVALID_LICENSE = 1201,
    EV_SERVER_INVALID_IMPORT_USER_FILE = 1202,
    EV_SERVER_INVALID_TIME_SERVICE_ADDRESS = 1300,
    EV_SERVER_FAILED_UPDATE_SYSTEM_PROPERTIES = 1301,
    EV_SERVER_CONF_NOT_EXISTS = 1400,
    EV_SERVER_NUMERICID_CONFLICTS = 1401,
    EV_SERVER_CONF_UPDATING_IN_PROGRESS = 1402,
    EV_SERVER_CONF_DELETING_IN_PROGRESS = 1403,
    EV_SERVER_CONF_TERMINATING_IN_PROGRESS = 1404,
    EV_SERVER_CONF_LAUNCHING_IN_PROGRESS = 1405,
    EV_SERVER_CONF_NOT_IN_APPROVED_STATUS = 1406,
    EV_SERVER_CONF_NUMERICID_ONGOING = 1407,
    EV_SERVER_CONF_NOT_APPROVED_OR_ONGOING = 1409,
    EV_SERVER_PARTICIPANT_NOT_EXISTS_IN_CONF = 1410,
    EV_SERVER_NUMERICID_ALREADY_IN_USE = 1412,
    EV_SERVER_INVALID_CONF_TIME = 1415,
    EV_SERVER_INVALID_CONF_ID = 1418,
    EV_SERVER_NOT_FOUND_SUITABLE_MRU = 1421,
    EV_SERVER_NOT_FOUND_SUITABLE_GATEWAY = 1422,
    EV_SERVER_FAILED_TO_CONNECT_MRU = 1424,
    EV_SERVER_NOT_ALLOW_DUPLICATED_NAME = 1427,
    EV_SERVER_NOT_FOUND_CONF_IN_REDIS = 1430,
    EV_SERVER_NOT_IN_LECTURER_MODE = 1431,
    EV_SERVER_FAILED_TO_MUTE_ALL_PARTICIPANTS = 1433,
    EV_SERVER_FAILED_TO_CONNECT_PARTICIPANT = 1436,
    EV_SERVER_FAILED_TO_DISCONNECT_PARTICIPANT = 1439,
    EV_SERVER_FAILED_TO_CHANGE_LAYOUT = 1442,
    EV_SERVER_FAILED_TO_SET_SUBTITLE = 1445,
    EV_SERVER_FAILED_TO_MUTE_PARTICIPANT_AUDIO = 1448,
    EV_SERVER_FAILED_TO_DELETE_PARTICIPANT = 1451,
    EV_SERVER_FAILED_TO_INVITE_AVC_ENDPOINT = 1454,
    EV_SERVER_FAILED_TO_INVITE_SVC_ENDPOINTS = 1455,
    EV_SERVER_CONF_ROOM_COMPLETELY_FULL = 1456,
    EV_SERVER_TIMEOUT_TO_GENERATE_NUMERICID = 1457,
    EV_SERVER_NOT_FOUND_PROFILE_NAMED_SVC = 1460,
    EV_SERVER_FAILED_TO_PROLONG_CONF = 1463,
    EV_SERVER_INVALID_MEETING_CONTROL_REQUEST = 1500,
    EV_SERVER_NAME_IN_USE = 1600,
    EV_SERVER_EMPTY_ENDPOINT_NAME = 1601,
    EV_SERVER_EMPTY_ENDPOINT_CALL_MODE = 1602,
    EV_SERVER_EMPTY_ENDPOINT_SIP_USERNAME = 1603,
    EV_SERVER_EMPTY_ENDPOINT_SIP_PASSWORD = 1604,
    EV_SERVER_EMPTY_ENDPOINT_ADDRESS = 1605,
    EV_SERVER_INVALID_SIP_USERNAME = 1606,
    EV_SERVER_INVALID_IP_ADDRESS = 1607,
    EV_SERVER_ENDPOINT_NOT_EXIST = 1608,
    EV_SERVER_E164_IN_USE = 1609,
    EV_SERVER_ENDPOINT_DEVICE_SN_EXIST = 1610,
    EV_SERVER_SIP_USERNAME_REGISTERED = 1611,
    EV_SERVER_ENDPOINT_E164_INVALID = 1612,
    EV_SERVER_NOT_FOUND_ENDPOINT_DEVICE_SN = 1613,
    EV_SERVER_NOT_FOUND_ENDPOINT_PROVISION_TEMPLATE = 1614,
    EV_SERVER_DEVICE_SN_EXISTS = 1615,
    EV_SERVER_CAN_NOT_DELETE_USER_IN_RESERVED_MEETING = 1700,
    EV_SERVER_EMPTY_USER_PASSWORD = 1701,
    EV_SERVER_EMPTY_USERNAME = 1702,
    EV_SERVER_EMPTY_USER_DISPLAY_NAME = 1703,
    EV_SERVER_INVALID_USER_EMAIL = 1704,
    EV_SERVER_INVALID_CELLPHONE_NUMBER = 1705,
    EV_SERVER_ORIGINAL_PASSWORD_WRONG = 1706,
    EV_SERVER_DUPLICATE_EMAIL_NAME = 1707,
    EV_SERVER_DUPLICATE_CELLPHONE_NUMBER = 1708,
    EV_SERVER_DUPLICATE_USERNAME = 1709,
    EV_SERVER_INVALID_CONF_ROOM_MAX_CAPACITY = 1710,
    EV_SERVER_SHOULD_ASSIGN_DEPARTMENT_TO_DEPARTMENT_ADMINISTRATOR = 1711,
    EV_SERVER_EMPTY_USER_EMAIL = 1712,
    EV_SERVER_EMPTY_USER_CELLPHONE_NUMBER = 1713,
    EV_SERVER_NOT_ORGANIZATION_ADMINISTRATOR = 1714,
    EV_SERVER_COMPANY_NOT_EXIST = 1800,
    EV_SERVER_SHORT_NAME_OF_COMPANY_USED = 1801,
    EV_SERVER_FULL_NAME_OF_COMPANY_USED = 1802,
    EV_SERVER_COMPANY_NOT_EMPTY = 1803,
    EV_SERVER_EMPTY_COMPANY_SHORT_NAME = 1804,
    EV_SERVER_AGENT_IN_USE = 1900,
    EV_SERVER_SHORT_NAME_IN_USE = 1901,
    EV_SERVER_FULL_NAME_IN_USE = 1902,
    EV_SERVER_AGENT_NOT_EXIST = 1903,
    EV_SERVER_AGENT_NOT_EMPTY = 1904,
    EV_SERVER_CONF_ROOM_EXPIRED = 2000,

    EV_SERVER_NOT_ACTIVED = 2001,

    EV_SERVER_NOT_FOUND_SUITABLE_ROOM = 2003,
    EV_SERVER_NOT_FOUND_TEMPLATE_OR_ROOM = 2005,
    EV_SERVER_CONF_ROOM_IN_USE = 2006,
    EV_SERVER_CONF_ROOM_NUMBER_IN_USE = 2009,
    EV_SERVER_CONF_ROOM_CAPACITY_EXCEEDS_LIMIT = 2012,
    EV_SERVER_INVALID_CONF_ROOM_CAPACITY = 2015, //PASSWORD REQUIRED
    EV_SERVER_INVALID_CONF_ROOM_NUMBER = 2018,
    EV_SERVER_ROOM_NOT_EXISTS = 2021,

    EV_SERVER_ROOM_NOT_ALLOW_ANONYMOUS_CALL = 2031,
    EV_SERVER_ROOM_ONLY_ALLOW_OWNER_ACTIVE = 2033,
    EV_SERVER_TRIAL_PERIOD_EXPIRED = 2035,

    EV_SERVER_CAN_NOT_DELETE_DEPARTMENT_WITH_SUBORDINATE_DEPARTMENT = 2100,
    EV_SERVER_CAN_NOT_DELETE_DEPARTMENT_WITH_USERS_OR_ENDPOINTS = 2101,
    EV_SERVER_INVALID_ACS_CONFIGURATION = 2200
} EV_SERVER_ERROR;

//Error code from locate server
typedef enum _EV_LOCATE_ERROR {
    EV_LOCATE_FAILED_TO_READ_BODY = 10000,
    EV_LOCATE_FAILED_TO_PARSE_BODY = 10001,
    EV_LOCATE_LOCATION_TIMEOUT = 10002,
    EV_LOCATE_ERROR_INFO_GENERAL = 10003,
    EV_LOCATE_ERROR_INFO_BAD_FORMAT = 10004,
    EV_LOCATE_UNEXPECTED = 10005,
    EV_LOCATE_FAILED_TO_LOCATE_CLIENT = 10006,
    EV_LOCATE_FAILED_TO_LOCATE_ZONE = 10007,
    EV_LOCATE_NO_LOCATION_DOMAIN = 10008,
    EV_LOCATE_ERROR_LOCATION_REQUEST = 10009
} EV_LOCATE_ERROR;

typedef enum _EV_CALL_ERROR {
    EV_CALL_INVALID_NUMERICID = 1001,
    EV_CALL_INVALID_USERNAME = 1003,
    EV_CALL_INVALID_USERID = 1005,
    EV_CALL_INVALID_DEVICEID = 1007,
    EV_CALL_INVALID_ENDPOINT = 1009,

    EV_CALL_SERVER_UNLICENSED = 2001,
    EV_CALL_NOT_FOUND_SUITABLE_MRU = 2003,
    EV_CALL_NEITHER_TEMPLATE_NOR_ONGOING_NOR_BINDED_ROOM = 2005,
    EV_CALL_LOCK_TIMEOUT = 2007,
    EV_CALL_TEMPLATE_CONF_WITHOUT_CONFROOM = 2009,
    EV_CALL_ROOM_EXPIRED = 2011,
    EV_CALL_INVALID_PASSWORD = 2015,
    EV_CALL_NO_TIME_SPACE_TO_ACTIVATE_ROOM = 2017,
    EV_CALL_CONF_PORT_COUNT_USED_UP = 2023,
    EV_CALL_ORG_PORT_COUNT_USED_UP = 2024,
    EV_CALL_HAISHEN_PORT_COUNT_USED_UP = 2025,
    EV_CALL_HAISHEN_GATEWAY_AUDIO_PORT_COUNT_USED_UP = 2027,
    EV_CALL_HAISHEN_GATEWAY_VIDEO_PORT_COUNT_USED_UP = 2029,
    EV_CALL_ONLY_ROOM_OWNER_CAN_ACTIVATE_ROOM = 2031,
    EV_CALL_NOT_ALLOW_ANONYMOUS_PARTY = 2033,
    EV_CALL_TRIAL_ORG_EXPIRED = 2035,
    EV_CALL_LOCAL_ZONE_NOT_STARTED = 2043,
    EV_CALL_LOCAL_ZONE_STOPPED = 2045
} EV_CALL_ERROR;

}
}

namespace ev {
namespace engine {

//////////////////////////////
//  Layout
//////////////////////////////

typedef enum _EV_LAYOUT_MODE{
    EV_LAYOUT_AUTO_MODE =  0,
    EV_LAYOUT_GALLERY_MODE =  1, 
    EV_LAYOUT_SPEAKER_MODE =  2,
    EV_LAYOUT_SPECIFIED_MODE =  3
} EV_LAYOUT_MODE;

typedef enum _EV_LAYOUT_TYPE{
    EV_LAYOUT_TYPE_AUTO       = -1, 
    EV_LAYOUT_TYPE_1          = 101,
    EV_LAYOUT_TYPE_2H         = 201,
    EV_LAYOUT_TYPE_2V         = 202,
    EV_LAYOUT_TYPE_2H_2       = 203,
    EV_LAYOUT_TYPE_2V_2       = 204,
    EV_LAYOUT_TYPE_2_1IN1     = 205,
    EV_LAYOUT_TYPE_2_1L_1RS   = 207,
    EV_LAYOUT_TYPE_3_1T_2B    = 301,
    EV_LAYOUT_TYPE_3_2T_1B    = 302,
    EV_LAYOUT_TYPE_3_1L_2R    = 303,
    EV_LAYOUT_TYPE_3_2IN1     = 304,
    EV_LAYOUT_TYPE_1P2W       = 305,
    EV_LAYOUT_TYPE_4          = 401,
    EV_LAYOUT_TYPE_4_3T_1B    = 402,
    EV_LAYOUT_TYPE_4_1L_3R    = 403,
    EV_LAYOUT_TYPE_4_1T_3B    = 404,
    EV_LAYOUT_TYPE_4_3IN1     = 405,
    EV_LAYOUT_TYPE_5_1L_4R    = 501,
    EV_LAYOUT_TYPE_5_4T_1B    = 502,
    EV_LAYOUT_TYPE_5_1T_4B    = 503,
    EV_LAYOUT_TYPE_6          = 601,
    EV_LAYOUT_TYPE_6W         = 602,
    EV_LAYOUT_TYPE_2P4W       = 603,
    EV_LAYOUT_TYPE_6CP        = 604,
    EV_LAYOUT_TYPE_8          = 801,
    EV_LAYOUT_TYPE_9          = 901,
    EV_LAYOUT_TYPE_9_1IN_8OUT = 902,
    EV_LAYOUT_TYPE_9_8T_1B    = 903,
    EV_LAYOUT_TYPE_9_1T_8B    = 904,
    EV_LAYOUT_TYPE_10         = 1001,
    EV_LAYOUT_TYPE_2TP8B      = 1002,
    EV_LAYOUT_TYPE_2CP4L4R    = 1003,
    EV_LAYOUT_TYPE_12W        = 1201,
    EV_LAYOUT_TYPE_13         = 1301,
    EV_LAYOUT_TYPE_1LTP12     = 1302,
    EV_LAYOUT_TYPE_16         = 1601,
    EV_LAYOUT_TYPE_1TLP16     = 1701,
    EV_LAYOUT_TYPE_1CP16      = 1702,
    EV_LAYOUT_TYPE_20         = 2001,
    EV_LAYOUT_TYPE_20_SQUARE  = 2002,
    EV_LAYOUT_TYPE_1TLP20     = 2101,
    EV_LAYOUT_TYPE_1CP20      = 2102,
    EV_LAYOUT_TYPE_25         = 2501,
    EV_LAYOUT_TYPE_30         = 3001,
    EV_LAYOUT_TYPE_30_SQUARE  = 3002,
    EV_LAYOUT_TYPE_36         = 3601
} EV_LAYOUT_TYPE;

typedef enum _EV_LAYOUT_PAGE{
	EV_LAYOUT_CURRENT_PAGE = 0,
	EV_LAYOUT_PREV_PAGE = 1,
	EV_LAYOUT_NEXT_PAGE = 2
} EV_LAYOUT_PAGE;

class EV_CLASS_API EVLayoutRequest {
public:
    EVLayoutRequest() {
        clear();
    }
    void clear() {
        mode = EV_LAYOUT_AUTO_MODE;
        max_type = EV_LAYOUT_TYPE_AUTO;
        page = EV_LAYOUT_CURRENT_PAGE;
        max_resolution.width = max_resolution.height = 0;
        windows_size = 0;
        memset(windows, 0, sizeof(windows));
    }
    EV_LAYOUT_MODE mode;
    EV_LAYOUT_TYPE max_type;
    EV_LAYOUT_PAGE page;
    EVVideoSize max_resolution;
    unsigned int windows_size;
    void * windows[EV_LAYOUT_SIZE];
};

class EV_CLASS_API EVSite {
public:
    EVSite() {
        clear();
    }
    void clear() {
        window = NULL;
        is_local = TRUE;
        name = "";
        device_id = 0;
        mic_muted = FALSE;
        remote_muted = FALSE;
    }
    void * window;
    bool is_local;
    std::string name;
    uint64_t device_id;
    bool mic_muted;
    bool remote_muted;
};

class EV_CLASS_API EVLayoutIndication {
public:
    EVLayoutIndication() {
        clear();
    }
    void clear() {
        mode = EV_LAYOUT_AUTO_MODE;
        setting_mode = EV_LAYOUT_AUTO_MODE;
        type = EV_LAYOUT_TYPE_1;
        mode_settable = TRUE;
        speaker_name = "";
        speaker_index = -1;
        sites_size = 0;
        int i;
        for(i = 0; i < EV_LAYOUT_SIZE; i++) {
            sites[i].clear();
        }
    }
    EV_LAYOUT_MODE mode;
    EV_LAYOUT_MODE setting_mode;
    EV_LAYOUT_TYPE type;
    bool mode_settable;
    std::string speaker_name;
    int speaker_index;
    unsigned int sites_size;
    EVSite sites[EV_LAYOUT_SIZE];
};

class EV_CLASS_API EVLayoutSpeakerIndication {
public:
    std::string speaker_name;
    int speaker_index;
};

//////////////////////////////
//  Event
//////////////////////////////

class EV_CLASS_API EVMessageOverlay {
public:
    bool enable;
    std::string content;
    int displayRepetitions;
    int displaySpeed;
    int verticalBorder;
    int transparency;
    int fontSize;
    std::string foregroundColor;
    std::string backgroundColor;
};

typedef enum _EV_RECORDING_STATE {
    EV_RECORDING_STATE_NONE = 0,
    EV_RECORDING_STATE_ON = 1,
    EV_RECORDING_STATE_PAUSE = 2
} EV_RECORDING_STATE;

class EV_CLASS_API EVRecordingInfo {
public:
    EV_RECORDING_STATE state;
    bool live; 
}; 

class EV_CLASS_API IEVEventHandler : public IEVEventCallBack {
public:
    virtual void onRegister(bool registered) {
        (void)registered;
    }

    virtual void onWhiteBoardIndication(EVWhiteBoardInfo & info) {
        (void)info;
    }

    virtual void onLayoutIndication(EVLayoutIndication & layout) {
        (void)layout;
    }

    virtual void onLayoutSiteIndication(EVSite & site) {
        (void)site;
    }

   virtual void onLayoutSpeakerIndication(EVLayoutSpeakerIndication & speaker) {
        (void)speaker;
    }

    virtual void onJoinConferenceIndication(EVCallInfo & info) {
        (void)info;
    }

    virtual void onConferenceEndIndication(int seconds) {
        (void)seconds;
    }

    virtual void onRecordingIndication(EVRecordingInfo & state) {
        (void)state;
    }

    virtual void onMessageOverlay(EVMessageOverlay & msg) {
        (void)msg;
    }

    virtual void onParticipant(int number) {
        (void)number;
    }
};

//////////////////////////////
//  EVEngine
//////////////////////////////

class EV_CLASS_API IEVEngine : public IEVCommon {
public:
    //CallBack
    virtual int registerEventHandler(IEVEventHandler * handler) = 0;
    virtual int unregisterEventHandler(IEVEventHandler * handler) = 0;

    //Login
    EV_DEPRECATED virtual int login(const char * server, unsigned int port, const char * username, const char * encrypted_password) = 0;
    virtual int loginWithLocation(const char * location_server, unsigned int port, const char * username, const char * encrypted_password) = 0;
    virtual int logout() = 0;

    //Conference & Layout
    virtual int setMaxRecvVideo(unsigned int num) = 0;
    virtual int setLayoutCapacity(EV_LAYOUT_MODE mode, EV_LAYOUT_TYPE types[], unsigned int size) = 0;
    virtual int joinConference(const char * conference_number, const char * display_name, const char * password) = 0;
    virtual int joinConference(const char * number, const char * display_name, const char * password, EV_SVC_CALL_TYPE type) = 0;
    EV_DEPRECATED virtual int joinConference(const char * server, unsigned int port, const char * conference_number, const char * display_name, const char * password) = 0;
    virtual int joinConferenceWithLocation(const char * location_server, unsigned int port, const char * conference_number, const char * display_name, const char * password) = 0;
    virtual int leaveConference() = 0;
    virtual int declineIncommingCall(const char * conference_number) = 0;
    virtual int setLayout(EVLayoutRequest & layout) = 0;

    //Set Windows
    virtual int setRemoteVideoWindow(void * id[], unsigned int size) = 0;
    virtual int getRemoteVideoWindow(void * id[], unsigned int size) = 0;
};

}
}

EV_API ev::engine::IEVEngine* createEVEngine();
EV_API void deleteEVEngine(ev::engine::IEVEngine* engine);

#endif
