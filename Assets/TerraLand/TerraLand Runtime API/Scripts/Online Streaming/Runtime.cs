using UnityEngine;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading;
using MEC;

namespace TerraLand
{
    public class Runtime : MonoBehaviour
    {
        public enum gridSizeEnumList
        {
            _1 = 1,
            _2x2 = 2,
            _4x4 = 4,
            _8x8 = 8,
            _16x16 = 16,
            _32x32 = 32,
            _64x64 = 64
        }

        // Main Settings
        public gridSizeEnumList terrainGridSize = gridSizeEnumList._8x8;
        public string latitudeUser = ""; // 27.98582
        public string longitudeUser = ""; // 86.9236
        public float areaSize = 25f;
        public int heightmapResolution = 1024;
        public int imageResolution = 1024;
        public float elevationExaggeration = 1.25f;
        public int smoothIterations = 1;
        public bool farTerrain = true;
        public int farTerrainHeightmapResolution = 512;
        public int farTerrainImageResolution = 1024;
        public float areaSizeFarMultiplier = 4f;


        // Performance Settings
        public float heightmapPixelError = 10f;
        public float farTerrainQuality = 10f;
        public int cellSize = 64;
        public int concurrentTasks = 4;
        public float elevationDelay = 0.5f;
        public float imageryDelay = 0.5f;

        // Advanced Settings
        public bool elevationOnly = false;
        public bool fastStartBuild = true;
        public bool showTileOnFinish = true;
        public bool progressiveTexturing = true;
        public bool spiralGeneration = true;
        public bool delayedLOD = false;
        [HideInInspector] public bool IsCustomGeoServer = false;
        [HideInInspector] public bool progressiveGeneration = false;
        [HideInInspector] public float terrainDistance;
        [HideInInspector] public float terrainCurvator;
        public float farTerrainBelowHeight = 100f;
        [HideInInspector] public int farTerrainCellSize;
        public bool stitchTerrainTiles = true;
        [Range(5, 100)] public int levelSmooth = 5;
        [Range(1, 7)] public int power = 1;
        public bool trend = false;
        public int stitchDistance = 4;
        public float stitchDelay = 0.25f;

        //TODO: User Geo-Server
        [HideInInspector] public string dataBasePath = "C:/Users/Amir/Desktop/GeoServer"; //public string dataBasePath = "http://terraunity.com/freedownload/TerraLand_GeoServer";

        public static bool initialRunInBackground;


        // Menu Settings Parameters
        // Main Settings
        public static string terrainGridSizeMenu;
        public static string latitudeMenu;
        public static string longitudeMenu;
        public static float areaSizeMenu;
        public static int heightmapResolutionMenu;
        public static int imageResolutionMenu;
        public static float elevationExaggerationMenu;
        public static int smoothIterationsMenu;
        public static bool farTerrainMenu;
        public static int farTerrainHeightmapResolutionMenu;
        public static int farTerrainImageResolutionMenu;
        public static float areaSizeFarMultiplierMenu;

        // Performance Settings
        public static float heightmapPixelErrorMenu;
        public static float farTerrainQualityMenu;
        public static int cellSizeMenu;
        public static int concurrentTasksMenu;
        public static float elevationDelayMenu;
        public static float imageryDelayMenu;

        // Advanced Settings
        public static bool elevationOnlyMenu;
        public static bool fastStartBuildMenu;
        public static bool showTileOnFinishMenu;
        public static bool progressiveTexturingMenu;
        public static bool spiralGenerationMenu;
        public static bool delayedLODMenu;
        public static float farTerrainBelowHeightMenu;
        public static bool stitchTerrainTilesMenu;
        public static int levelSmoothMenu;
        public static int powerMenu;
        public static int stitchDistanceMenu;
        public static float stitchDelayMenu;


        #region multithreading variables

        int maxThreads = 50;
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


        //public WorldElevation.Terrain_ImageServer mapserviceTerrain = null;

        void Start()
        {
#if UNITY_EDITOR
            initialRunInBackground = UnityEditor.PlayerSettings.runInBackground;
            UnityEditor.PlayerSettings.runInBackground = true;
#endif

            if (!MainMenu.latitude.Equals(""))
                SetFromMenu();

            terrainDistance = (areaSize * 1000f) / 3f; //2f
            farTerrainCellSize = cellSize;

            terrainCurvator = 0.00001f;

            int tileResolution = (heightmapResolution / (int)terrainGridSize);

            if (cellSize > tileResolution)
                cellSize = tileResolution;

            if (farTerrainCellSize > farTerrainHeightmapResolution)
                farTerrainCellSize = farTerrainHeightmapResolution;

            m_HasLoaded = true;

            //#if UNITY_EDITOR
            //ConnectionsManager.SetAsyncConnections();
            //#else
            ConnectionsManagerRuntime.SetAsyncConnections();
            //#endif

            //mapserviceTerrain = new WorldElevation.Terrain_ImageServer();
            TerraLand.TerraLandRuntime.Initialize();

            progressiveGeneration = false;
        }

        void Update()
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
        }

        public void SetFromMenu()
        {
            // Main Settings
            terrainGridSize = (gridSizeEnumList)Enum.Parse(typeof(gridSizeEnumList), terrainGridSizeMenu);
            latitudeUser = latitudeMenu;
            longitudeUser = longitudeMenu;
            areaSize = areaSizeMenu;
            heightmapResolution = heightmapResolutionMenu;
            imageResolution = imageResolutionMenu;
            elevationExaggeration = elevationExaggerationMenu;
            smoothIterations = smoothIterationsMenu;
            farTerrain = farTerrainMenu;
            farTerrainHeightmapResolution = farTerrainHeightmapResolutionMenu;
            farTerrainImageResolution = farTerrainImageResolutionMenu;
            areaSizeFarMultiplier = areaSizeFarMultiplierMenu;


            // Performance Settings
            heightmapPixelError = heightmapPixelErrorMenu;
            farTerrainQuality = farTerrainQualityMenu;
            cellSize = cellSizeMenu;
            concurrentTasks = concurrentTasksMenu;
            elevationDelay = elevationDelayMenu;
            imageryDelay = imageryDelayMenu;


            // Advanced Settings
            elevationOnly = elevationOnlyMenu;
            fastStartBuild = fastStartBuildMenu;
            showTileOnFinish = showTileOnFinishMenu;
            progressiveTexturing = progressiveTexturingMenu;
            spiralGeneration = spiralGenerationMenu;
            delayedLOD = delayedLODMenu;
            farTerrainBelowHeight = farTerrainBelowHeightMenu;
            stitchTerrainTiles = stitchTerrainTilesMenu;
            levelSmooth = levelSmoothMenu;
            power = powerMenu;
            stitchDistance = stitchDistanceMenu;
            stitchDelay = stitchDelayMenu;
        }

        public void LoadTerrainHeights()
        {
            RunAsync(() =>
            {
                TerraLand.TerraLandRuntime.SmoothAllHeights();

                QueueOnMainThread(() =>
                {
                    Timing.RunCoroutine(TerraLand.TerraLandRuntime.LoadTerrainHeightsFromTIFFDynamic(0));
                });
            });
        }

        public void LoadTerrainHeightsFAR()
        {
            RunAsync(() =>
            {
                TerraLand.TerraLandRuntime.SmoothFarTerrain();

                QueueOnMainThread(() =>
                {
                    Timing.RunCoroutine(TerraLand.TerraLandRuntime.LoadTerrainHeightsFromTIFFFAR());
                });
            });
        }

        public void LoadTerrainHeightsNORTH(int i)
        {
            RunAsync(() =>
            {
            //TerraLand.TerraLandRuntime.SmoothNORTH(i);

            if (i == (int)terrainGridSize)
                    TerraLand.TerraLandRuntime.SmoothNORTH(i);

                QueueOnMainThread(() =>
                {
                    if (i == (int)terrainGridSize)
                        Timing.RunCoroutine(HeightsFromNORTH());

                //                //Timing.RunCoroutine(TerraLand.TerraLandRuntime.LoadTerrainHeightsFromTIFFNORTH(i));
                //
                //
                //                //if(i == (int)terrainGridSize - 1)
                //                if(i == (int)terrainGridSize)
                //                {
                ////                    if(InfiniteTerrain.inProgressWest)
                ////                    {
                ////                        print("Moving North West");
                ////
                ////                        for(int x = 0; x < (int)terrainGridSize - 1; x++)
                ////                            Timing.RunCoroutine(TerraLand.TerraLandRuntime.LoadTerrainHeightsFromTIFFNORTH(InfiniteTerrain.northIndex + x));
                ////                    }
                ////                    else if(InfiniteTerrain.inProgressEast)
                ////                    {
                ////                        for(int x = 0; x < (int)terrainGridSize - 1; x++)
                ////                            Timing.RunCoroutine(TerraLand.TerraLandRuntime.LoadTerrainHeightsFromTIFFNORTH(InfiniteTerrain.northIndex + x));
                ////                    }
                ////                    else
                ////                    {
                //                        for(int x = 0; x < (int)terrainGridSize; x++)
                //                            Timing.RunCoroutine(TerraLand.TerraLandRuntime.LoadTerrainHeightsFromTIFFNORTH(InfiniteTerrain.northIndex + x));
                //                    //}
                //                }
            });
            });
        }

        public void LoadTerrainHeightsSOUTH(int i)
        {
            RunAsync(() =>
            {
                if (i == (int)terrainGridSize)
                    TerraLand.TerraLandRuntime.SmoothSOUTH();

                QueueOnMainThread(() =>
                {
                    if (i == (int)terrainGridSize)
                        Timing.RunCoroutine(HeightsFromSOUTH());
                });
            });
        }

        public void LoadTerrainHeightsEAST(int i)
        {
            RunAsync(() =>
            {
                if (i == (int)terrainGridSize)
                    TerraLand.TerraLandRuntime.SmoothEAST();

                QueueOnMainThread(() =>
                {
                    if (i == (int)terrainGridSize)
                        Timing.RunCoroutine(HeightsFromEAST());
                });
            });
        }

        public void LoadTerrainHeightsWEST(int i)
        {
            RunAsync(() =>
            {
                if (i == (int)terrainGridSize)
                    TerraLand.TerraLandRuntime.SmoothWEST();

                QueueOnMainThread(() =>
                {
                    if (i == (int)terrainGridSize)
                        Timing.RunCoroutine(HeightsFromWEST());
                });
            });
        }

        private IEnumerator<float> HeightsFromNORTH()
        {
            for (int x = 0; x < (int)terrainGridSize; x++)
            {
                Timing.RunCoroutine(TerraLand.TerraLandRuntime.LoadTerrainHeightsFromTIFFNORTH(InfiniteTerrain.northIndex + x));

                float tileDelay = (elevationDelay * Mathf.Pow((TerraLand.TerraLandRuntime.tileResolution - 1) / cellSize, 2f)) + (elevationDelay * 2);
                yield return Timing.WaitForSeconds(tileDelay);
            }
        }

        private IEnumerator<float> HeightsFromSOUTH()
        {
            for (int x = 0; x < (int)terrainGridSize; x++)
            {
                Timing.RunCoroutine(TerraLand.TerraLandRuntime.LoadTerrainHeightsFromTIFFSOUTH(InfiniteTerrain.southIndex + x));

                float tileDelay = (elevationDelay * Mathf.Pow((TerraLand.TerraLandRuntime.tileResolution - 1) / cellSize, 2f)) + elevationDelay;
                yield return Timing.WaitForSeconds(tileDelay);
            }
        }

        private IEnumerator<float> HeightsFromEAST()
        {
            for (int x = 0; x < (int)terrainGridSize; x++)
            {
                Timing.RunCoroutine(TerraLand.TerraLandRuntime.LoadTerrainHeightsFromTIFFEAST(InfiniteTerrain.eastIndex + (x * (int)terrainGridSize)));

                float tileDelay = (elevationDelay * Mathf.Pow((TerraLand.TerraLandRuntime.tileResolution - 1) / cellSize, 2f)) + elevationDelay;
                yield return Timing.WaitForSeconds(tileDelay);
            }
        }

        private IEnumerator<float> HeightsFromWEST()
        {
            for (int x = 0; x < (int)terrainGridSize; x++)
            {
                Timing.RunCoroutine(TerraLand.TerraLandRuntime.LoadTerrainHeightsFromTIFFWEST(InfiniteTerrain.westIndex + (x * (int)terrainGridSize)));

                float tileDelay = (elevationDelay * Mathf.Pow((TerraLand.TerraLandRuntime.tileResolution - 1) / cellSize, 2f)) + elevationDelay;
                yield return Timing.WaitForSeconds(tileDelay);
            }
        }

        public void GetHeightmaps()
        {
            RunAsync(() =>
            {
                TerraLand.TerraLandRuntime.ServerInfoElevation();
            });
        }

        public void GetHeightmapFAR()
        {
            RunAsync(() =>
            {
                TerraLand.TerraLandRuntime.ServerInfoElevationFAR();
            });
        }

        public void GetHeightmapsNORTH(int index)
        {
            RunAsync(() =>
            {
                TerraLand.TerraLandRuntime.ServerInfoElevationNORTH(index);
            });
        }

        public void GetHeightmapsSOUTH()
        {
            RunAsync(() =>
            {
                TerraLand.TerraLandRuntime.ServerInfoElevationSOUTH();
            });
        }

        public void GetHeightmapsEAST()
        {
            RunAsync(() =>
            {
                TerraLand.TerraLandRuntime.ServerInfoElevationEAST();
            });
        }

        public void GetHeightmapsWEST()
        {
            RunAsync(() =>
            {
                TerraLand.TerraLandRuntime.ServerInfoElevationWEST();
            });
        }

        //    public void ServerConnectHeightmap ()
        //    {
        //        RunAsync(()=>
        //        {
        //            TerraLand.TerraLandRuntime.ElevationDownload();
        //
        //            QueueOnMainThread(()=>
        //            {
        //                GenerateTerrainHeights();
        //            });
        //        });
        //    }

        public void ServerConnectHeightmap(int i)
        {
            RunAsync(() =>
            {
                TerraLand.TerraLandRuntime.ElevationDownload(i);

                QueueOnMainThread(() =>
                {
                    TerraLand.TerraLandRuntime.LoadHeights(i);
                });
            });
        }

        public void ServerConnectHeightmapFAR()
        {
            RunAsync(() =>
            {
                TerraLand.TerraLandRuntime.ElevationDownloadFAR();

                QueueOnMainThread(() =>
                {
                    TerraLand.TerraLandRuntime.LoadHeightsFAR();
                });
            });
        }

        public void ServerConnectHeightmapNORTH(int index)
        {
            RunAsync(() =>
            {
                TerraLand.TerraLandRuntime.ElevationDownloadNORTH(index);

                QueueOnMainThread(() =>
                {
                //                //if(!InfiniteTerrain.inProgressSouth)
                //                TerraLand.TerraLandRuntime.LoadHeightsNORTH(index);

                if (!InfiniteTerrain.inProgressSouth)
                    {
                        if (TerraLand.TerraLandRuntime.northCounter == (int)terrainGridSize)
                        {
                            for (int x = 0; x < (int)terrainGridSize; x++)
                                TerraLand.TerraLandRuntime.LoadHeightsNORTH(x + 1);
                        }
                    }
                    else
                    {
                    //TerraLand.TerraLandRuntime.northCounter = 0;
                    //InfiniteTerrain.northTerrains.Clear();
                }

                });
            });
        }

        public void ServerConnectHeightmapSOUTH(int i)
        {
            RunAsync(() =>
            {
                TerraLand.TerraLandRuntime.ElevationDownloadSOUTH(i);

                QueueOnMainThread(() =>
                {
                    if (!InfiniteTerrain.inProgressNorth)
                    {
                        if (TerraLand.TerraLandRuntime.southCounter == (int)terrainGridSize)
                        {
                            for (int x = 0; x < (int)terrainGridSize; x++)
                                TerraLand.TerraLandRuntime.LoadHeightsSOUTH(x + 1);
                        }
                    }
                    else
                    {
                    //TerraLand.TerraLandRuntime.southCounter = 0;
                    //InfiniteTerrain.southTerrains.Clear();
                }
                });
            });
        }

        public void ServerConnectHeightmapEAST(int i)
        {
            RunAsync(() =>
            {
                TerraLand.TerraLandRuntime.ElevationDownloadEAST(i);

                QueueOnMainThread(() =>
                {
                    if (!InfiniteTerrain.inProgressWest)
                    {
                        if (TerraLand.TerraLandRuntime.eastCounter == (int)terrainGridSize)
                        {
                            for (int x = 0; x < (int)terrainGridSize; x++)
                                TerraLand.TerraLandRuntime.LoadHeightsEAST(x + 1);
                        }
                    }
                    else
                    {
                    //TerraLand.TerraLandRuntime.eastCounter = 0;
                    //InfiniteTerrain.eastTerrains.Clear();
                }
                });
            });
        }

        public void ServerConnectHeightmapWEST(int i)
        {
            RunAsync(() =>
            {
                TerraLand.TerraLandRuntime.ElevationDownloadWEST(i);

                QueueOnMainThread(() =>
                {
                    if (!InfiniteTerrain.inProgressEast)
                    {
                        if (TerraLand.TerraLandRuntime.westCounter == (int)terrainGridSize)
                        {
                            for (int x = 0; x < (int)terrainGridSize; x++)
                                TerraLand.TerraLandRuntime.LoadHeightsWEST(x + 1);
                        }
                    }
                    else
                    {
                    //TerraLand.TerraLandRuntime.westCounter = 0;
                    //InfiniteTerrain.westTerrains.Clear();
                }
                });
            });
        }

        public void GenerateTerrainHeights()
        {
            RunAsync(() =>
            {
                TerraLand.TerraLandRuntime.TiffData(TerraLand.TerraLandRuntime.fileNameTerrainData);

                QueueOnMainThread(() =>
                {
                    FinalizeTerrainHeights(TerraLand.TerraLandRuntime.tiffData, TerraLand.TerraLandRuntime.tiffWidth, TerraLand.TerraLandRuntime.tiffLength);
                });
            });
        }

        public void FinalizeTerrainHeights(float[,] data, int width, int height)
        {
            RunAsync(() =>
            {
                TerraLand.TerraLandRuntime.SmoothHeights(data, width, height);

                QueueOnMainThread(() =>
                {
                    TerraLand.TerraLandRuntime.FinalizeHeights();
                });
            });
        }

        public void TerrainFromRAW()
        {
            RunAsync(() =>
            {
                TerraLand.TerraLandRuntime.RawData(TerraLand.TerraLandRuntime.geoDataPathElevation);

                QueueOnMainThread(() =>
                {
                    FinalizeTerrainHeights(TerraLand.TerraLandRuntime.rawData, TerraLand.TerraLandRuntime.m_Width, TerraLand.TerraLandRuntime.m_Height);
                });
            });
        }

        public void TerrainFromTIFF()
        {
            RunAsync(() =>
            {
                TerraLand.TerraLandRuntime.TiffData(TerraLand.TerraLandRuntime.geoDataPathElevation);

                QueueOnMainThread(() =>
                {
                    FinalizeTerrainHeights(TerraLand.TerraLandRuntime.tiffData, TerraLand.TerraLandRuntime.tiffWidth, TerraLand.TerraLandRuntime.tiffLength);
                });
            });
        }

        public void TerrainFromASCII()
        {
            RunAsync(() =>
            {
                TerraLand.TerraLandRuntime.AsciiData(TerraLand.TerraLandRuntime.geoDataPathElevation);

                QueueOnMainThread(() =>
                {
                    FinalizeTerrainHeights(TerraLand.TerraLandRuntime.asciiData, TerraLand.TerraLandRuntime.nCols, TerraLand.TerraLandRuntime.nRows);
                });
            });
        }

        public void ApplyElevationData()
        {
            IEnumerable<string> names = Directory.GetFiles(TerraLand.TerraLandRuntime.dataBasePathElevation, "*.*", SearchOption.AllDirectories)
                .Where(s => s.EndsWith(".asc")
                    || s.EndsWith(".raw")
                    || s.EndsWith(".tif"));

            if (names.ToArray().Length == 0)
                UnityEngine.Debug.LogError("NO AVILABLE DATA - No elevation data is available in selected folder.");
            else
            {
                TerraLand.TerraLandRuntime.geoDataPathElevation = names.ToArray()[0];

                if (TerraLand.TerraLandRuntime.geoDataPathElevation.EndsWith(".asc") || TerraLand.TerraLandRuntime.geoDataPathElevation.EndsWith(".raw") || TerraLand.TerraLandRuntime.geoDataPathElevation.EndsWith(".tif"))
                {
                    String[] pathParts = TerraLand.TerraLandRuntime.geoDataPathElevation.Split(char.Parse("."));
                    TerraLand.TerraLandRuntime.geoDataExtensionElevation = pathParts[pathParts.Length - 1];

                    if (TerraLand.TerraLandRuntime.geoDataExtensionElevation.Equals("raw"))
                    {
                        RunAsync(() =>
                        {
                            TerraLand.TerraLandRuntime.GetElevationFileInfo();

                            QueueOnMainThread(() =>
                            {
                                TerraLand.TerraLandRuntime.ApplyOfflineTerrain();
                            });
                        });
                    }
                }
                else
                    UnityEngine.Debug.LogError("UNKNOWN FORMAT - There are no valid ASCII, RAW or Tiff files in selected folder.");
            }
        }

        public void ApplyImageData()
        {
            TerraLand.TerraLandRuntime.GetFolderInfo(TerraLand.TerraLandRuntime.dataBasePathImagery);

            if (TerraLand.TerraLandRuntime.totalImagesDataBase == 0)
            {
                TerraLand.TerraLandRuntime.geoImagesOK = false;
                UnityEngine.Debug.LogError("There are no images in data base!");
            }
            else
                TerraLand.TerraLandRuntime.geoImagesOK = true;

            if (TerraLand.TerraLandRuntime.terrainChunks > TerraLand.TerraLandRuntime.totalImagesDataBase)
            {
                TerraLand.TerraLandRuntime.geoImagesOK = false;
                UnityEngine.Debug.LogError("No sufficient images to texture terrains. Select a lower Grid Size for terrains");
            }
            else
                TerraLand.TerraLandRuntime.geoImagesOK = true;

            if (TerraLand.TerraLandRuntime.geoImagesOK)
            {
                using (Image sourceImage = Image.FromFile(TerraLand.TerraLandRuntime.geoImageNames[0]))
                {
                    TerraLand.TerraLandRuntime.imageWidth = sourceImage.Width;
                    TerraLand.TerraLandRuntime.imageHeight = sourceImage.Height;
                    imageResolution = TerraLand.TerraLandRuntime.imageWidth;
                }

                for (int i = 0; i < TerraLand.TerraLandRuntime.geoImageNames.Length; i++)
                {
                    TerraLand.TerraLandRuntime.images.Add(new Texture2D(TerraLand.TerraLandRuntime.imageWidth, TerraLand.TerraLandRuntime.imageHeight, TextureFormat.RGB24, true, true));
                    TerraLand.TerraLandRuntime.images[i].wrapMode = TextureWrapMode.Clamp;
                }

                RunAsync(() =>
                {
                    TerraLand.TerraLandRuntime.imageBytes = new List<byte[]>();

                    for (int i = 0; i < TerraLand.TerraLandRuntime.geoImageNames.Length; i++)
                        TerraLand.TerraLandRuntime.DownloadImageData(TerraLand.TerraLandRuntime.geoImageNames[i]);

                    QueueOnMainThread(() =>
                    {
                        Timing.RunCoroutine(TerraLand.TerraLandRuntime.FillImages(TerraLand.TerraLandRuntime.totalImagesDataBase));
                    });
                });
            }
        }

        public void GetSatelliteImages()
        {
            RunAsync(() =>
            {
                TerraLand.TerraLandRuntime.ServerInfoImagery();
            });
        }

        public void GetSatelliteImagesFAR()
        {
            RunAsync(() =>
            {
                TerraLand.TerraLandRuntime.ServerInfoImageryFAR();
            });
        }

        public void GetSatelliteImagesNORTH()
        {
            RunAsync(() =>
            {
                TerraLand.TerraLandRuntime.ServerInfoImageryNORTH();
            });
        }

        public void GetSatelliteImagesSOUTH()
        {
            RunAsync(() =>
            {
                TerraLand.TerraLandRuntime.ServerInfoImagerySOUTH();
            });
        }

        public void GetSatelliteImagesEAST()
        {
            RunAsync(() =>
            {
                TerraLand.TerraLandRuntime.ServerInfoImageryEAST();
            });
        }

        public void GetSatelliteImagesWEST()
        {
            RunAsync(() =>
            {
                TerraLand.TerraLandRuntime.ServerInfoImageryWEST();
            });
        }

        public void ServerConnectImagery(int i)
        {
            RunAsync(() =>
            {
                TerraLand.TerraLandRuntime.ImageDownloader(i);

                QueueOnMainThread(() =>
                {
                    if (TerraLand.TerraLandRuntime.allBlack)
                    {
                        UnityEngine.Debug.LogError("UNAVAILABLE IMAGERY - There is no available imagery at this zoom level. Decrease TERRAIN GRID SIZE/IMAGE RESOLUTION or increase AREA SIZE.");
                        TerraLand.TerraLandRuntime.imageDownloadingStarted = false;
                        return;
                    }

                    if (progressiveTexturing)
                        Timing.RunCoroutine(TerraLand.TerraLandRuntime.FillImage(i));
                    else
                    {
                        if (TerraLand.TerraLandRuntime.downloadedImageIndex == TerraLand.TerraLandRuntime.totalImages)
                            Timing.RunCoroutine(TerraLand.TerraLandRuntime.FillImages(TerraLand.TerraLandRuntime.totalImages));
                    }
                });
            });
        }

        public void ServerConnectImageryFAR()
        {
            RunAsync(() =>
            {
                TerraLand.TerraLandRuntime.ImageDownloaderFAR();

                QueueOnMainThread(() =>
                {
                    Timing.RunCoroutine(TerraLand.TerraLandRuntime.FillImageFAR());
                });
            });
        }

        public void ServerConnectImageryNORTH(int i)
        {
            RunAsync(() =>
            {
                TerraLand.TerraLandRuntime.ImageDownloaderNORTH(i);

                QueueOnMainThread(() =>
                {
                    if (TerraLand.TerraLandRuntime.allBlack)
                        UnityEngine.Debug.LogError("UNAVAILABLE IMAGERY - There is no available imagery at this zoom level. Decrease TERRAIN GRID SIZE/IMAGE RESOLUTION or increase AREA SIZE.");

                    Timing.RunCoroutine(TerraLand.TerraLandRuntime.FillImageNORTH(i));
                });
            });
        }

        public void ServerConnectImagerySOUTH(int i)
        {
            RunAsync(() =>
            {
                TerraLand.TerraLandRuntime.ImageDownloaderSOUTH(i);

                QueueOnMainThread(() =>
                {
                    if (TerraLand.TerraLandRuntime.allBlack)
                        UnityEngine.Debug.LogError("UNAVAILABLE IMAGERY - There is no available imagery at this zoom level. Decrease TERRAIN GRID SIZE/IMAGE RESOLUTION or increase AREA SIZE.");

                    Timing.RunCoroutine(TerraLand.TerraLandRuntime.FillImageSOUTH(i));
                });
            });
        }

        public void ServerConnectImageryEAST(int i)
        {
            RunAsync(() =>
            {
                TerraLand.TerraLandRuntime.ImageDownloaderEAST(i);

                QueueOnMainThread(() =>
                {
                    if (TerraLand.TerraLandRuntime.allBlack)
                        UnityEngine.Debug.LogError("UNAVAILABLE IMAGERY - There is no available imagery at this zoom level. Decrease TERRAIN GRID SIZE/IMAGE RESOLUTION or increase AREA SIZE.");

                    Timing.RunCoroutine(TerraLand.TerraLandRuntime.FillImageEAST(i));
                });
            });
        }

        public void ServerConnectImageryWEST(int i)
        {
            RunAsync(() =>
            {
                TerraLand.TerraLandRuntime.ImageDownloaderWEST(i);

                QueueOnMainThread(() =>
                {
                    if (TerraLand.TerraLandRuntime.allBlack)
                        UnityEngine.Debug.LogError("UNAVAILABLE IMAGERY - There is no available imagery at this zoom level. Decrease TERRAIN GRID SIZE/IMAGE RESOLUTION or increase AREA SIZE.");

                    Timing.RunCoroutine(TerraLand.TerraLandRuntime.FillImageWEST(i));
                });
            });
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

        private void UnloadResources()
        {
            UnloadAllAssets();
            Resources.UnloadUnusedAssets();
            GC.Collect();
        }

        private void UnloadAllAssets()
        {
            try
            {
                Destroy(TerraLand.TerraLandRuntime.terrain);
                Destroy(TerraLand.TerraLandRuntime.firstTerrain);
                Destroy(TerraLand.TerraLandRuntime.secondaryTerrain);
                Destroy(TerraLand.TerraLandRuntime.currentTerrain);
                Destroy(TerraLand.TerraLandRuntime.farImage);
                Destroy(TerraLand.TerraLandRuntime.data);

                TerraLand.TerraLandRuntime.webClientTerrain = null;
                TerraLand.TerraLandRuntime.tiffData = null;
                TerraLand.TerraLandRuntime.tiffDataASCII = null;
                TerraLand.TerraLandRuntime.tiffDataFAR = null;
                TerraLand.TerraLandRuntime.tiffDataASCIIFAR = null;
                TerraLand.TerraLandRuntime.finalHeights = null;
                TerraLand.TerraLandRuntime.heightmapCell = null;
                TerraLand.TerraLandRuntime.heightmapCellSec = null;
                TerraLand.TerraLandRuntime.heightmapCellFar = null;
                TerraLand.TerraLandRuntime.rawData = null;
                TerraLand.TerraLandRuntime.webClientImage = null;
                TerraLand.TerraLandRuntime.smData = null;
                TerraLand.TerraLandRuntime.farImageBytes = null;
                TerraLand.TerraLandRuntime.asciiData = null;
                TerraLand.TerraLandRuntime.tiffDataFar = null;
                TerraLand.TerraLandRuntime._terrainDict = null;
                TerraLand.TerraLandRuntime.heights = null;
                TerraLand.TerraLandRuntime.secondHeights = null;

                for (int i = 0; i < TerraLand.TerraLandRuntime.croppedTerrains.Count; i++)
                    Destroy(TerraLand.TerraLandRuntime.croppedTerrains[i]);

                for (int i = 0; i < TerraLand.TerraLandRuntime.images.Count; i++)
                    Destroy(TerraLand.TerraLandRuntime.images[i]);

                for (int i = 0; i < TerraLand.TerraLandRuntime.imageBytes.Count; i++)
                    TerraLand.TerraLandRuntime.imageBytes[i] = null;

                for (int i = 0; i < TerraLand.TerraLandRuntime.tiffDataDynamic.Count; i++)
                    TerraLand.TerraLandRuntime.tiffDataDynamic[i] = null;

                for (int i = 0; i < TerraLand.TerraLandRuntime._terrains.Length; i++)
                    Destroy(TerraLand.TerraLandRuntime._terrains[i]);

                if (TerraLand.TerraLandRuntime.stitchingTerrainsList != null)
                {
                    for (int i = 0; i < TerraLand.TerraLandRuntime.stitchingTerrainsList.Count; i++)
                        Destroy(TerraLand.TerraLandRuntime.stitchingTerrainsList[i]);
                }
            }
            catch { }
        }

        public void OnDisable()
        {
            UnloadResources();
        }
    }
}

