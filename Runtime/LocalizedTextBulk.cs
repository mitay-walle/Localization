using TMPro;
using UnityEditor;
using UnityEngine;

namespace mitaywalle.Plugins.Localization
{
    public class LocalizedTextBulk : Localized
    {
        public TMP_Text[] Targets;
        public bool Resize;
        public bool ResizeXOnly;
        public RectTransform Resize_bg;

        public override string Localize(bool logs = true)
        {
            if (!logs && (Targets == null || Targets.Length == 0)) return string.Empty;

            for (int i = 0; i < Targets.Length; i++)
            {
                if (!Targets[i]) continue;

                Targets[i].text = 1.L(Key, logs);

                ResizeInternal(Targets[i]);
            }

            return Targets[0]?.text;
        }

        protected void Start()
        {
            if (!enabled) return;
            Localize();
        }

        protected void ResizeInternal(TMP_Text Target)
        {
            if (!Resize) return;

            if (ResizeXOnly)
            {
                Target.GetPreferredValues();

                var size = Target.rectTransform.sizeDelta;
                size.x = Target.GetPreferredValues().x;

                Target.rectTransform.sizeDelta = size;
            }
            else
            {
                Target.GetPreferredValues();
                Target.rectTransform.sizeDelta = Target.GetPreferredValues();
            }

            if (Resize_bg && Target) Resize_bg.sizeDelta = Target.rectTransform.sizeDelta;

#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                if (Target) EditorUtility.SetDirty(Target.rectTransform);
                if (Resize_bg) EditorUtility.SetDirty(Resize_bg);
            }
#endif
        }



        [ContextMenu("Заполнить ссылки")]
        public override void Reset()
        {
#if UNITY_EDITOR
            Undo.RecordObject(this, "fill refs");

            Targets = GetComponentsInChildren<TMP_Text>();

            EditorUtility.SetDirty(this);

            if (Targets != null && Targets.Length > 0) PreGUI();
#endif
        }
    
#if UNITY_EDITOR
    
        protected override void PreGUI()
        {
            if (!LocalizeOnChange) return;
            if (!enabled) return;
            if (string.IsNullOrEmpty(Key)) return;

            EditorUtility.SetDirty(this);

            Localize(false);

            EditorUtility.SetDirty(this);

            for (int i = 0; i < Targets.Length; i++)
            {
                if (!Targets[i]) continue;
                EditorUtility.SetDirty(Targets[i]);
            }
        }
#endif
    }
}