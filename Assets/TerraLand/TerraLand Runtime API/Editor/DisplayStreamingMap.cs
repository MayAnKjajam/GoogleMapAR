#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEngine.SceneManagement;
using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Threading;

namespace TerraLand
{
    public class DisplayStreamingMap : EditorWindow
    {
        static int windowSize = 800;

        [MenuItem("Tools/TerraUnity/TerraLand/Streaming Map", false, 10)]
        public static void Init()
        {
            DisplayStreamingMap window = (DisplayStreamingMap)GetWindow(typeof(DisplayStreamingMap));
            window.position = new Rect(5, 135, windowSize, windowSize);
        }

        RuntimeOffline runtimeOffline;
        Texture2D globalSatelliteImage;
        int heightmapResolution, satImageResolution;
        Texture2D placemark;
        int placemarkResolution = 32;
        Rect placemarkRect;
        Event key;
        string serverPath;
        string globalHeightmapPath, globalSatelliteImagePath;
        bool isValid = false;
        double top, left, bottom, right;
        double centerLat, centerLon, centerLatMouse, centerLonMouse;
        double latExtent, lonExtent;
        string tileLatitude, tileLongitude;
        string tileTLBRCoords;
        byte[] imageData;
        static int tileRow, tileColumn, tileIndex;
        int databaseTiles;
        int databaseGrid;
        bool reloadMap;
        private Material GUIMaterial;
        private static string sceneName;

        static int maxThreads = 8;
        private static int numThreads;
        private static int _count;

        private static bool m_HasLoaded = false;

        private static List<Action> _actions = new List<Action>();
        private static List<DelayedQueueItem> _delayed = new List<DelayedQueueItem>();

        private List<DelayedQueueItem> _currentDelayed = new List<DelayedQueueItem>();
        private List<Action> _currentActions = new List<Action>();

        public struct DelayedQueueItem
        {
            public float time;
            public Action action;
        }

        static void QueueOnMainThread(Action action)
        {
            QueueOnMainThread(action, 0f);
        }

        static void QueueOnMainThread(Action action, float time)
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

        static Thread RunAsync(Action a)
        {
            while (numThreads >= maxThreads)
            {
                Thread.Sleep(1);
            }
            Interlocked.Increment(ref numThreads);
            ThreadPool.QueueUserWorkItem(RunAction, a);
            return null;
        }

        static void RunAction(object action)
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
        }

        private void OnEnable()
        {
            Scene scene = SceneManager.GetActiveScene();
            sceneName = scene.name;
            List<GameObject> sceneObjects = new List<GameObject>();
            scene.GetRootGameObjects(sceneObjects);

            foreach (GameObject go in sceneObjects)
            {
                if (go.GetComponent<RuntimeOffline>() != null)
                {
                    runtimeOffline = go.GetComponent<RuntimeOffline>();
                    break;
                }
            }

            if (runtimeOffline != null)
            {
                serverPath = RuntimeOffline.dataBasePath;
                databaseTiles = RuntimeOffline.dataBaseTiles;
                databaseGrid = RuntimeOffline.dataBaseGrid;
                heightmapResolution = runtimeOffline.previewHeightmapResolution;
                satImageResolution = runtimeOffline.previewSatelliteImageResolution;
                reloadMap = runtimeOffline.reloadMap;
            }
            else
                return;

            if (Directory.Exists(serverPath))
            {
                SetParams();
                isValid = true;
            }
            else
            {
                EditorUtility.DisplayDialog("DIRECTORY NOT AVAILABLE", "Type database path correctly!", "Ok");
                isValid = false;
            }

            GUIMaterial = (Material)Resources.Load("Streaming/MarkerMat");
        }

        public void OnGUI()
        {
            if (!isValid)
                this.Close();

            key = Event.current;

            if (globalSatelliteImage != null)
                EditorGUI.DrawPreviewTexture(new Rect(0, 0, Screen.width, Screen.width), globalSatelliteImage);

            SelectStartingPoint();
        }

        private void SetParams()
        {
            placemark = Resources.Load("Streaming/Placemark") as Texture2D;

            runtimeOffline.SetupServer();
            globalHeightmapPath = RuntimeOffline.globalHeightmapPath;
            globalSatelliteImagePath = RuntimeOffline.globalSatelliteImagePath;
            top = RuntimeOffline.top;
            left = RuntimeOffline.left;
            bottom = RuntimeOffline.bottom;
            right = RuntimeOffline.right;
            latExtent = RuntimeOffline.latExtent;
            lonExtent = RuntimeOffline.lonExtent;

            if (PlayerPrefs.HasKey(sceneName + "_TilePixelX") && PlayerPrefs.HasKey(sceneName + "_TilePixelY") && PlayerPrefs.HasKey(sceneName + "_TilePreviewResolution"))
            {
                int pixelX = (int)(Mathf.InverseLerp(1, PlayerPrefs.GetInt(sceneName + "_TilePreviewResolution"), PlayerPrefs.GetInt(sceneName + "_TilePixelX")) * windowSize);
                int pixelY = (int)(Mathf.InverseLerp(1, PlayerPrefs.GetInt(sceneName + "_TilePreviewResolution"), PlayerPrefs.GetInt(sceneName + "_TilePixelY")) * windowSize);
                placemarkRect = new Rect(pixelX - (placemarkResolution / 2), pixelY - placemarkResolution, placemarkResolution, placemarkResolution);
            }
            else
                placemarkRect = new Rect((windowSize / 2) - (placemarkResolution / 2), (windowSize / 2) - placemarkResolution, placemarkResolution, placemarkResolution);

            if (PlayerPrefs.HasKey(sceneName + "_TileCenterLat") && PlayerPrefs.HasKey(sceneName + "_TileCenterLon"))
            {
                centerLat = double.Parse(PlayerPrefs.GetString(sceneName + "_TileCenterLat"));
                centerLon = double.Parse(PlayerPrefs.GetString(sceneName + "_TileCenterLon"));
            }
            else
            {
                centerLat = bottom + (latExtent * 0.5d);
                centerLon = left + (lonExtent * 0.5d);
            }

            if (PlayerPrefs.HasKey(sceneName + "_TileRow"))
                tileRow = PlayerPrefs.GetInt(sceneName + "_TileRow");
            else
                tileRow = (databaseGrid / 2) + 1;

            if (PlayerPrefs.HasKey(sceneName + "_TileColumn"))
                tileColumn = PlayerPrefs.GetInt(sceneName + "_TileColumn");
            else
                tileColumn = (databaseGrid / 2) + 1;

            tileIndex = ((tileRow - 1) * databaseGrid) + tileColumn - 1;

            EditorUtility.DisplayProgressBar("LOADING PREVIEW MAP", "Please Wait...", 1);

            if (reloadMap)
            {
                RunAsync(() =>
                {
                    Tile.LoadSatelliteMapFromESRIServer(globalSatelliteImagePath, top, left, bottom, right, satImageResolution);

                    QueueOnMainThread(() =>
                    {
                        globalSatelliteImage = Tile.LoadSatelliteMapFromLocalServer(globalSatelliteImagePath);
                        EditorUtility.ClearProgressBar();
                    });
                });

                RunAsync(() =>
                {
                    Tile.LoadHeightmapFromESRIServer(globalHeightmapPath, top, left, bottom, right, heightmapResolution);
                });
            }
            else
            {
                if (File.Exists(globalSatelliteImagePath))
                {
                    globalSatelliteImage = Tile.LoadSatelliteMapFromLocalServer(globalSatelliteImagePath);
                    EditorUtility.ClearProgressBar();
                }
                else
                {
                    RunAsync(() =>
                    {
                        Tile.LoadSatelliteMapFromESRIServer(globalSatelliteImagePath, top, left, bottom, right, satImageResolution);

                        QueueOnMainThread(() =>
                        {
                            globalSatelliteImage = Tile.LoadSatelliteMapFromLocalServer(globalSatelliteImagePath);
                            EditorUtility.ClearProgressBar();
                        });
                    });
                }

                if (!File.Exists(globalHeightmapPath))
                {
                    RunAsync(() =>
                    {
                        Tile.LoadHeightmapFromESRIServer(globalHeightmapPath, top, left, bottom, right, heightmapResolution);
                    });
                }
            }
        }

        private void SelectStartingPoint()
        {
            CalculateMouseCoords((int)key.mousePosition.x, (int)key.mousePosition.y, Screen.width);

            GUI.color = UnityEngine.Color.gray;
            EditorGUI.HelpBox(new Rect(5, Screen.height - 40, 250, 16), centerLatMouse + "  " + centerLonMouse, MessageType.None);

            if (key.button == 0 && key.type == EventType.MouseDown)
                CalculateParams((int)key.mousePosition.x, (int)key.mousePosition.y, Screen.width);

            EditorGUI.HelpBox(new Rect(Screen.width - 305, Screen.height - 40, 300, 16), "Placemark: " + centerLat + "  " + centerLon, MessageType.None);
            EditorGUI.HelpBox(new Rect(5, 5, 80, 16), "Tile: " + tileRow + "-" + tileColumn, MessageType.None);
            GUI.color = UnityEngine.Color.white;

            if (placemark != null)
                EditorGUI.DrawPreviewTexture(placemarkRect, placemark, GUIMaterial);
        }

        private void CalculateMouseCoords(int pixelX, int pixelY, int resolution)
        {
            centerLatMouse = bottom + (latExtent * Mathf.InverseLerp(resolution, 0, pixelY));
            centerLonMouse = left + (lonExtent * Mathf.InverseLerp(0, resolution, pixelX));
        }

        private void CalculateParams(int pixelX, int pixelY, int resolution)
        {
            placemarkRect = new Rect(pixelX - (placemarkResolution / 2), pixelY - placemarkResolution, placemarkResolution, placemarkResolution);

            centerLat = bottom + (latExtent * Mathf.InverseLerp(resolution, 0, pixelY));
            centerLon = left + (lonExtent * Mathf.InverseLerp(0, resolution, pixelX));

            float pixelNormalizedX = Mathf.InverseLerp(1, resolution, pixelX);
            float pixelNormalizedY = Mathf.InverseLerp(1, resolution, pixelY);

            tileRow = Mathf.CeilToInt(pixelNormalizedY * databaseGrid);
            tileColumn = Mathf.CeilToInt(pixelNormalizedX * databaseGrid);
            tileIndex = ((tileRow - 1) * databaseGrid) + tileColumn - 1;

            // If in corners, offset center tile to be in bounds range
            int activeTilesGrid = runtimeOffline.activeTilesGrid;
            int tileRowOffset = tileRow;
            int tileColOffset = tileColumn;

            if (tileRowOffset < activeTilesGrid / 2)
                tileRowOffset = (activeTilesGrid / 2);

            if (tileColOffset < activeTilesGrid / 2)
                tileColOffset = (activeTilesGrid / 2);

            if (tileRowOffset > databaseGrid - (activeTilesGrid / 2))
                tileRowOffset = databaseGrid - (activeTilesGrid / 2);

            if (tileColOffset > databaseGrid - (activeTilesGrid / 2))
                tileColOffset = databaseGrid - (activeTilesGrid / 2);

            double pixelNormalizedXTile = 1d - ((double)tileRowOffset / databaseGrid);
            double pixelNormalizedYTile = (double)tileColOffset / databaseGrid;

            tileLatitude = (bottom + ((top - bottom) * pixelNormalizedXTile)).ToString();
            tileLongitude = (left + ((right - left) * pixelNormalizedYTile)).ToString();

            //TODO: Calculate center tile's bbox
            string tileTop = "";
            string tileLeft = "";
            string tileBottom = "";
            string tileRight = "";
            tileTLBRCoords = tileTop + "," + tileLeft + "," + tileBottom + "," + tileRight;

            SetPlayerPrefs(pixelX, pixelY, resolution);
        }

        private void SetPlayerPrefs(int pixelX, int pixelY, int resolution)
        {
            PlayerPrefs.SetInt(sceneName + "_TilePixelX", pixelX);
            PlayerPrefs.SetInt(sceneName + "_TilePixelY", pixelY);
            PlayerPrefs.SetInt(sceneName + "_TilePreviewResolution", resolution);

            PlayerPrefs.SetString(sceneName + "_TileCenterLat", tileLatitude.ToString());
            PlayerPrefs.SetString(sceneName + "_TileCenterLon", tileLongitude.ToString());

            PlayerPrefs.SetString(sceneName + "_PointLat", centerLat.ToString());
            PlayerPrefs.SetString(sceneName + "_PointLon", centerLon.ToString());

            PlayerPrefs.SetInt(sceneName + "_TileRow", tileRow);
            PlayerPrefs.SetInt(sceneName + "_TileColumn", tileColumn);
            PlayerPrefs.SetInt(sceneName + "_TileIndex", tileIndex);

            PlayerPrefs.SetString(sceneName + "_TileTLBRCoords", tileTLBRCoords);

            PlayerPrefs.Save();
        }

        public void OnInspectorUpdate()
        {
            Repaint();
        }
    }
}
#endif

