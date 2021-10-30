using UnityEngine;
using System.Collections;

public class FPSDisplay : MonoBehaviour
{
    float deltaTime = 0.0f;
    int w, h, halfW;

    void Update()
    {
        deltaTime += (Time.deltaTime - deltaTime) * 0.1f;
        w = Screen.width;
        h = Screen.height;
        halfW = (w / 2) - (50);
    }

    void OnGUI()
    {
        GUIStyle style = new GUIStyle();

        Rect rect = new Rect(halfW, 10, 100, h * 2 / 60);
        style.alignment = TextAnchor.MiddleCenter;
        style.fontSize = h / 40;
        style.normal.textColor = new Color (1f, 1f, 1f, 1f);
        float msec = deltaTime * 1000.0f;
        float fps = 1.0f / deltaTime;
        string text = string.Format("{0:0.0} ms ({1:0.} fps)", msec, fps);
        GUI.Label(rect, text, style);
    }
}

