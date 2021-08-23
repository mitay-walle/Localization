namespace mitaywalle.Plugins.Localization
{
    public static class LocalizationHelper
    {
        public static string L(this object obj, string key, bool logs = true)
        {
#if UNITY_EDITOR
            if (!LocalizationManager.Inst) return string.Empty; // для OnValidate
#endif

            return LocalizationManager.Inst.Localize(key, logs);
        }

        #region Format

        public static string LF(this object obj, string key, object[] args, bool logs = true)
        {
            return string.Format(L(1, key), args: args);
        }

        public static string LF(this object obj, string key, object arg, bool logs = true)
        {
            return string.Format(L(1, key), arg);
        }

        public static string LF(this object obj, string key, object arg, object arg2, bool logs = true)
        {
            return string.Format(L(1, key), arg, arg2);
        }

        public static string LF(this object obj, string key, object arg, object arg2, object arg3, bool logs = true)
        {
            return string.Format(L(1, key), arg, arg2, arg3);
        }

        #endregion
    }
}