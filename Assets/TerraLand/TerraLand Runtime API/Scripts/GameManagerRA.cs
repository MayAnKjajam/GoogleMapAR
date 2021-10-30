/*
    _____  _____  _____  _____  ______
        |  _____ |      |      |  ___|
        |  _____ |      |      |     |
    
     U       N       I       T      Y
                                         
    
    TerraUnity Co. - Earth Simulation Tools - 2019
    
    http://terraunity.com
    info@terraunity.com
    
    This script is written for Unity 3D Engine
    Unity Version: 2017.2 and up
    
    
    INFO: Game Manger script for TerraLand Run-Time Demo
    
*/

#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using System;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace TerraLand
{
    public class GameManagerRA : MonoBehaviour
    {
        private static Runtime runTime;

        public GameObject worldGenerator;
        public Camera cam;
        public Light sun;
        public GameObject pauseMenu;
        public GameObject fadeIn;
        public GameObject loadingScreen;
        public GameObject loadingUI;
        public GameObject loadingHeightmap;
        public GameObject loadingImage;
        public GameObject heightmapPercent;
        public GameObject ImagePercent;

        public float startHeight = 300f; // From beneath terrain surface
        public float heightLimit = 1000f; // From highest elevation in active world
        public LayerMask terrainLayer;

        public bool enableDetailTextures = true;
        public Texture2D detailTexture;
        public Texture2D detailNormal;
        public Texture2D detailNormalFar;
        [Range(0, 100)] public float detailBelending = 25f;
        public float detailTileSize = 25f;

        public bool debugMode = false;

        private Terrain terrain;
        private RaycastHit hit;
        private Vector3 rayPosition;
        private bool playerIsSet = false;
        private float currentHighestPoint;

        private Ray ray;
        private float terrainHeight;
        private float playerHeight;

        private double initialLat;
        private double initialLon;
        private double playerLat;
        private double playerLon;
        private static double earthRadius = 6378137;
        private Vector3 realWorldPosition;

        public static bool enableDetailTexturesMenu;

        private bool isPaused = false;
        private AudioSource[] allAudioSources;
        private Image fade;
        private float startTime;
        private float fadeTime = 6f;
        private Image loading;

        private int downloadedHeightmaps;
        private int downloadedImages;
        private int totalchunks;

        private RectTransform barRectHeightmap;
        private RectTransform barRectImage;

        private AudioClip[] audioTracks;
        private AudioSource musicPlayer;
        private int trackIndex;

        private bool gameLoaded = false;
        private GameObject player;


        void Awake()
        {
            player = cam.gameObject;

            if (pauseMenu != null)
                pauseMenu.SetActive(false);

            barRectHeightmap = loadingHeightmap.GetComponent<RectTransform>();
            barRectImage = loadingImage.GetComponent<RectTransform>();

            barRectHeightmap.sizeDelta = Vector2.zero;
            barRectImage.sizeDelta = Vector2.zero;

            fade = fadeIn.GetComponent<Image>();
            fade.color = new UnityEngine.Color(fade.color.r, fade.color.g, fade.color.b, 1f);

            loading = loadingScreen.GetComponent<Image>();
            loading.color = new UnityEngine.Color(loading.color.r, loading.color.g, loading.color.b, 1f);

            if (!MainMenu.latitude.Equals(""))
                SetFromMenu();

            runTime = worldGenerator.GetComponent<Runtime>();

            rayPosition = new Vector3(1, 99000, 1);

            CheckDetailTextures();

            if (debugMode)
            {
                worldGenerator.SetActive(false);
                CreateTempTerrain();
                SetPlayer();
            }

            SetupMusic();

            QualitySettings.shadowDistance = (runTime.areaSize * 1000f) / 4f;

#if UNITY_EDITOR
            SetupShadowSettings();
#endif
        }

        void Update()
        {
            GetLatLon();

            if (Input.GetKeyDown(KeyCode.F))
                TerraLand.TerraLandRuntime.worldIsGenerated = true;

            if (!debugMode)
                if (!playerIsSet && TerraLand.TerraLandRuntime.worldIsGenerated)
                    SetPlayer();

            if (!playerIsSet)
                LoadingScreen();
            else
            {
                ray = new Ray(new Vector3(player.transform.position.x, 1000f, player.transform.position.z), Vector3.down);

                if (Physics.Raycast(ray, out hit, Mathf.Infinity, terrainLayer))
                    terrain = hit.transform.gameObject.GetComponent<Terrain>();

                if (terrain != null)
                {
                    terrainHeight = terrain.SampleHeight(player.transform.position);
                    playerHeight = player.transform.position.y - terrainHeight;
                }

                if (Input.GetKeyDown(KeyCode.R))
                {
                    player.transform.eulerAngles = new Vector3(0, player.transform.eulerAngles.y, 0);
                    player.transform.position += player.transform.up * startHeight;
                }

                // Display Game Menu by pressing Escape button
                if (Input.GetKeyDown(KeyCode.Escape))
                {
                    isPaused = !isPaused;

                    if (isPaused)
                        PauseGame();
                    else
                        ResumeGame();
                }

                if (Time.timeSinceLevelLoad > startTime + 2 && fade.enabled)
                {
                    float fadeAmount = 1f - Mathf.InverseLerp(0f, fadeTime, Time.timeSinceLevelLoad - (startTime + 2));
                    fade.color = new UnityEngine.Color(fade.color.r, fade.color.g, fade.color.b, fadeAmount);
                    musicPlayer.volume = fadeAmount;
                }

                if (fade.color.a == 0f)
                {
                    fade.enabled = false;
                    musicPlayer.enabled = false;
                    gameLoaded = true;
                }
            }
        }

        private void LoadingScreen()
        {
            totalchunks = (int)Mathf.Pow((int)runTime.terrainGridSize, 2);
            downloadedHeightmaps = TerraLand.TerraLandRuntime.downloadedHeightmapIndex;
            downloadedImages = TerraLand.TerraLandRuntime.downloadedImageIndex;

            float progressHeightmap = (float)downloadedHeightmaps / (float)totalchunks;
            float progressImage = (float)downloadedImages / (float)totalchunks;

            float progressHeightmapPercent = progressHeightmap * 100f;
            float progressImagePercent = progressImage * 100f;

            float barWidthHeightmap = progressHeightmap * 940f;
            float barHeightHeightmap = progressHeightmap * 27f;

            float barWidthImage = progressImage * 940f;
            float barHeightImage = progressImage * 27f;

            barRectHeightmap.sizeDelta = new Vector2(barWidthHeightmap, barHeightHeightmap);
            barRectImage.sizeDelta = new Vector2(barWidthImage, barHeightImage);

            heightmapPercent.GetComponent<Text>().text = (int)progressHeightmapPercent + "%";
            ImagePercent.GetComponent<Text>().text = (int)progressImagePercent + "%";

            PlayNextSong();
        }

        private void SetupMusic()
        {
            audioTracks = new AudioClip[]
            {
            Resources.Load("Menu/Music/BenSound-SciFi") as AudioClip,
            Resources.Load("Menu/Music/Exclusion-Earthshine") as AudioClip,
            Resources.Load("Menu/Music/Exclusion-Unity") as AudioClip,
            Resources.Load("Menu/Music/Machinimasound-SeptemberSky") as AudioClip,
            Resources.Load("Menu/Music/Mnykin-ElfSwamp") as AudioClip
            };

            musicPlayer = GetComponent<AudioSource>();
            trackIndex = UnityEngine.Random.Range(0, audioTracks.Length);
            musicPlayer.clip = audioTracks[trackIndex];
            musicPlayer.Play();
        }

        private void PlayNextSong()
        {
            if (!musicPlayer.isPlaying)
            {
                trackIndex++;

                if (trackIndex >= audioTracks.Length)
                    trackIndex = 0;

                musicPlayer.clip = audioTracks[trackIndex];
                musicPlayer.Play();
            }
        }

        public void PauseGame()
        {
            Cursor.visible = true;
            pauseMenu.SetActive(true);
            Time.timeScale = 0f; //0.025f
            allAudioSources = FindObjectsOfType(typeof(AudioSource)) as AudioSource[];

            foreach (AudioSource audio in allAudioSources)
            {
                if (!audio.gameObject.name.Equals("GameManager"))
                    audio.mute = true;
            }

            Resources.UnloadUnusedAssets();
        }

        public void ResumeGame()
        {
            isPaused = false;

            Cursor.visible = false;
            pauseMenu.SetActive(false);
            Time.timeScale = 1.0f;
            allAudioSources = FindObjectsOfType(typeof(AudioSource)) as AudioSource[];

            foreach (AudioSource audio in allAudioSources)
            {
                if (!audio.gameObject.name.Equals("GameManager"))
                    audio.mute = false;
            }
        }

        public void SetFromMenu()
        {
            enableDetailTextures = enableDetailTexturesMenu;
            debugMode = false;
        }

        private void GetLatLon()
        {
            realWorldPosition = Camera.main.GetComponent<FloatingOriginAdvanced>().absolutePosition;

            double offsetLat = (realWorldPosition.z / earthRadius) * 180 / Math.PI;
            playerLat = initialLat + offsetLat; // Moving NORTH/SOUTH

            double offsetLon = (realWorldPosition.x / (earthRadius * Math.Cos(Math.PI * playerLat / 180))) * 180 / Math.PI;
            playerLon = initialLon + offsetLon; // Moving EAST/WEST
        }

        public void SetPlayer()
        {
            if (!debugMode)
            {
                if (MainMenu.latitude.Equals(""))
                {
                    initialLat = double.Parse(runTime.latitudeUser);
                    initialLon = double.Parse(runTime.longitudeUser);
                }
                else
                {
                    initialLat = double.Parse(Runtime.latitudeMenu);
                    initialLon = double.Parse(Runtime.longitudeMenu);
                }
            }

            if (startHeight <= 0f)
                startHeight = 0.01f;

            ray = new Ray(rayPosition, Vector3.down);

            if (Physics.Raycast(ray, out hit, Mathf.Infinity, terrainLayer))
                terrain = hit.transform.gameObject.GetComponent<Terrain>();

            if (terrain != null)
            {
                terrainHeight = terrain.SampleHeight(player.transform.position);
                player.transform.position = new Vector3(0, terrainHeight + terrain.gameObject.transform.position.y + startHeight, 0);
                UnityEngine.Debug.Log("Player Loaded");
            }
            else
                UnityEngine.Debug.LogWarning("No Terrains Detected Underneath!");

            if (!debugMode && enableDetailTextures)
                AddDetailTexturesToTerrains();

            //#if UNITY_EDITOR
            //UnityEditor.PlayerSettings.runInBackground = Runtime.initialRunInBackground;
            //#endif

            loadingScreen.SetActive(false);
            loadingUI.SetActive(false);

            startTime = Time.timeSinceLevelLoad;

            playerIsSet = true;
        }

        private void CreateTempTerrain()
        {
            float terrainSize = 100000;

            GameObject terrainGameObject = new GameObject("Debug Terrain");
            terrainGameObject.transform.position = new Vector3(-(terrainSize / 2f), 0, -(terrainSize / 2f));
            terrainGameObject.AddComponent<Terrain>();

            TerrainData data = new TerrainData();
            data.size = new Vector3(terrainSize, 1, terrainSize);

            Terrain terrain = terrainGameObject.GetComponent<Terrain>();
            terrain.terrainData = data;

#if !UNITY_2019_1_OR_NEWER
            terrain.materialType = Terrain.MaterialType.Custom;
#endif
            terrain.materialTemplate = MaterialManager.GetTerrainMaterial();

            terrainGameObject.AddComponent<TerrainCollider>();
            terrainGameObject.GetComponent<TerrainCollider>().terrainData = data;

            terrainGameObject.layer = 8;

            if (enableDetailTextures)
                AddDetailTextures(terrain, detailBelending, false);
        }

        private void CheckDetailTextures()
        {
#if UNITY_EDITOR
            Texture2D[] detailTextures = new Texture2D[2] { detailTexture, detailNormal };

            foreach (Texture2D currentImage in detailTextures)
            {
                TextureImporter imageImport = AssetImporter.GetAtPath(AssetDatabase.GetAssetPath(currentImage)) as TextureImporter;

                if (imageImport != null && !imageImport.isReadable)
                {
                    imageImport.isReadable = true;
                    AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(currentImage), ImportAssetOptions.ForceUpdate);
                }
            }
#endif
        }

        private void AddDetailTexturesToTerrains()
        {
            List<Terrain> terrains = TerraLand.TerraLandRuntime.croppedTerrains;

            foreach (Terrain t in terrains)
                AddDetailTextures(t, detailBelending, false);

            if (runTime.farTerrain)
            {
                Terrain terrain1 = TerraLand.TerraLandRuntime.firstTerrain;
                Terrain terrain2 = TerraLand.TerraLandRuntime.secondaryTerrain;
                AddDetailTextures(terrain1, Mathf.Clamp(detailBelending * 1f, 0f, 100f), true);
                AddDetailTextures(terrain2, Mathf.Clamp(detailBelending * 1f, 0f, 100f), true);
            }
        }

        private void AddDetailTextures(Terrain terrain, float blend, bool farTerrain)
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

                        if (!farTerrain)
                        {
                            if (detailNormal != null)
                            {
                                terrainLayers[i].normalMapTexture = detailNormal;
                                terrainLayers[i].normalMapTexture.Apply();
                            }
                        }
                        else
                        {
                            if (detailNormalFar != null)
                            {
                                terrainLayers[i].normalMapTexture = detailNormalFar;
                                terrainLayers[i].normalMapTexture.Apply();
                            }
                        }

                        terrainLayers[i].tileSize = new Vector2(currentLayer.tileSize.x, currentLayer.tileSize.y);
                        terrainLayers[i].tileOffset = new Vector2(currentLayer.tileOffset.x, currentLayer.tileOffset.y);
                    }
                    else
                    {
                        terrainLayers[i] = new TerrainLayer();
                        if (detailTexture != null) terrainLayers[i].diffuseTexture = detailTexture;

                        if (!farTerrain)
                        {
                            if (detailNormal != null)
                            {
                                terrainLayers[i].normalMapTexture = detailNormal;
                                terrainLayers[i].normalMapTexture.Apply();
                            }
                        }
                        else
                        {
                            if (detailNormalFar != null)
                            {
                                terrainLayers[i].normalMapTexture = detailNormalFar;
                                terrainLayers[i].normalMapTexture.Apply();
                            }
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

                    if(!farTerrain)
                    {
                        if(detailNormal != null)
                        {
                            terrainTextures[i].normalMap = detailNormal;
                            terrainTextures[i].normalMap.Apply();
                        }
                    }
                    else
                    {
                        if(detailNormalFar != null)
                        {
                            terrainTextures[i].normalMap = detailNormalFar;
                            terrainTextures[i].normalMap.Apply();
                        }
                    }

                    terrainTextures[i].tileSize = new Vector2(currentSplatPrototye.tileSize.x, currentSplatPrototye.tileSize.y);
                    terrainTextures[i].tileOffset = new Vector2(currentSplatPrototye.tileOffset.x, currentSplatPrototye.tileOffset.y);
                }
                else
                {
                    terrainTextures[i] = new SplatPrototype();
                    if(detailTexture != null) terrainTextures[i].texture = detailTexture;

                    if(!farTerrain)
                    {
                        if(detailNormal != null)
                        {
                            terrainTextures[i].normalMap = detailNormal;
                            terrainTextures[i].normalMap.Apply();
                        }
                    }
                    else
                    {
                        if(detailNormalFar != null)
                        {
                            terrainTextures[i].normalMap = detailNormalFar;
                            terrainTextures[i].normalMap.Apply();
                        }
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

            int length = terrain.terrainData.alphamapResolution;
            float[,,] smData = new float[length, length, startIndex + 1];

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
        }

        public void QuitToMainMenu()
        {
            SceneManager.LoadSceneAsync("WorldExplorer Main Menu");
        }

        void OnGUI()
        {
            if (gameLoaded)
            {
                GUI.backgroundColor = new UnityEngine.Color(0.3f, 0.3f, 0.3f, 0.3f);
                GUI.Box(new Rect(10, Screen.height - 35, 220, 22), "Lat: " + playerLat.ToString("0.000000") + "   Lon: " + playerLon.ToString("0.000000"));
            }
        }

        private void UnloadResources()
        {
            Resources.UnloadUnusedAssets();
            GC.Collect();
        }

#if UNITY_EDITOR
        private void SetupShadowSettings()
        {
            SerializedObject qualitySettings = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/QualitySettings.asset")[0]);
            SerializedProperty levels = qualitySettings.FindProperty("m_QualitySettings");

            for (int i = 0; i < levels.arraySize; i++)
            {
                SerializedProperty level = levels.GetArrayElementAtIndex(i);
                GetChildProperty(level, "shadowCascades").enumValueIndex = 2;
                GetChildProperty(level, "shadowCascade4Split").vector3Value = new Vector3(0.005f, 0.05f, 0.35f);
            }

            qualitySettings.ApplyModifiedProperties();

            print("Shadow Settings have been overwritten");
        }

        private static SerializedProperty GetChildProperty(SerializedProperty parent, string name)
        {
            SerializedProperty child = parent.Copy();
            child.Next(true);

            do
            {
                if (child.name == name) return child;
            }

            while (child.Next(false));

            return null;
        }
#endif

        public void OnEnable()
        {
            UnloadResources();
        }

        public void OnDisable()
        {
            UnloadResources();
        }
    }
}

