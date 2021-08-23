using UnityEditor;
using UnityEngine;

namespace Plugins.GoogleSheetWindow
{
	public class EditorWindowBaseNonGeneric : EditorWindow { }

	public class EditorWindowBase<T> : EditorWindowBaseNonGeneric where T : EditorWindowBase<T>
	{
		public const string Title = "Editor";
		protected const string BasePath = "Tools/mitaywalle/";
		//[MenuItem(BasePath + Title)]
		public static void OpenWindow()
		{
			var window = GetWindow<T>(Title);

			if (!window) window = CreateInstance<T>();
			window.ShowUtility();
		}

		protected virtual void OnFocus()
		{
			titleContent = new GUIContent(Title);
		}
	
		protected Rect btnRect = new Rect(0f,0f,100f,35f);
		public bool uponOthers;
		protected Vector2 mPos;
	
		protected virtual void OnGUI()
		{
			GUI.enabled = false;
			var data = EditorGUILayout.ObjectField(MonoScript.FromScriptableObject(this), typeof(MonoScript), false) as MonoScript;
			GUI.enabled = true;
			GUILayout.Space(10f);
		
			mPos = Event.current.mousePosition;
		}
	
		protected virtual void Update()
		{
			var newmPos = position;
			newmPos.x = 0f;
			newmPos.y = 0f;

			if (uponOthers)
			{
				Repaint();
				if (newmPos.Contains(mPos) && focusedWindow != this) Focus();
			}
		
		}
		protected void ToggleUtility()
		{
			uponOthers = !uponOthers;
		}
	}
}