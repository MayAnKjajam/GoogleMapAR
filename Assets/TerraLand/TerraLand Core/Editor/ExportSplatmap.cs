using UnityEngine;
using UnityEditor;
using System.IO;

public class ExportSplatmap : MonoBehaviour
{
    [UnityEditor.MenuItem("Tools/TerraUnity/Common/Export Terrain Splatmap", false, 1)]
    static void Apply ()
    {
        Texture2D texture = Selection.activeObject as Texture2D;

        if (texture == null)
        {
            EditorUtility.DisplayDialog("Select Splat Map", "You Must Select a Splat Map first!\nGo to the project tab, find your terrain and open it's foldout. Then select either SplatAlpha0 or SplatAlpha1.", "Ok");
            return;
        }

        byte[] bytes = texture.EncodeToPNG();
        File.WriteAllBytes(Application.dataPath + "/Exported_Splatmap.png", bytes);

        AssetDatabase.Refresh();
    }
}

