/*
    _____  _____  _____  _____  ______
        |  _____ |      |      |  ___|
        |  _____ |      |      |     |
    
     U       N       I       T      Y
                                         
    
    TerraUnity Co. - Earth Simulation Tools - 2016
    
    http://terraunity.com
    info@terraunity.com
    
    This script is written for Unity 3D Engine.
    Unity 3D Version: Unity 5.x
    
    
    INFO: Main Menu for the TerraLand 2 Runtime Demo Scene

    Written by: Amir Badamchi
    
*/


using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System;
using System.Text.RegularExpressions;
using System.Collections.Generic;

namespace TerraLand
{
    public class MainMenu : MonoBehaviour
    {
        public GameObject setLocation;
        public GameObject settings;
        public GameObject mainSettings;
        public GameObject advancedSettings;
        public GameObject performanceSettings;
        public GameObject graphicSettings;
        public GameObject exit;

        public GameObject sources;
        public GameObject link;

        public GameObject addressField;
        public GameObject latitudeField;
        public GameObject longitudeField;
        public GameObject statusText;
        private Button[] buttons;
        private Text[] texts;

        public static string address = "";
        private static List<Vector2> coords;
        private static string status = "";

        // Main Settings
        public static string latitude = "";
        public static string longitude = "";
        public GameObject gridSizeText;
        public GameObject gridSizeDropdown;
        public GameObject areaSizeText;
        public GameObject areaSizeSlider;
        public GameObject heightmapResolutionText;
        public GameObject heightmapResolutionSlider;
        public GameObject imageResolutionText;
        public GameObject imageResolutionSlider;
        public GameObject elevationExaggerationText;
        public GameObject elevationExaggerationSlider;
        public GameObject terrainSmoothnessText;
        public GameObject terrainSmoothnessSlider;
        public GameObject farTerrainToggle;
        public GameObject farHeightmapResolutionText;
        public GameObject farHeightmapResolutionSlider;
        public GameObject farImageResolutionText;
        public GameObject farImageResolutionSlider;
        public GameObject farMultiplierText;
        public GameObject farMultiplierSlider;

        // Performance Settings
        public GameObject terrainQualityText;
        public GameObject terrainQualitySlider;
        public GameObject farTerrainQualityText;
        public GameObject farTerrainQualitySlider;
        public GameObject cellSizeText;
        public GameObject cellSizeSlider;
        public GameObject concurrentTasksText;
        public GameObject concurrentTasksSlider;
        public GameObject elevationDelayText;
        public GameObject elevationDelaySlider;
        public GameObject imageryDelayText;
        public GameObject imageryDelaySlider;

        // Advanced Settings
        public GameObject elevationOnlyToggle;
        public GameObject fastStartBuildToggle;
        public GameObject showTileOnFinishToggle;
        public GameObject progressiveTexturingToggle;
        public GameObject spiralGenerationToggle;
        public GameObject delayedLODToggle;
        public GameObject stitchToggle;
        public GameObject farTerrainBelowText;
        public GameObject farTerrainBelowSlider;
        public GameObject stitchSmoothnessText;
        public GameObject stitchSmoothnessSlider;
        public GameObject stitchPowerText;
        public GameObject stitchPowerSlider;
        public GameObject stitchDistanceText;
        public GameObject stitchDistanceSlider;
        public GameObject stitchDelayText;
        public GameObject stitchDelaySlider;

        // Graphic Settings
        public GameObject volumetricLightingToggle;
        public GameObject atmosphericScatteringToggle;
        public GameObject enableDetailTexturesToggle;
        public GameObject enableCloudsToggle;
        public GameObject enableCloudShadowsToggle;

        private static int currentGridSize;
        private static string currentGridSizeEnum;
        private RawImage sourcesPanel;
        private RawImage linkPanel;
        private bool isMainMenu = true;

        private UnityEngine.Color enabledColor;
        private UnityEngine.Color disabledColor;


        public void Start()
        {
            Time.fixedDeltaTime = 0.02f;
            Time.timeScale = 1.0f;

            setLocation.SetActive(true);
            mainSettings.SetActive(true);
            advancedSettings.SetActive(true);
            performanceSettings.SetActive(true);
            graphicSettings.SetActive(true);
            exit.SetActive(true);

            enabledColor = new UnityEngine.Color(0.8f, 0.8f, 0.8f, 1.0f);
            disabledColor = new UnityEngine.Color(0.3f, 0.3f, 0.3f, 1.0f);

            buttons = GetComponentsInChildren<Button>();
            texts = settings.GetComponentsInChildren<Text>();

            foreach (Text t in texts)
                t.color = enabledColor;

            sourcesPanel = sources.GetComponent<RawImage>();
            linkPanel = link.GetComponent<RawImage>();

            setLocation.SetActive(true);
            mainSettings.SetActive(false);
            advancedSettings.SetActive(false);
            performanceSettings.SetActive(false);
            graphicSettings.SetActive(false);
            exit.SetActive(false);

            status = "";

            InitializeSettings();
        }

        public void Update()
        {
            // Enable/Disable menu items based on their root setting
            //----------------------------------------------------------------------
            // Check Far Terrain Toggle
            if (farTerrainToggle.GetComponent<Toggle>().isOn == true)
            {
                farHeightmapResolutionSlider.GetComponent<Slider>().interactable = true;

                if (elevationOnlyToggle.GetComponent<Toggle>().isOn == false)
                    farImageResolutionSlider.GetComponent<Slider>().interactable = true;
                else
                    farImageResolutionSlider.GetComponent<Slider>().interactable = false;

                farMultiplierSlider.GetComponent<Slider>().interactable = true;
                farTerrainBelowSlider.GetComponent<Slider>().interactable = true;
                farTerrainQualitySlider.GetComponent<Slider>().interactable = true;


                farHeightmapResolutionText.GetComponent<Text>().color = enabledColor;

                if (elevationOnlyToggle.GetComponent<Toggle>().isOn == false)
                    farImageResolutionText.GetComponent<Text>().color = enabledColor;
                else
                    farImageResolutionText.GetComponent<Text>().color = disabledColor;

                farMultiplierText.GetComponent<Text>().color = enabledColor;
                farTerrainBelowText.GetComponent<Text>().color = enabledColor;
                farTerrainQualityText.GetComponent<Text>().color = enabledColor;
            }
            else
            {
                farHeightmapResolutionSlider.GetComponent<Slider>().interactable = false;
                farImageResolutionSlider.GetComponent<Slider>().interactable = false;
                farMultiplierSlider.GetComponent<Slider>().interactable = false;
                farTerrainBelowSlider.GetComponent<Slider>().interactable = false;
                farTerrainQualitySlider.GetComponent<Slider>().interactable = false;

                farHeightmapResolutionText.GetComponent<Text>().color = disabledColor;
                farImageResolutionText.GetComponent<Text>().color = disabledColor;
                farMultiplierText.GetComponent<Text>().color = disabledColor;
                farTerrainBelowText.GetComponent<Text>().color = disabledColor;
                farTerrainQualityText.GetComponent<Text>().color = disabledColor;
            }

            // Check Elevation Only Toggle
            if (elevationOnlyToggle.GetComponent<Toggle>().isOn == true)
            {
                imageResolutionSlider.GetComponent<Slider>().interactable = false;
                progressiveTexturingToggle.GetComponent<Toggle>().interactable = false;
                imageryDelaySlider.GetComponent<Slider>().interactable = false;

                imageResolutionText.GetComponent<Text>().color = disabledColor;
                progressiveTexturingToggle.transform.Find("Label").GetComponent<Text>().color = disabledColor;
                imageryDelayText.GetComponent<Text>().color = disabledColor;
            }
            else
            {
                imageResolutionSlider.GetComponent<Slider>().interactable = true;
                progressiveTexturingToggle.GetComponent<Toggle>().interactable = true;
                imageryDelaySlider.GetComponent<Slider>().interactable = true;

                imageResolutionText.GetComponent<Text>().color = enabledColor;
                progressiveTexturingToggle.transform.Find("Label").GetComponent<Text>().color = enabledColor;
                imageryDelayText.GetComponent<Text>().color = enabledColor;
            }

            // Check Terrain Stitching Toggle
            if (stitchToggle.GetComponent<Toggle>().isOn == true)
            {
                stitchSmoothnessSlider.GetComponent<Slider>().interactable = true;
                stitchPowerSlider.GetComponent<Slider>().interactable = true;
                stitchDelaySlider.GetComponent<Slider>().interactable = true;
                stitchDistanceSlider.GetComponent<Slider>().interactable = true;

                stitchSmoothnessText.GetComponent<Text>().color = enabledColor;
                stitchPowerText.GetComponent<Text>().color = enabledColor;
                stitchDelayText.GetComponent<Text>().color = enabledColor;
                stitchDistanceText.GetComponent<Text>().color = enabledColor;
            }
            else
            {
                stitchSmoothnessSlider.GetComponent<Slider>().interactable = false;
                stitchPowerSlider.GetComponent<Slider>().interactable = false;
                stitchDelaySlider.GetComponent<Slider>().interactable = false;
                stitchDistanceSlider.GetComponent<Slider>().interactable = false;

                stitchSmoothnessText.GetComponent<Text>().color = disabledColor;
                stitchPowerText.GetComponent<Text>().color = disabledColor;
                stitchDelayText.GetComponent<Text>().color = disabledColor;
                stitchDistanceText.GetComponent<Text>().color = disabledColor;
            }


            // Main Settings
            //----------------------------------------------------------------------
            gridSizeText.GetComponent<Text>().text = "Chunk Grid Size - " + Mathf.Pow(currentGridSize, 2).ToString() + " Terrains";

            if (gridSizeDropdown.GetComponent<Dropdown>().value == 0)
            {
                currentGridSize = 4;
                currentGridSizeEnum = "_4x4";
            }
            else if (gridSizeDropdown.GetComponent<Dropdown>().value == 1)
            {
                currentGridSize = 8;
                currentGridSizeEnum = "_8x8";
            }
            else if (gridSizeDropdown.GetComponent<Dropdown>().value == 2)
            {
                currentGridSize = 16;
                currentGridSizeEnum = "_16x16";
            }

            areaSizeText.GetComponent<Text>().text = "Area Size: " + areaSizeSlider.GetComponent<Slider>().value.ToString("0.000") + " KM";

            heightmapResolutionSlider.GetComponent<Slider>().minValue = currentGridSize * 32;
            heightmapResolutionSlider.GetComponent<Slider>().value = Mathf.ClosestPowerOfTwo((int)heightmapResolutionSlider.GetComponent<Slider>().value);
            heightmapResolutionText.GetComponent<Text>().text = "Heightmap Resolution: " + heightmapResolutionSlider.GetComponent<Slider>().value;
            imageResolutionSlider.GetComponent<Slider>().value = Mathf.ClosestPowerOfTwo((int)imageResolutionSlider.GetComponent<Slider>().value);
            imageResolutionText.GetComponent<Text>().text = "Satellite Image Resolution: " + imageResolutionSlider.GetComponent<Slider>().value;
            elevationExaggerationText.GetComponent<Text>().text = "Elevation Exaggeration: " + elevationExaggerationSlider.GetComponent<Slider>().value;
            terrainSmoothnessText.GetComponent<Text>().text = "Terrain Smoothness Steps: " + terrainSmoothnessSlider.GetComponent<Slider>().value;
            farHeightmapResolutionSlider.GetComponent<Slider>().value = Mathf.ClosestPowerOfTwo((int)farHeightmapResolutionSlider.GetComponent<Slider>().value);
            farHeightmapResolutionText.GetComponent<Text>().text = "Heightmap Resolution FAR: " + farHeightmapResolutionSlider.GetComponent<Slider>().value;
            farImageResolutionSlider.GetComponent<Slider>().value = Mathf.ClosestPowerOfTwo((int)farImageResolutionSlider.GetComponent<Slider>().value);
            farImageResolutionText.GetComponent<Text>().text = "Image Resolution FAR: " + farImageResolutionSlider.GetComponent<Slider>().value;

            float areaSizeFar = areaSizeSlider.GetComponent<Slider>().value * farMultiplierSlider.GetComponent<Slider>().value;
            farMultiplierText.GetComponent<Text>().text = "Area Multiplier: " + farMultiplierSlider.GetComponent<Slider>().value + "  -  " + areaSizeFar.ToString("0.000") + " KM";


            // Performance Settings
            //----------------------------------------------------------------------
            terrainQualityText.GetComponent<Text>().text = "Terrain Quality: " + (terrainQualitySlider.GetComponent<Slider>().value).ToString() + " %";
            farTerrainQualityText.GetComponent<Text>().text = "Terrain Quality FAR: " + (farTerrainQualitySlider.GetComponent<Slider>().value).ToString() + " %";
            cellSizeSlider.GetComponent<Slider>().minValue = 32;
            cellSizeSlider.GetComponent<Slider>().maxValue = heightmapResolutionSlider.GetComponent<Slider>().value / currentGridSize;
            cellSizeSlider.GetComponent<Slider>().value = Mathf.ClosestPowerOfTwo((int)cellSizeSlider.GetComponent<Slider>().value);
            cellSizeText.GetComponent<Text>().text = "Updating Cell Resolution: " + cellSizeSlider.GetComponent<Slider>().value;
            concurrentTasksText.GetComponent<Text>().text = "Concurrent Tasks: " + concurrentTasksSlider.GetComponent<Slider>().value;
            elevationDelayText.GetComponent<Text>().text = "Elevation Update Delay: " + elevationDelaySlider.GetComponent<Slider>().value;
            imageryDelayText.GetComponent<Text>().text = "Imagery Update Delay: " + imageryDelaySlider.GetComponent<Slider>().value;


            // Advanced Settings
            //----------------------------------------------------------------------
            farTerrainBelowText.GetComponent<Text>().text = "FAR Terrain Below:  " + farTerrainBelowSlider.GetComponent<Slider>().value.ToString("0.0") + " Meters";
            stitchSmoothnessText.GetComponent<Text>().text = "Stitch Smoothness: " + stitchSmoothnessSlider.GetComponent<Slider>().value;
            stitchPowerText.GetComponent<Text>().text = "Stitch Power: " + stitchPowerSlider.GetComponent<Slider>().value;
            stitchDistanceText.GetComponent<Text>().text = "Stitch Distance: " + stitchDistanceSlider.GetComponent<Slider>().value;
            stitchDelayText.GetComponent<Text>().text = "Stitch Delay: " + stitchDelaySlider.GetComponent<Slider>().value;


            // Blinking Sources Panel
            float blink = 1f + Mathf.Sin(Time.time);
            sourcesPanel.color = new UnityEngine.Color(sourcesPanel.color.r, sourcesPanel.color.g, sourcesPanel.color.b, blink);
            linkPanel.color = new UnityEngine.Color(linkPanel.color.r, linkPanel.color.g, linkPanel.color.b, blink);

            if (isMainMenu)
            {
                if (Input.GetKeyDown(KeyCode.Escape))
                    ExitMenu();
            }
            else
            {
                if (Input.GetKeyDown(KeyCode.Escape))
                    Back();
            }
        }

        private void InitializeSettings()
        {
            //address = "Mount Everest";
            //latitude = "27.98582";
            //longitude = "86.9236";

            DefaultMainSettings();
            DefaultPerformanceSettings();
            DefaultAdvancedSettings();
            DefaultGraphicSettings();
        }

        public void CheckFields()
        {
            status = "";
            address = addressField.transform.Find("Text").GetComponent<Text>().text;
            latitude = latitudeField.transform.Find("Text").GetComponent<Text>().text;
            longitude = longitudeField.transform.Find("Text").GetComponent<Text>().text;

            if (!latitude.Equals("") && !longitude.Equals(""))
            {
                double zero = 0.0;
                latitude = Regex.Replace(latitude, "[^0-9.-]", "");
                longitude = Regex.Replace(longitude, "[^0-9.-]", "");

                if (Double.TryParse(latitude, out zero))
                {
                    if (double.Parse(latitude) > 90.0 || double.Parse(latitude) < -90.0)
                        status = "INSERTED LATITUDE IS OUT OF BOUNDS!";
                    else
                        Runtime.latitudeMenu = latitude;
                }
                else
                    status = "INSERTED LATITUDE IS NOT IN THE CORRECT FORMAT!";

                if (Double.TryParse(longitude, out zero))
                {
                    if (double.Parse(longitude) > 180.0 || double.Parse(longitude) < -180.0)
                        status = "INSERTED LONGITUDE IS OUT OF BOUNDS!";
                    else
                        Runtime.longitudeMenu = longitude;
                }
                else
                    status = "INSERTED LONGITUDE IS NOT IN THE CORRECT FORMAT!";
            }
            else if (!address.Equals(""))
                CheckAddress();
            else
                status = "PLEASE INSERT ADDRESS OR GEO-COORDINATES";

            statusText.GetComponent<Text>().text = status;

            if (status.Equals(""))
                StartLevel();
        }

        private void CheckAddress()
        {
            coords = GeoCoder.AddressToLatLong(Regex.Replace(address, @"\s+", string.Empty));

            if (!GeoCoder.recognized)
                status = "ADDRESS/LOCATION IS NOT RECOGNIZED!";
            else if (GeoCoder.recognized && coords != null)
            {
                latitude = coords[0].x.ToString();
                longitude = coords[0].y.ToString();

                Runtime.latitudeMenu = latitude;
                Runtime.longitudeMenu = longitude;
            }
            else
                status = "ADDRESS/LOCATION IS NOT RECOGNIZED!";
        }

        private void SetupSettings()
        {
            TerraLand.TerraLandRuntime.sceneIsInitialized = false;
            TerraLand.TerraLandRuntime.imagesAreGenerated = false;
            TerraLand.TerraLandRuntime.terrainsAreGenerated = false;
            TerraLand.TerraLandRuntime.worldIsGenerated = false;
            TerraLand.TerraLandRuntime.farTerrainIsGenerated = false;

            // Main Settings
            Runtime.terrainGridSizeMenu = currentGridSizeEnum;
            Runtime.areaSizeMenu = areaSizeSlider.GetComponent<Slider>().value;
            Runtime.heightmapResolutionMenu = (int)heightmapResolutionSlider.GetComponent<Slider>().value;
            Runtime.imageResolutionMenu = (int)imageResolutionSlider.GetComponent<Slider>().value;
            Runtime.elevationExaggerationMenu = elevationExaggerationSlider.GetComponent<Slider>().value;
            Runtime.smoothIterationsMenu = (int)terrainSmoothnessSlider.GetComponent<Slider>().value;
            Runtime.farTerrainMenu = farTerrainToggle.GetComponent<Toggle>().isOn;
            Runtime.farTerrainHeightmapResolutionMenu = (int)farHeightmapResolutionSlider.GetComponent<Slider>().value;
            Runtime.farTerrainImageResolutionMenu = (int)farImageResolutionSlider.GetComponent<Slider>().value;
            Runtime.areaSizeFarMultiplierMenu = farMultiplierSlider.GetComponent<Slider>().value;


            // Performance Settings   
            Runtime.heightmapPixelErrorMenu = Mathf.Clamp((200 - (terrainQualitySlider.GetComponent<Slider>().value * 2f)), 1f, 200f);
            Runtime.farTerrainQualityMenu = Mathf.Clamp((200 - (farTerrainQualitySlider.GetComponent<Slider>().value * 2f)), 1f, 200f);
            Runtime.cellSizeMenu = (int)cellSizeSlider.GetComponent<Slider>().value;
            Runtime.concurrentTasksMenu = (int)concurrentTasksSlider.GetComponent<Slider>().value;
            Runtime.elevationDelayMenu = elevationDelaySlider.GetComponent<Slider>().value;
            Runtime.imageryDelayMenu = imageryDelaySlider.GetComponent<Slider>().value;


            // Advanced Settings
            Runtime.elevationOnlyMenu = elevationOnlyToggle.GetComponent<Toggle>().isOn;
            Runtime.fastStartBuildMenu = fastStartBuildToggle.GetComponent<Toggle>().isOn;
            Runtime.showTileOnFinishMenu = showTileOnFinishToggle.GetComponent<Toggle>().isOn;
            Runtime.progressiveTexturingMenu = progressiveTexturingToggle.GetComponent<Toggle>().isOn;
            Runtime.spiralGenerationMenu = spiralGenerationToggle.GetComponent<Toggle>().isOn;
            Runtime.delayedLODMenu = delayedLODToggle.GetComponent<Toggle>().isOn;
            Runtime.farTerrainBelowHeightMenu = farTerrainBelowSlider.GetComponent<Slider>().value;
            Runtime.stitchTerrainTilesMenu = stitchToggle.GetComponent<Toggle>().isOn;
            Runtime.levelSmoothMenu = (int)stitchSmoothnessSlider.GetComponent<Slider>().value;
            Runtime.powerMenu = (int)stitchPowerSlider.GetComponent<Slider>().value;
            Runtime.stitchDistanceMenu = (int)stitchDistanceSlider.GetComponent<Slider>().value;
            Runtime.stitchDelayMenu = stitchDelaySlider.GetComponent<Slider>().value;


            // Graphic Settings
            //GameManager.volumetricLightingMenu = volumetricLightingToggle.GetComponent<Toggle>().isOn;
            //GameManager.atmosphericScatteringMenu = atmosphericScatteringToggle.GetComponent<Toggle>().isOn;
            GameManagerRA.enableDetailTexturesMenu = enableDetailTexturesToggle.GetComponent<Toggle>().isOn;
            //GameManager.enableCloudsMenu = enableCloudsToggle.GetComponent<Toggle>().isOn;
            //GameManager.enableCloudShadowsMenu = enableCloudShadowsToggle.GetComponent<Toggle>().isOn;
        }

        public void DefaultMainSettings()
        {
            gridSizeDropdown.GetComponent<Dropdown>().value = 0;
            areaSizeSlider.GetComponent<Slider>().value = 32f;
            heightmapResolutionSlider.GetComponent<Slider>().value = 2048;
            imageResolutionSlider.GetComponent<Slider>().value = 1024;
            elevationExaggerationSlider.GetComponent<Slider>().value = 1.25f;
            terrainSmoothnessSlider.GetComponent<Slider>().value = 1;
            farTerrainToggle.GetComponent<Toggle>().isOn = true;
            farHeightmapResolutionSlider.GetComponent<Slider>().value = 512;
            farImageResolutionSlider.GetComponent<Slider>().value = 1024;
            farMultiplierSlider.GetComponent<Slider>().value = 6;
        }

        public void DefaultPerformanceSettings()
        {
            terrainQualitySlider.GetComponent<Slider>().value = 96;
            farTerrainQualitySlider.GetComponent<Slider>().value = 90;
            cellSizeSlider.GetComponent<Slider>().value = 128;
            concurrentTasksSlider.GetComponent<Slider>().value = 8;
            elevationDelaySlider.GetComponent<Slider>().value = 0.01f;
            imageryDelaySlider.GetComponent<Slider>().value = 0.01f;
        }

        public void DefaultAdvancedSettings()
        {
            elevationOnlyToggle.GetComponent<Toggle>().isOn = false;
            fastStartBuildToggle.GetComponent<Toggle>().isOn = true;
            showTileOnFinishToggle.GetComponent<Toggle>().isOn = true;
            progressiveTexturingToggle.GetComponent<Toggle>().isOn = true;
            spiralGenerationToggle.GetComponent<Toggle>().isOn = true;
            delayedLODToggle.GetComponent<Toggle>().isOn = false;

            farTerrainBelowSlider.GetComponent<Slider>().value = 1200;
            //farTerrainBelowSlider.GetComponent<Slider>().value = 0;

            stitchToggle.GetComponent<Toggle>().isOn = true;
            stitchSmoothnessSlider.GetComponent<Slider>().value = 5;
            stitchPowerSlider.GetComponent<Slider>().value = 1;
            stitchDistanceSlider.GetComponent<Slider>().value = 2;
            stitchDelaySlider.GetComponent<Slider>().value = 0.01f;
        }

        public void DefaultGraphicSettings()
        {
            volumetricLightingToggle.GetComponent<Toggle>().isOn = false;
            atmosphericScatteringToggle.GetComponent<Toggle>().isOn = false;
            enableDetailTexturesToggle.GetComponent<Toggle>().isOn = true;
            enableCloudsToggle.GetComponent<Toggle>().isOn = false;
            enableCloudShadowsToggle.GetComponent<Toggle>().isOn = false;
        }

        public void Back()
        {
            setLocation.SetActive(true);
            mainSettings.SetActive(false);
            advancedSettings.SetActive(false);
            performanceSettings.SetActive(false);
            graphicSettings.SetActive(false);
            exit.SetActive(false);

            isMainMenu = true;
        }

        public void MainSettings()
        {
            setLocation.SetActive(false);
            mainSettings.SetActive(true);
            advancedSettings.SetActive(false);
            performanceSettings.SetActive(false);
            graphicSettings.SetActive(false);
            exit.SetActive(false);

            isMainMenu = false;
        }

        public void AdvancedSettings()
        {
            setLocation.SetActive(false);
            mainSettings.SetActive(false);
            advancedSettings.SetActive(true);
            performanceSettings.SetActive(false);
            graphicSettings.SetActive(false);
            exit.SetActive(false);

            isMainMenu = false;
        }

        public void PerformanceSettings()
        {
            setLocation.SetActive(false);
            mainSettings.SetActive(false);
            advancedSettings.SetActive(false);
            performanceSettings.SetActive(true);
            graphicSettings.SetActive(false);
            exit.SetActive(false);

            isMainMenu = false;
        }

        public void GraphicSettings()
        {
            setLocation.SetActive(false);
            mainSettings.SetActive(false);
            advancedSettings.SetActive(false);
            performanceSettings.SetActive(false);
            graphicSettings.SetActive(true);
            exit.SetActive(false);

            isMainMenu = false;
        }

        public void StartLevel()
        {
            SetupSettings();

            SceneManager.LoadSceneAsync("TerraLand Runtime");

            foreach (Button b in buttons)
                b.interactable = false;

            System.GC.Collect();
        }

        private void ExitMenu()
        {
            exit.SetActive(true);
        }

        public void Resume()
        {
            exit.SetActive(false);
        }

        public void Quit()
        {
            Application.Quit();
        }

        private void UnloadResources()
        {
            Resources.UnloadUnusedAssets();
            GC.Collect();
        }

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

