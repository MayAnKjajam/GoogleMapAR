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

/*
	The ASCII Grid file example:
	
	ncols         768 (number of colums)
	nrows         736 (number of rows)
	xllcorner     474721.00 (lower left corner X of the grid)        Longitude:  MIN: -180(West)(LEFT)        MAX: 180(East)(RIGHT)   TOTAL: 360
	yllcorner     418933.00 (lower left corner Y of the grid)        Latitude:   MIN: -90(South)(BOTTOM)      MAX: 90(North)(TOP)     TOTAL: 180
	cellsize      1.00 (cell spacing)
	nodata_value  -9999
	 -9999 -9999 -9999 -9999 -9999
	 -9999 -9999 -9999 -9999 -9999
	 -9999 -9999 -9999 -9999 -9999
	 -9999 -9999 -9999 -9999 -9999
	 -9999 -9999 -9999 -9999 -9999
	 -9999 -9999 -9999 -9999 -9999
	 -9999 -9999 -9999 -9999 -9999
	 -213.20 -9999 -9999 -9999 -9999
	
	The ASCII XYZ file example:
	
	47472.00, 418933.00: -213.20
*/

using UnityEngine;
using UnityEditor;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading;
using System.Xml;
using System.Xml.Linq;
using BitMiracle.LibTiff.Classic;
using TerraLand.Utils;

namespace TerraLand
{
    public class TerrainGenerator : EditorWindow
    {
        [MenuItem("Tools/TerraUnity/TerraLand/Downloader", false, 0)]
        public static void Init()
        {
            TerrainGenerator window = (TerrainGenerator)GetWindow(typeof(TerrainGenerator));
            window.position = new Rect(5, 135, 430, 800);
            window.titleContent = new GUIContent("TerraLand Downloader", "Downloads Terrain Data from ESRI servers");
        }

        #region fields:

        public enum ServerGrid
        {
            _4x4 = 4,
            _8x8 = 8,
            _16x16 = 16,
            _32x32 = 32,
            _64x64 = 64,
            _128x128 = 128
        }
        static ServerGrid serverGrid = ServerGrid._32x32;

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

        public enum Neighbourhood
        {
            Moore = 0,
            VonNeumann = 1
        }
        static Neighbourhood neighbourhood = Neighbourhood.Moore;

        Vector2 scrollPosition = Vector2.zero;
        private bool engineOff = false;

        float windowWidth;

        public static string top = "1";
        public static string left = "0";
        public static string bottom = "0";
        public static string right = "1";

        Terrain terrain;

        float terrainSizeX;
        float terrainSizeY;
        float terrainSizeFactor;

        string address = "Mount Everest";

        string empty = "";
        List<Vector2> coords;
        List<string> locations;

        public static string latitudeUser = "27.98582";
        public static string longitudeUser = "86.9236";

        int heightmapResolution = 2048;
        int heightmapResolutionSplit;
        int imageResolution = 2048;

        float progressBarElevation;
        bool showProgressElevation = false;
        bool convertingElevationTiles = false;
        bool stitchingElevationTiles = false;
        bool showProgressImagery = false;
        bool showProgressData = false;
        bool showProgressGenerateASCII = false;
        bool showProgressGenerateRAW = false;

        float terrainHeightMultiplier = 1f;
        float maximumHeightMultiplier = 5f;
        string[] editMode = new string[] { "ON", "OFF" };

        string presetName;

        int gridNumber;
        int alphamapResolution = 512;

        int tileGrid = 2;
        float[,,] smData;
        float cellSizeX;
        float cellSizeY;
        float[] imageXOffset;
        float[] imageYOffset;
        int totalImages;
        string dataPath;

        float sizeFactorXHeightmap;
        float sizeFactorYHeightmap;

        float splatNormalizeX;
        float splatNormalizeY;

        float[] lengthz;
        float[] widthz;
        float[] lengthzOff;
        float[] widthzOff;

        int calculationsDone;

        double latCellSize;
        double lonCellSize;
        double latCellSizeTerrain;
        double lonCellSizeTerrain;

        double[] latCellTop;
        double[] latCellBottom;
        double[] lonCellLeft;
        double[] lonCellRight;

        TerraLandWorldElevation.TopoBathy_ImageServer mapserviceElevation;
        TerraLandWorldImagery.World_Imagery_MapServer mapserviceImagery;

        string directoryPathElevation;
        string directoryPathImagery;
        string directoryPathInfo;
        string directoryPathTerrainlayers;

        int downloadedImageIndex;
        int downloadedHeightmapIndex;
        float normalizedProgressSatelliteImage;

        List<float> terrainHeights;
        bool cancelOperation = false;
        bool cancelOperationHeightmapDownloader = false;
        bool terrainGenerationstarted = false;
        bool imageDownloadingStarted = false;

        //int cropOffsetX;
        //int cropOffsetY;
        //int cropSizeX;
        //int cropSizeY;

        double yMaxTop;
        double xMinLeft;
        double yMinBottom;
        double xMaxRight;
        double[] xMin;
        double[] yMin;
        double[] xMax;
        double[] yMax;
        double[] xMinFailedElevation;
        double[] yMinFailedElevation;
        double[] xMaxFailedElevation;
        double[] yMaxFailedElevation;
        double[] xMinFailedImagery;
        double[] yMinFailedImagery;
        double[] xMaxFailedImagery;
        double[] yMaxFailedImagery;

        bool finishedImporting = false;
        int textureOnFinish = 0;
        float elevationExaggeration = 1;
        List<float> terrainHeightsNormalized;

        string newLatCoord;
        string newLonCoord;

        int downloadIndexSRTM = 0;
        int downloadIndexSatellite = 0;
        int downloadIndexData = 0;
        int downloadIndexGenerationASCII = 0;
        int downloadIndexGenerationRAW = 0;

        int maxAsyncCalls = 50;

        int compressionQuality = 100;
        int anisotropicFilter = 4;

        int workerThreads;
        int completionPortThreads;
        int allThreads = 0;

        int frames2 = 0;
        int frames3 = 0;
        UnityEngine.Object failedFolder;
        FileAttributes attr;
        List<int> failedIndicesElevation;
        List<int> failedIndicesImagery;
        bool failedDownloading = false;

        int failedIndicesCountElevation;
        int failedIndicesCountImagery;

        bool showPresetManager = false;
        bool showTerrainBounds = false;
        bool showOptions = false;

        GameObject[] terrainGameObjects;
        Terrain[] terrains;
        TerrainData[] data;

        float tileWidth;
        float tileLength;
        float tileXPos;
        float tileZPos;
        int arrayPos;

        int totalTerrainsNew;
        string splitDirectoryPath;
        GameObject terrainsParent;
        GameObject splittedTerrains;
        int terrainChunks = 0;
        int gridPerTerrain = 1;
        List<Terrain> croppedTerrains;

        public int neighbourhoodInt = 0;

        bool compressionActive = false;
        bool autoScale = false;

        bool allBlack = false;

        bool failedHeightmapAvailable = false;
        int totalFailedHeightmaps = 0;
        bool failedImageAvailable = false;
        int totalFailedImages = 0;

        UnityEngine.Object[] terraUnityImages;
        Texture2D logo;
        Texture2D heightMapLogo;
        Texture2D landMapLogo;
        Texture2D statusGreen;
        Texture2D statusRed;
        Texture2D landMap;
        Texture2D satelliteImageTemp;
        Texture2D terrainButton;
        Texture2D serverButton;

        Texture2D aspectIcon;
        Texture2D elevationIcon;
        Texture2D hillshadeMDIcon;
        Texture2D slopeIcon;

        //bool areaIsSquare = true;
        //bool areaIsRectangleLat = false;
        //bool areaIsRectangleLon = false;
        
        //double topWebMercator;
        //double leftWebMercator;
        //double bottomWebMercator;
        //double rightWebMercator;

        string token = "";

        string terrainDataURL = "";

        WebClient webClientTerrain, webClientImagery;
        Stopwatch stopWatchTerrain = new Stopwatch();
        string downloadSpeedTerrain = "";
        string dataReceivedTerrain = "";

        bool saveTerrainDataASCII = false;
        bool saveTerrainDataRAW = false;
        bool saveTerrainDataTIFF = false;

        string projectPath;
        string fileNameTerrainData = "";
        string fileNameTerrainDataSaved = "";

        int croppedResolutionHeightmap;
        float croppedResolutionBase;
        float[,] resampledHeights;
        Rect rectToggle;
        bool extraOptions = false;


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
        bool squareArea = true;
        bool unitsToOneMeter = true;
        bool useScaleFactor = true;

        float scaleFactor = 1;
        int resolutionFinal;
        float extents;
        int smoothIterationsProgress = 1;
        int smoothIterations = 1;
        float smoothBlend = 0.8f;

        float smoothIterationProgress;
        float smoothProgress;
        bool showProgressSmoothen = false;
        int smoothStepIndex = 0;
        bool showProgressSmoothenOperation = false;
        int smoothIndex = 0;

        int smoothBlendIndex = 0;
        string[] smoothBlendMode = new string[] { "OFF", "ON" };

        int tiffWidth;
        int tiffLength;
        float[,] tiffData;
        float[,] tiffDataASCII;
        float[,] tiffDataSplitted;
        float highestPoint;
        float lowestPoint;
        float initialTerrainWidth;

        double UTMEasting;
        double UTMNorthing;
        string sUtmZone;
        double UTMEastingTop;
        double UTMNorthingTop;
        string sUtmZoneTop;
        double cellSize;
        string projectionStr;
        string sCentralMeridian;

        int engineModeIndex = 3;
        string[] engineMode = new string[] { "MANUAL", "LOWEST", "LOW", "MEDIUM", "HIGH" };

        int areaSelectionMode = 0;
        string[] selectionMode = new string[] { "METRICS", "COORDINATES" };

        float[,] heightMapSmoothed;

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
        string terrainName;

        int nCols;
        int nRows;
        double xllCorner;
        double yllCorner;
        double cellSizeASCII;

        int chunkImageResolution;

        string asciiPath;
        string rawPath;
        string tiffPath;

        int heightmapResFinalX;
        int heightmapResFinalY;
        int heightmapResXAll;
        int heightmapResYAll;
        int heightmapResFinalXAll;
        int heightmapResFinalYAll;

        float[,] finalHeights;

        const float everestPeak = 8848.0f;
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
        string downloadDateImagery;
        string unavailableTerrainStr = "No Terrains Selected.\n\nSelect Terrain(s) From The Scene Hierarchy Or Generate New Terrains First.";

        float[] initialHeightsValue;
        bool heightsAnalyzed = false;

        string presetFilePath = "";

        int terrainResolutionTotal;
        int terrainResolutionChunk;
        int textureResolutionTotal;
        int textureResolutionChunk;

        int terrainResolutionDownloading;
        string imageImportingWarning = "EXTRA OPERATIONS WILL BE APPLIED. IMPORTING WILL BE SLOWER";
        string dataResamplingWarning = "NON POWER OF 2 GRID. CAUSES DATA RESAMPLING & QUALITY LOSS";

        List<string> failedTerrainNames;
        int threadsCount = 0;
        //double noData;

        bool showResolutionPresetSection = true;
        bool showAvailableDataSection = true;
        bool showNewTerrainSection = true;
        bool showLocationSection = true;
        bool showAreaSizeSection = true;
        bool showInteractiveMapSection = true;
        bool showHeghtmapDownloaderSection = true;
        bool showSaveElevationSection = true;
        bool showImageDownloaderSection = true;
        bool showFailedDownloaderSection = true;
        bool showVisulizationMapsSection = true;
        bool showServerSection = true;

        InteractiveMap mapWindow;
        int mapWindowIsOpen = 0;
        string mapWindowButtonStr;
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
        static mapTypeBingEnum mapTypeBing = mapTypeBingEnum.Aerial;

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
        static mapTypeYandexEnum mapTypeYandex = mapTypeYandexEnum.MapGeoTraffic;

        bool updateArea = true;
        bool showArea = true;
        bool showCross = true;

        int visualMapIndex = 0;
        string[] visualMapMode = new string[] { "SLOPE", "ASPECT", "HILLSHADE", "ELEVATION" };
        UnityEngine.Color deactivatedCol = new UnityEngine.Color(1, 1, 1, 0.25f);

        bool aspectIsActive = false;
        bool elevationIsActive = false;
        bool hillshadeMDIsActive = false;
        bool slopeIsActive = false;

        string directoryPathVisual;
        string visualMapURLSlope = "";
        string visualMapURLAspect = "";
        string visualMapURLHillshade = "";
        string visualMapURLElevation = "";
        string fileNameSlope = "";
        string fileNameAspect = "";
        string fileNameHillshadeMD = "";
        string fileNameElevation = "";

        float progressBarVisual;
        float progressBarSlope;
        float progressBarAspect;
        float progressBarHillshade;
        float progressBarElevationRaster;

        bool showProgressVisual = false;
        int downloadIndexVisual = 0;
        WebClient webClientVisual;
        int visualMapResolution = 2048;

        private enum VisualFormat
        {
            AI,
            BMP,
            DIB,
            EMF,
            GIF,
            JPG,
            JPGPNG,
            PDF,
            PNG,
            PNG24,
            PNG32,
            SVG,
            TIFF
        }
        static VisualFormat visualFormat = VisualFormat.JPGPNG;
        string visualExtension;

        float slopeZFactor = 0.75f;
        float hillshadeAltitude = 15.0f;
        float hillshadeAzimuth = 315.0f;
        float hillshadeZFactor = 0.75f;
        bool automaticSunPosition = true;
        bool analysisDownloadOnly = false;
        int analysisCount = 0;

        UnityEngine.Object analysisFolder;
        FileAttributes analysisFolderAttr;
        string[] analysismapNames;
        List<string> analysisPreviewMode = new List<string>();
        int analysisPreviewIndex = 0;
        int mapsCount;
        bool mapPreviewIsActive = false;
        string previewStr;
        string projectorName = "Analysis Projector";
        Texture2D analysisMap;
        UnityEngine.Color projectorColor = UnityEngine.Color.black;
        Projector projector;
        GameObject projectorObject;
        float projectorStrength = 2f;
        float projectorPosX;
        float projectorPosY;
        float projectorPosZ;
        bool squareProjector = true;
        float rectangleScaleFactorX;
        float rectangleScaleFactorY;
        float chunkSizeX;
        float chunkSizeZ;
        float projectorOffsetX;
        float projectorOffsetZ;

        Light sun;
        GameObject sunDummy;
        Vector3 sunDirection;

        bool importAtEnd = false;
        string interactiveMapReminder;

        string[] infoFilePath;
        string[] allImageNames;
        int modeIndex = 0;
        string[] mode = new string[] { "STATIC WORLD", "DYNAMIC WORLD" };
        bool dynamicWorld;
        string serverPath;
        bool serverSetUpElevation = false;
        bool serverSetUpImagery = false;
        int formatIndex = 0;
        string[] formatMode = new string[] { "RAW", "ASC", "TIF" };

        static string imageName;
        string elevationFormat;
        string tempPattern = "_Temp";
        bool failedTilesAvailable = false;
        byte[] tempImageBytes;

        GameObject imageImportTiles;
        bool taskDone = false;
        int importAgentsChildrenCount;

        int retries = 0;
        private int reducedheightmapResolution;

        public bool isTopoBathy = true;
        // Above sea-level heights
        //"https://elevation.arcgis.com/arcgis/services/WorldElevation/Terrain/ImageServer?token=";
        // Bathymetric merged with above sea-level heights
        private static string elevationURL = "https://elevation.arcgis.com/arcgis/services/WorldElevation/TopoBathy/ImageServer?token=";

        private const string tokenURL = "https://www.arcgis.com/sharing/rest/oauth2/token/authorize?client_id=n0dpgUwqazrQTyXZ&client_secret=3d4867add8ee47b6ac0c498198995298&grant_type=client_credentials&expiration=20160";


        #endregion

        #region methods


        public void OnEnable()
        {
            LoadResources();

            dataPath = Application.dataPath;
            projectPath = Application.dataPath.Replace("Assets", "");
            corePath = dataPath + "/TerraLand/TerraLand Core/";
            downloadsPath = corePath + "Downloads";
            presetsPath = corePath + "Presets/Downloader";

#if UNITY_WEBPLAYER
			SwitchPlatform();
#endif

            AutoLoad();
        }

        public void LoadResources()
        {
            TextureImporter imageImport;
            bool forceUpdate = false;

            terraUnityImages = Resources.LoadAll("TerraUnity/Images", typeof(Texture2D));
            logo = Resources.Load("TerraUnity/Images/Logo/TerraLand-Downloader_Logo") as Texture2D;
            heightMapLogo = Resources.Load("TerraUnity/Images/Button/Heightmap") as Texture2D;
            landMapLogo = Resources.Load("TerraUnity/Images/Button/Landmap") as Texture2D;
            statusGreen = Resources.Load("TerraUnity/Images/Button/StatusGreen") as Texture2D;
            statusRed = Resources.Load("TerraUnity/Images/Button/StatusRed") as Texture2D;
            landMap = Resources.Load("TerraUnity/Images/Button/Landmap") as Texture2D;
            satelliteImageTemp = Resources.Load("TerraUnity/Downloader/NotDownloaded") as Texture2D;
            terrainButton = Resources.Load("TerraUnity/Images/Button/Terrain") as Texture2D;
            serverButton = Resources.Load("TerraUnity/Images/Button/Server") as Texture2D;

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

            UnityEngine.Object[] visualIcons = Resources.LoadAll("TerraUnity/Images/Visualization Maps", typeof(Texture2D));
            aspectIcon = Resources.Load("TerraUnity/Images/Visualization Maps/Aspect") as Texture2D;
            elevationIcon = Resources.Load("TerraUnity/Images/Visualization Maps/Elevation") as Texture2D;
            hillshadeMDIcon = Resources.Load("TerraUnity/Images/Visualization Maps/HillshadeMultiDirectional") as Texture2D;
            slopeIcon = Resources.Load("TerraUnity/Images/Visualization Maps/Slope") as Texture2D;

            foreach (Texture2D currentImage in visualIcons)
            {
                imageImport = AssetImporter.GetAtPath(AssetDatabase.GetAssetPath(currentImage)) as TextureImporter;

                if (imageImport.maxTextureSize != 64)
                {
                    imageImport.maxTextureSize = 64;
                    forceUpdate = true;
                }

                if (forceUpdate)
                    AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(currentImage), ImportAssetOptions.ForceUpdate);
            }
        }

        public void OnDisable()
        {
            AutoSave();
            RemoveProjector();
            RemoveSunDummy();
        }

        private void SwitchPlatform()
        {
#if UNITY_5_6_OR_NEWER
#if UNITY_2017_3_OR_NEWER
            if (Application.platform == RuntimePlatform.WindowsEditor)
                EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Standalone, BuildTarget.StandaloneWindows);
            else if (Application.platform == RuntimePlatform.OSXEditor)
                EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Standalone, BuildTarget.StandaloneOSX);
            else if (Application.platform == RuntimePlatform.LinuxPlayer)
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

            if (!engineOff)
            {
                GUIStyle buttonStyle = new GUIStyle(EditorStyles.toolbarButton);

                if (Event.current.type == EventType.Repaint)
                    windowWidth = GUILayoutUtility.GetLastRect().width;

                EditorGUILayout.BeginHorizontal();
                GUILayout.Space(windowWidth - 248);

                if (showPresetManager)
                    GUI.backgroundColor = UnityEngine.Color.green;
                else
                    GUI.backgroundColor = UnityEngine.Color.white;

                if (GUILayout.Button("Preset Management", buttonStyle, GUILayout.ExpandWidth(false)))
                {
                    showPresetManager = !showPresetManager;
                    showTerrainBounds = false;
                    showOptions = false;
                }

                GUI.backgroundColor = UnityEngine.Color.white;

                if (showPresetManager)
                {
                    GUILayout.Space(-windowWidth + 275);

                    EditorGUILayout.BeginVertical();
                    GUILayout.FlexibleSpace();
                    GUILayout.Space(40);
                    PresetManager();
                    GUILayout.Space(20);
                    GUILayout.FlexibleSpace();
                    EditorGUILayout.EndVertical();
                }

                GUILayout.Space(5);

                if (showTerrainBounds)
                    GUI.backgroundColor = UnityEngine.Color.green;
                else
                    GUI.backgroundColor = UnityEngine.Color.white;

                if (GUILayout.Button("Terrain Bounds", buttonStyle, GUILayout.ExpandWidth(false)))
                {
                    showTerrainBounds = !showTerrainBounds;
                    showPresetManager = false;
                    showOptions = false;
                }

                GUI.backgroundColor = UnityEngine.Color.white;

                if (showTerrainBounds)
                {
                    GUILayout.Space(-windowWidth + 110);

                    EditorGUILayout.BeginVertical();
                    GUILayout.FlexibleSpace();
                    GUILayout.Space(40);
                    TerrainBounds();
                    GUILayout.Space(20);
                    GUILayout.FlexibleSpace();
                    EditorGUILayout.EndVertical();
                }

                GUILayout.Space(5);


                if (showOptions)
                    GUI.backgroundColor = UnityEngine.Color.green;
                else
                    GUI.backgroundColor = UnityEngine.Color.white;

                EditorGUI.BeginChangeCheck();

                if (GUILayout.Button("Options", buttonStyle, GUILayout.ExpandWidth(false)))
                {
                    showOptions = !showOptions;
                    showPresetManager = false;
                    showTerrainBounds = false;
                }

                if (EditorGUI.EndChangeCheck())
                    GetInitialTerrainHeights();

                GUI.backgroundColor = UnityEngine.Color.white;

                if (showOptions)
                {
                    GUILayout.Space(-windowWidth);

                    EditorGUILayout.BeginVertical();
                    GUILayout.FlexibleSpace();
                    GUILayout.Space(40);
                    OptionsManager();
                    GUILayout.Space(20);
                    GUILayout.FlexibleSpace();
                    EditorGUILayout.EndVertical();
                }

                EditorGUILayout.EndHorizontal();

                GUILayout.Space(20);

                EditorGUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                GUI.backgroundColor = UnityEngine.Color.cyan;
                EditorGUILayout.HelpBox("WORLD MODE", MessageType.None);
                GUI.backgroundColor = UnityEngine.Color.white;
                GUILayout.FlexibleSpace();
                EditorGUILayout.EndHorizontal();

                GUILayout.Space(5);

                EditorGUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                GUI.backgroundColor = UnityEngine.Color.green;
                modeIndex = GUILayout.SelectionGrid(modeIndex, mode, 2);
                GUI.backgroundColor = UnityEngine.Color.white;
                GUILayout.FlexibleSpace();
                EditorGUILayout.EndHorizontal();

                if (modeIndex == 1)
                {
                    dynamicWorld = true;
                    textureOnFinish = 1;
                    importAtEnd = false;
                    saveTerrainDataRAW = true;
                }
                else
                    dynamicWorld = false;

                if (showProgressElevation || showProgressImagery || showProgressData || showProgressGenerateASCII || showProgressGenerateRAW || showProgressSmoothen || showProgressSmoothenOperation || showProgressVisual)
                {
                    GUILayout.Space(10);

                    EditorGUILayout.BeginVertical(GUI.skin.box, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));

                    GUILayout.Space(15);


                    // Heightmap Downloader Progress

                    if (showProgressElevation)
                    {
                        EditorGUILayout.BeginHorizontal();
                        GUILayout.FlexibleSpace();

                        Rect rect = GUILayoutUtility.GetLastRect();
                        rect.x = 47;
                        rect.width = position.width - 100;
                        rect.height = 18;

                        int percentage = Mathf.RoundToInt(progressBarElevation * 100f);

                        if (convertingElevationTiles)
                            EditorGUI.ProgressBar(rect, progressBarElevation, "Converting Elevation Tiles\t" + percentage + "%");
                        else if (stitchingElevationTiles)
                            EditorGUI.ProgressBar(rect, progressBarElevation, "Stitching Elevation Tiles\t" + percentage + "%");
                        else
                            EditorGUI.ProgressBar(rect, progressBarElevation, "Downloading Elevation Data\t" + percentage + "%");

                        GUILayout.FlexibleSpace();
                        EditorGUILayout.EndHorizontal();

                        if (downloadIndexSRTM != percentage)
                        {
                            Repaint();
                            downloadIndexSRTM = percentage;
                        }

                        if (!dynamicWorld && percentage > 0)
                        {
                            GUILayout.Space(25);

                            EditorGUILayout.BeginHorizontal();
                            GUILayout.FlexibleSpace();
                            EditorGUILayout.HelpBox("SPEED: " + downloadSpeedTerrain + " Kbps", MessageType.None);
                            GUILayout.Space(10);
                            EditorGUILayout.HelpBox(dataReceivedTerrain, MessageType.None);
                            GUILayout.FlexibleSpace();
                            EditorGUILayout.EndHorizontal();

                            //						EditorGUILayout.BeginHorizontal();
                            //						GUILayout.FlexibleSpace();
                            //						rect.height = 8;
                            //						rect.y = rect.y + 63;
                            //						float percentSpeed = float.Parse(downloadSpeedTerrain) / 1024f;
                            //						EditorGUI.ProgressBar(rect, percentSpeed, "");
                            //						GUILayout.FlexibleSpace();
                            //						EditorGUILayout.EndHorizontal();
                        }

                        GUILayout.Space(25);
                    }

                    if (Event.current.type == EventType.Repaint && progressBarElevation == 1f)
                        progressBarElevation = 0f;


                    // Visual Maps Downloader Progress

                    if (showProgressVisual)
                    {
                        EditorGUILayout.BeginHorizontal();
                        GUILayout.FlexibleSpace();

                        Rect rect = GUILayoutUtility.GetLastRect();
                        rect.x = 47;
                        rect.width = position.width - 100;
                        rect.height = 18;

                        int percentage = Mathf.RoundToInt(progressBarVisual * 100f);
                        EditorGUI.ProgressBar(rect, progressBarVisual, "Downloading Visual Maps\t" + percentage + "%");

                        GUILayout.FlexibleSpace();
                        EditorGUILayout.EndHorizontal();

                        if (downloadIndexVisual != percentage)
                        {
                            Repaint();
                            downloadIndexVisual = percentage;
                        }

                        GUILayout.Space(25);
                    }

                    if (Event.current.type == EventType.Repaint && progressBarVisual == 1f)
                        FinalizeAnalysisMaps();


                    // Satellite Image Downloader Progress

                    if (showProgressImagery)
                    {
                        EditorGUILayout.BeginHorizontal();
                        GUILayout.FlexibleSpace();

                        Rect rect = GUILayoutUtility.GetLastRect();
                        rect.x = 47;
                        rect.width = position.width - 100;
                        rect.height = 18;

                        if (!failedDownloading)
                            normalizedProgressSatelliteImage = Mathf.InverseLerp(0f, 1f, ((float)downloadedImageIndex / (float)totalImages));
                        else
                            normalizedProgressSatelliteImage = Mathf.InverseLerp(0f, 1f, ((float)downloadedImageIndex / (float)totalFailedImages));

                        string str = "";

                        if (downloadedImageIndex == 0)
                        {
                            if (totalImages == 1)
                                str = "Downloading Satellite Image";
                            else
                                str = "Initializing Satellite Image Downloader";
                        }
                        else if (downloadedImageIndex < totalImages)
                        {
                            if (!failedDownloading)
                                str = "Image   " + downloadedImageIndex + "   of   " + totalImages.ToString() + "   Downloaded";
                            else
                                str = "Image   " + downloadedImageIndex + "   of   " + totalFailedImages.ToString() + "   Failed Images Downloaded";
                        }
                        else
                            str = "Finished Downloading";

                        EditorGUI.ProgressBar(rect, normalizedProgressSatelliteImage, str);

                        GUILayout.FlexibleSpace();
                        EditorGUILayout.EndHorizontal();

                        if (downloadIndexSatellite != downloadedImageIndex)
                        {
                            Repaint();
                            downloadIndexSatellite = downloadedImageIndex;
                        }

                        GUILayout.Space(25);
                    }

                    if (Event.current.type == EventType.Repaint && normalizedProgressSatelliteImage == 1f)
                    {
                        showProgressImagery = false;
                        normalizedProgressSatelliteImage = 0f;
                        FinalizeTerrainImagery(true);
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

                    // Show Downloading Status

                    if (showProgressImagery)
                    {
                        GUILayout.Space(15);

                        GUI.backgroundColor = UnityEngine.Color.clear;
                        EditorGUILayout.BeginHorizontal();
                        GUILayout.FlexibleSpace();

                        //if(showProgressElevation)
                        //	threadsCount = (allThreads + 3) - workerThreads;
                        //else
                        //	threadsCount = allThreads - workerThreads;

                        //threadsCount = Mathf.Clamp(threadsCount, 0, 1000);

                        threadsCount = Mathf.Clamp(allThreads - workerThreads, 0, 1000);

                        EditorGUILayout.HelpBox("THREADS   " + (threadsCount).ToString(), MessageType.None);

                        GUILayout.FlexibleSpace();
                        EditorGUILayout.EndHorizontal();
                        GUI.backgroundColor = UnityEngine.Color.white;
                    }

                    GUILayout.Space(10);

                    EditorGUILayout.EndVertical();
                }

                if (modeIndex == 0)
                {
                    GUILayout.Space(20);

                    if (terrain || splittedTerrains)
                        GUI.color = UnityEngine.Color.green;
                    else
                        GUI.color = UnityEngine.Color.red;

                    EditorGUILayout.BeginHorizontal();
                    GUILayout.FlexibleSpace();

                    GUIStyle myStyle = new GUIStyle(GUI.skin.box);
                    myStyle.fontSize = 10;
                    myStyle.normal.textColor = UnityEngine.Color.black;

                    Rect rectTerrains = GUILayoutUtility.GetLastRect();
                    rectTerrains.x = GUILayoutUtility.GetLastRect().width - 150;
                    rectTerrains.width = 300;
                    rectTerrains.height = 20;

                    if (splittedTerrains)
                    {
                        try
                        {
                            heightmapResolutionSplit = heightmapResolution / (int)Mathf.Sqrt((float)terrainChunks);
                            splitSizeFinal = (int)Mathf.Sqrt((float)croppedTerrains.Count);
                            totalImages = Mathf.RoundToInt(Mathf.Pow(gridPerTerrain, 2)) * terrainChunks;
                            gridNumber = Mathf.RoundToInt(Mathf.Sqrt(totalImages));
                            chunkImageResolution = (imageResolution * gridNumber) / (int)Mathf.Sqrt((float)terrainChunks);
                        }
                        catch { }

                        GUI.Box(rectTerrains, new GUIContent(terrainChunks.ToString() + "  AVAILABLE TERRAINS"), myStyle);

                        rectTerrains.y = rectTerrains.y + 20;
                        GUI.Box(rectTerrains, new GUIContent(totalImages.ToString() + "  SATELLITE IMAGES"), myStyle);
                    }
                    else if (terrain)
                    {
                        terrainChunks = 1;
                        heightmapResolutionSplit = heightmapResolution;
                        splitSizeFinal = 1;
                        totalImages = Mathf.RoundToInt(Mathf.Pow(gridPerTerrain, 2));
                        gridNumber = gridPerTerrain;

                        GUI.Box(rectTerrains, new GUIContent("1 AVAILABLE TERRAIN"), myStyle);

                        rectTerrains.y = rectTerrains.y + 20;
                        if (totalImages == 1)
                            GUI.Box(rectTerrains, new GUIContent(totalImages.ToString() + "  SATELLITE IMAGE"), myStyle);
                        else
                            GUI.Box(rectTerrains, new GUIContent(totalImages.ToString() + "  SATELLITE IMAGES"), myStyle);
                    }
                    else
                    {
                        try
                        {
                            terrainChunks = totalTerrainsNew;
                            heightmapResolutionSplit = heightmapResolution / (int)Mathf.Sqrt((float)terrainChunks);
                            splitSizeFinal = (int)Mathf.Sqrt(terrainChunks);
                            totalImages = Mathf.RoundToInt(Mathf.Pow(gridPerTerrain, 2)) * terrainChunks;
                            gridNumber = Mathf.RoundToInt(Mathf.Sqrt(totalImages));
                            chunkImageResolution = (imageResolution * gridNumber) / (int)Mathf.Sqrt((float)terrainChunks);
                        }
                        catch { }

                        if (terrainChunks == 1)
                            GUI.Box(rectTerrains, new GUIContent(totalTerrainsNew + "  TERRAIN WILL BE GENERATED"), myStyle);
                        else
                            GUI.Box(rectTerrains, new GUIContent(totalTerrainsNew + "  TERRAINS WILL BE GENERATED"), myStyle);

                        rectTerrains.y = rectTerrains.y + 20;
                        if (totalImages == 1)
                            GUI.Box(rectTerrains, new GUIContent(totalImages.ToString() + "  SATELLITE IMAGE"), myStyle);
                        else
                            GUI.Box(rectTerrains, new GUIContent(totalImages.ToString() + "  SATELLITE IMAGES"), myStyle);
                    }

                    GUILayout.FlexibleSpace();
                    EditorGUILayout.EndHorizontal();

                    GUI.color = UnityEngine.Color.white;

                    GUILayout.Space(50);
                }
                else if (modeIndex == 1)
                {
                    heightmapResolutionSplit = heightmapResolution;
                    splitSizeFinal = 1;
                    gridPerTerrain = (int)serverGrid;
                    totalImages = (int)Mathf.Pow(gridPerTerrain, 2);
                    gridNumber = gridPerTerrain;

                    //terrainChunks = 1;
                    terrainChunks = totalImages;

                    GUILayout.Space(15);
                }


                //***********************************************************************************************************************************************************************


                EditorGUILayout.BeginVertical(GUI.skin.box, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
                scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));

                empty = "";

                double dTemp = 0;
                latitudeUser = Regex.Replace(latitudeUser, "[^0-9.-]", "");
                longitudeUser = Regex.Replace(longitudeUser, "[^0-9.-]", "");

                Double.TryParse(latitudeUser, out dTemp);
                latitudeUser = dTemp.ToString();
                Double.TryParse(longitudeUser, out dTemp);
                longitudeUser = dTemp.ToString();

                if (Double.Parse(latitudeUser) > 90)
                    latitudeUser = "90";
                else if (Double.Parse(latitudeUser) < -90)
                    latitudeUser = "-90";

                if (Double.Parse(longitudeUser) > 180)
                    longitudeUser = "180";
                else if (Double.Parse(longitudeUser) < -180)
                    longitudeUser = "-180";

                top = Regex.Replace(top, "[^0-9.-]", "");
                bottom = Regex.Replace(bottom, "[^0-9.-]", "");
                left = Regex.Replace(left, "[^0-9.-]", "");
                right = Regex.Replace(right, "[^0-9.-]", "");

                Double.TryParse(top, out dTemp);
                top = dTemp.ToString();
                Double.TryParse(bottom, out dTemp);
                bottom = dTemp.ToString();
                Double.TryParse(left, out dTemp);
                left = dTemp.ToString();
                Double.TryParse(right, out dTemp);
                right = dTemp.ToString();

                if (Double.Parse(top) > 90)
                    top = "90";
                else if (Double.Parse(top) < -89.999999)
                    top = "-89.999999";

                if (Double.Parse(bottom) < -90)
                    bottom = "-90";
                else if (Double.Parse(bottom) > 89.999999)
                    bottom = "89.999999";

                if (Double.Parse(right) > 180)
                    right = "180";
                else if (Double.Parse(right) < -179.999999)
                    right = "-179.999999";

                if (Double.Parse(left) < -180)
                    left = "-180";
                else if (Double.Parse(left) > 179.999999)
                    left = "179.999999";

                if (Double.Parse(bottom) >= Double.Parse(top))
                    bottom = (Double.Parse(top) - 0.000001).ToString();

                if (Double.Parse(left) >= Double.Parse(right))
                    left = (Double.Parse(right) - 0.000001).ToString();

                Double.TryParse(latitudeUser, out dTemp);
                latitudeUser = dTemp.ToString();
                Double.TryParse(longitudeUser, out dTemp);
                longitudeUser = dTemp.ToString();

                GUILayout.Space(5);

                if (modeIndex == 1)
                {
                    GUI.backgroundColor = UnityEngine.Color.gray;
                    EditorGUILayout.HelpBox("\nSTREAMING GEO-SERVER\n", MessageType.None);
                    GUI.backgroundColor = UnityEngine.Color.white;

                    showServerSection = EditorGUILayout.Foldout(showServerSection, "");

                    if (showServerSection)
                    {
                        GUILayout.Space(10);

                        EditorGUILayout.BeginHorizontal();
                        GUILayout.FlexibleSpace();
                        EditorGUILayout.HelpBox("TILES GRID", MessageType.None);

                        serverGrid = (ServerGrid)EditorGUILayout.EnumPopup(serverGrid);
                        splitSizeNew = (int)serverGrid;
                        totalTerrainsNew = Mathf.RoundToInt(Mathf.Pow(splitSizeNew, 2));

                        GUI.backgroundColor = UnityEngine.Color.green;

                        GUILayout.Space(5);

                        EditorGUILayout.HelpBox(totalTerrainsNew.ToString(), MessageType.None);
                        GUI.backgroundColor = UnityEngine.Color.gray;
                        EditorGUILayout.HelpBox("TERRAINS", MessageType.None);
                        GUI.backgroundColor = UnityEngine.Color.white;

                        GUILayout.FlexibleSpace();
                        EditorGUILayout.EndHorizontal();

                        GUILayout.Space(40);

                        GUI.backgroundColor = UnityEngine.Color.clear;
                        GUILayout.Button(heightMapLogo);
                        GUI.backgroundColor = UnityEngine.Color.white;

                        GUILayout.Space(10);

                        EditorGUILayout.BeginHorizontal();
                        GUILayout.FlexibleSpace();
                        if (GUILayout.Button("<<"))
                        {
                            heightmapResolution /= 2;
                        }

                        GUILayout.Space(10);

                        if (GUILayout.Button(">>"))
                        {
                            heightmapResolution *= 2;
                        }
                        GUILayout.FlexibleSpace();
                        EditorGUILayout.EndHorizontal();

                        GUILayout.Space(10);

                        EditorGUILayout.BeginHorizontal();
                        GUILayout.FlexibleSpace();
                        EditorGUILayout.HelpBox("PIXELS", MessageType.None);
                        heightmapResolution = EditorGUILayout.IntSlider(Mathf.ClosestPowerOfTwo(heightmapResolution), 32, 1024);
                        GUILayout.FlexibleSpace();
                        EditorGUILayout.EndHorizontal();

                        GUILayout.Space(10);

                        GUI.color = UnityEngine.Color.green;
                        EditorGUILayout.BeginHorizontal();
                        GUILayout.FlexibleSpace();

                        GUIStyle myStyle = new GUIStyle(GUI.skin.box);
                        myStyle.fontSize = 20;
                        myStyle.normal.textColor = UnityEngine.Color.black;

                        rectToggle = GUILayoutUtility.GetLastRect();
                        rectToggle.x = GUILayoutUtility.GetLastRect().width - 50;
                        rectToggle.width = 100;
                        rectToggle.height = 30;

                        GUI.Box(rectToggle, new GUIContent(heightmapResolution.ToString()), myStyle);

                        GUILayout.FlexibleSpace();
                        EditorGUILayout.EndHorizontal();
                        GUI.color = UnityEngine.Color.white;

                        GUILayout.Space(100);

                        GUI.backgroundColor = UnityEngine.Color.clear;
                        GUILayout.Button(landMapLogo);
                        GUI.backgroundColor = UnityEngine.Color.white;

                        GUILayout.Space(10);

                        EditorGUILayout.BeginHorizontal();
                        GUILayout.FlexibleSpace();
                        if (GUILayout.Button("<<"))
                        {
                            imageResolution /= 2;
                        }

                        GUILayout.Space(10);

                        if (GUILayout.Button(">>"))
                        {
                            imageResolution *= 2;
                        }
                        GUILayout.FlexibleSpace();
                        EditorGUILayout.EndHorizontal();

                        GUILayout.Space(10);

                        EditorGUILayout.BeginHorizontal();
                        GUILayout.FlexibleSpace();
                        EditorGUILayout.HelpBox("PIXELS", MessageType.None);
                        imageResolution = EditorGUILayout.IntSlider(Mathf.ClosestPowerOfTwo(imageResolution), 32, 2048);
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

                        GUI.Box(rectToggle, new GUIContent(imageResolution.ToString()), myStyle);

                        GUILayout.FlexibleSpace();
                        EditorGUILayout.EndHorizontal();
                        GUI.color = UnityEngine.Color.white;

                        GUILayout.Space(100);

                        EditorGUILayout.BeginHorizontal();
                        GUILayout.FlexibleSpace();
                        EditorGUILayout.HelpBox("SMOOTH STEPS", MessageType.None);
                        smoothIterations = EditorGUILayout.IntSlider(smoothIterations, 0, 10);
                        GUILayout.FlexibleSpace();
                        EditorGUILayout.EndHorizontal();

                        GUILayout.Space(60);

                        EditorGUILayout.BeginHorizontal();
                        GUILayout.FlexibleSpace();
                        EditorGUILayout.HelpBox("EXPORT FORMAT", MessageType.None, true);
                        GUILayout.FlexibleSpace();
                        EditorGUILayout.EndHorizontal();

                        GUILayout.Space(10);

                        EditorGUILayout.BeginHorizontal();
                        GUILayout.FlexibleSpace();
                        formatIndex = GUILayout.SelectionGrid(formatIndex, formatMode, 3);
                        GUILayout.FlexibleSpace();
                        EditorGUILayout.EndHorizontal();

                        GUILayout.Space(20);

                        if (formatIndex != 0)
                        {
                            EditorGUILayout.BeginHorizontal();
                            GUILayout.FlexibleSpace();
                            EditorGUILayout.HelpBox("Only RAW format is supported in TerraLand's Streaming system unless for personal use!", MessageType.Warning);
                            GUILayout.FlexibleSpace();
                            EditorGUILayout.EndHorizontal();
                        }

                        GUILayout.Space(100);
                    }
                    else
                        GUILayout.Space(15);
                }

                if (modeIndex == 0)
                {
                    CheckTerrainSizeUnits();

                    GUI.backgroundColor = UnityEngine.Color.gray;
                    EditorGUILayout.HelpBox("\nENGINE RESOLUTION PRESETS\n", MessageType.None);
                    GUI.backgroundColor = UnityEngine.Color.white;

                    showResolutionPresetSection = EditorGUILayout.Foldout(showResolutionPresetSection, "");

                    if (showResolutionPresetSection)
                    {
                        GUILayout.Space(30);

                        EditorGUILayout.BeginHorizontal();
                        GUILayout.FlexibleSpace();
                        EditorGUILayout.HelpBox("RESOLUTION MODE", MessageType.None);
                        GUILayout.FlexibleSpace();
                        EditorGUILayout.EndHorizontal();

                        GUILayout.Space(15);

                        if (engineModeIndex == 1)
                            GUI.backgroundColor = new UnityEngine.Color(1f, 0.8f, 0.6f);
                        else if (engineModeIndex == 2)
                            GUI.backgroundColor = new UnityEngine.Color(1f, 0.6f, 0.4f);
                        else if (engineModeIndex == 3)
                            GUI.backgroundColor = new UnityEngine.Color(1f, 0.5f, 0.2f);
                        else if (engineModeIndex == 4)
                            GUI.backgroundColor = new UnityEngine.Color(1f, 0.4f, 0.0f);

                        EditorGUILayout.BeginHorizontal();
                        GUILayout.FlexibleSpace();
                        engineModeIndex = GUILayout.SelectionGrid(engineModeIndex, engineMode, 5);
                        GUILayout.FlexibleSpace();
                        EditorGUILayout.EndHorizontal();

                        if (engineModeIndex == 1 || engineModeIndex == 2 || engineModeIndex == 3 || engineModeIndex == 4)
                        {
                            //                    if(!splittedTerrains && !terrain)
                            //                    {
                            //                        terrainSizeNewX  = areaSizeLat * 1000f;
                            //                        terrainSizeNewZ  = areaSizeLon * 1000f;
                            //                        constrainedAspect = true;
                            //                    }

                            textureOnFinish = 0;
                        }

                        if (engineModeIndex == 1)
                        {
                            if (!splittedTerrains && !terrain)
                            {
                                enumValueNew = SizeNew._1;
                                splitSizeNew = (int)enumValueNew;
                                totalTerrainsNew = Mathf.RoundToInt(Mathf.Pow(splitSizeNew, 2));
                            }

                            heightmapResolution = 512;
                            gridPerTerrain = 1;
                            imageResolution = 512;
                        }
                        else if (engineModeIndex == 2)
                        {
                            if (!splittedTerrains && !terrain)
                            {
                                enumValueNew = SizeNew._1;
                                splitSizeNew = (int)enumValueNew;
                                totalTerrainsNew = Mathf.RoundToInt(Mathf.Pow(splitSizeNew, 2));
                            }

                            heightmapResolution = 1024;
                            gridPerTerrain = 1;
                            imageResolution = 2048;
                        }
                        else if (engineModeIndex == 3)
                        {
                            if (!splittedTerrains && !terrain)
                            {
                                enumValueNew = SizeNew._2x2;
                                splitSizeNew = (int)enumValueNew;
                                totalTerrainsNew = Mathf.RoundToInt(Mathf.Pow(splitSizeNew, 2));
                            }

                            heightmapResolution = 2048;
                            gridPerTerrain = 1;
                            imageResolution = 4096;
                        }
                        else if (engineModeIndex == 4)
                        {
                            if (!splittedTerrains && !terrain)
                            {
                                enumValueNew = SizeNew._2x2;
                                splitSizeNew = (int)enumValueNew;
                                totalTerrainsNew = Mathf.RoundToInt(Mathf.Pow(splitSizeNew, 2));
                            }

                            heightmapResolution = 4096;
                            gridPerTerrain = 2;
                            imageResolution = 4096;
                        }

                        GUI.backgroundColor = UnityEngine.Color.white;

                        GUILayout.Space(40);

                        EditorGUILayout.BeginHorizontal();
                        GUILayout.FlexibleSpace();

                        if (engineModeIndex == 0)
                            EditorGUILayout.HelpBox("SET HEIGHTMAP & IMAGERY RESOLUTIONS MANUALLY", MessageType.Warning);
                        else
                            EditorGUILayout.HelpBox("AUTOMATIC RESOLUTIONS - SELECT MANUAL FOR CUSTOM RESOLUTIONS", MessageType.Warning);

                        GUILayout.FlexibleSpace();
                        EditorGUILayout.EndHorizontal();

                        GUILayout.Space(30);

                        GUI.backgroundColor = UnityEngine.Color.gray;

                        EditorGUILayout.BeginHorizontal();
                        GUILayout.FlexibleSpace();
                        EditorGUILayout.HelpBox("Total Terrain Resolution: " + terrainResolutionTotal, MessageType.None);
                        GUILayout.FlexibleSpace();
                        EditorGUILayout.EndHorizontal();

                        EditorGUILayout.BeginHorizontal();
                        GUILayout.FlexibleSpace();
                        EditorGUILayout.HelpBox("Total Image Resolution: " + textureResolutionTotal, MessageType.None);
                        GUILayout.FlexibleSpace();
                        EditorGUILayout.EndHorizontal();

                        if (totalTerrainsNew > 1)
                        {
                            EditorGUILayout.BeginHorizontal();
                            GUILayout.FlexibleSpace();
                            EditorGUILayout.HelpBox("Chunk Terrain Resolution: " + terrainResolutionChunk, MessageType.None);
                            GUILayout.FlexibleSpace();
                            EditorGUILayout.EndHorizontal();

                            EditorGUILayout.BeginHorizontal();
                            GUILayout.FlexibleSpace();
                            EditorGUILayout.HelpBox("Chunk Image Resolution: " + textureResolutionChunk, MessageType.None);
                            GUILayout.FlexibleSpace();
                            EditorGUILayout.EndHorizontal();
                        }

                        EditorGUILayout.BeginHorizontal();
                        GUILayout.FlexibleSpace();
                        EditorGUILayout.HelpBox("Total Terrains: " + totalTerrainsNew, MessageType.None);
                        GUILayout.FlexibleSpace();
                        EditorGUILayout.EndHorizontal();

                        EditorGUILayout.BeginHorizontal();
                        GUILayout.FlexibleSpace();
                        EditorGUILayout.HelpBox("Total Images: " + totalImages, MessageType.None);
                        GUILayout.FlexibleSpace();
                        EditorGUILayout.EndHorizontal();

                        GUI.backgroundColor = UnityEngine.Color.white;

                        GUILayout.Space(100);
                    }
                    else
                        GUILayout.Space(15);
                }

                GUI.backgroundColor = UnityEngine.Color.gray;
                EditorGUILayout.HelpBox("\nINTERACTIVE MAP\n", MessageType.None);
                GUI.backgroundColor = UnityEngine.Color.white;

                showInteractiveMapSection = EditorGUILayout.Foldout(showInteractiveMapSection, "");

                if (showInteractiveMapSection)
                {
                    mapWindowIsOpen = Resources.FindObjectsOfTypeAll<InteractiveMap>().Length;

                    if (updateArea && mapWindowIsOpen == 1)
                    {
                        latitudeUser = InteractiveMap.map_latlong_center.latitude.ToString();
                        longitudeUser = InteractiveMap.map_latlong_center.longitude.ToString();

                        zoomLevel = InteractiveMap.map_zoom;
                    }

                    GUILayout.Space(30);

                    mapWindowIsOpen = Resources.FindObjectsOfTypeAll<InteractiveMap>().Length;

                    if (mapWindowIsOpen == 0)
                        mapWindowButtonStr = "\nSHOW MAP\n";
                    else
                        mapWindowButtonStr = "\nFOCUS MAP\n";

                    EditorGUILayout.BeginHorizontal();
                    GUILayout.FlexibleSpace();
                    if (GUILayout.Button(mapWindowButtonStr))
                    {
                        ShowMapAndRefresh(false);
                    }
                    GUILayout.FlexibleSpace();
                    EditorGUILayout.EndHorizontal();

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
                        ShowMapAndRefresh(true);

                    GUILayout.Space(40);

                    EditorGUILayout.BeginHorizontal();
                    GUILayout.FlexibleSpace();
                    GUI.backgroundColor = UnityEngine.Color.clear;
                    EditorGUILayout.HelpBox("UPDATE AREA", MessageType.None);
                    GUI.backgroundColor = UnityEngine.Color.white;
                    rectToggle = GUILayoutUtility.GetLastRect();
                    rectToggle.x = (rectToggle.width / 2f) + 265f;
                    updateArea = EditorGUI.Toggle(rectToggle, updateArea);
                    GUILayout.FlexibleSpace();
                    EditorGUILayout.EndHorizontal();

                    if (updateArea)
                        InteractiveMap.updateArea = true;
                    else
                        InteractiveMap.updateArea = false;

                    GUILayout.Space(10);

                    EditorGUILayout.BeginHorizontal();
                    GUILayout.FlexibleSpace();
                    GUI.backgroundColor = UnityEngine.Color.clear;
                    EditorGUILayout.HelpBox("SHOW AREA", MessageType.None);
                    GUI.backgroundColor = UnityEngine.Color.white;
                    rectToggle = GUILayoutUtility.GetLastRect();
                    rectToggle.x = (rectToggle.width / 2f) + 265f;
                    showArea = EditorGUI.Toggle(rectToggle, showArea);
                    GUILayout.FlexibleSpace();
                    EditorGUILayout.EndHorizontal();

                    if (showArea)
                        InteractiveMap.showArea = true;
                    else
                        InteractiveMap.showArea = false;

                    GUILayout.Space(10);

                    EditorGUILayout.BeginHorizontal();
                    GUILayout.FlexibleSpace();
                    GUI.backgroundColor = UnityEngine.Color.clear;
                    EditorGUILayout.HelpBox("SHOW CENTER", MessageType.None);
                    GUI.backgroundColor = UnityEngine.Color.white;
                    rectToggle = GUILayoutUtility.GetLastRect();
                    rectToggle.x = (rectToggle.width / 2f) + 265f;
                    showCross = EditorGUI.Toggle(rectToggle, showCross);
                    GUILayout.FlexibleSpace();
                    EditorGUILayout.EndHorizontal();

                    if (showCross)
                        InteractiveMap.showCross = true;
                    else
                        InteractiveMap.showCross = false;

                    GUILayout.Space(100);
                }
                else
                    GUILayout.Space(15);

                GUI.backgroundColor = UnityEngine.Color.gray;
                EditorGUILayout.HelpBox("\nAREA LOCATION\n", MessageType.None);
                GUI.backgroundColor = UnityEngine.Color.white;

                showLocationSection = EditorGUILayout.Foldout(showLocationSection, "");

                if (showLocationSection)
                {
                    GUILayout.Space(30);

                    GUI.backgroundColor = new UnityEngine.Color(0.0f, 0.5f, 1f, 1f);
                    EditorGUILayout.BeginHorizontal();
                    GUILayout.FlexibleSpace();
                    EditorGUILayout.HelpBox("Lat: " + latitudeUser + "        Lon: " + longitudeUser, MessageType.None);
                    GUILayout.FlexibleSpace();
                    EditorGUILayout.EndHorizontal();
                    GUI.backgroundColor = UnityEngine.Color.white;

                    GUILayout.Space(30);

                    EditorGUILayout.BeginHorizontal();
                    GUILayout.FlexibleSpace();

                    EditorGUILayout.HelpBox("ADDRESS/LOCATION", MessageType.None, true);
                    address = EditorGUILayout.TextField(address);
                    GUILayout.FlexibleSpace();
                    EditorGUILayout.EndHorizontal();

                    GUILayout.Space(5);

                    EditorGUILayout.BeginHorizontal();
                    GUILayout.FlexibleSpace();
                    if (GUILayout.Button("SEARCH" + empty))
                    {
                        coords = GeoCoder.AddressToLatLong(Regex.Replace(address, @"\s+", string.Empty));
                        locations = GeoCoder.foundLocations;
                    }
                    GUILayout.FlexibleSpace();
                    EditorGUILayout.EndHorizontal();

                    GUILayout.Space(20);

                    if (coords != null && locations != null)
                    {
                        for (int i = 0; i < coords.Count; i++)
                        {
                            EditorGUILayout.BeginHorizontal();
                            GUILayout.FlexibleSpace();

                            GUI.backgroundColor = UnityEngine.Color.gray;
                            EditorGUILayout.HelpBox(locations[i], MessageType.None, true);
                            GUI.backgroundColor = UnityEngine.Color.white;

                            GUILayout.Space(10);
                            EditorGUILayout.TextArea(coords[i].x.ToString() + "   " + coords[i].y.ToString());

                            if (GUILayout.Button("SET LOCATION"))
                            {
                                latitudeUser = coords[i].x.ToString();
                                longitudeUser = coords[i].y.ToString();

                                ShowMapAndRefresh(true);
                            }

                            GUILayout.FlexibleSpace();
                            EditorGUILayout.EndHorizontal();

                            GUILayout.Space(10);
                        }
                    }

                    if (!GeoCoder.recognized)
                    {
                        EditorGUILayout.BeginHorizontal();
                        GUILayout.FlexibleSpace();
                        EditorGUILayout.HelpBox("Address/Location Is Not Recognized", MessageType.Error, true);
                        GUILayout.FlexibleSpace();
                        EditorGUILayout.EndHorizontal();
                    }
                    else
                        GUILayout.Space(5);

                    GUILayout.Space(20);

                    EditorGUILayout.BeginHorizontal();
                    GUILayout.FlexibleSpace();
                    GUI.backgroundColor = new UnityEngine.Color(0.0f, 0.5f, 1f, 1f);
                    EditorGUILayout.HelpBox("LATITUDE", MessageType.None);
                    GUI.backgroundColor = UnityEngine.Color.white;
                    latitudeUser = EditorGUILayout.TextField(latitudeUser);
                    GUILayout.FlexibleSpace();
                    EditorGUILayout.EndHorizontal();

                    GUILayout.Space(10);

                    EditorGUILayout.BeginHorizontal();
                    GUILayout.FlexibleSpace();
                    GUI.backgroundColor = new UnityEngine.Color(0.0f, 0.5f, 1f, 1f);
                    EditorGUILayout.HelpBox("LONGITUDE", MessageType.None);
                    GUI.backgroundColor = UnityEngine.Color.white;
                    longitudeUser = EditorGUILayout.TextField(longitudeUser);
                    GUILayout.FlexibleSpace();
                    EditorGUILayout.EndHorizontal();

                    //GUILayout.Space(5);

                    //EditorGUILayout.BeginHorizontal();
                    //GUILayout.FlexibleSpace();
                    //if (GUILayout.Button("GET ADDRESS"))
                    //{
                    //    address = GeoCoder.LatLongToAddress(longitudeUser, latitudeUser, out empty, out empty, out empty, out empty, out empty, out empty, out empty, out empty, out empty);
                    //}
                    //GUILayout.FlexibleSpace();
                    //EditorGUILayout.EndHorizontal();

                    GUILayout.Space(10);

                    if (mapWindowIsOpen == 0)
                        interactiveMapReminder = "OPEN INTERACTIVE MAP TO AUTOMATICALLY SET AREA ON MAP";
                    else if (mapWindowIsOpen == 1)
                        interactiveMapReminder = "CLOSE INTERACTIVE MAP TO MANUALLY INSERT COORDINATES";

                    EditorGUILayout.BeginHorizontal();
                    GUILayout.FlexibleSpace();

                    if (mapWindowIsOpen == 0)
                        EditorGUILayout.HelpBox(interactiveMapReminder, MessageType.Info);
                    else if (mapWindowIsOpen == 1)
                        EditorGUILayout.HelpBox(interactiveMapReminder, MessageType.Warning);

                    GUILayout.FlexibleSpace();
                    EditorGUILayout.EndHorizontal();

                    GUILayout.Space(60);

                    EditorGUILayout.BeginHorizontal();
                    GUILayout.FlexibleSpace();
                    EditorGUILayout.HelpBox("LOAD FROM FILE", MessageType.None);
                    coordinatesXMLFile = EditorGUILayout.ObjectField(coordinatesXMLFile, typeof(UnityEngine.Object), true) as UnityEngine.Object;
                    GUILayout.FlexibleSpace();
                    EditorGUILayout.EndHorizontal();

                    if (coordinatesXMLFile)
                    {
                        string xmlPath = AssetDatabase.GetAssetPath(coordinatesXMLFile);

                        if (!xmlPath.EndsWith(".xml"))
                        {
                            EditorUtility.DisplayDialog("UNKNOWN XML", "Please insert a valid XML file with format of \"xml\" which TerraLand Downloader has previously generated and contains geo-coordinates.", "Ok");
                            coordinatesXMLFile = null;
                        }

                        ReadXMLFile(xmlPath);
                    }

                    GUILayout.Space(100);
                }
                else
                    GUILayout.Space(15);

                if (modeIndex == 0)
                {
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

                        if (unitsToOneMeter || useScaleFactor)
                        {
                            GUILayout.Space(5);

                            EditorGUILayout.BeginHorizontal();
                            GUILayout.FlexibleSpace();
                            EditorGUILayout.HelpBox("Manual size selection is locked, \"Units To 1 Meter\" or \"Use Scale Factor\" is asctivated!", MessageType.Warning);
                            GUILayout.FlexibleSpace();
                            EditorGUILayout.EndHorizontal();
                        }

                        GUILayout.Space(30);

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

                        //if (areaSizeLat == areaSizeLon)
                        //    areaIsSquare = true;
                        //else
                        //    areaIsSquare = false;
                        //
                        //if(!areaIsSquare)
                        //{
                        //    constrainedAspect = true;
                        //
                        //    GUILayout.Space(5);
                        //
                        //    EditorGUILayout.BeginHorizontal();
                        //    GUILayout.FlexibleSpace();
                        //    EditorGUILayout.HelpBox("Aspect Ratio Is Locked For Rectangular Areas", MessageType.Warning);
                        //    GUILayout.FlexibleSpace();
                        //    EditorGUILayout.EndHorizontal();
                        //}

                        GUILayout.Space(50);

                        GUI.backgroundColor = UnityEngine.Color.clear;
                        EditorGUILayout.BeginHorizontal();
                        GUILayout.FlexibleSpace();
                        EditorGUILayout.HelpBox("USE SCALE FACTOR", MessageType.None);
                        GUILayout.FlexibleSpace();
                        EditorGUILayout.EndHorizontal();
                        GUI.backgroundColor = UnityEngine.Color.white;

                        rectToggle = GUILayoutUtility.GetLastRect();
                        rectToggle.x = (rectToggle.width / 2f) + 65f;

                        useScaleFactor = EditorGUI.Toggle(rectToggle, useScaleFactor);

                        GUILayout.Space(10);

                        EditorGUILayout.BeginHorizontal();
                        GUILayout.FlexibleSpace();
                        EditorGUILayout.HelpBox("SCALE FACTOR", MessageType.None);
                        scaleFactor = EditorGUILayout.Slider(scaleFactor, 0.001f, 100);
                        GUILayout.FlexibleSpace();
                        EditorGUILayout.EndHorizontal();

                        GUILayout.Space(20);

                        GUI.backgroundColor = UnityEngine.Color.clear;
                        EditorGUILayout.BeginHorizontal();
                        GUILayout.FlexibleSpace();
                        EditorGUILayout.HelpBox("UNITS TO 1 METER", MessageType.None);
                        GUILayout.FlexibleSpace();
                        EditorGUILayout.EndHorizontal();
                        GUI.backgroundColor = UnityEngine.Color.white;

                        rectToggle = GUILayoutUtility.GetLastRect();
                        rectToggle.x = (rectToggle.width / 2f) + 65f;

                        unitsToOneMeter = EditorGUI.Toggle(rectToggle, unitsToOneMeter);

                        SetWorldSize();

                        GUILayout.Space(20);

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

                        if (terrainSizeNewX == 0 || terrainSizeNewZ == 0)
                            SetUnitsTo1Meter();
                    }
                }

                GUI.backgroundColor = UnityEngine.Color.gray;
                EditorGUILayout.HelpBox("\nAREA SIZE\n", MessageType.None);
                GUI.backgroundColor = UnityEngine.Color.white;

                showAreaSizeSection = EditorGUILayout.Foldout(showAreaSizeSection, "");

                if (showAreaSizeSection)
                {
                    GUILayout.Space(30);

                    EditorGUILayout.BeginHorizontal();
                    GUILayout.FlexibleSpace();
                    EditorGUILayout.HelpBox("AREA SELECTION", MessageType.None, true);
                    GUILayout.FlexibleSpace();
                    EditorGUILayout.EndHorizontal();

                    GUILayout.Space(10);

                    EditorGUILayout.BeginHorizontal();
                    GUILayout.FlexibleSpace();
                    areaSelectionMode = GUILayout.SelectionGrid(areaSelectionMode, selectionMode, 2);
                    GUILayout.FlexibleSpace();
                    EditorGUILayout.EndHorizontal();

                    GUILayout.Space(40);

                    if (areaSelectionMode == 0)
                    {
                        if (coordinatesXMLFile)
                        {
                            EditorGUILayout.BeginHorizontal();
                            GUILayout.FlexibleSpace();
                            EditorGUILayout.HelpBox("\tCOORDINATES FROM FILE ACTIVE\n\nRemove XML Coordinates file to insert arbitrary coordinates\n", MessageType.Warning);
                            GUILayout.FlexibleSpace();
                            EditorGUILayout.EndHorizontal();
                        }
                        else
                            MetricsGUI();
                    }
                    else
                    {
                        if (coordinatesXMLFile)
                        {
                            EditorGUILayout.BeginHorizontal();
                            GUILayout.FlexibleSpace();
                            EditorGUILayout.HelpBox("\tCOORDINATES FROM FILE ACTIVE\n\nRemove XML Coordinates file to insert arbitrary coordinates\n", MessageType.Warning);
                            GUILayout.FlexibleSpace();
                            EditorGUILayout.EndHorizontal();
                        }
                        else
                            CoordinatesGUI();
                    }

                    GUILayout.Space(100);
                }
                else
                    GUILayout.Space(15);

                if (modeIndex == 0)
                {
                    GUI.backgroundColor = UnityEngine.Color.gray;
                    EditorGUILayout.HelpBox("\nOFFLINE DATA\n", MessageType.None);
                    GUI.backgroundColor = UnityEngine.Color.white;

                    showAvailableDataSection = EditorGUILayout.Foldout(showAvailableDataSection, "");

                    if (showAvailableDataSection)
                    {
                        GUILayout.Space(30);

                        EditorGUI.BeginChangeCheck();

                        EditorGUILayout.BeginHorizontal();
                        GUILayout.FlexibleSpace();
                        EditorGUILayout.HelpBox("SINGLE TERRAIN", MessageType.None);
                        terrain = EditorGUILayout.ObjectField(terrain, typeof(Terrain), true) as Terrain;
                        GUILayout.FlexibleSpace();
                        EditorGUILayout.EndHorizontal();

                        if (EditorGUI.EndChangeCheck())
                        {
                            if (terrain)
                                GetInitialTerrainHeights();
                        }

                        GUILayout.Space(10);

                        EditorGUI.BeginChangeCheck();

                        EditorGUILayout.BeginHorizontal();
                        GUILayout.FlexibleSpace();
                        EditorGUILayout.HelpBox("TERRAIN CHUNKS", MessageType.None);
                        splittedTerrains = EditorGUILayout.ObjectField(splittedTerrains, typeof(GameObject), true) as GameObject;
                        GUILayout.FlexibleSpace();
                        EditorGUILayout.EndHorizontal();

                        if (EditorGUI.EndChangeCheck())
                        {
                            if (splittedTerrains)
                            {
                                CheckTerrainChunks();
                                GetInitialTerrainHeights();
                            }
                        }

                        GUILayout.Space(100);
                    }
                    else
                        GUILayout.Space(15);
                }

                if (modeIndex == 0)
                {
                    GUI.backgroundColor = UnityEngine.Color.gray;
                    EditorGUILayout.HelpBox("\nHEIGHTMAP DOWNLOADER\n", MessageType.None, true);
                    GUI.backgroundColor = UnityEngine.Color.white;

                    showHeghtmapDownloaderSection = EditorGUILayout.Foldout(showHeghtmapDownloaderSection, "");

                    if (showHeghtmapDownloaderSection)
                    {
                        GUILayout.Space(30);

                        GUI.backgroundColor = UnityEngine.Color.clear;
                        GUILayout.Button(heightMapLogo);
                        GUI.backgroundColor = UnityEngine.Color.white;

                        GUILayout.Space(40);

                        GUI.backgroundColor = UnityEngine.Color.clear;
                        EditorGUILayout.BeginHorizontal();
                        GUILayout.FlexibleSpace();
                        EditorGUILayout.HelpBox("INCLUDE BATHYMETRY (UNDER WATER)", MessageType.None);
                        GUILayout.FlexibleSpace();
                        EditorGUILayout.EndHorizontal();
                        GUI.backgroundColor = UnityEngine.Color.white;

                        rectToggle = GUILayoutUtility.GetLastRect();
                        rectToggle.x = (rectToggle.width / 2f) + 120f;

                        isTopoBathy = EditorGUI.Toggle(rectToggle, isTopoBathy);

                        GUILayout.Space(30);

                        GUI.backgroundColor = UnityEngine.Color.gray;
                        EditorGUILayout.BeginHorizontal();
                        GUILayout.FlexibleSpace();
                        EditorGUILayout.HelpBox("\nHEIGHTMAP RESOLUTION\n", MessageType.None);
                        GUILayout.FlexibleSpace();
                        EditorGUILayout.EndHorizontal();
                        GUI.backgroundColor = UnityEngine.Color.white;

                        GUILayout.Space(20);

                        EditorGUILayout.BeginHorizontal();
                        GUILayout.FlexibleSpace();
                        if (GUILayout.Button("<<"))
                        {
                            heightmapResolution /= 2;
                        }

                        GUILayout.Space(10);

                        if (GUILayout.Button(">>"))
                        {
                            heightmapResolution *= 2;
                        }
                        GUILayout.FlexibleSpace();
                        EditorGUILayout.EndHorizontal();

                        GUILayout.Space(10);

                        EditorGUILayout.BeginHorizontal();
                        GUILayout.FlexibleSpace();
                        EditorGUILayout.HelpBox("PIXELS", MessageType.None);
                        heightmapResolution = EditorGUILayout.IntSlider(Mathf.ClosestPowerOfTwo(heightmapResolution), 32, 4096);
                        GUILayout.FlexibleSpace();
                        EditorGUILayout.EndHorizontal();

                        GUILayout.Space(10);

                        GUI.color = UnityEngine.Color.green;
                        EditorGUILayout.BeginHorizontal();
                        GUILayout.FlexibleSpace();

                        GUIStyle myStyle = new GUIStyle(GUI.skin.box);
                        myStyle.fontSize = 20;
                        myStyle.normal.textColor = UnityEngine.Color.black;

                        rectToggle = GUILayoutUtility.GetLastRect();
                        rectToggle.x = GUILayoutUtility.GetLastRect().width - 50;
                        rectToggle.width = 100;
                        rectToggle.height = 30;

                        GUI.Box(rectToggle, new GUIContent(heightmapResolution.ToString()), myStyle);

                        GUILayout.FlexibleSpace();
                        EditorGUILayout.EndHorizontal();
                        GUI.color = UnityEngine.Color.white;

                        GUILayout.Space(30);

                        //Check if Terrain resolution is not below 32
                        if ((heightmapResolution / splitSizeNew) < 32)
                        {
                            GUILayout.Space(20);

                            EditorGUILayout.BeginHorizontal();
                            GUILayout.FlexibleSpace();
                            EditorGUILayout.HelpBox("INCREASE HEIGHTMAP RESOLUTION", MessageType.Error);
                            GUILayout.FlexibleSpace();
                            EditorGUILayout.EndHorizontal();
                        }
                        else if (splittedTerrains && terrainResolutionChunk < 32)
                        {
                            GUILayout.Space(20);

                            EditorGUILayout.BeginHorizontal();
                            GUILayout.FlexibleSpace();
                            EditorGUILayout.HelpBox("INCREASE HEIGHTMAP RESOLUTION", MessageType.Error);
                            GUILayout.FlexibleSpace();
                            EditorGUILayout.EndHorizontal();
                        }

                        //Check if Terrain resolution is not above 4096
                        if ((heightmapResolution / splitSizeNew) > 4096)
                        {
                            GUILayout.Space(20);

                            EditorGUILayout.BeginHorizontal();
                            GUILayout.FlexibleSpace();
                            EditorGUILayout.HelpBox("DECREASE HEIGHTMAP RESOLUTION", MessageType.Warning);
                            GUILayout.FlexibleSpace();
                            EditorGUILayout.EndHorizontal();
                        }
                        else if (splittedTerrains && heightmapResolutionSplit > 4096)
                        {
                            GUILayout.Space(20);

                            EditorGUILayout.BeginHorizontal();
                            GUILayout.FlexibleSpace();
                            EditorGUILayout.HelpBox("DECREASE HEIGHTMAP RESOLUTION", MessageType.Warning);
                            GUILayout.FlexibleSpace();
                            EditorGUILayout.EndHorizontal();
                        }
                        else if (terrain && heightmapResolution > 4096)
                        {
                            GUILayout.Space(20);

                            EditorGUILayout.BeginHorizontal();
                            GUILayout.FlexibleSpace();
                            EditorGUILayout.HelpBox("DECREASE HEIGHTMAP RESOLUTION", MessageType.Warning);
                            GUILayout.FlexibleSpace();
                            EditorGUILayout.EndHorizontal();
                        }

                        GUILayout.Space(80);

                        EditorGUILayout.BeginHorizontal();
                        GUILayout.FlexibleSpace();
                        EditorGUILayout.HelpBox("SMOOTH STEPS", MessageType.None);
                        smoothIterations = EditorGUILayout.IntSlider(smoothIterations, 0, 10);
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

                        GUILayout.Space(60);

                        EditorGUILayout.BeginHorizontal();
                        GUILayout.FlexibleSpace();
                        EditorGUILayout.HelpBox("ELEVATION EXAGGERATION", MessageType.None);
                        GUILayout.FlexibleSpace();
                        EditorGUILayout.EndHorizontal();

                        EditorGUILayout.BeginHorizontal();
                        GUILayout.FlexibleSpace();
                        EditorGUILayout.HelpBox("X", MessageType.None);
                        elevationExaggeration = EditorGUILayout.Slider(elevationExaggeration, 0.5f, 40f);
                        GUILayout.FlexibleSpace();
                        EditorGUILayout.EndHorizontal();

                        GUILayout.Space(100);
                    }
                    else
                        GUILayout.Space(15);
                }

                if (modeIndex == 0)
                {
                    GUI.backgroundColor = UnityEngine.Color.gray;
                    EditorGUILayout.HelpBox("\nSAVE ELEVATION DATA\n", MessageType.None, true);
                    GUI.backgroundColor = UnityEngine.Color.white;

                    showSaveElevationSection = EditorGUILayout.Foldout(showSaveElevationSection, "");

                    if (showSaveElevationSection)
                    {
                        GUILayout.Space(30);

                        EditorGUILayout.BeginHorizontal();
                        GUILayout.FlexibleSpace();
                        EditorGUILayout.HelpBox("DATA FORMATS", MessageType.None);
                        GUILayout.FlexibleSpace();
                        EditorGUILayout.EndHorizontal();

                        GUILayout.Space(15);

                        EditorGUILayout.BeginHorizontal();
                        GUILayout.FlexibleSpace();
                        GUI.backgroundColor = UnityEngine.Color.clear;
                        EditorGUILayout.HelpBox("ASCII", MessageType.None);
                        GUI.backgroundColor = UnityEngine.Color.white;
                        rectToggle = GUILayoutUtility.GetLastRect();
                        rectToggle.x = (rectToggle.width / 2f) + 265f;
                        saveTerrainDataASCII = EditorGUI.Toggle(rectToggle, saveTerrainDataASCII);
                        GUILayout.FlexibleSpace();
                        EditorGUILayout.EndHorizontal();

                        GUILayout.Space(10);

                        EditorGUILayout.BeginHorizontal();
                        GUILayout.FlexibleSpace();
                        GUI.backgroundColor = UnityEngine.Color.clear;
                        EditorGUILayout.HelpBox("RAW", MessageType.None);
                        GUI.backgroundColor = UnityEngine.Color.white;
                        rectToggle = GUILayoutUtility.GetLastRect();
                        rectToggle.x = (rectToggle.width / 2f) + 265f;
                        saveTerrainDataRAW = EditorGUI.Toggle(rectToggle, saveTerrainDataRAW);
                        GUILayout.FlexibleSpace();
                        EditorGUILayout.EndHorizontal();

                        GUILayout.Space(10);

                        EditorGUILayout.BeginHorizontal();
                        GUILayout.FlexibleSpace();
                        GUI.backgroundColor = UnityEngine.Color.clear;
                        EditorGUILayout.HelpBox("TIFF", MessageType.None);
                        GUI.backgroundColor = UnityEngine.Color.white;
                        rectToggle = GUILayoutUtility.GetLastRect();
                        rectToggle.x = (rectToggle.width / 2f) + 265f;
                        saveTerrainDataTIFF = EditorGUI.Toggle(rectToggle, saveTerrainDataTIFF);
                        GUILayout.FlexibleSpace();
                        EditorGUILayout.EndHorizontal();

                        if (!saveTerrainDataASCII)
                        {
                            if (saveTerrainDataRAW || saveTerrainDataTIFF)
                            {
                                GUILayout.Space(30);

                                EditorGUILayout.BeginHorizontal();
                                GUILayout.FlexibleSpace();
                                EditorGUILayout.HelpBox("Use ASCII format for Georeferenced data collection between GIS programs", MessageType.Warning);
                                GUILayout.FlexibleSpace();
                                EditorGUILayout.EndHorizontal();
                            }
                        }

                        GUILayout.Space(100);
                    }
                    else
                        GUILayout.Space(15);
                }

                if (modeIndex == 0)
                {
                    GUI.backgroundColor = UnityEngine.Color.gray;
                    EditorGUILayout.HelpBox("\nSATELLITE IMAGE DOWNLOADER\n", MessageType.None);
                    GUI.backgroundColor = UnityEngine.Color.white;

                    showImageDownloaderSection = EditorGUILayout.Foldout(showImageDownloaderSection, "");

                    if (showImageDownloaderSection)
                    {
                        GUILayout.Space(30);

                        GUI.backgroundColor = UnityEngine.Color.clear;
                        GUILayout.Button(landMapLogo);
                        GUI.backgroundColor = UnityEngine.Color.white;

                        GUILayout.Space(40);

                        EditorGUILayout.BeginHorizontal();
                        GUILayout.FlexibleSpace();
                        EditorGUILayout.HelpBox("GRID PER TERRAIN", MessageType.None);
                        gridPerTerrain = EditorGUILayout.IntSlider(gridPerTerrain, 1, 32);
                        GUILayout.FlexibleSpace();
                        EditorGUILayout.EndHorizontal();

                        GUILayout.Space(60);

                        GUI.backgroundColor = UnityEngine.Color.gray;
                        EditorGUILayout.BeginHorizontal();
                        GUILayout.FlexibleSpace();
                        EditorGUILayout.HelpBox("\nIMAGE RESOLUTION\n", MessageType.None);
                        GUILayout.FlexibleSpace();
                        EditorGUILayout.EndHorizontal();
                        GUI.backgroundColor = UnityEngine.Color.white;

                        GUILayout.Space(20);

                        EditorGUILayout.BeginHorizontal();
                        GUILayout.FlexibleSpace();
                        if (GUILayout.Button("<<"))
                        {
                            imageResolution /= 2;
                        }

                        GUILayout.Space(10);

                        if (GUILayout.Button(">>"))
                        {
                            imageResolution *= 2;
                        }
                        GUILayout.FlexibleSpace();
                        EditorGUILayout.EndHorizontal();

                        GUILayout.Space(10);

                        EditorGUILayout.BeginHorizontal();
                        GUILayout.FlexibleSpace();
                        EditorGUILayout.HelpBox("PIXELS", MessageType.None);
                        imageResolution = EditorGUILayout.IntSlider(Mathf.ClosestPowerOfTwo(imageResolution), 32, 4096);
                        GUILayout.FlexibleSpace();
                        EditorGUILayout.EndHorizontal();

                        GUILayout.Space(10);

                        GUI.color = UnityEngine.Color.green;
                        EditorGUILayout.BeginHorizontal();
                        GUILayout.FlexibleSpace();

                        GUIStyle myStyle = new GUIStyle(GUI.skin.box);
                        myStyle.fontSize = 20;
                        myStyle.normal.textColor = UnityEngine.Color.black;

                        rectToggle = GUILayoutUtility.GetLastRect();
                        rectToggle.x = GUILayoutUtility.GetLastRect().width - 50;
                        rectToggle.width = 100;
                        rectToggle.height = 30;

                        GUI.Box(rectToggle, new GUIContent(imageResolution.ToString()), myStyle);

                        GUILayout.FlexibleSpace();
                        EditorGUILayout.EndHorizontal();
                        GUI.color = UnityEngine.Color.white;

                        GUILayout.Space(100);

                        EditorGUILayout.BeginHorizontal();
                        GUILayout.FlexibleSpace();
                        EditorGUILayout.HelpBox("TEXTURE TERRAIN", MessageType.None, true);
                        GUILayout.FlexibleSpace();
                        EditorGUILayout.EndHorizontal();

                        if (textureOnFinish == 0)
                            GUI.backgroundColor = UnityEngine.Color.green;
                        else
                            GUI.backgroundColor = UnityEngine.Color.red;

                        EditorGUILayout.BeginHorizontal();
                        GUILayout.FlexibleSpace();
                        textureOnFinish = GUILayout.SelectionGrid(textureOnFinish, editMode, 2);
                        GUILayout.FlexibleSpace();
                        EditorGUILayout.EndHorizontal();
                        GUI.backgroundColor = UnityEngine.Color.white;

                        if (textureOnFinish == 1)
                        {
                            GUILayout.Space(10);

                            EditorGUILayout.BeginHorizontal();
                            GUILayout.FlexibleSpace();
                            GUI.backgroundColor = UnityEngine.Color.clear;
                            EditorGUILayout.HelpBox("IMPORT AT END", MessageType.None);
                            GUI.backgroundColor = UnityEngine.Color.white;
                            rectToggle = GUILayoutUtility.GetLastRect();
                            rectToggle.x = (rectToggle.width / 2f) + 265f;
                            importAtEnd = EditorGUI.Toggle(rectToggle, importAtEnd);
                            GUILayout.FlexibleSpace();
                            EditorGUILayout.EndHorizontal();
                        }

                        GUILayout.Space(50);

                        extraOptions = EditorGUILayout.Foldout(extraOptions, "OTHER OPTIONS");

                        if (extraOptions)
                        {
                            GUILayout.Space(10);

                            GUI.backgroundColor = UnityEngine.Color.clear;
                            EditorGUILayout.BeginHorizontal();
                            GUILayout.FlexibleSpace();
                            EditorGUILayout.HelpBox("IMAGE QUALITY COMPRESSION", MessageType.None);
                            GUILayout.FlexibleSpace();
                            EditorGUILayout.EndHorizontal();
                            GUI.backgroundColor = UnityEngine.Color.white;

                            GUILayout.Space(5);

                            EditorGUILayout.BeginHorizontal();
                            GUILayout.FlexibleSpace();
                            EditorGUILayout.HelpBox("QUALITY", MessageType.None);
                            compressionQuality = EditorGUILayout.IntSlider(compressionQuality, 0, 100);
                            GUILayout.FlexibleSpace();
                            EditorGUILayout.EndHorizontal();

                            GUILayout.Space(20);

                            GUI.backgroundColor = UnityEngine.Color.clear;
                            EditorGUILayout.BeginHorizontal();
                            GUILayout.FlexibleSpace();
                            EditorGUILayout.HelpBox("IMPORT COMPRESSION", MessageType.None);
                            GUILayout.FlexibleSpace();
                            EditorGUILayout.EndHorizontal();
                            GUI.backgroundColor = UnityEngine.Color.white;

                            rectToggle = GUILayoutUtility.GetLastRect();
                            rectToggle.x = (rectToggle.width / 2f) + 55f;

                            compressionActive = EditorGUI.Toggle(rectToggle, compressionActive);

                            if (compressionActive)
                            {
                                this.ShowNotification(new GUIContent("SLOWER IMPORTING"));

                                frames2++;
                                if (frames2 > 50)
                                    this.RemoveNotification();
                            }
                            else
                                frames2 = 0;

                            GUILayout.Space(5);

                            GUI.backgroundColor = UnityEngine.Color.clear;
                            EditorGUILayout.BeginHorizontal();
                            GUILayout.FlexibleSpace();
                            EditorGUILayout.HelpBox("AUTO IMAGE SCALING", MessageType.None);
                            GUILayout.FlexibleSpace();
                            EditorGUILayout.EndHorizontal();
                            GUI.backgroundColor = UnityEngine.Color.white;

                            rectToggle = GUILayoutUtility.GetLastRect();
                            rectToggle.x = (rectToggle.width / 2f) + 55f;

                            autoScale = EditorGUI.Toggle(rectToggle, autoScale);

                            if (autoScale)
                            {
                                this.ShowNotification(new GUIContent("SLOWER PROCESSING"));

                                frames3++;
                                if (frames3 > 50)
                                    this.RemoveNotification();
                            }
                            else
                                frames3 = 0;

                            if (compressionQuality < 100 || compressionActive || autoScale)
                            {
                                GUILayout.Space(30);

                                EditorGUILayout.BeginHorizontal();
                                GUILayout.FlexibleSpace();
                                EditorGUILayout.HelpBox(imageImportingWarning, MessageType.Warning);
                                GUILayout.FlexibleSpace();
                                EditorGUILayout.EndHorizontal();
                            }

                            GUILayout.Space(50);

                            EditorGUILayout.BeginHorizontal();
                            GUILayout.FlexibleSpace();
                            EditorGUILayout.HelpBox("ANISOTROPIC", MessageType.None);
                            anisotropicFilter = EditorGUILayout.IntSlider(anisotropicFilter, 0, 9);
                            GUILayout.FlexibleSpace();
                            EditorGUILayout.EndHorizontal();

                            GUILayout.Space(30);

                            EditorGUILayout.BeginHorizontal();
                            GUILayout.FlexibleSpace();
                            EditorGUILayout.HelpBox("ALPHAMAP\n PER TERRAIN", MessageType.None);
                            alphamapResolution = EditorGUILayout.IntSlider(Mathf.ClosestPowerOfTwo(alphamapResolution), 16, 2048);
                            GUILayout.FlexibleSpace();
                            EditorGUILayout.EndHorizontal();

                            GUILayout.Space(30);

                            EditorGUILayout.BeginHorizontal();
                            GUILayout.FlexibleSpace();
                            EditorGUILayout.HelpBox(" ASYNC CALLS", MessageType.None, true);
                            maxAsyncCalls = EditorGUILayout.IntSlider(maxAsyncCalls, 2, 50);
                            GUILayout.FlexibleSpace();
                            EditorGUILayout.EndHorizontal();
                        }

                        GUILayout.Space(100);
                    }
                    else
                        GUILayout.Space(15);
                }

                if (modeIndex == 0)
                {
                    GUI.backgroundColor = UnityEngine.Color.gray;
                    EditorGUILayout.HelpBox("\nANALYSIS MAPS DOWNLOADER\n", MessageType.None);
                    GUI.backgroundColor = UnityEngine.Color.white;

                    showVisulizationMapsSection = EditorGUILayout.Foldout(showVisulizationMapsSection, "");

                    if (showVisulizationMapsSection)
                    {
                        GUILayout.Space(30);

                        GUI.backgroundColor = UnityEngine.Color.gray;
                        EditorGUILayout.BeginHorizontal();
                        GUILayout.FlexibleSpace();
                        EditorGUILayout.HelpBox("VISUALIZATION MAP TYPES", MessageType.None);
                        GUILayout.FlexibleSpace();
                        EditorGUILayout.EndHorizontal();
                        GUI.backgroundColor = UnityEngine.Color.white;

                        GUILayout.Space(10);

                        EditorGUILayout.BeginHorizontal();
                        GUILayout.FlexibleSpace();
                        visualMapIndex = GUILayout.SelectionGrid(visualMapIndex, visualMapMode, 4);
                        GUILayout.FlexibleSpace();
                        EditorGUILayout.EndHorizontal();

                        GUILayout.Space(20);

                        GUI.backgroundColor = UnityEngine.Color.clear;

                        EditorGUILayout.BeginHorizontal();
                        GUILayout.FlexibleSpace();

                        if (slopeIsActive)
                            GUI.color = UnityEngine.Color.white;
                        else
                            GUI.color = deactivatedCol;
                        if (GUILayout.Button(slopeIcon)) { }
                        GUI.color = UnityEngine.Color.white;

                        if (aspectIsActive)
                            GUI.color = UnityEngine.Color.white;
                        else
                            GUI.color = deactivatedCol;
                        if (GUILayout.Button(aspectIcon)) { }
                        GUI.color = UnityEngine.Color.white;

                        if (hillshadeMDIsActive)
                            GUI.color = UnityEngine.Color.white;
                        else
                            GUI.color = deactivatedCol;
                        if (GUILayout.Button(hillshadeMDIcon)) { }
                        GUI.color = UnityEngine.Color.white;

                        if (elevationIsActive)
                            GUI.color = UnityEngine.Color.white;
                        else
                            GUI.color = deactivatedCol;
                        if (GUILayout.Button(elevationIcon)) { }
                        GUI.color = UnityEngine.Color.white;

                        GUILayout.FlexibleSpace();
                        EditorGUILayout.EndHorizontal();

                        GUI.backgroundColor = UnityEngine.Color.white;

                        GUILayout.Space(10);

                        EditorGUILayout.BeginHorizontal();
                        GUILayout.FlexibleSpace();
                        GUI.backgroundColor = UnityEngine.Color.clear;
                        EditorGUILayout.HelpBox("SLOPE EXPORT", MessageType.None);
                        GUI.backgroundColor = UnityEngine.Color.white;
                        rectToggle = GUILayoutUtility.GetLastRect();
                        rectToggle.x = (rectToggle.width / 2f) + 268f;
                        slopeIsActive = EditorGUI.Toggle(rectToggle, slopeIsActive);
                        GUILayout.FlexibleSpace();
                        EditorGUILayout.EndHorizontal();

                        EditorGUILayout.BeginHorizontal();
                        GUILayout.FlexibleSpace();
                        GUI.backgroundColor = UnityEngine.Color.clear;
                        EditorGUILayout.HelpBox("ASPECT EXPORT", MessageType.None);
                        GUI.backgroundColor = UnityEngine.Color.white;
                        rectToggle = GUILayoutUtility.GetLastRect();
                        rectToggle.x = (rectToggle.width / 2f) + 265f;
                        aspectIsActive = EditorGUI.Toggle(rectToggle, aspectIsActive);
                        GUILayout.FlexibleSpace();
                        EditorGUILayout.EndHorizontal();

                        EditorGUILayout.BeginHorizontal();
                        GUILayout.FlexibleSpace();
                        GUI.backgroundColor = UnityEngine.Color.clear;
                        EditorGUILayout.HelpBox("HILLSHADE EXPORT", MessageType.None);
                        GUI.backgroundColor = UnityEngine.Color.white;
                        rectToggle = GUILayoutUtility.GetLastRect();
                        rectToggle.x = (rectToggle.width / 2f) + 256f;
                        hillshadeMDIsActive = EditorGUI.Toggle(rectToggle, hillshadeMDIsActive);
                        GUILayout.FlexibleSpace();
                        EditorGUILayout.EndHorizontal();

                        EditorGUILayout.BeginHorizontal();
                        GUILayout.FlexibleSpace();
                        GUI.backgroundColor = UnityEngine.Color.clear;
                        EditorGUILayout.HelpBox("ELEVATION EXPORT", MessageType.None);
                        GUI.backgroundColor = UnityEngine.Color.white;
                        rectToggle = GUILayoutUtility.GetLastRect();
                        rectToggle.x = (rectToggle.width / 2f) + 255f;
                        elevationIsActive = EditorGUI.Toggle(rectToggle, elevationIsActive);
                        GUILayout.FlexibleSpace();
                        EditorGUILayout.EndHorizontal();

                        GUILayout.Space(50);

                        if (visualMapIndex == 0)
                        {
                            EditorGUILayout.BeginHorizontal();
                            GUILayout.FlexibleSpace();
                            EditorGUILayout.HelpBox("OUTPUT POWER", MessageType.None);
                            slopeZFactor = EditorGUILayout.Slider(slopeZFactor, 0.001f, 1.0f);
                            GUILayout.FlexibleSpace();
                            EditorGUILayout.EndHorizontal();
                        }
                        else if (visualMapIndex == 2)
                        {
                            EditorGUILayout.BeginHorizontal();
                            GUILayout.FlexibleSpace();
                            GUI.backgroundColor = UnityEngine.Color.clear;
                            EditorGUILayout.HelpBox("AUTOMATIC SUN SETTINGS", MessageType.None);
                            GUI.backgroundColor = UnityEngine.Color.white;
                            rectToggle = GUILayoutUtility.GetLastRect();
                            rectToggle.x = (rectToggle.width / 2f) + 265f;
                            automaticSunPosition = EditorGUI.Toggle(rectToggle, automaticSunPosition);
                            GUILayout.FlexibleSpace();
                            EditorGUILayout.EndHorizontal();

                            GUILayout.Space(5);

                            EditorGUILayout.BeginHorizontal();
                            GUILayout.FlexibleSpace();
                            EditorGUILayout.HelpBox("SUN ALTITUDE", MessageType.None);
                            hillshadeAltitude = EditorGUILayout.Slider(hillshadeAltitude, 0.0f, 90.0f);
                            GUILayout.FlexibleSpace();
                            EditorGUILayout.EndHorizontal();

                            GUILayout.Space(5);

                            EditorGUILayout.BeginHorizontal();
                            GUILayout.FlexibleSpace();
                            EditorGUILayout.HelpBox("SUN  AZIMUTH", MessageType.None);
                            hillshadeAzimuth = EditorGUILayout.Slider(hillshadeAzimuth, 0.0f, 360.0f);
                            GUILayout.FlexibleSpace();
                            EditorGUILayout.EndHorizontal();

                            GUILayout.Space(30);

                            EditorGUILayout.BeginHorizontal();
                            GUILayout.FlexibleSpace();
                            EditorGUILayout.HelpBox("OUTPUT POWER", MessageType.None);
                            hillshadeZFactor = EditorGUILayout.Slider(hillshadeZFactor, 0.001f, 5.0f);
                            GUILayout.FlexibleSpace();
                            EditorGUILayout.EndHorizontal();
                        }

                        GUILayout.Space(100);

                        EditorGUILayout.BeginHorizontal();
                        GUILayout.FlexibleSpace();
                        if (GUILayout.Button("<<"))
                        {
                            visualMapResolution /= 2;
                        }

                        GUILayout.Space(10);

                        if (GUILayout.Button(">>"))
                        {
                            visualMapResolution *= 2;
                        }
                        GUILayout.FlexibleSpace();
                        EditorGUILayout.EndHorizontal();

                        GUILayout.Space(10);

                        EditorGUILayout.BeginHorizontal();
                        GUILayout.FlexibleSpace();
                        EditorGUILayout.HelpBox("RESOLUTION", MessageType.None);
                        visualMapResolution = EditorGUILayout.IntSlider(Mathf.ClosestPowerOfTwo(visualMapResolution), 32, 8192);
                        GUILayout.FlexibleSpace();
                        EditorGUILayout.EndHorizontal();

                        GUILayout.Space(10);

                        GUI.color = UnityEngine.Color.green;
                        EditorGUILayout.BeginHorizontal();
                        GUILayout.FlexibleSpace();

                        GUIStyle myStyle = new GUIStyle(GUI.skin.box);
                        myStyle.fontSize = 20;
                        myStyle.normal.textColor = UnityEngine.Color.black;

                        rectToggle = GUILayoutUtility.GetLastRect();
                        rectToggle.x = GUILayoutUtility.GetLastRect().width - 50;
                        rectToggle.width = 100;
                        rectToggle.height = 30;

                        GUI.Box(rectToggle, new GUIContent(visualMapResolution.ToString()), myStyle);

                        GUILayout.FlexibleSpace();
                        EditorGUILayout.EndHorizontal();
                        GUI.color = UnityEngine.Color.white;

                        GUILayout.Space(60);

                        EditorGUILayout.BeginHorizontal();
                        GUILayout.FlexibleSpace();
                        EditorGUILayout.HelpBox("FORMAT", MessageType.None);
                        visualFormat = (VisualFormat)EditorGUILayout.EnumPopup(visualFormat);
                        GUILayout.FlexibleSpace();
                        EditorGUILayout.EndHorizontal();

                        GUILayout.Space(40);

                        EditorGUILayout.BeginHorizontal();
                        GUILayout.FlexibleSpace();
                        if (GUILayout.Button("\nGET VISUAL MAPS\n"))
                        {
                            analysisDownloadOnly = true;
                            SetupDownloaderElevation();
                        }
                        GUILayout.FlexibleSpace();
                        EditorGUILayout.EndHorizontal();

                        GUILayout.Space(100);

                        GUI.backgroundColor = UnityEngine.Color.gray;
                        EditorGUILayout.BeginHorizontal();
                        GUILayout.FlexibleSpace();
                        EditorGUILayout.HelpBox("DISPLAY MAP IN SCENE", MessageType.None);
                        GUILayout.FlexibleSpace();
                        EditorGUILayout.EndHorizontal();
                        GUI.backgroundColor = UnityEngine.Color.white;

                        GUILayout.Space(10);

                        EditorGUILayout.BeginHorizontal();
                        GUILayout.FlexibleSpace();
                        EditorGUILayout.HelpBox("ANALYSIS MAPS FOLDER", MessageType.None);

                        EditorGUI.BeginChangeCheck();

                        analysisFolder = EditorGUILayout.ObjectField(analysisFolder, typeof(UnityEngine.Object), true) as UnityEngine.Object;

                        if (EditorGUI.EndChangeCheck())
                        {
                            if (splittedTerrains || terrain)
                            {
                                try
                                {
                                    if (analysisFolder)
                                        analysisFolderAttr = File.GetAttributes(UnityEditor.AssetDatabase.GetAssetPath(analysisFolder));

                                    if (analysisFolder != null && (analysisFolderAttr & FileAttributes.Directory) != FileAttributes.Directory)
                                    {
                                        EditorUtility.DisplayDialog("FOLDER NOT AVAILABLE", "Drag & drop a folder which contains analysis maps or images.", "Ok");
                                        analysisFolder = null;
                                        return;
                                    }

                                    CreateProjector();
                                }
                                catch { }
                            }
                            else
                            {
                                EditorUtility.DisplayDialog("UNAVAILABLE TERRAIN", unavailableTerrainStr, "Ok");
                                analysisFolder = null;
                                return;
                            }
                        }

                        GUILayout.FlexibleSpace();
                        EditorGUILayout.EndHorizontal();

                        GUILayout.Space(30);

                        if ((splittedTerrains || terrain) && analysisFolder)
                        {
                            EditorGUI.BeginChangeCheck();

                            EditorGUILayout.BeginHorizontal();
                            GUILayout.FlexibleSpace();
                            analysisPreviewIndex = GUILayout.SelectionGrid(analysisPreviewIndex, analysisPreviewMode.ToArray(), mapsCount);
                            GUILayout.FlexibleSpace();
                            EditorGUILayout.EndHorizontal();

                            if (EditorGUI.EndChangeCheck())
                                CreateProjector();

                            GUILayout.Space(20);

                            if (!mapPreviewIsActive)
                                previewStr = "\nSHOW PREVIEW\n";
                            else
                                previewStr = "\nHIDE  PREVIEW\n";

                            EditorGUILayout.BeginHorizontal();
                            GUILayout.FlexibleSpace();
                            if (GUILayout.Button(previewStr))
                            {
                                mapPreviewIsActive = !mapPreviewIsActive;
                                CreateProjector();
                            }
                            GUILayout.FlexibleSpace();
                            EditorGUILayout.EndHorizontal();

                            if (mapPreviewIsActive)
                            {
                                GUILayout.Space(40);

                                EditorGUILayout.BeginHorizontal();
                                GUILayout.FlexibleSpace();
                                EditorGUILayout.HelpBox("TINT COLOR", MessageType.None);
                                projectorColor = EditorGUILayout.ColorField(projectorColor);
                                GUILayout.FlexibleSpace();
                                EditorGUILayout.EndHorizontal();

                                GUILayout.Space(10);

                                EditorGUILayout.BeginHorizontal();
                                GUILayout.FlexibleSpace();
                                EditorGUILayout.HelpBox("STRENGTH", MessageType.None);
                                projectorStrength = EditorGUILayout.Slider(projectorStrength, 0.01f, 4f);
                                GUILayout.FlexibleSpace();
                                EditorGUILayout.EndHorizontal();
                            }
                        }

                        if ((!splittedTerrains && !terrain) || !analysisFolder)
                            RemoveProjector();

                        GUILayout.Space(100);
                    }
                    else
                        GUILayout.Space(15);
                }

                if (modeIndex == 0)
                {
                    GUI.backgroundColor = UnityEngine.Color.gray;
                    EditorGUILayout.HelpBox("\nFAILED IMAGES DOWNLOADER\n", MessageType.None);
                    GUI.backgroundColor = UnityEngine.Color.white;

                    showFailedDownloaderSection = EditorGUILayout.Foldout(showFailedDownloaderSection, "");

                    if (showFailedDownloaderSection)
                    {
                        GUILayout.Space(30);

                        EditorGUILayout.BeginHorizontal();
                        GUILayout.FlexibleSpace();
                        EditorGUILayout.HelpBox("FAILED IMAGES FOLDER", MessageType.None);

                        EditorGUI.BeginChangeCheck();

                        failedFolder = EditorGUILayout.ObjectField(failedFolder, typeof(UnityEngine.Object), true) as UnityEngine.Object;

                        if (EditorGUI.EndChangeCheck())
                        {
                            if (failedFolder)
                                CheckFailedImagesGUI();
                        }

                        try
                        {
                            if (failedFolder)
                                attr = File.GetAttributes(UnityEditor.AssetDatabase.GetAssetPath(failedFolder));

                            if (failedFolder != null && (attr & FileAttributes.Directory) != FileAttributes.Directory)
                            {
                                EditorUtility.DisplayDialog("FOLDER NOT AVAILABLE", "Drag & drop a folder which contains failed downloaded satellite images.", "Ok");
                                failedFolder = null;
                                return;
                            }
                        }
                        catch { }

                        GUI.backgroundColor = UnityEngine.Color.clear;
                        if (failedFolder && failedImageAvailable)
                        {
                            int currentSecond = System.DateTime.Now.Second;

                            if (currentSecond % 2 == 0)
                                GUI.color = UnityEngine.Color.clear;
                            else
                                GUI.color = UnityEngine.Color.white;

                            GUILayout.Button(statusRed);
                        }
                        else
                            GUILayout.Button(statusGreen);
                        GUI.backgroundColor = UnityEngine.Color.white;

                        GUI.color = UnityEngine.Color.white;

                        GUILayout.FlexibleSpace();
                        EditorGUILayout.EndHorizontal();

                        GUILayout.Space(10);

                        if (failedFolder)
                        {
                            EditorGUILayout.BeginHorizontal();
                            GUILayout.FlexibleSpace();

                            if (totalFailedImages == 0)
                                EditorGUILayout.HelpBox("NO FAILED IMAGES", MessageType.None);
                            else if (totalFailedImages == 1)
                                EditorGUILayout.HelpBox("1 FAILED IMAGE", MessageType.Warning);
                            else
                                EditorGUILayout.HelpBox(totalFailedImages.ToString() + "  FAILED IMAGES", MessageType.Warning);

                            GUILayout.FlexibleSpace();
                            EditorGUILayout.EndHorizontal();
                        }

                        GUILayout.Space(25);

                        GUI.backgroundColor = UnityEngine.Color.gray;
                        EditorGUILayout.BeginHorizontal();
                        GUILayout.FlexibleSpace();
                        if (GUILayout.Button("\nGET FAILED IMAGES\n"))
                        {
                            DownloadFailedImageTiles(true);
                        }
                        GUILayout.FlexibleSpace();
                        EditorGUILayout.EndHorizontal();
                        GUI.backgroundColor = UnityEngine.Color.white;

                        GUILayout.Space(100);
                    }
                    else
                        GUILayout.Space(15);
                }
                else if (modeIndex == 1)
                {
                    GUI.backgroundColor = UnityEngine.Color.gray;
                    EditorGUILayout.HelpBox("\nFAILED TILES DOWNLOADER\n", MessageType.None);
                    GUI.backgroundColor = UnityEngine.Color.white;

                    showFailedDownloaderSection = EditorGUILayout.Foldout(showFailedDownloaderSection, "");

                    if (showFailedDownloaderSection)
                    {
                        GUILayout.Space(30);

                        GUI.backgroundColor = UnityEngine.Color.gray;
                        EditorGUILayout.BeginHorizontal();
                        GUILayout.FlexibleSpace();
                        if (GUILayout.Button("\nGET FAILED TILES\n"))
                        {
                            serverPath = EditorUtility.OpenFolderPanel("Select the root folder of the server to download failed heightmap & image tiles", projectPath, "TerraLandServer");
                            directoryPathElevation = serverPath + "/Elevation";
                            directoryPathImagery = serverPath + "/Imagery";
                            directoryPathInfo = serverPath + "/Info";

                            CheckFailedTilesElevation();
                            CheckFailedTilesImagery();
                        }
                        GUILayout.FlexibleSpace();
                        EditorGUILayout.EndHorizontal();
                        GUI.backgroundColor = UnityEngine.Color.white;

                        GUILayout.Space(100);
                    }
                    else
                        GUILayout.Space(15);
                }

                GUILayout.Space(40);

                EditorGUILayout.EndScrollView();
                EditorGUILayout.EndVertical();

                GUILayout.Space(15);

                EditorGUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();

                EditorGUILayout.BeginVertical();
                GUILayout.FlexibleSpace();

                GUI.backgroundColor = UnityEngine.Color.gray;
                EditorGUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();

                if (dynamicWorld)
                {
                    if (GUILayout.Button("\nGENERATE HEIGHTS\n"))
                    {
                        serverSetUpElevation = false;
                        serverSetUpImagery = true;

                        SetServerLocation();
                        InitializeDownloader();
                        SetupDownloaderElevation();
                    }
                }
                else
                {
                    if (GUILayout.Button("\nGENERATE HEIGHTS\n"))
                    {
                        InitializeDownloader();
                        CheckHeightmapResolution();
                    }
                }

                GUILayout.FlexibleSpace();
                EditorGUILayout.EndHorizontal();
                GUI.backgroundColor = UnityEngine.Color.white;

                GUILayout.Space(10);

                GUI.color = UnityEngine.Color.green;
                EditorGUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();

                GUIStyle myStyle2 = new GUIStyle(GUI.skin.box);
                myStyle2.fontSize = 15;
                myStyle2.normal.textColor = UnityEngine.Color.black;

                rectToggle = GUILayoutUtility.GetLastRect();
                rectToggle.x = 70;
                rectToggle.width = 100;
                rectToggle.height = 25;

                if (!dynamicWorld)
                    terrainResolutionTotal = heightmapResolution;
                else
                    terrainResolutionTotal = heightmapResolution * gridPerTerrain;

                GUI.Box(rectToggle, new GUIContent(terrainResolutionTotal.ToString() + "  px"), myStyle2);

                GUILayout.FlexibleSpace();
                EditorGUILayout.EndHorizontal();
                GUI.color = UnityEngine.Color.white;

                if (!terrain && (splittedTerrains || splitSizeNew > 1))
                {
                    GUILayout.Space(30);

                    GUI.color = UnityEngine.Color.green;
                    EditorGUILayout.BeginHorizontal();
                    GUILayout.FlexibleSpace();

                    myStyle2 = new GUIStyle(GUI.skin.box);
                    myStyle2.fontSize = 10;
                    myStyle2.normal.textColor = UnityEngine.Color.black;

                    rectToggle = GUILayoutUtility.GetLastRect();
                    rectToggle.x = 70;
                    rectToggle.width = 100;
                    rectToggle.height = 20;

                    if (!dynamicWorld)
                    {
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
                    }
                    else
                        terrainResolutionChunk = heightmapResolution;

                    GUI.Box(rectToggle, new GUIContent(terrainResolutionChunk.ToString() + "  px"), myStyle2);

                    GUILayout.FlexibleSpace();
                    EditorGUILayout.EndHorizontal();
                    GUI.color = UnityEngine.Color.white;
                }

                GUILayout.Space(30);

                GUI.backgroundColor = UnityEngine.Color.gray;
                EditorGUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                if (terrainGenerationstarted && !cancelOperationHeightmapDownloader)
                {
                    if (GUILayout.Button("CANCEL"))
                    {
                        if (EditorUtility.DisplayDialog("CANCELLING DOWNLOAD", "Are you sure you want to cancel downloading?", "No", "Yes"))
                            return;

                        Repaint();
                        terrainGenerationstarted = false;
                        cancelOperationHeightmapDownloader = true;
                        showProgressElevation = false;
                        showProgressGenerateASCII = false;
                        showProgressGenerateRAW = false;
                        showProgressSmoothen = false;
                        showProgressSmoothenOperation = false;
                        convertingElevationTiles = false;
                        stitchingElevationTiles = false;

                        try
                        {
                            Directory.Delete(projectPath + "Temporary Elevation Data", true);
                            Directory.Delete(projectPath + "Temporary Visual Data", true);
                        }
                        catch { }

                        CheckImageDownloaderAndRecompile();
                    }
                }
                GUILayout.FlexibleSpace();
                EditorGUILayout.EndHorizontal();
                GUI.backgroundColor = UnityEngine.Color.white;

                GUILayout.FlexibleSpace();
                EditorGUILayout.EndVertical();

                GUI.backgroundColor = new UnityEngine.Color(0.8f, 0.8f, 0.8f, 0.5f);
                EditorGUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();

                if (dynamicWorld)
                {
                    if (GUILayout.Button(serverButton))
                    {
                        serverSetUpElevation = false;
                        serverSetUpImagery = false;
                        failedDownloading = false;

                        SetServerLocation();
                        SetupImagery();
                        InitializeDownloader();
                        SetupDownloaderElevation();
                        GetSatelliteImages();
                    }
                }
                else
                {
                    if (GUILayout.Button(terrainButton))
                    {
                        failedDownloading = false;
                        CheckHeightmapResolution();
                        SetupImagery();

                        if (cancelOperation)
                            return;

                        InitializeDownloader();
                        GetSatelliteImages();
                    }
                }

                GUILayout.FlexibleSpace();
                EditorGUILayout.EndHorizontal();
                GUI.backgroundColor = UnityEngine.Color.white;

                EditorGUILayout.BeginVertical();
                GUILayout.FlexibleSpace();

                GUI.backgroundColor = UnityEngine.Color.gray;
                EditorGUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();

                if (dynamicWorld)
                {
                    if (GUILayout.Button("\nGENERATE IMAGES\n"))
                    {
                        serverSetUpElevation = true;
                        serverSetUpImagery = false;
                        failedDownloading = false;

                        SetServerLocation();
                        SetupImagery();
                        InitializeDownloader();
                        GetSatelliteImages();
                    }
                }
                else
                {
                    if (GUILayout.Button("\nGENERATE IMAGES\n"))
                    {
                        failedDownloading = false;
                        SetupImagery();

                        if (cancelOperation)
                            return;

                        InitializeDownloader();
                        GetSatelliteImages();
                    }
                }

                GUILayout.FlexibleSpace();
                EditorGUILayout.EndHorizontal();
                GUI.backgroundColor = UnityEngine.Color.white;

                GUILayout.Space(10);

                GUI.color = UnityEngine.Color.green;
                EditorGUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();

                myStyle2 = new GUIStyle(GUI.skin.box);
                myStyle2.fontSize = 15;
                myStyle2.normal.textColor = UnityEngine.Color.black;

                rectToggle = GUILayoutUtility.GetLastRect();
                rectToggle.x = windowWidth - 170;
                rectToggle.width = 100;
                rectToggle.height = 25;

                if (!dynamicWorld)
                {
                    if (terrain)
                        textureResolutionTotal = imageResolution * gridPerTerrain;
                    else if (splittedTerrains)
                        textureResolutionTotal = imageResolution * gridPerTerrain * splitSizeFinal;
                    else
                        textureResolutionTotal = imageResolution * gridPerTerrain * splitSizeNew;
                }
                else
                    textureResolutionTotal = imageResolution * gridPerTerrain;

                GUI.Box(rectToggle, new GUIContent(textureResolutionTotal.ToString() + "  px"), myStyle2);

                GUILayout.FlexibleSpace();
                EditorGUILayout.EndHorizontal();
                GUI.color = UnityEngine.Color.white;

                if (!terrain && (splittedTerrains || splitSizeNew > 1))
                {
                    GUILayout.Space(30);

                    GUI.color = UnityEngine.Color.green;
                    EditorGUILayout.BeginHorizontal();
                    GUILayout.FlexibleSpace();

                    myStyle2 = new GUIStyle(GUI.skin.box);
                    myStyle2.fontSize = 10;
                    myStyle2.normal.textColor = UnityEngine.Color.black;

                    rectToggle = GUILayoutUtility.GetLastRect();
                    rectToggle.x = windowWidth - 170;
                    rectToggle.width = 100;
                    rectToggle.height = 20;

                    if (!dynamicWorld)
                    {
                        if (splittedTerrains)
                            textureResolutionChunk = chunkImageResolution;
                        else
                            textureResolutionChunk = imageResolution * gridPerTerrain;
                    }
                    else
                        textureResolutionChunk = imageResolution;

                    GUI.Box(rectToggle, new GUIContent(textureResolutionChunk.ToString() + "  px"), myStyle2);

                    GUILayout.FlexibleSpace();
                    EditorGUILayout.EndHorizontal();
                    GUI.color = UnityEngine.Color.white;
                }

                GUILayout.Space(30);

                GUI.backgroundColor = UnityEngine.Color.gray;
                EditorGUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                if (imageDownloadingStarted && !cancelOperation)
                {
                    if (GUILayout.Button("CANCEL"))
                    {
                        if (EditorUtility.DisplayDialog("CANCELLING DOWNLOAD", "Are you sure you want to cancel downloading?", "No", "Yes"))
                            return;

                        Repaint();
                        imageDownloadingStarted = false;
                        cancelOperation = true;
                        showProgressImagery = false;

                        if (!failedDownloading)
                        {
                            try
                            {
                                Directory.Delete(projectPath + "Temporary Imagery Data", true);
                            }
                            catch { }
                        }

                        CheckHeightmapDownloaderAndRecompile();
                    }

                    if (GUILayout.Button("FORCE FINISH"))
                    {
                        if (EditorUtility.DisplayDialog("FINISHING DOWNLOAD", "Are you sure you want to Finish downloading?\n\nThere will be empty images instead of non-downloaded images. Drag & drop images folder in \"FAILED IMAGES FOLDER\" in order to download failed downloaded images.", "No", "Yes"))
                            return;

                        FinalizeTerrainImagery();
                    }
                }
                GUILayout.FlexibleSpace();
                EditorGUILayout.EndHorizontal();
                GUI.backgroundColor = UnityEngine.Color.white;

                GUILayout.FlexibleSpace();
                EditorGUILayout.EndVertical();

                GUILayout.FlexibleSpace();
                EditorGUILayout.EndHorizontal();

                GUILayout.Space(15);
            }
        }

        private void SetServerLocation()
        {
            serverPath = EditorUtility.OpenFolderPanel("Select a folder on your computer to create server", projectPath, "TerraLand Server");
        }

        private void CheckFailedTilesElevation()
        {
            // Check for failed Elevation tiles
            if (!string.IsNullOrEmpty(directoryPathElevation))
            {
                CheckFailedHeightmapsGUIServer();

                if (!failedHeightmapAvailable)
                {
                    EditorUtility.DisplayDialog("NO FAILED HEIGHTMAPS", "There are no failed heightmaps in the selected server.\n\nNote: If any of the heightmap tiles has been downloaded incorrectly, you can rename its filename and include \"_Temp\" at the end of the name, then finally press GET FAILED TILES button again to redownload.", "Ok");
                    serverSetUpElevation = true;
                    return;
                }

                failedDownloading = true;
                GetPresetInfo();
                InitializeDownloader();
                SetupDownloaderElevation();
            }
            else
            {
                EditorUtility.DisplayDialog("NO SAVE LOCATION", "Select the root folder of the server to download Failed Imagery", "Ok");
                return;
            }
        }

        private void CheckFailedTilesImagery()
        {
            // Check for failed Image tiles
            if (!string.IsNullOrEmpty(directoryPathImagery))
            {
                CheckFailedImagesGUIServer();

                if (totalFailedImages == 0)
                {
                    EditorUtility.DisplayDialog("NO FAILED IMAGES", "There are no failed images in the selected server.\n\nNote: If any of the image tiles has been downloaded incorrectly, you can rename its filename and include \"_Temp\" at the end of the name, then finally press GET FAILED TILES button again to redownload.", "Ok");
                    serverSetUpImagery = true;
                    return;
                }

                failedDownloading = true;
                GetPresetInfo();
                SetupImagery();

                if (cancelOperation)
                    return;

                InitializeDownloader();
                GetSatelliteImages();
            }
            else
            {
                EditorUtility.DisplayDialog("NO SAVE LOCATION", "Select the root folder of the server to download Failed Imagery", "Ok");
                return;
            }
        }

        private void CreateProjector()
        {
            RemoveProjector();

            if (splittedTerrains)
            {
                CheckTerrainChunks();
                splitSizeFinal = (int)Mathf.Sqrt((float)croppedTerrains.Count);

                chunkSizeX = croppedTerrains[0].terrainData.size.x;
                chunkSizeZ = croppedTerrains[0].terrainData.size.z;
            }
            else if (terrain)
            {
                splitSizeFinal = 1;

                chunkSizeX = terrain.terrainData.size.x;
                chunkSizeZ = terrain.terrainData.size.z;
            }

            if (chunkSizeX == chunkSizeZ)
            {
                squareProjector = true;
                rectangleScaleFactorX = 1f;
                rectangleScaleFactorY = 1f;
            }
            else if (chunkSizeX < chunkSizeZ)
            {
                squareProjector = false;
                rectangleScaleFactorX = chunkSizeZ / chunkSizeX;
                rectangleScaleFactorY = 1f;
                projectorOffsetX = ((chunkSizeZ * splitSizeFinal) / 2f) - ((chunkSizeX * splitSizeFinal) / 2f);
                projectorOffsetZ = 0;
            }
            else if (chunkSizeX > chunkSizeZ)
            {
                squareProjector = false;
                rectangleScaleFactorY = chunkSizeX / chunkSizeZ;
                rectangleScaleFactorX = 1f;
                projectorOffsetX = 0;
                projectorOffsetZ = ((chunkSizeX * splitSizeFinal) / 2f) - ((chunkSizeZ * splitSizeFinal) / 2f);
            }

            IEnumerable<string> names = Directory.GetFiles(UnityEditor.AssetDatabase.GetAssetPath(analysisFolder), "*.*", SearchOption.AllDirectories)
                .Where(s => s.EndsWith(".jpg")
                    || s.EndsWith(".png")
                    || s.EndsWith(".gif")
                    || s.EndsWith(".bmp")
                    || s.EndsWith(".tga")
                    || s.EndsWith(".psd")
                    || s.EndsWith(".tif")
                    || s.EndsWith(".tiff"));

            analysismapNames = names.ToArray();

            mapsCount = analysismapNames.Length;
            analysisPreviewMode = new List<string>();

            if (mapsCount == 0)
            {
                EditorUtility.DisplayDialog("NO AVILABLE IMAGES", "There Are No Images Available In Selected Folder.", "Ok");
                analysisFolder = null;
                return;
            }

            foreach (string name in analysismapNames)
            {
                if (name.Contains("Slope"))
                    analysisPreviewMode.Add("SLOPE");

                if (name.Contains("Aspect"))
                    analysisPreviewMode.Add("ASPECT");

                if (name.Contains("Hillshade"))
                    analysisPreviewMode.Add("HILLSHADE");

                if (name.Contains("Elevation"))
                    analysisPreviewMode.Add("ELEVATION");
            }

            if (mapPreviewIsActive)
            {
                for (int i = 0; i < analysismapNames.Length; i++)
                {
                    if (analysisPreviewIndex == i)
                        analysisMap = AssetDatabase.LoadAssetAtPath(analysismapNames[i], typeof(Texture2D)) as Texture2D;
                    else if (analysisPreviewIndex == i)
                        analysisMap = AssetDatabase.LoadAssetAtPath(analysismapNames[i], typeof(Texture2D)) as Texture2D;
                    else if (analysisPreviewIndex == i)
                        analysisMap = AssetDatabase.LoadAssetAtPath(analysismapNames[i], typeof(Texture2D)) as Texture2D;
                    else if (analysisPreviewIndex == i)
                        analysisMap = AssetDatabase.LoadAssetAtPath(analysismapNames[i], typeof(Texture2D)) as Texture2D;
                }

                projectorObject = new GameObject(projectorName);

                SetProjectorPosition();

                projectorObject.transform.rotation = Quaternion.Euler(90, 0, 0);
                projectorObject.hideFlags = HideFlags.HideInHierarchy;
                projectorObject.AddComponent<Projector>();

                projector = projectorObject.GetComponent<Projector>();
                projector.aspectRatio = 1f;
                projector.orthographic = true;

                if (splittedTerrains)
                {
                    projector.farClipPlane = croppedTerrains[0].transform.position.y + (croppedTerrains[0].terrainData.size.y * 20f);

                    if (chunkSizeX > chunkSizeZ)
                        projector.orthographicSize = ((chunkSizeX * splitSizeFinal) / 2f) - 0.1f;
                    else
                        projector.orthographicSize = ((chunkSizeZ * splitSizeFinal) / 2f) - 0.1f;
                }
                else if (terrain)
                {
                    projector.farClipPlane = terrain.transform.position.y + (terrain.terrainData.size.y * 20f);

                    if (chunkSizeX > chunkSizeZ)
                        projector.orthographicSize = (chunkSizeX / 2f) - 0.1f;
                    else
                        projector.orthographicSize = (chunkSizeZ / 2f) - 0.1f;
                }

                projector.material = Resources.Load("TerraUnity/Projector/ProjectorMat") as Material;

                Texture2D falloff = Resources.Load("TerraUnity/Projector/Falloff") as Texture2D;
                projector.material.SetTexture("_ShadowTex", analysisMap);
                projector.material.SetTexture("_FalloffTex", falloff);
                projector.material.SetColor("_Color", projectorColor);
                projector.material.SetFloat("_Power", projectorStrength);
                projector.material.SetFloat("_ScaleX", rectangleScaleFactorX);
                projector.material.SetFloat("_ScaleY", rectangleScaleFactorY);
            }
            else
                RemoveProjector();
        }

        private void SetProjectorPosition()
        {
            if (splittedTerrains)
            {
                CheckTerrainChunks();
                splitSizeFinal = (int)Mathf.Sqrt((float)croppedTerrains.Count);
                float areaSizeY = (croppedTerrains[0].terrainData.size.y * splitSizeFinal) * 2f;

                projectorPosY = splittedTerrains.transform.position.y + areaSizeY;

                if (squareProjector)
                {
                    projectorPosX = splittedTerrains.transform.position.x;
                    projectorPosZ = splittedTerrains.transform.position.z;
                }
                else
                {
                    projectorPosX = splittedTerrains.transform.position.x + projectorOffsetX;
                    projectorPosZ = splittedTerrains.transform.position.z + projectorOffsetZ;
                }
            }
            else if (terrain)
            {
                projectorPosY = terrain.transform.position.y + (terrain.terrainData.size.y * 2f);

                if (squareProjector)
                {
                    projectorPosX = terrain.transform.position.x + (chunkSizeX / 2f);
                    projectorPosZ = terrain.transform.position.z + (chunkSizeZ / 2f);
                }
                else
                {
                    if (chunkSizeX > chunkSizeZ)
                    {
                        projectorPosX = terrain.transform.position.x + (chunkSizeX / 2f);
                        projectorPosZ = terrain.transform.position.z + (chunkSizeX / 2f);
                    }
                    else
                    {
                        projectorPosX = terrain.transform.position.x + (chunkSizeZ / 2f);
                        projectorPosZ = terrain.transform.position.z + (chunkSizeZ / 2f);
                    }
                }
            }

            projectorObject.transform.position = new Vector3(projectorPosX, projectorPosY, projectorPosZ);
        }

        private void RemoveProjector()
        {
            if (GameObject.Find(projectorName) != null)
                DestroyImmediate(GameObject.Find(projectorName).gameObject);
        }

        private void RemoveSunDummy()
        {
            if (GameObject.Find("Sun Dummy") != null)
                DestroyImmediate(GameObject.Find("Sun Dummy").gameObject);
        }

        private void MetricsGUI()
        {
            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            EditorGUILayout.HelpBox("LAT EXTENTS", MessageType.None, true);
            areaSizeLat = EditorGUILayout.Slider(areaSizeLat, 0.01f, 1000.0f);
            EditorGUILayout.HelpBox("KM", MessageType.None, true);
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();

            GUILayout.Space(10);

            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            EditorGUILayout.HelpBox("LON EXTENTS", MessageType.None, true);
            areaSizeLon = EditorGUILayout.Slider(areaSizeLon, 0.01f, 1000.0f);
            EditorGUILayout.HelpBox("KM", MessageType.None, true);
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();

            GUILayout.Space(15);

            GUI.backgroundColor = UnityEngine.Color.clear;
            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            EditorGUILayout.HelpBox("SQUARE AREA", MessageType.None);
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();
            GUI.backgroundColor = UnityEngine.Color.white;

            rectToggle = GUILayoutUtility.GetLastRect();
            rectToggle.x = (rectToggle.width / 2f) + 65f;

            squareArea = EditorGUI.Toggle(rectToggle, squareArea);

            if (squareArea)
                areaSizeLon = areaSizeLat;
        }

        private void CoordinatesGUI()
        {
            GUI.backgroundColor = UnityEngine.Color.gray;
            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            EditorGUILayout.HelpBox("TOP", MessageType.None, true);
            GUI.backgroundColor = UnityEngine.Color.white;
            top = EditorGUILayout.TextField(top);
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();

            GUILayout.Space(10);

            GUI.backgroundColor = UnityEngine.Color.gray;
            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            EditorGUILayout.HelpBox("LFT", MessageType.None, true);
            GUI.backgroundColor = UnityEngine.Color.white;
            left = EditorGUILayout.TextField(left);

            GUILayout.Space(10);

            GUI.backgroundColor = UnityEngine.Color.gray;
            EditorGUILayout.HelpBox("RGT", MessageType.None, true);
            GUI.backgroundColor = UnityEngine.Color.white;
            right = EditorGUILayout.TextField(right);
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();

            GUILayout.Space(10);

            GUI.backgroundColor = UnityEngine.Color.gray;
            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            EditorGUILayout.HelpBox("BTM", MessageType.None, true);
            GUI.backgroundColor = UnityEngine.Color.white;
            bottom = EditorGUILayout.TextField(bottom);
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();

            latitudeUser = ((double.Parse(top) + double.Parse(bottom)) / 2.0f).ToString();
            longitudeUser = ((double.Parse(left) + double.Parse(right)) / 2.0f).ToString();
        }

        private void SetUnitsTo1Meter()
        {
            terrainSizeNewX = areaSizeLon * 1000f;
            terrainSizeNewZ = areaSizeLat * 1000f;
        }

        private void CheckTerrainSizeUnits()
        {
            terrainSizeFactor = areaSizeLat / areaSizeLon;

            if (splittedTerrains)
            {
                float tsX = 0;
                float tsY = 0;
                bool error = false;

                foreach (Terrain tr in croppedTerrains)
                {
                    if (tr.terrainData == null)
                    {
                        error = true;
                        break;
                    }

                    tsX += tr.terrainData.size.x;
                    tsY += tr.terrainData.size.z;
                }

                if (!error)
                {
                    terrainSizeX = tsX;
                    terrainSizeY = tsY;
                }
            }
            else if (terrain)
            {
                if (terrain.terrainData != null)
                {
                    terrainSizeX = terrain.terrainData.size.x;
                    terrainSizeY = terrain.terrainData.size.z;
                }
            }
        }

        public void CheckHeightmapResolution()
        {
            if (!analysisDownloadOnly)
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
            else
                SetupDownloaderElevation();
        }

        private void SetupDownloaderElevation()
        {
            if (!analysisDownloadOnly)
            {
                if (!Directory.Exists(downloadsPath))
                    Directory.CreateDirectory(downloadsPath);

                try
                {
                    Directory.Delete(projectPath + "Temporary Elevation Data", true);
                    Directory.Delete(projectPath + "Temporary Visual Data", true);
                }
                catch { }

                Directory.CreateDirectory(projectPath + "Temporary Elevation Data");
                Directory.CreateDirectory(projectPath + "Temporary Visual Data");

                convertingElevationTiles = false;
                stitchingElevationTiles = false;
                showProgressElevation = true;
                terrainGenerationstarted = true;
                cancelOperationHeightmapDownloader = false;
                progressBarElevation = 0;
                progressBarVisual = 0;
                progressBarSlope = 0f;
                progressBarAspect = 0f;
                progressBarHillshade = 0f;
                progressBarElevationRaster = 0f;
                progressDATA = 0;
                progressGenerateASCII = 0;
                progressGenerateRAW = 0;
                smoothIterationProgress = 0;
                smoothProgress = 0;
                retries = 0;

                if (!dynamicWorld)
                {
                    if (aspectIsActive || elevationIsActive || hillshadeMDIsActive || slopeIsActive)
                    {
                        showProgressVisual = true;
                        directoryPathVisual = downloadsPath + "/" + downloadDateElevation + "/Visual Maps";

                        if (!Directory.Exists(directoryPathVisual))
                            Directory.CreateDirectory(directoryPathVisual);

                        analysisCount = Convert.ToInt32(slopeIsActive) + Convert.ToInt32(aspectIsActive) + Convert.ToInt32(hillshadeMDIsActive) + Convert.ToInt32(elevationIsActive);
                    }
                }

                if (imageDownloadingStarted)
                    downloadDateElevation = downloadDateImagery;
                else
                    downloadDateElevation = System.DateTime.Now.ToString("MM-dd-yyyy_HH-mm-ss");

                if (!dynamicWorld)
                {
                    if (!terrain && !splittedTerrains)
                        GenerateNewTerrainObject();

                    if (splittedTerrains)
                    {
                        CheckTerrainChunks();

                        initialTerrainWidth = croppedTerrains[0].terrainData.size.x;
                        splitSizeFinal = (int)Mathf.Sqrt((float)croppedTerrains.Count);

#if !UNITY_2_6 && !UNITY_2_6_1 && !UNITY_3_0 && !UNITY_3_0_0 && !UNITY_3_1 && !UNITY_3_2 && !UNITY_3_3 && !UNITY_3_4 && !UNITY_3_5 && !UNITY_4_0 && !UNITY_4_0_1 && !UNITY_4_1 && !UNITY_4_2 && !UNITY_4_3 && !UNITY_4_4 && !UNITY_4_5 && !UNITY_4_6 && !UNITY_4_7
                        RemoveLightmapStatic();
#endif
                    }
                    else if (terrain)
                    {
                        initialTerrainWidth = terrain.terrainData.size.x;
                        terrainChunks = 1;
                        splitSizeFinal = 1;

#if !UNITY_2_6 && !UNITY_2_6_1 && !UNITY_3_0 && !UNITY_3_0_0 && !UNITY_3_1 && !UNITY_3_2 && !UNITY_3_3 && !UNITY_3_4 && !UNITY_3_5 && !UNITY_4_0 && !UNITY_4_0_1 && !UNITY_4_1 && !UNITY_4_2 && !UNITY_4_3 && !UNITY_4_4 && !UNITY_4_5 && !UNITY_4_6 && !UNITY_4_7
                        RemoveLightmapStatic();
#endif
                    }
                }

                if (!dynamicWorld)
                    terrainResolutionDownloading = heightmapResolution + splitSizeFinal;
                else
                    terrainResolutionDownloading = heightmapResolution;

                topCorner = new List<float>();
                bottomCorner = new List<float>();
                leftCorner = new List<float>();
                rightCorner = new List<float>();

                AssetDatabase.Refresh();
            }
            else
            {
                if (!Directory.Exists(downloadsPath))
                    Directory.CreateDirectory(downloadsPath);

                try
                {
                    Directory.Delete(projectPath + "Temporary Visual Data", true);
                }
                catch { }

                Directory.CreateDirectory(projectPath + "Temporary Visual Data");
                AssetDatabase.Refresh();

                progressBarVisual = 0;
                progressBarSlope = 0f;
                progressBarAspect = 0f;
                progressBarHillshade = 0f;
                progressBarElevationRaster = 0f;

                if (aspectIsActive || elevationIsActive || hillshadeMDIsActive || slopeIsActive)
                    showProgressVisual = true;

                if (imageDownloadingStarted)
                    downloadDateElevation = downloadDateImagery;
                else
                    downloadDateElevation = System.DateTime.Now.ToString("MM-dd-yyyy_HH-mm-ss");

                if (aspectIsActive || elevationIsActive || hillshadeMDIsActive || slopeIsActive)
                {
                    directoryPathVisual = downloadsPath + "/" + downloadDateElevation + "/Visual Maps";

                    if (!Directory.Exists(directoryPathVisual))
                        Directory.CreateDirectory(directoryPathVisual);
                }

                analysisCount = Convert.ToInt32(slopeIsActive) + Convert.ToInt32(aspectIsActive) + Convert.ToInt32(hillshadeMDIsActive) + Convert.ToInt32(elevationIsActive);

                AssetDatabase.Refresh();
            }

            InitElevationServerRequest();

            if (!dynamicWorld)
                ServerConnectHeightmap(0, 0);
            else
                GetHeightmaps();
        }

        private void InitElevationServerRequest ()
        {
            mapserviceElevation = new TerraLandWorldElevation.TopoBathy_ImageServer();
            //mapserviceElevation.Timeout = 5000000;
            if (isTopoBathy) elevationURL = "https://elevation.arcgis.com/arcgis/services/WorldElevation/TopoBathy/ImageServer?token=";
            else elevationURL             = "https://elevation.arcgis.com/arcgis/services/WorldElevation/Terrain/ImageServer?token=";
            GenerateToken();
            mapserviceElevation.Url = elevationURL + token;
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
            }
            catch { }
        }

        private void GenerateNewTerrainObject()
        {
            SetData();

            splitDirectoryPath = downloadsPath + "/" + downloadDateElevation + "/Terrain Tiles";
            Directory.CreateDirectory(splitDirectoryPath);
            AssetDatabase.Refresh();

            CreateTerrainData();
            CreateTerrainObject();

            if (splitSizeFinal == 1)
            {
                terrain = terrains[0];
                terrain.transform.position = (Vector3)GetAbsoluteWorldPosition();
            }
            else
            {
                splittedTerrains = terrainsParent;
                splittedTerrains.transform.position = (Vector3)GetAbsoluteWorldPosition();
            }
        }

        private Vector3d GetAbsoluteWorldPosition()
        {
            AreaBounds.MetricsToBBox(double.Parse(latitudeUser), double.Parse(longitudeUser), areaSizeLat, areaSizeLon, out top, out left, out bottom, out right);
            double _yMaxTop = AreaBounds.LatitudeToMercator(double.Parse(top));
            double _xMinLeft = AreaBounds.LongitudeToMercator(double.Parse(left));
            double _yMinBottom = AreaBounds.LatitudeToMercator(double.Parse(bottom));
            double _xMaxRight = AreaBounds.LongitudeToMercator(double.Parse(right));
            double _latSize = Math.Abs(_yMaxTop - _yMinBottom);
            double _lonSize = Math.Abs(_xMinLeft - _xMaxRight);
            double _worldSizeX = terrainSizeNewX * scaleFactor;
            double _worldSizeY = terrainSizeNewZ * scaleFactor;
            double _LAT = AreaBounds.LatitudeToMercator(double.Parse(latitudeUser));
            double _LON = AreaBounds.LongitudeToMercator(double.Parse(longitudeUser));
            double[] _latlonDeltaNormalized = AreaBounds.GetNormalizedDelta(_LAT, _LON, _yMaxTop, _xMinLeft, _latSize, _lonSize);
            Vector2d _initialWorldPositionXZ = AreaBounds.GetWorldPositionFromTile(_latlonDeltaNormalized[0], _latlonDeltaNormalized[1], _worldSizeY, _worldSizeX);
            Vector3d _initialWorldPosition = Vector3d.zero;

            if (splitSizeFinal == 1)
                _initialWorldPosition = new Vector3d(_initialWorldPositionXZ.x, 0, -_initialWorldPositionXZ.y);
            else
                _initialWorldPosition = new Vector3d(_initialWorldPositionXZ.x + _worldSizeY / 2, 0, -_initialWorldPositionXZ.y + _worldSizeX / 2);

            return _initialWorldPosition;
        }

        private void GenerateToken()
        {
            HttpWebRequest req = (HttpWebRequest)WebRequest.Create(tokenURL);
            req.KeepAlive = false;
            req.ProtocolVersion = HttpVersion.Version10;
            req.Method = "POST";
            req.ContentType = "application/x-www-form-urlencoded";
            ServicePointManager.ServerCertificateValidationCallback = new System.Net.Security.RemoteCertificateValidationCallback(delegate { return true; });

            try
            {
                HttpWebResponse response = (HttpWebResponse)req.GetResponse();
                StreamReader sr = new StreamReader(response.GetResponseStream());
                string str = sr.ReadToEnd();
                token = str.Replace("{\"access_token\":\"", "").Replace("\",\"expires_in\":1209600}", "");
            }
            catch (Exception e)
            {
                UnityEngine.Debug.Log(e);

                terrainGenerationstarted = false;
                cancelOperationHeightmapDownloader = true;
                showProgressElevation = false;
                showProgressGenerateASCII = false;
                showProgressGenerateRAW = false;
                showProgressSmoothen = false;
                showProgressSmoothenOperation = false;
                convertingElevationTiles = false;
                stitchingElevationTiles = false;
            }
        }

        private double[] ToWebMercator(double mercatorY_lat, double mercatorX_lon)
        {
            const double earthRadiusEquatorial = 6378137; // 6378137 - 6371010
            const double earthRadiusPolar = 6356752.3142;
            double radiusEarthMeters = earthRadiusPolar + (90 - Math.Abs(mercatorY_lat)) / 90 * (earthRadiusEquatorial - earthRadiusPolar);
            //double radiusEarthMeters = 6378137d;

            //double latOffset = 0.02d;
            float latOffset = Mathf.InverseLerp(-90f, 90f, (float)mercatorY_lat) * 0.05f;
            //UnityEngine.Debug.Log(latOffset);
            latOffset = 0;

            double radiusEarthMetersHalf = radiusEarthMeters / 2d;
            double num = (mercatorY_lat - latOffset) * 0.017453292519943295d;  //0.9966760740043901
            double mercatorLat = radiusEarthMetersHalf * Math.Log((1.0 + Math.Sin(num)) / (1.0 - Math.Sin(num)));
            //double mercatorLat = 3189068.5d * Math.Log((1.0 + Math.Sin(num)) / (1.0 - Math.Sin(num)));

            double num2 = mercatorX_lon * 0.017453292519943295d;
            double mercatorLon = radiusEarthMeters * num2;

            //UnityEngine.Debug.Log(mercatorLat + "   " + mercatorLon);

            return new double[] { mercatorLat, mercatorLon };
        }

        private double[] GeoCoordsFromWebmercator(double x, double y)
        {
            double num3 = x / 6378137.0;
            double num4 = num3 * 57.295779513082323;
            double num5 = Math.Floor((num4 + 180.0) / 360.0);
            double num6 = num4 - (num5 * 360.0);
            double num7 = 1.5707963267948966 - (2.0 * Math.Atan(Math.Exp((-1.0 * y) / 6378137.0)));

            return new double[] { num7 * 57.295779513082323, num6 };
        }

        private static double RadToDeg(double rad)
        {
            double RAD2Deg = 180.0 / Math.PI;
            return rad * RAD2Deg;
        }

        private static double DegToRad(double deg)
        {
            double DEG2RAD = Math.PI / 180.0;
            return deg * DEG2RAD;
        }

        private void TerrainBounds()
        {
            GUI.backgroundColor = UnityEngine.Color.clear;
            GUILayout.Button(landMap);
            GUI.backgroundColor = UnityEngine.Color.white;

            GUILayout.Space(20);

            GUI.backgroundColor = UnityEngine.Color.gray;
            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            EditorGUILayout.HelpBox("TOP", MessageType.None, true);
            GUI.backgroundColor = UnityEngine.Color.white;
            EditorGUILayout.TextField(top);
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();

            GUILayout.Space(10);

            GUI.backgroundColor = UnityEngine.Color.gray;
            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            EditorGUILayout.HelpBox("LFT", MessageType.None, true);
            GUI.backgroundColor = UnityEngine.Color.white;
            EditorGUILayout.TextField(left);

            GUILayout.Space(10);

            GUI.backgroundColor = UnityEngine.Color.gray;
            EditorGUILayout.HelpBox("RGT", MessageType.None, true);
            GUI.backgroundColor = UnityEngine.Color.white;
            EditorGUILayout.TextField(right);
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();

            GUILayout.Space(10);

            GUI.backgroundColor = UnityEngine.Color.gray;
            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            EditorGUILayout.HelpBox("BTM", MessageType.None, true);
            GUI.backgroundColor = UnityEngine.Color.white;
            EditorGUILayout.TextField(bottom);
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();
        }

        private void OptionsManager()
        {
            GUILayout.Space(15);

            if (splittedTerrains)
            {
                GUI.backgroundColor = UnityEngine.Color.red;
                EditorGUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                EditorGUILayout.HelpBox("EDIT TERRAIN HEIGHTS", MessageType.None);
                GUILayout.FlexibleSpace();
                EditorGUILayout.EndHorizontal();
                GUI.backgroundColor = UnityEngine.Color.white;

                GUILayout.Space(10);

                EditorGUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                EditorGUILayout.HelpBox("ELEVATION  EXAGGERATION", MessageType.None);
                terrainHeightMultiplier = EditorGUILayout.Slider(terrainHeightMultiplier, 0.1f, maximumHeightMultiplier);
                GUILayout.FlexibleSpace();
                EditorGUILayout.EndHorizontal();

                CheckTerrainChunks();
                int totalChunksResolution = croppedTerrains[0].terrainData.heightmapResolution * (int)Mathf.Sqrt(terrainChunks);

                if (totalChunksResolution < 2048)
                {
                    if (Event.current.type == EventType.Repaint)
                    {
                        if (GUILayoutUtility.GetLastRect().Contains(Event.current.mousePosition))
                        {
                            if (!heightsAnalyzed)
                                GetInitialTerrainHeights();

                            for (int i = 0; i < croppedTerrains.Count; i++)
                                croppedTerrains[i].terrainData.size = new Vector3
                                (
                                    croppedTerrains[i].terrainData.size.x,
                                    initialHeightsValue[i] * terrainHeightMultiplier,
                                    croppedTerrains[i].terrainData.size.z
                                );
                        }
                    }
                }
                else
                {
                    GUILayout.Space(15);

                    EditorGUILayout.BeginHorizontal();
                    GUILayout.FlexibleSpace();

                    if (GUILayout.Button("SET HEIGHTS"))
                    {
                        if (!heightsAnalyzed)
                            GetInitialTerrainHeights();

                        for (int i = 0; i < croppedTerrains.Count; i++)
                            croppedTerrains[i].terrainData.size = new Vector3
                            (
                                croppedTerrains[i].terrainData.size.x,
                                initialHeightsValue[i] * terrainHeightMultiplier,
                                croppedTerrains[i].terrainData.size.z
                            );
                    }

                    GUILayout.FlexibleSpace();
                    EditorGUILayout.EndHorizontal();
                }
            }
            else if (terrain)
            {
                GUI.backgroundColor = UnityEngine.Color.red;
                EditorGUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                EditorGUILayout.HelpBox("EDIT TERRAIN HEIGHTS", MessageType.None);
                GUILayout.FlexibleSpace();
                EditorGUILayout.EndHorizontal();
                GUI.backgroundColor = UnityEngine.Color.white;

                GUILayout.Space(10);

                EditorGUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                EditorGUILayout.HelpBox("ELEVATION  EXAGGERATION", MessageType.None);
                terrainHeightMultiplier = EditorGUILayout.Slider(terrainHeightMultiplier, 0.1f, maximumHeightMultiplier);
                GUILayout.FlexibleSpace();
                EditorGUILayout.EndHorizontal();

                if (terrain.terrainData.heightmapResolution < 2048)
                {
                    if (Event.current.type == EventType.Repaint)
                    {
                        if (GUILayoutUtility.GetLastRect().Contains(Event.current.mousePosition))
                        {
                            if (!heightsAnalyzed)
                                GetInitialTerrainHeights();

                            terrain.terrainData.size = new Vector3(terrain.terrainData.size.x,
                                                                   initialHeightsValue[0] * terrainHeightMultiplier,
                                                                   terrain.terrainData.size.z
                                                                   );
                        }
                    }
                }
                else
                {
                    GUILayout.Space(15);

                    EditorGUILayout.BeginHorizontal();
                    GUILayout.FlexibleSpace();

                    if (GUILayout.Button("SET HEIGHTS"))
                    {
                        if (!heightsAnalyzed)
                            GetInitialTerrainHeights();

                        terrain.terrainData.size = new Vector3(terrain.terrainData.size.x,
                                                               initialHeightsValue[0] * terrainHeightMultiplier,
                                                               terrain.terrainData.size.z
                                                               );
                    }

                    GUILayout.FlexibleSpace();
                    EditorGUILayout.EndHorizontal();
                }
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

        private void CheckThreadStatusImageDownloader()
        {
            ThreadPool.GetAvailableThreads(out workerThreads, out completionPortThreads);
        }

        private void InitializeDownloader()
        {
            ConnectionsManager.SetAsyncConnections();

            if (dynamicWorld)
            {
                directoryPathElevation = serverPath + "/Elevation";
                directoryPathImagery = serverPath + "/Imagery";
                directoryPathInfo = serverPath + "/Info";

                if (!string.IsNullOrEmpty(directoryPathElevation))
                    Directory.CreateDirectory(directoryPathElevation);
                else
                {
                    EditorUtility.DisplayDialog("NO SAVE LOCATION", "Select a save location to download Elevation", "Ok");
                    return;
                }

                if (!string.IsNullOrEmpty(directoryPathImagery))
                    Directory.CreateDirectory(directoryPathImagery);
                else
                {
                    EditorUtility.DisplayDialog("NO SAVE LOCATION", "Select a save location to download Imagery", "Ok");
                    return;
                }

                if (!string.IsNullOrEmpty(directoryPathInfo))
                    Directory.CreateDirectory(directoryPathInfo);
                else
                {
                    EditorUtility.DisplayDialog("NO SAVE LOCATION", "Select a save location to create info files", "Ok");
                    return;
                }

                //SetCoordinates();
                //GenerateProjFile();
                GenerateXMLFile();
                WritePresetFile(directoryPathInfo + "/Terrain Info.tlps");

                failedTilesAvailable = false;
            }

            downloadedHeightmapIndex = 0;
            downloadedImageIndex = 0;

            tempImageBytes = satelliteImageTemp.EncodeToJPG();

            xMinLeft = double.Parse(left) * 20037508.34 / 180.0;
            yMaxTop = Math.Log(Math.Tan((90.0 + double.Parse(top)) * Math.PI / 360.0)) / (Math.PI / 180.0);
            yMaxTop = yMaxTop * 20037508.34 / 180.0;

            xMaxRight = double.Parse(right) * 20037508.34 / 180.0;
            yMinBottom = Math.Log(Math.Tan((90.0 + double.Parse(bottom)) * Math.PI / 360.0)) / (Math.PI / 180.0);
            yMinBottom = yMinBottom * 20037508.34 / 180.0;

            terrainSizeFactor = areaSizeLat / areaSizeLon;
            latCellSize = Math.Abs(yMaxTop - yMinBottom) / (double)gridNumber;
            lonCellSize = Math.Abs(xMinLeft - xMaxRight) / (double)gridNumber;

            int cellsOnTerrain = 0;

            if (dynamicWorld)
                //cellsOnTerrain = terrainChunks;
                cellsOnTerrain = (int)Mathf.Pow(gridNumber, 2);
            else
                cellsOnTerrain = totalImages;

            if (!failedDownloading)
            {
                xMin = new double[cellsOnTerrain];
                yMin = new double[cellsOnTerrain];
                xMax = new double[cellsOnTerrain];
                yMax = new double[cellsOnTerrain];
            }
            else
            {
                if (dynamicWorld)
                    SetFailedIndicesElevation();

                SetFailedIndicesImagery();
            }

            foreach (Transform t in Resources.FindObjectsOfTypeAll(typeof(Transform)))
            {
                if (t.name.Equals("Image Imports"))
                    DestroyImmediate(t.gameObject);
            }

            imageImportTiles = new GameObject("Image Imports");
            imageImportTiles.hideFlags = HideFlags.HideAndDontSave;

            taskDone = false;
            TerrainGridManager(gridNumber, cellsOnTerrain);
        }

        private void TerrainGridManager(int grid, int cells)
        {
            int index = 0;
            cellSizeX = terrainSizeX / (float)grid;
            cellSizeY = terrainSizeY / (float)grid;

            imageXOffset = new float[cells];
            imageYOffset = new float[cells];

            latCellTop = new double[cells];
            latCellBottom = new double[cells];
            lonCellLeft = new double[cells];
            lonCellRight = new double[cells];

            for (int i = 0; i < grid; i++)
            {
                for (int j = 0; j < grid; j++)
                {
                    imageXOffset[index] = (terrainSizeX - (cellSizeX * ((float)grid - (float)j))) * -1f;
                    imageYOffset[index] = (terrainSizeY - cellSizeY - ((float)cellSizeY * (float)i)) * -1f;

                    latCellTop[index] = yMaxTop - (latCellSize * (double)i);
                    latCellBottom[index] = latCellTop[index] - latCellSize;
                    lonCellLeft[index] = xMinLeft + (lonCellSize * (double)j);
                    lonCellRight[index] = lonCellLeft[index] + lonCellSize;

                    if (!failedDownloading)
                    {
                        xMin[index] = lonCellLeft[index];
                        yMin[index] = latCellBottom[index];
                        xMax[index] = lonCellRight[index];
                        yMax[index] = latCellTop[index];
                    }

                    index++;
                }
            }
        }

        private void SetFailedIndicesElevation()
        {
            failedIndicesElevation = FailedIndicesElevation();

            if (failedIndicesElevation != null && failedIndicesElevation.Count > 0)
            {
                failedIndicesCountElevation = failedIndicesElevation.Count;

                xMinFailedElevation = new double[failedIndicesCountElevation];
                yMinFailedElevation = new double[failedIndicesCountElevation];
                xMaxFailedElevation = new double[failedIndicesCountElevation];
                yMaxFailedElevation = new double[failedIndicesCountElevation];
            }
        }

        private List<int> FailedIndicesElevation()
        {
            List<int> index = new List<int>();
            string[] names = LogicalComparer(directoryPathElevation, ".tif");
            string removeString = tempPattern + ".tif";

            if (names.Length > 0)
            {
                for (int i = 0; i < names.Length; i++)
                {
                    string name = names[i];

                    if (name.Contains(tempPattern))
                    {
                        string str = name.Substring(name.LastIndexOf(@"\") + 1).Replace(removeString, "");
                        int[] result = new Regex(@"\d+").Matches(str).Cast<Match>().Select(m => Int32.Parse(m.Value)).ToArray();

                        int x = result[0];
                        int y = result[1];
                        int ind = ((x - 1) * gridNumber + y) - 1;

                        index.Add(ind);
                    }
                }

                return index;
            }

            return null;
        }

        private void SetFailedIndicesImagery()
        {
            failedIndicesImagery = FailedIndicesImagery();

            if (failedIndicesImagery != null && failedIndicesImagery.Count > 0)
            {
                failedIndicesCountImagery = failedIndicesImagery.Count;

                xMinFailedImagery = new double[failedIndicesCountImagery];
                yMinFailedImagery = new double[failedIndicesCountImagery];
                xMaxFailedImagery = new double[failedIndicesCountImagery];
                yMaxFailedImagery = new double[failedIndicesCountImagery];
            }
        }

        private List<int> FailedIndicesImagery()
        {
            List<int> index = new List<int>();
            string removeString = tempPattern + ".jpg";

            if (!dynamicWorld)
                allImageNames = Directory.GetFiles(AssetDatabase.GetAssetPath(failedFolder), "*.jpg", SearchOption.AllDirectories);
            else
                allImageNames = Directory.GetFiles(directoryPathImagery, "*.jpg", SearchOption.AllDirectories);

            allImageNames = LogicalComparer(allImageNames);

            if (!splittedTerrains)
            {
                if (allImageNames.Length > 0)
                {
                    for (int i = 0; i < allImageNames.Length; i++)
                    {
                        string name = allImageNames[i];

                        if (name.Contains(tempPattern))
                            index.Add(i);
                    }
                    return index;
                }
                return null;
            }
            else
            {
                if (allImageNames.Length > 0)
                {
                    for (int i = 0; i < totalImages; i++)
                    {
                        string name = allImageNames[i];

                        if (name.Contains(tempPattern))
                        {
                            string str = name.Substring(name.LastIndexOf(@"\") + 1).Replace(removeString, "");
                            int[] result = new Regex(@"\d+").Matches(str).Cast<Match>().Select(m => Int32.Parse(m.Value)).ToArray();

                            int x = result[0];
                            int y = result[1];
                            int ind = ((x - 1) * gridNumber + y) - 1;

                            index.Add(ind);
                        }
                    }
                    return index;
                }
                return null;
            }
        }

        private void GetHeightmaps()
        {
            RunAsync(() =>
            {
                ServerInfoElevation();
            });
        }

        private void ServerInfoElevation()
        {
            if (!failedDownloading)
            {
                for (int i = 0; i < terrainChunks; i++)
                {
                    if (cancelOperationHeightmapDownloader)
                    {
                        showProgressElevation = false;
                        return;
                    }

                    xMin[i] = lonCellLeft[i];
                    yMin[i] = latCellBottom[i];
                    xMax[i] = lonCellRight[i];
                    yMax[i] = latCellTop[i];

                    ServerConnectHeightmap(i, i);
                }
            }
            else
            {
                for (int i = 0; i < failedIndicesCountElevation; i++)
                {
                    if (cancelOperationHeightmapDownloader)
                    {
                        showProgressElevation = false;
                        return;
                    }

                    int currentIndex = failedIndicesElevation[i];

                    xMinFailedElevation[i] = lonCellLeft[currentIndex];
                    yMinFailedElevation[i] = latCellBottom[currentIndex];
                    xMaxFailedElevation[i] = lonCellRight[currentIndex];
                    yMaxFailedElevation[i] = latCellTop[currentIndex];

                    ServerConnectHeightmap(i, currentIndex);
                }
            }
        }

        private void ServerConnectHeightmap(int i, int current)
        {
            RunAsync(() =>
            {
                if (!dynamicWorld)
                {
                    EditorApplication.update += VisualProgress;

                    if (!analysisDownloadOnly)
                        ElevationDownload();

                    if (slopeIsActive)
                        VisualmapDownloadSlope();
                    if (aspectIsActive)
                        VisualmapDownloadAspect();
                    if (hillshadeMDIsActive)
                        VisualmapDownloadHillshade();
                    if (elevationIsActive)
                        VisualmapDownloadElevation();
                }
                else
                    ElevationDownload(i, current);

                QueueOnMainThread(() =>
                {
                    if (cancelOperationHeightmapDownloader)
                    {
                        showProgressElevation = false;
                        return;
                    }

                    if (dynamicWorld)
                    {
                        if (!failedDownloading)
                        {
                            if (downloadedHeightmapIndex == terrainChunks)
                                GenerateTerrainHeights();
                        }
                        else
                        {
                            if (downloadedHeightmapIndex == failedIndicesCountElevation)
                                GenerateTerrainHeights();
                        }
                    }
                });
            });
        }

        private void VisualProgress()
        {
            progressBarVisual = progressBarSlope + progressBarAspect + progressBarHillshade + progressBarElevationRaster;
        }

        private void ElevationDownload()
        {
            int finalResolution = heightmapResolution + splitSizeFinal;
            reducedheightmapResolution = heightmapResolution;

            if (retries > 0)
            {
                reducedheightmapResolution = Mathf.Clamp(heightmapResolution / retries, 32, 4096);
                finalResolution = reducedheightmapResolution + splitSizeFinal;

                if (splittedTerrains)
                    heightmapResolutionSplit = reducedheightmapResolution / (int)Mathf.Sqrt((float)terrainChunks);
                else
                    heightmapResolutionSplit = reducedheightmapResolution;
            }

            terrainResolutionDownloading = finalResolution;

            try
            {
                //TerraLandWorldElevation.ImageServiceInfo isInfo = mapserviceElevation.GetServiceInfo();
                ////isInfo.DefaultCompressionQuality = 100;
                ////isInfo.DefaultCompression = "None";
                //isInfo.MaxScaleSpecified = true;
                //isInfo.MaxPixelSize = terrainResolutionDownloading;
                //isInfo.MaxScale = terrainResolutionDownloading;

                //		TerraLandWorldElevation.PointN location = new TerraLandWorldElevation.PointN();
                //		location.X = ToWebMercatorLon(Double.Parse(longitudeUser));
                //		location.Y = ToWebMercatorLon(Double.Parse(latitudeUser));

                //TODO: Check if below lines needed
                //TerraLandWorldElevation.MosaicRule mosaicRule = new TerraLandWorldElevation.MosaicRule();
                //mosaicRule.MosaicMethod = TerraLandWorldElevation.esriMosaicMethod.esriMosaicAttribute;

                //		TerraLandWorldElevation.PointN inputpoint2 = new TerraLandWorldElevation.PointN();
                //		inputpoint2.X = 0.2;
                //		inputpoint2.Y = 0.2;
                //		
                //		TerraLandWorldElevation.ImageServerIdentifyResult identifyresults = mapserviceElevation.Identify(location, mosaicRule, inputpoint2);
                //
                //		double pixelResolution = Double.Parse (identifyresults.CatalogItems.Records [0].Values [5].ToString());
                //		string dataSource = identifyresults.CatalogItems.Records [0].Values [8].ToString ();
                //		
                //		int terrainHeight = (int)((double)((areaSizeLat * 4.0) * 1000.0) / pixelResolution);
                //		int terrainWidth = (int)((double)((areaSizeLon * 4.0) * 1000.0) / pixelResolution);

                //define image description
                TerraLandWorldElevation.GeoImageDescription geoImgDesc = new TerraLandWorldElevation.GeoImageDescription();
                //geoImgDesc.MosaicRule = mosaicRule;
                //geoImgDesc.Height = terrainHeight;
                //geoImgDesc.Width = terrainWidth;
                //geoImgDesc.Height = heightmapResolutionSplit + 1;
                //geoImgDesc.Width = heightmapResolutionSplit + 1;

                //TerraLandWorldElevation.ImageServiceInfo isInfo = mapserviceElevation.GetServiceInfo();
                //UnityEngine.Debug.Log(isInfo.RasterFunctions);

                geoImgDesc.Height = terrainResolutionDownloading;
                geoImgDesc.Width = terrainResolutionDownloading;


                geoImgDesc.Compression = "LZW";
                //geoImgDesc.CompressionQuality = 100;
                //geoImgDesc.Interpolation = TerraLandWorldElevation.rstResamplingTypes.RSP_CubicConvolution;
                //geoImgDesc.NoDataInterpretationSpecified = true;
                //geoImgDesc.NoDataInterpretation = TerraLandWorldElevation.esriNoDataInterpretation.esriNoDataMatchAny;

                //newLat = document.getElementById("inputLat").value;
                //newLng = document.getElementById("inputLng").value;

                //TerraLandWorldElevation.Geometry geom = new TerraLandWorldElevation.Geometry();
                //TerraLandWorldElevation.SpatialReference sr;

                //MapPoint mapPointObjectToConvert = new MapPoint(longitude, latitude, TerraLandWorldElevation.SpatialReference.Wgs84);
                //MapPoint mapPoint = Esri.ArcGISRuntime.Geometry.GeometryEngine.Project(mapPointObjectToConvert, SpatialReferences.WebMercator) as MapPoint;

                //TerraLandWorldElevation.PointN p = new TerraLandWorldElevation.PointN();
                //p.SpatialReference


                //Point pointGeometry = GeometryEngine.project(23.63733, 37.94721, SpatialReference.create(102113));
                //Point pointGeometry = GeometryEngine.project(24.63733, 38.94721, SpatialReference.create(102113));

                //newPoint = TerraLandWorldElevation.Geometry.Point(Double.Parse(top), Double.Parse(left), TerraLandWorldElevation.SpatialReference({ wkid: 4326 }));
                //wmPoint = new esri.geometry.geographicToWebMercator(newPoint);


                TerraLandWorldElevation.EnvelopeN extentElevation = new TerraLandWorldElevation.EnvelopeN();

                //TerraLandWorldElevation.SpatialReference sr;
                //extentElevation.SpatialReference = sr.
                //SpatialReference srIn = SpatialReference.Create(wkidIn);

                //UnityEngine.Debug.Log(extentElevation.SpatialReference);

                //extentElevation.XMin = ToWebMercator(Double.Parse(latitudeUser), Double.Parse(left))[1];
                //extentElevation.YMin = ToWebMercator(Double.Parse(bottom), Double.Parse(longitudeUser))[0];
                //extentElevation.XMax = ToWebMercator(Double.Parse(latitudeUser), Double.Parse(right))[1];
                //extentElevation.YMax = ToWebMercator(Double.Parse(top), Double.Parse(longitudeUser))[0];



                //40075016.6855784

                xMinLeft = double.Parse(left) * 20037508.34 / 180.0;
                yMaxTop = Math.Log(Math.Tan((90.0 + double.Parse(top)) * Math.PI / 360.0)) / (Math.PI / 180.0);
                yMaxTop = yMaxTop * 20037508.34 / 180.0;

                xMaxRight = double.Parse(right) * 20037508.34 / 180.0;
                yMinBottom = Math.Log(Math.Tan((90.0 + double.Parse(bottom)) * Math.PI / 360.0)) / (Math.PI / 180.0);
                yMinBottom = yMinBottom * 20037508.34 / 180.0;

                //https://epsg.io/transform#s_srs=4326&t_srs=3857&x=-121.0909435&y=38.8277527
                //UnityEngine.Debug.Log(xMinLeft + "   "+ yMaxTop);

                extentElevation.XMin = xMinLeft;
                extentElevation.YMin = yMinBottom;
                extentElevation.XMax = xMaxRight;
                extentElevation.YMax = yMaxTop;
                geoImgDesc.Extent = extentElevation;

                //UnityEngine.Debug.Log(extentElevation.SpatialReference);

                TerraLandWorldElevation.ImageType imageType = new TerraLandWorldElevation.ImageType();
                imageType.ImageFormat = TerraLandWorldElevation.esriImageFormat.esriImageTIFF;

                imageType.ImageReturnType = TerraLandWorldElevation.esriImageReturnType.esriImageReturnMimeData;
                //imageType.ImageReturnType = TerraLandWorldElevation.esriImageReturnType.esriImageReturnURL;

                TerraLandWorldElevation.ImageResult result = mapserviceElevation.ExportImage(geoImgDesc, imageType);
                //TerraLandWorldElevation.MapImage result = mapserviceElevation.ExportScaledImage(geoImgDesc, imageType);

                //terrainDataURL = result.ImageURL;
                ////terrainDataURL = mapserviceElevation.Url + "/exportImage?f=image&bbox=-141.19530416221985,-62.217823180545146,139.27427961579508,84.15317625109763&imageSR=4326&bboxSR=4326&size=" + terrainResolutionDownloading + "," + terrainResolutionDownloading;
                //UnityEngine.Debug.Log(terrainDataURL);

                fileNameTerrainData = projectPath + "Temporary Elevation Data/" + "TempElevation.tif";
                File.WriteAllBytes(fileNameTerrainData, result.ImageData);
                GenerateTerrainHeights();

                //DownloadTerrainData(terrainDataURL, fileNameTerrainData);
            }
            catch (Exception e)
            {
                if (retries == 0)
                    retries = 1;
                else if (retries == 1)
                    retries = 2;
                else if (retries == 2)
                    retries = 4;
                else if (retries == 4)
                    retries = 8;
                else if (retries == 8)
                    retries = 16;
                else if (retries == 16)
                    retries = 32;
                else if (retries == 32)
                    retries = 64;
            
                if (retries == 64)
                {
                    UnityEngine.Debug.Log(e);
            
                    terrainGenerationstarted = false;
                    cancelOperationHeightmapDownloader = true;
                    showProgressElevation = false;
                    showProgressGenerateASCII = false;
                    showProgressGenerateRAW = false;
                    showProgressSmoothen = false;
                    showProgressSmoothenOperation = false;
                    convertingElevationTiles = false;
                    stitchingElevationTiles = false;
            
                    return;
                }
                else
                    ServerConnectHeightmap(0, 0);
            }

            //fileNameTerrainData = projectPath + "Temporary Elevation Data/" + "TempElevation.tif";
            //DownloadTerrainData(terrainDataURL, fileNameTerrainData);

            if (cancelOperationHeightmapDownloader)
            {
                showProgressElevation = false;
                return;
            }
        }

        private void ElevationDownload(int i, int current)
        {
            int row = Mathf.CeilToInt((float)(current + 1) / (float)gridNumber);
            int column = (current + 1) - ((row - 1) * gridNumber);
            string imgName = "";

            try
            {
                TerraLandWorldElevation.GeoImageDescription geoImgDesc = new TerraLandWorldElevation.GeoImageDescription();

                geoImgDesc.Height = terrainResolutionDownloading;
                geoImgDesc.Width = terrainResolutionDownloading;

                geoImgDesc.Compression = "LZW";
                geoImgDesc.CompressionQuality = 100;
                geoImgDesc.Interpolation = TerraLandWorldElevation.rstResamplingTypes.RSP_CubicConvolution;
                geoImgDesc.NoDataInterpretationSpecified = true;
                geoImgDesc.NoDataInterpretation = TerraLandWorldElevation.esriNoDataInterpretation.esriNoDataMatchAny;

                TerraLandWorldElevation.EnvelopeN extentElevation = new TerraLandWorldElevation.EnvelopeN();

                if (!failedDownloading)
                {
                    extentElevation.XMin = xMin[i];
                    extentElevation.YMin = yMin[i];
                    extentElevation.XMax = xMax[i];
                    extentElevation.YMax = yMax[i];
                }
                else
                {
                    extentElevation.XMin = xMinFailedElevation[i];
                    extentElevation.YMin = yMinFailedElevation[i];
                    extentElevation.XMax = xMaxFailedElevation[i];
                    extentElevation.YMax = yMaxFailedElevation[i];
                }

                geoImgDesc.Extent = extentElevation;

                TerraLandWorldElevation.ImageType imageType = new TerraLandWorldElevation.ImageType();
                imageType.ImageFormat = TerraLandWorldElevation.esriImageFormat.esriImageTIFF;

                imageType.ImageReturnType = TerraLandWorldElevation.esriImageReturnType.esriImageReturnMimeData;
                //imageType.ImageReturnType = TerraLandWorldElevation.esriImageReturnType.esriImageReturnURL;

                TerraLandWorldElevation.ImageResult result = mapserviceElevation.ExportImage(geoImgDesc, imageType);

                imgName = directoryPathElevation + "/" + row.ToString() + "-" + column.ToString() + ".tif";
                File.WriteAllBytes(imgName, result.ImageData);

                string tempFileName = imgName.Replace(".tif", tempPattern + ".tif");

                if (File.Exists(tempFileName))
                    File.Delete(tempFileName);

                //DownloadTerrainData(result.ImageURL, imgName);
            }
            catch (Exception e)
            {
                imgName = directoryPathElevation + "/" + row.ToString() + "-" + column.ToString() + tempPattern + ".tif";

                if (!File.Exists(imgName))
                {
                    byte[] bytes = new byte[terrainResolutionDownloading * terrainResolutionDownloading];
                    File.WriteAllBytes(imgName, bytes);
                }

                // Following lines will remove tiles if were already available from previous download sessions
                imgName = directoryPathElevation + "/" + row.ToString() + "-" + column.ToString() + ".raw";

                if (File.Exists(imgName))
                    File.Delete(imgName);

                failedTilesAvailable = true;

                UnityEngine.Debug.Log(e);
            }
            finally
            {
                downloadedHeightmapIndex++;

                if (!failedDownloading)
                    progressBarElevation = Mathf.InverseLerp(0, terrainChunks, downloadedHeightmapIndex);
                else
                    progressBarElevation = Mathf.InverseLerp(0, failedIndicesCountElevation, downloadedHeightmapIndex);
            }

            if (cancelOperationHeightmapDownloader)
            {
                showProgressElevation = false;
                return;
            }
        }

        private void VisualmapDownloadSlope()
        {
            TerraLandWorldElevation.GeoImageDescription geoImgDesc = new TerraLandWorldElevation.GeoImageDescription();
            geoImgDesc.Height = visualMapResolution;
            geoImgDesc.Width = visualMapResolution;

            geoImgDesc.Compression = "LZW";
            geoImgDesc.CompressionQuality = 100;
            geoImgDesc.Interpolation = TerraLandWorldElevation.rstResamplingTypes.RSP_CubicConvolution;
            geoImgDesc.NoDataInterpretationSpecified = true;
            geoImgDesc.NoDataInterpretation = TerraLandWorldElevation.esriNoDataInterpretation.esriNoDataMatchAny;

            TerraLandWorldElevation.EnvelopeN extent = new TerraLandWorldElevation.EnvelopeN();
            extent.XMin = ToWebMercator(Double.Parse(latitudeUser), Double.Parse(left))[1];
            extent.YMin = ToWebMercator(Double.Parse(bottom), Double.Parse(longitudeUser))[0];
            extent.XMax = ToWebMercator(Double.Parse(latitudeUser), Double.Parse(right))[1];
            extent.YMax = ToWebMercator(Double.Parse(top), Double.Parse(longitudeUser))[0];
            geoImgDesc.Extent = extent;

            TerraLandWorldElevation.ImageType imageType = new TerraLandWorldElevation.ImageType();
            imageType.ImageReturnType = TerraLandWorldElevation.esriImageReturnType.esriImageReturnURL;

            TerraLandWorldElevation.RenderingRule renderRule = new TerraLandWorldElevation.RenderingRule();
            TerraLandWorldElevation.SlopeFunction function = new TerraLandWorldElevation.SlopeFunction();

            TerraLandWorldElevation.SlopeFunctionArguments argument = new TerraLandWorldElevation.SlopeFunctionArguments();
            argument.Names = new string[] { "ZFactor" };
            argument.Values = new object[] { (double)slopeZFactor };
            renderRule.Arguments = argument;

            renderRule.Function = function;
            renderRule.VariableName = "DEM";
            geoImgDesc.RenderingRule = renderRule;

            SetVisualFormat(imageType);

            try
            {
                TerraLandWorldElevation.ImageResult result = mapserviceElevation.ExportImage(geoImgDesc, imageType);
                visualMapURLSlope = result.ImageURL;
            }
            catch (Exception e)
            {
                UnityEngine.Debug.Log(e);
                //showProgressVisual = false;
            }

            visualExtension = visualMapURLSlope.Substring(visualMapURLSlope.LastIndexOf('.'));
            fileNameSlope = projectPath + "Temporary Visual Data/" + "Slope" + visualExtension;
            DownloadSlope(visualMapURLSlope, fileNameSlope);
        }

        private void VisualmapDownloadAspect()
        {
            TerraLandWorldElevation.GeoImageDescription geoImgDesc = new TerraLandWorldElevation.GeoImageDescription();
            geoImgDesc.Height = visualMapResolution;
            geoImgDesc.Width = visualMapResolution;

            geoImgDesc.Compression = "LZW";
            geoImgDesc.CompressionQuality = 100;
            geoImgDesc.Interpolation = TerraLandWorldElevation.rstResamplingTypes.RSP_CubicConvolution;
            geoImgDesc.NoDataInterpretationSpecified = true;
            geoImgDesc.NoDataInterpretation = TerraLandWorldElevation.esriNoDataInterpretation.esriNoDataMatchAny;

            TerraLandWorldElevation.EnvelopeN extent = new TerraLandWorldElevation.EnvelopeN();
            extent.XMin = ToWebMercator(Double.Parse(latitudeUser), Double.Parse(left))[1];
            extent.YMin = ToWebMercator(Double.Parse(bottom), Double.Parse(longitudeUser))[0];
            extent.XMax = ToWebMercator(Double.Parse(latitudeUser), Double.Parse(right))[1];
            extent.YMax = ToWebMercator(Double.Parse(top), Double.Parse(longitudeUser))[0];
            geoImgDesc.Extent = extent;

            TerraLandWorldElevation.ImageType imageType = new TerraLandWorldElevation.ImageType();
            imageType.ImageReturnType = TerraLandWorldElevation.esriImageReturnType.esriImageReturnURL;

            TerraLandWorldElevation.RenderingRule renderRule = new TerraLandWorldElevation.RenderingRule();
            TerraLandWorldElevation.AspectFunction function = new TerraLandWorldElevation.AspectFunction();

            renderRule.Function = function;
            geoImgDesc.RenderingRule = renderRule;

            SetVisualFormat(imageType);

            try
            {
                TerraLandWorldElevation.ImageResult result = mapserviceElevation.ExportImage(geoImgDesc, imageType);
                visualMapURLAspect = result.ImageURL;
            }
            catch (Exception e)
            {
                UnityEngine.Debug.Log(e);
            }

            visualExtension = visualMapURLAspect.Substring(visualMapURLAspect.LastIndexOf('.'));
            fileNameAspect = projectPath + "Temporary Visual Data/" + "Aspect" + visualExtension;
            DownloadAspect(visualMapURLAspect, fileNameAspect);
        }

        private void VisualmapDownloadHillshade()
        {
            TerraLandWorldElevation.GeoImageDescription geoImgDesc = new TerraLandWorldElevation.GeoImageDescription();
            geoImgDesc.Height = visualMapResolution;
            geoImgDesc.Width = visualMapResolution;

            geoImgDesc.Compression = "LZW";
            geoImgDesc.CompressionQuality = 100;
            geoImgDesc.Interpolation = TerraLandWorldElevation.rstResamplingTypes.RSP_CubicConvolution;
            geoImgDesc.NoDataInterpretationSpecified = true;
            geoImgDesc.NoDataInterpretation = TerraLandWorldElevation.esriNoDataInterpretation.esriNoDataMatchAny;

            TerraLandWorldElevation.EnvelopeN extent = new TerraLandWorldElevation.EnvelopeN();
            extent.XMin = ToWebMercator(Double.Parse(latitudeUser), Double.Parse(left))[1];
            extent.YMin = ToWebMercator(Double.Parse(bottom), Double.Parse(longitudeUser))[0];
            extent.XMax = ToWebMercator(Double.Parse(latitudeUser), Double.Parse(right))[1];
            extent.YMax = ToWebMercator(Double.Parse(top), Double.Parse(longitudeUser))[0];
            geoImgDesc.Extent = extent;

            TerraLandWorldElevation.ImageType imageType = new TerraLandWorldElevation.ImageType();
            imageType.ImageReturnType = TerraLandWorldElevation.esriImageReturnType.esriImageReturnURL;

            TerraLandWorldElevation.RenderingRule renderRule = new TerraLandWorldElevation.RenderingRule();
            TerraLandWorldElevation.HillshadeFunction function = new TerraLandWorldElevation.HillshadeFunction();

            TerraLandWorldElevation.HillshadeFunctionArguments argument = new TerraLandWorldElevation.HillshadeFunctionArguments();
            argument.Names = new string[] { "Altitude", "Azimuth", "ZFactor" };
            argument.Values = new object[] { (double)hillshadeAltitude, (double)hillshadeAzimuth, (double)hillshadeZFactor };
            renderRule.Arguments = argument;

            renderRule.Function = function;
            renderRule.VariableName = "DEM";
            geoImgDesc.RenderingRule = renderRule;

            SetVisualFormat(imageType);

            try
            {
                TerraLandWorldElevation.ImageResult result = mapserviceElevation.ExportImage(geoImgDesc, imageType);
                visualMapURLHillshade = result.ImageURL;
            }
            catch (Exception e)
            {
                UnityEngine.Debug.Log(e);
            }

            visualExtension = visualMapURLHillshade.Substring(visualMapURLHillshade.LastIndexOf('.'));
            fileNameHillshadeMD = projectPath + "Temporary Visual Data/" + "Hillshade" + visualExtension;
            DownloadHillshade(visualMapURLHillshade, fileNameHillshadeMD);
        }

        private void VisualmapDownloadElevation()
        {
            TerraLandWorldElevation.GeoImageDescription geoImgDesc = new TerraLandWorldElevation.GeoImageDescription();
            geoImgDesc.Height = visualMapResolution;
            geoImgDesc.Width = visualMapResolution;

            geoImgDesc.Compression = "LZW";
            geoImgDesc.CompressionQuality = 100;
            geoImgDesc.Interpolation = TerraLandWorldElevation.rstResamplingTypes.RSP_CubicConvolution;
            geoImgDesc.NoDataInterpretationSpecified = true;
            geoImgDesc.NoDataInterpretation = TerraLandWorldElevation.esriNoDataInterpretation.esriNoDataMatchAny;

            TerraLandWorldElevation.EnvelopeN extent = new TerraLandWorldElevation.EnvelopeN();
            extent.XMin = ToWebMercator(Double.Parse(latitudeUser), Double.Parse(left))[1];
            extent.YMin = ToWebMercator(Double.Parse(bottom), Double.Parse(longitudeUser))[0];
            extent.XMax = ToWebMercator(Double.Parse(latitudeUser), Double.Parse(right))[1];
            extent.YMax = ToWebMercator(Double.Parse(top), Double.Parse(longitudeUser))[0];
            geoImgDesc.Extent = extent;

            TerraLandWorldElevation.ImageType imageType = new TerraLandWorldElevation.ImageType();
            imageType.ImageReturnType = TerraLandWorldElevation.esriImageReturnType.esriImageReturnURL;

            SetVisualFormat(imageType);

            try
            {
                TerraLandWorldElevation.ImageResult result = mapserviceElevation.ExportImage(geoImgDesc, imageType);
                visualMapURLElevation = result.ImageURL;
            }
            catch (Exception e)
            {
                UnityEngine.Debug.Log(e);
            }

            visualExtension = visualMapURLElevation.Substring(visualMapURLElevation.LastIndexOf('.'));
            fileNameElevation = projectPath + "Temporary Visual Data/" + "Elevation" + visualExtension;
            DownloadElevationRaster(visualMapURLElevation, fileNameElevation);
        }

        private void DownloadTerrainData(string urlAddress, string path)
        {
            using (webClientTerrain = new WebClient())
            {
                webClientTerrain.DownloadFileCompleted += new AsyncCompletedEventHandler(Completed);
                webClientTerrain.DownloadProgressChanged += new DownloadProgressChangedEventHandler(ProgressChanged);

                Uri URL = urlAddress.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ? new Uri(urlAddress) : new Uri("http://" + urlAddress);
                stopWatchTerrain.Start();

                try
                {
                    webClientTerrain.DownloadFileAsync(URL, path);
                }
                catch (Exception e)
                {
                    UnityEngine.Debug.Log(e);

                    terrainGenerationstarted = false;
                    cancelOperationHeightmapDownloader = true;
                    showProgressElevation = false;
                    showProgressGenerateASCII = false;
                    showProgressGenerateRAW = false;
                    showProgressSmoothen = false;
                    showProgressSmoothenOperation = false;
                    convertingElevationTiles = false;
                    stitchingElevationTiles = false;

                    return;
                }
            }
        }

        private void ProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            downloadSpeedTerrain = string.Format((e.BytesReceived / 1024d / stopWatchTerrain.Elapsed.TotalSeconds).ToString("0.00"));
            progressBarElevation = (float)e.ProgressPercentage / 100f;
            dataReceivedTerrain = string.Format("{0} " + "--- " + "{1} MB", (e.BytesReceived / 1024d / 1024d).ToString("0.00"), (e.TotalBytesToReceive / 1024d / 1024d).ToString("0.00"));
        }

        private void Completed(object sender, AsyncCompletedEventArgs e)
        {
            stopWatchTerrain.Reset();

            if (e.Cancelled == true)
                UnityEngine.Debug.Log(e.Error);
            else
                GenerateTerrainHeights();
        }

        private void DownloadImageryData(string urlAddress, string location)
        {
            using (webClientImagery = new WebClient())
            {
                webClientImagery.DownloadFileCompleted += new AsyncCompletedEventHandler(CompletedImagery);
                Uri URL = urlAddress.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ? new Uri(urlAddress) : new Uri("http://" + urlAddress);

                try
                {
                    webClientImagery.DownloadFileAsync(URL, location);
                }
                catch (Exception e)
                {
                    UnityEngine.Debug.Log(e);

                    cancelOperation = true;
                    showProgressImagery = false;
                    imageDownloadingStarted = false;
                    finishedImporting = true;
                    allThreads = 0;
                    CheckHeightmapDownloaderAndRecompile();

                    return;
                }
            }
        }

        private void CompletedImagery(object sender, AsyncCompletedEventArgs e)
        {
            if (e.Cancelled == true)
                UnityEngine.Debug.Log(e.Error);
        }

        private void DownloadSlope(string urlAddress, string location)
        {
            using (webClientVisual = new WebClient())
            {
                webClientVisual.DownloadProgressChanged += new DownloadProgressChangedEventHandler(ProgressChangedSlope);

                Uri URL = urlAddress.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ? new Uri(urlAddress) : new Uri("http://" + urlAddress);

                try
                {
                    webClientVisual.DownloadFileAsync(URL, location);
                }
                catch (Exception e)
                {
                    UnityEngine.Debug.Log(e);

                    //showProgressVisual = false;
                }
            }
        }

        private void ProgressChangedSlope(object sender, DownloadProgressChangedEventArgs e)
        {
            progressBarSlope = (float)e.ProgressPercentage / (float)(100 * analysisCount);
        }

        private void DownloadAspect(string urlAddress, string location)
        {
            using (webClientVisual = new WebClient())
            {
                webClientVisual.DownloadProgressChanged += new DownloadProgressChangedEventHandler(ProgressChangedAspect);

                Uri URL = urlAddress.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ? new Uri(urlAddress) : new Uri("http://" + urlAddress);

                try
                {
                    webClientVisual.DownloadFileAsync(URL, location);
                }
                catch (Exception e)
                {
                    UnityEngine.Debug.Log(e);

                    //showProgressVisual = false;
                }
            }
        }

        private void ProgressChangedAspect(object sender, DownloadProgressChangedEventArgs e)
        {
            progressBarAspect = (float)e.ProgressPercentage / (float)(100 * analysisCount);
        }

        private void DownloadHillshade(string urlAddress, string location)
        {
            using (webClientVisual = new WebClient())
            {
                webClientVisual.DownloadProgressChanged += new DownloadProgressChangedEventHandler(ProgressChangedHillshade);

                Uri URL = urlAddress.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ? new Uri(urlAddress) : new Uri("http://" + urlAddress);

                try
                {
                    webClientVisual.DownloadFileAsync(URL, location);
                }
                catch (Exception e)
                {
                    UnityEngine.Debug.Log(e);

                    //showProgressVisual = false;
                }
            }
        }

        private void ProgressChangedHillshade(object sender, DownloadProgressChangedEventArgs e)
        {
            progressBarHillshade = (float)e.ProgressPercentage / (float)(100 * analysisCount);
        }

        private void DownloadElevationRaster(string urlAddress, string location)
        {
            using (webClientVisual = new WebClient())
            {
                webClientVisual.DownloadProgressChanged += new DownloadProgressChangedEventHandler(ProgressChangedElevationRaster);

                Uri URL = urlAddress.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ? new Uri(urlAddress) : new Uri("http://" + urlAddress);

                try
                {
                    webClientVisual.DownloadFileAsync(URL, location);
                }
                catch (Exception e)
                {
                    UnityEngine.Debug.Log(e);

                    //showProgressVisual = false;
                }
            }
        }

        private void ProgressChangedElevationRaster(object sender, DownloadProgressChangedEventArgs e)
        {
            progressBarElevationRaster = (float)e.ProgressPercentage / (float)(100 * analysisCount);
        }

        private void SetVisualFormat(TerraLandWorldElevation.ImageType image)
        {
            if (visualFormat.ToString().Equals("AI"))
                image.ImageFormat = TerraLandWorldElevation.esriImageFormat.esriImageAI;
            else if (visualFormat.ToString().Equals("BMP"))
                image.ImageFormat = TerraLandWorldElevation.esriImageFormat.esriImageBMP;
            else if (visualFormat.ToString().Equals("DIB"))
                image.ImageFormat = TerraLandWorldElevation.esriImageFormat.esriImageDIB;
            else if (visualFormat.ToString().Equals("EMF"))
                image.ImageFormat = TerraLandWorldElevation.esriImageFormat.esriImageEMF;
            else if (visualFormat.ToString().Equals("GIF"))
                image.ImageFormat = TerraLandWorldElevation.esriImageFormat.esriImageGIF;
            else if (visualFormat.ToString().Equals("JPG"))
                image.ImageFormat = TerraLandWorldElevation.esriImageFormat.esriImageJPG;
            else if (visualFormat.ToString().Equals("JPGPNG"))
                image.ImageFormat = TerraLandWorldElevation.esriImageFormat.esriImageJPGPNG;
            else if (visualFormat.ToString().Equals("PDF"))
                image.ImageFormat = TerraLandWorldElevation.esriImageFormat.esriImagePDF;
            else if (visualFormat.ToString().Equals("PNG"))
                image.ImageFormat = TerraLandWorldElevation.esriImageFormat.esriImagePNG;
            else if (visualFormat.ToString().Equals("PNG24"))
                image.ImageFormat = TerraLandWorldElevation.esriImageFormat.esriImagePNG24;
            else if (visualFormat.ToString().Equals("PNG32"))
                image.ImageFormat = TerraLandWorldElevation.esriImageFormat.esriImagePNG32;
            else if (visualFormat.ToString().Equals("SVG"))
                image.ImageFormat = TerraLandWorldElevation.esriImageFormat.esriImageSVG;
            else if (visualFormat.ToString().Equals("TIFF"))
                image.ImageFormat = TerraLandWorldElevation.esriImageFormat.esriImageTIFF;
        }

        private void FinalizeAnalysisMaps()
        {
            AssetDatabase.Refresh();

            if (File.Exists(fileNameSlope))
            {
                File.Move(fileNameSlope, directoryPathVisual + "/Slope" + visualExtension);
                File.SetAttributes(directoryPathVisual + "/Slope" + visualExtension, FileAttributes.Normal);
                AssetDatabase.Refresh();
                AnalysisMapImporter(directoryPathVisual + "/Slope" + visualExtension);
            }

            if (File.Exists(fileNameAspect))
            {
                File.Move(fileNameAspect, directoryPathVisual + "/Aspect" + visualExtension);
                File.SetAttributes(directoryPathVisual + "/Aspect" + visualExtension, FileAttributes.Normal);
                AssetDatabase.Refresh();
                AnalysisMapImporter(directoryPathVisual + "/Aspect" + visualExtension);
            }

            if (File.Exists(fileNameHillshadeMD))
            {
                File.Move(fileNameHillshadeMD, directoryPathVisual + "/Hillshade" + visualExtension);
                File.SetAttributes(directoryPathVisual + "/Hillshade" + visualExtension, FileAttributes.Normal);
                AssetDatabase.Refresh();
                AnalysisMapImporter(directoryPathVisual + "/Hillshade" + visualExtension);
            }

            if (File.Exists(fileNameElevation))
            {
                File.Move(fileNameElevation, directoryPathVisual + "/Elevation" + visualExtension);
                File.SetAttributes(directoryPathVisual + "/Elevation" + visualExtension, FileAttributes.Normal);
                AssetDatabase.Refresh();
                AnalysisMapImporter(directoryPathVisual + "/Elevation" + visualExtension);
            }

            analysisFolder = AssetDatabase.LoadMainAssetAtPath(directoryPathVisual.Substring(directoryPathVisual.LastIndexOf("Assets")));

            AssetDatabase.Refresh();

            try
            {
                Directory.Delete(projectPath + "Temporary Visual Data", true);
            }
            catch { }

            progressBarVisual = 0f;
            progressBarSlope = 0f;
            progressBarAspect = 0f;
            progressBarHillshade = 0f;
            progressBarElevationRaster = 0f;
            analysisDownloadOnly = false;
            showProgressVisual = false;

            CreateProjector();
        }

        private void AnalysisMapImporter(string fileName)
        {
            UnityEngine.Object asset = AssetDatabase.LoadAssetAtPath(fileName.Substring(fileName.LastIndexOf("Assets")), typeof(UnityEngine.Object)) as UnityEngine.Object;
            TextureImporter textureImporter = AssetImporter.GetAtPath(AssetDatabase.GetAssetPath(asset)) as TextureImporter;

            if (textureImporter != null)
            {
                textureImporter.alphaSource = TextureImporterAlphaSource.FromGrayScale;
                TextureImporterPlatformSettings platformSettings = new TextureImporterPlatformSettings();
                platformSettings.format = TextureImporterFormat.Alpha8;
                textureImporter.SetPlatformTextureSettings(platformSettings);
                textureImporter.wrapMode = TextureWrapMode.Clamp;
                textureImporter.maxTextureSize = visualMapResolution;

                AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(asset), ImportAssetOptions.ForceUpdate);
                AssetDatabase.Refresh();
            }
        }

        private void GetSunPosition()
        {
            Transform[] gameObjects = FindObjectsOfType(typeof(Transform)) as Transform[];
            List<Transform> detectedSun = new List<Transform>();
            int sunNO = 0;

            foreach (Transform gameObject in gameObjects)
            {
                if (gameObject.GetComponent<Light>() != null && gameObject.GetComponent<Light>().type == LightType.Directional)
                {
                    detectedSun.Add(gameObject);
                    sunNO++;
                }
            }

            if (sunNO == 0)
            {
                EditorUtility.DisplayDialog("DIRECTIONAL LIGHT NOT AVAILABLE", "There are no directional lights in scene to get sun direction.\n\nInsert a directional light in scene and try again.", "Ok");
                automaticSunPosition = false;
                return;
            }
            else if (sunNO == 1)
            {
                // Cartesian to Spherical conversion to get Altitude & Azimuth of the Sun from its direction vector
                sun = detectedSun[0].GetComponent<Light>();

                // Create a Sun Dummy and rotate 180 degrees in Y axis to start the Azimuth orientation from the North
                if (GameObject.Find("Sun Dummy") == null)
                    sunDummy = new GameObject("Sun Dummy");
                else
                {
                    try
                    {
                        sunDummy.hideFlags = HideFlags.HideInHierarchy;
                        sunDummy.transform.position = sun.transform.position;
                        sunDummy.transform.eulerAngles = new Vector3
                            (
                                sun.transform.eulerAngles.x,
                                sun.transform.eulerAngles.y + 180,
                                sun.transform.eulerAngles.z
                            );

                        sunDirection = -sunDummy.gameObject.transform.forward;
                    }
                    catch { }
                }

                if (sunDirection.x == 0)
                    sunDirection.x = Mathf.Epsilon;

                // Radius
                float hillshadeRadius = Mathf.Sqrt
                    (
                        (sunDirection.x * sunDirection.x) +
                        (sunDirection.y * sunDirection.y) +
                        (sunDirection.z * sunDirection.z)
                    );

                // Elevation
                hillshadeAltitude = Mathf.Asin(sunDirection.y / hillshadeRadius) * Mathf.Rad2Deg;

                // Polar
                hillshadeAzimuth = (Mathf.Atan2(sunDirection.x, sunDirection.z) * Mathf.Rad2Deg) + 180;
            }
            else
            {
                UnityEngine.Debug.LogError("There are multiple directional lights in scene.");
            }
        }

        private void GenerateTerrainHeights()
        {
            RunAsync(() =>
            {
                if (!dynamicWorld)
                {
                    showProgressElevation = false;
                    TiffData(fileNameTerrainData);
                }

                QueueOnMainThread(() =>
                {
                    FinalizeTerrainHeights();
                });
            });
        }

        private void FinalizeTerrainHeights()
        {
            RunAsync(() =>
            {
                if (!dynamicWorld)
                {
                    smoothIterationsProgress = smoothIterations;
                    FinalizeSmooth(tiffData, tiffWidth, tiffLength, smoothIterations, smoothBlendIndex, smoothBlend);
                }

                QueueOnMainThread(() =>
                {
                    if (!dynamicWorld)
                    {
                        LoadTerrainHeightsFromTIFF();
                        ManageNeighborings();

                        if (saveTerrainDataASCII || saveTerrainDataTIFF || saveTerrainDataRAW)
                            SetSaveLocation();
                    }

                    OfflineDataSave();
                });
            });
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

        /*
        private void SetCoordinates ()
        {
            // Calculate Easting, Northing & UTM Zone for Arc ASCII Grid file generation and Proj file generation
            nsBaseCmnGIS.cBaseCmnGIS baseGIS = new nsBaseCmnGIS.cBaseCmnGIS();
            string utmBottomLeft = baseGIS.iLatLon2UTM(Double.Parse(bottom), Double.Parse(left), ref UTMNorthing, ref UTMEasting, ref sUtmZone);
            string[] utmValues = utmBottomLeft.Split(',');

            UTMEasting = double.Parse(utmValues[0]);
            UTMNorthing = double.Parse(utmValues[1]);
            sUtmZone = utmValues[2];
        }
        */

        private void SetSaveLocation()
        {
            directoryPathElevation = downloadsPath + "/" + downloadDateElevation + "/Elevation";

            if (!string.IsNullOrEmpty(directoryPathElevation))
                Directory.CreateDirectory(directoryPathElevation);
            else
            {
                EditorUtility.DisplayDialog("NO SAVE LOCATION", "Select a save location to download Elevation", "Ok");
                return;
            }
        }

        private void OfflineDataSave()
        {
            RunAsync(() =>
            {
                if (!dynamicWorld)
                {
                // Create Projection & XML Info file
                if (saveTerrainDataASCII || saveTerrainDataTIFF || saveTerrainDataRAW)
                    {
                    //SetCoordinates();
                    //GenerateProjFile();
                    GenerateXMLFile();
                    }

                    if (saveTerrainDataASCII)
                        SaveTerrainDataASCII();

                    if (saveTerrainDataRAW)
                        SaveTerrainDataRAW();
                }

                QueueOnMainThread(() =>
                {
                    if (!dynamicWorld)
                    {
                        if (saveTerrainDataTIFF)
                            SaveTerrainDataTIFF();
                    }

                    FinalizeTerrainElevation();
                });
            });
        }

        private void FinalizeTerrainElevation()
        {
            showProgressElevation = false;
            showProgressGenerateASCII = false;
            showProgressGenerateRAW = false;
            showProgressSmoothen = false;
            showProgressSmoothenOperation = false;
            terrainGenerationstarted = false;
            heightsAnalyzed = false;
            terrainHeightMultiplier = 1;

            try
            {
                Directory.Delete(projectPath + "Temporary Elevation Data", true);
                Directory.Delete(projectPath + "Temporary Visual Data", true);
            }
            catch { }

            if (!dynamicWorld)
                CheckImageDownloaderAndRecompile();
            else
                GenerateTilesFromHeightmap();

            AssetDatabase.Refresh();
        }

        private void GenerateTilesFromHeightmap()
        {
            convertingElevationTiles = true;
            showProgressElevation = true;

            RunAsync(() =>
            {
                string[] fileNames = LogicalComparer(directoryPathElevation, ".tif");
                string fileName = "";
                int index = 0;

                if (!failedDownloading)
                {
                    for (int x = 1; x <= gridNumber; x++)
                    {
                        for (int y = 1; y <= gridNumber; y++)
                        {
                            fileName = fileNames[index];

                            if (!fileName.Contains(tempPattern))
                                TiffDataFast(fileName, x, y);

                            index++;

                            progressBarElevation = Mathf.InverseLerp(0, terrainChunks, index);
                        }
                    }
                }
                else
                {
                    for (int i = 0; i < fileNames.Length; i++)
                    {
                        fileName = fileNames[i];

                        if (!fileName.Contains(tempPattern))
                            TiffDataFast(fileName);

                        progressBarElevation = Mathf.InverseLerp(0, fileNames.Length - 1, i);
                    }
                }

                QueueOnMainThread(() =>
                {
                    convertingElevationTiles = false;

                    fileNames = LogicalComparer(directoryPathElevation, ".raw");
                    int length = fileNames.Length;

                    if (!Mathf.IsPowerOfTwo(length))
                    {
                        EditorUtility.DisplayDialog("STITCHING OPERATION SKIPPED", "Re-download Failed Tiles in order to Stitch data files", "Ok");

                        showProgressElevation = false;
                        serverSetUpElevation = true;

                        if (serverSetUpElevation && serverSetUpImagery)
                        {
                            if (failedTilesAvailable)
                            {
                                EditorUtility.DisplayDialog("FAILED TILES AVAILABLE", "There are some failed tile downloads for this session.\n\nGo to FAILED TILES DOWNLOADER section and press GET FAILED TILES button to re-download failed tiles.", "Ok");
                                showFailedDownloaderSection = true;
                            }

                            Process.Start(serverPath.Replace(@"/", @"\") + @"\");
                        }
                    }
                    else
                        StitchTiles();
                });
            });
        }

        private void StitchTiles()
        {
            stitchingElevationTiles = true;
            showProgressElevation = true;

            RunAsync(() =>
            {
                string[] fileNames = LogicalComparer(directoryPathElevation, ".raw");
                int length = fileNames.Length;

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

                int resolution = tiffWidth + 1;
                int depth = 2;
                int count = resolution * depth;

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

                        progressBarElevation = Mathf.InverseLerp(0, length, index);
                    }
                }

                QueueOnMainThread(() =>
                {
                    stitchingElevationTiles = false;
                    showProgressElevation = false;
                    serverSetUpElevation = true;

                    if (serverSetUpElevation && serverSetUpImagery)
                    {
                        if (failedTilesAvailable)
                        {
                            EditorUtility.DisplayDialog("FAILED TILES AVAILABLE", "There are some failed tile downloads for this session.\n\nGo to FAILED TILES DOWNLOADER section and press GET FAILED TILES button to re-download failed tiles.", "Ok");
                            showFailedDownloaderSection = true;
                        }

                        Process.Start(serverPath.Replace(@"/", @"\") + @"\");
                    }
                });
            });
        }

        private void RemoveLightmapStatic()
        {
#if UNITY_2019_1_OR_NEWER
            if (splittedTerrains)
            {
                foreach (Terrain t in croppedTerrains)
                {
                    StaticEditorFlags flags = GameObjectUtility.GetStaticEditorFlags(t.gameObject);
                    flags = flags & ~(StaticEditorFlags.ContributeGI);
                    GameObjectUtility.SetStaticEditorFlags(t.gameObject, flags);
                }
            }
            else if (terrain)
            {
                StaticEditorFlags flags = GameObjectUtility.GetStaticEditorFlags(terrain.gameObject);
                flags = flags & ~(StaticEditorFlags.ContributeGI);
                GameObjectUtility.SetStaticEditorFlags(terrain.gameObject, flags);
            }
#else
            if (splittedTerrains)
            {
                foreach (Terrain t in croppedTerrains)
                {
                    StaticEditorFlags flags = GameObjectUtility.GetStaticEditorFlags(t.gameObject);
                    flags = flags & ~(StaticEditorFlags.LightmapStatic);
                    GameObjectUtility.SetStaticEditorFlags(t.gameObject, flags);
                }
            }
            else if (terrain)
            {
                StaticEditorFlags flags = GameObjectUtility.GetStaticEditorFlags(terrain.gameObject);
                flags = flags & ~(StaticEditorFlags.LightmapStatic);
                GameObjectUtility.SetStaticEditorFlags(terrain.gameObject, flags);
            }
#endif
        }

        private void SaveTerrainDataASCII()
        {
            showProgressGenerateASCII = true;

            // Calculating Cell/Pixel Size in meters
            //nsBaseCmnGIS.cBaseCmnGIS baseGISTop = new nsBaseCmnGIS.cBaseCmnGIS();
            //string utmTopLeft = baseGISTop.iLatLon2UTM(Double.Parse(top), Double.Parse(left), ref UTMNorthingTop, ref UTMEastingTop, ref sUtmZoneTop);
            //string[] utmValuesTop = utmTopLeft.Split(',');
            //UTMNorthingTop = double.Parse(utmValuesTop[1]);
            //cellSize = Math.Abs((UTMNorthingTop - UTMNorthing) / (heightmapResolution));

            cellSize = 1;

            StreamWriter sw = new StreamWriter(directoryPathElevation + "/TerraLandWorldElevation.asc");

            sw.WriteLine("ncols         " + (tiffWidth).ToString());
            sw.WriteLine("nrows         " + (tiffLength).ToString());
            sw.WriteLine("xllcorner     " + UTMEasting);
            sw.WriteLine("yllcorner     " + UTMNorthing);
            sw.WriteLine("cellsize      " + cellSize);
            sw.WriteLine("nodata_value  " + "-9999.0");

            RAWElevationData(sw, tiffWidth, tiffLength, tiffDataASCII);

            sw.Close();

            showProgressGenerateASCII = false;
        }

        private void SaveTerrainDataASCII(float[,] cellData, string fileName)
        {
            int resolution = terrainResolutionDownloading + 1;

            // Calculating Cell/Pixel Size in meters
            //nsBaseCmnGIS.cBaseCmnGIS baseGISTop = new nsBaseCmnGIS.cBaseCmnGIS();
            //string utmTopLeft = baseGISTop.iLatLon2UTM(Double.Parse(top), Double.Parse(left), ref UTMNorthingTop, ref UTMEastingTop, ref sUtmZoneTop);
            //string[] utmValuesTop = utmTopLeft.Split(',');
            //UTMNorthingTop = double.Parse(utmValuesTop[1]);

            cellSize = 1;

            StreamWriter sw = new StreamWriter(fileName);

            sw.WriteLine("ncols         " + (resolution).ToString());
            sw.WriteLine("nrows         " + (resolution).ToString());
            sw.WriteLine("xllcorner     " + UTMEasting);
            sw.WriteLine("yllcorner     " + UTMNorthing);
            sw.WriteLine("cellsize      " + cellSize);
            sw.WriteLine("nodata_value  " + "-9999.0");

            RAWElevationData(sw, resolution, resolution, cellData);

            sw.Close();
        }

        private void RAWElevationData(StreamWriter sw, int width, int height, float[,] outputImageData)
        {
            string row = "";

            for (int i = 0; i < width; i++)
            {
                for (int j = 0; j < height; j++)
                {
                    row += outputImageData[i, j] + " ";
                }

                if (i < width - 1)
                    sw.Write(row.Remove(row.Length - 1) + Environment.NewLine);
                else
                    sw.Write(row.Remove(row.Length - 1));

                row = "";
            }
        }

        private void SaveTerrainDataRAW()
        {
            showProgressGenerateRAW = true;

            byte[] array = new byte[(tiffWidth * tiffLength) * 2];
            float num = 65536f;

            for (int i = 0; i < tiffWidth; i++)
            {
                for (int j = 0; j < tiffLength; j++)
                {
                    int num2 = j + i * (tiffWidth);
                    int value = (int)(((tiffDataASCII[i, j] + Mathf.Abs(lowestPoint)) / everestPeak) * num);
                    ushort value2 = (ushort)Mathf.Clamp(value, 0, 65535);
                    byte[] bytes = BitConverter.GetBytes(value2);
                    array[num2 * 2] = bytes[0];
                    array[num2 * 2 + 1] = bytes[1];

                    progressGenerateRAW = Mathf.InverseLerp(0f, (float)tiffWidth, (float)i);
                }
            }

            FileStream fileStream = new FileStream(directoryPathElevation + "/TerraLandWorldElevation.raw", FileMode.Create);
            fileStream.Write(array, 0, array.Length);
            fileStream.Close();
        }

        private void SaveTerrainDataRAW(float[,] cellData, string fileName)
        {
            int resolution = terrainResolutionDownloading + 1;
            byte[] array = new byte[(resolution * resolution) * 2];
            float num = 65536f;

            for (int i = 0; i < resolution; i++)
            {
                for (int j = 0; j < resolution; j++)
                {
                    int num2 = j + i * (resolution);
                    int value = (int)(((cellData[i, j] + Mathf.Abs(lowestPoint)) / everestPeak) * num);
                    ushort value2 = (ushort)Mathf.Clamp(value, 0, 65535);
                    byte[] bytes = BitConverter.GetBytes(value2);
                    array[num2 * 2] = bytes[0];
                    array[num2 * 2 + 1] = bytes[1];
                }
            }

            FileStream fileStream = new FileStream(fileName, FileMode.Create);
            fileStream.Write(array, 0, array.Length);
            fileStream.Close();
        }

        private void SaveTerrainDataTIFF()
        {
            fileNameTerrainDataSaved = directoryPathElevation + "/TerraLandWorldElevation.tif";

            if (File.Exists(fileNameTerrainDataSaved))
            {
                File.SetAttributes(fileNameTerrainDataSaved, FileAttributes.Normal);
                File.Delete(fileNameTerrainDataSaved);
            }

            File.Move(fileNameTerrainData, fileNameTerrainDataSaved);
            File.SetAttributes(fileNameTerrainDataSaved, FileAttributes.Normal);

            AssetDatabase.Refresh();
        }

        /*
        private void GenerateProjFile ()
        {
            string savePath = "";

            if(!dynamicWorld)
                savePath = directoryPathElevation + "/TerraLandWorldElevation.prj";
            else
                savePath = directoryPathInfo + "/TerraLandWorldElevation.prj";

            nsBaseCmnGIS.cBaseCmnGIS gis = new nsBaseCmnGIS.cBaseCmnGIS();
            string sGeoGCS = "GCS_WGS_1984",
            sUnit = "UNIT[\"Degree\",0.017453292519943295]", // ie, hard code: dCvtDeg2Rad.ToString()
            sEquatorialRadius = gis.dEquatorialRadius.ToString(), // ie, 6378137.0
            sDenominatorOfFlatteningRatio = gis.dDenominatorOfFlatteningRatio.ToString(), // ie, 298.257223563
            sSpheroid = "SPHEROID[\"WGS_1984\"," + sEquatorialRadius + "," + sDenominatorOfFlatteningRatio + "]";
            int iZoneNumber = Convert.ToInt32(sUtmZone.Substring(0, sUtmZone.Length - 1));
            double dCentralMeridian = gis.dSet_CentralMeridian_from_UtmZone(iZoneNumber);
            sCentralMeridian = dCentralMeridian.ToString("0.0");

            projectionStr = "PROJCS[\"WGS_1984_UTM_Zone_" + sUtmZone + "\",GEOGCS[\"" + sGeoGCS + "\"," +
                "DATUM[\"D_WGS_1984\"," + sSpheroid + "]," +
                    "PRIMEM[\"Greenwich\",0.0]," + sUnit + "]," + // ends the PROJCS[xxxx]]
                    "PROJECTION[\"Transverse_Mercator\"]," +
                    "PARAMETER[\"False_Easting\",500000.0]," +
                    "PARAMETER[\"False_Northing\",0.0]," +
                    "PARAMETER[\"Central_Meridian\"," + sCentralMeridian + "]," +
                    "PARAMETER[\"Scale_Factor\",0.9996]," +
                    "PARAMETER[\"Latitude_Of_Origin\",0.0]," +
                    "UNIT[\"Meter\",1.0]]";

            nsBaseFio.BaseFio bFio = new nsBaseFio.BaseFio();
            bFio.iWriteStringToFile_ASCII(savePath, projectionStr);
        }
        */

        private void GenerateXMLFile()
        {
            string savePath = "";

            if (!dynamicWorld)
                savePath = directoryPathElevation + "/TerraLandWorldElevation.xml";
            else
                savePath = directoryPathInfo + "/TerraLandWorldElevation.xml";

            new XDocument(
                new XElement("Coordinates",
                    new XElement("Latitude", latitudeUser),
                    new XElement("Longitude", longitudeUser),
                    new XElement("Top", top),
                    new XElement("Left", left),
                    new XElement("Bottom", bottom),
                    new XElement("Right", right),
                    new XElement("LatExtents", areaSizeLat.ToString()),
                    new XElement("LonExtents", areaSizeLon.ToString())
                )
            )
            .Save(savePath);
        }

        //private Vector3 RealTerrainSize (float width, float length, float height)
        //{
        //    //float realTerrainSizeX = initialTerrainWidth;
        //    //float realTerrainSizeZ = realTerrainSizeX * terrainSizeFactor;
        //
        //    float realTerrainSizeX = terrainSizeNewX / size;
        //    float realTerrainSizeZ = terrainSizeNewZ / size;
        //
        //    //float realToUnitsY = realTerrainSizeX * ((height * terrainEverestDiffer) / width);
        //    //float realTerrainSizeY = realToUnitsY * elevationExaggeration;
        //
        //    //if(realTerrainSizeY <= 0f ||  float.IsNaN(realTerrainSizeY) || float.IsInfinity(realTerrainSizeY) || float.IsPositiveInfinity(realTerrainSizeY) || float.IsNegativeInfinity(realTerrainSizeY))
        //    //realTerrainSizeY = 0.001f;
        //
        //    Vector3 finalTerrainSize = new Vector3(realTerrainSizeX, everestPeak * elevationExaggeration, realTerrainSizeZ);
        //
        //    return finalTerrainSize;
        //}

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
                        tiffDataSplitted = new float[heightmapResFinalX, heightmapResFinalY];

                        int xStart = (currentRow * (heightmapResFinalX - 1));
                        int yStart = (j * (heightmapResFinalY - 1));

                        for (int x = 0; x < xLength; x++)
                            for (int y = 0; y < yLength; y++)
                                tiffDataSplitted[x, y] = finalHeights[xStart + x, yStart + y];

                        croppedTerrains[counter].terrainData.SetHeights(0, 0, tiffDataSplitted);

                        float realTerrainWidth = areaSizeLon * 1000.0f / splitSizeFinal;
                        float realTerrainLength = areaSizeLat * 1000.0f / splitSizeFinal;
                        croppedTerrains[counter].terrainData.size = new Vector3(realTerrainWidth, everestPeak * elevationExaggeration, realTerrainLength);
                        //croppedTerrains[counter].terrainData.size = new Vector3(tileWidth, everestPeak * elevationExaggeration, tileLength);

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
                float realTerrainWidth = areaSizeLon * 1000.0f;
                float realTerrainLength = areaSizeLat * 1000.0f;
                terrain.terrainData.size = new Vector3(realTerrainWidth, everestPeak * elevationExaggeration, realTerrainLength);

                terrain.Flush();
            }

            EditorUtility.ClearProgressBar();
            AssetDatabase.Refresh();
        }

        private void TiffDataFast(string fileName, int row, int column)
        {
            int resolution = terrainResolutionDownloading + 1;

            try
            {
                using (Tiff inputImage = Tiff.Open(fileName, "r"))
                {
                    tiffWidth = inputImage.GetField(TiffTag.IMAGEWIDTH)[0].ToInt();
                    tiffLength = inputImage.GetField(TiffTag.IMAGELENGTH)[0].ToInt();
                    tiffData = new float[resolution, resolution];

                    int tileHeight = inputImage.GetField(TiffTag.TILELENGTH)[0].ToInt();
                    int tileWidth = inputImage.GetField(TiffTag.TILEWIDTH)[0].ToInt();

                    byte[] buffer = new byte[tileHeight * tileWidth * 4];
                    float[,] fBuffer = new float[tileHeight, tileWidth];

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

            // Add Bottom Row (PO2 + 1 Resolution)
            for (int i = 0; i < resolution; i++)
                tiffData[i, resolution - 1] = tiffData[i, resolution - 2];

            // Add Right Column (PO2 + 1 Resolution)
            for (int i = 0; i < resolution; i++)
                tiffData[resolution - 1, i] = tiffData[resolution - 2, i];

            if (smoothIterations > 0)
                tiffData = SmoothedHeightsFast(tiffData, resolution, resolution, smoothIterations);

            if (formatIndex == 0)
            {
                string fileNameRaw = directoryPathElevation + "/" + row + "-" + column + ".raw";
                SaveTerrainDataRAW(tiffData, fileNameRaw);
            }
            else if (formatIndex == 1)
            {
                string fileNameAsc = directoryPathElevation + "/" + row + "-" + column + ".asc";
                SaveTerrainDataASCII(tiffData, fileNameAsc);
            }

            File.Delete(fileName);
        }

        private void TiffDataFast(string fileName)
        {
            int resolution = terrainResolutionDownloading + 1;
            string trim = fileName.Substring(fileName.LastIndexOf(@"\") + 1);
            string row = trim.Substring(0, trim.LastIndexOf("-"));
            string column = fileName.Substring(fileName.LastIndexOf("-") + 1).Replace(".tif", "");

            try
            {
                using (Tiff inputImage = Tiff.Open(fileName, "r"))
                {
                    tiffWidth = inputImage.GetField(TiffTag.IMAGEWIDTH)[0].ToInt();
                    tiffLength = inputImage.GetField(TiffTag.IMAGELENGTH)[0].ToInt();
                    tiffData = new float[resolution, resolution];

                    int tileHeight = inputImage.GetField(TiffTag.TILELENGTH)[0].ToInt();
                    int tileWidth = inputImage.GetField(TiffTag.TILEWIDTH)[0].ToInt();

                    byte[] buffer = new byte[tileHeight * tileWidth * 4];
                    float[,] fBuffer = new float[tileHeight, tileWidth];

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

            // Add Bottom Row (PO2 + 1 Resolution)
            for (int i = 0; i < resolution; i++)
                tiffData[i, resolution - 1] = tiffData[i, resolution - 2];

            // Add Right Column (PO2 + 1 Resolution)
            for (int i = 0; i < resolution; i++)
                tiffData[resolution - 1, i] = tiffData[resolution - 2, i];

            if (smoothIterations > 0)
                tiffData = SmoothedHeightsFast(tiffData, resolution, resolution, smoothIterations);

            if (formatIndex == 0)
            {
                string fileNameRaw = directoryPathElevation + "/" + row + "-" + column + ".raw";
                SaveTerrainDataRAW(tiffData, fileNameRaw);
            }
            else if (formatIndex == 1)
            {
                string fileNameAsc = directoryPathElevation + "/" + row + "-" + column + ".asc";
                SaveTerrainDataASCII(tiffData, fileNameAsc);
            }

            File.Delete(fileName);
        }

        private void TiffData(string fileName)
        {
            highestPoint = float.MinValue;
            lowestPoint = float.MaxValue;

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

                heightmapResXAll = tiffWidth;
                heightmapResYAll = tiffLength;

                for (int y = 0; y < tiffLength; y += tileHeight)
                {
                    for (int x = 0; x < tiffWidth; x += tileWidth)
                    {
                        inputImage.ReadTile(buffer, 0, x, y, 0, 0);
                        Buffer.BlockCopy(buffer, 0, fBuffer, 0, buffer.Length);

                        for (int i = 0; i < tileHeight; i++)
                        {
                            for (int j = 0; j < tileWidth; j++)
                            {
                                if ((y + i) < tiffLength && (x + j) < tiffWidth)
                                {
                                    float current = fBuffer[i, j];
                                    tiffDataASCII[y + i, x + j] = current;

                                    if (i > 0 && i < tileHeight - 1 && j > 0 && j < tileWidth - 1)
                                    {
                                        if (highestPoint < current)
                                            highestPoint = current;

                                        if (lowestPoint > current)
                                            lowestPoint = current;
                                    }
                                }
                            }
                        }

                        progressDATA = Mathf.InverseLerp(0f, (float)tiffLength, (float)y);
                    }
                }
            }

            UnityEngine.Debug.Log("Highest Point: " + highestPoint + "   " + "Lowest Point: " + lowestPoint);

            // Rotate terrain heigts and normalize values
            for (int y = 0; y < tiffWidth; y++)
            {
                for (int x = 0; x < tiffLength; x++)
                {
                    currentHeight = tiffDataASCII[(tiffWidth - 1) - y, x];

                    try
                    {
                        if (lowestPoint >= 0)
                            //tiffData[y, x] = (currentHeight - lowestPoint) / everestPeak;
                            tiffData[y, x] = currentHeight / everestPeak;
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

        private void CalculateResampleHeightmaps()
        {
            // Set chunk resolutions to a "Previous Power of 2" value
            if (splittedTerrains)
            {
                if (!Mathf.IsPowerOfTwo(croppedTerrains.Count))
                {
                    heightmapResFinalX = ((Mathf.NextPowerOfTwo(reducedheightmapResolution / splitSizeFinal)) / 2) + 1;
                    heightmapResFinalY = ((Mathf.NextPowerOfTwo(reducedheightmapResolution / splitSizeFinal)) / 2) + 1;
                    heightmapResFinalXAll = heightmapResFinalX * splitSizeFinal;
                    heightmapResFinalYAll = heightmapResFinalY * splitSizeFinal;

                    ResampleOperation();
                }
                else
                {
                    heightmapResolutionSplit = reducedheightmapResolution / (int)Mathf.Sqrt((float)terrainChunks);

                    heightmapResFinalX = heightmapResolutionSplit + 1;
                    heightmapResFinalY = heightmapResolutionSplit + 1;
                    heightmapResFinalXAll = terrainResolutionDownloading;
                    heightmapResFinalYAll = terrainResolutionDownloading;

                    finalHeights = new float[heightmapResFinalXAll, heightmapResFinalYAll];
                    finalHeights = tiffData;
                }
            }
            else if (terrain)
            {
                heightmapResFinalX = terrainResolutionDownloading;
                heightmapResFinalY = terrainResolutionDownloading;
                heightmapResFinalXAll = terrainResolutionDownloading;
                heightmapResFinalYAll = terrainResolutionDownloading;

                finalHeights = new float[heightmapResFinalXAll, heightmapResFinalYAll];
                finalHeights = tiffData;
            }
            else
            {
                if (!Mathf.IsPowerOfTwo(splitSizeNew))
                {
                    heightmapResFinalX = ((Mathf.NextPowerOfTwo(reducedheightmapResolution / splitSizeNew)) / 2) + 1;
                    heightmapResFinalY = ((Mathf.NextPowerOfTwo(reducedheightmapResolution / splitSizeNew)) / 2) + 1;
                    heightmapResFinalXAll = heightmapResFinalX * splitSizeNew;
                    heightmapResFinalYAll = heightmapResFinalY * splitSizeNew;

                    ResampleOperation();
                }
                else
                {
                    heightmapResFinalX = (reducedheightmapResolution / splitSizeNew) + 1;
                    heightmapResFinalY = (reducedheightmapResolution / splitSizeNew) + 1;
                    heightmapResFinalXAll = terrainResolutionDownloading;
                    heightmapResFinalYAll = terrainResolutionDownloading;

                    finalHeights = new float[heightmapResFinalXAll, heightmapResFinalYAll];
                    finalHeights = tiffData;
                }
            }
        }

        private void ResampleOperation()
        {
            float scaleFactorLat = ((float)heightmapResFinalXAll) / ((float)heightmapResXAll);
            float scaleFactorLon = ((float)heightmapResFinalYAll) / ((float)heightmapResYAll);

            finalHeights = new float[heightmapResFinalXAll, heightmapResFinalYAll];

            for (int x = 0; x < heightmapResFinalXAll; x++)
            {
                for (int y = 0; y < heightmapResFinalYAll; y++)
                {
                    finalHeights[x, y] = ResampleHeights((float)x / scaleFactorLat, (float)y / scaleFactorLon);
                }
            }
        }

        private float ResampleHeights(float X, float Y)
        {
            try
            {
                int X1 = Mathf.RoundToInt((X + heightmapResXAll % heightmapResXAll));
                int Y1 = Mathf.RoundToInt((Y + heightmapResYAll % heightmapResYAll));

                return tiffData[X1, Y1];
            }
            catch
            {
                return 0f;
            }
        }

        private void FinalizeSmooth(float[,] heightMapSmoothed, int width, int height, int iterations, int blendIndex, float blending)
        {
            if (iterations != 0)
            {
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

                //tiffData = heightMapSmoothed;
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

        private float[,] SmoothedHeightsFast(float[,] heightMap, int tw, int th, int iterations)
        {
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
            }

            return heightMap;
        }

        private void SetWorldSize ()
        {
            if (unitsToOneMeter)
                SetUnitsTo1Meter();
            else if (useScaleFactor)
            {
                terrainSizeNewX = areaSizeLon * 1000f * scaleFactor;
                terrainSizeNewZ = areaSizeLat * 1000f * scaleFactor;
            }
        }

        private void SetData()
        {
            SetWorldSize();
            tileWidth = terrainSizeNewX / splitSizeFinal;
            tileLength = terrainSizeNewZ / splitSizeFinal;
            tileXPos = (terrainSizeNewX / 2f) * -1f;
            tileZPos = (terrainSizeNewZ / 2f) * -1f;
        }

        private void CreateTerrainData()
        {
            data = new TerrainData[splitSizeFinal * splitSizeFinal];
            terrainName = "Terrain";

            for (int y = 0; y < splitSizeFinal; y++)
            {
                for (int x = 0; x < splitSizeFinal; x++)
                {
                    AssetDatabase.CreateAsset(new TerrainData(), splitDirectoryPath.Substring(splitDirectoryPath.LastIndexOf("Assets")) + "/" + terrainName + " " + (y + 1) + "-" + (x + 1) + ".asset");
                    EditorUtility.DisplayProgressBar("CREATING DATA", "Creating Terrain Data Assets", Mathf.InverseLerp(0f, splitSizeFinal, y));
                }
            }

            EditorUtility.ClearProgressBar();
        }

        private void CreateTerrainObject()
        {
            terrainGameObjects = new GameObject[splitSizeFinal * splitSizeFinal];
            terrains = new Terrain[splitSizeFinal * splitSizeFinal];
            arrayPos = 0;

            if (splitSizeFinal > 1)
            {
                if (address != "")
                    terrainsParent = new GameObject("Terrains  " + downloadDateElevation + "  ---  " + address + "  " + splitSizeFinal + "x" + splitSizeFinal);
                else
                    terrainsParent = new GameObject("Terrains  " + downloadDateElevation + "  ---  " + splitSizeFinal + "x" + splitSizeFinal);
            }

            int currentRow = splitSizeFinal;

            for (int y = 0; y < splitSizeFinal; y++)
            {
                for (int x = 0; x < splitSizeFinal; x++)
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
                    data[arrayPos].heightmapResolution = 32;
                    data[arrayPos].size = new Vector3(tileWidth, terrainSizeNewY, tileLength);

                    terrainGameObjects[arrayPos].GetComponent<TerrainCollider>().terrainData = data[arrayPos];
                    terrainGameObjects[arrayPos].transform.position = new Vector3(x * tileWidth + tileXPos, 0, y * tileLength + tileZPos);

                    arrayPos++;

                    EditorUtility.DisplayProgressBar("CREATING TERRAIN", "Creating Terrain Objects", Mathf.InverseLerp(0f, splitSizeFinal, y));
                }
                currentRow--;
            }

            EditorUtility.ClearProgressBar();

            if (splitSizeFinal > 1)
            {
                int length = terrainGameObjects.Length;
                string[] terrainNames = new string[length];
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
                terrains[y].heightmapPixelError = pixelError;
        }

        //	private void RepositionTerrainChunks ()
        //	{
        //		int counter = 0;
        //		
        //		for(int y = 0; y < terrainsLong ; y++)
        //		{
        //			for(int x = 0; x < terrainsWide; x++)
        //			{
        //				//croppedTerrains[counter].terrainData.size = new Vector3(newWidth, oldHeight, newLength);
        //				croppedTerrains[counter].transform.position = new Vector3(x * newWidth + xPos, yPos, y * newLength + zPos);
        //				
        //				counter++;
        //			}
        //		}
        //	}

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

		for(int i = 0; i < terrainsWide * terrainsLong ; i++)
			croppedTerrains[i].Flush();
		
		EditorUtility.ClearProgressBar();
#endif
        }

        //	private static void StitchTerrains (Terrain[] terrainsLst, int stitchWidthInt, int terrainResInt, int tWidthInt, int tHeightInt)
        //	{
        //		foreach (Terrain t in terrainsLst) {
        //			Undo.RecordObject(t.terrainData, "Stitching Terrain");
        //		}
        //		
        //		stitchWidthInt = Mathf.Clamp(stitchWidthInt, 1, (terrainResInt - 1) / 2);
        //		int counter = 0;
        //		int total = tHeightInt * (tWidthInt - 1) + (tHeightInt - 1) * tWidthInt;
        //		
        //		for (int h = 0; h < tHeightInt; h++) {
        //			for (int w = 0; w < tWidthInt - 1; w++) {
        //				EditorUtility.DisplayProgressBar("STITCHING TERRAINS", "Stitching Terrain Tiles", Mathf.InverseLerp(0, total, ++counter));
        //				BlendData (terrainsLst[h * tWidthInt + w].terrainData, terrainsLst[h * tWidthInt + w + 1].terrainData, Direction.Across, false);
        //			}
        //		}
        //		
        //		for (int h = 0; h < tHeightInt - 1; h++) {
        //			for (int w = 0; w < tWidthInt; w++) {
        //				EditorUtility.DisplayProgressBar("STITCHING TERRAINS", "Stitching Terrain Tiles", Mathf.InverseLerp(0, total, ++counter));
        //				BlendData (terrainsLst[h * tWidthInt + w].terrainData, terrainsLst[(h + 1) * tWidthInt + w].terrainData, Direction.Down, false);
        //			}
        //		}
        //		
        //		EditorUtility.ClearProgressBar();
        //	}
        //	
        //	private static void BlendData (TerrainData terrain1, TerrainData terrain2, Direction thisDirection, bool singleTerrain) {
        //		
        //		float[,] heightmapData = terrain1.GetHeights(0, 0, terrainRes, terrainRes);
        //		float[,] heightmapData2 = terrain2.GetHeights(0, 0, terrainRes, terrainRes);
        //		int pos = terrainRes - 1;
        //		
        //		if (thisDirection == Direction.Across) {
        //			for (int i = 0; i < terrainRes; i++) {
        //				for (int j = 1; j < stitchWidth; j++) {
        //					
        //					float mix = Mathf.Lerp(heightmapData[i, pos - j], heightmapData2[i, j], .5f);
        //					
        //					if (j == 1) {
        //						heightmapData[i, pos] = mix;
        //						heightmapData2[i, 0] = mix;
        //					}
        //					
        //					float t = Mathf.SmoothStep(0.0f, 1.0f, Mathf.InverseLerp(1, stitchWidth - 1, j));
        //					heightmapData[i, pos - j] = Mathf.Lerp(mix, heightmapData[i, pos - j], t);
        //					
        //					if (!singleTerrain)
        //						heightmapData2[i, j] = Mathf.Lerp(mix, heightmapData2[i, j], t);
        //					else
        //						heightmapData[i, j] = Mathf.Lerp(mix, heightmapData2[i, j], t);
        //				}
        //			}
        //			if (singleTerrain) {
        //				for (int i = 0; i < terrainRes; i++) {
        //					heightmapData[i, 0] = heightmapData[i, pos];
        //				}
        //			}
        //		}
        //		else {
        //			for (int i = 0; i < terrainRes; i++) {
        //				for (int j = 1; j < stitchWidth; j++) {
        //					
        //					float mix = Mathf.Lerp(heightmapData2[pos - j, i], heightmapData[j, i], .5f);
        //					
        //					if (j == 1) {
        //						heightmapData2[pos, i] = mix;
        //						heightmapData[0, i] = mix;
        //					}
        //					
        //					float t = Mathf.SmoothStep(0.0f, 1.0f, Mathf.InverseLerp(1, stitchWidth - 1, j));
        //					
        //					if (!singleTerrain) {
        //						heightmapData2[pos - j, i] = Mathf.Lerp(mix, heightmapData2[pos - j, i], t);
        //					}
        //					else {
        //						heightmapData[pos - j, i] = Mathf.Lerp(mix, heightmapData2[pos - j, i], t);
        //					}
        //					
        //					heightmapData[j, i] = Mathf.Lerp(mix, heightmapData[j, i], t);
        //				}
        //			}
        //			if (singleTerrain) {
        //				for (int i = 0; i < terrainRes; i++) {
        //					heightmapData[pos, i] = heightmapData[0, i];
        //				}
        //			}
        //		}
        //		
        //		terrain1.SetHeights(0, 0, heightmapData);
        //		
        //		if (!singleTerrain) {
        //			terrain2.SetHeights(0, 0, heightmapData2);
        //		}
        //	}

        private void SetupImagery()
        {
            cancelOperation = false;
            SetGridParams();

            if (!failedDownloading)
            {
                if (!dynamicWorld)
                {
                    CheckAvailableTerrainTextures();

                    if (!cancelOperation)
                        SetTerrainSizes();
                }

                if (!cancelOperation)
                    SetSaveDirectories();
            }
            else
            {
                if (!dynamicWorld)
                {
                    SetTerrainSizes();
                    SetTerrainsInProgress();
                }
            }

            if (cancelOperation)
                return;

            if (!dynamicWorld)
                SetTempDirectory();

            SetProgressBarImagery();
        }

        private void SetGridParams()
        {
            if (splittedTerrains)
            {
                CheckTerrainChunks();
                splitSizeFinal = (int)Mathf.Sqrt((float)croppedTerrains.Count);
            }
            else if (terrain)
            {
                terrainChunks = 1;
                splitSizeFinal = 1;
            }
        }

        private void SetProgressBarImagery()
        {
            ThreadPool.GetAvailableThreads(out workerThreads, out completionPortThreads);

            if (showProgressElevation)
                allThreads = workerThreads + 2;
            else
                allThreads = workerThreads;

            imageDownloadingStarted = true;
            cancelOperation = false;
            showProgressImagery = true;
            finishedImporting = false;
            allBlack = false;
            downloadedImageIndex = 0;
            maxThreads = maxAsyncCalls + 1;

            EditorApplication.update += CheckThreadStatusImageDownloader;
            EditorGUILayout.HelpBox(completionPortThreads.ToString(), MessageType.None);
            //EditorGUILayout.HelpBox(noData.ToString(), MessageType.None);
        }

        private void SetTempDirectory()
        {
            try
            {
                Directory.Delete(projectPath + "Temporary Imagery Data", true);
            }
            catch { }

            Directory.CreateDirectory(projectPath + "Temporary Imagery Data");

            //if (areaSizeLat == areaSizeLon)
            //{
            //    areaIsSquare = true;
            //    areaIsRectangleLat = false;
            //    areaIsRectangleLon = false;
            //}
            //else if (areaSizeLat > areaSizeLon)
            //{
            //    areaIsSquare = false;
            //    areaIsRectangleLat = true;
            //    areaIsRectangleLon = false;
            //}
            //else if (areaSizeLon > areaSizeLat)
            //{
            //    areaIsSquare = false;
            //    areaIsRectangleLat = false;
            //    areaIsRectangleLon = true;
            //}
        }

        private void CheckAvailableTerrainTextures()
        {
            if (textureOnFinish == 0)
            {
                if (splittedTerrains)
                {
                    splittedTerrains.SetActive(true);
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

                    if (texturesCount == 1)
                    {
                        if (EditorUtility.DisplayDialog("TERRAIN TEXTURES", "There is a texture available on your terrain.\n\nPressing \"Continue\" will remove this texture and replace it with the downloading satellite images.", "Cancel", "Continue"))
                        {
                            cancelOperation = true;
                            showProgressImagery = false;
                            return;
                        }
                    }
                    else if (texturesCount > 1)
                    {
                        if (EditorUtility.DisplayDialog("TERRAIN TEXTURES", "There are textures available on your terrain.\n\nPressing \"Continue\" will remove them and replace it with the downloading satellite images.", "Cancel", "Continue"))
                        {
                            cancelOperation = true;
                            showProgressImagery = false;
                            return;
                        }
                    }
                }
                else if (terrain)
                {
                    terrain.gameObject.SetActive(true);
                    int texturesCount = 0;

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
                            cancelOperation = true;
                            showProgressImagery = false;
                            return;
                        }
                    }
                    else if (texturesCount > 1)
                    {
                        if (EditorUtility.DisplayDialog("TERRAIN TEXTURES", "There are textures available on your terrain.\n\nPressing \"Continue\" will remove them and replace it with the downloading satellite images.", "Cancel", "Continue"))
                        {
                            cancelOperation = true;
                            showProgressImagery = false;
                            return;
                        }
                    }
                }
            }
        }

        private void SetSaveDirectories()
        {
            if (!dynamicWorld && gridNumber > 10)
            {
                int alphamapsNo = Mathf.FloorToInt((float)totalImages / 4f);

                if (EditorUtility.DisplayDialog("HEAVY DATA LOAD", totalImages.ToString() + "  images will be downloaded and  " + alphamapsNo.ToString() + "  alphamaps will be created for terrain.\n\nThat will be heavy, Are you sure?", "No", "Yes"))
                {
                    cancelOperation = true;
                    showProgressImagery = false;
                    return;
                }
            }

            if (terrainGenerationstarted)
                downloadDateImagery = downloadDateElevation;
            else
                downloadDateImagery = DateTime.Now.ToString("MM-dd-yyyy_HH-mm-ss");

            if (!dynamicWorld)
            {
                if (!Directory.Exists(downloadsPath))
                    Directory.CreateDirectory(downloadsPath);

                if (textureOnFinish == 0)
                    importAtEnd = true;

                if (importAtEnd)
                {
                    directoryPathImagery = downloadsPath + "/" + downloadDateImagery + "/Satellite Images";
                    directoryPathTerrainlayers = downloadsPath + "/" + downloadDateImagery + "/Terrain Layers";
                    Directory.CreateDirectory(directoryPathTerrainlayers);
                }
                else
                    directoryPathImagery = EditorUtility.OpenFolderPanel("Select a folder to save satellite images", projectPath, "Imagery");
            }
            else
                directoryPathImagery = serverPath + "/Imagery";

            if (!string.IsNullOrEmpty(directoryPathImagery))
            {
                Directory.CreateDirectory(directoryPathImagery);

                if (!dynamicWorld)
                    WritePresetFile(directoryPathImagery + "/Terrain Info.tlps");
            }
            else
            {
                EditorUtility.DisplayDialog("NO SAVE LOCATION", "Select a save location to download Imagery", "Ok");
                return;
            }

            //if(splittedTerrains)
            //{
            //    for(int i = 0; i < croppedTerrains.Count; i++)
            //        Directory.CreateDirectory(directoryPathImagery + "/Terrain " + (i + 1).ToString());
            //}
        }

        private void GetPresetInfo()
        {
            if (!dynamicWorld)
                infoFilePath = Directory.GetFiles(AssetDatabase.GetAssetPath(failedFolder), "*.tlps", SearchOption.AllDirectories);
            else
                infoFilePath = Directory.GetFiles(directoryPathInfo, "*.tlps", SearchOption.AllDirectories);

            presetFilePath = infoFilePath[0];

            if (infoFilePath.Length == 0)
            {
                EditorUtility.DisplayDialog("TERRAIN INFO NOT AVILABLE", "There must be a text file \"Terrain Info\" in selected folder to continue.\n\nTerrain Info will be automatically created when you start downloading satellite images.", "Ok");
                return;
            }

            if (presetFilePath.Contains("tlps"))
                ReadPresetFile();
        }

        private void SetTerrainSizes()
        {
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
            else
            {
                if (textureOnFinish == 0)
                {
                    EditorUtility.DisplayDialog("UNAVAILABLE TERRAIN", unavailableTerrainStr, "Ok");
                    cancelOperation = true;
                    showProgressImagery = false;
                    return;
                }
            }
        }

        private void SetTerrainsInProgress()
        {
            if (splittedTerrains)
            {
                AssetDatabase.Refresh();

                failedTerrainNames = new List<string>();

#if UNITY_2018_3_OR_NEWER
                foreach (Terrain t in croppedTerrains)
                {
                    try
                    {
                        if (t.terrainData.terrainLayers != null && t.terrainData.terrainLayers.Length > 0)
                        {
                            int splatCount = t.terrainData.terrainLayers.Length;

                            for (int i = 0; i < splatCount; i++)
                            {
                                string textureName = t.terrainData.terrainLayers[i].diffuseTexture.name;

                                if (textureName.Contains(tempPattern))
                                    failedTerrainNames.Add(t.name);
                            }
                        }
                    }
                    catch { }
                }
#else
            foreach(Terrain t in croppedTerrains)
            {
                int splatCount = t.terrainData.splatPrototypes.Length;

                for (int i = 0; i < splatCount; i++)
                {
                    string textureName = t.terrainData.splatPrototypes[i].texture.name;

                    if(textureName.Contains(tempPattern))
                        failedTerrainNames.Add(t.name);
                }
            }
#endif
            }
        }

        private void GetSatelliteImages()
        {
            RunAsync(()=>
            {
                ServerInfoImagery();

                QueueOnMainThread(()=>
                {
                    if (cancelOperation)
                    {
                        showProgressImagery = false;
                        AssetDatabase.Refresh();
                        return;
                    }

                    AssetDatabase.Refresh();
                });
            });
        }

        private void ServerInfoImagery()
        {
            mapserviceImagery = new TerraLandWorldImagery.World_Imagery_MapServer();

            //TileImageInfo tileImageInfo = mapservice.GetTileImageInfo(mapserviceImagery.GetDefaultMapName());
            //tileImageInfo.CompressionQuality = compressionQuality;
            //mapinfo = mapservice.GetServerInfo(mapserviceImagery.GetDefaultMapName());
            //mapdesc = mapinfo.DefaultMapDescription;

            if (!failedDownloading)
            {
                for (int i = 0; i < totalImages; i++)
                {
                    if (cancelOperation)
                    {
                        showProgressImagery = false;
                        return;
                    }

                    xMin[i] = lonCellLeft[i];
                    yMin[i] = latCellBottom[i];
                    xMax[i] = lonCellRight[i];
                    yMax[i] = latCellTop[i];

                    ServerConnectImagery(i, i);
                }
            }
            else
            {
                for (int i = 0; i < failedIndicesCountImagery; i++)
                {
                    if (cancelOperation)
                    {
                        showProgressImagery = false;
                        return;
                    }

                    int currentIndex = failedIndicesImagery[i];

                    xMinFailedImagery[i] = lonCellLeft[currentIndex];
                    yMinFailedImagery[i] = latCellBottom[currentIndex];
                    xMaxFailedImagery[i] = lonCellRight[currentIndex];
                    yMaxFailedImagery[i] = latCellTop[currentIndex];

                    ServerConnectImagery(i, currentIndex);
                }
            }
        }

        private void ServerConnectImagery(int i, int current)
        {
            RunAsync(()=>
            {
                ImageDownloader(i, current);

                QueueOnMainThread(()=>
                {
                    if (cancelOperation)
                    {
                        showProgressImagery = false;
                        AssetDatabase.Refresh();
                        return;
                    }

                    if (allBlack && EditorUtility.DisplayDialog("UNAVAILABLE IMAGERY", "There is no available imagery at this resolution for this tile or there was an unknown internt connection error!\n\nIn SATELLITE IMAGE DOWNLOADER section, decrease GRID PER TERRAIN or IMAGE RESOLUTION value or increase AREA SIZE EXTENTS.\nIt may also be possible that you are behind a blocked IP or network limitations or no internet connection detected!\n\nDo you want to continue and try downloading other tiles?", "No", "Yes"))
                    {
                        cancelOperation = true;
                        showProgressImagery = false;
                        imageDownloadingStarted = false;
                        finishedImporting = true;

                        if (!dynamicWorld)
                        {
                            AssetDatabase.Refresh();
                            Directory.Delete(directoryPathImagery, true);
                            Directory.Delete(directoryPathTerrainlayers, true);
                            AssetDatabase.Refresh();
                        }

                        allThreads = 0;
                        CheckHeightmapDownloaderAndRecompile();
                        return;
                    }

                    allBlack = false;

                    if (!dynamicWorld)
                    {
                        AssetDatabase.Refresh();

                        int row = Mathf.CeilToInt((float)(current + 1) / (float)gridNumber);
                        int column = (current + 1) - ((row - 1) * gridNumber);
                        string imgName = directoryPathImagery + "/" + row + "-" + column + ".jpg";
                        string failedImgName = directoryPathImagery + "/" + row + "-" + column + tempPattern + ".jpg";
                        string tempFile = projectPath + "Temporary Imagery Data/" + row + "-" + column + ".jpg";

                        if (File.Exists(tempFile))
                        {
                            if (File.Exists(failedImgName))
                                File.Delete(failedImgName);

                            string tileName = "Terrain " + row + "-" + column;

                            GameObject tile = new GameObject(tileName);
                            tile.hideFlags = HideFlags.HideAndDontSave;
                            tile.transform.parent = imageImportTiles.transform;

                            TileImageImport TII = tile.AddComponent<TileImageImport>();
                            TII.tempPath = tempFile;
                            TII.imgName = imgName;
                            TII.imageResolution = imageResolution;
                            TII.anisotropicFilter = anisotropicFilter;
                            TII.ImportImage();
                        }

                        if (!failedDownloading)
                        {
                            if (downloadedImageIndex == totalImages && !finishedImporting)
                                FinalizeTerrainImagery(true);
                        }
                        else
                        {
                            if (downloadedImageIndex == totalFailedImages && !finishedImporting)
                                FinalizeTerrainImagery(true);
                        }
                    }
                });
            });
        }

        private void ImageDownloader(int i, int current)
        {
            if (!allBlack)
            {
                int row = Mathf.CeilToInt((float)(current + 1) / gridNumber);
                int column = (current + 1) - ((row - 1) * gridNumber);
                string imgName = "";

                try
                {
                    TerraLandWorldImagery.MapServerInfo mapinfo = mapserviceImagery.GetServerInfo(mapserviceImagery.GetDefaultMapName());
                    TerraLandWorldImagery.MapDescription mapdesc = mapinfo.DefaultMapDescription;
                    TerraLandWorldImagery.EnvelopeN extent = new TerraLandWorldImagery.EnvelopeN();

                    if (!failedDownloading)
                    {
                        extent.XMin = xMin[i];
                        extent.YMin = yMin[i];
                        extent.XMax = xMax[i];
                        extent.YMax = yMax[i];
                    }
                    else
                    {
                        extent.XMin = xMinFailedImagery[i];
                        extent.YMin = yMinFailedImagery[i];
                        extent.XMax = xMaxFailedImagery[i];
                        extent.YMax = yMaxFailedImagery[i];
                    }

                    mapdesc.MapArea.Extent = extent;

                    TerraLandWorldImagery.ImageType imgtype = new TerraLandWorldImagery.ImageType();
                    imgtype.ImageFormat = TerraLandWorldImagery.esriImageFormat.esriImageJPG;
                    imgtype.ImageReturnType = TerraLandWorldImagery.esriImageReturnType.esriImageReturnMimeData;
                    //imgtype.ImageReturnType = esriImageReturnType.esriImageReturnURL;

                    TerraLandWorldImagery.ImageDisplay imgdisp = new TerraLandWorldImagery.ImageDisplay();
                    imgdisp.ImageHeight = (int)(imageResolution * terrainSizeFactor);
                    imgdisp.ImageWidth = imageResolution;

                    imgdisp.ImageDPI = 72; // Default is 96

                    TerraLandWorldImagery.ImageDescription imgdesc = new TerraLandWorldImagery.ImageDescription();
                    imgdesc.ImageDisplay = imgdisp;
                    imgdesc.ImageType = imgtype;

                    TerraLandWorldImagery.MapImage mapimg = mapserviceImagery.ExportMapImage(mapdesc, imgdesc);
                    //mapservice.ExportMapImageCompleted += new ExportMapImageCompletedEventHandler(MapImageCompleted);
                    //mapservice.ExportMapImageAsync(mapdesc, imgdesc);

                    //if(!dynamicWorld)
                    //{
                    //    // Crop the satellite images if area is rectangular so that they match to their cell positions
                    //    if(areaIsRectangleLat)
                    //    {
                    //        EnvelopeN extentESRI = new EnvelopeN();
                    //        extentESRI = (EnvelopeN)mapimg.Extent;
                    //
                    //        cropSizeX = (int)((double)imageResolution * (extent.XMax - extent.XMin) / (extentESRI.XMax - extentESRI.XMin));
                    //        cropOffsetX = (imageResolution - cropSizeX) / 2;
                    //        cropSizeY = imageResolution;
                    //        cropOffsetY = 0;
                    //    }
                    //    else if(areaIsRectangleLon)
                    //    {
                    //        EnvelopeN extentESRI = new EnvelopeN();
                    //        extentESRI = (EnvelopeN)mapimg.Extent;
                    //
                    //        cropSizeX = imageResolution;
                    //        cropOffsetX = 0;
                    //        cropSizeY = (int)((double)imageResolution * (extent.YMax - extent.YMin) / (extentESRI.YMax - extentESRI.YMin));
                    //        cropOffsetY = (imageResolution - cropSizeY) / 2;
                    //    }
                    //}


                    byte[] imageData = mapimg.ImageData;
                    //string imgURL = mapimg.ImageURL;

                    if (!dynamicWorld)
                        imgName = projectPath + "Temporary Imagery Data/" + row + "-" + column + ".jpg";
                    else
                        imgName = directoryPathImagery + "/" + row + "-" + column + ".jpg";

                    string tempFileName = imgName.Replace(".jpg", tempPattern + ".jpg");

                    if (File.Exists(tempFileName))
                        File.Delete(tempFileName);

                    File.WriteAllBytes(imgName, imageData);
                    //DownloadImageryData(imgURL, imgName);
                }
                catch (Exception e)
                {
                    imgName = directoryPathImagery + "/" + row + "-" + column + tempPattern + ".jpg";

                    //if (downloadedImageIndex == 0 && !failedDownloading)
                        //CheckImageColors(imgName);

                    if (!File.Exists(imgName))
                        File.WriteAllBytes(imgName, tempImageBytes);

                    // Following lines will remove tiles if were already available from previous download sessions
                    imgName = directoryPathImagery + "/" + row + "-" + column + ".jpg";
                    
                    if (File.Exists(imgName))
                        File.Delete(imgName);

                    failedTilesAvailable = true;

                    UnityEngine.Debug.Log(e);

                    if (downloadedImageIndex == 0)
                        allBlack = true;
                }
                finally
                {
                    downloadedImageIndex++;
                }

                if (cancelOperation)
                {
                    showProgressImagery = false;
                    return;
                }
            }
        }

        private void MapImageCompleted(object sender, TerraLandWorldImagery.ExportMapImageCompletedEventArgs e)
        {
            TerraLandWorldImagery.MapImage mapimg = e.Result;
            string imgURL = mapimg.ImageURL;

            UnityEngine.Debug.Log(imgURL);

            string imgName = "";
            int row = 1;
            int column = 1;

            if (!dynamicWorld)
                imgName = projectPath + "Temporary Imagery Data/" + row + "-" + column + ".jpg";
            else
                imgName = directoryPathImagery + "/" + row + "-" + column + ".jpg";

            string tempFileName = imgName.Replace(".jpg", tempPattern + ".jpg");

            if (File.Exists(tempFileName))
                File.Delete(tempFileName);

            //File.WriteAllBytes(imgName, imageData);
            DownloadImageryData(imgURL, imgName);
        }

        //private void ImportImage(string imgName)
        //{
        //    TextureImporter textureImporter = (TextureImporter)TextureImporter.GetAtPath(imgName.Substring(imgName.LastIndexOf("Assets")));
        //
        //    if (textureImporter != null)
        //    {
        //        textureImporter.isReadable = true;
        //        textureImporter.mipmapEnabled = true;
        //        textureImporter.wrapMode = TextureWrapMode.Clamp;
        //        textureImporter.anisoLevel = anisotropicFilter;
        //
        //        TextureImporterPlatformSettings platformSettings = new TextureImporterPlatformSettings();
        //        platformSettings.overridden = true;
        //        platformSettings.format = TextureImporterFormat.Automatic;
        //        platformSettings.maxTextureSize = imageResolution;
        //        textureImporter.SetPlatformTextureSettings(platformSettings);
        //
        //        if (!areaIsSquare || autoScale)
        //        {
        //            Texture2D satelliteImage = AssetDatabase.LoadAssetAtPath(imgName.Substring(imgName.LastIndexOf("Assets")), typeof(Texture2D)) as Texture2D;
        //
        //            if (!areaIsSquare)
        //                satelliteImage = ImageCropper(satelliteImage, cropOffsetX, cropSizeX, cropSizeY, cropOffsetY);
        //
        //            if (autoScale)
        //                satelliteImage = ScaleTexture(satelliteImage, imageResolution, imageResolution);
        //
        //            if (!areaIsSquare || compressionQuality < 100)
        //            {
        //                byte[] bytes = satelliteImage.EncodeToJPG();
        //                File.WriteAllBytes(imgName, bytes);
        //            }
        //        }
        //    }
        //
        //    AssetDatabase.Refresh();
        //}

        private void FinalizeTerrainImageryRETRY()
        {
            //QueueOnMainThread(() =>
            //{
            importAgentsChildrenCount = imageImportTiles.transform.childCount;
            FinalizeTerrainImagery(true);
            //});
        }

        public void FinalizeTerrainImagery(bool checkFailedTiles = false)
        {
            ForceFailedImports();

            int importedImagesCount = Directory.GetFiles(directoryPathImagery, "*.jpg", SearchOption.AllDirectories).Length;
            importAgentsChildrenCount = imageImportTiles.transform.childCount;

            //RunAsync(() =>
            //{
            if (!taskDone && importAgentsChildrenCount > 0 && importedImagesCount < totalImages)
                FinalizeTerrainImageryRETRY();

            //QueueOnMainThread(() =>
            //{
            if (!taskDone && importAgentsChildrenCount == 0 && importedImagesCount == totalImages)
            {
                if (!dynamicWorld && !allBlack)
                {
                    //MoveDownloadedImages();

                    if (textureOnFinish == 0)
                        ImageTilerDownloader();
                    else
                        Process.Start(directoryPathImagery.Replace(@"/", @"\") + @"\");
                }

                cancelOperation = true;
                showProgressImagery = false;
                imageDownloadingStarted = false;
                finishedImporting = true;
                failedDownloading = false;
                allThreads = 0;
                normalizedProgressSatelliteImage = 0f;

                AssetDatabase.Refresh();

                if (!dynamicWorld)
                {
                    ManageNeighborings();
                    CheckFailedImages();
                }

                try
                {
                    Directory.Delete(projectPath + "Temporary Imagery Data", true);
                }
                catch { }

                CheckHeightmapDownloaderAndRecompile();

                if (dynamicWorld)
                {
                    serverSetUpImagery = true;

                    if (serverSetUpElevation && serverSetUpImagery)
                    {
                        if (failedTilesAvailable)
                        {
                            EditorUtility.DisplayDialog("FAILED TILES AVAILABLE", "There are some failed tile downloads for this session.\n\nGo to FAILED TILES DOWNLOADER section and press GET FAILED TILES button to re-download failed tiles.", "Ok");
                            showFailedDownloaderSection = true;
                        }

                        Process.Start(serverPath.Replace(@"/", @"\") + @"\");
                    }
                }

                taskDone = true;
            }
            //});
            //});

            if (checkFailedTiles)
                DownloadFailedImageTiles(false);
        }

        private void ForceFailedImports()
        {
            AssetDatabase.Refresh();
            if (!Directory.Exists(projectPath + "Temporary Imagery Data")) return;
            if (!Directory.Exists(directoryPathImagery)) Directory.CreateDirectory(directoryPathImagery);
            AssetDatabase.Refresh();

            string[] allImageNamesInTemp = LogicalComparer(projectPath + "Temporary Imagery Data", ".jpg");

            for (int i = 0; i < allImageNamesInTemp.Length; i++)
            {
                string tempPath = allImageNamesInTemp[i];
                string imgName = directoryPathImagery + "/" + System.IO.Path.GetFileName(tempPath);
                File.Move(tempPath, imgName);

                AssetDatabase.Refresh();
                TextureImporter textureImporter = (TextureImporter)TextureImporter.GetAtPath(imgName.Substring(imgName.LastIndexOf("Assets")));

                if (textureImporter != null)
                {
                    textureImporter.isReadable = true;
                    textureImporter.mipmapEnabled = true;
                    textureImporter.wrapMode = TextureWrapMode.Clamp;
                    textureImporter.anisoLevel = anisotropicFilter;

                    TextureImporterPlatformSettings platformSettings = new TextureImporterPlatformSettings();
                    platformSettings.overridden = true;
                    platformSettings.format = TextureImporterFormat.Automatic;
                    platformSettings.maxTextureSize = imageResolution;
                    textureImporter.SetPlatformTextureSettings(platformSettings);

                    UnityEngine.Object asset = AssetDatabase.LoadAssetAtPath(imgName.Substring(imgName.LastIndexOf("Assets")), typeof(UnityEngine.Object)) as UnityEngine.Object;
                    AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(asset), ImportAssetOptions.ForceUpdate);
                }
            }

            AssetDatabase.Refresh();
        }

        private void DownloadFailedImageTiles (bool showNotifications)
        {
            if (failedFolder)
                CheckFailedImagesGUI();

            if (failedFolder && totalFailedImages == 0)
            {
                if (showNotifications)
                    EditorUtility.DisplayDialog("NO FAILED IMAGES", "There are no failed images in the selected folder.\n\nNote: If any of the images has been downloaded incorrectly, you can rename its filename and include \"_Temp\" at the end of the name, then finally press GET FAILED IMAGES button again to redownload.", "Ok");
                
                return;
            }

            if (textureOnFinish == 0 && !terrain && !splittedTerrains)
            {
                if (showNotifications)
                    EditorUtility.DisplayDialog("UNAVAILABLE TERRAIN", unavailableTerrainStr, "Ok");

                return;
            }

            if (failedFolder)
            {
                failedDownloading = true;
                GetPresetInfo();
                SetupImagery();

                if (cancelOperation)
                    return;

                InitializeDownloader();
                GetSatelliteImages();
            }
            else
            {
                if (showNotifications)
                    EditorUtility.DisplayDialog("FOLDER NOT AVILABLE", "No Folders Assigned, Please First Select A Folder From The Project Panel.", "Ok");

                return;
            }
        }

        //private void MoveDownloadedImages()
        //{
        //    int counterImages = 0;
        //    string finalImageStr = "";
        //
        //    if (splittedTerrains)
        //    {
        //        int counterChunks = 0;
        //        List<Vector2> terrainCells = new List<Vector2>();
        //        int terrainRowCol = (int)Mathf.Sqrt((float)terrainChunks);
        //
        //        for (int i = 0; i < terrainRowCol; i++)
        //            for (int j = 0; j < terrainRowCol; j++)
        //                terrainCells.Add(new Vector2(i + 1, j + 1));
        //
        //        for (int z = 0; z < croppedTerrains.Count; z++)
        //        {
        //            int x = (int)terrainCells[counterChunks].x;
        //            int y = (int)terrainCells[counterChunks].y;
        //
        //            for (int i = 0; i < gridPerTerrain; i++)
        //            {
        //                for (int j = 0; j < gridPerTerrain; j++)
        //                {
        //                    int row = (i + 1) + (gridPerTerrain * (x - 1));
        //                    int col = (j + 1) + (gridPerTerrain * (y - 1));
        //
        //                    string current = row.ToString() + "-" + col.ToString();
        //                    string currentStr = projectPath + "Temporary Imagery Data/" + current + ".jpg";
        //
        //                    if (!failedDownloading)
        //                    {
        //                        if (File.Exists(currentStr))
        //                        {
        //                            finalImageStr = directoryPathImagery + "/Terrain " + (counterChunks + 1) + "/" + current + ".jpg";
        //
        //                            if (File.Exists(finalImageStr))
        //                            {
        //                                File.SetAttributes(finalImageStr, FileAttributes.Normal);
        //                                File.Delete(finalImageStr);
        //                            }
        //
        //                            try
        //                            {
        //                                File.Move(currentStr, finalImageStr);
        //                                File.SetAttributes(finalImageStr, FileAttributes.Normal);
        //                            }
        //                            catch { }
        //
        //                            AssetDatabase.Refresh();
        //
        //                            if (textureOnFinish == 0 || (textureOnFinish == 1 && importAtEnd))
        //                            {
        //                                UnityEngine.Object asset = AssetDatabase.LoadAssetAtPath(finalImageStr.Substring(finalImageStr.LastIndexOf("Assets")), typeof(UnityEngine.Object)) as UnityEngine.Object;
        //                                TextureImporter textureImporter = AssetImporter.GetAtPath(AssetDatabase.GetAssetPath(asset)) as TextureImporter;
        //
        //                                if (textureImporter != null && !textureImporter.isReadable)
        //                                {
        //                                    textureImporter.isReadable = true;
        //                                    textureImporter.mipmapEnabled = true;
        //                                    textureImporter.wrapMode = TextureWrapMode.Clamp;
        //                                    textureImporter.anisoLevel = anisotropicFilter;
        //                                    textureImporter.maxTextureSize = imageResolution;
        //
        //                                    if (!compressionActive)
        //                                    {
        //                                        TextureImporterPlatformSettings platformSettings = new TextureImporterPlatformSettings();
        //                                        platformSettings.textureCompression = TextureImporterCompression.Uncompressed;
        //                                        platformSettings.format = TextureImporterFormat.Automatic;
        //                                        textureImporter.SetPlatformTextureSettings(platformSettings);
        //                                    }
        //
        //                                    EditorUtility.DisplayProgressBar("IMPORTING IMAGE", "Image  " + (counterImages + 1).ToString() + "  of  " + downloadedImageIndex.ToString(), Mathf.InverseLerp(0f, (float)(downloadedImageIndex - 1), (float)(counterImages)));
        //                                    AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(asset), ImportAssetOptions.ForceUpdate);
        //
        //                                    Texture2D satelliteImage = UnityEditor.AssetDatabase.LoadAssetAtPath(finalImageStr.Substring(finalImageStr.LastIndexOf("Assets")), typeof(Texture2D)) as Texture2D;
        //
        //                                    if (!areaIsSquare)
        //                                    {
        //                                        EditorUtility.DisplayProgressBar("CROPPING IMAGE", "Image  " + (counterImages + 1).ToString() + "  of  " + downloadedImageIndex.ToString(), Mathf.InverseLerp(0f, (float)(downloadedImageIndex - 1), (float)(counterImages)));
        //                                        satelliteImage = ImageCropper(satelliteImage, cropOffsetX, cropSizeX, cropSizeY, cropOffsetY);
        //                                    }
        //
        //                                    if (autoScale)
        //                                    {
        //                                        EditorUtility.DisplayProgressBar("SCALING IMAGE", "Image  " + (counterImages + 1).ToString() + "  of  " + downloadedImageIndex.ToString(), Mathf.InverseLerp(0f, (float)(downloadedImageIndex - 1), (float)(counterImages)));
        //                                        satelliteImage = ScaleTexture(satelliteImage, imageResolution, imageResolution);
        //                                    }
        //
        //                                    if (!areaIsSquare || compressionQuality < 100)
        //                                    {
        //                                        EditorUtility.DisplayProgressBar("ENCODING IMAGE", "Image  " + (counterImages + 1).ToString() + "  of  " + downloadedImageIndex.ToString(), Mathf.InverseLerp(0f, (float)(downloadedImageIndex - 1), (float)(counterImages)));
        //                                        byte[] bytes = satelliteImage.EncodeToJPG();
        //                                        File.WriteAllBytes(finalImageStr, bytes);
        //                                    }
        //                                }
        //                                counterImages++;
        //                            }
        //                        }
        //                        else
        //                        {
        //                            string tempImageStr = directoryPathImagery + "/Terrain " + (counterChunks + 1) + "/" + current + tempPattern + ".jpg";
        //                            File.WriteAllBytes(tempImageStr, tempImageBytes);
        //
        //                            AssetDatabase.Refresh();
        //
        //                            if (textureOnFinish == 0 || (textureOnFinish == 1 && importAtEnd))
        //                            {
        //                                UnityEngine.Object asset = AssetDatabase.LoadAssetAtPath(tempImageStr.Substring(tempImageStr.LastIndexOf("Assets")), typeof(UnityEngine.Object)) as UnityEngine.Object;
        //                                TextureImporter textureImporter = AssetImporter.GetAtPath(AssetDatabase.GetAssetPath(asset)) as TextureImporter;
        //
        //                                textureImporter.isReadable = true;
        //                                textureImporter.mipmapEnabled = true;
        //                                textureImporter.wrapMode = TextureWrapMode.Clamp;
        //                                textureImporter.anisoLevel = 0;
        //                                textureImporter.maxTextureSize = satelliteImageTemp.width;
        //
        //                                if (!compressionActive)
        //                                {
        //                                    TextureImporterPlatformSettings platformSettings = new TextureImporterPlatformSettings();
        //                                    platformSettings.textureCompression = TextureImporterCompression.Uncompressed;
        //                                    platformSettings.format = TextureImporterFormat.Automatic;
        //                                    textureImporter.SetPlatformTextureSettings(platformSettings);
        //                                }
        //
        //                                AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(asset), ImportAssetOptions.ForceUpdate);
        //                            }
        //                        }
        //                    }
        //                    else
        //                    {
        //                        if (File.Exists(currentStr))
        //                        {
        //                            finalImageStr = directoryPathImagery + "/Terrain " + (counterChunks + 1) + "/" + current + ".jpg";
        //
        //                            if (File.Exists(finalImageStr))
        //                            {
        //                                File.SetAttributes(finalImageStr, FileAttributes.Normal);
        //                                File.Delete(finalImageStr);
        //                            }
        //
        //                            File.Move(currentStr, finalImageStr);
        //                            File.SetAttributes(finalImageStr, FileAttributes.Normal);
        //
        //                            string tempImageStr = directoryPathImagery + "/Terrain " + (counterChunks + 1) + "/" + current + tempPattern + ".jpg";
        //
        //                            if (File.Exists(tempImageStr))
        //                            {
        //                                File.SetAttributes(tempImageStr, FileAttributes.Normal);
        //                                File.Delete(tempImageStr);
        //                            }
        //
        //                            AssetDatabase.Refresh();
        //
        //                            if (textureOnFinish == 0 || (textureOnFinish == 1 && importAtEnd))
        //                            {
        //                                UnityEngine.Object asset = AssetDatabase.LoadAssetAtPath(finalImageStr.Substring(finalImageStr.LastIndexOf("Assets")), typeof(UnityEngine.Object)) as UnityEngine.Object;
        //                                TextureImporter textureImporter = AssetImporter.GetAtPath(AssetDatabase.GetAssetPath(asset)) as TextureImporter;
        //
        //                                if (textureImporter != null && !textureImporter.isReadable)
        //                                {
        //                                    textureImporter.isReadable = true;
        //                                    textureImporter.mipmapEnabled = true;
        //                                    textureImporter.wrapMode = TextureWrapMode.Clamp;
        //                                    textureImporter.anisoLevel = anisotropicFilter;
        //                                    textureImporter.maxTextureSize = imageResolution;
        //
        //                                    if (!compressionActive)
        //                                    {
        //                                        TextureImporterPlatformSettings platformSettings = new TextureImporterPlatformSettings();
        //                                        platformSettings.textureCompression = TextureImporterCompression.Uncompressed;
        //                                        platformSettings.format = TextureImporterFormat.Automatic;
        //                                        textureImporter.SetPlatformTextureSettings(platformSettings);
        //                                    }
        //
        //                                    EditorUtility.DisplayProgressBar("IMPORTING IMAGE", "Image  " + (counterImages + 1).ToString() + "  of  " + downloadedImageIndex.ToString(), Mathf.InverseLerp(0f, (float)(downloadedImageIndex - 1), (float)(counterImages)));
        //                                    AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(asset), ImportAssetOptions.ForceUpdate);
        //
        //                                    Texture2D satelliteImage = AssetDatabase.LoadAssetAtPath(finalImageStr.Substring(finalImageStr.LastIndexOf("Assets")), typeof(Texture2D)) as Texture2D;
        //
        //                                    if (!areaIsSquare)
        //                                    {
        //                                        EditorUtility.DisplayProgressBar("CROPPING IMAGE", "Image  " + (counterImages + 1).ToString() + "  of  " + downloadedImageIndex.ToString(), Mathf.InverseLerp(0f, (float)(downloadedImageIndex - 1), (float)(counterImages)));
        //                                        satelliteImage = ImageCropper(satelliteImage, cropOffsetX, cropSizeX, cropSizeY, cropOffsetY);
        //                                    }
        //
        //                                    if (autoScale)
        //                                    {
        //                                        EditorUtility.DisplayProgressBar("SCALING IMAGE", "Image  " + (counterImages + 1).ToString() + "  of  " + downloadedImageIndex.ToString(), Mathf.InverseLerp(0f, (float)(downloadedImageIndex - 1), (float)(counterImages)));
        //                                        satelliteImage = ScaleTexture(satelliteImage, imageResolution, imageResolution);
        //                                    }
        //
        //                                    if (!areaIsSquare || compressionQuality < 100)
        //                                    {
        //                                        EditorUtility.DisplayProgressBar("ENCODING IMAGE", "Image  " + (counterImages + 1).ToString() + "  of  " + downloadedImageIndex.ToString(), Mathf.InverseLerp(0f, (float)(downloadedImageIndex - 1), (float)(counterImages)));
        //                                        byte[] bytes = satelliteImage.EncodeToJPG();
        //                                        File.WriteAllBytes(finalImageStr, bytes);
        //                                    }
        //                                }
        //                                counterImages++;
        //                            }
        //                        }
        //                    }
        //                }
        //            }
        //            counterChunks++;
        //        }
        //        EditorUtility.ClearProgressBar();
        //    }
        //    else
        //    {
        //        for (int i = 0; i < gridPerTerrain; i++)
        //        {
        //            for (int j = 0; j < gridPerTerrain; j++)
        //            {
        //                int row = i + 1;
        //                int col = j + 1;
        //
        //                string current = row.ToString() + "-" + col.ToString();
        //                string currentStr = projectPath + "Temporary Imagery Data/" + current + ".jpg";
        //
        //                if (!failedDownloading)
        //                {
        //                    if (File.Exists(currentStr))
        //                    {
        //                        finalImageStr = directoryPathImagery + "/" + current + ".jpg";
        //
        //                        if (File.Exists(finalImageStr))
        //                        {
        //                            File.SetAttributes(finalImageStr, FileAttributes.Normal);
        //                            File.Delete(finalImageStr);
        //                        }
        //
        //                        try
        //                        {
        //                            File.Move(currentStr, finalImageStr);
        //                            File.SetAttributes(finalImageStr, FileAttributes.Normal);
        //                        }
        //                        catch { }
        //
        //                        AssetDatabase.Refresh();
        //
        //                        if (textureOnFinish == 0 || (textureOnFinish == 1 && importAtEnd))
        //                        {
        //                            UnityEngine.Object asset = AssetDatabase.LoadAssetAtPath(finalImageStr.Substring(finalImageStr.LastIndexOf("Assets")), typeof(UnityEngine.Object)) as UnityEngine.Object;
        //                            TextureImporter textureImporter = AssetImporter.GetAtPath(AssetDatabase.GetAssetPath(asset)) as TextureImporter;
        //
        //                            if (textureImporter != null && !textureImporter.isReadable)
        //                            {
        //                                textureImporter.isReadable = true;
        //                                textureImporter.mipmapEnabled = true;
        //                                textureImporter.wrapMode = TextureWrapMode.Clamp;
        //                                textureImporter.anisoLevel = anisotropicFilter;
        //                                textureImporter.maxTextureSize = imageResolution;
        //
        //                                if (!compressionActive)
        //                                {
        //                                    TextureImporterPlatformSettings platformSettings = new TextureImporterPlatformSettings();
        //                                    platformSettings.textureCompression = TextureImporterCompression.Uncompressed;
        //                                    platformSettings.format = TextureImporterFormat.Automatic;
        //                                    textureImporter.SetPlatformTextureSettings(platformSettings);
        //                                }
        //
        //                                EditorUtility.DisplayProgressBar("IMPORTING IMAGE", "Image  " + (counterImages + 1).ToString() + "  of  " + downloadedImageIndex.ToString(), Mathf.InverseLerp(0f, (float)(downloadedImageIndex - 1), (float)(counterImages)));
        //                                AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(asset), ImportAssetOptions.ForceUpdate);
        //
        //                                Texture2D satelliteImage = AssetDatabase.LoadAssetAtPath(finalImageStr.Substring(finalImageStr.LastIndexOf("Assets")), typeof(Texture2D)) as Texture2D;
        //
        //                                if (!areaIsSquare)
        //                                {
        //                                    EditorUtility.DisplayProgressBar("CROPPING IMAGE", "Image  " + (counterImages + 1).ToString() + "  of  " + downloadedImageIndex.ToString(), Mathf.InverseLerp(0f, (float)(downloadedImageIndex - 1), (float)(counterImages)));
        //                                    satelliteImage = ImageCropper(satelliteImage, cropOffsetX, cropSizeX, cropSizeY, cropOffsetY);
        //                                }
        //
        //                                if (autoScale)
        //                                {
        //                                    EditorUtility.DisplayProgressBar("SCALING IMAGE", "Image  " + (counterImages + 1).ToString() + "  of  " + downloadedImageIndex.ToString(), Mathf.InverseLerp(0f, (float)(downloadedImageIndex - 1), (float)(counterImages)));
        //                                    satelliteImage = ScaleTexture(satelliteImage, imageResolution, imageResolution);
        //                                }
        //
        //                                if (!areaIsSquare || compressionQuality < 100)
        //                                {
        //                                    EditorUtility.DisplayProgressBar("ENCODING IMAGE", "Image  " + (counterImages + 1).ToString() + "  of  " + downloadedImageIndex.ToString(), Mathf.InverseLerp(0f, (float)(downloadedImageIndex - 1), (float)(counterImages)));
        //                                    byte[] bytes = satelliteImage.EncodeToJPG();
        //                                    File.WriteAllBytes(finalImageStr, bytes);
        //                                }
        //                            }
        //                            counterImages++;
        //                        }
        //                    }
        //                    else
        //                    {
        //                        string tempImageStr = directoryPathImagery + "/" + current + tempPattern + ".jpg";
        //                        File.WriteAllBytes(tempImageStr, tempImageBytes);
        //
        //                        AssetDatabase.Refresh();
        //
        //                        if (textureOnFinish == 0 || (textureOnFinish == 1 && importAtEnd))
        //                        {
        //                            UnityEngine.Object asset = AssetDatabase.LoadAssetAtPath(tempImageStr.Substring(tempImageStr.LastIndexOf("Assets")), typeof(UnityEngine.Object)) as UnityEngine.Object;
        //                            TextureImporter textureImporter = AssetImporter.GetAtPath(AssetDatabase.GetAssetPath(asset)) as TextureImporter;
        //
        //                            textureImporter.isReadable = true;
        //                            textureImporter.mipmapEnabled = true;
        //                            textureImporter.wrapMode = TextureWrapMode.Clamp;
        //                            textureImporter.anisoLevel = 0;
        //                            textureImporter.maxTextureSize = satelliteImageTemp.width;
        //
        //                            if (!compressionActive)
        //                            {
        //                                TextureImporterPlatformSettings platformSettings = new TextureImporterPlatformSettings();
        //                                platformSettings.textureCompression = TextureImporterCompression.Uncompressed;
        //                                platformSettings.format = TextureImporterFormat.Automatic;
        //                                textureImporter.SetPlatformTextureSettings(platformSettings);
        //                            }
        //
        //                            AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(asset), ImportAssetOptions.ForceUpdate);
        //                        }
        //                    }
        //                }
        //                else
        //                {
        //                    if (File.Exists(currentStr))
        //                    {
        //                        finalImageStr = directoryPathImagery + "/" + current + ".jpg";
        //
        //                        if (File.Exists(finalImageStr))
        //                        {
        //                            File.SetAttributes(finalImageStr, FileAttributes.Normal);
        //                            File.Delete(finalImageStr);
        //                        }
        //
        //                        File.Move(currentStr, finalImageStr);
        //                        File.SetAttributes(finalImageStr, FileAttributes.Normal);
        //
        //                        string tempImageStr = directoryPathImagery + "/" + current + tempPattern + ".jpg";
        //
        //                        if (File.Exists(tempImageStr))
        //                        {
        //                            File.SetAttributes(tempImageStr, FileAttributes.Normal);
        //                            File.Delete(tempImageStr);
        //                        }
        //
        //                        AssetDatabase.Refresh();
        //
        //                        if (textureOnFinish == 0 || (textureOnFinish == 1 && importAtEnd))
        //                        {
        //                            UnityEngine.Object asset = AssetDatabase.LoadAssetAtPath(finalImageStr.Substring(finalImageStr.LastIndexOf("Assets")), typeof(UnityEngine.Object)) as UnityEngine.Object;
        //                            TextureImporter textureImporter = AssetImporter.GetAtPath(AssetDatabase.GetAssetPath(asset)) as TextureImporter;
        //
        //                            if (textureImporter != null && !textureImporter.isReadable)
        //                            {
        //                                textureImporter.isReadable = true;
        //                                textureImporter.mipmapEnabled = true;
        //                                textureImporter.wrapMode = TextureWrapMode.Clamp;
        //                                textureImporter.anisoLevel = anisotropicFilter;
        //                                textureImporter.maxTextureSize = imageResolution;
        //
        //                                if (!compressionActive)
        //                                {
        //                                    TextureImporterPlatformSettings platformSettings = new TextureImporterPlatformSettings();
        //                                    platformSettings.textureCompression = TextureImporterCompression.Uncompressed;
        //                                    platformSettings.format = TextureImporterFormat.Automatic;
        //                                    textureImporter.SetPlatformTextureSettings(platformSettings);
        //                                }
        //
        //                                EditorUtility.DisplayProgressBar("IMPORTING IMAGE", "Image  " + (counterImages + 1).ToString() + "  of  " + downloadedImageIndex.ToString(), Mathf.InverseLerp(0f, (float)(downloadedImageIndex - 1), (float)(counterImages)));
        //                                AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(asset), ImportAssetOptions.ForceUpdate);
        //
        //                                Texture2D satelliteImage = AssetDatabase.LoadAssetAtPath(finalImageStr.Substring(finalImageStr.LastIndexOf("Assets")), typeof(Texture2D)) as Texture2D;
        //
        //                                if (!areaIsSquare)
        //                                {
        //                                    EditorUtility.DisplayProgressBar("CROPPING IMAGE", "Image  " + (counterImages + 1).ToString() + "  of  " + downloadedImageIndex.ToString(), Mathf.InverseLerp(0f, (float)(downloadedImageIndex - 1), (float)(counterImages)));
        //                                    satelliteImage = ImageCropper(satelliteImage, cropOffsetX, cropSizeX, cropSizeY, cropOffsetY);
        //                                }
        //
        //                                if (autoScale)
        //                                {
        //                                    EditorUtility.DisplayProgressBar("SCALING IMAGE", "Image  " + (counterImages + 1).ToString() + "  of  " + downloadedImageIndex.ToString(), Mathf.InverseLerp(0f, (float)(downloadedImageIndex - 1), (float)(counterImages)));
        //                                    satelliteImage = ScaleTexture(satelliteImage, imageResolution, imageResolution);
        //                                }
        //
        //                                if (!areaIsSquare || compressionQuality < 100)
        //                                {
        //                                    EditorUtility.DisplayProgressBar("ENCODING IMAGE", "Image  " + (counterImages + 1).ToString() + "  of  " + downloadedImageIndex.ToString(), Mathf.InverseLerp(0f, (float)(downloadedImageIndex - 1), (float)(counterImages)));
        //                                    byte[] bytes = satelliteImage.EncodeToJPG();
        //                                    File.WriteAllBytes(finalImageStr, bytes);
        //                                }
        //                            }
        //
        //                            counterImages++;
        //                        }
        //                    }
        //                }
        //            }
        //        }
        //
        //        EditorUtility.ClearProgressBar();
        //    }
        //
        //    AssetDatabase.Refresh();
        //}

        private void CheckHeightmapDownloaderAndRecompile()
        {
            if (!terrainGenerationstarted)
                allThreads = 0;

            Resources.UnloadUnusedAssets();
        }

        private void CheckImageDownloaderAndRecompile()
        {
            if (!imageDownloadingStarted)
                allThreads = 0;

            Resources.UnloadUnusedAssets();
        }

        private Texture2D ScaleTexture(Texture2D source, int targetWidth, int targetHeight)
        {
            Texture2D result = new Texture2D(targetWidth, targetHeight, source.format, true);
            UnityEngine.Color[] rpixels = result.GetPixels(0);
            float incX = (1.0f / (float)targetWidth);
            float incY = (1.0f / (float)targetHeight);

            for (int px = 0; px < rpixels.Length; px++)
                rpixels[px] = source.GetPixelBilinear(incX * ((float)px % targetWidth), incY * ((float)Mathf.Floor(px / targetWidth)));

            result.SetPixels(rpixels, 0);
            result.Apply();
            return result;
        }

        private string[] LogicalComparer(string filePath, string fileType)
        {
            string[] names = Directory.GetFiles(filePath, "*" + fileType, SearchOption.AllDirectories);

            //NaturalStringComparer stringComparer = new NaturalStringComparer();
            //List<string> namesList = names.ToList();
            //namesList.Sort(stringComparer);

            ns.NumericComparer ns = new ns.NumericComparer();
            Array.Sort(names, ns);

            return names;
        }

        private string[] LogicalComparer(string[] names)
        {
            //NaturalStringComparer stringComparer = new NaturalStringComparer();
            //List<string> namesList = names.ToList();
            //namesList.Sort(stringComparer);

            ns.NumericComparer ns = new ns.NumericComparer();
            Array.Sort(names, ns);

            return names;
        }

        private void PopulateFailedImages()
        {
            string[] allImageNames = new string[totalImages];
            allImageNames = LogicalComparer(directoryPathImagery, ".jpg");

            for (int i = 0; i < totalImages; i++)
            {
                int rowTemp = Mathf.CeilToInt((float)(i + 1) / (float)gridNumber);
                int columnTemp = (i + 1) - ((rowTemp - 1) * gridNumber);
                allImageNames[i] = System.IO.Path.Combine(directoryPathImagery, rowTemp.ToString() + "-" + columnTemp.ToString() + ".jpg");
            }

            string[] items = allImageNames.Select(x => x.Replace(@"/", @"\")).ToArray();
            string[] items2 = allImageNames.Select(x => x.Replace(@"/", @"\")).ToArray();
            string[] result = items.Concat(items2).ToArray();

            List<string> nonDownloadedItems = new List<string>();

            foreach (string n in result)
            {
                if (nonDownloadedItems.Contains(n))
                    nonDownloadedItems.Remove(n);
                else
                    nonDownloadedItems.Add(n);
            }

            int tempItemsNo = nonDownloadedItems.Count;

            for (int i = 0; i < tempItemsNo; i++)
            {
                string tempImageStr = nonDownloadedItems[i].Replace(".jpg", tempPattern + ".jpg");
                File.WriteAllBytes(tempImageStr, tempImageBytes);

                AssetDatabase.Refresh();

                UnityEngine.Object asset = AssetDatabase.LoadAssetAtPath(tempImageStr.Substring(tempImageStr.LastIndexOf("Assets")), typeof(UnityEngine.Object)) as UnityEngine.Object;
                TextureImporter textureImporter = AssetImporter.GetAtPath(AssetDatabase.GetAssetPath(asset)) as TextureImporter;

                textureImporter.isReadable = true;
                textureImporter.mipmapEnabled = true;
                textureImporter.wrapMode = TextureWrapMode.Clamp;
                textureImporter.anisoLevel = 0;
                //textureImporter.maxTextureSize = satelliteImageTemp.width;

                if (!compressionActive)
                {
                    TextureImporterPlatformSettings platformSettings = new TextureImporterPlatformSettings();
                    platformSettings.textureCompression = TextureImporterCompression.Uncompressed;
                    platformSettings.format = TextureImporterFormat.Automatic;
                    platformSettings.maxTextureSize = satelliteImageTemp.width;
                    textureImporter.SetPlatformTextureSettings(platformSettings);
                }

                EditorUtility.DisplayProgressBar("CREATING TEMPORARY IMAGES", "Image  " + (i + 1).ToString() + "  of  " + (tempItemsNo).ToString(), Mathf.InverseLerp(0f, (float)(tempItemsNo - 1), (float)(i)));
                AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(asset), ImportAssetOptions.ForceUpdate);
            }

            EditorUtility.ClearProgressBar();
            AssetDatabase.Refresh();
        }

        public void ImageTilerDownloader()
        {
            AssetDatabase.Refresh();
            string[] allImageNames = LogicalComparer(directoryPathImagery, ".jpg");

            if (!splittedTerrains)
            {
#if UNITY_2018_3_OR_NEWER
                TerrainLayer[] terrainLayers = new TerrainLayer[totalImages];

                for (int i = 0; i < totalImages; i++)
                {
                    // TODO: Check the following line
                    Texture2D satelliteImage = AssetDatabase.LoadAssetAtPath(allImageNames[i].Substring(allImageNames[i].LastIndexOf("Assets")), typeof(Texture2D)) as Texture2D;

                    // Texturing Terrain
                    terrainLayers[i] = new TerrainLayer();
                    string layerName = directoryPathTerrainlayers.Substring(directoryPathTerrainlayers.LastIndexOf("Assets")) + "/" + satelliteImage.name.Replace(tempPattern, "") + ".terrainlayer";
                    AssetDatabase.CreateAsset(terrainLayers[i], layerName);
                    terrainLayers[i].diffuseTexture = satelliteImage;
                    terrainLayers[i].tileSize = new Vector2(cellSizeX, cellSizeY);
                    terrainLayers[i].tileOffset = new Vector2(imageXOffset[i], imageYOffset[i]);
                }

                terrain.terrainData.terrainLayers = terrainLayers;
                terrain.terrainData.RefreshPrototypes();
                terrain.Flush();

                try
                {
                    if (terrain.terrainData.terrainLayers == null || terrain.terrainData.terrainLayers.Length == 0)
                        return;
                }
                catch { }
#else
            terrainTextures = new SplatPrototype[totalImages];
			
			for (int i = 0; i < totalImages; i++)
			{
                // TODO: Check the following line
				Texture2D satelliteImage = AssetDatabase.LoadAssetAtPath(allImageNames[i].Substring(allImageNames[i].LastIndexOf("Assets")), typeof(Texture2D)) as Texture2D;
            
				// Texturing Terrain
				terrainTextures[i] = new SplatPrototype();
				terrainTextures[i].texture = satelliteImage;
				terrainTextures[i].tileSize = new Vector2(cellSizeX, cellSizeY);
				terrainTextures[i].tileOffset = new Vector2(imageXOffset[i], imageYOffset[i]);
			}

			terrain.terrainData.splatPrototypes = terrainTextures;
			terrain.terrainData.RefreshPrototypes();
			terrain.Flush();
			
			if (terrain.terrainData.splatPrototypes.Length == 0)
				return;
#endif

                terrain.terrainData.alphamapResolution = alphamapResolution;
                splatNormalizeX = terrainSizeX / alphamapResolution;
                splatNormalizeY = terrainSizeY / alphamapResolution;

                float[] lengthz = new float[totalImages];
                float[] widthz = new float[totalImages];
                float[] lengthzOff = new float[totalImages];
                float[] widthzOff = new float[totalImages];

                for (int i = 0; i < totalImages; i++)
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
                }

                AssetDatabase.Refresh();

                terrain.terrainData.RefreshPrototypes();
                terrain.Flush();

                smData = null;

                UnityEngine.Object terrainDataAsset = terrain.terrainData;
                AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(terrainDataAsset), ImportAssetOptions.ForceUpdate);
            }
            else
            {
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
                    // Only update terrains which have failed textures in them
                    if (failedDownloading && !failedTerrainNames.Contains(terrainSplitted.name, StringComparer.OrdinalIgnoreCase))
                        continue;

#if UNITY_2018_3_OR_NEWER
                    TerrainLayer[] terrainLayers = new TerrainLayer[imagesPerTerrain];

                    for (int i = 0; i < imagesPerTerrain; i++)
                    {
                        string name = allImageNames[index2D[imageIndex]].Substring(allImageNames[index2D[imageIndex]].LastIndexOf("Assets"));
                        Texture2D satelliteImage = AssetDatabase.LoadAssetAtPath(name, typeof(Texture2D)) as Texture2D;

                        // Texturing Terrain
                        terrainLayers[i] = new TerrainLayer();
                        string layerName = directoryPathTerrainlayers.Substring(directoryPathTerrainlayers.LastIndexOf("Assets")) + "/" + satelliteImage.name.Replace(tempPattern, "") + ".terrainlayer";
                        AssetDatabase.CreateAsset(terrainLayers[i], layerName);
                        terrainLayers[i].diffuseTexture = satelliteImage;
                        terrainLayers[i].tileSize = new Vector2(cellSizeSplittedX, cellSizeSplittedY);
                        terrainLayers[i].tileOffset = new Vector2(imageXOffset[i], imageYOffset[i]);

                        imageIndex++;
                    }

                    terrainSplitted.terrainData.terrainLayers = terrainLayers;
                    terrainSplitted.terrainData.RefreshPrototypes();
                    terrainSplitted.Flush();

                    try
                    {
                        if (terrainSplitted.terrainData.terrainLayers == null || terrainSplitted.terrainData.terrainLayers.Length == 0)
                            return;
                    }
                    catch { }
#else
				terrainTextures = new SplatPrototype[imagesPerTerrain];
				
				for (int i = 0; i < imagesPerTerrain; i++)
				{
                    string name = allImageNames[index2D[imageIndex]].Substring(allImageNames[index2D[imageIndex]].LastIndexOf("Assets"));
                    Texture2D satelliteImage = AssetDatabase.LoadAssetAtPath(name, typeof(Texture2D)) as Texture2D;
						
					// Texturing Terrain
					terrainTextures[i] = new SplatPrototype();
					terrainTextures[i].texture = satelliteImage;
					terrainTextures[i].tileSize = new Vector2(cellSizeSplittedX, cellSizeSplittedY);
					terrainTextures[i].tileOffset = new Vector2(imageXOffset[i], imageYOffset[i]);

                    imageIndex++;
                }

                terrainSplitted.terrainData.splatPrototypes = terrainTextures;
				terrainSplitted.terrainData.RefreshPrototypes();
				terrainSplitted.Flush();
				
				if (terrainSplitted.terrainData.splatPrototypes.Length == 0)
					return;
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

                    smData = null;

                    UnityEngine.Object terrainDataAsset = terrainSplitted.terrainData;
                    AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(terrainDataAsset), ImportAssetOptions.ForceUpdate);

                    index++;
                }

                EditorUtility.ClearProgressBar();
            }
        }

        private void CheckFailedImages()
        {
            string[] allImageNames = LogicalComparer(directoryPathImagery, ".jpg");
            totalFailedImages = 0;

            foreach (string imageName in allImageNames)
            {
                if (imageName.Contains(tempPattern))
                {
                    failedImageAvailable = true;
                    totalFailedImages++;
                }
            }

            if (failedImageAvailable)
            {
                if (importAtEnd)
                {
                    failedFolder = AssetDatabase.LoadMainAssetAtPath(directoryPathImagery.Substring(directoryPathImagery.LastIndexOf("Assets")));
                    showFailedDownloaderSection = true;
                }
            }

            if (totalFailedImages == 0)
                failedImageAvailable = false;
        }

        private void CheckFailedImagesGUI()
        {
            directoryPathImagery = dataPath.Replace("Assets", "") + AssetDatabase.GetAssetPath(failedFolder);

            string[] names = LogicalComparer(directoryPathImagery, ".jpg");
            totalFailedImages = 0;

            foreach (string imageName in names)
            {
                if (imageName.Contains(tempPattern))
                {
                    failedImageAvailable = true;
                    totalFailedImages++;
                }
            }

            if (totalFailedImages == 0)
                failedImageAvailable = false;
        }

        private void CheckFailedHeightmapsGUIServer()
        {
            string[] names = LogicalComparer(directoryPathElevation, ".tif");

            if (names != null && names.Length > 0)
            {
                //String[] pathParts = names[0].Split(char.Parse("."));

                totalFailedHeightmaps = 0;

                foreach (string name in names)
                {
                    if (name.Contains(tempPattern))
                    {
                        failedHeightmapAvailable = true;
                        totalFailedHeightmaps++;
                    }
                }

                if (totalFailedHeightmaps == 0)
                    failedHeightmapAvailable = false;

                //            if(names[0].EndsWith(".asc") || names[0].EndsWith(".raw") || names[0].EndsWith(".tif"))
                //            {
                //                String[] pathParts = names[0].Split(char.Parse("."));
                //                elevationFormat = pathParts[pathParts.Length - 1];
                //
                //                if(elevationFormat.EndsWith("raw"))
                //                    formatIndex = 0;
                //                else if(elevationFormat.EndsWith("asc"))
                //                    formatIndex = 1;
                //                else if(elevationFormat.EndsWith("tif"))
                //                    formatIndex = 2;
                //
                //                totalFailedHeightmaps = 0;
                //
                //                foreach(string name in names)
                //                {
                //                    if(name.Contains(tempPattern))
                //                    {
                //                        failedHeightmapAvailable = true;
                //                        totalFailedHeightmaps++;
                //                    }
                //                }
                //
                //                if(totalFailedHeightmaps == 0)
                //                    failedHeightmapAvailable = false;
                //            }
                //            else
                //            {
                //                UnityEngine.Debug.LogError("UNKNOWN FORMAT - There are no valid ASCII, RAW or Tiff files in selected server's Elevation directory.");
                //                return;
                //            }
            }
            else
            {
                totalFailedHeightmaps = 0;
                failedHeightmapAvailable = false;
            }
        }

        private void CheckFailedImagesGUIServer()
        {
            string[] names = LogicalComparer(directoryPathImagery, ".jpg");
            totalFailedImages = 0;

            foreach (string imageName in names)
            {
                if (imageName.Contains(tempPattern))
                {
                    failedImageAvailable = true;
                    totalFailedImages++;
                }
            }

            if (totalFailedImages == 0)
                failedImageAvailable = false;
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

        public Texture2D ImageCropper(Texture2D source, int offsetX, int targetWidth, int targetHeight, int offsetY)
        {
            Texture2D result = new Texture2D(targetWidth, targetHeight);
            result.SetPixels(source.GetPixels(offsetX, offsetY, targetWidth, targetHeight));
            result.Apply();

            return result;
        }

        private void convertIntVarsToEnums()
        {
            switch (neighbourhoodInt)
            {
                case 0:
                    neighbourhood = Neighbourhood.Moore;
                    break;
                case 1:
                    neighbourhood = Neighbourhood.VonNeumann;
                    break;
            }
        }

        //private void CheckImageColors(string fileName)
        //{
        //    Bitmap bmp = new Bitmap(fileName);
        //
        //    // Lock the bitmap's bits.  
        //    Rectangle rect = new Rectangle(0, 0, bmp.Width, bmp.Height);
        //    BitmapData bmpData = bmp.LockBits(rect, ImageLockMode.ReadWrite, bmp.PixelFormat);
        //
        //    // Get the address of the first line.
        //    IntPtr ptr = bmpData.Scan0;
        //
        //    // Declare an array to hold the bytes of the bitmap.
        //    int bytes = bmpData.Stride * bmp.Height;
        //    byte[] rgbValues = new byte[bytes];
        //
        //    // Copy the RGB values into the array.
        //    System.Runtime.InteropServices.Marshal.Copy(ptr, rgbValues, 0, bytes);
        //
        //    allBlack = true;
        //
        //    // Scanning for non-zero bytes
        //    for (int i = 0; i < rgbValues.Length; i++)
        //    {
        //        if (rgbValues[i] != 0)
        //        {
        //            allBlack = false;
        //            break;
        //        }
        //    }
        //
        //    // Unlock the bits.
        //    bmp.UnlockBits(bmpData);
        //    bmp.Dispose();
        //}

        //private void CheckImageColors()
        //{
        //    string[] allImageNames = Directory.GetFiles(projectPath + "Temporary Imagery Data", "*.jpg", SearchOption.AllDirectories);
        //
        //    if (allImageNames != null)
        //    {
        //        Bitmap bmp = new Bitmap(allImageNames[0]);
        //
        //        // Lock the bitmap's bits.  
        //        Rectangle rect = new Rectangle(0, 0, bmp.Width, bmp.Height);
        //        BitmapData bmpData = bmp.LockBits(rect, ImageLockMode.ReadWrite, bmp.PixelFormat);
        //
        //        // Get the address of the first line.
        //        IntPtr ptr = bmpData.Scan0;
        //
        //        // Declare an array to hold the bytes of the bitmap.
        //        int bytes = bmpData.Stride * bmp.Height;
        //        byte[] rgbValues = new byte[bytes];
        //
        //        // Copy the RGB values into the array.
        //        System.Runtime.InteropServices.Marshal.Copy(ptr, rgbValues, 0, bytes);
        //
        //        allBlack = true;
        //
        //        // Scanning for non-zero bytes
        //        for (int index = 0; index < rgbValues.Length; index++)
        //        {
        //            if (rgbValues[index] != 0)
        //            {
        //                allBlack = false;
        //                break;
        //            }
        //        }
        //
        //        // Unlock the bits.
        //        bmp.UnlockBits(bmpData);
        //        bmp.Dispose();
        //    }
        //}

        private void ShowMapAndRefresh(bool checkIsOpened)
        {
            if (checkIsOpened)
            {
                mapWindowIsOpen = Resources.FindObjectsOfTypeAll<InteractiveMap>().Length;

                if (mapWindowIsOpen == 1)
                {
                    InteractiveMap.requestIndex = 0;
                    InteractiveMap.map_latlong_center = new InteractiveMap.latlong_class(double.Parse(latitudeUser), double.Parse(longitudeUser));
                    InteractiveMap.map_zoom = zoomLevel;

                    mapWindow = (InteractiveMap)GetWindow(typeof(InteractiveMap), false, "Interactive Map", true);
                    mapWindow.RequestMap();
                }
            }
            else
            {
                InteractiveMap.requestIndex = 0;
                InteractiveMap.map_latlong_center = new InteractiveMap.latlong_class(double.Parse(latitudeUser), double.Parse(longitudeUser));
                InteractiveMap.map_zoom = zoomLevel;

                mapWindow = (InteractiveMap)GetWindow(typeof(InteractiveMap), false, "Interactive Map", true);
                mapWindow.RequestMap();
            }
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
            catch
            {
            }
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

            if (projector != null && projector.material != null)
            {
                SetProjectorPosition();
                projector.material.SetColor("_Color", projectorColor);
                projector.material.SetFloat("_Power", projectorStrength);
            }

            if (automaticSunPosition)
                GetSunPosition();

            if (downloadIndexSatellite != downloadedImageIndex)
                Repaint();

            if (areaSelectionMode != 1)
            {
                AreaBounds.MetricsToBBox(double.Parse(latitudeUser), double.Parse(longitudeUser), areaSizeLat, areaSizeLon, out top, out left, out bottom, out right);

                //double destLat = 0;
                //double destLon = 0;
                //
                ////double initialBearingRadiansTop = AreaBounds.DegreesToRadians(0d);
                ////double initialBearingRadiansLeft = AreaBounds.DegreesToRadians(270d);
                ////double initialBearingRadiansBottom = AreaBounds.DegreesToRadians(180d);
                ////double initialBearingRadiansRight = AreaBounds.DegreesToRadians(90d);
                //
                //double initialBearingRadiansTop = AreaBounds.DegreesToRadians(0d);
                //double initialBearingRadiansLeft = AreaBounds.DegreesToRadians(270d);
                //double initialBearingRadiansBottom = AreaBounds.DegreesToRadians(180d);
                //double initialBearingRadiansRight = AreaBounds.DegreesToRadians(90d);
                //
                //double distanceKilometresLat = areaSizeLat / 2d;
                //double distanceKilometresLon = areaSizeLon / 2d;
                //
                //AreaBounds.FindPointAtDistanceFrom(double.Parse(latitudeUser), double.Parse(longitudeUser), initialBearingRadiansTop, distanceKilometresLat, out destLat, out destLon);
                //top = destLat.ToString();
                //
                //AreaBounds.FindPointAtDistanceFrom(double.Parse(latitudeUser), double.Parse(longitudeUser), initialBearingRadiansLeft, distanceKilometresLon, out destLat, out destLon);
                //left = destLon.ToString();
                //
                //AreaBounds.FindPointAtDistanceFrom(double.Parse(latitudeUser), double.Parse(longitudeUser), initialBearingRadiansBottom, distanceKilometresLat, out destLat, out destLon);
                //bottom = destLat.ToString();
                //
                //AreaBounds.FindPointAtDistanceFrom(double.Parse(latitudeUser), double.Parse(longitudeUser), initialBearingRadiansRight, distanceKilometresLon, out destLat, out destLon);
                //right = destLon.ToString();

                //UnityEngine.Debug.Log(top + "   " + destLat);
                //UnityEngine.Debug.Log(right + "   " + destLon);

                //UnityEngine.Debug.Log(left + "   " + destLon);
                //UnityEngine.Debug.Log(bottom + "   " + destLat);
            }
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

                presetFilePath = EditorUtility.SaveFilePanel("Save Settings As Preset File", presetsPath, address, "tlps");

                if (!string.IsNullOrEmpty(presetFilePath))
                    WritePresetFile(presetFilePath);
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
            if (!Directory.Exists(downloadsPath))
                Directory.CreateDirectory(downloadsPath);

            presetFilePath = presetsPath + "/Downloader AutoSave.tlps";
            WritePresetFile(presetFilePath);
        }

        private void AutoLoad()
        {
            presetFilePath = presetsPath + "/Downloader AutoSave.tlps";

            if (File.Exists(presetFilePath) && presetFilePath.Contains("tlps"))
                ReadPresetFile();
        }

        private void WritePresetFile(string fileName)
        {
            string preset = "Terrain Generation Settings\n"

                + "\nAddress: " + address
                + "\nLatitude: " + latitudeUser + " Degree"
                + "\nLongitude: " + longitudeUser + " Degree"

                + "\nNewTerrainGrid: " + enumValueNew
                + "\nNewTerrainSizeX: " + terrainSizeNewX
                + "\nNewTerrainSizeZ: " + terrainSizeNewZ
                + "\nConstrainedAspect: " + constrainedAspect
                + "\nNewTerrainPixelError: " + pixelError

                + "\nAreaSelectionMode: " + areaSelectionMode
                + "\nLatExtents: " + areaSizeLat
                + "\nLonExtents: " + areaSizeLon
                + "\nSquareArea: " + squareArea
                + "\nArbitraryTop: " + top
                + "\nArbitraryLeft: " + left
                + "\nArbitraryBottom: " + bottom
                + "\nArbitraryRight: " + right

                + "\nMapType: " + mapTypeIndex
                + "\nPreviewZoomLevel: " + zoomLevel

                + "\nSaveASCII: " + saveTerrainDataASCII
                + "\nSaveRAW: " + saveTerrainDataRAW
                + "\nSaveTIFF: " + saveTerrainDataTIFF

                + "\nTerrainResolution: " + heightmapResolution
                + "\nTerrainSmooth: " + smoothIterations
                + "\nElevationExaggeration: " + elevationExaggeration

                + "\nGridTerrain: " + tileGrid

                + "\nImageGridPerTerrain: " + gridPerTerrain
                + "\nImageResolution: " + imageResolution
                + "\nTextureTerrain: " + textureOnFinish
                + "\nQuality: " + compressionQuality
                + "\nImportCompression: " + compressionActive
                + "\nAutoScale: " + autoScale
                + "\nAnisotropic: " + anisotropicFilter
                + "\nAlphaResolution: " + alphamapResolution
                + "\nAsyncCalls: " + maxAsyncCalls

                + "\nEngineResolutionMode: " + engineModeIndex

                + "\nResolutionPresetSection: " + showResolutionPresetSection
                + "\nAvailableDataSection: " + showAvailableDataSection
                + "\nNewTerrainSection: " + showNewTerrainSection
                + "\nLocationSection: " + showLocationSection
                + "\nAreaSizeSection: " + showAreaSizeSection
                + "\nInteractiveMapSection: " + showInteractiveMapSection
                + "\nHeghtmapDownloaderSection: " + showHeghtmapDownloaderSection
                + "\nSaveElevationSection: " + showSaveElevationSection
                + "\nImageDownloaderSection: " + showImageDownloaderSection
                + "\nFailedDownloaderSection: " + showFailedDownloaderSection

                + "\nSplitSizeNew: " + splitSizeNew
                + "\nTotalTerrainsNew: " + totalTerrainsNew

                + "\nUpdateArea: " + updateArea
                + "\nShowArea: " + showArea
                + "\nShowCross: " + showCross
                + "\nSquareArea: " + unitsToOneMeter

                + "\nSmoothBlendIndex: " + smoothBlendIndex
                + "\nSmoothBlend: " + smoothBlend
                + "\nVisualMapsSection: " + showVisulizationMapsSection

                + "\nVisualMapIndex: " + visualMapIndex
                + "\nAspectIsActive: " + aspectIsActive
                + "\nElevationIsActive: " + elevationIsActive
                + "\nHillshadeMDIsActive: " + hillshadeMDIsActive
                + "\nSlopeIsActive: " + slopeIsActive
                + "\nVisualMapResolution: " + visualMapResolution
                + "\nVisualFormat: " + visualFormat

                + "\nSlopeZFactor: " + slopeZFactor
                + "\nHillshadeAltitude: " + hillshadeAltitude
                + "\nHillshadeAzimuth: " + hillshadeAzimuth
                + "\nHillshadeZFactor: " + hillshadeZFactor
                + "\nAutomaticSunPosition: " + automaticSunPosition

                + "\nAnalysisPreviewIndex: " + analysisPreviewIndex
                + "\nProjectorColor: " + projectorColor.r + " " + projectorColor.g + " " + projectorColor.b + " " + projectorColor.a
                + "\nProjectorStrength: " + projectorStrength

                + "\nImportAtEnd: " + importAtEnd
                + "\nServerSection: " + showServerSection
                + "\nServerGrid: " + serverGrid
                + "\nWorldMode: " + modeIndex
                + "\nformatMode: " + formatIndex;


            File.WriteAllText(fileName, preset);
        }

        private void ReadPresetFile()
        {
            try
            {
                string text = File.ReadAllText(presetFilePath);
                string[] dataLines = text.Split('\n');
                string[][] dataPairs = new string[dataLines.Length][];
                int lineNum = 0;

                foreach (string line in dataLines)
                    dataPairs[lineNum++] = line.Split(' ');

                address = "";

                for (int i = 1; i < dataPairs[2].Length; i++)
                {
                    address += dataPairs[2][i];

                    if (i < dataPairs[2].Length - 1)
                        address += " ";
                }

                latitudeUser = dataPairs[3][1];
                longitudeUser = dataPairs[4][1];

                enumValueNew = (SizeNew)Enum.Parse(typeof(SizeNew), dataPairs[5][1]);
                terrainSizeNewX = float.Parse(dataPairs[6][1]);
                terrainSizeNewZ = float.Parse(dataPairs[7][1]);

                if (dataPairs[8][1].Contains("True"))
                    constrainedAspect = true;
                else
                    constrainedAspect = false;

                pixelError = float.Parse(dataPairs[9][1]);

                areaSelectionMode = int.Parse(dataPairs[10][1]);
                areaSizeLat = float.Parse(dataPairs[11][1]);
                areaSizeLon = float.Parse(dataPairs[12][1]);

                if (dataPairs[13][1].Contains("True"))
                    squareArea = true;
                else
                    squareArea = false;

                top = dataPairs[14][1];
                left = dataPairs[15][1];
                bottom = dataPairs[16][1];
                right = dataPairs[17][1];

                mapTypeIndex = int.Parse(dataPairs[18][1]);
                zoomLevel = int.Parse(dataPairs[19][1]);

                if (dataPairs[20][1].Contains("True"))
                    saveTerrainDataASCII = true;
                else
                    saveTerrainDataASCII = false;

                if (dataPairs[21][1].Contains("True"))
                    saveTerrainDataRAW = true;
                else
                    saveTerrainDataRAW = false;

                if (dataPairs[22][1].Contains("True"))
                    saveTerrainDataTIFF = true;
                else
                    saveTerrainDataTIFF = false;

                heightmapResolution = int.Parse(dataPairs[23][1]);
                smoothIterations = int.Parse(dataPairs[24][1]);
                elevationExaggeration = float.Parse(dataPairs[25][1]);

                tileGrid = int.Parse(dataPairs[26][1]);

                gridPerTerrain = int.Parse(dataPairs[27][1]);
                imageResolution = int.Parse(dataPairs[28][1]);
                textureOnFinish = int.Parse(dataPairs[29][1]);
                compressionQuality = int.Parse(dataPairs[30][1]);

                if (dataPairs[31][1].Contains("True"))
                    compressionActive = true;
                else
                    compressionActive = false;

                if (dataPairs[32][1].Contains("True"))
                    autoScale = true;
                else
                    autoScale = false;

                anisotropicFilter = int.Parse(dataPairs[33][1]);
                alphamapResolution = int.Parse(dataPairs[34][1]);
                maxAsyncCalls = int.Parse(dataPairs[35][1]);
                engineModeIndex = int.Parse(dataPairs[36][1]);

                if (dataPairs[37][1].Contains("True"))
                    showResolutionPresetSection = true;
                else
                    showResolutionPresetSection = false;

                if (dataPairs[38][1].Contains("True"))
                    showAvailableDataSection = true;
                else
                    showAvailableDataSection = false;

                if (dataPairs[39][1].Contains("True"))
                    showNewTerrainSection = true;
                else
                    showNewTerrainSection = false;

                if (dataPairs[40][1].Contains("True"))
                    showLocationSection = true;
                else
                    showLocationSection = false;

                if (dataPairs[41][1].Contains("True"))
                    showAreaSizeSection = true;
                else
                    showAreaSizeSection = false;

                if (dataPairs[42][1].Contains("True"))
                    showInteractiveMapSection = true;
                else
                    showInteractiveMapSection = false;

                if (dataPairs[43][1].Contains("True"))
                    showHeghtmapDownloaderSection = true;
                else
                    showHeghtmapDownloaderSection = false;

                if (dataPairs[44][1].Contains("True"))
                    showSaveElevationSection = true;
                else
                    showSaveElevationSection = false;

                if (dataPairs[45][1].Contains("True"))
                    showImageDownloaderSection = true;
                else
                    showImageDownloaderSection = false;

                if (dataPairs[46][1].Contains("True"))
                    showFailedDownloaderSection = true;
                else
                    showFailedDownloaderSection = false;

                splitSizeNew = int.Parse(dataPairs[47][1]);
                totalTerrainsNew = int.Parse(dataPairs[48][1]);

                if (dataPairs[49][1].Contains("True"))
                    updateArea = true;
                else
                    updateArea = false;

                if (dataPairs[50][1].Contains("True"))
                    showArea = true;
                else
                    showArea = false;

                if (dataPairs[51][1].Contains("True"))
                    showCross = true;
                else
                    showCross = false;

                if (dataPairs[52][1].Contains("True"))
                    unitsToOneMeter = true;
                else
                    unitsToOneMeter = false;

                smoothBlendIndex = int.Parse(dataPairs[53][1]);
                smoothBlend = float.Parse(dataPairs[54][1]);

                if (dataPairs[55][1].Contains("True"))
                    showVisulizationMapsSection = true;
                else
                    showVisulizationMapsSection = false;

                visualMapIndex = int.Parse(dataPairs[56][1]);

                if (dataPairs[57][1].Contains("True"))
                    aspectIsActive = true;
                else
                    aspectIsActive = false;

                if (dataPairs[58][1].Contains("True"))
                    elevationIsActive = true;
                else
                    elevationIsActive = false;

                if (dataPairs[59][1].Contains("True"))
                    hillshadeMDIsActive = true;
                else
                    hillshadeMDIsActive = false;

                if (dataPairs[60][1].Contains("True"))
                    slopeIsActive = true;
                else
                    slopeIsActive = false;

                visualMapResolution = int.Parse(dataPairs[61][1]);

                visualFormat = (VisualFormat)Enum.Parse(typeof(VisualFormat), dataPairs[62][1]);

                slopeZFactor = float.Parse(dataPairs[63][1]);
                hillshadeAltitude = float.Parse(dataPairs[64][1]);
                hillshadeAzimuth = float.Parse(dataPairs[65][1]);
                hillshadeZFactor = float.Parse(dataPairs[66][1]);

                if (dataPairs[67][1].Contains("True"))
                    automaticSunPosition = true;
                else
                    automaticSunPosition = false;


                analysisPreviewIndex = int.Parse(dataPairs[68][1]);

                projectorColor = new UnityEngine.Color
                    (
                        float.Parse(dataPairs[69][1]),
                        float.Parse(dataPairs[69][2]),
                        float.Parse(dataPairs[69][3]),
                        float.Parse(dataPairs[69][4])
                    );

                projectorStrength = float.Parse(dataPairs[70][1]);

                if (dataPairs[71][1].Contains("True"))
                    importAtEnd = true;
                else
                    importAtEnd = false;

                if (dataPairs[72][1].Contains("True"))
                    showServerSection = true;
                else
                    showServerSection = false;

                serverGrid = (ServerGrid)Enum.Parse(typeof(ServerGrid), dataPairs[73][1]);

                modeIndex = int.Parse(dataPairs[74][1]);

                formatIndex = int.Parse(dataPairs[75][1]);
            }
            catch
            {
                UnityEngine.Debug.Log("Preset file is not valid or it's outdated!");
            }

            if (!failedDownloading)
                ShowMapAndRefresh(true);
        }

        public float[,] Convert1DTo2DEarthNormalized(float[] heights1D, int width, int height)
        {
            float[,] array2D = new float[width, height];
            int i = 0;

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    array2D[x, y] = heights1D[i] / everestPeak;
                    i++;
                }
            }

            return array2D;
        }

        public void OnInspectorUpdate()
        {
            Repaint();
        }

        #endregion
    }
}

