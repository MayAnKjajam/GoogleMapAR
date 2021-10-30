/*
	_____  _____  _____  _____  ______
	    |  _____ |      |      |  ___|
	    |  _____ |      |      |     |
	
     U       N       I       T      Y
                                         
	
	TerraUnity Co. - Earth Simulation Tools
	February 2019
	
	http://terraunity.com
	info@terraunity.com
	
	This script is written for Unity Engine
    Unity Version: 2017.2 & up
	
	
	
	HOW TO USE:   This plugin is for creating photorealistic terrains from GIS data in your scene.
	
	For full info & documentation on how to use this plugin please visit: http://www.terraunity.com
	
	
	
	License: Copyright © All Rights Reserved. - TerraUnity Co.
	(C)2019 by TerraUnity Team <info@terraunity.com>
*/

using UnityEngine;
using UnityEditor;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Xml;
using BitMiracle.LibTiff.Classic;

namespace TerraLand
{
	public class TerraLand_Terrain : EditorWindow
	{
		[MenuItem("Tools/TerraUnity/TerraLand/Terrain", false, 1)]
		public static void Init()
		{
			TerraLand_Terrain window = (TerraLand_Terrain)GetWindow(typeof(TerraLand_Terrain));
			window.position = new Rect(5, 135, 430, 800);
		}

		#region fields:

		Vector2 scrollPosition = Vector2.zero;
		bool allowSceneObjects = true;
		string presetFilePath;
		string presetName;
		string dataPath;

		Terrain terrain;
		string terrainName;

		string decimalDegree = "";
		float degree;
		float minute;
		float second;
		string decimalDegree2 = "";
		float degree2;
		float minute2;
		float second2;

		string decimalDegreeConverting = "";
		int degreeConverted;
		int minuteConverted;
		double secondConverted;
		string decimalDegreeConverting2 = "";
		int degreeConverted2;
		int minuteConverted2;
		double secondConverted2;

		int terrainWidth;
		int terrainHeight;

		public static string top, left, bottom, right = "";

		public enum Size
		{
			_2x2 = 2,
			_3x3 = 3, // Needs Extra Operations
			_4x4 = 4,
			_5x5 = 5, // Needs Extra Operations
			_6x6 = 6, // Needs Extra Operations
			_7x7 = 7, // Needs Extra Operations
			_8x8 = 8,
			_9x9 = 9, // Needs Extra Operations
			_10x10 = 10, // Needs Extra Operations
			_11x11 = 11, // Needs Extra Operations
			_12x12 = 12, // Needs Extra Operations
			_13x13 = 13, // Needs Extra Operations
			_14x14 = 14, // Needs Extra Operations
			_15x15 = 15, // Needs Extra Operations
			_16x16 = 16,
			_32x32 = 32,
			_64x64 = 64
		}
		static Size enumValue = Size._2x2;

		public enum DataTiles
		{
			_1 = 1,
			_2x2 = 2,
			_4x4 = 4,
			_8x8 = 8,
			_16x16 = 16,
			_32x32 = 32,
			_64x64 = 64,
			_128x128 = 128
		}
		static DataTiles dataTiles = DataTiles._4x4;

		public enum SizeNew
		{
			_1 = 1,
			_2x2 = 2,
			_3x3 = 3,
			_4x4 = 4,
			_5x5 = 5,
			_6x6 = 6,
			_7x7 = 7,
			_8x8 = 8,
			_9x9 = 9,
			_10x10 = 10,
			_11x11 = 11,
			_12x12 = 12,
			_13x13 = 13,
			_14x14 = 14,
			_15x15 = 15,
			_16x16 = 16,
			_32x32 = 32,
			_64x64 = 64
		}
		static SizeNew enumValueNew = SizeNew._2x2;

		public enum MapType
		{
			BingSatelliteMap = 9,
			BingHybridMap = 10,
			BingMap = 7,
			OpenStreetMap = 1,
			ArcGIS_World_Topo_Map = 59,
			OviTerrainMap = 31,
			ArcGIS_World_Terrain_Base_Map = 58,
			ArcGIS_World_Shaded_Relief_Map = 56,
			OpenCycleMap = 3,
			OpenSeaMapHybrid = 6,
			OviMap = 28,
			OviSatelliteMap = 29,
			OviHybridMap = 30,
			ArcGIS_Imagery_World_2D_Map = 51,
			ArcGIS_StreetMap_World_2D_Map = 53,
			ArcGIS_Topo_US_2D_Map = 54,
			ArcGIS_World_Street_Map = 57,
			BingMapOld = 8,
			GoogleMap = 14,
			GoogleSatelliteMap = 15,
			GoogleHybridMap = 16,
			GoogleTerrainMap = 17,
			GoogleSatellite = 19,
			None = 0
		}
		static MapType mapType = MapType.BingHybridMap;

		public enum Neighbourhood
		{
			Moore = 0,
			VonNeumann = 1
		}
		static Neighbourhood neighbourhood = Neighbourhood.Moore;

		enum Direction
		{
			Across,
			Down
		}

		enum Depth
		{
			Bit8 = 1,
			Bit16
		}
		static Depth m_Depth = Depth.Bit16;

		enum ByteOrder
		{
			Mac = 1,
			Windows
		}
		static ByteOrder m_ByteOrder = ByteOrder.Windows;

		enum SaveFormat
		{
			Triangles,
			Quads
		}
		SaveFormat saveFormat = SaveFormat.Triangles;

		enum SaveResolution
		{
			Full = 0,
			Half,
			_4th,
			_8th,
			_16th,
			_32th,
			_64th,
			_128th,
		}
		SaveResolution saveResolution = SaveResolution.Half;

		float terrainSizeX;
		float terrainSizeY;
		float terrainSizeFactor;

		string latitudeUser = "27.98582";
		string longitudeUser = "86.9236";

		int heightmapResolution = 2048;
		int heightmapResolutionSplit;

		bool showProgressData = false;
		bool showProgressGenerateASCII = false;
		bool showProgressGenerateRAW = false;

		UnityEngine.Object tileFolder;
		int tileGrid = 2;
		FileAttributes attr;

		SplatPrototype[] terrainTextures;
		float[,,] smData;
		float cellSizeX;
		float cellSizeY;
		float[] imageXOffset;
		float[] imageYOffset;

		float splatNormalizeX;
		float splatNormalizeY;

		//string directoryPathImagery;

		int downloadIndexData = 0;
		int downloadIndexGenerationASCII = 0;
		int downloadIndexGenerationRAW = 0;

		GameObject[] terrainGameObjects;
		Terrain[] terrains;
		TerrainData[] data;
		Terrain baseTerrain;
		TerrainData baseData;

		int size;
		int terrainsWide;
		int terrainsLong;

		float oldWidth;
		float oldHeight;
		float oldLength;
		float newWidth;
		float newLength;
		float xPos;
		float yPos;
		float zPos;

		int newHeightMapResolution;
		int newEvenHeightMapResolution;
		int newDetailResolution;
		int newAlphaMapResolution;
		int newBaseMapResolution;

		float grassStrength;
		float grassAmount;
		float grassSpeed;
		UnityEngine.Color grassTint;

		int arrayPos;

		float treeDistance;
		float treeBillboardDistance;
		float treeCrossFadeLength;
		int treeMaximumFullLODCount;
		float detailObjectDistance;
		float detailObjectDensity;
		float heightmapPixelError;
		int heightmapMaximumLOD;
		float basemapDistance;
		int lightmapIndex;
		bool castShadows;
		Material materialTemplate;
		TerrainRenderFlags editorRenderFlags;

		int totalTerrains;
		int totalTerrainsNew;
		string splitDate;
		string splitDirectoryPath;
		GameObject terrainsParent;
		GameObject splittedTerrains;
		int terrainChunks = 0;
		List<Terrain> croppedTerrains;

		List<SplatPrototype[]> terrainTexturesSplitted;
		int directoryCount;
		int imagesPerTerrain;
		bool multipleTerrainsTiling;
		string[] imageFiles;
		string str1 = "N/A";
		string str2 = "N/A";

		UnityEngine.Object[] terraUnityImages;
		Texture2D logo;
		Texture2D terrainButton;

		bool needsResampling = false;
		int croppedResolutionHeightmap;
		float croppedResolutionBase;
		float[,] resampledHeights;
		Rect rectToggle;


		#region multithreading variables

		int maxThreads = 8;
		private int numThreads;
		private int _count;

		private bool m_HasLoaded = false;

		private List<Action> _actions = new List<Action>();
		private List<DelayedQueueItem> _delayed = new List<DelayedQueueItem>();

		private List<DelayedQueueItem> _currentDelayed = new List<DelayedQueueItem>();
		private List<Action> _currentActions = new List<Action>();

		public struct DelayedQueueItem
		{
			public float time;
			public Action action;
		}

		#endregion

		string latitudeMouse;
		string longitudeMouse;
		static int zoomLevel = 15;
		public static float areaSizeLat = 16f;
		public static float areaSizeLon = 16f;

		float scaleFactor;
		int resolutionFinal;
		float extents;
		int smoothIterationsProgress = 1;
		int smoothIterations = 1;
		int smoothIterationsSplitter = 1;
		int smoothIterationsOfflineData = 1;
		int smoothIterationsDataTile = 1;
		int smoothIterationsResample = 1;
		float smoothBlend = 0.8f;
		float smoothBlendSplitter = 0.8f;
		float smoothBlendOfflineData = 0.8f;

		float smoothIterationProgress;
		float smoothProgress;
		bool showProgressSmoothen = false;
		int smoothStepIndex = 0;
		bool showProgressSmoothenOperation = false;
		int smoothIndex = 0;

		float stitchProgress;
		bool showProgressStitch = false;
		int stitchIndex = 0;

		int smoothBlendIndex = 0;
		int smoothBlendIndexSplitter = 0;
		int smoothBlendIndexOfflineData = 0;
		string[] smoothBlendMode = new string[] { "OFF", "ON" };

		int tiffWidth;
		int tiffLength;
		float[,] tiffData;
		float[,] tiffDataASCII;
		float[,] asciiData;
		float[,] rawData;
		float highestPoint;
		float lowestPoint;
		float initialTerrainWidth;
		float initialTerrainLength;

		double cellSize;

		float[,] heightMapSmoothed;

		int offlineDataIndex = 0;
		int currentOfflineIndex;
		string[] offlineDataMode = new string[] { "ASCII", "RAW", "TIFF" };
		UnityEngine.Object asciiDataFile;
		UnityEngine.Object rawDataFile;
		UnityEngine.Object tiffDataFile;
		UnityEngine.Object coordinatesXMLFile;

		int splitSizeTerrainSplitter;

		float terrainSizeNewX = 16000;
		float terrainSizeNewY = 4000;
		float terrainSizeNewZ = 16000;
		bool constrainedAspect = true;
		float pixelError = 5f;
		int splitSizeNew;
		int splitSizeFinal;
		bool newTerrainGeneration = true;

		int nCols;
		int nRows;
		double xllCorner;
		double yllCorner;
		double cellSizeASCII;

		int chunkImageResolution;

		string asciiPath;
		string rawPath;
		string tiffPath;

		int heightmapResX;
		int heightmapResY;
		int heightmapResFinalX;
		int heightmapResFinalY;
		int heightmapResXAll;
		int heightmapResYAll;
		int heightmapResFinalXAll;
		int heightmapResFinalYAll;

		float[,] finalHeights;

		int m_Height = 1;
		int m_Width = 1;

		static float everestPeak = 19882.0f; // 8848m (Everest) + -11034m (Mariana Trench) = 19882

		float terrainEverestDiffer;
		float currentHeight;
		List<float> topCorner;
		List<float> bottomCorner;
		List<float> leftCorner;
		List<float> rightCorner;

		float progressDATA;
		float progressGenerateASCII;
		float progressGenerateRAW;

		string corePath;
		string downloadsPath;
		string presetsPath;
		string downloadDateElevation;
		string unavailableTerrainStr = "No Terrains Selected.\n\nSelect Terrain(s) From The Scene Hierarchy Or Generate New Terrains First.";

		float[] initialHeightsValue;
		bool heightsAnalyzed = false;

		int terrainResolutionTotal;
		int terrainResolutionChunk;
		int textureResolutionTotal;
		int textureResolutionChunk;

		int terrainResolutionDownloading;
		string dataResamplingWarning = "NON POWER OF 2 GRID. CAUSES DATA RESAMPLING & QUALITY LOSS";

		List<string> failedTerrainNames;
		double noData;

		bool showAvailableDataSection = true;
		bool showNewTerrainSection = true;
		bool showHeightmapResizerSection = true;
		bool showRaiseLowerSection = true;
		bool showNeighboringSection = true;
		bool showSplitterSection = true;
		bool showDataSplitterSection = true;
		bool showSmoothenSection = true;
		bool showStitchSection = true;
		bool showExporterSection = true;
		bool showImageTilerSection = true;
		bool showTerrain2MeshSection = true;
		bool showMesh2TerrainSection = true;
		bool showCoordinatesSection = true;
		bool showImageSlicerSection = true;
		bool showImageStitcherSection = true;
		bool showSplatAlphaSection = true;

		int neighborsWide;
		int neighborsLong;

		static Vector3 terrainPos;
		string meshName;
		int vertexCounter;
		int totalCount;
		int progressUpdateInterval = 10000;
		bool isScenePlace = true;

		private int resolutionMesh = 512;
		int bottomTopRadioSelected = 0;
		static string[] bottomTopRadio = new string[] { "Bottom Up", "Top Down" };
		private float shiftHeight = 0f;
		List<MeshCollider> collider = new List<MeshCollider>();
		CleanUp cleanUp;
		private GameObject meshObject;

		string terrainGenerationDate;
		string meshGenerationDate;
		string generatedTerrains;
		string generatedMeshes;
		string dataFolder;

		string[] exportFormat = new string[] { "ASCII", "RAW" };
		int exportFormatIndex = 1;
		string exportDate;
		string exportedData;

		InteractiveMap mapWindow;
		int mapWindowIsOpen = 0;
		string mapWindowButtonStr = "\nSHOW ON MAP\n";
		int mapTypeIndex = 0;
		string[] mapTypeMode = new string[] { "GOOGLE", "BING", "OSM", "MAPQUEST", "MAPBOX", "YANDEX" };

		enum mapTypeGoogleEnum
		{
			roadmap,
			terrain,
			satellite,
			hybrid
		}
		static mapTypeGoogleEnum mapTypeGoogle = mapTypeGoogleEnum.hybrid;

		enum mapTypeBingEnum
		{
			Aerial,
			AerialWithLabels,
			Road
		}
		static mapTypeBingEnum mapTypeBing = mapTypeBingEnum.AerialWithLabels;

		enum mapTypeMapBoxEnum
		{
			Streets,
			StreetsBasic,
			StreetsSatellite,
			Light,
			Dark,
			Satellite,
			Wheatpaste,
			Comic,
			Outdoors,
			RunBikeHike,
			Pencil,
			Pirates,
			Emerald,
			HighContrast
		}
		static mapTypeMapBoxEnum mapTypeMapBox = mapTypeMapBoxEnum.Emerald;

		enum mapTypeMapQuestEnum
		{
			Map,
			Satellite,
			Hybrid
		}
		static mapTypeMapQuestEnum mapTypeMapQuest = mapTypeMapQuestEnum.Map;

		enum mapTypeYandexEnum
		{
			Map,
			Satellite,
			Geo,
			Traffic,
			MapGeo,
			MapTraffic,
			MapGeoTraffic,
			SatelliteGeo,
			SatelliteTraffic,
			SatelliteGeoTraffic
		}
		static mapTypeYandexEnum mapTypeYandex = mapTypeYandexEnum.Map;

		public enum Tiles
		{
			_1 = 1,
			_2x2 = 2,
			_4x4 = 4,
			_8x8 = 8,
			_16x16 = 16,
			_32x32 = 32,
			_64x64 = 64,
			_128x128 = 128
		}
		static Tiles tilesCount = Tiles._1;

		bool customLocation;

		List<float[,]> splitHeights;

		string dataFormat;
		string[] exportFormatData = new string[] { "ASCII", "RAW" };
		int exportFormatIndexData = 1;

		int terrainResolution;
		bool resampleUp;

		float terrainHeightMultiplier = 1f;
		float maximumHeightMultiplier = 5f;

		public enum SliceGrid
		{
			_2x2 = 2,
			_4x4 = 4,
			_8x8 = 8,
			_16x16 = 16,
			_32x32 = 32,
			_64x64 = 64,
			_128x128 = 128
		}
		static SliceGrid sliceGrid = SliceGrid._32x32;

		Texture2D slicedImage;
		int slicedImageWidth;
		int slicedImageHeight;
		int slicedImageResolution;
		int tiledImageResolution;
		string slicePath, stitchPath;
		bool customLocationSlice = true;
		bool customLocationStitch = true;
		string[] exportFormatSlice = new string[] { "JPG", "PNG" };
		int exportFormatIndexSlice = 1;
		int exportFormatIndexStitch = 0;
		byte[] slicedBytes;
		string slicedFileName;

		Texture2D splatmap;
		//float damping = 0.25f;

		string xmlPath;
		string projPath;
		int cellResolution;
		string tempPattern = "_Temp";

		#endregion

		#region methods

		public void OnEnable()
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

		public void LoadResources()
		{
			TextureImporter imageImport;
			bool forceUpdate = false;

			terraUnityImages = Resources.LoadAll("TerraUnity/Images", typeof(Texture2D));
			logo = Resources.Load("TerraUnity/Images/Logo/TerraLand-Terrain_Logo") as Texture2D;
			terrainButton = Resources.Load("TerraUnity/Images/Button/Terrain") as Texture2D;

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

				if (forceUpdate)
					AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(currentImage), ImportAssetOptions.ForceUpdate);
			}

			dataPath = UnityEngine.Application.dataPath;
			corePath = dataPath + "/TerraLand/TerraLand Core";
			downloadsPath = corePath + "/Downloads";
			presetsPath = corePath + "/Presets/Terrain";

			generatedTerrains = corePath + "/Terrain From Mesh";
			generatedMeshes = corePath + "/Mesh From Terrain";
			exportedData = corePath + "/Exported Elevation";
			slicePath = corePath + "/Sliced Images";
			stitchPath = corePath + "/Combined Images";
		}

		private void SwitchPlatform()
		{
#if UNITY_5_6_OR_NEWER
#if UNITY_2017_3_OR_NEWER
			if (UnityEngine.Application.platform == RuntimePlatform.WindowsEditor)
				EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Standalone, BuildTarget.StandaloneWindows);
			else if (UnityEngine.Application.platform == RuntimePlatform.OSXEditor)
				EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Standalone, BuildTarget.StandaloneOSX);
			else if (UnityEngine.Application.platform == RuntimePlatform.LinuxPlayer)
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

		public void OnGUI()
		{
			GUILayout.Space(10);

			GUI.backgroundColor = new UnityEngine.Color(0, 0, 0, 1.0f);

			if (GUILayout.Button(logo))
				Help.BrowseURL("http://www.terraunity.com");

			GUI.backgroundColor = new UnityEngine.Color(1, 1, 1, 1.0f);

			if (showProgressData || showProgressGenerateASCII || showProgressGenerateRAW || showProgressSmoothen || showProgressSmoothenOperation || showProgressStitch)
			{
				GUILayout.Space(10);

				EditorGUILayout.BeginVertical(GUI.skin.box, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));

				GUILayout.Space(15);

				// Data Loader Progress

				if (showProgressData)
				{
					EditorGUILayout.BeginHorizontal();
					GUILayout.FlexibleSpace();

					Rect rect = GUILayoutUtility.GetLastRect();
					rect.x = 47;
					rect.width = position.width - 100;
					rect.height = 18;

					int percentage = Mathf.RoundToInt(progressDATA * 100f);
					EditorGUI.ProgressBar(rect, progressDATA, "Loading Elevation Data\t" + percentage + "%");

					GUILayout.FlexibleSpace();
					EditorGUILayout.EndHorizontal();

					if (downloadIndexData != percentage)
					{
						Repaint();
						downloadIndexData = percentage;
					}

					GUILayout.Space(25);
				}

				if (Event.current.type == EventType.Repaint && progressDATA == 1f)
				{
					showProgressData = false;
					progressDATA = 0f;
				}

				// ASCII File Generation Progress

				if (showProgressGenerateASCII)
				{
					EditorGUILayout.BeginHorizontal();
					GUILayout.FlexibleSpace();

					Rect rect = GUILayoutUtility.GetLastRect();
					rect.x = 47;
					rect.width = position.width - 100;
					rect.height = 18;

					int percentage = Mathf.RoundToInt(progressGenerateASCII * 100f);
					EditorGUI.ProgressBar(rect, progressGenerateASCII, "Generating ASCII Grid Elevation File\t" + percentage + "%");

					GUILayout.FlexibleSpace();
					EditorGUILayout.EndHorizontal();

					if (downloadIndexGenerationASCII != percentage)
					{
						Repaint();
						downloadIndexGenerationASCII = percentage;
					}

					GUILayout.Space(25);
				}

				if (Event.current.type == EventType.Repaint && progressGenerateASCII == 1f)
				{
					showProgressGenerateASCII = false;
					progressGenerateASCII = 0f;
				}

				// RAW File Generation Progress

				if (showProgressGenerateRAW)
				{
					EditorGUILayout.BeginHorizontal();
					GUILayout.FlexibleSpace();

					Rect rect = GUILayoutUtility.GetLastRect();
					rect.x = 47;
					rect.width = position.width - 100;
					rect.height = 18;

					int percentage = Mathf.RoundToInt(progressGenerateRAW * 100f);
					EditorGUI.ProgressBar(rect, progressGenerateRAW, "Generating RAW Elevation File\t" + percentage + "%");

					GUILayout.FlexibleSpace();
					EditorGUILayout.EndHorizontal();

					if (downloadIndexGenerationRAW != percentage)
					{
						Repaint();
						downloadIndexGenerationRAW = percentage;
					}

					GUILayout.Space(25);
				}

				if (Event.current.type == EventType.Repaint && progressGenerateRAW == 1f)
				{
					showProgressGenerateRAW = false;
					progressGenerateRAW = 0f;
				}


				// Smoothen Operation Iteraion Progress

				if (showProgressSmoothen)
				{
					EditorGUILayout.BeginHorizontal();
					GUILayout.FlexibleSpace();

					Rect rect = GUILayoutUtility.GetLastRect();
					rect.x = 47;
					rect.width = position.width - 100;
					rect.height = 18;

					int percentage = (int)(smoothIterationProgress);
					EditorGUI.ProgressBar(rect, smoothIterationProgress / (float)smoothIterationsProgress, "Smoothing Step\t" + percentage + "  of  " + smoothIterationsProgress);

					GUILayout.FlexibleSpace();
					EditorGUILayout.EndHorizontal();

					if (smoothStepIndex != percentage)
					{
						Repaint();
						smoothStepIndex = percentage;
					}

					GUILayout.Space(25);
				}

				if (Event.current.type == EventType.Repaint && smoothIterationProgress == smoothIterationsProgress)
				{
					showProgressSmoothen = false;
					smoothIterationProgress = 0f;
				}


				// Smoothen Operation Iteraion Progress

				if (showProgressSmoothenOperation)
				{
					EditorGUILayout.BeginHorizontal();
					GUILayout.FlexibleSpace();

					Rect rect = GUILayoutUtility.GetLastRect();
					rect.x = 47;
					rect.width = position.width - 100;
					rect.height = 18;

					int percentage = Mathf.RoundToInt(smoothProgress * 100f);
					EditorGUI.ProgressBar(rect, smoothProgress, "Smoothing Terrain Heights\t" + percentage + "%");

					GUILayout.FlexibleSpace();
					EditorGUILayout.EndHorizontal();

					if (smoothIndex != percentage)
					{
						Repaint();
						smoothIndex = percentage;
					}

					GUILayout.Space(25);
				}

				if (Event.current.type == EventType.Repaint && Mathf.RoundToInt(smoothProgress * 100f) == 100)
				{
					showProgressSmoothenOperation = false;
					smoothProgress = 0f;
				}


				// Stitching Operation Iteraion Progress

				if (showProgressStitch)
				{
					EditorGUILayout.BeginHorizontal();
					GUILayout.FlexibleSpace();

					Rect rect = GUILayoutUtility.GetLastRect();
					rect.x = 47;
					rect.width = position.width - 100;
					rect.height = 18;

					int percentage = Mathf.RoundToInt(stitchProgress * 100f);
					EditorGUI.ProgressBar(rect, stitchProgress, "Stitching Data Tiles\t" + percentage + "%");

					GUILayout.FlexibleSpace();
					EditorGUILayout.EndHorizontal();

					if (stitchIndex != percentage)
					{
						Repaint();
						stitchIndex = percentage;
					}

					GUILayout.Space(25);
				}

				if (Event.current.type == EventType.Repaint && Mathf.RoundToInt(stitchProgress * 100f) == 100)
				{
					showProgressStitch = false;
					stitchProgress = 0f;
				}


				GUILayout.Space(10);

				EditorGUILayout.EndVertical();
			}

			GUILayout.Space(20);

			if (terrain || splittedTerrains)
				GUI.color = UnityEngine.Color.green;
			else
				GUI.color = UnityEngine.Color.red;

			EditorGUILayout.BeginHorizontal();
			GUILayout.FlexibleSpace();

			GUIStyle myStyle = new GUIStyle(GUI.skin.box);
			myStyle.fontSize = 15;
			myStyle.normal.textColor = UnityEngine.Color.black;

			Rect rectTerrains = GUILayoutUtility.GetLastRect();
			rectTerrains.x = GUILayoutUtility.GetLastRect().width - 150;
			rectTerrains.width = 300;
			rectTerrains.height = 25;

			if (splittedTerrains)
			{
				try
				{
					heightmapResolutionSplit = heightmapResolution / (int)Mathf.Sqrt((float)terrainChunks);
					splitSizeFinal = (int)Mathf.Sqrt((float)croppedTerrains.Count);
				}
				catch { }

				GUI.Box(rectTerrains, new GUIContent(terrainChunks.ToString() + "  AVAILABLE TERRAINS"), myStyle);
			}
			else if (terrain)
			{
				terrainChunks = 1;
				heightmapResolutionSplit = heightmapResolution;
				splitSizeFinal = 1;

				GUI.Box(rectTerrains, new GUIContent("1 AVAILABLE TERRAIN"), myStyle);
			}
			else
			{
				try
				{
					terrainChunks = totalTerrainsNew;
					heightmapResolutionSplit = heightmapResolution / (int)Mathf.Sqrt((float)terrainChunks);
					splitSizeFinal = (int)Mathf.Sqrt(terrainChunks);
				}
				catch { }

				if (terrainChunks == 1)
					GUI.Box(rectTerrains, new GUIContent(totalTerrainsNew + "  TERRAIN WILL BE GENERATED"), myStyle);
				else
					GUI.Box(rectTerrains, new GUIContent(totalTerrainsNew + "  TERRAINS WILL BE GENERATED"), myStyle);
			}

			GUILayout.FlexibleSpace();
			EditorGUILayout.EndHorizontal();

			GUI.color = UnityEngine.Color.white;

			GUILayout.Space(45);


			//***********************************************************************************************************************************************************************



			EditorGUILayout.BeginVertical(GUI.skin.box, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
			scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));

			CheckTerrainSizeUnits();

			GUI.backgroundColor = UnityEngine.Color.gray;
			EditorGUILayout.HelpBox("\nOFFLINE DATA\n", MessageType.None);
			GUI.backgroundColor = UnityEngine.Color.white;

			showAvailableDataSection = EditorGUILayout.Foldout(showAvailableDataSection, "");

			if (showAvailableDataSection)
			{
				GUILayout.Space(30);

				EditorGUILayout.BeginHorizontal();
				GUILayout.FlexibleSpace();
				EditorGUILayout.HelpBox("OFFLINE ELEVATION DATA", MessageType.None);
				GUILayout.FlexibleSpace();
				EditorGUILayout.EndHorizontal();

				GUILayout.Space(10);

				if (offlineDataIndex == 0 && asciiDataFile)
					GUI.backgroundColor = UnityEngine.Color.red;
				else if (offlineDataIndex == 1 && rawDataFile)
					GUI.backgroundColor = UnityEngine.Color.red;
				else if (offlineDataIndex == 2 && tiffDataFile)
					GUI.backgroundColor = UnityEngine.Color.red;

				EditorGUILayout.BeginHorizontal();
				GUILayout.FlexibleSpace();
				offlineDataIndex = GUILayout.SelectionGrid(offlineDataIndex, offlineDataMode, 3);
				GUILayout.FlexibleSpace();
				EditorGUILayout.EndHorizontal();

				GUI.backgroundColor = UnityEngine.Color.white;

				if (offlineDataIndex == 0)
				{
					GUILayout.Space(40);

					EditorGUI.BeginChangeCheck();

					EditorGUILayout.BeginHorizontal();
					GUILayout.FlexibleSpace();
					EditorGUILayout.HelpBox("ASCII", MessageType.None);
					asciiDataFile = EditorGUILayout.ObjectField(asciiDataFile, typeof(UnityEngine.Object), allowSceneObjects) as UnityEngine.Object;
					GUILayout.FlexibleSpace();
					EditorGUILayout.EndHorizontal();

					if (asciiDataFile)
					{
						asciiPath = AssetDatabase.GetAssetPath(asciiDataFile);

						if (!asciiPath.EndsWith(".asc"))
						{
							EditorUtility.DisplayDialog("UNKNOWN ASCII", "Please insert a valid Arc ASCII Grid file with format of \"asc\"", "Ok");
							asciiDataFile = null;
						}
					}

					if (EditorGUI.EndChangeCheck())
					{
						if (asciiDataFile)
						{
							GetASCIIInfo();

							xmlPath = asciiPath.Replace(".asc", ".xml");
							projPath = asciiPath.Replace(".asc", ".prj");

							if (File.Exists(xmlPath))
								ReadXMLFile(xmlPath);
							else
							{
								if (File.Exists(projPath))
								{
									GUILayout.Space(15);

									EditorGUILayout.BeginHorizontal();
									GUILayout.FlexibleSpace();
									EditorGUILayout.HelpBox("No XML Data Files Found To Get Geo-Coordinates\n\n\tArea Size Is Approximate", MessageType.Warning);
									GUILayout.FlexibleSpace();
									EditorGUILayout.EndHorizontal();

									//CoordinatesFromASCII();
								}
								else
								{
									GUILayout.Space(15);

									EditorGUILayout.BeginHorizontal();
									GUILayout.FlexibleSpace();
									EditorGUILayout.HelpBox("No Projection Files Found To Get Geo-Coordinates", MessageType.Warning);
									GUILayout.FlexibleSpace();
									EditorGUILayout.EndHorizontal();
								}
							}
						}
					}

					if (asciiDataFile)
					{
						GUILayout.Space(20);
						GUI.backgroundColor = UnityEngine.Color.gray;

						EditorGUILayout.BeginHorizontal();
						GUILayout.FlexibleSpace();
						EditorGUILayout.HelpBox("RESOLUTION:  " + nRows + " x " + nCols, MessageType.None);
						GUILayout.FlexibleSpace();
						EditorGUILayout.EndHorizontal();

						heightmapResolution = nRows;

						GUILayout.Space(5);

						EditorGUILayout.BeginHorizontal();
						GUILayout.FlexibleSpace();
						EditorGUILayout.HelpBox("PIXEL SIZE:  " + cellSizeASCII.ToString("0.00") + " Meters", MessageType.None);
						GUILayout.FlexibleSpace();
						EditorGUILayout.EndHorizontal();

						GUILayout.Space(5);

						EditorGUILayout.BeginHorizontal();
						GUILayout.FlexibleSpace();
						EditorGUILayout.HelpBox("HIGHEST ELEVATION:  " + highestPoint, MessageType.None);
						GUILayout.FlexibleSpace();
						EditorGUILayout.EndHorizontal();

						GUILayout.Space(5);

						EditorGUILayout.BeginHorizontal();
						GUILayout.FlexibleSpace();
						EditorGUILayout.HelpBox("LOWEST ELEVATION:  " + lowestPoint, MessageType.None);
						GUILayout.FlexibleSpace();
						EditorGUILayout.EndHorizontal();

						if (File.Exists(xmlPath) || File.Exists(projPath))
						{
							GUILayout.Space(5);

							EditorGUILayout.BeginHorizontal();
							GUILayout.FlexibleSpace();
							EditorGUILayout.HelpBox("AREA SIZE:  " + areaSizeLat + " x " + areaSizeLon + "  Km2", MessageType.None);
							GUILayout.FlexibleSpace();
							EditorGUILayout.EndHorizontal();

							DisplayMap();
						}

						GUI.backgroundColor = UnityEngine.Color.white;
					}
				}
				else if (offlineDataIndex == 1)
				{
					GUILayout.Space(40);

					EditorGUI.BeginChangeCheck();

					EditorGUILayout.BeginHorizontal();
					GUILayout.FlexibleSpace();
					EditorGUILayout.HelpBox("RAW", MessageType.None);
					rawDataFile = EditorGUILayout.ObjectField(rawDataFile, typeof(UnityEngine.Object), allowSceneObjects) as UnityEngine.Object;
					GUILayout.FlexibleSpace();
					EditorGUILayout.EndHorizontal();

					if (rawDataFile)
					{
						rawPath = AssetDatabase.GetAssetPath(rawDataFile);

						if (!rawPath.EndsWith(".raw"))
						{
							EditorUtility.DisplayDialog("UNKNOWN RAW", "Please insert a valid RAW file with format of \"raw\"", "Ok");
							rawDataFile = null;
						}
					}

					if (EditorGUI.EndChangeCheck())
					{
						if (rawDataFile)
						{
							GetRAWInfo();

							xmlPath = rawPath.Replace(".raw", ".xml");

							if (File.Exists(xmlPath))
								ReadXMLFile(xmlPath);
							else
							{
								GUILayout.Space(15);

								EditorGUILayout.BeginHorizontal();
								GUILayout.FlexibleSpace();
								EditorGUILayout.HelpBox("No XML Data Files Found To Get Geo-Coordinates\n\n\tUse ASCII Grid File Instead", MessageType.Warning);
								GUILayout.FlexibleSpace();
								EditorGUILayout.EndHorizontal();
							}
						}
					}

					if (rawDataFile)
					{
						GUILayout.Space(20);
						GUI.backgroundColor = UnityEngine.Color.gray;

						EditorGUILayout.BeginHorizontal();
						GUILayout.FlexibleSpace();
						EditorGUILayout.HelpBox("RESOLUTION:  " + m_Width + " x " + m_Height, MessageType.None);
						GUILayout.FlexibleSpace();
						EditorGUILayout.EndHorizontal();

						heightmapResolution = m_Width;

						GUILayout.Space(5);

						EditorGUILayout.BeginHorizontal();
						GUILayout.FlexibleSpace();
						EditorGUILayout.HelpBox("BYTE ORDER:  " + m_ByteOrder + "  -  " + "BIT DEPTH:  " + m_Depth, MessageType.None);
						GUILayout.FlexibleSpace();
						EditorGUILayout.EndHorizontal();

						GUILayout.Space(5);

						EditorGUILayout.BeginHorizontal();
						GUILayout.FlexibleSpace();
						EditorGUILayout.HelpBox("HIGHEST ELEVATION:  " + highestPoint, MessageType.None);
						GUILayout.FlexibleSpace();
						EditorGUILayout.EndHorizontal();

						GUILayout.Space(5);

						EditorGUILayout.BeginHorizontal();
						GUILayout.FlexibleSpace();
						EditorGUILayout.HelpBox("LOWEST ELEVATION:  " + lowestPoint, MessageType.None);
						GUILayout.FlexibleSpace();
						EditorGUILayout.EndHorizontal();

						if (File.Exists(xmlPath))
						{
							GUILayout.Space(5);

							EditorGUILayout.BeginHorizontal();
							GUILayout.FlexibleSpace();
							EditorGUILayout.HelpBox("AREA SIZE:  " + areaSizeLat + " x " + areaSizeLon + "  Km2", MessageType.None);
							GUILayout.FlexibleSpace();
							EditorGUILayout.EndHorizontal();

							DisplayMap();
						}

						GUI.backgroundColor = UnityEngine.Color.white;
					}
				}
				else if (offlineDataIndex == 2)
				{
					GUILayout.Space(40);

					EditorGUI.BeginChangeCheck();

					EditorGUILayout.BeginHorizontal();
					GUILayout.FlexibleSpace();
					EditorGUILayout.HelpBox("TIFF", MessageType.None);
					tiffDataFile = EditorGUILayout.ObjectField(tiffDataFile, typeof(UnityEngine.Object), allowSceneObjects) as UnityEngine.Object;
					GUILayout.FlexibleSpace();
					EditorGUILayout.EndHorizontal();

					if (tiffDataFile)
					{
						tiffPath = AssetDatabase.GetAssetPath(tiffDataFile);

						if (!tiffPath.EndsWith(".tif"))
						{
							EditorUtility.DisplayDialog("UNKNOWN TIFF", "Please insert a valid TIFF file with format of \"tif\"", "Ok");
							tiffDataFile = null;
						}
					}

					if (EditorGUI.EndChangeCheck())
					{
						if (tiffDataFile)
						{
							GetTIFFInfo();

							xmlPath = tiffPath.Replace(".tif", ".xml");

							if (File.Exists(xmlPath))
								ReadXMLFile(xmlPath);
							else
							{
								GUILayout.Space(15);

								EditorGUILayout.BeginHorizontal();
								GUILayout.FlexibleSpace();
								EditorGUILayout.HelpBox("No XML Data Files Found To Get Geo-Coordinates\n\n\tUse ASCII Grid File Instead", MessageType.Warning);
								GUILayout.FlexibleSpace();
								EditorGUILayout.EndHorizontal();
							}
						}
					}

					if (tiffDataFile)
					{
						GUILayout.Space(20);
						GUI.backgroundColor = UnityEngine.Color.gray;

						EditorGUILayout.BeginHorizontal();
						GUILayout.FlexibleSpace();
						EditorGUILayout.HelpBox("RESOLUTION:  " + tiffWidth + " x " + tiffLength, MessageType.None);
						GUILayout.FlexibleSpace();
						EditorGUILayout.EndHorizontal();

						heightmapResolution = tiffWidth;

						GUILayout.Space(5);

						EditorGUILayout.BeginHorizontal();
						GUILayout.FlexibleSpace();
						EditorGUILayout.HelpBox("HIGHEST ELEVATION:  " + highestPoint, MessageType.None);
						GUILayout.FlexibleSpace();
						EditorGUILayout.EndHorizontal();

						GUILayout.Space(5);

						EditorGUILayout.BeginHorizontal();
						GUILayout.FlexibleSpace();
						EditorGUILayout.HelpBox("LOWEST ELEVATION:  " + lowestPoint, MessageType.None);
						GUILayout.FlexibleSpace();
						EditorGUILayout.EndHorizontal();

						if (File.Exists(xmlPath))
						{
							GUILayout.Space(5);

							EditorGUILayout.BeginHorizontal();
							GUILayout.FlexibleSpace();
							EditorGUILayout.HelpBox("AREA SIZE:  " + areaSizeLat + " x " + areaSizeLon + "  Km2", MessageType.None);
							GUILayout.FlexibleSpace();
							EditorGUILayout.EndHorizontal();

							DisplayMap();
						}

						GUI.backgroundColor = UnityEngine.Color.white;
					}
				}

				GUILayout.Space(40);

				EditorGUILayout.BeginHorizontal();
				GUILayout.FlexibleSpace();
				EditorGUILayout.HelpBox("SMOOTH STEPS", MessageType.None);
				smoothIterationsOfflineData = EditorGUILayout.IntSlider(smoothIterationsOfflineData, 0, 10);
				GUILayout.FlexibleSpace();
				EditorGUILayout.EndHorizontal();

				GUILayout.Space(20);

				EditorGUILayout.BeginHorizontal();
				GUILayout.FlexibleSpace();
				EditorGUILayout.HelpBox("NEW & OLD BLENDING", MessageType.None, true);
				GUILayout.FlexibleSpace();
				EditorGUILayout.EndHorizontal();

				GUILayout.Space(10);

				EditorGUILayout.BeginHorizontal();
				GUILayout.FlexibleSpace();
				smoothBlendIndexOfflineData = GUILayout.SelectionGrid(smoothBlendIndexOfflineData, smoothBlendMode, 2);
				GUILayout.FlexibleSpace();
				EditorGUILayout.EndHorizontal();

				if (smoothBlendIndexOfflineData == 1)
				{
					GUILayout.Space(10);

					EditorGUILayout.BeginHorizontal();
					GUILayout.FlexibleSpace();
					EditorGUILayout.HelpBox("BLENDING", MessageType.None);
					smoothBlendOfflineData = EditorGUILayout.Slider(smoothBlendOfflineData, 0f, 1f);
					GUILayout.FlexibleSpace();
					EditorGUILayout.EndHorizontal();
				}

				GUILayout.Space(60);

				EditorGUI.BeginChangeCheck();

				EditorGUILayout.BeginHorizontal();
				GUILayout.FlexibleSpace();
				EditorGUILayout.HelpBox("SINGLE TERRAIN", MessageType.None);
				terrain = EditorGUILayout.ObjectField(terrain, typeof(Terrain), allowSceneObjects) as Terrain;
				GUILayout.FlexibleSpace();
				EditorGUILayout.EndHorizontal();

				if (EditorGUI.EndChangeCheck())
				{
					if (terrain)
					{
						heightsAnalyzed = false;
						GetInitialTerrainHeights();
					}
				}

				GUILayout.Space(10);

				EditorGUI.BeginChangeCheck();

				EditorGUILayout.BeginHorizontal();
				GUILayout.FlexibleSpace();
				EditorGUILayout.HelpBox("TERRAIN CHUNKS", MessageType.None);
				splittedTerrains = EditorGUILayout.ObjectField(splittedTerrains, typeof(GameObject), allowSceneObjects) as GameObject;
				GUILayout.FlexibleSpace();
				EditorGUILayout.EndHorizontal();

				if (EditorGUI.EndChangeCheck())
				{
					if (splittedTerrains)
					{
						CheckTerrainChunks();
						heightsAnalyzed = false;
						GetInitialTerrainHeights();

						terrainsLong = splitSizeFinal;
						terrainsWide = splitSizeFinal;
					}
				}

				GUILayout.Space(100);
			}
			else
				GUILayout.Space(15);

			GUI.backgroundColor = UnityEngine.Color.gray;
			EditorGUILayout.HelpBox("\nNEW TERRAIN SETTINGS\n", MessageType.None);
			GUI.backgroundColor = UnityEngine.Color.white;

			showNewTerrainSection = EditorGUILayout.Foldout(showNewTerrainSection, "");

			if (showNewTerrainSection)
			{
				GUILayout.Space(30);

				EditorGUILayout.BeginHorizontal();
				GUILayout.FlexibleSpace();
				EditorGUILayout.HelpBox("TILES GRID", MessageType.None);

				enumValueNew = (SizeNew)EditorGUILayout.EnumPopup(enumValueNew);
				splitSizeNew = (int)enumValueNew;
				totalTerrainsNew = Mathf.RoundToInt(Mathf.Pow(splitSizeNew, 2));

				GUI.backgroundColor = UnityEngine.Color.green;

				GUILayout.Space(5);

				if (splitSizeNew > 1)
				{
					EditorGUILayout.HelpBox(totalTerrainsNew.ToString(), MessageType.None);
					GUI.backgroundColor = UnityEngine.Color.gray;
					EditorGUILayout.HelpBox("TERRAINS", MessageType.None);
					GUI.backgroundColor = UnityEngine.Color.white;
				}
				else
				{
					GUI.backgroundColor = UnityEngine.Color.gray;
					EditorGUILayout.HelpBox("SINGLE TERRAIN", MessageType.None);
					GUI.backgroundColor = UnityEngine.Color.white;
				}

				GUILayout.FlexibleSpace();
				EditorGUILayout.EndHorizontal();

				if (!Mathf.IsPowerOfTwo(splitSizeNew))
				{
					GUILayout.Space(20);

					EditorGUILayout.BeginHorizontal();
					GUILayout.FlexibleSpace();
					EditorGUILayout.HelpBox(dataResamplingWarning, MessageType.Warning);
					GUILayout.FlexibleSpace();
					EditorGUILayout.EndHorizontal();
				}

				GUILayout.Space(60);

				EditorGUILayout.BeginHorizontal();
				GUILayout.FlexibleSpace();
				EditorGUILayout.HelpBox("AREA SIZE UNITS", MessageType.None);
				GUILayout.FlexibleSpace();
				EditorGUILayout.EndHorizontal();

				GUILayout.Space(20);

				EditorGUILayout.BeginHorizontal();
				GUILayout.FlexibleSpace();

				EditorGUILayout.HelpBox("X", MessageType.None);
				terrainSizeNewX = EditorGUILayout.FloatField(terrainSizeNewX);

				GUILayout.Space(20);

				EditorGUILayout.HelpBox("Y", MessageType.None);

				if (constrainedAspect)
				{
					terrainSizeFactor = areaSizeLat / areaSizeLon;
					terrainSizeNewZ = terrainSizeNewX * terrainSizeFactor;
					EditorGUILayout.FloatField(terrainSizeNewZ);
				}
				else
					terrainSizeNewZ = EditorGUILayout.FloatField(terrainSizeNewZ);

				GUILayout.FlexibleSpace();
				EditorGUILayout.EndHorizontal();

				GUILayout.Space(10);

				GUI.backgroundColor = UnityEngine.Color.clear;
				EditorGUILayout.BeginHorizontal();
				GUILayout.FlexibleSpace();
				EditorGUILayout.HelpBox("CONSTRAIN ASPECT RATIO", MessageType.None);
				GUILayout.FlexibleSpace();
				EditorGUILayout.EndHorizontal();
				GUI.backgroundColor = UnityEngine.Color.white;

				rectToggle = GUILayoutUtility.GetLastRect();
				rectToggle.x = (rectToggle.width / 2f) + 85f;

				constrainedAspect = EditorGUI.Toggle(rectToggle, constrainedAspect);

				GUILayout.Space(15);

				EditorGUILayout.BeginHorizontal();
				GUILayout.FlexibleSpace();
				if (GUILayout.Button("SET UNITS TO 1 METER"))
				{
					SetUnitsTo1Meter();
				}
				GUILayout.FlexibleSpace();
				EditorGUILayout.EndHorizontal();

				GUILayout.Space(40);

				float unit2Meters = (((areaSizeLat * 1000f) / terrainSizeNewZ) + ((areaSizeLon * 1000f) / terrainSizeNewX)) / 2f;
				string unitStr = "";
				string meterStr = "";
				string terrainNO = "";

				if (constrainedAspect)
					unitStr = "Each Unit Is  ";
				else
					unitStr = "Each Unit Is .approx  ";

				if (unit2Meters > 1)
					meterStr = "  Meters ";
				else
					meterStr = "  Meter ";

				if (totalTerrainsNew > 1)
					terrainNO = "Each Terrain Is  ";
				else
					terrainNO = "Terrain Is  ";

				float newTerrainSizeX = areaSizeLon / (float)splitSizeNew;
				float newTerrainSizeY = areaSizeLat / (float)splitSizeNew;

				EditorGUILayout.BeginHorizontal();
				GUILayout.FlexibleSpace();
				EditorGUILayout.HelpBox(unitStr + unit2Meters + meterStr + "\n\n" + terrainNO + newTerrainSizeX + " x " + newTerrainSizeY + "  KM", MessageType.Info);
				GUILayout.FlexibleSpace();
				EditorGUILayout.EndHorizontal();

				GUILayout.Space(60);

				EditorGUILayout.BeginHorizontal();
				GUILayout.FlexibleSpace();
				EditorGUILayout.HelpBox("PIXEL ERROR QUALITY", MessageType.None);
				pixelError = EditorGUILayout.Slider(pixelError, 1f, 200f);
				GUILayout.FlexibleSpace();
				EditorGUILayout.EndHorizontal();

				GUILayout.Space(100);
			}
			else
			{
				GUILayout.Space(15);

				if (splitSizeNew == 0)
					splitSizeNew = 1;

				if (totalTerrainsNew == 0)
					totalTerrainsNew = 1;

				if (terrainSizeNewX == 0)
					terrainSizeNewX = 16000f;

				if (terrainSizeNewZ == 0)
					terrainSizeNewZ = 16000f;
			}

			GUI.backgroundColor = UnityEngine.Color.gray;
			EditorGUILayout.HelpBox("\nHEIGHTMAP RESIZER\n", MessageType.None);
			GUI.backgroundColor = UnityEngine.Color.white;

			showHeightmapResizerSection = EditorGUILayout.Foldout(showHeightmapResizerSection, "");

			if (showHeightmapResizerSection)
			{
				GUILayout.Space(30);

				if (splittedTerrains || terrain)
				{
					EditorGUILayout.BeginHorizontal();
					GUILayout.FlexibleSpace();
					if (GUILayout.Button("<<"))
					{
						resampleUp = false;
						CheckHeightmapResolutionResample();
					}

					GUILayout.Space(10);

					if (GUILayout.Button(">>"))
					{
						resampleUp = true;
						CheckHeightmapResolutionResample();
					}
					GUILayout.FlexibleSpace();
					EditorGUILayout.EndHorizontal();

					GUILayout.Space(10);

					if (splittedTerrains)
						terrainResolution = croppedTerrains[0].terrainData.heightmapResolution * splitSizeFinal;
					else if (terrain)
						terrainResolution = terrain.terrainData.heightmapResolution;

					EditorGUILayout.BeginHorizontal();
					GUILayout.FlexibleSpace();

					if (splittedTerrains)
						EditorGUILayout.HelpBox("ENTIRE RESOLUTION", MessageType.None);
					else if (terrain)
						EditorGUILayout.HelpBox("RESOLUTION", MessageType.None);

					terrainResolution = EditorGUILayout.IntSlider(Mathf.ClosestPowerOfTwo(terrainResolution), 32, 8192);
					GUILayout.FlexibleSpace();
					EditorGUILayout.EndHorizontal();

					GUILayout.Space(10);

					GUI.color = UnityEngine.Color.green;
					EditorGUILayout.BeginHorizontal();
					GUILayout.FlexibleSpace();

					myStyle = new GUIStyle(GUI.skin.box);
					myStyle.fontSize = 20;
					myStyle.normal.textColor = UnityEngine.Color.black;

					rectToggle = GUILayoutUtility.GetLastRect();
					rectToggle.x = GUILayoutUtility.GetLastRect().width - 50;
					rectToggle.width = 100;
					rectToggle.height = 30;

					GUI.Box(rectToggle, new GUIContent(terrainResolution.ToString()), myStyle);

					GUILayout.FlexibleSpace();
					EditorGUILayout.EndHorizontal();
					GUI.color = UnityEngine.Color.white;

					GUILayout.Space(80);

					EditorGUILayout.BeginHorizontal();
					GUILayout.FlexibleSpace();
					EditorGUILayout.HelpBox("UPSAMPLING SMOOTH STEPS", MessageType.None);
					smoothIterationsResample = EditorGUILayout.IntSlider(smoothIterationsResample, 0, 10);
					GUILayout.FlexibleSpace();
					EditorGUILayout.EndHorizontal();
				}
				else
				{
					EditorGUILayout.BeginHorizontal();
					GUILayout.FlexibleSpace();
					EditorGUILayout.HelpBox("INSERT TERRAIN'S PARENT OR A SINGLE TERRAIN IN THE OFFLINE DATA SECTION TO CONTINUE", MessageType.Warning);
					GUILayout.FlexibleSpace();
					EditorGUILayout.EndHorizontal();
				}

				GUILayout.Space(100);
			}
			else
				GUILayout.Space(15);

			GUI.backgroundColor = UnityEngine.Color.gray;
			EditorGUILayout.HelpBox("\nRAISE/LOWER TERRAIN\n", MessageType.None);
			GUI.backgroundColor = UnityEngine.Color.white;

			showRaiseLowerSection = EditorGUILayout.Foldout(showRaiseLowerSection, "");

			if (showRaiseLowerSection)
			{
				GUILayout.Space(30);

				if (splittedTerrains || terrain)
				{
					if (splittedTerrains)
					{
						GUI.backgroundColor = UnityEngine.Color.red;
						EditorGUILayout.BeginHorizontal();
						GUILayout.FlexibleSpace();
						EditorGUILayout.HelpBox("ELEVATION EXAGGERATION", MessageType.None);
						GUILayout.FlexibleSpace();
						EditorGUILayout.EndHorizontal();
						GUI.backgroundColor = UnityEngine.Color.white;

						GUILayout.Space(10);

						EditorGUILayout.BeginHorizontal();
						GUILayout.FlexibleSpace();
						EditorGUILayout.HelpBox("X", MessageType.None);
						terrainHeightMultiplier = EditorGUILayout.Slider(terrainHeightMultiplier, 0.01f, maximumHeightMultiplier);
						GUILayout.FlexibleSpace();
						EditorGUILayout.EndHorizontal();

						CheckTerrainChunks();

						//int totalChunksResolution = croppedTerrains[0].terrainData.heightmapResolution * (int)Mathf.Sqrt(terrainChunks);

						//if (totalChunksResolution < 2048)
						//{
						//    if (Event.current.type == EventType.Repaint)
						//    {
						//        if (GUILayoutUtility.GetLastRect().Contains(Event.current.mousePosition))
						//        {
						//            if (!heightsAnalyzed)
						//                GetInitialTerrainHeights();

						//            for (int i = 0; i < croppedTerrains.Count; i++)
						//                croppedTerrains[i].terrainData.size = new Vector3(croppedTerrains[i].terrainData.size.x,
						//                                                                  initialHeightsValue[i] * terrainHeightMultiplier,
						//                                                                  croppedTerrains[i].terrainData.size.z
						//                                                                  );
						//        }
						//    }
						//}
						//else
						//{
						GUILayout.Space(15);

						GUI.backgroundColor = UnityEngine.Color.gray;
						EditorGUILayout.BeginHorizontal();
						GUILayout.FlexibleSpace();

						if (GUILayout.Button("\nSET HEIGHTS\n"))
						{
							if (!heightsAnalyzed)
								GetInitialTerrainHeights();

							for (int i = 0; i < croppedTerrains.Count; i++)
							{
								croppedTerrains[i].terrainData.size = new Vector3
									(
										croppedTerrains[i].terrainData.size.x,
										initialHeightsValue[i] * terrainHeightMultiplier,
										croppedTerrains[i].terrainData.size.z
									);
							}
						}

						GUILayout.FlexibleSpace();
						EditorGUILayout.EndHorizontal();
						GUI.backgroundColor = UnityEngine.Color.white;
						//}
					}
					else if (terrain)
					{
						GUI.backgroundColor = UnityEngine.Color.red;
						EditorGUILayout.BeginHorizontal();
						GUILayout.FlexibleSpace();
						EditorGUILayout.HelpBox("ELEVATION  EXAGGERATION", MessageType.None);
						GUILayout.FlexibleSpace();
						EditorGUILayout.EndHorizontal();
						GUI.backgroundColor = UnityEngine.Color.white;

						GUILayout.Space(10);

						EditorGUILayout.BeginHorizontal();
						GUILayout.FlexibleSpace();
						EditorGUILayout.HelpBox("X", MessageType.None);
						terrainHeightMultiplier = EditorGUILayout.Slider(terrainHeightMultiplier, 0.01f, maximumHeightMultiplier);
						GUILayout.FlexibleSpace();
						EditorGUILayout.EndHorizontal();

						//if (terrain.terrainData.heightmapResolution < 2048)
						//{
						//    if (Event.current.type == EventType.Repaint)
						//    {
						//        if (GUILayoutUtility.GetLastRect().Contains(Event.current.mousePosition))
						//        {
						//            if (!heightsAnalyzed)
						//                GetInitialTerrainHeights();

						//            terrain.terrainData.size = new Vector3(terrain.terrainData.size.x,
						//                                                   initialHeightsValue[0] * terrainHeightMultiplier,
						//                                                   terrain.terrainData.size.z
						//                                                   );
						//        }
						//    }
						//}
						//else
						//{
						GUILayout.Space(15);

						GUI.backgroundColor = UnityEngine.Color.gray;
						EditorGUILayout.BeginHorizontal();
						GUILayout.FlexibleSpace();

						if (GUILayout.Button("\nSET HEIGHTS\n"))
						{
							if (!heightsAnalyzed)
								GetInitialTerrainHeights();

							terrain.terrainData.size = new Vector3
							(
								terrain.terrainData.size.x,
								initialHeightsValue[0] * terrainHeightMultiplier,
								terrain.terrainData.size.z
							);
						}

						GUILayout.FlexibleSpace();
						EditorGUILayout.EndHorizontal();
						GUI.backgroundColor = UnityEngine.Color.white;
						//}
					}
					else
					{
						EditorGUILayout.BeginHorizontal();
						GUILayout.FlexibleSpace();
						EditorGUILayout.HelpBox("NO TERRAINS AVAILABLE", MessageType.Warning);
						GUILayout.FlexibleSpace();
						EditorGUILayout.EndHorizontal();
					}
				}
				else
				{
					EditorGUILayout.BeginHorizontal();
					GUILayout.FlexibleSpace();
					EditorGUILayout.HelpBox("INSERT TERRAIN(S) IN THE OFFLINE DATA SECTION TO CONTINUE", MessageType.Warning);
					GUILayout.FlexibleSpace();
					EditorGUILayout.EndHorizontal();
				}

				GUILayout.Space(100);
			}
			else
				GUILayout.Space(15);

			GUI.backgroundColor = UnityEngine.Color.gray;
			EditorGUILayout.HelpBox("\nTERRAIN NEIGHBORS\n", MessageType.None);
			GUI.backgroundColor = UnityEngine.Color.white;

			showNeighboringSection = EditorGUILayout.Foldout(showNeighboringSection, "");

			if (showNeighboringSection)
			{
				GUILayout.Space(30);

				EditorGUILayout.BeginHorizontal();
				GUILayout.FlexibleSpace();
				neighborsWide = EditorGUILayout.IntField("TERRAINS ROW COUNT", neighborsWide);
				GUILayout.FlexibleSpace();
				EditorGUILayout.EndHorizontal();

				GUILayout.Space(10);

				EditorGUILayout.BeginHorizontal();
				GUILayout.FlexibleSpace();
				neighborsLong = EditorGUILayout.IntField("TERRAINS CLMN COUNT", neighborsLong);
				GUILayout.FlexibleSpace();
				EditorGUILayout.EndHorizontal();

				GUILayout.Space(20);

				if (neighborsWide < 2)
					neighborsWide = 2;

				if (neighborsLong < 2)
					neighborsLong = 2;

				GUI.backgroundColor = UnityEngine.Color.gray;
				EditorGUILayout.BeginHorizontal();
				GUILayout.FlexibleSpace();
				if (GUILayout.Button("\nSET TERRAIN NEIGHBORS\n"))
				{
					if (splittedTerrains)
					{
						CheckTerrainChunks();

						if (terrainChunks != (neighborsWide * neighborsLong))
						{
							EditorUtility.DisplayDialog("INCORRECT NUMBERS", "You Have Inserted Incorrect Numbers For Row and Columns.", "Ok");
							return;
						}

						ManageNeighborings();
					}
					else if (terrain)
					{
						ManageNeighborings();
					}
					else
					{
						EditorUtility.DisplayDialog("UNAVAILABLE TERRAIN", unavailableTerrainStr, "Ok");
						return;
					}
				}
				GUILayout.FlexibleSpace();
				EditorGUILayout.EndHorizontal();
				GUI.backgroundColor = UnityEngine.Color.white;

				GUILayout.Space(100);
			}
			else
				GUILayout.Space(15);

			GUI.backgroundColor = UnityEngine.Color.gray;
			EditorGUILayout.HelpBox("\nTERRAIN SPLITTER\n", MessageType.None);
			GUI.backgroundColor = UnityEngine.Color.white;

			showSplitterSection = EditorGUILayout.Foldout(showSplitterSection, "");

			if (showSplitterSection)
			{
				GUILayout.Space(30);

				EditorGUILayout.BeginHorizontal();
				GUILayout.FlexibleSpace();
				EditorGUILayout.HelpBox("TILES GRID", MessageType.None);

				enumValue = (Size)EditorGUILayout.EnumPopup(enumValue);
				splitSizeTerrainSplitter = (int)enumValue;
				totalTerrains = Mathf.RoundToInt(Mathf.Pow(splitSizeTerrainSplitter, 2));

				GUI.backgroundColor = UnityEngine.Color.green;
				EditorGUILayout.HelpBox(totalTerrains.ToString(), MessageType.None);
				GUI.backgroundColor = UnityEngine.Color.white;
				EditorGUILayout.HelpBox("TERRAINS", MessageType.None);

				GUILayout.FlexibleSpace();
				EditorGUILayout.EndHorizontal();

				if (!Mathf.IsPowerOfTwo(splitSizeTerrainSplitter))
				{
					needsResampling = true;

					GUILayout.Space(20);

					EditorGUILayout.BeginHorizontal();
					GUILayout.FlexibleSpace();
					EditorGUILayout.HelpBox(dataResamplingWarning, MessageType.Warning);
					GUILayout.FlexibleSpace();
					EditorGUILayout.EndHorizontal();

					GUILayout.Space(40);

					EditorGUILayout.BeginHorizontal();
					GUILayout.FlexibleSpace();
					EditorGUILayout.HelpBox("SMOOTH STEPS", MessageType.None);
					smoothIterationsSplitter = EditorGUILayout.IntSlider(smoothIterationsSplitter, 0, 10);
					GUILayout.FlexibleSpace();
					EditorGUILayout.EndHorizontal();

					GUILayout.Space(20);

					EditorGUILayout.BeginHorizontal();
					GUILayout.FlexibleSpace();
					EditorGUILayout.HelpBox("NEW & OLD BLENDING", MessageType.None, true);
					GUILayout.FlexibleSpace();
					EditorGUILayout.EndHorizontal();

					GUILayout.Space(10);

					EditorGUILayout.BeginHorizontal();
					GUILayout.FlexibleSpace();
					smoothBlendIndexSplitter = GUILayout.SelectionGrid(smoothBlendIndexSplitter, smoothBlendMode, 2);
					GUILayout.FlexibleSpace();
					EditorGUILayout.EndHorizontal();

					if (smoothBlendIndexSplitter == 1)
					{
						GUILayout.Space(10);

						EditorGUILayout.BeginHorizontal();
						GUILayout.FlexibleSpace();
						EditorGUILayout.HelpBox("BLENDING", MessageType.None);
						smoothBlendSplitter = EditorGUILayout.Slider(smoothBlendSplitter, 0f, 1f);
						GUILayout.FlexibleSpace();
						EditorGUILayout.EndHorizontal();
					}
				}
				else
					needsResampling = false;

				GUILayout.Space(60);

				GUI.backgroundColor = UnityEngine.Color.gray;
				EditorGUILayout.BeginHorizontal();
				GUILayout.FlexibleSpace();
				if (GUILayout.Button("\nGENERATE TERRAIN TILES\n"))
				{
					if (terrain)
					{
						baseTerrain = terrain;
						GetData();

						if (ErrorsPass())
						{
							splitDate = System.DateTime.Now.ToString("MM-dd-yyyy_HH-mm-ss");
							splitDirectoryPath = corePath + "/Terrain Tiles/" + splitDate;
							Directory.CreateDirectory(splitDirectoryPath);
							AssetDatabase.Refresh();

							newTerrainGeneration = false;
							CreateTerrainData();
							CreateTerrainObject();

							terrain.gameObject.SetActive(false);
							splittedTerrains = terrainsParent;

							CheckTerrainChunks();
							SetTerrainNeighbors();

							RemoveLightmapStatic(croppedTerrains);
						}
						else
							return;
					}
					else
					{
						EditorUtility.DisplayDialog("UNAVAILABLE TERRAIN", unavailableTerrainStr, "Ok");
						return;
					}
				}
				GUILayout.FlexibleSpace();
				EditorGUILayout.EndHorizontal();
				GUI.backgroundColor = UnityEngine.Color.white;

				GUILayout.Space(100);
			}
			else
				GUILayout.Space(15);

			GUI.backgroundColor = UnityEngine.Color.gray;
			EditorGUILayout.HelpBox("\nIMAGE SLICER\n", MessageType.None);
			GUI.backgroundColor = UnityEngine.Color.white;

			showImageSlicerSection = EditorGUILayout.Foldout(showImageSlicerSection, "");

			if (showImageSlicerSection)
			{
				GUILayout.Space(30);

				EditorGUILayout.BeginHorizontal();
				GUILayout.FlexibleSpace();
				EditorGUILayout.HelpBox("ORIGINAL IMAGE", MessageType.None);
				slicedImage = EditorGUILayout.ObjectField(slicedImage, typeof(Texture2D), allowSceneObjects) as Texture2D;
				GUILayout.FlexibleSpace();
				EditorGUILayout.EndHorizontal();

				GUILayout.Space(40);

				EditorGUILayout.BeginHorizontal();
				GUILayout.FlexibleSpace();
				EditorGUILayout.HelpBox("EXPORT FORMAT", MessageType.None);
				exportFormatIndexSlice = GUILayout.SelectionGrid(exportFormatIndexSlice, exportFormatSlice, 2);
				GUILayout.FlexibleSpace();
				EditorGUILayout.EndHorizontal();

				GUILayout.Space(30);

				EditorGUILayout.BeginHorizontal();
				GUILayout.FlexibleSpace();
				EditorGUILayout.HelpBox("TILES", MessageType.None);

				sliceGrid = (SliceGrid)EditorGUILayout.EnumPopup(sliceGrid);

				GUI.backgroundColor = UnityEngine.Color.green;
				EditorGUILayout.HelpBox((Mathf.Pow((int)sliceGrid, 2)).ToString(), MessageType.None);
				GUI.backgroundColor = UnityEngine.Color.white;
				EditorGUILayout.HelpBox("IMAGES", MessageType.None);

				GUILayout.FlexibleSpace();
				EditorGUILayout.EndHorizontal();

				GUILayout.Space(30);

				GUI.backgroundColor = UnityEngine.Color.clear;
				EditorGUILayout.BeginHorizontal();
				GUILayout.FlexibleSpace();
				EditorGUILayout.HelpBox("SELECT SAVE FOLDER", MessageType.None);
				GUILayout.FlexibleSpace();
				EditorGUILayout.EndHorizontal();
				GUI.backgroundColor = UnityEngine.Color.white;

				rectToggle = GUILayoutUtility.GetLastRect();
				rectToggle.x = (rectToggle.width / 2f) + 85f;

				customLocationSlice = EditorGUI.Toggle(rectToggle, customLocationSlice);

				GUILayout.Space(5);

				if (customLocationSlice)
				{
					EditorGUILayout.BeginHorizontal();
					GUILayout.FlexibleSpace();
					EditorGUILayout.HelpBox("GENERATED IMAGES WILL NOT BE IMPORTED IN PROJECT\n\nUSEFUL FOR RUNTIME STREAMING FROM LOCAL DATABASE", MessageType.Info);
					GUILayout.FlexibleSpace();
					EditorGUILayout.EndHorizontal();
				}

				GUILayout.Space(40);

				GUI.backgroundColor = UnityEngine.Color.gray;
				EditorGUILayout.BeginHorizontal();
				GUILayout.FlexibleSpace();
				if (GUILayout.Button("\nSLICE IMAGE\n"))
				{
					if (slicedImage != null)
						SliceImage();
					else
					{
						EditorUtility.DisplayDialog("IMAGE NOT AVAILABLE", "Drag & drop an image to process", "Ok");
						return;
					}
				}
				GUILayout.FlexibleSpace();
				EditorGUILayout.EndHorizontal();
				GUI.backgroundColor = UnityEngine.Color.white;

				GUILayout.Space(100);
			}
			else
				GUILayout.Space(15);

			GUI.backgroundColor = UnityEngine.Color.gray;
			EditorGUILayout.HelpBox("\nIMAGE STITCHER\n", MessageType.None);
			GUI.backgroundColor = UnityEngine.Color.white;

			showImageStitcherSection = EditorGUILayout.Foldout(showImageStitcherSection, "");

			if (showImageStitcherSection)
			{
				GUILayout.Space(30);

				EditorGUILayout.BeginHorizontal();
				GUILayout.FlexibleSpace();
				EditorGUILayout.HelpBox("TILE RESOLUTION", MessageType.None);
				cellResolution = EditorGUILayout.IntField(cellResolution);
				GUILayout.FlexibleSpace();
				EditorGUILayout.EndHorizontal();

				GUILayout.Space(40);

				EditorGUILayout.BeginHorizontal();
				GUILayout.FlexibleSpace();
				EditorGUILayout.HelpBox("EXPORT FORMAT", MessageType.None);
				exportFormatIndexStitch = GUILayout.SelectionGrid(exportFormatIndexStitch, exportFormatSlice, 2);
				GUILayout.FlexibleSpace();
				EditorGUILayout.EndHorizontal();

				GUILayout.Space(30);

				GUI.backgroundColor = UnityEngine.Color.clear;
				EditorGUILayout.BeginHorizontal();
				GUILayout.FlexibleSpace();
				EditorGUILayout.HelpBox("SELECT SAVE FOLDER", MessageType.None);
				GUILayout.FlexibleSpace();
				EditorGUILayout.EndHorizontal();
				GUI.backgroundColor = UnityEngine.Color.white;

				rectToggle = GUILayoutUtility.GetLastRect();
				rectToggle.x = (rectToggle.width / 2f) + 85f;

				customLocationStitch = EditorGUI.Toggle(rectToggle, customLocationStitch);

				GUILayout.Space(5);

				if (customLocationStitch)
				{
					EditorGUILayout.BeginHorizontal();
					GUILayout.FlexibleSpace();
					EditorGUILayout.HelpBox("GENERATED IMAGE WILL NOT BE IMPORTED IN PROJECT", MessageType.Info);
					GUILayout.FlexibleSpace();
					EditorGUILayout.EndHorizontal();
				}

				GUILayout.Space(40);

				GUI.backgroundColor = UnityEngine.Color.gray;
				EditorGUILayout.BeginHorizontal();
				GUILayout.FlexibleSpace();
				if (GUILayout.Button("\nCOMBINE IMAGES\n"))
				{
					CombineImages();
				}
				GUILayout.FlexibleSpace();
				EditorGUILayout.EndHorizontal();
				GUI.backgroundColor = UnityEngine.Color.white;

				GUILayout.Space(100);
			}
			else
				GUILayout.Space(15);

			GUI.backgroundColor = UnityEngine.Color.gray;
			EditorGUILayout.HelpBox("\nSPLATMAP ALPHA GENERATOR\n", MessageType.None);
			GUI.backgroundColor = UnityEngine.Color.white;

			showSplatAlphaSection = EditorGUILayout.Foldout(showSplatAlphaSection, "");

			if (showSplatAlphaSection)
			{
				GUILayout.Space(30);

				EditorGUILayout.BeginHorizontal();
				GUILayout.FlexibleSpace();
				EditorGUILayout.HelpBox("ORIGINAL SPLATMAP", MessageType.None);
				splatmap = EditorGUILayout.ObjectField(splatmap, typeof(Texture2D), allowSceneObjects) as Texture2D;
				GUILayout.FlexibleSpace();
				EditorGUILayout.EndHorizontal();

				GUILayout.Space(30);

				//GUI.backgroundColor = UnityEngine.Color.clear;
				//EditorGUILayout.BeginHorizontal();
				//GUILayout.FlexibleSpace();
				//EditorGUILayout.HelpBox("FROM GRAYSCALE ONLY", MessageType.None);
				//GUILayout.FlexibleSpace();
				//EditorGUILayout.EndHorizontal();
				//GUI.backgroundColor = UnityEngine.Color.white;

				//rectToggle = GUILayoutUtility.GetLastRect();
				//rectToggle.x = (rectToggle.width / 2f) + 85f;

				//EditorGUILayout.BeginHorizontal();
				//GUILayout.FlexibleSpace();
				//EditorGUILayout.HelpBox("DAMPING", MessageType.None);
				//damping = EditorGUILayout.Slider(damping, 0f, 1f);
				//GUILayout.FlexibleSpace();
				//EditorGUILayout.EndHorizontal();
				//
				//GUILayout.Space(40);

				GUI.backgroundColor = UnityEngine.Color.gray;
				EditorGUILayout.BeginHorizontal();
				GUILayout.FlexibleSpace();
				if (GUILayout.Button("\nGENERATE ALPHA CHANNEL\n"))
				{
					if (slicedImage != null)
						GenerateAlphaFromGrayscale();
					else
					{
						EditorUtility.DisplayDialog("IMAGE NOT AVAILABLE", "Drag & drop an image to process", "Ok");
						return;
					}
				}
				GUILayout.FlexibleSpace();
				EditorGUILayout.EndHorizontal();
				GUI.backgroundColor = UnityEngine.Color.white;

				GUILayout.Space(100);
			}
			else
				GUILayout.Space(15);

			GUI.backgroundColor = UnityEngine.Color.gray;
			EditorGUILayout.HelpBox("\nDATA SPLITTER & CONVERTER\n", MessageType.None);
			GUI.backgroundColor = UnityEngine.Color.white;

			showDataSplitterSection = EditorGUILayout.Foldout(showDataSplitterSection, "");

			if (showDataSplitterSection)
			{
				GUILayout.Space(30);

				EditorGUILayout.BeginHorizontal();
				GUILayout.FlexibleSpace();
				EditorGUILayout.HelpBox("EXPORT FORMAT", MessageType.None);
				exportFormatIndexData = GUILayout.SelectionGrid(exportFormatIndexData, exportFormatData, 2);
				GUILayout.FlexibleSpace();
				EditorGUILayout.EndHorizontal();

				GUILayout.Space(20);

				EditorGUILayout.BeginHorizontal();
				GUILayout.FlexibleSpace();
				EditorGUILayout.HelpBox("TILES GRID", MessageType.None);

				dataTiles = (DataTiles)EditorGUILayout.EnumPopup(dataTiles);

				GUI.backgroundColor = UnityEngine.Color.green;
				EditorGUILayout.HelpBox((Mathf.Pow((int)dataTiles, 2)).ToString(), MessageType.None);
				GUI.backgroundColor = UnityEngine.Color.white;
				EditorGUILayout.HelpBox("FILES", MessageType.None);

				GUILayout.FlexibleSpace();
				EditorGUILayout.EndHorizontal();

				GUILayout.Space(30);

				EditorGUILayout.BeginHorizontal();
				GUILayout.FlexibleSpace();
				EditorGUILayout.HelpBox("SMOOTH STEPS", MessageType.None);
				smoothIterationsDataTile = EditorGUILayout.IntSlider(smoothIterationsDataTile, 0, 20);
				GUILayout.FlexibleSpace();
				EditorGUILayout.EndHorizontal();

				GUILayout.Space(40);

				GUI.backgroundColor = UnityEngine.Color.gray;
				EditorGUILayout.BeginHorizontal();
				GUILayout.FlexibleSpace();
				if (GUILayout.Button("\nGENERATE DATA TILES\n"))
				{
					GenerateDataTiles();
				}
				GUILayout.FlexibleSpace();
				EditorGUILayout.EndHorizontal();
				GUI.backgroundColor = UnityEngine.Color.white;

				GUILayout.Space(100);
			}
			else
				GUILayout.Space(15);

			GUI.backgroundColor = UnityEngine.Color.gray;
			EditorGUILayout.HelpBox("\nSMOOTHEN TERRAIN HEIGHTS\n", MessageType.None);
			GUI.backgroundColor = UnityEngine.Color.white;

			showSmoothenSection = EditorGUILayout.Foldout(showSmoothenSection, "");

			if (showSmoothenSection)
			{
				GUILayout.Space(30);

				EditorGUILayout.BeginHorizontal();
				GUILayout.FlexibleSpace();
				EditorGUILayout.HelpBox("ITERATIONS", MessageType.None);
				smoothIterations = EditorGUILayout.IntSlider(smoothIterations, 1, 10);
				GUILayout.FlexibleSpace();
				EditorGUILayout.EndHorizontal();

				GUILayout.Space(20);

				EditorGUILayout.BeginHorizontal();
				GUILayout.FlexibleSpace();
				EditorGUILayout.HelpBox("NEW & OLD BLENDING", MessageType.None, true);
				GUILayout.FlexibleSpace();
				EditorGUILayout.EndHorizontal();

				GUILayout.Space(10);

				EditorGUILayout.BeginHorizontal();
				GUILayout.FlexibleSpace();
				smoothBlendIndex = GUILayout.SelectionGrid(smoothBlendIndex, smoothBlendMode, 2);
				GUILayout.FlexibleSpace();
				EditorGUILayout.EndHorizontal();

				if (smoothBlendIndex == 1)
				{
					GUILayout.Space(10);

					EditorGUILayout.BeginHorizontal();
					GUILayout.FlexibleSpace();
					EditorGUILayout.HelpBox("BLENDING", MessageType.None);
					smoothBlend = EditorGUILayout.Slider(smoothBlend, 0f, 1f);
					GUILayout.FlexibleSpace();
					EditorGUILayout.EndHorizontal();
				}

				GUILayout.Space(50);

				GUI.backgroundColor = UnityEngine.Color.gray;
				EditorGUILayout.BeginHorizontal();
				GUILayout.FlexibleSpace();
				if (GUILayout.Button("\nSMOOTHEN HEIGHTS\n"))
				{
					SmoothenTerrains();
				}
				GUILayout.FlexibleSpace();
				EditorGUILayout.EndHorizontal();
				GUI.backgroundColor = UnityEngine.Color.white;

				GUILayout.Space(100);
			}
			else
				GUILayout.Space(15);

			GUI.backgroundColor = UnityEngine.Color.gray;
			EditorGUILayout.HelpBox("\nSTITCH DATA TILES\n", MessageType.None);
			GUI.backgroundColor = UnityEngine.Color.white;

			showStitchSection = EditorGUILayout.Foldout(showStitchSection, "");

			if (showStitchSection)
			{
				GUILayout.Space(30);

				GUI.backgroundColor = UnityEngine.Color.gray;
				EditorGUILayout.BeginHorizontal();
				GUILayout.FlexibleSpace();
				if (GUILayout.Button("\nSTITCH TILES\n"))
				{
					StitchTiles();
				}
				GUILayout.FlexibleSpace();
				EditorGUILayout.EndHorizontal();
				GUI.backgroundColor = UnityEngine.Color.white;

				GUILayout.Space(100);
			}
			else
				GUILayout.Space(15);

			GUI.backgroundColor = UnityEngine.Color.gray;
			EditorGUILayout.HelpBox("\nEXPORT ELEVATION DATA\n", MessageType.None);
			GUI.backgroundColor = UnityEngine.Color.white;

			showExporterSection = EditorGUILayout.Foldout(showExporterSection, "");

			if (showExporterSection)
			{
				GUILayout.Space(30);

				EditorGUILayout.BeginHorizontal();
				GUILayout.FlexibleSpace();
				EditorGUILayout.HelpBox("EXPORT FORMAT", MessageType.None);
				exportFormatIndex = GUILayout.SelectionGrid(exportFormatIndex, exportFormat, 2);
				GUILayout.FlexibleSpace();
				EditorGUILayout.EndHorizontal();

				GUILayout.Space(20);

				EditorGUILayout.BeginHorizontal();
				GUILayout.FlexibleSpace();
				EditorGUILayout.HelpBox("TILES", MessageType.None);

				tilesCount = (Tiles)EditorGUILayout.EnumPopup(tilesCount);

				GUI.backgroundColor = UnityEngine.Color.green;
				EditorGUILayout.HelpBox((Mathf.Pow((int)tilesCount, 2)).ToString(), MessageType.None);
				GUI.backgroundColor = UnityEngine.Color.white;
				EditorGUILayout.HelpBox("FILES", MessageType.None);

				GUILayout.FlexibleSpace();
				EditorGUILayout.EndHorizontal();

				GUILayout.Space(20);

				GUI.backgroundColor = UnityEngine.Color.clear;
				EditorGUILayout.BeginHorizontal();
				GUILayout.FlexibleSpace();
				EditorGUILayout.HelpBox("SELECT SAVE FOLDER", MessageType.None);
				GUILayout.FlexibleSpace();
				EditorGUILayout.EndHorizontal();
				GUI.backgroundColor = UnityEngine.Color.white;

				rectToggle = GUILayoutUtility.GetLastRect();
				rectToggle.x = (rectToggle.width / 2f) + 85f;

				customLocation = EditorGUI.Toggle(rectToggle, customLocation);

				GUILayout.Space(5);

				if (customLocation)
				{
					EditorGUILayout.BeginHorizontal();
					GUILayout.FlexibleSpace();
					EditorGUILayout.HelpBox("GENERATED DATA FILES WILL NOT BE IMPORTED IN PROJECT\n\nUSEFUL FOR RUNTIME STREAMING FROM LOCAL DATABASE", MessageType.Info);
					GUILayout.FlexibleSpace();
					EditorGUILayout.EndHorizontal();
				}

				GUILayout.Space(40);

				GUI.backgroundColor = UnityEngine.Color.gray;
				EditorGUILayout.BeginHorizontal();
				GUILayout.FlexibleSpace();
				if (GUILayout.Button("\nEXPORT DATA\n"))
				{
					ExportData();
				}
				GUILayout.FlexibleSpace();
				EditorGUILayout.EndHorizontal();
				GUI.backgroundColor = UnityEngine.Color.white;

				GUILayout.Space(100);
			}
			else
				GUILayout.Space(15);

			GUI.backgroundColor = UnityEngine.Color.gray;
			EditorGUILayout.HelpBox("\nTERRAIN IMAGE TILER\n", MessageType.None);
			GUI.backgroundColor = UnityEngine.Color.white;

			showImageTilerSection = EditorGUILayout.Foldout(showImageTilerSection, "");

			if (showImageTilerSection)
			{
				GUILayout.Space(30);

				EditorGUI.BeginChangeCheck();

				EditorGUILayout.BeginHorizontal();
				GUILayout.FlexibleSpace();
				EditorGUILayout.HelpBox("TILE IMAGES FOLDER", MessageType.None);
				tileFolder = EditorGUILayout.ObjectField(tileFolder, typeof(UnityEngine.Object), allowSceneObjects) as UnityEngine.Object;
				GUILayout.FlexibleSpace();
				EditorGUILayout.EndHorizontal();

				if (EditorGUI.EndChangeCheck())
				{
					if (tileFolder)
						GetImageryFolderInfo();
				}

				if (tileFolder)
					attr = File.GetAttributes(AssetDatabase.GetAssetPath(tileFolder));

				if (tileFolder != null && (attr & FileAttributes.Directory) != FileAttributes.Directory)
				{
					EditorUtility.DisplayDialog("FOLDER NOT AVAILABLE", "Drag & drop a folder which contains previously downloaded satellite images for terrain texturing.", "Ok");
					tileFolder = null;
					return;
				}

				GUILayout.Space(30);

				if (tileFolder != null)
				{
					GUILayout.Space(10);

					EditorGUILayout.BeginHorizontal();
					GUILayout.FlexibleSpace();
					EditorGUILayout.HelpBox(str1, MessageType.None);
					GUILayout.FlexibleSpace();
					EditorGUILayout.EndHorizontal();

					EditorGUILayout.BeginHorizontal();
					GUILayout.FlexibleSpace();
					EditorGUILayout.HelpBox(str2, MessageType.None);
					GUILayout.FlexibleSpace();
					EditorGUILayout.EndHorizontal();
				}
				else
				{
					str1 = "N/A";
					str2 = "N/A";
				}

				GUILayout.Space(20);

				GUI.backgroundColor = UnityEngine.Color.gray;
				EditorGUILayout.BeginHorizontal();
				GUILayout.FlexibleSpace();
				if (GUILayout.Button("\nTERRAIN IMAGE TILER\n"))
				{
					if (tileFolder)
					{
						GetImageryFolderInfo();

						if (tileFolder)
						{
							if (terrain || splittedTerrains)
							{
								int texturesCount = 0;

#if UNITY_2018_3_OR_NEWER
								try
								{
									if (croppedTerrains[0].terrainData.terrainLayers != null && croppedTerrains[0].terrainData.terrainLayers.Length > 0)
										texturesCount = croppedTerrains[0].terrainData.terrainLayers.Length;
									else
										texturesCount = 0;
								}
								catch
								{
									texturesCount = 0;
								}
#else
                            texturesCount = croppedTerrains[0].terrainData.splatPrototypes.Length;
#endif

								if (splittedTerrains)
								{
									if (texturesCount == 1)
									{
										if (EditorUtility.DisplayDialog("TERRAIN TEXTURES", "There is a texture available on your terrain.\n\nPressing \"Continue\" will remove this texture and replace it with the downloading satellite images.", "Cancel", "Continue"))
										{
											return;
										}
									}
									else if (texturesCount > 1)
									{
										if (EditorUtility.DisplayDialog("TERRAIN TEXTURES", "There are textures available on your terrain.\n\nPressing \"Continue\" will remove them and replace it with the downloading satellite images.", "Cancel", "Continue"))
										{
											return;
										}
									}
								}
								else if (terrain)
								{
#if UNITY_2018_3_OR_NEWER
									try
									{
										if (terrain.terrainData.terrainLayers != null && terrain.terrainData.terrainLayers.Length > 0)
											texturesCount = terrain.terrainData.terrainLayers.Length;
										else
											texturesCount = 0;
									}
									catch
									{
										texturesCount = 0;
									}
#else
                texturesCount = terrain.terrainData.splatPrototypes.Length;
#endif

									if (texturesCount == 1)
									{
										if (EditorUtility.DisplayDialog("TERRAIN TEXTURES", "There is a texture available on your terrain.\n\nPressing \"Continue\" will remove this texture and replace it with the downloading satellite images.", "Cancel", "Continue"))
										{
											return;
										}
									}
									else if (texturesCount > 1)
									{
										if (EditorUtility.DisplayDialog("TERRAIN TEXTURES", "There are textures available on your terrain.\n\nPressing \"Continue\" will remove them and replace it with the downloading satellite images.", "Cancel", "Continue"))
										{
											return;
										}
									}
								}

								ImageTilerOffline(imageFiles);
							}
							else
							{
								EditorUtility.DisplayDialog("UNAVAILABLE TERRAIN", "No Terrains Selected, First drag & drop a terrain object or splitted terrains' parent object into the appropriate fields from the hierarchy panel.", "Ok");
								return;
							}
						}
					}
					else
					{
						EditorUtility.DisplayDialog("FOLDER NOT AVILABLE", "First select a folder including satellite images.\n\nDrag and drop the folder into the \"TILE IMAGES FOLDER\" field.", "Ok");
						return;
					}
				}
				GUILayout.FlexibleSpace();
				EditorGUILayout.EndHorizontal();
				GUI.backgroundColor = UnityEngine.Color.white;

				GUILayout.Space(100);
			}
			else
				GUILayout.Space(15);

			GUI.backgroundColor = UnityEngine.Color.gray;
			EditorGUILayout.HelpBox("\nTERRAIN TO MESH\n", MessageType.None);
			GUI.backgroundColor = UnityEngine.Color.white;

			showTerrain2MeshSection = EditorGUILayout.Foldout(showTerrain2MeshSection, "");

			if (showTerrain2MeshSection)
			{
				GUILayout.Space(30);

				EditorGUILayout.BeginHorizontal();
				GUILayout.FlexibleSpace();
				EditorGUILayout.HelpBox("Export Format", MessageType.None);
				saveFormat = (SaveFormat)EditorGUILayout.EnumPopup(saveFormat);
				GUILayout.FlexibleSpace();
				EditorGUILayout.EndHorizontal();

				GUILayout.Space(20);

				EditorGUILayout.BeginHorizontal();
				GUILayout.FlexibleSpace();
				EditorGUILayout.HelpBox("Resolution", MessageType.None);
				saveResolution = (SaveResolution)EditorGUILayout.EnumPopup(saveResolution);
				GUILayout.FlexibleSpace();
				EditorGUILayout.EndHorizontal();

				GUILayout.Space(20);

				GUI.backgroundColor = UnityEngine.Color.clear;
				EditorGUILayout.BeginHorizontal();
				GUILayout.FlexibleSpace();
				EditorGUILayout.HelpBox("PLACE IN THE SCENE", MessageType.None);
				GUILayout.FlexibleSpace();
				EditorGUILayout.EndHorizontal();
				GUI.backgroundColor = UnityEngine.Color.white;

				rectToggle = GUILayoutUtility.GetLastRect();
				rectToggle.x = (rectToggle.width / 2f) + 85f;

				isScenePlace = EditorGUI.Toggle(rectToggle, isScenePlace);

				if (isScenePlace)
				{
					GUILayout.Space(5);

					EditorGUILayout.BeginHorizontal();
					GUILayout.FlexibleSpace();
					EditorGUILayout.HelpBox("NEEDS MESH IMPORTING IN PROJECT", MessageType.Warning);
					GUILayout.FlexibleSpace();
					EditorGUILayout.EndHorizontal();
				}

				GUILayout.Space(40);

				GUI.backgroundColor = UnityEngine.Color.gray;
				EditorGUILayout.BeginHorizontal();
				GUILayout.FlexibleSpace();
				if (GUILayout.Button("\nCONVERT TERRAIN TO MESH\n"))
				{
					Terrain2Mesh();
				}
				GUILayout.FlexibleSpace();
				EditorGUILayout.EndHorizontal();
				GUI.backgroundColor = UnityEngine.Color.white;

				GUILayout.Space(100);
			}
			else
				GUILayout.Space(15);

			GUI.backgroundColor = UnityEngine.Color.gray;
			EditorGUILayout.HelpBox("\nMESH TO TERRAIN\n", MessageType.None);
			GUI.backgroundColor = UnityEngine.Color.white;

			showMesh2TerrainSection = EditorGUILayout.Foldout(showMesh2TerrainSection, "");

			if (showMesh2TerrainSection)
			{
				GUILayout.Space(30);

				EditorGUILayout.BeginHorizontal();
				GUILayout.FlexibleSpace();
				EditorGUILayout.HelpBox("3D MODEL", MessageType.None);
				meshObject = EditorGUILayout.ObjectField(meshObject, typeof(GameObject), allowSceneObjects) as GameObject;
				GUILayout.FlexibleSpace();
				EditorGUILayout.EndHorizontal();

				GUILayout.Space(40);

				EditorGUILayout.BeginHorizontal();
				GUILayout.FlexibleSpace();
				EditorGUILayout.HelpBox("OUTPUT RESOLUTION", MessageType.None);
				resolutionMesh = EditorGUILayout.IntSlider(Mathf.ClosestPowerOfTwo(resolutionMesh), 32, 8192);
				GUILayout.FlexibleSpace();
				EditorGUILayout.EndHorizontal();

				GUILayout.Space(20);

				EditorGUILayout.BeginHorizontal();
				GUILayout.FlexibleSpace();
				EditorGUILayout.HelpBox("HIGHTS SHIFT", MessageType.None);
				shiftHeight = EditorGUILayout.Slider(shiftHeight, -1f, 1f);
				GUILayout.FlexibleSpace();
				EditorGUILayout.EndHorizontal();

				GUILayout.Space(20);

				EditorGUILayout.BeginHorizontal();
				GUILayout.FlexibleSpace();
				EditorGUILayout.HelpBox("RAYCAST MODE", MessageType.None);
				bottomTopRadioSelected = GUILayout.SelectionGrid(bottomTopRadioSelected, bottomTopRadio, bottomTopRadio.Length, EditorStyles.radioButton);
				GUILayout.FlexibleSpace();
				EditorGUILayout.EndHorizontal();

				GUILayout.Space(50);

				GUI.backgroundColor = UnityEngine.Color.gray;
				EditorGUILayout.BeginHorizontal();
				GUILayout.FlexibleSpace();
				if (GUILayout.Button("\nCONVERT MESH TO TERRAIN\n"))
				{
					Mesh2Terrain();
				}
				GUILayout.FlexibleSpace();
				EditorGUILayout.EndHorizontal();
				GUI.backgroundColor = UnityEngine.Color.white;


				GUILayout.Space(100);
			}
			else
				GUILayout.Space(15);

			GUI.backgroundColor = UnityEngine.Color.gray;
			EditorGUILayout.HelpBox("\nGEO-COORDINATES CONVERTER\n", MessageType.None);
			GUI.backgroundColor = UnityEngine.Color.white;

			showCoordinatesSection = EditorGUILayout.Foldout(showCoordinatesSection, "");

			if (showCoordinatesSection)
			{
				GUILayout.Space(30);

				string hemisphereLat;
				string hemisphereLon;
				string hemisphereLat2;
				string hemisphereLon2;

				if (degree.ToString().StartsWith("-"))
					hemisphereLat = "S";
				else
					hemisphereLat = "N";

				if (degree2.ToString().StartsWith("-"))
					hemisphereLon = "W";
				else
					hemisphereLon = "E";

				if (degreeConverted.ToString().StartsWith("-"))
					hemisphereLat2 = "S";
				else
					hemisphereLat2 = "N";

				if (degreeConverted2.ToString().StartsWith("-"))
					hemisphereLon2 = "W";
				else
					hemisphereLon2 = "E";

				// Corrections
				if (degree >= 90)
				{
					degree = 90;
					minute = 0;
					second = 0;
				}
				else if (degree <= -90)
				{
					degree = -90;
					minute = 0;
					second = 0;
				}

				if (minute > 59)
					minute = 59;
				else if (minute < 0)
					minute = 0;

				if (second >= 60)
					second = 59.999f;
				else if (second < 0)
					second = 0;

				if (degree2 >= 180)
				{
					degree2 = 180;
					minute2 = 0;
					second2 = 0;
				}
				else if (degree2 <= -180)
				{
					degree2 = -180;
					minute2 = 0;
					second2 = 0;
				}

				if (minute2 > 59)
					minute2 = 59;
				else if (minute2 < 0)
					minute2 = 0;

				if (second2 >= 60)
					second2 = 59.999f;
				else if (minute2 < 0)
					second2 = 0;

				EditorGUILayout.BeginHorizontal();
				GUILayout.FlexibleSpace();
				GUI.backgroundColor = UnityEngine.Color.black;
				EditorGUILayout.HelpBox("Degree, Minute, Second", MessageType.None, true);
				GUI.backgroundColor = UnityEngine.Color.white;
				GUI.backgroundColor = UnityEngine.Color.clear;
				EditorGUILayout.HelpBox("-->", MessageType.None, true);
				GUI.backgroundColor = UnityEngine.Color.white;
				GUI.backgroundColor = UnityEngine.Color.black;
				EditorGUILayout.HelpBox("Decimal Degrees", MessageType.None, true);
				GUI.backgroundColor = UnityEngine.Color.white;
				GUILayout.FlexibleSpace();
				EditorGUILayout.EndHorizontal();

				GUILayout.Space(30);

				GUI.backgroundColor = UnityEngine.Color.gray;
				EditorGUILayout.BeginHorizontal();
				GUILayout.FlexibleSpace();
				EditorGUILayout.HelpBox("LATITUDE", MessageType.None, true);
				GUILayout.FlexibleSpace();
				EditorGUILayout.EndHorizontal();
				GUI.backgroundColor = UnityEngine.Color.white;

				GUILayout.Space(10);

				EditorGUILayout.BeginHorizontal();
				GUILayout.FlexibleSpace();
				EditorGUILayout.HelpBox("Deg", MessageType.None, true);
				degree = EditorGUILayout.FloatField(degree);
				EditorGUILayout.HelpBox("Min", MessageType.None, true);
				minute = EditorGUILayout.FloatField(minute);
				EditorGUILayout.HelpBox("Sec", MessageType.None, true);
				second = EditorGUILayout.FloatField(second);
				GUI.backgroundColor = UnityEngine.Color.blue;
				EditorGUILayout.HelpBox(hemisphereLat, MessageType.None, true);
				GUI.backgroundColor = UnityEngine.Color.white;
				GUILayout.FlexibleSpace();
				EditorGUILayout.EndHorizontal();

				double dds;

				if (degree.ToString().StartsWith("-"))
				{
					dds = -degree + (minute / 60.0f) + (second / 3600.0f);
					decimalDegree = "-" + dds;
				}
				else
				{
					dds = degree + (minute / 60.0f) + (second / 3600.0f);
					decimalDegree = "" + dds;
				}

				GUILayout.Space(10);

				EditorGUILayout.BeginHorizontal();
				GUILayout.FlexibleSpace();
				decimalDegree = EditorGUILayout.TextField(decimalDegree.ToString());
				EditorGUILayout.HelpBox("Decimal Degrees", MessageType.None, true);
				GUILayout.FlexibleSpace();
				EditorGUILayout.EndHorizontal();

				GUILayout.Space(20);

				GUI.backgroundColor = UnityEngine.Color.gray;
				EditorGUILayout.BeginHorizontal();
				GUILayout.FlexibleSpace();
				EditorGUILayout.HelpBox("LONGITUDE", MessageType.None, true);
				GUILayout.FlexibleSpace();
				EditorGUILayout.EndHorizontal();
				GUI.backgroundColor = UnityEngine.Color.white;

				GUILayout.Space(10);

				EditorGUILayout.BeginHorizontal();
				GUILayout.FlexibleSpace();
				EditorGUILayout.HelpBox("Deg", MessageType.None, true);
				degree2 = EditorGUILayout.FloatField(degree2);
				EditorGUILayout.HelpBox("Min", MessageType.None, true);
				minute2 = EditorGUILayout.FloatField(minute2);
				EditorGUILayout.HelpBox("Sec", MessageType.None, true);
				second2 = EditorGUILayout.FloatField(second2);
				GUI.backgroundColor = UnityEngine.Color.blue;
				EditorGUILayout.HelpBox(hemisphereLon, MessageType.None, true);
				GUI.backgroundColor = UnityEngine.Color.white;
				GUILayout.FlexibleSpace();
				EditorGUILayout.EndHorizontal();

				double dds2;

				if (degree2.ToString().StartsWith("-"))
				{
					dds2 = -degree2 + (minute2 / 60.0f) + (second2 / 3600.0f);
					decimalDegree2 = "-" + dds2;
				}
				else
				{
					dds2 = degree2 + (minute2 / 60.0f) + (second2 / 3600.0f);
					decimalDegree2 = "" + dds2;
				}

				GUILayout.Space(10);

				EditorGUILayout.BeginHorizontal();
				GUILayout.FlexibleSpace();
				decimalDegree2 = EditorGUILayout.TextField(decimalDegree2.ToString());
				EditorGUILayout.HelpBox("Decimal Degrees", MessageType.None, true);
				GUILayout.FlexibleSpace();
				EditorGUILayout.EndHorizontal();

				GUILayout.Space(80);

				EditorGUILayout.BeginHorizontal();
				GUILayout.FlexibleSpace();
				GUI.backgroundColor = UnityEngine.Color.black;
				EditorGUILayout.HelpBox("Decimal Degrees", MessageType.None, true);
				GUI.backgroundColor = UnityEngine.Color.white;
				GUI.backgroundColor = UnityEngine.Color.clear;
				EditorGUILayout.HelpBox("-->", MessageType.None, true);
				GUI.backgroundColor = UnityEngine.Color.white;
				GUI.backgroundColor = UnityEngine.Color.black;
				EditorGUILayout.HelpBox("Degree, Minute, Second", MessageType.None, true);
				GUI.backgroundColor = UnityEngine.Color.white;
				GUILayout.FlexibleSpace();
				EditorGUILayout.EndHorizontal();

				GUILayout.Space(30);

				GUI.backgroundColor = UnityEngine.Color.gray;
				EditorGUILayout.BeginHorizontal();
				GUILayout.FlexibleSpace();
				EditorGUILayout.HelpBox("LATITUDE", MessageType.None, true);
				GUILayout.FlexibleSpace();
				EditorGUILayout.EndHorizontal();
				GUI.backgroundColor = UnityEngine.Color.white;

				GUILayout.Space(10);

				EditorGUILayout.BeginHorizontal();
				GUILayout.FlexibleSpace();
				decimalDegreeConverting = EditorGUILayout.TextField(decimalDegreeConverting);
				decimalDegreeConverting = Regex.Replace(decimalDegreeConverting, "[^0-9.-]", "");

				try
				{
					if (double.Parse(decimalDegreeConverting) > 90)
						decimalDegreeConverting = "90";

					if (double.Parse(decimalDegreeConverting) < -90)
						decimalDegreeConverting = "-90";
				}
				catch { decimalDegreeConverting = "0"; }

				try
				{
					decimalDegreeConverting = (double.Parse(decimalDegreeConverting) * 1).ToString();
				}
				catch { decimalDegreeConverting = "0"; }

				EditorGUILayout.HelpBox("Decimal Degrees", MessageType.None, true);
				GUILayout.FlexibleSpace();
				EditorGUILayout.EndHorizontal();

				if (decimalDegreeConverting != "")
				{
					secondConverted = double.Parse(decimalDegreeConverting) * 3600.0f;
				}

				GUILayout.Space(10);

				degreeConverted = ((Int32)secondConverted) / 3600;
				secondConverted = Math.Abs(secondConverted % 3600.0f);
				minuteConverted = System.Convert.ToInt32(secondConverted) / 60;
				secondConverted %= 60.0f;

				string secondConvertedSt = secondConverted.ToString();

				EditorGUILayout.BeginHorizontal();
				GUILayout.FlexibleSpace();
				EditorGUILayout.HelpBox("Deg", MessageType.None, true);
				degreeConverted = EditorGUILayout.IntField(degreeConverted);
				EditorGUILayout.HelpBox("Min", MessageType.None, true);
				minuteConverted = EditorGUILayout.IntField(minuteConverted);
				EditorGUILayout.HelpBox("Sec", MessageType.None, true);
				secondConvertedSt = EditorGUILayout.TextField(secondConvertedSt);
				GUI.backgroundColor = UnityEngine.Color.blue;
				EditorGUILayout.HelpBox(hemisphereLat2, MessageType.None, true);
				GUI.backgroundColor = UnityEngine.Color.white;
				GUILayout.FlexibleSpace();
				EditorGUILayout.EndHorizontal();

				GUILayout.Space(20);

				GUI.backgroundColor = UnityEngine.Color.gray;
				EditorGUILayout.BeginHorizontal();
				GUILayout.FlexibleSpace();
				EditorGUILayout.HelpBox("LONGITUDE", MessageType.None, true);
				GUILayout.FlexibleSpace();
				EditorGUILayout.EndHorizontal();
				GUI.backgroundColor = UnityEngine.Color.white;

				GUILayout.Space(10);

				EditorGUILayout.BeginHorizontal();
				GUILayout.FlexibleSpace();
				decimalDegreeConverting2 = EditorGUILayout.TextField(decimalDegreeConverting2);
				decimalDegreeConverting2 = Regex.Replace(decimalDegreeConverting2, "[^0-9.-]", "");

				try
				{
					if (double.Parse(decimalDegreeConverting2) > 180)
						decimalDegreeConverting2 = "180";

					if (double.Parse(decimalDegreeConverting2) < -180)
						decimalDegreeConverting2 = "-180";
				}
				catch { decimalDegreeConverting2 = "0"; }

				try
				{
					decimalDegreeConverting2 = (double.Parse(decimalDegreeConverting2) * 1).ToString();
				}
				catch { decimalDegreeConverting2 = "0"; }

				GUILayout.Space(10);

				EditorGUILayout.HelpBox("Decimal Degrees", MessageType.None, true);
				GUILayout.FlexibleSpace();
				EditorGUILayout.EndHorizontal();

				if (decimalDegreeConverting2 != "")
				{
					secondConverted2 = double.Parse(decimalDegreeConverting2) * 3600.0f;
				}

				degreeConverted2 = System.Convert.ToInt32(secondConverted2) / 3600;
				secondConverted2 = Math.Abs(secondConverted2 % 3600.0f);
				minuteConverted2 = System.Convert.ToInt32(secondConverted2) / 60;
				secondConverted2 %= 60.0f;

				string secondConvertedSt2 = secondConverted2.ToString();

				GUILayout.Space(10);

				EditorGUILayout.BeginHorizontal();
				GUILayout.FlexibleSpace();
				EditorGUILayout.HelpBox("Deg", MessageType.None, true);
				degreeConverted2 = EditorGUILayout.IntField(degreeConverted2);
				EditorGUILayout.HelpBox("Min", MessageType.None, true);
				minuteConverted2 = EditorGUILayout.IntField(minuteConverted2);
				EditorGUILayout.HelpBox("Sec", MessageType.None, true);
				secondConvertedSt2 = EditorGUILayout.TextField(secondConvertedSt2);
				GUI.backgroundColor = UnityEngine.Color.blue;
				EditorGUILayout.HelpBox(hemisphereLon2, MessageType.None, true);
				GUI.backgroundColor = UnityEngine.Color.white;
				GUILayout.FlexibleSpace();
				EditorGUILayout.EndHorizontal();

				GUILayout.Space(100);
			}
			else
				GUILayout.Space(15);

			GUILayout.Space(40);

			EditorGUILayout.EndScrollView();
			EditorGUILayout.EndVertical();

			GUILayout.Space(15);

			EditorGUILayout.BeginVertical();
			GUILayout.FlexibleSpace();

			GUI.backgroundColor = new UnityEngine.Color(0.8f, 0.8f, 0.8f, 0.5f);
			EditorGUILayout.BeginHorizontal();
			GUILayout.FlexibleSpace();
			if (GUILayout.Button(terrainButton))
			{
				CheckAvailableData();
			}
			GUILayout.FlexibleSpace();
			EditorGUILayout.EndHorizontal();
			GUI.backgroundColor = UnityEngine.Color.white;

			GUILayout.Space(10);

			GUI.color = UnityEngine.Color.green;
			EditorGUILayout.BeginHorizontal();
			GUILayout.FlexibleSpace();

			myStyle = new GUIStyle(GUI.skin.box);
			myStyle.fontSize = 15;
			myStyle.normal.textColor = UnityEngine.Color.black;

			rectToggle = GUILayoutUtility.GetLastRect();
			rectToggle.x = (position.width / 2f) - 50f;
			rectToggle.width = 100;
			rectToggle.height = 25;

			terrainResolutionTotal = heightmapResolution;
			GUI.Box(rectToggle, new GUIContent(terrainResolutionTotal.ToString() + "  px"), myStyle);

			GUILayout.FlexibleSpace();
			EditorGUILayout.EndHorizontal();
			GUI.color = UnityEngine.Color.white;

			if (!terrain && (splittedTerrains || splitSizeNew > 1))
			{
				GUILayout.Space(30);

				GUI.color = UnityEngine.Color.green;
				EditorGUILayout.BeginHorizontal();
				GUILayout.FlexibleSpace();

				myStyle = new GUIStyle(GUI.skin.box);
				myStyle.fontSize = 10;
				myStyle.normal.textColor = UnityEngine.Color.black;

				rectToggle = GUILayoutUtility.GetLastRect();
				rectToggle.x = (position.width / 2f) - 50f;
				rectToggle.width = 100;
				rectToggle.height = 20;

				if (splittedTerrains)
				{
					if (!Mathf.IsPowerOfTwo(croppedTerrains.Count))
						terrainResolutionChunk = ((Mathf.NextPowerOfTwo(heightmapResolution / splitSizeFinal)) / 2);
					else
						terrainResolutionChunk = heightmapResolutionSplit;
				}
				else
				{
					if (!Mathf.IsPowerOfTwo(splitSizeNew))
						terrainResolutionChunk = ((Mathf.NextPowerOfTwo(heightmapResolution / splitSizeNew)) / 2);
					else
						terrainResolutionChunk = heightmapResolution / splitSizeNew;
				}

				GUI.Box(rectToggle, new GUIContent(terrainResolutionChunk.ToString() + "  px"), myStyle);

				GUILayout.FlexibleSpace();
				EditorGUILayout.EndHorizontal();
				GUI.color = UnityEngine.Color.white;
			}

			GUILayout.Space(30);

			GUILayout.FlexibleSpace();
			EditorGUILayout.EndVertical();

			GUILayout.Space(15);
		}

		private void DisplayMap()
		{
			GUILayout.Space(30);

			mapWindowIsOpen = Resources.FindObjectsOfTypeAll<InteractiveMap>().Length;

			if (mapWindowIsOpen == 0)
				mapWindowButtonStr = "\nSHOW ON MAP\n";
			else
				mapWindowButtonStr = "\nFOCUS ON MAP\n";

			EditorGUILayout.BeginHorizontal();
			GUILayout.FlexibleSpace();
			if (GUILayout.Button(mapWindowButtonStr))
			{
				ShowMapAndRefresh();
			}
			GUILayout.FlexibleSpace();
			EditorGUILayout.EndHorizontal();

			GUI.backgroundColor = UnityEngine.Color.white;

			if (mapWindowIsOpen == 1)
			{
				GUILayout.Space(50);

				EditorGUILayout.BeginHorizontal();
				GUILayout.FlexibleSpace();
				EditorGUILayout.HelpBox("MAP SOURCE", MessageType.None, true);
				GUILayout.FlexibleSpace();
				EditorGUILayout.EndHorizontal();

				GUILayout.Space(10);

				EditorGUI.BeginChangeCheck();

				EditorGUILayout.BeginHorizontal();
				GUILayout.FlexibleSpace();
				mapTypeIndex = GUILayout.SelectionGrid(mapTypeIndex, mapTypeMode, 6);
				GUILayout.FlexibleSpace();
				EditorGUILayout.EndHorizontal();

				GUILayout.Space(30);

				EditorGUILayout.BeginHorizontal();
				GUILayout.FlexibleSpace();

				if (mapTypeIndex == 0)
				{
					InteractiveMap.mapSource = InteractiveMap.mapSourceEnum.google;
					mapTypeGoogle = (mapTypeGoogleEnum)EditorGUILayout.EnumPopup(mapTypeGoogle);
					InteractiveMap.mapTypeGoogle = (InteractiveMap.mapTypeGoogleEnum)mapTypeGoogle;
				}
				else if (mapTypeIndex == 1)
				{
					InteractiveMap.mapSource = InteractiveMap.mapSourceEnum.bing;
					mapTypeBing = (mapTypeBingEnum)EditorGUILayout.EnumPopup(mapTypeBing);
					InteractiveMap.mapTypeBing = (InteractiveMap.mapTypeBingEnum)mapTypeBing;
				}
				else if (mapTypeIndex == 2)
				{
					InteractiveMap.mapSource = InteractiveMap.mapSourceEnum.openstreetmap;
				}
				else if (mapTypeIndex == 3)
				{
					InteractiveMap.mapSource = InteractiveMap.mapSourceEnum.mapquest;
					mapTypeMapQuest = (mapTypeMapQuestEnum)EditorGUILayout.EnumPopup(mapTypeMapQuest);
					InteractiveMap.mapTypeMapQuest = (InteractiveMap.mapTypeMapQuestEnum)mapTypeMapQuest;
				}
				else if (mapTypeIndex == 4)
				{
					InteractiveMap.mapSource = InteractiveMap.mapSourceEnum.mapbox;
					mapTypeMapBox = (mapTypeMapBoxEnum)EditorGUILayout.EnumPopup(mapTypeMapBox);
					InteractiveMap.mapTypeMapBox = (InteractiveMap.mapTypeMapBoxEnum)mapTypeMapBox;
				}
				else if (mapTypeIndex == 5)
				{
					InteractiveMap.mapSource = InteractiveMap.mapSourceEnum.yandex;
					mapTypeYandex = (mapTypeYandexEnum)EditorGUILayout.EnumPopup(mapTypeYandex);
					InteractiveMap.mapTypeYandex = (InteractiveMap.mapTypeYandexEnum)mapTypeYandex;
				}

				GUILayout.FlexibleSpace();
				EditorGUILayout.EndHorizontal();

				if (EditorGUI.EndChangeCheck())
					ShowMapAndRefresh();

				GUILayout.Space(100);
			}
		}

		private void SetUnitsTo1Meter()
		{
			terrainSizeNewX = areaSizeLon * 1000f;
			terrainSizeNewZ = areaSizeLat * 1000f;
		}

		private void ResampleOperation()
		{
			if (resampleUp)
				scaleFactor = 2f;
			else
				scaleFactor = 0.5f;

			TerrainData data = new TerrainData();
			float[,] heights = null;
			int resolution = 0;
			Vector3 terrainSize = Vector3.zero;
			int chunkResolution = 0;
			int chunkResolutionResampled = 0;

			if (splittedTerrains)
			{
				CheckTerrainChunks();

				data = croppedTerrains[0].terrainData;
				chunkResolution = data.heightmapResolution;
				chunkResolutionResampled = Mathf.RoundToInt((float)(chunkResolution - 1) * scaleFactor) + 1;
				resolution = ((chunkResolution - 1) * splitSizeFinal) + 1;
				resolutionFinal = Mathf.RoundToInt((float)(resolution - 1) * scaleFactor) + 1;
				terrainSize = data.size;
				heights = new float[resolution, resolution];
				finalHeights = new float[resolutionFinal, resolutionFinal];

				// Fill in a single heightmap from terrain tiles
				GetUberHeightmap(heights, chunkResolution);
			}
			else if (terrain)
			{
				data = terrain.terrainData;
				chunkResolution = data.heightmapResolution;
				resolution = chunkResolution;
				resolutionFinal = Mathf.RoundToInt((float)(resolution - 1) * scaleFactor) + 1;
				terrainSize = data.size;
				heights = data.GetHeights(0, 0, resolution, resolution);
				finalHeights = new float[resolutionFinal, resolutionFinal];
			}

			showProgressData = true;
			progressDATA = 0;

			RunAsync(() =>
			{
				for (int x = 0; x < resolutionFinal; x++)
				{
					for (int y = 0; y < resolutionFinal; y++)
					{
						finalHeights[x, y] = ResampleHeights((float)x / scaleFactor, (float)y / scaleFactor, resolution, heights);
						progressDATA = Mathf.InverseLerp(0f, (float)resolutionFinal, (float)x);
					}
				}

				showProgressData = false;

			// Smooth terrain heights if upsampling
			if (resampleUp)
					FinalizeSmooth(finalHeights, resolutionFinal, resolutionFinal, smoothIterationsResample, 0, 0);

				QueueOnMainThread(() =>
				{
					if (splittedTerrains)
					{
						int length = croppedTerrains.Count;
						int grid = (int)Mathf.Sqrt(length);

					// Roll back and slice single uber heightmap into heightmap tiles
					for (int i = 0; i < length; i++)
							UberHeightmap2Chunks(finalHeights, chunkResolutionResampled, grid, i);

					// Apply heightmap tiles on terrain tiles
					for (int i = 0; i < length; i++)
						{
							data = croppedTerrains[i].terrainData;
							data.heightmapResolution = chunkResolutionResampled;
							data.size = terrainSize;
							data.SetHeights(0, 0, splitHeights[i]);
							croppedTerrains[i].Flush();
						}
					}
					else
					{
						data = terrain.terrainData;
						data.heightmapResolution = resolutionFinal;
						data.size = terrainSize;
						data.SetHeights(0, 0, finalHeights);
						terrain.Flush();
					}

					showProgressSmoothen = false;
					showProgressSmoothenOperation = false;
				});
			});
		}

		private float ResampleHeights(float X, float Y, int resolution, float[,] heights)
		{
			int X1 = Mathf.FloorToInt((X + (float)resolution) % (float)resolution);
			int Y1 = Mathf.FloorToInt((Y + (float)resolution) % (float)resolution);
			float FinalValue = heights[X1, Y1];

			return FinalValue;
		}

		private void CheckTerrainSizeUnits()
		{
			terrainSizeFactor = areaSizeLat / areaSizeLon;

			if (splittedTerrains)
			{
				float tsX = 0;
				float tsY = 0;

				foreach (Terrain tr in croppedTerrains)
				{
					tsX += tr.terrainData.size.x;
					tsY += tr.terrainData.size.z;
				}

				terrainSizeX = tsX;
				terrainSizeY = tsY;
			}
			else if (terrain)
			{
				terrainSizeX = terrain.terrainData.size.x;
				terrainSizeY = terrain.terrainData.size.z;
			}
		}

		private void SmoothenTerrains()
		{
			if (splittedTerrains)
			{
				CheckTerrainChunks();

				int chunkHeightmapResolution = croppedTerrains[0].terrainData.heightmapResolution;
				int totalHeightmapResolution = chunkHeightmapResolution * splitSizeFinal;
				finalHeights = new float[totalHeightmapResolution, totalHeightmapResolution];

				GetUberHeightmap(finalHeights, chunkHeightmapResolution);

				RunAsync(() =>
				{
				// Smoothen Uber Heightmap
				FinalizeSmooth(finalHeights, totalHeightmapResolution, totalHeightmapResolution, smoothIterations, smoothBlendIndex, smoothBlend);

					QueueOnMainThread(() =>
					{
						UberHeightmap2ChunksSmooth(finalHeights, chunkHeightmapResolution);
						ManageNeighborings();

						showProgressSmoothen = false;
						showProgressSmoothenOperation = false;
					});
				});
			}
			else if (terrain)
			{
				TerrainData data = terrain.terrainData;
				float[,] heights = data.GetHeights(0, 0, data.heightmapResolution, data.heightmapResolution);
				terrainWidth = data.heightmapResolution;
				terrainHeight = data.heightmapResolution;

				RunAsync(() =>
				{
					FinalizeSmooth(heights, terrainWidth, terrainHeight, smoothIterations, smoothBlendIndex, smoothBlend);

					QueueOnMainThread(() =>
					{
						data.SetHeights(0, 0, heights);
					});
				});
			}
			else
			{
				EditorUtility.DisplayDialog("UNAVAILABLE TERRAIN", unavailableTerrainStr, "Ok");
				return;
			}
		}

		private void GetUberHeightmap(float[,] heights, int chunkResolution)
		{
			int counter = 0;
			int currentRow = splitSizeFinal - 1;
			int xLength = chunkResolution;
			int yLength = chunkResolution;

			for (int i = 0; i < splitSizeFinal; i++)
			{
				for (int j = 0; j < splitSizeFinal; j++)
				{
					TerrainData data = croppedTerrains[counter].terrainData;
					float[,] dataSplitted = data.GetHeights(0, 0, data.heightmapResolution, data.heightmapResolution);

					int xStart = (currentRow * (chunkResolution - 1));
					int yStart = (j * (chunkResolution - 1));

					// Populate Uber Heightmap
					for (int x = 0; x < xLength; x++)
						for (int y = 0; y < yLength; y++)
							heights[xStart + x, yStart + y] = dataSplitted[x, y];

					counter++;

					EditorUtility.DisplayProgressBar("CREATING UBER HEIGHTMAP", "Stage  " + (counter).ToString() + "  of  " + terrainChunks, Mathf.InverseLerp(0f, (float)(terrainChunks - 1), (float)(counter)));
				}
				currentRow--;
			}

			EditorUtility.ClearProgressBar();
		}

		private void UberHeightmap2Chunks(float[,] heights, int chunkResolution, int tiles, int index)
		{
			int counter = 0;
			int currentRow = tiles - 1;
			int xLength = chunkResolution;
			int yLength = chunkResolution;
			int count = (int)Mathf.Pow(tiles, 2);

			if (index == 0)
				splitHeights = new List<float[,]>();

			for (int i = 0; i < tiles; i++)
			{
				for (int j = 0; j < tiles; j++)
				{
					if (counter == index)
					{
						float[,] dataSplitted = new float[chunkResolution, chunkResolution];
						splitHeights.Add(new float[chunkResolution, chunkResolution]);

						int xStart = (currentRow * (chunkResolution - 1));
						int yStart = (j * (chunkResolution - 1));

						// Populate Terrain Chunks
						for (int x = 0; x < xLength; x++)
							for (int y = 0; y < yLength; y++)
								dataSplitted[x, y] = heights[xStart + x, yStart + y];

						splitHeights[counter] = dataSplitted;

						EditorUtility.DisplayProgressBar("UPLOADING CHUNK HEIGHTS", "Terrain  " + (counter).ToString() + "  of  " + count, Mathf.InverseLerp(0f, (float)(count - 1), (float)(counter)));
					}

					counter++;
				}

				currentRow--;
			}

			EditorUtility.ClearProgressBar();
		}

		private void UberHeightmap2ChunksSmooth(float[,] heights, int chunkResolution)
		{
			int counter = 0;
			int currentRow = splitSizeFinal - 1;
			int xLength = chunkResolution;
			int yLength = chunkResolution;

			for (int i = 0; i < splitSizeFinal; i++)
			{
				for (int j = 0; j < splitSizeFinal; j++)
				{
					float[,] dataSplitted = new float[chunkResolution, chunkResolution];

					int xStart = (currentRow * (chunkResolution - 1));
					int yStart = (j * (chunkResolution - 1));

					// Populate Terrain Chunks
					for (int x = 0; x < xLength; x++)
						for (int y = 0; y < yLength; y++)
							dataSplitted[x, y] = heights[xStart + x, yStart + y];

					croppedTerrains[counter].terrainData.SetHeights(0, 0, dataSplitted);
					croppedTerrains[counter].Flush();

					counter++;

					EditorUtility.DisplayProgressBar("UPLOADING CHUNK HEIGHTS", "Terrain  " + (counter + 1).ToString() + "  of  " + terrainChunks, Mathf.InverseLerp(0f, (float)(terrainChunks - 1), (float)(counter)));
				}
				currentRow--;
			}

			EditorUtility.ClearProgressBar();
		}

		private void StitchTiles()
		{
			string directoryPathElevation = UnityEditor.EditorUtility.OpenFolderPanel("Select Database Elevation Folder", Application.dataPath, "Elevation");

			if (string.IsNullOrEmpty(directoryPathElevation))
			{
				EditorUtility.DisplayDialog("UNKNOWN PATH", "Select a folder containing RAW elevation tiles", "Ok");
				return;
			}

			string[] fileNames = LogicalComparer(directoryPathElevation, ".raw");
			int length = fileNames.Length;

			if (length == 0)
			{
				EditorUtility.DisplayDialog("NO TILES FOUND", "Select a folder containing .raw elevation tiles", "Ok");
				return;
			}

			if (!Mathf.IsPowerOfTwo(length))
			{
				EditorUtility.DisplayDialog("NON-SQUARE TILES COUNT", "Number of tiles in the folder is not a power of 2 value.\n\nTiles should cover a perfect square area.", "Ok");
				return;
			}

			stitchProgress = 0;
			showProgressStitch = true;

			int grid = (int)Mathf.Sqrt(length);
			int index = 0;

			string tileName;
			string tileNameRgt;
			string tileNameTop;

			byte[] buffer;
			byte[] bufferRgt;
			byte[] bufferTop;

			bool hasTop = false;
			bool hasRgt = false;

			PickRawDefaults(fileNames[0]);

			int resolution = m_Width;
			int depth = 2;
			int count = resolution * depth;

			RunAsync(() =>
			{
				for (int i = 0; i < grid; i++)
				{
					for (int j = 0; j < grid; j++)
					{
						tileName = fileNames[index];

						if (i > 0)
							hasTop = true;
						else
							hasTop = false;

						if (j < grid - 1)
							hasRgt = true;
						else
							hasRgt = false;

						using (BinaryReader reader = new BinaryReader(File.Open(tileName, FileMode.Open, FileAccess.Read)))
						{
							buffer = reader.ReadBytes((resolution * resolution) * depth);
							reader.Close();
						}

						if (hasTop && hasRgt)
						{
							tileNameTop = fileNames[index - grid];
							tileNameRgt = fileNames[index + 1];

							using (BinaryReader reader = new BinaryReader(File.Open(tileNameTop, FileMode.Open, FileAccess.Read)))
							{
								bufferTop = reader.ReadBytes((resolution * resolution) * depth);
								reader.Close();
							}

							using (BinaryReader reader = new BinaryReader(File.Open(tileNameRgt, FileMode.Open, FileAccess.Read)))
							{
								bufferRgt = reader.ReadBytes((resolution * resolution) * depth);
								reader.Close();
							}

						// Stitch BOTTOM's top row to TOP's bottom row
						int offset = buffer.Length - count;

							Buffer.BlockCopy(buffer, 0, bufferTop, offset, count);

							FileStream fileStream = new FileStream(tileNameTop, FileMode.Create);
							fileStream.Write(bufferTop, 0, bufferTop.Length);
							fileStream.Close();

						// Stitch LEFT's right column to RIGHT's left column
						int offsetLft = count - depth;
							int offsetRgt = 0;

							for (int x = 0; x < resolution; x++)
							{
								if (x > 0)
									offsetLft += count;

								offsetRgt = x * count;

								bufferRgt[offsetRgt] = buffer[offsetLft];
								bufferRgt[offsetRgt + 1] = buffer[offsetLft + 1];
							}

							FileStream fileStream2 = new FileStream(tileNameRgt, FileMode.Create);
							fileStream2.Write(bufferRgt, 0, bufferRgt.Length);
							fileStream2.Close();
						}
						else if (hasTop)
						{
							tileNameTop = fileNames[index - grid];

							using (BinaryReader reader = new BinaryReader(File.Open(tileNameTop, FileMode.Open, FileAccess.Read)))
							{
								bufferTop = reader.ReadBytes((resolution * resolution) * depth);
								reader.Close();
							}

						// Stitch BOTTOM's top row to TOP's bottom row
						int offset = buffer.Length - count;

							Buffer.BlockCopy(buffer, 0, bufferTop, offset, count);

							FileStream fileStream = new FileStream(tileNameTop, FileMode.Create);
							fileStream.Write(bufferTop, 0, bufferTop.Length);
							fileStream.Close();
						}
						else if (hasRgt)
						{
							tileNameRgt = fileNames[index + 1];

							using (BinaryReader reader = new BinaryReader(File.Open(tileNameRgt, FileMode.Open, FileAccess.Read)))
							{
								bufferRgt = reader.ReadBytes((resolution * resolution) * depth);
								reader.Close();
							}

						// Stitch LEFT's right column to RIGHT's left column
						int offsetLft = count - depth;
							int offsetRgt = 0;

							for (int x = 0; x < resolution; x++)
							{
								if (x > 0)
									offsetLft += count;

								offsetRgt = x * count;

								bufferRgt[offsetRgt] = buffer[offsetLft];
								bufferRgt[offsetRgt + 1] = buffer[offsetLft + 1];
							}

							FileStream fileStream = new FileStream(tileNameRgt, FileMode.Create);
							fileStream.Write(bufferRgt, 0, bufferRgt.Length);
							fileStream.Close();
						}

						index++;

						stitchProgress = Mathf.InverseLerp(0, length, index);
					}
				}

				QueueOnMainThread(() =>
				{
					showProgressStitch = false;
				});
			});
		}

		private void CheckAvailableData()
		{
			if (offlineDataIndex == 0 && asciiDataFile)
				CheckHeightmapResolution();
			else if (offlineDataIndex == 1 && rawDataFile)
				CheckHeightmapResolution();
			else if (offlineDataIndex == 2 && tiffDataFile)
				CheckHeightmapResolution();
			else
			{
				EditorUtility.DisplayDialog("NO DATA INSERTED", "Insert a Data File First", "Ok");
				return;
			}

			// This is just to remove console warning
			EditorGUILayout.HelpBox(noData.ToString(), MessageType.None);
		}

		private void CheckHeightmapResolution()
		{
			//Check if Terrain resolution is not below 32
			if ((heightmapResolution / splitSizeNew) < 32)
			{
				EditorUtility.DisplayDialog("INSUFFICIENT HEIGHTMAP RESOLUTION", "Heightmap Resolution Is Below \"32\" For Each Terrain.\n\nIncrease Heightmap Resolution To Avoid Empty Areas In Terrain Chunks.", "Ok");
				return;
			}
			else if (splittedTerrains && terrainResolutionChunk < 32)
			{
				EditorUtility.DisplayDialog("INSUFFICIENT HEIGHTMAP RESOLUTION", "Heightmap Resolution Is Below \"32\" For Each Terrain.\n\nIncrease Heightmap Resolution To Avoid Empty Areas In Terrain Chunks.", "Ok");
				return;
			}
			else
			{
				//Check if Terrain resolution is not above 4096 & optionally continue
				if ((heightmapResolution / splitSizeNew) > 4096)
				{
					if (splitSizeNew > 1)
					{
						if (EditorUtility.DisplayDialog("HIGH TERRAIN RESOLUTION", "Heightmap Resolution Is Above \"4096\" For Each Terrain.\n\nOptionally You Can Press \"Continue\" And Have A High Value For Heightmap Resolution On Terrain Chunks In Cost Of Performance.", "Cancel", "Continue"))
							return;

						SetupDownloaderElevation();
					}
					else
					{
						if (EditorUtility.DisplayDialog("HIGH TERRAIN RESOLUTION", "Heightmap Resolution Is Above \"4096\" For Terrain.\n\nOptionally You Can Press \"Continue\" And Have A High Value For Heightmap Resolution On Terrain In Cost Of Performance.", "Cancel", "Continue"))
							return;

						SetupDownloaderElevation();
					}
				}
				else if (splittedTerrains && heightmapResolutionSplit > 4096)
				{
					if (EditorUtility.DisplayDialog("HIGH TERRAIN RESOLUTION", "Heightmap Resolution Is Above \"4096\" For Each Terrain.\n\nOptionally You Can Press \"Continue\" And Have A High Value For Heightmap Resolution On Terrain Chunks In Cost Of Performance.", "Cancel", "Continue"))
						return;

					SetupDownloaderElevation();
				}
				else if (terrain && heightmapResolution > 4096)
				{
					if (EditorUtility.DisplayDialog("HIGH TERRAIN RESOLUTION", "Heightmap Resolution Is Above \"4096\" For Terrain.\n\nOptionally You Can Press \"Continue\" And Have A High Value For Heightmap Resolution On Terrain In Cost Of Performance.", "Cancel", "Continue"))
						return;

					SetupDownloaderElevation();
				}
				else
				{
					SetupDownloaderElevation();
				}
			}
		}

		private void CheckHeightmapResolutionResample()
		{
			if (!resampleUp)
			{
				//Check if Terrain resolution is not below 32
				if ((terrainResolution / splitSizeFinal) <= 32)
				{
					EditorUtility.DisplayDialog("INSUFFICIENT HEIGHTMAP RESOLUTION", "Heightmap Resolution Is Below \"32\" For Each Terrain.\n\nIncrease Heightmap Resolution To Avoid Empty Areas In Terrain Chunks.", "Ok");
					return;
				}
				else if (splittedTerrains && terrainResolutionChunk <= 32)
				{
					EditorUtility.DisplayDialog("INSUFFICIENT HEIGHTMAP RESOLUTION", "Heightmap Resolution Is Below \"32\" For Each Terrain.\n\nIncrease Heightmap Resolution To Avoid Empty Areas In Terrain Chunks.", "Ok");
					return;
				}
			}
			else
			{
				//Check if Terrain resolution is not above 8192
				if ((terrainResolution / splitSizeFinal) >= 8192)
				{
					EditorUtility.DisplayDialog("VERY HIGH TERRAIN RESOLUTION", "Heightmap Resolution Can Not Exceed Above \"8192\" For Each Terrain.", "Ok");
					return;
				}
				else if (splittedTerrains && terrainResolutionChunk >= 8192)
				{
					EditorUtility.DisplayDialog("VERY HIGH TERRAIN RESOLUTION", "Heightmap Resolution Can Not Exceed Above \"8192\" For Each Terrain.", "Ok");
					return;
				}

				//Check if Terrain resolution is not above 4096 & optionally continue
				if ((terrainResolution / splitSizeFinal) >= 4096)
				{
					if (splitSizeFinal > 1)
					{
						if (EditorUtility.DisplayDialog("HIGH TERRAIN RESOLUTION", "Heightmap Resolution Is Above \"4096\" For Each Terrain.\n\nOptionally You Can Press \"Continue\" And Have A High Value For Heightmap Resolution On Terrain Chunks In Cost Of Performance.", "Cancel", "Continue"))
							return;
					}
					else
					{
						if (EditorUtility.DisplayDialog("HIGH TERRAIN RESOLUTION", "Heightmap Resolution Is Above \"4096\" For Terrain.\n\nOptionally You Can Press \"Continue\" And Have A High Value For Heightmap Resolution On Terrain In Cost Of Performance.", "Cancel", "Continue"))
							return;
					}
				}
				else if (splittedTerrains && heightmapResolutionSplit >= 4096)
				{
					if (EditorUtility.DisplayDialog("HIGH TERRAIN RESOLUTION", "Heightmap Resolution Is Above \"4096\" For Each Terrain.\n\nOptionally You Can Press \"Continue\" And Have A High Value For Heightmap Resolution On Terrain Chunks In Cost Of Performance.", "Cancel", "Continue"))
						return;
				}
				else if (terrain && terrainResolution >= 4096)
				{
					if (EditorUtility.DisplayDialog("HIGH TERRAIN RESOLUTION", "Heightmap Resolution Is Above \"4096\" For Terrain.\n\nOptionally You Can Press \"Continue\" And Have A High Value For Heightmap Resolution On Terrain In Cost Of Performance.", "Cancel", "Continue"))
						return;
				}
			}

			ResampleOperation();
		}

		private void SetupDownloaderElevation()
		{
			if (!Directory.Exists(downloadsPath))
				Directory.CreateDirectory(downloadsPath);

			showProgressData = true;
			progressDATA = 0;
			progressGenerateASCII = 0;
			progressGenerateRAW = 0;
			smoothIterationProgress = 0;
			smoothProgress = 0;

			downloadDateElevation = System.DateTime.Now.ToString("MM-dd-yyyy_HH-mm-ss");

			if (terrain || splittedTerrains)
				newTerrainGeneration = false;
			else
			{
				newTerrainGeneration = true;
				GenerateNewTerrainObject();
			}

			if (splittedTerrains)
			{
				CheckTerrainChunks();

				initialTerrainWidth = croppedTerrains[0].terrainData.size.x;
				initialTerrainLength = croppedTerrains[0].terrainData.size.z;
				splitSizeFinal = (int)Mathf.Sqrt((float)croppedTerrains.Count);

				RemoveLightmapStatic(croppedTerrains);
			}
			else if (terrain)
			{
				initialTerrainWidth = terrain.terrainData.size.x;
				initialTerrainLength = terrain.terrainData.size.z;
				terrainChunks = 1;
				splitSizeFinal = 1;

				RemoveLightmapStatic(terrain);
			}

			topCorner = new List<float>();
			bottomCorner = new List<float>();
			leftCorner = new List<float>();
			rightCorner = new List<float>();

			GenerateTerrainHeights();
		}

		private void GenerateNewTerrainObject()
		{
			SetData();

			splitDirectoryPath = downloadsPath + "/" + downloadDateElevation + "/Terrain Tiles";
			Directory.CreateDirectory(splitDirectoryPath);
			AssetDatabase.Refresh();

			CreateTerrainData();
			CreateTerrainObject();

			if (size == 1)
				terrain = terrains[0];
			else
				splittedTerrains = terrainsParent;
		}

		private void SetData()
		{
			size = (int)enumValueNew;

			terrainsLong = size;
			terrainsWide = size;

			oldWidth = terrainSizeNewX;
			oldHeight = terrainSizeNewY;
			oldLength = terrainSizeNewZ;

			newWidth = oldWidth / terrainsWide;
			newLength = oldLength / terrainsLong;

			xPos = (terrainSizeNewX / 2f) * -1f;
			yPos = lowestPoint;
			zPos = (terrainSizeNewZ / 2f) * -1f;

			newHeightMapResolution = ((heightmapResolutionSplit - 1) / size) + 1;
			newEvenHeightMapResolution = newHeightMapResolution - 1;

			heightmapPixelError = pixelError;
		}

		private void GenerateTerrainHeights()
		{
			AssetDatabase.Refresh();

			if (offlineDataIndex == 0 && asciiDataFile)
			{
				RunAsync(() =>
				{
					AsciiData(asciiPath);

					QueueOnMainThread(() =>
					{
						FinalizeTerrainHeights();
					});
				});
			}
			else if (offlineDataIndex == 1 && rawDataFile)
			{
				RunAsync(() =>
				{
					RawData(rawPath);

					QueueOnMainThread(() =>
					{
						FinalizeTerrainHeights();
					});
				});
			}
			else if (offlineDataIndex == 2 && tiffDataFile)
			{
				RunAsync(() =>
				{
					TiffData(tiffPath);

					QueueOnMainThread(() =>
					{
						FinalizeTerrainHeights();
					});
				});
			}
		}

		private void FinalizeTerrainHeights()
		{
			if (offlineDataIndex == 0 && asciiDataFile)
			{
				RunAsync(() =>
				{
					FinalizeSmooth(asciiData, nRows, nCols, smoothIterationsOfflineData, smoothBlendIndexOfflineData, smoothBlendOfflineData);

					QueueOnMainThread(() =>
					{
						LoadTerrainHeightsFromASCII();
						showProgressData = false;
						ManageNeighborings();
						FinalizeTerrainElevation();
					});
				});
			}
			else if (offlineDataIndex == 1 && rawDataFile)
			{
				RunAsync(() =>
				{
					FinalizeSmooth(rawData, m_Width, m_Height, smoothIterationsOfflineData, smoothBlendIndexOfflineData, smoothBlendOfflineData);

					QueueOnMainThread(() =>
					{
						LoadTerrainHeightsFromRAW();
						showProgressData = false;
						ManageNeighborings();
						FinalizeTerrainElevation();
					});
				});
			}
			else if (offlineDataIndex == 2 && tiffDataFile)
			{
				RunAsync(() =>
				{
					FinalizeSmooth(tiffData, tiffWidth, tiffLength, smoothIterationsOfflineData, smoothBlendIndexOfflineData, smoothBlendOfflineData);

					QueueOnMainThread(() =>
					{
						LoadTerrainHeightsFromTIFF();
						showProgressData = false;
						ManageNeighborings();
						FinalizeTerrainElevation();
					});
				});
			}
		}

		private void ManageNeighborings()
		{
			if (splittedTerrains)
			{
				SetTerrainNeighbors();

				if (splittedTerrains.GetComponent<TerrainNeighbors>() == null)
					splittedTerrains.AddComponent<TerrainNeighbors>();
			}
			else if (terrain)
			{
				if (terrain.gameObject.GetComponent<TerrainNeighbors>() == null)
					terrain.gameObject.AddComponent<TerrainNeighbors>();
			}
		}

		private void FinalizeTerrainElevation()
		{
			showProgressGenerateASCII = false;
			showProgressGenerateRAW = false;
			showProgressSmoothen = false;
			showProgressSmoothenOperation = false;
			heightsAnalyzed = false;

			GCAndRecompile();
		}

		private void RemoveLightmapStatic(List<Terrain> croppedTerrains)
		{
#if UNITY_2019_1_OR_NEWER
			foreach (Terrain t in croppedTerrains)
			{
				UnityEditor.StaticEditorFlags flags = UnityEditor.GameObjectUtility.GetStaticEditorFlags(t.gameObject);
				flags = flags & ~(UnityEditor.StaticEditorFlags.ContributeGI);
				UnityEditor.GameObjectUtility.SetStaticEditorFlags(t.gameObject, flags);
			}
#else
			foreach (Terrain t in croppedTerrains)
			{
				UnityEditor.StaticEditorFlags flags = UnityEditor.GameObjectUtility.GetStaticEditorFlags(t.gameObject);
				flags = flags & ~(UnityEditor.StaticEditorFlags.LightmapStatic);
				UnityEditor.GameObjectUtility.SetStaticEditorFlags(t.gameObject, flags);
			}
#endif
		}

		private void RemoveLightmapStatic(Terrain terrain)
		{
#if UNITY_2019_1_OR_NEWER
			UnityEditor.StaticEditorFlags flags = UnityEditor.GameObjectUtility.GetStaticEditorFlags(terrain.gameObject);
			flags = flags & ~(UnityEditor.StaticEditorFlags.ContributeGI);
			UnityEditor.GameObjectUtility.SetStaticEditorFlags(terrain.gameObject, flags);
#else
			UnityEditor.StaticEditorFlags flags = UnityEditor.GameObjectUtility.GetStaticEditorFlags(terrain.gameObject);
			flags = flags & ~(UnityEditor.StaticEditorFlags.LightmapStatic);
			UnityEditor.GameObjectUtility.SetStaticEditorFlags(terrain.gameObject, flags);
#endif
		}

		private void AsciiData(string fileName)
		{
			StreamReader sr = new StreamReader(fileName, Encoding.ASCII, true);
			//ncols
			string[] line1 = sr.ReadLine().Replace(',', '.').Split(new char[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
			nCols = (Convert.ToInt32(line1[1]));
			//nrows
			string[] line2 = sr.ReadLine().Replace(',', '.').Split(new char[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
			nRows = (Convert.ToInt32(line2[1]));
			//xllcorner
			string[] line3 = sr.ReadLine().Replace(',', '.').Split(new char[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
			xllCorner = Convert.ToDouble((line3[1]), CultureInfo.InvariantCulture);
			//yllcorner
			string[] line4 = sr.ReadLine().Replace(',', '.').Split(new char[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
			yllCorner = Convert.ToDouble((line4[1]), CultureInfo.InvariantCulture);
			//cellsize
			string[] line5 = sr.ReadLine().Replace(',', '.').Split(new char[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
			cellSizeASCII = Convert.ToDouble((line5[1]), CultureInfo.InvariantCulture);
			//nodata
			string[] line6 = sr.ReadLine().Replace(',', '.').Split(new char[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
			noData = Convert.ToDouble((line6[1]), CultureInfo.InvariantCulture);

			asciiData = new float[nRows, nCols];

			heightmapResX = Mathf.FloorToInt((float)Mathf.ClosestPowerOfTwo(nRows) / (float)splitSizeFinal);
			heightmapResY = Mathf.FloorToInt((float)Mathf.ClosestPowerOfTwo(nCols) / (float)splitSizeFinal);
			heightmapResXAll = nRows;
			heightmapResYAll = nCols;

			highestPoint = float.MinValue;
			lowestPoint = float.MaxValue;

			for (int y = 0; y < nRows; y++)
			{
				string[] line = sr.ReadLine().Split(new char[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);

				for (int x = 0; x < nCols; x++)
				{
					currentHeight = float.Parse(line[x].Replace(',', '.'));

					if (currentHeight != (float)noData)
					{
						if (currentHeight >= highestPoint)
							highestPoint = currentHeight;
						else if (currentHeight <= lowestPoint)
							lowestPoint = currentHeight;
					}

					asciiData[(nRows - 1) - y, x] = currentHeight / everestPeak;
					progressDATA = Mathf.InverseLerp(0f, (float)nRows, (float)y);
				}
			}

			sr.Close();

			bool containsBathyData = false;

			if (lowestPoint < 0)
				containsBathyData = true;

			float lowestPointNormalized = lowestPoint / everestPeak;
			terrainEverestDiffer = everestPeak / highestPoint;

			for (int y = 0; y < nRows; y++)
			{
				for (int x = 0; x < nCols; x++)
				{
					if (!containsBathyData)
						asciiData[(nRows - 1) - y, x] -= lowestPointNormalized;
					else
						asciiData[(nRows - 1) - y, x] += Mathf.Abs(lowestPointNormalized);

					// Check Terrain Corners
					// Top Row
					if (y == 0)
						topCorner.Add(currentHeight);

					// Bottom Row
					else if (y == nRows - 1)
						bottomCorner.Add(currentHeight);

					// Left Column
					if (x == 0)
						leftCorner.Add(currentHeight);

					// Right Column
					else if (x == nCols - 1)
						rightCorner.Add(currentHeight);
				}
			}

			CheckCornersASCII();
		}

		private void RawData(string fileName)
		{
			PickRawDefaults(fileName);

			byte[] buffer;

			using (BinaryReader reader = new BinaryReader(File.Open(fileName, FileMode.Open, FileAccess.Read)))
			{
				buffer = reader.ReadBytes((m_Width * m_Height) * (int)m_Depth);
				reader.Close();
			}

			rawData = new float[m_Width, m_Height];

			if (m_Depth == Depth.Bit16)
			{
				float num = 1.525879E-05f;

				for (int y = 0; y < m_Width; y++)
				{
					for (int x = 0; x < m_Height; x++)
					{
						int num2 = Clamp(x, 0, m_Width - 1) + Clamp(y, 0, m_Height - 1) * m_Width;

						if (m_ByteOrder == ByteOrder.Mac == BitConverter.IsLittleEndian)
						{
							byte b = buffer[num2 * 2];
							buffer[num2 * 2] = buffer[num2 * 2 + 1];
							buffer[num2 * 2 + 1] = b;
						}

						ushort num3 = BitConverter.ToUInt16(buffer, num2 * 2);
						float num4 = (float)num3 * num;
						currentHeight = num4;

						rawData[(m_Width - 1) - y, x] = num4;

						progressDATA = Mathf.InverseLerp(0f, (float)m_Width, (float)y);
					}
				}
			}
			else
			{
				float num10 = 0.00390625f;

				for (int y = 0; y < m_Width; y++)
				{
					for (int x = 0; x < m_Height; x++)
					{
						int index = Clamp(x, 0, m_Width - 1) + (Clamp(y, 0, m_Height - 1) * m_Width);
						byte num14 = buffer[index];
						float num15 = num14 * num10;
						currentHeight = num15;

						rawData[(m_Width - 1) - y, x] = num15;

						progressDATA = Mathf.InverseLerp(0f, (float)m_Width, (float)y);
					}
				}
			}

			highestPoint = rawData.Cast<float>().Max() * everestPeak;
			lowestPoint = rawData.Cast<float>().Min() * everestPeak;
			float lowestPointNormalized = rawData.Cast<float>().Min();
			terrainEverestDiffer = everestPeak / highestPoint;

			if (m_Depth == Depth.Bit16)
			{
				for (int y = 0; y < m_Width; y++)
				{
					for (int x = 0; x < m_Height; x++)
					{
						if (lowestPointNormalized >= 0)
							rawData[(m_Width - 1) - y, x] -= lowestPointNormalized;
						else
							rawData[(m_Width - 1) - y, x] += Mathf.Abs(lowestPointNormalized);

						// Check Terrain Corners
						// Top Row
						if (y == 0)
							topCorner.Add(currentHeight);

						// Bottom Row
						else if (y == m_Width - 1)
							bottomCorner.Add(currentHeight);

						// Left Column
						if (x == 0)
							leftCorner.Add(currentHeight);

						// Right Column
						else if (x == m_Height - 1)
							rightCorner.Add(currentHeight);
					}
				}
			}
			else
			{
				for (int y = 0; y < m_Width; y++)
				{
					for (int x = 0; x < m_Height; x++)
					{
						rawData[(m_Width - 1) - y, x] -= lowestPointNormalized;

						// Check Terrain Corners
						// Top Row
						if (y == 0)
							topCorner.Add(currentHeight);

						// Bottom Row
						else if (y == m_Width - 1)
							bottomCorner.Add(currentHeight);

						// Left Column
						if (x == 0)
							leftCorner.Add(currentHeight);

						// Right Column
						else if (x == m_Height - 1)
							rightCorner.Add(currentHeight);
					}
				}
			}

			CheckCornersRAW();
		}

		private void TiffData(string fileName)
		{
			try
			{
				using (Tiff inputImage = Tiff.Open(fileName, "r"))
				{
					tiffWidth = inputImage.GetField(TiffTag.IMAGEWIDTH)[0].ToInt();
					tiffLength = inputImage.GetField(TiffTag.IMAGELENGTH)[0].ToInt();
					tiffData = new float[tiffLength, tiffWidth];
					tiffDataASCII = new float[tiffLength, tiffWidth];

					int tileHeight = inputImage.GetField(TiffTag.TILELENGTH)[0].ToInt();
					int tileWidth = inputImage.GetField(TiffTag.TILEWIDTH)[0].ToInt();

					byte[] buffer = new byte[tileHeight * tileWidth * 4];
					float[,] fBuffer = new float[tileHeight, tileWidth];

					heightmapResX = Mathf.FloorToInt((float)Mathf.ClosestPowerOfTwo(tiffWidth) / (float)splitSizeFinal);
					heightmapResY = Mathf.FloorToInt((float)Mathf.ClosestPowerOfTwo(tiffLength) / (float)splitSizeFinal);
					heightmapResXAll = tiffWidth;
					heightmapResYAll = tiffLength;

					for (int y = 0; y < tiffLength; y += tileHeight)
					{
						for (int x = 0; x < tiffWidth; x += tileWidth)
						{
							inputImage.ReadTile(buffer, 0, x, y, 0, 0);
							Buffer.BlockCopy(buffer, 0, fBuffer, 0, buffer.Length);

							for (int i = 0; i < tileHeight; i++)
								for (int j = 0; j < tileWidth; j++)
									if ((y + i) < tiffLength && (x + j) < tiffWidth)
										tiffDataASCII[y + i, x + j] = fBuffer[i, j];

							progressDATA = Mathf.InverseLerp(0f, (float)tiffLength, (float)y);
						}
					}
				}
			}
			catch { }

			highestPoint = tiffDataASCII.Cast<float>().Max();
			lowestPoint = tiffDataASCII.Cast<float>().Min();
			terrainEverestDiffer = everestPeak / highestPoint;

			// Rotate terrain heigts and normalize values
			for (int y = 0; y < tiffWidth; y++)
			{
				for (int x = 0; x < tiffLength; x++)
				{
					currentHeight = tiffDataASCII[(tiffWidth - 1) - y, x];

					try
					{
						if (lowestPoint >= 0)
							tiffData[y, x] = (currentHeight - lowestPoint) / everestPeak;
						else
							tiffData[y, x] = (currentHeight + Mathf.Abs(lowestPoint)) / everestPeak;
					}
					catch (ArgumentOutOfRangeException)
					{
						tiffData[y, x] = 0f;
					}

					// Check Terrain Corners
					// Top Row
					if (y == 0)
						topCorner.Add(currentHeight);

					// Bottom Row
					else if (y == tiffWidth - 1)
						bottomCorner.Add(currentHeight);

					// Left Column
					if (x == 0)
						leftCorner.Add(currentHeight);

					// Right Column
					else if (x == tiffLength - 1)
						rightCorner.Add(currentHeight);
				}
			}

			CheckCornersTIFF();
		}

		private void CheckCornersASCII()
		{
			// Check Top
			if (topCorner.All(o => o == topCorner.First()))
			{
				for (int y = 0; y < nRows; y++)
					for (int x = 0; x < nCols; x++)
						if (y == 0)
							asciiData[(nRows - 1) - y, x] = asciiData[(nRows - 1) - (y + 1), x];
			}

			// Check Bottom
			if (bottomCorner.All(o => o == bottomCorner.First()))
			{
				for (int y = 0; y < nRows; y++)
					for (int x = 0; x < nCols; x++)
						if (y == nRows - 1)
							asciiData[(nRows - 1) - y, x] = asciiData[(nRows - 1) - (y - 1), x];
			}

			// Check Left
			if (leftCorner.All(o => o == leftCorner.First()))
			{
				for (int y = 0; y < nRows; y++)
					for (int x = 0; x < nCols; x++)
						if (x == 0)
							asciiData[(nRows - 1) - y, x] = asciiData[(nRows - 1) - y, x + 1];
			}

			// Check Right
			if (rightCorner.All(o => o == rightCorner.First()))
			{
				for (int y = 0; y < nRows; y++)
					for (int x = 0; x < nCols; x++)
						if (x == nCols - 1)
							asciiData[(nRows - 1) - y, x] = asciiData[(nRows - 1) - y, x - 1];
			}
		}

		private void CheckCornersRAW()
		{
			// Check Top
			if (topCorner.All(o => o == topCorner.First()))
			{
				for (int y = 0; y < m_Width; y++)
					for (int x = 0; x < m_Height; x++)
						if (y == 0)
							rawData[(m_Width - 1) - y, x] = rawData[(m_Width - 1) - (y + 1), x];
			}

			// Check Bottom
			if (bottomCorner.All(o => o == bottomCorner.First()))
			{
				for (int y = 0; y < m_Width; y++)
					for (int x = 0; x < m_Height; x++)
						if (y == m_Width - 1)
							rawData[(m_Width - 1) - y, x] = rawData[(m_Width - 1) - (y - 1), x];
			}

			// Check Left
			if (leftCorner.All(o => o == leftCorner.First()))
			{
				for (int y = 0; y < m_Width; y++)
					for (int x = 0; x < m_Height; x++)
						if (x == 0)
							rawData[(m_Width - 1) - y, x] = rawData[(m_Width - 1) - y, x + 1];
			}

			// Check Right
			if (rightCorner.All(o => o == rightCorner.First()))
			{
				for (int y = 0; y < m_Width; y++)
					for (int x = 0; x < m_Height; x++)
						if (x == m_Height - 1)
							rawData[(m_Width - 1) - y, x] = rawData[(m_Width - 1) - y, x - 1];
			}
		}

		private void CheckCornersTIFF()
		{
			// Check Top
			if (topCorner.All(o => o == topCorner.First()))
			{
				for (int y = 0; y < tiffWidth; y++)
					for (int x = 0; x < tiffLength; x++)
						if (y == 0)
							tiffData[y, x] = tiffData[y + 1, x];
			}

			// Check Bottom
			if (bottomCorner.All(o => o == bottomCorner.First()))
			{
				for (int y = 0; y < tiffWidth; y++)
					for (int x = 0; x < tiffLength; x++)
						if (y == tiffWidth - 1)
							tiffData[y, x] = tiffData[y - 1, x];
			}

			// Check Left
			if (leftCorner.All(o => o == leftCorner.First()))
			{
				for (int y = 0; y < tiffWidth; y++)
					for (int x = 0; x < tiffLength; x++)
						if (x == 0)
							tiffData[y, x] = tiffData[y, x + 1];
			}

			// Check Right
			if (rightCorner.All(o => o == rightCorner.First()))
			{
				for (int y = 0; y < tiffWidth; y++)
					for (int x = 0; x < tiffLength; x++)
						if (x == tiffLength - 1)
							tiffData[y, x] = tiffData[y, x - 1];
			}
		}

		private static float Clamp(float value, float min, float max)
		{
			if (value < min)
				value = min;
			else if (value > max)
				value = max;

			return value;
		}

		private static int Clamp(int value, int min, int max)
		{
			if (value < min)
				value = min;
			else if (value > max)
				value = max;

			return value;
		}

		private void LoadTerrainHeightsFromASCII()
		{
			CalculateResampleHeightmaps();

			int counter = 0;
			int currentRow = splitSizeFinal - 1;
			int xLength = heightmapResFinalX;
			int yLength = heightmapResFinalY;

			if (splittedTerrains)
			{
				for (int i = 0; i < splitSizeFinal; i++)
				{
					for (int j = 0; j < splitSizeFinal; j++)
					{
						croppedTerrains[counter].terrainData.heightmapResolution = heightmapResFinalX;
						float[,] asciiDataSplitted = new float[heightmapResFinalX, heightmapResFinalY];

						int xStart = (currentRow * (heightmapResFinalX - 1));
						int yStart = (j * (heightmapResFinalY - 1));

						for (int x = 0; x < xLength; x++)
							for (int y = 0; y < yLength; y++)
								asciiDataSplitted[x, y] = finalHeights[xStart + x, yStart + y];

						croppedTerrains[counter].terrainData.SetHeights(0, 0, asciiDataSplitted);
						croppedTerrains[counter].Flush();

						float realTerrainWidth = areaSizeLon * 1000.0f / splitSizeFinal;
						float realTerrainLength = areaSizeLat * 1000.0f / splitSizeFinal;
						croppedTerrains[counter].terrainData.size = RealTerrainSize(realTerrainWidth, realTerrainLength, highestPoint);
						croppedTerrains[counter].Flush();

						counter++;

						EditorUtility.DisplayProgressBar("LOADING HEIGHTS", "Terrain  " + (counter + 1).ToString() + "  of  " + terrainChunks, Mathf.InverseLerp(0f, (float)(terrainChunks - 1), (float)(counter)));
					}

					currentRow--;
				}
			}
			else if (terrain)
			{
				terrain.terrainData.heightmapResolution = heightmapResFinalXAll;

				EditorUtility.DisplayProgressBar("LOADING HEIGHTS", "Loading Terrain Heights", Mathf.InverseLerp(0f, 1f, 1f));

				terrain.terrainData.SetHeights(0, 0, finalHeights);
				terrain.Flush();

				float realTerrainWidth = areaSizeLon * 1000.0f;
				float realTerrainLength = areaSizeLat * 1000.0f;
				terrain.terrainData.size = RealTerrainSize(realTerrainWidth, realTerrainLength, highestPoint);
				terrain.Flush();
			}

			EditorUtility.ClearProgressBar();
			AssetDatabase.Refresh();
		}

		private void LoadTerrainHeightsFromRAW()
		{
			CalculateResampleHeightmaps();

			int counter = 0;
			int currentRow = splitSizeFinal - 1;
			int xLength = heightmapResFinalX;
			int yLength = heightmapResFinalY;

			if (splittedTerrains)
			{
				for (int i = 0; i < splitSizeFinal; i++)
				{
					for (int j = 0; j < splitSizeFinal; j++)
					{
						croppedTerrains[counter].terrainData.heightmapResolution = heightmapResFinalX;
						float[,] rawDataSplitted = new float[heightmapResFinalX, heightmapResFinalY];

						int xStart = (currentRow * (heightmapResFinalX - 1));
						int yStart = (j * (heightmapResFinalY - 1));

						for (int x = 0; x < xLength; x++)
							for (int y = 0; y < yLength; y++)
								rawDataSplitted[x, y] = finalHeights[xStart + x, yStart + y];

						croppedTerrains[counter].terrainData.SetHeights(0, 0, rawDataSplitted);
						croppedTerrains[counter].Flush();

						float realTerrainWidth = areaSizeLon * 1000.0f / splitSizeFinal;
						float realTerrainLength = areaSizeLat * 1000.0f / splitSizeFinal;
						croppedTerrains[counter].terrainData.size = RealTerrainSize(realTerrainWidth, realTerrainLength, highestPoint);
						croppedTerrains[counter].Flush();

						counter++;

						EditorUtility.DisplayProgressBar("LOADING HEIGHTS", "Terrain  " + (counter + 1).ToString() + "  of  " + terrainChunks, Mathf.InverseLerp(0f, (float)(terrainChunks - 1), (float)(counter)));
					}

					currentRow--;
				}
			}
			else if (terrain)
			{
				terrain.terrainData.heightmapResolution = heightmapResFinalXAll;

				EditorUtility.DisplayProgressBar("LOADING HEIGHTS", "Loading Terrain Heights", Mathf.InverseLerp(0f, 1f, 1f));

				terrain.terrainData.SetHeights(0, 0, finalHeights);
				terrain.Flush();

				float realTerrainWidth = areaSizeLon * 1000.0f;
				float realTerrainLength = areaSizeLat * 1000.0f;
				terrain.terrainData.size = RealTerrainSize(realTerrainWidth, realTerrainLength, highestPoint);
				terrain.Flush();
			}

			EditorUtility.ClearProgressBar();
			AssetDatabase.Refresh();
		}

		private void LoadTerrainHeightsFromTIFF()
		{
			CalculateResampleHeightmaps();

			int counter = 0;
			int currentRow = splitSizeFinal - 1;
			int xLength = heightmapResFinalX;
			int yLength = heightmapResFinalY;

			if (splittedTerrains)
			{
				for (int i = 0; i < splitSizeFinal; i++)
				{
					for (int j = 0; j < splitSizeFinal; j++)
					{
						croppedTerrains[counter].terrainData.heightmapResolution = heightmapResFinalX;
						float[,] tiffDataSplitted = new float[heightmapResFinalX, heightmapResFinalY];

						int xStart = (currentRow * (heightmapResFinalX - 1));
						int yStart = (j * (heightmapResFinalY - 1));

						for (int x = 0; x < xLength; x++)
							for (int y = 0; y < yLength; y++)
								tiffDataSplitted[x, y] = finalHeights[xStart + x, yStart + y];

						croppedTerrains[counter].terrainData.SetHeights(0, 0, tiffDataSplitted);
						croppedTerrains[counter].Flush();

						float realTerrainWidth = areaSizeLon * 1000.0f / splitSizeFinal;
						float realTerrainLength = areaSizeLat * 1000.0f / splitSizeFinal;
						croppedTerrains[counter].terrainData.size = RealTerrainSize(realTerrainWidth, realTerrainLength, highestPoint);
						croppedTerrains[counter].Flush();

						counter++;

						EditorUtility.DisplayProgressBar("LOADING HEIGHTS", "Terrain  " + (counter + 1).ToString() + "  of  " + terrainChunks, Mathf.InverseLerp(0f, (float)(terrainChunks - 1), (float)(counter)));
					}

					currentRow--;
				}
			}
			else if (terrain)
			{
				terrain.terrainData.heightmapResolution = heightmapResFinalXAll;

				EditorUtility.DisplayProgressBar("LOADING HEIGHTS", "Loading Terrain Heights", Mathf.InverseLerp(0f, 1f, 1f));

				terrain.terrainData.SetHeights(0, 0, finalHeights);
				terrain.Flush();

				float realTerrainWidth = areaSizeLon * 1000.0f;
				float realTerrainLength = areaSizeLat * 1000.0f;
				terrain.terrainData.size = RealTerrainSize(realTerrainWidth, realTerrainLength, highestPoint);
				terrain.Flush();
			}

			EditorUtility.ClearProgressBar();
			AssetDatabase.Refresh();
		}

		private void FinalizeSmooth(float[,] heightMapSmoothed, int width, int height, int iterations, int blendIndex, float blending)
		{
			if (iterations != 0)
			{
				smoothIterationsProgress = iterations;

				int Tw = width;
				int Th = height;

				if (blendIndex == 1)
				{
					float[,] generatedHeightMap = (float[,])heightMapSmoothed.Clone();
					generatedHeightMap = SmoothedHeights(generatedHeightMap, Tw, Th, iterations);

					showProgressSmoothenOperation = true;

					for (int Ty = 0; Ty < Th; Ty++)
					{
						for (int Tx = 0; Tx < Tw; Tx++)
						{
							float oldHeightAtPoint = heightMapSmoothed[Tx, Ty];
							float newHeightAtPoint = generatedHeightMap[Tx, Ty];
							float blendedHeightAtPoint = 0.0f;

							blendedHeightAtPoint = (newHeightAtPoint * blending) + (oldHeightAtPoint * (1.0f - blending));

							heightMapSmoothed[Tx, Ty] = blendedHeightAtPoint;
						}

						smoothProgress = Mathf.InverseLerp(0f, (float)Th, (float)Ty);
					}
				}
				else
					heightMapSmoothed = SmoothedHeights(heightMapSmoothed, Tw, Th, iterations);
			}
		}

		private float[,] SmoothedHeights(float[,] heightMap, int tw, int th, int iterations)
		{
			showProgressSmoothen = true;

			int Tw = tw;
			int Th = th;
			int xNeighbours;
			int yNeighbours;
			int xShift;
			int yShift;
			int xIndex;
			int yIndex;
			int Tx;
			int Ty;

			for (int iter = 0; iter < iterations; iter++)
			{
				for (Ty = 0; Ty < Th; Ty++)
				{
					if (Ty == 0)
					{
						yNeighbours = 2;
						yShift = 0;
						yIndex = 0;
					}
					else if (Ty == Th - 1)
					{
						yNeighbours = 2;
						yShift = -1;
						yIndex = 1;
					}
					else
					{
						yNeighbours = 3;
						yShift = -1;
						yIndex = 1;
					}

					for (Tx = 0; Tx < Tw; Tx++)
					{
						if (Tx == 0)
						{
							xNeighbours = 2;
							xShift = 0;
							xIndex = 0;
						}
						else if (Tx == Tw - 1)
						{
							xNeighbours = 2;
							xShift = -1;
							xIndex = 1;
						}
						else
						{
							xNeighbours = 3;
							xShift = -1;
							xIndex = 1;
						}

						int Ny;
						int Nx;
						float hCumulative = 0.0f;
						int nNeighbours = 0;

						for (Ny = 0; Ny < yNeighbours; Ny++)
						{
							for (Nx = 0; Nx < xNeighbours; Nx++)
							{
								if (neighbourhood == Neighbourhood.Moore || (neighbourhood == Neighbourhood.VonNeumann && (Nx == xIndex || Ny == yIndex)))
								{
									float heightAtPoint = heightMap[Tx + Nx + xShift, Ty + Ny + yShift]; // Get height at point
									hCumulative += heightAtPoint;
									nNeighbours++;
								}
							}
						}

						float hAverage = hCumulative / nNeighbours;
						heightMap[Tx + xIndex + xShift, Ty + yIndex + yShift] = hAverage;
					}
				}

				smoothIterationProgress = iter + 1;
			}

			return heightMap;
		}

		private void CalculateResampleHeightmaps()
		{
			if ((offlineDataIndex == 0 && asciiDataFile) || (offlineDataIndex == 1 && rawDataFile) || (offlineDataIndex == 2 && tiffDataFile))
			{
				if (heightmapResXAll == Mathf.ClosestPowerOfTwo(heightmapResXAll) + splitSizeFinal)
				{
					heightmapResFinalX = Mathf.ClosestPowerOfTwo(heightmapResX) + 1;
					heightmapResFinalXAll = heightmapResXAll;

					//heightmapResFinalY = Mathf.ClosestPowerOfTwo(heightmapResY) + 1;
					//heightmapResFinalYAll = heightmapResYAll;

					heightmapResFinalY = heightmapResFinalX;
					heightmapResFinalYAll = heightmapResFinalXAll;

					finalHeights = new float[heightmapResFinalXAll, heightmapResFinalYAll];

					if (offlineDataIndex == 0 && asciiDataFile)
						finalHeights = asciiData;
					else if (offlineDataIndex == 1 && rawDataFile)
						finalHeights = rawData;
					else if (offlineDataIndex == 2 && tiffDataFile)
						finalHeights = tiffData;
				}
				else
				{
					heightmapResFinalX = Mathf.ClosestPowerOfTwo(heightmapResX) + 1;
					heightmapResFinalXAll = heightmapResFinalX * splitSizeFinal;

					//heightmapResFinalY = Mathf.ClosestPowerOfTwo(heightmapResY) + 1;
					//heightmapResFinalYAll = heightmapResFinalY * splitSizeFinal;

					heightmapResFinalY = heightmapResFinalX;
					heightmapResFinalYAll = heightmapResFinalXAll;

					//UnityEngine.Debug.Log(heightmapResFinalXAll);
					//UnityEngine.Debug.Log(heightmapResFinalYAll);

					ResampleOperation("");
				}
			}
		}

		private void ResampleData(string format)
		{
			if (heightmapResXAll == Mathf.ClosestPowerOfTwo(heightmapResXAll) + splitSizeFinal)
			{
				heightmapResFinalX = Mathf.ClosestPowerOfTwo(heightmapResX) + 1;
				heightmapResFinalXAll = heightmapResXAll;

				heightmapResFinalY = Mathf.ClosestPowerOfTwo(heightmapResY) + 1;
				heightmapResFinalYAll = heightmapResYAll;

				finalHeights = new float[heightmapResFinalXAll, heightmapResFinalYAll];

				heightmapResolution = heightmapResFinalXAll;

				if (format.Equals("asc"))
				{
					if (smoothIterationsDataTile > 0)
						FinalizeSmooth(asciiData, heightmapResolution, heightmapResolution, smoothIterationsDataTile, 0, 0);

					finalHeights = asciiData;
				}
				else if (format.Equals("raw"))
				{
					if (smoothIterationsDataTile > 0)
						FinalizeSmooth(rawData, heightmapResolution, heightmapResolution, smoothIterationsDataTile, 0, 0);

					finalHeights = rawData;
				}

				else if (format.Equals("tif"))
				{
					if (smoothIterationsDataTile > 0)
						FinalizeSmooth(tiffData, heightmapResolution, heightmapResolution, smoothIterationsDataTile, 0, 0);

					finalHeights = tiffData;
				}
			}
			else
			{
				heightmapResFinalX = Mathf.ClosestPowerOfTwo(heightmapResX) + 1;
				heightmapResFinalXAll = heightmapResFinalX * splitSizeFinal;

				heightmapResFinalY = Mathf.ClosestPowerOfTwo(heightmapResY) + 1;
				heightmapResFinalYAll = heightmapResFinalY * splitSizeFinal;

				heightmapResolution = heightmapResFinalXAll;

				ResampleOperation(format);
			}
		}

		private void ResampleOperation(string format)
		{
			float scaleFactorLat = ((float)heightmapResFinalXAll) / ((float)heightmapResXAll);
			float scaleFactorLon = ((float)heightmapResFinalYAll) / ((float)heightmapResYAll);

			finalHeights = new float[heightmapResFinalXAll, heightmapResFinalYAll];

			for (int x = 0; x < heightmapResFinalXAll; x++)
				for (int y = 0; y < heightmapResFinalYAll; y++)
					finalHeights[x, y] = ResampleHeights((float)x / scaleFactorLat, (float)y / scaleFactorLon, format);

			if (!string.IsNullOrEmpty(format) && smoothIterationsDataTile > 0)
				FinalizeSmooth(finalHeights, heightmapResolution, heightmapResolution, smoothIterationsDataTile, 0, 0);
		}

		private float ResampleHeights(float X, float Y, string format)
		{
			try
			{
				int X1 = Mathf.RoundToInt((X + heightmapResXAll % heightmapResXAll));
				int Y1 = Mathf.RoundToInt((Y + heightmapResYAll % heightmapResYAll));

				if (string.IsNullOrEmpty(format))
				{
					if (offlineDataIndex == 0 && asciiDataFile)
						return asciiData[X1, Y1];
					else if (offlineDataIndex == 1 && rawDataFile)
						return rawData[X1, Y1];
					else if (offlineDataIndex == 2 && tiffDataFile)
						return tiffData[X1, Y1];
				}
				else
				{
					if (format.Equals("asc"))
						return asciiData[X1, Y1];
					else if (format.Equals("raw"))
						return rawData[X1, Y1];
					else if (format.Equals("tif"))
						return tiffData[X1, Y1];
				}

				return 0f;
			}
			catch
			{
				return 0f;
			}
		}

		private Vector3 RealTerrainSize(float width, float length, float height)
		{
			float realTerrainSizeX = initialTerrainWidth;
			//float realTerrainSizeZ = realTerrainSizeX * terrainSizeFactor;
			float realTerrainSizeZ = initialTerrainLength;

			//float realToUnitsY = realTerrainSizeX * ((height * terrainEverestDiffer) / width);
			float realToUnitsY = height * terrainEverestDiffer;

			float realTerrainSizeY = realToUnitsY;

			if (realTerrainSizeY <= 0f || float.IsNaN(realTerrainSizeY) || float.IsInfinity(realTerrainSizeY) || float.IsPositiveInfinity(realTerrainSizeY) || float.IsNegativeInfinity(realTerrainSizeY))
				realTerrainSizeY = 0.001f;

			Vector3 finalTerrainSize = new Vector3(realTerrainSizeX, realTerrainSizeY, realTerrainSizeZ);

			return finalTerrainSize;
		}

		private void GetASCIIInfo()
		{
			//RunAsync(()=>
			//{
			StreamReader sr = new StreamReader(asciiPath, Encoding.ASCII, true);
			//ncols
			string[] line1 = sr.ReadLine().Replace(',', '.').Split(new char[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
			nCols = (Convert.ToInt32(line1[1]));
			//nrows
			string[] line2 = sr.ReadLine().Replace(',', '.').Split(new char[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
			nRows = (Convert.ToInt32(line2[1]));
			//xllcorner
			string[] line3 = sr.ReadLine().Replace(',', '.').Split(new char[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
			xllCorner = Convert.ToDouble((line3[1]), CultureInfo.InvariantCulture);
			//yllcorner
			string[] line4 = sr.ReadLine().Replace(',', '.').Split(new char[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
			yllCorner = Convert.ToDouble((line4[1]), CultureInfo.InvariantCulture);
			//cellsize
			string[] line5 = sr.ReadLine().Replace(',', '.').Split(new char[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
			cellSizeASCII = Convert.ToDouble((line5[1]), CultureInfo.InvariantCulture);
			//nodata
			string[] line6 = sr.ReadLine().Replace(',', '.').Split(new char[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
			noData = Convert.ToDouble((line6[1]), CultureInfo.InvariantCulture);

			heightmapResX = Mathf.FloorToInt((float)Mathf.ClosestPowerOfTwo(nRows) / (float)splitSizeFinal);
			heightmapResY = Mathf.FloorToInt((float)Mathf.ClosestPowerOfTwo(nCols) / (float)splitSizeFinal);
			heightmapResXAll = nRows;
			heightmapResYAll = nCols;

			asciiData = new float[nRows, nCols];

			highestPoint = float.MinValue;
			lowestPoint = float.MaxValue;

			EditorUtility.DisplayProgressBar("ANALYZING HEIGHTMAP", "Please Wait...", 0.5f);

			for (int y = 0; y < nRows; y++)
			{
				string[] line = sr.ReadLine().Split(new char[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);

				for (int x = 0; x < nCols; x++)
				{
					currentHeight = float.Parse(line[x].Replace(',', '.'));
					asciiData[(nRows - 1) - y, x] = currentHeight / everestPeak;

					if (currentHeight != (float)noData)
					{
						if (currentHeight >= highestPoint)
							highestPoint = currentHeight;
						else if (currentHeight <= lowestPoint)
							lowestPoint = currentHeight;
					}
				}
			}

			EditorUtility.ClearProgressBar();

			sr.Close();

			//});
		}

		private void GetRAWInfo()
		{
			EditorUtility.DisplayProgressBar("ANALYZING HEIGHTMAP", "Please Wait...", 0.5f);

			//RunAsync(()=>
			//{
			PickRawDefaults(rawPath);

			byte[] buffer;

			using (BinaryReader reader = new BinaryReader(File.Open(rawPath, FileMode.Open, FileAccess.Read)))
			{
				buffer = reader.ReadBytes((m_Width * m_Height) * (int)m_Depth);
				reader.Close();
			}

			rawData = new float[m_Width, m_Height];

			if (m_Depth == Depth.Bit16)
			{
				float num = 1.525879E-05f;

				for (int i = 0; i < m_Width; i++)
				{
					for (int j = 0; j < m_Height; j++)
					{
						int num2 = Mathf.Clamp(j, 0, m_Width - 1) + Mathf.Clamp(i, 0, m_Height - 1) * m_Width;

						if (m_ByteOrder == ByteOrder.Mac == BitConverter.IsLittleEndian)
						{
							byte b = buffer[num2 * 2];
							buffer[num2 * 2] = buffer[num2 * 2 + 1];
							buffer[num2 * 2 + 1] = b;
						}

						ushort num3 = BitConverter.ToUInt16(buffer, num2 * 2);
						float num4 = (float)num3 * num;
						rawData[(m_Width - 1) - i, j] = num4;
					}
				}
			}
			else
			{
				float num10 = 0.00390625f;

				for (int k = 0; k < m_Width; k++)
				{
					for (int m = 0; m < m_Height; m++)
					{
						int index = Mathf.Clamp(m, 0, m_Width - 1) + (Mathf.Clamp(k, 0, m_Height - 1) * m_Width);
						byte num14 = buffer[index];
						float num15 = num14 * num10;
						rawData[(m_Width - 1) - k, m] = num15;
					}
				}
			}

			highestPoint = rawData.Cast<float>().Max() * everestPeak;
			lowestPoint = rawData.Cast<float>().Min() * everestPeak;
			//});

			EditorUtility.ClearProgressBar();
		}

		private void GetTIFFInfo()
		{
			EditorUtility.DisplayProgressBar("ANALYZING HEIGHTMAP", "Please Wait...", 0.5f);

			//RunAsync(()=>
			//{
			try
			{
				using (Tiff inputImage = Tiff.Open(tiffPath, "r"))
				{
					tiffWidth = inputImage.GetField(TiffTag.IMAGEWIDTH)[0].ToInt();
					tiffLength = inputImage.GetField(TiffTag.IMAGELENGTH)[0].ToInt();
					tiffData = new float[tiffLength, tiffWidth];

					int tileHeight = inputImage.GetField(TiffTag.TILELENGTH)[0].ToInt();
					int tileWidth = inputImage.GetField(TiffTag.TILEWIDTH)[0].ToInt();

					byte[] buffer = new byte[tileHeight * tileWidth * 4];
					float[,] fBuffer = new float[tileHeight, tileWidth];

					heightmapResX = Mathf.FloorToInt((float)Mathf.ClosestPowerOfTwo(tiffWidth) / (float)splitSizeFinal);
					heightmapResY = Mathf.FloorToInt((float)Mathf.ClosestPowerOfTwo(tiffLength) / (float)splitSizeFinal);
					heightmapResXAll = tiffWidth;
					heightmapResYAll = tiffLength;

					for (int y = 0; y < tiffLength; y += tileHeight)
					{
						for (int x = 0; x < tiffWidth; x += tileWidth)
						{
							inputImage.ReadTile(buffer, 0, x, y, 0, 0);
							Buffer.BlockCopy(buffer, 0, fBuffer, 0, buffer.Length);

							for (int i = 0; i < tileHeight; i++)
								for (int j = 0; j < tileWidth; j++)
									if ((y + i) < tiffLength && (x + j) < tiffWidth)
										tiffData[y + i, x + j] = fBuffer[i, j];
						}
					}
				}
			}
			catch { }

			highestPoint = tiffData.Cast<float>().Max();
			lowestPoint = tiffData.Cast<float>().Min();
			//});

			EditorUtility.ClearProgressBar();
		}

		private void ReadXMLFile(string xmlPath)
		{
			try
			{
				XmlDocument coordinatesDoc = new XmlDocument();
				coordinatesDoc.Load(xmlPath);
				XmlNode nodeLat = coordinatesDoc.DocumentElement.SelectSingleNode("/Coordinates/Latitude");
				XmlNode nodeLon = coordinatesDoc.DocumentElement.SelectSingleNode("/Coordinates/Longitude");
				XmlNode nodeTop = coordinatesDoc.DocumentElement.SelectSingleNode("/Coordinates/Top");
				XmlNode nodeLft = coordinatesDoc.DocumentElement.SelectSingleNode("/Coordinates/Left");
				XmlNode nodeBtm = coordinatesDoc.DocumentElement.SelectSingleNode("/Coordinates/Bottom");
				XmlNode nodeRgt = coordinatesDoc.DocumentElement.SelectSingleNode("/Coordinates/Right");
				XmlNode nodeLatExt = coordinatesDoc.DocumentElement.SelectSingleNode("/Coordinates/LatExtents");
				XmlNode nodeLonExt = coordinatesDoc.DocumentElement.SelectSingleNode("/Coordinates/LonExtents");

				top = nodeTop.InnerText;
				left = nodeLft.InnerText;
				bottom = nodeBtm.InnerText;
				right = nodeRgt.InnerText;
				latitudeUser = nodeLat.InnerText;
				longitudeUser = nodeLon.InnerText;
				areaSizeLat = float.Parse(nodeLatExt.InnerText);
				areaSizeLon = float.Parse(nodeLonExt.InnerText);

				SetUnitsTo1Meter();
			}
			catch { }
		}

		/*
		private void CoordinatesFromASCII ()
		{
			try
			{
				string projPath = asciiPath.Replace(".asc", ".prj");
				string utmZone = Between(File.ReadAllText(projPath), "UTM_Zone_", "\",GEOGCS");
				double dLatBottom = 0;
				double dLonLeft = 0;
				double dLatTop = 0;
				double dLonRight = 0;

				// Get Bottom & Left coordinates
				nsBaseCmnGIS.cBaseCmnGIS baseGISBottomLeft = new nsBaseCmnGIS.cBaseCmnGIS();
				string utmBottomLeft = baseGISBottomLeft.iUTM2LatLon(yllCorner, xllCorner, utmZone, ref dLatBottom, ref dLonLeft);
				string[] utmValuesBottomLeft = utmBottomLeft.Split(',');

				dLatBottom = double.Parse(utmValuesBottomLeft[0]);
				dLonLeft = double.Parse(utmValuesBottomLeft[1]);

				// Get Top & Right coordinates
				double yllCornerTR = yllCorner + (nCols * cellSizeASCII);
				double xllCornerTR = xllCorner + (nRows * cellSizeASCII);

				nsBaseCmnGIS.cBaseCmnGIS baseGISTopRight = new nsBaseCmnGIS.cBaseCmnGIS();
				string utmTopRight = baseGISTopRight.iUTM2LatLon(yllCornerTR, xllCornerTR, utmZone, ref dLatTop, ref dLonRight);
				string[] utmValuesTopRight = utmTopRight.Split(',');

				dLatTop = double.Parse(utmValuesTopRight[0]);
				dLonRight = double.Parse(utmValuesTopRight[1]);

				// Results
				top = dLatTop.ToString();
				left = dLonLeft.ToString();
				bottom = dLatBottom.ToString();
				right = dLonRight.ToString();
				latitudeUser = ((double.Parse(top) + double.Parse(bottom)) / 2.0f).ToString();
				longitudeUser = ((double.Parse(left) + double.Parse(right)) / 2.0f).ToString();

				areaSizeLat = (float)GetDistanceFromLatLonInKm(double.Parse(top), double.Parse(left), double.Parse(bottom), double.Parse(left));
				areaSizeLon = (float)GetDistanceFromLatLonInKm(double.Parse(top), double.Parse(left), double.Parse(top), double.Parse(right));

				SetUnitsTo1Meter();
			}
			catch{}
		}
		*/

		public static string Between(string value, string a, string b)
		{
			int posA = value.IndexOf(a);
			int posB = value.LastIndexOf(b);

			if (posA == -1)
				return "";

			if (posB == -1)
				return "";

			int adjustedPosA = posA + a.Length;

			if (adjustedPosA >= posB)
				return "";

			return value.Substring(adjustedPosA, posB - adjustedPosA);
		}

		private double GetDistanceFromLatLonInKm(double lat1, double lon1, double lat2, double lon2)
		{
			double R = 6378137.0; // Radius of the earth in km
			double dLat = Deg2rad(lat2 - lat1);  // deg2rad below
			double dLon = Deg2rad(lon2 - lon1);

			double a = Math.Sin(dLat / 2.0) * Math.Sin(dLat / 2.0) +
				Math.Cos(Deg2rad(lat1)) * Math.Cos(Deg2rad(lat2)) *
					Math.Sin(dLon / 2.0) * Math.Sin(dLon / 2.0);

			double c = 2.0 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1.0 - a));
			double d = (R * c) / 1000.0; // Distance in km
			return d;
		}

		private double Deg2rad(double deg)
		{
			return deg * (Math.PI / 180.0);
		}

		private void GetInitialTerrainHeights()
		{
			if (splittedTerrains)
			{
				initialHeightsValue = new float[croppedTerrains.Count];

				for (int i = 0; i < croppedTerrains.Count; i++)
					initialHeightsValue[i] = croppedTerrains[i].terrainData.size.y;
			}
			else if (terrain)
			{
				initialHeightsValue = new float[1];
				initialHeightsValue[0] = terrain.terrainData.size.y;
			}

			heightsAnalyzed = true;
		}

		private void CheckTerrainChunks()
		{
			if (splittedTerrains.transform.childCount == 0)
			{
				EditorUtility.DisplayDialog("UNAVAILABLE TERRAINS", "There are no terrains available in the selected game object.", "Ok");
				splittedTerrains = null;
				return;
			}
			else
			{
				int counter = 0;

				foreach (Transform t in splittedTerrains.transform)
				{
					if (t.GetComponent<Terrain>() != null)
					{
						if (counter == 0)
							croppedTerrains = new List<Terrain>();

						croppedTerrains.Add(t.GetComponent<Terrain>());
						counter++;
					}
				}

				terrainChunks = counter;
			}
		}

		private void GetData()
		{
			size = (int)enumValue;
			terrainsLong = size;
			terrainsWide = size;

			baseData = baseTerrain.terrainData;

			oldWidth = baseData.size.x;
			oldHeight = baseData.size.y;
			oldLength = baseData.size.z;

			newWidth = oldWidth / terrainsWide;
			newLength = oldLength / terrainsLong;

			xPos = baseTerrain.GetPosition().x;
			yPos = baseTerrain.GetPosition().y;
			zPos = baseTerrain.GetPosition().z;

			newHeightMapResolution = ((baseData.heightmapResolution - 1) / size) + 1;
			newEvenHeightMapResolution = newHeightMapResolution - 1;

			newDetailResolution = baseData.detailResolution;
			newAlphaMapResolution = baseData.alphamapResolution;
			newBaseMapResolution = baseData.baseMapResolution;

			treeDistance = baseTerrain.treeDistance;
			treeBillboardDistance = baseTerrain.treeBillboardDistance;
			treeCrossFadeLength = baseTerrain.treeCrossFadeLength;
			treeMaximumFullLODCount = baseTerrain.treeMaximumFullLODCount;
			detailObjectDistance = baseTerrain.detailObjectDistance;
			detailObjectDensity = baseTerrain.detailObjectDensity;
			heightmapPixelError = baseTerrain.heightmapPixelError;
			heightmapMaximumLOD = baseTerrain.heightmapMaximumLOD;
			basemapDistance = baseTerrain.basemapDistance;
			lightmapIndex = baseTerrain.lightmapIndex;
			castShadows = baseTerrain.castShadows;
			editorRenderFlags = baseTerrain.editorRenderFlags;
			materialTemplate = baseTerrain.materialTemplate;

			grassStrength = baseData.wavingGrassStrength;
			grassAmount = baseData.wavingGrassAmount;
			grassSpeed = baseData.wavingGrassSpeed;
			grassTint = baseData.wavingGrassTint;
		}

		private bool ErrorsPass()
		{
			if (newHeightMapResolution < 33)
			{
				EditorUtility.DisplayDialog("INSUFFICIENT HEIGHTMAP RESOLUTION", "Heightmap Resolution for tiled terrains have smaller values than 33.\n\nEither Base Terrain must have higher resolution or a smaller TILES GRID value must be selected.", "Ok");
				return false;
			}
			else
				return true;
		}

		private void CreateTerrainData()
		{
			terrainGameObjects = new GameObject[terrainsLong * terrainsWide];
			terrains = new Terrain[terrainsLong * terrainsWide];
			data = new TerrainData[terrainsLong * terrainsWide];

			if (newTerrainGeneration)
				terrainName = "Terrain";
			else
				terrainName = baseTerrain.name;

			for (int y = 0; y < terrainsLong; y++)
			{
				for (int x = 0; x < terrainsWide; x++)
				{
					TerrainData td = new TerrainData();
					AssetDatabase.CreateAsset(td, splitDirectoryPath.Substring(splitDirectoryPath.LastIndexOf("Assets")) + "/" + terrainName + " " + (y + 1) + "-" + (x + 1) + ".asset");
					//EditorUtility.DisplayProgressBar("CREATING DATA", "Creating Terrain Data Assets", Mathf.InverseLerp(0f, terrainsLong, y));
				}
			}
			//AssetDatabase.SaveAssets();
			//AssetDatabase.Refresh();
			//EditorUtility.ClearProgressBar();
		}

		private void CreateTerrainObject()
		{
			arrayPos = 0;

			if (size > 1)
				terrainsParent = new GameObject("Terrains  " + downloadDateElevation + "  ---  " + size + "x" + size);

			int currentRow = size;

			if (!newTerrainGeneration && needsResampling)
			{
				int croppedResolution = Mathf.RoundToInt(((float)baseData.heightmapResolution) / (float)size);
				croppedResolutionHeightmap = Mathf.ClosestPowerOfTwo(croppedResolution) + 1;
				croppedResolutionBase = croppedResolutionHeightmap * size;

				float[,] baseHeights = baseData.GetHeights(0, 0, baseData.heightmapResolution, baseData.heightmapResolution);

				ResampleOperationSplitter(terrain, croppedResolutionBase, baseHeights);
			}

			for (int y = 0; y < terrainsLong; y++)
			{
				for (int x = 0; x < terrainsWide; x++)
				{
					TerrainData td = (TerrainData)AssetDatabase.LoadAssetAtPath(splitDirectoryPath.Substring(splitDirectoryPath.LastIndexOf("Assets")) + "/" + terrainName + " " + (currentRow) + "-" + (x + 1) + ".asset", typeof(TerrainData)) as TerrainData;
					terrainGameObjects[arrayPos] = Terrain.CreateTerrainGameObject(td);
					terrainGameObjects[arrayPos].name = terrainName + " " + (currentRow) + "-" + (x + 1);
					terrains[arrayPos] = terrainGameObjects[arrayPos].GetComponent<Terrain>();

#if UNITY_2018_3_OR_NEWER
					terrains[arrayPos].drawInstanced = true;
					terrains[arrayPos].groupingID = 0;
					terrains[arrayPos].allowAutoConnect = true;
#endif

					data[arrayPos] = terrains[arrayPos].terrainData;
					data[arrayPos].heightmapResolution = newEvenHeightMapResolution;
					data[arrayPos].size = new Vector3(newWidth, oldHeight, newLength);

					if (!newTerrainGeneration)
					{
						data[arrayPos].alphamapResolution = newAlphaMapResolution;
						data[arrayPos].baseMapResolution = newBaseMapResolution;
						data[arrayPos].SetDetailResolution(newDetailResolution, 128);
						data[arrayPos].wavingGrassStrength = grassStrength;
						data[arrayPos].wavingGrassAmount = grassAmount;
						data[arrayPos].wavingGrassSpeed = grassSpeed;
						data[arrayPos].wavingGrassTint = grassTint;

						if (needsResampling)
						{
							data[arrayPos].heightmapResolution = croppedResolutionHeightmap;

							float croppedWidth = baseData.size.x / (float)size;
							float croppedHeight = baseData.size.y;
							float croppedLength = baseData.size.z / (float)size;
							data[arrayPos].size = new Vector3(croppedWidth, croppedHeight, croppedLength);

							int xStart = x * (croppedResolutionHeightmap - 1);
							int yStart = y * (croppedResolutionHeightmap - 1);

							float[,] croppedHeights = new float[croppedResolutionHeightmap, croppedResolutionHeightmap];

							for (int j = 0; j < croppedResolutionHeightmap; j++)
							{
								for (int i = 0; i < croppedResolutionHeightmap; i++)
								{
									croppedHeights[i, j] = resampledHeights[i + yStart, j + xStart];
								}
							}

							data[arrayPos].SetHeights(0, 0, croppedHeights);
						}
						else
						{
							int xStart = x * (newHeightMapResolution - 1);
							int yStart = y * (newHeightMapResolution - 1);
							int width = newHeightMapResolution;
							int hight = newHeightMapResolution;

							data[arrayPos].SetHeights(0, 0, baseData.GetHeights(xStart, yStart, width, hight));
						}
					}

					terrainGameObjects[arrayPos].GetComponent<TerrainCollider>().terrainData = data[arrayPos];
					terrainGameObjects[arrayPos].transform.position = new Vector3(x * newWidth + xPos, yPos, y * newLength + zPos);

					arrayPos++;

					//EditorUtility.DisplayProgressBar("CREATING TERRAIN", "Creating Terrain Objects", Mathf.InverseLerp(0f, terrainsWide, y));
				}
				currentRow--;
			}

			//EditorUtility.ClearProgressBar();

			if (size > 1)
			{
				int length = terrainGameObjects.Length;
				String[] terrainNames = new string[length];
				GameObject tempParnet = new GameObject("Temp Parent");

				for (int i = 0; i < terrainGameObjects.Length; i++)
				{
					terrainNames[i] = terrainGameObjects[i].name;
					terrainGameObjects[i].transform.parent = tempParnet.transform;
				}

				terrainNames = LogicalComparer(terrainNames);

				for (int i = 0; i < length; i++)
				{
					terrainGameObjects[i] = tempParnet.transform.Find(terrainNames[i]).gameObject;
					terrainGameObjects[i].transform.parent = terrainsParent.transform;
				}

				DestroyImmediate(tempParnet);
			}

			int terrainsCount = terrains.Length;

			for (int y = 0; y < terrainsCount; y++)
			{
				terrains[y].heightmapPixelError = heightmapPixelError;

				if (!newTerrainGeneration)
				{
					terrains[y].treeDistance = treeDistance;
					terrains[y].treeBillboardDistance = treeBillboardDistance;
					terrains[y].treeCrossFadeLength = treeCrossFadeLength;
					terrains[y].treeMaximumFullLODCount = treeMaximumFullLODCount;
					terrains[y].detailObjectDistance = detailObjectDistance;
					terrains[y].detailObjectDensity = detailObjectDensity;
					terrains[y].heightmapMaximumLOD = heightmapMaximumLOD;
					terrains[y].basemapDistance = basemapDistance;
					terrains[y].lightmapIndex = lightmapIndex;
					terrains[y].castShadows = castShadows;
					terrains[y].editorRenderFlags = editorRenderFlags;
					terrains[y].materialTemplate = materialTemplate; //If using this file on a 3.x.x version of Unity, you will need to comment out the below line
				}
			}
		}

		private string[] LogicalComparer(string filePath, string fileType)
		{
			string[] names = Directory.GetFiles(filePath, "*" + fileType, SearchOption.AllDirectories);
			ns.NumericComparer ns = new ns.NumericComparer();
			Array.Sort(names, ns);

			return names;
		}

		private string[] LogicalComparer(string[] names)
		{
			ns.NumericComparer ns = new ns.NumericComparer();
			Array.Sort(names, ns);

			return names;
		}

		private float[,] ResampleOperationSplitter(Terrain splitTerrain, float croppedResolution, float[,] croppedHeights)
		{
			float resolutionFinal = splitTerrain.terrainData.heightmapResolution;
			resampledHeights = new float[(int)croppedResolution, (int)croppedResolution];
			float scaleFactorLat = (croppedResolution) / (resolutionFinal);
			float scaleFactorLon = (croppedResolution) / (resolutionFinal);

			for (int x = 0; x < croppedResolution; x++)
				for (int y = 0; y < croppedResolution; y++)
					resampledHeights[x, y] = ResampleHeightsSplitter((float)x / scaleFactorLat, (float)y / scaleFactorLon, croppedHeights, croppedResolution);

			FinalizeSmooth(resampledHeights, (int)croppedResolution, (int)croppedResolution, smoothIterationsSplitter, smoothBlendIndexSplitter, smoothBlendSplitter);

			showProgressSmoothen = false;
			showProgressSmoothenOperation = false;

			return resampledHeights;
		}

		private float ResampleHeightsSplitter(float X, float Y, float[,] croppedHeights, float croppedResolution)
		{
			try
			{
				int X1 = Mathf.RoundToInt((X + croppedResolution % croppedResolution));
				int Y1 = Mathf.RoundToInt((Y + croppedResolution % croppedResolution));

				float FinalValue = croppedHeights[X1, Y1];

				return FinalValue;
			}
			catch
			{
				return 0f;
			}
		}

		private void SetTerrainNeighbors()
		{
#if UNITY_2018_3_OR_NEWER
			for (int i = 0; i < (int)Mathf.Pow(splitSizeFinal, 2); i++)
			{
				croppedTerrains[i].groupingID = 0;
				croppedTerrains[i].allowAutoConnect = true;
			}
#else
        terrainsLong = splitSizeFinal;
        terrainsWide = splitSizeFinal;
		arrayPos = 0;
		
		for(int y = 0; y < terrainsLong ; y++)
		{
			for(int x = 0; x < terrainsWide; x++)
			{
				try
				{
					int indexLft = arrayPos - 1;
					int indexTop = arrayPos - terrainsWide;
					int indexRgt = arrayPos + 1;
					int indexBtm = arrayPos + terrainsWide;
					
					if(y == 0)
					{
						if(x == 0)
							croppedTerrains[arrayPos].SetNeighbors(null, null, croppedTerrains[indexRgt], croppedTerrains[indexBtm]);
						else if(x == terrainsWide - 1)
							croppedTerrains[arrayPos].SetNeighbors(croppedTerrains[indexLft], null, null, croppedTerrains[indexBtm]);
						else
							croppedTerrains[arrayPos].SetNeighbors(croppedTerrains[indexLft], null, croppedTerrains[indexRgt], croppedTerrains[indexBtm]);
					}
					else if(y == terrainsLong - 1)
					{
						if(x == 0)
							croppedTerrains[arrayPos].SetNeighbors(null, croppedTerrains[indexTop], croppedTerrains[indexRgt], null);
						else if(x == terrainsWide - 1)
							croppedTerrains[arrayPos].SetNeighbors(croppedTerrains[indexLft], croppedTerrains[indexTop], null, null);
						else
							croppedTerrains[arrayPos].SetNeighbors(croppedTerrains[indexLft], croppedTerrains[indexTop], croppedTerrains[indexRgt], null);
					}
					else
					{
						if(x == 0)
							croppedTerrains[arrayPos].SetNeighbors(null, croppedTerrains[indexTop], croppedTerrains[indexRgt], croppedTerrains[indexBtm]);
						else if(x == terrainsWide - 1)
							croppedTerrains[arrayPos].SetNeighbors(croppedTerrains[indexLft], croppedTerrains[indexTop], null, croppedTerrains[indexBtm]);
                        else
                            croppedTerrains[arrayPos].SetNeighbors(croppedTerrains[indexLft], croppedTerrains[indexTop], croppedTerrains[indexRgt], croppedTerrains[indexBtm]);
                    }
                    
                    arrayPos++;
                }
                catch{}
                
                EditorUtility.DisplayProgressBar("SETTING NEIGHBORS", "Setting Terrain Neighbors", Mathf.InverseLerp(0f, terrainsWide, y));
            }
        }
        
		try
        {
            for(int i = 0; i < terrainsWide * terrainsLong ; i++)
                croppedTerrains[i].Flush();
		}
		catch{}
		
        EditorUtility.ClearProgressBar();
#endif
		}

		private void GCAndRecompile()
		{
			AssetDatabase.Refresh();
			Resources.UnloadUnusedAssets();
		}

		private void GetImageryFolderInfo()
		{
			IEnumerable<string> names = Directory.GetFiles(UnityEditor.AssetDatabase.GetAssetPath(tileFolder), "*.*", SearchOption.AllDirectories)
				.Where(s => s.EndsWith(".jpg")
					   || s.EndsWith(".png")
					   || s.EndsWith(".gif")
					   || s.EndsWith(".bmp")
					   || s.EndsWith(".tga")
					   || s.EndsWith(".psd")
					   || s.EndsWith(".tiff")
					   || s.EndsWith(".iff")
					   || s.EndsWith(".pict"));

			imageFiles = names.ToArray();
			//string fileType = imageFiles[0].Substring(imageFiles[0].LastIndexOf('.'));
			imageFiles = LogicalComparer(imageFiles);

			int imageCount = imageFiles.Length;

			// Check if there are any images available in folder(s)
			if (imageCount == 0)
			{
				EditorUtility.DisplayDialog("NO AVILABLE IMAGES", "There Are No Images Available In Selected Folder.", "Ok");
				tileFolder = null;
				return;
			}

			//directoryCount = Directory.GetDirectories(UnityEditor.AssetDatabase.GetAssetPath(tileFolder)).Length;

			//// Check if terrain chunks parent object is available
			//if (!terrain && directoryCount > 0 && !splittedTerrains)
			//{
			//	EditorUtility.DisplayDialog("TILING TERRAINS NOT SET", "Select tiling terrains parent in \"TERRAIN CHUNKS\" field from hierarchy panel.", "Ok");
			//	tileFolder = null;
			//	return;
			//}

			//// Check if single terrain object is available
			//if(directoryCount == 0 && !terrain)
			//{
			//	EditorUtility.DisplayDialog("TERRAIN NOT SET", "Select terrain gameobject in \"TERRAIN\" field from hierarchy panel.", "Ok");
			//	tileFolder = null;
			//	return;
			//}

			//// Check if number of terrain chunks is equal to number of avilable image folders in selected directory
			//if(directoryCount > 0 && splittedTerrains)
			//{
			//	if(directoryCount != terrainChunks)
			//	{
			//		EditorUtility.DisplayDialog("NON-MATCHING ASSETS", "Number of terrains in Selected TERRAIN CHUNKS does not match with number of Terrain folders inside selected directory.", "Ok");
			//		tileFolder = null;
			//		return;
			//	}
			//}

			//// Check if terrain chunks parent is selected for multiple terrains tiling process
			//if(directoryCount > 0 && !splittedTerrains)
			//{
			//	EditorUtility.DisplayDialog("TILING TERRAINS NOT SET", "Selected folder consists of multiple terrain chunks tiling.\n\nSelect tiling terrains parent in \"TERRAIN CHUNKS\" field from hierarchy panel.", "Ok");
			//	tileFolder = null;
			//	return;
			//}

			if (imageCount < terrainChunks)
			{
				EditorUtility.DisplayDialog("NOT ENOUGH TEXTURES", "Image count is less than available terrain tiles!", "Ok");
				tileFolder = null;
				return;
			}

			// Check if there are correct number of images available in folder(s) for proper tiling
			float subtract = (float)imageCount / terrainChunks;

			if (imageCount % subtract != 0)
			{
				EditorUtility.DisplayDialog("EXTRA/INSUFFICIENT IMAGE FILES", "There are whether insufficient or extra image files existing in selected directory", "Ok");
				tileFolder = null;
				return;
			}

			// If all above conditions are ok, calculate values of "image number" & "grid size" for terrain(s)
			if (splittedTerrains)
			{
				multipleTerrainsTiling = true;
				imagesPerTerrain = imageCount / terrainChunks;
				tileGrid = (int)Mathf.Sqrt(imagesPerTerrain);

				str1 = "Total Terrains: " + terrainChunks;
				str2 = "Grid per Terrain: " + tileGrid + "   Image per Terrain: " + imagesPerTerrain + "   Image Count: " + imageCount;
			}
			else
			{
				multipleTerrainsTiling = false;
				tileGrid = (int)Mathf.Sqrt(imageCount);

				str1 = "Single Terrain";
				str2 = "Grid: " + tileGrid + "   Image Count: " + imageCount;
			}
		}

		private void ImageTilerOffline(string[] allImageNames)
		{
			int totalImages = allImageNames.Length;

			// Importing Finalizer
			for (int i = 0; i < totalImages; i++)
			{
				try
				{
					UnityEngine.Object asset = AssetDatabase.LoadAssetAtPath(allImageNames[i].Substring(allImageNames[i].LastIndexOf("Assets")), typeof(UnityEngine.Object)) as UnityEngine.Object;
					TextureImporter textureImporter = AssetImporter.GetAtPath(AssetDatabase.GetAssetPath(asset)) as TextureImporter;

					if (textureImporter != null && textureImporter.isReadable == false)
					{
						textureImporter.isReadable = true;
						textureImporter.mipmapEnabled = true;
						textureImporter.wrapMode = TextureWrapMode.Clamp;

						//string terrainInfoPath = directoryPathImagery + "/Terrain Info.tlps";

						//if (File.Exists(terrainInfoPath) && terrainInfoPath.Contains("tlps"))
						//{
							string text = File.ReadAllText(presetFilePath);
							string[] dataLines = text.Split('\n');
							string[][] dataPairs = new string[dataLines.Length][];

							textureImporter.anisoLevel = int.Parse(dataPairs[35][1]);
							textureImporter.maxTextureSize = int.Parse(dataPairs[30][1]);
						//}

						EditorUtility.DisplayProgressBar("IMPORTING IMAGE", "Image  " + (i + 1).ToString() + "  of  " + totalImages.ToString(), Mathf.InverseLerp(0f, (float)(totalImages - 1), (float)(i)));
						AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(asset), ImportAssetOptions.ForceUpdate);
					}
				}
				catch (IndexOutOfRangeException)
				{
					// Error caught
				}
			}

			EditorUtility.ClearProgressBar();

			int counter = 0;

			if (!multipleTerrainsTiling)
			{
				cellSizeX = terrainSizeX / (float)tileGrid;
				cellSizeY = terrainSizeY / (float)tileGrid;

				float imageXOffsetF = 0f;
				float imageYOffsetF = 0f;
				int totalTileImages = Mathf.RoundToInt(Mathf.Pow(tileGrid, 2));

#if UNITY_2018_3_OR_NEWER

				if (terrain.terrainData.terrainLayers != null && terrain.terrainData.terrainLayers.Length > 0)
				{
					foreach (TerrainLayer t in terrain.terrainData.terrainLayers)
					{
						try
						{
							string assetPath = Path.GetFullPath(AssetDatabase.GetAssetPath(t));

							if (File.Exists(assetPath))
								File.Delete(assetPath);
						}
						catch { }
					}

					terrain.terrainData.terrainLayers = null;
				}

				TerrainLayer[] terrainLayers = new TerrainLayer[totalTileImages];

				for (int i = 0; i < tileGrid; i++)
				{
					for (int j = 0; j < tileGrid; j++)
					{
						try
						{
							imageXOffsetF = (terrainSizeX - (cellSizeX * ((float)tileGrid - (float)j))) * -1f;
							imageYOffsetF = (terrainSizeY - cellSizeY - ((float)cellSizeY * (float)i)) * -1f;

							if (imageXOffsetF > 0f)
								imageXOffsetF = 0f;
							if (imageYOffsetF > 0f)
								imageYOffsetF = 0f;

							Texture2D satelliteImage = AssetDatabase.LoadAssetAtPath(allImageNames[counter], typeof(Texture2D)) as Texture2D;

							// Texturing Terrain
							terrainLayers[counter] = new TerrainLayer();
							string layersPath = Path.GetDirectoryName(AssetDatabase.GetAssetPath(satelliteImage));
							string layerName = layersPath + "/" + satelliteImage.name.Replace(tempPattern, "") + ".terrainlayer";
							AssetDatabase.CreateAsset(terrainLayers[counter], layerName);
							terrainLayers[counter].diffuseTexture = satelliteImage;
							terrainLayers[counter].tileSize = new Vector2(cellSizeX, cellSizeY);
							terrainLayers[counter].tileOffset = new Vector2(imageXOffsetF, imageYOffsetF);

							EditorUtility.DisplayProgressBar("LOADING SATELLITE IMAGES", "Loading Image:  " + (counter + 1).ToString(), Mathf.InverseLerp(0.0f, (float)(totalTileImages - 1), (float)(counter + 1)));

							counter++;
						}
						catch (IndexOutOfRangeException)
						{
							// Error caught
						}
					}
				}

				EditorUtility.ClearProgressBar();

				terrain.terrainData.terrainLayers = terrainLayers;
#else
terrainTextures = new SplatPrototype[totalTileImages];
			
			for (int i = 0; i < tileGrid; i++)
			{
				for (int j = 0; j < tileGrid; j++)
				{
					try
					{
						imageXOffsetF = (terrainSizeX - (cellSizeX * ((float)tileGrid - (float)j))) * -1f;
						imageYOffsetF = (terrainSizeY - cellSizeY - ((float)cellSizeY * (float)i)) * -1f;
						
						if (imageXOffsetF > 0f)
							imageXOffsetF = 0f;
						if (imageYOffsetF > 0f)
							imageYOffsetF = 0f;
						
						Texture2D satelliteImage = AssetDatabase.LoadAssetAtPath(allImageNames[counter], typeof(Texture2D)) as Texture2D;
						
						// Texturing Terrain
						terrainTextures[counter] = new SplatPrototype();
						terrainTextures[counter].texture = satelliteImage;
						terrainTextures[counter].tileSize = new Vector2(cellSizeX, cellSizeY);
						terrainTextures[counter].tileOffset = new Vector2(imageXOffsetF, imageYOffsetF);
						
						EditorUtility.DisplayProgressBar("LOADING SATELLITE IMAGES", "Loading Image:  " + (counter + 1).ToString(), Mathf.InverseLerp(0.0f, (float)(totalTileImages - 1), (float)(counter  + 1)));
						
						counter++;
					}
					catch(IndexOutOfRangeException)
                    {
						// Error caught
					}
				}
			}
			
			EditorUtility.ClearProgressBar();
			
			terrain.terrainData.splatPrototypes = terrainTextures;
#endif

				splatNormalizeX = terrain.terrainData.size.x / terrain.terrainData.alphamapResolution;
				splatNormalizeY = terrain.terrainData.size.z / terrain.terrainData.alphamapResolution;

				float[] lengthz = new float[totalTileImages];
				float[] widthz = new float[totalTileImages];
				float[] lengthzOff = new float[totalTileImages];
				float[] widthzOff = new float[totalTileImages];

				for (int i = 0; i < totalTileImages; i++)
				{
					try
					{
#if UNITY_2018_3_OR_NEWER
						lengthz[i] = terrain.terrainData.terrainLayers[i].tileSize.y / splatNormalizeY;
						widthz[i] = terrain.terrainData.terrainLayers[i].tileSize.x / splatNormalizeX;
						lengthzOff[i] = terrain.terrainData.terrainLayers[i].tileOffset.y / splatNormalizeY;
						widthzOff[i] = terrain.terrainData.terrainLayers[i].tileOffset.x / splatNormalizeX;
#else
                    lengthz[i] = terrain.terrainData.splatPrototypes[i].tileSize.y / splatNormalizeY;
				    widthz[i] = terrain.terrainData.splatPrototypes[i].tileSize.x / splatNormalizeX;
				    lengthzOff[i] = terrain.terrainData.splatPrototypes[i].tileOffset.y / splatNormalizeY;
				    widthzOff[i] = terrain.terrainData.splatPrototypes[i].tileOffset.x / splatNormalizeX;
#endif

						smData = new float[Mathf.RoundToInt(lengthz[i]), Mathf.RoundToInt(widthz[i]), terrain.terrainData.alphamapLayers];

						for (int y = 0; y < Mathf.RoundToInt(lengthz[i]); y++)
							for (int z = 0; z < Mathf.RoundToInt(widthz[i]); z++)
								smData[y, z, i] = 1;

						int alphaXOffset = Mathf.RoundToInt(-widthzOff[i]);
						int alphaYOffset = Mathf.RoundToInt(-lengthzOff[i]);

						terrain.terrainData.SetAlphamaps(alphaXOffset, alphaYOffset, smData);

						EditorUtility.DisplayProgressBar("TEXTURING TERRAIN", "Texturing Terrain   " + (i + 1).ToString() + "  of  " + totalTileImages.ToString(), Mathf.InverseLerp(0.0f, (float)(totalTileImages - 1), (float)(i + 1)));
					}
					catch (IndexOutOfRangeException)
					{
						// Error caught
					}
				}

				EditorUtility.ClearProgressBar();
				AssetDatabase.Refresh();

				terrain.terrainData.RefreshPrototypes();
				terrain.Flush();

				terrainTextures = null;
				smData = null;
				allImageNames = null;

				UnityEngine.Object terrainDataAsset = terrain.terrainData;
				AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(terrainDataAsset), ImportAssetOptions.ForceUpdate);
			}
			else
			{
				int gridPerTerrain = tileGrid;
				int imagesPerTerrain = Mathf.RoundToInt(Mathf.Pow(gridPerTerrain, 2));
				int index = 0;
				float terrainSizeSplittedX = croppedTerrains[0].terrainData.size.x;
				float terrainSizeSplittedY = croppedTerrains[0].terrainData.size.z;
				float cellSizeSplittedX = terrainSizeSplittedX / (float)gridPerTerrain;
				float cellSizeSplittedY = terrainSizeSplittedY / (float)gridPerTerrain;
				imageXOffset = new float[imagesPerTerrain];
				imageYOffset = new float[imagesPerTerrain];

				for (int i = 0; i < gridPerTerrain; i++)
				{
					for (int j = 0; j < gridPerTerrain; j++)
					{
						imageXOffset[index] = (terrainSizeSplittedX - (cellSizeSplittedX * ((float)gridPerTerrain - (float)j))) * -1f;
						imageYOffset[index] = (terrainSizeSplittedY - cellSizeSplittedY - ((float)cellSizeSplittedY * (float)i)) * -1f;

						if (imageXOffset[index] > 0f)
							imageXOffset[index] = 0f;
						if (imageYOffset[index] > 0f)
							imageYOffset[index] = 0f;

						index++;
					}
				}

				List<Terrain> stitchingTerrains = OrderedTerrainChunks(splittedTerrains);

				index = 0;
				int imageIndex = 0;
				int offset = -1;
				int totalTiles = stitchingTerrains.Count;
				int gridTerrains = (int)Mathf.Sqrt(totalTiles);
				int gridImages = (int)Mathf.Sqrt(totalImages);
				int pad = gridImages - gridPerTerrain;
				int reverse = gridImages * (gridPerTerrain - 1);
				int[] index2D = new int[totalImages];

				foreach (Terrain terrainSplitted in stitchingTerrains)
				{
					for (int i = 0; i < imagesPerTerrain; i++)
					{
						offset++;

						if (gridPerTerrain > 1 && i > 0 && i % gridPerTerrain == 0)
							offset += pad;

						index2D[imageIndex] = offset;

						imageIndex++;
					}

					if (gridPerTerrain > 1 && (index + 1) % gridTerrains != 0)
						offset -= reverse;

					index++;
				}

				index = 0;
				imageIndex = 0;

				foreach (Terrain terrainSplitted in stitchingTerrains)
				{
#if UNITY_2018_3_OR_NEWER
					TerrainLayer[] terrainLayers = new TerrainLayer[imagesPerTerrain];

					for (int i = 0; i < imagesPerTerrain; i++)
					{
						string name = allImageNames[index2D[imageIndex]].Substring(allImageNames[index2D[imageIndex]].LastIndexOf("Assets"));
						Texture2D satelliteImage = AssetDatabase.LoadAssetAtPath(name, typeof(Texture2D)) as Texture2D;

						// Texturing Terrain
						terrainLayers[i] = new TerrainLayer();
						string layersPath = Path.GetDirectoryName(AssetDatabase.GetAssetPath(satelliteImage));
						string layerName = layersPath + "/" + satelliteImage.name.Replace(tempPattern, "") + ".terrainlayer";
						AssetDatabase.CreateAsset(terrainLayers[i], layerName);
						terrainLayers[i].diffuseTexture = satelliteImage;
						terrainLayers[i].tileSize = new Vector2(cellSizeSplittedX, cellSizeSplittedY);
						terrainLayers[i].tileOffset = new Vector2(imageXOffset[i], imageYOffset[i]);

						imageIndex++;
					}

					terrainSplitted.terrainData.terrainLayers = terrainLayers;
#else
                terrainTextures = new SplatPrototype[imagesPerTerrain];

                for (int i = 0; i < imagesPerTerrain; i++)
                {
                    string name = allImageNames[index2D[imageIndex]].Substring(allImageNames[index2D[imageIndex]].LastIndexOf("Assets"));
                    Texture2D satelliteImage = UnityEditor.AssetDatabase.LoadAssetAtPath(name, typeof(Texture2D)) as Texture2D;

                    // Texturing Terrain
                    terrainTextures[i] = new SplatPrototype();
                    terrainTextures[i].texture = satelliteImage;
                    terrainTextures[i].tileSize = new Vector2(cellSizeSplittedX, cellSizeSplittedY);
                    terrainTextures[i].tileOffset = new Vector2(imageXOffset[i], imageYOffset[i]);

                    imageIndex++;
                }

                terrainSplitted.terrainData.splatPrototypes = terrainTextures;
#endif

					splatNormalizeX = terrainSplitted.terrainData.size.x / terrainSplitted.terrainData.alphamapResolution;
					splatNormalizeY = terrainSplitted.terrainData.size.z / terrainSplitted.terrainData.alphamapResolution;

					float[] lengthz = new float[imagesPerTerrain];
					float[] widthz = new float[imagesPerTerrain];
					float[] lengthzOff = new float[imagesPerTerrain];
					float[] widthzOff = new float[imagesPerTerrain];

					for (int i = 0; i < imagesPerTerrain; i++)
					{
#if UNITY_2018_3_OR_NEWER
						lengthz[i] = terrainSplitted.terrainData.terrainLayers[i].tileSize.y / splatNormalizeY;
						widthz[i] = terrainSplitted.terrainData.terrainLayers[i].tileSize.x / splatNormalizeX;
						lengthzOff[i] = terrainSplitted.terrainData.terrainLayers[i].tileOffset.y / splatNormalizeY;
						widthzOff[i] = terrainSplitted.terrainData.terrainLayers[i].tileOffset.x / splatNormalizeX;
#else
                    lengthz[i] = terrainSplitted.terrainData.splatPrototypes[i].tileSize.y / splatNormalizeY;
			        widthz[i] = terrainSplitted.terrainData.splatPrototypes[i].tileSize.x / splatNormalizeX;
			        lengthzOff[i] = terrainSplitted.terrainData.splatPrototypes[i].tileOffset.y / splatNormalizeY;
			        widthzOff[i] = terrainSplitted.terrainData.splatPrototypes[i].tileOffset.x / splatNormalizeX;
#endif

						smData = new float[Mathf.RoundToInt(lengthz[i]), Mathf.RoundToInt(widthz[i]), terrainSplitted.terrainData.alphamapLayers];

						for (int y = 0; y < Mathf.RoundToInt(lengthz[i]); y++)
							for (int z = 0; z < Mathf.RoundToInt(widthz[i]); z++)
								smData[y, z, i] = 1;

						int alphaXOffset = Mathf.RoundToInt(-widthzOff[i]);
						int alphaYOffset = Mathf.RoundToInt(-lengthzOff[i]);

						terrainSplitted.terrainData.SetAlphamaps(alphaXOffset, alphaYOffset, smData);

						EditorUtility.DisplayProgressBar("TEXTURING TERRAIN " + (index + 1).ToString(), "Image   " + (i + 1).ToString() + "  of  " + imagesPerTerrain.ToString(), Mathf.InverseLerp(0.0f, (float)(imagesPerTerrain - 1), (float)(i + 1)));
					}

					AssetDatabase.Refresh();

					terrainSplitted.terrainData.RefreshPrototypes();
					terrainSplitted.Flush();

					terrainTextures = null;
					smData = null;

					UnityEngine.Object terrainDataAsset = terrainSplitted.terrainData;
					AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(terrainDataAsset), ImportAssetOptions.ForceUpdate);

					index++;
				}

				EditorUtility.ClearProgressBar();
			}

			AssetDatabase.Refresh();
		}

		public List<Terrain> OrderedTerrainChunks(GameObject terrainsParentGo)
		{
			string names = "";

			foreach (Transform child in terrainsParentGo.transform)
				names += child.name + Environment.NewLine;

			string[] lines = names.Replace("\r", "").Split('\n');
			lines = LogicalComparer(lines);

			List<Terrain> stitchingTerrains = new List<Terrain>();

			foreach (string s in lines)
				if (s != "")
					stitchingTerrains.Add(terrainsParentGo.transform.Find(s).GetComponent<Terrain>());

			names = null;

			return stitchingTerrains;
		}

		private void PickRawDefaults(string fileName)
		{
			FileStream stream = File.Open(fileName, FileMode.Open, FileAccess.Read);
			int length = (int)stream.Length;
			stream.Close();

			m_Depth = Depth.Bit16;
			int num2 = length / (int)m_Depth;
			int num3 = Mathf.RoundToInt(Mathf.Sqrt((float)num2));
			int num4 = Mathf.RoundToInt(Mathf.Sqrt((float)num2));

			if (((num3 * num4) * (int)m_Depth) == length)
			{
				m_Width = num3;
				m_Height = num4;

				heightmapResX = Mathf.FloorToInt((float)Mathf.ClosestPowerOfTwo(m_Width) / (float)splitSizeFinal);
				heightmapResY = Mathf.FloorToInt((float)Mathf.ClosestPowerOfTwo(m_Height) / (float)splitSizeFinal);
				heightmapResXAll = m_Width;
				heightmapResYAll = m_Height;

				return;
			}
			else
			{
				m_Depth = Depth.Bit8;
				num2 = length / (int)m_Depth;
				num3 = (int)Math.Round(Math.Sqrt((float)num2));
				num4 = (int)Math.Round(Math.Sqrt((float)num2));

				if (((num3 * num4) * (int)m_Depth) == length)
				{
					m_Width = num3;
					m_Height = num4;

					heightmapResX = Mathf.FloorToInt((float)Mathf.ClosestPowerOfTwo(m_Width) / (float)splitSizeFinal);
					heightmapResY = Mathf.FloorToInt((float)Mathf.ClosestPowerOfTwo(m_Height) / (float)splitSizeFinal);
					heightmapResXAll = m_Width;
					heightmapResYAll = m_Height;

					return;
				}

				m_Depth = Depth.Bit16;
			}
		}

		private void Terrain2Mesh()
		{
			if (splittedTerrains)
			{
				if (isScenePlace)
				{
					meshGenerationDate = System.DateTime.Now.ToString("/MM-dd-yyyy_HH-mm-ss");
					dataFolder = generatedMeshes + meshGenerationDate;
					Directory.CreateDirectory(dataFolder);
					meshName = dataFolder + "/Terrain.obj";
				}
				else
					meshName = EditorUtility.SaveFilePanel("Export .obj file", "", "Terrain", "obj");

				if (isScenePlace)
				{
					if (!meshName.Contains(dataPath))
					{
						EditorUtility.DisplayDialog("WRONG DIRECTORY", "Select A Folder Inside Project To Import Mesh.", "Ok");
						return;
					}
				}

				CheckTerrainChunks();

				int chunkHeightmapResolution = croppedTerrains[0].terrainData.heightmapResolution;
				int totalHeightmapResolution = chunkHeightmapResolution * splitSizeFinal;
				int w = totalHeightmapResolution;
				int h = totalHeightmapResolution;

				int tRes = (int)Mathf.Pow(2, (int)saveResolution);
				int wT = (w - 1) / tRes + 0;
				int hT = (h - 1) / tRes + 0;

				if (wT >= 256 && hT >= 256)
				{
					if (EditorUtility.DisplayDialog("HIGH VERTEX COUNT", "Generated Mesh Will Have a High Vertex and Poly Count.\n\nAre You Sure You Want To Continue?", "No", "Yes"))
					{
						Directory.Delete(dataFolder);
						return;
					}

					PrepareMeshMultiple();
				}
				else
					PrepareMeshMultiple();
			}
			else if (terrain)
			{
				if (isScenePlace)
				{
					meshGenerationDate = System.DateTime.Now.ToString("/MM-dd-yyyy_HH-mm-ss");
					dataFolder = generatedMeshes + meshGenerationDate;
					Directory.CreateDirectory(dataFolder);
					meshName = dataFolder + "/Terrain.obj";
				}
				else
					meshName = EditorUtility.SaveFilePanel("Export .obj file", "", "Terrain", "obj");

				if (isScenePlace)
				{
					if (!meshName.Contains(dataPath))
					{
						EditorUtility.DisplayDialog("WRONG DIRECTORY", "Select A Folder Inside Project To Import Mesh.", "Ok");
						return;
					}
				}

				TerrainData data = terrain.terrainData;
				int w = data.heightmapResolution;
				int h = data.heightmapResolution;

				int tRes = (int)Mathf.Pow(2, (int)saveResolution);
				int wT = (w - 1) / tRes + 1;
				int hT = (h - 1) / tRes + 1;

				if (wT >= 256 && hT >= 256)
				{
					if (EditorUtility.DisplayDialog("HIGH VERTEX COUNT", "Generated Mesh Will Have a High Vertex and Poly Count.\n\nAre You Sure You Want To Continue?", "No", "Yes"))
					{
						Directory.Delete(dataFolder);
						return;
					}

					PrepareMeshSingle();
				}
				else
					PrepareMeshSingle();
			}
			else
			{
				EditorUtility.DisplayDialog("UNAVAILABLE TERRAIN", unavailableTerrainStr, "Ok");
				return;
			}
		}

		private void PrepareMeshMultiple()
		{
			int chunkHeightmapResolution = croppedTerrains[0].terrainData.heightmapResolution;
			int totalHeightmapResolution = chunkHeightmapResolution * splitSizeFinal;
			finalHeights = new float[totalHeightmapResolution, totalHeightmapResolution];

			GetUberHeightmap(finalHeights, chunkHeightmapResolution);

			int w = finalHeights.GetLength(0);
			int h = finalHeights.GetLength(1);
			float meshScaleY = 0;

			foreach (Terrain t in croppedTerrains)
			{
				float currentElevation = t.terrainData.size.y;

				if (meshScaleY < currentElevation)
					meshScaleY = currentElevation;
			}

			float meshScaleX = croppedTerrains[0].terrainData.size.x * splitSizeFinal;
			float meshScaleZ = croppedTerrains[0].terrainData.size.z * splitSizeFinal;

			Vector3 meshScale = new Vector3(meshScaleX, meshScaleY, meshScaleZ);

			terrainPos = Vector3.zero;

			GenerateMesh(finalHeights, w, h, meshScale, terrainPos, meshName);

			if (isScenePlace)
			{
				// Getting Bottom-Left terrain in chunks
				Vector2 terrainPos1st = new Vector2(croppedTerrains[0].transform.position.x, croppedTerrains[0].transform.position.z);
				int index = 0;

				for (int i = 1; i < croppedTerrains.Count; i++)
				{
					Vector2 currentXZ = new Vector2(croppedTerrains[i].transform.position.x, croppedTerrains[i].transform.position.z);

					if (currentXZ.x <= terrainPos1st.x && currentXZ.y <= terrainPos1st.y)
						index = i;
				}

				terrainPos = croppedTerrains[index].transform.position;

				PlaceMesh(terrainPos);
			}
		}

		private void PrepareMeshSingle()
		{
			TerrainData data = terrain.terrainData;
			int w = data.heightmapResolution;
			int h = data.heightmapResolution;

			Vector3 meshScale = data.size;
			finalHeights = data.GetHeights(0, 0, w, h);
			terrainPos = Vector3.zero;

			GenerateMesh(finalHeights, w, h, meshScale, terrainPos, meshName);

			if (isScenePlace)
				PlaceMesh(terrain.transform.position);
		}

		private void GenerateMesh(float[,] tData, int w, int h, Vector3 meshScale, Vector3 terrainPosition, string fileName)
		{
			int tRes = (int)Mathf.Pow(2, (int)saveResolution);
			meshScale = new Vector3(meshScale.x / (w - 1) * tRes, meshScale.y, meshScale.z / (h - 1) * tRes);
			Vector2 uvScale = new Vector2(1.0f / (w - 1), 1.0f / (h - 1));

			w = (w - 1) / tRes + 1;
			h = (h - 1) / tRes + 1;

			Vector3[] tVertices = new Vector3[w * h];
			Vector2[] tUV = new Vector2[w * h];

			int[] tPolys;

			if (saveFormat == SaveFormat.Triangles)
				tPolys = new int[(w - 1) * (h - 1) * 6];
			else
				tPolys = new int[(w - 1) * (h - 1) * 4];

			// Build vertices and UVs
			for (int y = 0; y < h; y++)
			{
				for (int x = 0; x < w; x++)
				{
					tVertices[y * w + x] = Vector3.Scale(meshScale, new Vector3(-y, tData[x * tRes, y * tRes], x)) + terrainPosition;
					tUV[y * w + x] = Vector2.Scale(new Vector2(x * tRes, y * tRes), uvScale);
				}
			}

			int index = 0;
			if (saveFormat == SaveFormat.Triangles)
			{
				// Build triangle indices: 3 indices into vertex array for each triangle
				for (int y = 0; y < h - 1; y++)
				{
					for (int x = 0; x < w - 1; x++)
					{
						// For each grid cell output two triangles
						tPolys[index++] = (y * w) + x;
						tPolys[index++] = ((y + 1) * w) + x;
						tPolys[index++] = (y * w) + x + 1;

						tPolys[index++] = ((y + 1) * w) + x;
						tPolys[index++] = ((y + 1) * w) + x + 1;
						tPolys[index++] = (y * w) + x + 1;
					}
				}
			}
			else
			{
				// Build quad indices: 4 indices into vertex array for each quad
				for (int y = 0; y < h - 1; y++)
				{
					for (int x = 0; x < w - 1; x++)
					{
						// For each grid cell output one quad
						tPolys[index++] = (y * w) + x;
						tPolys[index++] = ((y + 1) * w) + x;
						tPolys[index++] = ((y + 1) * w) + x + 1;
						tPolys[index++] = (y * w) + x + 1;
					}
				}
			}

			// Export to .obj
			StreamWriter sw = new StreamWriter(fileName);

			try
			{
				sw.WriteLine("# Unity terrain OBJ File");

				// Write vertices
				System.Threading.Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("en-US");

				vertexCounter = 0;
				totalCount = (tVertices.Length * 2 + (saveFormat == SaveFormat.Triangles ? tPolys.Length / 3 : tPolys.Length / 4)) / progressUpdateInterval;

				for (int i = 0; i < tVertices.Length; i++)
				{
					UpdateProgress();

					StringBuilder sb = new StringBuilder("v ", 20);

					// StringBuilder stuff is done this way because it's faster than using the "{0} {1} {2}"etc. format
					// Which is important when you're exporting huge terrains.
					sb.Append(tVertices[i].x.ToString()).Append(" ").
						Append(tVertices[i].y.ToString()).Append(" ").
							Append(tVertices[i].z.ToString());

					sw.WriteLine(sb);
				}

				// Write UVs
				for (int i = 0; i < tUV.Length; i++)
				{
					UpdateProgress();

					StringBuilder sb = new StringBuilder("vt ", 22);

					sb.Append(tUV[i].x.ToString()).Append(" ").
						Append(tUV[i].y.ToString());
					sw.WriteLine(sb);
				}

				if (saveFormat == SaveFormat.Triangles)
				{
					// Write triangles
					for (int i = 0; i < tPolys.Length; i += 3)
					{
						UpdateProgress();

						StringBuilder sb = new StringBuilder("f ", 43);

						sb.Append(tPolys[i] + 1).Append("/").Append(tPolys[i] + 1).Append(" ").
							Append(tPolys[i + 1] + 1).Append("/").Append(tPolys[i + 1] + 1).Append(" ").
								Append(tPolys[i + 2] + 1).Append("/").Append(tPolys[i + 2] + 1);

						sw.WriteLine(sb);
					}
				}
				else
				{
					// Write quads
					for (int i = 0; i < tPolys.Length; i += 4)
					{
						UpdateProgress();

						StringBuilder sb = new StringBuilder("f ", 57);

						sb.Append(tPolys[i] + 1).Append("/").Append(tPolys[i] + 1).Append(" ").
							Append(tPolys[i + 1] + 1).Append("/").Append(tPolys[i + 1] + 1).Append(" ").
								Append(tPolys[i + 2] + 1).Append("/").Append(tPolys[i + 2] + 1).Append(" ").
								Append(tPolys[i + 3] + 1).Append("/").Append(tPolys[i + 3] + 1);

						sw.WriteLine(sb);
					}
				}
			}
			catch (Exception err)
			{
				UnityEngine.Debug.Log("Error saving file: " + err.Message);
			}
			sw.Close();

			data = null;
			EditorUtility.DisplayProgressBar("Saving file to disc.", "This might take a while...", 1f);
			EditorUtility.ClearProgressBar();

			AssetDatabase.Refresh();
		}

		private void UpdateProgress()
		{
			if (vertexCounter++ == progressUpdateInterval)
			{
				vertexCounter = 0;
				EditorUtility.DisplayProgressBar("Saving...", "", Mathf.InverseLerp(0, totalCount, ++vertexCounter));
			}
		}

		private void PlaceMesh(Vector3 terrainPosition)
		{
			GameObject model = AssetDatabase.LoadAssetAtPath(meshName.Substring(meshName.LastIndexOf("Assets")), typeof(GameObject)) as GameObject;
			GameObject terrainMesh = Instantiate(model, terrainPosition, Quaternion.identity);
			terrainMesh.name = "Terrain";
		}

		private void Mesh2Terrain()
		{
			bool disabled = false;

			if (!meshObject.activeSelf)
			{
				meshObject.SetActive(true);
				disabled = true;
			}

			collider = new List<MeshCollider>();

			//Add a collider to our source object if it does not exist, otherwise raycasting doesn't work.
			if (meshObject.GetComponent<MeshFilter>() != null)
			{
				if (meshObject.GetComponent<MeshCollider>() != null)
				{
					collider.Add(meshObject.GetComponent<MeshCollider>());
					cleanUp = null;
				}
				else
				{
					collider.Add(meshObject.AddComponent<MeshCollider>());
					cleanUp = () => DestroyImmediate(meshObject.GetComponent<MeshCollider>());
				}
			}
			else
			{
				Transform[] children = meshObject.GetComponentsInChildren<Transform>();

				foreach (Transform t in children)
				{
					if (t.gameObject.GetComponent<MeshFilter>() != null)
					{
						if (t.gameObject.GetComponent<MeshCollider>() != null)
						{
							collider.Add(t.gameObject.GetComponent<MeshCollider>());
							cleanUp = null;
							break; // 1st collider found
						}
						else
						{
							collider.Add(t.gameObject.AddComponent<MeshCollider>());
							cleanUp = () => DestroyImmediate(meshObject.GetComponent<MeshCollider>());
							break; // 1st collider found
						}
					}
				}
			}

			if (collider.Count == 0)
			{
				EditorUtility.DisplayDialog("MESH NOT AVAILABLE", "There Are No Available Meshes In The Selected Game Object.", "Ok");
				return;
			}

			ShowProgressBar(1, 100);

			terrainGenerationDate = System.DateTime.Now.ToString("/MM-dd-yyyy_HH-mm-ss");
			dataFolder = generatedTerrains + terrainGenerationDate;
			Directory.CreateDirectory(dataFolder);
			string terrainDataPath = dataFolder.Substring(dataFolder.LastIndexOf("Assets")) + "/TerrainData.asset";

			TerrainData data = new TerrainData();
			AssetDatabase.CreateAsset(new TerrainData(), terrainDataPath);
			data = AssetDatabase.LoadAssetAtPath(terrainDataPath, typeof(TerrainData)) as TerrainData;

			data.heightmapResolution = resolutionMesh + 1;
			GameObject terrainObject = Terrain.CreateTerrainGameObject(data);
			terrainObject.name = "Terrain";

			Terrain ter = terrainObject.GetComponent<Terrain>();
			ter.terrainData = data;

#if UNITY_2018_3_OR_NEWER
			ter.drawInstanced = true;
			ter.groupingID = 0;
			ter.allowAutoConnect = true;
#endif

			RemoveLightmapStatic(terrainObject.GetComponent<Terrain>());

			Bounds bounds = collider[0].bounds;
			data.size = collider[0].bounds.size;

			// Do raycasting samples over the object to see what terrain heights should be
			float[,] heights = new float[data.heightmapResolution, data.heightmapResolution];
			Ray ray = new Ray(new Vector3(bounds.min.x, bounds.max.y + bounds.size.y, bounds.min.z), -Vector3.up);
			RaycastHit hit = new RaycastHit();
			float meshHeightInverse = 1 / bounds.size.y;
			Vector3 rayOrigin = ray.origin;

			int maxHeight = heights.GetLength(0);
			int maxLength = heights.GetLength(1);

			Vector2 stepXZ = new Vector2(bounds.size.x / maxLength, bounds.size.z / maxHeight);

			for (int zCount = 0; zCount < maxHeight; zCount++)
			{
				ShowProgressBar(zCount, maxHeight);

				for (int xCount = 0; xCount < maxLength; xCount++)
				{
					float height = 0.0f;

					if (collider[0].Raycast(ray, out hit, bounds.size.y * 3))
					{
						height = (hit.point.y - bounds.min.y) * meshHeightInverse;
						height += shiftHeight;

						//clamp
						if (height < 0)
							height = 0;
					}

					heights[zCount, xCount] = height;
					rayOrigin.x += stepXZ[0];
					ray.origin = rayOrigin;
				}

				rayOrigin.z += stepXZ[1];
				rayOrigin.x = bounds.min.x;
				ray.origin = rayOrigin;
			}

			data.SetHeights(0, 0, heights);
			terrainObject.transform.position = meshObject.transform.position;

			if (disabled)
				meshObject.SetActive(false);

			EditorUtility.ClearProgressBar();

			if (cleanUp != null)
				cleanUp();
		}

		private void ShowProgressBar(float progress, float maxProgress)
		{
			float p = progress / maxProgress;
			EditorUtility.DisplayProgressBar("Creating Terrain...", Mathf.RoundToInt(p * 100f) + " %", p);
		}

		delegate void CleanUp();

		private void ExportData()
		{
			if (heightmapResolution / (int)tilesCount < 32)
			{
				EditorUtility.DisplayDialog("NOT ENOUGH RESOLUTION", "Generated tiles can't have resolution lower than 32!\n\nSelect another Tiles value.", "Ok");
				tilesCount = Tiles._1;
				return;
			}

			if (customLocation)
				dataFolder = EditorUtility.OpenFolderPanel("Select Database Elevation Folder", Application.dataPath, "Elevation");
			else
			{
				exportDate = DateTime.Now.ToString("/MM-dd-yyyy_HH-mm-ss");
				dataFolder = exportedData + exportDate;
				Directory.CreateDirectory(dataFolder);
			}

			if (string.IsNullOrEmpty(dataFolder))
			{
				EditorUtility.DisplayDialog("NO SAVE LOCATION", "Select a save location to download Imagery", "Ok");
				return;
			}

			if (splittedTerrains)
			{
				CheckTerrainChunks();

				if (tilesCount.Equals(Tiles._1))
				{
					int chunkHeightmapResolution = croppedTerrains[0].terrainData.heightmapResolution;
					int totalHeightmapResolution = chunkHeightmapResolution * splitSizeFinal;
					finalHeights = new float[totalHeightmapResolution, totalHeightmapResolution];

					GetUberHeightmap(finalHeights, chunkHeightmapResolution);

					RunAsync(() =>
					{
						if (exportFormatIndex == 0)
						{
							string fileName = dataFolder + "/Terrain.asc";
							SaveTerrainDataASCII(finalHeights, fileName);
						}
						else
						{
							string fileName = dataFolder + "/Terrain.raw";
							SaveTerrainDataRAW(finalHeights, fileName);
						}

						QueueOnMainThread(() =>
						{
							if (customLocation)
								Process.Start(dataFolder.Replace(@"/", @"\") + @"\");
							else
								AssetDatabase.Refresh();
						});
					});
				}
				else if ((int)tilesCount == splitSizeFinal)
				{
					int counter = 0;
					splitHeights = new List<float[,]>();

					for (int i = 0; i < croppedTerrains.Count; i++)
					{
						TerrainData tileData = croppedTerrains[i].terrainData;
						splitHeights.Add(tileData.GetHeights(0, 0, tileData.heightmapResolution, tileData.heightmapResolution));
					}

					RunAsync(() =>
					{
						for (int i = 0; i < splitSizeFinal; i++)
						{
							for (int j = 0; j < splitSizeFinal; j++)
							{
								if (exportFormatIndex == 0)
								{
									string fileName = dataFolder + "/Terrain " + (i + 1).ToString() + "-" + (j + 1).ToString() + ".asc";
									showProgressGenerateASCII = true;
									cellSize = 30.0;

									StreamWriter sw = new StreamWriter(fileName);

									sw.WriteLine("ncols         " + (splitHeights[counter].GetLength(0)).ToString());
									sw.WriteLine("nrows         " + (splitHeights[counter].GetLength(1)).ToString());
									sw.WriteLine("xllcorner     " + 0.0);
									sw.WriteLine("yllcorner     " + 0.0);
									sw.WriteLine("cellsize      " + cellSize);
									sw.WriteLine("nodata_value  " + "-9999.0");

									RAWElevationData(sw, splitHeights[counter].GetLength(0), splitHeights[counter].GetLength(1), splitHeights[counter]);

									sw.Close();

									showProgressGenerateASCII = false;
								}
								else
								{
									string fileName = dataFolder + "/Terrain " + (i + 1).ToString() + "-" + (j + 1).ToString() + ".raw";
									showProgressGenerateRAW = true;

									byte[] array = new byte[(splitHeights[counter].GetLength(0) * splitHeights[counter].GetLength(1)) * 2];
									float num = 65536f;

									for (int x = 0; x < splitHeights[counter].GetLength(0); x++)
									{
										for (int y = 0; y < splitHeights[counter].GetLength(1); y++)
										{
											int num2 = y + x * (splitHeights[counter].GetLength(0));
											int value = (int)(splitHeights[counter][(splitHeights[counter].GetLength(0) - 1) - x, y] * num);

											ushort value2 = (ushort)Mathf.Clamp(value, 0, 65535);
											byte[] bytes = BitConverter.GetBytes(value2);
											array[num2 * 2] = bytes[0];
											array[num2 * 2 + 1] = bytes[1];

											progressGenerateRAW = Mathf.InverseLerp(0f, (float)splitHeights[counter].GetLength(0), (float)x);
										}
									}

									FileStream fileStream = new FileStream(fileName, FileMode.Create);
									fileStream.Write(array, 0, array.Length);
									fileStream.Close();

									showProgressGenerateRAW = false;
								}

								counter++;
							}
						}

						QueueOnMainThread(() =>
						{
							if (customLocation)
								Process.Start(dataFolder.Replace(@"/", @"\") + @"\");
							else
								AssetDatabase.Refresh();
						});
					});
				}
				else
				{
					int chunkHeightmapResolution = croppedTerrains[0].terrainData.heightmapResolution;
					int totalHeightmapResolution = chunkHeightmapResolution * splitSizeFinal;
					float[,] heights = new float[totalHeightmapResolution, totalHeightmapResolution];

					GetUberHeightmap(heights, chunkHeightmapResolution);

					chunkHeightmapResolution = (totalHeightmapResolution / (int)tilesCount) + 1;
					int counter = 0;
					int length = (int)Mathf.Pow((int)tilesCount, 2);

					for (int i = 0; i < length; i++)
						UberHeightmap2Chunks(heights, chunkHeightmapResolution, (int)tilesCount, i);

					RunAsync(() =>
					{
						for (int i = 0; i < (int)tilesCount; i++)
						{
							for (int j = 0; j < (int)tilesCount; j++)
							{
								if (exportFormatIndex == 0)
								{
									string fileName = dataFolder + "/Terrain " + (i + 1).ToString() + "-" + (j + 1).ToString() + ".asc";
									showProgressGenerateASCII = true;
									cellSize = 30.0;

									StreamWriter sw = new StreamWriter(fileName);

									sw.WriteLine("ncols         " + (splitHeights[counter].GetLength(0)).ToString());
									sw.WriteLine("nrows         " + (splitHeights[counter].GetLength(1)).ToString());
									sw.WriteLine("xllcorner     " + 0.0);
									sw.WriteLine("yllcorner     " + 0.0);
									sw.WriteLine("cellsize      " + cellSize);
									sw.WriteLine("nodata_value  " + "-9999.0");

									RAWElevationData(sw, splitHeights[counter].GetLength(0), splitHeights[counter].GetLength(1), splitHeights[counter]);

									sw.Close();

									showProgressGenerateASCII = false;
								}
								else
								{
									string fileName = dataFolder + "/Terrain " + (i + 1).ToString() + "-" + (j + 1).ToString() + ".raw";
									showProgressGenerateRAW = true;

									byte[] array = new byte[(splitHeights[counter].GetLength(0) * splitHeights[counter].GetLength(1)) * 2];
									float num = 65536f;

									for (int x = 0; x < splitHeights[counter].GetLength(0); x++)
									{
										for (int y = 0; y < splitHeights[counter].GetLength(1); y++)
										{
											int num2 = y + x * (splitHeights[counter].GetLength(0));
											int value = (int)(splitHeights[counter][(splitHeights[counter].GetLength(0) - 1) - x, y] * num);

											ushort value2 = (ushort)Mathf.Clamp(value, 0, 65535);
											byte[] bytes = BitConverter.GetBytes(value2);
											array[num2 * 2] = bytes[0];
											array[num2 * 2 + 1] = bytes[1];

											progressGenerateRAW = Mathf.InverseLerp(0f, (float)splitHeights[counter].GetLength(0), (float)x);
										}
									}

									FileStream fileStream = new FileStream(fileName, FileMode.Create);
									fileStream.Write(array, 0, array.Length);
									fileStream.Close();

									showProgressGenerateRAW = false;
								}

								counter++;
							}
						}

						QueueOnMainThread(() =>
						{
							if (customLocation)
								Process.Start(dataFolder.Replace(@"/", @"\") + @"\");
							else
								AssetDatabase.Refresh();
						});
					});
				}
			}
			else if (terrain)
			{
				if (tilesCount.Equals(Tiles._1))
				{
					TerrainData data = terrain.terrainData;
					finalHeights = data.GetHeights(0, 0, data.heightmapResolution, data.heightmapResolution);

					RunAsync(() =>
					{
						if (exportFormatIndex == 0)
						{
							string fileName = dataFolder + "/Terrain.asc";
							SaveTerrainDataASCII(finalHeights, fileName);
						}
						else
						{
							string fileName = dataFolder + "/Terrain.raw";
							SaveTerrainDataRAW(finalHeights, fileName);
						}

						QueueOnMainThread(() =>
						{
							if (customLocation)
								Process.Start(dataFolder.Replace(@"/", @"\") + @"\");
							else
								AssetDatabase.Refresh();
						});
					});
				}
				else
				{
					TerrainData data = terrain.terrainData;
					float[,] heights = data.GetHeights(0, 0, data.heightmapResolution, data.heightmapResolution);

					int chunkHeightmapResolution = ((data.heightmapResolution - 1) / (int)tilesCount) + 1;
					int counter = 0;
					int length = (int)Mathf.Pow((int)tilesCount, 2);

					for (int i = 0; i < length; i++)
						UberHeightmap2Chunks(heights, chunkHeightmapResolution, (int)tilesCount, i);

					RunAsync(() =>
					{
						for (int i = 0; i < (int)tilesCount; i++)
						{
							for (int j = 0; j < (int)tilesCount; j++)
							{
								if (exportFormatIndex == 0)
								{
									string fileName = dataFolder + "/Terrain " + (i + 1).ToString() + "-" + (j + 1).ToString() + ".asc";
									showProgressGenerateASCII = true;
									cellSize = 30.0;

									StreamWriter sw = new StreamWriter(fileName);

									sw.WriteLine("ncols         " + (splitHeights[counter].GetLength(0)).ToString());
									sw.WriteLine("nrows         " + (splitHeights[counter].GetLength(1)).ToString());
									sw.WriteLine("xllcorner     " + 0.0);
									sw.WriteLine("yllcorner     " + 0.0);
									sw.WriteLine("cellsize      " + cellSize);
									sw.WriteLine("nodata_value  " + "-9999.0");

									RAWElevationData(sw, splitHeights[counter].GetLength(0), splitHeights[counter].GetLength(1), splitHeights[counter]);

									sw.Close();

									showProgressGenerateASCII = false;
								}
								else
								{
									string fileName = dataFolder + "/Terrain " + (i + 1).ToString() + "-" + (j + 1).ToString() + ".raw";
									showProgressGenerateRAW = true;

									byte[] array = new byte[(splitHeights[counter].GetLength(0) * splitHeights[counter].GetLength(1)) * 2];
									float num = 65536f;

									for (int x = 0; x < splitHeights[counter].GetLength(0); x++)
									{
										for (int y = 0; y < splitHeights[counter].GetLength(1); y++)
										{
											int num2 = y + x * (splitHeights[counter].GetLength(0));
											int value = (int)(splitHeights[counter][(splitHeights[counter].GetLength(0) - 1) - x, y] * num);

											ushort value2 = (ushort)Mathf.Clamp(value, 0, 65535);
											byte[] bytes = BitConverter.GetBytes(value2);
											array[num2 * 2] = bytes[0];
											array[num2 * 2 + 1] = bytes[1];

											progressGenerateRAW = Mathf.InverseLerp(0f, (float)splitHeights[counter].GetLength(0), (float)x);
										}
									}

									FileStream fileStream = new FileStream(fileName, FileMode.Create);
									fileStream.Write(array, 0, array.Length);
									fileStream.Close();

									showProgressGenerateRAW = false;
								}

								counter++;
							}
						}

						QueueOnMainThread(() =>
						{
							if (customLocation)
								Process.Start(dataFolder.Replace(@"/", @"\") + @"\");
							else
								AssetDatabase.Refresh();
						});
					});
				}
			}
			else
			{
				EditorUtility.DisplayDialog("UNAVAILABLE TERRAIN", unavailableTerrainStr, "Ok");
				return;
			}
		}

		private void SliceImage()
		{
			slicedImageWidth = slicedImage.width;
			slicedImageHeight = slicedImage.height;

			if (slicedImageWidth != slicedImageHeight)
			{
				EditorUtility.DisplayDialog("NON SQUARE IMAGE", "Inserted image is not square.\n\nImage needs to be square for any further processings.", "Ok");
				return;
			}

			slicedImageResolution = slicedImage.width;

			if (!Mathf.IsPowerOfTwo(slicedImageResolution))
			{
				EditorUtility.DisplayDialog("NON PO2 Resolution", "Inserted image's resolution is not power of 2.\n\nImage's resolution needs to be a power of 2 value for any further processings.", "Ok");
				return;
			}

			tiledImageResolution = slicedImageResolution / (int)sliceGrid;

			if (tiledImageResolution < 8)
			{
				EditorUtility.DisplayDialog("NOT ENOUGH RESOLUTION", "Generated images can't have resolution lower than 8!\n\nSelect another Tiles value.", "Ok");
				return;
			}

			// TODO: Check if texture is Normal map and convert its type to Default before slicing with a OK, Cancel dialogue box
			//Type t = slicedImage.GetType();
			//if(t.Equals()
			//if (slicedImage == TextureFormat)

			if (customLocationSlice)
				dataFolder = EditorUtility.OpenFolderPanel("Select Database Imagery Folder", Application.dataPath, "Imagery");
			else
			{
				exportDate = DateTime.Now.ToString("/MM-dd-yyyy_HH-mm-ss");
				dataFolder = slicePath + exportDate;
				Directory.CreateDirectory(dataFolder);
			}

			if (string.IsNullOrEmpty(dataFolder))
			{
				EditorUtility.DisplayDialog("NO SAVE LOCATION", "Select a save location to save images", "Ok");
				return;
			}

			string slicedImagePath = AssetDatabase.GetAssetPath(slicedImage);
			UnityEngine.Object asset = AssetDatabase.LoadAssetAtPath(slicedImagePath.Substring(slicedImagePath.LastIndexOf("Assets")), typeof(UnityEngine.Object)) as UnityEngine.Object;
			TextureImporter textureImporter = AssetImporter.GetAtPath(AssetDatabase.GetAssetPath(asset)) as TextureImporter;

			if (textureImporter.isReadable == false)
			{
				textureImporter.isReadable = true;
				textureImporter.mipmapEnabled = true;
				textureImporter.wrapMode = TextureWrapMode.Clamp;
				AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(asset), ImportAssetOptions.ForceUpdate);
			}

			Color32[] imageColors1D = slicedImage.GetPixels32();
			Color32[,] imageColors2D = Convert1DTo2D(imageColors1D);
			Color32[,] tiledImageColors2D = new Color32[tiledImageResolution, tiledImageResolution];
			int tiledImagesCount = (int)sliceGrid * (int)sliceGrid;
			int currentRow = (int)sliceGrid - 1;
			int counter = 0;

			for (int i = 0; i < (int)sliceGrid; i++)
			{
				for (int j = 0; j < (int)sliceGrid; j++)
				{
					int xStart = currentRow * tiledImageResolution;
					int yStart = j * tiledImageResolution;

					for (int x = 0; x < tiledImageResolution; x++)
						for (int y = 0; y < tiledImageResolution; y++)
							tiledImageColors2D[x, y] = imageColors2D[xStart + x, yStart + y];

					Texture2D tiledImage = new Texture2D(tiledImageResolution, tiledImageResolution);
					tiledImage.SetPixels32(Convert2DTo1D(tiledImageColors2D));
					tiledImage.Apply();

					if (exportFormatIndexSlice == 0)
					{
						slicedBytes = tiledImage.EncodeToJPG();
						slicedFileName = dataFolder + "/Image " + (i + 1).ToString() + "-" + (j + 1).ToString() + ".jpg";
					}
					else if (exportFormatIndexSlice == 1)
					{
						slicedBytes = tiledImage.EncodeToPNG();
						slicedFileName = dataFolder + "/Image " + (i + 1).ToString() + "-" + (j + 1).ToString() + ".png";
					}

					File.WriteAllBytes(slicedFileName, slicedBytes);

					//TODO: Check if "Alpha is Transparency" needed for splatmaps when importing in project
					// if (!customLocationSlice)

					DestroyImmediate(tiledImage);

					counter++;
					EditorUtility.DisplayProgressBar("CREATING TILES", "Image  " + (counter + 1).ToString() + "  of  " + tiledImagesCount, Mathf.InverseLerp(0f, (float)(tiledImagesCount - 1), (float)(counter)));
				}
				currentRow--;
			}

			EditorUtility.ClearProgressBar();

			if (customLocationSlice)
				Process.Start(dataFolder.Replace(@"/", @"\") + @"\");
			else
				AssetDatabase.Refresh();
		}

		private void CombineImages()
		{
			if (cellResolution <= 0)
			{
				EditorUtility.DisplayDialog("INVALID RESOLUTION", "Set correct image resolution for each tile in the selected directory", "Ok");
				cellResolution = 0;
				return;
			}

			string path = EditorUtility.OpenFolderPanel("Select a directory including jpg or png image files", Application.dataPath, "");

			if (customLocationStitch)
				dataFolder = EditorUtility.OpenFolderPanel("Select Database Imagery Folder", Application.dataPath, "Imagery");
			else
			{
				exportDate = DateTime.Now.ToString("/MM-dd-yyyy_HH-mm-ss");
				dataFolder = stitchPath + exportDate;
				Directory.CreateDirectory(dataFolder);
			}

			if (string.IsNullOrEmpty(dataFolder))
			{
				EditorUtility.DisplayDialog("NO SAVE LOCATION", "Select a save location to save image", "Ok");
				return;
			}

			string[] names = Directory.GetFiles(@path, "*.*", SearchOption.AllDirectories).Where(s => s.EndsWith(".jpg") || s.EndsWith(".png")).ToArray();
			names = LogicalComparer(names);
			int count = names.Length;
			int xTiles = (int)Mathf.Sqrt(count);
			int yTiles = xTiles;
			int width = xTiles * cellResolution;
			int length = yTiles * cellResolution;
			Texture2D combinedImage = new Texture2D(width, length);
			int index = 0;

			for (int y = (yTiles - 1); y >= 0; y--)
			{
				for (int x = 0; x < xTiles; x++)
				{
					string name = names[index];
					byte[] bytes = File.ReadAllBytes(name);
					Texture2D cellImage = new Texture2D(cellResolution, cellResolution);
					cellImage.LoadImage(bytes);

					combinedImage.SetPixels(x * cellResolution, y * cellResolution, cellResolution, cellResolution, cellImage.GetPixels());

					EditorUtility.DisplayProgressBar("CREATING IMAGE", "Tile  " + (index + 1).ToString() + "  of  " + count, Mathf.InverseLerp(0f, (float)(count - 1), (float)(index)));

					index++;
				}
			}

			EditorUtility.ClearProgressBar();

			combinedImage.Apply();

			string format = "";
			byte[] combinedData = new byte[1];

			if (exportFormatIndexStitch == 0)
			{
				format = ".jpg";
				combinedData = combinedImage.EncodeToJPG();
			}
			else if (exportFormatIndexStitch == 1)
			{
				format = ".png";
				combinedData = combinedImage.EncodeToPNG();
			}

			string fileName = dataFolder + "/Stitched Image" + format;
			File.WriteAllBytes(fileName, combinedData);

			DestroyImmediate(combinedImage);
			combinedImage = null;

			Process.Start(dataFolder.Replace(@"/", @"\") + @"\");
			AssetDatabase.Refresh();
		}

		public Color32[] Convert2DTo1D(Color32[,] array2D)
		{
			int arrayResolution = array2D.GetLength(0);
			Color32[] array1D = new Color32[arrayResolution * arrayResolution];

			for (int i = 0; i < arrayResolution; i++)
				for (int j = 0; j < arrayResolution; j++)
					array1D[i * arrayResolution + j] = array2D[i, j];

			return array1D;
		}

		public Color32[,] Convert1DTo2D(Color32[] array1D)
		{
			int arrayResolution = (int)Mathf.Sqrt(array1D.Length);
			Color32[,] array2D = new Color32[arrayResolution, arrayResolution];
			int i = 0;

			for (int x = 0; x < arrayResolution; x++)
			{
				for (int y = 0; y < arrayResolution; y++)
				{
					array2D[x, y] = array1D[i];
					i++;
				}
			}

			return array2D;
		}

		private void GenerateAlphaFromGrayscale()
		{
			slicedImageWidth = splatmap.width;
			slicedImageHeight = splatmap.height;

			if (slicedImageWidth != slicedImageHeight)
			{
				EditorUtility.DisplayDialog("NON SQUARE IMAGE", "Inserted splatmap is not square.\n\nSplatmap needs to be square for any further processings.", "Ok");
				return;
			}

			int splatmapResolution = splatmap.width;

			if (!Mathf.IsPowerOfTwo(splatmapResolution))
			{
				EditorUtility.DisplayDialog("NON PO2 Resolution", "Inserted splatmap's resolution is not power of 2.\n\nSplatmap's resolution needs to be a power of 2 value for any further processings.", "Ok");
				return;
			}
			string imagePath = AssetDatabase.GetAssetPath(splatmap);
			UnityEngine.Object asset = AssetDatabase.LoadAssetAtPath(imagePath.Substring(imagePath.LastIndexOf("Assets")), typeof(UnityEngine.Object)) as UnityEngine.Object;
			TextureImporter textureImporter = AssetImporter.GetAtPath(AssetDatabase.GetAssetPath(asset)) as TextureImporter;

			if (!textureImporter.isReadable)
			{
				textureImporter.isReadable = true;
				textureImporter.alphaSource = TextureImporterAlphaSource.FromInput;
				AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(asset), ImportAssetOptions.ForceUpdate);
			}

			Texture2D splatWithAlpha = new Texture2D(splatmap.width, splatmap.height, TextureFormat.RGBA32, true);
			splatWithAlpha.SetPixels32(splatmap.GetPixels32());

			UnityEngine.Color[] imageColors1D = splatWithAlpha.GetPixels();
			UnityEngine.Color[,] imageColors2D = Convert1DTo2D(imageColors1D);
			int length = imageColors1D.Length;
			int counter = 0;

			for (int i = 0; i < splatmapResolution; i++)
			{
				for (int j = 0; j < splatmapResolution; j++)
				{
					UnityEngine.Color col = imageColors2D[j, i];

					//if ((col.r >= col.g - damping && col.r <= col.g + damping) && (col.r >= col.b - damping && col.r <= col.b + damping))
					//{
					float sum = (col.r + col.g + col.b) / 3f;
					UnityEngine.Color alpha = new UnityEngine.Color(col.r, col.g, col.b, 1 - sum);
					splatWithAlpha.SetPixel(i, j, alpha);
					//}
					//else
					//{
					//    UnityEngine.Color alpha = new UnityEngine.Color(col.r, col.g, col.b, 0);
					//    splatWithAlpha.SetPixel(i, j, alpha);
					//}

					if (counter % 65536 == 0)
						EditorUtility.DisplayProgressBar("GENERATING ALPHA PIXELS", "Pixel  " + counter + "  of  " + length, Mathf.InverseLerp(0f, (float)length, (float)counter));

					counter++;
				}
			}

			EditorUtility.ClearProgressBar();
			splatWithAlpha.Apply();
			slicedBytes = splatWithAlpha.EncodeToPNG();
			slicedFileName = imagePath.Replace(".png", "_Alpha.png");
			File.WriteAllBytes(slicedFileName, slicedBytes);

			AssetDatabase.Refresh();

			asset = AssetDatabase.LoadAssetAtPath(slicedFileName.Substring(slicedFileName.LastIndexOf("Assets")), typeof(UnityEngine.Object)) as UnityEngine.Object;
			textureImporter = AssetImporter.GetAtPath(AssetDatabase.GetAssetPath(asset)) as TextureImporter;

			if (textureImporter.alphaSource != TextureImporterAlphaSource.FromInput || textureImporter.maxTextureSize != splatmapResolution)
			{
				textureImporter.alphaSource = TextureImporterAlphaSource.FromInput;
				textureImporter.maxTextureSize = splatmapResolution;
				AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(asset), ImportAssetOptions.ForceUpdate);
			}

			DestroyImmediate(splatWithAlpha);
			AssetDatabase.Refresh();

			//    string imagePath = AssetDatabase.GetAssetPath(splatmap);
			//    Texture2D splatWithAlpha = new Texture2D(splatmap.width, splatmap.height, TextureFormat.RGBA32, true);
			//    splatWithAlpha.SetPixels32(splatmap.GetPixels32());
			//    splatWithAlpha.Apply();
			//    slicedBytes = splatWithAlpha.EncodeToPNG();
			//    slicedFileName = imagePath.Replace(".png", "_Alpha.png");
			//    File.WriteAllBytes(slicedFileName, slicedBytes);

			//    AssetDatabase.Refresh();

			//    UnityEngine.Object asset = AssetDatabase.LoadAssetAtPath(slicedFileName.Substring(slicedFileName.LastIndexOf("Assets")), typeof(UnityEngine.Object)) as UnityEngine.Object;
			//    TextureImporter textureImporter = AssetImporter.GetAtPath(AssetDatabase.GetAssetPath(asset)) as TextureImporter;

			//    if (textureImporter.alphaSource != TextureImporterAlphaSource.FromGrayScale || textureImporter.maxTextureSize != splatmapResolution)
			//    {
			//        textureImporter.alphaSource = TextureImporterAlphaSource.FromGrayScale;
			//        textureImporter.maxTextureSize = splatmapResolution;
			//        AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(asset), ImportAssetOptions.ForceUpdate);
			//    }

			//    DestroyImmediate(splatWithAlpha);
			//    AssetDatabase.Refresh();
		}

		public UnityEngine.Color[,] Convert1DTo2D(UnityEngine.Color[] array1D)
		{
			int arrayResolution = (int)Mathf.Sqrt(array1D.Length);
			UnityEngine.Color[,] array2D = new UnityEngine.Color[arrayResolution, arrayResolution];
			int i = 0;

			for (int x = 0; x < arrayResolution; x++)
			{
				for (int y = 0; y < arrayResolution; y++)
				{
					array2D[x, y] = array1D[i];
					i++;
				}
			}

			return array2D;
		}

		private void GenerateDataTiles()
		{
			string[] filters = new string[] { "Elevation Files", "asc,raw,tif" };
			string dataFile = EditorUtility.OpenFilePanelWithFilters("Select Tif, Raw or Ascii Elevation File", "", filters);

			try
			{
				splitSizeFinal = (int)dataTiles;

				if (dataFile.EndsWith(".asc"))
				{
					dataFormat = "asc";
					asciiPath = dataFile;
					GetASCIIInfo();
				}
				else if (dataFile.EndsWith(".raw"))
				{
					dataFormat = "raw";
					rawPath = dataFile;
					GetRAWInfo();
				}
				else if (dataFile.EndsWith(".tif"))
				{
					dataFormat = "tif";
					tiffPath = dataFile;
					GetTIFFInfo();
				}
				else if (!string.IsNullOrEmpty(dataFile))
				{
					EditorUtility.DisplayDialog("UNKNOWN DATA FILE", "Please select a valid Tif, Raw or Ascii elevation file!", "Ok");
					return;
				}
				else
					return;

				ResampleData(dataFormat);
			}
			catch (Exception e)
			{
				EditorUtility.DisplayDialog("INTERNAL ERROR", e.Message, "Ok");
				return;
			}

			if (heightmapResolution / (int)dataTiles < 32)
			{
				EditorUtility.DisplayDialog("NOT ENOUGH RESOLUTION", "Generated tiles can't have resolution lower than 32!\n\nSelect another Tiles value.", "Ok");
				dataTiles = DataTiles._1;
				return;
			}

			dataFolder = EditorUtility.OpenFolderPanel("Select Database Elevation Folder", Application.dataPath, "Elevation");

			if (string.IsNullOrEmpty(dataFolder))
			{
				EditorUtility.DisplayDialog("NO SAVE LOCATION", "Select a save location to save tiles", "Ok");
				return;
			}

			int chunkHeightmapResolution = (Mathf.ClosestPowerOfTwo(heightmapResolution) / (int)dataTiles) + 1;
			int counter = 0;
			int length = (int)Mathf.Pow((int)dataTiles, 2);

			for (int i = 0; i < length; i++)
				UberHeightmap2Chunks(finalHeights, chunkHeightmapResolution, (int)dataTiles, i);

			RunAsync(() =>
			{
				for (int i = 0; i < (int)dataTiles; i++)
				{
					for (int j = 0; j < (int)dataTiles; j++)
					{
						if (exportFormatIndexData == 0)
						{
							string fileName = dataFolder + "/Terrain " + (i + 1).ToString() + "-" + (j + 1).ToString() + ".asc";
							showProgressGenerateASCII = true;
							cellSize = 30.0;

							StreamWriter sw = new StreamWriter(fileName);

							sw.WriteLine("ncols         " + splitHeights[counter].GetLength(0).ToString());
							sw.WriteLine("nrows         " + splitHeights[counter].GetLength(1).ToString());
							sw.WriteLine("xllcorner     " + 0.0);
							sw.WriteLine("yllcorner     " + 0.0);
							sw.WriteLine("cellsize      " + cellSize);
							sw.WriteLine("nodata_value  " + "-9999.0");

							RAWElevationData(sw, splitHeights[counter].GetLength(0), splitHeights[counter].GetLength(1), splitHeights[counter]);

							sw.Close();

							showProgressGenerateASCII = false;
						}
						else if (exportFormatIndexData == 1)
						{
							string fileName = dataFolder + "/Terrain " + (i + 1).ToString() + "-" + (j + 1).ToString() + ".raw";
							showProgressGenerateRAW = true;

							byte[] array = new byte[splitHeights[counter].GetLength(0) * splitHeights[counter].GetLength(1) * 2];
							float num = 65536f;
							int height = 0;

							for (int x = 0; x < splitHeights[counter].GetLength(0); x++)
							{
								for (int y = 0; y < splitHeights[counter].GetLength(1); y++)
								{
									int num2 = y + x * splitHeights[counter].GetLength(0);

									if (dataFormat.Equals("asc"))
										height = (int)(splitHeights[counter][splitHeights[counter].GetLength(0) - 1 - x, y] * num);
									else if (dataFormat.Equals("raw"))
										height = (int)(splitHeights[counter][splitHeights[counter].GetLength(0) - 1 - x, y] * num);
									else if (dataFormat.Equals("tif"))
										height = (int)(splitHeights[counter][splitHeights[counter].GetLength(0) - 1 - x, y] * num / everestPeak);

									ushort value2 = (ushort)Mathf.Clamp(height, 0, 65535);
									byte[] bytes = BitConverter.GetBytes(value2);
									array[num2 * 2] = bytes[0];
									array[num2 * 2 + 1] = bytes[1];

									progressGenerateRAW = Mathf.InverseLerp(0f, (float)splitHeights[counter].GetLength(0), (float)x);
								}
							}

							FileStream fileStream = new FileStream(fileName, FileMode.Create);
							fileStream.Write(array, 0, array.Length);
							fileStream.Close();

							showProgressGenerateRAW = false;
						}

						counter++;
					}
				}

				QueueOnMainThread(() =>
				{
					Process.Start(dataFolder.Replace(@"/", @"\") + @"\");
				});
			});
		}

		private void SaveTerrainDataASCII(float[,] heights, string fileName)
		{
			showProgressGenerateASCII = true;
			cellSize = 30.0;

			StreamWriter sw = new StreamWriter(fileName);

			sw.WriteLine("ncols         " + (heights.GetLength(0)).ToString());
			sw.WriteLine("nrows         " + (heights.GetLength(1)).ToString());
			sw.WriteLine("xllcorner     " + 0.0);
			sw.WriteLine("yllcorner     " + 0.0);
			sw.WriteLine("cellsize      " + cellSize);
			sw.WriteLine("nodata_value  " + "-9999.0");

			RAWElevationData(sw, heights.GetLength(0), heights.GetLength(1), heights);

			sw.Close();

			showProgressGenerateASCII = false;


			//		string xmlPath = asciiPath.Replace(".asc", ".xml");
			//		
			//		if(File.Exists(xmlPath))
			//			ReadXMLFile(xmlPath);
			//		else
			//		{
			//			EditorUtility.DisplayDialog("XML DATA NOT FOUND", "Exported ASCII File Will Be Generated By False Geo-Coordinates.", "Ok");
			//			top = "0";
			//			left = "0";
			//		}
		}

		private void SaveTerrainDataRAW(float[,] heights, string fileName)
		{
			showProgressGenerateRAW = true;

			byte[] array = new byte[(heights.GetLength(0) * heights.GetLength(1)) * 2];
			float num = 65536f;

			for (int i = 0; i < heights.GetLength(0); i++)
			{
				for (int j = 0; j < heights.GetLength(1); j++)
				{
					int num2 = j + i * (heights.GetLength(0));
					int value = (int)(heights[(heights.GetLength(0) - 1) - i, j] * num);

					ushort value2 = (ushort)Mathf.Clamp(value, 0, 65535);
					byte[] bytes = BitConverter.GetBytes(value2);
					array[num2 * 2] = bytes[0];
					array[num2 * 2 + 1] = bytes[1];

					progressGenerateRAW = Mathf.InverseLerp(0f, (float)heights.GetLength(0), (float)i);
				}
			}

			FileStream fileStream = new FileStream(fileName, FileMode.Create);
			fileStream.Write(array, 0, array.Length);
			fileStream.Close();

			showProgressGenerateRAW = false;
		}

		private void RAWElevationData(StreamWriter sw, int width, int height, float[,] outputImageData)
		{
			string row = "";

			for (int i = 0; i < width; i++)
			{
				for (int j = 0; j < height; j++)
				{
					if (dataFormat.Equals("asc"))
						row += (outputImageData[(width - 1) - i, j]) + " ";
					else if (dataFormat.Equals("raw"))
						row += (outputImageData[(width - 1) - i, j] * everestPeak) + " ";
					else if (dataFormat.Equals("tif"))
						row += (outputImageData[(width - 1) - i, j]) + " ";

					progressGenerateASCII = Mathf.InverseLerp(0f, (float)width, (float)i);
				}

				if (i < width - 1)
					sw.Write(row.Remove(row.Length - 1) + Environment.NewLine);
				else
					sw.Write(row.Remove(row.Length - 1));

				row = "";
			}
		}

		private void ShowMapAndRefresh()
		{
			InteractiveMap.requestIndex = 1;

			InteractiveMap.map_latlong_center = new InteractiveMap.latlong_class(Double.Parse(latitudeUser), Double.Parse(longitudeUser));

			mapWindow = (InteractiveMap)EditorWindow.GetWindow(typeof(InteractiveMap), false, "Interactive Map", true);
			mapWindow.RequestMap();
		}

		#region multithreading functions

		protected void QueueOnMainThread(Action action)
		{
			QueueOnMainThread(action, 0f);
		}

		protected void QueueOnMainThread(Action action, float time)
		{
			if (time != 0)
			{
				lock (_delayed)
				{
					_delayed.Add(new DelayedQueueItem { time = Time.time + time, action = action });
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

		protected Thread RunAsync(Action a)
		{
			while (numThreads >= maxThreads)
			{
				Thread.Sleep(1);
			}
			Interlocked.Increment(ref numThreads);
			ThreadPool.QueueUserWorkItem(RunAction, a);
			return null;
		}

		private void RunAction(object action)
		{
			try
			{
				((Action)action)();
			}
			catch { }
			finally
			{
				Interlocked.Decrement(ref numThreads);
			}
		}

		protected virtual void Start()
		{
			m_HasLoaded = true;
		}

		protected virtual void Update()
		{
			if (m_HasLoaded == false)
				Start();

			if (_actions != null && _actions.Count > 0)
			{
				lock (_actions)
				{
					_currentActions.Clear();
					_currentActions.AddRange(_actions);
					_actions.Clear();
				}

				foreach (var a in _currentActions)
					a();
			}

			if (_delayed != null && _delayed.Count > 0)
			{
				lock (_delayed)
				{
					_currentDelayed.Clear();
					_currentDelayed.AddRange(_delayed.Where(d => d.time <= Time.time));

					foreach (var item in _currentDelayed)
						_delayed.Remove(item);
				}

				foreach (var delayed in _currentDelayed)
					delayed.action();
			}

			#endregion
		}

		private void PresetManager()
		{
			EditorGUILayout.BeginHorizontal();
			GUILayout.FlexibleSpace();
			if (GUILayout.Button("SAVE PRESET"))
			{
				if (!Directory.Exists(presetsPath))
				{
					Directory.CreateDirectory(presetsPath);
					AssetDatabase.Refresh();
				}

				presetFilePath = EditorUtility.SaveFilePanel("Save Settings As Preset File", presetsPath, "Preset", "tlps");

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
				presetFilePath = EditorUtility.OpenFilePanel("Load Preset File", presetsPath, "tlps");

				if (presetFilePath.Contains("tlps"))
					ReadPresetFile();
			}
			GUILayout.FlexibleSpace();
			EditorGUILayout.EndHorizontal();
		}

		private void AutoSave()
		{
			if (!Directory.Exists(presetsPath))
				Directory.CreateDirectory(presetsPath);

			presetFilePath = presetsPath + "/Terrain AutoSave.tlps";
			WritePresetFile(presetFilePath);
		}

		private void AutoLoad()
		{
			presetFilePath = presetsPath + "/Terrain AutoSave.tlps";

			if (File.Exists(presetFilePath) && presetFilePath.Contains("tlps"))
				ReadPresetFile();
		}

		private void WritePresetFile(string fileName)
		{
			string preset = "Terrain Settings\n"

				+ "\nLatitude: " + latitudeUser + " Degree"
				+ "\nLongitude: " + longitudeUser + " Degree"

				+ "\nNewTerrainGrid: " + enumValueNew
				+ "\nNewTerrainSizeX: " + terrainSizeNewX
				+ "\nNewTerrainSizeZ: " + terrainSizeNewZ
				+ "\nConstrainedAspect: " + constrainedAspect
				+ "\nNewTerrainPixelError: " + pixelError

				+ "\nLatExtents: " + areaSizeLat
				+ "\nLonExtents: " + areaSizeLon

				+ "\nMapType: " + mapType
				+ "\nPreviewZoomLevel: " + zoomLevel

				+ "\nSplitterGrid: " + enumValue
				+ "\nSplitterSmooth: " + smoothIterationsSplitter

				+ "\nTerrainResolution: " + heightmapResolution
				+ "\nTerrainSmooth: " + smoothIterationsOfflineData

				+ "\nGridTerrain: " + tileGrid

				+ "\nAvailableDataSection: " + showAvailableDataSection
				+ "\nNewTerrainSection: " + showNewTerrainSection
				+ "\nNeighboringSection: " + showNeighboringSection
				+ "\nSplitterSection: " + showSplitterSection
				+ "\nSmoothenSection: " + showSmoothenSection
				+ "\nExporterSection: " + showExporterSection
				+ "\nImageTilerSection: " + showImageTilerSection
				+ "\nTerrain2MeshSection: " + showTerrain2MeshSection
				+ "\nMesh2TerrainSection: " + showMesh2TerrainSection
				+ "\nCoordinatesSection: " + showCoordinatesSection

				+ "\nSplitSizeNew: " + splitSizeNew
				+ "\nTotalTerrainsNew: " + totalTerrainsNew

				+ "\nSmoothIterations: " + smoothIterations
				+ "\nSmoothBlend: " + smoothBlend

				+ "\nSaveFormat: " + saveFormat
				+ "\nSaveResolution: " + saveResolution
				+ "\nScenePlace: " + isScenePlace

				+ "\nResolutionMesh: " + resolutionMesh
				+ "\nHeightShift: " + shiftHeight
				+ "\nRayCastZMode: " + bottomTopRadioSelected

				+ "\nSmoothBlendSplitter: " + smoothBlendSplitter
				+ "\nSmoothBlendOffline: " + smoothBlendOfflineData
				+ "\nSmoothBlendIndex: " + smoothBlendIndex
				+ "\nSmoothBlendIndexSplitter: " + smoothBlendIndexSplitter
				+ "\nSmoothBlendIndexOffline: " + smoothBlendIndexOfflineData

				+ "\nExportFormatIndex: " + exportFormatIndex
				+ "\nTilesCount: " + tilesCount
				+ "\nCustomLocation: " + customLocation
				+ "\nExportFormatIndexData: " + exportFormatIndexData
				+ "\nDataTiles: " + dataTiles
				+ "\nDataSplitterSection: " + showDataSplitterSection
				+ "\nTileSmooth: " + smoothIterationsDataTile
				+ "\nHeightmapResizerSection: " + showHeightmapResizerSection
				+ "\nTileSmooth: " + smoothIterationsResample
				+ "\nRaiseLowerSection: " + showRaiseLowerSection

				+ "\nStitchSection: " + showStitchSection;

			File.WriteAllText(fileName, preset);
		}

		private void ReadPresetFile()
		{
			string text = File.ReadAllText(presetFilePath);
			string[] dataLines = text.Split('\n');
			string[][] dataPairs = new string[dataLines.Length][];
			int lineNum = 0;

			foreach (string line in dataLines)
				dataPairs[lineNum++] = line.Split(' ');

			latitudeUser = dataPairs[2][1];
			longitudeUser = dataPairs[3][1];

			enumValueNew = (SizeNew)Enum.Parse(typeof(SizeNew), dataPairs[4][1]);
			terrainSizeNewX = float.Parse(dataPairs[5][1]);
			terrainSizeNewZ = float.Parse(dataPairs[6][1]);

			if (dataPairs[7][1].Contains("True"))
				constrainedAspect = true;
			else
				constrainedAspect = false;

			pixelError = float.Parse(dataPairs[8][1]);

			areaSizeLat = float.Parse(dataPairs[9][1]);
			areaSizeLon = float.Parse(dataPairs[10][1]);

			mapType = (MapType)Enum.Parse(typeof(MapType), dataPairs[11][1]);
			zoomLevel = int.Parse(dataPairs[12][1]);

			enumValue = (Size)Enum.Parse(typeof(Size), dataPairs[13][1]);
			smoothIterationsSplitter = int.Parse(dataPairs[14][1]);

			heightmapResolution = int.Parse(dataPairs[15][1]);
			smoothIterationsOfflineData = int.Parse(dataPairs[16][1]);

			tileGrid = int.Parse(dataPairs[17][1]);

			if (dataPairs[18][1].Contains("True"))
				showAvailableDataSection = true;
			else
				showAvailableDataSection = false;

			if (dataPairs[19][1].Contains("True"))
				showNewTerrainSection = true;
			else
				showNewTerrainSection = false;

			if (dataPairs[20][1].Contains("True"))
				showNeighboringSection = true;
			else
				showNeighboringSection = false;

			if (dataPairs[21][1].Contains("True"))
				showSplitterSection = true;
			else
				showSplitterSection = false;

			if (dataPairs[22][1].Contains("True"))
				showSmoothenSection = true;
			else
				showSmoothenSection = false;

			if (dataPairs[23][1].Contains("True"))
				showExporterSection = true;
			else
				showExporterSection = false;

			if (dataPairs[24][1].Contains("True"))
				showImageTilerSection = true;
			else
				showImageTilerSection = false;

			if (dataPairs[25][1].Contains("True"))
				showTerrain2MeshSection = true;
			else
				showTerrain2MeshSection = false;

			if (dataPairs[26][1].Contains("True"))
				showMesh2TerrainSection = true;
			else
				showMesh2TerrainSection = false;

			if (dataPairs[27][1].Contains("True"))
				showCoordinatesSection = true;
			else
				showCoordinatesSection = false;

			splitSizeNew = int.Parse(dataPairs[28][1]);
			totalTerrainsNew = int.Parse(dataPairs[29][1]);

			smoothIterations = int.Parse(dataPairs[30][1]);
			smoothBlend = float.Parse(dataPairs[31][1]);

			saveFormat = (SaveFormat)Enum.Parse(typeof(SaveFormat), dataPairs[32][1]);
			saveResolution = (SaveResolution)Enum.Parse(typeof(SaveResolution), dataPairs[33][1]);

			if (dataPairs[34][1].Contains("True"))
				isScenePlace = true;
			else
				isScenePlace = false;

			resolutionMesh = int.Parse(dataPairs[35][1]);
			shiftHeight = float.Parse(dataPairs[36][1]);
			bottomTopRadioSelected = int.Parse(dataPairs[37][1]);

			smoothBlendSplitter = float.Parse(dataPairs[38][1]);
			smoothBlendOfflineData = float.Parse(dataPairs[39][1]);
			smoothBlendIndex = int.Parse(dataPairs[40][1]);
			smoothBlendIndexSplitter = int.Parse(dataPairs[41][1]);
			smoothBlendIndexOfflineData = int.Parse(dataPairs[42][1]);

			exportFormatIndex = int.Parse(dataPairs[43][1]);
			tilesCount = (Tiles)Enum.Parse(typeof(Tiles), dataPairs[44][1]);

			if (dataPairs[45][1].Contains("True"))
				customLocation = true;
			else
				customLocation = false;

			exportFormatIndexData = int.Parse(dataPairs[46][1]);
			dataTiles = (DataTiles)Enum.Parse(typeof(DataTiles), dataPairs[47][1]);

			if (dataPairs[48][1].Contains("True"))
				showDataSplitterSection = true;
			else
				showDataSplitterSection = false;

			smoothIterationsDataTile = int.Parse(dataPairs[49][1]);

			if (dataPairs[50][1].Contains("True"))
				showHeightmapResizerSection = true;
			else
				showHeightmapResizerSection = false;

			smoothIterationsResample = int.Parse(dataPairs[51][1]);

			if (dataPairs[52][1].Contains("True"))
				showRaiseLowerSection = true;
			else
				showRaiseLowerSection = false;

			if (dataPairs[53][1].Contains("True"))
				showStitchSection = true;
			else
				showStitchSection = false;
		}

		public void OnInspectorUpdate()
		{
			Repaint();
		}

		#endregion
	}
}

