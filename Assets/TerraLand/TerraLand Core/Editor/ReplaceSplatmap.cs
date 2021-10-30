using UnityEngine;
using UnityEditor;
using System;

public class ReplaceSplatmap : ScriptableWizard
{
    public Texture2D Splatmap;
    public Texture2D New;
    public bool FlipVertical;

    [UnityEditor.MenuItem("Tools/TerraUnity/Common/Replace Terrain Splatmap", false, 0)]
    static void Replace ()
    {
        ScriptableWizard.DisplayWizard<ReplaceSplatmap> ("ReplaceSplatmap", "Replace");
    }

    void OnWizardUpdate ()
    {
        helpString = "Replace the existing splatmap of your terrain with a new one.\nDrag the embedded splatmap texture of your terrain to the 'Splatmap box'.\nThen drag the replacement splatmap texture to the 'New' box\nThen hit 'Replace'.";
        isValid = (Splatmap != null) && (New != null);
    }

    void OnWizardCreate ()
    {
        if (New.format != TextureFormat.RGBA32)
        {
            EditorUtility.DisplayDialog("Wrong format", "Splatmap must be converted to RGBA 32 bit format.\nMake sure the type is Advanced and set the format!", "Cancel"); 
            return;
        }

        int w = New.width;

        if (Mathf.ClosestPowerOfTwo(w) != w)
        {
            EditorUtility.DisplayDialog("Wrong size", "Splatmap width and height must be a power of two!", "Cancel"); 
            return; 
        }  

        try
        {
            Color[] pixels = New.GetPixels();

            if (FlipVertical)
            {
                int h = w;

                for (var y = 0; y < h / 2; y++)
                {
                    var otherY = h - y - 1;

                    for (int x  = 0; x < w; x++)
                    {
                        var swapval = pixels[y * w + x];                  
                        pixels[y * w + x] = pixels[otherY * w + x];
                        pixels[otherY * w + x] = swapval;
                    }       
                }
            }

            Splatmap.Resize(New.width, New.height, New.format, true);
            Splatmap.SetPixels (pixels);
            Splatmap.Apply();
        }
        catch (Exception err)
        {
            //EditorUtility.DisplayDialog("Not readable", "The 'New' splatmap must be readable. Make sure the type is Advanced and enable read/write and try again!", "Cancel");
            UnityEngine.Debug.Log(err);
            return;
        }           
    }
}

