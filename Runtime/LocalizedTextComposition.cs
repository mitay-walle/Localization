using System;
using TMPro;
using UnityEditor;
using UnityEngine;

namespace mitaywalle.Plugins.Localization
{
	[Serializable]
	public class LocalizedTextComposition : LocalizedComposition
	{
		public TMP_Text Target;
		public bool Resize;
		public bool ResizeXOnly;
		public RectTransform Resize_bg;
	
		public override string Set(bool logs = true)
		{
			var targetExist = Target != null;
			if (!logs && !targetExist) return null;

			var text = 1.L(Key, logs);

			if (targetExist)
			{
				Target.text = text;
		
				ResizeInternal();	
			}
		
			return text;
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
 