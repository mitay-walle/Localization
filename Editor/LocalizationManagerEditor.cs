using System;
using mitaywalle.Plugins.Editor.GoogleSheetWindow;
using mitaywalle.Plugins.Localization.StaticData;
using UnityEditor;
using UnityEngine;

namespace mitaywalle.Plugins.Localization.Editor
{
    [CustomEditor(typeof(LocalizationManager), true), CanEditMultipleObjects]
    public class LocalizationManagerEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            EditorGUILayout.Toggle("Inited", LocalizationManager.Inited);
            
            base.OnInspectorGUI();
            
            DrawButton("Window", (t) => GoogleSheetWindow.OpenWindow());
            DrawButton("Table", (t) => GoogleSheetWindow.OpenLink());
            
            GUILayout.Label("JSON");
            DrawButton("Reimport", (t) => LocalizationManager.ReimportLocalization_DB());
            DrawButton("Select", (t) => StaticLocalizationData.Select());
            
            GUILayout.Label("Scene Components");
            DrawButton("Localize All", (t) => LocalizationManager.LocalizeAll());
        }

        private void DrawButton(string btnText, Action<LocalizationManager> action)
        {
            if (GUILayout.Button(btnText))
            {
                foreach (var targ in targets)
                {
                    if (targ is LocalizationManager subTarg) action.Invoke(subTarg);
                }
            }
        }
    }
}