/*
    _____  _____  _____  _____  ______
        |  _____ |      |      |  ___|
        |  _____ |      |      |     |
    
     U       N       I       T      Y
                                         
    
    TerraUnity Co. - Earth Simulation Tools - 2020
    
    http://terraunity.com
    info@terraunity.com
    
    This script is written for Unity Engine
    Unity Version: 2018.3 & up
    
    
    INFO: Modifies Project Settings to match up with the original setup so that Terraland operates properly.
    
*/


using UnityEngine;
using UnityEditor;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Threading;

[InitializeOnLoad]
public class TerraLandSettings : EditorWindow
{
    private static ApiCompatibilityLevel recommended_APICompatibilityLevel = ApiCompatibilityLevel.NET_4_6;
    private static bool isPlayed = false;
    private static Vector2 windowSize = new Vector2(540, 680);
    private static Texture2D logo;
    private static Color statusColor = Color.red;
    private static string statusStr = "Project Is Not Setup";
    private static Rect statusRect = new Rect(170, 500, 200, 25);
    private static string buttonTitle;

    private static bool show;

    static TerraLandSettings window;

    static TerraLandSettings ()
    {
        EditorApplication.update += Update;
    }

    static void Initialize ()
    {
        PlayerSettings.scriptingRuntimeVersion = ScriptingRuntimeVersion.Latest;
        PlayerSettings.SetApiCompatibilityLevel(BuildTargetGroup.Standalone, recommended_APICompatibilityLevel);
        logo = Resources.Load("TerraUnity/Images/Button/Landmap") as Texture2D;
        isPlayed = true;
    }

    static void Update ()
    {
        if (!isPlayed)
            Initialize();

        if (!PlayerPrefs.HasKey("TL") || PlayerPrefs.GetInt("TL") != 1)
            show = true;
        else
            show = false;

        if (show)
        {
            Screen.SetResolution(Screen.currentResolution.width, Screen.currentResolution.height, true);

            window = GetWindow<TerraLandSettings>(true, "TerraLand Settings", true);
            window.position = new Rect
                (
                    (Screen.currentResolution.width / 2) - (windowSize.x / 2),
                    (Screen.currentResolution.height / 2) - (windowSize.y / 2),
                    windowSize.x,
                    windowSize.y
                );

            window.minSize = new Vector2(windowSize.x, windowSize.y);
            window.maxSize = new Vector2(windowSize.x, windowSize.y);
        }
        else
        {
            statusColor = Color.green;
            statusStr = "Everything Is Ok";
        }

        EditorApplication.update -= Update;
    }

    public void OnGUI ()
    {
        Repaint();

        GUILayout.Space(330);

        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        GUILayout.Label("Welcome to TerraLand", EditorStyles.boldLabel);
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();

        GUILayout.Space(15);

        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        GUILayout.Label("Before starting to use TerraLand, some settings needs to be modified");
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();

//        GUILayout.BeginHorizontal();
//        GUILayout.FlexibleSpace();
//        GUILayout.Label("Make sure you have admin rights on this machine");
//        GUILayout.FlexibleSpace();
//        GUILayout.EndHorizontal();

        GUILayout.Space(15);

        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        GUILayout.Label("It is RECOMMENDED to import the package in a new empty project");
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();

        GUILayout.Space(15);

        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        GUILayout.Label("Press ACCEPT to setup project settings");
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();

        GUILayout.Space(20);

        if(statusStr.Equals("Everything Is Ok"))
            buttonTitle = "Restart Unity :)";
        else
            buttonTitle = "ACCEPT";

        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        if (GUILayout.Button(buttonTitle))
        {
            if(statusStr.Equals("Everything Is Ok"))
                CloseWindow();
            else
                SetSettings();
        }
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();

        GUI.color = statusColor;

        GUIStyle myStyle = new GUIStyle(GUI.skin.box);
        myStyle.fontSize = 15;
        myStyle.normal.textColor = Color.black;

        EditorGUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        GUI.Box(statusRect, new GUIContent(statusStr), myStyle);
        GUILayout.FlexibleSpace();
        EditorGUILayout.EndHorizontal();

        GUI.color = Color.white;

        GUILayout.Space(70);

        GUI.backgroundColor = new Color(1,1,1,0.1f);
        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();

        if (GUILayout.Button(logo))
            Help.BrowseURL("http://terraunity.com/product/terraland/");

        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();
        GUI.backgroundColor = Color.white;
    }

    static void SetSettings ()
    {
        PlayerPrefs.SetInt("TL", 1);
        PlayerPrefs.Save();

        PlayerSettings.scriptingRuntimeVersion = ScriptingRuntimeVersion.Latest;
        PlayerSettings.SetApiCompatibilityLevel(BuildTargetGroup.Standalone, recommended_APICompatibilityLevel);
        PlayerSettings.colorSpace = ColorSpace.Linear;

#if UNITY_2018_1_OR_NEWER
        PlayerSettings.allowUnsafeCode = true;
#endif

        ModifyConfigFiles();
        ShowTerraLandReadMe();
    }

    private static void ShowTerraLandReadMe()
    {
        string[] assetPaths = AssetDatabase.GetAllAssetPaths();
        string configReadMePath = "";

        foreach (string assetPath in assetPaths)
        {
            if (assetPath.EndsWith("TerraLand ReadMe.txt"))
                configReadMePath = assetPath;
        }

        if (!string.IsNullOrEmpty(configReadMePath) && File.Exists(configReadMePath))
        {
            string path = Path.GetFullPath(configReadMePath);
            Process.Start(path);
        }
        else
            EditorUtility.DisplayDialog("NO ReadMe FOUND", "ReadMe Not Found.", "OK");

        statusColor = Color.green;
        statusStr = "Everything Is Ok";
    }

    private static void ModifyConfigFiles ()
    {
        ConnectionsManager.SetAsyncConnections();
    }

    private void CloseWindow ()
    {
        this.Close();
    }
}

[InitializeOnLoad]
public static class FixCultureEditor
{
    static FixCultureEditor()
    {
        Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
    }
}

public static class FixCultureRuntime
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void FixCulture()
    {
        Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
    }
}

