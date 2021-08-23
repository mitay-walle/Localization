using System;
using UnityEngine;

namespace mitaywalle.Plugins.Localization
{
	[Serializable]
	public class LocalizedTextFormatComposition : LocalizedTextComposition
	{
		public string[] formatItems = new string[0];
		public string prefix;
		public string postfix;

		public virtual string SetItems(params string[] newItems)
		{
			if (formatItems.Length != newItems.Length)
			{
				formatItems = newItems;
			}
			else
			{
				for (int i = 0; i < newItems.Length; i++)
					formatItems[i] = newItems[i];	
			}

			return Set();
		}
		public override string Set(bool logs = true)
		{
			var targetExist = Target != null;
		
			if (!logs) // OnValidate
			{
				if (!targetExist) return null;
			}

			if (formatItems.Length == 0)
			{
#if UNITY_EDITOR
				//Debug.LogError("не указано аргументов для string.format !",this);
#endif
				var text = prefix + 1.L(Key, logs) + postfix;
				if (targetExist)
				{
					Target.text = text;
					ResizeInternal();	
				}
			
			
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
					Debug.LogError($"неправильный формат строчки! {e}",Target);
				}
			}
			else
#endif
			{
				text2 = prefix + 1.LF(Key,formatItems,logs) + postfix;
			}

			if (targetExist)
			{
				Target.text = text2;
		
				ResizeInternal();	
			}
		
			return text2;
		}
	}
}
