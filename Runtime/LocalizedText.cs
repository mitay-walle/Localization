using TMPro;
using UnityEditor;
using UnityEngine;

namespace mitaywalle.Plugins.Localization
{
	public class LocalizedText : Localized<TextMeshProUGUI>
	{
		public bool Resize;
		public bool ResizeXOnly;
		public RectTransform Resize_bg;
	
		public override string Localize(bool logs = true)
		{
			if (!logs && !Target) return string.Empty;

			if (CheckEqualsOnLocalize)
			{
				var newText = 1.L(Key, logs);
				if (!newText.Equals(Target.text)) Target.text = newText;
			}
			else
			{
				Target.text = 1.L(Key, logs);
			}

			ResizeInternal();
		
			return Target.text;
		}

		public void ResetLayout()
		{
			ResizeInternal();
		}
		protected void ResizeInternal()
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
	}
}
 