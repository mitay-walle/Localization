using System.Linq;
using mitaywalle.Plugins.Localization.StaticData;
using UnityEditor;
using UnityEngine;

namespace mitaywalle.Plugins.Localization
{
    [CreateAssetMenu]
    public class LocalizationManager : ScriptableObject
    {
        public const int MAX_LANGUAGES_SUPPORT = (int) GameLanguage.Count;

        public static LocalizationManager Inst;

        public static bool Inited = false;
        public const bool Debugging = false;

        public enum GameLanguage
        {
            Russian = 0,
            English = 1,
            Count,
        }

        public GameLanguage CurrentLanguage = GameLanguage.Russian;

        //public UnityPathTextAsset json;

        public void Init()
        {
            Inited = true;
            Inst = this;
        }

#if UNITY_EDITOR

        private static TextAsset LocalizeTextAsset;// = Resources.Load<TextAsset>(StaticLocalizationData.ASSETS_DATA_PATH);

        [MenuItem("Tools/mitaywalle/Reimport Localization"), InitializeOnLoadMethod]
        public static void ReimportLocalization_DB()
        {
            ReimportLocalization_DB(true);
        }

        [ContextMenu("Reimport Localization")]
        void ReimportLocalization_DBInternal()
        {
            //GoogleSheetWindow.GoogleSheetWindow.ReimportLocalization();
            //EditorApplication.delayCall += () =>
            {
                EditorApplication.delayCall += () => { ReimportLocalization_DB(true); };
            }
            ;
        }

        [ContextMenu("Открыть окно загрузки из таблиц")]
        void OpenImportGoogleSheet()
        {
            //EditorWindow.GetWindow<GoogleSheetWindow.GoogleSheetWindow>();
        }

        [MenuItem("Tools/mitaywalle/Localization Manager")]
        public static void Select()
        {
            Selection.activeObject = Inst;
        }
#endif

        public static void LocalizeAll()
        {
            if (StaticLocalizationData.DB.Count == 0)
                StaticLocalizationData.DeserealizeDataToString(
                    Resources.Load<TextAsset>(StaticLocalizationData.ASSET_PATH_CONST).text, false);

            var components = FindObjectsOfType<MonoBehaviour>().OfType<ILocalized>().ToList();

            components = components.Distinct().ToList();
            //Debug.Log(components.Length);

            for (int i = 0; i < components.Count; i++)
            {
                if (components[i].enabled) components[i].Localize();

#if UNITY_EDITOR
                if (!Application.isPlaying) EditorUtility.SetDirty(components[i] as MonoBehaviour);
#endif
                //Debug.Log(components[i].name);
            }
        }

        public static void ReimportLocalization_DB(bool logs)
        {
            StaticLocalizationData.DB.Clear();
            StaticLocalizationData.DeserealizeDataToString(
                Resources.Load<TextAsset>(StaticLocalizationData.ASSET_PATH_CONST).text, logs);
            //if (logs) Debug.Log("локализация загружена");
        }

        public string
            Localize(string key, bool logs = true) // Подгружаем перевод необходимого текста из файлика локализации
        {
#if UNITY_EDITOR
            if (StaticLocalizationData.DB.Count == 0) ReimportLocalization_DB(false);
#endif
            if (key == null) return null;

            string text = StaticLocalizationData.DB.ContainsKey(key)
                ? StaticLocalizationData.DB[key].text[(int) CurrentLanguage]
                : string.Empty;

#if UNITY_EDITOR
            if (Debugging) Debug.Log($"Localize: язык = {CurrentLanguage}, ключ= {key}, перевод = {text}");
#endif

            if (string.IsNullOrEmpty(text))
            {
#if UNITY_EDITOR
                if (Debugging)
                    if (logs)
                        Debug.Log($"ключ {key} не имеет перевода на {CurrentLanguage} !!!");
#endif
                return key;
            }

            if (CurrentLanguage == GameLanguage.Russian && !string.IsNullOrEmpty(text))
            {
                text = text.Replace('ё', 'e');
                return text.Replace('Ё', 'Е');
            }

            return text;
        }


        public static void DrawSelect()
        {
#if UNITY_EDITOR
            EditorGUILayout.ObjectField(Inst, typeof(LocalizationManager),false);
#endif
        }
    }
}