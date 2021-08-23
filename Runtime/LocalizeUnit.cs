using System;

namespace mitaywalle.Plugins.Localization
{
    [Serializable]
    public class LocalizeUnit
    {
        public string key;
        public string[] text = new string[LocalizationManager.MAX_LANGUAGES_SUPPORT];
    }
}