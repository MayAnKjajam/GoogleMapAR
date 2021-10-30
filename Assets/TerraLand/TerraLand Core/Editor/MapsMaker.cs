using UnityEngine;
using UnityEditor;
using AForge;
using AForge.Imaging;
using AForge.Imaging.ColorReduction;
using AForge.Imaging.ComplexFilters;
using AForge.Imaging.Filters;
using AForge.Imaging.Textures;
using AForge.Math.Geometry;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.InteropServices;
using System.Threading;


public class MapsMaker : EditorWindow
{
	[UnityEditor.MenuItem("Tools/TerraUnity/TerraLand/Maps Maker", false, 2)]
	public static void Init()
	{
		MapsMaker window = (MapsMaker)EditorWindow.GetWindow(typeof(MapsMaker));
		window.position = new Rect(5, 135, 430, 800);
	}

	Vector2 scrollPosition = Vector2.zero;
	private bool allowSceneObjects = true;
	
    UnityEngine.Object[] terraUnityImages;
    Texture2D logo;
	
    string dataPath;
	string folderPath;
	string sourceImagePath;
	string outputImagePath;
	string imageFormatStr;
	ImageFormat imageFormat;
	Bitmap sourceImage;
	float shadowBrightness;
	UnityEngine.Color shadowColor = UnityEngine.Color.gray;
	Texture2D satelliteImage;
	int imageResolution;
	int satelliteImageWidth;
	int satelliteImageHeight;
	int mapZoomWidth;
	int mapZoomHeight;
	string unavailableImageStr = "No Images Selected.\n\nSelect Image(s) From The Project Panel First.";
	int erodeSize = 3;
	int colormapKernelSize;
	int colormapSharpness;
	
	int healingBlockSize = 50;
	System.Drawing.Color color;
	float currentPixelbrightness;
	System.Drawing.Color pixelColor;
	float pixelColorBrightness;
	float shadowColorBrightness;
	
	float H;
	float S;
	float V;
	
	int filtersNo;
	List<string> filterNames = new List<string>();
	List<UnityEngine.Color> filterColors = new List<UnityEngine.Color>();
	List<System.Drawing.Color> filterColorsDrawing = new List<System.Drawing.Color>();
	List<System.Drawing.Color> outputColorsDrawing = new List<System.Drawing.Color>();
	List<int> filterRadius = new List<int>();
	List<UnityEngine.Color> outputFilterColors = new List<UnityEngine.Color>();
	int mapColorIndex;
	
	string presetFilePath;
	string presetName;
	
	Dictionary<System.Drawing.Color, int> dictColors;
	int tolerance;
	int filterRadiusAuto;
	int filterSmoothingKernelSize;
	int topColorsCount;
	string[] landCoverMode = new string[] {"ON", "OFF"};
	int landCoverModeIndex = 0;
	bool showPresetManager = false;
	List<System.Drawing.Color> extractedColors = new List<System.Drawing.Color>();
	
	string[] splatFormat = new string[] {"PNG", "TIFF"};
	int splatFormatIndex = 0;
	
	string[] outputOrdering = new string[] {"SEQUENTIAL", "PREDICTABLE"};
	int outputOrderingIndex = 0;
	int resetter = 0;
	string outColStr;
	List<UnityEngine.Color> outCols = new List<UnityEngine.Color>()
	{
		UnityEngine.Color.red,
		UnityEngine.Color.green,
		UnityEngine.Color.blue,
		UnityEngine.Color.black
	};
	
	bool shadowRemoverOperation = false;
	bool colormapOperation = false;
	bool landCoverOperation = false;
	bool filtersGenerationOperation = false;
	
	float progressShadow = 0;
	float progressColormap = 0;
	float progressLandCover = 0;
	float progressFilters = 0;
	bool showProgressShadow = false;
	bool showProgressColormap = false;
	bool showProgressLandCover = false;
	bool showProgressFilters = false;
	int progressShadowIndex = 0;
	int progressColormapIndex = 0;
	int progressLandCoverIndex = 0;
	int progressFiltersIndex = 0;
	int progressShadowStage = 0;
	int progressColormapStage = 0;
	int progressLandCoverStage = 0;
	int progressFiltersStage = 0;
	string progressShadowStr;
	string progressColormapStr;
	string progressLandCoverStr;
	string progressFiltersStr;

	bool showProcessingSection = true;
	bool showShadowSection = true;
	bool showColormapSection = true;
	bool showLandcoverSection = true;
	string engineBusyStr = "Another Operation Is Running.\n\nWait For The Operation to Finish and Try Again.";
	string engineBusySelfStr = "Operation Is Currently Running";
	string mapTypeStr;

    string corePath;
    string presetsPath;

	[System.Serializable]
	public struct HSBColor
	{
		public float h;
		public float s;
		public float b;
		public float a;
		
		public HSBColor(float h, float s, float b, float a)
		{
			this.h = h;
			this.s = s;
			this.b = b;
			this.a = a;
		}
		
		public HSBColor(float h, float s, float b)
		{
			this.h = h;
			this.s = s;
			this.b = b;
			this.a = 1f;
		}
		
		public HSBColor(UnityEngine.Color col)
		{
			HSBColor temp = FromColor(col);
			h = temp.h;
			s = temp.s;
			b = temp.b;
			a = temp.a;
		}
		
		public static HSBColor FromColor(UnityEngine.Color color)
		{
			HSBColor ret = new HSBColor(0f, 0f, 0f, color.a);
			
			float r = color.r;
			float g = color.g;
			float b = color.b;
			
			float max = Mathf.Max(r, Mathf.Max(g, b));
			
			if (max <= 0)
			{
				return ret;
			}
			
			float min = Mathf.Min(r, Mathf.Min(g, b));
			float dif = max - min;
			
			if (max > min)
			{
				if (g == max)
				{
					ret.h = (b - r) / dif * 60f + 120f;
				}
				else if (b == max)
				{
					ret.h = (r - g) / dif * 60f + 240f;
				}
				else if (b > g)
				{
					ret.h = (g - b) / dif * 60f + 360f;
				}
				else
				{
					ret.h = (g - b) / dif * 60f;
				}
				if (ret.h < 0)
				{
					ret.h = ret.h + 360f;
				}
			}
			else
			{
				ret.h = 0;
			}
			
			ret.h *= 1f / 360f;
			ret.s = (dif / max) * 1f;
			ret.b = max;
			
			return ret;
		}
		
		public static UnityEngine.Color ToColor(HSBColor hsbColor)
		{
			float r = hsbColor.b;
			float g = hsbColor.b;
			float b = hsbColor.b;

			if (hsbColor.s != 0)
			{
				float max = hsbColor.b;
				float dif = hsbColor.b * hsbColor.s;
				float min = hsbColor.b - dif;
				
				float h = hsbColor.h * 360f;
				
				if (h < 60f)
				{
					r = max;
					g = h * dif / 60f + min;
					b = min;
				}
				else if (h < 120f)
				{
					r = -(h - 120f) * dif / 60f + min;
					g = max;
					b = min;
				}
				else if (h < 180f)
				{
					r = min;
					g = max;
					b = (h - 120f) * dif / 60f + min;
				}
				else if (h < 240f)
				{
					r = min;
					g = -(h - 240f) * dif / 60f + min;
					b = max;
				}
				else if (h < 300f)
				{
					r = (h - 240f) * dif / 60f + min;
					g = min;
					b = max;
				}
				else if (h <= 360f)
				{
					r = max;
					g = min;
					b = -(h - 360f) * dif / 60 + min;
				}
				else
				{
					r = 0;
					g = 0;
					b = 0;
				}
			}
			
			return new UnityEngine.Color(Mathf.Clamp01(r),Mathf.Clamp01(g),Mathf.Clamp01(b),hsbColor.a);
		}
		
		public UnityEngine.Color ToColor()
		{
			return ToColor(this);
		}
		
		public override string ToString()
		{
			return "H:" + h + " S:" + s + " B:" + b;
		}
		
		public static HSBColor Lerp(HSBColor a, HSBColor b, float t)
		{
			float h,s;
			
			//check special case black (color.b==0): interpolate neither hue nor saturation!
			//check special case grey (color.s==0): don't interpolate hue!
			if(a.b == 0)
			{
				h = b.h;
				s = b.s;
			}
			else if(b.b == 0)
			{
				h = a.h;
				s = a.s;
			}
			else
			{
				if(a.s == 0)
				{
					h = b.h;
				}
				else if(b.s == 0)
				{
					h = a.h;
				}
				else
				{
					// works around bug with LerpAngle
					float angle = Mathf.LerpAngle(a.h * 360f, b.h * 360f, t);

					while (angle < 0f)
						angle += 360f;

					while (angle > 360f)
						angle -= 360f;

					h = angle / 360f;
				}
				s = Mathf.Lerp(a.s,b.s,t);
			}
			return new HSBColor(h, s, Mathf.Lerp(a.b, b.b, t), Mathf.Lerp(a.a, b.a, t));
		}
	}

	public enum MapColor
	{
		_Red = 1,
		_Green,
		_Blue,
		_Alpha
	}
	static List<MapColor> mapColors = new List<MapColor>();


	#region multithreading variables
	
	int maxThreads = 8;
	private int numThreads;
	private int _count;
	
	private bool m_HasLoaded = false;
	
	private List<Action> _actions = new List<Action>();
	private List<DelayedQueueItem> _delayed = new  List<DelayedQueueItem>();
	
	private List<DelayedQueueItem> _currentDelayed = new List<DelayedQueueItem>();
	private List<Action> _currentActions = new List<Action>();
	
	public struct DelayedQueueItem
	{
		public float time;
		public Action action;
	}
	
	#endregion

                        
  	public void OnEnable ()
	{
		LoadResources();
		
		#if UNITY_WEBPLAYER
		SwitchPlatform();
		#endif

		AutoLoad();
	}

	public void OnDisable()
	{
		AutoSave();
	}
	
	public void LoadResources ()
	{
		TextureImporter imageImport;
		bool forceUpdate = false;

        terraUnityImages = Resources.LoadAll("TerraUnity/Images", typeof(Texture2D));
        logo = Resources.Load("TerraUnity/Images/Logo/TerraLand-MapsMaker_Logo") as Texture2D;
		
		foreach (Texture2D currentImage in terraUnityImages)
		{
			imageImport = AssetImporter.GetAtPath(AssetDatabase.GetAssetPath(currentImage)) as TextureImporter;
			
			if (imageImport.npotScale != TextureImporterNPOTScale.None)
			{
				imageImport.npotScale = TextureImporterNPOTScale.None;
				forceUpdate = true;
			}
			if (imageImport.mipmapEnabled)
			{
				imageImport.mipmapEnabled = false;
				forceUpdate = true;
			}
			
			if(forceUpdate)
				AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(currentImage), ImportAssetOptions.ForceUpdate);
		}

        dataPath = UnityEngine.Application.dataPath;
        corePath = dataPath + "/TerraLand/TerraLand Core/";
        presetsPath = corePath + "Presets/MapsMaker";
		
		System.GC.Collect();
		System.GC.WaitForPendingFinalizers();
		System.GC.Collect();
	}
	
	private void SwitchPlatform ()
	{  
#if UNITY_5_6_OR_NEWER
#if UNITY_2017_3_OR_NEWER
        if(UnityEngine.Application.platform == RuntimePlatform.WindowsEditor)
            EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Standalone, BuildTarget.StandaloneWindows);
        else if(UnityEngine.Application.platform == RuntimePlatform.OSXEditor)
            EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Standalone, BuildTarget.StandaloneOSX);
        else if(UnityEngine.Application.platform == RuntimePlatform.LinuxPlayer)
            EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Standalone, BuildTarget.StandaloneLinux);
#else
            if(UnityEngine.Application.platform == RuntimePlatform.WindowsEditor)
                EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Standalone, BuildTarget.StandaloneWindows);
            else if(UnityEngine.Application.platform == RuntimePlatform.OSXEditor)
                EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Standalone, BuildTarget.StandaloneOSXIntel);
            else if(UnityEngine.Application.platform == RuntimePlatform.LinuxPlayer)
                EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Standalone, BuildTarget.StandaloneLinux);
#endif
#else
            if(UnityEngine.Application.platform == RuntimePlatform.WindowsEditor)
                EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTarget.StandaloneWindows);
            else if(UnityEngine.Application.platform == RuntimePlatform.OSXEditor)
                EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTarget.StandaloneOSXIntel);
            else if(UnityEngine.Application.platform == RuntimePlatform.LinuxPlayer)
                EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTarget.StandaloneLinux);
#endif
	}
	
	public void OnGUI ()
	{
		GUILayout.Space(10);
		
		GUI.backgroundColor = new UnityEngine.Color(0, 0, 0, 1.0f);
		
		if (GUILayout.Button(logo))
			UnityEditor.Help.BrowseURL("http://www.terraunity.com");
		
		GUI.backgroundColor = new UnityEngine.Color(1, 1, 1, 1.0f);
			
		GUIStyle buttonStyle = new GUIStyle(EditorStyles.toolbarButton);
		
		EditorGUILayout.BeginHorizontal();
		
		if (showPresetManager)
			GUI.backgroundColor = UnityEngine.Color.green;
		else
			GUI.backgroundColor = UnityEngine.Color.white;
		
		if (GUILayout.Button ("Preset Management", buttonStyle, GUILayout.ExpandWidth (false)))
		{
			showPresetManager = !showPresetManager;
		}
		
		GUI.backgroundColor = UnityEngine.Color.white;
		
		if (showPresetManager)
		{
			GUILayout.Space(-125);

			EditorGUILayout.BeginVertical();
            GUILayout.Space(40);
			PresetManager();
			GUILayout.Space(20);
			EditorGUILayout.EndVertical();
        }
        
        EditorGUILayout.EndHorizontal();
			
		if (showProgressShadow || showProgressColormap || showProgressLandCover || showProgressFilters)
		{
			GUILayout.Space(10);
			
			EditorGUILayout.BeginVertical(GUI.skin.box, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));

			GUILayout.Space(15);
			
			// Shadow Remover Progress
			
			if (showProgressShadow)
			{
				EditorGUILayout.BeginHorizontal();
				GUILayout.FlexibleSpace();
				
				Rect rect = GUILayoutUtility.GetLastRect();
				rect.x = 47;
				rect.width = position.width - 100;
				rect.height = 18;

				if(progressShadowStage == 1)
					progressShadowStr = "Removing Shadows  -  Stage 1\t";
				else if(progressShadowStage == 2)
					progressShadowStr = "Removing Shadows  -  Stage 2\t";
				else if(progressShadowStage == 3)
					progressShadowStr = "Removing Shadows  -  Stage 3\t";

				int percentage = Mathf.RoundToInt(progressShadow * 100f);
				EditorGUI.ProgressBar(rect, progressShadow, progressShadowStr + percentage + "%");
				
				GUILayout.FlexibleSpace();
				EditorGUILayout.EndHorizontal();
				
				if (progressShadowIndex != percentage)
				{
					Repaint();
					progressShadowIndex = percentage;
				}
				
				GUILayout.Space(25);
			}
			
			if (Event.current.type == EventType.Repaint && progressShadow == 1f)
			{
				showProgressShadow = false;
				progressShadow = 0f;
			}
			
			// Colormap Maker Progress

			if (showProgressColormap)
			{
				EditorGUILayout.BeginHorizontal();
				GUILayout.FlexibleSpace();
				
				Rect rect = GUILayoutUtility.GetLastRect();
				rect.x = 47;
				rect.width = position.width - 100;
				rect.height = 18;

				if(progressColormapStage == 1)
					progressColormapStr = "Generating Colormap  -  Erosion Filtering\t";
				else if(progressColormapStage == 2)
					progressColormapStr = "Generating Colormap  -  Smoothness Filtering\t";
				else if(progressColormapStage == 3)
					progressColormapStr = "Generating Colormap  -  Sharpness Filtering\t";
				
				int percentage = Mathf.RoundToInt(progressColormap * 100f);
				EditorGUI.ProgressBar(rect, progressColormap, progressColormapStr + percentage + "%");
				
				GUILayout.FlexibleSpace();
				EditorGUILayout.EndHorizontal();
				
				if (progressColormapIndex != percentage)
				{
					Repaint();
					progressColormapIndex = percentage;
				}
				
				GUILayout.Space(25);
			}
			
			if (Event.current.type == EventType.Repaint && progressColormap == 1f)
			{
				showProgressColormap = false;
				progressColormap = 0f;
			}
			
			// LandCover Maker Progress

			if (showProgressLandCover)
			{
				EditorGUILayout.BeginHorizontal();
				GUILayout.FlexibleSpace();
				
				Rect rect = GUILayoutUtility.GetLastRect();
				rect.x = 47;
				rect.width = position.width - 100;
				rect.height = 18;

				if(progressLandCoverStage == 1)
					progressLandCoverStr = "Generating Land-Cover  -  RGB Color Filtering\t";
				else if(progressLandCoverStage == 2)
					progressLandCoverStr = "Generating Land-Cover  -  Alpha Filtering\t";
				else if(progressLandCoverStage == 3)
					progressLandCoverStr = "Generating Land-Cover  -  Smoothness Filtering\t";
				else if(progressLandCoverStage == 4)
					progressLandCoverStr = "Generating Land-Cover  -  Smoothing Alpha Channel\t";
				
				int percentage = Mathf.RoundToInt(progressLandCover * 100f);
				EditorGUI.ProgressBar(rect, progressLandCover, progressLandCoverStr + percentage + "%");
				
				GUILayout.FlexibleSpace();
				EditorGUILayout.EndHorizontal();
				
				if (progressLandCoverIndex != percentage)
				{
					Repaint();
					progressLandCoverIndex = percentage;
				}
				
				GUILayout.Space(25);
			}
			
			if (Event.current.type == EventType.Repaint && progressLandCover == 1f)
			{
				showProgressLandCover = false;
				progressLandCover = 0f;
			}
				
			// Automatic Filters Progress
			
			if (showProgressFilters)
			{
				EditorGUILayout.BeginHorizontal();
				GUILayout.FlexibleSpace();
				
				Rect rect = GUILayoutUtility.GetLastRect();
				rect.x = 47;
				rect.width = position.width - 100;
				rect.height = 18;
				
				if(progressFiltersStage == 1)
					progressFiltersStr = "Generating Filters  -  Analysing Image Pixels\t";
				else if(progressFiltersStage == 2)
					progressFiltersStr = "Generating Filters  -  Detecting Top Colors\t";
				else if(progressFiltersStage == 3)
					progressFiltersStr = "Generating Filters  -  Filtering Top Colors\t";
				
				int percentage = Mathf.RoundToInt(progressFilters * 100f);
				EditorGUI.ProgressBar(rect, progressFilters, progressFiltersStr + percentage + "%");
				
				GUILayout.FlexibleSpace();
				EditorGUILayout.EndHorizontal();
				
				if (progressFiltersIndex != percentage)
				{
					Repaint();
					progressFiltersIndex = percentage;
				}
				
				GUILayout.Space(25);
			}
			
			if (Event.current.type == EventType.Repaint && progressFilters == 1f)
			{
				showProgressFilters = false;
				progressFilters = 0f;
			}

			GUILayout.Space(10);

			EditorGUILayout.EndVertical();
		}
			
		EditorGUILayout.BeginVertical(GUI.skin.box, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
		scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));

		GUI.backgroundColor = UnityEngine.Color.gray;
		EditorGUILayout.HelpBox("\nPROCESSING IMAGE\n", MessageType.None);
		GUI.backgroundColor = UnityEngine.Color.white;

		showProcessingSection = EditorGUILayout.Foldout(showProcessingSection, "");

		if(showProcessingSection)
		{
			GUILayout.Space(30);

			EditorGUI.BeginChangeCheck();

			EditorGUILayout.BeginHorizontal();
			GUILayout.FlexibleSpace();
			EditorGUILayout.HelpBox("SATELLITE IMAGE", MessageType.None);
			satelliteImage = (Texture2D)EditorGUILayout.ObjectField(satelliteImage, typeof(Texture2D), allowSceneObjects) as Texture2D;
			GUILayout.FlexibleSpace();
			EditorGUILayout.EndHorizontal();

			if (EditorGUI.EndChangeCheck())
			{
				if(satelliteImage)
					Initialize();
			}
			
			if (satelliteImage)
			{
				GUILayout.Space(20);
				
				EditorGUILayout.BeginHorizontal();
				GUILayout.FlexibleSpace();
				if (GUILayout.Button("SHOW IMAGE DISPLAY"))
				{
					Initialize();

					int displayWindowAspectRatio = satelliteImageHeight / satelliteImageWidth;
					int displayWindowWidth = 680;
					int displayWindowHeight = (displayWindowWidth * displayWindowAspectRatio) + 70;

                    DisplayImage displayImage = (DisplayImage)EditorWindow.GetWindow(typeof(DisplayImage), false, "Image Display", true);
                    displayImage.position = new Rect(5, 135, displayWindowWidth, displayWindowHeight);
                    displayImage.satelliteImage = satelliteImage;
                    displayImage.imageResolution = imageResolution;
                    displayImage.CenterMap();
				}
				GUILayout.FlexibleSpace();
				EditorGUILayout.EndHorizontal();
			}
				
			GUILayout.Space(100);
		}
		else
			GUILayout.Space(15);

		GUI.backgroundColor = UnityEngine.Color.gray;
		EditorGUILayout.HelpBox("\nSHADOW REMOVER\n", MessageType.None);
		GUI.backgroundColor = UnityEngine.Color.white;

		showShadowSection = EditorGUILayout.Foldout(showShadowSection, "");
		
		if(showShadowSection)
		{
			GUILayout.Space(30);

			EditorGUI.BeginChangeCheck();

			EditorGUILayout.BeginHorizontal();
			GUILayout.FlexibleSpace();
			EditorGUILayout.HelpBox("SHADOW COLOR", MessageType.None);
			shadowColor = EditorGUILayout.ColorField(shadowColor);
			GUILayout.FlexibleSpace();
			EditorGUILayout.EndHorizontal();

			HSBColor shadowHSB = HSBColor.FromColor(shadowColor);

			if (EditorGUI.EndChangeCheck())
				shadowColorBrightness = shadowHSB.b * 100f;

			GUILayout.Space(20);

			EditorGUILayout.BeginHorizontal();
			GUILayout.FlexibleSpace();
			EditorGUILayout.HelpBox("SHADOW BRIGHTNESS", MessageType.None);

			shadowColorBrightness = EditorGUILayout.Slider(shadowColorBrightness, 0f, 100f);

			shadowHSB.b = shadowColorBrightness / 100f;
			shadowColor = HSBColor.ToColor(shadowHSB);

			GUILayout.FlexibleSpace();
			EditorGUILayout.EndHorizontal();

			GUILayout.Space(10);
			
			EditorGUILayout.BeginHorizontal();
			GUILayout.FlexibleSpace();
			EditorGUILayout.HelpBox("HEALING BLOCK SIZE", MessageType.None);

			if(satelliteImage)
				healingBlockSize = EditorGUILayout.IntSlider(healingBlockSize, 1, imageResolution / 2);
			else
				healingBlockSize = EditorGUILayout.IntSlider(healingBlockSize, 1, 2048);

			GUILayout.FlexibleSpace();
			EditorGUILayout.EndHorizontal();
	    	
			GUILayout.Space(100);
		}
		else
			GUILayout.Space(15);

		GUI.backgroundColor = UnityEngine.Color.gray;
		EditorGUILayout.HelpBox("\nCOLORMAP GENERATOR\n", MessageType.None);
		GUI.backgroundColor = UnityEngine.Color.white;

		showColormapSection = EditorGUILayout.Foldout(showColormapSection, "");
		
		if(showColormapSection)
		{
			GUILayout.Space(30);
			
			EditorGUILayout.BeginHorizontal();
			GUILayout.FlexibleSpace();
			EditorGUILayout.HelpBox("EROSION RADIUS", MessageType.None);
			erodeSize = EditorGUILayout.IntSlider(erodeSize, 3, 99);
			GUILayout.FlexibleSpace();
			EditorGUILayout.EndHorizontal();

			// Must be an Odd value between 3 & 99
			if(erodeSize % 2 == 0)
				erodeSize += 1;

			GUILayout.Space(10);
			
			EditorGUILayout.BeginHorizontal();
			GUILayout.FlexibleSpace();
			EditorGUILayout.HelpBox("SMOOTHNESS", MessageType.None);
			colormapKernelSize = EditorGUILayout.IntSlider(colormapKernelSize, 3, 25);
			GUILayout.FlexibleSpace();
			EditorGUILayout.EndHorizontal();
			
			// Must be an Odd value between 3 & 25
			if(colormapKernelSize % 2 == 0)
				colormapKernelSize += 1;

			GUILayout.Space(10);
			
			EditorGUILayout.BeginHorizontal();
			GUILayout.FlexibleSpace();
			EditorGUILayout.HelpBox("SHARPNESS", MessageType.None);
			colormapSharpness = EditorGUILayout.IntSlider(colormapSharpness, 0, 40);
			GUILayout.FlexibleSpace();
			EditorGUILayout.EndHorizontal();
			
//			GUILayout.Space(60);
//			
//			EditorGUILayout.BeginHorizontal();
//			GUILayout.FlexibleSpace();
//			if (GUILayout.Button("\nBLOB DETECTOR\n"))
//			{
//				if (satelliteImage)
//					BlobDetector();
//				else {
//					EditorUtility.DisplayDialog("UNAVAILABLE IMAGE", unavailableImageStr, "Ok");
//					return;
//				}
//			}
//			GUILayout.FlexibleSpace();
//			EditorGUILayout.EndHorizontal();

			GUILayout.Space(100);
		}
		else
			GUILayout.Space(15);

		GUI.backgroundColor = UnityEngine.Color.gray;
		EditorGUILayout.HelpBox("\nLANDCOVER GENERATOR\n", MessageType.None);
		GUI.backgroundColor = UnityEngine.Color.white;

		showLandcoverSection = EditorGUILayout.Foldout(showLandcoverSection, "");
		
		if(showLandcoverSection)
		{
			GUILayout.Space(30);
			
			EditorGUILayout.BeginHorizontal();
			GUILayout.FlexibleSpace();
			EditorGUILayout.HelpBox("FORMAT", MessageType.None);
			splatFormatIndex = GUILayout.SelectionGrid(splatFormatIndex, splatFormat, 2);
			GUILayout.FlexibleSpace();
			EditorGUILayout.EndHorizontal();

			GUILayout.Space(20);
			
			EditorGUILayout.BeginHorizontal();
			GUILayout.FlexibleSpace();
			EditorGUILayout.HelpBox("MAP SMOOTHNESS", MessageType.None);
			filterSmoothingKernelSize = EditorGUILayout.IntSlider(filterSmoothingKernelSize, 3, 25);
			GUILayout.FlexibleSpace();
			EditorGUILayout.EndHorizontal();
			
			if(filterSmoothingKernelSize % 2 == 0)
				filterSmoothingKernelSize += 1;

			GUILayout.Space(35);
			
			EditorGUILayout.BeginHorizontal();
			GUILayout.FlexibleSpace();
			EditorGUILayout.HelpBox("AUTOMATIC MODE", MessageType.None);
			GUILayout.FlexibleSpace();
			EditorGUILayout.EndHorizontal();

			GUILayout.Space(10);

			EditorGUILayout.BeginHorizontal();
			GUILayout.FlexibleSpace();
			landCoverModeIndex = GUILayout.SelectionGrid(landCoverModeIndex, landCoverMode, 2);
			GUILayout.FlexibleSpace();
			EditorGUILayout.EndHorizontal();

			GUILayout.Space(20);

			if(landCoverModeIndex == 0)
			{
				EditorGUILayout.BeginHorizontal();
				GUILayout.FlexibleSpace();
				EditorGUILayout.HelpBox("FILTERS COUNT", MessageType.None);
				topColorsCount = EditorGUILayout.IntSlider(topColorsCount, 4, 16);
				GUILayout.FlexibleSpace();
				EditorGUILayout.EndHorizontal();

				filtersNo = topColorsCount;

				GUILayout.Space(10);

				EditorGUILayout.BeginHorizontal();
				GUILayout.FlexibleSpace();
				EditorGUILayout.HelpBox("COLOR DAMPING", MessageType.None);
				tolerance = EditorGUILayout.IntSlider(tolerance, 0, 64);
				GUILayout.FlexibleSpace();
				EditorGUILayout.EndHorizontal();

				GUILayout.Space(10);
				
				EditorGUILayout.BeginHorizontal();
				GUILayout.FlexibleSpace();
				EditorGUILayout.HelpBox("FILTER RADIUS", MessageType.None);
				filterRadiusAuto = EditorGUILayout.IntSlider(filterRadiusAuto, 0, 450);
				GUILayout.FlexibleSpace();
				EditorGUILayout.EndHorizontal();

				GUILayout.Space(20);
				
				EditorGUILayout.BeginHorizontal();
				GUILayout.FlexibleSpace();
				EditorGUILayout.HelpBox("OUTPUT ORDERING", MessageType.None);
				outputOrderingIndex = GUILayout.SelectionGrid(outputOrderingIndex, outputOrdering, 2);
				GUILayout.FlexibleSpace();
				EditorGUILayout.EndHorizontal();

				GUILayout.Space(40);
				
				EditorGUILayout.BeginHorizontal();
				GUILayout.FlexibleSpace();
				if (GUILayout.Button("\nGENERATE FILTERS\n"))
				{
					if (satelliteImage)
					{
						if(filtersGenerationOperation)
						{
							EditorUtility.DisplayDialog("ENGINE BUSY", engineBusySelfStr, "Ok");
							return;
						}
                        else
                        {
							GenerateFilters();
						}

					}
					else {
						EditorUtility.DisplayDialog("UNAVAILABLE IMAGE", unavailableImageStr, "Ok");
						return;
					}
				}
				GUILayout.FlexibleSpace();
				EditorGUILayout.EndHorizontal();
			}
				
			GUILayout.Space(80);
				
			GUI.backgroundColor = UnityEngine.Color.gray;
			EditorGUILayout.BeginHorizontal();
			GUILayout.FlexibleSpace();
			EditorGUILayout.HelpBox("\nLAND COVER FILTERS\n", MessageType.None);
			GUILayout.FlexibleSpace();
			EditorGUILayout.EndHorizontal();
			GUI.backgroundColor = UnityEngine.Color.white;

			GUILayout.Space(40);

			EditorGUILayout.BeginHorizontal();
			GUILayout.FlexibleSpace();
			EditorGUILayout.HelpBox("SIZE", MessageType.None);
			filtersNo = EditorGUILayout.IntSlider(filtersNo, 1, 30);
			GUILayout.FlexibleSpace();
			EditorGUILayout.EndHorizontal();

			GUILayout.Space(40);

			for(int i = 0; i < filtersNo; i++)
			{
				EditorGUILayout.BeginHorizontal();
				GUILayout.FlexibleSpace();

				GUI.color = UnityEngine.Color.green;
				EditorGUILayout.HelpBox((i + 1).ToString(), MessageType.None);
				GUI.color = UnityEngine.Color.white;

				EditorGUILayout.HelpBox("FILTER NAME", MessageType.None);

				filterNames.Add("");
				filterNames[i] = EditorGUILayout.TextField(filterNames[i]);

				GUILayout.FlexibleSpace();
				EditorGUILayout.EndHorizontal();

				GUILayout.Space(20);

				EditorGUILayout.BeginHorizontal();
				GUILayout.FlexibleSpace();

				EditorGUILayout.HelpBox("FILTER IMAGE COLOR", MessageType.None);
				filterColors.Add(UnityEngine.Color.black);
				filterColors[i] = EditorGUILayout.ColorField(filterColors[i]);

				GUILayout.FlexibleSpace();
				EditorGUILayout.EndHorizontal();

				GUILayout.Space(15);
				
				EditorGUILayout.BeginHorizontal();
				GUILayout.FlexibleSpace();

				EditorGUILayout.HelpBox("RADIUS", MessageType.None);
				filterRadius.Add(100);
				filterRadius[i] = EditorGUILayout.IntSlider(filterRadius[i], 0, 450);
				
				GUILayout.FlexibleSpace();
				EditorGUILayout.EndHorizontal();

				GUILayout.Space(20);
				
				EditorGUILayout.BeginHorizontal();
				GUILayout.FlexibleSpace();
				EditorGUILayout.HelpBox("OUTPUT MAP COLOR", MessageType.None);

				mapColors.Add(MapColor._Red);
				mapColors[i] = (MapColor)EditorGUILayout.EnumPopup(mapColors[i]);
				mapColorIndex = (int)mapColors[i];

				GUILayout.FlexibleSpace();
				EditorGUILayout.EndHorizontal();

				GUILayout.Space(10);
				
				EditorGUILayout.BeginHorizontal();
				GUILayout.FlexibleSpace();
				
				EditorGUILayout.HelpBox("OUTPUT", MessageType.None);

				outputFilterColors.Add(UnityEngine.Color.black);
				UnityEngine.Color colRGBA = UnityEngine.Color.black;

				if(mapColorIndex == 1)
				{
					colRGBA = UnityEngine.Color.red;
					outputFilterColors[i] = colRGBA;
				}
				else if(mapColorIndex == 2)	
	            {
					colRGBA = UnityEngine.Color.green;
					outputFilterColors[i] = colRGBA;
	            }
				else if(mapColorIndex == 3)
	            {
					colRGBA = UnityEngine.Color.blue;
					outputFilterColors[i] = colRGBA;
	            }
				else if(mapColorIndex == 4)
	            {
					colRGBA = UnityEngine.Color.black;
					outputFilterColors[i] = colRGBA;
	            }

				outputFilterColors[i] = EditorGUILayout.ColorField(colRGBA);
				
				GUILayout.FlexibleSpace();
				EditorGUILayout.EndHorizontal();

				GUILayout.Space(20);
			}
		}
		else
			GUILayout.Space(15);

		GUILayout.Space(80);

    	EditorGUILayout.EndScrollView();
		EditorGUILayout.EndVertical();
			
		GUILayout.Space(15);

		EditorGUILayout.BeginHorizontal();
		GUILayout.FlexibleSpace();

		if (GUILayout.Button("\nELIMINATE SHADOWS\n"))
		{
			if (satelliteImage)
			{
				if(shadowRemoverOperation)
				{
					EditorUtility.DisplayDialog("ENGINE BUSY", engineBusySelfStr, "Ok");
					return;
				}
				else
				{
					ShadowRemover();
					//ShadowlessOperation1();
				}
			}
			else {
				EditorUtility.DisplayDialog("UNAVAILABLE IMAGE", unavailableImageStr, "Ok");
				return;
			}
		}

		GUILayout.Space(20);

		if (GUILayout.Button("\nGENERATE COLORMAP\n"))
		{
			if (satelliteImage)
			{
				if(colormapOperation)
				{
					EditorUtility.DisplayDialog("ENGINE BUSY", engineBusySelfStr, "Ok");
					return;
				}
				else
				{
					GenerateColormap();
                }
            }
            else {
				EditorUtility.DisplayDialog("UNAVAILABLE IMAGE", unavailableImageStr, "Ok");
				return;
			}
		}
        
        GUILayout.Space(20);
        
		if (GUILayout.Button("\nGENERATE SPLATMAP\n"))
		{
			if (satelliteImage)
			{
				if(landCoverOperation)
				{
					EditorUtility.DisplayDialog("ENGINE BUSY", engineBusySelfStr, "Ok");
					return;
				}
				else if(shadowRemoverOperation || colormapOperation || filtersGenerationOperation)
				{
					EditorUtility.DisplayDialog("ENGINE BUSY", engineBusyStr, "Ok");
					return;
				}
                else
                {
					GenerateLandCover();
                }
            }
            else {
				EditorUtility.DisplayDialog("UNAVAILABLE IMAGE", unavailableImageStr, "Ok");
				return;
			}
		}
        
        GUILayout.FlexibleSpace();
        EditorGUILayout.EndHorizontal();

        GUILayout.Space(15);
    }
    
    private void Initialize ()
	{
		string assetPath = AssetDatabase.GetAssetPath(satelliteImage);
		sourceImagePath = dataPath + assetPath.Replace("Assets", "");
		
		sourceImage = new Bitmap(sourceImagePath);

		imageResolution = sourceImage.Width;
		satelliteImageWidth = sourceImage.Width;
		satelliteImageHeight = sourceImage.Height;

		if (sourceImage.RawFormat.Equals(ImageFormat.Jpeg))
		{
			imageFormat = ImageFormat.Jpeg;
			imageFormatStr = ".jpg";
		}
		else if (sourceImage.RawFormat.Equals(ImageFormat.Png))
		{
			imageFormat = ImageFormat.Png;
			imageFormatStr = ".png";
		}
		else if (sourceImage.RawFormat.Equals(ImageFormat.Bmp))
		{
			imageFormat = ImageFormat.Bmp;
			imageFormatStr = ".bmp";
		}
		else if (sourceImage.RawFormat.Equals(ImageFormat.Tiff))
		{
			imageFormat = ImageFormat.Tiff;
			imageFormatStr = ".tif";
		}
		else if (sourceImage.RawFormat.Equals(ImageFormat.Gif))
		{
			imageFormat = ImageFormat.Gif;
			imageFormatStr = ".gif";
		}
	}

	private void GenerateColormap ()
	{
		if(!shadowRemoverOperation && !landCoverOperation && !filtersGenerationOperation)
			Initialize();

		RunAsync(()=>
        {
			colormapOperation = true;
			showProgressColormap = true;
			Rectangle rect = new Rectangle( 0, 0, sourceImage.Width, sourceImage.Height);
			Bitmap outputImage = (Bitmap)sourceImage.Clone(rect, sourceImage.PixelFormat);
			progressColormapStage = 1;


			// Erosion
			short[,] se = new short[erodeSize, erodeSize];
			short sev = 1;
			
			for (int i = 0; i < erodeSize; i++)
				for (int j = 0; j < erodeSize; j++)
					se[i, j] = sev;

			Opening erosion = new Opening(se);
			//outputImage = erosion.Apply(sourceImage);
			erosion.ApplyInPlace(outputImage);


			// Bilateral Smoothing
			progressColormap = Mathf.InverseLerp(0f, 3f, 2f);
			progressColormapStage = 2;
			SmoothenImage(outputImage, colormapKernelSize);


			// Sharpen
			progressColormap = Mathf.InverseLerp(0f, 3f, 3f);
			progressColormapStage = 3;

//			int[,] kernel =
//			{
//				{ -2, -1,  0 },
//				{ -1,  1,  1 },
//				{  0,  1,  2 }
//			};
//
//			int[,] kernel2 =
//			{
//				{ 0, -1,  0 },
//				{ -1,  5,  -1 },
//	            {  0,  -1,  0 }
//	        };

	        Sharpen sharpen = new Sharpen();
			//sharpen.Kernel = kernel2;
			sharpen.Threshold = colormapSharpness;
			//sharpen.Divisor = 2;
			//sharpen.DynamicDivisorForEdges = true;
			sharpen.ApplyInPlace(outputImage);



			// Gaussian Sharpen
			//GaussianSharpen gaussianSharpen = new GaussianSharpen(40, 11);
			//gaussianSharpen.ApplyInPlace(outputImage);

			QueueOnMainThread(()=>
			{
				ExportGeneratedMapColor(outputImage);
                ImportImage(colormapOperation);
				colormapOperation = false;
				showProgressColormap = false;
				progressColormap = 0;
			});
		});
  	}

	private void ShadowRemover ()
	{

		if(!colormapOperation && !landCoverOperation && !filtersGenerationOperation)
			Initialize();

		RunAsync(()=>
		{
			shadowRemoverOperation = true;
			showProgressShadow = true;
			Rectangle rect = new Rectangle( 0, 0, sourceImage.Width, sourceImage.Height);
			Bitmap outputImage = (Bitmap)sourceImage.Clone(rect, sourceImage.PixelFormat);

			int R = (int)Mathf.Clamp((shadowColor.r * 255f), 0f, 255f);
			int G = (int)Mathf.Clamp((shadowColor.g * 255f), 0f, 255f);
			int B = (int)Mathf.Clamp((shadowColor.b * 255f), 0f, 255f);
			System.Drawing.Color shColor = System.Drawing.Color.FromArgb(R, G, B);
			shadowBrightness = shColor.GetBrightness(); //From 0 (black) to 1 (white)
			
			int imageWidth = sourceImage.Width;
			int imageHeight = sourceImage.Height;
			//outputImage = new Bitmap(imageWidth, imageHeight, PixelFormat.Format24bppRgb);
			//System.Drawing.Color lastBrightPixel = new System.Drawing.Color();
			int healingSizeVariated = (int)((float)healingBlockSize * 1.5f);

			for (int y = 0; y < imageHeight; y++)
			{
				for (int x = 0; x < imageWidth; x++)
				{
					color = sourceImage.GetPixel(x, y);
					currentPixelbrightness = color.GetBrightness();
					
					try
					{
						if (currentPixelbrightness < shadowBrightness)
						{
							// Type 1
							//int addMount = 60;
							//System.Drawing.Color finalColor = System.Drawing.Color.FromArgb(Red + addMount, Grn + addMount, Blu + addMount);
							//outputImage.SetPixel(x, y, finalColor);
							
							
							// Type 2
							//int xPixel = (int)lastLightPixelIndex.x - resetter;
							//int yPixel = (int)lastLightPixelIndex.y - resetter;
							//xPixel = (int)llp[resetter].x;
							//yPixel = (int)llp[resetter].y;
							//outputImage.SetPixel(x, y, sourceImage.GetPixel(xPixel, yPixel));
							//outputImage.SetPixel(x, y, lightColors[resetter]);
							
							
							// Type 3
							//System.Random random = new System.Random();
							//int healingBlockSize = random.Next(-100, 100);
							//int healingBlockSize = 500;
							//outputImage.SetPixel(x, y, sourceImage.GetPixel(x + healingBlockSize, y  + healingBlockSize));




							// LEFT ==> RIGHT
							pixelColor = sourceImage.GetPixel(x + healingBlockSize, y);
							pixelColorBrightness = pixelColor.GetBrightness();
							
							if(pixelColorBrightness > shadowBrightness)
								outputImage.SetPixel(x, y, pixelColor);

							// RIGHT ==> LEFT
							pixelColor = sourceImage.GetPixel(x - healingBlockSize, y);
							pixelColorBrightness = pixelColor.GetBrightness();
							
							if(pixelColorBrightness > shadowBrightness)
								outputImage.SetPixel(x, y, pixelColor);

							// TOP ==> BOTTOM
							pixelColor = sourceImage.GetPixel(x, y + healingBlockSize);
							pixelColorBrightness = pixelColor.GetBrightness();
							
							if(pixelColorBrightness > shadowBrightness)
								outputImage.SetPixel(x, y, pixelColor);
							
							// BOTTOM ==> TOP
							pixelColor = sourceImage.GetPixel(x, y - healingBlockSize);
							pixelColorBrightness = pixelColor.GetBrightness();
							
							if(pixelColorBrightness > shadowBrightness)
								outputImage.SetPixel(x, y, pixelColor);

//							float processedPixelBrightness = outputImage.GetPixel(x, y).GetBrightness();
//
//							if(processedPixelBrightness < shadowBrightness)
//								outputImage.SetPixel(x, y, lastBrightPixel);
						}
						else
						{
							if (currentPixelbrightness < shadowBrightness * 1.25f)
							{
								pixelColor = sourceImage.GetPixel(x - healingBlockSize, y);
								pixelColorBrightness = pixelColor.GetBrightness();
								
								if(pixelColorBrightness > shadowBrightness)
									outputImage.SetPixel(x, y, pixelColor);
							}
							else
							{
								outputImage.SetPixel(x, y, color);
								//lastBrightPixel = color;
							}
						}
					}
					catch {}
						
					progressShadow = Mathf.InverseLerp(0f, (float)(imageHeight * 3), (float)y);
					progressShadowStage = 1;
				}
			}

			for (int y = 0; y < imageHeight; y++)
			{
				for (int x = 0; x < imageWidth; x++)
				{
					color = outputImage.GetPixel(x, y);
					currentPixelbrightness = color.GetBrightness();
					
					try
					{
						if (currentPixelbrightness < shadowBrightness)
						{
							// TOP-LEFT ==> BOTTOM-RIGHT
							pixelColor = outputImage.GetPixel(x + healingSizeVariated, y + healingSizeVariated);
							pixelColorBrightness = pixelColor.GetBrightness();
							
							if(pixelColorBrightness > shadowBrightness)
								outputImage.SetPixel(x, y, pixelColor);

							// TOP-RIGHT ==> BOTTOM-LEFT
							pixelColor = outputImage.GetPixel(x - healingSizeVariated, y + healingSizeVariated);
							pixelColorBrightness = pixelColor.GetBrightness();
							
							if(pixelColorBrightness > shadowBrightness)
								outputImage.SetPixel(x, y, pixelColor);

							// BOTTOM-LEFT ==> TOP-RIGHT
							pixelColor = outputImage.GetPixel(x + healingSizeVariated, y - healingSizeVariated);
							pixelColorBrightness = pixelColor.GetBrightness();
							
							if(pixelColorBrightness > shadowBrightness)
								outputImage.SetPixel(x, y, pixelColor);

							// BOTTOM-RIGHT ==> TOP-LEFT
							pixelColor = outputImage.GetPixel(x - healingSizeVariated, y - healingSizeVariated);
							pixelColorBrightness = pixelColor.GetBrightness();
							
							if(pixelColorBrightness > shadowBrightness)
								outputImage.SetPixel(x, y, pixelColor);

//							float processedPixelBrightness = outputImage.GetPixel(x, y).GetBrightness();
//							
//							if(processedPixelBrightness < shadowBrightness)
//								outputImage.SetPixel(x, y, lastBrightPixel);
						}
						else
						{
							if (currentPixelbrightness < shadowBrightness * 1.25f)
							{
								pixelColor = sourceImage.GetPixel(x - healingSizeVariated, y - healingSizeVariated);
								pixelColorBrightness = pixelColor.GetBrightness();
								
								if(pixelColorBrightness > shadowBrightness)
									outputImage.SetPixel(x, y, pixelColor);
							}
							else
							{
								outputImage.SetPixel(x, y, color);
								//lastBrightPixel = color;
							}
						}
					}
					catch {}

					progressShadow = Mathf.InverseLerp(0f, (float)(imageHeight * 3), (float)(y + imageHeight));
					progressShadowStage = 2;
				}
			}

			for (int y = 0; y < imageHeight; y++)
			{
				for (int x = 0; x < imageWidth; x++)
				{
					color = outputImage.GetPixel(x, y);
					currentPixelbrightness = color.GetBrightness();
					
					try
					{
						if (currentPixelbrightness < shadowBrightness)
						{
							// TOP-LEFT CORNER
							if(x <= healingSizeVariated && y <= healingSizeVariated)
							{
								// LEFT ==> RIGHT
								pixelColor = outputImage.GetPixel(x + healingSizeVariated, y);
								pixelColorBrightness = pixelColor.GetBrightness();
								
								if(pixelColorBrightness > shadowBrightness)
									outputImage.SetPixel(x, y, pixelColor);
								else
								{
									// TOP-LEFT ==> BOTTOM-RIGHT
									pixelColor = outputImage.GetPixel(x + healingSizeVariated, y + healingSizeVariated);
									pixelColorBrightness = pixelColor.GetBrightness();
									
									if(pixelColorBrightness > shadowBrightness)
										outputImage.SetPixel(x, y, pixelColor);
									else
									{
										// TOP ==> BOTTOM
										pixelColor = outputImage.GetPixel(x, y + healingSizeVariated);
										pixelColorBrightness = pixelColor.GetBrightness();
										
										if(pixelColorBrightness > shadowBrightness)
											outputImage.SetPixel(x, y, pixelColor);
									}
								}
							}
								
							// TOP-RIGHT CORNER
							else if(x >= imageWidth - healingSizeVariated && y <= healingSizeVariated)
							{
								// RIGHT ==> LEFT
								pixelColor = outputImage.GetPixel(x - healingSizeVariated, y);
								pixelColorBrightness = pixelColor.GetBrightness();
								
								if(pixelColorBrightness > shadowBrightness)
									outputImage.SetPixel(x, y, pixelColor);
								{
									// TOP-RIGHT ==> BOTTOM-LEFT
									pixelColor = outputImage.GetPixel(x - healingSizeVariated, y + healingSizeVariated);
									pixelColorBrightness = pixelColor.GetBrightness();
									
									if(pixelColorBrightness > shadowBrightness)
										outputImage.SetPixel(x, y, pixelColor);
									else
									{
										// TOP ==> BOTTOM
										pixelColor = outputImage.GetPixel(x, y + healingSizeVariated);
										pixelColorBrightness = pixelColor.GetBrightness();
										
										if(pixelColorBrightness > shadowBrightness)
											outputImage.SetPixel(x, y, pixelColor);
									}
								}
							}
							
							// BOTTOM-LEFT CORNER
							else if(x <= healingSizeVariated && y >= imageHeight - healingSizeVariated)
							{
								// LEFT ==> RIGHT
								pixelColor = outputImage.GetPixel(x + healingSizeVariated, y);
								pixelColorBrightness = pixelColor.GetBrightness();
								
								if(pixelColorBrightness > shadowBrightness)
									outputImage.SetPixel(x, y, pixelColor);
								else
								{
									// BOTTOM-LEFT ==> TOP-RIGHT
									pixelColor = outputImage.GetPixel(x + healingSizeVariated, y - healingSizeVariated);
									pixelColorBrightness = pixelColor.GetBrightness();
									
									if(pixelColorBrightness > shadowBrightness)
										outputImage.SetPixel(x, y, pixelColor);
									else
									{
										// BOTTOM ==> TOP
										pixelColor = outputImage.GetPixel(x, y - healingSizeVariated);
										pixelColorBrightness = pixelColor.GetBrightness();
										
										if(pixelColorBrightness > shadowBrightness)
											outputImage.SetPixel(x, y, pixelColor);
									}
								}
							}
							
							// BOTTOM-RIGHT CORNER
							else if(x >= imageWidth - healingSizeVariated && y >= imageHeight - healingSizeVariated)
							{
								// RIGHT ==> LEFT
								pixelColor = outputImage.GetPixel(x - healingSizeVariated, y);
								pixelColorBrightness = pixelColor.GetBrightness();
								
								if(pixelColorBrightness > shadowBrightness)
									outputImage.SetPixel(x, y, pixelColor);
								{
									// BOTTOM-RIGHT ==> TOP-LEFT
									pixelColor = outputImage.GetPixel(x - healingSizeVariated, y - healingSizeVariated);
									pixelColorBrightness = pixelColor.GetBrightness();
									
									if(pixelColorBrightness > shadowBrightness)
										outputImage.SetPixel(x, y, pixelColor);
									else
									{
										// BOTTOM ==> TOP
										pixelColor = outputImage.GetPixel(x, y - healingSizeVariated);
										pixelColorBrightness = pixelColor.GetBrightness();
										
										if(pixelColorBrightness > shadowBrightness)
											outputImage.SetPixel(x, y, pixelColor);
									}
								}
							}
							else
							{
								// LEFT COLUMN
								if(x <= healingSizeVariated)
								{
									// LEFT ==> RIGHT
									pixelColor = outputImage.GetPixel(x + healingSizeVariated, y);
									pixelColorBrightness = pixelColor.GetBrightness();
									
									if(pixelColorBrightness > shadowBrightness)
										outputImage.SetPixel(x, y, pixelColor);
									else
									{
										// TOP-LEFT ==> BOTTOM-RIGHT
										pixelColor = outputImage.GetPixel(x + healingSizeVariated, y + healingSizeVariated);
										pixelColorBrightness = pixelColor.GetBrightness();
										
										if(pixelColorBrightness > shadowBrightness)
											outputImage.SetPixel(x, y, pixelColor);
										else
										{
											// BOTTOM-LEFT ==> TOP-RIGHT
											pixelColor = outputImage.GetPixel(x + healingSizeVariated, y - healingSizeVariated);
											pixelColorBrightness = pixelColor.GetBrightness();
											
											if(pixelColorBrightness > shadowBrightness)
												outputImage.SetPixel(x, y, pixelColor);
										}
									}
								}
								
								// RIGHT COLUMN
								if(x >= imageWidth - healingSizeVariated)
								{
									// RIGHT ==> LEFT
									pixelColor = outputImage.GetPixel(x - healingSizeVariated, y);
									pixelColorBrightness = pixelColor.GetBrightness();
									
									if(pixelColorBrightness > shadowBrightness)
										outputImage.SetPixel(x, y, pixelColor);
									{
										// TOP-RIGHT ==> BOTTOM-LEFT
										pixelColor = outputImage.GetPixel(x - healingSizeVariated, y + healingSizeVariated);
										pixelColorBrightness = pixelColor.GetBrightness();
										
										if(pixelColorBrightness > shadowBrightness)
											outputImage.SetPixel(x, y, pixelColor);
										else
										{
											// BOTTOM-RIGHT ==> TOP-LEFT
											pixelColor = outputImage.GetPixel(x - healingSizeVariated, y - healingSizeVariated);
											pixelColorBrightness = pixelColor.GetBrightness();
											
											if(pixelColorBrightness > shadowBrightness)
												outputImage.SetPixel(x, y, pixelColor);
										}
									}
								}
								
								// TOP ROW
								if(y <= healingSizeVariated)
								{
									// TOP ==> BOTTOM
									pixelColor = outputImage.GetPixel(x, y + healingSizeVariated);
									pixelColorBrightness = pixelColor.GetBrightness();
									
									if(pixelColorBrightness > shadowBrightness)
										outputImage.SetPixel(x, y, pixelColor);
									else
									{
										// TOP-LEFT ==> BOTTOM-RIGHT
										pixelColor = outputImage.GetPixel(x + healingSizeVariated, y + healingSizeVariated);
										pixelColorBrightness = pixelColor.GetBrightness();
										
										if(pixelColorBrightness > shadowBrightness)
											outputImage.SetPixel(x, y, pixelColor);
										else
										{
											// TOP-RIGHT ==> BOTTOM-LEFT
											pixelColor = outputImage.GetPixel(x - healingSizeVariated, y + healingSizeVariated);
											pixelColorBrightness = pixelColor.GetBrightness();
											
											if(pixelColorBrightness > shadowBrightness)
												outputImage.SetPixel(x, y, pixelColor);
										}
									}
								}
								
								// BOTTOM ROW
								if(y >= imageHeight - healingSizeVariated)
								{
									// BOTTOM ==> TOP
									pixelColor = outputImage.GetPixel(x, y - healingSizeVariated);
									pixelColorBrightness = pixelColor.GetBrightness();
									
									if(pixelColorBrightness > shadowBrightness)
										outputImage.SetPixel(x, y, pixelColor);
									else
									{
										// BOTTOM-LEFT ==> TOP-RIGHT
										pixelColor = outputImage.GetPixel(x + healingSizeVariated, y - healingSizeVariated);
										pixelColorBrightness = pixelColor.GetBrightness();
										
										if(pixelColorBrightness > shadowBrightness)
											outputImage.SetPixel(x, y, pixelColor);
										else
										{
											// BOTTOM-RIGHT ==> TOP-LEFT
											pixelColor = outputImage.GetPixel(x - healingSizeVariated, y - healingSizeVariated);
											pixelColorBrightness = pixelColor.GetBrightness();
											
											if(pixelColorBrightness > shadowBrightness)
												outputImage.SetPixel(x, y, pixelColor);
										}
									}
								}


//								float processedPixelBrightness = outputImage.GetPixel(x, y).GetBrightness();
//								
//								if(processedPixelBrightness < shadowBrightness)
//									outputImage.SetPixel(x, y, lastBrightPixel);
							}
						}
						else
						{
							if (currentPixelbrightness < shadowBrightness * 1.25f)
							{
								pixelColor = sourceImage.GetPixel(x - healingSizeVariated, y);
								pixelColorBrightness = pixelColor.GetBrightness();
								
								if(pixelColorBrightness > shadowBrightness)
									outputImage.SetPixel(x, y, pixelColor);
							}
							else
							{
								outputImage.SetPixel(x, y, color);
								//lastBrightPixel = color;
							}
						}
					}
					catch {}

					progressShadow = Mathf.InverseLerp(0f, (float)(imageHeight * 3), (float)(y + (imageHeight * 2)));
					progressShadowStage = 3;
				}
			}
			
			QueueOnMainThread(()=>
			{
				ExportGeneratedMapShadow(outputImage);
				ImportImage(shadowRemoverOperation);
				shadowRemoverOperation = false;
				showProgressShadow = false;
                progressShadow = 0;
			});
		});
	}

	private void FinalizeSplatMap (Bitmap outputImage)
	{
		RunAsync(()=>
		{
			Rectangle rect = new Rectangle(0, 0, outputImage.Width, outputImage.Height);
			BitmapData bmpData = outputImage.LockBits(rect, ImageLockMode.ReadWrite, outputImage.PixelFormat);
			int stride = bmpData.Stride;
			int indexR, indexG, indexB, indexA;
			byte R, G, B, A;

			unsafe
			{
				byte* ptr = (byte*)bmpData.Scan0;
				
				// x = row & y = column
				for (int y = 0; y < bmpData.Height; y++)
				{
					for (int x = 0; x < bmpData.Width; x++)
					{
						indexA = (x * 4) + y * stride + 3;
						indexR = (x * 4) + y * stride + 2;
						indexG = (x * 4) + y * stride + 1;
						indexB = (x * 4) + y * stride;

						A = ptr[indexA];
						R = ptr[indexR];
						G = ptr[indexG];
						B = ptr[indexB];

						color = System.Drawing.Color.FromArgb(A, R, G, B);

						if(color.A == 255)
						{
							try
							{
								//if(color.R <= 10 && color.G <= 10 && color.B <= 10)
									//ptr[indexA] = (byte)(255 - ptr[indexR]);

								ptr[indexR] = 0;
								ptr[indexG] = 0;
								ptr[indexB] = 0;
							}
							catch{}
						}
						else
						{
							ptr[indexR] = ptr[indexR];
							ptr[indexG] = ptr[indexG];
							ptr[indexB] = ptr[indexB];
						}

						progressLandCover = Mathf.InverseLerp(0f, (float)bmpData.Height, (float)y);
						progressLandCoverStage = 2;
					}
				}
			}
			
			outputImage.UnlockBits(bmpData);

			progressLandCover = Mathf.InverseLerp(0f, 1f, 0.5f);
			progressLandCoverStage = 3;
			//SmoothenImage(outputImage, filterSmoothingKernelSize);
			progressLandCover = Mathf.InverseLerp(0f, 1f, 1f);

			// Last Step To Smoothen Alpha Channel
			//SmoothenAlpha(outputImage);

			QueueOnMainThread(()=>
			{
				ExportGeneratedMapLand(outputImage);
				ImportImage(landCoverOperation);
				landCoverOperation = false;
				showProgressLandCover = false;
				progressLandCover = 0;
			});
		});
	}

	private void SmoothenAlpha (Bitmap outputImage)
	{
		Rectangle rect = new Rectangle(0, 0, outputImage.Width, outputImage.Height);
		BitmapData bmpData = outputImage.LockBits(rect, ImageLockMode.ReadWrite, outputImage.PixelFormat);
		int stride = bmpData.Stride;
		int indexR, indexG, indexB, indexA;
		byte R, G, B, A;

		unsafe
		{
			byte* ptr = (byte*)bmpData.Scan0;
			
			// x = row & y = column
			for (int y = 0; y < bmpData.Height; y++)
			{
				for (int x = 0; x < bmpData.Width; x++)
				{
					indexA = (x * 4) + y * stride + 3;
					indexR = (x * 4) + y * stride + 2;
					indexG = (x * 4) + y * stride + 1;
					indexB = (x * 4) + y * stride;
					
					A = ptr[indexA];
					R = ptr[indexR];
					G = ptr[indexG];
					B = ptr[indexB];
					
					color = System.Drawing.Color.FromArgb(A, R, G, B);
					
					if(color.R <= 16 && color.G <= 16 && color.B <= 16)
					{
						try
						{
							ptr[indexA] = (byte)(255 - ptr[indexR]);
						}
						catch{}
					}
					else
					{
						ptr[indexA] = 0;
					}
					
					progressLandCover = Mathf.InverseLerp(0f, (float)bmpData.Height, (float)y);
					progressLandCoverStage = 4;
				}
			}
		}
		outputImage.UnlockBits(bmpData);
	}

	private void SmoothenImage (Bitmap outputImage, int kernelSize)
	{
		// Bilateral Smoothing
		BilateralSmoothing bilateralSmoothing = new BilateralSmoothing();
		bilateralSmoothing.KernelSize    = kernelSize; // Odd integer 3 ~ 25
		bilateralSmoothing.SpatialFactor = 12; // 0 ~ ?
		bilateralSmoothing.ColorFactor   = 75; // 0 ~ ?
		bilateralSmoothing.ColorPower    = 0.5; // 0 ~ ?
		bilateralSmoothing.EnableParallelProcessing = true;
		bilateralSmoothing.ApplyInPlace(outputImage);

//		BilateralSmoothing bilateralSmoothing = new BilateralSmoothing();
//		bilateralSmoothing.KernelSize    = 21; // Odd integer 3 ~ 25
//		bilateralSmoothing.SpatialFactor = 20; // 0 ~ ?
//		bilateralSmoothing.ColorFactor   = 120; // 0 ~ ?
//		bilateralSmoothing.ColorPower    = 0.5; // 0 ~ ?
//		bilateralSmoothing.EnableParallelProcessing = true;
//		bilateralSmoothing.ApplyInPlace(outputImage);
    }
    
    private void ShadowlessOperation1 ()
    {
        Initialize();
        
        RunAsync(()=>
        {
        	int Red = (int)Mathf.Clamp((shadowColor.r * 255f), 0f, 255f);
			int Green = (int)Mathf.Clamp((shadowColor.g * 255f), 0f, 255f);
			int Blue = (int)Mathf.Clamp((shadowColor.b * 255f), 0f, 255f);
			System.Drawing.Color shColor = System.Drawing.Color.FromArgb(Red, Green, Blue);
			shadowBrightness = shColor.GetBrightness(); //From 0 (black) to 1 (white)

			Rectangle rect = new Rectangle( 0, 0, sourceImage.Width, sourceImage.Height);
			Bitmap outputImage = (Bitmap)sourceImage.Clone(rect, sourceImage.PixelFormat);

			//TODO Check image's pixel format and set "bpp - bits per pixel" (e.g. 3, 4 &...) - SEARCH INTERNET TO GET BPP AUTOMATICALLY
			BitmapData bmpData = outputImage.LockBits(rect, ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);

			int stride = bmpData.Stride;

			int indexR;
			int indexG;
			int indexB;
			byte R;
			byte G;
			byte B;
			int indexROffset;
			int indexGOffset;
			int indexBOffset;
			byte ROffset;
			byte GOffset;
			byte BOffset;

			unsafe
			{
				byte* ptr = (byte*)bmpData.Scan0;

				// x = row & y = column
				for (int y = 0; y < bmpData.Height; y++)
				{
					for (int x = 0; x < bmpData.Width; x++)
					{
                    	indexR = (x * 3) + y * stride + 2;
						indexG = (x * 3) + y * stride + 1;
						indexB = (x * 3) + y * stride;

						R = ptr[indexR];
						G = ptr[indexG];
						B = ptr[indexB];

						color = System.Drawing.Color.FromArgb(R, G, B);
						currentPixelbrightness = color.GetBrightness();
							
                    	if(currentPixelbrightness < shadowBrightness)
						{
							try
							{


//								// LEFT ==> RIGHT
//								indexROffset = ((x * 3) + healingBlockSize) + y * stride + 2;
//								indexGOffset = ((x * 3) + healingBlockSize) + y * stride + 1;
//								indexBOffset = ((x * 3) + healingBlockSize) + y * stride;
//
//								ROffset = ptr[indexROffset];
//								GOffset = ptr[indexGOffset];
//								BOffset = ptr[indexBOffset];
//								
//								pixelColor = System.Drawing.Color.FromArgb(ROffset, GOffset, BOffset);
//								pixelColorBrightness = pixelColor.GetBrightness();
//								
//								if(pixelColorBrightness > shadowBrightness)
//								{
//									ptr[indexR] = pixelColor.R;
//									ptr[indexG] = pixelColor.G;
//									ptr[indexB] = pixelColor.B;
//								}
//
//								// RIGHT ==> LEFT
//								indexROffset = ((x * 3) - healingBlockSize) + y * stride + 2;
//								indexGOffset = ((x * 3) - healingBlockSize) + y * stride + 1;
//								indexBOffset = ((x * 3) - healingBlockSize) + y * stride;
//
//								ROffset = ptr[indexROffset];
//								GOffset = ptr[indexGOffset];
//								BOffset = ptr[indexBOffset];
//								
//								pixelColor = System.Drawing.Color.FromArgb(ROffset, GOffset, BOffset);
//								pixelColorBrightness = pixelColor.GetBrightness();
//								
//								if(pixelColorBrightness > shadowBrightness)
//								{
//									ptr[indexR] = pixelColor.R;
//									ptr[indexG] = pixelColor.G;
//									ptr[indexB] = pixelColor.B;
//								}
//
//								// TOP ==> BOTTOM
//								indexROffset = (x * 3) + (y + healingBlockSize) * stride + 2;
//								indexGOffset = (x * 3) + (y + healingBlockSize) * stride + 1;
//								indexBOffset = (x * 3) + (y + healingBlockSize) * stride;
//
//								ROffset = ptr[indexROffset];
//								GOffset = ptr[indexGOffset];
//								BOffset = ptr[indexBOffset];
//								
//								pixelColor = System.Drawing.Color.FromArgb(ROffset, GOffset, BOffset);
//								pixelColorBrightness = pixelColor.GetBrightness();
//								
//								if(pixelColorBrightness > shadowBrightness)
//								{
//									ptr[indexR] = pixelColor.R;
//									ptr[indexG] = pixelColor.G;
//									ptr[indexB] = pixelColor.B;
//								}
//
//								// BOTTOM ==> TOP
//								indexROffset = (x * 3) + (y - healingBlockSize) * stride + 2;
//								indexGOffset = (x * 3) + (y - healingBlockSize) * stride + 1;
//								indexBOffset = (x * 3) + (y - healingBlockSize) * stride;
//
//								ROffset = ptr[indexROffset];
//								GOffset = ptr[indexGOffset];
//								BOffset = ptr[indexBOffset];
//								
//								pixelColor = System.Drawing.Color.FromArgb(ROffset, GOffset, BOffset);
//								pixelColorBrightness = pixelColor.GetBrightness();
//								
//								if(pixelColorBrightness > shadowBrightness)
//								{
//									ptr[indexR] = pixelColor.R;
//									ptr[indexG] = pixelColor.G;
//									ptr[indexB] = pixelColor.B;
//								}



								// TOP-RIGHT ==> BOTTOM-LEFT
								indexROffset = ((x * 3) + healingBlockSize) + (y + healingBlockSize) * stride + 2;
								indexGOffset = ((x * 3) + healingBlockSize) + (y + healingBlockSize) * stride + 1;
								indexBOffset = ((x * 3) + healingBlockSize) + (y + healingBlockSize) * stride;
								
								ROffset = ptr[indexROffset];
								GOffset = ptr[indexGOffset];
								BOffset = ptr[indexBOffset];
								
								pixelColor = System.Drawing.Color.FromArgb(ROffset, GOffset, BOffset);
								pixelColorBrightness = pixelColor.GetBrightness();
								
								if(pixelColorBrightness > shadowBrightness)
								{
									ptr[indexR] = pixelColor.R;
									ptr[indexG] = pixelColor.G;
									ptr[indexB] = pixelColor.B;
								}
								
								// TOP-RIGHT ==> BOTTOM-LEFT
								indexROffset = ((x * 3) - healingBlockSize) + (y + healingBlockSize) * stride + 2;
								indexGOffset = ((x * 3) - healingBlockSize) + (y + healingBlockSize) * stride + 1;
								indexBOffset = ((x * 3) - healingBlockSize) + (y + healingBlockSize) * stride;
								
								ROffset = ptr[indexROffset];
								GOffset = ptr[indexGOffset];
								BOffset = ptr[indexBOffset];
								
								pixelColor = System.Drawing.Color.FromArgb(ROffset, GOffset, BOffset);
								pixelColorBrightness = pixelColor.GetBrightness();
								
								if(pixelColorBrightness > shadowBrightness)
								{
									ptr[indexR] = pixelColor.R;
									ptr[indexG] = pixelColor.G;
									ptr[indexB] = pixelColor.B;
								}
								
								// BOTTOM-LEFT ==> TOP-RIGHT
								indexROffset = ((x * 3) + healingBlockSize) + (y - healingBlockSize) * stride + 2;
								indexGOffset = ((x * 3) + healingBlockSize) + (y - healingBlockSize) * stride + 1;
								indexBOffset = ((x * 3) + healingBlockSize) + (y - healingBlockSize) * stride;
								
								ROffset = ptr[indexROffset];
								GOffset = ptr[indexGOffset];
								BOffset = ptr[indexBOffset];
								
								pixelColor = System.Drawing.Color.FromArgb(ROffset, GOffset, BOffset);
								pixelColorBrightness = pixelColor.GetBrightness();
								
								if(pixelColorBrightness > shadowBrightness)
								{
									ptr[indexR] = pixelColor.R;
									ptr[indexG] = pixelColor.G;
									ptr[indexB] = pixelColor.B;
								}
								
								// BOTTOM-RIGHT ==> TOP-LEFT
								indexROffset = ((x * 3) - healingBlockSize) + (y - healingBlockSize) * stride + 2;
								indexGOffset = ((x * 3) - healingBlockSize) + (y - healingBlockSize) * stride + 1;
								indexBOffset = ((x * 3) - healingBlockSize) + (y - healingBlockSize) * stride;
								
								ROffset = ptr[indexROffset];
								GOffset = ptr[indexGOffset];
								BOffset = ptr[indexBOffset];
								
								pixelColor = System.Drawing.Color.FromArgb(ROffset, GOffset, BOffset);
								pixelColorBrightness = pixelColor.GetBrightness();
								
								if(pixelColorBrightness > shadowBrightness)
								{
									ptr[indexR] = pixelColor.R;
									ptr[indexG] = pixelColor.G;
									ptr[indexB] = pixelColor.B;
								}
							}
							catch{}
						}
						else
						{
							ptr[indexR] = ptr[indexR];
							ptr[indexG] = ptr[indexG];
                        	ptr[indexB] = ptr[indexB];
                    	}
					}
				}
			}

			outputImage.UnlockBits(bmpData);

			QueueOnMainThread(()=>
			{
				ExportGeneratedMapShadow(outputImage);
				ImportImage(shadowRemoverOperation);
				//ShadowlessOperation2();
			});
		});
	}

	private void ShadowlessOperation2 ()
	{
		Initialize();

		Rectangle rect = new Rectangle( 0, 0, sourceImage.Width, sourceImage.Height);
		Bitmap outputImage = (Bitmap)sourceImage.Clone(rect, sourceImage.PixelFormat);

		RunAsync(()=>
        {
			try
            {
                int Red = (int)Mathf.Clamp((shadowColor.r * 255f), 0f, 255f);
				int Green = (int)Mathf.Clamp((shadowColor.g * 255f), 0f, 255f);
				int Blue = (int)Mathf.Clamp((shadowColor.b * 255f), 0f, 255f);
				System.Drawing.Color shColor = System.Drawing.Color.FromArgb(Red, Green, Blue);
				shadowBrightness = shColor.GetBrightness(); //From 0 (black) to 1 (white)

				BitmapData bmpData = outputImage.LockBits(rect, ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);
				int stride = bmpData.Stride;
				
				int indexR;
				int indexG;
				int indexB;
				byte R;
				byte G;
				byte B;
				int indexROffset;
				int indexGOffset;
				int indexBOffset;
				byte ROffset;
				byte GOffset;
				byte BOffset;
				
				unsafe
				{
					byte* ptr = (byte*)bmpData.Scan0;
					
					// x = row & y = column
					for (int y = 0; y < bmpData.Height; y++)
					{
						for (int x = 0; x < bmpData.Width; x++)
						{
							indexR = (x * 3) + y * stride + 2;
							indexG = (x * 3) + y * stride + 1;
							indexB = (x * 3) + y * stride;
							
							R = ptr[indexR];
							G = ptr[indexG];
							B = ptr[indexB];
							
							color = System.Drawing.Color.FromArgb(R, G, B);
							currentPixelbrightness = color.GetBrightness();
							
							if(currentPixelbrightness < shadowBrightness)
							{
								// TOP-RIGHT ==> BOTTOM-LEFT
								indexROffset = ((x * 3) + healingBlockSize) + (y + healingBlockSize) * stride + 2;
								indexGOffset = ((x * 3) + healingBlockSize) + (y + healingBlockSize) * stride + 1;
								indexBOffset = ((x * 3) + healingBlockSize) + (y + healingBlockSize) * stride;
								
								ROffset = ptr[indexROffset];
								GOffset = ptr[indexGOffset];
								BOffset = ptr[indexBOffset];
								
								pixelColor = System.Drawing.Color.FromArgb(ROffset, GOffset, BOffset);
								pixelColorBrightness = pixelColor.GetBrightness();
								
								if(pixelColorBrightness > shadowBrightness)
								{
									ptr[indexR] = pixelColor.R;
									ptr[indexG] = pixelColor.G;
	                                ptr[indexB] = pixelColor.B;
	                            }
	                            
	                            // TOP-RIGHT ==> BOTTOM-LEFT
	                            indexROffset = ((x * 3) - healingBlockSize) + (y + healingBlockSize) * stride + 2;
	                            indexGOffset = ((x * 3) - healingBlockSize) + (y + healingBlockSize) * stride + 1;
	                            indexBOffset = ((x * 3) - healingBlockSize) + (y + healingBlockSize) * stride;
	                            
	                            ROffset = ptr[indexROffset];
	                            GOffset = ptr[indexGOffset];
	                            BOffset = ptr[indexBOffset];
	                            
	                            pixelColor = System.Drawing.Color.FromArgb(ROffset, GOffset, BOffset);
	                            pixelColorBrightness = pixelColor.GetBrightness();
	                            
	                            if(pixelColorBrightness > shadowBrightness)
	                            {
	                                ptr[indexR] = pixelColor.R;
	                                ptr[indexG] = pixelColor.G;
	                                ptr[indexB] = pixelColor.B;
	                            }
	                            
	                            // BOTTOM-LEFT ==> TOP-RIGHT
	                            indexROffset = ((x * 3) + healingBlockSize) + (y - healingBlockSize) * stride + 2;
	                            indexGOffset = ((x * 3) + healingBlockSize) + (y - healingBlockSize) * stride + 1;
	                            indexBOffset = ((x * 3) + healingBlockSize) + (y - healingBlockSize) * stride;
	                            
	                            ROffset = ptr[indexROffset];
	                            GOffset = ptr[indexGOffset];
	                            BOffset = ptr[indexBOffset];
	                            
	                            pixelColor = System.Drawing.Color.FromArgb(ROffset, GOffset, BOffset);
	                            pixelColorBrightness = pixelColor.GetBrightness();
	                            
	                            if(pixelColorBrightness > shadowBrightness)
	                            {
	                                ptr[indexR] = pixelColor.R;
	                                ptr[indexG] = pixelColor.G;
	                                ptr[indexB] = pixelColor.B;
	                            }
	                            
	                            // BOTTOM-RIGHT ==> TOP-LEFT
	                            indexROffset = ((x * 3) - healingBlockSize) + (y - healingBlockSize) * stride + 2;
	                            indexGOffset = ((x * 3) - healingBlockSize) + (y - healingBlockSize) * stride + 1;
	                            indexBOffset = ((x * 3) - healingBlockSize) + (y - healingBlockSize) * stride;
	                            
	                            ROffset = ptr[indexROffset];
	                            GOffset = ptr[indexGOffset];
	                            BOffset = ptr[indexBOffset];
	                            
	                            pixelColor = System.Drawing.Color.FromArgb(ROffset, GOffset, BOffset);
	                            pixelColorBrightness = pixelColor.GetBrightness();
	                            
	                            if(pixelColorBrightness > shadowBrightness)
	                            {
	                                ptr[indexR] = pixelColor.R;
	                                ptr[indexG] = pixelColor.G;
	                                ptr[indexB] = pixelColor.B;
	                            }
							}
							else
							{
								ptr[indexR] = ptr[indexR];
								ptr[indexG] = ptr[indexG];
								ptr[indexB] = ptr[indexB];
	                		}
	                    }
	                }
		        }
		        outputImage.UnlockBits(bmpData);
			}
			catch{}
			
            QueueOnMainThread(()=>
			{
				ExportGeneratedMapShadow(outputImage);
				ImportImage(shadowRemoverOperation);
			});
		});
	}

	private void BlobDetector ()
	{
		Initialize();

		Rectangle rect = new Rectangle( 0, 0, sourceImage.Width, sourceImage.Height);
		Bitmap outputImage = (Bitmap)sourceImage.Clone(rect, sourceImage.PixelFormat);

		FiltersSequence filters = new FiltersSequence (

			new Grayscale(0.2125, 0.7154, 0.0721),

			new ContrastCorrection(50),

			//new HistogramEqualization(),

			//new ContrastStretch(),

			new ConnectedComponentsLabeling()

			//new ExtractBiggestBlob()
		);

		filters.Apply(outputImage);


		BlobCounter blobCounter = new BlobCounter();
		blobCounter.FilterBlobs = true;
		blobCounter.MinWidth  = 0;
		blobCounter.MinHeight = 0;
		blobCounter.ObjectsOrder = ObjectsOrder.Size;
		blobCounter.ProcessImage(outputImage);

		Blob[] blobs = blobCounter.GetObjectsInformation();
		
		//if (blobs.Length > 0)
			//blobCounter.ExtractBlobsImage(outputImage, blobs[0], true);

		GrahamConvexHull hullFinder = new GrahamConvexHull();

		System.Drawing.Graphics g = System.Drawing.Graphics.FromImage(outputImage);
		FillMode fillMode = FillMode.Winding;
		System.Drawing.Color currentPolygonColor = new System.Drawing.Color();

		System.Drawing.Color[] colorTable = new System.Drawing.Color[]
		{
//			System.Drawing.Color.Red, System.Drawing.Color.Green, System.Drawing.Color.Blue, System.Drawing.Color.Yellow,
//			System.Drawing.Color.Violet, System.Drawing.Color.Brown, System.Drawing.Color.Olive, System.Drawing.Color.Cyan,
//			
//			System.Drawing.Color.Magenta, System.Drawing.Color.Gold, System.Drawing.Color.Indigo, System.Drawing.Color.Ivory,
//			System.Drawing.Color.HotPink, System.Drawing.Color.DarkRed, System.Drawing.Color.DarkGreen, System.Drawing.Color.DarkBlue,
//			
//			System.Drawing.Color.DarkSeaGreen, System.Drawing.Color.Gray, System.Drawing.Color.DarkKhaki, System.Drawing.Color.DarkGray,
//			System.Drawing.Color.LimeGreen, System.Drawing.Color.Tomato, System.Drawing.Color.SteelBlue, System.Drawing.Color.SkyBlue,
//			
//			System.Drawing.Color.Silver, System.Drawing.Color.Salmon, System.Drawing.Color.SaddleBrown, System.Drawing.Color.RosyBrown,
//			System.Drawing.Color.PowderBlue, System.Drawing.Color.Plum, System.Drawing.Color.PapayaWhip, System.Drawing.Color.Orange

			System.Drawing.Color.Red, System.Drawing.Color.Green, System.Drawing.Color.Blue, System.Drawing.Color.Black
		};

		int colorsLength = colorTable.Length;
		int counter = 0;

		//foreach (Blob blob in blobs)
		for (int i = 0; i < blobs.Length; i++)
		{
			if(counter >= colorsLength)
				counter = 0;

			currentPolygonColor = colorTable[counter];

			//Pen pen = new Pen(currentPolygonColor, 2);
			SolidBrush brush = new SolidBrush(currentPolygonColor);

			List<IntPoint> leftPoints, rightPoints, edgePoints = new List<IntPoint>();

			blobCounter.GetBlobsLeftAndRightEdges(blobs[i], out leftPoints, out rightPoints);
			
			edgePoints.AddRange(leftPoints);
			edgePoints.AddRange(rightPoints);

			List<IntPoint> hull = hullFinder.FindHull(edgePoints);

			System.Drawing.Point[] points = new System.Drawing.Point[hull.Count];
			
			for (int j = 0; j < hull.Count; j++)
				points[j] = new System.Drawing.Point(hull[j].X, hull[j].Y);

			g.FillPolygon(brush, points, fillMode);
			//g.DrawPolygon(pen, points);

			try {
				//if(blobs[i].Area <= blobs[i - 1].Area)
				//if(counter % 10 == 0)
					counter++;
			}
			catch{}
		}



		//outputImage = filters.Apply(sourceImage);

		//ExportGeneratedMap();

		//AssetDatabase.Refresh();
		//ImportImage();
	}

	private void GenerateFilters ()
	{
		if(!shadowRemoverOperation && !colormapOperation && !landCoverOperation)
			Initialize();

		RunAsync(()=>
		{
			showProgressFilters = true;
			filtersGenerationOperation = true;
			UnmanagedImage unmanagedImage = UnmanagedImage.FromManagedImage(sourceImage);
			Rectangle rect = new Rectangle( 0, 0, sourceImage.Width, sourceImage.Height);
			dictColors = new Dictionary<System.Drawing.Color, int>();
			ReadImagePixels(unmanagedImage, rect);

			progressFilters = Mathf.InverseLerp(0f, 1f, 0.5f);
			progressFiltersStage = 2;
			
			var sortedDict = from entry in dictColors orderby entry.Value descending select entry;
			List<KeyValuePair<System.Drawing.Color, int>> dictionary = sortedDict.ToList();
			
			System.Drawing.Color currentColor = System.Drawing.Color.Black;
			//KeyValuePair<System.Drawing.Color, int> pair = new KeyValuePair<System.Drawing.Color, int>();
			extractedColors = new List<System.Drawing.Color>();
			int counter = 0;
			
			for (int i = 0; i < dictionary.Count; i++)
			{
				currentColor = dictionary[i].Key;
				
				if(tolerance > 0)
				{
					if(i == 0)
					{
						extractedColors.Add(currentColor);
						counter++;
					}
					else
					{
						if(CheckAvailability(currentColor, extractedColors) == true)
						{
							extractedColors.Add(currentColor);
							counter++;
						}
					}
				}
				else
				{
					extractedColors.Add(currentColor);
					counter++;
				}

				progressFilters = Mathf.InverseLerp(0f, (float)dictionary.Count, (float)i);
				progressFiltersStage = 3;
				
				if(counter == topColorsCount)
					break;
			}
			
			PopulateFilters();

			QueueOnMainThread(()=>
			{
				showProgressFilters = false;
				filtersGenerationOperation = false;
			});
		});
	}
		
	private void GenerateLandCover ()
	{
		if(outputFilterColors.Count == 0)
		{
			EditorUtility.DisplayDialog("FILTERS NOT SET", "Please Review Filters and Try Again.", "Ok");
			showLandcoverSection = true;
			return;
		}

		if(!shadowRemoverOperation && !colormapOperation && !filtersGenerationOperation)
			Initialize();

		RunAsync(()=>
		{
			landCoverOperation = true;
			showProgressLandCover = true;
			Rectangle rect = new Rectangle( 0, 0, sourceImage.Width, sourceImage.Height);
			Bitmap outputImage = (Bitmap)sourceImage.Clone(rect, PixelFormat.Format32bppArgb);
			progressLandCoverStage = 1;

			EuclideanColorFilteringOperation euclideanColorFiltering  = new EuclideanColorFilteringOperation();

			for (int i = 0; i < filtersNo; i++)
			{
				filterColorsDrawing.Add(System.Drawing.Color.Black);
				filterColorsDrawing[i] = System.Drawing.Color.FromArgb((int)(filterColors[i].a * 255f),
				                                                      (int)(filterColors[i].r * 255f),
				                                                      (int)(filterColors[i].g * 255f),
				                                                      (int)(filterColors[i].b * 255f)
				                                                      );

				outputColorsDrawing.Add(System.Drawing.Color.Black);
				outputColorsDrawing[i] = System.Drawing.Color.FromArgb((int)(outputFilterColors[i].a * 255f),
				                                                      (int)(outputFilterColors[i].r * 255f),
				                                                      (int)(outputFilterColors[i].g * 255f),
				                                                      (int)(outputFilterColors[i].b * 255f)
				                                                      );

				euclideanColorFiltering.CenterColor.Color = filterColorsDrawing[i];
				euclideanColorFiltering.OutputColor.Color = outputColorsDrawing[i];
				euclideanColorFiltering.Radius = (short)filterRadius[i];
				euclideanColorFiltering.FillOutside = false;

				euclideanColorFiltering.ApplyInPlace(outputImage);

				progressLandCover = Mathf.InverseLerp(0f, (float)filtersNo, (float)i);
			}

			QueueOnMainThread(()=>
			{
				FinalizeSplatMap(outputImage);
			});
		});
	}

	private bool CheckAvailability (System.Drawing.Color currentColor, List<System.Drawing.Color> extractedColors)
	{
		int checkedIndex = 0;

		foreach(System.Drawing.Color c in extractedColors)
		{
			if(currentColor.R >= c.R + tolerance || currentColor.R <= c.R - tolerance)
				if(currentColor.G >= c.G + tolerance || currentColor.G <= c.G - tolerance)
					if(currentColor.B >= c.B + tolerance || currentColor.B <= c.B - tolerance)
						checkedIndex++;
		}

		if(checkedIndex == extractedColors.Count)
			return true;

		return false;
	}
	
	protected unsafe void ReadImagePixels(UnmanagedImage image, Rectangle rect)
	{
		// get pixel size
		int pixelSize = ( image.PixelFormat == PixelFormat.Format24bppRgb ) ? 3 : 4;
		
		int startX  = rect.Left;
		int startY  = rect.Top;
		int stopX   = startX + rect.Width;
		int stopY   = startY + rect.Height;
		int offset  = image.Stride - rect.Width * pixelSize;
		
		// do the job
		byte* ptr = (byte*) image.ImageData.ToPointer();
		
		// allign pointer to the first pixel to process
		ptr += ( startY * image.Stride + startX * pixelSize );

		System.Drawing.Color color = System.Drawing.Color.Black;

		// for each row
		for ( int y = startY; y < stopY; y++ )
		{
			// for each pixel
			for ( int x = startX; x < stopX; x++, ptr += pixelSize )
			{
				color = System.Drawing.Color.FromArgb(ptr[RGB.A], ptr[RGB.R], ptr[RGB.G], ptr[RGB.B]);

				if(dictColors.ContainsKey(color) == true)
					dictColors[color]++;
                else
					dictColors.Add(color, 0);

				progressFilters = Mathf.InverseLerp(0f, (float)stopY, (float)y);
				progressFiltersStage = 1;
			}
			ptr += offset;
		}
	}

	private void PopulateFilters ()
	{
		for(int i = 0; i < filtersNo; i++)
		{
			filterNames[i] = "Filter " + (i + 1).ToString();
			
			filterColors[i] = new UnityEngine.Color((float)extractedColors[i].R / 255f,
			                                        (float)extractedColors[i].G / 255f,
			                                        (float)extractedColors[i].B / 255f,
			                                        (float)extractedColors[i].A / 255f
			                                        );

			filterRadius[i] = Mathf.Clamp(filterRadiusAuto - (i * 0), 0, 450);

			if(outputOrderingIndex == 0)
			{
				if(resetter == outCols.Count)
					resetter = 0;

				UnityEngine.Color outCol = outCols[resetter];

				if(outCol.Equals(UnityEngine.Color.red))
					outColStr = "_Red";
				else if(outCol.Equals(UnityEngine.Color.green))
					outColStr = "_Green";
				else if(outCol.Equals(UnityEngine.Color.blue))
					outColStr = "_Blue";
				else if(outCol.Equals(UnityEngine.Color.black))
					outColStr = "_Alpha";

				mapColors[i] = (MapColor)Enum.Parse(typeof(MapColor), outColStr);
				outputFilterColors[i] = outCol;

				resetter++;
			}
			else
			{
				float cR = filterColors[i].r;
				float cG = filterColors[i].g;
				float cB = filterColors[i].b;
				
				if(cR > cG && cR > cB)
				{
					mapColors[i] = MapColor._Red;
					outputFilterColors[i] = UnityEngine.Color.red;
				}
				else if(cG > cR && cG > cB)
				{
					mapColors[i] = MapColor._Green;
					outputFilterColors[i] = UnityEngine.Color.green;
				}
				else if(cB > cR && cB > cG)
				{
					mapColors[i] = MapColor._Blue;
					outputFilterColors[i] = UnityEngine.Color.blue;
				}
				else
				{
					mapColors[i] = MapColor._Alpha;
					outputFilterColors[i] = UnityEngine.Color.black;
				}
			}
		}
	}

	private void ImportImage (bool mapType)
	{
		if(mapType.Equals(shadowRemoverOperation))
			mapTypeStr = "IMPORTING SHADOWLESS MAP";
		else if(mapType.Equals(colormapOperation))
			mapTypeStr = "IMPORTING COLORMAP";
		else if(mapType.Equals(landCoverOperation))
			mapTypeStr = "IMPORTING LAND=COVER";

		UnityEngine.Object asset = UnityEditor.AssetDatabase.LoadAssetAtPath(outputImagePath.Substring(outputImagePath.LastIndexOf("Assets")), typeof(UnityEngine.Object)) as UnityEngine.Object;
		UnityEditor.TextureImporter textureImporter = UnityEditor.AssetImporter.GetAtPath(UnityEditor.AssetDatabase.GetAssetPath(asset)) as UnityEditor.TextureImporter;
		int counterImages = 0;
		int downloadedImageIndex = 1;

		if(textureImporter != null)
		{
			textureImporter.mipmapEnabled = true;
			textureImporter.wrapMode = TextureWrapMode.Clamp;
			textureImporter.maxTextureSize = Mathf.ClosestPowerOfTwo(imageResolution);

			if(landCoverOperation)
			{
                #if UNITY_5_6_OR_NEWER
                    TextureImporterPlatformSettings platformSettings = new TextureImporterPlatformSettings();
                    platformSettings.format = TextureImporterFormat.ARGB32;
                    textureImporter.SetPlatformTextureSettings(platformSettings);
                #else
                    textureImporter.textureFormat = TextureImporterFormat.ARGB32;
                #endif

				textureImporter.npotScale = TextureImporterNPOTScale.ToNearest;
			}
			
			EditorUtility.DisplayProgressBar(mapTypeStr, "Image  " + (counterImages + 1).ToString() +"  of  "+ downloadedImageIndex.ToString(), Mathf.InverseLerp(0f, (float)(downloadedImageIndex - 1), (float)(counterImages)));
			UnityEditor.AssetDatabase.ImportAsset(UnityEditor.AssetDatabase.GetAssetPath(asset), UnityEditor.ImportAssetOptions.ForceUpdate);
        }
		EditorUtility.ClearProgressBar();
    }

	private void ExportGeneratedMapShadow (Bitmap outputImage)
	{
		outputImagePath = sourceImagePath.Replace(imageFormatStr, "_Shadowless" + imageFormatStr);
		outputImage.Save(outputImagePath, imageFormat);
		
		AssetDatabase.Refresh();
	}

	private void ExportGeneratedMapColor (Bitmap outputImage)
	{
		outputImagePath = sourceImagePath.Replace(imageFormatStr, "_Colormap" + imageFormatStr);
		outputImage.Save(outputImagePath, imageFormat);
		
		AssetDatabase.Refresh();
	}

	private void ExportGeneratedMapLand (Bitmap outputImage)
	{
		if(splatFormatIndex == 0)
		{
			outputImagePath = sourceImagePath.Replace(imageFormatStr, "_LandCover" + ".png");
			outputImage.Save(outputImagePath, ImageFormat.Png);
		}
		else
		{
			outputImagePath = sourceImagePath.Replace(imageFormatStr, "_LandCover" + ".tif");
			outputImage.Save(outputImagePath, ImageFormat.Tiff);
		}
		
		AssetDatabase.Refresh();
	}
    
    private void ExtraFunctions ()
    {
//		Vector2 index = new Vector2(x, y);
//		Vector2 topIndex = new Vector2(x - 1, y);
//		Vector2 leftIndex = new Vector2(x, y - 1);
//		Vector2 bottomIndex = new Vector2(x + 1, y);
//		Vector2 rightIndex = new Vector2(x, y + 1);
//		Vector2 topLeftIndex = new Vector2(x - 1, y - 1);
//		Vector2 topRightIndex = new Vector2(x - 1, y + 1);
//		Vector2 bottomLeftIndex = new Vector2(x + 1, y - 1);
//		Vector2 bottomRightIndex = new Vector2(x + 1, y + 1);

//		int imageWidth = sourceImage.Width;
//		int imageHeight = sourceImage.Height;
//		outputImage = new Bitmap(imageWidth, imageHeight, sourceImage.PixelFormat);
//		
//		for (int y = 0; y < imageHeight; y++)
//		{
//			for (int x = 0; x < imageWidth; x++)
//			{
//				Vector2 index = new Vector2(x, y);
//				
//				System.Drawing.Color color = sourceImage.GetPixel((int)index.x, (int)index.y);
//
//				byte R = color.R;
//				byte G = color.G;
//				byte B = color.B;
//				int bwFilterRed = 16;
//				int bwFilterGrn = 4;
//				int bwFilterBlu = 16;
//				
//				try
//				{
//					// Black & White Filter BLUE
//					if(Mathf.Abs(R - G) >= bwFilterBlu && Mathf.Abs(R - B) >= bwFilterBlu)
//					{
//						// Blue Filter
//						if(B > R && B > G) {
//							// Light Blue
//							if(B >= 128)
//								outputImage.SetPixel(x, y, System.Drawing.Color.Blue);
//							
//							// Dark Blue
//							if(B < 128)
//								outputImage.SetPixel(x, y, System.Drawing.Color.Blue);
//						}
//					}
//
//					// Black & White Filter RED
//					if(Mathf.Abs(R - G) >= bwFilterRed && Mathf.Abs(R - B) >= bwFilterRed)
//					{
//						// Red Filter
//						if(R > G && R > B) {
//							// Light Red
//							if(R >= 128)
//								outputImage.SetPixel(x, y, System.Drawing.Color.Red);
//							
//							// Dark Red
//							if(R < 128)
//								outputImage.SetPixel(x, y, System.Drawing.Color.Red);
//						}
//					}
//
//					// Black & White Filter GREEN
//					if(Mathf.Abs(R - G) >= bwFilterGrn && Mathf.Abs(R - B) >= bwFilterGrn)
//					{
//						// Green Filter
//						if(G > R && G > B) {
//							// Light Green
//							if(G >= 128)
//								outputImage.SetPixel(x, y, System.Drawing.Color.Green);
//							
//							// Dark Green
//							if(G < 128)
//								outputImage.SetPixel(x, y, System.Drawing.Color.Green);
//						}
//					}
//
//
////					else
////					{
////						outputImage.SetPixel(x, y, System.Drawing.Color.Black);
////					}
//				}
//				catch {}
//			}
//		}
	}

	#region multithreading functions
	
	protected void QueueOnMainThread (Action action)
	{
		QueueOnMainThread( action, 0f);
	}
	
	protected void QueueOnMainThread (Action action, float time)
	{
		if(time != 0)
		{
			lock(_delayed)
			{
				_delayed.Add(new DelayedQueueItem { time = Time.time + time, action = action});
			}
		}
		else
		{
			lock (_actions)
			{
				_actions.Add(action);
			}
		}
	}
	
	protected Thread RunAsync (Action a)
	{
		while(numThreads >= maxThreads)
		{
			Thread.Sleep(1);
		}
		Interlocked.Increment(ref numThreads);
		ThreadPool.QueueUserWorkItem(RunAction, a);
		return null;
	}
	
	private void RunAction (object action)
	{
		try
		{
			((Action)action)();
		}
		catch {}
		finally
		{
			Interlocked.Decrement(ref numThreads);
		}
	}
	
	protected virtual void Start ()
	{
		m_HasLoaded = true;
	}
	
	protected virtual void Update ()
	{
		if(m_HasLoaded == false)
			Start();
		
        if(_actions != null && _actions.Count > 0)
        {
            lock (_actions)
            {
                _currentActions.Clear();
                _currentActions.AddRange(_actions);
                _actions.Clear();
            }

            foreach(var a in _currentActions)
                a();
        }
		
        if(_delayed != null && _delayed.Count > 0)
        {
    		lock(_delayed)
    		{
    			_currentDelayed.Clear();
    			_currentDelayed.AddRange(_delayed.Where(d => d.time <= Time.time));
    			
    			foreach(var item in _currentDelayed)
    				_delayed.Remove(item);
    		}
    		
    		foreach(var delayed in _currentDelayed)
    			delayed.action();
        }
	}

	#endregion

	private void PresetManager ()
	{
		EditorGUILayout.BeginHorizontal();
		GUILayout.FlexibleSpace();
		if (GUILayout.Button("SAVE PRESET"))
		{
            if (!Directory.Exists(presetsPath))
			{
                Directory.CreateDirectory(presetsPath);
				UnityEditor.AssetDatabase.Refresh();
			}
			
            presetFilePath = EditorUtility.SaveFilePanel("Save Settings As Preset File", presetsPath, "Color Filters", "filters");
			
			if (presetFilePath.Contains("/"))
				presetName = presetFilePath.Replace(presetFilePath.Substring(0, presetFilePath.LastIndexOf('/')), "").Replace("/", "");
			
			if (presetName != null && !presetName.Equals(""))
                WritePresetFile(presetsPath + "/" + presetName);
		}
		GUILayout.FlexibleSpace();
		EditorGUILayout.EndHorizontal();

		GUILayout.Space(5);

		EditorGUILayout.BeginHorizontal();
		GUILayout.FlexibleSpace();
		if (GUILayout.Button("LOAD PRESET"))
		{
            presetFilePath = EditorUtility.OpenFilePanel("Load Preset File", presetsPath, "filters");

			if (presetFilePath.Contains("filters"))
				ReadPresetFile();
		}
		GUILayout.FlexibleSpace();
		EditorGUILayout.EndHorizontal();
	}

	private void AutoSave ()
	{
        if (!Directory.Exists(presetsPath))
            Directory.CreateDirectory(presetsPath);
		
        presetFilePath = presetsPath + "/MapsMaker AutoSave.filters";
		WritePresetFile(presetFilePath);
	}
	
	private void AutoLoad ()
	{
        presetFilePath = presetsPath + "/MapsMaker AutoSave.filters";
		
		if (File.Exists(presetFilePath) && presetFilePath.Contains("filters"))
			ReadPresetFile();
	}

	private void WritePresetFile (string fileName)
	{
		string presetStrHeader = "TerraLand-MapsMaker Filters and Settings\n" + "\nFiltersNO.: " + filtersNo;
		string presetStrLandCoverFilters = "";

		for(int i = 0; i < filtersNo; i++)
		{
			presetStrLandCoverFilters += "\nName: " + filterNames[i]
			+ "\nColor: " + filterColors[i].r +" "+ filterColors[i].g +" "+ filterColors[i].b +" "+ filterColors[i].a
			+ "\nRadius: " + filterRadius[i]
			+ "\nMapColor: " + mapColors[i];
		}

		string presetStrFields = "\nShadowColor: " + shadowColor.r +" "+ shadowColor.g +" "+ shadowColor.b +" "+ shadowColor.a
			+"\nShadowColorBrightness: " + shadowColorBrightness
			+"\nHealingBlockSize: " + healingBlockSize
			+"\nErodeSize: " + erodeSize
			+"\nColormapSmoothness: " + colormapKernelSize
			+"\nColormapSharpness: " + colormapSharpness
			+"\nSplatmapFormat: " + splatFormatIndex
			+"\nLandCoverAuto: " + landCoverModeIndex
			+"\nFiltersCount: " + topColorsCount
			+"\nColorDamping: " + tolerance
			+"\nColorRadius: " + filterRadiusAuto
			+"\nOutputOrdering: " + outputOrderingIndex
			+"\nOutputSmoothness: " + filterSmoothingKernelSize
			+"\nProcessingSection: " + showProcessingSection
			+"\nShadowSection: " + showShadowSection
			+"\nColormapSection: " + showColormapSection
			+"\nLandcoverSection: " + showLandcoverSection;

		string presetStr = presetStrHeader + presetStrLandCoverFilters + presetStrFields;
		File.WriteAllText(fileName, presetStr);
	}
	
	private void ReadPresetFile ()
	{
		string text = File.ReadAllText(presetFilePath);
		string[] dataLines = text.Split('\n');
		string[][] dataPairs = new string[dataLines.Length][];
		int lineNum = 0;
		int lastlandCoverFiltersLine = 0;
		
		foreach (string line in dataLines)
			dataPairs[lineNum++] = line.Split(' ');

		filtersNo = int.Parse(dataPairs[2][1]);

		for(int i = 0; i < filtersNo; i++)
		{
			filterNames.Add("");
			filterNames[i] = "";
			
			for(int j = 1; j < dataPairs[(i * 4) + 3].Length; j++)
            {
				filterNames[i] += dataPairs[(i * 4) + 3][j];

				if(j < dataPairs[(i * 4) + 3].Length - 1)
					filterNames[i] += " ";
            }

			filterColors.Add(UnityEngine.Color.black);
			filterColors[i] = new UnityEngine.Color
                (
                    float.Parse(dataPairs[(i * 4) + 4][1]),
    			    float.Parse(dataPairs[(i * 4) + 4][2]),
    			    float.Parse(dataPairs[(i * 4) + 4][3]),
    			    float.Parse(dataPairs[(i * 4) + 4][4])
    			);

			filterRadius.Add(100);
			filterRadius[i] = int.Parse(dataPairs[(i * 4) + 5][1]);

			mapColors.Add(MapColor._Red);
			mapColors[i] = (MapColor)Enum.Parse(typeof(MapColor), dataPairs[(i * 4) + 6][1]);

			lastlandCoverFiltersLine = (i * 4) + 7;
		}

		shadowColor = new UnityEngine.Color
            (
                float.Parse(dataPairs[lastlandCoverFiltersLine][1]),	
                float.Parse(dataPairs[lastlandCoverFiltersLine][2]),
                float.Parse(dataPairs[lastlandCoverFiltersLine][3]),
                float.Parse(dataPairs[lastlandCoverFiltersLine][4])
    	    );

		shadowColorBrightness = float.Parse(dataPairs[lastlandCoverFiltersLine + 1][1]);
		healingBlockSize = int.Parse(dataPairs[lastlandCoverFiltersLine + 2][1]);
		erodeSize = int.Parse(dataPairs[lastlandCoverFiltersLine + 3][1]);
		colormapKernelSize = int.Parse(dataPairs[lastlandCoverFiltersLine + 4][1]);
		colormapSharpness = int.Parse(dataPairs[lastlandCoverFiltersLine + 5][1]);
		splatFormatIndex = int.Parse(dataPairs[lastlandCoverFiltersLine + 6][1]);
		landCoverModeIndex = int.Parse(dataPairs[lastlandCoverFiltersLine + 7][1]);
		topColorsCount = int.Parse(dataPairs[lastlandCoverFiltersLine + 8][1]);
		tolerance = int.Parse(dataPairs[lastlandCoverFiltersLine + 9][1]);
		filterRadiusAuto = int.Parse(dataPairs[lastlandCoverFiltersLine + 10][1]);
		outputOrderingIndex = int.Parse(dataPairs[lastlandCoverFiltersLine + 11][1]);
		filterSmoothingKernelSize = int.Parse(dataPairs[lastlandCoverFiltersLine + 12][1]);

		if (dataPairs[lastlandCoverFiltersLine + 13][1].Contains("True"))
			showProcessingSection = true;
		else
			showProcessingSection = false;

		if (dataPairs[lastlandCoverFiltersLine + 14][1].Contains("True"))
			showShadowSection = true;
		else
			showShadowSection = false;

		if (dataPairs[lastlandCoverFiltersLine + 15][1].Contains("True"))
			showColormapSection = true;
		else
			showColormapSection = false;

		if (dataPairs[lastlandCoverFiltersLine + 16][1].Contains("True"))
			showLandcoverSection = true;
		else
			showLandcoverSection = false;
	}

	public void OnInspectorUpdate()
	{
		Repaint();
	}
}

