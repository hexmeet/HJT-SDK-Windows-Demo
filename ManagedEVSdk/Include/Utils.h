#pragma once

#include <msclr\marshal_cppstd.h>

#include "../../sdk_libs/include/EVCommon.h"
#include "../../sdk_libs/include/IEVEngine.h"
#include "ErrorInfo.h"

namespace EasyVideoWin {
namespace ManagedEVSdk {

public class Utils {
public:
    static System::String^ Utf8Str2ManagedStr(std::string str);
    static char* ManagedStr2Utf8Char(System::String^ str);
    static std::string ManagedStr2Utf8Str(System::String^ str);
};

} // ManagedEVSdk
} // EasyVideoWin
