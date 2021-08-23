using System;
using UnityEngine;

namespace mitaywalle.Plugins.Localization
{
	public class LocalizedTextFormat : LocalizedText
	{
		public string[] formatItems;
		public string prefix;
		public string postfix;

		public virtual string SetItems(params string[] newItems)
		{
			for (int i = 0; i < newItems.Length; i++)
				formatItems[i] = newItems[i];

			return Localize();
		}
		public override string Localize(bool logs = true)
		{
			if (!logs) // OnValidate
			{
				if (!Target) return string.Empty;
			}

			if (formatItems.Length == 0)
			{
#if UNITY_EDITOR
				//Debug.LogError("не указано аргументов для string.format !",this);
#endif
				var text = prefix + 1.L(Key, logs) + postfix;
				Target.text = text;
				ResizeInternal();
			
				return text;
			
			}

			var text2 = string.Empty;
		
#if UNITY_EDITOR
			if (!Application.isPlaying)
			{
				try
				{
					text2 = prefix + 1.LF(Key,formatItems,logs) + postfix;
				}
				catch(Exception e)
				{
					Debug.LogError($"неправильный формат строчки! {e}",gameObject);
				}
			}
			else
#endif
			{
				text2 = prefix + 1.LF(Key,formatItems,logs) + postfix;
			}
		
			Target.text = text2;
		
			ResizeInternal();

			return Target.text = text2;
		}

		protected override void OnEnable()
		{
			base.OnEnable();
		}
	}
}
