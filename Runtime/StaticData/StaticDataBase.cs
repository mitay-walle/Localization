using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace mitaywalle.Plugins.Localization.StaticData
{
    public abstract class StaticDataBase
    {
    }

    public abstract class StaticDataBase<T, T2, T3> : StaticDataBase where T3 : StaticDataBase<T, T2, T3>
    {
        public static T3 _inst;

        public static T3 Inst
        {
            get { return _inst ?? (_inst = CreateInstance()); }
        }

        public const string ASSETS_DATA_PATH = "Assets/Resources/";
        public const string STATIC_DATA_PATH = "";
        protected const string JSON_EXTENSION = ".json";

        public static Dictionary<T, T2> DB = new Dictionary<T, T2>();

        public abstract string DATA_TYPE();

        public abstract string ASSET_PATH();

        public static T3 CreateInstance()
        {
            return null;
        }

        public abstract void DeserealizeData(string jsonString, bool logs = true);


        public static string DeserealizeDataToString(string path, bool logs = true)
        {
            var textAsset = (TextAsset) Resources.Load(path);
#if UNITY_EDITOR
            if (textAsset) EditorUtility.SetDirty(textAsset);
            else return string.Empty;
#endif
            return textAsset.text;
        }
    }
}