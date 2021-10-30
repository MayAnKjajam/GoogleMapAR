using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using MEC;

#if UNITY_EDITOR
using UnityEditor;
#endif

[DefaultExecutionOrder(-100)]
public class RuntimeOffline : MonoBehaviour
{
    public bool showStartingLocation = false;
    [Range(33, 1025)] public int previewHeightmapResolution = 1024;
    [Range(32, 4096)] public int previewSatelliteImageResolution = 1024;
    public bool reloadMap = false;
    public static int startingTileIndex;
    public static int startingTileRow;
    public static int startingTileColumn;
    public bool startFromCenter = false;
    public static string centerTileCoordsTLBR;

    public GameObject player;
    public static double centerLatitude;
    public static double centerLongitude;

    public static float areaSize;
    public static string locationName, serverInfoPath;
    public static string globalHeightmapPath, globalHeightmapPath2, globalSatelliteImagePath, globalSatelliteImagePath2;
    public static double top, left, bottom, right;
    public static double latExtent, lonExtent;

    public float sizeExaggeration = 10f;
    public static float exaggeratedWorldSize;
    public static int heightmapResolution;

    public bool drawInstanced = false;
    private static bool drawInstancedState = false;

    public bool circularLOD = true;
    [Range(1f, 200f)] public float heightmapPixelError = 5f;
    [Range(1f, 200f)] public float heightmapPixelErrorFurthest = 100f;
    [Range(1, 8)] public int centerLayersCount = 1;
    public int smoothIterations = 1;
    public static bool farTerrain = false;
    public int cellSize = 64;
    //private static bool IsCustomGeoServer = false;
    public bool delayedLOD = true;
    public static int concurrentTasks = 1;
    [HideInInspector] public bool spiralGeneration = false;
    public bool showTileOnFinish = true;
    public static int imageResolution;
    //private bool progressiveTexturing = true;
    public float elevationExaggeration = 1;

    //public bool stitchTerrainTiles = true;
    [HideInInspector] public bool stitchTerrainTiles = false;
    [HideInInspector] [Range(5, 100)] public int levelSmooth = 5;
    [HideInInspector] [Range(1, 7)] public int power = 1;
    [HideInInspector] [Range(1, 32)] public int stitchDistance = 1;

    public string serverPath = "C:/Users/Amir/Desktop/TerraLand_GeoServer"; // "http://terraunity.com/freedownload/TerraLand_GeoServer";
    public static string dataBasePath;
    public bool projectRootPath = true;
    
    //android Build
    public bool androidBuild = false;
    //android Build

    public bool elevationOnly = false;
    [HideInInspector] public bool progressiveGeneration = false;

    //private int counterNorth = 0;
    //private int counterSouth = 0;
    //private int counterEast = 0;
    //private int counterWest = 0;

    [HideInInspector] public float terrainDistance = 1000f;

    public bool terrainColliders = false;
    public bool fastStartBuild = true;

    public static bool tiledElevation;

    [Range(4, 32)] public int activeTilesGrid = 4;
    public static int totalTiles;
    public static int dataBaseTiles;
    public static int dataBaseGrid;
    public static int padStartX;
    public static int padStartY;
    public static int padEndX;
    public static int padEndY;

    public static string[] elevationTileNames;
    public static string[] imageryTileNames;
    public static string[] normalTileNames;

    public float delayBetweenConnections;
    public float elevationDelay = 0.5f;
    public float imageryDelay = 0.5f;
    //public float stitchDelay = 0.25f;

    public Material terrainMaterial;
    public bool enableDetailTextures = true;
    public Texture2D detailTexture;
    public Texture2D detailNormal;
    //public Texture2D detailNormalFar;
    [Range(0, 100)] public float detailBlending = 25f;
    public float detailTileSize = 25f;

    [HideInInspector] public bool asyncImageLoading = true;

#if UNITY_EDITOR
    private TextureImporter imageImport;
#endif

    private Texture2D[] detailTextures;
    private SplatPrototype[] terrainTextures;
    private SplatPrototype currentSplatPrototye;
    private List<Terrain> terrains;
    private int startIndex;
    private int texturesNO;
    private int length;
    private float[,,] smData;
    private int index;
    private int filteredIndex;
    private String[] pathParts;
    public static int northCounter = 0;
    public static int southCounter = 0;
    public static int eastCounter = 0;
    public static int westCounter = 0;

    public bool enableSplatting;
    public Texture2D layer1Albedo;
    public Texture2D layer1Normal;
    public int tiling1;
    public Texture2D layer2Albedo;
    public Texture2D layer2Normal;
    public int tiling2;
    public Texture2D layer3Albedo;
    public Texture2D layer3Normal;
    public int tiling3;
    public Texture2D layer4Albedo;
    public Texture2D layer4Normal;
    public int tiling4;

    private IEnumerable<string> imageryNames;
    private IEnumerable<string> normalNames;
    [HideInInspector] public bool normalsAvailable;
    [HideInInspector] public bool imageryAvailable;

    private static string serverError;
    public Text notificationText;

    public static Timing timing;
    public static bool updatingSurfaceNORTH;
    public static bool updatingSurfaceSOUTH;
    public static bool updatingSurfaceEAST;
    public static bool updatingSurfaceWEST;

    public static float hiddenTerrainsBelowUnits = 100000f;

    public StreamingAssetsManager streamingAssets;
    [HideInInspector] public List<Terrain> processedTiles;

    public float activeDistance = 10000f;
    public bool isStreamingAssets = false;

    public static float worldPositionOffsetX;
    public static float worldPositionOffsetY;

    public static bool isGeoReferenced;
    private static string sceneName;

    #region multithreading variables

    int maxThreads = 50;
    private int numThreads;
    private int _count;

    private bool m_HasLoaded = false;

    private List<Action> _actions;
    private List<DelayedQueueItem> _delayed;

    private List<DelayedQueueItem> _currentDelayed;
    private List<Action> _currentActions;

    public struct DelayedQueueItem
    {
        public float time;
        public Action action;
    }

    #endregion


    void Start()
    {
        showStartingLocation = false;

        if (Application.isPlaying)
        {
            _actions = new List<Action>();
            _delayed = new List<DelayedQueueItem>();
            _currentDelayed = new List<DelayedQueueItem>();
            _currentActions = new List<Action>();
            m_HasLoaded = true;

            SetupServer();
            TerraLand.TerraLandRuntimeOffline.Initialize();
            //OfflineStreaming.Initialize();
        }
    }

    private void CheckDetailTextures()
    {
#if UNITY_EDITOR
        detailTextures = new Texture2D[2] { detailTexture, detailNormal };

        foreach (Texture2D currentImage in detailTextures)
        {
            imageImport = AssetImporter.GetAtPath(AssetDatabase.GetAssetPath(currentImage)) as TextureImporter;

            if (imageImport != null && !imageImport.isReadable)
            {
                imageImport.isReadable = true;
                AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(currentImage), ImportAssetOptions.ForceUpdate);
            }
        }
#endif
    }

    void Update()
    {
        if (Application.isPlaying)
        {
            if (m_HasLoaded == false)
                Start();

            lock (_actions)
            {
                _currentActions.Clear();
                _currentActions.AddRange(_actions);
                _actions.Clear();
            }

            foreach (var a in _currentActions)
                a();

            lock (_delayed)
            {
                _currentDelayed.Clear();
                _currentDelayed.AddRange(_delayed.Where(d => d.time <= Time.time));

                foreach (var item in _currentDelayed)
                    _delayed.Remove(item);
            }

            foreach (var delayed in _currentDelayed)
                delayed.action();

            if (GameObject.Find("Movement Effects") != null && timing == null)
                timing = GameObject.Find("Movement Effects").GetComponent<Timing>();

            if (TerraLand.TerraLandRuntimeOffline.worldIsGenerated)
            {
                if (enableDetailTextures)
                    AddDetailTexturesToTerrains();

                //if(timing.UpdateCoroutines == 0)
                //{
                //    if (updatingSurfaceNORTH)
                //    {
                //        TerraLand.TerraLandRuntimeOffline.ManageNeighborings("North");
                //        updatingSurfaceNORTH = false;
                //    }
                //    else if (updatingSurfaceSOUTH)
                //    {
                //        TerraLand.TerraLandRuntimeOffline.ManageNeighborings("South");
                //        updatingSurfaceSOUTH = false;
                //    }
                //    else if (updatingSurfaceEAST)
                //    {
                //        TerraLand.TerraLandRuntimeOffline.ManageNeighborings("East");
                //        updatingSurfaceEAST = false;
                //    }
                //    else if (updatingSurfaceWEST)
                //    {
                //        TerraLand.TerraLandRuntimeOffline.ManageNeighborings("West");
                //        updatingSurfaceWEST = false;
                //    }
                //}
            }
        }
    }

#if UNITY_EDITOR
    private void OnValidate ()
    {
        if (showStartingLocation)
        {
            SetupServer();

            if (isGeoReferenced)
                EditorApplication.ExecuteMenuItem("Tools/TerraUnity/TerraLand/Streaming Map");
            else
                EditorUtility.DisplayDialog("NON GEO-REFERENCED SERVER", "This option is not available for non geo-referenced servers!", "Ok");

            showStartingLocation = false;
        }

        if (activeTilesGrid % 2 != 0) activeTilesGrid = activeTilesGrid + 1;
        if (!Mathf.IsPowerOfTwo(previewHeightmapResolution)) previewHeightmapResolution = Mathf.ClosestPowerOfTwo(previewHeightmapResolution) + 1;
        if (!Mathf.IsPowerOfTwo(previewSatelliteImageResolution)) previewSatelliteImageResolution = Mathf.ClosestPowerOfTwo(previewSatelliteImageResolution);

        if (drawInstancedState != drawInstanced)
        {
            //f (drawInstanced) EditorUtility.DisplayDialog("ALWAYS INCLUDED SHADERS", "Make sure to add Nature/Terrain/Standard shader in Project Settings => Graphics => Always Included Shaders list\n\nAnd under Shader Stripping select Keep All in Instancing Variants drop down menu.", "Ok");
            if (drawInstanced) Debug.Log("ALWAYS INCLUDED SHADERS, Make sure to add Nature/Terrain/Standard shader in Project Settings => Graphics => Always Included Shaders list\n\nAnd under Shader Stripping select Keep All in Instancing Variants drop down menu.");
            drawInstancedState = drawInstanced;
        }
    }
#endif

    public void SetupServer ()
    {
        sceneName = SceneManager.GetActiveScene().name;

        TerraLand.TerraLandRuntimeOffline.runTime = this;
        InfiniteTerrainOffline.runTime = this;
        OfflineStreaming.runtime = this;

        progressiveGeneration = false;
        spiralGeneration = false;
        concurrentTasks = 1;
        stitchDistance = 1;
        asyncImageLoading = true;

        //android Build
        if (androidBuild)
        {
#if UNITY_EDITOR
            if (projectRootPath)
                dataBasePath = Application.dataPath.Replace("Assets", "") + serverPath;
            else
                dataBasePath = serverPath;
#else
            dataBasePath = Application.persistentDataPath + "/" + serverPath;
#endif
        }
        else
        {
            if (projectRootPath)
            {
#if UNITY_EDITOR
                dataBasePath = Application.dataPath.Replace("Assets", "") + serverPath;
#else
                dataBasePath = Application.dataPath + "/" + serverPath;
#endif
            }
            else
                dataBasePath = serverPath;
        }
        //android Build

        if (!Directory.Exists(dataBasePath))
        {
            //serverError = "Server Directory Not Found!\n\nDownload sample servers from links in ReadMe file next to scene file\nDataBasePath parameter in RuntimeOffline script must be typed correctly\nApplication will quit now";
            serverError = dataBasePath;
            UnityEngine.Debug.LogError(serverError);
            notificationText.text = serverError;
            StartCoroutine(StopApplication());
            return;
        }
        else
        {
            serverError = "";
           // notificationText.text = serverError;
        }

        TerraLand.TerraLandRuntimeOffline.dataBasePathElevation = dataBasePath + "/Elevation/";
        TerraLand.TerraLandRuntimeOffline.dataBasePathImagery = dataBasePath + "/Imagery/"; // "/Imagery/512/64/"; // 1 4 16 64 256 1024
        TerraLand.TerraLandRuntimeOffline.dataBasePathNormals = dataBasePath + "/Normals/";

        if (!Directory.Exists(TerraLand.TerraLandRuntimeOffline.dataBasePathElevation))
        {
            serverError = "Server's Elevation Directory Not Found!\n\nElevation directory in server is not available\nApplication will quit now";
            UnityEngine.Debug.LogError(serverError);
            notificationText.text = serverError;
            StartCoroutine(StopApplication());
            return;
        }
        else
        {
            serverError = "";
            notificationText.text = serverError;
        }

        if (!elevationOnly && !Directory.Exists(TerraLand.TerraLandRuntimeOffline.dataBasePathImagery))
        {
            serverError = "Server's Imagery Directory Not Found!\n\nImagery directory in server is not available but texturing is activated!\nApplication will quit now";
            UnityEngine.Debug.LogError(serverError);
            notificationText.text = serverError;
            StartCoroutine(StopApplication());
            return;
        }
        else
        {
            serverError = "";
            notificationText.text = serverError;
        }

        IEnumerable<string> elevationNames = Directory.GetFiles(TerraLand.TerraLandRuntimeOffline.dataBasePathElevation, "*.*", SearchOption.AllDirectories)
            .Where(s => s.EndsWith(".asc")
                || s.EndsWith(".raw")
                || s.EndsWith(".tif"));

        if (Directory.Exists(TerraLand.TerraLandRuntimeOffline.dataBasePathImagery))
        {
            imageryNames = Directory.GetFiles(TerraLand.TerraLandRuntimeOffline.dataBasePathImagery, "*.*", SearchOption.AllDirectories)
            .Where
            (
                s => s.EndsWith(".jpg")
                || s.EndsWith(".png")
                || s.EndsWith(".gif")
                || s.EndsWith(".bmp")
                || s.EndsWith(".tga")
                || s.EndsWith(".psd")
                || s.EndsWith(".tiff")
                || s.EndsWith(".iff")
                || s.EndsWith(".pict")
            );

            imageryAvailable = true;
        }
        else
            imageryAvailable = false;

        if (Directory.Exists(TerraLand.TerraLandRuntimeOffline.dataBasePathNormals))
        {
            normalNames = Directory.GetFiles(TerraLand.TerraLandRuntimeOffline.dataBasePathNormals, "*.*", SearchOption.AllDirectories)
            .Where
            (
                s => s.EndsWith(".jpg")
                || s.EndsWith(".png")
                || s.EndsWith(".gif")
                || s.EndsWith(".bmp")
                || s.EndsWith(".tga")
                || s.EndsWith(".psd")
                || s.EndsWith(".tiff")
                || s.EndsWith(".iff")
                || s.EndsWith(".pict")
            );

            normalsAvailable = true;
        }
        else
            normalsAvailable = false;

        string infoFilePathT = Path.GetFullPath(dataBasePath) + "/Info/Terrain Info.tlps";

        if(File.Exists(infoFilePathT))
        {
            ServerInfo.GetServerCoords(dataBasePath, out locationName, out serverInfoPath, out globalHeightmapPath, out globalHeightmapPath2, out globalSatelliteImagePath, out globalSatelliteImagePath2, out top, out left, out bottom, out right, out latExtent, out lonExtent, out areaSize);

            if (PlayerPrefs.HasKey(sceneName + "_TileCenterLat"))
                centerLatitude = double.Parse(PlayerPrefs.GetString(sceneName + "_TileCenterLat"));
            else
                centerLatitude = bottom + ((top - bottom) * 0.5d);

            if (PlayerPrefs.HasKey(sceneName + "_TileCenterLon"))
                centerLongitude = double.Parse(PlayerPrefs.GetString(sceneName + "_TileCenterLon"));
            else
                centerLongitude = left + ((right - left) * 0.5d);

            isGeoReferenced = true;
        }
        else
        {
            /// If server is not geo-referenced then manually set a world size in kilometers. You can change it to your actual area size
            /// In World Machine demo, the world size is considered to be 480km2, so the calculation of the final area size in scene is as follows
            areaSize = 480 / sizeExaggeration;
            isGeoReferenced = false;
        }

        dataBaseTiles = elevationNames.ToArray().Length;
        dataBaseGrid = (int)(Mathf.Sqrt(dataBaseTiles));

        elevationTileNames = new string[dataBaseTiles];
        elevationTileNames = elevationNames.ToArray();
        elevationTileNames = TerraLand.TerraLandRuntimeOffline.LogicalComparer(elevationTileNames);

        if (imageryAvailable)
        {
            imageryTileNames = new string[dataBaseTiles];
            imageryTileNames = imageryNames.ToArray();
            imageryTileNames = TerraLand.TerraLandRuntimeOffline.LogicalComparer(imageryTileNames);

            if (imageryTileNames.Length == 0)
            {
                elevationOnly = true;
                enableDetailTextures = false;
            }
        }

        if (normalsAvailable && normalNames.ToArray().Length > 0)
        {
            normalTileNames = new string[dataBaseTiles];
            normalTileNames = normalNames.ToArray();
            normalTileNames = TerraLand.TerraLandRuntimeOffline.LogicalComparer(normalTileNames);
        }

        if (activeTilesGrid > dataBaseGrid)
            activeTilesGrid = dataBaseGrid;

        if (heightmapPixelErrorFurthest < heightmapPixelError)
            heightmapPixelErrorFurthest = heightmapPixelError;

        if (activeTilesGrid <= 4)
            centerLayersCount = 1;

        if (dataBaseTiles > 1)
            tiledElevation = true;
        else
            tiledElevation = false;

        totalTiles = (int)(Mathf.Pow(activeTilesGrid, 2));

        processedTiles = new List<Terrain>();

        if (tiledElevation)
            TerraLand.TerraLandRuntimeOffline.terrainChunks = activeTilesGrid;
        else
            TerraLand.TerraLandRuntimeOffline.terrainChunks = dataBaseTiles;

        TerraLand.TerraLandRuntimeOffline.croppedTerrains = new List<Terrain>();
        TerraLand.TerraLandRuntimeOffline.spiralIndex = new List<int>();
        TerraLand.TerraLandRuntimeOffline.spiralCell = new List<Vector2>();
        TerraLand.TerraLandRuntimeOffline.images = new List<Texture2D>();
        TerraLand.TerraLandRuntimeOffline.imageBytes = new List<byte[]>();

        terrainDistance = 1000000f;

        exaggeratedWorldSize = areaSize * sizeExaggeration;
        float normalizedPercentage = 1f - (((float)dataBaseGrid - (float)activeTilesGrid) / (float)dataBaseGrid);
        TerraLand.TerraLandRuntimeOffline.areaSizeLat = exaggeratedWorldSize * normalizedPercentage;
        TerraLand.TerraLandRuntimeOffline.areaSizeLon = exaggeratedWorldSize * normalizedPercentage;
        TerraLand.TerraLandRuntimeOffline.terrainSizeNewX = TerraLand.TerraLandRuntimeOffline.areaSizeLon * 1000f;
        TerraLand.TerraLandRuntimeOffline.terrainSizeNewY = 100;
        TerraLand.TerraLandRuntimeOffline.terrainSizeNewZ = TerraLand.TerraLandRuntimeOffline.areaSizeLat * 1000f;
        TerraLand.TerraLandRuntimeOffline.terrainSizeFactor = TerraLand.TerraLandRuntimeOffline.areaSizeLat / TerraLand.TerraLandRuntimeOffline.areaSizeLon;
        TerraLand.TerraLandRuntimeOffline.generatedTerrainsCount = 0;
        TerraLand.TerraLandRuntimeOffline.taskIndex = concurrentTasks;
        TerraLand.TerraLandRuntimeOffline.concurrentUpdates = 0;

        if (PlayerPrefs.HasKey(sceneName + "_TileRow"))
            startingTileRow = PlayerPrefs.GetInt(sceneName + "_TileRow");
        else
            startingTileRow = dataBaseGrid / 2;

        if (PlayerPrefs.HasKey(sceneName + "_TileColumn"))
            startingTileColumn = PlayerPrefs.GetInt(sceneName + "_TileColumn");
        else
            startingTileColumn = dataBaseGrid / 2;

        if (PlayerPrefs.HasKey(sceneName + "_TileIndex"))
            startingTileIndex = PlayerPrefs.GetInt(sceneName + "_TileIndex");
        else
            startingTileIndex = ((startingTileRow - 1) * dataBaseGrid) + startingTileColumn;

        // If in corners, offset center tile to be in bounds range
        if (startingTileRow < activeTilesGrid / 2)
            startingTileRow = (activeTilesGrid / 2);

        if (startingTileColumn < activeTilesGrid / 2)
            startingTileColumn = (activeTilesGrid / 2);

        if (startingTileRow > dataBaseGrid - (activeTilesGrid / 2))
            startingTileRow = dataBaseGrid - (activeTilesGrid / 2);

        if (startingTileColumn > dataBaseGrid - (activeTilesGrid / 2))
            startingTileColumn = dataBaseGrid - (activeTilesGrid / 2);

        padStartX = startingTileRow - (activeTilesGrid / 2); // (dataBaseGrid - activeTilesGrid) / 2
        padStartY = startingTileColumn - (activeTilesGrid / 2); // padStartX
        padEndX = dataBaseGrid - (padStartX + activeTilesGrid); // dataBaseGrid - (padStartX + activeTilesGrid)
        padEndY = dataBaseGrid - (padStartY + activeTilesGrid); // padEndX

        int centerTile = dataBaseGrid / 2;
        int tilesFromCenterX = centerTile - startingTileColumn;
        int tilesFromCenterY = centerTile - startingTileRow;
        float worldSize = exaggeratedWorldSize * 1000f;
        float tileWorldSize = worldSize / dataBaseGrid;

        worldPositionOffsetX = tilesFromCenterX * tileWorldSize;
        worldPositionOffsetY = -(tilesFromCenterY * tileWorldSize);

        TerraLand.TerraLandRuntimeOffline.elevationNames = new string[(int)Mathf.Pow(activeTilesGrid, 2)];
        TerraLand.TerraLandRuntimeOffline.imageryNames = new string[(int)Mathf.Pow(activeTilesGrid, 2)];

        if (normalsAvailable)
            TerraLand.TerraLandRuntimeOffline.normalNames = new string[(int)Mathf.Pow(activeTilesGrid, 2)];

        GetElevationInfo();

        if (!elevationOnly)
            GetImageryInfo();

        CheckDetailTextures();

        if (streamingAssets != null && streamingAssets.enabled && streamingAssets.gameObject.activeSelf)
            isStreamingAssets = true;
        else
            isStreamingAssets = false;
    }

    private IEnumerator StopApplication()
    {
        yield return new WaitForSeconds(10);

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    public void GetElevationInfo()
    {
        if (dataBaseTiles == 0)
        {
            UnityEngine.Debug.LogError("NO AVILABLE DATA - No elevation data is available in selected folder.");
            return;
        }
        else
        {
            index = 0;
            filteredIndex = 0;

            for (int i = 0; i < dataBaseGrid; i++)
            {
                for (int j = 0; j < dataBaseGrid; j++)
                {
                    if (i > padStartX - 1 && i < (dataBaseGrid - padEndX) && j > padStartY - 1 && j < (dataBaseGrid - padEndY))
                    {
                        TerraLand.TerraLandRuntimeOffline.elevationNames[index] = elevationTileNames[filteredIndex];
                        index++;
                    }

                    filteredIndex++;
                }
            }

            if (TerraLand.TerraLandRuntimeOffline.elevationNames[0].EndsWith(".asc") || TerraLand.TerraLandRuntimeOffline.elevationNames[0].EndsWith(".raw") || TerraLand.TerraLandRuntimeOffline.elevationNames[0].EndsWith(".tif"))
            {
                pathParts = TerraLand.TerraLandRuntimeOffline.elevationNames[0].Split(char.Parse("."));
                TerraLand.TerraLandRuntimeOffline.geoDataExtensionElevation = pathParts[pathParts.Length - 1];

                TerraLand.TerraLandRuntimeOffline.GetElevationFileInfo();
                TerraLand.TerraLandRuntimeOffline.tileResolution = Mathf.ClosestPowerOfTwo(heightmapResolution / dataBaseGrid) + 1;
                TerraLand.TerraLandRuntimeOffline.heightmapResolutionSplit = heightmapResolution / (int)Mathf.Sqrt(TerraLand.TerraLandRuntimeOffline.terrainChunks);

                if (cellSize > TerraLand.TerraLandRuntimeOffline.tileResolution)
                    cellSize = TerraLand.TerraLandRuntimeOffline.tileResolution - 1;
            }
            else
            {
                UnityEngine.Debug.LogError("UNKNOWN FORMAT - There are no valid ASCII, RAW or Tiff files in selected folder.");
                return;
            }
        }
    }

    public void GetImageryInfo()
    {
        index = 0;
        filteredIndex = 0;

        for (int i = 0; i < dataBaseGrid; i++)
        {
            for (int j = 0; j < dataBaseGrid; j++)
            {
                if (i > padStartX - 1 && i < (dataBaseGrid - padEndX) && j > padStartY - 1 && j < (dataBaseGrid - padEndY))
                {
                    TerraLand.TerraLandRuntimeOffline.imageryNames[index] = imageryTileNames[filteredIndex];

                    if (normalsAvailable)
                        TerraLand.TerraLandRuntimeOffline.normalNames[index] = normalTileNames[filteredIndex];

                    index++;
                }

                filteredIndex++;
            }
        }

        TerraLand.TerraLandRuntimeOffline.totalImagesDataBase = dataBaseTiles;

        if (TerraLand.TerraLandRuntimeOffline.terrainChunks > 1)
        {
            TerraLand.TerraLandRuntimeOffline.multipleTerrainsTiling = true;

            if (tiledElevation)
                TerraLand.TerraLandRuntimeOffline.imagesPerTerrain = (int)((float)activeTilesGrid / (float)TerraLand.TerraLandRuntimeOffline.terrainChunks);
            else
                TerraLand.TerraLandRuntimeOffline.imagesPerTerrain = (int)((float)TerraLand.TerraLandRuntimeOffline.totalImagesDataBase / (float)TerraLand.TerraLandRuntimeOffline.terrainChunks);

            TerraLand.TerraLandRuntimeOffline.tileGrid = (int)(Mathf.Sqrt((float)TerraLand.TerraLandRuntimeOffline.imagesPerTerrain));

            //if(!allDatabase)
            TerraLand.TerraLandRuntimeOffline.splitSizeFinal = activeTilesGrid;
            //else
            //TerraLand.TerraLandRuntimeOffline.splitSizeFinal = (int)Mathf.Sqrt(TerraLand.TerraLandRuntimeOffline.terrainChunks);

            TerraLand.TerraLandRuntimeOffline.totalImages = (int)(Mathf.Pow(TerraLand.TerraLandRuntimeOffline.gridPerTerrain, 2)) * TerraLand.TerraLandRuntimeOffline.terrainChunks;
            TerraLand.TerraLandRuntimeOffline.chunkImageResolution = (RuntimeOffline.imageResolution * (int)Mathf.Sqrt(TerraLand.TerraLandRuntimeOffline.totalImages)) / (int)Mathf.Sqrt((float)TerraLand.TerraLandRuntimeOffline.terrainChunks);
        }
        else
        {
            TerraLand.TerraLandRuntimeOffline.multipleTerrainsTiling = false;
            TerraLand.TerraLandRuntimeOffline.tileGrid = (int)(Mathf.Sqrt((float)TerraLand.TerraLandRuntimeOffline.totalImagesDataBase));
            TerraLand.TerraLandRuntimeOffline.terrainSizeX = TerraLand.TerraLandRuntimeOffline.terrainSizeNewX;
            TerraLand.TerraLandRuntimeOffline.terrainSizeY = TerraLand.TerraLandRuntimeOffline.terrainSizeNewZ;
        }

        if (TerraLand.TerraLandRuntimeOffline.totalImagesDataBase == 0)
        {
            TerraLand.TerraLandRuntimeOffline.geoImagesOK = false;
            UnityEngine.Debug.LogError("There are no images in data base!");
            return;
        }
        else
            TerraLand.TerraLandRuntimeOffline.geoImagesOK = true;

        if (TerraLand.TerraLandRuntimeOffline.terrainChunks > TerraLand.TerraLandRuntimeOffline.totalImagesDataBase)
        {
            TerraLand.TerraLandRuntimeOffline.geoImagesOK = false;
            UnityEngine.Debug.LogError("No sufficient images to texture terrains. Select a lower Grid Size for terrains");
            return;
        }
        else
            TerraLand.TerraLandRuntimeOffline.geoImagesOK = true;

        if (TerraLand.TerraLandRuntimeOffline.geoImagesOK)
        {
            //android Build
            Vector2Int imageDimensions = GetJpegImageSize(TerraLand.TerraLandRuntimeOffline.imageryNames[0]);
            TerraLand.TerraLandRuntimeOffline.imageWidth = imageDimensions.x;
            TerraLand.TerraLandRuntimeOffline.imageHeight = imageDimensions.y;
            imageResolution = TerraLand.TerraLandRuntimeOffline.imageWidth;

            //using (Image sourceImage = Image.FromFile(TerraLand.TerraLandRuntimeOffline.imageryNames[0]))
            //{
            //    TerraLand.TerraLandRuntimeOffline.imageWidth = sourceImage.Width;
            //    TerraLand.TerraLandRuntimeOffline.imageHeight = sourceImage.Height;
            //    imageResolution = TerraLand.TerraLandRuntimeOffline.imageWidth;
            //}
            //android Build

            //for(int i = 0; i < totalTiles; i++)
            //{
            //    TerraLand.TerraLandRuntimeOffline.images.Add(new Texture2D(TerraLand.TerraLandRuntimeOffline.imageWidth, TerraLand.TerraLandRuntimeOffline.imageHeight, TextureFormat.RGB24, true, true));
            //    TerraLand.TerraLandRuntimeOffline.images[i].wrapMode = TextureWrapMode.Clamp;
            //    TerraLand.TerraLandRuntimeOffline.images[i].name = (i + 1).ToString();
            //    TerraLand.TerraLandRuntimeOffline.LoadImageData(TerraLand.TerraLandRuntimeOffline.imageryNames[i]);
            //}
        }
    }

    //android Build
    public static Vector2Int GetJpegImageSize(string filename)
    {
        FileStream stream = null;
        BinaryReader rdr = null;

        try
        {
            stream = File.OpenRead(filename);
            rdr = new BinaryReader(stream);

            // keep reading packets until we find one that contains Size info
            for (;;)
            {
                byte code = rdr.ReadByte();

                if (code != 0xFF)
                    throw new ApplicationException("Unexpected value in file " + filename);

                code = rdr.ReadByte();

                switch (code)
                {
                    // filler byte
                    case 0xFF:
                        stream.Position--;
                        break;
                    // packets without data
                    case 0xD0:
                    case 0xD1:
                    case 0xD2:
                    case 0xD3:
                    case 0xD4:
                    case 0xD5:
                    case 0xD6:
                    case 0xD7:
                    case 0xD8:
                    case 0xD9:
                        break;
                    // packets with size information
                    case 0xC0:
                    case 0xC1:
                    case 0xC2:
                    case 0xC3:
                    case 0xC4:
                    case 0xC5:
                    case 0xC6:
                    case 0xC7:
                    case 0xC8:
                    case 0xC9:
                    case 0xCA:
                    case 0xCB:
                    case 0xCC:
                    case 0xCD:
                    case 0xCE:
                    case 0xCF:
                        ReadBEUshort(rdr);
                        rdr.ReadByte();
                        ushort h = ReadBEUshort(rdr);
                        ushort w = ReadBEUshort(rdr);
                        return new Vector2Int(w, h);
                    // irrelevant variable-length packets
                    default:
                        int len = ReadBEUshort(rdr);
                        stream.Position += len - 2;
                        break;
                }
            }
        }
        finally
        {
            if (rdr != null) rdr.Close();
            if (stream != null) stream.Close();
        }
    }

    private static ushort ReadBEUshort(BinaryReader rdr)
    {
        ushort hi = rdr.ReadByte();
        hi <<= 8;
        ushort lo = rdr.ReadByte();
        return (ushort)(hi | lo);
    }
    //android Build

    private void AddDetailTexturesToTerrains()
    {
        terrains = TerraLand.TerraLandRuntimeOffline.croppedTerrains;

        foreach (Terrain t in terrains)
            AddDetailTextures(t, detailBlending);
    }

    private void AddDetailTextures(Terrain terrain, float blend)
    {
        int startIndex = 0;

#if UNITY_2018_3_OR_NEWER
        try
        {
            if (terrain.terrainData.terrainLayers != null && terrain.terrainData.terrainLayers.Length > 0)
                startIndex = terrain.terrainData.terrainLayers.Length;
            else
                startIndex = 0;
        }
        catch
        {
            startIndex = 0;
        }

        TerrainLayer[] terrainLayers = new TerrainLayer[startIndex + 1];
#else
        startIndex = terrain.terrainData.splatPrototypes.Length;
        SplatPrototype[] terrainTextures = new SplatPrototype[startIndex + 1];
#endif

        for (int i = 0; i < startIndex + 1; i++)
        {
            try
            {
                if (i < startIndex)
                {
#if UNITY_2018_3_OR_NEWER
                    TerrainLayer currentLayer = terrain.terrainData.terrainLayers[i];

                    terrainLayers[i] = new TerrainLayer();
                    if (currentLayer.diffuseTexture != null) terrainLayers[i].diffuseTexture = currentLayer.diffuseTexture;

                    if (detailNormal != null)
                    {
                        terrainLayers[i].normalMapTexture = detailNormal;
                        terrainLayers[i].normalMapTexture.Apply();
                    }

                    terrainLayers[i].tileSize = new Vector2(currentLayer.tileSize.x, currentLayer.tileSize.y);
                    terrainLayers[i].tileOffset = new Vector2(currentLayer.tileOffset.x, currentLayer.tileOffset.y);
                }
                else
                {
                    terrainLayers[i] = new TerrainLayer();
                    if (detailTexture != null) terrainLayers[i].diffuseTexture = detailTexture;

                    if (detailNormal != null)
                    {
                        terrainLayers[i].normalMapTexture = detailNormal;
                        terrainLayers[i].normalMapTexture.Apply();
                    }

                    if (!farTerrain)
                        terrainLayers[i].tileSize = new Vector2(detailTileSize, detailTileSize);
                    else
                        terrainLayers[i].tileSize = new Vector2(detailTileSize * 200f, detailTileSize * 200f);

                    terrainLayers[i].tileOffset = Vector2.zero;
                }
#else
                    SplatPrototype currentSplatPrototye = terrain.terrainData.splatPrototypes[i];

                    terrainTextures[i] = new SplatPrototype();
                    if(currentSplatPrototye.texture != null) terrainTextures[i].texture = currentSplatPrototye.texture;

                    if(detailNormal != null)
                    {
                        terrainTextures[i].normalMap = detailNormal;
                        terrainTextures[i].normalMap.Apply();
                    }

                    terrainTextures[i].tileSize = new Vector2(currentSplatPrototye.tileSize.x, currentSplatPrototye.tileSize.y);
                    terrainTextures[i].tileOffset = new Vector2(currentSplatPrototye.tileOffset.x, currentSplatPrototye.tileOffset.y);
                }
                else
                {
                    terrainTextures[i] = new SplatPrototype();
                    if(detailTexture != null) terrainTextures[i].texture = detailTexture;

                    if(detailNormal != null)
                    {
                        terrainTextures[i].normalMap = detailNormal;
                        terrainTextures[i].normalMap.Apply();
                    }

                    if(!farTerrain)
                        terrainTextures[i].tileSize = new Vector2(detailTileSize, detailTileSize);
                    else
                        terrainTextures[i].tileSize = new Vector2(detailTileSize * 200f, detailTileSize * 200f);

                    terrainTextures[i].tileOffset = Vector2.zero;
                }
#endif
            }
            catch (Exception e)
            {
                UnityEngine.Debug.LogError(e);
            }
        }

#if UNITY_2018_3_OR_NEWER
        terrain.terrainData.terrainLayers = terrainLayers;
#else
        terrain.terrainData.splatPrototypes = terrainTextures;
#endif

        length = terrain.terrainData.alphamapResolution;
        smData = new float[length, length, startIndex + 1];

        try
        {
            for (int y = 0; y < length; y++)
            {
                for (int z = 0; z < length; z++)
                {
                    if (startIndex + 1 > 1)
                    {
                        smData[y, z, 0] = 1f - (blend / 100f);
                        smData[y, z, 1] = blend / 100f;
                    }
                    else
                        smData[y, z, 0] = 1f;
                }
            }

            terrain.terrainData.SetAlphamaps(0, 0, smData);
        }
        catch (Exception e)
        {
            UnityEngine.Debug.LogError(e);
        }

        terrain.terrainData.RefreshPrototypes();
        terrain.Flush();

        smData = null;

#if UNITY_2018_3_OR_NEWER
        terrainLayers = null;
#else
        terrainTextures = null;
#endif

        enableDetailTextures = false;
    }

    public void ApplyElevationData()
    {
        TerraLand.TerraLandRuntimeOffline.ApplyOfflineTerrain();
    }

    public void TerrainFromRAW()
    {
        RunAsync(() =>
        {
            //TerraLand.TerraLandRuntimeOffline.rawData = new List<float[,]>();
            TerraLand.TerraLandRuntimeOffline.rawData.Clear();

            if (tiledElevation)
            {
                for (int i = 0; i < totalTiles; i++)
                    TerraLand.TerraLandRuntimeOffline.RawData(TerraLand.TerraLandRuntimeOffline.elevationNames[i], i);
            }
            else
                TerraLand.TerraLandRuntimeOffline.RawData(TerraLand.TerraLandRuntimeOffline.elevationNames[0], 0);

            QueueOnMainThread(() =>
            {
                if (tiledElevation)
                {
                    for (int i = 0; i < totalTiles; i++)
                        FinalizeTerrainHeights(TerraLand.TerraLandRuntimeOffline.rawData[i], TerraLand.TerraLandRuntimeOffline.m_Width, TerraLand.TerraLandRuntimeOffline.m_Height, i);
                }
                else
                    FinalizeTerrainHeights(TerraLand.TerraLandRuntimeOffline.rawData[0], TerraLand.TerraLandRuntimeOffline.m_Width, TerraLand.TerraLandRuntimeOffline.m_Height, 0);

                //FinalizeTerrainHeights(null, TerraLand.TerraLandRuntimeOffline.m_Width, TerraLand.TerraLandRuntimeOffline.m_Height, 0);
            });
        });
    }

    public void TerrainFromRAW(int index)
    {
        RunAsync(() =>
        {
            TerraLand.TerraLandRuntimeOffline.RawData(TerraLand.TerraLandRuntimeOffline.elevationNames[index], index);
        });
    }

    public void FinalizeTerrainFromRAW()
    {
        QueueOnMainThread(() =>
        {
            if (tiledElevation)
            {
                for (int i = 0; i < totalTiles; i++)
                    FinalizeTerrainHeights(TerraLand.TerraLandRuntimeOffline.rawData[i], TerraLand.TerraLandRuntimeOffline.m_Width, TerraLand.TerraLandRuntimeOffline.m_Height, i);
            }
            else
                FinalizeTerrainHeights(TerraLand.TerraLandRuntimeOffline.rawData[0], TerraLand.TerraLandRuntimeOffline.m_Width, TerraLand.TerraLandRuntimeOffline.m_Height, 0);

            //FinalizeTerrainHeights(null, TerraLand.TerraLandRuntimeOffline.m_Width, TerraLand.TerraLandRuntimeOffline.m_Height, 0);
        });
    }

    public void TerrainFromTIFF()
    {
        RunAsync(() =>
        {
            TerraLand.TerraLandRuntimeOffline.TiffData(TerraLand.TerraLandRuntimeOffline.elevationNames[0]);

            QueueOnMainThread(() =>
            {
                FinalizeTerrainHeights(TerraLand.TerraLandRuntimeOffline.tiffData, TerraLand.TerraLandRuntimeOffline.tiffWidth, TerraLand.TerraLandRuntimeOffline.tiffLength, 0);
            });
        });
    }

    public void TerrainFromASCII()
    {
        RunAsync(() =>
        {
            TerraLand.TerraLandRuntimeOffline.AsciiData(TerraLand.TerraLandRuntimeOffline.elevationNames[0]);

            QueueOnMainThread(() =>
            {
                FinalizeTerrainHeights(TerraLand.TerraLandRuntimeOffline.asciiData, TerraLand.TerraLandRuntimeOffline.nCols, TerraLand.TerraLandRuntimeOffline.nRows, 0);
            });
        });
    }

    public void FinalizeTerrainHeights(float[,] data, int width, int height, int index)
    {
        //TerraLand.TerraLandRuntimeOffline.SmoothHeights(TerraLand.TerraLandRuntimeOffline.rawData[index], width, height, index);
        //TerraLand.TerraLandRuntimeOffline.SmoothHeights(data, width, height, index);

        if (smoothIterations > 0)
            TerraLand.TerraLandRuntimeOffline.FinalizeSmooth(data, width, height, smoothIterations, TerraLand.TerraLandRuntimeOffline.smoothBlendIndex, TerraLand.TerraLandRuntimeOffline.smoothBlend);

        if (index == totalTiles - 1)
        {
            if (!tiledElevation)
                TerraLand.TerraLandRuntimeOffline.CalculateResampleHeightmapsGeoServer(index);

            FinalizeHeights();
        }
    }

    public void FinalizeHeights()
    {
        QueueOnMainThread(() =>
        {
            TerraLand.TerraLandRuntimeOffline.FinalizeHeights();
        });
    }


    public void ServerConnectHeightmapNORTH(int i)
    {
        RunAsync(() =>
        {
            TerraLand.TerraLandRuntimeOffline.ElevationDownload(i);

            QueueOnMainThread(() =>
            {
                northCounter++;

                if (northCounter == activeTilesGrid)
                    LoadTerrainHeightsNORTH("North");
                else
                    Timing.RunCoroutine(ConnectTileNORTH());
            });
        });
    }

    private IEnumerator<float> ConnectTileNORTH()
    {
        if (InfiniteTerrainOffline.northTerrains.Count > 0)
        {
            yield return Timing.WaitForSeconds(delayBetweenConnections);

            try
            {
                if (InfiniteTerrainOffline.inProgressWest && northCounter != 0)
                    ServerConnectHeightmapNORTH(TerraLand.TerraLandRuntimeOffline.northIndices[northCounter]);
                else if (InfiniteTerrainOffline.inProgressEast && northCounter != (activeTilesGrid - 1))
                    ServerConnectHeightmapNORTH(TerraLand.TerraLandRuntimeOffline.northIndices[northCounter]);
                else
                    ServerConnectHeightmapNORTH(TerraLand.TerraLandRuntimeOffline.northIndices[northCounter]);
            }
            catch { }
        }
    }

    public void ServerConnectHeightmapSOUTH(int i)
    {
        RunAsync(() =>
        {
            TerraLand.TerraLandRuntimeOffline.ElevationDownload(i);

            QueueOnMainThread(() =>
            {
                southCounter++;

                if (southCounter == activeTilesGrid)
                    LoadTerrainHeightsSOUTH("South");
                else
                    Timing.RunCoroutine(ConnectTileSOUTH());
            });
        });
    }

    private IEnumerator<float> ConnectTileSOUTH()
    {
        if (InfiniteTerrainOffline.southTerrains.Count > 0)
        {
            yield return Timing.WaitForSeconds(delayBetweenConnections);

            try
            {
                if (InfiniteTerrainOffline.inProgressWest && southCounter != 0)
                    ServerConnectHeightmapSOUTH(TerraLand.TerraLandRuntimeOffline.southIndices[southCounter]);
                else if (InfiniteTerrainOffline.inProgressEast && southCounter != (activeTilesGrid - 1))
                    ServerConnectHeightmapSOUTH(TerraLand.TerraLandRuntimeOffline.southIndices[southCounter]);
                else
                    ServerConnectHeightmapSOUTH(TerraLand.TerraLandRuntimeOffline.southIndices[southCounter]);
            }
            catch { }
        }
    }

    public void ServerConnectHeightmapEAST(int i)
    {
        RunAsync(() =>
        {
            TerraLand.TerraLandRuntimeOffline.ElevationDownload(i);

            QueueOnMainThread(() =>
            {
                eastCounter++;

                if (eastCounter == activeTilesGrid)
                    LoadTerrainHeightsEAST("East");
                else
                    Timing.RunCoroutine(ConnectTileEAST());
            });
        });
    }

    private IEnumerator<float> ConnectTileEAST()
    {
        if (InfiniteTerrainOffline.eastTerrains.Count > 0)
        {
            yield return Timing.WaitForSeconds(delayBetweenConnections);

            try
            {
                if (InfiniteTerrainOffline.inProgressNorth && eastCounter != 0)
                    ServerConnectHeightmapEAST(TerraLand.TerraLandRuntimeOffline.eastIndices[eastCounter]);
                else if (InfiniteTerrainOffline.inProgressSouth && eastCounter != (activeTilesGrid - 1))
                    ServerConnectHeightmapEAST(TerraLand.TerraLandRuntimeOffline.eastIndices[eastCounter]);
                else
                    ServerConnectHeightmapEAST(TerraLand.TerraLandRuntimeOffline.eastIndices[eastCounter]);
            }
            catch { }
        }
    }

    public void ServerConnectHeightmapWEST(int i)
    {
        RunAsync(() =>
        {
            TerraLand.TerraLandRuntimeOffline.ElevationDownload(i);

            QueueOnMainThread(() =>
            {
                westCounter++;

                if (westCounter == activeTilesGrid)
                    LoadTerrainHeightsWEST("West");
                else
                    Timing.RunCoroutine(ConnectTileWEST());
            });
        });
    }

    private IEnumerator<float> ConnectTileWEST()
    {
        if (InfiniteTerrainOffline.westTerrains.Count > 0)
        {
            yield return Timing.WaitForSeconds(delayBetweenConnections);

            try
            {
                if (InfiniteTerrainOffline.inProgressNorth && westCounter != 0)
                    ServerConnectHeightmapWEST(TerraLand.TerraLandRuntimeOffline.westIndices[westCounter]);
                else if (InfiniteTerrainOffline.inProgressSouth && westCounter != (activeTilesGrid - 1))
                    ServerConnectHeightmapWEST(TerraLand.TerraLandRuntimeOffline.westIndices[westCounter]);
                else
                    ServerConnectHeightmapWEST(TerraLand.TerraLandRuntimeOffline.westIndices[westCounter]);
            }
            catch { }
        }
    }


    public void LoadTerrainHeightsNORTH(string dir)
    {
        RunAsync(() =>
        {
            TerraLand.TerraLandRuntimeOffline.SmoothNORTH();

            QueueOnMainThread(() =>
            {
                Timing.RunCoroutine(TerraLand.TerraLandRuntimeOffline.LoadTerrainHeightsNORTH(dir));

                //print(InfiniteTerrainOffline.northTerrains.Count);

                //if (InfiniteTerrainOffline.northTerrains.Count == 0)
                //TerraLand.TerraLandRuntimeOffline.ManageNeighborings(dir);
            });
        });
    }

    public void LoadTerrainHeightsSOUTH(string dir)
    {
        RunAsync(() =>
        {
            TerraLand.TerraLandRuntimeOffline.SmoothSOUTH();

            QueueOnMainThread(() =>
            {
                Timing.RunCoroutine(TerraLand.TerraLandRuntimeOffline.LoadTerrainHeightsSOUTH(dir));
            });
        });
    }

    public void LoadTerrainHeightsEAST(string dir)
    {
        RunAsync(() =>
        {
            TerraLand.TerraLandRuntimeOffline.SmoothEAST();

            QueueOnMainThread(() =>
            {
                Timing.RunCoroutine(TerraLand.TerraLandRuntimeOffline.LoadTerrainHeightsEAST(dir));
            });
        });
    }

    public void LoadTerrainHeightsWEST(string dir)
    {
        RunAsync(() =>
        {
            TerraLand.TerraLandRuntimeOffline.SmoothWEST();

            QueueOnMainThread(() =>
            {
                Timing.RunCoroutine(TerraLand.TerraLandRuntimeOffline.LoadTerrainHeightsWEST(dir));
            });
        });
    }

    public void ApplyImageData()
    {
        if (TerraLand.TerraLandRuntimeOffline.geoImagesOK)
            StartCoroutine(TerraLand.TerraLandRuntimeOffline.FillImagesFAST());
        //Timing.RunCoroutine(TerraLand.TerraLandRuntimeOffline.FillImages(totalTiles));
    }

    public void ServerConnectImagery(int i, string dir)
    {
        //StartCoroutine(TerraLand.TerraLandRuntimeOffline.FillImageFAST(i, dir));
    }

    public void ServerConnectImagery(string dir)
    {
        StartCoroutine(TerraLand.TerraLandRuntimeOffline.FillImageFAST(dir));
    }

    public void SendNewTiles(List<Terrain> tiles)
    {
        //StartCoroutine(streamingAssets.ClearNewTileAssets(tiles));

        //if (isStreamingAssets)
            //streamingAssets.ClearNewTileAssets(tiles);
    }

    public void SendProcessedTiles(List<Terrain> tiles)
    {
        if (isStreamingAssets)
            StartCoroutine(streamingAssets.PopulateTiles(tiles));
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

    #endregion
}

