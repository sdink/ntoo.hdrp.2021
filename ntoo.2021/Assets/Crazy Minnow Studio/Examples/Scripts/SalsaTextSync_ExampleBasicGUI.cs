using UnityEngine;

namespace CrazyMinnow.SALSA.TextSync
{
	public class SalsaTextSync_ExampleBasicGUI : MonoBehaviour
	{
		[Range(0f, 1f)]
		public float fillPercentage = 0.25f;

		private string text = "";

		/// <summary>
		/// Simple UI to show dialogue text
		/// </summary>
		void OnGUI()
		{
			if (text.Length > 0)
			{
				RectOffset rectOffset = new RectOffset(100, 100, 20, 20);
				Texture2D tex = new Texture2D(1, 1);
				GUIStyle style = new GUIStyle();
				style.fontSize = 16;
				style.normal.textColor = new Color(1f, 1f, 1f);
				style.wordWrap = true;
				style.padding = rectOffset;
				style.normal.background = tex;
				tex.SetPixel(1, 1, new Color(0, 0, 0, 0.75f));
				tex.Apply();

				Rect rect = new Rect(0f, Screen.height - (Screen.height * fillPercentage), Screen.width, Screen.height);
				GUI.Box(rect, text, style);
			}
		}
	}
}