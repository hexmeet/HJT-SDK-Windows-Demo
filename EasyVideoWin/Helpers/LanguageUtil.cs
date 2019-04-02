using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace EasyVideoWin.Helpers
{
    public enum LanguageType
    {
        EN_US,
        ZH_CN
    }

    class LanguageUtil
    {
        #region -- Members --
        
        private static LanguageUtil _instance = new LanguageUtil();

        private Dictionary<LanguageType, String> _languageTypeVsFileName = new Dictionary<LanguageType, string>();
        private readonly String LANGUAGE_FILE_PATH = @"Resources\Languages\{0}.xaml";
        private ResourceDictionary _languageResource = null;
        private LanguageType _currentLanguage = LanguageType.ZH_CN;
        private Dictionary<LanguageType, string> _dictWebLanguage = new Dictionary<LanguageType, string>();

        public EventHandler<LanguageType> LanguageChanged;

        #endregion

        #region -- Properties --

        public LanguageType CurrentLanguage
        {
            get
            {
                return _currentLanguage;
            }
            set
            {
                if (_currentLanguage != value)
                {
                    _currentLanguage = value;
                    LanguageChanged?.Invoke(this, _currentLanguage);
                }
            }
        }

        public static LanguageUtil Instance
        {
            get
            {
                return _instance;
            }
        }

        #endregion

        #region -- Constructor --

        public LanguageUtil()
        {
            _languageTypeVsFileName.Add(LanguageType.EN_US, "en-US");
            _languageTypeVsFileName.Add(LanguageType.ZH_CN, "zh-CN");

            _dictWebLanguage.Add(LanguageType.EN_US, "en");
            _dictWebLanguage.Add(LanguageType.ZH_CN, "cn");
        }

        #endregion

        #region -- Public Methods --
        
        public void InitLanguage()
        {
            LanguageType lang = GetCurrentLanguage();
            UpdateLanguage(lang);
        }

        public bool UpdateLanguage(LanguageType languageType)
        {
            String requestedlanguageFilePath = GetFilePathByType(languageType);

            List<ResourceDictionary> dictionaryList = new List<ResourceDictionary>();
            foreach (ResourceDictionary dictionary in Application.Current.Resources.MergedDictionaries)
            {
                dictionaryList.Add(dictionary);
            }

            ResourceDictionary resourceDictionary = dictionaryList.FirstOrDefault(d => d.Source.OriginalString.Equals(requestedlanguageFilePath));
            if (null == resourceDictionary)
            {
                return false;
            }

            Application.Current.Resources.MergedDictionaries.Remove(resourceDictionary);
            Application.Current.Resources.MergedDictionaries.Add(resourceDictionary);

            _languageResource = resourceDictionary;
            CurrentLanguage = languageType;
            SaveCurrentLanguage(CurrentLanguage);

            return true;
        }

        public String GetValueByKey(String key)
        {
            if (_languageResource == null)
            {
                return "";
            }

            return (_languageResource[key] as String);
        }

        public string GetCurrentWebLanguage()
        {
            if (_dictWebLanguage.ContainsKey(CurrentLanguage))
            {
                return _dictWebLanguage[CurrentLanguage];
            }

            return "en";
        }

        #endregion

        #region -- Private Methods --

        private String GetFilePathByType(LanguageType languageType)
        {
            String fileName;
            if (_languageTypeVsFileName.ContainsKey(languageType))
            {
                fileName = _languageTypeVsFileName[languageType];
            }
            else
            {
                fileName = _languageTypeVsFileName[LanguageType.ZH_CN];
            }

            return String.Format(LANGUAGE_FILE_PATH, fileName);
        }


        private static LanguageType GetCurrentLanguage()
        {
            String lang = "";
            try
            {
                lang = Utils.GetLanguage();
            }
            catch (Exception)
            {
            }

            if (String.IsNullOrEmpty(lang))
            {
                return LanguageType.ZH_CN;
            }
            else
            {
                LanguageType currentLang = LanguageType.ZH_CN;
                try
                {
                    currentLang = (LanguageType)Enum.Parse(typeof(LanguageType), lang);
                }
                catch (Exception)
                {
                    currentLang = LanguageType.ZH_CN;
                }

                return currentLang;
            }
        }

        private static void SaveCurrentLanguage(LanguageType language)
        {
            try
            {
                String strLang = Enum.GetName(typeof(LanguageType), language);
                Utils.SaveLanguage(strLang);
            }
            catch (Exception)
            {
            }
        }

        #endregion
        
    }
}
