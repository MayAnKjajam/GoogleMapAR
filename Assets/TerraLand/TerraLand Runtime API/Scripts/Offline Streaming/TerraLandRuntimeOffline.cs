using UnityEngine;
using UnityEngine.Experimental.TerrainAPI;
using UnityEngine.Networking;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using BitMiracle.LibTiff.Classic;
using MEC;
using TerraLandWorldElevation;
using TerraLandWorldImagery;

namespace TerraLand
{
    public class TerraLandRuntimeOffline : MonoBehaviour
    {
        public static RuntimeOffline runTime;
        private static FloatingOriginAdvanced floatingOriginAdvanced;

        public static string top, left, bottom, right;
        public static float areaSizeLat;
        public static float areaSizeLon;
        public static int gridPerTerrain = 1;

        public enum Neighbourhood
        {
            Moore = 0,
            VonNeumann = 1
        }
        static Neighbourhood neighbourhood = Neighbourhood.Moore;

        public bool imageDownloadingStarted = false;
        string downloadDateImagery;
        static Terrain terrain;
        public static GameObject splittedTerrains;
        public static int splitSizeFinal;
        public static List<Terrain> croppedTerrains;
        public static int terrainChunks;
        public static int checkLength;
        //static int terrainResolutionDownloading;

        static List<float> topCorner, bottomCorner, leftCorner, rightCorner;
        public static GameObject terrainsParent;

        static int terrainsLong, terrainsWide;
        static float oldWidth, oldHeight, oldLength;

        public static float terrainSizeNewX;
        public static float terrainSizeNewY;
        public static float terrainSizeNewZ;

        static float newWidth, newLength;
        static float xPos, yPos, zPos;
        static int newHeightmapResolution;
        public static int heightmapResolutionSplit;

        public static float terrainSizeFactor;
        public static float terrainSizeX;
        public static float terrainSizeY;

        private static GameObject[] terrainGameObjects;
        private static Terrain currentTerrain;
        //private static TerrainData[] datas;
        private static TerrainData data;

        static int arrayPos;
        //static string address;

        //World_Imagery_MapServer mapservice;
        //public TopoBathy_ImageServer mapserviceElevation;
        //MapServerInfo mapinfo;
        //MapDescription mapdesc;

        public static string fileNameTerrainData;
        public static List<string> fileNameTerrainDataDynamic;

        WebClient webClientTerrain;

        public static int tiffWidth;
        public static int tiffLength;
        public static float[,] tiffData;
        static float[,] tiffDataASCII;
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

        static float everestPeak = 8848.0f;
        static float terrainEverestDiffer;
        static float currentHeight;

        public static float smoothBlend = 0.8f;

        public static int smoothBlendIndex = 0;
        //static float smoothProgress;
        //static float smoothIterationProgress;

        static float[,] finalHeights;

        //static bool saveTerrainDataASCII = false;
        //static bool saveTerrainDataRAW = false;
        //static bool saveTerrainDataTIFF = false;

        public static int totalImages;
        public static int totalImagesDataBase;
        //public static int gridNumber;
        public static int chunkImageResolution;

        private static float[,] heightmapCell;
        private static float[,] heightmapCellTopRow;
        private static float[,] heightmapCellRightColumn;

        public static string[] elevationNames;
        public static string[] imageryNames;
        public static string[] normalNames;
        public static string dataBasePathElevation;
        public static string geoDataExtensionElevation;
        public static string dataBasePathImagery;
        public static string dataBasePathNormals;
        //string geoDataPathImagery;
        //string geoDataExtensionImagery;

        public static List<float[,]> rawData;
        public static int m_Width = 1;
        public static int m_Height = 1;

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
        public static int taskIndex;
        public static List<int> spiralIndex;
        public static List<Vector2> spiralCell;

        public static List<Texture2D> images;
        static SplatPrototype[] terrainTextures;
        static float[,,] smData;
        static float cellSizeX;
        static float cellSizeY;
        static float[] imageXOffset;
        static float[] imageYOffset;

        static float splatNormalizeX;
        static float splatNormalizeY;

        static string directoryPathImagery;
        public static int imagesPerTerrain;
        public static bool multipleTerrainsTiling;
        public static int tileGrid;

        static WebClient webClientImage;
        public static List<byte[]> imageBytes;
        public static int imageWidth;
        public static int imageHeight;
        public static bool geoImagesOK = false;

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

        //public int downloadedImageIndex;

        public static double[] latCellTop;
        public static double[] latCellBottom;
        public static double[] lonCellLeft;
        public static double[] lonCellRight;

        //int compressionQuality = 100;

        byte[] tiffBuffer;

        public static int nCols;
        public static int nRows;
        static double xllCorner;
        static double yllCorner;
        static double cellSizeASCII;
        static double noData;
        public static float[,] asciiData;
        static int downloadedHeightmapIndex;

        static string tempFolder;
        static List<float[,]> tiffDataDynamic;
        public static int tileResolution;

        static Dictionary<int[],Terrain> _terrainDict = null;
        static Terrain[] _terrains;
        public enum Side
        {
            Left,
            Right,
            Top,
            Bottom
        }
        public static int concurrentUpdates = 0;
        static bool hasTop = false;
        static bool hasRight = false;
        static bool hasLeft = false;
        static bool hasBottom = false;

        static int stitchedTerrainsCount = 0;

        static double latUser;
        static double lonUser;

        public static List<Terrain> stitchingTerrainsList;

        public static bool sceneIsInitialized = false;
        public static bool imagesAreGenerated = false;
        public static bool terrainsAreGenerated = false;
        public static bool farTerrainIsGenerated = false;
        public static bool worldIsGenerated = false;

        public static int northIndex;
        public static int southIndex;
        public static int westIndex;
        public static int eastIndex;
        public static int[] northIndices;
        public static int[] southIndices;
        public static int[] eastIndices;
        public static int[] westIndices;
        public static int[] indicesMoveNORTH;
        public static int[] indicesMoveSOUTH;
        public static int[] indicesMoveEAST;
        public static int[] indicesMoveWEST;

        public static int northTerrainsUseTimes = 0;
        public static int southTerrainsUseTimes = 0;
        public static int eastTerrainsUseTimes = 0;
        public static int westTerrainsUseTimes = 0;

        public static string direction = "";
        public static float tileDelay = 1f;
        private static int downloadedImages;

        public static bool stitchingInProgress;
        public static float[,] heights;
        public static float[,] secondHeights;

        public static bool needsStitching = true;

        private static int[,] indexFromCenter;
        private static int length;
        private static int index;
        private static int filteredIndex;
        private static int topIndex;
        private static int downIndex;
        private static int leftIndex;
        private static int rightIndex;
        private static float tsX;
        private static float tsY;
        private static Uri URL;
        private static SplatPrototype[] terrainLayers;
        private static UnityWebRequest uwr;
        private static UnityWebRequestAsyncOperation uwra;
        private static int currentRow;
        private static GameObject terrainGameObject;
        private static string nameStr;
        private static string[] names;
        private static ns.NumericComparer ns;
        private static int counter;
        private static int tileHeight;
        private static int tileWidth;
        private static byte[] buffer;
        private static float[,] fBuffer;
        private static float num = 1.525879E-05f;
        private static float num10 = 0.00390625f;
        private static int num2;
        private static byte b;
        private static ushort num3;
        private static float num4;
        private static int indx;
        private static byte num14;
        private static float num15;
        private static float[,] rawDataFloat;
        private static FileStream stream;
        private static int numSqr;
        private static StreamReader sr;
        private static string[] line1;
        private static string[] line2;
        private static string[] line3;
        private static string[] line4;
        private static string[] line5;
        private static string[] line6;
        private static string[] line;
        private static float lowestPointNormalized;
        private static int xLength;
        private static int yLength;
        private static int xStart;
        private static int yStart;
        private static float[,] dataSplitted;
        private static float realTerrainWidth;
        private static float realTerrainLength;
        private static int sizeX;
        private static int sizeZ;
        private static int[] posTer;
        private static Terrain topTerrain;
        private static Terrain leftTerrain;
        private static Terrain rightTerrain;
        private static Terrain bottomTerrain;
        private static Terrain rightBottom;
        private static int size;
        private static float cornerHeight;
        private static float[,] heights1x1;
        private static float[,] generatedHeightMap;
        private static float oldHeightAtPoint;
        private static float newHeightAtPoint;
        private static float blendedHeightAtPoint;
        private static int xNeighbours;
        private static int yNeighbours;
        private static int xShift;
        private static int yShift;
        private static int xIndex;
        private static int yIndex;
        private static int Tx;
        private static int Ty;
        private static int Ny;
        private static int Nx;
        private static float hCumulative;
        private static int nNeighbours;
        private static float heightAtPoint;
        private static float hAverage;
        private static float scaleFactorLat;
        private static float scaleFactorLon;
        private static int X1;
        private static int Y1;
        private static float realTerrainSizeY;
        private static float realTerrainSizeZ;
        private static Vector3 finalTerrainSize;
        private static int gridCount;
        private static int row;
        private static int col;
        private static string childName;
        private static List<Terrain> stitchingTerrains;
        private static int centerLayer;
        private static int tiles;
        private static int tileImages;
        private static int indexLft;
        private static int indexTop;
        private static int indexRgt;
        private static int indexBtm;
        private static int lengthz;
        private static int widthz;
        private static int lengthzOff;
        private static int widthzOff;
        private static float terrainSizeSplittedX;
        private static float terrainSizeSplittedY;
        private static float cellSizeSplittedX;
        private static float cellSizeSplittedY;
        private static int[] cellIndex;
        private static int multiplier;
        private static int[] topLeftCellIndex;
        private static int adder;
        private static SplatPrototype[] splatPrototypes;
        private static Texture2D splatmap;
        private static SplatPrototype[] sp;
        private static int alphamapResolution;

        //private static int requestStep = 1;


        private static void SelectDatabaseDirectory ()
        {
            //#if UNITY_EDITOR
            //dataBasePath = UnityEditor.EditorUtility.OpenFolderPanel("Select Database Folder", "", "");
            //#endif
        }

        private static void LoadDatabaseElevation ()
        {
            if(Directory.Exists(dataBasePathElevation))
                runTime.ApplyElevationData();
            else
                Debug.LogError("UNKNOWN DATABASE DIRECTORY (ELEVATION) - Select a directory which includes Elevation data folder.");
        }

        private static void LoadDatabaseImagery ()
        {
            if(Directory.Exists(dataBasePathImagery))
                runTime.ApplyImageData();
            else
                Debug.LogError("UNKNOWN DATABASE DIRECTORY (IMAGERY) - Select a directory which includes Imagery data folder.");
        }

        public static void Initialize ()
        {
            northIndices = new int[runTime.activeTilesGrid];
            southIndices = new int[runTime.activeTilesGrid];
            westIndices = new int[runTime.activeTilesGrid];
            eastIndices = new int[runTime.activeTilesGrid];

            indicesMoveNORTH = new int[RuntimeOffline.totalTiles];
            indicesMoveSOUTH = new int[RuntimeOffline.totalTiles];
            indicesMoveEAST = new int[RuntimeOffline.totalTiles];
            indicesMoveWEST = new int[RuntimeOffline.totalTiles];

            northIndex = 0;
            southIndex = RuntimeOffline.totalTiles - runTime.activeTilesGrid;
            westIndex = 0;
            eastIndex = runTime.activeTilesGrid - 1;

            for(int i = 0; i < runTime.activeTilesGrid; i++)
            {
                northIndices[i] = i;
                southIndices[i] = southIndex + i;
                westIndices[i] = i * runTime.activeTilesGrid;
                eastIndices[i] = eastIndex + (i * runTime.activeTilesGrid);
            }

            counter = 0;

            for(int i = 0; i < runTime.activeTilesGrid; i++)
            {
                for(int j = 0; j < runTime.activeTilesGrid; j++)
                {
                    // NORTH
                    if(i == 0)
                        indicesMoveNORTH[counter] = j + southIndex;
                    else
                        indicesMoveNORTH[counter] = counter - runTime.activeTilesGrid;

                    // SOUTH
                    if(i < runTime.activeTilesGrid - 1)
                        indicesMoveSOUTH[counter] = counter + runTime.activeTilesGrid;
                    else
                        indicesMoveSOUTH[counter] = j + northIndex;

                    // EAST
                    if(j < runTime.activeTilesGrid - 1)
                        indicesMoveEAST[counter] = counter + 1;
                    else
                        indicesMoveEAST[counter] = westIndices[i];

                    // WEST
                    if(j == 0)
                        indicesMoveWEST[counter] = eastIndices[i];
                    else
                        indicesMoveWEST[counter] = counter - 1;

                    counter++;
                }
            }

            northTerrainsUseTimes = 0;
            southTerrainsUseTimes = 0;
            eastTerrainsUseTimes = 0;
            westTerrainsUseTimes = 0;

            rawData = new List<float[,]>();

            topCorner = new List<float>();
            bottomCorner = new List<float>();
            leftCorner = new List<float>();
            rightCorner = new List<float>();

            sp = new SplatPrototype[1];
            sp[0] = new SplatPrototype();

            stitchingTerrains = new List<Terrain>();
            stitchingTerrainsList = new List<Terrain>();

            heights1x1 = new float[1, 1];

            CheckTerrainSizeUnits();

            if(RuntimeOffline.concurrentTasks > terrainChunks)
                RuntimeOffline.concurrentTasks = terrainChunks;
            else if(RuntimeOffline.concurrentTasks < 1)
                RuntimeOffline.concurrentTasks = 1;

            if(runTime.progressiveGeneration)
                runTime.spiralGeneration = false;

            if(runTime.spiralGeneration)
                SpiralOrder();
            else
                NormalOrder();
            
            LoadDatabaseElevation();

            if(!runTime.elevationOnly)
                LoadDatabaseImagery();
        }

        private static void NormalOrder ()
        {
            for(int i = 0; i < terrainChunks; i++)
                spiralIndex.Add(i);
        }

        private static void SpiralOrder ()
        {
            indexFromCenter = new int[runTime.activeTilesGrid, runTime.activeTilesGrid];
            length = runTime.activeTilesGrid;
            index = 0;

            for(int i = 0; i < length; i++)
                for(int j = 0; j < length; j++)
                    indexFromCenter[i, j] = index++;

            SpiralOrderOperation(indexFromCenter);

            spiralIndex.Reverse();
        }

        private static void SpiralOrderOperation (int[,] matrix)
        {
            if(matrix == null || matrix.Length == 0)
                return;

            topIndex = 0;
            downIndex = runTime.activeTilesGrid - 1;
            leftIndex = 0;
            rightIndex = runTime.activeTilesGrid - 1;

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

                // the bottom row
                for(int j = rightIndex; j >= leftIndex; --j)
                    spiralIndex.Add(matrix[downIndex, j]);

                downIndex--;

                if(topIndex > downIndex || leftIndex > rightIndex)
                    break;

                // the leftmost column
                for(int i = downIndex; i >= topIndex; --i)
                    spiralIndex.Add(matrix[i, leftIndex]);

                leftIndex++;

                if(topIndex > downIndex || leftIndex > rightIndex)
                    break;
            }
        }

        public static void GetTerrainBounds ()
        {
            UpdateTileDataFiles(direction);
        }

        private static void UpdateTileDataFiles (string dir)
        {
            index = 0;
            filteredIndex = 0;
            //List<Texture2D> tempImages = new List<Texture2D>();

            for(int i = 0; i < RuntimeOffline.dataBaseGrid; i++)
            {
                for(int j = 0; j < RuntimeOffline.dataBaseGrid; j++)
                {
                    if(i > RuntimeOffline.padStartX - 1 && i < (RuntimeOffline.dataBaseGrid - RuntimeOffline.padEndX) && j > RuntimeOffline.padStartY - 1 && j < (RuntimeOffline.dataBaseGrid - RuntimeOffline.padEndY))
                    {
                        elevationNames[index] = RuntimeOffline.elevationTileNames[filteredIndex];
                        //Debug.Log(filteredIndex);

                        //print(elevationNames[index]);

                        if (!runTime.elevationOnly)
                        {
                            imageryNames[index] = RuntimeOffline.imageryTileNames[filteredIndex];

                            //print(imageryNames[index]);

                            if (runTime.normalsAvailable)
                                normalNames[index] = RuntimeOffline.normalTileNames[filteredIndex];

                            //if(dir.Equals("North"))
                            //{
                            //    tempImages.Add(images[indicesMoveNORTH[index]]);
                            //    imageBytes[index] = imageBytes[indicesMoveNORTH[index]];
                            //}
                            //else if(dir.Equals("South"))
                            //{
                            //    tempImages.Add(images[indicesMoveSOUTH[index]]);
                            //    imageBytes[index] = imageBytes[indicesMoveSOUTH[index]];
                            //}
                            //else if(dir.Equals("East"))
                            //{
                            //    tempImages.Add(images[indicesMoveEAST[index]]);
                            //    imageBytes[index] = imageBytes[indicesMoveEAST[index]];
                            //}
                            //else if(dir.Equals("West"))
                            //{
                            //    tempImages.Add(images[indicesMoveWEST[index]]);
                            //    imageBytes[index] = imageBytes[indicesMoveWEST[index]];
                            //}
                        }

                        index++;
                    }

                    filteredIndex++;
                }
            }

            //images = tempImages;

            if (RuntimeOffline.padStartX == 0)
            {
                if (!InfiniteTerrainOffline.reachedMostNORTH)
                    print("Reached the NORTH borders");

                InfiniteTerrainOffline.reachedMostNORTH = true;
            }

            if(RuntimeOffline.padEndX == 0)
            {
                if (!InfiniteTerrainOffline.reachedMostSOUTH)
                    print("Reached the SOUTH borders");

                InfiniteTerrainOffline.reachedMostSOUTH = true;
            }

            if(RuntimeOffline.padEndY == 0)
            {
                if (!InfiniteTerrainOffline.reachedMostEAST)
                    print("Reached the EAST borders");

                InfiniteTerrainOffline.reachedMostEAST = true;
            }

            if(RuntimeOffline.padStartY == 0)
            {
                if (!InfiniteTerrainOffline.reachedMostWEST)
                    print("Reached the WEST borders");

                InfiniteTerrainOffline.reachedMostWEST = true;
            }

            //if (requestStep == runTime.activeTilesGrid * runTime.activeTilesGrid)
            //{
            //    stitchingTerrainsList.RemoveRange(0, runTime.activeTilesGrid);
            //    requestStep = 1;
            //}
            //else
            //    requestStep += runTime.activeTilesGrid - 1;

            //stitchingTerrainsList.Clear();
            //
            //for (int y = 0; y < runTime.activeTilesGrid; y++)
            //    for (int x = 0; x < runTime.activeTilesGrid; x++)
            //        stitchingTerrainsList.Add(InfiniteTerrainOffline._grid[x, y]);

            Timing.RunCoroutine(ConnectToServers(dir));

            //if (!runTime.elevationOnly)
                //Timing.RunCoroutine(LoadTerrainTextures(dir));
        }

        private static IEnumerator<float> ConnectToServers (string dir)
        {
            if(dir.Equals("North"))
            {
                if(InfiniteTerrainOffline.northTerrains.Count > 0)
                {
                    RuntimeOffline.northCounter = 0;
                    runTime.ServerConnectHeightmapNORTH(northIndices[0]);
                }
            }
            else if(dir.Equals("South"))
            {
                if (InfiniteTerrainOffline.southTerrains.Count > 0)
                {
                    RuntimeOffline.southCounter = 0;
                    runTime.ServerConnectHeightmapSOUTH(southIndices[0]);
                }
            }
            else if(dir.Equals("East"))
            {
                if (InfiniteTerrainOffline.eastTerrains.Count > 0)
                {
                    RuntimeOffline.eastCounter = 0;
                    runTime.ServerConnectHeightmapEAST(eastIndices[0]);
                }
            }
            else if(dir.Equals("West"))
            {
                if (InfiniteTerrainOffline.westTerrains.Count > 0)
                {
                    RuntimeOffline.westCounter = 0;
                    runTime.ServerConnectHeightmapWEST(westIndices[0]);
                }
            }

            yield return 0;
        }

        private static void CheckTerrainSizeUnits ()
        {
            terrainSizeFactor = areaSizeLat / areaSizeLon;

            if (runTime.activeTilesGrid > 1)
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
            else if(terrain)
            {
                terrainSizeX = terrain.terrainData.size.x;
                terrainSizeY = terrain.terrainData.size.z;
            }
        }

        public static void ElevationDownload (int i)
        {
            try
            {
                if (geoDataExtensionElevation.Equals("raw"))
                    RawDataDynamic(i);

                tiffDataDynamic = new List<float[,]>();

                //if (geoDataExtensionElevation.Equals("raw"))
                //    rawData[i] = RawDataDynamic(elevationNames[i]);
                //else if(geoDataExtensionElevation.Equals("tif"))
                //tiffDataDynamic[i] = TiffDataDynamic(fileNameTerrainDataDynamic[i]);
                //else if(geoDataExtensionElevation.Equals("asc"))
                //asciiData[i] = AsciiDataDynamic(fileNameTerrainDataDynamic[i]);
            }
            catch (Exception e)
            {
                UnityEngine.Debug.Log(e);
                return;
            }
        }

        private void DownloadTerrainData (string urlAddress, string location)
        {
            using (webClientTerrain = new WebClient())
            {
                URL = urlAddress.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ? new Uri(urlAddress) : new Uri("http://" + urlAddress);

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

            //if(geoImagesOK && !runTime.elevationOnly)
                //ImageTiler();
        }

        public static void LoadImageData (string urlAddress)
        {
            using (webClientImage = new WebClient())
            {
                //URL = urlAddress.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ? new Uri(urlAddress) : new Uri("http://" + urlAddress);
                URL = new Uri(urlAddress);

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

        public static void ImageDownloader(int i)
        {
            try
            {
                using (webClientImage = new WebClient())
                {
                    imageBytes[i] = webClientImage.DownloadData(imageryNames[i]);
                }
            }
            catch (Exception e)
            {
                Debug.Log(e);
                return;
            }
        }

        public static IEnumerator<float> FillImages (int length)
        {
            for(int i = 0; i < length; i++)
            {
                if(!runTime.fastStartBuild)
                    yield return Timing.WaitForSeconds(runTime.imageryDelay);

                if(runTime.spiralGeneration)
                    images[spiralIndex[i]].LoadImage(imageBytes[spiralIndex[i]]);
                else
                    images[i].LoadImage(imageBytes[i]);
            }

            imagesAreGenerated = true;
            UnityEngine.Debug.Log("Satellite Images Are Generated");

            if(terrainsAreGenerated)
            {
                if(RuntimeOffline.farTerrain)
                {
                    if(farTerrainIsGenerated)
                        Timing.RunCoroutine(WorldIsGenerated());
                }
                else
                    Timing.RunCoroutine(WorldIsGenerated());
            }
        }

        public static IEnumerator FillImagesFAST()
        {
            terrainSizeX = croppedTerrains[0].terrainData.size.x;
            terrainSizeY = croppedTerrains[0].terrainData.size.z;
            index = 0;

            if (runTime.enableSplatting)
            {
#if UNITY_2018_3_OR_NEWER
                TerrainLayer[] terrainLayers = new TerrainLayer[4];
                terrainLayers[0] = new TerrainLayer();
                terrainLayers[1] = new TerrainLayer();
                terrainLayers[2] = new TerrainLayer();
                terrainLayers[3] = new TerrainLayer();

                for (int i = 0; i < RuntimeOffline.totalTiles; i++)
                {
                    if (runTime.spiralGeneration)
                        index = spiralIndex[i];
                    else
                        index = i;

                    terrainLayers[0].diffuseTexture = runTime.layer1Albedo;
                    terrainLayers[0].normalMapTexture = runTime.layer1Normal;
                    terrainLayers[0].tileSize = new Vector2(runTime.tiling1, runTime.tiling1);
                    terrainLayers[0].tileOffset = new Vector2(0, 0);
                    terrainLayers[0].metallic = 0;
                    terrainLayers[0].smoothness = 0;

                    terrainLayers[1].diffuseTexture = runTime.layer2Albedo;
                    terrainLayers[1].normalMapTexture = runTime.layer2Normal;
                    terrainLayers[1].tileSize = new Vector2(runTime.tiling2, runTime.tiling2);
                    terrainLayers[1].tileOffset = new Vector2(0, 0);
                    terrainLayers[1].metallic = 0;
                    terrainLayers[1].smoothness = 0;

                    terrainLayers[2].diffuseTexture = runTime.layer3Albedo;
                    terrainLayers[2].normalMapTexture = runTime.layer3Normal;
                    terrainLayers[2].tileSize = new Vector2(runTime.tiling3, runTime.tiling3);
                    terrainLayers[2].tileOffset = new Vector2(0, 0);
                    terrainLayers[2].metallic = 0;
                    terrainLayers[2].smoothness = 0;

                    terrainLayers[3].diffuseTexture = runTime.layer4Albedo;
                    terrainLayers[3].normalMapTexture = runTime.layer4Normal;
                    //terrainLayers[3].tileSize = new Vector2(terrainSizeX, terrainSizeY);
                    terrainLayers[3].tileSize = new Vector2(runTime.tiling4, runTime.tiling4);
                    terrainLayers[3].tileOffset = new Vector2(0, 0);
                    terrainLayers[3].metallic = 0;
                    terrainLayers[3].smoothness = 0;

                    //if (RuntimeOffline.normalsAvailable)
                    //{
                    //    using (WWW www = new WWW("file:///" + normalNames[index]))
                    //    {
                    //        yield return www;

                    //        if (!string.IsNullOrEmpty(www.error))
                    //            Debug.Log(www.error);

                    //        terrainLayers[3].normalMap = www.texture;
                    //    }
                    //}

                    croppedTerrains[index].terrainData.terrainLayers = terrainLayers;
                }
#else
                splatPrototypes = new SplatPrototype[4];
                splatPrototypes[0] = new SplatPrototype();
                splatPrototypes[1] = new SplatPrototype();
                splatPrototypes[2] = new SplatPrototype();
                splatPrototypes[3] = new SplatPrototype();

                for (int i = 0; i < RuntimeOffline.totalTiles; i++)
                {
                    if (runTime.spiralGeneration)
                        index = spiralIndex[i];
                    else
                        index = i;

                    splatPrototypes[0].texture = runTime.layer1Albedo;
                    splatPrototypes[0].normalMap = runTime.layer1Normal;
                    splatPrototypes[0].tileSize = new Vector2(runTime.tiling1, runTime.tiling1);
                    splatPrototypes[0].tileOffset = new Vector2(0, 0);
                    splatPrototypes[0].metallic = 0;
                    splatPrototypes[0].smoothness = 0;

                    splatPrototypes[1].texture = runTime.layer2Albedo;
                    splatPrototypes[1].normalMap = runTime.layer2Normal;
                    splatPrototypes[1].tileSize = new Vector2(runTime.tiling2, runTime.tiling2);
                    splatPrototypes[1].tileOffset = new Vector2(0, 0);
                    splatPrototypes[1].metallic = 0;
                    splatPrototypes[1].smoothness = 0;

                    splatPrototypes[2].texture = runTime.layer3Albedo;
                    splatPrototypes[2].normalMap = runTime.layer3Normal;
                    splatPrototypes[2].tileSize = new Vector2(runTime.tiling3, runTime.tiling3);
                    splatPrototypes[2].tileOffset = new Vector2(0, 0);
                    splatPrototypes[2].metallic = 0;
                    splatPrototypes[2].smoothness = 0;

                    splatPrototypes[3].texture = runTime.layer4Albedo;
                    splatPrototypes[3].normalMap = runTime.layer4Normal;
                    //splatPrototypes[3].tileSize = new Vector2(terrainSizeX, terrainSizeY);
                    splatPrototypes[3].tileSize = new Vector2(runTime.tiling4, runTime.tiling4);
                    splatPrototypes[3].tileOffset = new Vector2(0, 0);
                    splatPrototypes[3].metallic = 0;
                    splatPrototypes[3].smoothness = 0;

                    //if (RuntimeOffline.normalsAvailable)
                    //{
                    //    using (WWW www = new WWW("file:///" + normalNames[index]))
                    //    {
                    //        yield return www;

                    //        if (!string.IsNullOrEmpty(www.error))
                    //            Debug.Log(www.error);

                    //        splatPrototypes[3].normalMap = www.texture;
                    //    }
                    //}

                    croppedTerrains[index].terrainData.splatPrototypes = splatPrototypes;
                }
#endif

                for (int x = 0; x < RuntimeOffline.totalTiles; x++)
                {
                    if (!runTime.fastStartBuild)
                        yield return Timing.WaitForSeconds(runTime.imageryDelay);

                    if (runTime.spiralGeneration)
                        index = spiralIndex[x];
                    else
                        index = x;

                    if (File.Exists(imageryNames[index]))
                    {
                        using (WWW www = new WWW("file:///" + imageryNames[index]))
                        {
                            yield return www;

                            if (!string.IsNullOrEmpty(www.error))
                                Debug.Log(www.error);

                            splatmap = www.texture;
                            Color32[] pixels = splatmap.GetPixels32();

                            alphamapResolution = splatmap.width;
                            croppedTerrains[index].terrainData.alphamapResolution = alphamapResolution;
                            counter = 0;
                            smData = new float[alphamapResolution, alphamapResolution, croppedTerrains[index].terrainData.alphamapLayers];

                            for (int y = 0; y < alphamapResolution; y++)
                            {
                                for (int z = 0; z < alphamapResolution; z++)
                                {
                                    smData[y, z, 0] = pixels[counter].r / 255f;
                                    smData[y, z, 1] = pixels[counter].g / 255f;
                                    smData[y, z, 2] = pixels[counter].b / 255f;
                                    smData[y, z, 3] = pixels[counter].a / 255f;

                                    //smData[y, z, 3] = 0;
                                    //smData[y, z, 3] = 1;
                                    //smData[y, z, 3] = pixels[counter].a - (pixels[counter].r + pixels[counter].g + pixels[counter].b);
                                    //smData[y, z, 3] = 1 - (pixels[counter].r + pixels[counter].g + pixels[counter].b);

                                    counter++;
                                }
                            }

                            // Uncommnet following lines just for Editor Debugging
                            /*
                            TerrainData tData = new TerrainData();
                            tData.splatPrototypes = splatPrototypes;
                            tData.alphamapResolution = alphamapResolution;
                            tData.SetAlphamaps(0, 0, smData);
                            UnityEditor.AssetDatabase.CreateAsset(croppedTerrains[index].terrainData, "Assets/" + croppedTerrains[index].name + ".asset");
                            UnityEditor.AssetDatabase.Refresh();
                            Destroy(tData);
                            */

                            croppedTerrains[index].terrainData.SetAlphamaps(0, 0, smData);
                            croppedTerrains[index].Flush();
                        }
                    }
                }
            }
            else
            {
                for (int i = 0; i < RuntimeOffline.totalTiles; i++)
                {
                    if (!runTime.fastStartBuild)
                        yield return Timing.WaitForSeconds(runTime.imageryDelay);

                    if (runTime.spiralGeneration)
                        index = spiralIndex[i];
                    else
                        index = i;

                    if (File.Exists(imageryNames[index]))
                    {
                        using (WWW www = new WWW("file:///" + imageryNames[index]))
                        {
                            yield return www;

                            if (!string.IsNullOrEmpty(www.error))
                                Debug.Log(www.error);

#if UNITY_2018_3_OR_NEWER
                            TerrainLayer[] terrainLayers = new TerrainLayer[1];
                            terrainLayers[0] = new TerrainLayer();
                            terrainLayers[0].diffuseTexture = www.textureNonReadable;
                            terrainLayers[0].tileSize = new Vector2(terrainSizeX, terrainSizeY);
                            terrainLayers[0].tileOffset = new Vector2(0, 0);
                            terrainLayers[0].diffuseTexture.wrapMode = TextureWrapMode.Clamp;
                            terrainLayers[0].diffuseTexture.name = System.IO.Path.GetFileName(imageryNames[index]);
                            terrainLayers[0].name = System.IO.Path.GetFileName(imageryNames[index]);

                            croppedTerrains[index].terrainData.terrainLayers = terrainLayers;
#else
                            sp[0].texture = www.textureNonReadable;
                            sp[0].tileSize = new Vector2(terrainSizeX, terrainSizeY);
                            sp[0].tileOffset = new Vector2(0, 0);
                            sp[0].texture.wrapMode = TextureWrapMode.Clamp;
                            sp[0].texture.name = Path.GetFileName(imageryNames[index]);

                            croppedTerrains[index].terrainData.splatPrototypes = sp;
#endif
                        }
                    }
                }

                if (geoImagesOK && !runTime.elevationOnly)
                    ImageTiler();
            }

            imagesAreGenerated = true;
            UnityEngine.Debug.Log("Satellite Images Are Generated");

            if (terrainsAreGenerated)
            {
                if (RuntimeOffline.farTerrain)
                {
                    if (farTerrainIsGenerated)
                        Timing.RunCoroutine(WorldIsGenerated());
                }
                else
                    Timing.RunCoroutine(WorldIsGenerated());
            }
        }

        public static IEnumerator<float> FillImage (int i, string dir)
        {
            images[i].LoadImage(imageBytes[i]);

            downloadedImages++;

            if (downloadedImages == runTime.activeTilesGrid)
            {
                FinalizeTiles(dir);
                downloadedImages = 0;
            }

            yield return 0;
        }

        public static IEnumerator FillImageFAST(string dir)
        {
            if (runTime.enableSplatting)
            {
                for (int i = 0; i < runTime.activeTilesGrid; i++)
                {
                    //int index = 0;
                    int imageIndex = 0;

                    //if (dir.Equals("North"))
                    //    index = ((northIndices[i] + 1) * requestStep) - 1;
                    //else if (dir.Equals("South"))
                    //    index = ((southIndices[i] + 1) * requestStep) - 1;
                    //else if (dir.Equals("East"))
                    //    index = ((eastIndices[i] + 1) * requestStep) - 1;
                    //else if (dir.Equals("West"))
                    //    index = ((westIndices[i] + 1) * requestStep) - 1;

                    if (dir.Equals("North"))
                        imageIndex = northIndices[i];
                    else if (dir.Equals("South"))
                        imageIndex = southIndices[i];
                    else if (dir.Equals("East"))
                        imageIndex = eastIndices[i];
                    else if (dir.Equals("West"))
                        imageIndex = westIndices[i];

                    if (runTime.normalsAvailable)
                    {
#if UNITY_2018_3_OR_NEWER
                        TerrainLayer[] terrainLayers = new TerrainLayer[4];
                        terrainLayers[0] = new TerrainLayer();
                        terrainLayers[0].diffuseTexture = runTime.layer1Albedo;
                        terrainLayers[0].normalMapTexture = runTime.layer1Normal;
                        terrainLayers[0].tileSize = new Vector2(runTime.tiling1, runTime.tiling1);
                        terrainLayers[0].tileOffset = new Vector2(0, 0);
                        terrainLayers[0].metallic = 0;
                        terrainLayers[0].smoothness = 0;

                        terrainLayers[1] = new TerrainLayer();
                        terrainLayers[1].diffuseTexture = runTime.layer2Albedo;
                        terrainLayers[1].normalMapTexture = runTime.layer2Normal;
                        terrainLayers[1].tileSize = new Vector2(runTime.tiling2, runTime.tiling2);
                        terrainLayers[1].tileOffset = new Vector2(0, 0);
                        terrainLayers[1].metallic = 0;
                        terrainLayers[1].smoothness = 0;

                        terrainLayers[2] = new TerrainLayer();
                        terrainLayers[2].diffuseTexture = runTime.layer3Albedo;
                        terrainLayers[2].normalMapTexture = runTime.layer3Normal;
                        terrainLayers[2].tileSize = new Vector2(runTime.tiling3, runTime.tiling3);
                        terrainLayers[2].tileOffset = new Vector2(0, 0);
                        terrainLayers[2].metallic = 0;
                        terrainLayers[2].smoothness = 0;

                        terrainLayers[3] = new TerrainLayer();
                        terrainLayers[3].diffuseTexture = runTime.layer4Albedo;
                        terrainLayers[3].tileSize = new Vector2(terrainSizeX, terrainSizeY);
                        terrainLayers[3].tileOffset = new Vector2(0, 0);
                        terrainLayers[3].metallic = 0;
                        terrainLayers[3].smoothness = 0;

                        if (runTime.normalsAvailable)
                        {
                            using (WWW www = new WWW("file:///" + normalNames[imageIndex]))
                            {
                                yield return www;

                                if (!string.IsNullOrEmpty(www.error))
                                    Debug.Log(www.error);

                                terrainLayers[3].normalMapTexture = www.texture;
                            }
                        }

                        stitchingTerrainsList[imageIndex].terrainData.terrainLayers = terrainLayers;
#else
                        splatPrototypes[0].texture = runTime.layer1Albedo;
                        splatPrototypes[0].normalMap = runTime.layer1Normal;
                        splatPrototypes[0].tileSize = new Vector2(runTime.tiling1, runTime.tiling1);
                        splatPrototypes[0].tileOffset = new Vector2(0, 0);
                        splatPrototypes[0].metallic = 0;
                        splatPrototypes[0].smoothness = 0;

                        splatPrototypes[1].texture = runTime.layer2Albedo;
                        splatPrototypes[1].normalMap = runTime.layer2Normal;
                        splatPrototypes[1].tileSize = new Vector2(runTime.tiling2, runTime.tiling2);
                        splatPrototypes[1].tileOffset = new Vector2(0, 0);
                        splatPrototypes[1].metallic = 0;
                        splatPrototypes[1].smoothness = 0;

                        splatPrototypes[2].texture = runTime.layer3Albedo;
                        splatPrototypes[2].normalMap = runTime.layer3Normal;
                        splatPrototypes[2].tileSize = new Vector2(runTime.tiling3, runTime.tiling3);
                        splatPrototypes[2].tileOffset = new Vector2(0, 0);
                        splatPrototypes[2].metallic = 0;
                        splatPrototypes[2].smoothness = 0;

                        splatPrototypes[3].texture = runTime.layer4Albedo;
                        splatPrototypes[3].tileSize = new Vector2(terrainSizeX, terrainSizeY);
                        splatPrototypes[3].tileOffset = new Vector2(0, 0);
                        splatPrototypes[3].metallic = 0;
                        splatPrototypes[3].smoothness = 0;

                        if (RuntimeOffline.normalsAvailable)
                        {
                            using (WWW www = new WWW("file:///" + normalNames[imageIndex]))
                            {
                                yield return www;

                                if (!string.IsNullOrEmpty(www.error))
                                    Debug.Log(www.error);

                                splatPrototypes[3].normalMap = www.texture;
                            }
                        }

                        stitchingTerrainsList[imageIndex].terrainData.splatPrototypes = splatPrototypes;
#endif
                    }

                    uwr = UnityWebRequestTexture.GetTexture("file:///" + imageryNames[imageIndex]);
                    yield return uwr.SendWebRequest();

                    if (uwr.isNetworkError || uwr.isHttpError)
                        print(uwr.error);

                    while (!uwr.isDone)
                    {
                        yield return null;
                    }

                    splatmap = ((DownloadHandlerTexture)uwr.downloadHandler).texture;
                    Color32[] pixels = splatmap.GetPixels32();

                    alphamapResolution = splatmap.width;
                    stitchingTerrainsList[imageIndex].terrainData.alphamapResolution = alphamapResolution;
                    counter = 0;
                    smData = new float[alphamapResolution, alphamapResolution, stitchingTerrainsList[imageIndex].terrainData.alphamapLayers];

                    for (int y = 0; y < alphamapResolution; y++)
                    {
                        for (int z = 0; z < alphamapResolution; z++)
                        {
                            smData[y, z, 0] = pixels[counter].r / 255f;
                            smData[y, z, 1] = pixels[counter].g / 255f;
                            smData[y, z, 2] = pixels[counter].b / 255f;
                            smData[y, z, 3] = pixels[counter].a / 255f;
                            //smData[y, z, 3] = 1;

                            counter++;
                        }
                    }

                    stitchingTerrainsList[imageIndex].terrainData.SetAlphamaps(0, 0, smData);
                    stitchingTerrainsList[imageIndex].Flush();

                    yield return new WaitForSeconds(runTime.imageryDelay);

                    if (i == runTime.activeTilesGrid - 1)
                        FinalizeTiles(dir);
                }
            }
            else
            {
                for (int i = 0; i < runTime.activeTilesGrid; i++)
                {
                    //int index = 0;
                    int imageIndex = 0;

                    //if (dir.Equals("North"))
                    //    index = ((northIndices[i] + 1) * requestStep) - 1;
                    //else if (dir.Equals("South"))
                    //    index = ((southIndices[i] + 1) * requestStep) - 1;
                    //else if (dir.Equals("East"))
                    //    index = ((eastIndices[i] + 1) * requestStep) - 1;
                    //else if (dir.Equals("West"))
                    //    index = ((westIndices[i] + 1) * requestStep) - 1;

                    if (dir.Equals("North"))
                        imageIndex = northIndices[i];
                    else if (dir.Equals("South"))
                        imageIndex = southIndices[i];
                    else if (dir.Equals("East"))
                        imageIndex = eastIndices[i];
                    else if (dir.Equals("West"))
                        imageIndex = westIndices[i];

                    uwr = UnityWebRequestTexture.GetTexture("file:///" + imageryNames[imageIndex], true);

                    //uwra = uwr.SendWebRequest();
                    //yield return uwra;
                    yield return uwr.SendWebRequest();

                    if (uwr.isNetworkError || uwr.isHttpError)
                        print(uwr.error);

                    while (!uwr.isDone)
                    {
                        yield return null;
                    }

#if UNITY_2018_3_OR_NEWER
                    TerrainLayer[] terrainLayers = new TerrainLayer[1];
                    terrainLayers[0] = new TerrainLayer();
                    terrainLayers[0].diffuseTexture = ((DownloadHandlerTexture)uwr.downloadHandler).texture;
                    terrainLayers[0].tileSize = new Vector2(terrainSizeX, terrainSizeY);
                    terrainLayers[0].tileOffset = new Vector2(0, 0);
                    terrainLayers[0].diffuseTexture.wrapMode = TextureWrapMode.Clamp;
                    terrainLayers[0].diffuseTexture.name = System.IO.Path.GetFileName(imageryNames[imageIndex]);
                    terrainLayers[0].name = System.IO.Path.GetFileName(imageryNames[imageIndex]);

                    stitchingTerrainsList[imageIndex].terrainData.terrainLayers = terrainLayers;
#else
                    sp[0].texture = ((DownloadHandlerTexture)uwr.downloadHandler).texture;
                    sp[0].tileSize = new Vector2(terrainSizeX, terrainSizeY);
                    sp[0].tileOffset = new Vector2(0, 0);
                    sp[0].texture.wrapMode = TextureWrapMode.Clamp;
                    sp[0].texture.name = Path.GetFileName(imageryNames[imageIndex]);
                    //sp[0].texture.mipMapBias = 0.2f;
                    //print(sp[0].texture.mipmapCount);
                    stitchingTerrainsList[imageIndex].terrainData.splatPrototypes = sp;
#endif

                    yield return new WaitForSeconds(runTime.imageryDelay);

                    if (i == runTime.activeTilesGrid - 1)
                        FinalizeTiles(dir);
                }
            }
        }

        public static IEnumerator FillImageFAST(int i, string dir)
        {
            uwr = UnityWebRequestTexture.GetTexture("file:///" + imageryNames[i], true);
            yield return uwr.SendWebRequest();

            if (uwr.isNetworkError || uwr.isHttpError)
                print(uwr.error);

            while (!uwr.isDone)
            {
                yield return null;
            }

#if UNITY_2018_3_OR_NEWER
            TerrainLayer[] terrainLayers = new TerrainLayer[1];
            terrainLayers[0] = new TerrainLayer();
            terrainLayers[0].diffuseTexture = ((DownloadHandlerTexture)uwr.downloadHandler).texture;
            terrainLayers[0].tileSize = new Vector2(terrainSizeX, terrainSizeY);
            terrainLayers[0].tileOffset = new Vector2(0, 0);
            terrainLayers[0].diffuseTexture.wrapMode = TextureWrapMode.Clamp;
            terrainLayers[0].diffuseTexture.name = System.IO.Path.GetFileName(imageryNames[i]);
            terrainLayers[0].name = System.IO.Path.GetFileName(imageryNames[i]);

            stitchingTerrainsList[i].terrainData.terrainLayers = terrainLayers;
#else
            sp[0].texture = ((DownloadHandlerTexture)uwr.downloadHandler).texture;
            sp[0].tileSize = new Vector2(terrainSizeX, terrainSizeY);
            sp[0].tileOffset = new Vector2(0, 0);
            sp[0].texture.wrapMode = TextureWrapMode.Clamp;
            sp[0].texture.name = Path.GetFileName(imageryNames[i]);
            //sp[0].texture.mipMapBias = 0.2f;
            //print(sp[0].texture.mipmapCount);
            stitchingTerrainsList[i].terrainData.splatPrototypes = sp;
#endif

            //using (WWW www = new WWW("file:///" + imageryNames[i]))
            //{
            //    yield return www;

            //    if (!string.IsNullOrEmpty(www.error))
            //        Debug.Log(www.error);

            //    //www.LoadImageIntoTexture(images[i]);

            //    //sp[0].texture = www.texture;
            //    sp[0].texture = www.textureNonReadable;
            //    sp[0].tileSize = new Vector2(terrainSizeX, terrainSizeY);
            //    sp[0].tileOffset = new Vector2(0, 0);
            //    sp[0].texture.wrapMode = TextureWrapMode.Clamp;
            //    //sp[0].texture.Apply();
            //    stitchingTerrainsList[i].terrainData.splatPrototypes = sp;
            //}

            downloadedImages++;

            if (downloadedImages == runTime.activeTilesGrid)
            {
                FinalizeTiles(dir);
                downloadedImages = 0;
            }
        }

        public static void FinalizeTiles (string dir)
        {
            if(runTime.showTileOnFinish)
                RevealTiles(dir);

            if (dir.Equals("North"))
            {
                InfiniteTerrainOffline.northTerrains.Clear();
                InfiniteTerrainOffline.inProgressNorth = false;
            }
            else if (dir.Equals("South"))
            {
                InfiniteTerrainOffline.southTerrains.Clear();
                InfiniteTerrainOffline.inProgressSouth = false;
            }
            else if (dir.Equals("East"))
            {
                InfiniteTerrainOffline.eastTerrains.Clear();
                InfiniteTerrainOffline.inProgressEast = false;
            }
            else if (dir.Equals("West"))
            {
                InfiniteTerrainOffline.westTerrains.Clear();
                InfiniteTerrainOffline.inProgressWest = false;
            }

            //InfiniteTerrainOffline.northTerrains.Clear();
            //InfiniteTerrainOffline.inProgressNorth = false;
            //InfiniteTerrainOffline.southTerrains.Clear();
            //InfiniteTerrainOffline.inProgressSouth = false;
            //InfiniteTerrainOffline.eastTerrains.Clear();
            //InfiniteTerrainOffline.inProgressEast = false;
            //InfiniteTerrainOffline.westTerrains.Clear();
            //InfiniteTerrainOffline.inProgressWest = false;

            if (!string.IsNullOrEmpty(InfiniteTerrainOffline.excludedTerrainNORTH))
                InfiniteTerrainOffline.excludedTerrainNORTH = "";

            if (!string.IsNullOrEmpty(InfiniteTerrainOffline.excludedTerrainSOUTH))
                InfiniteTerrainOffline.excludedTerrainSOUTH = "";

            if (!string.IsNullOrEmpty(InfiniteTerrainOffline.excludedTerrainEAST))
                InfiniteTerrainOffline.excludedTerrainEAST = "";

            if (!string.IsNullOrEmpty(InfiniteTerrainOffline.excludedTerrainWEST))
                InfiniteTerrainOffline.excludedTerrainWEST = "";
        }

        private static void RevealTiles (string dir)
        {
            List<string> excludedNamesList = new List<string>
            {
                InfiniteTerrainOffline.excludedTerrainNORTH,
                InfiniteTerrainOffline.excludedTerrainSOUTH,
                InfiniteTerrainOffline.excludedTerrainEAST,
                InfiniteTerrainOffline.excludedTerrainWEST
            };

            string currentTerrainName = "";

            for (int i = 0; i < stitchingTerrainsList.Count; i++)
            {
                currentTerrainName = stitchingTerrainsList[i].name;

                if (!excludedNamesList.Contains(currentTerrainName))
                {
                    if (dir.Equals("North"))
                    {
                        if (InfiniteTerrainOffline.northTerrainsNeighbor.Contains(currentTerrainName))
                        {
                            stitchingTerrainsList[i].transform.localPosition = new Vector3
                            (
                                stitchingTerrainsList[i].transform.localPosition.x,
                                0,
                                stitchingTerrainsList[i].transform.localPosition.z
                            );

                            stitchingTerrainsList[i].drawHeightmap = true;
                            runTime.processedTiles.Add(stitchingTerrainsList[i]);
                        }
                    }
                    else if (dir.Equals("South"))
                    {
                        if (InfiniteTerrainOffline.southTerrainsNeighbor.Contains(currentTerrainName))
                        {
                            stitchingTerrainsList[i].transform.localPosition = new Vector3
                            (
                                stitchingTerrainsList[i].transform.localPosition.x,
                                0,
                                stitchingTerrainsList[i].transform.localPosition.z
                            );

                            stitchingTerrainsList[i].drawHeightmap = true;
                            runTime.processedTiles.Add(stitchingTerrainsList[i]);
                        }
                    }
                    else if (dir.Equals("East"))
                    {
                        if (InfiniteTerrainOffline.eastTerrainsNeighbor.Contains(currentTerrainName))
                        {
                            stitchingTerrainsList[i].transform.localPosition = new Vector3
                            (
                                stitchingTerrainsList[i].transform.localPosition.x,
                                0,
                                stitchingTerrainsList[i].transform.localPosition.z
                            );

                            stitchingTerrainsList[i].drawHeightmap = true;
                            runTime.processedTiles.Add(stitchingTerrainsList[i]);
                        }
                    }
                    else if (dir.Equals("West"))
                    {
                        if (InfiniteTerrainOffline.westTerrainsNeighbor.Contains(currentTerrainName))
                        {
                            stitchingTerrainsList[i].transform.localPosition = new Vector3
                            (
                                stitchingTerrainsList[i].transform.localPosition.x,
                                0,
                                stitchingTerrainsList[i].transform.localPosition.z
                            );

                            stitchingTerrainsList[i].drawHeightmap = true;
                            runTime.processedTiles.Add(stitchingTerrainsList[i]);
                        }
                    }
                }
            }

            runTime.SendProcessedTiles(runTime.processedTiles);
        }

        // Offset terrains' Z position to avoid origin drift due to Equator diatcance in Latitudes
        private static Utils.Vector3d GetAbsoluteWorldPosition (double top, double left, double bottom, double right, Vector3 terrainSize, bool isSingleTerrain)
        {
            double lat = (top + bottom) / 2d;
            double lon = (left + right) / 2d;
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
            Utils.Vector2d _initialWorldPositionXZ = AreaBounds.GetWorldPositionFromTile(_latlonDeltaNormalized[0], _latlonDeltaNormalized[1], _worldSizeY, _worldSizeX);
            Utils.Vector3d _initialWorldPosition = Utils.Vector3d.zero;

            if (isSingleTerrain)
                _initialWorldPosition = new Utils.Vector3d(_initialWorldPositionXZ.x, 0, -_initialWorldPositionXZ.y);
            else
                _initialWorldPosition = new Utils.Vector3d(_initialWorldPositionXZ.x + _worldSizeY / 2, 0, -_initialWorldPositionXZ.y + _worldSizeX / 2);

            return _initialWorldPosition;
        }

        private static void GenerateNewTerrainObject ()
        {
            SetData();
            CreateTerrainObject();

            if (runTime.activeTilesGrid == 1)
            {
                terrain = currentTerrain;
                initialTerrainWidth = terrainSizeNewX;

                if(RuntimeOffline.isGeoReferenced)
                    terrain.transform.position = (Vector3)GetAbsoluteWorldPosition(RuntimeOffline.top, RuntimeOffline.left, RuntimeOffline.bottom, RuntimeOffline.right, new Vector3(RuntimeOffline.exaggeratedWorldSize * 1000f, 100, RuntimeOffline.exaggeratedWorldSize * 1000f), true);
            }
            else
            {
                splittedTerrains = terrainsParent;
                splittedTerrains.AddComponent<InfiniteTerrainOffline>();
                //splittedTerrains.GetComponent<InfiniteTerrainOffline>().Initialize();
                CheckTerrainChunks();
                initialTerrainWidth = terrainSizeNewX / (int)Mathf.Sqrt((float)terrainChunks);

                if (RuntimeOffline.isGeoReferenced)
                    splittedTerrains.transform.position = (Vector3)GetAbsoluteWorldPosition(RuntimeOffline.top, RuntimeOffline.left, RuntimeOffline.bottom, RuntimeOffline.right, new Vector3(RuntimeOffline.exaggeratedWorldSize * 1000f, 100, RuntimeOffline.exaggeratedWorldSize * 1000f), false);
            }

            sceneIsInitialized = true;
            UnityEngine.Debug.Log("Scene Is Initialized");

            AddTerrainsToFloatingOrigin();
        }

        private static void SetData ()
        {
            terrainsLong = runTime.activeTilesGrid;
            terrainsWide = runTime.activeTilesGrid;

            oldWidth = terrainSizeNewX;
            oldHeight = terrainSizeNewY;
            oldLength = terrainSizeNewZ;

            newWidth = oldWidth / terrainsWide;
            newLength = oldLength / terrainsLong;

            xPos = (terrainSizeNewX / 2f) * -1f;
            yPos = 0f;
            zPos = (terrainSizeNewZ / 2f) * -1f;

            if(RuntimeOffline.tiledElevation)
                newHeightmapResolution = m_Width;
            else
                newHeightmapResolution = ((heightmapResolutionSplit - 1) / RuntimeOffline.dataBaseGrid);
        }

        private static void CreateTerrainObject ()
        {
            arrayPos = 0;
            currentRow = 0;
            tempParnet = new GameObject("Temp Parent");

            terrainsParent = new GameObject ("Terrains" +"  ---  "+ runTime.activeTilesGrid + "x" + runTime.activeTilesGrid);
            terrainNames = new string[(int)Mathf.Pow(runTime.activeTilesGrid, 2)];
            currentRow = runTime.activeTilesGrid;

            for(int y = 0; y < terrainsLong ; y++)
            {
                for(int x = 0; x < terrainsWide; x++)
                {
                    terrainGameObject = new GameObject("Terrain_" + (currentRow) + "-" + (x + 1));
                    terrainGameObject.AddComponent<Terrain>();

                    if (runTime.activeTilesGrid > 1)
                    {
                        terrainNames[arrayPos] = terrainGameObject.name;
                        terrainGameObject.transform.parent = tempParnet.transform;
                    }

                    data = new TerrainData();
                    data.heightmapResolution = newHeightmapResolution;
                    data.size = new Vector3(newWidth, oldHeight, newLength);
                    data.name = currentRow + "-" + (x + 1);

                    currentTerrain = terrainGameObject.GetComponent<Terrain>();
                    currentTerrain.terrainData = data;
                    currentTerrain.heightmapPixelError = runTime.heightmapPixelError;
                    currentTerrain.basemapDistance = terrainSizeNewX * 4f;

#if !UNITY_2019_1_OR_NEWER
                    currentTerrain.materialType = Terrain.MaterialType.Custom;
#endif

                    if (runTime.terrainMaterial != null)
                    {
                        //runTime.terrainMaterial.SetFloat("_MeshDistance", runTime.terrainDistance);
                        currentTerrain.materialTemplate = runTime.terrainMaterial;
                    }
                    else
                        currentTerrain.materialTemplate = MaterialManager.GetTerrainMaterial();

                    if (runTime.showTileOnFinish)
                        currentTerrain.drawHeightmap = false;

#if UNITY_2018_3_OR_NEWER
                    if (runTime.drawInstanced)
                        currentTerrain.drawInstanced = true;
                    else
                        currentTerrain.drawInstanced = false;

                    currentTerrain.groupingID = 0;
                    currentTerrain.allowAutoConnect = true;
#endif

                    terrainGameObject.AddComponent<TerrainCollider>();
                    terrainGameObject.GetComponent<TerrainCollider>().terrainData = data;

                    if(!runTime.terrainColliders)
                        terrainGameObject.GetComponent<TerrainCollider>().enabled = false;

                    terrainGameObject.transform.position = new Vector3(x * newWidth + xPos, yPos, y * newLength + zPos);

                    terrainGameObject.layer = 8;

                    arrayPos++;
                }
                currentRow--;
            }

            if (runTime.activeTilesGrid > 1)
            {
//                terrainNames = LogicalComparer(terrainNames);
//
//                for(int i = 0; i < terrainNames.Length; i++)
//                {
//                    tempParnet.transform.FindChild(terrainNames[i]).transform.parent = terrainsParent.transform;
//                    terrainsParent.transform.FindChild(terrainNames[i]).name = (i + 1).ToString() +" "+ terrainNames[i];
//                }

                if(!runTime.spiralGeneration)
                {
                    terrainNames = LogicalComparer(terrainNames);

                    for(int i = 0; i < terrainNames.Length; i++)
                    {
                        tempParnet.transform.Find(terrainNames[i]).transform.parent = terrainsParent.transform;
                        terrainsParent.transform.Find(terrainNames[i]).name = (i + 1).ToString() +" "+ terrainNames[i];
                    }
                }
                else
                {
                    for(int i = 0; i < terrainNames.Length; i++)
                    {
                        nameStr = terrainNames[spiralIndex[i]];
                        tempParnet.transform.Find(nameStr).transform.parent = terrainsParent.transform;
                        spiralCell.Add
                        (
                            new Vector2
                            (
                                int.Parse(nameStr.Remove(nameStr.LastIndexOf("-")).Replace("Terrain_", "")) - 0,
                                int.Parse(nameStr.Substring(nameStr.LastIndexOf("-") + 1)) - 0
                            )
                        );
                    }
                }

                DestroyImmediate(tempParnet);
            }
        }

        private static void AddTerrainsToFloatingOrigin ()
        {
            if(Camera.main.GetComponent<FloatingOriginAdvanced>() != null)
            {
                floatingOriginAdvanced = Camera.main.GetComponent<FloatingOriginAdvanced>();
                floatingOriginAdvanced.CollectObjectsOnce();
            }
        }

        public static string[] LogicalComparer (string filePath, string fileType)
        {
            names = Directory.GetFiles(filePath, "*" + fileType, SearchOption.AllDirectories);
            ns = new ns.NumericComparer();
            Array.Sort(names, ns);

            return names;
        }

        public static string[] LogicalComparer (string[] names)
        {
            ns = new ns.NumericComparer();
            Array.Sort(names, ns);

            return names;
        }

        private static void CheckTerrainChunks ()
        {
            if(splittedTerrains.transform.childCount > 0)
            {
                counter = 0;

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

        public static void FinalizeHeights ()
        {
//            if(geoDataExtensionElevation.Equals("raw"))
//                Timing.RunCoroutine(LoadTerrainHeightsFromRAW());
//            else if(geoDataExtensionElevation.Equals("tif"))
//                Timing.RunCoroutine(LoadTerrainHeightsFromTIFF());
//            else if(geoDataExtensionElevation.Equals("asc"))
//                Timing.RunCoroutine(LoadTerrainHeightsFromASCII());

            Timing.RunCoroutine(LoadTerrainHeights());

            //if (generatedTerrainsCount == terrainChunks)
            //{
            //    if(splittedTerrains)
            //        ManageNeighborings("");
            //}
        }

        public static void GetElevationFileInfo ()
        {
            if(geoDataExtensionElevation.Equals("raw"))
            {
                PickRawDefaults(elevationNames[0]);

                if(RuntimeOffline.tiledElevation)
                    RuntimeOffline.heightmapResolution = m_Width * RuntimeOffline.dataBaseGrid;
                else
                    RuntimeOffline.heightmapResolution = m_Width;
            }
            else if(geoDataExtensionElevation.Equals("tif"))
            {
                GetTIFFInfo();
                RuntimeOffline.heightmapResolution = tiffWidth;
            }
            else if(geoDataExtensionElevation.Equals("asc"))
            {
                GetASCIIInfo();
                RuntimeOffline.heightmapResolution = nRows;
            }
        }

        public static void ApplyOfflineTerrain ()
        {
            SetupDownloaderElevationGeoServer();

            if(geoDataExtensionElevation.Equals("raw"))
            {
                runTime.TerrainFromRAW();

//                if(RuntimeOffline.tiledElevation)
//                {
//                    rawData = new List<float[,]>();
//
//                    for(int i = 0; i < TerraLand.TerraLandRuntimeOffline.totalImagesDataBase; i++)
//                        runTime.TerrainFromRAW(i);
//                }
//                else
//                    runTime.TerrainFromRAW();
            }
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

                    tileHeight = inputImage.GetField(TiffTag.TILELENGTH)[0].ToInt();
                    tileWidth = inputImage.GetField(TiffTag.TILEWIDTH)[0].ToInt();

                    if (buffer == null || buffer.Length == 0)
                        buffer = new byte[tileHeight * tileWidth * 4];

                    if (fBuffer == null || fBuffer.Length == 0)
                        fBuffer = new float[tileHeight, tileWidth];

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

            terrainEverestDiffer = everestPeak / highestPoint;

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

        public static float[,] TiffDataDynamic (string fileName)
        {
            try
            {
                using (Tiff inputImage = Tiff.Open(fileName, "r"))
                {
                    tiffWidth = inputImage.GetField(TiffTag.IMAGEWIDTH)[0].ToInt();
                    tiffLength = inputImage.GetField(TiffTag.IMAGELENGTH)[0].ToInt();
                    tiffData = new float[tiffLength, tiffWidth];
                    tiffDataASCII = new float[tiffLength, tiffWidth];

                    tileHeight = inputImage.GetField(TiffTag.TILELENGTH)[0].ToInt();
                    tileWidth = inputImage.GetField(TiffTag.TILEWIDTH)[0].ToInt();

                    if (buffer == null || buffer.Length == 0)
                        buffer = new byte[tileHeight * tileWidth * 4];

                    if (fBuffer == null || fBuffer.Length == 0)
                        fBuffer = new float[tileHeight, tileWidth];

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
            {
                highestPoint = tiffDataASCII.Cast<float>().Max();
                lowestPoint = tiffDataASCII.Cast<float>().Min();
            }

            downloadedHeightmapIndex++;

            terrainEverestDiffer = everestPeak / highestPoint;

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

            return tiffData;
        }

        public static void RawData (string fileName, int index)
        {
            if(index == 0)
                PickRawDefaults(fileName);

            //byte[] buffer;

            using (BinaryReader reader = new BinaryReader(File.Open(fileName, FileMode.Open, FileAccess.Read)))
            {
                buffer = reader.ReadBytes((m_Width * m_Height) * (int)m_Depth);
                reader.Close();
            }

            rawData.Add(new float[m_Width, m_Height]);

            if (m_Depth == Depth.Bit16)
            {
                for (int y = 0; y < m_Width; y++)
                {
                    for (int x = 0; x < m_Height; x++)
                    {
                        num2 = Clamp(x, 0, m_Width - 1) + Clamp(y, 0, m_Height - 1) * m_Width;

                        if (m_ByteOrder == ByteOrder.Mac == BitConverter.IsLittleEndian)
                        {
                            b = buffer[num2 * 2];
                            buffer[num2 * 2] = buffer[num2 * 2 + 1];
                            buffer[num2 * 2 + 1] = b;
                        }

                        num3 = BitConverter.ToUInt16(buffer, num2 * 2);
                        num4 = (float)num3 * num;
                        currentHeight = num4;

                        rawData[index][(m_Width - 1) - y, x] = num4;
                    }
                }
            }
            else
            {
                for (int y = 0; y < m_Width; y++)
                {
                    for (int x = 0; x < m_Height; x++)
                    {
                        indx = Clamp(x, 0, m_Width - 1) + (Clamp(y, 0, m_Height - 1) * m_Width);
                        num14 = buffer[indx];
                        num15 = num14 * num10;
                        currentHeight = num15;

                        rawData[index][(m_Width - 1) - y, x] = num15;
                    }
                }
            }

            //float lowestPointNormalized = 0;

            if(index == 0)
            {
                highestPoint = -Mathf.Infinity;
                //lowestPoint = Mathf.Infinity;
                //lowestPointNormalized = Mathf.Infinity;
            }

            if(highestPoint < rawData[index].Cast<float>().Max() * everestPeak)
            {
                highestPoint = rawData[index].Cast<float>().Max() * everestPeak;
                terrainEverestDiffer = everestPeak / highestPoint;
            }

//            if(lowestPoint > rawData[index].Cast<float>().Min() * everestPeak)
//                lowestPoint = rawData[index].Cast<float>().Min() * everestPeak;
//
//            if(lowestPointNormalized > rawData[index].Cast<float>().Min())
//                lowestPointNormalized = rawData[index].Cast<float>().Min();

//            if (m_Depth == Depth.Bit16)
//            {
//                for (int y = 0; y < m_Width; y++)
//                {
//                    for (int x = 0; x < m_Height; x++)
//                    {
//                        if(lowestPointNormalized >= 0)
//                            rawData[index][(m_Width - 1) - y, x] -= lowestPointNormalized;
//                        else
//                            rawData[index][(m_Width - 1) - y, x] += Mathf.Abs(lowestPointNormalized);
//
////                        // Check Terrain Corners
////                        // Top Row
////                        if(y == 0)
////                            topCorner.Add(currentHeight);
////
////                        // Bottom Row
////                        else if(y == m_Width - 1)
////                            bottomCorner.Add(currentHeight);
////
////                        // Left Column
////                        if(x == 0)
////                            leftCorner.Add(currentHeight);
////
////                        // Right Column
////                        else if(x == m_Height - 1)
////                            rightCorner.Add(currentHeight);
//                    }
//                }
//            }
//            else
//            {
//                for (int y = 0; y < m_Width; y++)
//                {
//                    for (int x = 0; x < m_Height; x++)
//                    {
//                        rawData[index][(m_Width - 1) - y, x] -= lowestPointNormalized;
//
////                        // Check Terrain Corners
////                        // Top Row
////                        if(y == 0)
////                            topCorner.Add(currentHeight);
////
////                        // Bottom Row
////                        else if(y == m_Width - 1)
////                            bottomCorner.Add(currentHeight);
////
////                        // Left Column
////                        if(x == 0)
////                            leftCorner.Add(currentHeight);
////
////                        // Right Column
////                        else if(x == m_Height - 1)
////                            rightCorner.Add(currentHeight);
//                    }
//                }
//            }

            //CheckCornersRAW(index);

            //if(index == totalImages - 1)
                //runTime.FinalizeTerrainFromRAW();
        }

        public static void RawDataDynamic(int i)
        {
            using (BinaryReader reader = new BinaryReader(File.Open(elevationNames[i], FileMode.Open, FileAccess.Read)))
            {
                buffer = reader.ReadBytes((m_Width * m_Height) * (int)m_Depth);
                reader.Close();
            }

            if (m_Depth == Depth.Bit16)
            {
                for (int y = 0; y < m_Width; y++)
                {
                    for (int x = 0; x < m_Height; x++)
                    {
                        num2 = Clamp(x, 0, m_Width - 1) + Clamp(y, 0, m_Height - 1) * m_Width;

                        if (m_ByteOrder == ByteOrder.Mac == BitConverter.IsLittleEndian)
                        {
                            b = buffer[num2 * 2];
                            buffer[num2 * 2] = buffer[num2 * 2 + 1];
                            buffer[num2 * 2 + 1] = b;
                        }

                        num3 = BitConverter.ToUInt16(buffer, num2 * 2);
                        num4 = (float)num3 * num;
                        currentHeight = num4;

                        rawData[i][(m_Width - 1) - y, x] = num4;
                    }
                }
            }
            else
            {
                for (int y = 0; y < m_Width; y++)
                {
                    for (int x = 0; x < m_Height; x++)
                    {
                        indx = Clamp(x, 0, m_Width - 1) + (Clamp(y, 0, m_Height - 1) * m_Width);
                        num14 = buffer[indx];
                        num15 = num14 * num10;
                        currentHeight = num15;

                        rawData[i][(m_Width - 1) - y, x] = num15;
                    }
                }
            }
        }

        public static float[,] RawDataDynamic (string fileName)
        {
            using (BinaryReader reader = new BinaryReader(File.Open(fileName, FileMode.Open, FileAccess.Read)))
            {
                buffer = reader.ReadBytes((m_Width * m_Height) * (int)m_Depth);
                rawDataFloat = new float[m_Height, m_Width];

                reader.Close();
            }

            if (m_Depth == Depth.Bit16)
            {
                for (int y = 0; y < m_Width; y++)
                {
                    for (int x = 0; x < m_Height; x++)
                    {
                        num2 = Clamp(x, 0, m_Width - 1) + Clamp(y, 0, m_Height - 1) * m_Width;

                        if (m_ByteOrder == ByteOrder.Mac == BitConverter.IsLittleEndian)
                        {
                            b = buffer[num2 * 2];
                            buffer[num2 * 2] = buffer[num2 * 2 + 1];
                            buffer[num2 * 2 + 1] = b;
                        }

                        num3 = BitConverter.ToUInt16(buffer, num2 * 2);
                        num4 = (float)num3 * num;
                        currentHeight = num4;

                        rawDataFloat[(m_Width - 1) - y, x] = num4;
                    }
                }
            }
            else
            {
                for (int y = 0; y < m_Width; y++)
                {
                    for (int x = 0; x < m_Height; x++)
                    {
                        indx = Clamp(x, 0, m_Width - 1) + (Clamp(y, 0, m_Height - 1) * m_Width);
                        num14 = buffer[indx];
                        num15 = num14 * num10;
                        currentHeight = num15;

                        rawDataFloat[(m_Width - 1) - y, x] = num15;
                    }
                }
            }

//            if(index == 0)
//                highestPoint = -Mathf.Infinity;
//
//            if(highestPoint < rawData[index].Cast<float>().Max() * everestPeak)
//            {
//                highestPoint = data.Cast<float>().Max() * everestPeak;
//                terrainEverestDiffer = everestPeak / highestPoint;
//            }

            return rawDataFloat;
        }

        private static void PickRawDefaults (string fileName)
        {
            stream = File.Open(fileName, FileMode.Open, FileAccess.Read);
            length = (int)stream.Length;
            stream.Close();

            m_Depth = Depth.Bit16;
            num2 = length / (int)m_Depth;
            numSqr = Mathf.RoundToInt(Mathf.Sqrt((float)num2));

            if (((numSqr * numSqr) * (int)m_Depth) == length)
            {
                m_Width = numSqr;
                m_Height = numSqr;

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
                numSqr = (int)Math.Round(Math.Sqrt((float)num2));

                if (((numSqr * numSqr) * (int)m_Depth) == length)
                {
                    m_Width = numSqr;
                    m_Height = numSqr;

                    heightmapResX = Mathf.FloorToInt((float)Mathf.ClosestPowerOfTwo(m_Width) / (float)splitSizeFinal);
                    heightmapResY = Mathf.FloorToInt((float)Mathf.ClosestPowerOfTwo(m_Height) / (float)splitSizeFinal);
                    heightmapResXAll = m_Width;
                    heightmapResYAll = m_Height;

                    return;
                }

                m_Depth = Depth.Bit16;
            }
        }

        private static float Clamp (float value, float min, float max)
        {
            if (value < min)
                value = min;
            else if (value > max)
                value = max;

            return value;
        }

        private static int Clamp (int value, int min, int max)
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
            line1 = sr.ReadLine().Replace(',', '.').Split(new char[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
            nCols = (Convert.ToInt32(line1[1]));
            //nrows
            line2 = sr.ReadLine().Replace(',', '.').Split(new char[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
            nRows = (Convert.ToInt32(line2[1]));
            //xllcorner
            line3 = sr.ReadLine().Replace(',', '.').Split(new char[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
            xllCorner = Convert.ToDouble((line3[1]), CultureInfo.InvariantCulture);
            //yllcorner
            line4 = sr.ReadLine().Replace(',', '.').Split(new char[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
            yllCorner = Convert.ToDouble((line4[1]), CultureInfo.InvariantCulture);
            //cellsize
            line5 = sr.ReadLine().Replace(',', '.').Split(new char[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
            cellSizeASCII = Convert.ToDouble((line5[1]), CultureInfo.InvariantCulture);
            //nodata
            line6 = sr.ReadLine().Replace(',', '.').Split(new char[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
            noData = Convert.ToDouble((line6[1]), CultureInfo.InvariantCulture);

            if(asciiData == null || asciiData.Length == 0)
                asciiData = new float[nCols, nRows];

            heightmapResX = Mathf.FloorToInt((float)Mathf.ClosestPowerOfTwo(nRows) / (float)splitSizeFinal);
            heightmapResY = Mathf.FloorToInt((float)Mathf.ClosestPowerOfTwo(nCols) / (float)splitSizeFinal);
            heightmapResXAll = nRows;
            heightmapResYAll = nCols;

            for (int y = 0; y < nRows; y++)
            {
                line = sr.ReadLine().Split(new char[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);

                for (int x = 0; x < nCols; x++)
                {
                    currentHeight = float.Parse(line[x].Replace(',', '.'));
                    asciiData[(nRows - 1) - y, x] = currentHeight / everestPeak;
                }
            }

            sr.Close();

            highestPoint = asciiData.Cast<float>().Max() * everestPeak;
            lowestPoint = asciiData.Cast<float>().Min() * everestPeak;
            lowestPointNormalized = asciiData.Cast<float>().Min();
            terrainEverestDiffer = everestPeak / highestPoint;

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

            print(xllCorner + yllCorner + cellSizeASCII + noData);
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

        private static void CheckCornersRAW (int index)
        {
            // Check Top
            if (topCorner.All(o => o == topCorner.First()))
            {
                for (int y = 0; y < m_Width; y++)
                    for (int x = 0; x < m_Height; x++)
                        if(y == 0)
                            rawData[index][(m_Width - 1) - y, x] = rawData[index][(m_Width - 1) - (y + 1), x];
            }

            // Check Bottom
            if (bottomCorner.All(o => o == bottomCorner.First()))
            {
                for (int y = 0; y < m_Width; y++)
                    for (int x = 0; x < m_Height; x++)
                        if(y == m_Width - 1)
                            rawData[index][(m_Width - 1) - y, x] = rawData[index][(m_Width - 1) - (y - 1), x];
            }

            // Check Left
            if (leftCorner.All(o => o == leftCorner.First()))
            {
                for (int y = 0; y < m_Width; y++)
                    for (int x = 0; x < m_Height; x++)
                        if(x == 0)
                            rawData[index][(m_Width - 1) - y, x] = rawData[index][(m_Width - 1) - y, x + 1];
            }

            // Check Right
            if (rightCorner.All(o => o == rightCorner.First()))
            {
                for (int y = 0; y < m_Width; y++)
                    for (int x = 0; x < m_Height; x++)
                        if(x == m_Height - 1)
                            rawData[index][(m_Width - 1) - y, x] = rawData[index][(m_Width - 1) - y, x - 1];
            }
        }

        public static IEnumerator<float> LoadTerrainHeights ()
        {
            counter = 0;
            currentRow = runTime.activeTilesGrid - 1;
            xLength = heightmapResFinalX;
            yLength = heightmapResFinalY;
            xStart = 0;
            yStart = 0;

            if (splittedTerrains)
            {
                for (int i = 0; i < runTime.activeTilesGrid; i++)
                {
                    for (int j = 0; j < runTime.activeTilesGrid; j++)
                    {
                        //if(counter >= taskIndex - RuntimeOffline.concurrentTasks && counter < taskIndex)

                        //croppedTerrains[counter].terrainData.heightmapResolution = heightmapResFinalX;

                        if(RuntimeOffline.tiledElevation)
                        {
                            if(geoDataExtensionElevation.Equals("raw"))
                                Timing.RunCoroutine(FillHeights(croppedTerrains[counter], m_Width, rawData[counter]));
//                            else if(geoDataExtensionElevation.Equals("tif"))
//                                Timing.RunCoroutine(FillHeights(croppedTerrains[counter], tiffWidth, tiffData[counter]));
//                            else if(geoDataExtensionElevation.Equals("asc"))
//                                Timing.RunCoroutine(FillHeights(croppedTerrains[counter], nRows, asciiData[counter]));
                        }
                        else
                        {
                            if(dataSplitted == null || dataSplitted.Length == 0)
                                dataSplitted = new float[heightmapResFinalX, heightmapResFinalY];

                            if(!runTime.spiralGeneration)
                            {
                                xStart = (currentRow * (heightmapResFinalX - 1));
                                yStart = (j * (heightmapResFinalY - 1));
                            }
                            else
                            {
                                xStart = ((runTime.activeTilesGrid - ((int)spiralCell[counter].x  - 1)) - 1) * (heightmapResFinalX - 1);
                                yStart = ((int)spiralCell[counter].y - 1) * (heightmapResFinalY - 1);
                            }

                            for(int x = 0; x < xLength; x++)
                                for(int y = 0; y < yLength; y++)
                                    dataSplitted[x, y] = finalHeights[xStart + x, yStart + y];

                            Timing.RunCoroutine(FillHeights(croppedTerrains[counter], heightmapResFinalX, dataSplitted));
                        }

                        realTerrainWidth = areaSizeLon * 1000.0f / runTime.activeTilesGrid;
                        realTerrainLength = areaSizeLat * 1000.0f / runTime.activeTilesGrid;
                        croppedTerrains[counter].terrainData.size = RealTerrainSize(realTerrainWidth, realTerrainLength, highestPoint);

                        if(runTime.terrainColliders)
                            croppedTerrains[counter].GetComponent<TerrainCollider>().enabled = true;

                        croppedTerrains[counter].Flush();

                        if(!runTime.fastStartBuild)
                        {
                            tileDelay = (runTime.elevationDelay * Mathf.Pow((tileResolution - 1) / runTime.cellSize, 2f)) + (runTime.elevationDelay * 4);
                            yield return Timing.WaitForSeconds(tileDelay);
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

        public static IEnumerator<float> LoadTerrainHeightsNORTH (string dir)
        {
            for(int i = 0; i < runTime.activeTilesGrid; i++)
            {
                if(InfiniteTerrainOffline.northTerrains.Count > 0)
                {
                    int index = northIndices[i];
                    currentTerrain = splittedTerrains.transform.Find(InfiniteTerrainOffline.northTerrains[0]).GetComponent<Terrain>();

                    if(!currentTerrain.name.Equals(InfiniteTerrainOffline.excludedTerrainNORTH))
                    {
                        if(geoDataExtensionElevation.Equals("raw"))
                            Timing.RunCoroutine(FillHeightsNEWS(currentTerrain, tileResolution, rawData[index], dir));
                        else if(geoDataExtensionElevation.Equals("tif"))
                            Timing.RunCoroutine(FillHeightsNEWS(currentTerrain, tileResolution, tiffDataDynamic[index], dir));

                        realTerrainWidth = areaSizeLon * 1000.0f;
                        realTerrainLength = areaSizeLat * 1000.0f;

                        currentTerrain.terrainData.size = RealTerrainSize(realTerrainWidth, realTerrainLength, highestPoint);
                        //currentTerrain.Flush();

                        tileDelay = (runTime.elevationDelay * ((tileResolution - 1) / runTime.cellSize)) + (runTime.elevationDelay * 4);
                        yield return Timing.WaitForSeconds(tileDelay);
                    }

                    InfiniteTerrainOffline.northTerrains.Remove(currentTerrain.name);
                }
            }

            if(InfiniteTerrainOffline.northTerrains.Count == 0)
                //RuntimeOffline.updatingSurfaceNORTH = true;
                ManageNeighborings("North");
        }

        public static IEnumerator<float> LoadTerrainHeightsSOUTH (string dir)
        {
            for(int i = 0; i < runTime.activeTilesGrid; i++)
            {
                if(InfiniteTerrainOffline.southTerrains.Count > 0)
                {
                    index = southIndices[i];
                    currentTerrain = splittedTerrains.transform.Find(InfiniteTerrainOffline.southTerrains[0]).GetComponent<Terrain>();

                    if(!currentTerrain.name.Equals(InfiniteTerrainOffline.excludedTerrainSOUTH))
                    {
                        if(geoDataExtensionElevation.Equals("raw"))
                            Timing.RunCoroutine(FillHeightsNEWS(currentTerrain, tileResolution, rawData[index], dir));
                        else if(geoDataExtensionElevation.Equals("tif"))
                            Timing.RunCoroutine(FillHeightsNEWS(currentTerrain, tileResolution, tiffDataDynamic[index], dir));

                        realTerrainWidth = areaSizeLon * 1000.0f;
                        realTerrainLength = areaSizeLat * 1000.0f;

                        currentTerrain.terrainData.size = RealTerrainSize(realTerrainWidth, realTerrainLength, highestPoint);
                        //currentTerrain.Flush();

                        tileDelay = (runTime.elevationDelay * ((tileResolution - 1) / runTime.cellSize)) + (runTime.elevationDelay * 4);
                        yield return Timing.WaitForSeconds(tileDelay);
                    }

                    InfiniteTerrainOffline.southTerrains.Remove(currentTerrain.name);
                }
            }

            if (InfiniteTerrainOffline.southTerrains.Count == 0)
                //RuntimeOffline.updatingSurfaceSOUTH = true;
                ManageNeighborings("South");
        }

        public static IEnumerator<float> LoadTerrainHeightsEAST (string dir)
        {
            for(int i = 0; i < runTime.activeTilesGrid; i++)
            {
                if(InfiniteTerrainOffline.eastTerrains.Count > 0)
                {
                    index = eastIndices[i];
                    currentTerrain = splittedTerrains.transform.Find(InfiniteTerrainOffline.eastTerrains[0]).GetComponent<Terrain>();

                    if(!currentTerrain.name.Equals(InfiniteTerrainOffline.excludedTerrainEAST))
                    {
                        if(geoDataExtensionElevation.Equals("raw"))
                            Timing.RunCoroutine(FillHeightsNEWS(currentTerrain, tileResolution, rawData[index], dir));
                        else if(geoDataExtensionElevation.Equals("tif"))
                            Timing.RunCoroutine(FillHeightsNEWS(currentTerrain, tileResolution, tiffDataDynamic[index], dir));

                        realTerrainWidth = areaSizeLon * 1000.0f;
                        realTerrainLength = areaSizeLat * 1000.0f;

                        currentTerrain.terrainData.size = RealTerrainSize(realTerrainWidth, realTerrainLength, highestPoint);
                        //currentTerrain.Flush();

                        tileDelay = (runTime.elevationDelay * ((tileResolution - 1) / runTime.cellSize)) + (runTime.elevationDelay * 4);
                        yield return Timing.WaitForSeconds(tileDelay);
                    }

                    InfiniteTerrainOffline.eastTerrains.Remove(currentTerrain.name);
                }
            }

            if (InfiniteTerrainOffline.eastTerrains.Count == 0)
                //RuntimeOffline.updatingSurfaceEAST = true;
                ManageNeighborings("East");
        }

        public static IEnumerator<float> LoadTerrainHeightsWEST (string dir)
        {
            for(int i = 0; i < runTime.activeTilesGrid; i++)
            {
                if(InfiniteTerrainOffline.westTerrains.Count > 0)
                {
                    index = westIndices[i];
                    currentTerrain = splittedTerrains.transform.Find(InfiniteTerrainOffline.westTerrains[0]).GetComponent<Terrain>();

                    if(!currentTerrain.name.Equals(InfiniteTerrainOffline.excludedTerrainWEST))
                    {
                        if(geoDataExtensionElevation.Equals("raw"))
                            Timing.RunCoroutine(FillHeightsNEWS(currentTerrain, tileResolution, rawData[index], dir));
                        else if(geoDataExtensionElevation.Equals("tif"))
                            Timing.RunCoroutine(FillHeightsNEWS(currentTerrain, tileResolution, tiffDataDynamic[index], dir));

                        realTerrainWidth = areaSizeLon * 1000.0f;
                        realTerrainLength = areaSizeLat * 1000.0f;

                        currentTerrain.terrainData.size = RealTerrainSize(realTerrainWidth, realTerrainLength, highestPoint);
                        //currentTerrain.Flush();

                        tileDelay = (runTime.elevationDelay * ((tileResolution - 1) / runTime.cellSize)) + (runTime.elevationDelay * 4);
                        yield return Timing.WaitForSeconds(tileDelay);
                    }

                    InfiniteTerrainOffline.westTerrains.Remove(currentTerrain.name);
                }
            }

            if (InfiniteTerrainOffline.westTerrains.Count == 0)
                //RuntimeOffline.updatingSurfaceWEST = true;
                ManageNeighborings("West");
        }

        private static void StitchNORTH()
        {
            StitchTerrain(croppedTerrains, "North");
        }

        private static void StitchSOUTH()
        {
            StitchTerrain(croppedTerrains, "South");
        }

        private static void StitchEAST()
        {
            StitchTerrain(croppedTerrains, "East");
        }

        private static void StitchWEST()
        {
            StitchTerrain(croppedTerrains, "West");
        }

        private static void StitchTerrain(List<Terrain> terrains, string dir)
        {
            stitchingInProgress = true;

            stitchedTerrainsCount = 0;
            checkLength = runTime.stitchDistance;
            yLength = tileResolution - 1;
            _terrainDict = new Dictionary<int[], Terrain>(new IntArrayComparer());
            _terrains = terrains.ToArray();
            int[] _posTer;

            if (_terrains.Length > 0)
            {
                sizeX = (int)_terrains[0].terrainData.size.x;
                sizeZ = (int)_terrains[0].terrainData.size.z;

                foreach (Terrain ter in _terrains)
                {
                    try
                    {
                        _posTer = new int[]
                        {
                            (int)(Mathf.Round(ter.transform.position.x / sizeX)),
                            (int)(Mathf.Round(ter.transform.position.z / sizeZ))
                        };

                        _terrainDict.Add(_posTer, ter);
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
                    if (leftTerrain != null) hasLeft = true; else hasLeft = false;
                    if (bottomTerrain != null) hasBottom = true; else hasBottom = false;

                    if (worldIsGenerated)
                    {
                        if (dir.Equals("North"))
                        {
                            if (InfiniteTerrainOffline.northTerrainsNeighbor.Contains(item.Value.name))
                                StitchTerrainsNORTH(item.Value, rightTerrain, bottomTerrain);
                        }
                        else if (dir.Equals("South"))
                        {
                            if (InfiniteTerrainOffline.southTerrainsNeighbor.Contains(item.Value.name))
                                StitchTerrainsSOUTH(item.Value, rightTerrain, topTerrain);
                        }
                        else if (dir.Equals("East"))
                        {
                            if (InfiniteTerrainOffline.eastTerrainsNeighbor.Contains(item.Value.name))
                                StitchTerrainsEAST(item.Value, leftTerrain, bottomTerrain);
                        }
                        else if (dir.Equals("West"))
                        {
                            if (InfiniteTerrainOffline.westTerrainsNeighbor.Contains(item.Value.name))
                                StitchTerrainsWEST(item.Value, rightTerrain, bottomTerrain);
                        }

                        stitchedTerrainsCount++;
                    }
                    else
                        StitchTerrains(item.Value, rightTerrain, topTerrain, hasRight, hasTop, terrains.Count);
                }

                StitchTerrainCorners(dir);
            }
        }

        private static void StitchTerrains(Terrain ter, Terrain rightTerrain, Terrain topTerrain, bool hasRight, bool hasTop, int terrainCount)
        {
            checkLength = runTime.stitchDistance;
            yLength = tileResolution - 1;

            if (hasRight)
            {
                heights = ter.terrainData.GetHeights(yLength, 0, 1, tileResolution);

                //// Check if stitching is needed
                //if (needsStitching)
                //{
                //    secondHeights = rightTerrain.terrainData.GetHeights(0, 0, 1, tileResolution);
                //    bool differentHeightDetected = false;

                //    for (int i = 0; i < tileResolution; i++)
                //    {
                //        float height1 = heights[i, 0];
                //        float height2 = secondHeights[i, 0];

                //        if(!height1.Equals(height2))
                //            differentHeightDetected = true;
                //    }

                //    if (!differentHeightDetected)
                //    {
                //        runTime.stitchTerrainTiles = false;
                //        print("No Stitching Operations Are Needed!");
                //        needsStitching = false;
                //    }
                //}

                //for (int x = 0; x < tileResolution; x++)
                //heights[x, 0] += Mathf.Abs(heights[x, 0] - secondHeights[x, 0]) / runTime.levelSmooth;

                for (int i = 0; i < checkLength; i++)
                    rightTerrain.terrainData.SetHeights(i, 0, heights);

                //rightTerrain.Flush();
            }

            if (hasTop)
            {
                heights = ter.terrainData.GetHeights(0, yLength, tileResolution, 1);

                for (int i = 0; i < checkLength; i++)
                    topTerrain.terrainData.SetHeights(0, i, heights);

                //topTerrain.Flush();
            }

            stitchedTerrainsCount++;

            if (stitchedTerrainsCount == terrainCount)
                stitchingInProgress = false;
        }

        private static void StitchTerrainsNORTH(Terrain ter, Terrain rightTerrain, Terrain bottomTerrain)
        {
            if (hasRight)
            {
                float[,] heights = ter.terrainData.GetHeights(yLength, 0, 1, tileResolution);
                //secondHeights = rightTerrain.terrainData.GetHeights(0, 0, 1, tileResolution);
                //for (int x = 0; x < tileResolution; x++)
                //heights[x, 0] += Mathf.Abs(heights[x, 0] - secondHeights[x, 0]) / runTime.levelSmooth;
                //for (int i = 0; i < checkLength; i++)
                //rightTerrain.terrainData.SetHeights(0, 0, heights);
                rightTerrain.terrainData.SetHeights(0, 0, heights);
                //rightTerrain.Flush();
            }

            if (hasBottom)
            {
                float[,] heights = ter.terrainData.GetHeights(0, 0, tileResolution, 1);
                bottomTerrain.terrainData.SetHeights(0, yLength, heights);
                //bottomTerrain.Flush();
            }
        }

        private static void StitchTerrainsSOUTH(Terrain ter, Terrain rightTerrain, Terrain topTerrain)
        {
            if (hasRight)
            {
                float[,] heights = ter.terrainData.GetHeights(yLength, 0, 1, tileResolution);
                rightTerrain.terrainData.SetHeights(0, 0, heights);
                //rightTerrain.Flush();
            }

            if (hasTop)
            {
                float[,] heights = ter.terrainData.GetHeights(0, yLength, tileResolution, 1);
                topTerrain.terrainData.SetHeights(0, 0, heights);
                //topTerrain.Flush();
            }
        }

        private static void StitchTerrainsEAST(Terrain ter, Terrain leftTerrain, Terrain bottomTerrain)
        {
            if (hasLeft)
            {
                float[,] heights = ter.terrainData.GetHeights(0, 0, 1, tileResolution);
                leftTerrain.terrainData.SetHeights(yLength, 0, heights);
                //leftTerrain.Flush();
            }

            if (hasBottom)
            {
                float[,] heights = ter.terrainData.GetHeights(0, 0, tileResolution, 1);
                bottomTerrain.terrainData.SetHeights(0, yLength, heights);
                //bottomTerrain.Flush();
            }
        }

        private static void StitchTerrainsWEST(Terrain ter, Terrain rightTerrain, Terrain bottomTerrain)
        {
            if (hasRight)
            {
                float[,] heights = ter.terrainData.GetHeights(yLength, 0, 1, tileResolution);
                rightTerrain.terrainData.SetHeights(0, 0, heights);
                //rightTerrain.Flush();
            }

            if (hasBottom)
            {
                float[,] heights = ter.terrainData.GetHeights(0, 0, tileResolution, 1);
                bottomTerrain.terrainData.SetHeights(0, yLength, heights);
                //bottomTerrain.Flush();
            }
        }

        private static void StitchTerrainCorners (string dir)
        {
            stitchedTerrainsCount = 0;

            if (_terrains.Length > 0)
            {
                //Checks neighbours and stitches them
                foreach (var item in _terrainDict)
                {
                    posTer = item.Key;
                    rightTerrain = null;
                    bottomTerrain = null;

                    _terrainDict.TryGetValue(new int[]
                    {
                        posTer [0] + 1,
                        posTer [1]
                    },
                        out rightTerrain
                    );

                    _terrainDict.TryGetValue(new int[]
                    {
                        posTer [0],
                        posTer [1] - 1
                    },
                        out bottomTerrain
                    );

                    if (rightTerrain != null && bottomTerrain != null)
                    {
                        rightBottom = null;

                        _terrainDict.TryGetValue(new int[]
                        {
                            posTer [0] + 1,
                            posTer [1] - 1
                        },
                            out rightBottom
                        );

                        if (rightBottom != null)
                            StitchCorners(item.Value, rightTerrain, bottomTerrain, rightBottom);
                    }

                    stitchedTerrainsCount++;
                }

                stitchingInProgress = false;
                Timing.RunCoroutine(LoadTerrainTextures(dir));
            }
        }

        private static void StitchCorners(Terrain terrainTL, Terrain terrainTR, Terrain terrainBL, Terrain terrainBR)
        {
            int size = tileResolution - 1;
            cornerHeight = terrainTL.terrainData.GetHeights(size, 0, 1, 1)[0, 0];
            heights1x1[0, 0] = cornerHeight;

            terrainTR.terrainData.SetHeights(0, 0, heights1x1);
            //terrainTR.Flush();

            terrainBL.terrainData.SetHeights(size, size, heights1x1);
            //terrainBL.Flush();

            terrainBR.terrainData.SetHeights(0, size, heights1x1);
            //terrainBR.Flush();
        }

        private static float Average (float first, float second)
        {
            return Mathf.Pow ((Mathf.Pow (first, runTime.power) + Mathf.Pow (second, runTime.power)) / 2.0f, 1 / runTime.power);
        }

        public static void SmoothNORTH ()
        {
            try
            {
                if(runTime.smoothIterations > 0)
                {
                    if(geoDataExtensionElevation.Equals("raw"))
                    {
                        for(int x = 0; x < runTime.activeTilesGrid; x++)
                            FinalizeSmooth(rawData[northIndices[x]], m_Width, m_Width, runTime.smoothIterations, smoothBlendIndex, smoothBlend);
                    }
                    else if(geoDataExtensionElevation.Equals("tif"))
                    {
                        for(int x = 0; x < runTime.activeTilesGrid; x++)
                            FinalizeSmooth(tiffDataDynamic[northIndices[x]], tiffWidth, tiffLength, runTime.smoothIterations, smoothBlendIndex, smoothBlend);
                    }
                }
            }
            catch(Exception e)
            {
                UnityEngine.Debug.Log(e);
            }
        }

        public static void SmoothSOUTH ()
        {
            try
            {
                if(runTime.smoothIterations > 0)
                {
                    if(geoDataExtensionElevation.Equals("raw"))
                    {
                        for(int x = 0; x < runTime.activeTilesGrid; x++)
                            FinalizeSmooth(rawData[southIndices[x]], m_Width, m_Width, runTime.smoothIterations, smoothBlendIndex, smoothBlend);
                    }
                    else if(geoDataExtensionElevation.Equals("tif"))
                    {
                        for(int x = 0; x < runTime.activeTilesGrid; x++)
                            FinalizeSmooth(tiffDataDynamic[southIndices[x]], tiffWidth, tiffLength, runTime.smoothIterations, smoothBlendIndex, smoothBlend);
                    }
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
                if(runTime.smoothIterations > 0)
                {
                    if(geoDataExtensionElevation.Equals("raw"))
                    {
                        for(int x = 0; x < runTime.activeTilesGrid; x++)
                            FinalizeSmooth(rawData[eastIndices[x]], m_Width, m_Width, runTime.smoothIterations, smoothBlendIndex, smoothBlend);
                    }
                    else if(geoDataExtensionElevation.Equals("tif"))
                    {
                        for(int x = 0; x < runTime.activeTilesGrid; x++)
                            FinalizeSmooth(tiffDataDynamic[eastIndices[x]], tiffWidth, tiffLength, runTime.smoothIterations, smoothBlendIndex, smoothBlend);
                    }
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
                if(runTime.smoothIterations > 0)
                {
                    if(geoDataExtensionElevation.Equals("raw"))
                    {
                        for(int x = 0; x < runTime.activeTilesGrid; x++)
                            FinalizeSmooth(rawData[westIndices[x]], m_Width, m_Width, runTime.smoothIterations, smoothBlendIndex, smoothBlend);
                    }
                    else if(geoDataExtensionElevation.Equals("tif"))
                    {
                        for(int x = 0; x < runTime.activeTilesGrid; x++)
                            FinalizeSmooth(tiffDataDynamic[westIndices[x]], tiffWidth, tiffLength, runTime.smoothIterations, smoothBlendIndex, smoothBlend);
                    }
                }
            }
            catch(Exception e)
            {
                UnityEngine.Debug.Log(e);
            }
        }

        //public static void SmoothHeights (float[,] data, int width, int height, int index)
        //{
        //    //File.Delete(fileNameTerrainData);

        //    if (runTime.smoothIterations > 0)
        //        FinalizeSmooth(data, width, height, runTime.smoothIterations, smoothBlendIndex, smoothBlend);

        //    if(!RuntimeOffline.tiledElevation)
        //        CalculateResampleHeightmapsGeoServer(index);

        //    if (index == RuntimeOffline.totalTiles - 1)
        //        runTime.FinalizeHeights();
        //}

        public static void FinalizeSmooth (float[,] heightMapSmoothed, int width, int height, int iterations, int blendIndex, float blending)
        {
            if(iterations != 0)
            {
                if(blendIndex == 1)
                {
                    generatedHeightMap = (float[,])heightMapSmoothed.Clone();
                    generatedHeightMap = SmoothedHeights(generatedHeightMap, width, height, iterations);

                    for (int Ty = 0; Ty < height; Ty++)
                    {
                        for (int Tx = 0; Tx < width; Tx++)
                        {
                            oldHeightAtPoint = heightMapSmoothed[Tx, Ty];
                            newHeightAtPoint = generatedHeightMap[Tx, Ty];
                            blendedHeightAtPoint = 0.0f;

                            blendedHeightAtPoint = (newHeightAtPoint * blending) + (oldHeightAtPoint * (1.0f - blending));
                            heightMapSmoothed[Tx, Ty] = blendedHeightAtPoint;
                        }
                    }
                }
                else
                    heightMapSmoothed = SmoothedHeights(heightMapSmoothed, width, height, iterations);
            }
        }

        private static float[,] SmoothedHeights (float[,] heightMap, int width, int height, int iterations)
        {
            for (int iter = 0; iter < iterations; iter++)
            {
                for (Ty = 1; Ty < height - 1; Ty++) // Bypass first & last vertical column in tile
                {
                    if (Ty == 0)
                    {
                        yNeighbours = 2;
                        yShift = 0;
                        yIndex = 0;
                    }
                    else if (Ty == height - 1)
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

                    for (Tx = 1; Tx < width - 1; Tx++) // Bypass first & last horizontal row in tile
                    {
                        if (Tx == 0)
                        {
                            xNeighbours = 2;
                            xShift = 0;
                            xIndex = 0;
                        }
                        else if (Tx == width - 1)
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

                        hCumulative = 0.0f;
                        nNeighbours = 0;

                        for (Ny = 0; Ny < yNeighbours; Ny++)
                        {
                            for (Nx = 0; Nx < xNeighbours; Nx++)
                            {
                                if (neighbourhood == Neighbourhood.Moore || (neighbourhood == Neighbourhood.VonNeumann && (Nx == xIndex || Ny == yIndex)))
                                {
                                    heightAtPoint = heightMap[Tx + Nx + xShift, Ty + Ny + yShift]; // Get height at point
                                    hCumulative += heightAtPoint;
                                    nNeighbours++;
                                }
                            }
                        }

                        hAverage = hCumulative / nNeighbours;
                        heightMap[Tx + xIndex + xShift, Ty + yIndex + yShift] = hAverage;
                    }
                }
            }

            return heightMap;
        }

        public static void CalculateResampleHeightmapsGeoServer (int index)
        {
            if(heightmapResXAll == Mathf.ClosestPowerOfTwo(heightmapResXAll) + splitSizeFinal)
            {
                heightmapResFinalX = Mathf.ClosestPowerOfTwo(heightmapResX) + 1;
                heightmapResFinalXAll = heightmapResXAll;

                heightmapResFinalY = Mathf.ClosestPowerOfTwo(heightmapResY) + 1;
                heightmapResFinalYAll = heightmapResYAll;

                if(finalHeights == null || finalHeights.Length == 0)
                    finalHeights = new float[heightmapResFinalXAll, heightmapResFinalYAll];

                if(geoDataExtensionElevation.Equals("raw"))
                    finalHeights = rawData[index];
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

                ResampleOperation(index);
            }
        }

        private static void ResampleOperation (int index)
        {
            scaleFactorLat = ((float)heightmapResFinalXAll) / ((float)heightmapResXAll);
            scaleFactorLon = ((float)heightmapResFinalYAll) / ((float)heightmapResYAll);

            if (finalHeights == null || finalHeights.Length == 0)
                finalHeights = new float[heightmapResFinalXAll, heightmapResFinalYAll];

            for (int x = 0; x < heightmapResFinalXAll; x++)
                for (int y = 0; y < heightmapResFinalYAll; y++)
                    finalHeights[x, y] = ResampleHeights((float)x / scaleFactorLat, (float)y / scaleFactorLon, index);
        }

        private static float ResampleHeights (float X, float Y, int index)
        {
            try
            {
                X1 = Mathf.RoundToInt((X + heightmapResXAll % heightmapResXAll));
                Y1 = Mathf.RoundToInt((Y + heightmapResYAll % heightmapResYAll));

                if(geoDataExtensionElevation.Equals("raw"))
                    return rawData[index][X1, Y1];
                else if(geoDataExtensionElevation.Equals("tif"))
                    return tiffData[X1, Y1];
                else if(geoDataExtensionElevation.Equals("asc"))
                    return asciiData[X1, Y1];

                return 0f;
            }
            catch
            {
                return 0f;
            }
        }

        private static Vector3 RealTerrainSize (float width, float length, float height)
        {
            //float realTerrainSizeY = ((initialTerrainWidth * splitSizeFinal) * ((height * terrainEverestDiffer) / width)) * runTime.elevationExaggeration;
            realTerrainSizeY = highestPoint * terrainEverestDiffer * (runTime.elevationExaggeration * runTime.sizeExaggeration);

            if(realTerrainSizeY <= 0f ||  float.IsNaN(realTerrainSizeY) || float.IsInfinity(realTerrainSizeY) || float.IsPositiveInfinity(realTerrainSizeY) || float.IsNegativeInfinity(realTerrainSizeY))
                realTerrainSizeY = 0.001f;

            realTerrainSizeZ = initialTerrainWidth * terrainSizeFactor;

            finalTerrainSize = new Vector3(initialTerrainWidth, realTerrainSizeY, realTerrainSizeZ);

            return finalTerrainSize;
        }

        public static void GetTIFFInfo ()
        {
            try
            {
                using (Tiff inputImage = Tiff.Open(elevationNames[0], "r"))
                {
                    tiffWidth = inputImage.GetField(TiffTag.IMAGEWIDTH)[0].ToInt();
                    tiffLength = inputImage.GetField(TiffTag.IMAGELENGTH)[0].ToInt();
                    tiffDataASCII = new float[tiffLength, tiffWidth];

                    tileHeight = inputImage.GetField(TiffTag.TILELENGTH)[0].ToInt();
                    tileWidth = inputImage.GetField(TiffTag.TILEWIDTH)[0].ToInt();

                    if (buffer == null || buffer.Length == 0)
                        buffer = new byte[tileHeight * tileWidth * 4];

                    if (fBuffer == null || fBuffer.Length == 0)
                        fBuffer = new float[tileHeight, tileWidth];

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
            sr = new StreamReader(elevationNames[0], Encoding.ASCII, true);
            //ncols
            line1 = sr.ReadLine().Replace(',', '.').Split(new char[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
            nCols = (Convert.ToInt32(line1[1]));
            //nrows
            line2 = sr.ReadLine().Replace(',', '.').Split(new char[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
            nRows = (Convert.ToInt32(line2[1]));
            //xllcorner
            line3 = sr.ReadLine().Replace(',', '.').Split(new char[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
            xllCorner = Convert.ToDouble((line3[1]), CultureInfo.InvariantCulture);
            //yllcorner
            line4 = sr.ReadLine().Replace(',', '.').Split(new char[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
            yllCorner = Convert.ToDouble((line4[1]), CultureInfo.InvariantCulture);
            //cellsize
            line5 = sr.ReadLine().Replace(',', '.').Split(new char[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
            cellSizeASCII = Convert.ToDouble((line5[1]), CultureInfo.InvariantCulture);
            //nodata
            line6 = sr.ReadLine().Replace(',', '.').Split(new char[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
            noData = Convert.ToDouble((line6[1]), CultureInfo.InvariantCulture);

            if (asciiData == null || asciiData.Length == 0)
                asciiData = new float[nCols, nRows];

            for (int y = 0; y < nRows; y++)
            {
                line = sr.ReadLine().Split(new char[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);

                for (int x = 0; x < nCols; x++)
                {
                    asciiData[(nRows - 1) - y, x] = (float.Parse(line[x].Replace(',', '.'))) / everestPeak;
                }
            }

            sr.Close();

            highestPoint = asciiData.Cast<float>().Max() * everestPeak;
            lowestPoint = asciiData.Cast<float>().Min() * everestPeak;

            print(xllCorner + yllCorner + cellSizeASCII + noData);
        }

        private static IEnumerator<float> FillHeights (Terrain terrainTile, int terrainRes, float[,] terrainHeights)
        {
            if(runTime.cellSize > terrainRes)
                runTime.cellSize = terrainRes;

            gridCount = (terrainRes - 1) / runTime.cellSize;

            for(int i = 0; i < gridCount; i++)
            {
                for(int j = 0; j < gridCount; j++)
                {
                    if(heightmapCell == null || heightmapCell.Length == 0)
                        heightmapCell = new float[runTime.cellSize, runTime.cellSize];

                    row = i * runTime.cellSize;
                    col = j * runTime.cellSize;

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

                    yield return Timing.WaitForSeconds(runTime.elevationDelay);
                }
            }

            // Fill Top Row
            if (heightmapCellTopRow == null || heightmapCellTopRow.Length == 0)
                heightmapCellTopRow = new float[1, terrainRes];

            for(int x = 0; x < terrainRes; x++)
                heightmapCellTopRow[0, x] = terrainHeights[terrainRes - 1, x];

            if(runTime.delayedLOD)
                terrainTile.terrainData.SetHeightsDelayLOD(0, terrainRes - 1, heightmapCellTopRow);
            else
                terrainTile.terrainData.SetHeights(0, terrainRes - 1, heightmapCellTopRow);

            // Fill Right Column
            if (heightmapCellRightColumn == null || heightmapCellRightColumn.Length == 0)
                heightmapCellRightColumn = new float[terrainRes, 1];

            for(int x = 0; x < terrainRes; x++)
                heightmapCellRightColumn[x, 0] = terrainHeights[x, terrainRes - 1];

            if(runTime.delayedLOD)
                terrainTile.terrainData.SetHeightsDelayLOD(terrainRes - 1, 0, heightmapCellRightColumn);
            else
                terrainTile.terrainData.SetHeights(terrainRes - 1, 0, heightmapCellRightColumn);

            if(runTime.delayedLOD)
            {
                yield return Timing.WaitForSeconds(runTime.elevationDelay);
                terrainTile.ApplyDelayedHeightmapModification();
            }

            if(runTime.showTileOnFinish)
                terrainTile.drawHeightmap = true;

            generatedTerrainsCount++;

            if (generatedTerrainsCount == terrainChunks && splittedTerrains)
            {
                if(runTime.stitchTerrainTiles)
                {
                    ManageNeighborings("");
                    StitchTerrain(croppedTerrains, "");
                }
                else
                    ManageNeighborings("");
            }
        }

        private static IEnumerator<float> FillHeightsNEWS (Terrain terrainTile, int terrainRes, float[,] terrainHeights, string dir)
        {
            if (!runTime.showTileOnFinish)
            {
                terrainTile.transform.localPosition = new Vector3(terrainTile.transform.localPosition.x, 0, terrainTile.transform.localPosition.z);
                terrainTile.drawHeightmap = true;
            }

            if (runTime.cellSize > terrainRes)
                runTime.cellSize = terrainRes;
            
            gridCount = (terrainRes - 1) / runTime.cellSize;

            // Fill Tile Cells
            for (int i = 0; i < gridCount; i++)
            {
                for(int j = 0; j < gridCount; j++)
                {
                    row = i * runTime.cellSize;
                    col = j * runTime.cellSize;

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

                    yield return Timing.WaitForSeconds(runTime.elevationDelay);
                }
            }

            // Fill Top Row
            for (int x = 0; x < terrainRes; x++)
                heightmapCellTopRow[0, x] = terrainHeights[terrainRes - 1, x];

            if(runTime.delayedLOD)
                terrainTile.terrainData.SetHeightsDelayLOD(0, terrainRes - 1, heightmapCellTopRow);
            else
                terrainTile.terrainData.SetHeights(0, terrainRes - 1, heightmapCellTopRow);

            // Fill Right Column
            for (int x = 0; x < terrainRes; x++)
                heightmapCellRightColumn[x, 0] = terrainHeights[x, terrainRes - 1];

            if(runTime.delayedLOD)
                terrainTile.terrainData.SetHeightsDelayLOD(terrainRes - 1, 0, heightmapCellRightColumn);
            else
                terrainTile.terrainData.SetHeights(terrainRes - 1, 0, heightmapCellRightColumn);

            if(runTime.delayedLOD)
                terrainTile.ApplyDelayedHeightmapModification();

            //if (runTime.showTileOnFinish)
            //{
            //    terrainTile.transform.localPosition = new Vector3(terrainTile.transform.localPosition.x, 0, terrainTile.transform.localPosition.z);
            //    terrainTile.drawHeightmap = true;
            //}
        }

        public static void ManageNeighborings (string dir)
        {
            terrainsLong = splitSizeFinal;
            terrainsWide = splitSizeFinal;
            SetTerrainNeighbors(dir);
        }

        private static void SetTerrainNeighbors (string dir)
        {
            if(splittedTerrains)
            {
                //                if(!worldIsGenerated)
                //                    Timing.RunCoroutine(PerformNeighboring(OrderedTerrainChunks(splittedTerrains)));
                //                else
                //                {
                GetTerrainList(dir);
                //Timing.RunCoroutine(PerformNeighboring(stitchingTerrainsList));
                //}
            }
            else if(terrain)
                terrain.gameObject.AddComponent<TerrainNeighbors>();
        }

        public static List<Terrain> OrderedTerrainChunks (GameObject terrainsParentGo)
        {
            childName = "";

            foreach (Transform child in terrainsParentGo.transform)
                childName += child.name + Environment.NewLine;

            line = childName.Replace("\r","").Split('\n');
            line = LogicalComparer(line);

            stitchingTerrains.Clear();

            foreach (string s in line)
                if(s != "")
                    stitchingTerrains.Add(terrainsParentGo.transform.Find(s).GetComponent<Terrain>());

            childName = null;

            return stitchingTerrains;
        }

        private static void GetTerrainList (string dir)
        {
            if(!worldIsGenerated)
            {
                stitchingTerrainsList.Clear();
                stitchingTerrainsList = croppedTerrains;
            }

            //if(runTime.circularLOD)
            //{
            //    // Set Tile LODs based on distance from center
            //    if (runTime.heightmapPixelError != runTime.heightmapPixelErrorFurthest)
            //    {
            //        topIndex = 0;
            //        downIndex = runTime.activeTilesGrid - 1;
            //        leftIndex = 0;
            //        rightIndex = runTime.activeTilesGrid - 1;
            //        centerLayer = (runTime.activeTilesGrid / 2) - runTime.centerLayersCount;
            //
            //        while (true)
            //        {
            //            // top row
            //            for (int j = leftIndex; j <= rightIndex; ++j)
            //            {
            //                if (topIndex >= centerLayer)
            //                    InfiniteTerrainOffline._grid[j, topIndex].heightmapPixelError = runTime.heightmapPixelError;
            //                else
            //                    InfiniteTerrainOffline._grid[j, topIndex].heightmapPixelError = Mathf.Clamp(runTime.heightmapPixelErrorFurthest / (topIndex + 1), runTime.heightmapPixelError, 200);
            //            }
            //
            //            topIndex++;
            //
            //            if (topIndex > downIndex || leftIndex > rightIndex)
            //                break;
            //
            //            // rightmost column
            //            for (int i = topIndex; i <= downIndex; ++i)
            //            {
            //                if (runTime.activeTilesGrid - rightIndex - 1 >= centerLayer)
            //                    InfiniteTerrainOffline._grid[rightIndex, i].heightmapPixelError = runTime.heightmapPixelError;
            //                else
            //                    InfiniteTerrainOffline._grid[rightIndex, i].heightmapPixelError = Mathf.Clamp(runTime.heightmapPixelErrorFurthest / (runTime.activeTilesGrid - rightIndex), runTime.heightmapPixelError, 200);
            //            }
            //
            //            rightIndex--;
            //
            //            if (topIndex > downIndex || leftIndex > rightIndex)
            //                break;
            //
            //            // the bottom row
            //            for (int j = rightIndex; j >= leftIndex; --j)
            //            {
            //                if (runTime.activeTilesGrid - downIndex - 1 >= centerLayer)
            //                    InfiniteTerrainOffline._grid[j, downIndex].heightmapPixelError = runTime.heightmapPixelError;
            //                else
            //                    InfiniteTerrainOffline._grid[j, downIndex].heightmapPixelError = Mathf.Clamp(runTime.heightmapPixelErrorFurthest / (runTime.activeTilesGrid - downIndex), runTime.heightmapPixelError, 200);
            //            }
            //
            //            downIndex--;
            //
            //            if (topIndex > downIndex || leftIndex > rightIndex)
            //                break;
            //
            //            // the leftmost column
            //            for (int i = downIndex; i >= topIndex; --i)
            //            {
            //                if (leftIndex >= centerLayer)
            //                    InfiniteTerrainOffline._grid[leftIndex, i].heightmapPixelError = runTime.heightmapPixelError;
            //                else
            //                    InfiniteTerrainOffline._grid[leftIndex, i].heightmapPixelError = Mathf.Clamp(runTime.heightmapPixelErrorFurthest / (leftIndex + 1), runTime.heightmapPixelError, 200);
            //            }
            //
            //            leftIndex++;
            //
            //            if (topIndex > downIndex || leftIndex > rightIndex)
            //                break;
            //        }
            //    }
            //}

            PerformNeighboring(stitchingTerrainsList, dir);
        }

        private static void PerformNeighboring (List<Terrain> terrains, string dir)
        {
            tiles = runTime.activeTilesGrid;

#if UNITY_2018_3_OR_NEWER
            if (!terrainsAreGenerated)
            {
                for (int i = 0; i < (int)Mathf.Pow(tiles, 2); i++)
                {
                    terrains[i].groupingID = 0;
                    terrains[i].allowAutoConnect = true;
                }
            }
#endif
            int counter = 0;

            for (int y = 0; y < tiles ; y++)
            {
                for(int x = 0; x < tiles; x++)
                {
                    indexLft = counter - 1;
                    indexTop = counter - tiles;
                    indexRgt = counter + 1;
                    indexBtm = counter + tiles;

                    if(y == 0)
                    {
                        if(x == 0)
                            terrains[counter].SetNeighbors(null, null, terrains[indexRgt], terrains[indexBtm]);
                        else if(x == tiles - 1)
                            terrains[counter].SetNeighbors(terrains[indexLft], null, null, terrains[indexBtm]);
                        else
                            terrains[counter].SetNeighbors(terrains[indexLft], null, terrains[indexRgt], terrains[indexBtm]);
                    }
                    else if(y == tiles - 1)
                    {
                        if(x == 0)
                            terrains[counter].SetNeighbors(null, terrains[indexTop], terrains[indexRgt], null);
                        else if(x == tiles - 1)
                            terrains[counter].SetNeighbors(terrains[indexLft], terrains[indexTop], null, null);
                        else
                            terrains[counter].SetNeighbors(terrains[indexLft], terrains[indexTop], terrains[indexRgt], null);
                    }
                    else
                    {
                        if(x == 0)
                            terrains[counter].SetNeighbors(null, terrains[indexTop], terrains[indexRgt], terrains[indexBtm]);
                        else if(x == tiles - 1)
                            terrains[counter].SetNeighbors(terrains[indexLft], terrains[indexTop], null, terrains[indexBtm]);
                        else
                            terrains[counter].SetNeighbors(terrains[indexLft], terrains[indexTop], terrains[indexRgt], terrains[indexBtm]);
                    }

                    terrains[counter].Flush();
                    counter++;
                }
            }

            if (worldIsGenerated)
            {
                runTime.stitchTerrainTiles = false;

                if (runTime.stitchTerrainTiles)
                {
                    if (dir.Equals("North"))
                        StitchNORTH();
                    else if (dir.Equals("South"))
                        StitchSOUTH();
                    else if (dir.Equals("East"))
                        StitchEAST();
                    else if (dir.Equals("West"))
                        StitchWEST();
                }
                else
                    Timing.RunCoroutine(LoadTerrainTextures(dir));
            }

            CheckInitialization();
        }

        private static void CheckInitialization ()
        {
            if(!terrainsAreGenerated)
            {
                terrainsAreGenerated = true;
                UnityEngine.Debug.Log("Terrains Are Generated");

                if(runTime.elevationOnly)
                    Timing.RunCoroutine(WorldIsGenerated());
                else
                {
                    if(imagesAreGenerated)
                        Timing.RunCoroutine(WorldIsGenerated());
                }
            }
        }

        private static IEnumerator<float> WorldIsGenerated ()
        {
            yield return Timing.WaitForSeconds(2);

            QualitySettings.shadowDistance = areaSizeLat * 1000 / 4;

            Camera.main.transform.position = new Vector3
                (
                    0,
                    terrainsParent.transform.position.y + ((highestPoint + 5) * (runTime.elevationExaggeration * runTime.sizeExaggeration)),
                    0)
                ;
            
            if(!runTime.isStreamingAssets)
            {
                Camera.main.farClipPlane = areaSizeLat * 1000 * 3;

                if(floatingOriginAdvanced != null)
                    floatingOriginAdvanced.enabled = true;
            }

            //android Build
            if (Camera.main.GetComponent<ExtendedFlyCam>() != null)
                Camera.main.GetComponent<ExtendedFlyCam>().enabled = true;
            //android Build

            OfflineManager.releaseDisplay = true;

            worldIsGenerated = true;
            UnityEngine.Debug.Log("World Is Generated");

            //CheckTerrainChunks();
            runTime.SendProcessedTiles(croppedTerrains);
        }

        private static IEnumerator<float> LoadTerrainTextures(string dir)
        {
            if (!runTime.elevationOnly)
            {
                if (runTime.asyncImageLoading)
                    runTime.ServerConnectImagery(dir);
                else
                {
                    if (dir.Equals("North"))
                    {
                        for (int i = 0; i < runTime.activeTilesGrid; i++)
                        {
                            yield return Timing.WaitForSeconds(runTime.imageryDelay);
                            runTime.ServerConnectImagery(northIndices[i], dir);
                        }

                        //northTerrainsUseTimes--;
                    }
                    else if (dir.Equals("South"))
                    {
                        for (int i = 0; i < runTime.activeTilesGrid; i++)
                        {
                            yield return Timing.WaitForSeconds(runTime.imageryDelay);
                            runTime.ServerConnectImagery(southIndices[i], dir);
                        }

                        //southTerrainsUseTimes--;
                    }
                    else if (dir.Equals("East"))
                    {
                        for (int i = 0; i < runTime.activeTilesGrid; i++)
                        {
                            yield return Timing.WaitForSeconds(runTime.imageryDelay);
                            runTime.ServerConnectImagery(eastIndices[i], dir);
                        }

                        //eastTerrainsUseTimes--;
                    }
                    else if (dir.Equals("West"))
                    {
                        for (int i = 0; i < runTime.activeTilesGrid; i++)
                        {
                            yield return Timing.WaitForSeconds(runTime.imageryDelay);
                            runTime.ServerConnectImagery(westIndices[i], dir);
                        }

                        //westTerrainsUseTimes--;
                    }
                }
            }
            else
                FinalizeTiles(dir);
        }

        private static void ImageTiler ()
        {
            counter = 0;
            tileImages = 0;

            if(runTime.startFromCenter)
                tileImages = RuntimeOffline.totalTiles;
            else
                tileImages = totalImagesDataBase;

            if(!multipleTerrainsTiling)
            {
                cellSizeX = terrainSizeX / tileGrid;
                cellSizeY = terrainSizeY / tileGrid;

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
                            terrainTextures[counter].texture.Apply();

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

                lengthz = (int)(cellSizeY / splatNormalizeY);
                widthz = (int)(cellSizeX / splatNormalizeX);

                for (int i = 0; i < tileImages; i++)
                {
                    try
                    {
                        lengthzOff = (int)(imageYOffset[i] / splatNormalizeY);
                        widthzOff = (int)(imageXOffset[i] / splatNormalizeX);

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
                index = 0;
                terrainSizeSplittedX = croppedTerrains[0].terrainData.size.x;
                terrainSizeSplittedY = croppedTerrains[0].terrainData.size.z;
                cellSizeSplittedX = terrainSizeSplittedX / (float)tileGrid;
                cellSizeSplittedY = terrainSizeSplittedY / (float)tileGrid;
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

                stitchingTerrainsList = OrderedTerrainChunks(splittedTerrains);

                //cellIndex = new int[tileImages];
                //index = 0;
                //multiplier = 0;
                //
                //if(tileImages == terrainChunks)
                //{
                //    for(int i = 0; i < terrainChunks; i++)
                //        cellIndex[index++] = i;
                //}
                //else
                //{
                //    topLeftCellIndex = new int[terrainChunks];
                //    multiplier = tileImages / terrainChunks;
                //
                //    for(int i = 0; i < runTime.activeTilesGrid; i++)
                //        for(int j = 0; j < runTime.activeTilesGrid; j++)
                //            topLeftCellIndex[index++] = ((i * 1) * runTime.activeTilesGrid) + ((j * 1 * terrainChunks) * runTime.activeTilesGrid);
                //
                //    Array.Sort(topLeftCellIndex);
                //
                //    index = 0;
                //
                //    for(int z = 0; z < terrainChunks; z++)
                //    {
                //        adder = 0;
                //
                //        for(int i = 0; i < runTime.activeTilesGrid; i++)
                //        {
                //            for(int j = 0; j < runTime.activeTilesGrid; j++)
                //            {
                //                try
                //                {
                //                    cellIndex[index++] = topLeftCellIndex[counter] + j + adder;
                //                }
                //                catch(Exception e)
                //                {
                //                    UnityEngine.Debug.Log(e);
                //                }
                //            }
                //            adder += multiplier;
                //        }
                //        counter++;
                //    }
                //}
                //
                //counter = 0;
                //index = 0;

                foreach(Terrain terrainSplitted in stitchingTerrainsList)
                {
                    //terrainTextures = new SplatPrototype[imagesPerTerrain];

                    //for (int i = 0; i < imagesPerTerrain; i++)
                    //{
                    //    try
                    //    {
                    //        Texture2D satelliteImage = images[cellIndex[index]];

                    //        // Texturing Terrain
                    //        terrainTextures[i] = new SplatPrototype();
                    //        terrainTextures[i].texture = satelliteImage;
                    //        terrainTextures[i].tileSize = new Vector2(cellSizeSplittedX, cellSizeSplittedY);
                    //        terrainTextures[i].tileOffset = new Vector2(imageXOffset[i], imageYOffset[i]);
                    //        terrainTextures[i].texture.Apply();
                    //    }
                    //    catch(Exception e)
                    //    {
                    //        UnityEngine.Debug.LogError(e);
                    //    }

                    //    index++;
                    //}

                    //terrainSplitted.terrainData.splatPrototypes = terrainTextures;

                    splatNormalizeX = terrainSplitted.terrainData.size.x / terrainSplitted.terrainData.alphamapResolution;
                    splatNormalizeY = terrainSplitted.terrainData.size.z / terrainSplitted.terrainData.alphamapResolution;

                    lengthz = (int)(cellSizeSplittedY / splatNormalizeY);
                    widthz = (int)(cellSizeSplittedX / splatNormalizeX);

                    for (int i = 0; i < imagesPerTerrain; i++)
                    {
                        //try
                        //{
                            lengthzOff = (int)(imageYOffset[i] / splatNormalizeY);
                            widthzOff = (int)(imageXOffset[i] / splatNormalizeX);

                            smData = new float[lengthz, widthz, terrainSplitted.terrainData.alphamapLayers];

                            for(int y = 0; y <lengthz; y++)
                                for(int z = 0; z < widthz; z++)
                                    smData[y, z, i] = 1;

                            terrainSplitted.terrainData.SetAlphamaps(-widthzOff, -lengthzOff, smData);
                        //}
                        //catch(Exception e)
                        //{
                            //UnityEngine.Debug.LogError(e);
                        //}
                    }

                    terrainSplitted.terrainData.RefreshPrototypes();
                    terrainSplitted.Flush();

                    terrainTextures = null;
                    smData = null;

                    counter++;
                }
            }
        }
    }
}

