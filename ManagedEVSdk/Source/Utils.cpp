
#include "../Include/Utils.h"
#include <msclr/marshal_windows.h>

namespace EasyVideoWin {
namespace ManagedEVSdk {

    System::String^ Utils::Utf8Str2ManagedStr(std::string str)
    {
        array<System::Byte>^ bufUtf8 = gcnew array<System::Byte>(str.size());
        for (int i = 0; i < str.size(); ++i)
        {
            bufUtf8[i] = str[i];
        }

        array<unsigned char, 1>^ arrUnicode = System::Text::Encoding::Convert(System::Text::Encoding::UTF8, System::Text::Encoding::Unicode, bufUtf8, 0, bufUtf8->Length);
        return System::Text::Encoding::Unicode->GetString(arrUnicode, 0, arrUnicode->Length);
    }

    char* Utils::ManagedStr2Utf8Char(System::String^ str)
    {
        array<System::Byte>^ bufManaged = System::Text::Encoding::UTF8->GetBytes(str);
        char* psz = new char[bufManaged->Length + 1];
        for (int i = 0; i < bufManaged->Length; ++i)
        {
            psz[i] = bufManaged[i];
        }
        psz[bufManaged->Length] = '\0';
        return psz;
    }


    std::string Utils::ManagedStr2Utf8Str(System::String^ str)
    {
        char *psz = ManagedStr2Utf8Char(str);
        std::string sz = psz;
        delete[] psz;
        return sz;
    }


} // ManagedEVSdk
} // EasyVideoWin