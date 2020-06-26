#ifndef IAV_ENGINE_H
#define IAV_ENGINE_H
#include "EVCommon.h"

namespace ev {
namespace common {

//Error code from server
typedef enum _AV_SERVER_ERROR {
    AV_SERVER_INVALID_TOKEN = 1010,
    AV_SERVER_OPERATION_FAILURE = 1060,
    AV_SERVER_INTERNAL_ERROR = 1999,
    AV_SERVER_INVALID_USERNAME_PASSWORD = 2000,
    AV_SERVER_LOGIN_RETRY_FAILURE = 2001,
    AV_SERVER_ACCOUNT_LOCKED = 2010,
    AV_SERVER_ACCOUNT_DISUSE = 2020
} AV_SERVER_ERROR;

}
}

namespace ev {
namespace engine {

//////////////////////////////
//  Event
//////////////////////////////

class EV_CLASS_API IAVEventHandler : public IEVEventCallBack {
public:
    virtual void onSIPRegister(bool registered) {
        (void)registered;
    }

    virtual void onSIPForceClear() {
    }

    virtual void onCallIncoming(EVCallInfo & info) {
        (void)info;
    }
};

//////////////////////////////
//  EVAVEngine
//////////////////////////////

class EV_CLASS_API IAVEngine : public IEVCommon {
public:
    //CallBack
    virtual int registerAVEventHandler(IAVEventHandler * handler) = 0;
    virtual int unregisterAVEventHandler(IAVEventHandler * handler) = 0;

    //Login
    virtual int avlogin(const char * server, unsigned int port, const char * username, const char * encrypted_password) = 0;
    virtual int avlogout() = 0;

    //anonymous call
    virtual int registerSIPServer(const char * domain, const char * displayName, const char * username, const char * password, const char * protocol) = 0;
    virtual int unregisterSIPServer() = 0;

    //call
    virtual int dialOut(const char * number, bool enable_video) = 0;
    virtual int acceptCall(bool enable_video) = 0;
    virtual int hangUp() = 0;
    virtual int declineCall() = 0;
};

}
}

EV_API ev::engine::IAVEngine* createAVEngine();
EV_API void deleteAVEngine(ev::engine::IAVEngine* engine);

#endif
