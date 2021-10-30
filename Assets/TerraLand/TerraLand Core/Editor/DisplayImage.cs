using UnityEngine;
using UnityEditor;

public class DisplayImage : EditorWindow
{
	public Vector2 scrollPosition = Vector2.zero;
	private bool allowSceneObjects = true;
	public Texture2D satelliteImage;
	public float windowWidth;
	public int imageResolution;
	public int mapZoomWidth;
	public int mapZoomHeight;

	public void OnGUI ()
	{
		GUILayout.Space(10);

		EditorGUILayout.BeginHorizontal();
		GUILayout.FlexibleSpace();
		EditorGUILayout.HelpBox("IMAGE", MessageType.None, true);
		satelliteImage = (Texture2D)EditorGUILayout.ObjectField(satelliteImage, typeof(Texture2D), allowSceneObjects) as Texture2D;
		GUILayout.FlexibleSpace();
		EditorGUILayout.EndHorizontal();

		GUILayout.Space(20);

		EditorGUILayout.BeginHorizontal();
		GUILayout.FlexibleSpace();
		EditorGUILayout.HelpBox("ZOOM", MessageType.None, true);
		
		mapZoomWidth = EditorGUILayout.IntSlider(mapZoomWidth, 64, imageResolution);
		mapZoomHeight = mapZoomWidth;

		GUILayout.Space(5);

		if (GUILayout.Button("FIT IMAGE"))
			CenterMap();
		
		GUILayout.FlexibleSpace();
		EditorGUILayout.EndHorizontal();
		
		Rect rect2 = GUILayoutUtility.GetLastRect();
		float xOffset = 0;
		
		if (mapZoomWidth < position.width - 24)
			xOffset = (position.width / 2) - (mapZoomWidth / 2) - 20;
		
		scrollPosition = GUI.BeginScrollView
		(
			new Rect
			(
				10,
				rect2.y + 40,
				GUILayoutUtility.GetLastRect().width - 20,
				GUILayoutUtility.GetLastRect().width - 20
			),
		    scrollPosition,
		    new Rect
			(
				0,
				0,
				mapZoomWidth,
				mapZoomHeight
			)
		);
		
		EditorGUI.DrawPreviewTexture(new Rect(xOffset, 0, mapZoomWidth, mapZoomHeight), satelliteImage);
		GUI.EndScrollView();
		
		if (Event.current.type == EventType.Repaint)
			windowWidth = GUILayoutUtility.GetLastRect().width;
		
		if (mapZoomWidth > windowWidth)
			GUILayout.Space(windowWidth + 10);
		else if (mapZoomHeight > windowWidth)
			GUILayout.Space(windowWidth + 10);
		else
			GUILayout.Space(mapZoomHeight + 30);
		
		GUILayout.Space(30);
	}

	public void CenterMap ()
	{
		mapZoomWidth = Mathf.RoundToInt(position.width - 24);
	}
}

