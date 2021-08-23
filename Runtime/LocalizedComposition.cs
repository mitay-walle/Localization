using System;

namespace mitaywalle.Plugins.Localization
{
    [Serializable]
    public class LocalizedComposition
    {
        public string Key;

        public string Set(string key, bool logs = true)
        {
            Key = key;
            return Set(logs);
        }

        public virtual string Set(bool logs = true)
        {
            return Key;
        }
    }
}