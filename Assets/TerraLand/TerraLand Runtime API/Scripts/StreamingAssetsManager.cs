using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using TerraLand;
using TerraLand.Utils;

#if UNITY_EDITOR
using UnityEditor;
#endif

[ExecuteInEditMode]
public class StreamingAssetsManager : MonoBehaviour
{
    [Header("LAYER 1 SETTINGS")]
    [Space(5)]

    public bool useLayer1 = true;
    public string nameLayer1 = "Trees";
    [Range(0, 99999)] public int seedLayer1 = 10000;
    public List<GameObject> prefabsLayer1;
    [Range(-20, 20)] public int listPriorityLayer1 = 0;
    [Range(1, 100)] public int densityLayer1 = 80;
    //[Range(8, 128)] public int densityResolutionLayer1 = 64;
    [Range(0f, 90f)] public float minSlopeLayer1 = 0f;
    [Range(0f, 90f)] public float maxSlopeLayer1 = 45f;
    public float minElevationLayer1 = -100000f;
    public float maxElevationLayer1 = 100000f;
    [Range(0.01f, 100f)] public float heightLayer1 = 1.0f;
    [Range(0.01f, 100f)] public float widthLayer1 = 1.0f;
    [Range(0f, 4f)] public float sizeRandomLayer1 = 0.5f;
    [Range(0f, 500f)] public float positionRandomLayer1 = 200f;
    [Range(-50f, 50f)] public float positionYOffsetLayer1 = 0f;
    public bool rotation90Layer1 = false;
    public bool lockYRotationLayer1 = true;
    public bool surfaceAngleAlignLayer1 = false;
    [Range(0f, 360f)] public float rotationLayer1 = 360f;
    public bool checkMaskLayer1 = true;
    public UnityEngine.Color32 maskColorLayer1 = new Color32(64, 95, 64, 255);
    [Range(1, 255)] public int maskColorDampingLayer1 = 25;
    public LayerMask layermaskLayer1;
    public float renderDistanceLayer1 = 20000f;

    [Space(20)]
    [Header("LAYER 2 SETTINGS")]
    [Space(5)]

    public bool useLayer2 = true;
    public string nameLayer2 = "Buildings";
    [Range(0, 99999)] public int seedLayer2 = 20000;
    public List<GameObject> prefabsLayer2;
    [Range(-20, 20)] public int listPriorityLayer2 = 0;
    [Range(1, 100)] public int densityLayer2 = 5;
    //[Range(8, 2048)] public int densityResolutionLayer2 = 512;
    [Range(0f, 90f)] public float minSlopeLayer2 = 0f;
    [Range(0f, 90f)] public float maxSlopeLayer2 = 1f;
    public float minElevationLayer2 = -100000f;
    public float maxElevationLayer2 = 100000f;
    [Range(0.01f, 100f)] public float heightLayer2 = 1.0f;
    [Range(0.01f, 100f)] public float widthLayer2 = 1.0f;
    [Range(0f, 4f)] public float sizeRandomLayer2 = 0.15f;
    [Range(0f, 500f)] public float positionRandomLayer2 = 0f;
    [Range(-50f, 50f)] public float positionYOffsetLayer2 = 0f;
    public bool rotation90Layer2 = true;
    public bool lockYRotationLayer2 = true;
    public bool surfaceAngleAlignLayer2 = false;
    [Range(0f, 360f)] public float rotationLayer2 = 360f;
    public bool checkMaskLayer2 = false;
    public UnityEngine.Color32 maskColorLayer2 = new Color32(128, 128, 128, 255);
    [Range(1, 255)] public int maskColorDampingLayer2 = 25;
    public LayerMask layermaskLayer2;
    public float renderDistanceLayer2 = 20000f;

    [Space(20)]
    [Header("LAYER 3 SETTINGS")]
    [Space(5)]

    public bool useLayer3 = true;
    public string nameLayer3 = "Props";
    [Range(0, 99999)] public int seedLayer3 = 30000;
    public List<GameObject> prefabsLayer3;
    [Range(-20, 20)] public int listPriorityLayer3 = 0;
    [Range(1, 100)] public int densityLayer3 = 80;
    //[Range(8, 2048)] public int densityResolutionLayer3 = 512;
    [Range(0f, 90f)] public float minSlopeLayer3 = 15f;
    [Range(0f, 90f)] public float maxSlopeLayer3 = 60f;
    public float minElevationLayer3 = -100000f;
    public float maxElevationLayer3 = 100000f;
    [Range(0.01f, 100f)] public float heightLayer3 = 1.0f;
    [Range(0.01f, 100f)] public float widthLayer3 = 1.0f;
    [Range(0f, 4f)] public float sizeRandomLayer3 = 0.5f;
    [Range(0f, 500f)] public float positionRandomLayer3 = 200f;
    [Range(-50f, 50f)] public float positionYOffsetLayer3 = 0f;
    public bool rotation90Layer3 = false;
    public bool lockYRotationLayer3 = false;
    public bool surfaceAngleAlignLayer3 = false;
    [Range(0f, 360f)] public float rotationLayer3 = 360f;
    public bool checkMaskLayer3 = true;
    public UnityEngine.Color32 maskColorLayer3 = new Color32(204, 184, 171, 255);
    [Range(1, 255)] public int maskColorDampingLayer3 = 25;
    public LayerMask layermaskLayer3;
    public float renderDistanceLayer3 = 20000f;

    [Space(20)]
    [Header("OPTIONS")]
    [Space(5)]

    public RuntimeOffline runtimeOffline;
    [Range(8, 128)] public int densityResolutionPerKilometer = 32;
    public bool createBackgroundMountain = true;
    public bool createBackgroundMountain2 = true;
    //[Range(0f, 1f)] public float spawnDelay = 0.1f;
    //[Range(1, 10)] public int clearMemoryInterval = 3;
    public bool savePreset = false;
    public bool loadPreset = false;

    private int normalizedDensity;
    private int green;
    private int red;
    private int blue;
    private bool worldIsFilled = false;
    private int layerIndexLayer1;
    private int layerIndexLayer2;
    private int layerIndexLayer3;
    private List<Terrain> processedTiles;
    private int processCounter = 0;
    private int processCounterDone = 0;
    private int imageResolution;
    private static float resolutionDiffer;
    private List<UnityEngine.Color32[]> mapColors;
    private float[] rotations90Degrees = new float[] { 0f, 90, 180f, 270f, 360f };
    //private int cycles = 0;
    private Terrain globalTerrain;
    private static float[,] heightmapData;
    private static Texture2D satelliteImage;
    private static bool globalHeightmapGenerated, globalSatImageGenerated = false;
    private const float everestHeight = 8848f;
    private const string prefabExtension = ".prefab";
    private float BGMountainYOffset = -150f; // Will move backfround mountains specified meters/units below streaming tiles to avoid overlapping (value must be negative)
    private float BGMountainPixelError = 50f;
    private int globalTerrainsCounter = 0;
    private FloatingOriginAdvanced floatingOriginAdvanced;
    private static bool FOAIsActive;
    private List<Terrain> passedTiles;

    void Start()
    {
        if (Application.isPlaying)
        {
            SetCameraRenderingDistances();

            if (Camera.main.GetComponent<FloatingOriginAdvanced>() != null)
            {
                floatingOriginAdvanced = Camera.main.GetComponent<FloatingOriginAdvanced>();
                FOAIsActive = floatingOriginAdvanced.enabled;

                if (FOAIsActive)
                    floatingOriginAdvanced.enabled = false;
            }

            globalTerrainsCounter = 0;

            if (createBackgroundMountain)
                CreateBackgroundMountain();

            if (!createBackgroundMountain && createBackgroundMountain2)
                CreateBackgroundMountain2();
        }
    }

#if UNITY_EDITOR
    void OnValidate()
    {
        if (Application.isPlaying) return;

        CheckIsPO2();
        CheckIsPrefab(prefabsLayer1);
        CheckIsPrefab(prefabsLayer2);
        CheckIsPrefab(prefabsLayer3);
        PresetManagement();
    }

    private void CheckIsPO2()
    {
        if (!Mathf.IsPowerOfTwo(densityResolutionPerKilometer))
            densityResolutionPerKilometer = Mathf.ClosestPowerOfTwo(densityResolutionPerKilometer);
    }

    private void CheckIsPrefab(List<GameObject> objects)
    {
        if (objects != null && objects.Count > 0)
        {
            int i = 0;
            foreach (GameObject g in objects)
            {
                if (g == null) return;
                string assetPath = AssetDatabase.GetAssetPath(g);

                if (!assetPath.EndsWith(prefabExtension))
                {
                    EditorUtility.DisplayDialog("PREFAB MISMATCH", "Selected object is not a project prefab!", "Ok");
                    objects[i] = null;
                    break;
                }
                i++;
            }
        }
    }

    private void PresetManagement()
    {
        if (savePreset)
        {
            savePreset = false;
            runtimeOffline.SetupServer();
            PresetManager.SavePreset(this, "Streaming" + "_" + Regex.Replace(RuntimeOffline.locationName, @"\s+", ""));
        }
        else if (loadPreset)
        {
            loadPreset = false;
            PresetManager.LoadPreset(this);
        }
    }
#endif

    private void SetCameraRenderingDistances()
    {
        layerIndexLayer1 = Mathf.RoundToInt(Mathf.Log(layermaskLayer1.value, 2));
        layerIndexLayer2 = Mathf.RoundToInt(Mathf.Log(layermaskLayer2.value, 2));
        layerIndexLayer3 = Mathf.RoundToInt(Mathf.Log(layermaskLayer3.value, 2));

        Camera camera = Camera.main;
        float[] distances = new float[32];

        if (layerIndexLayer1 >= 0 && layerIndexLayer1 < 32)
            distances[layerIndexLayer1] = renderDistanceLayer1;

        if (layerIndexLayer2 >= 0 && layerIndexLayer2 < 32)
            distances[layerIndexLayer2] = renderDistanceLayer2;

        if (layerIndexLayer3 >= 0 && layerIndexLayer3 < 32)
            distances[layerIndexLayer3] = renderDistanceLayer3;

        camera.layerCullDistances = distances;
    }

    private void CreateBackgroundMountain()
    {
        string locationName = RuntimeOffline.locationName;
        string globalHeightmapPath = RuntimeOffline.globalHeightmapPath;
        string globalSatelliteImagePath = RuntimeOffline.globalSatelliteImagePath;
        double top = RuntimeOffline.top;
        double left = RuntimeOffline.left;
        double bottom = RuntimeOffline.bottom;
        double right = RuntimeOffline.right;
        double latitude = RuntimeOffline.centerLatitude;
        double longitude = RuntimeOffline.centerLongitude;
        int heightmapResolution = runtimeOffline.previewHeightmapResolution;
        int satImageResolution = runtimeOffline.previewSatelliteImageResolution;
        string name = "Global Terrain";
        float areaSize = RuntimeOffline.exaggeratedWorldSize * 1000f;
        float offsetY = BGMountainYOffset * runtimeOffline.sizeExaggeration;
        Vector3 terrainSize = new Vector3(areaSize, everestHeight * runtimeOffline.elevationExaggeration * runtimeOffline.sizeExaggeration, areaSize);
        globalHeightmapGenerated = globalSatImageGenerated = false;

        Camera.main.farClipPlane = RuntimeOffline.areaSize * 1000 * 3;

        GameObject terrainObject = CreateGlobalTerrain(name, areaSize, offsetY, heightmapResolution, terrainSize, BGMountainPixelError);
        Terrain terrain = terrainObject.GetComponent<Terrain>();

#if !UNITY_2019_1_OR_NEWER
        terrain.materialType = Terrain.MaterialType.Custom;
#endif
        terrain.materialTemplate = MaterialManager.GetTerrainMaterial();

        CheckAndDownloadGlobalHeightmap(globalHeightmapPath, heightmapResolution, top, left, bottom, right);
        CheckAndDownloadGlobalSatelliteImage(globalSatelliteImagePath, satImageResolution, top, left, bottom, right);
        StartCoroutine(UpdateGlobalTerrain());

        Vector3 absoluteWorldPos = (Vector3)GetAbsoluteWorldPosition(latitude, longitude, top, left, bottom, right, terrainSize, true);
        absoluteWorldPos.y = offsetY;
        terrainObject.transform.position = absoluteWorldPos;
    }

    private void CreateBackgroundMountain2()
    {
        string locationName = RuntimeOffline.locationName;
        string globalHeightmapPath = RuntimeOffline.globalHeightmapPath2;
        string globalSatelliteImagePath = RuntimeOffline.globalSatelliteImagePath2;

        float areaMultiplyFactor = 4f;
        float areaSize = RuntimeOffline.exaggeratedWorldSize * 1000f * areaMultiplyFactor;
        float areaSizeOriginal = RuntimeOffline.areaSize;
        float areaSizeLat = areaSizeOriginal * areaMultiplyFactor;
        float areaSizeLon = areaSizeOriginal * areaMultiplyFactor;
        double areaCenterLat = (RuntimeOffline.top + RuntimeOffline.bottom) / 2d;
        double areaCenterLon = (RuntimeOffline.right + RuntimeOffline.left) / 2d;
        double latitude = RuntimeOffline.centerLatitude;
        double longitude = RuntimeOffline.centerLongitude;
        double top, left, bottom, right = 0;
        AreaBounds.MetricsToBBox(areaCenterLat, areaCenterLon, areaSizeLat, areaSizeLon, out top, out left, out bottom, out right);

        int heightmapResolution = runtimeOffline.previewHeightmapResolution;
        int satImageResolution = runtimeOffline.previewSatelliteImageResolution;
        string name = "Global Terrain 2";
        float offsetY = BGMountainYOffset * runtimeOffline.sizeExaggeration * 4;
        Vector3 terrainSize = new Vector3(areaSize, everestHeight * runtimeOffline.elevationExaggeration * runtimeOffline.sizeExaggeration, areaSize);
        globalHeightmapGenerated = globalSatImageGenerated = false;

        Camera.main.farClipPlane = areaSizeLat * 1000 * 3;

        GameObject terrainObject = CreateGlobalTerrain(name, areaSize, offsetY, heightmapResolution, terrainSize, BGMountainPixelError);
        Terrain terrain = terrainObject.GetComponent<Terrain>();

#if !UNITY_2019_1_OR_NEWER
        terrain.materialType = Terrain.MaterialType.Custom;
#endif
        terrain.materialTemplate = MaterialManager.GetTerrainMaterial();

        CheckAndDownloadGlobalHeightmap(globalHeightmapPath, heightmapResolution, top, left, bottom, right);
        CheckAndDownloadGlobalSatelliteImage(globalSatelliteImagePath, satImageResolution, top, left, bottom, right);
        StartCoroutine(UpdateGlobalTerrain());

        Vector3 absoluteWorldPos = (Vector3)GetAbsoluteWorldPosition(latitude, longitude, top, left, bottom, right, terrainSize, true);
        absoluteWorldPos.y = offsetY;
        terrainObject.transform.position = absoluteWorldPos;
    }

    private GameObject CreateGlobalTerrain(string name, float areaSize, float offsetY, int heightmapResolution, Vector3 terrainSize, float pixelError)
    {
        GameObject globalTerrainObject = new GameObject(name);
        globalTerrainObject.AddComponent<Terrain>();

        TerrainData globalTerrainData = new TerrainData();
        globalTerrainData.heightmapResolution = heightmapResolution;
        globalTerrainData.size = terrainSize;
        globalTerrainData.name = name;

        globalTerrain = globalTerrainObject.GetComponent<Terrain>();
        globalTerrain.terrainData = globalTerrainData;
        globalTerrain.heightmapPixelError = pixelError;
        globalTerrain.basemapDistance = areaSize;

#if UNITY_2018_3_OR_NEWER
        if (runtimeOffline.drawInstanced)
            globalTerrain.drawInstanced = true;
        else
            globalTerrain.drawInstanced = false;
#endif

        globalTerrainObject.AddComponent<TerrainCollider>();
        globalTerrainObject.GetComponent<TerrainCollider>().terrainData = globalTerrainData;
        globalTerrainObject.transform.position = new Vector3(-(areaSize / 2f), offsetY, -(areaSize / 2f));
        globalTerrainObject.transform.position += new Vector3(RuntimeOffline.worldPositionOffsetX, 0, RuntimeOffline.worldPositionOffsetY);

        globalTerrain.Flush();

        return globalTerrainObject;
    }

    private void CheckAndDownloadGlobalHeightmap(string globalHeightmapPath, int heightmapResolution, double top, double left, double bottom, double right)
    {
        heightmapData = new float[heightmapResolution, heightmapResolution];

        if (File.Exists(globalHeightmapPath))
        {
            LoomRuntime.RunAsync(() =>
            {
                heightmapData = Tile.LoadHeightmapFromLocalServer(globalHeightmapPath);

                LoomRuntime.QueueOnMainThread(() =>
                {
                    globalHeightmapGenerated = true;
                });
            });
        }
        else
        {
            LoomRuntime.RunAsync(() =>
            {
                Tile.LoadHeightmapFromESRIServer(globalHeightmapPath, top, left, bottom, right, heightmapResolution);
                heightmapData = Tile.LoadHeightmapFromLocalServer(globalHeightmapPath);

                LoomRuntime.QueueOnMainThread(() =>
                {
                    globalHeightmapGenerated = true;
                });
            });
        }
    }

    private void CheckAndDownloadGlobalSatelliteImage(string globalSatelliteImagePath, int satImageResolution, double top, double left, double bottom, double right)
    {
        if (File.Exists(globalSatelliteImagePath))
        {
            satelliteImage = Tile.LoadSatelliteMapFromLocalServer(globalSatelliteImagePath);
            globalSatImageGenerated = true;
        }
        else
        {
            LoomRuntime.RunAsync(() =>
            {
                Tile.LoadSatelliteMapFromESRIServer(globalSatelliteImagePath, top, left, bottom, right, satImageResolution);

                LoomRuntime.QueueOnMainThread(() =>
                {
                    satelliteImage = Tile.LoadSatelliteMapFromLocalServer(globalSatelliteImagePath);
                    globalSatImageGenerated = true;
                });
            });
        }
    }

    private IEnumerator UpdateGlobalTerrain()
    {
        if (globalHeightmapGenerated && globalSatImageGenerated)
        {
            TerrainData tData = globalTerrain.terrainData;
            tData.SetHeights(0, 0, heightmapData);

#if UNITY_2018_3_OR_NEWER
            TerrainLayer[] terrainLayers = new TerrainLayer[1];
            terrainLayers[0] = new TerrainLayer();
            terrainLayers[0].diffuseTexture = satelliteImage;
            terrainLayers[0].tileSize = new Vector2(tData.size.x, tData.size.z);
            terrainLayers[0].tileOffset = new Vector2(0, 0);

            tData.terrainLayers = terrainLayers;
#else
            SplatPrototype[] terrainTextures = new SplatPrototype[1];
            terrainTextures[0] = new SplatPrototype();
            terrainTextures[0].texture = satelliteImage;
            terrainTextures[0].tileSize = new Vector2(tData.size.x, tData.size.z);
            terrainTextures[0].tileOffset = new Vector2(0, 0);

            tData.splatPrototypes = terrainTextures;
#endif

            tData.RefreshPrototypes();
            globalTerrain.Flush();

            int alphamapResolution = 32;
            tData.alphamapResolution = alphamapResolution;
            float splatNormalizeX = tData.size.x / alphamapResolution;
            float splatNormalizeY = tData.size.z / alphamapResolution;
            float lengthz, widthz, lengthzOff, widthzOff;
            lengthz = widthz = lengthzOff = widthzOff = 0;

#if UNITY_2018_3_OR_NEWER
            lengthz = tData.terrainLayers[0].tileSize.y / splatNormalizeY;
            widthz = tData.terrainLayers[0].tileSize.x / splatNormalizeX;
            lengthzOff = tData.terrainLayers[0].tileOffset.y / splatNormalizeY;
            widthzOff = tData.terrainLayers[0].tileOffset.x / splatNormalizeX;
#else
            lengthz = tData.splatPrototypes[0].tileSize.y / splatNormalizeY;
            widthz = tData.splatPrototypes[0].tileSize.x / splatNormalizeX;
            lengthzOff = tData.splatPrototypes[0].tileOffset.y / splatNormalizeY;
            widthzOff = tData.splatPrototypes[0].tileOffset.x / splatNormalizeX;
#endif

            float[,,] smData = new float[Mathf.RoundToInt(lengthz), Mathf.RoundToInt(widthz), tData.alphamapLayers];

            for (int y = 0; y < Mathf.RoundToInt(lengthz); y++)
                for (int z = 0; z < Mathf.RoundToInt(widthz); z++)
                    smData[y, z, 0] = 1;

            tData.SetAlphamaps(0, 0, smData);

            tData.RefreshPrototypes();
            globalTerrain.Flush();

            smData = null;

#if UNITY_2018_3_OR_NEWER
            terrainLayers = null;
#else
            terrainTextures = null;
#endif

            globalTerrainsCounter++;

            print("Global Terrain " + globalTerrainsCounter + " Is Generated");

            if (globalTerrainsCounter == 1 && createBackgroundMountain && createBackgroundMountain2)
                CreateBackgroundMountain2();

            if (floatingOriginAdvanced != null && FOAIsActive)
                StartCoroutine(ActivateFloatingOrigin());
        }
        else
        {
            yield return new WaitForSeconds(1);
            StartCoroutine(UpdateGlobalTerrain());
        }
    }

    private IEnumerator ActivateFloatingOrigin()
    {
        if (globalTerrainsCounter == 2 && createBackgroundMountain && createBackgroundMountain2)
        {
            if (worldIsFilled)
                floatingOriginAdvanced.enabled = true;
            else
            {
                yield return new WaitForSeconds(1);
                StartCoroutine(ActivateFloatingOrigin());
            }
        }
        else if (globalTerrainsCounter == 1 && (createBackgroundMountain && !createBackgroundMountain2))
        {
            if (worldIsFilled)
                floatingOriginAdvanced.enabled = true;
            else
            {
                yield return new WaitForSeconds(1);
                StartCoroutine(ActivateFloatingOrigin());
            }
        }
        else if (globalTerrainsCounter == 1 && (!createBackgroundMountain && createBackgroundMountain2))
        {
            if (worldIsFilled)
                floatingOriginAdvanced.enabled = true;
            else
            {
                yield return new WaitForSeconds(1);
                StartCoroutine(ActivateFloatingOrigin());
            }
        }
    }

    // Offset terrain position to avoid origin drift due to Equator diatcance in Latitudes
    private Vector3d GetAbsoluteWorldPosition(double lat, double lon, double top, double left, double bottom, double right, Vector3 terrainSize, bool isSingleTerrain)
    {
        double _yMaxTop = AreaBounds.LatitudeToMercator(top);
        double _xMinLeft = AreaBounds.LongitudeToMercator(left);
        double _yMinBottom = AreaBounds.LatitudeToMercator(bottom);
        double _xMaxRight = AreaBounds.LongitudeToMercator(right);
        double _latSize = Math.Abs(_yMaxTop - _yMinBottom);
        double _lonSize = Math.Abs(_xMinLeft - _xMaxRight);
        double _worldSizeX = terrainSize.x;
        double _worldSizeY = terrainSize.z;
        double _LAT = AreaBounds.LatitudeToMercator(lat);
        double _LON = AreaBounds.LongitudeToMercator(lon);
        double[] _latlonDeltaNormalized = AreaBounds.GetNormalizedDelta(_LAT, _LON, _yMaxTop, _xMinLeft, _latSize, _lonSize);
        Vector2d _initialWorldPositionXZ = AreaBounds.GetWorldPositionFromTile(_latlonDeltaNormalized[0], 1d - _latlonDeltaNormalized[1], _worldSizeY, _worldSizeX);
        Vector3d _initialWorldPosition = new Vector3d(_initialWorldPositionXZ.x, 0, -_initialWorldPositionXZ.y);

        return _initialWorldPosition;
    }

    //// Receive new streaming tiles and clear them from previously placed assets
    //public void ClearNewTileAssets(List<Terrain> tiles)
    //{
    //    foreach (Terrain t in tiles)
    //    {
    //        //yield return new WaitForSeconds(0.001f);
    //
    //        for (int i = 0; i < t.transform.childCount; i++)
    //        {
    //            Transform tr = t.transform.GetChild(i);
    //
    //            if (tr.name.Equals(nameLayer1) || tr.name.Equals(nameLayer2) || tr.name.Equals(nameLayer3))
    //                Destroy(tr.gameObject);
    //        }
    //    }
    //}

    //// Clear tile from previously placed assets
    //public void ClearTileAssets(Terrain tile)
    //{
    //    for (int i = 0; i < tile.transform.childCount; i++)
    //    {
    //        Transform t = tile.transform.GetChild(i);
    //    
    //        if (t.name.Equals(nameLayer1) || t.name.Equals(nameLayer2) || t.name.Equals(nameLayer3))
    //            Destroy(t.gameObject);
    //    }
    //}

    // Receive latest processed streaming tiles and populate assets on them
    public IEnumerator PopulateTiles(List<Terrain> currentPassedTiles)
    {
        if (!worldIsFilled)
            yield return new WaitForSeconds(1f);

        // Wait until all previous stages are passed and then continue populating on new passed tiles
        if (processCounter != 0)
            yield return new WaitUntil(() => processCounter == 3);

        processCounter = 0;
        processCounterDone = 0;
        passedTiles = currentPassedTiles;

        StartCoroutine(PlaceAssets(useLayer1, passedTiles, seedLayer1, checkMaskLayer1, nameLayer1, listPriorityLayer1, densityResolutionPerKilometer, densityLayer1, positionRandomLayer1, positionYOffsetLayer1, minSlopeLayer1, maxSlopeLayer1, minElevationLayer1, maxElevationLayer1, maskColorLayer1, maskColorDampingLayer1, prefabsLayer1, rotation90Layer1, lockYRotationLayer1, surfaceAngleAlignLayer1, rotationLayer1, widthLayer1, heightLayer1, sizeRandomLayer1, layermaskLayer1, layerIndexLayer1));

        // Wait until layer 1 is populated and then continue populating next layer
        yield return new WaitUntil(() => processCounter == 1);
        StartCoroutine(PlaceAssets(useLayer2, passedTiles, seedLayer2, checkMaskLayer2, nameLayer2, listPriorityLayer2, densityResolutionPerKilometer, densityLayer2, positionRandomLayer2, positionYOffsetLayer2, minSlopeLayer2, maxSlopeLayer2, minElevationLayer2, maxElevationLayer2, maskColorLayer2, maskColorDampingLayer2, prefabsLayer2, rotation90Layer2, lockYRotationLayer2, surfaceAngleAlignLayer2, rotationLayer2, widthLayer2, heightLayer2, sizeRandomLayer2, layermaskLayer2, layerIndexLayer2));

        // Wait until layer 2 is populated and then continue populating next layer
        yield return new WaitUntil(() => processCounter == 2);
        StartCoroutine(PlaceAssets(useLayer3, passedTiles, seedLayer3, checkMaskLayer3, nameLayer3, listPriorityLayer3, densityResolutionPerKilometer, densityLayer3, positionRandomLayer3, positionYOffsetLayer3, minSlopeLayer3, maxSlopeLayer3, minElevationLayer3, maxElevationLayer3, maskColorLayer3, maskColorDampingLayer3, prefabsLayer3, rotation90Layer3, lockYRotationLayer3, surfaceAngleAlignLayer3, rotationLayer3, widthLayer3, heightLayer3, sizeRandomLayer3, layermaskLayer3, layerIndexLayer3));
    }

    private IEnumerator PlaceAssets
        (
        bool useLayer,
        List<Terrain> tiles,
        int seedNo,
        bool checkMapColors,
        string layerName,
        int priority,
        int densityResolutionPerKM,
        int density,
        float positionVariation,
        float positionYOffset,
        float minSlope,
        float maxSlope,
        float minElevation,
        float maxElevation,
        UnityEngine.Color32 mapColor,
        int colorDamping,
        List<GameObject> prefabs,
        bool rotation90Degrees,
        bool lockYRotation,
        bool surfaceAngleAlign,
        float rotation,
        float width,
        float height,
        float sizeVariation,
        LayerMask layerMask,
        int layerIndex
        )
    {
        if (!worldIsFilled)
        {
            if (tiles == null)
            {
                Debug.LogError("There are no streaming tiles available!");
                yield break;
            }

            if (maxSlope < minSlope)
            {
                Debug.LogError("Max Steepness must be greater than Min Steepness");
                yield break;
            }

            if (maxElevation < minElevation)
            {
                Debug.LogError("Max Elevation must be greater than Min Elevation");
                yield break;
            }
        }

        if (processCounter == 0)
        {
            mapColors = new List<Color32[]>();
            processedTiles = new List<Terrain>();
        }

        green = mapColor.g;
        red = mapColor.r;
        blue = mapColor.b;
        int index = 0;
        int randomIndex = 0;
        int prefabsCount = prefabs.Count;
        UnityEngine.Random.InitState(seedNo);
        normalizedDensity = (100 + ((100 - density) * 100)) / density;

        // Normalize parameters based on world's exaggerated size
        minElevation *= runtimeOffline.sizeExaggeration;
        maxElevation *= runtimeOffline.sizeExaggeration;
        width *= runtimeOffline.sizeExaggeration;
        height *= runtimeOffline.sizeExaggeration;
        positionVariation *= runtimeOffline.sizeExaggeration;
        positionYOffset *= runtimeOffline.sizeExaggeration;

        float tileSizeInKilometers = tiles[0].terrainData.size.x / runtimeOffline.sizeExaggeration / 1000f;
        densityResolutionPerKM = (int)(densityResolutionPerKM * tileSizeInKilometers);

        GameObject tileAssets = null;

        if (useLayer && prefabsCount > 0)
        {
            if (!worldIsFilled)
                print("Placing " + layerName);

            foreach (Terrain t in tiles)
            {
                // Double check if there are previously placed assets on tile and remove them
                //if (processCounterDone == 0)
                //ClearTileAssets(t);

                TerrainData data = t.terrainData;

                tileAssets = t.transform.Find(layerName)?.gameObject;

                if (tileAssets == null)
                {
                    tileAssets = new GameObject(layerName);
                    tileAssets.transform.parent = t.transform;
                }

                if (processCounter == 0)
                    DisableChildren(tileAssets);

                //if (worldIsFilled)
                //yield return new WaitForSeconds(spawnDelay);

                if (checkMapColors && runtimeOffline.imageryAvailable && !runtimeOffline.elevationOnly)
                {
                    int itemCounter = 0;

                    if (processCounterDone == 0)
                    {
                        string satelliteImageName = data.splatPrototypes[0].texture.name;
                        string imageName = Path.GetFullPath(TerraLandRuntimeOffline.dataBasePathImagery + "/" + satelliteImageName);
                        byte[] imageData = File.ReadAllBytes(imageName);
                        Texture2D satelliteImage = new Texture2D(1, 1);
                        satelliteImage.LoadImage(imageData);
                        mapColors.Add(satelliteImage.GetPixels32()); // Cache satellite image pixels into a list

                        if (satelliteImage)
                        {
                            imageResolution = satelliteImage.width;
                            resolutionDiffer = (float)imageResolution / densityResolutionPerKM;

                            for (int y = 0; y < densityResolutionPerKM; y += normalizedDensity)
                            {
                                for (int x = 0; x < densityResolutionPerKM; x += normalizedDensity)
                                {
                                    float xPointLocal = (float)x / densityResolutionPerKM + (UnityEngine.Random.Range(-positionVariation, positionVariation) / data.size.x);
                                    float yPointLocal = (float)y / densityResolutionPerKM + (UnityEngine.Random.Range(-positionVariation, positionVariation) / data.size.z);
                                    float steepness = data.GetSteepness(xPointLocal, yPointLocal);
                                    float elevation = data.GetInterpolatedHeight(xPointLocal, yPointLocal);

                                    if (steepness >= minSlope && steepness <= maxSlope) // Check Steepness Range
                                    {
                                        if (elevation >= minElevation && elevation <= maxElevation)  // Check Elevation Zone
                                        {
                                            int indexX = (int)(x * resolutionDiffer);
                                            int indexY = (int)(y * resolutionDiffer);
                                            int colorIndex = indexY * imageResolution + indexX;

                                            UnityEngine.Color32 col = mapColors[index][colorIndex];

                                            if ((col.r <= red + colorDamping) && (col.r >= red - colorDamping)) // Check Red Color
                                            {
                                                if ((col.g <= green + colorDamping) && (col.g >= green - colorDamping))  // Check Green Color
                                                {
                                                    if ((col.b <= blue + colorDamping) && (col.b >= blue - colorDamping)) // Check Blue Color
                                                    {
                                                        if (priority == 0)
                                                            randomIndex = UnityEngine.Random.Range(0, prefabsCount);
                                                        else if (priority > 0)
                                                            randomIndex = Mathf.Clamp(UnityEngine.Random.Range(0, prefabsCount + priority), 0, prefabsCount - 1);
                                                        else if (priority < 0)
                                                            randomIndex = Mathf.Clamp(UnityEngine.Random.Range(priority, prefabsCount), 0, prefabsCount - 1);

                                                        if (prefabs[randomIndex] != null)
                                                        {
                                                            GameObject go = null;

                                                            if (tileAssets.transform.childCount > itemCounter)
                                                                go = tileAssets.transform.GetChild(itemCounter)?.gameObject;

                                                            if (go == null)
                                                                go = Instantiate(prefabs[randomIndex]);

                                                            float xPointWorld = xPointLocal * data.size.x;
                                                            float zPointWorld = yPointLocal * data.size.z;
                                                            go.transform.position = new Vector3(xPointWorld, elevation + positionYOffset, zPointWorld) + t.transform.position;

                                                            if (!rotation90Degrees)
                                                            {
                                                                if (lockYRotation)
                                                                {
                                                                    go.transform.eulerAngles = new Vector3(0, UnityEngine.Random.Range(0f, rotation), 0);

                                                                    if (surfaceAngleAlign)
                                                                    {
                                                                        Vector3 terrainAngle = data.GetInterpolatedNormal(xPointLocal, yPointLocal);
                                                                        go.transform.rotation = Quaternion.FromToRotation(Vector3.up, terrainAngle);
                                                                    }
                                                                }
                                                                else
                                                                    go.transform.eulerAngles = new Vector3(UnityEngine.Random.Range(0f, rotation), UnityEngine.Random.Range(0f, rotation), UnityEngine.Random.Range(0f, rotation));
                                                            }
                                                            else
                                                            {
                                                                if (lockYRotation)
                                                                {
                                                                    int ind = UnityEngine.Random.Range(0, rotations90Degrees.Length);
                                                                    go.transform.eulerAngles = new Vector3(0, rotations90Degrees[ind], 0);

                                                                    if (surfaceAngleAlign)
                                                                    {
                                                                        Vector3 terrainAngle = data.GetInterpolatedNormal(xPointLocal, yPointLocal);
                                                                        go.transform.rotation = Quaternion.FromToRotation(Vector3.up, terrainAngle);
                                                                    }
                                                                }
                                                                else
                                                                    go.transform.eulerAngles = new Vector3(rotations90Degrees[UnityEngine.Random.Range(0, rotations90Degrees.Length)], rotations90Degrees[UnityEngine.Random.Range(0, rotations90Degrees.Length)], rotations90Degrees[UnityEngine.Random.Range(0, rotations90Degrees.Length)]);
                                                            }

                                                            float rand = UnityEngine.Random.Range(-sizeVariation, sizeVariation);
                                                            float widthScale = width + (width * rand);
                                                            float heightScale = height + (height * rand);
                                                            go.transform.localScale = new Vector3(widthScale, heightScale, widthScale);

                                                            go.transform.parent = tileAssets.transform;

                                                            foreach (Transform tr in go.GetComponentsInChildren<Transform>(true))
                                                                tr.gameObject.layer = layerIndex;

                                                            go.SetActive(true);

                                                            itemCounter++;
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        for (int y = 0; y < densityResolutionPerKM; y += normalizedDensity)
                        {
                            for (int x = 0; x < densityResolutionPerKM; x += normalizedDensity)
                            {
                                float xPointLocal = (float)x / densityResolutionPerKM + (UnityEngine.Random.Range(-positionVariation, positionVariation) / data.size.x);
                                float yPointLocal = (float)y / densityResolutionPerKM + (UnityEngine.Random.Range(-positionVariation, positionVariation) / data.size.z);
                                float steepness = data.GetSteepness(xPointLocal, yPointLocal);
                                float elevation = data.GetInterpolatedHeight(xPointLocal, yPointLocal);

                                if (steepness >= minSlope && steepness <= maxSlope) // Check Steepness Range
                                {
                                    if (elevation >= minElevation && elevation <= maxElevation)  // Check Elevation Zone
                                    {
                                        int indexX = (int)(x * resolutionDiffer);
                                        int indexY = (int)(y * resolutionDiffer);
                                        int colorIndex = indexY * imageResolution + indexX;

                                        UnityEngine.Color32 col = mapColors[index][colorIndex];

                                        if ((col.r <= red + colorDamping) && (col.r >= red - colorDamping)) // Check Red Color
                                        {
                                            if ((col.g <= green + colorDamping) && (col.g >= green - colorDamping))  // Check Green Color
                                            {
                                                if ((col.b <= blue + colorDamping) && (col.b >= blue - colorDamping)) // Check Blue Color
                                                {
                                                    if (priority == 0)
                                                        randomIndex = UnityEngine.Random.Range(0, prefabsCount);
                                                    else if (priority > 0)
                                                        randomIndex = Mathf.Clamp(UnityEngine.Random.Range(0, prefabsCount + priority), 0, prefabsCount - 1);
                                                    else if (priority < 0)
                                                        randomIndex = Mathf.Clamp(UnityEngine.Random.Range(priority, prefabsCount), 0, prefabsCount - 1);

                                                    if (prefabs[randomIndex] != null)
                                                    {
                                                        GameObject go = null;

                                                        if (tileAssets.transform.childCount > itemCounter)
                                                            go = tileAssets.transform.GetChild(itemCounter)?.gameObject;

                                                        if (go == null)
                                                            go = Instantiate(prefabs[randomIndex]);

                                                        float xPointWorld = xPointLocal * data.size.x;
                                                        float zPointWorld = yPointLocal * data.size.z;
                                                        go.transform.position = new Vector3(xPointWorld, elevation + positionYOffset, zPointWorld) + t.transform.position;

                                                        if (!rotation90Degrees)
                                                        {
                                                            if (lockYRotation)
                                                            {
                                                                go.transform.eulerAngles = new Vector3(0, UnityEngine.Random.Range(0f, rotation), 0);

                                                                if (surfaceAngleAlign)
                                                                {
                                                                    Vector3 terrainAngle = data.GetInterpolatedNormal(xPointLocal, yPointLocal);
                                                                    go.transform.rotation = Quaternion.FromToRotation(Vector3.up, terrainAngle);
                                                                }
                                                            }
                                                            else
                                                                go.transform.eulerAngles = new Vector3(UnityEngine.Random.Range(0f, rotation), UnityEngine.Random.Range(0f, rotation), UnityEngine.Random.Range(0f, rotation));
                                                        }
                                                        else
                                                        {
                                                            if (lockYRotation)
                                                            {
                                                                int ind = UnityEngine.Random.Range(0, rotations90Degrees.Length);
                                                                go.transform.eulerAngles = new Vector3(0, rotations90Degrees[ind], 0);

                                                                if (surfaceAngleAlign)
                                                                {
                                                                    Vector3 terrainAngle = data.GetInterpolatedNormal(xPointLocal, yPointLocal);
                                                                    go.transform.rotation = Quaternion.FromToRotation(Vector3.up, terrainAngle);
                                                                }
                                                            }
                                                            else
                                                                go.transform.eulerAngles = new Vector3(rotations90Degrees[UnityEngine.Random.Range(0, rotations90Degrees.Length)], rotations90Degrees[UnityEngine.Random.Range(0, rotations90Degrees.Length)], rotations90Degrees[UnityEngine.Random.Range(0, rotations90Degrees.Length)]);
                                                        }

                                                        float rand = UnityEngine.Random.Range(-sizeVariation, sizeVariation);
                                                        float widthScale = width + (width * rand);
                                                        float heightScale = height + (height * rand);
                                                        go.transform.localScale = new Vector3(widthScale, heightScale, widthScale);

                                                        go.transform.parent = tileAssets.transform;

                                                        foreach (Transform tr in go.GetComponentsInChildren<Transform>(true))
                                                            tr.gameObject.layer = layerIndex;

                                                        go.SetActive(true);

                                                        itemCounter++;
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }

                    for (int i = itemCounter; i < tileAssets.transform.childCount; i++)
                        tileAssets.transform.GetChild(i)?.gameObject.SetActive(false);
                }

                if (!checkMapColors)
                {
                    int itemCounter = 0;

                    for (int y = 0; y < densityResolutionPerKM; y += normalizedDensity)
                    {
                        for (int x = 0; x < densityResolutionPerKM; x += normalizedDensity)
                        {
                            float xPointLocal = (float)x / densityResolutionPerKM + (UnityEngine.Random.Range(-positionVariation, positionVariation) / data.size.x);
                            float yPointLocal = (float)y / densityResolutionPerKM + (UnityEngine.Random.Range(-positionVariation, positionVariation) / data.size.z);
                            float steepness = data.GetSteepness(xPointLocal, yPointLocal);
                            float elevation = data.GetInterpolatedHeight(xPointLocal, yPointLocal);

                            if (steepness >= minSlope && steepness <= maxSlope) // Check Steepness Range
                            {
                                if (elevation >= minElevation && elevation <= maxElevation)  // Check Elevation Zone
                                {
                                    if (priority == 0)
                                        randomIndex = UnityEngine.Random.Range(0, prefabsCount);
                                    else if (priority > 0)
                                        randomIndex = Mathf.Clamp(UnityEngine.Random.Range(0, prefabsCount + priority), 0, prefabsCount - 1);
                                    else if (priority < 0)
                                        randomIndex = Mathf.Clamp(UnityEngine.Random.Range(priority, prefabsCount), 0, prefabsCount - 1);

                                    if (prefabs[randomIndex] != null)
                                    {
                                        GameObject go = null;

                                        if (tileAssets.transform.childCount > itemCounter)
                                            go = tileAssets.transform.GetChild(itemCounter)?.gameObject;

                                        if (go == null)
                                            go = Instantiate(prefabs[randomIndex]);

                                        float xPointWorld = xPointLocal * data.size.x;
                                        float zPointWorld = yPointLocal * data.size.z;
                                        go.transform.position = new Vector3(xPointWorld, elevation + positionYOffset, zPointWorld) + t.transform.position;

                                        if (!rotation90Degrees)
                                        {
                                            if (lockYRotation)
                                            {
                                                go.transform.eulerAngles = new Vector3(0, UnityEngine.Random.Range(0f, rotation), 0);

                                                if (surfaceAngleAlign)
                                                {
                                                    Vector3 terrainAngle = data.GetInterpolatedNormal(xPointLocal, yPointLocal);
                                                    go.transform.rotation = Quaternion.FromToRotation(Vector3.up, terrainAngle);
                                                }
                                            }
                                            else
                                                go.transform.eulerAngles = new Vector3(UnityEngine.Random.Range(0f, rotation), UnityEngine.Random.Range(0f, rotation), UnityEngine.Random.Range(0f, rotation));
                                        }
                                        else
                                        {
                                            if (lockYRotation)
                                            {
                                                int ind = UnityEngine.Random.Range(0, rotations90Degrees.Length);
                                                go.transform.eulerAngles = new Vector3(0, rotations90Degrees[ind], 0);

                                                if (surfaceAngleAlign)
                                                {
                                                    Vector3 terrainAngle = data.GetInterpolatedNormal(xPointLocal, yPointLocal);
                                                    go.transform.rotation = Quaternion.FromToRotation(Vector3.up, terrainAngle);
                                                }
                                            }
                                            else
                                                go.transform.eulerAngles = new Vector3(rotations90Degrees[UnityEngine.Random.Range(0, rotations90Degrees.Length)], rotations90Degrees[UnityEngine.Random.Range(0, rotations90Degrees.Length)], rotations90Degrees[UnityEngine.Random.Range(0, rotations90Degrees.Length)]);
                                        }

                                        float rand = UnityEngine.Random.Range(-sizeVariation, sizeVariation);
                                        float widthScale = width + (width * rand);
                                        float heightScale = height + (height * rand);
                                        go.transform.localScale = new Vector3(widthScale, heightScale, widthScale);

                                        go.transform.parent = tileAssets.transform;

                                        foreach (Transform tr in go.GetComponentsInChildren<Transform>(true))
                                            tr.gameObject.layer = layerIndex;

                                        go.SetActive(true);

                                        itemCounter++;
                                    }
                                }
                            }
                        }
                    }

                    for (int i = itemCounter; i < tileAssets.transform.childCount; i++)
                        tileAssets.transform.GetChild(i)?.gameObject.SetActive(false);
                }

                if (processCounterDone == 0)
                    processedTiles.Add(t);

                index++;
            }

            processCounterDone++;
        }

        processCounter++;

        if (processCounter == 3)
        {
            // Remove streamed tiles list after placing objects on them
            if (runtimeOffline.processedTiles.Count > 0)
            {
                foreach (Terrain t in processedTiles)
                    runtimeOffline.processedTiles.Remove(t);
            }

            if (!worldIsFilled)
                print("Finished Placing Streaming Objects");

            //if (!worldIsFilled)
            //    ClearMemory();
            //else
            //{
            //    cycles++;
            //
            //    if (cycles % clearMemoryInterval == 0)
            //        ClearMemory();
            //}

            worldIsFilled = true;
        }
    }

    private void DisableChildren(GameObject go)
    {
        for (int i = 0; i < go.transform.childCount; i++)
            go.transform.GetChild(i)?.gameObject.SetActive(false);
    }

    //private void ClearMemory()
    //{
    //    Resources.UnloadUnusedAssets();
    //}
}

