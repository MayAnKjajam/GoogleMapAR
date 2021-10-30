#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.Collections;
using System.IO;

public class TileImageImport : MonoBehaviour
{
    public string tempPath;
    public string imgName;
    public int imageResolution;
    public int anisotropicFilter;

    public void ImportImage()
    {
        StartCoroutine(Import());
    }

    private IEnumerator Import()
    {
        if (this.transform.parent.transform.childCount > 1)
        {
            yield return new WaitForSeconds(1);
            StartCoroutine(Import());
        }
        else
        {
            File.Move(tempPath, imgName);
            AssetDatabase.Refresh();
            TextureImporter textureImporter = (TextureImporter)TextureImporter.GetAtPath(imgName.Substring(imgName.LastIndexOf("Assets")));

            if (textureImporter != null)
            {
                textureImporter.isReadable = true;
                textureImporter.mipmapEnabled = true;
                textureImporter.wrapMode = TextureWrapMode.Clamp;
                textureImporter.anisoLevel = anisotropicFilter;

                TextureImporterPlatformSettings platformSettings = new TextureImporterPlatformSettings();
                platformSettings.overridden = true;
                platformSettings.format = TextureImporterFormat.Automatic;
                platformSettings.maxTextureSize = imageResolution;
                textureImporter.SetPlatformTextureSettings(platformSettings);

                Object asset = AssetDatabase.LoadAssetAtPath(imgName.Substring(imgName.LastIndexOf("Assets")), typeof(Object)) as Object;
                AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(asset), ImportAssetOptions.ForceUpdate);
            }

            AssetDatabase.Refresh();
            DestroyImmediate(this.gameObject);
        }
    }
}
#endif

