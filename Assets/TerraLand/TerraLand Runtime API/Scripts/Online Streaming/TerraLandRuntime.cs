using UnityEngine;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using BitMiracle.LibTiff.Classic;
using MEC;

namespace TerraLand
{
    public class TerraLandRuntime : MonoBehaviour
    {
        private static Runtime runTime;
        private static FloatingOriginAdvanced floatingOriginAdvanced;

        public static string top, left, bottom, right;
        public static string topFar, leftFar, bottomFar, rightFar;
        static float areaSizeLat;
        static float areaSizeLon;
        static int gridPerTerrain = 1;

        public enum Neighbourhood
        {
            Moore = 0,
            VonNeumann = 1
        }
        static Neighbourhood neighbourhood = Neighbourhood.Moore;

        //public static string projectPath;
        public static bool imageDownloadingStarted = false;
        string downloadDateImagery;
        public static Terrain terrain;
        public static GameObject splittedTerrains;
        public static Terrain firstTerrain;
        public static Terrain secondaryTerrain;
        static bool secondaryTerrainInProgress = true;
        static int splitSizeFinal;
        public static int gridSizeTerrain;
        public static List<Terrain> croppedTerrains;
        public static int terrainChunks;
        public static int checkLength;
        static int terrainResolutionDownloading;

        static List<float> topCorner, bottomCorner, leftCorner, rightCorner;
        static GameObject terrainsParent;

        static int terrainsLong, terrainsWide;
        static float oldWidth, oldHeight, oldLength;

        static float terrainSizeNewX;
        static float terrainSizeNewY;
        static float terrainSizeNewZ;

        static float newWidth, newLength;
        static float xPos, yPos, zPos;
        static int newHeightMapResolution, newEvenHeightMapResolution;
        static int heightmapResolutionSplit;

        static float terrainSizeFactor;
        static float terrainSizeX;
        static float terrainSizeY;

        static GameObject[] terrainGameObjects;
        public static Terrain currentTerrain;
        public static TerrainData data;

        static int arrayPos;

        private static TerraLandWorldImagery.World_Imagery_MapServer mapservice;
        //private static TerraLandWorldElevation.TopoBathy_ImageServer mapserviceElevation = new TerraLandWorldElevation.TopoBathy_ImageServer();
        private static TerraLandWorldElevation.TopoBathy_ImageServer mapserviceElevation;
        static TerraLandWorldImagery.MapServerInfo mapinfo;
        static TerraLandWorldImagery.MapDescription mapdesc;

        static string token = "";
        public static string fileNameTerrainData;
        public static List<string> fileNameTerrainDataDynamic;

        public static WebClient webClientTerrain;

        public static int tiffWidth;
        public static int tiffLength;
        public static float[,] tiffData;
        public static float[,] tiffDataASCII;

        public static int tiffWidthFAR;
        public static int tiffLengthFAR;
        public static float[,] tiffDataFAR;
        public static float[,] tiffDataASCIIFAR;

        public static List<float> highestPoints;
        static float highestPoint;
        static float lowestPoint;
        static float initialTerrainWidth;

        static int heightmapResX;
        static int heightmapResY;
        static int heightmapResFinalX;
        static int heightmapResFinalY;
        static int heightmapResXAll;
        static int heightmapResYAll;
        static int heightmapResFinalXAll;
        static int heightmapResFinalYAll;

        static int heightmapResXFAR;
        static int heightmapResYFAR;
        static int heightmapResFinalXFAR;
        static int heightmapResFinalYFAR;
        static int heightmapResFinalXAllFAR;
        static int heightmapResFinalYAllFAR;

        static float everestPeak = 8848.0f;
        static float currentHeight;

        public static float smoothBlend = 0.8f;
        public static int smoothBlendIndex = 0;

        public static float[,] finalHeights;

        public static int totalImages;
        public static int totalImagesDataBase;
        static int gridNumber;

        public static float[,] heightmapCell;
        public static float[,] heightmapCellSec;
        public static float[,] heightmapCellFar;

        public static string dataBasePathElevation;
        public static string geoDataPathElevation;
        public static string geoDataExtensionElevation;
        public static string dataBasePathImagery;
        //static string geoDataPathImagery;
        //static string geoDataExtensionImagery;

        public static float[,] rawData;
        public static int m_Width = 1;
        public static int m_Height = 1;

        //https://elevation.arcgis.com/arcgis/services/WorldElevation/Terrain/ImageServer?token=
        private static string elevationURL = "https://elevation.arcgis.com/arcgis/services/WorldElevation/TopoBathy/ImageServer?token=";

        private const string tokenURL = "https://www.arcgis.com/sharing/rest/oauth2/token/authorize?client_id=n0dpgUwqazrQTyXZ&client_secret=3d4867add8ee47b6ac0c498198995298&grant_type=client_credentials&expiration=20160";

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

        static string[] terrainNames;
        static GameObject tempParnet;

        public static int generatedTerrainsCount = 0;
        static int taskIndex;
        static List<int> spiralIndex;
        static List<Vector2> spiralCell;

        public static List<Texture2D> images;
        public static Texture2D farImage;
        public static SplatPrototype[] terrainTextures;
        public static float[,,] smData;
        static float cellSizeX;
        static float cellSizeY;
        static float[] imageXOffset;
        static float[] imageYOffset;

        static float splatNormalizeX;
        static float splatNormalizeY;

        //static string directoryPathImagery;
        static int imagesPerTerrain;
        static bool multipleTerrainsTiling;
        static int tileGrid;

        public static string[] geoImageNames;
        public static WebClient webClientImage;
        public static List<byte[]> imageBytes;
        public static byte[] farImageBytes;
        public static int imageWidth;
        public static int imageHeight;
        public static bool geoImagesOK = true;

        public static bool allBlack = false;

        static double yMaxTop;
        static double xMinLeft;
        static double yMinBottom;
        static double xMaxRight;

        static double latCellSize;
        static double lonCellSize;

        public static double[] xMin;
        public static double[] yMin;
        public static double[] xMax;
        public static double[] yMax;

        public static double[] xMinImage;
        public static double[] yMinImage;
        public static double[] xMaxImage;
        public static double[] yMaxImage;

        public static int downloadedImageIndex;

        public static double[] latCellTop;
        public static double[] latCellBottom;
        public static double[] lonCellLeft;
        public static double[] lonCellRight;

        static int compressionQuality = 100;
        static bool availableImageryCheked = false;

        public static int nCols;
        public static int nRows;
        public static float[,] asciiData;
        public static int downloadedHeightmapIndex;
        static int downloadedFarTerrains;
        public static int downloadedFarTerrainImages;

        static string tempFolder;
        public static List<float[,]> tiffDataDynamic;
        public static float[,] tiffDataFar;
        public static int tileResolution;

        public static Dictionary<int[],Terrain> _terrainDict;
        public static Terrain[] _terrains;
        public enum Side
        {
            LeftSide,
            RightSide,
            TopSide,
            BottomSide
        }
        static int concurrentUpdates = 0;
        static bool hasTop = false;
        static bool hasRight = false;

        static int stitchedTerrainsCount = 0;

        static double latUser;
        static double lonUser;
        static double earthRadius = 6378137;

        public static int northCounter = 0;
        public static int southCounter = 0;
        public static int eastCounter = 0;
        public static int westCounter = 0;
        public static int northCounterGenerated = 0;
        public static int southCounterGenerated = 0;
        public static int eastCounterGenerated = 0;
        public static int westCounterGenerated = 0;

        public static List<Terrain> stitchingTerrainsList;

        public static bool sceneIsInitialized = false;
        public static bool imagesAreGenerated = false;
        public static bool terrainsAreGenerated = false;
        public static bool farTerrainIsGenerated = false;
        public static bool worldIsGenerated = false;

        public static float realTerrainHeight;
        static float realTerrainWidth;
        static float realTerrainLength;

        static string farTerrainTiffName;
        static double areaOffsetMeters;
        //static string direction = "";
        static GameObject farTerrainDummy;
        static bool farTerrainInProgress;
        static float farTerrainSize;
        static bool statusIsOKNorth;
        static bool statusIsOKSouth;
        static bool statusIsOKEast;
        static bool statusIsOKWest;

        public static float[,] heights;
        public static float[,] secondHeights;

        static Image satImage;

        public static bool stitchingInProgress;
        public static List<Terrain> terrainsInProgress;

        public static bool generationIsBusyNORTH;
        public static bool generationIsBusySOUTH;
        public static bool generationIsBusyEAST;
        public static bool generationIsBusyWEST;

        //private static int northIterations = 0;
        //private static bool xxx = false;

        //        private static bool skipNorthCell1st = false;
        //        private static bool skipNorthCellLast = false;
        //        private static bool skipSouthCell1st = false;
        //        private static bool skipSouthCellLast = false;
        //        private static bool skipWestCell1st = false;
        //        private static bool skipWestCellLast = false;
        //        private static bool skipEastCell1st = false;
        //        private static bool skipEastCellLast = false;


        private static void SelectDatabaseDirectory ()
        {
            //#if UNITY_EDITOR
            //dataBasePath = UnityEditor.EditorUtility.OpenFolderPanel("Select Database Folder", "", "");
            //#endif
        }

        private static void LoadDatabaseElevation ()
        {
            dataBasePathElevation = runTime.dataBasePath + "/Elevation/";

            if(Directory.Exists(dataBasePathElevation))
                runTime.ApplyElevationData();
            else
                UnityEngine.Debug.LogError("UNKNOWN DATABASE DIRECTORY (ELEVATION) - Select a directory which includes Elevation data folder.");
        }

        private static void LoadDatabaseImagery ()
        {
            //dataBasePathImagery = runTime.dataBasePath + "/Imagery/";
            dataBasePathImagery = runTime.dataBasePath + "/Imagery/512/64/"; // 1 4 16 64 256 1024

            if(Directory.Exists(dataBasePathImagery))
                runTime.ApplyImageData();
            else
                UnityEngine.Debug.LogError("UNKNOWN DATABASE DIRECTORY (IMAGERY) - Select a directory which includes Imagery data folder.");
        }

        public static void Initialize ()
        {
            runTime = GameObject.Find("World Generator").GetComponent<Runtime>();

            areaSizeLat = runTime.areaSize;
            areaSizeLon = runTime.areaSize;

            GetTerrainBounds();
            GetTerrainBoundsFar();

            gridSizeTerrain = (int)runTime.terrainGridSize;
            tileResolution = (runTime.heightmapResolution / gridSizeTerrain) + 1;
            terrainSizeNewX = areaSizeLon * 1000f;
            terrainSizeNewY = 4000;
            terrainSizeNewZ = areaSizeLat * 1000f;
            generatedTerrainsCount = 0;
            taskIndex = runTime.concurrentTasks;
            concurrentUpdates = 0;
            checkLength = runTime.stitchDistance;

            terrainsInProgress = new List<Terrain>();

            //projectPath = UnityEngine.Application.dataPath.Replace("Assets", "");

            try
            {
                terrainChunks = (int)(Mathf.Pow(gridSizeTerrain, 2));
                heightmapResolutionSplit = runTime.heightmapResolution / (int)Mathf.Sqrt((float)terrainChunks);
                splitSizeFinal = (int)Mathf.Sqrt(terrainChunks);
                totalImages = (int)(Mathf.Pow(gridPerTerrain, 2)) * terrainChunks;
                gridNumber = (int)(Mathf.Sqrt(totalImages));
            }
            catch(Exception e)
            {
                UnityEngine.Debug.LogError(e);
            }

            terrainSizeNewX = areaSizeLon * 1000f;
            terrainSizeNewZ = areaSizeLat * 1000f;
            terrainSizeFactor = areaSizeLat / areaSizeLon;
            terrainSizeNewZ = terrainSizeNewX * terrainSizeFactor;

            CheckTerrainSizeUnits();

            if(runTime.concurrentTasks > terrainChunks)
                runTime.concurrentTasks = terrainChunks;
            else if(runTime.concurrentTasks < 1)
                runTime.concurrentTasks = 1;

            if(runTime.progressiveGeneration)
                runTime.spiralGeneration = false;

            if(runTime.spiralGeneration)
                SpiralOrder();
            else
                NormalOrder();

            if(!runTime.IsCustomGeoServer)
            {
                SetupDownloaderElevation();
                InitializeDownloader();
                runTime.GetHeightmaps();

                if(runTime.farTerrain)
                    runTime.GetHeightmapFAR();

                if(!runTime.elevationOnly)
                {
                    GetImagesInfo();
                    ImageTilerOnline();
                    runTime.GetSatelliteImages();

                    if(runTime.farTerrain)
                        runTime.GetSatelliteImagesFAR();
                }
            }
            else
            {
                SelectDatabaseDirectory();
                LoadDatabaseElevation();
                LoadDatabaseImagery();
            }
        }

        private static void SpiralOrder ()
        {
            spiralIndex = new List<int>();
            int[,] indexFromCenter = new int[gridSizeTerrain, gridSizeTerrain];
            int length = gridSizeTerrain;
            int index = 0;

            for(int i = 0; i < length; i++)
                for(int j = 0; j < length; j++)
                    indexFromCenter[i, j] = index++;

            SpiralOrderOperation(indexFromCenter);

            spiralIndex.Reverse();
        }

        private static void NormalOrder ()
        {
            for(int i = 0; i < terrainChunks; i++)
                spiralIndex.Add(i);
        }

        public static void GetTerrainBounds ()
        {
            try
            {
                latUser = double.Parse(runTime.latitudeUser);
                lonUser = double.Parse(runTime.longitudeUser);

                //offsets in meters
                double dn = (areaSizeLat * 1000f) / 2.0;
                double de = (areaSizeLon * 1000f) / 2.0;

                //Coordinate offsets in radians
                double dLat = dn / earthRadius;
                double dLon = de / (earthRadius * Math.Cos(Math.PI * latUser / 180));

                top = (latUser + dLat * 180 / Math.PI).ToString(); // Top
                left = (lonUser - dLon * 180 / Math.PI).ToString(); // Left
                bottom = (latUser - dLat * 180 / Math.PI).ToString(); // Bottom
                right = (lonUser + dLon * 180 / Math.PI).ToString(); // Right
            }
            catch(Exception e)
            {
                UnityEngine.Debug.LogError(e);
            }
        }

        public static void GetTerrainBoundsFar ()
        {
            try
            {
                latUser = double.Parse(runTime.latitudeUser);
                lonUser = double.Parse(runTime.longitudeUser);

                //offsets in meters
                double dn = (areaSizeLat * 1000f * runTime.areaSizeFarMultiplier) / 2.0;
                double de = (areaSizeLon * 1000f * runTime.areaSizeFarMultiplier) / 2.0;

                //Coordinate offsets in radians
                double dLat = dn / earthRadius;
                double dLon = de / (earthRadius * Math.Cos(Math.PI * latUser / 180));

                topFar = (latUser + dLat * 180 / Math.PI).ToString(); // Top
                leftFar = (lonUser - dLon * 180 / Math.PI).ToString(); // Left
                bottomFar = (latUser - dLat * 180 / Math.PI).ToString(); // Bottom
                rightFar = (lonUser + dLon * 180 / Math.PI).ToString(); // Right
            }
            catch(Exception e)
            {
                UnityEngine.Debug.LogError(e);
            }
        }

        public static void GetTerrainBoundsNORTH ()
        {
            try
            {
                areaOffsetMeters = (areaSizeLat * 1000f) / gridSizeTerrain;
                double areaOffsetDegrees = (areaOffsetMeters / earthRadius) * 180 / Math.PI;
                top = (double.Parse(top) + areaOffsetDegrees).ToString();
                bottom = (double.Parse(bottom) + areaOffsetDegrees).ToString();

                InitializeDownloader();
                GetDynamicTerrainNORTH(0);

                if(!runTime.elevationOnly)
                    runTime.GetSatelliteImagesNORTH();

                if(runTime.farTerrain)
                {
                    topFar = (double.Parse(topFar) + areaOffsetDegrees).ToString();
                    bottomFar = (double.Parse(bottomFar) + areaOffsetDegrees).ToString();

                    farTerrainDummy.transform.position = new Vector3
                        (
                            farTerrainDummy.transform.position.x,
                            farTerrainDummy.transform.position.y,
                            farTerrainDummy.transform.position.z + (float)areaOffsetMeters
                        );
                }

                //direction = "north";

                if(!InfiniteTerrain.inProgressSouth && !InfiniteTerrain.inProgressEast && !InfiniteTerrain.inProgressWest)
                {
                    //if(InfiniteTerrain.isOneStepNorth)
                    statusIsOKNorth = true;
                }   
                else
                    statusIsOKNorth = false;

                if(runTime.farTerrain && statusIsOKNorth && !farTerrainInProgress)
                {
                    farTerrainInProgress = true;
                    SwitchFarTerrains(farTerrainDummy.transform.position);
                    runTime.GetHeightmapFAR();

                    if(!runTime.elevationOnly)
                        runTime.GetSatelliteImagesFAR();
                }
            }
            catch(Exception e)
            {
                UnityEngine.Debug.LogError(e);
            }
        }

        public static void GetTerrainBoundsNORTH (int index)
        {
            try
            {
//                //print();
//
//                //areaOffsetMeters = (areaSizeLat * 1000f) / gridSizeTerrain;
//
//                if(northIterations == 4)
//                    northIterations = gridSizeTerrain;
//
//                //areaOffsetMeters = (areaSizeLat * 1000f) - (areaSizeLat * 1000f / gridSizeTerrain);
//                areaOffsetMeters = (areaSizeLat * 1000f) - (areaSizeLat * 1000f / northIterations);
//
//                northIterations = 4;
//
//                //if(northIterations <= 1)
//                    //northIterations = gridSizeTerrain;


                if(!InfiniteTerrain.northDetected)
                {
                    areaOffsetMeters = (areaSizeLat * 1000f) - (areaSizeLat * 1000f / gridSizeTerrain);

                    double areaOffsetDegrees = (areaOffsetMeters / earthRadius) * 180 / Math.PI;
                    top = (double.Parse(top) + areaOffsetDegrees).ToString();
                    bottom = (double.Parse(bottom) + areaOffsetDegrees).ToString();

                    InitializeDownloader();

                    InfiniteTerrain.northDetected = true;
                    //GetDynamicTerrainNORTH(index);
                }

                GetDynamicTerrainNORTH(index);




//                GetDynamicTerrainNORTH();
//
//                if(!runTime.elevationOnly)
//                    runTime.GetSatelliteImagesNORTH();
//
//                if(runTime.farTerrain)
//                {
//                    topFar = (double.Parse(topFar) + areaOffsetDegrees).ToString();
//                    bottomFar = (double.Parse(bottomFar) + areaOffsetDegrees).ToString();
//
//                    farTerrainDummy.transform.position = new Vector3
//                        (
//                            farTerrainDummy.transform.position.x,
//                            farTerrainDummy.transform.position.y,
//                            farTerrainDummy.transform.position.z + (float)areaOffsetMeters
//                        );
//                }
//
//                direction = "north";
//                statusIsOKNorth = true;
//
////                statusIsOKNorth = false;
////
////                if(InfiniteTerrain.inProgressSouth || InfiniteTerrain.inProgressEast || InfiniteTerrain.inProgressWest)
////                    statusIsOKNorth = false;
////                else
////                {
////                    if(InfiniteTerrain.isOneStepNorth)
////                      statusIsOKNorth = true;
////                }
//
//                if(runTime.farTerrain && statusIsOKNorth && !farTerrainInProgress)
//                {
//                    farTerrainInProgress = true;
//                    SwitchFarTerrains(farTerrainDummy.transform.position);
//                    runTime.GetHeightmapFAR();
//
//                    if(!runTime.elevationOnly)
//                        runTime.GetSatelliteImagesFAR();
//                }
            }
            catch(Exception e)
            {
                UnityEngine.Debug.LogError(e);
            }
        }

        public static void GetTerrainBoundsSOUTH ()
        {
            try
            {
                areaOffsetMeters = (areaSizeLat * 1000f) / gridSizeTerrain;
                double areaOffsetDegrees = (areaOffsetMeters / earthRadius) * 180 / Math.PI;
                top = (double.Parse(top) - areaOffsetDegrees).ToString();
                bottom = (double.Parse(bottom) - areaOffsetDegrees).ToString();

                InitializeDownloader();
                GetDynamicTerrainSOUTH();
                
                if(!runTime.elevationOnly)
                    runTime.GetSatelliteImagesSOUTH();

                if(runTime.farTerrain)
                {
                    topFar = (double.Parse(topFar) - areaOffsetDegrees).ToString();
                    bottomFar = (double.Parse(bottomFar) - areaOffsetDegrees).ToString();

                    farTerrainDummy.transform.position = new Vector3
                        (
                            farTerrainDummy.transform.position.x,
                            farTerrainDummy.transform.position.y,
                            farTerrainDummy.transform.position.z - (float)areaOffsetMeters
                        );
                }

                //direction = "south";

                if(!InfiniteTerrain.inProgressNorth && !InfiniteTerrain.inProgressEast && !InfiniteTerrain.inProgressWest)
                {
                    //if(InfiniteTerrain.isOneStepSouth)
                    statusIsOKSouth = true;
                }
                else
                    statusIsOKSouth = false;

                if(runTime.farTerrain && statusIsOKSouth && !farTerrainInProgress)
                {
                    farTerrainInProgress = true;
                    SwitchFarTerrains(farTerrainDummy.transform.position);
                    runTime.GetHeightmapFAR();

                    if(!runTime.elevationOnly)
                        runTime.GetSatelliteImagesFAR();
                }
            }
            catch(Exception e)
            {
                UnityEngine.Debug.LogError(e);
            }
        }

        public static void GetTerrainBoundsEAST ()
        {
            try
            {
                double centerLat = (double.Parse(top) + double.Parse(bottom)) / 2.0;
                areaOffsetMeters = (areaSizeLat * 1000f) / gridSizeTerrain;
                double areaOffsetDegrees = (areaOffsetMeters / (earthRadius * Math.Cos(Math.PI * centerLat / 180))) * 180 / Math.PI;
                right = (double.Parse(right) + areaOffsetDegrees).ToString();
                left = (double.Parse(left) + areaOffsetDegrees).ToString();

                InitializeDownloader();
                GetDynamicTerrainEAST();
                
                if(!runTime.elevationOnly)
                    runTime.GetSatelliteImagesEAST();

                if(runTime.farTerrain)
                {
                    rightFar = (double.Parse(rightFar) + areaOffsetDegrees).ToString();
                    leftFar = (double.Parse(leftFar) + areaOffsetDegrees).ToString();

                    farTerrainDummy.transform.position = new Vector3
                        (
                            farTerrainDummy.transform.position.x + (float)areaOffsetMeters,
                            farTerrainDummy.transform.position.y,
                            farTerrainDummy.transform.position.z
                        );
                }

                //direction = "east";

                if(!InfiniteTerrain.inProgressNorth && !InfiniteTerrain.inProgressSouth && !InfiniteTerrain.inProgressWest)
                {
                    //if(InfiniteTerrain.isOneStepEast)
                    statusIsOKEast = true;
                }
                else
                    statusIsOKEast = false;

                if(runTime.farTerrain && statusIsOKEast && !farTerrainInProgress)
                {
                    farTerrainInProgress = true;
                    SwitchFarTerrains(farTerrainDummy.transform.position);
                    runTime.GetHeightmapFAR();

                    if(!runTime.elevationOnly)
                        runTime.GetSatelliteImagesFAR();
                }
            }
            catch(Exception e)
            {
                UnityEngine.Debug.LogError(e);
            }
        }

        public static void GetTerrainBoundsWEST ()
        {
            try
            {
                double centerLat = (double.Parse(top) + double.Parse(bottom)) / 2.0;
                areaOffsetMeters = (areaSizeLat * 1000f) / gridSizeTerrain;
                double areaOffsetDegrees = (areaOffsetMeters / (earthRadius * Math.Cos(Math.PI * centerLat / 180))) * 180 / Math.PI;
                right = (double.Parse(right) - areaOffsetDegrees).ToString();
                left = (double.Parse(left) - areaOffsetDegrees).ToString();

                InitializeDownloader();
                GetDynamicTerrainWEST();

                if(!runTime.elevationOnly)
                    runTime.GetSatelliteImagesWEST();

                if(runTime.farTerrain)
                {
                    rightFar = (double.Parse(rightFar) - areaOffsetDegrees).ToString();
                    leftFar = (double.Parse(leftFar) - areaOffsetDegrees).ToString();

                    farTerrainDummy.transform.position = new Vector3
                        (
                            farTerrainDummy.transform.position.x - (float)areaOffsetMeters,
                            farTerrainDummy.transform.position.y,
                            farTerrainDummy.transform.position.z
                        );
                }

                //direction = "west";

                if(!InfiniteTerrain.inProgressNorth && !InfiniteTerrain.inProgressSouth && !InfiniteTerrain.inProgressEast)
                {
                    //if(InfiniteTerrain.isOneStepWest)
                    statusIsOKWest = true;
                }
                else
                    statusIsOKWest = false;

                if(runTime.farTerrain && statusIsOKWest && !farTerrainInProgress)
                {
                    farTerrainInProgress = true;
                    SwitchFarTerrains(farTerrainDummy.transform.position);
                    runTime.GetHeightmapFAR();

                    if(!runTime.elevationOnly)
                        runTime.GetSatelliteImagesFAR();
                }
            }
            catch(Exception e)
            {
                UnityEngine.Debug.LogError(e);
            }
        }

        private static void SwitchFarTerrains (Vector3 currentPos)
        {
            if(secondaryTerrainInProgress)
                terrain = secondaryTerrain;
            else
                terrain = firstTerrain;

            terrain.transform.position = currentPos;
        }

        private static void SwitchFarTerrainsCompleted ()
        {
            if(secondaryTerrainInProgress)
            {
                firstTerrain.drawHeightmap = false;
                secondaryTerrain.drawHeightmap = true;
            }
            else
            {
                secondaryTerrain.drawHeightmap = false;
                firstTerrain.drawHeightmap = true;
            }

            farTerrainInProgress = false;
            secondaryTerrainInProgress = !secondaryTerrainInProgress;
        }

        public static void GetDynamicTerrainNORTH (int index)
        {
            runTime.GetHeightmapsNORTH(index);
        }

        public static void GetDynamicTerrainSOUTH ()
        {
            runTime.GetHeightmapsSOUTH();
        }

        public static void GetDynamicTerrainEAST ()
        {
            runTime.GetHeightmapsEAST();
        }

        public static void GetDynamicTerrainWEST ()
        {
            runTime.GetHeightmapsWEST();
        }

        private static void SpiralOrderOperation(int[,] matrix)
        {
            if(matrix.Length == 0)
                return;

            int topIndex = 0;
            int downIndex = gridSizeTerrain - 1;
            int leftIndex = 0;
            int rightIndex = gridSizeTerrain - 1;

            while(true)
            {
                // top row
                for(int j = leftIndex; j <= rightIndex; ++j)
                    spiralIndex.Add(matrix[topIndex, j]);

                topIndex++;

                if(topIndex > downIndex || leftIndex > rightIndex)
                    break;

                // rightmost column
                for(int i = topIndex; i <= downIndex; ++i)
                    spiralIndex.Add(matrix[i, rightIndex]);

                rightIndex--;

                if(topIndex > downIndex || leftIndex > rightIndex)
                    break;

                // bottom row
                for(int j = rightIndex; j >= leftIndex; --j)
                    spiralIndex.Add(matrix[downIndex, j]);

                downIndex--;

                if(topIndex > downIndex || leftIndex > rightIndex)
                    break;

                // leftmost column
                for(int i = downIndex; i >= topIndex; --i)
                    spiralIndex.Add(matrix[i, leftIndex]);

                leftIndex++;

                if(topIndex > downIndex || leftIndex > rightIndex)
                    break;
            }
        }

        private static void CheckTerrainSizeUnits ()
        {
            terrainSizeFactor = areaSizeLat / areaSizeLon;

            if (gridSizeTerrain > 1)
            {
//                float tsX = 0;
//                float tsY = 0;
//
//                foreach (Terrain tr in croppedTerrains)
//                {
//                    tsX += tr.terrainData.size.x;
//                    tsY += tr.terrainData.size.z;
//                }
//
//                terrainSizeX = tsX;
//                terrainSizeY = tsY;

                terrainSizeX = areaSizeLon / gridSizeTerrain;
                terrainSizeY = areaSizeLat / gridSizeTerrain;
            }
            else if(terrain)
            {
                terrainSizeX = terrain.terrainData.size.x;
                terrainSizeY = terrain.terrainData.size.z;
            }
        }

        public static void ServerInfoElevation ()
        {
            mapserviceElevation = new TerraLandWorldElevation.TopoBathy_ImageServer();
            mapserviceElevation.Timeout = 5000000;

            for (int i = 0; i < terrainChunks; i++)
            {
                Thread.Sleep(500);

                xMin[i] = lonCellLeft[i];
                yMin[i] = latCellBottom[i];
                xMax[i] = lonCellRight[i];
                yMax[i] = latCellTop[i];

                runTime.ServerConnectHeightmap(i);
            }
        }

        public static void ServerInfoElevationFAR ()
        {
            mapserviceElevation = new TerraLandWorldElevation.TopoBathy_ImageServer();
            mapserviceElevation.Timeout = 5000000;

            Thread.Sleep(500);

            runTime.ServerConnectHeightmapFAR();
        }

        public static void ServerInfoElevationNORTH (int index)
        {
//            runTime.ServerConnectHeightmapNORTH(index);
//            //Thread.Sleep(500);

            if(InfiniteTerrain.northTerrains.Count > 0)
            {
                northCounter = 0;

                for(int x = 0; x < gridSizeTerrain; x++)
                {
                    runTime.ServerConnectHeightmapNORTH(InfiniteTerrain.northIndex + x);
                    Thread.Sleep(500);
                }
            }
        }

        public static void ServerInfoElevationSOUTH ()
        {
            if(InfiniteTerrain.southTerrains.Count > 0)
            {
                southCounter = 0;

                for(int x = 0; x < gridSizeTerrain; x++)
                {
                    runTime.ServerConnectHeightmapSOUTH(InfiniteTerrain.southIndex + x);
                    Thread.Sleep(500);
                }
            }
        }

        public static void ServerInfoElevationEAST ()
        {
            if(InfiniteTerrain.eastTerrains.Count > 0)
            {
                eastCounter = 0;

                for(int x = 0; x < gridSizeTerrain; x++)
                {
                    runTime.ServerConnectHeightmapEAST(InfiniteTerrain.eastIndex + (x * gridSizeTerrain));
                    Thread.Sleep(500);
                }
            }
        }

        public static void ServerInfoElevationWEST ()
        {
            if(InfiniteTerrain.westTerrains.Count > 0)
            {
                westCounter = 0;

                for(int x = 0; x < gridSizeTerrain; x++)
                {
                    runTime.ServerConnectHeightmapWEST(InfiniteTerrain.westIndex + (x * gridSizeTerrain));
                    Thread.Sleep(500);
                }
            }
        }

        private static void SetupDownloaderElevation ()
        {  
            tempFolder = Path.GetTempPath() + "TerraLand/";
            //tempFolder = Path.GetTempPath() + "TerraLand/" + System.DateTime.Now.ToLongDateString() + "/" + System.DateTime.Now.ToLongTimeString().Replace(":", ".") + "/";

            if(!Directory.Exists(tempFolder))
                Directory.CreateDirectory(tempFolder);

            GenerateNewTerrainObject();

            if(runTime.farTerrain)
            {
                farTerrainTiffName = tempFolder + "FarTerrain.tif";
                CreateFarTerrainObject();
            }

            #if !UNITY_2_6 && !UNITY_2_6_1 && !UNITY_3_0 && !UNITY_3_0_0 && !UNITY_3_1 && !UNITY_3_2 && !UNITY_3_3 && !UNITY_3_4 && !UNITY_3_5 && !UNITY_4_0 && !UNITY_4_0_1 && !UNITY_4_1 && !UNITY_4_2 && !UNITY_4_3 && !UNITY_4_4 && !UNITY_4_5 && !UNITY_4_6 && !UNITY_4_7
            RemoveLightmapStatic();
            #endif

            terrainResolutionDownloading = runTime.heightmapResolution + splitSizeFinal;

            topCorner = new List<float>();
            bottomCorner = new List<float>();
            leftCorner = new List<float>();
            rightCorner = new List<float>();

            fileNameTerrainDataDynamic = new List<string>();
            tiffDataDynamic = new List<float[,]>();

            DirectoryInfo di = new DirectoryInfo(tempFolder);
                
            foreach (FileInfo file in di.GetFiles())
                file.Delete();
            
            foreach (DirectoryInfo dir in di.GetDirectories())
                dir.Delete(true);

            for(int i = 0; i < terrainChunks; i++)
            {
                fileNameTerrainDataDynamic.Add(tempFolder + (i + 1).ToString() + ".tif");
                tiffDataDynamic.Add(new float[tileResolution, tileResolution]);
            }

            highestPoints = new List<float>();
            downloadedHeightmapIndex = 0;
            downloadedFarTerrains = 0;
            downloadedFarTerrainImages = 0;

            realTerrainHeight = everestPeak * runTime.elevationExaggeration;
        }

        public static void ElevationDownload (int i)
        {
            GenerateToken(false, false, false, false, i);

            mapserviceElevation.Url = elevationURL + token;

            TerraLandWorldElevation.GeoImageDescription geoImgDesc = new TerraLandWorldElevation.GeoImageDescription();

            geoImgDesc.Height = tileResolution;
            geoImgDesc.Width = tileResolution;

            geoImgDesc.Compression = "LZW";
            geoImgDesc.CompressionQuality = 100;
            geoImgDesc.Interpolation = TerraLandWorldElevation.rstResamplingTypes.RSP_CubicConvolution;
            geoImgDesc.NoDataInterpretationSpecified = true;
            geoImgDesc.NoDataInterpretation = TerraLandWorldElevation.esriNoDataInterpretation.esriNoDataMatchAny;

            TerraLandWorldElevation.EnvelopeN extentElevation = new TerraLandWorldElevation.EnvelopeN();
            extentElevation.XMin = xMin[i];
            extentElevation.YMin = yMin[i];
            extentElevation.XMax = xMax[i];
            extentElevation.YMax = yMax[i];
            geoImgDesc.Extent = extentElevation;

            TerraLandWorldElevation.ImageType imageType = new TerraLandWorldElevation.ImageType();
            imageType.ImageFormat = TerraLandWorldElevation.esriImageFormat.esriImageTIFF;
            imageType.ImageReturnType = TerraLandWorldElevation.esriImageReturnType.esriImageReturnMimeData;

            try
            {
                TerraLandWorldElevation.ImageResult result = mapserviceElevation.ExportImage(geoImgDesc, imageType);
                File.WriteAllBytes(fileNameTerrainDataDynamic[i], result.ImageData);

                if(downloadedHeightmapIndex == 0)
                {
                    if (!runTime.IsCustomGeoServer)
                        CalculateResampleHeightmaps();
                    else
                        CalculateResampleHeightmapsGeoServer();
                }
            }
//            catch (Exception e)
//            {
//                UnityEngine.Debug.Log(e);
//                return;
//            }
            catch
            {}
            finally
            {
                downloadedHeightmapIndex++;
            }
        }

        public static void LoadHeights (int i)
        {
            if(runTime.progressiveGeneration)
            {
                if (runTime.IsCustomGeoServer)
                {
                    if(geoDataExtensionElevation.Equals("raw"))
                        Timing.RunCoroutine(LoadTerrainHeightsFromRAW());
                    else if(geoDataExtensionElevation.Equals("tif"))
                        Timing.RunCoroutine(LoadTerrainHeightsFromTIFFDynamic(i));
                    else if(geoDataExtensionElevation.Equals("asc"))
                        Timing.RunCoroutine(LoadTerrainHeightsFromASCII());
                }
                else
                    Timing.RunCoroutine(LoadTerrainHeightsFromTIFFDynamic(i));
            }
            else
            {
                if(downloadedHeightmapIndex == terrainChunks)
                {
                    if (runTime.IsCustomGeoServer)
                    {
                        if(geoDataExtensionElevation.Equals("raw"))
                            Timing.RunCoroutine(LoadTerrainHeightsFromRAW());
                        else if(geoDataExtensionElevation.Equals("tif"))
                            Timing.RunCoroutine(LoadTerrainHeightsFromTIFFDynamic(i));
                        else if(geoDataExtensionElevation.Equals("asc"))
                            Timing.RunCoroutine(LoadTerrainHeightsFromASCII());
                    }
                    else
                    {
                        //Timing.RunCoroutine(LoadTerrainHeightsFromTIFFDynamic(i));
                        runTime.LoadTerrainHeights();
                    }
                }
            }   
        }

        public static void ElevationDownloadFAR ()
        {
            GenerateToken(false, false, false, false, 0);

            mapserviceElevation.Url = elevationURL + token;

            TerraLandWorldElevation.GeoImageDescription geoImgDesc = new TerraLandWorldElevation.GeoImageDescription();

            geoImgDesc.Height = runTime.farTerrainHeightmapResolution + 1;
            geoImgDesc.Width = runTime.farTerrainHeightmapResolution + 1;

            geoImgDesc.Compression = "LZW";
            geoImgDesc.CompressionQuality = 100;
            geoImgDesc.Interpolation = TerraLandWorldElevation.rstResamplingTypes.RSP_CubicConvolution;
            geoImgDesc.NoDataInterpretationSpecified = true;
            geoImgDesc.NoDataInterpretation = TerraLandWorldElevation.esriNoDataInterpretation.esriNoDataMatchAny;

            TerraLandWorldElevation.EnvelopeN extentElevation = new TerraLandWorldElevation.EnvelopeN();
            extentElevation.XMin = double.Parse(leftFar) * 20037508.34 / 180.0;
            double yMaxTopFar = Math.Log(Math.Tan((90.0 + double.Parse(topFar)) * Math.PI / 360.0)) / (Math.PI / 180.0);
            extentElevation.YMax = yMaxTopFar * 20037508.34 / 180.0;

            extentElevation.XMax = double.Parse(rightFar) * 20037508.34 / 180.0;
            double yMinBottomFar = Math.Log(Math.Tan((90.0 + double.Parse(bottomFar)) * Math.PI / 360.0)) / (Math.PI / 180.0);
            extentElevation.YMin = yMinBottomFar * 20037508.34 / 180.0;
            geoImgDesc.Extent = extentElevation;

            TerraLandWorldElevation.ImageType imageType = new TerraLandWorldElevation.ImageType();
            imageType.ImageFormat = TerraLandWorldElevation.esriImageFormat.esriImageTIFF;
            imageType.ImageReturnType = TerraLandWorldElevation.esriImageReturnType.esriImageReturnMimeData;

            try
            {
                TerraLandWorldElevation.ImageResult result = mapserviceElevation.ExportImage(geoImgDesc, imageType);
                File.WriteAllBytes(farTerrainTiffName, result.ImageData);
            }
//            catch (Exception e)
//            {
//                UnityEngine.Debug.Log(e);
//                return;
//            }
            catch
            {}
            finally
            {
                downloadedFarTerrains++;
            }
        }

        public static void LoadHeightsFAR ()
        {
            runTime.LoadTerrainHeightsFAR();
        }

        public static void ElevationDownloadNORTH (int i)
        {
            if(i == InfiniteTerrain.northIndex)
                GenerateToken(true, false, false, false, i);

            mapserviceElevation.Url = elevationURL + token;

            TerraLandWorldElevation.GeoImageDescription geoImgDesc = new TerraLandWorldElevation.GeoImageDescription();

            geoImgDesc.Height = tileResolution;
            geoImgDesc.Width = tileResolution;

            geoImgDesc.Compression = "LZW";
            geoImgDesc.CompressionQuality = 100;
            geoImgDesc.Interpolation = TerraLandWorldElevation.rstResamplingTypes.RSP_CubicConvolution;
            geoImgDesc.NoDataInterpretationSpecified = true;
            geoImgDesc.NoDataInterpretation = TerraLandWorldElevation.esriNoDataInterpretation.esriNoDataMatchAny;

            TerraLandWorldElevation.EnvelopeN extentElevation = new TerraLandWorldElevation.EnvelopeN();
            extentElevation.XMin = xMin[i];
            extentElevation.YMin = yMin[i];
            extentElevation.XMax = xMax[i];
            extentElevation.YMax = yMax[i];
            geoImgDesc.Extent = extentElevation;

            TerraLandWorldElevation.ImageType imageType = new TerraLandWorldElevation.ImageType();
            imageType.ImageFormat = TerraLandWorldElevation.esriImageFormat.esriImageTIFF;
            imageType.ImageReturnType = TerraLandWorldElevation.esriImageReturnType.esriImageReturnMimeData;

            try
            {
                TerraLandWorldElevation.ImageResult result = mapserviceElevation.ExportImage(geoImgDesc, imageType);
                File.WriteAllBytes(fileNameTerrainDataDynamic[i], result.ImageData);
            }
//            catch (Exception e)
//            {
//                UnityEngine.Debug.Log(e);
//                return;
//            }
            catch
            {}
            finally
            {
                northCounter++;
            }
        }

        public static void ElevationDownloadSOUTH (int i)
        {
            if(i == InfiniteTerrain.southIndex)
                GenerateToken(false, true, false, false, i);

            mapserviceElevation.Url = elevationURL + token;

            TerraLandWorldElevation.GeoImageDescription geoImgDesc = new TerraLandWorldElevation.GeoImageDescription();

            geoImgDesc.Height = tileResolution;
            geoImgDesc.Width = tileResolution;

            geoImgDesc.Compression = "LZW";
            geoImgDesc.CompressionQuality = 100;
            geoImgDesc.Interpolation = TerraLandWorldElevation.rstResamplingTypes.RSP_CubicConvolution;
            geoImgDesc.NoDataInterpretationSpecified = true;
            geoImgDesc.NoDataInterpretation = TerraLandWorldElevation.esriNoDataInterpretation.esriNoDataMatchAny;

            TerraLandWorldElevation.EnvelopeN extentElevation = new TerraLandWorldElevation.EnvelopeN();
            extentElevation.XMin = xMin[i];
            extentElevation.YMin = yMin[i];
            extentElevation.XMax = xMax[i];
            extentElevation.YMax = yMax[i];
            geoImgDesc.Extent = extentElevation;

            TerraLandWorldElevation.ImageType imageType = new TerraLandWorldElevation.ImageType();
            imageType.ImageFormat = TerraLandWorldElevation.esriImageFormat.esriImageTIFF;
            imageType.ImageReturnType = TerraLandWorldElevation.esriImageReturnType.esriImageReturnMimeData;

            try
            {
                TerraLandWorldElevation.ImageResult result = mapserviceElevation.ExportImage(geoImgDesc, imageType);
                File.WriteAllBytes(fileNameTerrainDataDynamic[i], result.ImageData);
            }
//            catch (Exception e)
//            {
//                UnityEngine.Debug.Log(e);
//                return;
//            }
            catch
            {}
            finally
            {
                southCounter++;
            }
        }

        public static void ElevationDownloadEAST (int i)
        {
            if(i == InfiniteTerrain.eastIndex)
                GenerateToken(false, false, true, false, i);

            mapserviceElevation.Url = elevationURL + token;

            TerraLandWorldElevation.GeoImageDescription geoImgDesc = new TerraLandWorldElevation.GeoImageDescription();

            geoImgDesc.Height = tileResolution;
            geoImgDesc.Width = tileResolution;

            geoImgDesc.Compression = "LZW";
            geoImgDesc.CompressionQuality = 100;
            geoImgDesc.Interpolation = TerraLandWorldElevation.rstResamplingTypes.RSP_CubicConvolution;
            geoImgDesc.NoDataInterpretationSpecified = true;
            geoImgDesc.NoDataInterpretation = TerraLandWorldElevation.esriNoDataInterpretation.esriNoDataMatchAny;

            TerraLandWorldElevation.EnvelopeN extentElevation = new TerraLandWorldElevation.EnvelopeN();
            extentElevation.XMin = xMin[i];
            extentElevation.YMin = yMin[i];
            extentElevation.XMax = xMax[i];
            extentElevation.YMax = yMax[i];
            geoImgDesc.Extent = extentElevation;

            TerraLandWorldElevation.ImageType imageType = new TerraLandWorldElevation.ImageType();
            imageType.ImageFormat = TerraLandWorldElevation.esriImageFormat.esriImageTIFF;
            imageType.ImageReturnType = TerraLandWorldElevation.esriImageReturnType.esriImageReturnMimeData;

            try
            {
                TerraLandWorldElevation.ImageResult result = mapserviceElevation.ExportImage(geoImgDesc, imageType);
                File.WriteAllBytes(fileNameTerrainDataDynamic[i], result.ImageData);
            }
//            catch (Exception e)
//            {
//                UnityEngine.Debug.Log(e);
//                return;
//            }
            catch
            {}
            finally
            {
                eastCounter++;
            }
        }

        public static void ElevationDownloadWEST (int i)
        {
            if(i == InfiniteTerrain.westIndex)
                GenerateToken(false, false, false, true, i);

            mapserviceElevation.Url = elevationURL + token;

            TerraLandWorldElevation.GeoImageDescription geoImgDesc = new TerraLandWorldElevation.GeoImageDescription();

            geoImgDesc.Height = tileResolution;
            geoImgDesc.Width = tileResolution;

            geoImgDesc.Compression = "LZW";
            geoImgDesc.CompressionQuality = 100;
            geoImgDesc.Interpolation = TerraLandWorldElevation.rstResamplingTypes.RSP_CubicConvolution;
            geoImgDesc.NoDataInterpretationSpecified = true;
            geoImgDesc.NoDataInterpretation = TerraLandWorldElevation.esriNoDataInterpretation.esriNoDataMatchAny;

            TerraLandWorldElevation.EnvelopeN extentElevation = new TerraLandWorldElevation.EnvelopeN();
            extentElevation.XMin = xMin[i];
            extentElevation.YMin = yMin[i];
            extentElevation.XMax = xMax[i];
            extentElevation.YMax = yMax[i];
            geoImgDesc.Extent = extentElevation;

            TerraLandWorldElevation.ImageType imageType = new TerraLandWorldElevation.ImageType();
            imageType.ImageFormat = TerraLandWorldElevation.esriImageFormat.esriImageTIFF;
            imageType.ImageReturnType = TerraLandWorldElevation.esriImageReturnType.esriImageReturnMimeData;

            try
            {
                TerraLandWorldElevation.ImageResult result = mapserviceElevation.ExportImage(geoImgDesc, imageType);
                File.WriteAllBytes(fileNameTerrainDataDynamic[i], result.ImageData);
            }
//            catch (Exception e)
//            {
//                UnityEngine.Debug.Log(e);
//                return;
//            }
            catch
            {}
            finally
            {
                westCounter++;
            }
        }

        public static void LoadHeightsNORTH (int index)
        {
            runTime.LoadTerrainHeightsNORTH(index);
        }

        public static void LoadHeightsSOUTH (int i)
        {
            runTime.LoadTerrainHeightsSOUTH(i);
        }

        public static void LoadHeightsEAST (int i)
        {
            runTime.LoadTerrainHeightsEAST(i);
        }

        public static void LoadHeightsWEST (int i)
        {
            runTime.LoadTerrainHeightsWEST(i);
        }

        public static void SmoothNORTH (int index)
        {
            try
            {
                for(int x = 0; x < (int)gridSizeTerrain; x++)
                {
                    int indx = InfiniteTerrain.northIndex + x;
                    tiffDataDynamic[indx] = TiffDataDynamic(fileNameTerrainDataDynamic[indx], indx);
                }
            }
            catch(Exception e)
            {
                UnityEngine.Debug.Log(e);
            }




//            try
//            {
//                //tiffDataDynamic[index] = TiffDataDynamic(fileNameTerrainDataDynamic[index], index);
//
//
//                //if(index.Equals(InfiniteTerrain.northIndexes[gridSizeTerrain - 1]))
//                if(!xxx)
//                {
//                    //for(int x = 0; x < InfiniteTerrain.northIndexes.Count; x++)
//                    for(int x = 0; x < gridSizeTerrain; x++)
//                    {
//                        int indx = InfiniteTerrain.northIndexes[x];
//                        tiffDataDynamic[indx] = TiffDataDynamic(fileNameTerrainDataDynamic[indx], indx);
//                    }
//                }
//
//                xxx = true;
//
//            }
//            catch(Exception e)
//            {
//                UnityEngine.Debug.Log(e);
//            }
        }
            
        public static void SmoothSOUTH ()
        {
            try
            {
                for(int x = 0; x < (int)gridSizeTerrain; x++)
                {
                    int indx = InfiniteTerrain.southIndex + x;
                    tiffDataDynamic[indx] = TiffDataDynamic(fileNameTerrainDataDynamic[indx], indx);
                }
            }
            catch(Exception e)
            {
                UnityEngine.Debug.Log(e);
            }
        }
            
        public static void SmoothEAST ()
        {
            try
            {
                for(int x = 0; x < (int)gridSizeTerrain; x++)
                {
                    int indx = InfiniteTerrain.eastIndex + (x * gridSizeTerrain);
                    tiffDataDynamic[indx] = TiffDataDynamic(fileNameTerrainDataDynamic[indx], indx);
                }
            }
            catch(Exception e)
            {
                UnityEngine.Debug.Log(e);
            }
        }

        public static void SmoothWEST ()
        {
            try
            {
                for(int x = 0; x < (int)gridSizeTerrain; x++)
                {
                    int indx = InfiniteTerrain.westIndex + (x * gridSizeTerrain);
                    tiffDataDynamic[indx] = TiffDataDynamic(fileNameTerrainDataDynamic[indx], indx);
                }
            }
            catch(Exception e)
            {
                UnityEngine.Debug.Log(e);
            }
        }

        public static void SmoothAllHeights ()
        {
            try
            {
                string[] tiffnames = Directory.GetFiles(tempFolder);

                foreach(string s in tiffnames)
                {
                    if(!s.Equals(farTerrainTiffName))
                    {
                        int i = int.Parse(Path.GetFileNameWithoutExtension(s)) - 1;

                        if(File.Exists(fileNameTerrainDataDynamic[i]))
                            tiffDataDynamic[i] = TiffDataDynamic(fileNameTerrainDataDynamic[i], i);
                    }
                }

                highestPoint = highestPoints.Max();
            }
            catch(Exception e)
            {
                UnityEngine.Debug.Log(e);
            }
        }

        public static void SmoothFarTerrain ()
        {
            try
            {
                if(File.Exists(farTerrainTiffName))
                    tiffDataFar = TiffDataDynamicFAR(farTerrainTiffName);
            }
            catch(Exception e)
            {
                UnityEngine.Debug.Log(e);
            }
        }

        private static void DownloadTerrainData (string urlAddress, string location)
        {
            using (webClientTerrain = new WebClient())
            {
                Uri URL = urlAddress.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ? new Uri(urlAddress) : new Uri("http://" + urlAddress);

                try
                {
                    webClientTerrain.DownloadFile(URL, location);
                    //webClientTerrain.DownloadFileAsync(URL, location);
                }
                catch (Exception e)
                {
                    UnityEngine.Debug.Log(e);
                    return;
                }
            }
        }

        public static void SetupDownloaderElevationGeoServer ()
        {
            GenerateNewTerrainObject();

            #if !UNITY_2_6 && !UNITY_2_6_1 && !UNITY_3_0 && !UNITY_3_0_0 && !UNITY_3_1 && !UNITY_3_2 && !UNITY_3_3 && !UNITY_3_4 && !UNITY_3_5 && !UNITY_4_0 && !UNITY_4_0_1 && !UNITY_4_1 && !UNITY_4_2 && !UNITY_4_3 && !UNITY_4_4 && !UNITY_4_5 && !UNITY_4_6 && !UNITY_4_7
            RemoveLightmapStatic();
            #endif

            terrainResolutionDownloading = runTime.heightmapResolution + splitSizeFinal;

            topCorner = new List<float>();
            bottomCorner = new List<float>();
            leftCorner = new List<float>();
            rightCorner = new List<float>();

            if(geoImagesOK)
                ImageTiler();
        }

        public static void DownloadImageData (string urlAddress)
        {
            using (webClientImage = new WebClient())
            {
                //Uri URL = urlAddress.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ? new Uri(urlAddress) : new Uri("http://" + urlAddress);
                Uri URL = new Uri(urlAddress);

                try
                {
                    imageBytes.Add(webClientImage.DownloadData(URL));
                }
                catch (Exception e)
                {
                    UnityEngine.Debug.Log(e);
                    return;
                }
            }
        }

        public static IEnumerator<float> FillImage (int index)
        {
            images[index].LoadImage(imageBytes[index]);

            if(downloadedImageIndex == terrainChunks)
            {
                imagesAreGenerated = true;
                UnityEngine.Debug.Log("Satellite Images Are Generated");

                if(terrainsAreGenerated)
                {
                    if(runTime.farTerrain)
                    {
                        if(farTerrainIsGenerated)
                            Timing.RunCoroutine(WorldIsGenerated());
                    }
                    else
                        Timing.RunCoroutine(WorldIsGenerated());
                }
            }

            yield return 0;
        }

        public static IEnumerator<float> FillImageFAR ()
        {
            farImage.LoadImage(farImageBytes);

            downloadedFarTerrainImages++;

            yield return 0;
        }

        public static IEnumerator<float> FillImageNORTH (int index)
        {
            images[index].LoadImage(imageBytes[index]);

            yield return 0;
        }

        public static IEnumerator<float> FillImageSOUTH (int index)
        {
            images[index].LoadImage(imageBytes[index]);

            yield return 0;
        }

        public static IEnumerator<float> FillImageEAST (int index)
        {
            images[index].LoadImage(imageBytes[index]);

            yield return 0;
        }

        public static IEnumerator<float> FillImageWEST (int index)
        {
            images[index].LoadImage(imageBytes[index]);

            yield return 0;
        }

        public static IEnumerator<float> FillImages (int length)
        {
            for(int z = 0; z < length; z++)
            {
                if(runTime.spiralGeneration)
                {
                    images[spiralIndex[z]].LoadImage(imageBytes[spiralIndex[z]]);
                        

                    //satImage = Image.FromFile(tempFolder + "SatelliteImage" + (z + 1).ToString() + ".jpg");
                    //SaveSatelliteImage(imageBytes[spiralIndex[z]], z);
                }
                else
                {
                    images[z].LoadImage(imageBytes[z]);
                        

                    //satImage = Image.FromFile(tempFolder + "SatelliteImage" + (z + 1).ToString() + ".jpg");
                    //SaveSatelliteImage(imageBytes[z], z);
                }

                yield return Timing.WaitForSeconds(runTime.imageryDelay);
            }
        }

        private static void SaveSatelliteImage (byte[] bytes, int index)
        {
            using (MemoryStream ms = new MemoryStream(bytes))
            {
                Image.FromStream(ms);
                Image.FromFile(tempFolder + "SatelliteImage" + (index + 1).ToString() + ".jpg");
            }
        }

        private static void InitializeDownloader ()
        {
            xMinLeft = double.Parse(left) * 20037508.34 / 180.0;
            yMaxTop = Math.Log(Math.Tan((90.0 + double.Parse(top)) * Math.PI / 360.0)) / (Math.PI / 180.0);
            yMaxTop = yMaxTop * 20037508.34 / 180.0;

            xMaxRight = double.Parse(right) * 20037508.34 / 180.0;
            yMinBottom = Math.Log(Math.Tan((90.0 + double.Parse(bottom)) * Math.PI / 360.0)) / (Math.PI / 180.0);
            yMinBottom = yMinBottom * 20037508.34 / 180.0;

            latCellSize = Math.Abs(yMaxTop - yMinBottom) / (double)gridNumber;
            lonCellSize = Math.Abs(xMinLeft - xMaxRight) / (double)gridNumber;

            xMin = new double[terrainChunks];
            yMin = new double[terrainChunks];
            xMax = new double[terrainChunks];
            yMax = new double[terrainChunks];

            xMinImage = new double[terrainChunks];
            yMinImage = new double[terrainChunks];
            xMaxImage = new double[terrainChunks];
            yMaxImage = new double[terrainChunks];

            downloadedImageIndex = 0;

            TerrainGridManager(gridNumber, terrainChunks);
        }

        private static void TerrainGridManager (int grid, int terrainChunks)
        {
            int index = 0;
            cellSizeX = terrainSizeX / (float)grid;
            cellSizeY = terrainSizeY / (float)grid;

            imageXOffset = new float[terrainChunks];
            imageYOffset = new float[terrainChunks];

            latCellTop = new double[terrainChunks];
            latCellBottom = new double[terrainChunks];
            lonCellLeft = new double[terrainChunks];
            lonCellRight = new double[terrainChunks];

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

                    xMin[index] = lonCellLeft[index];
                    yMin[index] = latCellBottom[index];
                    xMax[index] = lonCellRight[index];
                    yMax[index] = latCellTop[index];

                    index++;
                }
            }
        }

        private static void GetImagesInfo ()
        {
            images = new List<Texture2D>();
            imageBytes = new List<byte[]>();
            geoImagesOK = true;
            imageWidth = runTime.imageResolution;
            imageHeight = runTime.imageResolution;

            for(int i = 0; i < totalImages; i++)
            {
                imageBytes.Add(new byte[(int)Mathf.Pow(runTime.imageResolution, 2)]);

                // Don't use decompressed formats as decompression at runtime causes lags and spikes and lowers the FPS
                images.Add(new Texture2D(imageWidth, imageHeight, TextureFormat.RGB24, true, true));

                images[i].wrapMode = TextureWrapMode.Clamp;
                images[i].name = (i + 1).ToString();

//                byte[] bytes = new byte[imageWidth * imageHeight];
//                bytes = images[i].EncodeToJPG(100);
//                File.WriteAllBytes(tempFolder + "SatelliteImage" + (i + 1).ToString() + ".jpg", bytes);
            }

            if(terrainChunks > 1)
            {
                multipleTerrainsTiling = true;
                imagesPerTerrain = (int)((float)totalImages / (float)terrainChunks);

                //TODO: Check if tileGrid = 1 always ==> tileGrid = gridPerTerrain;
                tileGrid = (int)(Mathf.Sqrt((float)imagesPerTerrain));
            }
            else
            {
                multipleTerrainsTiling = false;
                tileGrid = (int)(Mathf.Sqrt((float)totalImages));
                //terrainSizeX = terrainSizeNewX;
                //terrainSizeY = terrainSizeNewZ;
            }

            // Prepare Image for Far Terrain
            if(runTime.farTerrain)
            {
                farImageBytes = new byte[(int)Mathf.Pow(runTime.farTerrainImageResolution, 2)];

                // Don't use decompressed formats as decompression at runtime causes lags and spikes and lowers the FPS
                // Optionally we can use Gamma(Non-Linear) color space for more clear colors
                farImage = new Texture2D(runTime.farTerrainImageResolution, runTime.farTerrainImageResolution, TextureFormat.RGB24, true, true);

                farImage.wrapMode = TextureWrapMode.Clamp;
                farImage.mipMapBias = -0.5f;
                farImage.name = "FarTerrainImage";

//                byte[] bytes = new byte[runTime.farTerrainImageResolution * runTime.farTerrainImageResolution];
//                bytes = farImage.EncodeToJPG(100);
//                File.WriteAllBytes(tempFolder + "FarSatelliteImage.jpg", bytes);
            }
        }

        public static void ServerInfoImagery ()
        {
            mapservice = new TerraLandWorldImagery.World_Imagery_MapServer();
            mapservice.Timeout = 5000000;
            TerraLandWorldImagery.TileImageInfo tileImageInfo = mapservice.GetTileImageInfo(mapservice.GetDefaultMapName());
            tileImageInfo.CompressionQuality = compressionQuality;

            for (int i = 0; i < totalImages; i++)
                runTime.ServerConnectImagery(i);
        }

        public static void ServerInfoImageryFAR ()
        {
            mapservice = new TerraLandWorldImagery.World_Imagery_MapServer();
            mapservice.Timeout = 5000000;
            TerraLandWorldImagery.TileImageInfo tileImageInfo = mapservice.GetTileImageInfo(mapservice.GetDefaultMapName());
            tileImageInfo.CompressionQuality = compressionQuality;

            runTime.ServerConnectImageryFAR();
        }

        public static void ServerInfoImageryNORTH ()
        {
            for(int x = 0; x < gridSizeTerrain; x++)
            {
                int i = InfiniteTerrain.northIndexImagery[x];
                int j = InfiniteTerrain.northIndex + x;

                xMinImage[i] = lonCellLeft[j];
                yMinImage[i] = latCellBottom[j];
                xMaxImage[i] = lonCellRight[j];
                yMaxImage[i] = latCellTop[j];

                runTime.ServerConnectImageryNORTH(i);
                Thread.Sleep(500);
            }
        }

        public static void ServerInfoImagerySOUTH ()
        {
            for(int x = 0; x < gridSizeTerrain; x++)
            {
                int i = InfiniteTerrain.southIndexImagery[x];
                int j = InfiniteTerrain.southIndex + x;

                xMinImage[i] = lonCellLeft[j];
                yMinImage[i] = latCellBottom[j];
                xMaxImage[i] = lonCellRight[j];
                yMaxImage[i] = latCellTop[j];

                runTime.ServerConnectImagerySOUTH(i);
                Thread.Sleep(500);
            }
        }

        public static void ServerInfoImageryEAST ()
        {
            for(int x = 0; x < gridSizeTerrain; x++)
            {
                int i = InfiniteTerrain.eastIndexImagery[x];
                int j = InfiniteTerrain.eastIndex + (x * gridSizeTerrain);

                xMinImage[i] = lonCellLeft[j];
                yMinImage[i] = latCellBottom[j];
                xMaxImage[i] = lonCellRight[j];
                yMaxImage[i] = latCellTop[j];

                runTime.ServerConnectImageryEAST(i);
                Thread.Sleep(500);
            }
        }

        public static void ServerInfoImageryWEST ()
        {
            for(int x = 0; x < gridSizeTerrain; x++)
            {
                int i = InfiniteTerrain.westIndexImagery[x];
                int j = InfiniteTerrain.westIndex + (x * gridSizeTerrain);

                xMinImage[i] = lonCellLeft[j];
                yMinImage[i] = latCellBottom[j];
                xMaxImage[i] = lonCellRight[j];
                yMaxImage[i] = latCellTop[j];

                runTime.ServerConnectImageryWEST(i);
                Thread.Sleep(500);
            }
        }

        public static void ImageDownloader (int i)
        {
            if(!allBlack)
            {
                mapinfo = mapservice.GetServerInfo(mapservice.GetDefaultMapName());
                mapdesc = mapinfo.DefaultMapDescription;

                TerraLandWorldImagery.EnvelopeN extent = new TerraLandWorldImagery.EnvelopeN();
                extent.XMin = xMin[i];
                extent.YMin = yMin[i];
                extent.XMax = xMax[i];
                extent.YMax = yMax[i];
                mapdesc.MapArea.Extent = extent;

                TerraLandWorldImagery.ImageType imgtype = new TerraLandWorldImagery.ImageType();
                imgtype.ImageFormat = TerraLandWorldImagery.esriImageFormat.esriImageJPG;
                imgtype.ImageReturnType = TerraLandWorldImagery.esriImageReturnType.esriImageReturnMimeData;

                TerraLandWorldImagery.ImageDisplay imgdisp = new TerraLandWorldImagery.ImageDisplay();
                imgdisp.ImageHeight = runTime.imageResolution;
                imgdisp.ImageWidth = runTime.imageResolution;
                imgdisp.ImageDPI = 96;

                TerraLandWorldImagery.ImageDescription imgdesc = new TerraLandWorldImagery.ImageDescription();
                imgdesc.ImageDisplay = imgdisp;
                imgdesc.ImageType = imgtype;

                try
                {
                    TerraLandWorldImagery.MapImage mapimg = mapservice.ExportMapImage(mapdesc, imgdesc);

                    imageBytes[i] = mapimg.ImageData;

                    if(!availableImageryCheked)
                        CheckImageColors(i);
                }
//                catch (Exception e)
//                {
//                    UnityEngine.Debug.Log(e);
//                    return;
//                }
                catch
                {}
                finally
                {
                    downloadedImageIndex++;
                }
            }
        }

        public static void ImageDownloaderFAR ()
        {
            mapinfo = mapservice.GetServerInfo(mapservice.GetDefaultMapName());
            mapdesc = mapinfo.DefaultMapDescription;

            TerraLandWorldImagery.EnvelopeN extent = new TerraLandWorldImagery.EnvelopeN();
            extent.XMin = double.Parse(leftFar) * 20037508.34 / 180.0;
            double yMaxTopFar = Math.Log(Math.Tan((90.0 + double.Parse(topFar)) * Math.PI / 360.0)) / (Math.PI / 180.0);
            extent.YMax = yMaxTopFar * 20037508.34 / 180.0;

            extent.XMax = double.Parse(rightFar) * 20037508.34 / 180.0;
            double yMinBottomFar = Math.Log(Math.Tan((90.0 + double.Parse(bottomFar)) * Math.PI / 360.0)) / (Math.PI / 180.0);
            extent.YMin = yMinBottomFar * 20037508.34 / 180.0;
            mapdesc.MapArea.Extent = extent;

            TerraLandWorldImagery.ImageType imgtype = new TerraLandWorldImagery.ImageType();
            imgtype.ImageFormat = TerraLandWorldImagery.esriImageFormat.esriImageJPG;
            imgtype.ImageReturnType = TerraLandWorldImagery.esriImageReturnType.esriImageReturnMimeData;

            TerraLandWorldImagery.ImageDisplay imgdisp = new TerraLandWorldImagery.ImageDisplay();
            imgdisp.ImageHeight = runTime.farTerrainImageResolution;
            imgdisp.ImageWidth = runTime.farTerrainImageResolution;
            imgdisp.ImageDPI = 96;

            TerraLandWorldImagery.ImageDescription imgdesc = new TerraLandWorldImagery.ImageDescription();
            imgdesc.ImageDisplay = imgdisp;
            imgdesc.ImageType = imgtype;

            try
            {
                TerraLandWorldImagery.MapImage mapimg = mapservice.ExportMapImage(mapdesc, imgdesc);

                farImageBytes = mapimg.ImageData;
            }
            catch (Exception e)
            {
                UnityEngine.Debug.Log(e);
                return;
            }
        }

        public static void ImageDownloaderNORTH (int i)
        {
            if(!allBlack)
            {
                mapinfo = mapservice.GetServerInfo(mapservice.GetDefaultMapName());
                mapdesc = mapinfo.DefaultMapDescription;

                TerraLandWorldImagery.EnvelopeN extent = new TerraLandWorldImagery.EnvelopeN();
                extent.XMin = xMinImage[i];
                extent.YMin = yMinImage[i];
                extent.XMax = xMaxImage[i];
                extent.YMax = yMaxImage[i];
                mapdesc.MapArea.Extent = extent;

                TerraLandWorldImagery.ImageType imgtype = new TerraLandWorldImagery.ImageType();
                imgtype.ImageFormat = TerraLandWorldImagery.esriImageFormat.esriImageJPG;
                imgtype.ImageReturnType = TerraLandWorldImagery.esriImageReturnType.esriImageReturnMimeData;

                TerraLandWorldImagery.ImageDisplay imgdisp = new TerraLandWorldImagery.ImageDisplay();
                imgdisp.ImageHeight = runTime.imageResolution;
                imgdisp.ImageWidth = runTime.imageResolution;
                imgdisp.ImageDPI = 96;

                TerraLandWorldImagery.ImageDescription imgdesc = new TerraLandWorldImagery.ImageDescription();
                imgdesc.ImageDisplay = imgdisp;
                imgdesc.ImageType = imgtype;

                try
                {
                    TerraLandWorldImagery.MapImage mapimg = mapservice.ExportMapImage(mapdesc, imgdesc);

                    imageBytes[i] = mapimg.ImageData;

                    if(!availableImageryCheked)
                        CheckImageColors(i);
                }
//                catch (Exception e)
//                {
//                    UnityEngine.Debug.Log(e);
//                    return;
//                }
                catch
                {}
                finally
                {
                    downloadedImageIndex++;
                }
            }
        }

        public static void ImageDownloaderSOUTH (int i)
        {
            if(!allBlack)
            {
                mapinfo = mapservice.GetServerInfo(mapservice.GetDefaultMapName());
                mapdesc = mapinfo.DefaultMapDescription;

                TerraLandWorldImagery.EnvelopeN extent = new TerraLandWorldImagery.EnvelopeN();
                extent.XMin = xMinImage[i];
                extent.YMin = yMinImage[i];
                extent.XMax = xMaxImage[i];
                extent.YMax = yMaxImage[i];
                mapdesc.MapArea.Extent = extent;

                TerraLandWorldImagery.ImageType imgtype = new TerraLandWorldImagery.ImageType();
                imgtype.ImageFormat = TerraLandWorldImagery.esriImageFormat.esriImageJPG;
                imgtype.ImageReturnType = TerraLandWorldImagery.esriImageReturnType.esriImageReturnMimeData;

                TerraLandWorldImagery.ImageDisplay imgdisp = new TerraLandWorldImagery.ImageDisplay();
                imgdisp.ImageHeight = runTime.imageResolution;
                imgdisp.ImageWidth = runTime.imageResolution;
                imgdisp.ImageDPI = 96;

                TerraLandWorldImagery.ImageDescription imgdesc = new TerraLandWorldImagery.ImageDescription();
                imgdesc.ImageDisplay = imgdisp;
                imgdesc.ImageType = imgtype;

                try
                {
                    TerraLandWorldImagery.MapImage mapimg = mapservice.ExportMapImage(mapdesc, imgdesc);

                    imageBytes[i] = mapimg.ImageData;

                    if(!availableImageryCheked)
                        CheckImageColors(i);
                }
//                catch (Exception e)
//                {
//                    UnityEngine.Debug.Log(e);
//                    return;
//                }
                catch
                {}
                finally
                {
                    downloadedImageIndex++;
                }
            }
        }

        public static void ImageDownloaderEAST (int i)
        {
            if(!allBlack)
            {
                mapinfo = mapservice.GetServerInfo(mapservice.GetDefaultMapName());
                mapdesc = mapinfo.DefaultMapDescription;

                TerraLandWorldImagery.EnvelopeN extent = new TerraLandWorldImagery.EnvelopeN();
                extent.XMin = xMinImage[i];
                extent.YMin = yMinImage[i];
                extent.XMax = xMaxImage[i];
                extent.YMax = yMaxImage[i];
                mapdesc.MapArea.Extent = extent;

                TerraLandWorldImagery.ImageType imgtype = new TerraLandWorldImagery.ImageType();
                imgtype.ImageFormat = TerraLandWorldImagery.esriImageFormat.esriImageJPG;
                imgtype.ImageReturnType = TerraLandWorldImagery.esriImageReturnType.esriImageReturnMimeData;

                TerraLandWorldImagery.ImageDisplay imgdisp = new TerraLandWorldImagery.ImageDisplay();
                imgdisp.ImageHeight = runTime.imageResolution;
                imgdisp.ImageWidth = runTime.imageResolution;
                imgdisp.ImageDPI = 96;

                TerraLandWorldImagery.ImageDescription imgdesc = new TerraLandWorldImagery.ImageDescription();
                imgdesc.ImageDisplay = imgdisp;
                imgdesc.ImageType = imgtype;

                try
                {
                    TerraLandWorldImagery.MapImage mapimg = mapservice.ExportMapImage(mapdesc, imgdesc);

                    imageBytes[i] = mapimg.ImageData;

                    if(!availableImageryCheked)
                        CheckImageColors(i);
                }
//                catch (Exception e)
//                {
//                    UnityEngine.Debug.Log(e);
//                    return;
//                }
                catch
                {}
                finally
                {
                    downloadedImageIndex++;
                }
            }
        }

        public static void ImageDownloaderWEST (int i)
        {
            if(!allBlack)
            {
                mapinfo = mapservice.GetServerInfo(mapservice.GetDefaultMapName());
                mapdesc = mapinfo.DefaultMapDescription;

                TerraLandWorldImagery.EnvelopeN extent = new TerraLandWorldImagery.EnvelopeN();
                extent.XMin = xMinImage[i];
                extent.YMin = yMinImage[i];
                extent.XMax = xMaxImage[i];
                extent.YMax = yMaxImage[i];
                mapdesc.MapArea.Extent = extent;

                TerraLandWorldImagery.ImageType imgtype = new TerraLandWorldImagery.ImageType();
                imgtype.ImageFormat = TerraLandWorldImagery.esriImageFormat.esriImageJPG;
                imgtype.ImageReturnType = TerraLandWorldImagery.esriImageReturnType.esriImageReturnMimeData;

                TerraLandWorldImagery.ImageDisplay imgdisp = new TerraLandWorldImagery.ImageDisplay();
                imgdisp.ImageHeight = runTime.imageResolution;
                imgdisp.ImageWidth = runTime.imageResolution;
                imgdisp.ImageDPI = 96;

                TerraLandWorldImagery.ImageDescription imgdesc = new TerraLandWorldImagery.ImageDescription();
                imgdesc.ImageDisplay = imgdisp;
                imgdesc.ImageType = imgtype;

                try
                {
                    TerraLandWorldImagery.MapImage mapimg = mapservice.ExportMapImage(mapdesc, imgdesc);

                    imageBytes[i] = mapimg.ImageData;

                    if(!availableImageryCheked)
                        CheckImageColors(i);
                }
//                catch (Exception e)
//                {
//                    UnityEngine.Debug.Log(e);
//                    return;
//                }
                catch
                {}
                finally
                {
                    downloadedImageIndex++;
                }
            }
        }

        private static void CheckImageColors (int i)
        {
            MemoryStream ms = new MemoryStream(imageBytes[i]);
            Bitmap bmp = new Bitmap(ms);

            // Lock the bitmap's bits.  
            Rectangle rect = new Rectangle(0, 0, bmp.Width, bmp.Height);
            BitmapData bmpData = bmp.LockBits(rect, ImageLockMode.ReadWrite, bmp.PixelFormat);

            // Get the address of the first line.
            IntPtr ptr = bmpData.Scan0;

            // Declare an array to hold the bytes of the bitmap.
            int bytes  = bmpData.Stride * bmp.Height;
            byte[] rgbValues = new byte[bytes];

            // Copy the RGB values into the array.
            System.Runtime.InteropServices.Marshal.Copy(ptr, rgbValues, 0, bytes);

            allBlack = true;

            // Scanning for non-zero bytes
            for (int index = 0; index < rgbValues.Length; index++)
            {
                if (rgbValues[index] != 0) 
                {
                    allBlack = false;
                    break;
                }
            }

            // Unlock the bits.
            bmp.UnlockBits(bmpData);
            bmp.Dispose();

            availableImageryCheked = true;
        }

        private static void GenerateToken (bool north, bool south, bool east, bool west, int i)
        {
            HttpWebRequest req = (HttpWebRequest)WebRequest.Create(tokenURL);

            req.KeepAlive = false;
            req.ProtocolVersion = HttpVersion.Version10;
            req.Method = "POST";
            req.ContentType = "application/x-www-form-urlencoded";

            ServicePointManager.ServerCertificateValidationCallback = new System.Net.Security.RemoteCertificateValidationCallback(delegate {return true;});

            try
            {
                HttpWebResponse response = (HttpWebResponse)req.GetResponse();
                StreamReader sr = new StreamReader(response.GetResponseStream());
                string str = sr.ReadToEnd();
                token = str.Replace("{\"access_token\":\"", "").Replace("\",\"expires_in\":1209600}", "");
            }
            catch (Exception e)
            {
                if(north)
                    ElevationDownloadNORTH(i);
                else if(south)
                    ElevationDownloadSOUTH(i);
                else if(east)
                    ElevationDownloadEAST(i);
                else if(west)
                    ElevationDownloadWEST(i);

                UnityEngine.Debug.Log(e);
            }
        }

        private static double ToWebMercatorLat(double mercatorY_lat)
        {
            double num = mercatorY_lat * 0.017453292519943295;
            double result = 3189068.5 * Math.Log((1.0 + Math.Sin(num)) / (1.0 - Math.Sin(num)));
            return result;
        }

        private static double ToWebMercatorLon(double mercatorX_lon)
        {
            double num = mercatorX_lon * 0.017453292519943295;
            double result = 6378137.0 * num;
            return result;
        }

        private static void GenerateNewTerrainObject ()
        {
            SetData();

            CreateTerrainObject();

            if (gridSizeTerrain == 1)
            {
                terrain = currentTerrain;
                initialTerrainWidth = terrainSizeNewX;
            }
            else
            {
                splittedTerrains = terrainsParent;

                splittedTerrains.AddComponent<InfiniteTerrain>();

                CheckTerrainChunks();

                if(!runTime.IsCustomGeoServer)
                    initialTerrainWidth = terrainSizeNewX / splitSizeFinal;
                else
                    initialTerrainWidth = terrainSizeNewX / (int)Mathf.Sqrt((float)terrainChunks);
            }

            sceneIsInitialized = true;
            UnityEngine.Debug.Log("Scene Is Initialized");

            AddTerrainsToFloatingOrigin();
        }

        private static void AddTerrainsToFloatingOrigin ()
        {
            floatingOriginAdvanced = Camera.main.GetComponent<FloatingOriginAdvanced>();
            floatingOriginAdvanced.CollectObjectsOnce();
        }

        private static void SetData ()
        {
            terrainsLong = gridSizeTerrain;
            terrainsWide = gridSizeTerrain;

            oldWidth = terrainSizeNewX;
            oldHeight = terrainSizeNewY;
            oldLength = terrainSizeNewZ;

            newWidth = oldWidth / terrainsWide;
            newLength = oldLength / terrainsLong;

            xPos = (terrainSizeNewX / 2f) * -1f;
            yPos = 0f;
            zPos = (terrainSizeNewZ / 2f) * -1f;

            newHeightMapResolution = ((heightmapResolutionSplit - 1) / gridSizeTerrain) + 1;
            newEvenHeightMapResolution = newHeightMapResolution - 1;
        }

        private static void CreateTerrainObject ()
        {
            arrayPos = 0;

            if (gridSizeTerrain > 1)
            {
                terrainsParent = new GameObject ("Terrains" +"  ---  "+ gridSizeTerrain + "x" + gridSizeTerrain);
                terrainNames = new string[(int)Mathf.Pow(gridSizeTerrain, 2)];
                tempParnet = new GameObject("Temp Parent");
            }

            int currentRow = gridSizeTerrain;

            for(int y = 0; y < terrainsLong ; y++)
            {
                for(int x = 0; x < terrainsWide; x++)
                {
                    GameObject terrainGameObject = new GameObject("Terrain_" + (currentRow) + "-" + (x + 1));
                    terrainGameObject.AddComponent<Terrain>();

                    if (gridSizeTerrain > 1)
                    {
                        terrainNames[arrayPos] = terrainGameObject.name;
                        terrainGameObject.transform.parent = tempParnet.transform;
                    }

                    data = new TerrainData();
                    data.heightmapResolution = newEvenHeightMapResolution;
                    data.size = new Vector3(newWidth, oldHeight, newLength);
                    data.name = currentRow + "-" + (x + 1);
                    data.alphamapResolution = 16;

                    currentTerrain = terrainGameObject.GetComponent<Terrain>();
                    currentTerrain.terrainData = data;
                    currentTerrain.heightmapPixelError = runTime.heightmapPixelError;
                    currentTerrain.basemapDistance = terrainSizeNewX * 4f;

#if !UNITY_2019_1_OR_NEWER
                    currentTerrain.materialType = Terrain.MaterialType.Custom;
#endif
                    currentTerrain.materialTemplate = MaterialManager.GetTerrainMaterial();

                    currentTerrain.materialTemplate.renderQueue = 1900;
                    //currentTerrain.materialTemplate.renderQueue = -1;
                    //if (currentTerrain.materialTemplate.HasProperty("_MeshDistance")) currentTerrain.materialTemplate.SetFloat("_MeshDistance", runTime.terrainDistance);

                    if (currentTerrain.materialTemplate.HasProperty("_MeshDistance")) currentTerrain.materialTemplate.SetFloat("_MeshDistance", 0f);
                    if (currentTerrain.materialTemplate.HasProperty("_Curvature")) currentTerrain.materialTemplate.SetFloat("_Curvature", runTime.terrainCurvator);
                    if (currentTerrain.materialTemplate.HasProperty("_Smoothness0")) currentTerrain.materialTemplate.SetFloat("_Smoothness0", 0f);
                    if (currentTerrain.materialTemplate.HasProperty("_Smoothness1")) currentTerrain.materialTemplate.SetFloat("_Smoothness1", 0f);
                    if (currentTerrain.materialTemplate.HasProperty("_Smoothness2")) currentTerrain.materialTemplate.SetFloat("_Smoothness2", 0f);
                    if (currentTerrain.materialTemplate.HasProperty("_Smoothness3")) currentTerrain.materialTemplate.SetFloat("_Smoothness3", 0f);

                    if (runTime.showTileOnFinish)
                        currentTerrain.drawHeightmap = false;

#if UNITY_2018_3_OR_NEWER
                    currentTerrain.drawInstanced = true;
                    currentTerrain.groupingID = 0;
                    currentTerrain.allowAutoConnect = true;
#endif

                    terrainGameObject.AddComponent<TerrainCollider>();
                    terrainGameObject.GetComponent<TerrainCollider>().terrainData = data;

                    terrainGameObject.transform.position = new Vector3(x * newWidth + xPos, yPos, y * newLength + zPos);

                    terrainGameObject.layer = 8;

                    arrayPos++;
                }
                currentRow--;
            }

            if (gridSizeTerrain > 1)
            {
                terrainNames = LogicalComparer(terrainNames);

                for(int i = 0; i < terrainNames.Length; i++)
                {
                    tempParnet.transform.Find(terrainNames[i]).transform.parent = terrainsParent.transform;
                    terrainsParent.transform.Find(terrainNames[i]).name = (i + 1).ToString() +" "+ terrainNames[i];
                }

                spiralCell = new List<Vector2>();

//                if(!runTime.spiralGeneration)
//                {
//                    terrainNames = LogicalComparer(terrainNames);
//
//                    for(int i = 0; i < terrainNames.Length; i++)
//                    {
//                        tempParnet.transform.FindChild(terrainNames[i]).transform.parent = terrainsParent.transform;
//                        terrainsParent.transform.FindChild(terrainNames[i]).name = (i + 1).ToString() +" "+ terrainNames[i];
//                    }
//                }
//                else
//                {
//                    for(int i = 0; i < terrainNames.Length; i++)
//                    {
//                        string nameStr = terrainNames[spiralIndex[i]];
//                        tempParnet.transform.FindChild(nameStr).transform.parent = terrainsParent.transform;
//                        spiralCell.Add
//                        (
//                            new Vector2
//                            (
//                                int.Parse(nameStr.Remove(nameStr.LastIndexOf("-")).Replace("Terrain_", "")) - 0,
//                                int.Parse(nameStr.Substring(nameStr.LastIndexOf("-") + 1)) - 0
//                            )
//                        );
//                    }
//                }

                DestroyImmediate(tempParnet);
            }
        }

        private static void CreateFarTerrainObject ()
        {
            farTerrainSize = runTime.areaSize * runTime.areaSizeFarMultiplier * 1000f;

            for(int i = 1; i <= 2; i++)
            {
                GameObject terrainGameObject = new GameObject("Far Terrain " + i.ToString());
                terrainGameObject.AddComponent<Terrain>();

                data = new TerrainData();
                data.heightmapResolution = runTime.farTerrainHeightmapResolution + 1;
                data.size = new Vector3(farTerrainSize, oldHeight, farTerrainSize);
                data.name = "Far Terrain " + i.ToString();
                data.alphamapResolution = 16;

                currentTerrain = terrainGameObject.GetComponent<Terrain>();
                currentTerrain.terrainData = data;
                currentTerrain.heightmapPixelError = runTime.farTerrainQuality;
                currentTerrain.basemapDistance = farTerrainSize * 4f;
                //currentTerrain.basemapDistance = terrainSizeNewX / 4f;

#if !UNITY_2019_1_OR_NEWER
                currentTerrain.materialType = Terrain.MaterialType.Custom;
#endif
                currentTerrain.materialTemplate = MaterialManager.GetTerrainMaterial();

                currentTerrain.materialTemplate.renderQueue = 1899;
                //currentTerrain.materialTemplate.renderQueue = -1;
                //if (currentTerrain.materialTemplate.HasProperty("_MeshDistance")) currentTerrain.materialTemplate.SetFloat("_MeshDistance", runTime.terrainDistance * runTime.areaSizeFarMultiplier);

                if (currentTerrain.materialTemplate.HasProperty("_MeshDistance")) currentTerrain.materialTemplate.SetFloat("_MeshDistance", runTime.terrainDistance);
                if (currentTerrain.materialTemplate.HasProperty("_Curvature")) currentTerrain.materialTemplate.SetFloat("_Curvature", runTime.terrainCurvator / runTime.areaSizeFarMultiplier);
                if (currentTerrain.materialTemplate.HasProperty("_Smoothness0")) currentTerrain.materialTemplate.SetFloat("_Smoothness0", 0f);
                if (currentTerrain.materialTemplate.HasProperty("_Smoothness1")) currentTerrain.materialTemplate.SetFloat("_Smoothness1", 0f);
                if (currentTerrain.materialTemplate.HasProperty("_Smoothness2")) currentTerrain.materialTemplate.SetFloat("_Smoothness2", 0f);
                if (currentTerrain.materialTemplate.HasProperty("_Smoothness3")) currentTerrain.materialTemplate.SetFloat("_Smoothness3", 0f);

                currentTerrain.drawHeightmap = false;
                currentTerrain.castShadows = false;

                terrainGameObject.AddComponent<TerrainCollider>();
                terrainGameObject.GetComponent<TerrainCollider>().terrainData = data;
                terrainGameObject.GetComponent<TerrainCollider>().enabled = false;

                terrainGameObject.transform.position = new Vector3
                    (
                        -(farTerrainSize / 2f),
                        -runTime.farTerrainBelowHeight,
                        -(farTerrainSize / 2f)
                    );

                terrainGameObject.layer = 8;

                if(i == 1)
                    firstTerrain = currentTerrain;
                else if(i == 2)
                    secondaryTerrain = currentTerrain;
            }

            terrain = firstTerrain;
            secondaryTerrainInProgress = true;

            farTerrainDummy = new GameObject("Far Terrain Dummy");
            farTerrainDummy.transform.position = terrain.transform.position;
        }

        private static string[] LogicalComparer (string filePath, string fileType)
        {
            string[] names = Directory.GetFiles(filePath, "*" + fileType, SearchOption.AllDirectories);
            ns.NumericComparer ns = new ns.NumericComparer();
            Array.Sort(names, ns);

            return names;
        }

        private static string[] LogicalComparer (string[] names)
        {
            ns.NumericComparer ns = new ns.NumericComparer();
            Array.Sort(names, ns);

            return names;
        }

        private static void CheckTerrainChunks ()
        {
            if(splittedTerrains.transform.childCount > 0)
            {
                int counter = 0;

                foreach (Transform t in splittedTerrains.transform)
                {
                    if(t.GetComponent<Terrain>() != null)
                    {
                        if(counter == 0)
                            croppedTerrains = new List<Terrain>();

                        croppedTerrains.Add(t.GetComponent<Terrain>());
                        counter++;
                    }
                }
                terrainChunks = counter;
            }
        }

        private static void RemoveLightmapStatic()
        {
#if UNITY_EDITOR
#if UNITY_2019_1_OR_NEWER
            if (splittedTerrains)
            {
                foreach (Terrain t in croppedTerrains)
                {
                    UnityEditor.StaticEditorFlags flags = UnityEditor.GameObjectUtility.GetStaticEditorFlags(t.gameObject);
                    flags = flags & ~(UnityEditor.StaticEditorFlags.ContributeGI);
                    UnityEditor.GameObjectUtility.SetStaticEditorFlags(t.gameObject, flags);
                }
            }
            else if (terrain)
            {
                UnityEditor.StaticEditorFlags flags = UnityEditor.GameObjectUtility.GetStaticEditorFlags(terrain.gameObject);
                flags = flags & ~(UnityEditor.StaticEditorFlags.ContributeGI);
                UnityEditor.GameObjectUtility.SetStaticEditorFlags(terrain.gameObject, flags);
            }
#else
            if (splittedTerrains)
            {
                foreach (Terrain t in croppedTerrains)
                {
                    UnityEditor.StaticEditorFlags flags = UnityEditor.GameObjectUtility.GetStaticEditorFlags(t.gameObject);
                    flags = flags & ~(UnityEditor.StaticEditorFlags.LightmapStatic);
                    UnityEditor.GameObjectUtility.SetStaticEditorFlags(t.gameObject, flags);
                }
            }
            else if (terrain)
            {
                UnityEditor.StaticEditorFlags flags = UnityEditor.GameObjectUtility.GetStaticEditorFlags(terrain.gameObject);
                flags = flags & ~(UnityEditor.StaticEditorFlags.LightmapStatic);
                UnityEditor.GameObjectUtility.SetStaticEditorFlags(terrain.gameObject, flags);
            }
#endif
#endif
        }

        public static void SmoothHeights (float[,] terrainData, int width, int height)
        {
            //File.Delete(fileNameTerrainData);

            if(runTime.smoothIterations > 0)
                FinalizeSmooth(terrainData, width, height, runTime.smoothIterations, smoothBlendIndex, smoothBlend);

            if (!runTime.IsCustomGeoServer)
                CalculateResampleHeightmaps();
            else
                CalculateResampleHeightmapsGeoServer();
        }

        public static void FinalizeHeights ()
        {
            if (runTime.IsCustomGeoServer)
            {
                if(geoDataExtensionElevation.Equals("raw"))
                    Timing.RunCoroutine(LoadTerrainHeightsFromRAW());
                else if(geoDataExtensionElevation.Equals("tif"))
                    Timing.RunCoroutine(LoadTerrainHeightsFromTIFF());
                else if(geoDataExtensionElevation.Equals("asc"))
                    Timing.RunCoroutine(LoadTerrainHeightsFromASCII());
            }
            else
                Timing.RunCoroutine(LoadTerrainHeightsFromTIFF());


            if(generatedTerrainsCount == terrainChunks)
            {
                if(splittedTerrains)
                    ManageNeighborings();
            }
        }

        public static void GetElevationFileInfo ()
        {
            if(geoDataExtensionElevation.Equals("raw"))
                GetRAWInfo();
            else if(geoDataExtensionElevation.Equals("tif"))
                GetTIFFInfo();
            else if(geoDataExtensionElevation.Equals("asc"))
               GetASCIIInfo();
        }

        public static void ApplyOfflineTerrain ()
        {
            if(geoDataExtensionElevation.Equals("raw"))
                runTime.heightmapResolution = m_Width;
            else if(geoDataExtensionElevation.Equals("tif"))
                runTime.heightmapResolution = tiffWidth;
            else if(geoDataExtensionElevation.Equals("asc"))
                runTime.heightmapResolution = nRows;

            SetupDownloaderElevationGeoServer();

            if(geoDataExtensionElevation.Equals("raw"))
                runTime.TerrainFromRAW();
            else if(geoDataExtensionElevation.Equals("tif"))
                runTime.TerrainFromTIFF();
            else if(geoDataExtensionElevation.Equals("asc"))
                runTime.TerrainFromASCII();
        }

        public static void TiffData (string fileName)
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
                        }
                    }
                }
            }
            catch (Exception e)
            {
                UnityEngine.Debug.Log(e);
            }

            highestPoint = tiffDataASCII.Cast<float>().Max();
            lowestPoint = tiffDataASCII.Cast<float>().Min();

            // Rotate terrain heigts and normalize values
            for (int y = 0; y < tiffWidth; y++)
            {
                for (int x = 0; x < tiffLength; x++)
                {
                    currentHeight = tiffDataASCII[(tiffWidth - 1) - y, x];

                    try
                    {
                        if(lowestPoint >= 0)
                            tiffData[y, x] = (currentHeight - lowestPoint) / everestPeak;
                        else
                            tiffData[y, x] = (currentHeight + Mathf.Abs(lowestPoint)) / everestPeak;
                    }
                    catch(ArgumentOutOfRangeException)
                    {
                        tiffData[y, x] = 0f;
                    }

                    // Check Terrain Corners
                    // Top Row
                    if(y == 0)
                        topCorner.Add(currentHeight);

                    // Bottom Row
                    else if(y == tiffWidth - 1)
                        bottomCorner.Add(currentHeight);

                    // Left Column
                    if(x == 0)
                        leftCorner.Add(currentHeight);

                    // Right Column
                    else if(x == tiffLength - 1)
                        rightCorner.Add(currentHeight);
                }
            }

            CheckCornersTIFF();
        }

        public static float[,] TiffDataDynamic (string fileName, int index)
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
                        }
                    }
                }
            }
            catch (Exception e)
            {
                UnityEngine.Debug.Log(e);
            }

            if(downloadedHeightmapIndex == 0)
                lowestPoint = tiffDataASCII.Cast<float>().Min();

            if(!worldIsGenerated)
                highestPoints.Add(tiffDataASCII.Cast<float>().Max());
            else
                highestPoints[index] = tiffDataASCII.Cast<float>().Max();

            // Rotate terrain heigts and normalize values
            for (int y = 0; y < tiffWidth; y++)
            {
                for (int x = 0; x < tiffLength; x++)
                {
                    currentHeight = tiffDataASCII[(tiffWidth - 1) - y, x];

                    try
                    {
                        if(lowestPoint >= 0)
                            tiffData[y, x] = (currentHeight - lowestPoint) / everestPeak;
                        else
                            tiffData[y, x] = (currentHeight + Mathf.Abs(lowestPoint)) / everestPeak;
                    }
                    catch(ArgumentOutOfRangeException)
                    {
                        tiffData[y, x] = 0f;
                    }
                }
            }

            CheckCornersTIFF();

            if(runTime.smoothIterations > 0)
                FinalizeSmooth(tiffData, tiffWidth, tiffLength, runTime.smoothIterations, smoothBlendIndex, smoothBlend);

            return tiffData;
        }

        public static float[,] TiffDataDynamicFAR (string fileName)
        {
            try
            {
                using (Tiff inputImage = Tiff.Open(fileName, "r"))
                {
                    tiffWidthFAR = inputImage.GetField(TiffTag.IMAGEWIDTH)[0].ToInt();
                    tiffLengthFAR = inputImage.GetField(TiffTag.IMAGELENGTH)[0].ToInt();
                    tiffDataFAR = new float[tiffLengthFAR, tiffWidthFAR];
                    tiffDataASCIIFAR = new float[tiffLengthFAR, tiffWidthFAR];

                    int tileHeight = inputImage.GetField(TiffTag.TILELENGTH)[0].ToInt();
                    int tileWidth = inputImage.GetField(TiffTag.TILEWIDTH)[0].ToInt();

                    byte[] buffer = new byte[tileHeight * tileWidth * 4];
                    float[,] fBuffer = new float[tileHeight, tileWidth];

                    for (int y = 0; y < tiffLengthFAR; y += tileHeight)
                    {
                        for (int x = 0; x < tiffWidthFAR; x += tileWidth)
                        {
                            inputImage.ReadTile(buffer, 0, x, y, 0, 0);
                            Buffer.BlockCopy(buffer, 0, fBuffer, 0, buffer.Length);

                            for (int i = 0; i < tileHeight; i++)
                                for (int j = 0; j < tileWidth; j++)
                                    if ((y + i) < tiffLengthFAR && (x + j) < tiffWidthFAR)
                                        tiffDataASCIIFAR[y + i, x + j] = fBuffer[i, j];
                        }
                    }
                }
            }
            catch (Exception e)
            {
                UnityEngine.Debug.Log(e);
            }

            // Rotate terrain heigts and normalize values
            for (int y = 0; y < tiffWidthFAR; y++)
            {
                for (int x = 0; x < tiffLengthFAR; x++)
                {
                    currentHeight = tiffDataASCIIFAR[(tiffWidthFAR - 1) - y, x];

                    try
                    {
                        if(lowestPoint >= 0)
                            tiffDataFAR[y, x] = (currentHeight - lowestPoint) / everestPeak;
                        else
                            tiffDataFAR[y, x] = (currentHeight + Mathf.Abs(lowestPoint)) / everestPeak;
                    }
                    catch(ArgumentOutOfRangeException)
                    {
                        tiffDataFAR[y, x] = 0f;
                    }
                }
            }

            CheckCornersTIFFFAR();

            //if(runTime.smoothIterations > 0)
                //FinalizeSmoothFAR(tiffDataFar, tiffWidthFAR, tiffLengthFAR, runTime.smoothIterations, smoothBlendIndex, smoothBlend);

            return tiffDataFAR;
        }

        public static void RawData (string fileName)
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
                    }
                }
            }

            highestPoint = rawData.Cast<float>().Max() * everestPeak;
            lowestPoint = rawData.Cast<float>().Min() * everestPeak;
            float lowestPointNormalized = rawData.Cast<float>().Min();

            if (m_Depth == Depth.Bit16)
            {
                for (int y = 0; y < m_Width; y++)
                {
                    for (int x = 0; x < m_Height; x++)
                    {
                        if(lowestPointNormalized >= 0)
                            rawData[(m_Width - 1) - y, x] -= lowestPointNormalized;
                        else
                            rawData[(m_Width - 1) - y, x] += Mathf.Abs(lowestPointNormalized);

                        // Check Terrain Corners
                        // Top Row
                        if(y == 0)
                            topCorner.Add(currentHeight);

                        // Bottom Row
                        else if(y == m_Width - 1)
                            bottomCorner.Add(currentHeight);

                        // Left Column
                        if(x == 0)
                            leftCorner.Add(currentHeight);

                        // Right Column
                        else if(x == m_Height - 1)
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
                        if(y == 0)
                            topCorner.Add(currentHeight);

                        // Bottom Row
                        else if(y == m_Width - 1)
                            bottomCorner.Add(currentHeight);

                        // Left Column
                        if(x == 0)
                            leftCorner.Add(currentHeight);

                        // Right Column
                        else if(x == m_Height - 1)
                            rightCorner.Add(currentHeight);
                    }
                }
            }

            CheckCornersRAW();
        }

        private static void PickRawDefaults(string fileName)
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

        public static void AsciiData (string fileName)
        {
            StreamReader sr = new StreamReader(fileName, Encoding.ASCII, true);

            //ncols
            string[] line1 = sr.ReadLine().Replace(',', '.').Split(new char[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
            nCols = (Convert.ToInt32(line1[1]));
            //nrows
            string[] line2 = sr.ReadLine().Replace(',', '.').Split(new char[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
            nRows = (Convert.ToInt32(line2[1]));

//            //xllcorner
//            string[] line3 = sr.ReadLine().Replace(',', '.').Split(new char[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
//            //yllcorner
//            string[] line4 = sr.ReadLine().Replace(',', '.').Split(new char[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
//            //cellsize
//            string[] line5 = sr.ReadLine().Replace(',', '.').Split(new char[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
//            //nodata
//            string[] line6 = sr.ReadLine().Replace(',', '.').Split(new char[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);

            //xllcorner
            sr.ReadLine();
            //yllcorner
            sr.ReadLine();
            //cellsize
            sr.ReadLine();
            //nodata
            sr.ReadLine();

            asciiData = new float[nCols, nRows];

            heightmapResX = Mathf.FloorToInt((float)Mathf.ClosestPowerOfTwo(nRows) / (float)splitSizeFinal);
            heightmapResY = Mathf.FloorToInt((float)Mathf.ClosestPowerOfTwo(nCols) / (float)splitSizeFinal);
            heightmapResXAll = nRows;
            heightmapResYAll = nCols;

            for (int y = 0; y < nRows; y++)
            {
                string[] line = sr.ReadLine().Split(new char[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);

                for (int x = 0; x < nCols; x++)
                {
                    currentHeight = float.Parse(line[x].Replace(',', '.'));
                    asciiData[(nRows - 1) - y, x] = currentHeight / everestPeak;
                }
            }

            sr.Close();

            highestPoint = asciiData.Cast<float>().Max() * everestPeak;
            lowestPoint = asciiData.Cast<float>().Min() * everestPeak;
            float lowestPointNormalized = asciiData.Cast<float>().Min();

            for (int y = 0; y < nRows; y++)
            {
                for (int x = 0; x < nCols; x++)
                {
                    if(lowestPointNormalized >= 0)
                        asciiData[(nRows - 1) - y, x] -= lowestPointNormalized;
                    else
                        asciiData[(nRows - 1) - y, x] += Mathf.Abs(lowestPointNormalized);

                    // Check Terrain Corners
                    // Top Row
                    if(y == 0)
                        topCorner.Add(currentHeight);

                    // Bottom Row
                    else if(y == nRows - 1)
                        bottomCorner.Add(currentHeight);

                    // Left Column
                    if(x == 0)
                        leftCorner.Add(currentHeight);

                    // Right Column
                    else if(x == nCols - 1)
                        rightCorner.Add(currentHeight);
                }
            }

            CheckCornersASCII();
        }

        private static void CheckCornersTIFF ()
        {
            // Check Top
            if (topCorner.All(o => o == topCorner.First()))
            {
                for (int y = 0; y < tiffWidth; y++)
                    for (int x = 0; x < tiffLength; x++)
                        if(y == 0)
                            tiffData[y, x] = tiffData[y + 1, x];
            }

            // Check Bottom
            if (bottomCorner.All(o => o == bottomCorner.First()))
            {
                for (int y = 0; y < tiffWidth; y++)
                    for (int x = 0; x < tiffLength; x++)
                        if(y == tiffWidth - 1)
                            tiffData[y, x] = tiffData[y - 1, x];
            }

            // Check Left
            if (leftCorner.All(o => o == leftCorner.First()))
            {
                for (int y = 0; y < tiffWidth; y++)
                    for (int x = 0; x < tiffLength; x++)
                        if(x == 0)
                            tiffData[y, x] = tiffData[y, x + 1];
            }

            // Check Right
            if (rightCorner.All(o => o == rightCorner.First()))
            {
                for (int y = 0; y < tiffWidth; y++)
                    for (int x = 0; x < tiffLength; x++)
                        if(x == tiffLength - 1)
                            tiffData[y, x] = tiffData[y, x - 1];
            }
        }

        private static void CheckCornersTIFFFAR ()
        {
            // Check Top
            if (topCorner.All(o => o == topCorner.First()))
            {
                for (int y = 0; y < tiffWidthFAR; y++)
                    for (int x = 0; x < tiffLengthFAR; x++)
                        if(y == 0)
                            tiffDataFAR[y, x] = tiffDataFAR[y + 1, x];
            }

            // Check Bottom
            if (bottomCorner.All(o => o == bottomCorner.First()))
            {
                for (int y = 0; y < tiffWidthFAR; y++)
                    for (int x = 0; x < tiffLengthFAR; x++)
                        if(y == tiffWidthFAR - 1)
                            tiffDataFAR[y, x] = tiffDataFAR[y - 1, x];
            }

            // Check Left
            if (leftCorner.All(o => o == leftCorner.First()))
            {
                for (int y = 0; y < tiffWidthFAR; y++)
                    for (int x = 0; x < tiffLengthFAR; x++)
                        if(x == 0)
                            tiffDataFAR[y, x] = tiffDataFAR[y, x + 1];
            }

            // Check Right
            if (rightCorner.All(o => o == rightCorner.First()))
            {
                for (int y = 0; y < tiffWidthFAR; y++)
                    for (int x = 0; x < tiffLengthFAR; x++)
                        if(x == tiffLengthFAR - 1)
                            tiffDataFAR[y, x] = tiffDataFAR[y, x - 1];
            }
        }

        private static void CheckCornersASCII ()
        {
            // Check Top
            if (topCorner.All(o => o == topCorner.First()))
            {
                for (int y = 0; y < nRows; y++)
                    for (int x = 0; x < nCols; x++)
                        if(y == 0)
                            asciiData[(nRows - 1) - y, x] = asciiData[(nRows - 1) - (y + 1), x];
            }

            // Check Bottom
            if (bottomCorner.All(o => o == bottomCorner.First()))
            {
                for (int y = 0; y < nRows; y++)
                    for (int x = 0; x < nCols; x++)
                        if(y == nRows - 1)
                            asciiData[(nRows - 1) - y, x] = asciiData[(nRows - 1) - (y - 1), x];
            }

            // Check Left
            if (leftCorner.All(o => o == leftCorner.First()))
            {
                for (int y = 0; y < nRows; y++)
                    for (int x = 0; x < nCols; x++)
                        if(x == 0)
                            asciiData[(nRows - 1) - y, x] = asciiData[(nRows - 1) - y, x + 1];
            }

            // Check Right
            if (rightCorner.All(o => o == rightCorner.First()))
            {
                for (int y = 0; y < nRows; y++)
                    for (int x = 0; x < nCols; x++)
                        if(x == nCols - 1)
                            asciiData[(nRows - 1) - y, x] = asciiData[(nRows - 1) - y, x - 1];
            }
        }

        private static void CheckCornersRAW ()
        {
            // Check Top
            if (topCorner.All(o => o == topCorner.First()))
            {
                for (int y = 0; y < m_Width; y++)
                    for (int x = 0; x < m_Height; x++)
                        if(y == 0)
                            rawData[(m_Width - 1) - y, x] = rawData[(m_Width - 1) - (y + 1), x];
            }

            // Check Bottom
            if (bottomCorner.All(o => o == bottomCorner.First()))
            {
                for (int y = 0; y < m_Width; y++)
                    for (int x = 0; x < m_Height; x++)
                        if(y == m_Width - 1)
                            rawData[(m_Width - 1) - y, x] = rawData[(m_Width - 1) - (y - 1), x];
            }

            // Check Left
            if (leftCorner.All(o => o == leftCorner.First()))
            {
                for (int y = 0; y < m_Width; y++)
                    for (int x = 0; x < m_Height; x++)
                        if(x == 0)
                            rawData[(m_Width - 1) - y, x] = rawData[(m_Width - 1) - y, x + 1];
            }

            // Check Right
            if (rightCorner.All(o => o == rightCorner.First()))
            {
                for (int y = 0; y < m_Width; y++)
                    for (int x = 0; x < m_Height; x++)
                        if(x == m_Height - 1)
                            rawData[(m_Width - 1) - y, x] = rawData[(m_Width - 1) - y, x - 1];
            }
        }

        public static void FinalizeSmooth (float[,] heightMapSmoothed, int width, int height, int iterations, int blendIndex, float blending)
        {
            if(iterations != 0)
            {
                int Tw = width;
                int Th = height;

                if(blendIndex == 1)
                {
                    float[,] generatedHeightMap = (float[,])heightMapSmoothed.Clone();
                    generatedHeightMap = SmoothedHeights(generatedHeightMap, Tw, Th, iterations);

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
                    }
                }
                else
                    heightMapSmoothed = SmoothedHeights(heightMapSmoothed, Tw, Th, iterations);
            }
        }

        private static float[,] SmoothedHeights(float[,] heightMap, int tw, int th, int iterations)
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

        public static void FinalizeSmoothFAR (float[,] heightMapSmoothed, int width, int height, int iterations, int blendIndex, float blending)
        {
            if(iterations != 0)
            {
                int Tw = width;
                int Th = height;

                if(blendIndex == 1)
                {
                    float[,] generatedHeightMap = (float[,])heightMapSmoothed.Clone();
                    generatedHeightMap = SmoothedHeightsFAR(generatedHeightMap, Tw, Th, iterations);

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
                    }
                }
                else
                    heightMapSmoothed = SmoothedHeightsFAR(heightMapSmoothed, Tw, Th, iterations);
            }
        }

        private static float[,] SmoothedHeightsFAR(float[,] heightMap, int tw, int th, int iterations)
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

        public static IEnumerator<float> LoadTerrainHeightsFromTIFF ()
        {
            int counter = 0;
            int currentRow = splitSizeFinal - 1;
            int xLength = heightmapResFinalX;
            int yLength = heightmapResFinalY;
            int xStart = 0;
            int yStart = 0;

            if(splittedTerrains)
            {
                for (int i = 0; i < splitSizeFinal; i++)
                {
                    for (int j = 0; j < splitSizeFinal; j++)
                    {
                        if(counter >= taskIndex - runTime.concurrentTasks && counter < taskIndex)
                        {
                            croppedTerrains[counter].terrainData.heightmapResolution = heightmapResFinalX;
                            float[,] tiffDataSplitted = new float[heightmapResFinalX, heightmapResFinalY];

                            if(!runTime.spiralGeneration)
                            {
                                xStart = (currentRow * (heightmapResFinalX - 1));
                                yStart = (j * (heightmapResFinalY - 1));
                            }
                            else
                            {
                                xStart = ((splitSizeFinal - ((int)spiralCell[counter].x  - 1)) - 1) * (heightmapResFinalX - 1);
                                yStart = ((int)spiralCell[counter].y - 1) * (heightmapResFinalY - 1);
                            }
                            try
                            {
                                for(int x = 0; x < xLength; x++)
                                    for(int y = 0; y < yLength; y++)
                                        tiffDataSplitted[x, y] = finalHeights[xStart + x, yStart + y];

                                Timing.RunCoroutine(FillHeights(croppedTerrains[counter], heightmapResFinalX, tiffDataSplitted));

                                realTerrainWidth = areaSizeLon * 1000.0f / splitSizeFinal;
                                realTerrainLength = areaSizeLat * 1000.0f / splitSizeFinal;

                                croppedTerrains[counter].terrainData.size = RealTerrainSize(realTerrainWidth, realTerrainLength, highestPoint);
                                croppedTerrains[counter].Flush();
                            }
                            catch(Exception e)
                            {
                                UnityEngine.Debug.Log(e);
                            }
                        }

                        counter++;
                    }
                    currentRow--;
                }

                yield return 0;
            }
            else if (terrain)
            {
                terrain.terrainData.heightmapResolution = heightmapResFinalXAll;

                try
                {
                    Timing.RunCoroutine(FillHeights(terrain, heightmapResFinalXAll, finalHeights));

                    realTerrainWidth = areaSizeLon * 1000.0f;
                    realTerrainLength = areaSizeLat * 1000.0f;

                    terrain.terrainData.size = RealTerrainSize(realTerrainWidth, realTerrainLength, highestPoint);
                    terrain.Flush();
                }
                catch(Exception e)
                {
                    UnityEngine.Debug.Log(e);
                }
            }
        }

        public static IEnumerator<float> LoadTerrainHeightsFromTIFFDynamic (int i)
        {
            if(splittedTerrains)
            {
                if(runTime.progressiveGeneration)
                {
                    if(concurrentUpdates > runTime.concurrentTasks - 1)
                    {
                        yield return Timing.WaitForSeconds(1);
                        Timing.RunCoroutine(LoadTerrainHeightsFromTIFFDynamic(i));
                    }
                    else
                    {
                        try
                        {
                            croppedTerrains[i].terrainData.heightmapResolution = tileResolution;

                            Timing.RunCoroutine(FillHeightsDynamic(croppedTerrains[i], tileResolution, tiffDataDynamic[i]));

                            realTerrainWidth = areaSizeLon * 1000.0f;
                            realTerrainLength = areaSizeLat * 1000.0f;

                            croppedTerrains[i].terrainData.size = RealTerrainSize(realTerrainWidth, realTerrainLength, highestPoint);
                            croppedTerrains[i].Flush();
                        }
                        catch(Exception e)
                        {
                            UnityEngine.Debug.Log(e);
                        }
                    }
                }
                else
                {
                    for (int x = 0; x < terrainChunks; x++)
                    {
                        if(runTime.fastStartBuild)
                        {
                            try
                            {
                                int index = spiralIndex[x];

                                croppedTerrains[index].terrainData.heightmapResolution = tileResolution;
                                croppedTerrains[index].terrainData.SetHeights(0, 0, tiffDataDynamic[index]);

                                realTerrainWidth = areaSizeLon * 1000.0f;
                                realTerrainLength = areaSizeLat * 1000.0f;

                                croppedTerrains[index].terrainData.size = RealTerrainSize(realTerrainWidth, realTerrainLength, highestPoint);
                                croppedTerrains[index].Flush();

                                croppedTerrains[index].drawHeightmap = true;
                            }
                            catch(Exception e)
                            {
                                UnityEngine.Debug.Log(e);
                            }
                        }
                        else
                        {
                            if(x >= taskIndex - runTime.concurrentTasks && x < taskIndex)
                            {
                                try
                                {
                                    int index = spiralIndex[x];

                                    croppedTerrains[index].terrainData.heightmapResolution = tileResolution;

                                    Timing.RunCoroutine(FillHeightsDynamic(croppedTerrains[index], tileResolution, tiffDataDynamic[index]));

                                    realTerrainWidth = areaSizeLon * 1000.0f;
                                    realTerrainLength = areaSizeLat * 1000.0f;

                                    croppedTerrains[index].terrainData.size = RealTerrainSize(realTerrainWidth, realTerrainLength, highestPoint);
                                    croppedTerrains[index].Flush();

                                    croppedTerrains[index].drawHeightmap = true;
                                }
                                catch(Exception e)
                                {
                                    UnityEngine.Debug.Log(e);
                                }
                            }
                        }
                    }

                    if(runTime.stitchTerrainTiles)
                        Timing.RunCoroutine(StitchTerrain(croppedTerrains, 0f, terrainChunks));
                    else
                    {
                        if(!terrainsAreGenerated)
                        {
                            terrainsAreGenerated = true;
                            UnityEngine.Debug.Log("Terrains Are Generated");

                            if(runTime.elevationOnly)
                            {
                                if(runTime.farTerrain)
                                {
                                    if(farTerrainIsGenerated)
                                        Timing.RunCoroutine(WorldIsGenerated());
                                }
                                else
                                    Timing.RunCoroutine(WorldIsGenerated());
                            }
                            else
                            {
                                if(imagesAreGenerated)
                                {
                                    if(runTime.farTerrain)
                                    {
                                        if(farTerrainIsGenerated)
                                            Timing.RunCoroutine(WorldIsGenerated());
                                    }
                                    else
                                        Timing.RunCoroutine(WorldIsGenerated());
                                }
                            }
                        }
                    }
                }

                yield return 0;
            }
            else if (terrain)
            {
                try
                {
                    terrain.terrainData.heightmapResolution = runTime.heightmapResolution + 1;

                    Timing.RunCoroutine(FillHeightsDynamic(terrain, runTime.heightmapResolution + 1, finalHeights));

                    realTerrainWidth = areaSizeLon * 1000.0f;
                    realTerrainLength = areaSizeLat * 1000.0f;

                    terrain.terrainData.size = RealTerrainSize(realTerrainWidth, realTerrainLength, highestPoint);
                    terrain.Flush();
                }
                catch(Exception e)
                {
                    UnityEngine.Debug.Log(e);
                }
            }
        }

        public static IEnumerator<float> LoadTerrainHeightsFromTIFFFAR ()
        {
            try
            {
                terrain.terrainData.heightmapResolution = runTime.farTerrainHeightmapResolution + 1;

                Timing.RunCoroutine(FillHeightsDynamicFAR(terrain, runTime.farTerrainHeightmapResolution + 1, tiffDataFAR));

                terrain.terrainData.size = RealTerrainSizeFAR(farTerrainSize, farTerrainSize, highestPoint);
                terrain.Flush();
            }
            catch(Exception e)
            {
                UnityEngine.Debug.Log(e);
            }

            yield return 0;
        }

        public static IEnumerator<float> LoadTerrainHeightsFromTIFFNORTH (int i)
        {
            try
            {
                if(InfiniteTerrain.northTerrains.Count > 0)
                {
                    Terrain currentTerrain = splittedTerrains.transform.Find(InfiniteTerrain.northTerrains[0]).GetComponent<Terrain>();

                    currentTerrain.terrainData.heightmapResolution = tileResolution;

                    Timing.RunCoroutine(FillHeightsNORTH(currentTerrain, tileResolution, tiffDataDynamic[i]));

                    realTerrainWidth = areaSizeLon * 1000.0f;
                    realTerrainLength = areaSizeLat * 1000.0f;

                    currentTerrain.terrainData.size = RealTerrainSize(realTerrainWidth, realTerrainLength, highestPoint);
                    currentTerrain.Flush();

                    InfiniteTerrain.northTerrains.Remove(currentTerrain.name);
                }


////                //if(index == 12)
////                if(index.Equals(InfiniteTerrain.northIndexes[0]))
////                    Timing.RunCoroutine(FillHeightsNORTH2());
//
//
//                Terrain currentTerrain = splittedTerrains.transform.FindChild(InfiniteTerrain.northTerrains[0]).GetComponent<Terrain>();
//                //Terrain currentTerrain = splittedTerrains.transform.GetChild(index).GetComponent<Terrain>();
//
//                //print(currentTerrain.name);
//
//                currentTerrain.terrainData.heightmapResolution = tileResolution;
//
//                Timing.RunCoroutine(FillHeightsNORTH(currentTerrain, tileResolution, tiffDataDynamic[i]));
//
//                realTerrainWidth = areaSizeLon * 1000.0f;
//                realTerrainLength = areaSizeLat * 1000.0f;
//
//                currentTerrain.terrainData.size = RealTerrainSize(realTerrainWidth, realTerrainLength, highestPoint);
//                currentTerrain.Flush();
            }
            catch(Exception e)
            {
                UnityEngine.Debug.Log(e);
            }

            yield return 0;
        }

        public static IEnumerator<float> LoadTerrainHeightsFromTIFFSOUTH (int i)
        {
            try
            {
                if(InfiniteTerrain.southTerrains.Count > 0)
                {
                    Terrain currentTerrain = splittedTerrains.transform.Find(InfiniteTerrain.southTerrains[0]).GetComponent<Terrain>();

                    currentTerrain.terrainData.heightmapResolution = tileResolution;

                    Timing.RunCoroutine(FillHeightsSOUTH(currentTerrain, tileResolution, tiffDataDynamic[i]));

                    realTerrainWidth = areaSizeLon * 1000.0f;
                    realTerrainLength = areaSizeLat * 1000.0f;

                    currentTerrain.terrainData.size = RealTerrainSize(realTerrainWidth, realTerrainLength, highestPoint);
                    currentTerrain.Flush();

                    InfiniteTerrain.southTerrains.Remove(currentTerrain.name);
                }
            }
            catch(Exception e)
            {
                UnityEngine.Debug.Log(e);
            }

            yield return 0;
        }

        public static IEnumerator<float> LoadTerrainHeightsFromTIFFEAST (int i)
        {
            try
            {
                if(InfiniteTerrain.eastTerrains.Count > 0)
                {
                    Terrain currentTerrain = splittedTerrains.transform.Find(InfiniteTerrain.eastTerrains[0]).GetComponent<Terrain>();

                    currentTerrain.terrainData.heightmapResolution = tileResolution;

                    Timing.RunCoroutine(FillHeightsEAST(currentTerrain, tileResolution, tiffDataDynamic[i]));

                    realTerrainWidth = areaSizeLon * 1000.0f;
                    realTerrainLength = areaSizeLat * 1000.0f;

                    currentTerrain.terrainData.size = RealTerrainSize(realTerrainWidth, realTerrainLength, highestPoint);
                    currentTerrain.Flush();

                    InfiniteTerrain.eastTerrains.Remove(currentTerrain.name);
                }
            }
            catch(Exception e)
            {
                UnityEngine.Debug.Log(e);
            }

            yield return 0;
        }

        public static IEnumerator<float> LoadTerrainHeightsFromTIFFWEST (int i)
        {
            try
            {
                if(InfiniteTerrain.westTerrains.Count > 0)
                {
                    Terrain currentTerrain = splittedTerrains.transform.Find(InfiniteTerrain.westTerrains[0]).GetComponent<Terrain>();

                    currentTerrain.terrainData.heightmapResolution = tileResolution;

                    Timing.RunCoroutine(FillHeightsWEST(currentTerrain, tileResolution, tiffDataDynamic[i]));

                    realTerrainWidth = areaSizeLon * 1000.0f;
                    realTerrainLength = areaSizeLat * 1000.0f;

                    currentTerrain.terrainData.size = RealTerrainSize(realTerrainWidth, realTerrainLength, highestPoint);
                    currentTerrain.Flush();

                    InfiniteTerrain.westTerrains.Remove(currentTerrain.name);
                }
            }
            catch(Exception e)
            {
                UnityEngine.Debug.Log(e);
            }

            yield return 0;
        }

        private static IEnumerator<float> FillHeightsDynamic (Terrain terrainTile, int terrainRes, float[,] terrainHeights)
        {
            concurrentUpdates++;

            int gridCount = (terrainRes - 1) / runTime.cellSize;

            for(int i = 0; i < gridCount; i++)
            {
                for(int j = 0; j < gridCount; j++)
                {
                    try
                    {
                        heightmapCell = new float[runTime.cellSize, runTime.cellSize];
                        int row = i * runTime.cellSize;
                        int col = j * runTime.cellSize;

                        for (int x = 0; x < runTime.cellSize; x++)
                        {
                            Array.Copy
                            (
                                terrainHeights,
                                (x + col) * (terrainRes) + row,
                                heightmapCell,
                                x * runTime.cellSize,
                                runTime.cellSize
                            );
                        }

                        if(runTime.delayedLOD)
                            terrainTile.terrainData.SetHeightsDelayLOD(row, col, heightmapCell);
                        else
                            terrainTile.terrainData.SetHeights(row, col, heightmapCell);
                    }
                    catch{}

                    yield return Timing.WaitForSeconds(runTime.elevationDelay);
                }
            }

            try
            {
                // Fill Top Row
                heightmapCell = new float[1, terrainRes];

                for(int x = 0; x < terrainRes; x++)
                    heightmapCell[0, x] = terrainHeights[terrainRes - 1, x];

                if(runTime.delayedLOD)
                    terrainTile.terrainData.SetHeightsDelayLOD(0, terrainRes - 1, heightmapCell);
                else
                    terrainTile.terrainData.SetHeights(0, terrainRes - 1, heightmapCell);

                // Fill Right Column
                heightmapCell = new float[terrainRes, 1];

                for(int x = 0; x < terrainRes; x++)
                    heightmapCell[x, 0] = terrainHeights[x, terrainRes - 1];

                if(runTime.delayedLOD)
                    terrainTile.terrainData.SetHeightsDelayLOD(terrainRes - 1, 0, heightmapCell);
                else
                    terrainTile.terrainData.SetHeights(terrainRes - 1, 0, heightmapCell);

                if(runTime.delayedLOD)
                    terrainTile.ApplyDelayedHeightmapModification();

                if(runTime.showTileOnFinish)
                    terrainTile.drawHeightmap = true;
            }
            catch{}

            generatedTerrainsCount++;
            concurrentUpdates--;

            if(generatedTerrainsCount < terrainChunks)
            {
                if(!runTime.progressiveGeneration)
                {
                    if(generatedTerrainsCount % runTime.concurrentTasks == 0)
                    {
                        taskIndex += runTime.concurrentTasks;

                        if (runTime.IsCustomGeoServer)
                        {
                            if(geoDataExtensionElevation.Equals("raw"))
                                Timing.RunCoroutine(LoadTerrainHeightsFromRAW());
                            else if(geoDataExtensionElevation.Equals("tif"))
                                Timing.RunCoroutine(LoadTerrainHeightsFromTIFFDynamic(0));
                            else if(geoDataExtensionElevation.Equals("asc"))
                                Timing.RunCoroutine(LoadTerrainHeightsFromASCII());
                        }
                        else
                            Timing.RunCoroutine(LoadTerrainHeightsFromTIFFDynamic(0));
                    }
                }
            }
            else
            {
                if(splittedTerrains)
                {
                    if(runTime.stitchTerrainTiles)
                        //Timing.RunCoroutine(StitchTerrain(croppedTerrains, runTime.elevationDelay, terrainChunks));
                        Timing.RunCoroutine(StitchTerrain(croppedTerrains, 0, terrainChunks));
                    else
                    {
//                        if(runTime.showTileOnFinish)
//                        {
//                            foreach(Terrain t in croppedTerrains)
//                                t.drawHeightmap = true;
//                        }

                        if(!terrainsAreGenerated)
                        {
                            terrainsAreGenerated = true;
                            UnityEngine.Debug.Log("Terrains Are Generated");

                            if(runTime.elevationOnly)
                            {
                                if(runTime.farTerrain)
                                {
                                    if(farTerrainIsGenerated)
                                        Timing.RunCoroutine(WorldIsGenerated());
                                }
                                else
                                    Timing.RunCoroutine(WorldIsGenerated());
                            }
                            else
                            {
                                if(imagesAreGenerated)
                                {
                                    if(runTime.farTerrain)
                                    {
                                        if(farTerrainIsGenerated)
                                            Timing.RunCoroutine(WorldIsGenerated());
                                    }
                                    else
                                        Timing.RunCoroutine(WorldIsGenerated());
                                }
                            }
                        }
                    }
                }
            }
        }

        private static IEnumerator<float> FillHeightsDynamicFAR (Terrain terrainTile, int terrainRes, float[,] terrainHeights)
        {
            int gridCount = (terrainRes - 1) / runTime.farTerrainCellSize;
            int xyStart = Mathf.FloorToInt(gridCount / runTime.areaSizeFarMultiplier / 2f);
            int center = (gridCount / 2) - 1;

            for(int i = 0; i < gridCount; i++)
            {
                for(int j = 0; j < gridCount; j++)
                {
                    if(downloadedFarTerrains >= 2)
                    {
                        // Only load terrain parts in view if there are too many tiles
                        if(i <= center - xyStart || i > center + xyStart || j <= center - xyStart || j > center + xyStart)
                        {
                            try
                            {
                                heightmapCellFar = new float[runTime.farTerrainCellSize, runTime.farTerrainCellSize];
                                int row = i * runTime.farTerrainCellSize;
                                int col = j * runTime.farTerrainCellSize;

                                for (int x = 0; x < runTime.farTerrainCellSize; x++)
                                {
                                    Array.Copy
                                    (
                                        terrainHeights,
                                        (x + col) * (terrainRes) + row,
                                        heightmapCellFar,
                                        x * runTime.farTerrainCellSize,
                                        runTime.farTerrainCellSize
                                    );
                                }

                                if(runTime.delayedLOD)
                                    terrainTile.terrainData.SetHeightsDelayLOD(row, col, heightmapCellFar);
                                else
                                    terrainTile.terrainData.SetHeights(row, col, heightmapCellFar);
                            }
                            catch{}

                            yield return Timing.WaitForSeconds(runTime.elevationDelay);
                        }
                    }
                    else
                    {
                        try
                        {
                            heightmapCellFar = new float[runTime.farTerrainCellSize, runTime.farTerrainCellSize];
                            int row = i * runTime.farTerrainCellSize;
                            int col = j * runTime.farTerrainCellSize;

                            for (int x = 0; x < runTime.farTerrainCellSize; x++)
                            {
                                Array.Copy
                                (
                                    terrainHeights,
                                    (x + col) * (terrainRes) + row,
                                    heightmapCellFar,
                                    x * runTime.farTerrainCellSize,
                                    runTime.farTerrainCellSize
                                );
                            }

                            if(runTime.delayedLOD)
                                terrainTile.terrainData.SetHeightsDelayLOD(row, col, heightmapCellFar);
                            else
                                terrainTile.terrainData.SetHeights(row, col, heightmapCellFar);
                        }
                        catch{}

                        yield return Timing.WaitForSeconds(runTime.elevationDelay);
                    }
                }
            }

            try
            {
                // Fill Top Row
                heightmapCellFar = new float[1, terrainRes];

                for(int x = 0; x < terrainRes; x++)
                    heightmapCellFar[0, x] = terrainHeights[terrainRes - 1, x];

                if(runTime.delayedLOD)
                    terrainTile.terrainData.SetHeightsDelayLOD(0, terrainRes - 1, heightmapCellFar);
                else
                    terrainTile.terrainData.SetHeights(0, terrainRes - 1, heightmapCellFar);

                // Fill Right Column
                heightmapCellFar = new float[terrainRes, 1];

                for(int x = 0; x < terrainRes; x++)
                    heightmapCellFar[x, 0] = terrainHeights[x, terrainRes - 1];

                if(runTime.delayedLOD)
                    terrainTile.terrainData.SetHeightsDelayLOD(terrainRes - 1, 0, heightmapCellFar);
                else
                    terrainTile.terrainData.SetHeights(terrainRes - 1, 0, heightmapCellFar);

                if(runTime.delayedLOD)
                    terrainTile.ApplyDelayedHeightmapModification();

                if(downloadedFarTerrains == 1)
                {
                    terrainTile.drawHeightmap = true;

                    if(!farTerrainIsGenerated)
                    {
                        farTerrainIsGenerated = true;
                        UnityEngine.Debug.Log("Far Terrain Is Generated");

                        if(runTime.elevationOnly && terrainsAreGenerated)
                            Timing.RunCoroutine(WorldIsGenerated());
                        else if(!runTime.elevationOnly && imagesAreGenerated && terrainsAreGenerated)
                            Timing.RunCoroutine(WorldIsGenerated());
                    }
                }
                else
                    SwitchFarTerrainsCompleted();
            }
            catch{}
        }

        private static IEnumerator<float> FillHeightsNORTH (Terrain terrainTile, int terrainRes, float[,] terrainHeights)
        {
            generationIsBusyNORTH = true;
            terrainsInProgress.Add(terrainTile);

            int gridCount = (terrainRes - 1) / runTime.cellSize;

            for(int i = 0; i < gridCount; i++)
            {
                for(int j = 0; j < gridCount; j++)
                {
                    try
                    {
                        heightmapCell = new float[runTime.cellSize, runTime.cellSize];
                        int row = i * runTime.cellSize;
                        int col = j * runTime.cellSize;

                        for (int x = 0; x < runTime.cellSize; x++)
                        {
                            Array.Copy
                            (
                                terrainHeights,
                                (x + col) * (terrainRes) + row,
                                heightmapCell,
                                x * runTime.cellSize,
                                runTime.cellSize
                            );
                        }

                        if(runTime.delayedLOD)
                            terrainTile.terrainData.SetHeightsDelayLOD(row, col, heightmapCell);
                        else
                            terrainTile.terrainData.SetHeights(row, col, heightmapCell);
                    }
                    catch{}

                    yield return Timing.WaitForSeconds(runTime.elevationDelay);
                }
            }

            try
            {
                // Fill Top Row
                heightmapCell = new float[1, terrainRes];

                for(int x = 0; x < terrainRes; x++)
                    heightmapCell[0, x] = terrainHeights[terrainRes - 1, x];

                if(runTime.delayedLOD)
                    terrainTile.terrainData.SetHeightsDelayLOD(0, terrainRes - 1, heightmapCell);
                else
                    terrainTile.terrainData.SetHeights(0, terrainRes - 1, heightmapCell);

                // Fill Right Column
                heightmapCell = new float[terrainRes, 1];

                for(int x = 0; x < terrainRes; x++)
                    heightmapCell[x, 0] = terrainHeights[x, terrainRes - 1];

                if(runTime.delayedLOD)
                    terrainTile.terrainData.SetHeightsDelayLOD(terrainRes - 1, 0, heightmapCell);
                else
                    terrainTile.terrainData.SetHeights(terrainRes - 1, 0, heightmapCell);

                if(runTime.delayedLOD)
                    terrainTile.ApplyDelayedHeightmapModification();

                terrainsInProgress.Remove(terrainTile);
                terrainTile.drawHeightmap = true;

                northCounterGenerated++;

                if(runTime.stitchTerrainTiles && northCounterGenerated % gridSizeTerrain == 0)
                {
                    generationIsBusyNORTH = false;
                    Timing.RunCoroutine(WaitAndStitchNORTH());
                }
                else
                    ManageNeighborings();
            }
            catch{}
        }

        private static IEnumerator<float> FillHeightsNORTH2 ()
        {
            //Timing.RunCoroutine(WaitAndFillNORTH(currentTerrain, tileResolution, tiffDataDynamic[index]));

            for(int z = 0; z < InfiniteTerrain.northIndexes.Count; z++)
            {
                //yield return Timing.WaitForSeconds(2);

                int index = InfiniteTerrain.northIndexes[z];
                Terrain currentTerrain = splittedTerrains.transform.GetChild(index).GetComponent<Terrain>();


                float[,] terrainHeights = tiffDataDynamic[index];
                //float[,] terrainHeights = TiffDataDynamic(fileNameTerrainDataDynamic[index], index);


                int gridCount = (tileResolution - 1) / runTime.cellSize;

                print(index);

                generationIsBusyNORTH = true;
                terrainsInProgress.Add(currentTerrain);
                currentTerrain.terrainData.heightmapResolution = tileResolution;
                currentTerrain.drawHeightmap = false;

                yield return Timing.WaitForSeconds(runTime.elevationDelay);

                for(int i = 0; i < gridCount; i++)
                {
                    for(int j = 0; j < gridCount; j++)
                    {
                        try
                        {
                            heightmapCell = new float[runTime.cellSize, runTime.cellSize];
                            int row = i * runTime.cellSize;
                            int col = j * runTime.cellSize;

                            for (int x = 0; x < runTime.cellSize; x++)
                            {
                                Array.Copy
                                (
                                    terrainHeights,
                                    (x + col) * (tileResolution) + row,
                                    heightmapCell,
                                    x * runTime.cellSize,
                                    runTime.cellSize
                                );
                            }

                            if(runTime.delayedLOD)
                                currentTerrain.terrainData.SetHeightsDelayLOD(row, col, heightmapCell);
                            else
                                currentTerrain.terrainData.SetHeights(row, col, heightmapCell);
                        }
                        catch{}
                    }
                }

                try
                {
                    // Fill Top Row
                    heightmapCell = new float[1, tileResolution];

                    for(int x = 0; x < tileResolution; x++)
                        heightmapCell[0, x] = terrainHeights[tileResolution - 1, x];

                    if(runTime.delayedLOD)
                        currentTerrain.terrainData.SetHeightsDelayLOD(0, tileResolution - 1, heightmapCell);
                    else
                        currentTerrain.terrainData.SetHeights(0, tileResolution - 1, heightmapCell);

                    // Fill Right Column
                    heightmapCell = new float[tileResolution, 1];

                    for(int x = 0; x < tileResolution; x++)
                        heightmapCell[x, 0] = terrainHeights[x, tileResolution - 1];

                    if(runTime.delayedLOD)
                        currentTerrain.terrainData.SetHeightsDelayLOD(tileResolution - 1, 0, heightmapCell);
                    else
                        currentTerrain.terrainData.SetHeights(tileResolution - 1, 0, heightmapCell);

                    if(runTime.delayedLOD)
                        currentTerrain.ApplyDelayedHeightmapModification();

                    terrainsInProgress.Remove(currentTerrain);
                    //currentTerrain.drawHeightmap = true;

                    generationIsBusyNORTH = false;

                    northCounterGenerated++;

                    //if(runTime.stitchTerrainTiles && northCounterGenerated % gridSizeTerrain == 0)
                        //Timing.RunCoroutine(WaitAndStitchNORTH());



                    realTerrainWidth = areaSizeLon * 1000.0f;
                    realTerrainLength = areaSizeLat * 1000.0f;

                    currentTerrain.terrainData.size = RealTerrainSize(realTerrainWidth, realTerrainLength, highestPoint);
                    currentTerrain.Flush();




                    //InfiniteTerrain.northIndexes.Remove(z);

                    if(z == InfiniteTerrain.northIndexes.Count - 1)
                        InfiniteTerrain.northIndexes.Clear();




                    if(runTime.stitchTerrainTiles && northCounterGenerated % gridSizeTerrain == 0)
                        Timing.RunCoroutine(WaitAndStitchNORTH());
                }
                catch{}
            }

            //yield return 0;
        }

        private static IEnumerator<float> FillHeightsSOUTH (Terrain terrainTile, int terrainRes, float[,] terrainHeights)
        {
            generationIsBusySOUTH = true;
            terrainsInProgress.Add(terrainTile);

            int gridCount = (terrainRes - 1) / runTime.cellSize;

            for(int i = 0; i < gridCount; i++)
            {
                for(int j = 0; j < gridCount; j++)
                {
                    try
                    {
                        heightmapCell = new float[runTime.cellSize, runTime.cellSize];
                        int row = i * runTime.cellSize;
                        int col = j * runTime.cellSize;

                        for (int x = 0; x < runTime.cellSize; x++)
                        {
                            Array.Copy
                            (
                                terrainHeights,
                                (x + col) * (terrainRes) + row,
                                heightmapCell,
                                x * runTime.cellSize,
                                runTime.cellSize
                            );
                        }

                        if(runTime.delayedLOD)
                            terrainTile.terrainData.SetHeightsDelayLOD(row, col, heightmapCell);
                        else
                            terrainTile.terrainData.SetHeights(row, col, heightmapCell);
                    }
                    catch{}

                    yield return Timing.WaitForSeconds(runTime.elevationDelay);
                }
            }

            try
            {
                // Fill Top Row
                heightmapCell = new float[1, terrainRes];

                for(int x = 0; x < terrainRes; x++)
                    heightmapCell[0, x] = terrainHeights[terrainRes - 1, x];

                if(runTime.delayedLOD)
                    terrainTile.terrainData.SetHeightsDelayLOD(0, terrainRes - 1, heightmapCell);
                else
                    terrainTile.terrainData.SetHeights(0, terrainRes - 1, heightmapCell);

                // Fill Right Column
                heightmapCell = new float[terrainRes, 1];

                for(int x = 0; x < terrainRes; x++)
                    heightmapCell[x, 0] = terrainHeights[x, terrainRes - 1];

                if(runTime.delayedLOD)
                    terrainTile.terrainData.SetHeightsDelayLOD(terrainRes - 1, 0, heightmapCell);
                else
                    terrainTile.terrainData.SetHeights(terrainRes - 1, 0, heightmapCell);

                if(runTime.delayedLOD)
                    terrainTile.ApplyDelayedHeightmapModification();

                terrainsInProgress.Remove(terrainTile);
                terrainTile.drawHeightmap = true;

                southCounterGenerated++;

                if(runTime.stitchTerrainTiles && southCounterGenerated % gridSizeTerrain == 0)
                {
                    generationIsBusySOUTH = false;
                    Timing.RunCoroutine(WaitAndStitchSOUTH());
                }
                else
                    ManageNeighborings();
            }
            catch{}
        }

        private static IEnumerator<float> FillHeightsEAST (Terrain terrainTile, int terrainRes, float[,] terrainHeights)
        {
            generationIsBusyEAST = true;
            terrainsInProgress.Add(terrainTile);

            int gridCount = (terrainRes - 1) / runTime.cellSize;

            for(int i = 0; i < gridCount; i++)
            {
                for(int j = 0; j < gridCount; j++)
                {
                    try
                    {
                        heightmapCell = new float[runTime.cellSize, runTime.cellSize];
                        int row = i * runTime.cellSize;
                        int col = j * runTime.cellSize;

                        for (int x = 0; x < runTime.cellSize; x++)
                        {
                            Array.Copy
                            (
                                terrainHeights,
                                (x + col) * (terrainRes) + row,
                                heightmapCell,
                                x * runTime.cellSize,
                                runTime.cellSize
                            );
                        }

                        if(runTime.delayedLOD)
                            terrainTile.terrainData.SetHeightsDelayLOD(row, col, heightmapCell);
                        else
                            terrainTile.terrainData.SetHeights(row, col, heightmapCell);
                    }
                    catch{}

                    yield return Timing.WaitForSeconds(runTime.elevationDelay);
                }
            }

            try
            {
                // Fill Top Row
                heightmapCell = new float[1, terrainRes];

                for(int x = 0; x < terrainRes; x++)
                    heightmapCell[0, x] = terrainHeights[terrainRes - 1, x];

                if(runTime.delayedLOD)
                    terrainTile.terrainData.SetHeightsDelayLOD(0, terrainRes - 1, heightmapCell);
                else
                    terrainTile.terrainData.SetHeights(0, terrainRes - 1, heightmapCell);

                // Fill Right Column
                heightmapCell = new float[terrainRes, 1];

                for(int x = 0; x < terrainRes; x++)
                    heightmapCell[x, 0] = terrainHeights[x, terrainRes - 1];

                if(runTime.delayedLOD)
                    terrainTile.terrainData.SetHeightsDelayLOD(terrainRes - 1, 0, heightmapCell);
                else
                    terrainTile.terrainData.SetHeights(terrainRes - 1, 0, heightmapCell);

                if(runTime.delayedLOD)
                    terrainTile.ApplyDelayedHeightmapModification();

                terrainsInProgress.Remove(terrainTile);
                terrainTile.drawHeightmap = true;

                eastCounterGenerated++;

                if(runTime.stitchTerrainTiles && eastCounterGenerated % gridSizeTerrain == 0)
                {
                    generationIsBusyEAST = false;
                    Timing.RunCoroutine(WaitAndStitchEAST());
                }
                else
                    ManageNeighborings();
            }
            catch{}
        }

        private static IEnumerator<float> FillHeightsWEST (Terrain terrainTile, int terrainRes, float[,] terrainHeights)
        {
            generationIsBusyWEST = true;
            terrainsInProgress.Add(terrainTile);

            int gridCount = (terrainRes - 1) / runTime.cellSize;

            for(int i = 0; i < gridCount; i++)
            {
                for(int j = 0; j < gridCount; j++)
                {
                    try
                    {
                        heightmapCell = new float[runTime.cellSize, runTime.cellSize];
                        int row = i * runTime.cellSize;
                        int col = j * runTime.cellSize;

                        for (int x = 0; x < runTime.cellSize; x++)
                        {
                            Array.Copy
                            (
                                terrainHeights,
                                (x + col) * (terrainRes) + row,
                                heightmapCell,
                                x * runTime.cellSize,
                                runTime.cellSize
                            );
                        }

                        if(runTime.delayedLOD)
                            terrainTile.terrainData.SetHeightsDelayLOD(row, col, heightmapCell);
                        else
                            terrainTile.terrainData.SetHeights(row, col, heightmapCell);
                    }
                    catch{}

                    yield return Timing.WaitForSeconds(runTime.elevationDelay);
                }
            }

            try
            {
                // Fill Top Row
                heightmapCell = new float[1, terrainRes];

                for(int x = 0; x < terrainRes; x++)
                    heightmapCell[0, x] = terrainHeights[terrainRes - 1, x];

                if(runTime.delayedLOD)
                    terrainTile.terrainData.SetHeightsDelayLOD(0, terrainRes - 1, heightmapCell);
                else
                    terrainTile.terrainData.SetHeights(0, terrainRes - 1, heightmapCell);

                // Fill Right Column
                heightmapCell = new float[terrainRes, 1];

                for(int x = 0; x < terrainRes; x++)
                    heightmapCell[x, 0] = terrainHeights[x, terrainRes - 1];

                if(runTime.delayedLOD)
                    terrainTile.terrainData.SetHeightsDelayLOD(terrainRes - 1, 0, heightmapCell);
                else
                    terrainTile.terrainData.SetHeights(terrainRes - 1, 0, heightmapCell);

                if(runTime.delayedLOD)
                    terrainTile.ApplyDelayedHeightmapModification();

                terrainsInProgress.Remove(terrainTile);
                terrainTile.drawHeightmap = true;

                westCounterGenerated++;

                if(runTime.stitchTerrainTiles && westCounterGenerated % gridSizeTerrain == 0)
                {
                    generationIsBusyWEST = false;
                    Timing.RunCoroutine(WaitAndStitchWEST());
                }
                else
                    ManageNeighborings();
            }
            catch{}
        }

        private static IEnumerator<float> WaitAndStitchNORTH ()
        {
            if(!stitchingInProgress && !InfiniteTerrain.inProgressWest && !InfiniteTerrain.inProgressEast)
            {
                Timing.RunCoroutine(StitchTerrain(croppedTerrains, runTime.stitchDelay, croppedTerrains.Count));
            }
            else
            {
                if(!InfiniteTerrain.inProgressWest && !InfiniteTerrain.inProgressEast)
                {
                    yield return Timing.WaitForSeconds(1f);
                    Timing.RunCoroutine(WaitAndStitchNORTH());
                }
//                else
//                {
//                    if(InfiniteTerrain.inProgressWest)
//                        skipNorthCell1st = true;
//                    else if(InfiniteTerrain.inProgressEast)
//                        skipNorthCellLast = true;
//                }
            }

            yield return 0;
        }

        private static IEnumerator<float> WaitAndStitchSOUTH ()
        {
            if(!stitchingInProgress && !InfiniteTerrain.inProgressWest && !InfiniteTerrain.inProgressEast)
            {
                Timing.RunCoroutine(StitchTerrain(croppedTerrains, runTime.stitchDelay, croppedTerrains.Count));
            }
            else
            {
                if(!InfiniteTerrain.inProgressWest && !InfiniteTerrain.inProgressEast)
                {
                    yield return Timing.WaitForSeconds(1f);
                    Timing.RunCoroutine(WaitAndStitchSOUTH());
                }
//                else
//                {
//                    if(InfiniteTerrain.inProgressWest)
//                        skipSouthCell1st = true;
//                    else if(InfiniteTerrain.inProgressEast)
//                        skipSouthCellLast = true;
//                }
            }

            yield return 0;
        }

        private static IEnumerator<float> WaitAndStitchEAST ()
        {
            if(!stitchingInProgress && !InfiniteTerrain.inProgressNorth && !InfiniteTerrain.inProgressSouth)
            {
                Timing.RunCoroutine(StitchTerrain(croppedTerrains, runTime.stitchDelay, croppedTerrains.Count));
            }
            else
            {
                if(!InfiniteTerrain.inProgressNorth && !InfiniteTerrain.inProgressSouth)
                {
                    yield return Timing.WaitForSeconds(1f);
                    Timing.RunCoroutine(WaitAndStitchEAST());
                }
//                else
//                {
//                    if(InfiniteTerrain.inProgressNorth)
//                        skipEastCell1st = true;
//                    else if(InfiniteTerrain.inProgressSouth)
//                        skipEastCellLast = true;
//                }  
            }

            yield return 0;
        }
            
        private static IEnumerator<float> WaitAndStitchWEST ()
        {
            if(!stitchingInProgress && !InfiniteTerrain.inProgressNorth && !InfiniteTerrain.inProgressSouth)
            {
                Timing.RunCoroutine(StitchTerrain(croppedTerrains, runTime.stitchDelay, croppedTerrains.Count));
            }
            else
            {
                if(!InfiniteTerrain.inProgressNorth && !InfiniteTerrain.inProgressSouth)
                {
                    yield return Timing.WaitForSeconds(1f);
                    Timing.RunCoroutine(WaitAndStitchWEST());
                }
//                else
//                {
//                    if(InfiniteTerrain.inProgressNorth)
//                        skipWestCell1st = true;
//                    else if(InfiniteTerrain.inProgressSouth)
//                        skipWestCellLast = true;
//                }
            }

            yield return 0;
        }

        private static void StitchNORTH ()
        {
            if(InfiniteTerrain.hybridEast)
            {
                List<Terrain> mixedTerrains = InfiniteTerrain.northTerrainsNeighbor.Union(InfiniteTerrain.eastTerrainsNeighbor).ToList();
                Timing.RunCoroutine(StitchTerrain(mixedTerrains, runTime.stitchDelay, mixedTerrains.Count));
            }
            else if(InfiniteTerrain.hybridWest)
            {
                List<Terrain> mixedTerrains = InfiniteTerrain.northTerrainsNeighbor.Union(InfiniteTerrain.westTerrainsNeighbor).ToList();
                Timing.RunCoroutine(StitchTerrain(mixedTerrains, runTime.stitchDelay, mixedTerrains.Count));
            }
            else
                Timing.RunCoroutine(StitchTerrain(InfiniteTerrain.northTerrainsNeighbor, runTime.stitchDelay, InfiniteTerrain.northTerrainsNeighbor.Count));
        }

        private static void StitchSOUTH ()
        {
            if(InfiniteTerrain.hybridEast)
            {
                List<Terrain> mixedTerrains = InfiniteTerrain.southTerrainsNeighbor.Union(InfiniteTerrain.eastTerrainsNeighbor).ToList();
                Timing.RunCoroutine(StitchTerrain(mixedTerrains, runTime.stitchDelay, mixedTerrains.Count));
            }
            else if(InfiniteTerrain.hybridWest)
            {
                List<Terrain> mixedTerrains = InfiniteTerrain.southTerrainsNeighbor.Union(InfiniteTerrain.westTerrainsNeighbor).ToList();
                Timing.RunCoroutine(StitchTerrain(mixedTerrains, runTime.stitchDelay, mixedTerrains.Count));
            }
            else
                Timing.RunCoroutine(StitchTerrain(InfiniteTerrain.southTerrainsNeighbor, runTime.stitchDelay, InfiniteTerrain.southTerrainsNeighbor.Count));
        }

        private static void StitchEAST ()
        {
            if(InfiniteTerrain.hybridNorth)
            {
                List<Terrain> mixedTerrains = InfiniteTerrain.eastTerrainsNeighbor.Union(InfiniteTerrain.northTerrainsNeighbor).ToList();
                Timing.RunCoroutine(StitchTerrain(mixedTerrains, runTime.stitchDelay, mixedTerrains.Count));
            }
            else if(InfiniteTerrain.hybridSouth)
            {
                List<Terrain> mixedTerrains = InfiniteTerrain.eastTerrainsNeighbor.Union(InfiniteTerrain.southTerrainsNeighbor).ToList();
                Timing.RunCoroutine(StitchTerrain(mixedTerrains, runTime.stitchDelay, mixedTerrains.Count));
            }
            else
                Timing.RunCoroutine(StitchTerrain(InfiniteTerrain.eastTerrainsNeighbor, runTime.stitchDelay, InfiniteTerrain.eastTerrainsNeighbor.Count));
        }

        private static void StitchWEST ()
        {
            if(InfiniteTerrain.hybridNorth)
            {
                List<Terrain> mixedTerrains = InfiniteTerrain.westTerrainsNeighbor.Union(InfiniteTerrain.northTerrainsNeighbor).ToList();
                Timing.RunCoroutine(StitchTerrain(mixedTerrains, runTime.stitchDelay, mixedTerrains.Count));
            }
            else if(InfiniteTerrain.hybridSouth)
            {
                List<Terrain> mixedTerrains = InfiniteTerrain.westTerrainsNeighbor.Union(InfiniteTerrain.southTerrainsNeighbor).ToList();
                Timing.RunCoroutine(StitchTerrain(mixedTerrains, runTime.stitchDelay, mixedTerrains.Count));
            }
            else
                Timing.RunCoroutine(StitchTerrain(InfiniteTerrain.westTerrainsNeighbor, runTime.stitchDelay, InfiniteTerrain.westTerrainsNeighbor.Count));
        }

        private static IEnumerator<float> StitchTerrain (List<Terrain> allTerrains, float delay, int terrainCount)
        {
            ManageNeighborings();

            stitchingInProgress = true;

            stitchedTerrainsCount = 0;
            _terrainDict = new Dictionary<int[], Terrain> (new IntArrayComparer());
            _terrains = allTerrains.ToArray();

            if (_terrains.Length > 0)
            {
                int sizeX = (int)_terrains[0].terrainData.size.x;
                int sizeZ = (int)_terrains[0].terrainData.size.z;

                foreach (Terrain ter in _terrains)
                {
                    try
                    {
                        int[] posTer = new int[]
                        {
                            (int)(Mathf.Round(ter.transform.position.x / sizeX)),
                            (int)(Mathf.Round(ter.transform.position.z / sizeZ))
                        };

                        _terrainDict.Add(posTer, ter);
                    }
                    catch(Exception e)
                    {
                        UnityEngine.Debug.Log(e);
                    }
                }

                //Checks neighbours and stitches them
                foreach (var item in _terrainDict)
                {
                    int[] posTer = item.Key;
                    Terrain topTerrain = null;
                    Terrain leftTerrain = null;
                    Terrain rightTerrain = null;
                    Terrain bottomTerrain = null;

                    _terrainDict.TryGetValue (new int[]
                    {
                        posTer [0],
                        posTer [1] + 1
                    },
                        out topTerrain
                    );

                    _terrainDict.TryGetValue (new int[]
                    {
                        posTer [0] - 1,
                        posTer [1]
                    },
                        out leftTerrain
                    );

                    _terrainDict.TryGetValue (new int[]
                    {
                        posTer [0] + 1,
                        posTer [1]
                    },
                        out rightTerrain
                    );

                    _terrainDict.TryGetValue (new int[]
                    {
                        posTer [0],
                        posTer [1] - 1
                    },
                        out bottomTerrain
                    );

                    if (rightTerrain != null) hasRight = true; else hasRight = false;
                    if (topTerrain != null) hasTop = true; else hasTop = false;
                    
                    Timing.RunCoroutine(StitchTerrains(item.Value, rightTerrain, topTerrain, hasRight, hasTop, true, delay, terrainCount));

                    yield return Timing.WaitForSeconds(delay);
                }
            }

            yield return 0;
        }

        private static IEnumerator<float> StitchTerrains (Terrain ter, Terrain rightTerrain, Terrain topTerrain, bool hasRight, bool hasTop, bool smooth, float delay, int terrainCount)
        {
            int YLength = tileResolution - checkLength;

            if (hasRight)
            {
                int y = checkLength - 1;

                heights = ter.terrainData.GetHeights (YLength, 0, checkLength, ter.terrainData.heightmapResolution);
                secondHeights = rightTerrain.terrainData.GetHeights (0, 0, checkLength, rightTerrain.terrainData.heightmapResolution);

                for (int x = 0; x < tileResolution; x++)
                {
                    heights [x, y] = Average (heights [x, y], secondHeights [x, 0]);

                    if (smooth)
                        heights [x, y] += Mathf.Abs (heights [x, y - 1] - secondHeights [x, 1]) / runTime.levelSmooth;

                    secondHeights [x, 0] = heights [x, y];

                    for (int i = 1; i < checkLength; i++)
                    {
                        heights [x, y - i] = (Average (heights [x, y - i], heights [x, y - i + 1]) + Mathf.Abs (heights [x, y - i] - heights [x, y - i + 1]) / runTime.levelSmooth) * (checkLength - i) / checkLength + heights [x, y - i] * i / checkLength;
                        secondHeights [x, i] = (Average (secondHeights [x, i], secondHeights [x, i - 1]) + Mathf.Abs (secondHeights [x, i] - secondHeights [x, i - 1]) / runTime.levelSmooth) * (checkLength - i) / checkLength + secondHeights [x, i] * i / checkLength;
                    }
                }

                // Right Columns
                ter.terrainData.SetHeights(YLength, 0, heights);
                ter.Flush();

                // Left Columns
                rightTerrain.terrainData.SetHeights(0, 0, secondHeights);
                rightTerrain.Flush();
            }

            if (hasTop)
            {
                int x = checkLength - 1;

                heights = ter.terrainData.GetHeights (0, YLength, ter.terrainData.heightmapResolution, checkLength);
                secondHeights = topTerrain.terrainData.GetHeights (0, 0, topTerrain.terrainData.heightmapResolution, checkLength);

                for (int y = 0; y < tileResolution; y++)
                {
                    heights [x, y] = Average (heights [x, y], secondHeights [0, y]);

                    if (smooth)
                        heights [x, y] += Mathf.Abs (heights [x - 1, y] - secondHeights [1, y]) / runTime.levelSmooth;

                    secondHeights [0, y] = heights [x, y];

                    for (int i = 1; i < checkLength; i++)
                    {
                        heights [x - i, y] = (Average (heights [x - i, y], heights [x - i + 1, y]) + Mathf.Abs (heights [x - i, y] - heights [x - i + 1, y]) / runTime.levelSmooth) * (checkLength - i) / checkLength + heights [x - i, y] * i / checkLength;
                        secondHeights [i, y] = (Average (secondHeights [i, y], secondHeights [i - 1, y]) + Mathf.Abs (secondHeights [i, y] - secondHeights [i - 1, y]) / runTime.levelSmooth) * (checkLength - i) / checkLength + secondHeights [i, y] * i / checkLength;
                    }
                }

                // Top Rows
                ter.terrainData.SetHeights(0, YLength, heights);
                ter.Flush();

                // Bottom Rows
                topTerrain.terrainData.SetHeights(0, 0, secondHeights);
                topTerrain.Flush();
            }

            stitchedTerrainsCount++;

            if(stitchedTerrainsCount == terrainCount)
            {
                InfiniteTerrain.hybridNorth = false;
                InfiniteTerrain.hybridSouth = false;
                InfiniteTerrain.hybridEast = false;
                InfiniteTerrain.hybridWest = false;
            }

            yield return 0;
        }

        private static IEnumerator<float> RepairCorners (float delay)
        {
            foreach (var item in _terrainDict)
            {
                int[] posTer = item.Key;
                //Terrain topTerrain = null;
                //Terrain leftTerrain = null;
                Terrain rightTerrain = null;
                Terrain bottomTerrain = null;

//                _terrainDict.TryGetValue (new int[]
//                {
//                    posTer [0],
//                    posTer [1] + 1
//                },
//                    out topTerrain
//                );
//
//                _terrainDict.TryGetValue (new int[]
//                {
//                    posTer [0] - 1,
//                    posTer [1]
//                },
//                    out leftTerrain
//                );

                _terrainDict.TryGetValue (new int[]
                {
                    posTer [0] + 1,
                    posTer [1]
                },
                    out rightTerrain
                );

                _terrainDict.TryGetValue (new int[]
                {
                    posTer [0],
                    posTer [1] - 1
                },
                    out bottomTerrain
                );

                if (rightTerrain != null && bottomTerrain != null)
                {
                    Terrain rightBottom = null;

                    _terrainDict.TryGetValue (new int[]
                    {
                        posTer [0] + 1,
                        posTer [1] - 1
                    },
                        out rightBottom
                    );

                    if (rightBottom != null)
                        Timing.RunCoroutine(StitchTerrainsRepair(item.Value, rightTerrain, bottomTerrain, rightBottom, delay));
                }
            }

            yield return 0;
        }

        private static IEnumerator<float> StitchTerrainsRepair (Terrain terrain11, Terrain terrain21, Terrain terrain12, Terrain terrain22, float delay)
        {
            yield return Timing.WaitForSeconds(delay);

            try
            {
                int size = terrain11.terrainData.heightmapResolution - 1;
                List<float> heights = new List<float>();

                heights.Add (terrain11.terrainData.GetHeights (size, 0, 1, 1) [0, 0]);
                heights.Add (terrain21.terrainData.GetHeights (0, 0, 1, 1) [0, 0]);
                heights.Add (terrain12.terrainData.GetHeights (size, size, 1, 1) [0, 0]);
                heights.Add (terrain22.terrainData.GetHeights (0, size, 1, 1) [0, 0]);

                float[,] height = new float[1, 1];
                height [0, 0] = heights.Max();

                terrain11.terrainData.SetHeights (size, 0, height);
                terrain21.terrainData.SetHeights (0, 0, height);
                terrain12.terrainData.SetHeights (size, size, height);
                terrain22.terrainData.SetHeights (0, size, height);

                terrain11.Flush();
                terrain12.Flush();
                terrain21.Flush();
                terrain22.Flush();

                ManageNeighborings();
            }
            catch{}
        }

        static float Average (float first, float second)
        {
            return Mathf.Pow ((Mathf.Pow (first, runTime.power) + Mathf.Pow (second, runTime.power)) / 2.0f, 1 / runTime.power);
        }

        private static IEnumerator<float> FillHeights (Terrain terrainTile, int terrainRes, float[,] terrainHeights)
        {
            if(splittedTerrains)
                yield return Timing.WaitForSeconds(runTime.elevationDelay);

            int gridCount = (terrainRes - 1) / runTime.cellSize;

            for(int i = 0; i < gridCount; i++)
            {
                for(int j = 0; j < gridCount; j++)
                {
                    try
                    {
                        heightmapCell = new float[runTime.cellSize, runTime.cellSize];
                        int row = i * runTime.cellSize;
                        int col = j * runTime.cellSize;

                        for (int x = 0; x < runTime.cellSize; x++)
                        {
                            Array.Copy
                            (
                                terrainHeights,
                                (x + col) * (terrainRes) + row,
                                heightmapCell,
                                x * runTime.cellSize,
                                runTime.cellSize
                            );
                        }

                        if(runTime.delayedLOD)
                            terrainTile.terrainData.SetHeightsDelayLOD(row, col, heightmapCell);
                        else
                            terrainTile.terrainData.SetHeights(row, col, heightmapCell);
                    }
                    catch{}

                    yield return Timing.WaitForSeconds(runTime.elevationDelay);
                }
            }

            try
            {
                // Fill Top Row
                heightmapCell = new float[1, terrainRes];

                for(int x = 0; x < terrainRes; x++)
                    heightmapCell[0, x] = terrainHeights[terrainRes - 1, x];

                if(runTime.delayedLOD)
                    terrainTile.terrainData.SetHeightsDelayLOD(0, terrainRes - 1, heightmapCell);
                else
                    terrainTile.terrainData.SetHeights(0, terrainRes - 1, heightmapCell);

                // Fill Right Column
                heightmapCell = new float[terrainRes, 1];

                for(int x = 0; x < terrainRes; x++)
                    heightmapCell[x, 0] = terrainHeights[x, terrainRes - 1];

                if(runTime.delayedLOD)
                    terrainTile.terrainData.SetHeightsDelayLOD(terrainRes - 1, 0, heightmapCell);
                else
                    terrainTile.terrainData.SetHeights(terrainRes - 1, 0, heightmapCell);

                if(runTime.delayedLOD)
                    terrainTile.ApplyDelayedHeightmapModification();

                if(runTime.showTileOnFinish)
                    terrainTile.drawHeightmap = true;
            }
            catch{}

            generatedTerrainsCount++;

            if(generatedTerrainsCount < terrainChunks)
            {
                if(generatedTerrainsCount % runTime.concurrentTasks == 0)
                {
                    taskIndex += runTime.concurrentTasks;

                    if (runTime.IsCustomGeoServer)
                    {
                        if(geoDataExtensionElevation.Equals("raw"))
                            Timing.RunCoroutine(LoadTerrainHeightsFromRAW());
                        else if(geoDataExtensionElevation.Equals("tif"))
                            Timing.RunCoroutine(LoadTerrainHeightsFromTIFF());
                        else if(geoDataExtensionElevation.Equals("asc"))
                            Timing.RunCoroutine(LoadTerrainHeightsFromASCII());
                    }
                    else
                        Timing.RunCoroutine(LoadTerrainHeightsFromTIFF());
                }
            }
            else
            {
                if(splittedTerrains)
                    ManageNeighborings();
            }
        }

        public static void ManageNeighborings ()
        {
            terrainsLong = splitSizeFinal;
            terrainsWide = splitSizeFinal;
            SetTerrainNeighbors();
        }

        private static void SetTerrainNeighbors ()
        {
//            if(runTime.spiralGeneration)
//            {
//                terrainNames = LogicalComparer(terrainNames);
//
//                for(int i = 0; i < terrainNames.Length; i++)
//                {
//                    splittedTerrains.transform.FindChild(terrainNames[i]).transform.parent = null;
//                    GameObject.Find(terrainNames[i]).transform.parent = splittedTerrains.transform;
//                }
//            }

//            if(splittedTerrains)
//            {
//                GetTerrainList();
//                Timing.RunCoroutine(PerformNeighboring(stitchingTerrainsList));
//            }
//            else if(terrain)
//                terrain.gameObject.AddComponent<TerrainNeighbors>();

            GetTerrainList();
            Timing.RunCoroutine(PerformNeighboring(stitchingTerrainsList));
        }

        private static void GetTerrainList ()
        {
            stitchingTerrainsList = new List<Terrain>();

            for (int x = 0; x < gridSizeTerrain; x++)
                for (int y = 0; y < gridSizeTerrain; y++)
                    stitchingTerrainsList.Add(InfiniteTerrain._grid[y, x]);
        }

        private static IEnumerator<float> PerformNeighboring (List<Terrain> terrains)
        {
            int counter = 0;

            for(int y = 0; y < gridSizeTerrain ; y++)
            {
                for(int x = 0; x < gridSizeTerrain; x++)
                {
                    try
                    {
                        int indexLft = counter - 1;
                        int indexTop = counter - gridSizeTerrain;
                        int indexRgt = counter + 1;
                        int indexBtm = counter + gridSizeTerrain;

                        if(y == 0)
                        {
                            // TopLeft Corner
                            if(x == 0)
                            {
                                //                                if(skipNorthCell1st || skipWestCell1st)
                                //                                {
                                //                                    terrains[counter].drawHeightmap = false;
                                //                                    skipNorthCell1st = false;
                                //                                    skipWestCell1st = false;
                                //                                }
                                //                                else
                                //                                    terrains[counter].drawHeightmap = true;

#if UNITY_2018_3_OR_NEWER
                                terrains[counter].groupingID = 0;
                                terrains[counter].allowAutoConnect = true;
#else
                                terrains[counter].SetNeighbors(null, null, terrains[indexRgt], terrains[indexBtm]);
#endif
                            }
                            // TopRight Corner
                            else if(x == gridSizeTerrain - 1)
                            {
//                                if(skipNorthCellLast || skipEastCell1st)
//                                {
//                                    terrains[counter].drawHeightmap = false;
//                                    skipNorthCellLast = false;
//                                    skipEastCell1st = false;
//                                }
//                                else
//                                    terrains[counter].drawHeightmap = true;
#if UNITY_2018_3_OR_NEWER
                                terrains[counter].groupingID = 0;
                                terrains[counter].allowAutoConnect = true;
#else
                                terrains[counter].SetNeighbors(terrains[indexLft], null, null, terrains[indexBtm]);
#endif
                            }
                            else
                            {
                                //terrains[counter].drawHeightmap = true;
#if UNITY_2018_3_OR_NEWER
                                terrains[counter].groupingID = 0;
                                terrains[counter].allowAutoConnect = true;
#else
                                terrains[counter].SetNeighbors(terrains[indexLft], null, terrains[indexRgt], terrains[indexBtm]);
#endif
                            }
                        }
                        else if(y == gridSizeTerrain - 1)
                        {
                            // BottomLeft Corner
                            if(x == 0)
                            {
//                                if(skipSouthCell1st || skipWestCellLast)
//                                {
//                                    terrains[counter].drawHeightmap = false;
//                                    skipSouthCell1st = false;
//                                    skipWestCellLast = false;
//                                }
//                                else
//                                    terrains[counter].drawHeightmap = true;  

#if UNITY_2018_3_OR_NEWER
                                terrains[counter].groupingID = 0;
                                terrains[counter].allowAutoConnect = true;
#else
                                terrains[counter].SetNeighbors(null, terrains[indexTop], terrains[indexRgt], null);
#endif
                            }
                            // BottomRight Corner
                            else if(x == gridSizeTerrain - 1)
                            {
//                                if(skipSouthCellLast || skipEastCellLast)
//                                {
//                                    terrains[counter].drawHeightmap = false;
//                                    skipSouthCellLast = false;
//                                    skipEastCellLast = false;
//                                }
//                                else
//                                    terrains[counter].drawHeightmap = true;                       

#if UNITY_2018_3_OR_NEWER
                                terrains[counter].groupingID = 0;
                                terrains[counter].allowAutoConnect = true;
#else
                                terrains[counter].SetNeighbors(terrains[indexLft], terrains[indexTop], null, null);
#endif
                            }
                            else
                            {
                                //terrains[counter].drawHeightmap = true;
#if UNITY_2018_3_OR_NEWER
                                terrains[counter].groupingID = 0;
                                terrains[counter].allowAutoConnect = true;
#else
                                terrains[counter].SetNeighbors(terrains[indexLft], terrains[indexTop], terrains[indexRgt], null);
#endif
                            }
                        }
                        else
                        {
                            //terrains[counter].drawHeightmap = true;

#if UNITY_2018_3_OR_NEWER
                            terrains[counter].groupingID = 0;
                            terrains[counter].allowAutoConnect = true;
#else
                            if(x == 0)
                                terrains[counter].SetNeighbors(null, terrains[indexTop], terrains[indexRgt], terrains[indexBtm]);
                            else if(x == gridSizeTerrain - 1)
                                terrains[counter].SetNeighbors(terrains[indexLft], terrains[indexTop], null, terrains[indexBtm]);
                            else
                                terrains[counter].SetNeighbors(terrains[indexLft], terrains[indexTop], terrains[indexRgt], terrains[indexBtm]);
#endif
                        }

                        terrains[counter].Flush();
                    }
                    catch{}

                    counter++;
                }
            }

            stitchingInProgress = false;

            CheckInitialization();

            yield return 0;
        }

        private static void CheckInitialization ()
        {
            if(!terrainsAreGenerated)
            {
                terrainsAreGenerated = true;
                UnityEngine.Debug.Log("Terrains Are Generated");

                if(runTime.elevationOnly)
                {
                    if(runTime.farTerrain)
                    {
                        if(farTerrainIsGenerated)
                            Timing.RunCoroutine(WorldIsGenerated());
                    }
                    else
                        Timing.RunCoroutine(WorldIsGenerated());
                }
                else
                {
                    if(imagesAreGenerated)
                    {
                        if(runTime.farTerrain)
                        {
                            if(farTerrainIsGenerated)
                                Timing.RunCoroutine(WorldIsGenerated());
                        }
                        else
                            Timing.RunCoroutine(WorldIsGenerated());
                    }
                }
            }
        }

        public static void CalculateResampleHeightmaps ()
        {
            // Set chunk resolutions to a "Previous Power of 2" value
            if(splittedTerrains)
            {
                if(!Mathf.IsPowerOfTwo(croppedTerrains.Count))
                {
                    heightmapResFinalX = ((Mathf.NextPowerOfTwo(runTime.heightmapResolution / splitSizeFinal)) / 2) + 1;
                    heightmapResFinalY = ((Mathf.NextPowerOfTwo(runTime.heightmapResolution / splitSizeFinal)) / 2) + 1;
                    heightmapResFinalXAll = heightmapResFinalX * splitSizeFinal;
                    heightmapResFinalYAll = heightmapResFinalY * splitSizeFinal;

                    ResampleOperation();
                }
                else
                {
                    heightmapResFinalX = heightmapResolutionSplit + 1;
                    heightmapResFinalY = heightmapResolutionSplit + 1;
                    heightmapResFinalXAll = terrainResolutionDownloading;
                    heightmapResFinalYAll = terrainResolutionDownloading;

                    finalHeights = new float[heightmapResFinalXAll, heightmapResFinalYAll];
                    finalHeights = tiffData;
                }
            }
            else if(terrain)
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
                if(!Mathf.IsPowerOfTwo(gridSizeTerrain))
                {
                    heightmapResFinalX = ((Mathf.NextPowerOfTwo(runTime.heightmapResolution / gridSizeTerrain)) / 2) + 1;
                    heightmapResFinalY = ((Mathf.NextPowerOfTwo(runTime.heightmapResolution / gridSizeTerrain)) / 2) + 1;
                    heightmapResFinalXAll = heightmapResFinalX * gridSizeTerrain;
                    heightmapResFinalYAll = heightmapResFinalY * gridSizeTerrain;

                    ResampleOperation();
                }
                else
                {
                    heightmapResFinalX = tileResolution;
                    heightmapResFinalY = tileResolution;
                    heightmapResFinalXAll = terrainResolutionDownloading;
                    heightmapResFinalYAll = terrainResolutionDownloading;

                    finalHeights = new float[heightmapResFinalXAll, heightmapResFinalYAll];
                    finalHeights = tiffData;
                }
            }
        }

        public static void CalculateResampleHeightmapsGeoServer ()
        {
            if(heightmapResXAll == Mathf.ClosestPowerOfTwo(heightmapResXAll) + splitSizeFinal)
            {
                heightmapResFinalX = Mathf.ClosestPowerOfTwo(heightmapResX) + 1;
                heightmapResFinalXAll = heightmapResXAll;

                heightmapResFinalY = Mathf.ClosestPowerOfTwo(heightmapResY) + 1;
                heightmapResFinalYAll = heightmapResYAll;

                finalHeights = new float[heightmapResFinalXAll, heightmapResFinalYAll];

                if(geoDataExtensionElevation.Equals("raw"))
                    finalHeights = rawData;
                else if(geoDataExtensionElevation.Equals("tif"))
                    finalHeights = tiffData;
                else if(geoDataExtensionElevation.Equals("asc"))
                    finalHeights = asciiData;
            }
            else
            {
                heightmapResFinalX = Mathf.ClosestPowerOfTwo(heightmapResX) + 1;
                heightmapResFinalXAll = heightmapResFinalX * splitSizeFinal;

                heightmapResFinalY = Mathf.ClosestPowerOfTwo(heightmapResY) + 1;
                heightmapResFinalYAll = heightmapResFinalY * splitSizeFinal;

                ResampleOperation();
            }
        }

        private static Vector3 RealTerrainSize (float width, float length, float height)
        {
            /*
            terrainEverestDiffer = everestPeak / highestPoint;
            realTerrainHeight = ((initialTerrainWidth * splitSizeFinal) * ((height * terrainEverestDiffer) / width)) * runTime.elevationExaggeration;

            if(realTerrainHeight <= 0f ||  float.IsNaN(realTerrainHeight) || float.IsInfinity(realTerrainHeight) || float.IsPositiveInfinity(realTerrainHeight) || float.IsNegativeInfinity(realTerrainHeight))
                realTerrainHeight = 0.001f;
            */

            float realTerrainSizeZ = initialTerrainWidth * terrainSizeFactor;
            Vector3 finalTerrainSize = new Vector3(initialTerrainWidth, realTerrainHeight, realTerrainSizeZ);

            return finalTerrainSize;
        }

        private static Vector3 RealTerrainSizeFAR (float width, float length, float height)
        {
            Vector3 finalTerrainSize = new Vector3(width, realTerrainHeight, length);

            return finalTerrainSize;
        }

        private static void ResampleOperation ()
        {
            float scaleFactorLat = ((float)heightmapResFinalXAll) / ((float)heightmapResXAll);
            float scaleFactorLon = ((float)heightmapResFinalYAll) / ((float)heightmapResYAll);

            finalHeights = new float[heightmapResFinalXAll, heightmapResFinalYAll];

            for (int x = 0; x < heightmapResFinalXAll; x++)
                for (int y = 0; y < heightmapResFinalYAll; y++)
                    finalHeights[x, y] = ResampleHeights((float)x / scaleFactorLat, (float)y / scaleFactorLon);
        }

        private static float ResampleHeights(float X, float Y)
        {
            try
            {
                int X1 = Mathf.RoundToInt((X + heightmapResXAll % heightmapResXAll));
                int Y1 = Mathf.RoundToInt((Y + heightmapResYAll % heightmapResYAll));

                if(runTime.IsCustomGeoServer)
                {
                    if(geoDataExtensionElevation.Equals("raw"))
                        return rawData[X1, Y1];
                    else if(geoDataExtensionElevation.Equals("tif"))
                        return tiffData[X1, Y1];
                    else if(geoDataExtensionElevation.Equals("asc"))
                        return asciiData[X1, Y1];
                }
                else
                    return tiffData[X1, Y1];

                return 0f;
            }
            catch
            {
                return 0f;
            }
        }

        public static IEnumerator<float> LoadTerrainHeightsFromRAW ()
        {
            int counter = 0;
            int currentRow = splitSizeFinal - 1;
            int xLength = heightmapResFinalX;
            int yLength = heightmapResFinalY;
            int xStart = 0;
            int yStart = 0;

            if(splittedTerrains)
            {
                for (int i = 0; i < splitSizeFinal; i++)
                {
                    for (int j = 0; j < splitSizeFinal; j++)
                    {
                        if(counter >= taskIndex - runTime.concurrentTasks && counter < taskIndex)
                        {
                            croppedTerrains[counter].terrainData.heightmapResolution = heightmapResFinalX;
                            float[,] rawDataSplitted = new float[heightmapResFinalX, heightmapResFinalY];

                            if(!runTime.spiralGeneration)
                            {
                                xStart = (currentRow * (heightmapResFinalX - 1));
                                yStart = (j * (heightmapResFinalY - 1));
                            }
                            else
                            {
                                xStart = ((splitSizeFinal - ((int)spiralCell[counter].x  - 1)) - 1) * (heightmapResFinalX - 1);
                                yStart = ((int)spiralCell[counter].y - 1) * (heightmapResFinalY - 1);
                            }

                            try
                            {
                                for(int x = 0; x < xLength; x++)
                                    for(int y = 0; y < yLength; y++)
                                        rawDataSplitted[x, y] = finalHeights[xStart + x, yStart + y];

                                Timing.RunCoroutine(FillHeights(croppedTerrains[counter], heightmapResFinalX, rawDataSplitted));

                                realTerrainWidth = areaSizeLon * 1000.0f / splitSizeFinal;
                                realTerrainLength = areaSizeLat * 1000.0f / splitSizeFinal;

                                croppedTerrains[counter].terrainData.size = RealTerrainSize(realTerrainWidth, realTerrainLength, highestPoint);
                                croppedTerrains[counter].Flush();
                            }
                            catch(Exception e)
                            {
                                UnityEngine.Debug.Log(e);
                            }
                        }

                        counter++;
                    }
                    currentRow--;
                }

                yield return 0;
            }
            else if (terrain)
            {
                terrain.terrainData.heightmapResolution = heightmapResFinalXAll;

                try
                {
                    Timing.RunCoroutine(FillHeights(terrain, heightmapResFinalXAll, finalHeights));

                    realTerrainWidth = areaSizeLon * 1000.0f;
                    realTerrainLength = areaSizeLat * 1000.0f;

                    terrain.terrainData.size = RealTerrainSize(realTerrainWidth, realTerrainLength, highestPoint);
                    terrain.Flush();
                }
                catch(Exception e)
                {
                    UnityEngine.Debug.Log(e);
                }
            }
        }

        public static IEnumerator<float> LoadTerrainHeightsFromASCII ()
        {
            int counter = 0;
            int currentRow = splitSizeFinal - 1;
            int xLength = heightmapResFinalX;
            int yLength = heightmapResFinalY;
            int xStart = 0;
            int yStart = 0;

            if(splittedTerrains)
            {
                for (int i = 0; i < splitSizeFinal; i++)
                {
                    for (int j = 0; j < splitSizeFinal; j++)
                    {
                        if(counter >= taskIndex - runTime.concurrentTasks && counter < taskIndex)
                        {
                            croppedTerrains[counter].terrainData.heightmapResolution = heightmapResFinalX;
                            float[,] asciiDataSplitted = new float[heightmapResFinalX, heightmapResFinalY];

                            if(!runTime.spiralGeneration)
                            {
                                xStart = (currentRow * (heightmapResFinalX - 1));
                                yStart = (j * (heightmapResFinalY - 1));
                            }
                            else
                            {
                                xStart = ((splitSizeFinal - ((int)spiralCell[counter].x  - 1)) - 1) * (heightmapResFinalX - 1);
                                yStart = ((int)spiralCell[counter].y - 1) * (heightmapResFinalY - 1);
                            }

                            try
                            {
                                for(int x = 0; x < xLength; x++)
                                    for(int y = 0; y < yLength; y++)
                                        asciiDataSplitted[x, y] = finalHeights[xStart + x, yStart + y];

                                Timing.RunCoroutine(FillHeights(croppedTerrains[counter], heightmapResFinalX, asciiDataSplitted));

                                realTerrainWidth = areaSizeLon * 1000.0f / splitSizeFinal;
                                realTerrainLength = areaSizeLat * 1000.0f / splitSizeFinal;

                                croppedTerrains[counter].terrainData.size = RealTerrainSize(realTerrainWidth, realTerrainLength, highestPoint);
                                croppedTerrains[counter].Flush();
                            }
                            catch(Exception e)
                            {
                                UnityEngine.Debug.Log(e);
                            }
                        }

                        counter++;
                    }
                    currentRow--;
                }

                yield return 0;
            }
            else if (terrain)
            {
                terrain.terrainData.heightmapResolution = heightmapResFinalXAll;

                try
                {
                    Timing.RunCoroutine(FillHeights(terrain, heightmapResFinalXAll, finalHeights));

                    realTerrainWidth = areaSizeLon * 1000.0f;
                    realTerrainLength = areaSizeLat * 1000.0f;

                    terrain.terrainData.size = RealTerrainSize(realTerrainWidth, realTerrainLength, highestPoint);
                    terrain.Flush();
                }
                catch(Exception e)
                {
                    UnityEngine.Debug.Log(e);
                }
            }
        }

        public static void GetRAWInfo ()
        {
            PickRawDefaults(geoDataPathElevation);

            byte[] buffer;

            using (BinaryReader reader = new BinaryReader(File.Open(geoDataPathElevation, FileMode.Open, FileAccess.Read)))
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
        }

        public static void GetTIFFInfo ()
        {
            try
            {
                using (Tiff inputImage = Tiff.Open(geoDataPathElevation, "r"))
                {
                    tiffWidth = inputImage.GetField(TiffTag.IMAGEWIDTH)[0].ToInt();
                    tiffLength = inputImage.GetField(TiffTag.IMAGELENGTH)[0].ToInt();
                    tiffDataASCII = new float[tiffLength, tiffWidth];

                    int tileHeight = inputImage.GetField(TiffTag.TILELENGTH)[0].ToInt();
                    int tileWidth = inputImage.GetField(TiffTag.TILEWIDTH)[0].ToInt();

                    byte[] buffer = new byte[tileHeight * tileWidth * 4];
                    float[,] fBuffer = new float[tileHeight, tileWidth];

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
                        }
                    }
                }
            }
            catch(Exception e)
            {
                UnityEngine.Debug.Log(e);
            }

            highestPoint = tiffDataASCII.Cast<float>().Max();
            lowestPoint = tiffDataASCII.Cast<float>().Min();
        }

        public static void GetASCIIInfo ()
        {
            StreamReader sr = new StreamReader(geoDataPathElevation, Encoding.ASCII, true);

            //ncols
            string[] line1 = sr.ReadLine().Replace(',', '.').Split(new char[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
            nCols = (Convert.ToInt32(line1[1]));
            //nrows
            string[] line2 = sr.ReadLine().Replace(',', '.').Split(new char[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
            nRows = (Convert.ToInt32(line2[1]));

            //xllcorner
            sr.ReadLine();
            //yllcorner
            sr.ReadLine();
            //cellsize
            sr.ReadLine();
            //nodata
            sr.ReadLine();

            asciiData = new float[nCols, nRows];

            for (int y = 0; y < nRows; y++)
            {
                string[] line = sr.ReadLine().Split(new char[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);

                for (int x = 0; x < nCols; x++)
                {
                    asciiData[(nRows - 1) - y, x] = (float.Parse(line[x].Replace(',', '.'))) / everestPeak;
                }
            }

            sr.Close();

            highestPoint = asciiData.Cast<float>().Max() * everestPeak;
            lowestPoint = asciiData.Cast<float>().Min() * everestPeak;
        }

        public static void GetFolderInfo (string path)
        {
            IEnumerable<string> names = Directory.GetFiles(path, "*.*", SearchOption.AllDirectories)
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

            geoImageNames = names.ToArray();
            geoImageNames = LogicalComparer(geoImageNames);
            totalImagesDataBase = geoImageNames.Length;

            if(terrainChunks > 1)
            {
                multipleTerrainsTiling = true;
                imagesPerTerrain = (int)((float)totalImagesDataBase / (float)terrainChunks);
                tileGrid = (int)(Mathf.Sqrt((float)imagesPerTerrain));
            }
            else
            {
                multipleTerrainsTiling = false;
                tileGrid = (int)(Mathf.Sqrt((float)totalImagesDataBase));
                terrainSizeX = terrainSizeNewX;
                terrainSizeY = terrainSizeNewZ;
            }
        }

        private static void ImageTiler ()
        {
            int counter = 0;
            int tileImages = totalImagesDataBase;

            if(!multipleTerrainsTiling)
            {
                cellSizeX = terrainSizeX / (float)tileGrid;
                cellSizeY = terrainSizeY / (float)tileGrid;

                imageXOffset = new float[tileImages];
                imageYOffset = new float[tileImages];

#if UNITY_2018_3_OR_NEWER
                TerrainLayer[] terrainLayers = new TerrainLayer[tileImages];

                for (int i = 0; i < tileGrid; i++)
                {
                    for (int j = 0; j < tileGrid; j++)
                    {
                        try
                        {
                            imageXOffset[counter] = (terrainSizeX - (cellSizeX * ((float)tileGrid - (float)j))) * -1f;
                            imageYOffset[counter] = (terrainSizeY - cellSizeY - ((float)cellSizeY * (float)i)) * -1f;

                            Texture2D satelliteImage = images[counter];

                            // Texturing Terrain
                            terrainLayers[counter] = new TerrainLayer();
                            terrainLayers[counter].diffuseTexture = satelliteImage;
                            terrainLayers[counter].tileSize = new Vector2(cellSizeX, cellSizeY);
                            terrainLayers[counter].tileOffset = new Vector2(imageXOffset[counter], imageYOffset[counter]);
                            terrainLayers[counter].metallic = 0f;
                            terrainLayers[counter].smoothness = 0f;

                            counter++;
                        }
                        catch (Exception e)
                        {
                            UnityEngine.Debug.LogError(e);
                        }
                    }
                }

                terrain.terrainData.terrainLayers = terrainLayers;
#else
                terrainTextures = new SplatPrototype[tileImages];

                for (int i = 0; i < tileGrid; i++)
                {
                    for (int j = 0; j < tileGrid; j++)
                    {
                        try
                        {
                            imageXOffset[counter] = (terrainSizeX - (cellSizeX * ((float)tileGrid - (float)j))) * -1f;
                            imageYOffset[counter] = (terrainSizeY - cellSizeY - ((float)cellSizeY * (float)i)) * -1f;

                            Texture2D satelliteImage = images[counter];

                            // Texturing Terrain
                            terrainTextures[counter] = new SplatPrototype();
                            terrainTextures[counter].texture = satelliteImage;
                            terrainTextures[counter].tileSize = new Vector2(cellSizeX, cellSizeY);
                            terrainTextures[counter].tileOffset = new Vector2(imageXOffset[counter], imageYOffset[counter]);
                            terrainTextures[counter].metallic = 0f;
                            terrainTextures[counter].smoothness = 0f;

                            counter++;
                        }
                        catch(Exception e)
                        {
                            UnityEngine.Debug.LogError(e);
                        }
                    }
                }

                terrain.terrainData.splatPrototypes = terrainTextures;
#endif

                splatNormalizeX = terrainSizeX / terrain.terrainData.alphamapResolution;
                splatNormalizeY = terrainSizeY / terrain.terrainData.alphamapResolution;

                int lengthz = (int)(cellSizeY / splatNormalizeY);
                int widthz = (int)(cellSizeX / splatNormalizeX);

                for (int i = 0; i < tileImages; i++)
                {
                    try
                    {
                        int lengthzOff = (int)(imageYOffset[i] / splatNormalizeY);
                        int widthzOff = (int)(imageXOffset[i] / splatNormalizeX);

                        smData = new float[lengthz, widthz, terrain.terrainData.alphamapLayers];

                        for(int y = 0; y < lengthz; y++)
                            for(int z = 0; z < widthz; z++)
                                smData[y, z, i] = 1;

                        terrain.terrainData.SetAlphamaps(-widthzOff, -lengthzOff, smData);
                    }
                    catch(Exception e)
                    {
                        UnityEngine.Debug.LogError(e);
                    }
                }

                terrain.terrainData.RefreshPrototypes();
                terrain.Flush();

                terrainTextures = null;
                smData = null;
            }
            else
            {
                int index = 0;
                float terrainSizeSplittedX = croppedTerrains[0].terrainData.size.x;
                float terrainSizeSplittedY = croppedTerrains[0].terrainData.size.z;

                float cellSizeSplittedX = terrainSizeSplittedX / (float)tileGrid;
                float cellSizeSplittedY = terrainSizeSplittedY / (float)tileGrid;

                imageXOffset = new float[imagesPerTerrain];
                imageYOffset = new float[imagesPerTerrain];

                for (int i = 0; i < tileGrid; i++)
                {
                    for (int j = 0; j < tileGrid; j++)
                    {
                        imageXOffset[index] = (terrainSizeSplittedX - (cellSizeSplittedX * ((float)tileGrid - (float)j))) * -1f;
                        imageYOffset[index] = (terrainSizeSplittedY - cellSizeSplittedY - ((float)cellSizeSplittedY * (float)i)) * -1f;

                        index++;
                    }
                }

                List<Terrain> stitchingTerrains = OrderedTerrainChunks(splittedTerrains);

                int[] cellIndex = new int[tileImages];
                index = 0;
                int multiplier = 0;

                if(tileImages == terrainChunks)
                {
                    for(int i = 0; i < terrainChunks; i++)
                        cellIndex[index++] = i;
                }
                else
                {
                    int[] topLeftCellIndex = new int[terrainChunks];
                    multiplier = tileImages / terrainChunks;

                    for(int i = 0; i < gridSizeTerrain; i++)
                        for(int j = 0; j < gridSizeTerrain; j++)
                            topLeftCellIndex[index++] = ((i * 1) * gridSizeTerrain) + ((j * 1 * terrainChunks) * gridSizeTerrain);

                    Array.Sort(topLeftCellIndex);

                    index = 0;

                    for(int z = 0; z < terrainChunks; z++)
                    {
                        int adder = 0;

                        for(int i = 0; i < gridSizeTerrain; i++)
                        {
                            for(int j = 0; j < gridSizeTerrain; j++)
                            {
                                try
                                {
                                    cellIndex[index++] = topLeftCellIndex[counter] + j + adder;
                                }
                                catch(Exception e)
                                {
                                    UnityEngine.Debug.Log(e);
                                }
                            }
                            adder += multiplier;
                        }
                        counter++;
                    }
                }

                counter = 0;
                index = 0;

                foreach(Terrain terrainSplitted in stitchingTerrains)
                {
#if UNITY_2018_3_OR_NEWER
                    TerrainLayer[] terrainLayers = new TerrainLayer[imagesPerTerrain];

                    for (int i = 0; i < imagesPerTerrain; i++)
                    {
                        try
                        {
                            Texture2D satelliteImage = images[cellIndex[index]];

                            // Texturing Terrain
                            terrainLayers[i] = new TerrainLayer();
                            terrainLayers[i].diffuseTexture = satelliteImage;
                            terrainLayers[i].tileSize = new Vector2(cellSizeSplittedX, cellSizeSplittedY);
                            terrainLayers[i].tileOffset = new Vector2(imageXOffset[i], imageYOffset[i]);
                            terrainLayers[i].metallic = 0f;
                            terrainLayers[i].smoothness = 0f;
                        }
                        catch (Exception e)
                        {
                            UnityEngine.Debug.LogError(e);
                        }

                        index++;
                    }

                    terrainSplitted.terrainData.terrainLayers = terrainLayers;
#else
                    terrainTextures = new SplatPrototype[imagesPerTerrain];

                    for (int i = 0; i < imagesPerTerrain; i++)
                    {
                        try
                        {
                            Texture2D satelliteImage = images[cellIndex[index]];

                            // Texturing Terrain
                            terrainTextures[i] = new SplatPrototype();
                            terrainTextures[i].texture = satelliteImage;
                            terrainTextures[i].tileSize = new Vector2(cellSizeSplittedX, cellSizeSplittedY);
                            terrainTextures[i].tileOffset = new Vector2(imageXOffset[i], imageYOffset[i]);
                            terrainTextures[i].texture.Apply();

                            terrainTextures[i].metallic = 0f;
                            terrainTextures[i].smoothness = 0f;
                        }
                        catch(Exception e)
                        {
                            UnityEngine.Debug.LogError(e);
                        }

                        index++;
                    }

                    terrainSplitted.terrainData.splatPrototypes = terrainTextures;
#endif

                    splatNormalizeX = terrainSplitted.terrainData.size.x / terrainSplitted.terrainData.alphamapResolution;
                    splatNormalizeY = terrainSplitted.terrainData.size.z / terrainSplitted.terrainData.alphamapResolution;

                    int lengthz = (int)(cellSizeSplittedY / splatNormalizeY);
                    int widthz = (int)(cellSizeSplittedX / splatNormalizeX);

                    for (int i = 0; i < imagesPerTerrain; i++)
                    {
                        try
                        {
                            int lengthzOff = (int)(imageYOffset[i] / splatNormalizeY);
                            int widthzOff = (int)(imageXOffset[i] / splatNormalizeX);

                            smData = new float[lengthz, widthz, terrainSplitted.terrainData.alphamapLayers];

                            for(int y = 0; y <lengthz; y++)
                                for(int z = 0; z < widthz; z++)
                                    smData[y, z, i] = 1;

                            terrainSplitted.terrainData.SetAlphamaps(-widthzOff, -lengthzOff, smData);
                        }
                        catch(Exception e)
                        {
                            UnityEngine.Debug.LogError(e);
                        }
                    }

                    terrainSplitted.terrainData.RefreshPrototypes();
                    terrainSplitted.Flush();

                    terrainTextures = null;
                    smData = null;

                    counter++;
                }
            }
        }

        public static List<Terrain> OrderedTerrainChunks (GameObject terrainsParentGo)
        {
            string names = "";

            foreach (Transform child in terrainsParentGo.transform)
                names += child.name + Environment.NewLine;

            String[] lines = names.Replace("\r","").Split('\n');
            lines = LogicalComparer(lines);

            List<Terrain> stitchingTerrains = new List<Terrain>();

            foreach (string s in lines)
                if(s != "")
                    stitchingTerrains.Add(terrainsParentGo.transform.Find(s).GetComponent<Terrain>());

            names = null;

            return stitchingTerrains;
        }

        private static void ImageTilerOnline ()
        {
            imageDownloadingStarted = true;
            availableImageryCheked = false;
            allBlack = false;
            int counter = 0;
            int tileImages = totalImages;

            if(!splittedTerrains)
            {
#if UNITY_2018_3_OR_NEWER
                TerrainLayer[] terrainLayers = new TerrainLayer[tileImages];

                for (int i = 0; i < tileImages; i++)
                {
                    try
                    {
                        Texture2D satelliteImage = images[i];

                        // Texturing Terrain
                        terrainLayers[i] = new TerrainLayer();
                        terrainLayers[i].diffuseTexture = satelliteImage;
                        terrainLayers[i].tileSize = new Vector2(cellSizeX, cellSizeY);
                        terrainLayers[i].tileOffset = new Vector2(imageXOffset[i], imageYOffset[i]);
                        terrainLayers[i].metallic = 0f;
                        terrainLayers[i].smoothness = 0f;
                    }
                    catch (Exception e)
                    {
                        UnityEngine.Debug.LogError(e);
                    }
                }

                terrain.terrainData.terrainLayers = terrainLayers;
#else
                terrainTextures = new SplatPrototype[tileImages];

                for (int i = 0; i < tileImages; i++)
                {
                    try
                    {
                        Texture2D satelliteImage = images[i];

                        // Texturing Terrain
                        terrainTextures[i] = new SplatPrototype();
                        terrainTextures[i].texture = satelliteImage;
                        terrainTextures[i].tileSize = new Vector2(cellSizeX, cellSizeY);
                        terrainTextures[i].tileOffset = new Vector2(imageXOffset[i], imageYOffset[i]);
                        terrainTextures[i].texture.Apply();

                        terrainTextures[i].metallic = 0f;
                        terrainTextures[i].smoothness = 0f;
                    }
                    catch(Exception e)
                    {
                        UnityEngine.Debug.LogError(e);
                    }
                }

                terrain.terrainData.splatPrototypes = terrainTextures;
#endif

                splatNormalizeX = terrainSizeX / terrain.terrainData.alphamapResolution;
                splatNormalizeY = terrainSizeY / terrain.terrainData.alphamapResolution;

                int lengthz = (int)(cellSizeY / splatNormalizeY);
                int widthz = (int)(cellSizeX / splatNormalizeX);

                for (int i = 0; i < tileImages; i++)
                {
                    try
                    {
                        int lengthzOff = (int)(imageYOffset[i] / splatNormalizeY);
                        int widthzOff = (int)(imageXOffset[i] / splatNormalizeX);

                        smData = new float[lengthz, widthz, terrain.terrainData.alphamapLayers];

                        for(int y = 0; y < lengthz; y++)
                            for(int z = 0; z < widthz; z++)
                                smData[y, z, i] = 1;

                        terrain.terrainData.SetAlphamaps(-widthzOff, -lengthzOff, smData);
                    }
                    catch(Exception e)
                    {
                        UnityEngine.Debug.LogError(e);
                    }
                }

                terrain.terrainData.RefreshPrototypes();
                terrain.Flush();

                terrainTextures = null;
                smData = null;
            }
            else
            {
                int index = 0;
                float terrainSizeSplittedX = croppedTerrains[0].terrainData.size.x;
                float terrainSizeSplittedY = croppedTerrains[0].terrainData.size.z;

                float cellSizeSplittedX = terrainSizeSplittedX / (float)tileGrid;
                float cellSizeSplittedY = terrainSizeSplittedY / (float)tileGrid;

                imageXOffset = new float[imagesPerTerrain];
                imageYOffset = new float[imagesPerTerrain];

                for (int i = 0; i < tileGrid; i++)
                {
                    for (int j = 0; j < tileGrid; j++)
                    {
                        imageXOffset[index] = (terrainSizeSplittedX - (cellSizeSplittedX * ((float)tileGrid - (float)j))) * -1f;
                        imageYOffset[index] = (terrainSizeSplittedY - cellSizeSplittedY - ((float)cellSizeSplittedY * (float)i)) * -1f;

                        index++;
                    }
                }

                List<Terrain> stitchingTerrains = OrderedTerrainChunks(splittedTerrains);

                int[] cellIndex = new int[tileImages];
                index = 0;

                for(int i = 0; i < terrainChunks; i++)
                    cellIndex[index++] = i;

                counter = 0;
                index = 0;

                foreach(Terrain terrainSplitted in stitchingTerrains)
                {
#if UNITY_2018_3_OR_NEWER
                    TerrainLayer[] terrainLayers = new TerrainLayer[imagesPerTerrain];

                    for (int i = 0; i < imagesPerTerrain; i++)
                    {
                        try
                        {
                            Texture2D satelliteImage = images[cellIndex[index]];

                            // Texturing Terrain
                            terrainLayers[i] = new TerrainLayer();
                            terrainLayers[i].diffuseTexture = satelliteImage;
                            terrainLayers[i].tileSize = new Vector2(cellSizeSplittedX, cellSizeSplittedY);
                            terrainLayers[i].tileOffset = new Vector2(imageXOffset[i], imageYOffset[i]);
                            terrainLayers[i].metallic = 0f;
                            terrainLayers[i].smoothness = 0f;
                        }
                        catch (Exception e)
                        {
                            UnityEngine.Debug.LogError(e);
                        }

                        index++;
                    }

                    terrainSplitted.terrainData.terrainLayers = terrainLayers;
#else
                    terrainTextures = new SplatPrototype[imagesPerTerrain];

                    for (int i = 0; i < imagesPerTerrain; i++)
                    {
                        try
                        {
                            Texture2D satelliteImage = images[cellIndex[index]];

                            // Texturing Terrain
                            terrainTextures[i] = new SplatPrototype();
                            terrainTextures[i].texture = satelliteImage;
                            terrainTextures[i].tileSize = new Vector2(cellSizeSplittedX, cellSizeSplittedY);
                            terrainTextures[i].tileOffset = new Vector2(imageXOffset[i], imageYOffset[i]);
                            terrainTextures[i].texture.Apply();

                            terrainTextures[i].metallic = 0f;
                            terrainTextures[i].smoothness = 0f;
                        }
                        catch(Exception e)
                        {
                            UnityEngine.Debug.LogError(e);
                        }

                        index++;
                    }

                    terrainSplitted.terrainData.splatPrototypes = terrainTextures;
#endif

                    splatNormalizeX = terrainSplitted.terrainData.size.x / terrainSplitted.terrainData.alphamapResolution;
                    splatNormalizeY = terrainSplitted.terrainData.size.z / terrainSplitted.terrainData.alphamapResolution;

                    int lengthz = (int)(cellSizeSplittedY / splatNormalizeY);
                    int widthz = (int)(cellSizeSplittedX / splatNormalizeX);

                    for (int i = 0; i < imagesPerTerrain; i++)
                    {
                        try
                        {
                            int lengthzOff = (int)(imageYOffset[i] / splatNormalizeY);
                            int widthzOff = (int)(imageXOffset[i] / splatNormalizeX);

                            smData = new float[lengthz, widthz, terrainSplitted.terrainData.alphamapLayers];

                            for(int y = 0; y <lengthz; y++)
                                for(int z = 0; z < widthz; z++)
                                    smData[y, z, i] = 1;

                            terrainSplitted.terrainData.SetAlphamaps(-widthzOff, -lengthzOff, smData);
                        }
                        catch(Exception e)
                        {
                            UnityEngine.Debug.LogError(e);
                        }
                    }

                    terrainSplitted.terrainData.RefreshPrototypes();
                    terrainSplitted.Flush();

                    terrainTextures = null;
                    smData = null;

                    counter++;
                }
            }

            // Setup Far Terrains
            if(runTime.farTerrain)
            {
                for(int i = 1; i <= 2; i++)
                {
                    if(i == 1)
                        currentTerrain = firstTerrain;
                    else if(i == 2)
                        currentTerrain = secondaryTerrain;

#if UNITY_2018_3_OR_NEWER
                    TerrainLayer[] terrainLayers = new TerrainLayer[1];

                    try
                    {
                        // Texturing Terrain
                        terrainLayers[0] = new TerrainLayer();
                        terrainLayers[0].diffuseTexture = farImage;
                        terrainLayers[0].tileSize = new Vector2(farTerrainSize, farTerrainSize);
                        terrainLayers[0].tileOffset = Vector2.zero;
                        terrainLayers[0].metallic = 0f;
                        terrainLayers[0].smoothness = 0f;
                    }
                    catch (Exception e)
                    {
                        UnityEngine.Debug.LogError(e);
                    }

                    currentTerrain.terrainData.terrainLayers = terrainLayers;
#else
                    terrainTextures = new SplatPrototype[1];

                    try
                    {
                        // Texturing Terrain
                        terrainTextures[0] = new SplatPrototype();
                        terrainTextures[0].texture = farImage;
                        terrainTextures[0].tileSize = new Vector2(farTerrainSize, farTerrainSize);
                        terrainTextures[0].tileOffset = Vector2.zero;
                        terrainTextures[0].texture.Apply();

                        terrainTextures[0].metallic = 0f;
                        terrainTextures[0].smoothness = 0f;
                    }
                    catch(Exception e)
                    {
                        UnityEngine.Debug.LogError(e);
                    }

                    currentTerrain.terrainData.splatPrototypes = terrainTextures;
#endif

                    int length = currentTerrain.terrainData.alphamapResolution;
                    smData = new float[length, length, 1];

                    try
                    {
                        for(int y = 0; y < length; y++)
                            for(int z = 0; z < length; z++)
                                smData[y, z, 0] = 1f;

                        currentTerrain.terrainData.SetAlphamaps(0, 0, smData);
                    }
                    catch(Exception e)
                    {
                        UnityEngine.Debug.LogError(e);
                    }

                    currentTerrain.terrainData.RefreshPrototypes();
                    currentTerrain.Flush();

                    terrainTextures = null;
                    smData = null;
                }
            }
        }

        private static IEnumerator<float> WorldIsGenerated ()
        {
            yield return Timing.WaitForSeconds(2);

            worldIsGenerated = true;
            UnityEngine.Debug.Log("World Is Generated");
        }
    }
}

