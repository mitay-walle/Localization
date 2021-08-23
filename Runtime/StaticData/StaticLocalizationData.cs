using CarConstructorV2.Scripts.Additional;
using UnityEditor;
using UnityEngine;

namespace mitaywalle.Plugins.Localization.StaticData
{
    public sealed class StaticLocalizationData : StaticDataBase<string, LocalizeUnit, StaticLocalizationData>
    {
        public const string DATA_TYPE_CONST = "Localization";

        public override string DATA_TYPE()
        {
            return DATA_TYPE_CONST;
        }

        public const string ASSET_PATH_CONST = STATIC_DATA_PATH + DATA_TYPE_CONST;

        public override string ASSET_PATH()
        {
            return ASSET_PATH_CONST;
        }

#if UNITY_EDITOR
        [InitializeOnLoadMethod]
#endif
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        public static void InitStaticData()
        {
            CreateInstance();
            Inst.DeserealizeData(DeserealizeDataToString(Inst.ASSET_PATH()));
            var manager = Resources.Load<LocalizationManager>("LocalizationManager");
            manager.Init();
        }

        public override void DeserealizeData(string jsonString, bool logs = true)
        {
            if (string.IsNullOrEmpty(jsonString))
            {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                Debug.LogError($"Deserealize {DATA_TYPE_CONST} Data: пустой json!");
#endif
                return;
            }

            JSONClass nodes = JSON.Parse(jsonString) as JSONClass;

            foreach (var dataConverted in nodes.GetDict())
            {
                Debug.LogError(dataConverted.Key);

                string key = dataConverted.Key;

                if (DB.ContainsKey(key)) continue;

                var data = new LocalizeUnit();

                var enumerator = dataConverted.Value.Childs.GetEnumerator();
                enumerator.MoveNext();
                data.text[(int) LocalizationManager.GameLanguage.Russian] = enumerator.Current.Value;
                enumerator.MoveNext();
                data.text[(int) LocalizationManager.GameLanguage.English] = enumerator.Current.Value;

                DB.Add(key, data);
            }

#if UNITY_EDITOR
            //Debug.Log($"Deserealized {DATA_TYPE_CONST} Data: {DB.Keys.Count } \n data = {db}" );
#endif
        }

        public new static StaticLocalizationData CreateInstance()
        {
            return _inst = new StaticLocalizationData();
        }

#if UNITY_EDITOR
        [MenuItem("Tools/mitaywalle/выделить " + DATA_TYPE_CONST + " Data")]
        public static void Select()
        {
            Debug.Log(ASSETS_DATA_PATH + Inst.ASSET_PATH() + JSON_EXTENSION);

            Selection.activeObject =
                AssetDatabase.LoadAssetAtPath<TextAsset>(ASSETS_DATA_PATH + Inst.ASSET_PATH() + JSON_EXTENSION);
            EditorGUIUtility.PingObject(Selection.activeObject);
        }
#endif
    }
}