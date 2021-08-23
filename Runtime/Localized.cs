using System;
using UnityEditor;
using UnityEngine;

namespace mitaywalle.Plugins.Localization
{
	public abstract class Localized : MonoBehaviour,ILocalized
	{
		public bool LocalizeOnEnable = true;
		public bool LocalizeOnChange = true;
		public bool CheckEqualsOnLocalize = true;
		public string Key;

		public string Set(string key, bool logs = true)
		{
			Key = key;
			return Localize(logs);
		}
	
		public abstract string Localize(bool logs = true);


		#region Editor

#if UNITY_EDITOR

		protected virtual void PreGUI()
		{
			if (!LocalizeOnChange) return;
			if (!enabled) return;
			if (string.IsNullOrEmpty(Key)) return;
			EditorUtility.SetDirty(this);
			Localize(false);
		}
		public abstract void Reset();

		[CustomEditor(typeof(Localized), true), CanEditMultipleObjects]
		public class LocalizedEditor : Editor
		{
			private Localized Localized;

			private void OnEnable()
			{
				Localized = target as Localized;
			}

			public override void OnInspectorGUI()
			{
				LocalizationManager.DrawSelect();
				Localized.PreGUI();

				base.OnInspectorGUI();
				DrawButton("Reset", (t) => t.Reset());
			}

			private void DrawButton(string btnText, Action<Localized> action)
			{
				if (GUILayout.Button(btnText))
				{
					foreach (var targ in targets)
					{
						if (targ is Localized subTarg) action.Invoke(subTarg);
					}
				}
			}
		}
#endif

		#endregion
	}

	public abstract class Localized<T> : Localized where T: Component 
	{
		public T Target;

		protected virtual void OnEnable()
		{
			if (!enabled || !LocalizeOnEnable) return; Localize();
		}

		#region Editor

#if UNITY_EDITOR

		protected override void PreGUI()
		{
			if (!LocalizeOnChange) return;
			if (!enabled) return;
			if (!Target) return;
			if (string.IsNullOrEmpty(Key)) return;
			EditorUtility.SetDirty(this);
			Localize(false);
			EditorUtility.SetDirty(Target);
		} 
		[ContextMenu("Заполнить ссылки")]
		public override void Reset()
		{
			Undo.RecordObject(this,"fill refs");
		
			Target = GetComponent<T>();
		
			EditorUtility.SetDirty(this);
		
			if (Target) PreGUI();
		}

#endif

		#endregion
	}
}