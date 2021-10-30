using UnityEngine;

public class Logo : MonoBehaviour
{
    private Texture2D logo;

    void Start ()
    {
        logo = Resources.Load("TerraUnity/Images/Logo/Logo") as Texture2D;
        //Cursor.visible = false;
    }

    void OnGUI ()
    {
        GUI.backgroundColor = UnityEngine.Color.clear;
        GUI.Box (new Rect(Screen.width - 165, Screen.height - 59, 160, 54), logo);
    }
}

